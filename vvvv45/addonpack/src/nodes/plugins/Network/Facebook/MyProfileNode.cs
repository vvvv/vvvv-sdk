using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using Facebook.API;
using vvvv.Nodes.Internal;
using Facebook.Entity;

namespace vvvv.Nodes
{
    public class FaceBookProfileNode : IPlugin, IDisposable
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "MyProfile";							//use CamelCaps and no spaces
                Info.Category = "Facebook";						//try to use an existing one
                Info.Version = "";						//versions are optional. leave blank if not needed
                Info.Help = "Gets your profile information in facebook";
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

        private IStringIn FPinInAppKey;
        private IStringIn FPinInSecret;

        private IStringOut FPinOutFriends;
        private IStringOut FPinOutGroups;

        private bool FInvalidate = false;

        private FacebookAPI api;

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;

            this.FHost.CreateStringInput("Application Key", TSliceMode.Single, TPinVisibility.True, out this.FPinInAppKey);
            this.FHost.CreateStringInput("Secret Key", TSliceMode.Single, TPinVisibility.True, out this.FPinInSecret);

            this.FHost.CreateStringOutput("Friends", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutFriends);
            this.FHost.CreateStringOutput("Groups", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutGroups);

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
            if (this.FPinInAppKey.PinIsChanged || this.FPinInSecret.PinIsChanged)
            {
                this.ClearAPI();
                string appkey;
                string appsec;

                this.FPinInAppKey.GetString(0, out appkey);
                this.FPinInSecret.GetString(0, out appsec);

                if (appkey != "" && appsec != "")
                {

                    api.ApplicationKey = appkey;
                    api.IsDesktopApplication = true;
                    api.Secret = appsec;
                    api.ConnectToFacebook();

                    this.FInvalidate = true;
                }
            }

            if (this.FInvalidate)
            {
                IList<string> frids = api.GetFriendIds();
                this.FPinOutFriends.SliceCount = frids.Count;
                for (int i = 0; i < frids.Count; i++)
                {
                    this.FPinOutFriends.SetString(i, frids[i]);
                }

                IList<Group> grpids = api.GetGroups();
                this.FPinOutGroups.SliceCount = grpids.Count;
                for (int i = 0; i < grpids.Count; i++)
                {
                    this.FPinOutGroups.SetString(i, grpids[i].GroupId);
                }


                this.FInvalidate = false;
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

        #region IDisposable Members

        public void Dispose()
        {
            this.ClearAPI();
        }

        #endregion
    }
}
