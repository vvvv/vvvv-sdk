using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;

namespace VVVV.Nodes
{
    public class SegmentHitTestNode : Abstract2dhitTestNode, IPlugin
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "HitTest";							//use CamelCaps and no spaces
                Info.Category = "2d";						//try to use an existing one
                Info.Version = "Segment";						//versions are optional. leave blank if not needed
                Info.Help = "Performs a Hittest between a set of points and circle shapes";
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

        private IValueIn FPinInInnerRadius;
        private IValueIn FPinInCycles;
        private IValueIn FPinInPhase;
       
        protected override void SetInputPins()
        {     
            this.FHost.CreateValueInput("Inner Radius", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInInnerRadius);
            this.FPinInInnerRadius.SetSubType(0, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueInput("Phase", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInPhase);
            this.FPinInPhase.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
         
            this.FHost.CreateValueInput("Cycles", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInCycles);
            this.FPinInCycles.SetSubType(double.MinValue, double.MaxValue, 0.01, 1, false, false, false);
           
        }

        protected override bool ObjectChanged()
        {
            return this.FPinInCycles.PinIsChanged
                || this.FPinInInnerRadius.PinIsChanged
                || this.FPinInTransform.PinIsChanged
                || this.FPinInPhase.PinIsChanged;
        }

        protected override int GetSpreadObject()
        {
            int ret = Math.Max(base.GetSpreadObject(),this.FPinInPhase.SliceCount);
            ret = Math.Max(ret, this.FPinInInnerRadius.SliceCount);
            ret = Math.Max(this.FPinInCycles.SliceCount, ret);
            return ret;
        }

        protected override void SetOutputPins()
        {
            
        }

        protected override bool OnEvaluate(int SpreadMax, bool inputchanged)
        {
            if (inputchanged)
            {
                this.ResetLists();

                for (int i = 0; i < this.GetSpreadObject(); i++)
                {
                    this.FObjectHit.Add(false);
                }

                for (int i = 0; i < this.FPinInPoint.SliceCount; i++)
                {
                    double ptx, pty;

                    this.FPinInPoint.GetValue2D(i, out ptx, out pty);
                    Vector2D v = new Vector2D(ptx, pty);

                    double dblinner,dblphase,dblcycles;
                    for (int j = 0; j < this.GetSpreadObject(); j++)
                    {
                        Matrix4x4 trobject;
                        this.FPinInTransform.GetMatrix(j, out trobject);
                        Vector2D trv = ((!trobject) * v).xy;

                        
                        this.FPinInInnerRadius.GetValue(j, out dblinner);
                        this.FPinInPhase.GetValue(j, out dblphase);
                        this.FPinInCycles.GetValue(j, out dblcycles);
                        dblinner *= 0.5;

                        //Calculate distance to origin
                        double dist = Math.Sqrt(trv.x * trv.x + trv.y * trv.y);
                        double angle = Math.Atan2(trv.y, trv.x);
                        angle /= (Math.PI * 2.0);
                        angle = angle < 0 ? (angle + 1.0) : angle;
                        //angle = angle % 1.0d;
                        
                        //double dist = Math.Sqrt(angle*angle+dblphase

                        dblphase = dblphase % 1.0d;
                        //angle += dblphase;

                        ////if (dblphase < 0)
                        //{
                        //    dblphase++;
                        //    angle++;
                        //}

                        //double d = Math.Sqrt(angle * angle + 
                        dblphase = dblphase < 0 ? (dblphase + 1.0) : dblphase;
                        double d = Math.Abs(angle - dblphase);
                        if (d > 0.5)
                        {
                            if (dblphase > angle) { dblphase -= 1.0; }
                        }

                        angle = angle % 1.0d;
                        //dblphase = dblphase < 0 ? (dblphase + 1.0) : dblphase;

                        double rng = dblcycles + dblphase;
                        //rng = rng % 1.0d;

                        if (dist < 0.5 && dist > dblinner && angle > dblphase && angle < rng)
                        {
                            this.FHits.Add(new VVVV.Lib.Hit(i, j)); ;
                            this.FObjectHit[j] = true;
                            this.FPointHit[i] = true;
                        }

                    }
                }

                return true;
            }

            return false;
        }
    }
}
