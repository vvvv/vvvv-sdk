using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using NTrees.Lib.Oct;

namespace VVVV.Nodes
{
    public class OctTreeNode : IPlugin
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "OctTree";
                Info.Category = "3d";
                Info.Version = "";
                Info.Help = "Get Cube tree subdivision for a set of points";
                Info.Bugs = "";
                Info.Credits = "";
                Info.Warnings = "";
				Info.Author = "vux";

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

        #region Pins
        private IValueIn FPinInput;
        private IValueIn FPinInElementCount;
        private IValueIn FPinInBoundsMin;
        private IValueIn FPinInBoundsMax;

        private IValueOut FPinOutputCenter;
        private IValueOut FPinOutputWidth;
        private IValueOut FPinOutputHeight;
        private IValueOut FPinOutputDepth;
        #endregion

        #region Set Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;

            this.FHost.CreateValueInput("Input", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInput);
            this.FPinInput.SetSubType3D(double.MinValue, double.MaxValue, 0.01,0, 0, 0, false, false, false);

            this.FHost.CreateValueInput("Maximum Elements", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInElementCount);
            this.FPinInElementCount.SetSubType(1, double.MaxValue, 1, 1, false, false, true);

            this.FHost.CreateValueInput("Bounds Min", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInBoundsMin);
            this.FPinInBoundsMin.SetSubType3D(double.MinValue, double.MaxValue, 0.01, -1, -1,-1, false, false, false);

            this.FHost.CreateValueInput("Bounds Max", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInBoundsMax);
            this.FPinInBoundsMax.SetSubType3D(double.MinValue, double.MaxValue, 0.01, 1, 1,1, false, false, false);

            this.FHost.CreateValueOutput("Centers", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutputCenter);
            this.FPinOutputCenter.SetSubType3D(double.MinValue, double.MaxValue, 0.01,0, 0, 0, false, false, false);

            this.FHost.CreateValueOutput("Width", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutputWidth);
            this.FPinOutputWidth.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueOutput("Height", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutputHeight);
            this.FPinOutputHeight.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueOutput("Depth", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutputDepth);
            this.FPinOutputDepth.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
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
            if (this.FPinInput.PinIsChanged ||
                this.FPinInElementCount.PinIsChanged ||
                this.FPinInBoundsMax.PinIsChanged ||
                this.FPinInBoundsMin.PinIsChanged)
            {

                double elemcnt;
                double xmin, xmax, ymin, ymax, zmin, zmax;

                this.FPinInElementCount.GetValue(0, out elemcnt);
                this.FPinInBoundsMin.GetValue3D(0, out xmin, out ymin, out zmin);
                this.FPinInBoundsMax.GetValue3D(0, out xmax, out ymax, out zmax);

                //Not sure of the order, but constructor swaps anyway

                Box defbounds = new Box(ymax, ymin, xmin, xmax, zmin, zmax);

                if (!defbounds.Zero)
                {
                    DefaultOctTree qt = new DefaultOctTree(Convert.ToInt32(elemcnt), defbounds);
                    for (int i = 0; i < this.FPinInput.SliceCount; i++)
                    {
                        double x, y, z;
                        this.FPinInput.GetValue3D(i, out x, out y, out z);
                        qt.Add(new Point3d(x, y, z));
                    }

                    List<Box> bounds = qt.GetAllBounds();
                    this.FPinOutputCenter.SliceCount = bounds.Count;
                    this.FPinOutputHeight.SliceCount = bounds.Count;
                    this.FPinOutputWidth.SliceCount = bounds.Count;
                    this.FPinOutputDepth.SliceCount = bounds.Count;

                    for (int i = 0; i < bounds.Count; i++)
                    {
                        Point3d center = bounds[i].Center;
                        this.FPinOutputCenter.SetValue3D(i, center.x, center.y, center.z);
                        this.FPinOutputWidth.SetValue(i, bounds[i].Width);
                        this.FPinOutputHeight.SetValue(i, bounds[i].Height);
                        this.FPinOutputDepth.SetValue(i, bounds[i].Depth);
                    }
                }
                else
                {
                    this.FPinOutputCenter.SliceCount = 0;
                    this.FPinOutputHeight.SliceCount = 0;
                    this.FPinOutputWidth.SliceCount = 0;
                }
            }
        }
        #endregion

        #region Auto evaluate
        public bool AutoEvaluate
        {
            get { return false; }
        }
        #endregion
    }
}

