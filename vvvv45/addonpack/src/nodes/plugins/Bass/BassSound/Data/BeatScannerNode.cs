using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using BassSound.Data.BeatScanner;
using System.IO;
using Un4seen.Bass;
using vvvv.Utils;

namespace VVVV.Nodes
{
    public class BeatScannerNode : IPlugin
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "BeatScanner";
                Info.Category = "Bass";
                Info.Version = "";
                Info.Help = "Scans file(s) for Beats, one thread per parameters";
                Info.Bugs = "";
                Info.Credits = "";
                Info.Warnings = "";
                Info.Author = "vux";
                Info.Tags = "Audio,Sound,Analysis";

                //leave below as is
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                System.Diagnostics.StackFrame sf = st.GetFrame(0);
                System.Reflection.MethodBase method = sf.GetMethod();
                Info.Namespace = method.DeclaringType.Namespace;
                Info.Class = method.DeclaringType.Name;
                return Info;
            }
        }
        #endregion

        private IPluginHost FHost;

        private List<ThreadedBeatScanner> FScanners;
        private bool FInvalidate = false;
        private List<List<double>> FPositions;
        private List<double> FProgress;

        #region Pins
        private IStringIn FPinInFilename;
        private IValueIn FPinInRelease;
        private IValueIn FPinInBandWidth;
        private IValueIn FPinInCenter;

        private IValueOut FPinOutBeatPositions;
        private IValueOut FPinOutPercent;
        private IValueOut FPinOutBinSizes;
        #endregion

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            Bass.BASS_Init(0, 48000, 0, IntPtr.Zero, null);
            BassUtils.LoadPlugins();

            this.FHost = Host;
            this.FScanners = new List<ThreadedBeatScanner>();
            this.FPositions = new List<List<double>>();
            this.FProgress = new List<double>();
            
            //Input Pins
            
            this.FHost.CreateStringInput("Path", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInFilename);
            this.FPinInFilename.SetSubType("", true);
            this.FHost.CreateValueInput("BandWidth", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInBandWidth);
            this.FPinInBandWidth.SetSubType(0, double.MaxValue, 0.01, 10.0, false, false, false);
            this.FHost.CreateValueInput("Center Frequency", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInCenter);
            this.FPinInCenter.SetSubType(0, double.MaxValue, 0.01, 90.0, false, false, false);
            this.FHost.CreateValueInput("Release Time", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInRelease);
            this.FPinInRelease.SetSubType(0, double.MaxValue, 1, 20.0, false, false, true);

            //Output pins
            this.FHost.CreateValueOutput("Beat Positions", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutBeatPositions);
            this.FPinOutBeatPositions.SetSubType(0, double.MaxValue, 0.01, 0, false, false, false);
            this.FHost.CreateValueOutput("Percent Complete", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutPercent);
            this.FPinOutPercent.SetSubType(0, 100, 0.01, 0, false, false, false);
            this.FHost.CreateValueOutput("Bin Sizes", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutBinSizes);
            this.FPinOutBinSizes.SetSubType(0, double.MaxValue, 0.01, 0, false, false, true);
        }
        #endregion

        #region Configurate
        public void Configurate(IPluginConfig Input)
        {

        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            #region Pin Changed
            if (this.FPinInFilename.PinIsChanged ||
                this.FPinInBandWidth.PinIsChanged ||
                this.FPinInCenter.PinIsChanged ||
                this.FPinInRelease.PinIsChanged)
            {
                this.FInvalidate = false;
                this.StopScan();

                string path;
                double center, width, release;

                this.FPinOutPercent.SliceCount = SpreadMax;
                this.FPinOutBinSizes.SliceCount = SpreadMax;
                this.FPinOutBeatPositions.SliceCount = 0;

                for (int i = 0; i < SpreadMax; i++)
                {
                    this.FPinInCenter.GetValue(i, out center);
                    this.FPinInFilename.GetString(i, out path);
                    this.FPinInRelease.GetValue(i, out release);
                    this.FPinInBandWidth.GetValue(i, out width);

                    if (File.Exists(path))
                    {
                        BeatScannerParameters prms = new BeatScannerParameters();
                        prms.Index = i;
                        prms.Center = Convert.ToSingle(center);
                        prms.Release = Convert.ToSingle(release);
                        prms.Width = Convert.ToSingle(width);

                        ThreadedBeatScanner scanner = new ThreadedBeatScanner(path, prms);
                        this.FScanners.Add(scanner);
                        this.FPositions.Add(new List<double>());     
                        this.FProgress.Add(0);
                        this.FPinOutPercent.SetValue(i, 0);
                        this.FPinOutBinSizes.SetValue(i, 0);
                        
                        scanner.OnBeatFound += scanner_OnBeatFound;
                        scanner.OnComplete += scanner_OnComplete;
                        scanner.Start();

                        
                    }
                    else
                    {
                        this.FProgress.Add(-1);
                        this.FPinOutPercent.SetValue(i, -1);
                        this.FPinOutBinSizes.SetValue(i, 0);
                    }
                }
            }
            #endregion

            if (this.FInvalidate)
            {
                int totalpositions = 0;
                for (int i = 0; i < this.FProgress.Count; i++)
                {
                    this.FPinOutPercent.SetValue(i, this.FProgress[i]);
                    this.FPinOutBinSizes.SetValue(i, this.FPositions[i].Count);
                    totalpositions += this.FPositions[i].Count;
                }

                this.FPinOutBeatPositions.SliceCount = totalpositions;
                int counter = 0;
                foreach (List<double> lstpositions in this.FPositions)
                {
                    foreach (double dbl in lstpositions)
                    {
                        this.FPinOutBeatPositions.SetValue(counter, dbl);
                        counter++;
                    }
                }
            }
        }
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return false; }
        }
        #endregion

        #region IDisposable Members
        public virtual void Dispose()
        {
            this.StopScan();
        }
        #endregion

        #region StopScanning
        private void StopScan()
        {
            foreach (ThreadedBeatScanner scanner in this.FScanners)
            {
                scanner.Stop();
            }
            this.FPositions.Clear();
            this.FScanners.Clear();
            this.FProgress.Clear();
        }
        #endregion

        #region Events 
        private void scanner_OnBeatFound(int index, double position, double progress)
        {
            this.FInvalidate = true;
            this.FPositions[index].Add(position);
            this.FProgress[index] = progress;
        }

        private void scanner_OnComplete(int index)
        {
            this.FInvalidate = true;
            this.FProgress[index] = 100.0;
        }
        #endregion
    }
}
