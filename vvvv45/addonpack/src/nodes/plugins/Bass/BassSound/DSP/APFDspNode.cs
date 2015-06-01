using System;
using System.Collections.Generic;
using System.Text;
using Un4seen.Bass.AddOn.Fx;
using VVVV.PluginInterfaces.V1;
using Un4seen.Bass;

namespace vvvv.Nodes
{
    public class APFDSPNode : AbstractDSPNode<BASS_BFX_APF>,IPlugin
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "APF";
                Info.Category = "Bass";
                Info.Version = "";
                Info.Help = "All Pass Filter DSP for a stream";
                Info.Bugs = "";
                Info.Credits = "";
                Info.Warnings = "";
                Info.Author = "vux";
                Info.Tags = "Audio,Sound,DSP";

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

        #region Properties
        private IValueIn FPinInGain;
        private IValueIn FPinInDelay;

        protected override BASSFXType EffectType
        {
            get { return BASSFXType.BASS_FX_BFX_APF; }
        }
        #endregion

        #region On Plugin Host Set
        protected override void OnPluginHostSet()
        {
            this.FHost.CreateValueInput("Gain", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInGain);
            this.FPinInGain.SetSubType(-1, 1, 0.01, 0, false, false, false);

            this.FHost.CreateValueInput("Delay", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInDelay);
            this.FPinInDelay.SetSubType(0, 6000, 1, 1, false, false, true);

            this.FDsp = new BASS_BFX_APF();
        }
        #endregion

        #region On Evaluate
        protected override void OnEvaluate()
        {
            if (this.FPinInDelay.PinIsChanged || this.FPinInGain.PinIsChanged)
            {
                double dgain, ddelay;
                this.FPinInGain.GetValue(0, out dgain);
                this.FPinInDelay.GetValue(0, out ddelay);

                this.FDsp.fGain = (float)dgain;
                this.FDsp.fDelay = Convert.ToSingle(ddelay / 1000.0);

                this.UpdateDSP();
            }
        }
        #endregion
    }
}
