#region usings
using System;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

#endregion usings

namespace VVVV.Nodes
{
    public abstract class AdvancedMapNode : IPluginEvaluate
    {





        public virtual void Evaluate(int SpreadMax) { }


    }

    #region PluginInfo
    [PluginInfo(Name = "Map", Category = "Value", Version = "Advanced", Help = "Maps the value in the given range to a proportional value in the given output range", Tags = "velcrome")]
    #endregion PluginInfo
    public class AdvancedValueMapNode : AdvancedMapNode
    {
        #region fields & pins
        [Input("Input", DefaultValue = 0.5, Order = 0)]
        public ISpread<double> FInput;

        [Input("Input Binsize", DefaultValue = 1, Order = 1, Visibility = PinVisibility.Hidden, MinValue = 1)]
        public ISpread<int> FBinsize;

        [Input("Source Minimum", DefaultValue = 0.0, Order = 2)]
        public ISpread<double> FInFrom;

        [Input("Source Maximum", DefaultValue = 1.0, Order = 3)]
        public ISpread<double> FInTo;

        [Input("Destination Minimum", DefaultValue = 0.0, Order = 4)]
        public ISpread<double> FOutFrom;

        [Input("Destination Maximum", DefaultValue = 1.0, Order = 5)]
        public ISpread<double> FOutTo;

        [Input("Mapping", DefaultEnumEntry = "Float", Order = 6)]
        public ISpread<TMapMode> FMapping;

        [Output("Output")]
        protected ISpread<double> FOutput;

        #endregion fields & pins

        override public void Evaluate(int SpreadMax)
        {
            SpreadMax = FInput.SliceCount;
            FOutput.SliceCount = SpreadMax;

            //			direct access to the buffers is faster than accessing ISpread
            int[] binsize = FBinsize.Stream.Buffer;
            double[] input = FInput.Stream.Buffer;
            double[] fromI = FInFrom.Stream.Buffer;
            double[] toI = FInTo.Stream.Buffer;
            double[] fromO = FOutFrom.Stream.Buffer;
            double[] toO = FOutTo.Stream.Buffer;
            TMapMode[] map = FMapping.Stream.Buffer;

            //			this means we will have to take care of not exceeding any SpreadCount ourselves
            int iBinLength = FBinsize.SliceCount;
            int iFromLength = FInFrom.SliceCount;
            int iToLength = FInTo.SliceCount;
            int oFromLength = FOutFrom.SliceCount;
            int oToLength = FOutTo.SliceCount;
            int mapLength = FMapping.SliceCount;

            //			with these counters the buffers are indexed
            int binCounter = 0;
            int iBin = 0;
            int iFromC = 0; int iToC = 0;
            int oToC = 0; int oFromC = 0;
            int mapC = 0;

            for (int index = 0; index < SpreadMax; index++)
            {
                FOutput[index] = VMath.Map(input[index], fromI[iFromC], toI[iToC], fromO[oFromC], toO[oToC], map[mapC]); 

                // increment and resetting counters if needed
                if (++binCounter >= binsize[iBin])
                {
                    binCounter = 0;
                    if (++iBin >= iBinLength) iBin = 0;

                    if (++iFromC >= iFromLength) iFromC = 0;
                    if (++iToC >= iToLength) iToC = 0;
                    if (++oFromC >= oFromLength) oFromC = 0;
                    if (++oToC >= oToLength) oToC = 0;
                    if (++mapC >= mapLength) mapC = 0;
                }
            }
        }
    }




    #region PluginInfo
    [PluginInfo(Name = "MapRange", Category = "Value", Version = "Advanced", Help = "Maps the value in the given range to a proportional value in the given output range", Tags = "velcrome")]
    #endregion PluginInfo
    public class AdvancedValueMapRangeNode : AdvancedMapNode
    {
        #region fields & pins
        [Input("Input", DefaultValue = 0.5, Order = 0)]
        public ISpread<double> FInput;

        [Input("Input Binsize", DefaultValue = 1, Order = 1, Visibility = PinVisibility.Hidden, MinValue = 1)]
        public ISpread<int> FBinsize;
        
        [Input("Source Center", DefaultValue = 0.5, Order = 2)]
        public ISpread<double> FInFrom;

        [Input("Source Width", DefaultValue = 1.0, Order = 3)]
        public ISpread<double> FInTo;

        [Input("Destination Center", DefaultValue = 0.5, Order = 4)]
        public ISpread<double> FOutFrom;

        [Input("Destination Width", DefaultValue = 1.0, Order = 5)]
        public ISpread<double> FOutTo;

        [Input("Mapping", DefaultEnumEntry = "Float", Order = 6)]
        public ISpread<TMapMode> FMapping;

        [Output("Output")]
        protected ISpread<double> FOutput;

        #endregion fields & pins

        override public void Evaluate(int SpreadMax)
        {
            SpreadMax = FInput.SliceCount;
            FOutput.SliceCount = SpreadMax;

            //			direct access to the buffers is faster than accessing ISpread
            int[] binsize = FBinsize.Stream.Buffer;
            double[] input = FInput.Stream.Buffer;
            double[] fromI = FInFrom.Stream.Buffer;
            double[] toI = FInTo.Stream.Buffer;
            double[] fromO = FOutFrom.Stream.Buffer;
            double[] toO = FOutTo.Stream.Buffer;
            TMapMode[] map = FMapping.Stream.Buffer;

            //			this means we will have to take care of not exceeding any SpreadCount ourselves
            int iBinLength = FBinsize.SliceCount;
            int iFromLength = FInFrom.SliceCount;
            int iToLength = FInTo.SliceCount;
            int oFromLength = FOutFrom.SliceCount;
            int oToLength = FOutTo.SliceCount;
            int mapLength = FMapping.SliceCount;

            //			with these counters the buffers are indexed
            int binCounter = 0;
            int iBin = 0;
            int iFromC = 0; int iToC = 0;
            int oToC = 0; int oFromC = 0;
            int mapC = 0;

            for (int index = 0; index < SpreadMax; index++)
            {
                double halfWidth = toI[iToC] / 2.0;
                double ratio = VMath.Ratio(input[index], fromI[iFromC] - halfWidth, fromI[iFromC] + halfWidth, map[mapC]);

                halfWidth = toO[oToC] / 2.0;
                FOutput[index] = VMath.Lerp(fromO[oFromC] - halfWidth, fromO[oFromC] + halfWidth, ratio);

                // increment and resetting counters if needed
                if (++binCounter >= binsize[iBin])
                {
                    binCounter = 0;
                    if (++iBin >= iBinLength) iBin = 0;

                    if (++iFromC >= iFromLength) iFromC = 0;
                    if (++iToC >= iToLength) iToC = 0;
                    if (++oFromC >= oFromLength) oFromC = 0;
                    if (++oToC >= oToLength) oToC = 0;
                    if (++mapC >= mapLength) mapC = 0;
                }
            }
        }
    }
}