using System;
using System.Collections.Generic;
using System.Text;
using BassSound.Encoding.Internals;
using Un4seen.Bass.Misc;
using VVVV.PluginInterfaces.V1;

namespace BassSound.Encoding
{
    public class WmaEncodingNode : AbstractEncoderNode<EncoderWMA>, IPlugin, IDisposable
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "WMAEncoder";
                Info.Category = "Bass";
                Info.Version = "";
                Info.Help = "Wma encoder";
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

        private EncoderWMA FEncoderWMA;

        private IValueIn FPinInQuality;

        #region Get Encoder
        protected override EncoderWMA GetEncoder(int handle, out string msg)
        {
            EncoderWMA wma = new EncoderWMA(handle);
            wma.InputFile = null;
            wma.OutputFile = this.FFilename;
            wma.WMA_UseVBR = true;

            if (wma.EncoderExists)
            {
                this.FEncoderWMA = wma;
                msg = "OK";
                return wma;
            }
            else
            {
                this.FEncoderWMA = null;
                msg = "Can't start wma";
                return null;
            }
        }
        #endregion

        #region On Plugin Host Set
        protected override void OnPluginHostSet()
        {
            this.FHost.CreateValueInput("Quality", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInQuality);
            this.FPinInQuality.SetSubType(0, 100, 1, 100, false, false, true);
        }
        #endregion
    }
}
