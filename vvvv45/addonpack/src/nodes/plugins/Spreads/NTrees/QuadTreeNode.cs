using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using NTrees.Lib.Quad;

namespace VVVV.Nodes
{
    public class QuadTreeNode : IPlugin
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "QuadTree";
                Info.Category = "2d";
                Info.Version = "";
                Info.Help = "Get Quad tree subdivision for a set of points";
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

        private IValueIn FPinInput;
        private IValueIn FPinInElementCount;
        private IValueIn FPinInBoundsMin;
        private IValueIn FPinInBoundsMax;

        #region Center /Width Height output
        private IValueOut FPinOutputCenter;
        private IValueOut FPinOutputWidth;
        private IValueOut FPinOutputHeight;
        #endregion

        #region Selected Items outputs
        private IValueOut FPinOutputCenterSelected;
        private IValueOut FPinOutputWidthSelected;
        private IValueOut FPinOutputHeightSelected;
        #endregion

        #region Set Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;

            this.FHost.CreateValueInput("Input", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInput);
            this.FPinInput.SetSubType2D(double.MinValue, double.MaxValue, 0.01, 0, 0, false, false, false);

            this.FHost.CreateValueInput("Maximum Elements", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInElementCount);
            this.FPinInElementCount.SetSubType(1, double.MaxValue, 1, 1, false, false, true);

            this.FHost.CreateValueInput("Bounds Min", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInBoundsMin);
            this.FPinInBoundsMin.SetSubType2D(double.MinValue, double.MaxValue, 0.01, -1, -1, false, false, false);

            this.FHost.CreateValueInput("Bounds Max", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInBoundsMax);
            this.FPinInBoundsMax.SetSubType2D(double.MinValue, double.MaxValue, 0.01, 1, 1, false, false, false);

            this.FHost.CreateValueOutput("Centers", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutputCenter);
            this.FPinOutputCenter.SetSubType2D(double.MinValue, double.MaxValue, 0.01, 0, 0, false, false, false);

            this.FHost.CreateValueOutput("Width", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutputWidth);
            this.FPinOutputWidth.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueOutput("Height", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutputHeight);
            this.FPinOutputHeight.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueOutput("Centers Points", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutputCenterSelected);
            this.FPinOutputCenterSelected.SetSubType2D(double.MinValue, double.MaxValue, 0.01, 0, 0, false, false, false);

            this.FHost.CreateValueOutput("Width Points", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutputWidthSelected);
            this.FPinOutputWidthSelected.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueOutput("Height Points", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutputHeightSelected);
            this.FPinOutputHeightSelected.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

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
                double xmin, xmax, ymin, ymax;
                this.FPinInElementCount.GetValue(0, out elemcnt);
                this.FPinInBoundsMin.GetValue2D(0, out xmin, out ymin);
                this.FPinInBoundsMax.GetValue2D(0, out xmax, out ymax);

                Rect defbounds = new Rect(ymax, ymin, xmin, xmax);

                if (!defbounds.Zero)
                {
                    List<Point2d> points = new List<Point2d>();
                    DefaultQuadTree qt = new DefaultQuadTree(Convert.ToInt32(elemcnt), defbounds);
                    for (int i = 0; i < this.FPinInput.SliceCount; i++)
                    {
                        double x, y;
                        this.FPinInput.GetValue2D(i, out x, out y);
                        Point2d point = new Point2d(x, y);

                        qt.Add(point);
                        points.Add(point);
                    }

                    List<Rect> bounds = qt.GetAllBounds();
                    this.FPinOutputCenter.SliceCount = bounds.Count;
                    this.FPinOutputHeight.SliceCount = bounds.Count;
                    this.FPinOutputWidth.SliceCount = bounds.Count;

                    for (int i = 0; i < bounds.Count; i++)
                    {
                        Point2d center = bounds[i].Center;
                        this.FPinOutputCenter.SetValue2D(i, center.x, center.y);
                        this.FPinOutputWidth.SetValue(i, bounds[i].Width);
                        this.FPinOutputHeight.SetValue(i, bounds[i].Height);
                    }

                    this.FPinOutputCenterSelected.SliceCount = points.Count;
                    this.FPinOutputHeightSelected.SliceCount = points.Count;
                    this.FPinOutputWidthSelected.SliceCount = points.Count;

                    for (int i = 0; i < points.Count; i++)
                    {
                        Rect bound = qt.FindNodeBounds(points[i]);
                        this.FPinOutputCenterSelected.SetValue2D(i, bound.Center.x, bound.Center.y);
                        this.FPinOutputWidthSelected.SetValue(i, bound.Width);
                        this.FPinOutputHeightSelected.SetValue(i, bound.Height);
                    }
                }
                else
                {
                    //Invalid Bounds, zero
                    this.FPinOutputCenter.SliceCount = 0;
                    this.FPinOutputHeight.SliceCount =0;
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
