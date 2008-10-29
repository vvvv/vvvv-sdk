using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using Facebook.API;
using Facebook.Entity;
using vvvv.Nodes.Internal;
using System.Collections.ObjectModel;
using System.IO;

namespace vvvv.Nodes
{
    public class GroupInfoNode : IPlugin
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "GroupInfo";							//use CamelCaps and no spaces
                Info.Category = "Facebook";						//try to use an existing one
                Info.Version = "";						//versions are optional. leave blank if not needed
                Info.Help = "Gets a group info on facebook";
                Info.Bugs = "";
                Info.Credits = "";
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

        private IPluginHost FHost;
        private FacebookAPI api;

        private IStringIn FPinInGroups;

        private IStringOut FPinOutName;
        private IStringOut FPinOutDescription;
        private IStringOut FPinOutTypes;

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;

            this.FHost.CreateStringInput("Groups", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInGroups);
            this.FHost.CreateStringOutput("Names", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutName);
            this.FHost.CreateStringOutput("Description", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutDescription);
            this.FHost.CreateStringOutput("Types", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutTypes);

            this.api = APIShared.API;
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
            if (this.FPinInGroups.PinIsChanged)
            {
                Collection<string> ids = new Collection<string>();
                for (int i = 0; i < this.FPinInGroups.SliceCount; i++) 
                {
                    string id;
                    this.FPinInGroups.GetString(i,out id);
                    ids.Add(id);
                }

                Collection<Group> groups = api.GetGroups(ids);
                this.FPinOutName.SliceCount = groups.Count;
                this.FPinOutTypes.SliceCount = groups.Count;
                this.FPinOutDescription.SliceCount = groups.Count;

                string tmp = Path.GetTempPath();

                for (int i = 0; i < groups.Count; i++)
                {
                    this.FPinOutName.SetString(i, groups[i].Name);
                    this.FPinOutTypes.SetString(i, groups[i].Type);
                    this.FPinOutDescription.SetString(i, groups[i].Description);
                }       
            }
        }
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return false; }
        }
        #endregion

        #region Clear API
        private void ClearAPI()
        {
            try
            {
                api.LogOff();
            }
            catch
            {

            }
        }
        #endregion

    }
}
