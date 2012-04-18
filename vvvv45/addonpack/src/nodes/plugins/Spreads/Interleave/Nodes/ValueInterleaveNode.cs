using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1; 

namespace VVVV.Nodes
{   
    public unsafe class ValueInterleaveNode : IPlugin, IDisposable
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "Vector";							//use CamelCaps and no spaces
                Info.Category = "Spreads";						//try to use an existing one
                Info.Version = "Join";						//versions are optional. leave blank if not needed
                Info.Help = "Vector (nd) Join";
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

        private IValueConfig FPinCfgInputCount;
        private IValueIn FPinInAllowEmpty;
        private List<IValueFastIn> FPinInputList = new List<IValueFastIn>();

        private IValueOut FPinOutput;
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

            this.FHost.CreateValueInput("Allow Empty Spreads", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out this.FPinInAllowEmpty);
            this.FPinInAllowEmpty.SetSubType(0, 1, 1, 0, false, true, false);

            this.FHost.CreateValueConfig("Input Count", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinCfgInputCount);
            this.FPinCfgInputCount.SetSubType(2, double.MaxValue, 1, 2, false, false, true);
            Configurate(this.FPinCfgInputCount);

            this.FHost.CreateValueOutput("Output", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutput);
            this.FPinOutput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
        }
        #endregion

        #region Configurate
        public void Configurate(IPluginConfig Input)
        {
            if (Input == this.FPinCfgInputCount)
            {
                double dblcount;
                this.FPinCfgInputCount.GetValue(0, out dblcount);

                //Always 2 inputs minimum
                dblcount = Math.Max(dblcount, 2);

                if (dblcount < this.FPinInputList.Count)
                {
                    //Remove pins, as value is lower
                    while (this.FPinInputList.Count > dblcount)
                    {
                        this.FHost.DeletePin(this.FPinInputList[this.FPinInputList.Count - 1]);
                        this.FPinInputList.RemoveAt(this.FPinInputList.Count - 1);
                    }
                }

                if (dblcount > this.FPinInputList.Count)
                {
                    //Add new pins, as value is bigger
                    while (this.FPinInputList.Count < dblcount)
                    {
                        IValueFastIn pin;

                        int index = this.FPinInputList.Count + 1;

                        this.FHost.CreateValueFastInput("Input " + index, 1, null, TSliceMode.Dynamic, TPinVisibility.True, out pin);
                        pin.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
                        this.FPinInputList.Add(pin);
                    }
                }
            }
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            double allowempty;
            this.FPinInAllowEmpty.GetValue(0, out allowempty);

            //Early exit
            if (allowempty <= 0.5 && SpreadMax == 0)
            {
                this.FPinOutput.SliceCount = 0;
                return;
            }

            List<IValueFastIn> pins = new List<IValueFastIn>();
            int max = 0;
            if (allowempty > 0.5)
            {
                foreach (IValueFastIn fin in this.FPinInputList)
                {
                    if (fin.SliceCount > 0)
                    {
                        pins.Add(fin);
                    }

                    max = Math.Max(fin.SliceCount, max);
                }
            }
            else
            {
                pins = this.FPinInputList;
                max = SpreadMax;
            }


            int outcount = max * pins.Count;
            this.FPinOutput.SliceCount = outcount;

            double*[] ptrs = new double*[pins.Count];
            int[] cnts = new int[pins.Count];
            int vcount = pins.Count;

            for (int i = 0; i < pins.Count; i++)
            {
                pins[i].GetValuePointer(out cnts[i], out ptrs[i]);
            }

            double* outptr;
            this.FPinOutput.GetValuePointer(out outptr);

            for (int i = 0; i < max; i++)
            {
                for (int j = 0; j < vcount; j++)
                {
                    *outptr = ptrs[j][i % cnts[j]];
                    outptr++;
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
