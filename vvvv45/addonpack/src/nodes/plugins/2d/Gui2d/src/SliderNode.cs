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
using System.Threading;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;
using VVVV.Utils.VColor;


namespace VVVV.Nodes
{
	public class SliderNode: IPlugin, IDisposable
	{
		#region field declaration

		//the host
		private IPluginHost FHost;
		// Track whether Dispose has been called.
   		private bool FDisposed = false;
		
		//input pin declaration
		private ITransformIn FTransformIn;
		private IValueIn FValueIn;
		private IValueIn FSetValueIn;
		private IValueIn FPosXIn;
		private IValueIn FPosYIn;
		private IValueIn FScaleXIn;
		private IValueIn FScaleYIn;
		private IValueIn FCountXIn;
		private IValueIn FCountYIn;
		private IValueIn FSizeXIn;
		private IValueIn FSizeYIn;
		private IValueIn FSizeSliderIn;
		private IValueIn FMouseXIn;
		private IValueIn FMouseYIn;
		private IValueIn FLeftButtonIn;
		private IColorIn FColorIn;
		private IColorIn FOverColorIn;
		private IColorIn FActiveColorIn;
		private IColorIn FSliderColorIn;
		
		//output pin declaration
		private ITransformOut FTransformOut;
		private IColorOut FColorOut;
		private IValueOut FValueOut;
		private IValueOut FHitOut;
		private IValueOut FMouseOverOut;
		private IValueOut FSpreadCountsOut;
		
		private ArrayList FSlidersList;
		private bool FLastMouseLeft = false;
		
		#endregion field declaration
		
		#region constructor/destructor
    	
        public SliderNode()
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
        ~SliderNode()
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
				Info.Name = "Slider";
				Info.Category = "2d GUI";
				Info.Version = "";
				Info.Help = "A spread of slider groups";
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
		public void SetPluginHost(IPluginHost Host)
		{
			//assign host
			FHost = Host;

			//create inputs:
			
			//transform
			FHost.CreateTransformInput("Tarnsform In", TSliceMode.Dynamic, TPinVisibility.True, out FTransformIn);
			
			//value
			FHost.CreateValueInput("Value Input", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FValueIn);
			FValueIn.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
			
			FHost.CreateValueInput("Set Value", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSetValueIn);
			FSetValueIn.SetSubType(0, 1, 1, 0, true, false, false);
			
			//position
			FHost.CreateValueInput("Pos X", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FPosXIn);
			FPosXIn.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
			
			FHost.CreateValueInput("Pos Y", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FPosYIn);
			FPosYIn.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
			
			//scaling
			FHost.CreateValueInput("Scale X", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FScaleXIn);
			FScaleXIn.SetSubType(double.MinValue, double.MaxValue, 0.01, 1, false, false, false);
			
			FHost.CreateValueInput("Scale Y", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FScaleYIn);
			FScaleYIn.SetSubType(double.MinValue, double.MaxValue, 0.01, 1, false, false, false);
			
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
			
			FHost.CreateValueInput("Size Slider", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSizeSliderIn);
			FSizeSliderIn.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.02, false, false, false);
			
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
			FActiveColorIn.SetSubType(new RGBAColor(0.3, 0.3, 0.3, 1), true);
			
			FHost.CreateColorInput("Slider Color", TSliceMode.Dynamic, TPinVisibility.True, out FSliderColorIn);
			FSliderColorIn.SetSubType(new RGBAColor(1, 1, 1, 1), true);


			//create outputs
			FHost.CreateTransformOutput("Transform Out", TSliceMode.Dynamic, TPinVisibility.True, out FTransformOut);
			
			FHost.CreateColorOutput("Color", TSliceMode.Dynamic, TPinVisibility.True, out FColorOut);
			FColorOut.SetSubType(new RGBAColor(0.2, 0.2, 0.2, 1), true);
			
			FHost.CreateValueOutput("Value Output", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FValueOut);
			FValueOut.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
			
			FHost.CreateValueOutput("Hit", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FHitOut);
			FHitOut.SetSubType(0, 1, 1, 0, true, false, false);
			
			FHost.CreateValueOutput("Mouse Over", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMouseOverOut);
			FMouseOverOut.SetSubType(0, 1, 1, 0, false, true, false);
			
			FHost.CreateValueOutput("Spread Counts", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSpreadCountsOut);
			FSpreadCountsOut.SetSubType(0, double.MaxValue, 0.01, 0, false, false, true);
			
			FSlidersList = new ArrayList();
			
		}
		
		#endregion pin creation
		
		#region mainloop
		
