using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;

namespace VVVV.Nodes
{
    public class CircleHitTestNode : Abstract2dhitTestNode, IPlugin
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "HitTest";							//use CamelCaps and no spaces
                Info.Category = "2d";						//try to use an existing one
                Info.Version = "Circle";						//versions are optional. leave blank if not needed
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
       
        protected override void SetInputPins()
        {     
        }

        protected override void SetOutputPins()
        {
            
        }

        protected override bool OnEvaluate(int SpreadMax, bool inputchanged)
        {
            if (inputchanged)
            {
                this.ResetLists();

                for (int i = 0; i < this.FPinInTransform.SliceCount; i++)
                {
                    this.FObjectHit.Add(false);
                }

                for (int i = 0; i < this.FPinInPoint.SliceCount; i++)
                {
                    double ptx, pty;

                    this.FPinInPoint.GetValue2D(i, out ptx, out pty);
                    Vector2D v = new Vector2D(ptx, pty);

                    for (int j = 0; j < this.FPinInTransform.SliceCount; j++)
                    {
                        Matrix4x4 trobject;
                        this.FPinInTransform.GetMatrix(j, out trobject);
                        Vector2D trv = ((!trobject) * v).xy;

                        //Calculate distance to origin
                        double dist = Math.Sqrt(trv.x * trv.x + trv.y * trv.y);


                        if (dist < 0.5)
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
