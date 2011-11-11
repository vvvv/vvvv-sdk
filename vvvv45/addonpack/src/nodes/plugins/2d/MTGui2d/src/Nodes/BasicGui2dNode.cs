#region licence/info

//////project name
//2d gui nodes

//////description
//nodes to build 2d guis in a EX9 renderer

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop 

//////dependencies
//VVVV.PluginInterfaces.V1;
//VVVV.Utils.VMath;

//////initial author
//tonfilm

#endregion licence/info

//use what you need
using System;
using System.Collections;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;
using VVVV.Utils.VColor;
using VVVV.Nodes.src.Controllers;


namespace VVVV.Nodes
{
	//parent class for gu2d nodes
    public class MTBasicGui2dNode<T,U> where T : BasicGui2dGroup<U> where U : AbstractMTGui2dController,new()
	{
		#region field declaration

		//the host
		protected IPluginHost FHost;
		// Track whether Dispose has been called.
   		protected bool FDisposed = false;
		
		//input pin declaration
		protected ITransformIn FTransformIn;
		protected IValueIn FValueIn;
		protected IValueIn FSetValueIn;
		protected IValueIn FCountXIn;
		protected IValueIn FCountYIn;
		protected IValueIn FSizeXIn;
		protected IValueIn FSizeYIn;
        protected IValueIn FPinInTouchId;
        protected IValueIn FPinInTouchPos;
        protected IValueIn FPinInIsNew;
				
		//output pin declaration
		protected ITransformOut FTransformOut;
		protected IValueOut FValueOut;
		protected IValueOut FHitOut;
		protected IValueOut FSpreadCountsOut;
		
		protected List<T> FControllerGroups;
		protected bool FFirstframe = true;
		
		#endregion field declaration
		
		#region constructor/destructor   	
        public MTBasicGui2dNode()
        {
			//the nodes constructor
			//nothing to declare for this node
		}
        #endregion constructor/destructor
				
		#region pin creation
		
		//this method is called by vvvv when the node is created
		public virtual void SetPluginHost(IPluginHost Host)
		{
			//assign host
			FHost = Host;

			//create inputs:
			
			//transform
			FHost.CreateTransformInput("Transform In", TSliceMode.Dynamic, TPinVisibility.True, out FTransformIn);
            //FHost.CreateValueInput("Transform In",1,null, TSliceMode.Dynamic, TPinVisibility.True, out FTransformIn);
			//value
			FHost.CreateValueInput("Value Input", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FValueIn);
			FValueIn.SetSubType(0, double.MaxValue, 1, 0, false, false, true);
			
			FHost.CreateValueInput("Set Value", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSetValueIn);
			FSetValueIn.SetSubType(0, 1, 1, 0, true, false, false);
			
			//counts
			FHost.CreateValueInput("Count X", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FCountXIn);
			FCountXIn.SetSubType(1, double.MaxValue, 1, 1, false, false, true);
			
			FHost.CreateValueInput("Count Y", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FCountYIn);
			FCountYIn.SetSubType(1, double.MaxValue, 1, 1, false, false, true);
			
			//size
			FHost.CreateValueInput("Size X", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSizeXIn);
			FSizeXIn.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.9, false, false, false);
			
			FHost.CreateValueInput("Size Y", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSizeYIn);
			FSizeYIn.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.9, false, false, false);
			
			//mouse
            this.FHost.CreateValueInput("Touch Id", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInTouchId);
            this.FPinInTouchId.SetSubType(double.MinValue, double.MaxValue, 1, 0, false, false, true);
  
            this.FHost.CreateValueInput("Touch Position", 2, null , TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInTouchPos);
            this.FPinInTouchPos.SetSubType2D(double.MinValue, double.MaxValue, 0.01, 0, 0, false, false, false);
           
            this.FHost.CreateValueInput("Is new", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInIsNew);
            this.FPinInIsNew.SetSubType(0, 1, 1, 0, false, true, false);
          

			//create outputs
			FHost.CreateTransformOutput("Transform Out", TSliceMode.Dynamic, TPinVisibility.True, out FTransformOut);
			
			
			FHost.CreateValueOutput("Value Output", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FValueOut);
			FValueOut.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
			
			FHost.CreateValueOutput("Hit", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FHitOut);
			FHitOut.SetSubType(0, 1, 1, 0, true, false, false);
						
			FHost.CreateValueOutput("Spread Counts", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSpreadCountsOut);
			FSpreadCountsOut.SetSubType(0, double.MaxValue, 0.01, 0, false, false, true);
					
			FControllerGroups = new List<T>();
			
		}
		
		#endregion pin creation
		
		#region mainloop
		
		public virtual void Evaluate(int SpreadMax)
		{
		}
		
		
		//configuration functions:
		public bool AutoEvaluate
		{
			//return true if this node needs to calculate every frame even if nobody asks for its output
			get {return false;}
		}
		
		public void Configurate(IPluginConfig Input)
		{

		}
		
		#endregion mainloop

        protected void UpdateTouches(int inputSpreadCount)
        {
            TouchList touches = new TouchList();
            for (int i = 0; i < this.FPinInTouchId.SliceCount; i++)
            {
                double id, x, y, n;
                this.FPinInTouchId.GetValue(i, out id);
                this.FPinInTouchPos.GetValue2D(i, out x, out y);
                this.FPinInIsNew.GetValue(i, out n);
                Touch t = new Touch();
                t.Id = Convert.ToInt32(id);
                t.X = x;
                t.Y = y;
                t.IsNew = n >= 0.5;
                touches.Add(t);
            }

            for (int slice = 0; slice < inputSpreadCount; slice++)
            {
                T group = FControllerGroups[slice];
                group.UpdateTouches(touches);
            }
        }
		

	}
	


}

