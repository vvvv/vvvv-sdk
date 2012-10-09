using System;
using System.Collections.Generic;
using System.Text;
using Un4seen.Bass.AddOn.Fx;
using VVVV.PluginInterfaces.V1;
using Un4seen.Bass;

namespace vvvv.Nodes
{
    public class DistorsionDspNode : AbstractDSPNode<BASS_BFX_DISTORTION>, IPlugin
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "Distorsion";
                Info.Category = "Bass";
                Info.Version = "";
                Info.Help = "Distorsion DSP for a stream";
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
        private IValueIn FPinInDrive;
        private IValueIn FPinInDry;
        private IValueIn FPinInFeedback;
        private IValueIn FPinInVolume;
        private IValueIn FPinInWet;

        protected override BASSFXType EffectType
        {
            get { return BASSFXType.BASS_FX_BFX_DISTORTION; }
        }
        #endregion

        #region On Plugin Host Set
        protected override void OnPluginHostSet()
        {
            
            this.FHost.CreateValueInput("Drive", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInDrive);
            this.FPinInDrive.SetSubType(0, 5, 0.01, 0, false, false, false);
        
            this.FHost.CreateValueInput("Dry Mix", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInDry);
            this.FPinInDry.SetSubType(-5,5, 0.01, 0, false, false, false);

            this.FHost.CreateValueInput("Feedback", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInFeedback);
            this.FPinInFeedback.SetSubType(-1, 1, 0.01, 0, false, false, false);

            this.FHost.CreateValueInput("Volume", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInVolume);
            this.FPinInVolume.SetSubType(0, 2, 0.01, 0, false, false, false);

            this.FHost.CreateValueInput("Wet Mix", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInWet);
            this.FPinInWet.SetSubType(-5, 5, 0.01, 0, false, false, false);

            this.FDsp = new BASS_BFX_DISTORTION();
        }
        #endregion

        #region On Evaluate
        protected override void OnEvaluate()
        {
            if (this.FPinInDry.PinIsChanged || 
                this.FPinInFeedback.PinIsChanged ||
                this.FPinInDrive.PinIsChanged ||
                this.FPinInVolume.PinIsChanged ||
                this.FPinInWet.PinIsChanged      
                )
            {
                double drive,dry, feedback, volume, wet;
                this.FPinInDrive.GetValue(0, out drive);
                this.FPinInDry.GetValue(0, out dry);
                this.FPinInFeedback.GetValue(0, out feedback);
                this.FPinInVolume.GetValue(0, out volume);
                this.FPinInWet.GetValue(0, out wet);

                this.FDsp.fDryMix = (float)dry;
                this.FDsp.fFeedback = (float)feedback;
                this.FDsp.fDrive = (float)drive;
                this.FDsp.fVolume = (float)volume;
                this.FDsp.fWetMix = (float)wet;

                this.UpdateDSP();
            }
        }
        #endregion
    }
}
