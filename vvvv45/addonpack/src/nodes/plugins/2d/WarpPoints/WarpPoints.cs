#region licence/info

//////project name
//Warp Points

//////description
//warp Points from one 2D space to another 2D space
//useful for manual calibration as known from most touch devices

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
//wirmachenbunt - Heinrich Löwe, Chris Engler 

#endregion licence/info




//  http://openbook.galileocomputing.de/csharp/ 

using System;
using System.Collections.Generic;
using System.Text;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using GridTransform;

namespace VVVV.Nodes
{
    //class definition
    public class GridTransform : IPlugin
    {
        #region field declaration

        // the host (mandatory)
        private IPluginHost FHost;
        // Track whether Dispose has been called.
        private bool FDisposed = false;

        // input pins
        private IValueFastIn FInputPoints;
        private IValueIn FInputGridFrom;
        private IValueIn FInputGridTo;
        private IValueFastIn FPinWidth;
        private IValueFastIn FPinHeight;

        // output pins
        private IValueOut FOutputPoints;
		private IValueOut FOutputHit;
		
        // data
        private int FWidth = 0, FHeight = 0;
        private int FBufSize;

        // gridTransformer
        GridTransformer FTrafo;

        #endregion field declaration

        #region constructor/destructor
    	
        public GridTransform()
        {
			// ...
		}
        
        // Implementing IDisposable's Dispose method.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
        	Dispose(true);
        	// Take yourself off the Finalization queue
        	// to prevent finalization code for this object
        	// from executing a second time.
        	GC.SuppressFinalize(this);
        }
        
        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
        	// Check to see if Dispose has already been called.
        	if(!FDisposed)
        	{
        		if(disposing)
        		{
        			// clear stuff here if needed
        		}
        		// Release unmanaged resources. If disposing is false,
        		// only the following code is executed.

        		// Note that this is not thread safe.
        		// Another thread could start disposing the object
        		// after the managed resources are disposed,
        		// but before the disposed flag is set to true.
        		// If thread safety is necessary, it must be
        		// implemented by the client.
        	}
        	FDisposed = true;
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~GridTransform()
        {
        	// Do not re-create Dispose clean-up code here.
        	// Calling Dispose(false) is optimal in terms of
        	// readability and maintainability.
        	Dispose(false);
        }

        #endregion constructor/destructor

        #region node name and infos

