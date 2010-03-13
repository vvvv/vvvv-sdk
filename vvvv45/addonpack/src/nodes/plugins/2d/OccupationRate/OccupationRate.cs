using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;

namespace vvvv.Nodes
{
    public class OccupationRateNode : IPlugin
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "OccupationRate";
                Info.Category = "2d";
                Info.Version = "";
                Info.Help = "Calculate the occupation rate of a value within a grid";
                Info.Bugs = "";
                Info.Credits = "";
                Info.Warnings = "";
                Info.Author = "vux";
                Info.Tags = "Statistics";

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

        private IPluginHost FHost;

        private IValueIn FPinInReset;

        private IValueIn FPinInput;

        private IValueIn FPinInResolutionX;
        private IValueIn FPinInResolutionY;

        private IValueOut FPinOutput;

        private int FSizeX;
        private int FSizeY;
        private double[,] FArea;
        private double FTotal = 0;

        #region Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;

            //Input pins
            this.FHost.CreateValueInput("Input", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInput);
            this.FPinInput.SetSubType2D(-1, 1, 0.01, 0,0, false, false, false);

            this.FHost.CreateValueInput("Reset", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInReset);
            this.FPinInReset.SetSubType(0, 1, 1, 0, true, false, true);

            //Grid resolution
            this.FHost.CreateValueInput("Resolution X", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInResolutionX);
            this.FPinInResolutionX.SetSubType(1, double.MaxValue,1, 1, false, false, true);

            this.FHost.CreateValueInput("Resolution Y", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInResolutionY);
            this.FPinInResolutionY.SetSubType(1, double.MaxValue, 1, 1, false, false, true);

            this.FHost.CreateValueOutput("Output", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutput);
            this.FPinOutput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
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
            #region Resolution Pins
            if (this.FPinInResolutionX.PinIsChanged || this.FPinInResolutionY.PinIsChanged)
            {
                //In this case we reset
                double d;

                this.FPinInResolutionX.GetValue(0, out d);
                this.FSizeX = Convert.ToInt32(d);

                this.FPinInResolutionY.GetValue(0, out d);
                this.FSizeY = Convert.ToInt32(d);

                this.FArea = new double[this.FSizeX, this.FSizeY];
                this.FTotal = 0;
            }
            #endregion

            #region Reset Pin
            if (this.FPinInReset.PinIsChanged)
            {
                double d;
                this.FPinInReset.GetValue(0, out d);

                if (d == 1)
                {
                    this.FArea = new double[this.FSizeX, this.FSizeY];
                    this.FTotal = 0;
                }
            }
            #endregion

            /*Note: here we don't check if input pin is changed has if we stay
            in the same place it has to be processed
             */
            for (int i = 0; i < this.FPinInput.SliceCount; i++)
            {
                double idx, idy;
                this.FPinInput.GetValue2D(i, out idx, out idy);

                //Increment the found area for each slice
                this.FArea[GetIndexX(idx), GetIndexY(idy)]++;
                this.FTotal++;
            }
                      
            //Output the table
            this.FPinOutput.SliceCount = this.FSizeX * this.FSizeY;
            
            int count = 0;
            for (int y = 0; y < this.FSizeY; y++)
            {
                for (int x = 0; x < this.FSizeX; x++)
                {
                    double rate = 0;
                    if (this.FTotal > 0)
                    {
                        //Percentage of occupation
                        rate = (double)FArea[x, y] / (double)this.FTotal;
                    }
                    this.FPinOutput.SetValue(count, rate);
                    count++;
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

        #region Get Indexes
        private int GetIndexX(double x)
        {
            x++;
            double stepX = 2.0 / (double)this.FSizeX;       
            double sx = x / stepX;
            return Convert.ToInt32(Math.Truncate(sx));
        }

        private int GetIndexY(double y)
        {
            y++;
            double stepY = 2.0 / (double)this.FSizeY;
            double sy = y / stepY;
            return Convert.ToInt32(Math.Truncate(sy));
        }
        #endregion
    }
}
