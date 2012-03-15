using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;
using VVVV.Nodes.Lib;

namespace VVVV.Nodes
{
    public class AdvMultiplyTransNode : IPlugin, IDisposable
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "Multiply";							//use CamelCaps and no spaces
                Info.Category = "Transform";						//try to use an existing one
                Info.Version = "Priority";						//versions are optional. leave blank if not needed
                Info.Help = "Multiplies transform with priority";
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
        private IValueConfig FPinInNumInputs;
        private List<ITransformIn> FPinTransforms = new List<ITransformIn>();
        private List<IValueIn> FPInPriorities = new List<IValueIn>();
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

            this.FHost.CreateValueConfig("Inputs Count", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInNumInputs);
            this.FPinInNumInputs.SetSubType(2, double.MaxValue, 1, 2, false, false, true);

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

                    IValueIn pinprio;
                    
                    this.FHost.CreateValueInput("Priority " + index, 1, null, TSliceMode.Dynamic, TPinVisibility.True, out pinprio);
                    pinprio.SetSubType(double.MinValue, double.MaxValue, 1, index, false, false, true);
                    this.FPInPriorities.Add(pinprio);
        
                }
            }
            else
            {
                //Remove pins, as value is lower
                while (FPinTransforms.Count > this.FInputCount)
                {
                    this.FHost.DeletePin(this.FPinTransforms[this.FPinTransforms.Count - 1]);
                    this.FHost.DeletePin(this.FPInPriorities[this.FPInPriorities.Count - 1]);
                    this.FPinTransforms.RemoveAt(this.FPinTransforms.Count - 1);
                    this.FPInPriorities.RemoveAt(this.FPInPriorities.Count - 1);
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
                if (this.FPinTransforms[i].PinIsChanged || this.FPInPriorities[i].PinIsChanged)
                {
                    changed = true;
                }
            }

            if (changed)
            {
                this.FPinOutput.SliceCount = SpreadMax;

                for (int i = 0; i < SpreadMax; i++)
                {
                    List<double> priority = new List<double>();
                    double dblpri;
                    for (int j = 0; j < this.FInputCount; j++)
                    {
                        this.FPInPriorities[j].GetValue(j, out dblpri);
                        priority.Add(dblpri);
                    }

                    List<ITransformIn> sorted = ReorderPins.Sort(this.FPinTransforms, priority);

                    bool first = true;
                    Matrix4x4 mat;
                    sorted[0].GetMatrix(i, out mat);

                    Matrix4x4 tmp;


                    for (int j = 1; j < this.FPinTransforms.Count; j++)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        sorted[j].GetMatrix(i, out tmp);
                        mat *= tmp;
                    }

                    this.FPinOutput.SetMatrix(i, mat);
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
