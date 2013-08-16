using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;
using VVVV.Nodes.Lib;

using SlimDX;

namespace VVVV.Nodes
{
    public unsafe class DecomposeNode : IPlugin, IDisposable
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "Decompose";							//use CamelCaps and no spaces
                Info.Category = "Transform";						//try to use an existing one
                Info.Version = "Quaternion";						//versions are optional. leave blank if not needed
                Info.Help = "Decompose a transform into translate/scale/rotate";
                Info.Bugs = "";
                Info.Credits = "";								//give credits to thirdparty code used
                Info.Warnings = "";
                Info.Author = "vux";
                Info.Tags = "";

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
        private IPluginHost FHost;

        private ITransformIn FInTransform;

        private IValueOut FOutTranslate;
        private IValueOut FOutScale;
        private IValueOut FOutRotate;

        private IValueOut FOutSuccess;

        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return false; }
        }
        #endregion

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            //assign host
            this.FHost = Host;

            this.FHost.CreateTransformInput("Transform In", TSliceMode.Dynamic, TPinVisibility.True, out this.FInTransform);
           
            this.FHost.CreateValueOutput("Translate", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FOutTranslate);
            this.FOutTranslate.SetSubType3D(double.MinValue, double.MaxValue, 0.01, 0, 0, 0, false, false, false);
    
            this.FHost.CreateValueOutput("Scale", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FOutScale);
            this.FOutScale.SetSubType3D(double.MinValue, double.MaxValue, 0.01, 0, 0, 0, false, false, false);
    
            this.FHost.CreateValueOutput("Rotate", 4, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FOutRotate);
            this.FOutRotate.SetSubType4D(double.MinValue, double.MaxValue, 0.01, 0, 0, 0,1, false, false, false);
          
            this.FHost.CreateValueOutput("Success", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FOutSuccess);
            this.FOutSuccess.SetSubType(0, 1, 1, 0, false, true, false);
           
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
            this.FOutTranslate.SliceCount = this.FInTransform.SliceCount;
            this.FOutScale.SliceCount = this.FInTransform.SliceCount;
            this.FOutRotate.SliceCount = this.FInTransform.SliceCount;
            this.FOutSuccess.SliceCount = this.FInTransform.SliceCount;

            double* tr;
            double* sc;
            double* rot;
            this.FOutTranslate.GetValuePointer(out tr);
            this.FOutScale.GetValuePointer(out sc);
            this.FOutRotate.GetValuePointer(out rot);

            Matrix4x4 mat;
            Vector3 tv;
            Vector3 sv;
            Quaternion rv;
            for (int i = 0; i < SpreadMax; i++)
            {
                
                this.FInTransform.GetMatrix(i, out mat);

                bool res = this.Convert(mat).Decompose(out sv, out rv, out tv);

                if (!res)
                {
                    this.FOutSuccess.SetValue(i, 0);
                    tv = Vector3.Zero;
                    sv = new Vector3(1.0f, 1.0f, 1.0f);
                    rv = Quaternion.Identity;
                }
                else
                {
                    this.FOutSuccess.SetValue(i, 1);
                }

                tr[i * 3] = tv.X;
                tr[i * 3 + 1] = tv.Y;
                tr[i * 3 + 2] = tv.Z;

                sc[i * 3] = sv.X;
                sc[i * 3 + 1] = sv.Y;
                sc[i * 3 + 2] = sv.Z;

                rot[i * 4] = rv.X;
                rot[i * 4 + 1] = rv.Y;
                rot[i * 4 + 2] = rv.Z;
                rot[i * 4 + 3] = rv.W;
            }
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
        }
        #endregion

        private Matrix Convert(Matrix4x4 m)
        {
            Matrix sm = new Matrix();
            sm.M11 = (float)m.m11;
            sm.M12 = (float)m.m12;
            sm.M13 = (float)m.m13;
            sm.M14 = (float)m.m14;
            sm.M21 = (float)m.m21;
            sm.M22 = (float)m.m22;
            sm.M23 = (float)m.m23;
            sm.M24 = (float)m.m24;
            sm.M31 = (float)m.m31;
            sm.M32 = (float)m.m32;
            sm.M33 = (float)m.m33;
            sm.M34 = (float)m.m34;
            sm.M41 = (float)m.m41;
            sm.M42 = (float)m.m42;
            sm.M43 = (float)m.m43;
            sm.M44 = (float)m.m44;
            return sm;
        }
    }


}
