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

        [Input("New", IsSingle = true, IsBang = true)]
        public ISpread<bool> FDoSplit;

        [Input("Duplicate Last Input", IsSingle = true, IsToggle = true)]
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

        [Output("Output Queue Size")]
        public ISpread<int> FFramesRecorded;

        [Import()]
        public ILogger FLogger;

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
                    }
                }
            }

            if (FDoInsert[0])
            {
                // is empty, so insert new slice
                if (FFramesRecorded.SliceCount < 1)
                    FFramesRecorded.Insert(0, 0);
                // new slice for FFramesRecorded reqested
                else if (FDoSplit[0])
                {
                    // duplicate current slice and insert in old queue
                    if (FSplitDuplicate[0])
                    {
                        FBuffer.Insert(0, FInput.Clone());
                        FFramesRecorded[0]++;
                    }
                    FFramesRecorded.Insert(0, 0);
                }

                FBuffer.Insert(0, FInput.Clone());
                FFramesRecorded[0]++;
            }
            
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
    }
}
