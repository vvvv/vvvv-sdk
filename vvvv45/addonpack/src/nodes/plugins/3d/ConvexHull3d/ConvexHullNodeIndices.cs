using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using ConvexHull3d.QuickHull;

namespace vvvv.Nodes
{
    public class ConvexHullNodeIndices : IPlugin
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "ConvexHull";
                Info.Category = "3d";
                Info.Version = "";
                Info.Help = "Convex Hull 3d (QuickHull algorithm)";
                Info.Bugs = "";
                Info.Credits = "John E. Lloyd : http://www.cs.ubc.ca/~lloyd/index.html";
                Info.Warnings = "";
                Info.Author = "vux";
                Info.Tags = "Geometry,Triangle";

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

        private IValueIn FPinInVector;

        private IValueOut FPinOutIndices;
        private IValueOut FPinOutVertices;


        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;

            //Input
            this.FHost.CreateValueInput("Input", 3, null , TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInVector);
            this.FPinInVector.SetSubType3D(double.MinValue, double.MaxValue, 0.01, 0, 0, 0, false, false, false);

            //Outputs
            this.FHost.CreateValueOutput("Vertices", 3, null , TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutVertices);
            this.FPinOutVertices.SetSubType3D(double.MinValue, double.MaxValue, 0.01, 0, 0, 0, false, false, false);

            this.FHost.CreateValueOutput("Indices", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutIndices);
            this.FPinOutIndices.SetSubType(0, double.MaxValue, 1, 0, false, false, true);
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
            if (this.FPinInVector.PinIsChanged)
            {
                List<Point3d> points = new List<Point3d>();

                #region Initialize List

                for (int i = 0; i < this.FPinInVector.SliceCount; i++)
                {
                    double x, y,z ;
                    this.FPinInVector.GetValue3D(i, out x, out y, out z);
                    points.Add(new Point3d(x, y,z));

                }
                #endregion

                #region Compute Hull
                QuickHull3D q = new QuickHull3D();
                q.build(points.ToArray());
                Point3d[] vertices = q.getVertices();
                int[][] faces = q.getFaces();
                #endregion

                #region Output
                this.FPinOutVertices.SliceCount = vertices.Length;
                for (int i = 0; i < vertices.Length; i++)
                {
                    this.FPinOutVertices.SetValue3D(i, vertices[i].x, vertices[i].y, vertices[i].z);
                }

                
                this.FPinOutIndices.SliceCount = faces.Length * 3;
                int idx = 0;
                for (int i = 0; i < faces.Length; i++)
                {
                    this.FPinOutIndices.SetValue(idx, faces[i][0]);
                    this.FPinOutIndices.SetValue(idx + 1, faces[i][1]);
                    this.FPinOutIndices.SetValue(idx + 2, faces[i][2]);
                    idx += 3;
                }
                #endregion
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
