using System;
using System.Collections.Generic;
using System.Text;
using Un4seen.Bass.AddOn.Fx;
using VVVV.PluginInterfaces.V1;
using Un4seen.Bass;

namespace vvvv.Nodes
{
    public class ReverbDspNode : AbstractDSPNode<BASS_BFX_REVERB>,IPlugin
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "Reverb";
                Info.Category = "Bass";
                Info.Version = "";
                Info.Help = "Reverb DSP for a stream";
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
        private IValueIn FPinInLevel;
        private IValueIn FPinInDelay;

        protected override BASSFXType EffectType
        {
            get { return BASSFXType.BASS_FX_BFX_REVERB; }
        }
        #endregion

        #region On Plugin Host Set
        protected override void OnPluginHostSet()
        {
            this.FHost.CreateValueInput("Level", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInLevel);
            this.FPinInLevel.SetSubType(0, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueInput("Delay", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInDelay);
            this.FPinInDelay.SetSubType(1200, 10000, 1, 1200, false, false, true);

            this.FDsp = new BASS_BFX_REVERB();
        }
        #endregion

        #region On Evaluate
        protected override void OnEvaluate()
        {
            if (this.FPinInDelay.PinIsChanged || this.FPinInLevel.PinIsChanged)
            {
                double dlevel, ddelay;
                this.FPinInLevel.GetValue(0, out dlevel);
                this.FPinInDelay.GetValue(0, out ddelay);

                this.FDsp.fLevel = (float)dlevel;
                this.FDsp.lDelay = Convert.ToInt32(ddelay);

                this.UpdateDSP();
            }
        }
        #endregion
    }
}
