using System;
using System.Collections.Generic;
using System.Text;
using Un4seen.Bass.AddOn.Fx;
using VVVV.PluginInterfaces.V1;
using Un4seen.Bass;

namespace vvvv.Nodes
{
    public class FlangerDspNode : AbstractDSPNode<BASS_BFX_FLANGER>, IPlugin
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "Flanger";
                Info.Category = "Bass";
                Info.Version = "";
                Info.Help = "Flanger DSP for a stream";
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
        private IValueIn FPinInSpeed;
        private IValueIn FPinInWetDry;

        protected override BASSFXType EffectType
        {
            get { return BASSFXType.BASS_FX_BFX_FLANGER; }
        }
        #endregion

        #region On Plugin Host Set
        protected override void OnPluginHostSet()
        {
            this.FHost.CreateValueInput("Speed", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInSpeed);
            this.FPinInSpeed.SetSubType(0, 0.09, 0.01, 0.01, false, false, false);

            this.FHost.CreateValueInput("Wet Dry Ratio", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInWetDry);
            this.FPinInWetDry.SetSubType(0, double.MaxValue,0.01, 1, false, false, false);

            this.FDsp = new BASS_BFX_FLANGER();
        }
        #endregion

        #region On Evaluate
        protected override void OnEvaluate()
        {
            if (this.FPinInWetDry.PinIsChanged || this.FPinInSpeed.PinIsChanged)
            {
                double dspeed, dwetdry;
                this.FPinInWetDry.GetValue(0, out dwetdry);
                this.FPinInSpeed.GetValue(0, out dspeed);

                this.FDsp.fSpeed = (float)dspeed;
                this.FDsp.fWetDry = (float)dwetdry;

                this.UpdateDSP();
            }
        }
        #endregion
    }
}
