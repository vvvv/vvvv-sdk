using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Lib;
using SlimDX;

namespace VVVV.Nodes
{

    public unsafe class HeightFieldNode : AbstractMeshNode, IPlugin, IDisposable, IPluginDXMesh
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "HeightField";							//use CamelCaps and no spaces
                Info.Category = "EX9.Geometry";						//try to use an existing one
                Info.Version = "2d";						//versions are optional. leave blank if not needed
                Info.Help = "Creates a mesh from a 1d heightfield";
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
        private IValueIn FPinInWidth;
        private IValueIn FPinInLowOffset;
        private IEnumIn FPinInTexMode;
        #endregion

        #region Set Plugin Host
        protected override void SetInputs()
        {
            this.FHost.UpdateEnum("2d Mesh Texture Mapping", "Stretch", Enum.GetNames(typeof(e2dMeshTextureMapping)));

            this.FHost.CreateValueInput("Input", 1, null , TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInVertices);
            this.FPinInVertices.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
       
            this.FHost.CreateValueInput("Width", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInWidth);
            this.FPinInWidth.SetSubType(double.MinValue, double.MaxValue, 0.01, 1.0, false, false, false);
      
            this.FHost.CreateValueInput("Low Offset", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInLowOffset);
            this.FPinInLowOffset.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.0, false, false, false);

            this.FHost.CreateEnumInput("Texture Mapping", TSliceMode.Single, TPinVisibility.True, out this.FPinInTexMode);
            this.FPinInTexMode.SetSubType("2d Mesh Texture Mapping");
        
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            this.FInvalidate = false;

            if (this.FPinInVertices.PinIsChanged || this.FPinInLowOffset.PinIsChanged 
                || this.FPinInWidth.PinIsChanged || this.FPinInTexMode.PinIsChanged)
            {
                this.FVertex.Clear();
                this.FIndices.Clear();

                double dblwidth,dbloffset;
                string stexmode;
                this.FPinInLowOffset.GetValue(0, out dbloffset);
                this.FPinInWidth.GetValue(0, out dblwidth);
                this.FPinInTexMode.GetString(0, out stexmode);
                e2dMeshTextureMapping tm = (e2dMeshTextureMapping)Enum.Parse(typeof(e2dMeshTextureMapping), stexmode);


                double lowbound = -dblwidth / 2.0;
                double step = dblwidth / (this.FPinInVertices.SliceCount - 1);

                double lowest = double.MaxValue;

                //Highest also for tex coords
                double highest = double.MinValue; 

                double* hptr;
                int ptrcount;

                this.FPinInVertices.GetValuePointer(out ptrcount, out hptr);

                //Get lowest indice
                for (int i = 0; i < ptrcount; i++)
                {
                    if (hptr[i] < lowest) { lowest = hptr[i]; }
                    if (hptr[i] > highest) { highest = hptr[i]; }
                }

                //Substract the offset
                lowest -= dbloffset;

                //Build all the vertices

                Vertex[] verts = new Vertex[ptrcount * 2];

                double xpos = lowbound;

                double height = highest - lowest;
                for (int i = 0; i < ptrcount; i++)
                {
                    //High bound
                    Vertex high = new Vertex();
                    high.pv = new Vector3(Convert.ToSingle(xpos), Convert.ToSingle(hptr[i]), 0.0f);
                    high.nv = new Vector3(0.0f, 0.0f, 1.0f);
                    high.tu1 = Convert.ToSingle((high.pv.X - lowbound) / dblwidth);

                    if (tm == e2dMeshTextureMapping.Crop)
                    {
                        high.tv1 = 1.0f - Convert.ToSingle((high.pv.Y - lowest) / height);
                    }
                    else
                    {
                        high.tv1 = 0.0f;
                    }

                    verts[i] = high;

                    //Low Bound
                    Vertex low = new Vertex();
                    low.pv = new Vector3(Convert.ToSingle(xpos), Convert.ToSingle(lowest), 0.0f);
                    low.nv = new Vector3(0.0f, 0.0f, 1.0f);
                    low.tu1 = Convert.ToSingle((low.pv.X - lowbound) / dblwidth);
                    low.tv1 = 1.0f;

                    verts[i + ptrcount] = low;

                    xpos += step;

                }

                //List<short> indices = new List<short>();

                short[] indices = new short[(ptrcount-1) * 6];

                int indstep = 0;
                for (int i = 0; i < ptrcount - 1; i++)
                {
                    //Triangle from low to high
                    indices[indstep] = Convert.ToInt16(i);
                    indices[indstep + 1] = Convert.ToInt16(i + 1);
                    indices[indstep + 2] = Convert.ToInt16(ptrcount + i);

                    //Triangle from high to low
                    indices[indstep + 3] = Convert.ToInt16(i + 1);
                    indices[indstep + 4] = Convert.ToInt16(ptrcount + i);
                    indices[indstep + 5] = Convert.ToInt16(ptrcount + i + 1);

                    indstep += 6;
                }

                this.FVertex.Add(verts);
                this.FIndices.Add(indices);
                this.InvalidateMesh(this.FVertex.Count);
  
            }
        }
        #endregion

    }
        
        
}

