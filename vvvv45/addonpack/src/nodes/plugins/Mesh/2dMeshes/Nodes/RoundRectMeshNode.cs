using System;
using System.Collections.Generic;
using System.Text;
using VVVV.Lib;
using VVVV.PluginInterfaces.V1;
using SlimDX;

namespace VVVV.Nodes
{
    public class RoundRectMeshNode :  AbstractMeshNode, IPlugin, IDisposable, IPluginDXMesh
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "RoundRect";							//use CamelCaps and no spaces
                Info.Category = "EX9.Geometry";						//try to use an existing one
                Info.Version = "2d";						//versions are optional. leave blank if not needed
                Info.Help = "Round Rectangle mesh";
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

        private IValueIn FPinInInnerRadius;
        private IValueIn FPinInOuterRadius;
        private IValueIn FPinInCornerResolution;

        #region Set Plugin Host
        protected override void SetInputs()
        {           
            this.FHost.CreateValueInput("Inner Radius", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInInnerRadius);
            this.FPinInInnerRadius.SetSubType2D(0, double.MaxValue, 0.01, 0.35, 0.35, false, false, false);

            this.FHost.CreateValueInput("Outer Radius", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInOuterRadius);
            this.FPinInOuterRadius.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.15, false, false, false);
       
            this.FHost.CreateValueInput("Corner Resolution", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInCornerResolution);
            this.FPinInCornerResolution.SetSubType(1, double.MaxValue, 1, 1, false, false, true);   
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            this.FInvalidate = false;

            if (this.FPinInInnerRadius.PinIsChanged
               || this.FPinInOuterRadius.PinIsChanged
               || this.FPinInCornerResolution.PinIsChanged)
            {
                this.FVertex.Clear();
                this.FIndices.Clear();

                double innerx, innery, outer, res;
                int ires;

                for (int i = 0; i < SpreadMax; i++)
                {
                    this.FPinInCornerResolution.GetValue(i, out res);
                    this.FPinInInnerRadius.GetValue2D(i, out innerx, out innery);
                    this.FPinInOuterRadius.GetValue(i, out outer);

                    ires = Convert.ToInt32(res);

                    List<Vertex> vl = new List<Vertex>();
                    List<short> il = new List<short>();

                    short idx = 0;

                    float ucy = Convert.ToSingle(innery + outer);
                    float ucx = Convert.ToSingle(innerx + outer);

                    float mx = ucx * 2.0f ;
                    float my = ucy * 2.0f;

                    //Need 1 quad for center
                    idx = this.SetQuad(vl, il, 0.0f, 0.0f, (float)innerx, (float)innery, idx,mx,my);

                    //Need 2 quads up/down
                    idx = this.SetQuad(vl, il, 0.0f, ucy, (float)innerx, (float)outer, idx,mx,my);
                    idx = this.SetQuad(vl, il, 0.0f, -ucy, (float)innerx, (float)outer, idx, mx, my);

                    //Need 2 quads left/right
                    idx = this.SetQuad(vl, il, -ucx, 0.0f, (float)outer, (float)innery, idx, mx, my);
                    idx = this.SetQuad(vl, il, ucx, 0.0f, (float)outer, (float)innery, idx, mx, my);

                    float radius = (float)outer * 2.0f;

                    //Add the 4 corners
                    idx = this.SetSegment(vl, il, (float)innerx, (float)innery, 0.0f, radius, ires, idx, mx, my);
                    idx = this.SetSegment(vl, il, (float)-innerx, (float)innery, 0.25f, radius, ires, idx, mx, my);
                    idx = this.SetSegment(vl, il, (float)-innerx, (float)-innery, 0.5f, radius, ires, idx, mx, my);
                    idx = this.SetSegment(vl, il, (float)innerx, (float)-innery, 0.75f, radius, ires, idx, mx, my);



                    this.FIndices.Add(il.ToArray());
                    this.FVertex.Add(vl.ToArray());
                }

                this.InvalidateMesh(this.FVertex.Count);
            }
        }
        #endregion

        private short SetQuad(List<Vertex> verts, List<short> inds, float cx, float cy, float sx, float sy, short lastindex, float mx, float my)
        {
            Vertex innerv = new Vertex();
            innerv.pv = new Vector3(cx - sx, cy - sy, 0.0f);
            innerv.nv = new Vector3(0.0f, 0.0f, 1.0f);
            innerv = this.TexCoord(innerv, mx, my);
            verts.Add(innerv);

            innerv = new Vertex();
            innerv.pv = new Vector3(cx + sx, cy + sy, 0.0f);
            innerv.nv = new Vector3(0.0f, 0.0f, 1.0f);
            innerv = this.TexCoord(innerv, mx, my);
            verts.Add(innerv);

            innerv = new Vertex();
            innerv.pv = new Vector3(cx - sx, cy + sy, 0.0f);
            innerv.nv = new Vector3(0.0f, 0.0f, 1.0f);
            innerv = this.TexCoord(innerv, mx, my);
            verts.Add(innerv);

            innerv = new Vertex();
            innerv.pv = new Vector3(cx + sx,cy - sy, 0.0f);
            innerv.nv = new Vector3(0.0f, 0.0f, 1.0f);
            innerv = this.TexCoord(innerv, mx, my);
            verts.Add(innerv);

            short[] idxarray = new short[] { 0, 1, 2, 0, 3, 1 };
            for (int i = 0; i < idxarray.Length; i++) { idxarray[i] += lastindex; }
            inds.AddRange(idxarray);

            return Convert.ToInt16(lastindex + 4);
        }

        private Vertex TexCoord(Vertex v, float mx, float my)
        {
            v.tu1 = 0.5f + (v.pv.X / (mx * 2.0f));
            v.tv1 = 0.5f + (v.pv.Y / (my * 2.0f));
            return v;
        }

        private short SetSegment(List<Vertex> verts, List<short> inds, float cx, float cy, 
            float phase,float radius, int ires, short lastindex,float mx,float my)
        {
            //Center vertex
            Vertex innerv = new Vertex();
            innerv.pv = new Vector3(cx, cy, 0.0f);
            innerv.nv = new Vector3(0.0f, 0.0f, 1.0f);
            innerv = this.TexCoord(innerv, mx, my);
            verts.Add(innerv);

            double inc = (Math.PI / 2.0) / (double)ires;
            double phi = phase * (Math.PI * 2.0);

            //Build triangle strip here
            for (int i = 0; i < ires + 1; i++ )
            {
                float x = Convert.ToSingle(cx + radius * Math.Cos(phi));
                float y = Convert.ToSingle(cy + radius * Math.Sin(phi));

                innerv = new Vertex();
                innerv.pv = new Vector3(x, y, 0.0f);
                innerv.nv = new Vector3(0.0f, 0.0f, 1.0f);
                innerv = this.TexCoord(innerv, mx, my);
                verts.Add(innerv);

                phi += inc;
            }

            for (int i = 0; i < ires; i++)
            {
                inds.Add(lastindex);
                inds.Add(Convert.ToInt16(lastindex + i + 1));
                inds.Add(Convert.ToInt16(lastindex + i + 2));
            }

            return Convert.ToInt16(lastindex + ires + 2);
        }

    }
}
