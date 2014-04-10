using System;
using System.Collections.Generic;
using System.Text;

using VVVV.PluginInterfaces.V1;
using System.IO;

namespace VVVV.Nodes
{
    
    public class FileSystemWatcherNode : IPlugin, IDisposable
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "Watcher";							//use CamelCaps and no spaces
                Info.Category = "File";						//try to use an existing one
                Info.Version = "";						//versions are optional. leave blank if not needed
                Info.Help = "Checks if a file has changed in the file system";
                Info.Bugs = "";
                Info.Credits = "";								//give credits to thirdparty code used
                Info.Warnings = "";
                Info.Author = "vux";

                //leave below as is
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                System.Diagnostics.StackFrame sf = st.GetFrame(0);
                System.Reflection.MethodBase method = sf.GetMethod();
                Info.Namespace = method.DeclaringType.Namespace;
                Info.Class = method.DeclaringType.Name;
                return Info;
                //leave above as is
            }
        }
        #endregion

        #region Fields
        //private readonly string  WATCHER_ENUM = "FileSystemWatcher Event";
        private IPluginHost FHost;
        private IStringIn FPinInPath;
        private IStringIn FPinInFilter;
        private IValueIn FPinInRecurse;

        private IStringOut FPinOutName;
        private IStringOut FPinOutEventType;

        private bool FInvalidate = false;
        private List<string> FFileNames = new List<string>();
        private List<string> FEventTypes = new List<string>();
        private object m_lock = new object();

        private FileSystemWatcher FWatcher;
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return true; }
        }
        #endregion

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            //assign host
            this.FHost = Host;

            //this.FHost.UpdateEnum(WATCHER_ENUM,"All",Enum.GetNames(typeof(WatcherChangeTypes)));
      
            this.FHost.CreateStringInput("Path", TSliceMode.Single, TPinVisibility.True, out this.FPinInPath);
            this.FPinInPath.SetSubType2(string.Empty, int.MaxValue, string.Empty, TStringType.Directory);
   
            this.FHost.CreateStringInput("Filter", TSliceMode.Single, TPinVisibility.True, out this.FPinInFilter);
            this.FPinInFilter.SetSubType("*.*", false);
           
            this.FHost.CreateValueInput("Include Subdirectories", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInRecurse);
            this.FPinInRecurse.SetSubType(0, 1, 1, 0, false, true, false);
        
            this.FHost.CreateStringOutput("Path", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutName);
            this.FPinOutName.SetSubType2(string.Empty, int.MaxValue, string.Empty, TStringType.Directory);
         
            this.FHost.CreateStringOutput("Event Type", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutEventType);
            this.FPinOutEventType.SetSubType("", false);
        
            //this.FHost.CreateEnumOutput("Event Type", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutEventType);
            //this.FPinOutEventType.SetSubType(WATCHER_ENUM);
      
        }
        #endregion

        #region Configurate
        public void Configurate(IPluginConfig Input)
        {
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            if (this.FPinInPath.PinIsChanged || 
                this.FPinInFilter.PinIsChanged ||
                this.FPinInRecurse.PinIsChanged
                )
            {
                string path, filter;
                this.FPinInPath.GetString(0, out path);
                this.FPinInFilter.GetString(0, out filter);

                double rec;
                this.FPinInRecurse.GetValue(0, out rec);

                this.Reset();
                this.FWatcher = new FileSystemWatcher(path, filter);
                this.FWatcher.IncludeSubdirectories = rec >= 0.5;
                this.FWatcher.EnableRaisingEvents = true;

                this.FWatcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName
                | NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Security | NotifyFilters.Size;

                this.FWatcher.Changed += Changed;
                this.FWatcher.Created += Changed;
                this.FWatcher.Deleted += Changed;
                this.FWatcher.Renamed += Changed;

            }

            if (this.FInvalidate)
            {
                lock (m_lock)
                {
                    this.FPinOutName.SliceCount = this.FFileNames.Count;
                    this.FPinOutEventType.SliceCount = this.FEventTypes.Count;
                    for (int i = 0; i < this.FFileNames.Count; i++)
                    {
                        this.FPinOutName.SetString(i, this.FFileNames[i]);
                        this.FPinOutEventType.SetString(i, this.FEventTypes[i]);
                    }

                    this.FFileNames.Clear();
                    this.FEventTypes.Clear();
                    this.FInvalidate = true;
                }
            }
        }

        void Changed(object sender, FileSystemEventArgs e)
        {
            lock (m_lock)
            {
                this.FFileNames.Add(e.FullPath);
                this.FEventTypes.Add(e.ChangeType.ToString());
                this.FInvalidate = true;

            }
        }
        #endregion

        #region Dispose
        public void Dispose()
        {

        }
        #endregion

        private void Reset()
        {
            if (this.FWatcher != null)
            {
                this.FWatcher.Dispose();
            }
        }
    }
        
        
}
