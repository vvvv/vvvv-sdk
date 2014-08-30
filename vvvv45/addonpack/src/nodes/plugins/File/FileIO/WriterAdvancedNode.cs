using System;
using System.Collections.Generic;
using System.Text;

using VVVV.PluginInterfaces.V1;
using System.IO;

public class WriterAdvancedNode : IPlugin, IDisposable
{
    #region Plugin Info
    public static IPluginInfo PluginInfo
    {
        get
        {
            IPluginInfo Info = new PluginInfo();
            Info.Name = "Writer";							//use CamelCaps and no spaces
            Info.Category = "File";						//try to use an existing one
            Info.Version = "Advanced";						//versions are optional. leave blank if not needed
            Info.Help = "Writer with additional options";
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
    private IPluginHost FHost;

    private IStringIn FPinInContent;
    private IStringIn FPinInPath;
    private IValueIn FPinInAppend;
    private IValueIn FPinInWrite;
    private IValueIn FPinInEmpty;
    private IValueIn FPinInCreateDir;

    private IValueOut FPinOutAction;
    private IValueOut FPinOutSuccess;
    private IStringOut FPinOutMessage;
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

        this.FHost.CreateStringInput("Content", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInContent);
        this.FPinInContent.SetSubType("", false);
   
        this.FHost.CreateValueInput("Write", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInWrite);
        this.FPinInWrite.SetSubType(0, 1, 1, 0, true, false, false);
    
        this.FHost.CreateValueInput("Append", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInAppend);
        this.FPinInAppend.SetSubType(0, 1, 1, 0, false, true, false);
        
        this.FHost.CreateValueInput("Allow Empty File", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInEmpty);
        this.FPinInEmpty.SetSubType(0, 1, 1, 1, false, true, false);

        this.FHost.CreateStringInput("Filename", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInPath);
        this.FPinInPath.SetSubType("file.txt", true);
  
        this.FHost.CreateValueInput("Create Directory", 1, null, TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out this.FPinInCreateDir);
        this.FPinInCreateDir.SetSubType(0, 1, 1, 0, false, true, false);
        
      
        this.FHost.CreateValueOutput("Success", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutSuccess);
        this.FPinOutSuccess.SetSubType(0, 1, 1, 0, false, true, false);
     
        this.FHost.CreateStringOutput("Message", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutMessage);
        this.FPinOutMessage.SetSubType("", false);

        this.FHost.CreateValueOutput("On Write", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutAction);
        this.FPinOutAction.SetSubType(0, 1, 1, 0, true, false, false);
           
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
        this.FPinOutAction.SliceCount = SpreadMax;
        this.FPinOutMessage.SliceCount = SpreadMax;
        this.FPinOutSuccess.SliceCount = SpreadMax;

        for (int i = 0; i < SpreadMax; i++)
        {
            double dblwrite;
            this.FPinInWrite.GetValue(i, out dblwrite);

            if (dblwrite >= 0.5)
            {
                double dblappend, dblempty,dblcreate;
                string content, path;

                this.FPinInContent.GetString(i, out content);
                this.FPinInEmpty.GetValue(i, out dblempty);

                content = content != null ? content : String.Empty;

                if (!(dblempty < 0.5 && content.Length == 0))
                {
                    this.FPinInPath.GetString(i, out path);
                    this.FPinInAppend.GetValue(i, out dblappend);
                    this.FPinInCreateDir.GetValue(i, out dblcreate);

                    this.FPinOutAction.SetValue(i, 1);

                    try
                    {
                        if (dblcreate >= 0.5)
                        {
                            if (!Directory.Exists(Path.GetDirectoryName(path)))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(path));
                            }
                        }

                        path = path != null ? path : String.Empty;

                        StreamWriter sw = new StreamWriter(path, dblappend >= 0.5);
                        sw.Write(content);
                        sw.Close();

                        this.FPinOutMessage.SetString(i, "OK");
                        this.FPinOutSuccess.SetValue(i, 1);
                    }
                    catch (Exception ex)
                    {
                        this.FPinOutMessage.SetString(i, ex.Message);
                        this.FPinOutSuccess.SetValue(i, 0);
                    }

                    
                }
                else
                {
                    this.FPinOutAction.SetValue(i, 1);
                    this.FPinOutMessage.SetString(i, "Empty file");
                    this.FPinOutSuccess.SetValue(i, 0);
                }
            }
            else
            {
                this.FPinOutAction.SetValue(i, 0);
                this.FPinOutMessage.SetString(i, "");
                this.FPinOutSuccess.SetValue(i, 0);
            }
        }
    }
    #endregion

    #region Dispose
    public void Dispose()
    {
    }
    #endregion
}
        
        
