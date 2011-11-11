using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;

namespace VVVV.Nodes
{

    public class SuperformulaHitTestNode : Abstract2dhitTestNode, IPlugin
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "HitTest";							//use CamelCaps and no spaces
                Info.Category = "2d";						//try to use an existing one
                Info.Version = "Superformula";						//versions are optional. leave blank if not needed
                Info.Help = "Performs a Hittest between a set of points and superformula shapes";
                Info.Bugs = "";
                Info.Credits = "";								//give credits to thirdparty code used
                Info.Warnings = "";
                Info.Author = "vux";

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

        private IValueIn FPinInAB;
        private IValueIn FPinInMN;

        protected override void SetInputPins()
        {
            
            this.FHost.CreateValueInput("AB", 2, null , TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInAB);
            this.FPinInAB.SetSubType2D(double.MinValue, double.MaxValue, 0.01, 0, 0, false, false, false);
 
            this.FHost.CreateValueInput("MN", 4, null , TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInMN);
            this.FPinInMN.SetSubType4D(double.MinValue, double.MaxValue, 0.01, 0, 0, 0,0, false, false, false);
           
        }

        protected override void SetOutputPins()
        {
            
        }

        protected override int GetSpreadObject()
        {
            int ret = Math.Max(base.GetSpreadObject(), this.FPinInAB.SliceCount);
            ret = Math.Max(ret, this.FPinInMN.SliceCount);
            return ret;
        }

        protected override bool ObjectChanged()
        {
            return this.FPinInMN.PinIsChanged
                || this.FPinInAB.PinIsChanged
                || this.FPinInTransform.PinIsChanged;
        }

        protected override bool OnEvaluate(int SpreadMax, bool inputchanged)
        {
            if (this.FPinInMN.PinIsChanged || this.FPinInAB.PinIsChanged || inputchanged)
            {
                this.ResetLists();
                int maxobjects = Math.Max(this.FPinInAB.SliceCount, Math.Max(this.FPinInMN.SliceCount, this.FPinInTransform.SliceCount));

                for (int i = 0; i < maxobjects; i++)
                {
                    this.FObjectHit.Add(false);
                }

                for (int i = 0; i < this.FPinInPoint.SliceCount; i++)
                {
                    double ptx, pty;
                    double a, b, m, n1, n2, n3;

                    this.FPinInPoint.GetValue2D(i, out ptx, out pty);
                    Vector2D v = new Vector2D(ptx, pty);

                    for (int j = 0; j < maxobjects; j++)
                    {
                        Matrix4x4 trobject;
                       
                        this.FPinInTransform.GetMatrix(j, out trobject);
                        this.FPinInAB.GetValue2D(j, out a, out b);
                        this.FPinInMN.GetValue4D(j, out m, out n1,out n2,out n3);

                        //Bring point in formula space
                        Vector2D trv = ((!trobject) * v).xy;


                        //Get angle/distance between superformula origin and point

                        double phi = Math.Atan2(trv.y, trv.x);
                        double dist = Math.Sqrt(trv.x * trv.x + trv.y * trv.y);

                        //Get radius of formula
                        double r = Math.Pow(Math.Abs(Math.Cos(m * phi / 4.0) / a), n2);
                        r += Math.Pow(Math.Abs(Math.Sin(m * phi / 4.0) / b), n3);
                        r = Math.Pow(r, 1.0 / n1);

                        if (dist < r)
                        {
                            this.FHits.Add(new VVVV.Lib.Hit(i, j)); ;
                            this.FObjectHit[j] = true;
                            this.FPointHit[i] = true;
                        }

                    }
                }


                return true;
            }
            else
            {
                return false;
            }

            
        }
    }
        
        
}
