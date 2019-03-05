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
    
    public class Polygon2dNode : AbstractMeshNode, IPlugin, IDisposable, IPluginDXMesh
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "Polygon";							//use CamelCaps and no spaces
                Info.Category = "EX9.Geometry";						//try to use an existing one
                Info.Version = "2d";						//versions are optional. leave blank if not needed
                Info.Help = "Creates a polygon from a set of vertices";
                Info.Bugs = "";
                Info.Credits = "";								//give credits to thirdparty code used
                Info.Warnings = "";
                Info.Author = "vux";
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
        private IValueIn FPinInVertices;
        private IValueIn FPinInVerticesCount;
        #endregion

        #region Set Plugin Host
        protected override void SetInputs()
        {
            this.FHost.CreateValueInput("Vertices", 2, null , TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInVertices);
            this.FPinInVertices.SetSubType2D(double.MinValue, double.MaxValue, 0.01, 0, 0, false, false, false);

            this.FHost.CreateValueInput("Vertex Count", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInVerticesCount);
            this.FPinInVerticesCount.SetSubType(3, double.MaxValue, 1, 3, false, false, true);
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            this.FInvalidate = false;

            if (this.FPinInVertices.PinIsChanged || this.FPinInVerticesCount.PinIsChanged)
            {
                this.FVertex.Clear();
                this.FIndices.Clear();
                int cnt = 0;
                for (int i = 0; i < this.FPinInVerticesCount.SliceCount; i++)
                {
                    double dblcount;
                    this.FPinInVerticesCount.GetValue(i, out dblcount);

                    if (dblcount >= 3)
                    {
                        double cx = 0;
                        double cy = 0;
                        double x, y;

                        double minx = double.MaxValue, miny = double.MaxValue;
                        double maxx = double.MinValue, maxy = double.MinValue;

                        Vertex[] verts = new Vertex[Convert.ToInt32(dblcount) + 1];

                        for (int j = 0; j < dblcount; j++)
                        {
                            this.FPinInVertices.GetValue2D(cnt,out x,out y);
                            verts[j + 1].pv = new Vector3(Convert.ToSingle(x), Convert.ToSingle(y), 0);
                            verts[j + 1].nv = new Vector3(0, 0, 1);
                            verts[j+1].tu1 = 0.0f;
                            verts[j+1].tv1 = 0.0f;

                            cx += x;
                            cy += y;

                            if (x < minx) { minx = x; }
                            if (x > maxx) { maxx = x; }
                            if (y < miny) { miny = y; }
                            if (y > maxy) { maxy = y; }

                            cnt++;
                        }

                        verts[0].pv = new Vector3(Convert.ToSingle(cx/dblcount),Convert.ToSingle(cy/dblcount),0);
                        verts[0].nv = new Vector3(0, 0, 1);


                        double w = maxx - minx;
                        double h = maxy - miny;
                        for (int j = 0; j <= dblcount; j++)
                        {
                            verts[j].tu1 = Convert.ToSingle((verts[j].pv.X - minx) / w);
                            verts[j].tv1 = 1.0f - Convert.ToSingle((verts[j].pv.Y - miny) / h);
                        }

                        this.FVertex.Add(verts);

                        List<short> inds = new List<short>();

                        for (int j = 0; j < dblcount - 1; j++)
                        {
                            inds.Add(0);
                            inds.Add(Convert.ToInt16(j + 1));
                            inds.Add(Convert.ToInt16(j + 2));
                        }

                        inds.Add(0);
                        inds.Add(Convert.ToInt16(verts.Length - 1));
                        inds.Add(1);

                        this.FIndices.Add(inds.ToArray());
                    }
                }
                this.InvalidateMesh(this.FVertex.Count);
            }
        }
        #endregion

    }
        
        
}
