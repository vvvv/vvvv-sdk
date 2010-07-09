using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using NTrees.Lib.Quad;

namespace VVVV.Nodes
{
    public class EdgeQuadTreeNode : IPlugin
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "QuadTree";
                Info.Category = "2d";
                Info.Version = "Edges";
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

        #region Edges output
        private IValueOut FPinOutputEdgeX1;
        private IValueOut FPinOutputEdgeX2;
        private IValueOut FPinOutputEdgeY1;
        private IValueOut FPinOutputEdgeY2;
        #endregion

        #region Set Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;

            this.FHost.CreateValueInput("Input", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInput);
            this.FPinInput.SetSubType2D(double.MinValue, double.MaxValue, 0.01, 0,0, false, false, false);

            this.FHost.CreateValueInput("Maximum Elements", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInElementCount);
            this.FPinInElementCount.SetSubType(1, double.MaxValue, 1, 1, false, false, true);

            this.FHost.CreateValueInput("Bounds Min", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInBoundsMin);
            this.FPinInBoundsMin.SetSubType2D(double.MinValue, double.MaxValue, 0.01, -1, -1, false, false, false);

            this.FHost.CreateValueInput("Bounds Max", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInBoundsMax);
            this.FPinInBoundsMax.SetSubType2D(double.MinValue, double.MaxValue, 0.01, 1, 1, false, false, false);

            this.FHost.CreateValueOutput("Edge X1", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutputEdgeX1);
            this.FPinOutputEdgeX1.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueOutput("Edge X2", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutputEdgeX2);
            this.FPinOutputEdgeX2.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueOutput("Edge Y1", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutputEdgeY1);
            this.FPinOutputEdgeY1.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueOutput("Edge Y2", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutputEdgeY2);
            this.FPinOutputEdgeY2.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);


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

                    DefaultQuadTree qt = new DefaultQuadTree(Convert.ToInt32(elemcnt), defbounds);
                    for (int i = 0; i < this.FPinInput.SliceCount; i++)
                    {
                        double x, y;
                        this.FPinInput.GetValue2D(i, out x, out y);
                        qt.Add(new Point2d(x, y));
                    }

                    List<Rect> bounds = qt.GetAllBounds();

                    int edgeindex = 0;
                    //Four edges per box output
                    this.FPinOutputEdgeX1.SliceCount = bounds.Count * 4;
                    this.FPinOutputEdgeX2.SliceCount = bounds.Count * 4;
                    this.FPinOutputEdgeY1.SliceCount = bounds.Count * 4;
                    this.FPinOutputEdgeY2.SliceCount = bounds.Count * 4;

                    for (int i = 0; i < bounds.Count; i++)
                    {
                        Edge2d[] edges = bounds[i].Edges;
                        foreach (Edge2d edge in edges)
                        {
                            this.FPinOutputEdgeX1.SetValue(edgeindex, edge.Point1.x);
                            this.FPinOutputEdgeX2.SetValue(edgeindex, edge.Point2.x);
                            this.FPinOutputEdgeY1.SetValue(edgeindex, edge.Point1.y);
                            this.FPinOutputEdgeY2.SetValue(edgeindex, edge.Point2.y);
                            edgeindex++;
                        }
                    }
                }
                else
                {
                    //Invalid bounds
                    this.FPinOutputEdgeX1.SliceCount = 0;
                    this.FPinOutputEdgeX2.SliceCount = 0;
                    this.FPinOutputEdgeY1.SliceCount = 0;
                    this.FPinOutputEdgeY2.SliceCount = 0;
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
