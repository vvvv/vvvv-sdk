using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;

namespace SwitchTransAdv
{
    
    public class AdvSwitchTransNode : IPlugin, IDisposable
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "Switch";							//use CamelCaps and no spaces
                Info.Category = "Transform";						//try to use an existing one
                Info.Version = "Advanced";						//versions are optional. leave blank if not needed
                Info.Help = "Switch for transforms with spreadable switch input";
                Info.Bugs = "";
                Info.Credits = "";								//give credits to thirdparty code used
                Info.Warnings = "";
                Info.Author = "vux";
                Info.Tags = "";

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

        private IValueIn FPinInput;
        private IValueConfig FPinInNumInputs;
        private List<ITransformIn> FPinTransforms = new List<ITransformIn>();
        private ITransformOut FPinOutput;
        private int FInputCount = 0;
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
   
            this.FHost.CreateValueInput("Input", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInput);
            this.FPinInput.SetSubType(double.MinValue, double.MaxValue, 1, 0, false, false, true);

            this.FHost.CreateValueConfig("Inputs Count", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInNumInputs);
            this.FPinInNumInputs.SetSubType(2, double.MaxValue, 1,2, false, false, true);

            this.FHost.CreateTransformOutput("Output", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutput);

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
                while (FPinTransforms.Count < this.FInputCount)
                {
                    ITransformIn pin;

                    int index = FPinTransforms.Count + 1;

                    //Add new Transform pin
                    this.FHost.CreateTransformInput("Transform " + index, TSliceMode.Dynamic, TPinVisibility.True, out pin);
                    this.FPinTransforms.Add(pin);
                }
            }
            else
            {
                //Remove pins, as value is lower
                while (FPinTransforms.Count > this.FInputCount)
                {
                    this.FHost.DeletePin(this.FPinTransforms[this.FPinTransforms.Count - 1]);
                    this.FPinTransforms.RemoveAt(this.FPinTransforms.Count - 1);
                }
            }
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            bool changed = false;
            for (int i = 0; i < this.FPinTransforms.Count; i++)
            {
                if (this.FPinTransforms[i].PinIsChanged)
                {
                    changed = true;
                }
            }

            if (this.FPinInput.PinIsChanged || changed)
            {

                
                List<ITransformIn> usedpins = new List<ITransformIn>();

                if (this.FPinInput.SliceCount > 0)
                {
                    //Get spread max
                    int spmaxnonnil = this.FPinInput.SliceCount;
                    for (int i = 0; i < this.FPinTransforms.Count; i++)
                    {
                        spmaxnonnil = Math.Max(spmaxnonnil, this.FPinTransforms[i].SliceCount);
                    }

                    //Prepass for nil check and slice count
                    int spmax = this.FPinInput.SliceCount;
                    bool hasnil = false;
                    List<int> trpinidx = new List<int>();

                    for (int i = 0; i < spmaxnonnil; i++)
                    {
                        double dblswitch;
                        this.FPinInput.GetValue(i, out dblswitch);
                        int sw = Convert.ToInt32(dblswitch)% this.FPinTransforms.Count;

                        int cnt = this.FPinTransforms[sw].SliceCount;
                        if (cnt == 0) { hasnil = true; break; }
                        else
                        {
                            spmax = Math.Max(spmax, cnt);
                            if (!trpinidx.Contains(sw))
                            {
                                trpinidx.Add(sw);
                                //All pins in use, break
                                if (trpinidx.Count == this.FPinTransforms.Count) { break; }

                            }
                        }
                    }

                    if (hasnil)
                    {
                        this.FPinOutput.SliceCount = 0;
                    }
                    else
                    {
                        this.FPinOutput.SliceCount = spmax;
                        for (int i = 0; i < spmax; i++)
                        {
                            double dblswitch;
                            this.FPinInput.GetValue(i, out dblswitch);
                            Matrix4x4 tr;
                            int pinidx = Convert.ToInt32(dblswitch) % this.FPinTransforms.Count;
                            if (this.FPinTransforms[pinidx].SliceCount > 0)
                            {
                                this.FPinTransforms[pinidx].GetMatrix(i, out tr);
                                this.FPinOutput.SetMatrix(i, tr);
                            }
                        }
                    }
                }
                else
                {
                    this.FPinOutput.SliceCount = 0;
                }
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
