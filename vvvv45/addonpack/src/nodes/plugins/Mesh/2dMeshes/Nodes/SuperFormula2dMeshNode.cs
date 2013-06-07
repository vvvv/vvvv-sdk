using System;
using System.Collections.Generic;
using System.Text;
using VVVV.Lib;
using VVVV.PluginInterfaces.V1;
using SlimDX;

namespace VVVV.Nodes
{
    public class SuperFormula2dMeshNode : AbstractMeshNode, IPlugin, IDisposable, IPluginDXMesh
    {
        #region Fields
        private IValueIn FPinInM;
        private IValueIn FPinInN;
        private IValueIn FPinInInnerRadius;
        private IValueIn FPinInCycles;
        private IValueIn FPinInResolution;
        #endregion


        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "SuperFormula";							//use CamelCaps and no spaces
                Info.Category = "EX9.Geometry";						//try to use an existing one
                Info.Version = "2d";						//versions are optional. leave blank if not needed
                Info.Help = "Generates a 2d superformula mesh";
                Info.Bugs = "";
                Info.Credits = "";								//give credits to thirdparty code used
                Info.Warnings = "";
                Info.Author = "vux";
                Info.Tags = "superformula,geometry";

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

        protected override void SetInputs()
        {
            this.FHost.CreateValueInput("M", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInM);
            this.FPinInM.SetSubType(double.MinValue, double.MaxValue, 0.01, 2, false, false, false);
              
            this.FHost.CreateValueInput("N", 3, new string[] { "1", "2","3" } , TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInN);
            this.FPinInN.SetSubType3D(double.MinValue, double.MaxValue, 0.01, 2,2,2, false, false, false);

            this.FHost.CreateValueInput("Inner Radius", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInInnerRadius);
            this.FPinInInnerRadius.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueInput("Cycles", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInCycles);
            this.FPinInCycles.SetSubType(double.MinValue, double.MaxValue, 0.01, 1.0, false, false, false);
       
            this.FHost.CreateValueInput("Resolution", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInResolution);
            this.FPinInResolution.SetSubType(0, double.MaxValue, 1, 20, false, false, true);
         
        }

        public void Evaluate(int SpreadMax)
        {
            this.FInvalidate = false;

            if (this.FPinInCycles.PinIsChanged
                || this.FPinInInnerRadius.PinIsChanged
                || this.FPinInM.PinIsChanged
                || this.FPinInN.PinIsChanged
                || this.FPinInResolution.PinIsChanged)
            {
                this.FVertex.Clear();
                this.FIndices.Clear();
                for (int i = 0; i < SpreadMax; i++)
                {
                    double m, n1, n2, n3, dblres,inner,cycles;
                    
                    this.FPinInResolution.GetValue(i, out dblres);
                    this.FPinInInnerRadius.GetValue(i, out inner);
                    this.FPinInCycles.GetValue(i, out cycles);

                    int ires = Convert.ToInt32(dblres);

                    
                    this.FPinInM.GetValue(i, out m);
                    this.FPinInN.GetValue3D(i, out n1, out n2, out n3);

                    Vertex[] verts = new Vertex[ires * 2];
                    short[] indices = new short[(ires - 1) * 6];

                    double inc = ((Math.PI * 2.0 * cycles) / (ires - 1.0));
                    double phi = 0;

                    double maxx = double.MinValue;
                    double minx = double.MaxValue;
                    double maxy = double.MinValue;
                    double miny = double.MaxValue;

                    for (int j = 0; j < ires; j++)
                    {
                        double r = Math.Pow(Math.Abs(Math.Cos(m * phi / 4.0)), n2);
                        r += Math.Pow(Math.Abs(Math.Sin(m * phi / 4.0)), n3);
                        r = Math.Pow(r, 1.0 / n1);

                        float x = Convert.ToSingle(r * inner * 0.5 * Math.Cos(phi));
                        float y = Convert.ToSingle(r * inner * 0.5 * Math.Sin(phi));

                        Vertex innerv = new Vertex();
                        innerv.pv = new Vector3(x, y, 0.0f);
                        innerv.nv = new Vector3(0.0f, 0.0f, 1.0f);
                        innerv.tu1 = 0.5f - x;
                        innerv.tv1 = 0.5f - y;

                        if (x < minx) { minx = x; }
                        if (x > maxx) { maxx = x; }
                        if (y < miny) { miny = y; }
                        if (y > maxy) { maxy = y; }

                        x = Convert.ToSingle(r * 0.5 * Math.Cos(phi));
                        y = Convert.ToSingle(r * 0.5 * Math.Sin(phi));

                        Vertex outerv = new Vertex();
                        outerv.pv = new Vector3(x, y, 0.0f);
                        outerv.nv = new Vector3(0.0f, 0.0f, 1.0f);
                        outerv.tu1 = 0.5f - x;
                        outerv.tv1 = 0.5f - y;

                        verts[j] = innerv;
                        verts[j + ires] = outerv;

                        if (x < minx) { minx = x; }
                        if (x > maxx) { maxx = x; }
                        if (y < miny) { miny = y; }
                        if (y > maxy) { maxy = y; }
                        
                        phi += inc;
                    }

                    double w = maxx - minx;
                    double h = maxy - miny;
                    for (int j = 0; j < verts.Length; j++)
                    {
                        verts[j].tu1 = Convert.ToSingle((verts[j].pv.X - minx) / w);
                        verts[j].tv1 = 1.0f - Convert.ToSingle((verts[j].pv.Y - miny) / h);
                    }

                    int indstep = 0;
                    for (int j = 0; j < ires - 1; j++)
                    {
                        //Triangle from low to high
                        indices[indstep] = Convert.ToInt16(j);
                        indices[indstep + 1] = Convert.ToInt16(j + 1);
                        indices[indstep + 2] = Convert.ToInt16(ires + j);

                        //Triangle from high to low
                        indices[indstep + 3] = Convert.ToInt16(j + 1);
                        indices[indstep + 4] = Convert.ToInt16(ires + j);
                        indices[indstep + 5] = Convert.ToInt16(ires + j + 1);

                        indstep += 6;
                    }

                    this.FVertex.Add(verts);
                    this.FIndices.Add(indices);
                }

                this.InvalidateMesh(this.FVertex.Count);
            }
        }
    }
}
