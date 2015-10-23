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


namespace VVVV.Nodes
{
	//parent class for gu2d nodes
	public class BasicGui2dNode: IDisposable
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
		protected IValueIn FMouseXIn;
		protected IValueIn FMouseYIn;
		protected IValueIn FLeftButtonIn;
		protected IColorIn FColorIn;
		protected IColorIn FOverColorIn;
		protected IColorIn FActiveColorIn;
		
		//this is to store the values with the patch
		protected IValueConfig FInternalValueConfig;
		
		//output pin declaration
		protected ITransformOut FTransformOut;
		protected IColorOut FColorOut;
		protected IValueOut FValueOut;
		protected IValueOut FHitOut;
		protected IValueOut FActiveOut;
		protected IValueOut FMouseOverOut;
		protected IValueOut FSpreadCountsOut;
		
		protected ArrayList FControllerGroups;
		protected bool FLastMouseLeft = false;
		protected bool FFirstframe = true;
		
		#endregion field declaration
		
		#region constructor/destructor
    	
        public BasicGui2dNode()
        {
			//the nodes constructor
			//nothing to declare for this node
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
        			// Dispose managed resources.
        		}
        		// Release unmanaged resources. If disposing is false,
        		// only the following code is executed.
	        	
        		FHost.Log(TLogType.Debug, "Plugin is being deleted");
        		
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
        ~BasicGui2dNode()
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
				Info.Name = "Basic";
				Info.Category = "GUI";
				Info.Version = "Legacy";
				Info.Help = "A spread of radio buttons";
				Info.Tags = "EX9, DX9, transform, interaction, mouse";
				Info.Author = "tonfilm";
				Info.Bugs = "";
				Info.Credits = "";
				Info.Warnings = "";
				
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
		
		#endregion node name and infos
		
		#region pin creation
		
		//this method is called by vvvv when the node is created
		public virtual void SetPluginHost(IPluginHost Host)
		{
			//assign host
			FHost = Host;

			//create inputs:
			
			//transform
			FHost.CreateTransformInput("Transform In", TSliceMode.Dynamic, TPinVisibility.True, out FTransformIn);
			
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
			FHost.CreateValueInput("Mouse X", 1, null, TSliceMode.Single, TPinVisibility.True, out FMouseXIn);
			FMouseXIn.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
			
			FHost.CreateValueInput("Mouse Y", 1, null, TSliceMode.Single, TPinVisibility.True, out FMouseYIn);
			FMouseYIn.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
			
			FHost.CreateValueInput("Mouse Left", 1, null, TSliceMode.Single, TPinVisibility.True, out FLeftButtonIn);
			FLeftButtonIn.SetSubType(0, 1, 1, 0, false, true, false);
			
			//color
			FHost.CreateColorInput("Color", TSliceMode.Dynamic, TPinVisibility.True, out FColorIn);
			FColorIn.SetSubType(new RGBAColor(0.2, 0.2, 0.2, 1), true);
			
			FHost.CreateColorInput("Mouse Over Color", TSliceMode.Dynamic, TPinVisibility.True, out FOverColorIn);
			FOverColorIn.SetSubType(new RGBAColor(0.5, 0.5, 0.5, 1), true);
			
			FHost.CreateColorInput("Activated Color", TSliceMode.Dynamic, TPinVisibility.True, out FActiveColorIn);
			FActiveColorIn.SetSubType(new RGBAColor(1, 1, 1, 1), true);


			//create outputs
			FHost.CreateTransformOutput("Transform Out", TSliceMode.Dynamic, TPinVisibility.True, out FTransformOut);
			
			FHost.CreateColorOutput("Color", TSliceMode.Dynamic, TPinVisibility.True, out FColorOut);
			FColorOut.SetSubType(new RGBAColor(0.2, 0.2, 0.2, 1), true);
			
			FHost.CreateValueOutput("Value Output", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FValueOut);
			FValueOut.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
			
			FHost.CreateValueOutput("Active", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FActiveOut);
			FActiveOut.SetSubType(0, 1, 1, 0, false, true, false);
			
			FHost.CreateValueOutput("Hit", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FHitOut);
			FHitOut.SetSubType(0, 1, 1, 0, true, false, false);
			
			FHost.CreateValueOutput("Mouse Over", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMouseOverOut);
			FMouseOverOut.SetSubType(0, 1, 1, 0, false, true, false);
			
			FHost.CreateValueOutput("Spread Counts", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSpreadCountsOut);
			FSpreadCountsOut.SetSubType(0, double.MaxValue, 0.01, 0, false, false, true);
			
			//create config pin
			FHost.CreateValueConfig("Internal Value", 1, null, TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out FInternalValueConfig);
			FInternalValueConfig.SetSubType(0, double.MaxValue, 1, 0, false, false, true);
			
			FControllerGroups = new ArrayList();
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
			//nothing to configure in this plugin
			//only used in conjunction with inputs of type cmpdConfigurate
		}
		
		#endregion mainloop
	}
	


}