        //provide node infos 
        public static IPluginInfo PluginInfo
        {
            get
            {
                //fill out nodes info
                IPluginInfo Info = new PluginInfo();
                Info.Name = "WarpPoints";
                Info.Category = "2D";
                Info.Version = "";
                Info.Help = "Transforms one gridspace to another";
                Info.Bugs = "";
                Info.Credits = "gnox & u7angel";
                Info.Warnings = "Radioactive, don't touch!";

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
            get { return false; }
        }

        #endregion node name and infos

        #region pin creation

        //this method is called by vvvv when the node is created
        public void SetPluginHost(IPluginHost Host)
        {
            // assign host
            FHost = Host;

            // create inputs
            FHost.CreateValueFastInput("Vector2D In", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FInputPoints);
            FInputPoints.SetSubType(double.MinValue, double.MaxValue, 0.001, 0, false, false, false);

            FHost.CreateValueInput("Source Grid", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FInputGridFrom);
            FInputGridFrom.SetSubType(double.MinValue, double.MaxValue, 0.001, 0, false, false, false);

            FHost.CreateValueInput("Target Grid", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FInputGridTo);
            FInputGridTo.SetSubType(double.MinValue, double.MaxValue, 0.001, 0, false, false, false);

            FHost.CreateValueFastInput("Width", 1, null, TSliceMode.Single, TPinVisibility.True, out FPinWidth);
            FPinWidth.SetSubType(2, double.MaxValue, 1, 2, false, false, true);

            FHost.CreateValueFastInput("Height", 1, null, TSliceMode.Single, TPinVisibility.True, out FPinHeight);
            FPinHeight.SetSubType(2, double.MaxValue, 1, 2, false, false, true);

            FHost.CreateValueOutput("Vector2D Out", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FOutputPoints);
            FOutputPoints.SetSubType(0, double.MaxValue, 0.001, 0, false, false, false); 
            
            FHost.CreateValueOutput("Hit Tester", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FOutputHit);
            FOutputHit.SetSubType(0, double.MaxValue, 0.001, 0, false, false, false); 
        }

        #endregion pin creation

        #region mainloop

        public void Configurate(IPluginConfig Input)
        {
            // ... 
        }

        //here we go, thats the method called by vvvv each frame
        //all data handling should be in here
        public void Evaluate(int SpreadMax)
        {
            double width; FPinWidth.GetValue(0, out width);
            double height; FPinHeight.GetValue(0, out height);

            // if something changed, recreate Trafo
            if (FInputGridFrom.PinIsChanged || FInputGridTo.PinIsChanged || width != FWidth || height != FHeight)
            {
                // prepare GridTransformer
                FTrafo = new GridTransformer();

                int i, j;
                Point2D p1, p2, p3;

                FWidth = (int)width;
                FHeight = (int)height;

                // check Input ranges
                FBufSize = (int)height * (int)width * 2;
                if (FInputGridFrom.SliceCount != FBufSize || FInputGridTo.SliceCount != FBufSize) return;
                               

                // loop through points and create triangles
                for (i = 0; i < FHeight - 1; ++i)
                    for (j=0; j<FWidth - 1; ++j)
                    {
                        int index = j + i * FWidth;

                        // upper triangle
                        FInputGridFrom.GetValue2D(index, out p1.x, out p1.y);
                        FInputGridFrom.GetValue2D(index + 1, out p2.x, out p2.y);
                        FInputGridFrom.GetValue2D(index + FWidth, out p3.x, out p3.y);
                        Triangle triFrom1 = new Triangle(p1, p2, p3);

                        FInputGridTo.GetValue2D(index, out p1.x, out p1.y);
                        FInputGridTo.GetValue2D(index + 1, out p2.x, out p2.y);
                        FInputGridTo.GetValue2D(index + FWidth, out p3.x, out p3.y);
                        Triangle triTo1 = new Triangle(p1, p2, p3);

                        FTrafo.Insert(triFrom1, triTo1);

                        // lower triangle
                        FInputGridFrom.GetValue2D(index + 1, out p1.x, out p1.y);
                        FInputGridFrom.GetValue2D(index + FWidth, out p2.x, out p2.y);
                        FInputGridFrom.GetValue2D(index + FWidth + 1, out p3.x, out p3.y);
                        Triangle triFrom2 = new Triangle(p1, p2, p3);

                        FInputGridTo.GetValue2D(index + 1, out p1.x, out p1.y);
                        FInputGridTo.GetValue2D(index + FWidth, out p2.x, out p2.y);
                        FInputGridTo.GetValue2D(index + FWidth + 1, out p3.x, out p3.y);
                        Triangle triTo2 = new Triangle(p1, p2, p3);

                        FTrafo.Insert(triFrom2, triTo2);
                    }
            }

            ///////////////////////////////////////////////////////
            // do transformation
            
            // prepare data
            int sliceCount = FInputPoints.SliceCount;
            Point2D pIn, pOut;
            List<Point2D> pList = new List<Point2D>();
            int[] hitter;
			hitter = new int[sliceCount / 2];
			int number = (sliceCount / 2);

            // loop throug input points and calc Transformation
            for (int i = 0; i < sliceCount / 2; ++i)
            {
                FInputPoints.GetValue2D(i, out pIn.x, out pIn.y);
                if (FTrafo.Transform(pIn, out pOut))   // inside ?
                {pList.Add(pOut);
                	hitter[i] = 1;
                } else
                {
                	hitter[i] = 0;
                }
                   
            }

            // set final slicecount
            FOutputPoints.SliceCount = pList.Count * 2;
            FOutputHit.SliceCount = number;
            // set output
            for (int i=0; i<pList.Count; ++i){
                FOutputPoints.SetValue2D(i, pList[i].x, pList[i].y);
           	    FOutputHit.SetValue(i, hitter[i]);
            }
            for (int i=0; i<number; ++i){
               
           	    FOutputHit.SetValue(i, hitter[i]);
            }
            
        }
      

        #endregion mainloop
    }

}
