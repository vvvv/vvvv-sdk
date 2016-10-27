using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes.Lib
{
    public class ReorderPins
    {
        public static List<ITransformIn> Sort(List<ITransformIn> initial, List<double> priority)
        {
            List<ITransformIn> result = new List<ITransformIn>();
            List<ITransformIn> clone = new List<ITransformIn>(initial);
            List<double> clonepri = new List<double>(priority);

            while (clone.Count > 1)
            {
                double min = clonepri[0];
                int minidx = 0;
                for (int i = 1; i < clone.Count; i++)
                {
                    if (priority[i] < min)
                    {
                        minidx = i;
                        min = clonepri[i];
                    }

                    
                }
                result.Add(clone[minidx]);
                clonepri.RemoveAt(minidx);
                clone.RemoveAt(minidx);
            }
            result.Add(clone[0]);


            return result;
        }
    }
}
