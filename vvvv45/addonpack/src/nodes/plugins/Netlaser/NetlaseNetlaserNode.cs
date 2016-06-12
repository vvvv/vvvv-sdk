#region usings
using System;
using System.ComponentModel.Composition;
//using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;

using System.Runtime.InteropServices;
#endregion usings

namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Netlaser", Category = "Netlase", Help = "Control that Laser", Tags = "Netlase, Laser, ILDA", Author = "digitalWannabe")]
    #endregion PluginInfo
    public class NetlaseNetlaserNode : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
    {

        #region fields & pins	
        [Input("Refresh Device List", DefaultValue = 0, IsBang = true, IsSingle = true, Order = 3)]
        public ISpread<bool> FRefreshDeviceList;

        [Input("Enable", Order = 4)]
        public ISpread<bool> FEnable;

        [Input("NetlaseFrame", Order = 0)]
        public ISpread<NetLaseFrame> FFrameIn;

        [Input("ScanRate(pps)", MinValue = 500, Order = 2)]
        public ISpread<uint> FPointsPerSecond;

        [Input("Debug", Order = 6, DefaultValue = 0, Visibility = PinVisibility.OnlyInspector, IsSingle = true)]
        public ISpread<bool> FDebug;

        [Input("Use Fixed Timing", Order = 5, DefaultValue = 0, Visibility = PinVisibility.OnlyInspector, IsSingle = true)]
        public ISpread<bool> FUseFixedTiming;

        [Output("Handle", Order = 1)]
        public ISpread<int> FHandle;

        [Output("MinSpeed", Visibility = PinVisibility.OnlyInspector)]
        public ISpread<int> FMinSpeed;

        [Output("MaxSpeed", Visibility = PinVisibility.OnlyInspector)]
        public ISpread<int> FMaxSpeed;

        [Output("Used Color Channels", Visibility = PinVisibility.OnlyInspector)]
        public ISpread<int> FNumColorChannels;

        [Output("MaxFrameSize", Visibility = PinVisibility.OnlyInspector)]
        public ISpread<int> FMaxFrameSize;

        [Output("Device Count", Order = 2, IsSingle = true)]
        public ISpread<int> FDeviceCount;

        [Output("Device List", Visibility = PinVisibility.OnlyInspector)]
        public ISpread<string> FDeviceList;

        [Output("Friendly Name", Order = 0)]
        public ISpread<string> FFriendlyName;

        [Output("Frame Duration", Visibility = PinVisibility.OnlyInspector)]
        public ISpread<int> FFrameDuration;
        #endregion fields & pins	

        [Import]
        public ILogger FLogger;

        #region dllimport

        [System.Runtime.InteropServices.DllImport("jmlaser.dll")]
        private static extern int jmLaserEnumerateDevices();

        [System.Runtime.InteropServices.DllImport("jmlaser.dll")]
        private static extern int jmLaserGetDeviceListEntry(uint deviceID, System.IntPtr deviceName, uint blength);

        [System.Runtime.InteropServices.DllImport("jmlaser.dll")]
        private static extern int jmLaserGetDeviceListLength();

        [System.Runtime.InteropServices.DllImport("jmlaser.dll")]
        private static extern int jmLaserGetDeviceListEntryLength(uint deviceID);

        [System.Runtime.InteropServices.DllImport("jmlaser.dll")]
        private static extern int jmLaserOpenDevice(string deviceName);
        //	private static extern int jmLaserOpenDevice(string deviceName);

        [System.Runtime.InteropServices.DllImport("jmlaser.dll")]
        private static extern int jmLaserStartOutput(int handle);

        [System.Runtime.InteropServices.DllImport("jmlaser.dll")]
        private static extern int jmLaserStopOutput(int handle);

        //		[System.Runtime.InteropServices.DllImport("jmlaser.dll")]
        //        private static extern int jmLaserWriteFrame (int deviceID, JMVector[]	vectors, int count, int speed, int repetitions);

        [System.Runtime.InteropServices.DllImport("jmlaser.dll")]
        private static extern int jmLaserGetFriendlyNameLength(string deviceName);

        [System.Runtime.InteropServices.DllImport("jmlaser.dll")]
        private static extern int jmLaserGetFriendlyName(string deviceName, System.IntPtr deviceFriendlyName, uint length);

        [System.Runtime.InteropServices.DllImport("jmlaser.dll")]
        private static extern int jmLaserGetDeviceName(int handle, System.IntPtr deviceName, uint length);

        [System.Runtime.InteropServices.DllImport("jmlaser.dll")]
        private static extern int jmLaserIsDeviceReady(int handle);

        [System.Runtime.InteropServices.DllImport("jmlaser.dll")]
        private static extern int jmLaserGetMinSpeed(int handle);

        [System.Runtime.InteropServices.DllImport("jmlaser.dll")]
        private static extern int jmLaserGetMaxSpeed(int handle);

        [System.Runtime.InteropServices.DllImport("jmlaser.dll")]
        private static extern int jmLaserGetNumUsedColorChannels(int handle);

        [System.Runtime.InteropServices.DllImport("jmlaser.dll")]
        private static extern int jmLaserCloseDevice(int handle);

        [System.Runtime.InteropServices.DllImport("jmlaser.dll")]
        private static extern int jmLaserGetMaxFrameSize(int handle);

        [System.Runtime.InteropServices.DllImport("jmlaser.dll")]
        private static extern int jmLaserWriteFrameNL(int handle, NetLaseVector[] vectors, uint count, uint speed, uint repetitions);

        [System.Runtime.InteropServices.DllImport("jmlaser.dll")]
        private static extern int jmLaserWaitForDeviceReady(int handle);

        //		[System.Runtime.InteropServices.DllImport("jmlaser.dll")]
        //		 private static extern int jmLaserWriteFrameEL (int handle, EasyLaseVector[] vectors, uint count, uint speed, uint repetitions);	

        #endregion dllimport

        private readonly Spread<Task> FTasks = new Spread<Task>();
        private readonly Spread<CancellationTokenSource> FCts = new Spread<CancellationTokenSource>();
        private readonly Spread<CancellationToken> ct = new Spread<CancellationToken>();
        private int taskCount;

        private int deviceCount;
        private Spread<int> handle;
        private Spread<bool> runOnce;
        //		private Spread<int> wFrame;
        private Spread<bool> enabled;
        private int checkReady;
        private string deviceName;
        private Spread<IntPtr> deviceNamesPtr;
        private Spread<IntPtr> friendlyNamePtr;
        private string[] Error;


        // Called when this plugin was created
        public void OnImportsSatisfied()
        {
            // Do any initialization logic here. In this example we won't need to
            // do anything special.
            taskCount = 0;
            deviceCount = 0;
            handle = new Spread<int>();
            runOnce = new Spread<bool>();
            enabled = new Spread<bool>();
            checkReady = 0;
            deviceName = "not initialized";
            deviceNamesPtr = new Spread<IntPtr>();
            friendlyNamePtr = new Spread<IntPtr>();
            Error = new string[] { "OK", "NOT ENUMERATED", "INVALID HANDLE", "DEVICE NOT OPEN", "DEVICE NOT FOUND", "OUTPUT NOT STARTED", "INVALID UNIVERSE", "OUT OF RANGE", "DEVICE BUSY", "IO- ERROR" };

            deviceCount = jmLaserEnumerateDevices();
        }

        // Called when this plugin gets deleted
        public void Dispose()
        {
            // Should this plugin get deleted by the user or should vvvv shutdown
            // we need to wait until all still running tasks ran to a completion
            // state.
            for (int i = 0; i < taskCount; i++)
            {
                int index = i;
                jmLaserStopOutput(FHandle[index]);
                jmLaserCloseDevice(FHandle[index]);
                if (FDebug[0]) FLogger.Log(LogType.Message, "Dispose task:" + index);
                CancelRunningTasks(index);
            }
        }

        // Called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            SpreadMax = SpreadUtils.SpreadMax(FFrameIn, FEnable /* state ALL your inputs here*/);

            // Set the slice counts of our outputs.
            FTasks.SliceCount = SpreadMax;
            FCts.SliceCount = SpreadMax;
            ct.SliceCount = SpreadMax;
            taskCount = SpreadMax;

            FHandle.SliceCount = SpreadMax;
            FDeviceList.SliceCount = SpreadMax;
            runOnce.SliceCount = SpreadMax;
            handle.SliceCount = SpreadMax;
            enabled.SliceCount = SpreadMax;
            FFrameIn.SliceCount = SpreadMax;
            deviceNamesPtr.SliceCount = SpreadMax;
            friendlyNamePtr.SliceCount = SpreadMax;
            FFrameDuration.SliceCount = SpreadMax;
            FMinSpeed.SliceCount = SpreadMax;
            FMaxSpeed.SliceCount = SpreadMax;
            FMaxFrameSize.SliceCount = SpreadMax;
            FNumColorChannels.SliceCount = SpreadMax;
            FFriendlyName.SliceCount = SpreadMax;


            for (int i = 0; i < SpreadMax; i++)
            {
                // store i to a new variable so it won't change when tasks are running over longer period.
                int index = i;
                uint deviceID = (uint)i;
                int ListEntryLength = 0;

                if (FRefreshDeviceList[0])
                {
                    deviceCount = jmLaserEnumerateDevices();
                    try
                    {
                        ListEntryLength = jmLaserGetDeviceListEntryLength(deviceID);
                        deviceNamesPtr[index] = Marshal.AllocHGlobal(ListEntryLength);
                        jmLaserGetDeviceListEntry(deviceID, deviceNamesPtr[index], (uint)(ListEntryLength));
                        deviceName = Marshal.PtrToStringAnsi(deviceNamesPtr[index]);

                        if (!runOnce[index])
                        {
                            FHandle[index] = jmLaserOpenDevice(deviceName);
                            runOnce[index] = true;
                        }

                        int minSpeed = jmLaserGetMinSpeed(FHandle[index]);
                        int maxSpeed = jmLaserGetMaxSpeed(FHandle[index]);
                        int maxFrameSize = jmLaserGetMaxFrameSize(FHandle[index]);
                        int friendlyNameLength = jmLaserGetFriendlyNameLength(deviceName);
                        friendlyNamePtr[index] = Marshal.AllocHGlobal(friendlyNameLength);
                        jmLaserGetFriendlyName(deviceName, friendlyNamePtr[index], (uint)friendlyNameLength);
                        string friendlyNamePtr = Marshal.PtrToStringAnsi(friendlyNamePtr[index]);
                        int numUsedColChannels = jmLaserGetNumUsedColorChannels(FHandle[index]);

                        FDeviceList[index] = deviceName;
                        FMinSpeed[index] = minSpeed;
                        FMaxSpeed[index] = maxSpeed;
                        FMaxFrameSize[index] = maxFrameSize;
                        FNumColorChannels[index] = numUsedColChannels;
                        FFriendlyName[index] = friendlyNamePtr;

                        if (FDebug[0]) FLogger.Log(LogType.Debug, "Device " + index + ": " + deviceName + ", " + friendlyNamePtr + ", Minimum Speed: " + minSpeed + ", Maximum Speed: " + maxSpeed + ", Maximum Frame Size: " + maxFrameSize + ", Used Color Channels: " + numUsedColChannels);
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(deviceNamesPtr[index]);
                    }
                }

                //show output
                if (FEnable[index])
                {
                    if (!enabled[index])
                    {
                        int start = jmLaserStartOutput(FHandle[index]);
                        enabled[index] = true;
                        if (FDebug[0]) FLogger.Log(LogType.Debug, "Start Output Device " + index + ":" + Error[start * (-1)]);
                    }

                    // Let's first cancel all running tasks (if any).
                    //                    CancelRunningTasks(index);

                    // Create a new task cancellation source object.
                    FCts[index] = new CancellationTokenSource();
                    // Retrieve the cancellation token from it which we'll use for
                    // the new tasks we setup up now.
                    ct[index] = FCts[index].Token;

                    // Now setup a new task which will perform the long running
                    FTasks[index] = Task.Factory.StartNew(() =>
                    {
                        // Should a cancellation be requested throw the task
                        // canceled exception.
                        // In this specific scenario this seems a little useless,
                        // but imagine your long running computation runs in a loop 
                        // you could call this method in each iteration. 
                        // Also many asynchronously methods in .NET provide an overload
                        // which takes a cancellation token.
                        ct[index].ThrowIfCancellationRequested();
                        int wFrame = 0;
                        int wait = 0;
                        // Here is the actual compution:
                        if (FUseFixedTiming[0])
                        {
                            wait = (int)Math.Ceiling((1000.0 / FPointsPerSecond[index]) * FFrameIn[index].frameSize);
                            wFrame = jmLaserWriteFrameNL(FHandle[index], FFrameIn[index].vectors, (uint)FFrameIn[index].frameSize, FPointsPerSecond[index], 0);
                            Thread.Sleep(wait);
                        }
                        else {
                            checkReady = jmLaserIsDeviceReady(FHandle[index]);
                            if (checkReady == 1)
                            {
                                wFrame = jmLaserWriteFrameNL(FHandle[index], FFrameIn[index].vectors, (uint)FFrameIn[index].frameSize, FPointsPerSecond[index], 0);
                            }
                        }
                        return new { Value = wFrame, Value2 = wait };
                    },
                    // The cancellation token should also be passed to the StartNew method.
                    // For details see http://msdn.microsoft.com/en-us/library/dd997396%28v=vs.110%29.aspx
                    ct[index]
                    // Once the task is completed we want to write the result to the output.
                    // Writing to pins is only allowed in the main thread of vvvv. To achieve
                    // this we setup a so called continuation which we tell to run on the
                    // task scheduler of the main thread, which is in fact the one who called
                    // the Evaluate method of this plugin.
                    ).ContinueWith(t =>
                    {
                        // Write the result to the outputs
                        //                        FOutput[index] = t.Result.Value;
                        // Note that in this particular example writing out the string
                        // will take a very long time - so should more or less be seen
                        // as a debug output.
                        //                        FStringOut[index] = t.Result.ValueAsString;
                        // And set the ready state to true
                        //FReadyOut[index] = true;
                        FFrameDuration[index] = t.Result.Value2;
                        if (FDebug[0]) FLogger.Log(LogType.Debug, "Write Frame " + index + ":" + Error[t.Result.Value * (-1)]);

                    },
                    // Same as in StartNew we pass the used cancellation token
                    ct[index],
                    // Here we can specify some options under which circumstances the 
                    // continuation should run. In this case we only want it to run if
                    // the task wasn't cancelled before.
                    TaskContinuationOptions.OnlyOnRanToCompletion,
                    // This way we tell the continuation to run on the main thread of vvvv.
                    TaskScheduler.FromCurrentSynchronizationContext()
                    );
                }
                else {
                    if (enabled[index])
                    {
                        int stop = jmLaserStopOutput(FHandle[index]);
                        if (FDebug[0]) FLogger.Log(LogType.Debug, "Stop Output " + index + ":" + Error[stop * (-1)]);
                        CancelRunningTasks(index);
                        enabled[index] = false;
                    }
                }
                FDeviceCount[0] = deviceCount;
                FMaxSpeed[index] = jmLaserGetMaxSpeed(FHandle[index]);
            }
        }

        private void CancelRunningTasks(int index)
        {
            if (FCts[index] != null)
            {
                // All our running tasks use the cancellation token of this cancellation
                // token source. Once we call cancel the ct.ThrowIfCancellationRequested()
                // will throw and the task will transition to the canceled state.
                FCts[index].Cancel();

                // Dispose the cancellation token source and set it to null so we know
                // to setup a new one in a next frame.
                FCts[index].Dispose();
                FCts[index] = null;
            }
        }
    }

    #region PluginInfo
    [PluginInfo(Name = "ResampleAndBlanking", Category = "Netlase", Help = "Insert Points and set Blanking", Tags = "Netlase, Laser, ILDA", Author = "digitalWannabe")]
    #endregion PluginInfo
    public class NetlaseResampleAndBlankingNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        #region fields & pins				
        [Input("Vectors", Order = 0, BinVisibility = PinVisibility.Hidden)]
        public ISpread<ISpread<Vector2D>> FXY;

        [Input("Color", BinVisibility = PinVisibility.Hidden)]
        public ISpread<ISpread<RGBAColor>> FColorIn;

        [Input("Point Type", DefaultValue = 0, MinValue = 0, MaxValue = 4, BinVisibility = PinVisibility.Hidden)]
        public ISpread<ISpread<int>> FPtype;

        [Input("Blanking Color", IsSingle = true)]
        public ISpread<RGBAColor> FColorBlankIn;

        //		[Input("Before Point Blanking", DefaultValue = 0, MinValue =0)]
        //		public ISpread<int> FPbefore;

        [Input("Before Point Blanking Minimum", DefaultValue = 0, MinValue = 0)]
        public ISpread<int> FPbeforeMin;

        [Input("Before Point Blanking Maximum", DefaultValue = 0, MinValue = 0)]
        public ISpread<int> FPbeforeMax;

        [Input("Point Repeats", DefaultValue = 0, MinValue = 0)]
        public ISpread<int> FPrepeat;

        //		[Input("After Point Blanking", DefaultValue = 0, MinValue =0)]
        //		public ISpread<int> FPafter;

        [Input("After Point Blanking Minimum", DefaultValue = 0, MinValue = 0)]
        public ISpread<int> FPafterMin;

        [Input("After Point Blanking Maximum", DefaultValue = 0, MinValue = 0)]
        public ISpread<int> FPafterMax;

        [Input("Before Line Blanking Minimum", DefaultValue = 0, MinValue = 0)]
        public ISpread<int> FLbeforeMin;

        [Input("Before Line Blanking Maximum", DefaultValue = 0, MinValue = 0)]
        public ISpread<int> FLbeforeMax;

        [Input("After Line Blanking Minimum", DefaultValue = 0, MinValue = 0)]
        public ISpread<int> FLafterMin;

        [Input("After Line Blanking Maximum", DefaultValue = 0, MinValue = 0)]
        public ISpread<int> FLafterMax;

        [Input("Line Point Repeats", DefaultValue = 0, MinValue = 0)]
        public ISpread<int> FInline;

        [Input("Target Point Count", DefaultValue = 1.0, StepSize = 1)]
        public ISpread<double> FPamount;

        [Input("Step Size", DefaultValue = 0.02)]
        public ISpread<double> FStepSize;


        [Output("PointStartIndex", Visibility = PinVisibility.False, BinVisibility = PinVisibility.False)]
        public ISpread<ISpread<int>> FPstartIndex;

        [Output("Actual Frame Size", Order = 1)]
        public ISpread<int> FFrameSize;

        //		[Output("ColorOut", Visibility  = PinVisibility.False, BinVisibility = PinVisibility.False)]
        //		public ISpread<ISpread<RGBAColor>> FColorOut;

        //		[Output("VectorOut", Visibility  = PinVisibility.False , BinVisibility = PinVisibility.False)]
        //		public ISpread<ISpread<Vector2D>> FXYout;

        [Output("Frame", Order = 0)]
        public ISpread<NetLaseFrame> FFrameOut;

        [Import()]
        public ILogger FLogger;
        #endregion fields & pins


        Spread<NetLaseVector> netVectorSpread;
        NetLaseVector netVector;
        NetLaseFrame nFrame;
        Vector2D Null;
        RGBAColor black;
        Spread<Spread<RGBAColor>> Color;
        Spread<RGBAColor> color;
        Spread<Spread<Vector2D>> Vector;
        Spread<int> before;
        Spread<int> after;
        Spread<Spread<int>> Before;
        Spread<Spread<int>> After;
        Spread<Vector2D> vector;
        Spread<Vector2D> FXYedit;
        Spread<double> angles;
        Spread<Spread<int>> startIndex;
        Spread<int> binSizes;
        int maxBinSize;



        public void OnImportsSatisfied()
        {

            netVectorSpread = new Spread<NetLaseVector>();
            netVector = new NetLaseVector();
            nFrame = new NetLaseFrame();
            black = new RGBAColor(0.0, 0.0, 0.0, 0.0);
            Null.x = 32767;
            Null.y = 32767;
            Color = new Spread<Spread<RGBAColor>>();
            color = new Spread<RGBAColor>();
            Vector = new Spread<Spread<Vector2D>>();
            vector = new Spread<Vector2D>();
            FXYedit = new Spread<Vector2D>();
            //mit fpstartindex ersetzen
            startIndex = new Spread<Spread<int>>();
            before = new Spread<int>();
            after = new Spread<int>();
            Before = new Spread<Spread<int>>();
            After = new Spread<Spread<int>>();
            angles = new Spread<double>();
            binSizes = new Spread<int>();
            maxBinSize = 0;
        }

        public ISpread<Vector2D> RemapAllInputs(ISpread<Vector2D> points)
        {
            int binSize = points.SliceCount;
            for (int i = 0; i < binSize; i++)
            {
                int index = i;
                Vector2D Vec;
                Vec.x = Math.Floor(VMath.Map(points[index].x, -1, 1, 0, 65535, TMapMode.Clamp));
                Vec.y = Math.Floor(VMath.Map(points[index].y, -1, 1, 0, 65535, TMapMode.Clamp));
                points[index] = Vec;
            }
            return points;
        }

        public double SumUpLength(ISpread<Vector2D> points)
        {
            double length = 0;
            int binSize = points.SliceCount;
            for (int i = 0; i < binSize; i++)
            {
                int index = i;
                length += VMath.Dist(points[index], points[index + 1]);
            }
            return length;
        }

        public void CountRepeats(int bin, int binSize)
        {
            int repeat = 0;
            //				int binSize = FXYedit.SliceCount;
            int[] repeats = { FPrepeat[bin], 0, FInline[bin], 0, 0 };

            for (int i = 0; i < binSize; i++)
            {
                int index = i;
                //				int[] before = {Before[bin][index],FLbefore[bin],0,0,0};
                //				int[] after = {After[bin][index],0,0,FLafter[bin],0};
                repeat += repeats[FPtype[bin][index]] + Before[bin][index] + After[bin][index];
            }
            FPamount[bin] -= repeat;

        }

        public void CountBlanks(int bin, int binSize)
        {

            before.SliceCount = binSize;
            after.SliceCount = binSize;
            for (int i = 0; i < binSize; i++)
            {
                int index = i;
                int[] bforeMin = { FPbeforeMin[bin], FLbeforeMin[bin], 0, 0, 0 };
                int[] bforeMax = { FPbeforeMax[bin], FLbeforeMax[bin], 0, 0, 0 };
                int[] aftrMin = { FPafterMin[bin], 0, 0, FLafterMin[bin], 0 };
                int[] aftrMax = { FPafterMax[bin], 0, 0, FLafterMax[bin], 0 };
                before[index] = (int)Math.Floor(VMath.Lerp(bforeMin[FPtype[bin][index]], bforeMax[FPtype[bin][index]], angles[i]));
                after[index] = (int)Math.Floor(VMath.Lerp(aftrMin[FPtype[bin][index]], aftrMax[FPtype[bin][index]], angles[i]));
            }
            Before[bin] = before;
            After[bin] = after;
        }

        public void ResamplePoints(int bin, int binSize)
        {
            //			FXYout[bin].SliceCount=0;
            //			FColorOut[bin].SliceCount=0;
            color.SliceCount = 0;
            vector.SliceCount = 0;


            //			int binSize= Vector[bin].SliceCount;
            FPstartIndex[bin].SliceCount = binSize;

            //			FLogger.Log(LogType.Debug, "binsize " +bin + ":" + binSize);
            int sum = 0;
            //			double stepsize = length/(FPamount[bin]-1);
            //			ColOut[bin].Add(FColorIn[bin][0]);


            for (int i = 0; i < binSize - 1; i++)
            {
                int index = i;
                int steps = (int)Math.Floor(VMath.Dist(Vector[bin][index], Vector[bin][index + 1]) * (FStepSize[index] / 1000));
                steps = Math.Max(1, steps);
                FPstartIndex[bin][index] = sum;

                for (int j = 0; j < steps; j++)
                {
                    int jndex = j;
                    double stepsize2 = 1.0 / steps;
                    Vector2D Vec;
                    //cannot floor vector2d(?) -> split to components
                    //ausserdem: entweder points oder fxy[bin] verwenden
                    Vec.x = Math.Floor(VMath.Lerp(Vector[bin][index].x, Vector[bin][index + 1].x, stepsize2 * jndex));
                    Vec.y = Math.Floor(VMath.Lerp(Vector[bin][index].y, Vector[bin][index + 1].y, stepsize2 * jndex));

                    vector.Add(Vec);
                    switch (FPtype[bin][index])
                    {
                        case 0:
                            if (jndex == 0) { color.Add(Color[bin][index]); } else { color.Add(FColorBlankIn[0]); }
                            break;
                        case 1:
                            if (jndex == 0) { color.Add(Color[bin][index]); } else { color.Add(VColor.LerpRGBA(Color[bin][index], Color[bin][index + 1], stepsize2 * jndex)); }
                            break;
                        case 2:
                            if (jndex == 0) { color.Add(Color[bin][index]); } else { color.Add(VColor.LerpRGBA(Color[bin][index], Color[bin][index + 1], stepsize2 * jndex)); }
                            break;
                        case 3:
                            //						color.Add(Color[bin][index]);
                            if (jndex == 0) { color.Add(Color[bin][index]); } else { color.Add(FColorBlankIn[0]); }
                            break;
                        case 4: break;
                        default: break;
                    }
                }
                FPstartIndex[bin][0] = 0;

                switch (FPtype[bin][index])
                {
                    case 0:
                        for (int k = 0; k < FPrepeat[bin]; k++)
                        {
                            int kndex = k;
                            //entweder points oder fxy[bin] verwenden
                            vector.Insert(FPstartIndex[bin][index], Vector[bin][index]);
                            color.Insert(FPstartIndex[bin][index], Color[bin][index]);
                            sum++;
                        }
                        for (int k = 0; k < Before[bin][index]; k++)
                        {
                            int kndex = k;
                            vector.Insert(FPstartIndex[bin][index], Vector[bin][index]);
                            color.Insert(FPstartIndex[bin][index], FColorBlankIn[0]);
                            sum++;
                        }
                        for (int k = 0; k < After[bin][index]; k++)
                        {
                            int kndex = k;
                            vector.Insert(FPstartIndex[bin][index] + Before[bin][index] + FPrepeat[bin] + 1, Vector[bin][index]);
                            color.Insert(FPstartIndex[bin][index] + Before[bin][index] + FPrepeat[bin] + 1, FColorBlankIn[0]);
                            sum++;
                        }
                        break;
                    case 1:
                        //						vector.Insert(FPstartIndex[bin][index],Vector[bin][index]);
                        //						color.Insert(FPstartIndex[bin][index],Color[bin][index]);
                        //						sum++;
                        for (int k = 0; k < Before[bin][index]; k++)
                        {
                            int kndex = k;
                            vector.Insert(FPstartIndex[bin][index], Vector[bin][index]);
                            color.Insert(FPstartIndex[bin][index], FColorBlankIn[0]);
                            sum++;
                        }
                        break;
                    case 2:
                        for (int k = 0; k < FInline[bin]; k++)
                        {
                            int kndex = k;
                            vector.Insert(FPstartIndex[bin][index], Vector[bin][index]);
                            color.Insert(FPstartIndex[bin][index], Color[bin][index]);
                            sum++;
                        }
                        break;
                    case 3:

                        for (int k = 0; k < After[bin][index]; k++)
                        {
                            int kndex = k;
                            vector.Insert(FPstartIndex[bin][index] + Before[bin][index] + FInline[bin] + 2, Vector[bin][index]);
                            color.Insert(FPstartIndex[bin][index] + Before[bin][index] + FInline[bin] + 2, FColorBlankIn[0]);
                            sum++;
                        }
                        //						vector.Insert(FPstartIndex[bin][index]+Before[bin][index]+FInline[bin],Vector[bin][index]);
                        //						color.Insert(FPstartIndex[bin][index]+Before[bin][index]+FInline[bin],Color[bin][index]);
                        //						sum++;

                        break;
                    case 4: break;
                    default: break;
                }
                //add the resampled points				
                sum += steps;
            }

            vector.Add(Null);
            Vector[bin] = vector;
            color.Add(black);
            Color[bin] = color;

            sum++;
        }


        public void AddStartPoints(int bin)
        {

            int steps = (int)Math.Floor(VMath.Dist(Vector[bin][0], Null) * (FStepSize[bin] / 1000));
            steps = Math.Max(1, steps);
            double stepSize = 1.0 / steps;
            Vector2D Vec, firstVec;
            firstVec = Vector[bin][0];
            for (int j = 1; j < steps; j++)
            {
                Vec.x = Math.Floor(VMath.Lerp(firstVec.x, Null.x, stepSize * j));
                Vec.y = Math.Floor(VMath.Lerp(firstVec.y, Null.y, stepSize * j));
                Vector[bin].Insert(0, Vec);
                Color[bin].Insert(0, FColorBlankIn[0]);
            }

        }

        public void ComputeAbsAngles(int bin, int binSize)
        {


            angles.SliceCount = binSize;
            Vector2D Vec1, Vec2;
            for (int j = 0; j < binSize; j++)
            {

                if (j == 0) Vec1 = Null - Vector[bin][j];
                else Vec1 = Vector[bin][j - 1] - Vector[bin][j];

                Vec2 = Vector[bin][j + 1] - Vector[bin][j];
                angles[j] = VMath.Map(Math.Abs(Math.Atan2(Vec2.y, Vec2.x) - Math.Atan2(Vec1.y, Vec1.x)), Math.PI, 0.0, 0.0, 1.0, TMapMode.Clamp);
            }
        }

        public void FillSmallFrames(int bin, int binSize)
        {

            int repeats = 10 - binSize;
            Vector2D Vec;
            for (int j = 0; j < repeats; j++)
            {
                Vec.x = j * 1000;
                Vec.y = j * 1000;
                //					Vector[bin].SliceCount+=1;
                Vector[bin].Add(Vector[bin][j]);
                Color[bin].Add(black);
            }

        }

        public void GetBinSizes(int SpreadMax)
        {

            maxBinSize = 0;
            for (int j = 0; j < SpreadMax; j++)
            {
                binSizes[j] = FXY[j].SliceCount;
                if (binSizes[j] >= maxBinSize)
                {
                    maxBinSize = binSizes[j];
                }
            }

        }

        public void EqualizeFrameSize(int bin)
        {

            //				Vector2D Vec;								
            double diff = (binSizes[bin] / (double)maxBinSize);
            int fill = maxBinSize - binSizes[bin];
            //				if (diff<0.8){

            for (int i = 0; i < fill; i++)
            {

                //						Vec.x= i*5;
                //						Vec.y= -i*5;
                Vector[bin].Add(Vector[bin][i]);
                Color[bin].Add(black);

            }
        }



        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            SpreadMax = SpreadUtils.SpreadMax(FXY, FColorIn);
            FFrameOut.SliceCount = SpreadMax;
            FFrameSize.SliceCount = SpreadMax;
            Vector.SliceCount = SpreadMax;
            Color.SliceCount = SpreadMax;
            Before.SliceCount = SpreadMax;
            After.SliceCount = SpreadMax;
            binSizes.SliceCount = SpreadMax;

            GetBinSizes(SpreadMax);

            for (int i = 0; i < SpreadMax; i++)
            {
                int index = i;


                FXY[index] = RemapAllInputs(FXY[index]);
                FXYedit = FXY[index].ToSpread();
                Vector[index] = FXY[index].ToSpread();
                Color[index] = FColorIn[index].ToSpread();
                //				int binSize = Vector[index].SliceCount;
                //brauchts nimma
                //				double length = SumUpLength(FXY[index]);
                //eigtl: erst resample, dann die punkte wiederholen
                //				if (FXY[index].SliceCount<10.0) RepeatPoints(index);
                if (binSizes[index] < 10) FillSmallFrames(index, binSizes[index]);

                EqualizeFrameSize(index);
                //Slicecount changes-> ask again
                int binSize = Vector[index].SliceCount;
                ComputeAbsAngles(index, binSize);
                CountBlanks(index, binSize);
                CountRepeats(index, binSize);
                //				
                ResamplePoints(index, binSize);
                AddStartPoints(index);
                int vCount = Vector[index].SliceCount;
                netVectorSpread.SliceCount = 0;
                for (int j = 0; j < vCount; j++)
                {
                    int jndex = j;
                    netVector.x = (ushort)Vector[index][jndex].x;
                    netVector.y = (ushort)Vector[index][jndex].y;
                    netVector.r = (byte)(Math.Floor(Color[index][jndex].R * 255));
                    netVector.g = (byte)(Math.Floor(Color[index][jndex].G * 255));
                    netVector.b = (byte)(Math.Floor(Color[index][jndex].B * 255));
                    netVector.i = (byte)(Math.Floor(Color[index][jndex].A * 255));
                    netVector.v = 0;
                    netVector.c = 0;
                    netVector.ye = 0;
                    netVector.reserved = 0;
                    netVectorSpread.Add(netVector);
                }
                nFrame.vectors = netVectorSpread.ToArray();
                nFrame.frameSize = vCount;
                FFrameOut[index] = nFrame;
                FFrameSize[index] = vCount;
            }
        }
    }


    [PluginInfo(Name = "SplitFrame", Category = "Netlase", Help = "Split a Netlase frame into it's components", Tags = "Netlase, Laser, ILDA", Author = "digitalWannabe")]

    public class NetlaseSplitFrameNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        #region fields & pins				
        [Input("input")]
        public ISpread<NetLaseFrame> FNFrameIn;

        [Output("NetVectorOut")]
        public ISpread<NetLaseVector[]> FNvec;

        [Output("Frame Size")]
        public ISpread<int> FFSize;

        [Import()]
        public ILogger FLogger;
        #endregion fields & pins

        public void OnImportsSatisfied()
        {
        }

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            FNvec.SliceCount = SpreadMax;
            FFSize.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                int index = i;
                FNvec[index] = FNFrameIn[index].vectors;
                FFSize[index] = FNFrameIn[index].frameSize;
            }
        }
    }

    [PluginInfo(Name = "SplitVector", Category = "Netlase", Help = "Split a Netlase vector into it's components", Tags = "Netlase, Laser, ILDA", Author = "digitalWannabe")]

    public class NetlaseSplitVectorNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        #region fields & pins				
        [Input("input")]
        public ISpread<NetLaseVector[]> FNVecIn;

        [Output("Output", BinVisibility = PinVisibility.Hidden)]
        public ISpread<ISpread<Vector2D>> Fvec;

        [Output("Colors", BinVisibility = PinVisibility.Hidden)]
        public ISpread<ISpread<RGBAColor>> FColOut;

        [Import()]
        public ILogger FLogger;
        #endregion fields & pins

        public void OnImportsSatisfied()
        {
        }

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            Fvec.SliceCount = SpreadMax;
            FColOut.SliceCount = SpreadMax;
            for (int i = 0; i < SpreadMax; i++)
            {
                int index = i;
                int binsize = FNVecIn[index].Length;
                Fvec[index].SliceCount = binsize;
                FColOut[index].SliceCount = binsize;
                Vector2D vec;
                RGBAColor col;
                for (int j = 0; j < binsize; j++)
                {
                    int jndex = j;
                    vec.x = FNVecIn[index][jndex].x;
                    vec.y = FNVecIn[index][jndex].y;
                    col.R = FNVecIn[index][jndex].r / 255.0;
                    col.G = FNVecIn[index][jndex].g / 255.0;
                    col.B = FNVecIn[index][jndex].b / 255.0;
                    col.A = FNVecIn[index][jndex].i / 255.0;
                    FColOut[index][jndex] = col;
                    Fvec[index][jndex] = vec;
                }
            }
        }
    }

    [PluginInfo(Name = "JoinFrame", Category = "Netlase", Help = "Build a Netlase Frame", Tags = "Netlase, Laser, ILDA", Author = "digitalWannabe")]

    public class NetlaseJoinFrameNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        #region fields & pins				
        [Output("Output")]
        public ISpread<NetLaseFrame> FNFrameOut;

        [Input("netVector")]
        public ISpread<NetLaseVector[]> FNvec;

        [Input("Frame Size")]
        public ISpread<int> FFSize;

        [Import()]
        public ILogger FLogger;
        #endregion fields & pins

        NetLaseFrame nFrame;

        public void OnImportsSatisfied()
        {
            nFrame = new NetLaseFrame();
        }

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            FNFrameOut.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                int index = i;
                nFrame.vectors = FNvec[index];
                nFrame.frameSize = FFSize[index];
                FNFrameOut[index] = nFrame;
            }
        }
    }

    [PluginInfo(Name = "JoinVector", Category = "Netlase", Help = "Build Netlase Vectors", Tags = "Netlase, Laser, ILDA", Author = "digitalWannabe")]

    public class NetlaseJoinVectorNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        #region fields & pins				
        [Output("Output")]
        public ISpread<NetLaseVector[]> FNVecOut;

        [Input("Input", BinVisibility = PinVisibility.Hidden)]
        public ISpread<ISpread<Vector2D>> FvecIn;

        [Input("Colors", BinVisibility = PinVisibility.Hidden)]
        public ISpread<ISpread<RGBAColor>> FColIn;

        [Import()]
        public ILogger FLogger;
        #endregion fields & pins

        Spread<NetLaseVector> netVectorSpread;
        NetLaseVector netVector;

        public void OnImportsSatisfied()
        {

            netVectorSpread = new Spread<NetLaseVector>();
            netVector = new NetLaseVector();
        }

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            SpreadMax = SpreadUtils.SpreadMax(FvecIn, FColIn);
            FNVecOut.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                int index = i;
                int vCount = FvecIn[index].SliceCount;
                netVectorSpread.SliceCount = 0;
                for (int j = 0; j < vCount; j++)
                {
                    int jndex = j;
                    netVector.x = (ushort)FvecIn[index][jndex].x;
                    netVector.y = (ushort)FvecIn[index][jndex].y;
                    netVector.r = (byte)(Math.Floor(FColIn[index][jndex].R * 255));
                    netVector.g = (byte)(Math.Floor(FColIn[index][jndex].G * 255));
                    netVector.b = (byte)(Math.Floor(FColIn[index][jndex].B * 255));
                    netVector.i = (byte)(Math.Floor(FColIn[index][jndex].A * 255));
                    netVector.v = 0;
                    netVector.c = 0;
                    netVector.ye = 0;
                    netVector.reserved = 0;
                    netVectorSpread.Add(netVector);
                }

                FNVecOut[index] = netVectorSpread.ToArray();

            }

        }
    }

    #region vectors
    public struct NetLaseVector
    {
        public ushort x;
        public ushort y;
        public byte r;
        public byte g;
        public byte b;
        public byte i;
        public byte v;
        public byte c;
        public byte ye;
        public byte reserved;

    }

    public struct NetLaseFrame
    {
        public NetLaseVector[] vectors;
        public int frameSize;
    }

    #endregion vectors	
}
