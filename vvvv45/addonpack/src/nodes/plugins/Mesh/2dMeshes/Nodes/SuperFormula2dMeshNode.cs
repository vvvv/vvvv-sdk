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
        private IValueIn FPinInAB;
        private IValueIn FPinInM;
        private IValueIn FPinInN;
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
            
            this.FHost.CreateValueInput("Size", 2, new string[] { "A", "B" } , TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInAB);
            this.FPinInAB.SetSubType2D(double.MinValue, double.MaxValue, 0.01, 1, 1, false, false, false);
     
            this.FHost.CreateValueInput("M", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInM);
            this.FPinInM.SetSubType(double.MinValue, double.MaxValue, 0.01, 2, false, false, false);
              
            this.FHost.CreateValueInput("N", 3, new string[] { "1", "2","3" } , TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInN);
            this.FPinInN.SetSubType3D(double.MinValue, double.MaxValue, 0.01, 2,2,2, false, false, false);
       
            this.FHost.CreateValueInput("Resolution", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInResolution);
            this.FPinInResolution.SetSubType(0, double.MaxValue, 1, 20, false, false, true);
         
        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FPinInAB.PinIsChanged
                || this.FPinInM.PinIsChanged
                || this.FPinInN.PinIsChanged
                || this.FPinInResolution.PinIsChanged)
            {
                this.FVertex.Clear();
                this.FIndices.Clear();
                for (int i = 0; i < SpreadMax; i++)
                {
                    double a, b, m, n1, n2, n3, dblres;
                    
                    this.FPinInResolution.GetValue(i, out dblres);
                    int ires = Convert.ToInt32(dblres);

                    this.FPinInAB.GetValue2D(i, out a, out b);
                    this.FPinInM.GetValue(i, out m);
                    this.FPinInN.GetValue3D(i, out n1, out n2, out n3);

                    Vertex[] vertices = new Vertex[ires + 1];

                    vertices[0].pv = new Vector3(0, 0, 0);
                    vertices[0].nv = new Vector3(0, 0, 1);
                    vertices[0].tu1 = 0.5f;
                    vertices[0].tv1 = 0.5f;


                    double inc = ((Math.PI * 2.0) / ((double)ires));
                    double phi = 0;

                    double maxx = double.MinValue;
                    double minx = double.MaxValue;
                    double maxy = double.MinValue;
                    double miny = double.MaxValue;

                    for (int j = 0; j < ires; j++)
                    {
                        double r = Math.Pow(Math.Abs(Math.Cos(m * phi / 4.0) / a), n2);
                        r += Math.Pow(Math.Abs(Math.Sin(m * phi / 4.0) / b), n3);
                        r = Math.Pow(r, 1.0 / n1);

                        float x = Convert.ToSingle(r * Math.Cos(phi));
                        float y = Convert.ToSingle(r * Math.Sin(phi));


                        vertices[j + 1].pv = new Vector3(x, y, 0);
                        vertices[j + 1].nv = new Vector3(0, 0, 1);

                        if (x < minx) { minx = x; }
                        if (x > maxx) { maxx = x; }
                        if (y < miny) { miny = y; }
                        if (y > maxy) { maxy = y; }
                        
                        phi += inc;
                    }

                    double w = maxx - minx;
                    double h = maxy - miny;
                    for (int j = 0; j < ires; j++)
                    {
                        vertices[j + 1].tu1 = Convert.ToSingle((vertices[j + 1].pv.X - minx) / w);
                        vertices[j + 1].tv1 = 1.0f - Convert.ToSingle((vertices[j + 1].pv.Y - miny) / h);
                    }


                    this.FVertex.Add(vertices);

                    List<short> inds = new List<short>();

                    for (int j = 0; j < ires - 1; j++)
                    {
                        inds.Add(0);
                        inds.Add(Convert.ToInt16(j + 1));
                        inds.Add(Convert.ToInt16(j + 2));
                    }

                    inds.Add(0);
                    inds.Add(Convert.ToInt16(vertices.Length - 1));
                    inds.Add(1);

                    this.FIndices.Add(inds.ToArray());
                }

                this.InvalidateMesh(this.FVertex.Count);
            }
        }
    }
}
