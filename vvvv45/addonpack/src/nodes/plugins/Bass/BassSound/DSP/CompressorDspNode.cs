using System;
using System.Collections.Generic;
using System.Text;
using Un4seen.Bass.AddOn.Fx;
using VVVV.PluginInterfaces.V1;
using Un4seen.Bass;

namespace vvvv.Nodes
{
    public class CompressorDSPNode : AbstractDSPNode<BASS_BFX_COMPRESSOR>, IPlugin
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "Compressor";
                Info.Category = "Bass";
                Info.Version = "";
                Info.Help = "Compressor DSP for a stream";
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
        private IValueIn FPinInAttackTime;
        private IValueIn FPinInReleaseTime;
        private IValueIn FPinInThreshold;

        protected override BASSFXType EffectType
        {
            get { return BASSFXType.BASS_FX_BFX_COMPRESSOR; }
        }
        #endregion

        #region On Plugin Host Set
        protected override void OnPluginHostSet()
        {
            this.FHost.CreateValueInput("Attack Time", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInAttackTime);
            this.FPinInAttackTime.SetSubType(0,1000,0.01,0, false, false, false);

            this.FHost.CreateValueInput("Release Time", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInReleaseTime);
            this.FPinInReleaseTime.SetSubType(0, 5000, 0.01, 0, false, false, false);

            this.FHost.CreateValueInput("Threshold", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInThreshold);
            this.FPinInThreshold.SetSubType(0, 1, 0.01, 0, false, false, false);

            this.FDsp = new BASS_BFX_COMPRESSOR();
        }
        #endregion

        #region On Evaluate
        protected override void OnEvaluate()
        {
            if (this.FPinInAttackTime.PinIsChanged || 
                this.FPinInReleaseTime.PinIsChanged || 
                this.FPinInThreshold.PinIsChanged)
            {
                double dattack, drelease, dthreshold;
                this.FPinInAttackTime.GetValue(0, out dattack);
                this.FPinInReleaseTime.GetValue(0, out drelease);
                this.FPinInThreshold.GetValue(0, out dthreshold);

                this.FDsp.fAttacktime = (float)dattack;
                this.FDsp.fReleasetime = (float)drelease;
                this.FDsp.fThreshold = (float)dthreshold;

                this.UpdateDSP();
            }
        }
        #endregion
    }
}
