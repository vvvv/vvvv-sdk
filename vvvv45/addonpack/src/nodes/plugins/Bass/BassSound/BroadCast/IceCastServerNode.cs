using System;
using System.Collections.Generic;
using System.Text;
using Un4seen.Bass.Misc;
using VVVV.PluginInterfaces.V1;

namespace BassSound.BroadCast
{
    public class IceCastServerNode : AbstractBroadCastNode<ICEcast>,IPlugin,IDisposable
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "IceCast";
                Info.Category = "Bass";
                Info.Version = "";
                Info.Help = "Broadcasts a channel to IceCast";
                Info.Bugs = "";
                Info.Credits = "";
                Info.Warnings = "";
                Info.Author = "vux";
                Info.Tags = "Audio,Sound,Broadcast";

                //leave below as is
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                System.Diagnostics.StackFrame sf = st.GetFrame(0);
                System.Reflection.MethodBase method = sf.GetMethod();
                Info.Namespace = method.DeclaringType.Namespace;
                Info.Class = method.DeclaringType.Name;
                return Info;
            }
        }
        #endregion

        private IStringIn FPinInMountPoint;
        private IStringIn FPinInName;
        private IStringIn FPinInDesc;
        private IStringIn FPinInGenre;
        private IStringIn FPinSongTitle;
        private IStringIn FPinSongUrl;

        #region Get Stream Class
        protected override ICEcast GetStreamClass(int handle, out string msg)
        {
            string mountpoint,name,desc,genre;
            double rate;
            int irate;

            this.FPinInMountPoint.GetString(0, out mountpoint);
            this.FPinInGenre.GetString(0, out genre);
            this.FPinInDesc.GetString(0, out desc);
            this.FPinInName.GetString(0, out name);
            this.FPinInBitrate.GetValue(0, out rate);
            irate = Convert.ToInt32(rate);

            EncoderOGG ogg = new EncoderOGG(handle);
            ogg.InputFile = null;
            ogg.OutputFile = null;
            ogg.EncoderDirectory = AppDomain.CurrentDomain.BaseDirectory + "\\bin";
            ogg.OGG_UseQualityMode = false;
            ogg.OGG_Bitrate = irate;
            ogg.OGG_MinBitrate = irate;
            ogg.OGG_MaxBitrate = irate;
            ogg.OGG_UseManagedBitrate = true;
            ogg.Start(null, IntPtr.Zero, true);
            ogg.Pause(false);

            
            string server, pwd;
            double port;

            this.FPinInServer.GetString(0, out server);
            this.FPinInPort.GetValue(0, out port);
            this.FPinInPwd.GetString(0, out pwd);

            ICEcast ice = new ICEcast(ogg,true);
            ice.MountPoint = mountpoint;
            
            ice.StreamName = name;
            ice.StreamDescription = desc;
            ice.StreamGenre = genre;

            ice.Password = pwd;
            ice.ServerAddress = server;
            ice.ServerPort = Convert.ToInt32(port);
            ice.SongTitle = "Vux Test";
            ice.Username = "source";
            

            msg = "OK";
            return ice;
        }
        #endregion

        #region On Plugin Host Set
        protected override void OnPluginHostSet()
        {
            this.FHost.CreateStringInput("Mount Point", TSliceMode.Single, TPinVisibility.True, out this.FPinInMountPoint);
            this.FPinInMountPoint.SetSubType("/vvvv.ogg", false);

            this.FHost.CreateStringInput("Station Name", TSliceMode.Single, TPinVisibility.True, out this.FPinInName);
            this.FPinInName.SetSubType("vvvv", false);

            this.FHost.CreateStringInput("Station Description", TSliceMode.Single, TPinVisibility.True, out this.FPinInDesc);
            this.FPinInDesc.SetSubType("vvvv broadcast", false);

            this.FHost.CreateStringInput("Station Genre", TSliceMode.Single, TPinVisibility.True, out this.FPinInGenre);
            this.FPinInGenre.SetSubType("music", false);

            this.FHost.CreateStringInput("Song Title", TSliceMode.Single, TPinVisibility.True, out this.FPinSongTitle);
            this.FPinSongTitle.SetSubType("", false);

            this.FHost.CreateStringInput("Song URL", TSliceMode.Single, TPinVisibility.True, out this.FPinSongUrl);
            this.FPinSongUrl.SetSubType("", false);
        }
        #endregion

        protected override void BeginEvaluate()
        {

        }

        #region End Evaluate
        protected override void EndEvaluate()
        {
            if (this.IsConnected && this.FPinSongTitle.PinIsChanged)
            {
                string title,url;
                this.FPinSongTitle.GetString(0, out title);
                this.FPinSongUrl.GetString(0, out url);

                this.FBroadCast.UpdateTitle(title, url);
            }
        }
        #endregion
    }
}
