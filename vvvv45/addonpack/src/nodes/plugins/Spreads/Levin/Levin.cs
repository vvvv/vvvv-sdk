#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

using System.IO;
using System.Collections.Generic;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Levin", Category = "Spreads", Version = "Legacy", Help = "", Tags = "", Author = "phlegma", Credits="http://acg.media.mit.edu/people/golan/scribble/")]
    #endregion PluginInfo
    public class Levin : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        #pragma warning disable 649, 169
        #region pins & fields
        [Input("Input")]
        protected ISpread<double> FInput;

        [Input("Insert")]
        protected IDiffSpread<bool> FDoInsert;

        [Input("Frame Count", MinValue = 0, DefaultValue = 1)]
        protected ISpread<int> FFrameCount;

        [Input("Reset", IsBang = true)]
        protected IDiffSpread<bool> FReset;

        [Output("Output")]
        protected ISpread<ISpread<double>> FOutput;

        [Output("Delta")]
        protected ISpread<double> FDeltaOut;

        [Import()]
        ILogger FLogger;


        Spread<ISpread<double>> FBuffer = new Spread<ISpread<double>>();
        Spread<double> FDelta = new Spread<double>();

        #endregion
        #pragma warning restore

        #region OnImportsSatisfied
        public void OnImportsSatisfied()
        {
            FFrameCount.SliceCount = 0;
        }
        #endregion

        public void Evaluate(int spreadMax)
        {
            FBuffer.ResizeAndDismiss(spreadMax, () => new Spread<double>());
            FDelta.SliceCount = spreadMax;

            //return null if one of the control inputs is null
            if (FDoInsert.IsAnyEmpty(FFrameCount, FReset))
            {
                FOutput.SliceCount = 0;
                return;
            }

            //Reset the BufferSlice 
            if (FReset.IsChanged)
            {
                for (int i = 0; i < spreadMax; i++)
                {
                    if (FReset[i])
                    {
                        FBuffer[i] = new Spread<double>();
                        FDelta[i] = 0; 
                    }
                }
            }
 
            //
            for (int i = 0; i < spreadMax; i++)
            {
                if (FDoInsert[i])
                {
                    //insert new values to the buffer
                    FBuffer[i].Insert(0, FInput[i]);

                    //calculate the delta between the first an last slice 
                    if (FBuffer[i].SliceCount > 0)
                        FDelta[i] = (FBuffer[i][0] - FBuffer[i][FBuffer[i].SliceCount - 1]);
                }
                else
                {
                    if (FBuffer.SliceCount > 0)
                    {
                        if (FBuffer[i].SliceCount > 0)
                        {
                            //add a new caluclate slice via delta to the spread 
                            Spread<double> MovedBuffer = new Spread<double>();
                            ISpread<double> MovedSpread = FBuffer[i].GetRange(0, FBuffer[i].SliceCount - 1);
                            double NewSlice = FBuffer[i][FBuffer[i].SliceCount - 1] + FDelta[i];
                            MovedBuffer.Add(NewSlice);
                            MovedBuffer.AddRange(MovedSpread);
                            FBuffer[i] = MovedBuffer;
                        }
                    }
                }

                //remove slices from the Buffer if the framecount gets smaller 
                if (FFrameCount.IsChanged)
                {
                    if (FFrameCount[i] >= 0 && FBuffer[i].SliceCount > FFrameCount[i])
                        FBuffer[i].RemoveRange(FFrameCount[i], FBuffer[i].SliceCount - FFrameCount[i]);
                }
            }

            //set the output pins
            FDeltaOut.AssignFrom(FDelta as ISpread<double>);
            FOutput.AssignFrom(FBuffer);
        }
    }
}
