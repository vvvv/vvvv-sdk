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
    public class UserInfoNode : IPlugin
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "UserInfo";							//use CamelCaps and no spaces
                Info.Category = "Facebook";						//try to use an existing one
                Info.Version = "";						//versions are optional. leave blank if not needed
                Info.Help = "Gets a user info on facebook";
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

        private IStringIn FPinInUsers;

        private IStringOut FPinOutFirstName;
        private IStringOut FPinOutSurname;
        private IStringOut FPinOutBirthday;
        private IStringOut FPinOutPicture;
        private IStringOut FPinOutStatus;
        private IStringOut FPinOutGender;
        private IStringOut FPinOutInterests;
        

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;


            this.FHost.CreateStringInput("Users", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInUsers);

            this.FHost.CreateStringOutput("First Name", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutFirstName);
            this.FHost.CreateStringOutput("Surname", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutSurname);
            this.FHost.CreateStringOutput("Birthday", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutBirthday);
            this.FHost.CreateStringOutput("Picture", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutPicture);
            this.FHost.CreateStringOutput("Status", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutStatus);
            this.FHost.CreateStringOutput("Gender", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutGender);
            this.FHost.CreateStringOutput("Interests", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutInterests);

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
            if (this.FPinInUsers.PinIsChanged)
            {
                Collection<string> ids = new Collection<string>();
                for (int i = 0; i < this.FPinInUsers.SliceCount; i++) 
                {
                    string id;
                    this.FPinInUsers.GetString(i,out id);
                    ids.Add(id);
                }

                Collection<User> users = api.GetUserInfo(ids);
                this.FPinOutFirstName.SliceCount = users.Count;
                this.FPinOutSurname.SliceCount = users.Count;
                this.FPinOutPicture.SliceCount = users.Count;
                this.FPinOutGender.SliceCount = users.Count;
                this.FPinOutInterests.SliceCount = users.Count;

                string tmp = Path.GetTempPath();

                for (int i = 0; i < users.Count; i++)
                {
                    this.FPinOutFirstName.SetString(i, users[i].FirstName);
                    this.FPinOutSurname.SetString(i,users[i].LastName);           
                    this.FPinOutPicture.SetString(i, users[i].PictureUrl.AbsoluteUri);

                    if (users[i].Status != null)
                    {
                        this.FPinOutStatus.SetString(i, users[i].Status.Message);
                    }
                    else
                    {
                        this.FPinOutStatus.SetString(i, "");
                    }

                    this.FPinOutGender.SetString(i, users[i].Sex.ToString());
                    this.FPinOutInterests.SetString(i, users[i].Interests);

                    if (users[i].Birthday.HasValue)
                    {
                        this.FPinOutBirthday.SetString(i, users[i].Birthday.Value.ToShortDateString());
                    }
                    else
                    {
                        this.FPinOutBirthday.SetString(i, "");
                    }
                    
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
