using System;
using System.Collections.Generic;
using System.Text;

using VVVV.PluginInterfaces.V1;
using VVVV.Lib;
using System.Runtime.InteropServices;

namespace VVVV.Nodes
{
    
    public unsafe class Particle3dfieldNode : IPlugin, IDisposable
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "Particles";							//use CamelCaps and no spaces
                Info.Category = "3d";						//try to use an existing one
                Info.Version = "Field";						//versions are optional. leave blank if not needed
                Info.Help = "3d Velocity field based particle system";
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
        private int FSizeZ;

        private ParticleSystem3d FParticleSystem = new ParticleSystem3d();
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return false; }
        }
        #endregion

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            //assign host
            this.FHost = Host;
   
            this.FHost.CreateValueInput("Position", 3, null , TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInPosition);
            this.FPinInPosition.SetSubType3D(double.MinValue, double.MaxValue, 0.01, 0, 0,0, false, false, false);
       
            this.FHost.CreateValueInput("Age", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInAge);
            this.FPinInAge.SetSubType(0, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueInput("Age Deviation", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInAgeDeviation);
            this.FPinInAgeDeviation.SetSubType(0, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueInput("Velocity Deviation", 3, null , TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInVelocityDeviation);
            this.FPinInVelocityDeviation.SetSubType3D(double.MinValue, double.MaxValue, 0.01, 0, 0,0, false, false, false);

            this.FHost.CreateValueInput("Box Size", 3, null , TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInGridSize);
            this.FPinInGridSize.SetSubType3D(double.MinValue, double.MaxValue, 0.01, 0,0, 0, false, false, false);

            this.FHost.CreateValueFastInput("Velocity Field", 3, null , TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInField);
            this.FPinInField.SetSubType3D(double.MinValue, double.MaxValue, 0.01, 0, 0,0, false, false, false);

            this.FHost.CreateValueInput("Age Time Step", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInDtAge);
            this.FPinInDtAge.SetSubType(0, double.MaxValue, 0.01, 0.01, false, false, false);

            this.FHost.CreateValueInput("Velocity Time Step", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInDtVelocity);
            this.FPinInDtVelocity.SetSubType(0, double.MaxValue, 0.01, 0.01, false, false, false);


            this.FHost.CreateValueInput("Count", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInMaxP);
            this.FPinInMaxP.SetSubType(1, double.MaxValue, 1, 7000, false, false, true);
            
            this.FHost.CreateValueInput("Reset", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInReset);
            this.FPinInReset.SetSubType(0, 1, 1, 0, true, false, false);
        
            this.FHost.CreateValueInput("Emit", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInEmit);
            this.FPinInEmit.SetSubType(0, 1, 1, 0, true, false, false);
        
        
            this.FHost.CreateValueOutput("Position", 3, null , TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutPosition);
            this.FPinOutPosition.SetSubType3D(double.MinValue, double.MaxValue, 0.01, 0, 0,0, false, false, false);

            this.FHost.CreateValueOutput("Previous", 3, null , TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutPrevious);
            this.FPinOutPrevious.SetSubType3D(double.MinValue, double.MaxValue, 0.01, 0, 0, 0, false, false, false);

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
            double dblreset;
            bool reset = false;
            this.FPinInReset.GetValue(0, out dblreset);
            if (dblreset >= 0.5 || this.FPinInMaxP.PinIsChanged)
            {
                double m;
                this.FPinInMaxP.GetValue(0, out m);
                this.FParticleSystem = new ParticleSystem3d(Convert.ToInt32(m));
                reset = true;
            }

            if (this.FPinInGridSize.PinIsChanged || reset)
            {
                double x, y,z;
                this.FPinInGridSize.GetValue3D(0, out x, out y,out z);
                this.FSizeX = Convert.ToInt32(x);
                this.FSizeY = Convert.ToInt32(y);
                this.FSizeZ = Convert.ToInt32(z);
            }

            if (this.FPinInDtAge.PinIsChanged
                || this.FPinInDtVelocity.PinIsChanged || reset)
            {
                double dtage,dtvel;
                this.FPinInDtAge.GetValue(0, out dtage);
                this.FPinInDtVelocity.GetValue(0, out dtvel);
                this.FParticleSystem.DtAge = dtage;
                this.FParticleSystem.DtVelocity = dtvel;
            }

            double dblemit;
            this.FPinInEmit.GetValue(0, out dblemit);

            if (dblemit >= 0.5)
            {
                int slicemax = Math.Max(this.FPinInPosition.SliceCount, this.FPinInAge.SliceCount);
                for (int i = 0; i < slicemax; i++)
                {
                    double x, y,z, age,devage,devx,devy,devz;
                    this.FPinInPosition.GetValue3D(i, out x, out y,out z);
                    this.FPinInAge.GetValue(i, out age);
                    this.FPinInAgeDeviation.GetValue(i, out devage);
                    this.FPinInVelocityDeviation.GetValue3D(i, out devx, out devy, out devz);
                    this.FParticleSystem.AddParticle(x, y,z, age,devage,devx,devy,devz);
                }
            }

            List<Particle3d> result = this.FParticleSystem.Update(this.FPinInField,this.FSizeX,this.FSizeY,this.FSizeZ);

            this.FPinOutPosition.SliceCount = result.Count;
            this.FPinOutPrevious.SliceCount = result.Count;
            this.FPinOutAge.SliceCount = result.Count;

            double* dblage, dblpos, dblprev;
            this.FPinOutAge.GetValuePointer(out dblage);
            this.FPinOutPosition.GetValuePointer(out dblpos);
            this.FPinOutPrevious.GetValuePointer(out dblprev);

            for (int i = 0; i < result.Count; i++)
            {
                Particle3d p = result[i];
                dblage[i] = p.Age;
                dblpos[i * 3] = p.PositionX;
                dblpos[i * 3 + 1] = p.PositionY;
                dblpos[i * 3 + 2] = p.PositionZ;
                dblprev[i * 3] = p.PreviousX;
                dblprev[i * 3 + 1] = p.PreviousY;
                dblprev[i * 3 + 2] = p.PreviousZ;
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
