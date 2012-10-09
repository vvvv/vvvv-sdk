using System;
using System.Collections.Generic;
using System.Text;
using Un4seen.Bass.AddOn.Fx;
using VVVV.PluginInterfaces.V1;
using Un4seen.Bass;

namespace vvvv.Nodes
{
    public class FPFDSPNode : AbstractDSPNode<BASS_BFX_LPF>,IPlugin
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "LPF";
                Info.Category = "Bass";
                Info.Version = "";
                Info.Help = "Low Pass Filter DSP for a stream";
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
        private IValueIn FPinInCutOff;
        private IValueIn FPinInResonance;

        protected override BASSFXType EffectType
        {
            get { return BASSFXType.BASS_FX_BFX_LPF; }
        }
        #endregion

        #region On Plugin Host Set
        protected override void OnPluginHostSet()
        {
            this.FHost.CreateValueInput("Cut Off Frequency", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInCutOff);
            this.FPinInCutOff.SetSubType(1, double.MaxValue, 1, 200, false, false, true);

            this.FHost.CreateValueInput("Resonance", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInResonance);
            this.FPinInResonance.SetSubType(0.01, 10, 0.01,2, false, false, false);

            this.FDsp = new BASS_BFX_LPF();
        }
        #endregion

        #region On Evaluate
        protected override void OnEvaluate()
        {
            if (this.FPinInCutOff.PinIsChanged || this.FPinInResonance.PinIsChanged)
            {
                double dcutoff, dreso;
                this.FPinInCutOff.GetValue(0, out dcutoff);
                this.FPinInResonance.GetValue(0, out dreso);

                this.FDsp.fCutOffFreq = (float)dcutoff;
                this.FDsp.fResonance = (float)dreso;

                this.UpdateDSP();
            }
        }
        #endregion
    }
}
