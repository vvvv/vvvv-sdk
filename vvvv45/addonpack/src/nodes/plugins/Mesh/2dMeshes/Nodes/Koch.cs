using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using SlimDX.Direct3D9;
using SlimDX;
using System.Runtime.InteropServices;
using VVVV.Lib;

namespace VVVV.Nodes
{
    
    public class Koch : AbstractMeshNode, IPlugin, IDisposable, IPluginDXMesh
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "Koch";							//use CamelCaps and no spaces
                Info.Category = "EX9.Geometry";						//try to use an existing one
                Info.Version = "2d";						//versions are optional. leave blank if not needed
                Info.Help = "Generalized Koch Curve";
                Info.Bugs = "";
                Info.Credits = "";								//give credits to thirdparty code used
                Info.Warnings = "";
                Info.Author = "fibo";
                Info.Tags = "geometry";

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

        #region Fields

        private IValueIn FPinInNumIterations;

        private IValueIn FPinInVertex1;
        private IValueIn FPinInVertex2;
        private IValueIn FPinInVertex3;
        private IValueIn FPinInVertex4;
        private IValueIn FPinInVertex5;
        
        #endregion

        #region Set Plugin Host
        protected override void SetInputs()
        {
            this.FHost.CreateValueInput("Iterations ", 1, new string[] { "N" }, TSliceMode.Single, TPinVisibility.True, out FPinInNumIterations);
            this.FPinInNumIterations.SetSubType(1, double.MaxValue, 1, 1, false, false, true);

            this.FHost.CreateValueInput("Vertex 1 ", 2, new string[] { "X", "Y" }, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInVertex1);
            this.FPinInVertex1.SetSubType2D(double.MinValue, double.MaxValue, 0.01, 0, 0, false, false, false);
            this.FHost.CreateValueInput("Vertex 2 ", 2, new string[] { "X", "Y" }, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInVertex2);
            this.FPinInVertex2.SetSubType2D(double.MinValue, double.MaxValue, 0.01, .3333, 0, false, false, false);
            this.FHost.CreateValueInput("Vertex 3 ", 2, new string[] { "X", "Y" }, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInVertex3);
            this.FPinInVertex3.SetSubType2D(double.MinValue, double.MaxValue, 0.01, .5, Math.Sqrt(3)/4, false, false, false);
            this.FHost.CreateValueInput("Vertex 4 ", 2, new string[] { "X", "Y" }, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInVertex4);
            this.FPinInVertex4.SetSubType2D(double.MinValue, double.MaxValue, 0.01, .6666, 0, false, false, false);
            this.FHost.CreateValueInput("Vertex 5 ", 2, new string[] { "X", "Y" }, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInVertex5);
            this.FPinInVertex5.SetSubType2D(double.MinValue, double.MaxValue, 0.01, 1, 0, false, false, false);

        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            if (FPinInNumIterations.PinIsChanged||FPinInVertex1.PinIsChanged || FPinInVertex2.PinIsChanged || FPinInVertex3.PinIsChanged || FPinInVertex4.PinIsChanged || FPinInVertex5.PinIsChanged)
            {
                this.FVertex.Clear();
                this.FIndices.Clear();

                int index=3; //starts from 3, then incremented each iteration
                double v1x, v1y,v2x, v2y,v3x, v3y,v4x, v4y,v5x,v5y;
                double numIterations;
                Vertex[] previousVerts = null;

                FPinInNumIterations.GetValue(0, out numIterations);
                FPinInVertex1.GetValue2D(0, out v1x, out v1y);
                FPinInVertex2.GetValue2D(0, out v2x, out v2y);
                FPinInVertex3.GetValue2D(0, out v3x, out v3y);
                FPinInVertex4.GetValue2D(0, out v4x, out v4y);
                FPinInVertex5.GetValue2D(0, out v5x, out v5y);

                for (int i = 1; i <= numIterations; i++)
                {
                    if (i == 1)
                    {
                        Vertex[] verts = new Vertex[3];
                        
                        verts[0].pv = new Vector3(Convert.ToSingle(v2x), Convert.ToSingle(v2y), 0);
                        verts[0].nv = new Vector3(0, 0, 1);
                        verts[0].tu1 = 0.0f;
                        verts[0].tv1 = 0.0f;
                        
                        verts[1].pv = new Vector3(Convert.ToSingle(v3x), Convert.ToSingle(v3y), 0);
                        verts[1].nv = new Vector3(0, 0, 1);
                        verts[1].tu1 = 0.0f;
                        verts[1].tv1 = 0.0f;
                        
                        verts[2].pv = new Vector3(Convert.ToSingle(v4x), Convert.ToSingle(v4y), 0);
                        verts[2].nv = new Vector3(0, 0, 1);
                        verts[2].tu1 = 0.0f;
                        verts[2].tv1 = 0.0f;

                        this.FVertex.Add(verts);

                        previousVerts = new Vertex[3];
                        verts.CopyTo(previousVerts,0);

                        List<short> inds = new List<short>();

                        inds.Add(0);
                        inds.Add(1);
                        inds.Add(2);

                        this.FIndices.Add(inds.ToArray());
                    }
                    else {
                        int numOfVertices = 3*(int)Math.Pow(4, (i - 1));
                        Vertex[] verts = new Vertex[numOfVertices];

                        double x, y;

                        //transformation 1
                        for (int j = 0; j < previousVerts.Length; j++)
                        {
                            Vertex v = (Vertex)previousVerts.GetValue(j);

                            //translate to the origin
                            x = v.pv.X - v1x;
                            y = v.pv.Y - v1y;
                            //scale
                            x /= Math.Sqrt((v5x - v1x) * (v5x - v1x) + (v5y - v1y) * (v5y - v1y));
                            y /= Math.Sqrt((v5x - v1x) * (v5x - v1x) + (v5y - v1y) * (v5y - v1y));
                            x *= Math.Sqrt((v2x - v1x) * (v2x - v1x) + (v2y - v1y) * (v2y - v1y));
                            y *= Math.Sqrt((v2x - v1x) * (v2x - v1x) + (v2y - v1y) * (v2y - v1y));
                            //rotate
                            //...
                            //translate back
                            x += v1x;
                            y += v1y;

                            verts[j].pv = new Vector3(Convert.ToSingle(x), Convert.ToSingle(y), 0);
                            verts[j].nv = new Vector3(0, 0, 1);
                            verts[j].tu1 = 0.0f;
                            verts[j].tv1 = 0.0f;
                        }

                        //transformation 2
                        for (int j = 0; j < previousVerts.Length; j++)
                        {
                            Vertex v = (Vertex)previousVerts.GetValue(j);

                            //translate to the origin
                            x = v.pv.X - v1x;
                            y = v.pv.Y - v1y;
                            //scale
                            x /= Math.Sqrt((v5x - v1x) * (v5x - v1x) + (v5y - v1y) * (v5y - v1y));
                            y /= Math.Sqrt((v5x - v1x) * (v5x - v1x) + (v5y - v1y) * (v5y - v1y));
                            x *= Math.Sqrt((v3x - v2x) * (v3x - v2x) + (v3y - v2y) * (v3y - v2y));
                            y *= Math.Sqrt((v3x - v2x) * (v3x - v2x) + (v3y - v2y) * (v3y - v2y));
                            //rotate
                            //...
                            //translate back
                            x += v2x;
                            y += v2y;

                            verts[previousVerts.Length + j].pv = new Vector3(Convert.ToSingle(x), Convert.ToSingle(y), 0);
                            verts[previousVerts.Length + j].nv = new Vector3(0, 0, 1);
                            verts[previousVerts.Length + j].tu1 = 0.0f;
                            verts[previousVerts.Length + j].tv1 = 0.0f;
                        }

                        //tranformation 3
                        for (int j = 0; j < previousVerts.Length; j++)
                        {
                            Vertex v = (Vertex)previousVerts.GetValue(j);

                            //translate to the origin
                            x = v.pv.X - v1x;
                            y = v.pv.Y - v1y;
                            //scale
                            x /= Math.Sqrt((v5x - v1x) * (v5x - v1x) + (v5y - v1y) * (v5y - v1y));
                            y /= Math.Sqrt((v5x - v1x) * (v5x - v1x) + (v5y - v1y) * (v5y - v1y));
                            x *= Math.Sqrt((v4x - v3x) * (v4x - v3x) + (v4y - v3y) * (v4y - v3y));
                            y *= Math.Sqrt((v4x - v3x) * (v4x - v3x) + (v4y - v3y) * (v4y - v3y));
                            //rotate
                            //...
                            //translate back
                            x += v3x;
                            y += v3y;

                            verts[2 * previousVerts.Length + j].pv = new Vector3(Convert.ToSingle(x), Convert.ToSingle(y), 0);
                            verts[2 * previousVerts.Length + j].nv = new Vector3(0, 0, 1);
                            verts[2 * previousVerts.Length + j].tu1 = 0.0f;
                            verts[2 * previousVerts.Length + j].tv1 = 0.0f;
                        }

                        //transformation 4
                        for (int j = 0; j < previousVerts.Length; j++)
                        {
                            Vertex v = (Vertex)previousVerts.GetValue(j);

                            //translate to the origin
                            x = v.pv.X - v1x;
                            y = v.pv.Y - v1y;
                            //scale
                            x /= Math.Sqrt((v5x - v1x) * (v5x - v1x) + (v5y - v1y) * (v5y - v1y));
                            y /= Math.Sqrt((v5x - v1x) * (v5x - v1x) + (v5y - v1y) * (v5y - v1y));
                            x *= Math.Sqrt((v5x - v4x) * (v5x - v4x) + (v5y - v4y) * (v5y - v4y));
                            y *= Math.Sqrt((v5x - v4x) * (v5x - v4x) + (v5y - v4y) * (v5y - v4y));
                            //rotate
                            //...
                            //translate back
                            x += v4x;
                            y += v4y;

                            verts[3 * previousVerts.Length + j].pv = new Vector3(Convert.ToSingle(x), Convert.ToSingle(y), 0);
                            verts[3 * previousVerts.Length + j].nv = new Vector3(0, 0, 1);
                            verts[3 * previousVerts.Length + j].tu1 = 0.0f;
                            verts[3 * previousVerts.Length + j].tv1 = 0.0f;
                        }

                        this.FVertex.Add(verts);

                        previousVerts = new Vertex[numOfVertices];
                        verts.CopyTo(previousVerts, 0);

                        List<short> inds = new List<short>();
                        for (int j = index; j < numOfVertices; j++) {
                            inds.Add(Convert.ToInt16(index));
                            index++;
                        }
                        this.FIndices.Add(inds.ToArray());
                    }
                }
                this.InvalidateMesh(this.FVertex.Count);
            }
        }
        #endregion

    }
        
        
}
