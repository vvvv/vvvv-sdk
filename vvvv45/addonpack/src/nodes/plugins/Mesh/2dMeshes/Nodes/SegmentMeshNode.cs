using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Lib;
using SlimDX;

namespace VVVV.Nodes
{
    public class SegmentMeshNode : AbstractMeshNode, IPlugin, IDisposable, IPluginDXMesh
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "Segment";							//use CamelCaps and no spaces
                Info.Category = "EX9.Geometry";						//try to use an existing one
                Info.Version = "2d";						//versions are optional. leave blank if not needed
                Info.Help = "Orthogonal to Segment (DX9)";
                Info.Tags = "circle, disk";
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
        private IValueIn FPinInPhase;
        private IValueIn FPinInInnerRadius;
        private IValueIn FPinInCycles;
        private IValueIn FPinInResolution;
        private IValueIn FPinInFlatTexture;
        #endregion

        #region Set Plugin Host
        protected override void SetInputs()
        {         
            this.FHost.CreateValueInput("Phase", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInPhase);
            this.FPinInPhase.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
        
            this.FHost.CreateValueInput("Inner Radius", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInInnerRadius);
            this.FPinInInnerRadius.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueInput("Cycles", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInCycles);
            this.FPinInCycles.SetSubType(double.MinValue, double.MaxValue, 0.01, 1.0, false, false, false);
    
            this.FHost.CreateValueInput("Flat Texture", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInFlatTexture);
            this.FPinInFlatTexture.SetSubType(0, 1, 1, 1, false, true, false);
               
            this.FHost.CreateValueInput("Resolution", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInResolution);
            this.FPinInResolution.SetSubType(2, double.MaxValue, 1, 20, false, false, true);
        
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            this.FInvalidate = false;

            if (this.FPinInInnerRadius.PinIsChanged 
                || this.FPinInResolution.PinIsChanged 
                || this.FPinInCycles.PinIsChanged
                || this.FPinInPhase.PinIsChanged
                || this.FPinInFlatTexture.PinIsChanged)
            {
                this.FVertex.Clear();
                this.FIndices.Clear();

                double inner, res,cycles,phase,flat;
                this.FPinInFlatTexture.GetValue(0, out flat);

                for (int j = 0; j < SpreadMax; j++)
                {
                    this.FPinInInnerRadius.GetValue(j, out inner);
                    this.FPinInResolution.GetValue(j, out res);
                    this.FPinInCycles.GetValue(j, out cycles);
                    this.FPinInPhase.GetValue(j, out phase);

                    int ires = Convert.ToInt32(res);

                    Vertex[] verts = new Vertex[ires * 2];
                    short[] indices = new short[(ires - 1) * 6];

                    double inc = ((Math.PI * 2.0 * cycles) / (ires -1.0));
                    double phi = phase * (Math.PI * 2.0) ;

                    for (int i = 0; i < ires; i++)
                    {
                        float x = Convert.ToSingle(0.5 * inner * Math.Cos(phi));
                        float y = Convert.ToSingle(0.5 * inner * Math.Sin(phi));

                        Vertex innerv = new Vertex();
                        innerv.pv = new Vector3(x,y, 0.0f);
                        innerv.nv = new Vector3(0.0f, 0.0f, 1.0f);

                        if (flat > 0.5)
                        {
                            innerv.tu1 = 0.5f - x;
                            innerv.tv1 = 0.5f - y;
                        }
                        else
                        {
                            innerv.tu1 = (1.0f * (float)i) / ((float)ires - 1.0f);
                            innerv.tv1 = 0.0f;
                        }

                        verts[i] = innerv;

                        x = Convert.ToSingle(0.5 * Math.Cos(phi));
                        y = Convert.ToSingle(0.5 * Math.Sin(phi));

                        Vertex outerv = new Vertex();
                        outerv.pv = new Vector3(x,y, 0.0f);
                        outerv.nv = new Vector3(0.0f, 0.0f, 1.0f);
                        if (flat > 0.5)
                        {
                            outerv.tu1 = 0.5f - x;
                            outerv.tv1 = 0.5f - y;
                        }
                        else
                        {
                            outerv.tu1 = (1.0f * (float)i) / ((float)ires -1.0f);
                            outerv.tv1 = 1.0f;
                        }


                        verts[i] = innerv;
                        verts[i + ires] = outerv;


                        phi += inc;
                    }

                    int indstep = 0;
                    for (int i = 0; i < ires - 1; i++)
                    {
                        //Triangle from low to high
                        indices[indstep] = Convert.ToInt16(i);
                        indices[indstep + 1] = Convert.ToInt16(i + 1);
                        indices[indstep + 2] = Convert.ToInt16(ires + i);

                        //Triangle from high to low
                        indices[indstep + 3] = Convert.ToInt16(i + 1);
                        indices[indstep + 4] = Convert.ToInt16(ires + i);
                        indices[indstep + 5] = Convert.ToInt16(ires + i + 1);

                        indstep += 6;
                    }

                    this.FIndices.Add(indices);
                    this.FVertex.Add(verts);
                }

                this.InvalidateMesh(this.FVertex.Count);
            }
        }
        #endregion
    }
}
