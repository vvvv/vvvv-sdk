using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using BenTools.Mathematics;

namespace vvvv.Nodes
{
    public class VoronoiNode : IPlugin
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "Voronoi";
                Info.Category = "2d";
                Info.Version = "";
                Info.Help = "Computes voronoi algorithm on a set of points";
                Info.Bugs = "";
                Info.Credits = "BenDi http://www.codeproject.com/KB/recipes/fortunevoronoi.aspx";
                Info.Warnings = "";
                Info.Author = "vux";
                Info.Tags = "Space,Subdivision";
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

        private IValueIn FPinInX;
        private IValueIn FPinInY;

        private IValueOut FPinOutVertX;
        private IValueOut FPinOutVertY;

        private IValueOut FPinOutEdgesX1;
        private IValueOut FPinOutEdgesY1;
        private IValueOut FPinOutEdgesX2;
        private IValueOut FPinOutEdgesY2;

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;

            //Input
            this.FHost.CreateValueInput("X",1,null,TSliceMode.Dynamic,TPinVisibility.True, out this.FPinInX);
            this.FPinInX.SetSubType(double.MinValue,double.MaxValue, 0.01,0,false,false,false);

            this.FHost.CreateValueInput("Y",1,null,TSliceMode.Dynamic,TPinVisibility.True, out this.FPinInY);
            this.FPinInY.SetSubType(double.MinValue,double.MaxValue, 0.01,0,false,false,false);

            //Outputs
            this.FHost.CreateValueOutput("Vertices X", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutVertX);
            this.FPinOutVertX.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueOutput("Vertices Y", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutVertY);
            this.FPinOutVertY.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueOutput("Edges X1", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutEdgesX1);
            this.FPinOutEdgesX1.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueOutput("Edges Y1", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutEdgesY1);
            this.FPinOutEdgesY1.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueOutput("Edges X2", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutEdgesX2);
            this.FPinOutEdgesX2.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueOutput("Edges Y2", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutEdgesY2);
            this.FPinOutEdgesY2.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
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
            if (this.FPinInY.PinIsChanged || this.FPinInX.PinIsChanged)
            {
                IList<Vector> vectors = new List<Vector>();

                #region Initialize List
                int ix = 0;
                int iy = 0;
                for (int i = 0; i < SpreadMax; i++)
                {
                    double x, y;
                    this.FPinInX.GetValue(ix, out x);
                    this.FPinInY.GetValue(iy, out y);

                    vectors.Add(new Vector(x, y));

                    ix++;
                    iy++;

                    if (ix >= this.FPinInX.SliceCount)
                    {
                        ix = 0;
                    }

                    if (iy >= this.FPinInY.SliceCount)
                    {
                        iy = 0;
                    }
                }
                #endregion

                VoronoiGraph graph = Fortune.ComputeVoronoiGraph(vectors);

                //Outputs vertices
                this.FPinOutVertX.SliceCount = graph.Vertizes.Count;
                this.FPinOutVertY.SliceCount = graph.Vertizes.Count;

                for (int i = 0; i < graph.Vertizes.Count; i++)
                {
                    this.FPinOutVertX.SetValue(i,graph.Vertizes[i][0]);
                    this.FPinOutVertY.SetValue(i,graph.Vertizes[i][1]);
                }


                //Outputs the edges
                this.FPinOutEdgesX1.SliceCount = graph.Edges.Count;
                this.FPinOutEdgesY1.SliceCount = graph.Edges.Count;
                this.FPinOutEdgesX2.SliceCount = graph.Edges.Count;
                this.FPinOutEdgesY2.SliceCount = graph.Edges.Count;

                for (int i = 0; i < graph.Edges.Count; i++)
                {
                    this.FPinOutEdgesX1.SetValue(i, graph.Edges[i].VVertexA[0]);
                    this.FPinOutEdgesY1.SetValue(i, graph.Edges[i].VVertexA[1]);
                    this.FPinOutEdgesX2.SetValue(i, graph.Edges[i].VVertexB[0]);
                    this.FPinOutEdgesY2.SetValue(i, graph.Edges[i].VVertexB[1]);
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
    }
}
