using System;
using System.Collections.Generic;
using System.Text;
using BassSound.Encoding.Internals;
using Un4seen.Bass.Misc;
using VVVV.PluginInterfaces.V1;

namespace BassSound.Encoding
{
    public class WavEncodingNode : AbstractEncoderNode<EncoderWAV>, IPlugin, IDisposable
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "WAVEncoder";
                Info.Category = "Bass";
                Info.Version = "";
                Info.Help = "Wav encoder";
                Info.Bugs = "";
                Info.Credits = "";
                Info.Warnings = "";
                Info.Author = "vux";
                Info.Tags = "Audio,Sound,Encoding";

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

        private EncoderWAV FEncoderWAV;

        #region Get Encoder
        protected override EncoderWAV GetEncoder(int handle, out string msg)
        {
            EncoderWAV wav = new EncoderWAV(handle);
            wav.InputFile = null;
            wav.OutputFile = this.FFilename;

            if (wav.EncoderExists)
            {
                this.FEncoderWAV = wav;
                msg = "OK";
                return wav;
            }
            else
            {
                this.FEncoderWAV = null;
                msg = "Can't start wma";
                return null;
            }
        }
        #endregion

        #region On Plugin Host Set
        protected override void OnPluginHostSet()
        {

        }
        #endregion
    }
}
