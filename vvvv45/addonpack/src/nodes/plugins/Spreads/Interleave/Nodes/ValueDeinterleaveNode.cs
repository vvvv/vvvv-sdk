using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes
{
    public unsafe class ValueDeInterleaveNode : IPlugin, IDisposable
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "Vector";							//use CamelCaps and no spaces
                Info.Category = "Spreads";						//try to use an existing one
                Info.Version = "Split";						//versions are optional. leave blank if not needed
                Info.Help = "Vector (nd) split";
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

        private IValueConfig FPinCfgOutputCount;
        private IValueFastIn FPinInput;

        private List<IValueOut> FPinOutputList = new List<IValueOut>();

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

            this.FHost.CreateValueConfig("Output Count", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinCfgOutputCount);
            this.FPinCfgOutputCount.SetSubType(2, double.MaxValue, 1, 2, false, false, true);
            Configurate(this.FPinCfgOutputCount);

            this.FHost.CreateValueFastInput("Input", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInput);
            this.FPinInput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
        }
        #endregion

        #region Configurate
        public void Configurate(IPluginConfig Input)
        {
            if (Input == this.FPinCfgOutputCount)
            {
                double dblcount;
                this.FPinCfgOutputCount.GetValue(0, out dblcount);

                //Always 2 inputs minimum
                dblcount = Math.Max(dblcount, 2);

                if (dblcount < this.FPinOutputList.Count)
                {
                    //Remove pins, as value is lower
                    while (this.FPinOutputList.Count > dblcount)
                    {
                        this.FHost.DeletePin(this.FPinOutputList[this.FPinOutputList.Count - 1]);
                        this.FPinOutputList.RemoveAt(this.FPinOutputList.Count - 1);
                    }
                }

                if (dblcount > this.FPinOutputList.Count)
                {
                    //Add new pins, as value is bigger
                    while (this.FPinOutputList.Count < dblcount)
                    {
                        IValueOut pin;

                        int index = this.FPinOutputList.Count + 1;

                        this.FHost.CreateValueOutput("Output " + index, 1, null, TSliceMode.Dynamic, TPinVisibility.True, out pin);
                        pin.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
                        this.FPinOutputList.Add(pin);
                    }
                }
            }
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            if (SpreadMax > 0)
            {
                int outcount = (int)Math.Ceiling((double)SpreadMax / (double)this.FPinOutputList.Count);

                double* inptr;
                int incnt;

                this.FPinInput.GetValuePointer(out incnt, out inptr);

                int vcount = this.FPinOutputList.Count;
                double*[] outptrs = new double*[vcount];

                for (int i = 0; i < this.FPinOutputList.Count; i++)
                {
                    this.FPinOutputList[i].SliceCount = outcount;
                    this.FPinOutputList[i].GetValuePointer(out outptrs[i]);
                }

                int idx = 0;
                int ptidx = 0;
                for (int i = 0; i < SpreadMax; i += vcount)
                {
                    for (int j = 0; j < vcount; j++)
                    {
                        outptrs[j][idx] = inptr[ptidx % incnt];
                        ptidx++;
                    }
                    idx++;
                }
            }
            else
            {
                for (int i = 0; i < this.FPinOutputList.Count; i++)
                {
                    this.FPinOutputList[i].SliceCount = 0;
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
