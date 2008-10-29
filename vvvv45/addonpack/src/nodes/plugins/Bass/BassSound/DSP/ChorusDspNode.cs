using System;
using System.Collections.Generic;
using System.Text;
using Un4seen.Bass.AddOn.Fx;
using VVVV.PluginInterfaces.V1;
using Un4seen.Bass;

namespace vvvv.Nodes
{
    public class ChorusDspNode : AbstractDSPNode<BASS_BFX_CHORUS>, IPlugin
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "Chorus";
                Info.Category = "Bass";
                Info.Version = "";
                Info.Help = "Chorus DSP for a stream";
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
        private IValueIn FPinInDry;
        private IValueIn FPinInFeedback;
        private IValueIn FPinInMaxSweep;
        private IValueIn FPinInMinSweep;
        private IValueIn FPinInRate;
        private IValueIn FPinInWet;

        protected override BASSFXType EffectType
        {
            get { return BASSFXType.BASS_FX_BFX_CHORUS; }
        }
        #endregion

        #region On Plugin Host Set
        protected override void OnPluginHostSet()
        {
            this.FHost.CreateValueInput("Dry Mix", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInDry);
            this.FPinInDry.SetSubType(-2,2, 0.01, 0, false, false, false);

            this.FHost.CreateValueInput("Feedback", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInFeedback);
            this.FPinInFeedback.SetSubType(-1, 1, 0.01, 0, false, false, false);

            this.FHost.CreateValueInput("Max Sweep", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInMaxSweep);
            this.FPinInMaxSweep.SetSubType(0, 6000, 1, 0, false, false, true);

            this.FHost.CreateValueInput("Min Sweep", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInMinSweep);
            this.FPinInMinSweep.SetSubType(0, 6000, 1, 0, false, false, true);

            this.FHost.CreateValueInput("Rate", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInRate);
            this.FPinInRate.SetSubType(0, 1000, 0.01, 0, false, false, false);

            this.FHost.CreateValueInput("Wet Mix", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInWet);
            this.FPinInWet.SetSubType(-2, 2, 0.01, 0, false, false, false);

            this.FDsp = new BASS_BFX_CHORUS();
        }
        #endregion

        #region On Evaluate
        protected override void OnEvaluate()
        {
            if (this.FPinInDry.PinIsChanged || 
                this.FPinInFeedback.PinIsChanged ||
                this.FPinInMaxSweep.PinIsChanged ||
                this.FPinInMinSweep.PinIsChanged ||
                this.FPinInRate.PinIsChanged ||
                this.FPinInWet.PinIsChanged      
                )
            {
                double dry, feedback, maxsweep, minsweep, rate, wet;
                this.FPinInDry.GetValue(0, out dry);
                this.FPinInFeedback.GetValue(0, out feedback);
                this.FPinInMaxSweep.GetValue(0, out maxsweep);
                this.FPinInMinSweep.GetValue(0, out minsweep);
                this.FPinInRate.GetValue(0, out rate);
                this.FPinInWet.GetValue(0, out wet);

                this.FDsp.fDryMix = (float)dry;
                this.FDsp.fFeedback = (float)feedback;
                this.FDsp.fMaxSweep = (float)maxsweep;
                this.FDsp.fMinSweep = (float)minsweep;
                this.FDsp.fRate = (float)rate;
                this.FDsp.fWetMix = (float)wet;

                this.UpdateDSP();
            }
        }
        #endregion
    }
}
