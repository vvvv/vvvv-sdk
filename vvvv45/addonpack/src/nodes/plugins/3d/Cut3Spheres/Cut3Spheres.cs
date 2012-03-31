#region licence/info

//////project name
//Cut 3 Spheres

//////description
//ouputs the intersection points of 3 overlapping spheres

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop 

//////dependencies
//VVVV.PluginInterfaces.V1;
//VVVV.Utils.VColor;
//VVVV.Utils.VMath;

//////initial author
//vvvv group

#endregion licence/info

//use what you need
using System;
using System.Drawing;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition
	public class CutSpheresPlugin: IPlugin
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	
    	//input pin declaration
    	private IValueIn FA;
    	private IValueIn FB;
    	private IValueIn FC;
    	private IValueIn FRA;
    	private IValueIn FRB;
    	private IValueIn FRC;
    	
    	//output pin declaration
		private IValueOut FIntersections;
		private IValueOut FP1;
    	private IValueOut FP2;

    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public CutSpheresPlugin()
        {
			//the nodes constructor
			//nothing to declare for this node
		}
        
        ~CutSpheresPlugin()
	    {
	    	//the nodes destructor
        	//nothing to destruct
	    }

        #endregion constructor/destructor
        
        #region node name and infos
       
        //provide node infos 
        public static IPluginInfo PluginInfo
	    {
	        get 
	        {
	        	//fill out nodes info
	        	//see: http://www.vvvv.org/tiki-index.php?page=vvvv+naming+conventions
	        	IPluginInfo Info = new PluginInfo();
	        	Info.Name = "CutSpheres";					//use CamelCaps and no spaces
	        	Info.Category = "3d";						//try to use an existing one
	        	Info.Version = "Three";						//versions are optional. leave blank if not needed
	        	Info.Help = "Ouputs the 2 points as the result of cutting 3 spheres.";
	        	Info.Bugs = "The 3 points cannot have the same z value";
	        	Info.Credits = "";								//give credits to thirdparty code used
	        	Info.Warnings = "";
	        	Info.Tags = "intersect, intersection, cut, points, purely functional, deterministic, overlap";
	        	Info.Author = "vvvv group";
	        	
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

        public bool AutoEvaluate
        {
        	//return true if this node needs to calculate every frame even if nobody asks for its output
        	get {return false;}
        }
        
        #endregion node name and infos
        
      	#region pin creation
        
        //this method is called by vvvv when the node is created
        public void SetPluginHost(IPluginHost Host)
	    {
        	//assign host
	    	FHost = Host;

	    	//create inputs
	    	FHost.CreateValueInput("A.", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out FA);
	    	FA.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.0, false, false, false);
	        
	    	FHost.CreateValueInput("B.", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out FB);
	    	FB.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.5, false, false, false);
	    	
	    	FHost.CreateValueInput("C.", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out FC);
	    	FC.SetSubType(double.MinValue, double.MaxValue, 0.01, 1.0, false, false, false);
	    	
	    	FHost.CreateValueInput("Radius A", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FRA);
	    	FRA.SetSubType(double.MinValue, double.MaxValue, 0.01, 1.0, false, false, false);

	    	FHost.CreateValueInput("Radius B", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FRB);
	    	FRB.SetSubType(double.MinValue, double.MaxValue, 0.01, 1.0, false, false, false);

	    	FHost.CreateValueInput("Radius C", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FRC);
	    	FRC.SetSubType(double.MinValue, double.MaxValue, 0.01, 1.0, false, false, false);

	    	//create outputs
	    	FHost.CreateValueOutput("Intersections", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FIntersections);
	    	FIntersections.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);	  
	    	
	    	FHost.CreateValueOutput("Intersect Point 1", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out FP1);
	    	FP1.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
	    	
	    	FHost.CreateValueOutput("Intersect Point 2", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out FP2);
	    	FP2.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
	    	
        }

        #endregion pin creation
        
        #region mainloop
        
        public void Configurate(IPluginConfig Input)
        {
        	//nothing to configure in this plugin
        	//only used in conjunction with inputs of type cmpdConfigurate
        }
        
        public void QuadraticEquation(double a, double b, double c, out double x1, out double x2, out int solutions)
        {
        	x1 = 0;
        	x2 = 0;
        	
        	if (a==0)
        	{
        		if ((b==0) && (c==0)) 
        		{
        			solutions = int.MaxValue;
        		}
        		else
        		{
        		 	x1 = - c / b;
      				solutions = 1;	
        		}	
        	}
        	else
        	{
        		double D = Math.Pow(b, 2) - 4 * a * c;

        		if (D == 0) 
        		{
      				x1 = -b / (2*a);
      				solutions = 1;
        		}
        		else
        		{
        			if (D > 0)
          			{
        				D = Math.Sqrt(D);
      					x1 = (-b + D) / (2*a);
      					x2 = (-b - D) / (2*a);
      					solutions = 2;
        			}
        			else
         			{
        				solutions = 0;
        			}
        		}
        	}

        }
        	
        
        //here we go, thats the method called by vvvv each frame
        //all data handling should be in here
        public void Evaluate(int SpreadMax)
        {     	
        	//if any of the inputs has changed
        	//recompute the outputs

        	FP1.SliceCount = SpreadMax;
        	FP2.SliceCount = SpreadMax;
        	FIntersections.SliceCount = SpreadMax;
        	
        	for(int i=0; i<SpreadMax; i++)
        	{
        		
        		double Ax, Ay, Az;
        		double Bx, By, Bz;
        		double Cx, Cy, Cz;
        		
        		FA.GetValue3D(0, out Ax, out Ay, out Az);
        		FB.GetValue3D(0, out Bx, out By, out Bz);
        		FC.GetValue3D(0, out Cx, out Cy, out Cz);
        		
        		double r_a, r_b, r_c;
        		
        		FRA.GetValue(i, out r_a);
        		FRB.GetValue(i, out r_b);
        		FRC.GetValue(i, out r_c);
        		
        		double P1x = 0, P1y = 0, P1z = 0;
        		double P2x = 0, P2y = 0, P2z = 0;

        		double Asquare = Math.Pow(Ax, 2) + Math.Pow(Ay, 2) + Math.Pow(Az, 2);
        		double Bsquare = Math.Pow(Bx, 2) + Math.Pow(By, 2) + Math.Pow(Bz, 2);
        		double Csquare = Math.Pow(Cx, 2) + Math.Pow(Cy, 2) + Math.Pow(Cz, 2);
        		
        		double ABx = Bx - Ax;
        		double ABy = By - Ay;
        		double ABz = Bz - Az;
        		
        		double ACx = Cx - Ax;
        		double ACy = Cy - Ay;
        		double ACz = Cz - Az;
        		
        		double m = (Math.Pow(r_a, 2) - Math.Pow(r_b, 2) - Asquare + Bsquare) * ACz;
        		double n = (Math.Pow(r_a, 2) - Math.Pow(r_c, 2) - Asquare + Csquare) * ABz;
        		double o = ABy*ACz - ACy*ABz;
        		double q = ACx*ABz - ABx*ACz;
        		
        		double v = (Math.Pow(r_a, 2) - Math.Pow(r_b, 2) - Asquare + Bsquare) / (2*ABz) - Az;
        		double w = (1+Math.Pow(ABy, 2)/Math.Pow(ABz, 2));

        		double r = ABx/ABz;
        		double s = ABy/ABz;

        		double t = Math.Pow(Ax, 2) + Math.Pow(Ay, 2) + Math.Pow(v, 2) - Math.Pow(r_a, 2);
        		double a = (m-n) / (2*o);
        		double b = q / o;
        		
        		//(1+r^2+b^2*w+2brs)P.x^2 + (2abw+2ars-2A.x-2rv-2b(A.y + sv))P.x + w*a^2 - 2a(A.y + sv) + t = 0
        		
        		int solutions;
        		QuadraticEquation(1 + Math.Pow(r, 2) + Math.Pow(b, 2)*w + 2*b*r*s,
        		                  2*a*b*w + 2*a*r*s - 2*Ax - 2*r*v - 2*b*(Ay + s*v),
        		                  w*Math.Pow(a, 2) - 2*a*(Ay + s*v) + t,
        		                  out P1x, out P2x, out solutions);
        		
        		FIntersections.SetValue(i, solutions);

        		P1y = P1x * b + a;
        		//P.z = (r_a^2 - r_b^2 - 2*P.x*AB.x - 2*P.y*AB.y - |A|^2 + |B|^2) / (2*AB.z)
        		P1z = (Math.Pow(r_a, 2) - Math.Pow(r_b, 2) - 2*P1x*ABx - 2*P1y*ABy - Asquare + Bsquare) / (2*ABz);

        		P2y = P2x * b + a;
        		P2z = (Math.Pow(r_a, 2) - Math.Pow(r_b, 2) - 2*P2x*ABx - 2*P2y*ABy - Asquare + Bsquare) / (2*ABz);
        		
        		FP1.SetValue3D(i, P1x, P1y, P1z);
        		FP2.SetValue3D(i, P2x, P2y, P2z);
        	}
        	
        }
             
        #endregion mainloop  
	}
}
/*



length (P - A) = r_a
d_a = P - A

d_a.x^2 + d_a.y^2 + d_a.z^2 = r_a^2
(P.x^2 - 2*P.x*A.x + A.x^2) + (...y) + (...z) = r_a^2         (I)

length (P - B) = r_b
(P.x^2 - 2*P.x*B.x + B.x^2) + (...y) + (...z) = r_b^2         (II)

length (P - C) = r_c
(P.x^2 - 2*P.x*C.x + C.x^2) + (...y) + (...z) = r_c^2         (III)

->
I - II
2*P.x*(B.x-A.x) + 2*P.y*(B.y-A.y) + 2*P.z*(B.z-A.z) + |A|^2 - |B|^2 = r_a^2 - r_b^2
P.z = (r_a^2 - r_b^2 - 2*P.x*AB.x - 2*P.y*AB.y - |A|^2 + |B|^2) / (2*AB.z)                          (IV)

I - III
2*P.x*(C.x-A.x) + 2*P.y*(C.y-A.y) + 2*P.z*(C.z-A.z) + |A|^2 - |C|^2 = r_a^2 - r_c^2         
P.z = (r_a^2 - r_c^2 - 2*P.x*AC.x - 2*P.y*AC.y - |A|^2 + |C|^2) / (2*AC.z)                          (V)

getting rid of P.z :
IV = V
(r_a^2 - r_b^2 - 2*P.x*AB.x - 2*P.y*AB.y - |A|^2 + |B|^2) / (2*AB.z) = 
(r_a^2 - r_c^2 - 2*P.x*AC.x - 2*P.y*AC.y - |A|^2 + |C|^2) / (2*AC.z)   

(r_a^2 - r_b^2 - 2*P.x*AB.x - 2*P.y*AB.y - |A|^2 + |B|^2) * AC.z = 
(r_a^2 - r_c^2 - 2*P.x*AC.x - 2*P.y*AC.y - |A|^2 + |C|^2) * AB.z   

(r_a^2 - r_b^2 - 2*P.x*AB.x - |A|^2 + |B|^2) * AC.z  - 2*P.y*AB.y*AC.z =
(r_a^2 - r_c^2 - 2*P.x*AC.x - |A|^2 + |C|^2) * AB.z  - 2*P.y*AC.y*AB.z

(r_a^2 - r_b^2 - 2*P.x*AB.x - |A|^2 + |B|^2) * AC.z - 
(r_a^2 - r_c^2 - 2*P.x*AC.x - |A|^2 + |C|^2) * AB.z = 2*P.y*(AB.y*AC.z - AC.y*AB.z)

P.y = ((r_a^2 - r_b^2 - 2*P.x*AB.x - |A|^2 + |B|^2) * AC.z - 
	   (r_a^2 - r_c^2 - 2*P.x*AC.x - |A|^2 + |C|^2) * AB.z) / (2*(AB.y*AC.z - AC.y*AB.z))                           

P.y = ( 2*(AC.x*AB.z - AB.x*AC.z)*P.x + m-n ) / (2*(AB.y*AC.z - AC.y*AB.z))     
with m = (r_a^2 - r_b^2 - |A|^2 + |B|^2) * AC.z
and  n = (r_a^2 - r_c^2 - |A|^2 + |C|^2) * AB.z

P.y = P.x * q / o + (m-n) / (2*o)					   (VI)
  with o = AB.y*AC.z - AC.y*AB.z
  and  q = AC.x*AB.z - AB.x*AC.z
                                                                                                             
I
d_a.x^2 + d_a.y^2 + (P.z - A.z)^2 = r_a^2

IV in I 
d_a.x^2 + d_a.y^2 + ((r_a^2 - r_b^2 - 2*P.x*AB.x - 2*P.y*AB.y - |A|^2 + |B|^2) / (2*AB.z) - A.z)^2 = r_a^2
(P.y-A.y)^2 + ((r_a^2 - r_b^2 - 2*P.x*AB.x - 2*P.y*AB.y - |A|^2 + |B|^2) / (2*AB.z) - A.z)^2 = r_a^2 - d_a.x^2 




((r_a^2 - r_b^2 - 2*P.x*AB.x - 2*P.y*AB.y - |A|^2 + |B|^2) / (2*AB.z) - A.z)^2 = 
((r_a^2 - r_b^2 - 2*P.x*AB.x - |A|^2 + |B|^2) / (2*AB.z) - A.z - P.y*AB.y/AB.z)^2 =
(u - P.y*AB.y/AB.z)^2     with   u := (r_a^2 - r_b^2 - 2*P.x*AB.x - |A|^2 + |B|^2) / (2*AB.z) - A.z
= u^2 - 2*u*P.y*AB.y/AB.z + P.y^2*AB.y^2/AB.z^2


->
(P.y-A.y)^2 + u^2 - 2*u*P.y*AB.y/AB.z + P.y^2*AB.y^2/AB.z^2 = r_a^2 - d_a.x^2 

(1+AB.y^2/AB.z^2)*P.y^2 - 2*(A.y + u*AB.y/AB.z)*P.y + A.y^2 + u^2 - r_a^2 + d_a.x^2 = 0

u = -(AB.x/AB.z)*P.x + v
with v =  (r_a^2 - r_b^2 - |A|^2 + |B|^2) / (2*AB.z) - A.z

->
w*P.y^2 - 2*(A.y + (-(AB.x/AB.z)*P.x + v)*AB.y/AB.z)*P.y + A.y^2 + (-(AB.x/AB.z)*P.x + v)^2 - r_a^2 + d_a.x^2 = 0
with w = (1+AB.y^2/AB.z^2)


w*P.y^2 - 2*(A.y + (-r*P.x+v)*s)*P.y + A.y^2 + (r*P.x-v)^2 - r_a^2 + d_a.x^2 = 0


w*P.y^2 + r^2*P.x^2 + 2*r*s*P.x*P.y - 2*(A.y + v*s)*P.y - 2*r*v*P.x + A.y^2 + v^2 - r_a^2 + (P.x-A.x)^2= 0           
with r = AB.x/AB.z
and  s = AB.y/AB.z

(1+r^2)*P.x^2 + w*P.y^2 + 2*r*s*P.x*P.y - 2*(A.x + r*v)*P.x - 2*(A.y + s*v)*P.y + t = 0            (VII)
with t = A.x^2 + A.y^2 + v^2 - r_a^2


VI
P.y = P.x * b + a
with a = (m-n) / (2*o)
and b = q / o

VI in VII
(1+r^2)*P.x^2 + w*(P.x * b + a)^2 + 2*r*s*P.x*(P.x * b + a) - 2*(A.x + r*v)*P.x - 2*(A.y + s*v)*(P.x * b + a) + t = 0           

(1+r^2)*P.x^2 + w*(b^2*P.x^2 + 2*a*b*P.x + a^2) + 2*b*r*s*P.x^2 + 2*a*r*s*P.x - 2*(A.x + r*v)*P.x - 2*b*(A.y + s*v)*P.x - 2*a*(A.y + s*v) + t = 0           
(1+r^2+b^2*w+2brs)P.x^2 + (2abw+2ars-2A.x-2rv-2b(A.y + sv))P.x + w*a^2 - 2a(A.y + sv) + t = 0  

 */ 
