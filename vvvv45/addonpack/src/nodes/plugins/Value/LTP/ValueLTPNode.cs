using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes
{
    public enum eLTPTakeOverMode { Immediate, Pickup }
   
    public unsafe class ValueLTPNode : IPlugin, IDisposable
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "LTP";							//use CamelCaps and no spaces
                Info.Category = "Value";						//try to use an existing one
                Info.Version = "";						//versions are optional. leave blank if not needed
                Info.Help = "Slicewise LTP node, first input has highe priority";
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

        private IValueConfig FPinInNumInputs;
        private List<IValueIn> FPinInputs = new List<IValueIn>();
        private List<IEnumIn> FPinInMode = new List<IEnumIn>();
        private List<eLTPTakeOverMode> FModes = new List<eLTPTakeOverMode>();
        private IValueIn FPinInPickUpEpsilon;

        private IValueOut FPinOutput;
        private IValueOut FPinOutIdx;
        private int FInputCount = 0;
        private double FEps = 0;

        private List<double[]> FOldValues = new List<double[]>();
        private bool FFirstFrame = true;
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
            this.FHost.UpdateEnum("LTP Takeover Mode", "Immediate", Enum.GetNames(typeof(eLTPTakeOverMode)));

            this.FHost.CreateValueConfig("Inputs Count", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInNumInputs);
            this.FPinInNumInputs.SetSubType(2, double.MaxValue, 1, 2, false, false, true);
         
            this.FHost.CreateValueInput("Pickup Epsilon", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out this.FPinInPickUpEpsilon);
            FPinInPickUpEpsilon.SetSubType(0, double.MaxValue, 0.01, 0, false, false, false);
        
            this.FHost.CreateValueOutput("Output", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutput);
            this.FPinOutput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueOutput("Last Changed Pin Index", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutIdx);
            FPinOutIdx.SetSubType(0, double.MaxValue, 1, 0, false, false, true);
        

            Configurate(this.FPinInNumInputs);
        }
        #endregion

        #region Configurate
        public void Configurate(IPluginConfig Input)
        {
            double inputcnt;
            this.FPinInNumInputs.GetValue(0, out inputcnt);

            int prevcnt = this.FInputCount;
            this.FInputCount = Convert.ToInt32(inputcnt);

            if (this.FInputCount > prevcnt)
            {
                //Add new pins, as value is bigger
                while (FPinInputs.Count < this.FInputCount)
                {
                    IValueIn pin;

                    int index = FPinInputs.Count + 1;
      
                    this.FHost.CreateValueInput("Input "  + index, 1, null, TSliceMode.Dynamic, TPinVisibility.True, out pin);
                    pin.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
                    this.FPinInputs.Add(pin);

                    IEnumIn topin;

                    this.FHost.CreateEnumInput("Takeover Mode " + index, TSliceMode.Single, TPinVisibility.True, out topin);
                    topin.SetSubType("LTP Takeover Mode");
                    this.FPinInMode.Add(topin);
                    this.FModes.Add(eLTPTakeOverMode.Immediate);


                }
                this.FFirstFrame = true;
            }
            else
            {
                //Remove pins, as value is lower
                while (FPinInputs.Count > this.FInputCount)
                {
                    this.FHost.DeletePin(this.FPinInputs[this.FPinInputs.Count - 1]);
                    this.FHost.DeletePin(this.FPinInMode[this.FPinInMode.Count - 1]);
                    this.FPinInputs.RemoveAt(this.FPinInputs.Count - 1);
                    this.FPinInMode.RemoveAt(this.FPinInMode.Count - 1);
                    this.FModes.RemoveAt(this.FModes.Count - 1);
                }
                this.FFirstFrame = true;
            }

            this.FPinInPickUpEpsilon.Order = this.FInputCount + 1;
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            double* oldvals;
            this.FPinOutput.GetValuePointer(out oldvals);

            this.FPinOutput.SliceCount = SpreadMax;
            this.FPinOutIdx.SliceCount = SpreadMax;

            for (int i = 0; i < this.FPinInMode.Count; i++)
            {
                string val;
                this.FPinInMode[i].GetString(0, out val);
                this.FModes[i] = (eLTPTakeOverMode)Enum.Parse(typeof(eLTPTakeOverMode), val);
            }

            if (this.FPinInPickUpEpsilon.PinIsChanged)
            {
                this.FPinInPickUpEpsilon.GetValue(0, out this.FEps);
                //Keep absolute value here
                this.FEps = Math.Abs(this.FEps);
            }

            if (this.FFirstFrame)
            {
                //On first frame, we take the first input only (highest priority)
                double* ptr;
                int ptrcnt;
                this.FPinInputs[0].GetValuePointer(out ptrcnt, out ptr);

                for (int i = 0; i < SpreadMax; i++)
                {
                    this.FPinOutput.SetValue(i, ptr[i % ptrcnt]);
                    this.FPinOutIdx.SetValue(i, 0);
                }
                this.FFirstFrame = false;
            }
            else
            {
                bool[] updated = new bool[SpreadMax];

                for (int i = 0; i < SpreadMax; i++)
                {
                    for (int j = 0; j < this.FPinInputs.Count; j++)
                    {
                        if (!updated[i])
                        {
                            double dblval;
                            this.FPinInputs[j].GetValue(i, out dblval);
                            double oldval = this.FOldValues[j][i % this.FOldValues[j].Length];

                            double currval = oldvals[i];
                            if (dblval != oldval)
                            {
                                if (this.FModes[j] == eLTPTakeOverMode.Immediate)
                                {
                                    this.FPinOutput.SetValue(i, dblval);
                                    this.FPinOutIdx.SetValue(i, j);
                                    updated[i] = true;
                                }
                                else
                                {
                                    if ((dblval <= currval + this.FEps && oldval >= currval - this.FEps)
                                    || (dblval >= currval - this.FEps && oldval <= currval + this.FEps))
                                    {
                                        this.FPinOutput.SetValue(i, dblval);
                                        this.FPinOutIdx.SetValue(i, j);
                                        updated[i] = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }


            //Save old values
            this.FOldValues.Clear();
            for (int i = 0; i < this.FPinInputs.Count; i++)
            {
                IValueIn pin = this.FPinInputs[i];
                double[] dbl = new double[pin.SliceCount];

                double* ptr;
                int ptrcnt;
                pin.GetValuePointer(out ptrcnt,out ptr);

                for (int j = 0; j < pin.SliceCount; j++)
                {
                    dbl[j] = ptr[j];
                }
                this.FOldValues.Add(dbl);
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
