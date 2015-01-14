#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Collections;
using System.Linq;


using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes.Generic
{
    public abstract class QueueStore<T> : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Input")]
        public ISpread<T> FInput;

        [Input("Insert", IsSingle = true)]
        public ISpread<bool> FDoInsert;

        [Input("Split", IsSingle = true, IsBang = true)]
        public ISpread<bool> FDoSplit;

        [Input("Duplicate on split", IsSingle = true, IsToggle = true)]
        public ISpread<bool> FSplitDuplicate;

        [Input("Remove Index", DefaultValue = 0)]
        public ISpread<int> FIndex;

        [Input("Remove", IsSingle = true, IsBang = true)]
        public ISpread<bool> FRemove;

        [Input("Consolidate", IsSingle = true, IsBang = true)]
        public ISpread<bool> FConsolidate;

        [Input("Reset", IsSingle = true, IsBang = true)]
        public ISpread<bool> FReset;

        [Output("Output")]
        public ISpread<ISpread<T>> FOutput;

        [Output("Frames Recorded")]
        public ISpread<int> FFramesRecorded;

        [Import()]
        public ILogger FLogger;

        bool FRecording = false;
        List<ISpread<T>> FBuffer = new List<ISpread<T>>();

        public void OnImportsSatisfied()
        {
            //start with an empty stream output
            FOutput.SliceCount = 0;
        }

        public void Evaluate(int SpreadMax)
        {
            //return null if one of the control inputs is null
            if (FDoInsert.IsAnyEmpty(FReset))
            {
                FOutput.SliceCount = 0;
                return;
            }

            if (FReset[0])
            {
                FBuffer.Clear();
                FFramesRecorded.SliceCount = 0;
            }

            if (FRemove[0])
            {
                //FLogger.Log(LogType.Debug,"---------------------");

                if (FFramesRecorded.SliceCount > 0)
                {

                    foreach (int i in FIndex.Select(x => x % FFramesRecorded.SliceCount).Distinct().OrderByDescending(x => x))
                    {
                        //FLogger.Log(LogType.Debug, "i="+i.ToString()+" , FFramesRec.SliceCount=" +FFramesRecorded.SliceCount.ToString());
                        int offset = 0;
                        for (int j = 0; j < i; j++)
                        {

                            offset += FFramesRecorded[j];
                            //FLogger.Log(LogType.Debug, "+ " + FFramesRecorded[j].ToString() + " = " + offset.ToString());
                        }

                        //FLogger.Log(LogType.Debug, "offset: "+offset.ToString()+ " - FramesRecorded[i]: "+ FFramesRecorded[i].ToString());


                        if (FFramesRecorded.SliceCount > 1)
                        {
                            //FLogger.Log(LogType.Debug, ". Removing " + i.ToString() +": "+ FFramesRecorded[i].ToString());
                            FBuffer.RemoveRange(offset, FFramesRecorded[i]);
                        }
                        else
                        {
                            //FLogger.Log(LogType.Debug, ".. Removing " + i.ToString() +": "+ FFramesRecorded[i].ToString());
                            FBuffer.RemoveRange(0, FFramesRecorded[i]);
                        }

                        FFramesRecorded.RemoveAt(i);

                        // set FRecording to false so another FFramesRecorded slice can be 
                        // inserted when FDoInsert is true in the same frame
                        if (FFramesRecorded.SliceCount < 1)
                            FRecording = false;
                    }
                }
            }

            if (FDoInsert[0])
            {
                if (!FRecording)
                {
                    FRecording = true;
                    FFramesRecorded.Insert(0, 0);
                }
                if (FDoSplit[0])
                {
                    if (FSplitDuplicate[0])
                    {
                        FBuffer.Insert(0, CloneInputSpread(FInput));
                        FFramesRecorded[0]++;
                    }
                    FFramesRecorded.Insert(0, 0);
                }

                FBuffer.Insert(0, CloneInputSpread(FInput));
                FFramesRecorded[0]++;
            }
            else
                FRecording = false;


            FOutput.AssignFrom(FBuffer);

            if (FOutput.SliceCount == 0)
            {
                FFramesRecorded.SliceCount = 0;
            }

            // combines all recorded queues to one big queue
            if (FConsolidate[0] == true)
            {
                int count = 0;

                foreach (int current in FFramesRecorded)
                    count += current;

                FFramesRecorded.SliceCount = 1;
                FFramesRecorded[0] = count;
            }
        }


        abstract protected ISpread<T> CloneInputSpread(ISpread<T> spread);
    }
}