		public void Evaluate(int SpreadMax)
		{
			
			//calc input spreadcount
			int inputSpreadCount = GetSpreadMax();
			
			//create or delete button groups
			int diff = inputSpreadCount - FSlidersList.Count;
			if (diff > 0)
			{
				for (int i=0; i<diff; i++)
				{
					FSlidersList.Add(new SliderGroup());
				}
			}
			else if (diff < 0)
			{
				for (int i=0; i< -diff; i++)
				{
					FSlidersList.RemoveAt(FSlidersList.Count-1-i);
				}
			}
			
			//update parameters
			int slice;
			if (   FPosXIn.PinIsChanged
			    || FPosYIn.PinIsChanged
			    || FScaleXIn.PinIsChanged
			    || FScaleYIn.PinIsChanged
			    || FCountXIn.PinIsChanged
			    || FCountYIn.PinIsChanged
			    || FSizeXIn.PinIsChanged
			    || FSizeYIn.PinIsChanged
			    || FSizeSliderIn.PinIsChanged
			    || FTransformIn.PinIsChanged
			    || FColorIn.PinIsChanged
			    || FOverColorIn.PinIsChanged
			    || FActiveColorIn.PinIsChanged
			    || FSliderColorIn.PinIsChanged)
			{
				
				for (slice = 0; slice < inputSpreadCount; slice++)
				{
					SliderGroup group = (SliderGroup) FSlidersList[slice];
					
					Matrix4x4 trans;
					Vector2D pos, scale, count, size;
					double sizeSlider;
					RGBAColor col, over, active, slider;
					
					FTransformIn.GetMatrix(slice, out trans);
					FPosXIn.GetValue(slice, out pos.x);
					FPosYIn.GetValue(slice, out pos.y);
					FScaleXIn.GetValue(slice, out scale.x);
					FScaleYIn.GetValue(slice, out scale.y);
					FCountXIn.GetValue(slice, out count.x);
					FCountYIn.GetValue(slice, out count.y);
					FSizeXIn.GetValue(slice, out size.x);
					FSizeYIn.GetValue(slice, out size.y);
					FSizeSliderIn.GetValue(slice, out sizeSlider);
					FColorIn.GetColor(slice, out col);
					FOverColorIn.GetColor(slice, out over);
					FActiveColorIn.GetColor(slice, out active);
					FSliderColorIn.GetColor(slice, out slider);

					group.UpdateTransform(trans, pos, scale, count, size, sizeSlider, col, over, active, slider);
					
				}
			}
			
			//update mouse and colors
			if (   FMouseXIn.PinIsChanged
			    || FMouseYIn.PinIsChanged
			    || FLeftButtonIn.PinIsChanged
			    || FColorIn.PinIsChanged
			    || FOverColorIn.PinIsChanged
			    || FActiveColorIn.PinIsChanged
			    || FSliderColorIn.PinIsChanged
			    || FCountXIn.PinIsChanged
			    || FCountYIn.PinIsChanged
			    || FLastMouseLeft)
			{
				
				Vector2D mouse;
				double mouseLeft;
				
				FMouseXIn.GetValue(0, out mouse.x);
				FMouseYIn.GetValue(0, out mouse.y);
				FLeftButtonIn.GetValue(0, out mouseLeft);
				
				for (slice = 0; slice < inputSpreadCount; slice++)
				{
					
					SliderGroup group = (SliderGroup) FSlidersList[slice];
					group.UpdateMouse(mouse, (mouseLeft >= 0.5) && !FLastMouseLeft, mouseLeft >= 0.5);
						
				}
				
				FLastMouseLeft = mouseLeft >= 0.5;
			}
			
			//set value
			slice = 0;
			if (   FValueIn.PinIsChanged
			    || FSetValueIn.PinIsChanged)
			{
				
				for (int i = 0; i < inputSpreadCount; i++)
				{
					SliderGroup group = (SliderGroup) FSlidersList[i];
					int pcount = group.Sliders.Length;
					
					for (int j = 0; j < pcount; j++)
					{
						
						double val;
						
						FSetValueIn.GetValue(slice, out val);
						
						if (val >= 0.5)
						{
							//update value
							FValueIn.GetValue(slice, out val);
							
							group.UpdateValue(group.Sliders[j], val);
							group.Sliders[j].Hit = true;
						}
						else
						{
							group.Sliders[j].Hit = false;
						}
						
						slice++;
					}
				}
			}

			//get spread counts
			int outcount = 0;
			FSpreadCountsOut.SliceCount = inputSpreadCount;
			
			for (slice = 0; slice < inputSpreadCount; slice++)
			{
				SliderGroup group = (SliderGroup) FSlidersList[slice];
				
				outcount += group.Sliders.Length;
				FSpreadCountsOut.SetValue(slice, group.Sliders.Length);
				
			}
			
			
			//write output to pins
			FValueOut.SliceCount = outcount;
			FTransformOut.SliceCount = outcount * 2;
			FColorOut.SliceCount = outcount * 2;
			FHitOut.SliceCount = outcount;
			FMouseOverOut.SliceCount = outcount;
			
			slice = 0;
			for (int i = 0; i < inputSpreadCount; i++)
			{
				SliderGroup group = (SliderGroup) FSlidersList[i];
				int pcount = group.Sliders.Length;
				
				for (int j = 0; j < pcount; j++)
				{
					Slider s = (Slider) group.Sliders[j];
					
					FTransformOut.SetMatrix(slice * 2, s.Transform);
					FTransformOut.SetMatrix(slice * 2 + 1, s.SliderTransform);
					FColorOut.SetColor(slice * 2, s.Color);
					FColorOut.SetColor(slice * 2 + 1, s.ColorSlider);
					FValueOut.SetValue(slice, s.Value);
					FMouseOverOut.SetValue(slice, s.MouseOver ? 1 : 0);
					FHitOut.SetValue(slice, s.Hit ? 1 : 0);
					
					slice++;
				}
			}

		}
		
		private int GetSpreadMax()
		{
			
			int max = 0;
			
			max = Math.Max(max, FPosXIn.SliceCount);
			max = Math.Max(max, FPosYIn.SliceCount);
			
			max = Math.Max(max, FScaleXIn.SliceCount);
			max = Math.Max(max, FScaleYIn.SliceCount);
			
			max = Math.Max(max, FCountXIn.SliceCount);
			max = Math.Max(max, FCountYIn.SliceCount);
			
			max = Math.Max(max, FSizeXIn.SliceCount);
			max = Math.Max(max, FSizeYIn.SliceCount);
			
			max = Math.Max(max, FSizeSliderIn.SliceCount);
			
			max = Math.Max(max, FTransformIn.SliceCount);
			max = Math.Max(max, FColorIn.SliceCount);
			max = Math.Max(max, FActiveColorIn.SliceCount);
			max = Math.Max(max, FOverColorIn.SliceCount);
			max = Math.Max(max, FSliderColorIn.SliceCount);
		
			return max;
			
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

