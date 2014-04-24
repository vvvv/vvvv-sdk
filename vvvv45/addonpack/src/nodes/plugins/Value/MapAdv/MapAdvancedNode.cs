#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.Utils.Streams;
using VVVV.Utils;
#endregion usings

namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Map", Category = "Value", Version = "Bin", Help = "Maps the value in the given range to a proportional value in the given output range", Tags = "velcrome, woei")]
    #endregion PluginInfo
    public class MapAdvancedNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Input")]
        public IValueFastIn FInput;

        [Input("Bin Size", AsInt = true, DefaultValue = 1, Visibility = PinVisibility.Hidden)]
        public IValueFastIn FBin;

        [Input("Source Minimum", DefaultValue = 0.0)]
        public IValueFastIn FSrcMin;

        [Input("Source Maximum", DefaultValue = 1.0)]
        public IValueFastIn FSrcMax;

        [Input("Destination Minimum", DefaultValue = 0.0)]
        public IValueFastIn FDstMin;

        [Input("Destination Maximum", DefaultValue = 1.0)]
        public IValueFastIn FDstMax;

        [Input("Mapping", EnumName = "MapRangeMode")]
        public ISpread<TMapMode> FMapping;

        [Output("Output")]
        public IValueOut FOutput;
        #endregion fields & pins

        public unsafe void Evaluate(int spreadMax)
        {
            int cIn, cBin, cSrcMin, cSrcMax, cDstMin, cDstMax;
            double* inP, binP, srcMinP, srcMaxP, dstMinP, dstMaxP, outP;
            

            FInput.GetValuePointer(out cIn, out inP);
            FBin.GetValuePointer(out cBin, out binP);
            FSrcMin.GetValuePointer(out cSrcMin, out srcMinP);
            FSrcMax.GetValuePointer(out cSrcMax, out srcMaxP);
            FDstMin.GetValuePointer(out cDstMin, out dstMinP);
            FDstMax.GetValuePointer(out cDstMax, out dstMaxP);

            spreadMax = cBin.CombineStreams(cSrcMin).CombineStreams(cSrcMax)
                            .CombineStreams(cDstMin).CombineStreams(cDstMax)
                            .CombineStreams(FMapping.SliceCount);

            int binC = 0;
            int sliceMax = 0;
            List<int> bins = new List<int>();

            if (spreadMax > 0)
            {
                while (binC < spreadMax || sliceMax < cIn)
                {
                    int bin = (int)binP[binC % cBin];
                    if (bin < 0)
                        bin = (int)Math.Round(cIn / (double)Math.Abs(bin));
                    sliceMax += bin;
                    binC++;
                    bins.Add(bin);
                }
            }

            FOutput.SliceCount = sliceMax;
            FOutput.GetValuePointer(out outP);
            int i = 0;
            for (int b = 0; b < bins.Count; b++)
            {
                double sMin = srcMinP[b % cSrcMin];
                double sMax = srcMaxP[b % cSrcMax];
                double dMin = dstMinP[b % cDstMin];
                double dMax = dstMaxP[b % cDstMax];
                TMapMode mode = FMapping[b];


                for (int s = 0; s < bins[b]; s++)
                {
                    outP[i] = VMath.Map(inP[i % cIn], sMin, sMax, dMin, dMax, mode);
                    i++;
                }
            }
        }

        private double FromRange(double val, double sMin, double sMax)
        {
            double sLen = sMax - sMin;
            if (sLen == 0)
                return 0;
            else
                return (val - sMin) / sLen;
        }

        private double ToRange(double val, double dMin, double dMax)
        {
            double dLen = dMax - dMin;
            val *= dLen;
            return val + dMin;
        }
    }

    #region PluginInfo
    [PluginInfo(Name = "MapRange", Category = "Value", Version = "Bin", Help = "Maps the value in the given range to a proportional value in the given output range", Tags = "velcrome, woei")]
    #endregion PluginInfo
    public class MapRangePointerNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Input")]
        public IValueFastIn FInput;

        [Input("Bin Size", AsInt = true, DefaultValue = 1, Visibility = PinVisibility.Hidden)]
        public IValueFastIn FBin;

        [Input("Source Center", DefaultValue = 0.5)]
        public IValueFastIn FSrcCenter;

        [Input("Source Width", DefaultValue = 1.0)]
        public IValueFastIn FSrcWidth;

        [Input("Destination Center", DefaultValue = 0.5)]
        public IValueFastIn FDestCenter;

        [Input("Destination Width", DefaultValue = 1.0)]
        public IValueFastIn FDestWidth;

        [Input("Mapping", EnumName = "MapRangeMode")]
        public ISpread<TMapMode> FMapping;

        [Output("Output")]
        public IValueOut FOutput;

        private double width;

        #endregion fields & pins

        public unsafe void Evaluate(int spreadMax)
        {
            int countIn, countBin, countSrcCenter, countSrcWidth, countDestCenter, countDestWidth;
            double* inP, binP, srcCenterP, srcWidthP, destCenterP, destWidthP, outP;

            FInput.GetValuePointer(out countIn, out inP);
            FBin.GetValuePointer(out countBin, out binP);
            FSrcCenter.GetValuePointer(out countSrcCenter, out srcCenterP);
            FSrcWidth.GetValuePointer(out countSrcWidth, out srcWidthP);
            FDestCenter.GetValuePointer(out countDestCenter, out destCenterP);
            FDestWidth.GetValuePointer(out countDestWidth, out destWidthP);

            spreadMax = countBin.CombineStreams(countSrcCenter).CombineStreams(countSrcWidth)
                            .CombineStreams(countDestCenter).CombineStreams(countDestWidth)
                            .CombineStreams(FMapping.SliceCount);

            int binC = 0;
            int sliceMax = 0;
            List<int> bins = new List<int>();
            if (spreadMax > 0)
            {
                while (binC < spreadMax || sliceMax < countIn)
                {
                    int bin = (int)binP[binC % countBin];
                    if (bin < 0)
                        bin = (int)Math.Round(countIn / (double)Math.Abs(bin));
                    sliceMax += bin;
                    binC++;
                    bins.Add(bin);
                }
            }

            FOutput.SliceCount = sliceMax;
            FOutput.GetValuePointer(out outP);
            int i = 0;
            for (int b = 0; b < bins.Count; b++)
            {
                width = srcWidthP[b % countSrcWidth];
                double sMin = srcCenterP[b % countSrcCenter] - width / 2;
                double sMax = sMin + width;

                width = destWidthP[b%countDestWidth];
                double dMin = destCenterP[b % countDestCenter] - width / 2;
                double dMax = dMin + width;

                TMapMode mode = FMapping[b];

                for (int s = 0; s < bins[b]; s++)
                {
                    outP[i] = VMath.Map(inP[i % countIn], sMin, sMax, dMin, dMax, mode);
                    i++;
                }
            }
        }
    }
}