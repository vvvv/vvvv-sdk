using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Generic
{
    public abstract class NearestNeighbour<T> : IPluginEvaluate
    {
        [Input("Reference")]
        protected ISpread<T> FInReference;

        [Input("Input")]
        protected ISpread<T> FInput;

        [Output("Nearest Index")]
        protected ISpread<int> FOutput;

        public void Evaluate(int SpreadMax)
        {
            if (SpreadMax > 1)
            {
                this.FOutput.SliceCount = this.FInput.SliceCount;

                for (int i = 0; i < this.FInput.SliceCount; i++)
                {
                    double dblmin = double.MaxValue;
                    int minidx = -1;

                    double dist;

                    T input = this.FInput[i];

                    for (int j = 0; j < this.FInReference.SliceCount; j++)
                    {

                        T refobj = this.FInReference[j];

                        dist = this.Distance(input, refobj);
                        if (dist < dblmin)
                        {
                            minidx = j;
                            dblmin = dist;
                        }
                    }
                    this.FOutput[i] = minidx;
                }
            }
            else
            {
                this.FOutput.SliceCount = 0;
            }
        }

        protected abstract double Distance(T t1, T t2);
    }
}
