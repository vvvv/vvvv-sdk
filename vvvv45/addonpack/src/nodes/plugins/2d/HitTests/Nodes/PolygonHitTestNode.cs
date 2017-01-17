using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;

namespace VVVV.Nodes
{
    public class PolygonHitTestNode : Abstract2dhitTestNode,IPlugin
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "HitTest";							//use CamelCaps and no spaces
                Info.Category = "2d";						//try to use an existing one
                Info.Version = "Polygon";						//versions are optional. leave blank if not needed
                Info.Help = "Performs a Hittest between a set of points and polygon shapes";
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

        private IValueIn FPinInVertices;
        private IValueIn FPinInVerticesCount;

        protected override void SetInputPins()
        {  
            this.FHost.CreateValueInput("Vertices", 2, null , TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInVertices);
            this.FPinInVertices.SetSubType2D(double.MinValue, double.MaxValue, 0.01, 0, 0, false, false, false);
       
            this.FHost.CreateValueInput("Vertices Count", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInVerticesCount);
            this.FPinInVerticesCount.SetSubType(3, double.MaxValue, 1, 0, false, false, true);
        
        }

        protected override void SetOutputPins()
        {

        }

        private bool PointInPoly(int nvert, double[] vertx, double[] verty, double testx, double testy)
        {
            bool c = false;
            int j = nvert - 1;
            for (int i = 0; i < nvert; i++)
            {
                if (((verty[i] > testy) != (verty[j] > testy)) &&
                    (testx < (vertx[j] - vertx[i]) * (testy - verty[i]) / (verty[j] - verty[i]) + vertx[i]))
                    c = !c;

                j = i;
            }

            return c;
        }

        private bool inpoly(                            /* is target point inside a 2D polygon? */
double[] polyx,double[] polyy ,           /*   polygon points, [0]=x, [1]=y       */
int npoints,                       /*   number of points in polygon        */
double xt,
            double yt)                 /*   x (horizontal) of target point  yt)                   /*   y (vertical) of target point       */
        {
            double xnew, ynew;
            double xold, yold;
            double x1, y1;
            double x2, y2;
            int i;
            bool inside = false;

            if (npoints < 3)
            {
                return (false);
            }
            xold = polyx[npoints - 1];
            yold = polyy[npoints - 1];
            for (i = 0; i < npoints; i++)
            {
                xnew = polyx[i];
                ynew = polyy[i];
                if (xnew > xold)
                {
                    x1 = xold;
                    x2 = xnew;
                    y1 = yold;
                    y2 = ynew;
                }
                else
                {
                    x1 = xnew;
                    x2 = xold;
                    y1 = ynew;
                    y2 = yold;
                }
                if ((xnew < xt) == (xt <= xold)          /* edge "open" at one end */
                 && ((double)yt - (double)y1) * (double)(x2 - x1)
                  < ((double)y2 - (double)y1) * (double)(xt - x1))
                {
                    inside = !inside;
                }
                xold = xnew;
                yold = ynew;
            }
            return (inside);
        }


        protected override bool OnEvaluate(int SpreadMax, bool inputchanged)
        {
            if (this.FPinInVertices.PinIsChanged || this.FPinInVerticesCount.PinIsChanged || inputchanged)
            {
                this.ResetLists();
                int maxobjects = Math.Max(this.FPinInVerticesCount.SliceCount, this.FPinInTransform.SliceCount);

                for (int i = 0; i < maxobjects; i++)
                {
                    this.FObjectHit.Add(false);
                }

                for (int i = 0; i < this.FPinInPoint.SliceCount; i++)
                {
                    double ptx, pty;



                    this.FPinInPoint.GetValue2D(i, out ptx, out pty);
                    Vector2D v = new Vector2D(ptx, pty);

                    int cnt = 0;
                    for (int j = 0; j < maxobjects; j++)
                    {
                        Matrix4x4 trobject;
                        double vcount;
                        this.FPinInTransform.GetMatrix(j, out trobject);
                        this.FPinInVerticesCount.GetValue(j, out vcount);

                        double[] dx = new double[Convert.ToInt32(vcount)];
                        double[] dy = new double[Convert.ToInt32(vcount)];
                        for (int k = 0; k < vcount; k++)
                        {
                            double x,y;
                            this.FPinInVertices.GetValue2D(cnt, out x, out y);

                            Vector2D v2 = new Vector2D(x, y);
                            Vector2D trv = (trobject * v2).xy;

                            dx[k] = trv.x;
                            dy[k] = trv.y;
                            cnt++;
                        }


                        //if (this.PointInPoly(Convert.ToInt32(vcount),dx,dy,ptx,pty))
                        if (this.inpoly(dx,dy,Convert.ToInt32(vcount),ptx,pty))
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
