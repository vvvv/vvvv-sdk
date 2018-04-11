using System;
using System.Collections.Generic;
using System.Text;

using VVVV.PluginInterfaces.V1;
using VVVV.Lib;

namespace VVVV.Nodes
{
    
    public unsafe class Particle2dfieldNode : IPlugin, IDisposable
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "Particles";							//use CamelCaps and no spaces
                Info.Category = "2d";						//try to use an existing one
                Info.Version = "Field";						//versions are optional. leave blank if not needed
                Info.Help = "2d Velocity field based particle system";
                Info.Bugs = "";
                Info.Credits = "";								//give credits to thirdparty code used
                Info.Warnings = "";
                Info.Author = "vux";

                //leave below as is
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                System.Diagnostics.StackFrame sf = st.GetFrame(0);
                System.Reflection.MethodBase method = sf.GetMethod();
                Info.Namespace = method.DeclaringType.Namespace;
                Info.Class = method.DeclaringType.Name;
                return Info;
                //leave above as is
            }
        }
        #endregion

        #region Fields
        private IPluginHost FHost;

        private IValueIn FPinInPosition;
        private IValueIn FPinInAge;
        private IValueIn FPinInAgeDeviation;
        private IValueIn FPinInVelocityDeviation;

        private IValueIn FPInInFlip;

        private IValueIn FPinInDtAge;
        private IValueIn FPinInDtVelocity;

        private IValueIn FPinInGridSize;
        private IValueFastIn FPinInField;

        private IValueIn FPinInEmit;

        private IValueIn FPinInMaxP;
        private IValueIn FPinInReset;

        private IValueOut FPinOutPosition;
        private IValueOut FPinOutPrevious;
        private IValueOut FPinOutAge;

        private int FSizeX;
        private int FSizeY;

        private ParticleSystem FParticleSystem = new ParticleSystem();
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return true; }
        }
        #endregion

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            //assign host
            this.FHost = Host;
     
            this.FHost.CreateValueInput("Position", 2, null , TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInPosition);
            this.FPinInPosition.SetSubType2D(double.MinValue, double.MaxValue, 0.01, 0, 0, false, false, false);
       
            this.FHost.CreateValueInput("Age", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInAge);
            this.FPinInAge.SetSubType(0, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueInput("Age Deviation", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInAgeDeviation);
            this.FPinInAgeDeviation.SetSubType(0, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueInput("Velocity Deviation", 2, null , TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInVelocityDeviation);
            this.FPinInVelocityDeviation.SetSubType2D(double.MinValue, double.MaxValue, 0.01, 0, 0, false, false, false);

            this.FHost.CreateValueInput("Grid Size", 2, null , TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInGridSize);
            this.FPinInGridSize.SetSubType2D(double.MinValue, double.MaxValue, 0.01, 0, 0, false, false, false);

            this.FHost.CreateValueFastInput("Velocity Field", 2, null , TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInField);
            this.FPinInField.SetSubType2D(double.MinValue, double.MaxValue, 0.01, 0, 0, false, false, false);

            this.FHost.CreateValueInput("Age Time Step", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInDtAge);
            this.FPinInDtAge.SetSubType(0, double.MaxValue, 0.01, 0.01, false, false, false);

            this.FHost.CreateValueInput("Velocity Time Step", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInDtVelocity);
            this.FPinInDtVelocity.SetSubType(0, double.MaxValue, 0.01, 0.01, false, false, false);
       
            this.FHost.CreateValueInput("Flip", 2, null , TSliceMode.Dynamic, TPinVisibility.True, out this.FPInInFlip);
            this.FPInInFlip.SetSubType2D(0, 1, 1, 0, 0, false, false, false);
        
            this.FHost.CreateValueInput("Count", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInMaxP);
            this.FPinInMaxP.SetSubType(1, double.MaxValue, 1, 7000, false, false, true);
      
            this.FHost.CreateValueInput("Reset", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInReset);
            this.FPinInReset.SetSubType(0, 1, 1, 0, true, false, false);
        
            this.FHost.CreateValueInput("Emit", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInEmit);
            this.FPinInEmit.SetSubType(0, 1, 1, 0, true, false, false);
        
        
            this.FHost.CreateValueOutput("Position", 2, null , TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutPosition);
            this.FPinOutPosition.SetSubType2D(double.MinValue, double.MaxValue, 0.01, 0, 0, false, false, false);

            this.FHost.CreateValueOutput("Previous", 2, null , TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutPrevious);
            this.FPinOutPrevious.SetSubType2D(double.MinValue, double.MaxValue, 0.01, 0, 0, false, false, false);

            this.FHost.CreateValueOutput("Age", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutAge);
            this.FPinOutAge.SetSubType(0, 1, 0.01, 0, false, false, false);
 
        }
        #endregion

        #region Configurate
        public void Configurate(IPluginConfig Input)
        {
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            bool hasreset = false;
            try
            {

                double dblreset;
                this.FPinInReset.GetValue(0, out dblreset);
                if (dblreset >= 0.5 || this.FPinInMaxP.PinIsChanged)
                {
                    double m;
                    this.FPinInMaxP.GetValue(0, out m);
                    this.FParticleSystem = new ParticleSystem(Convert.ToInt32(m));
                    hasreset = true;
                }

                if (this.FPinInGridSize.PinIsChanged || hasreset)
                {
                    double x, y;
                    this.FPinInGridSize.GetValue2D(0, out x, out y);
                    this.FSizeX = Convert.ToInt32(x);
                    this.FSizeY = Convert.ToInt32(y);
                }

                if (this.FPinInDtAge.PinIsChanged
                    || this.FPinInDtVelocity.PinIsChanged || hasreset)
                {
                    double dtage, dtvel;
                    this.FPinInDtAge.GetValue(0, out dtage);
                    this.FPinInDtVelocity.GetValue(0, out dtvel);
                    this.FParticleSystem.DtAge = dtage;
                    this.FParticleSystem.DtVelocity = dtvel;
                }

                if (this.FPInInFlip.PinIsChanged
                    || hasreset)
                {
                    double fx, fy;
                    this.FPInInFlip.GetValue2D(0, out fx, out fy);
                    this.FParticleSystem.FlipX = fx >= 0.5;
                    this.FParticleSystem.FlipY = fy >= 0.5;
                }

                double dblemit;
                this.FPinInEmit.GetValue(0, out dblemit);

                if (dblemit >= 0.5)
                {
                    int slicemax = Math.Max(this.FPinInPosition.SliceCount, this.FPinInAge.SliceCount);
                    for (int i = 0; i < slicemax; i++)
                    {
                        double x, y, age, devage, devx, devy;
                        this.FPinInPosition.GetValue2D(i, out x, out y);
                        this.FPinInAge.GetValue(i, out age);
                        this.FPinInAgeDeviation.GetValue(i, out devage);
                        this.FPinInVelocityDeviation.GetValue2D(i, out devx, out devy);
                        this.FParticleSystem.AddParticle(x, y, age, devage, devx, devy);
                    }
                }

                List<Particle> result = this.FParticleSystem.Update(this.FPinInField, this.FSizeX, this.FSizeY);

                this.FPinOutPosition.SliceCount = result.Count;
                this.FPinOutPrevious.SliceCount = result.Count;
                this.FPinOutAge.SliceCount = result.Count;

                double* dblage, dblpos, dblprev;
                this.FPinOutAge.GetValuePointer(out dblage);
                this.FPinOutPosition.GetValuePointer(out dblpos);
                this.FPinOutPrevious.GetValuePointer(out dblprev);

                for (int i = 0; i < result.Count; i++)
                {
                    Particle p = result[i];
                    dblage[i] = p.Age;
                    dblpos[i * 2] = p.PositionX;
                    dblpos[i * 2 + 1] = p.PositionY;
                    dblprev[i * 2] = p.PreviousX;
                    dblprev[i * 2 + 1] = p.PreviousY;
                }
            }
            catch (Exception ex)
            {
                this.FHost.Log(TLogType.Error, ex.Message + ":" + ex.StackTrace);
            }

        }
        #endregion

        #region Dispose
        public void Dispose()
        {
        }
        #endregion


    }
        
        
}
