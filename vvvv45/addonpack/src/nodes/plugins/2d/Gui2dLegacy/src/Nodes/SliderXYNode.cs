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
	public class SliderXYNode: BasicGui2dNode, IPlugin
	{
		#region field declaration

		//additional slider size pin
		private IValueIn FSizeSliderIn;
		private IValueIn FSliderSpeedIn;
		
		//additional slider color pin
		private IColorIn FSliderColorIn;
		
		#endregion field declaration
		
		#region constructor/destructor
    	
        public SliderXYNode()
        {
			//the nodes constructor
			//nothing to declare for this node
		}
        
        #endregion constructor/destructor
		
		#region node name and infos

		//provide node infos
		new public static IPluginInfo PluginInfo
		{
			get
			{
				//fill out nodes info
				IPluginInfo Info = new PluginInfo();
				Info.Name = "SliderXY";
				Info.Category = "GUI";
				Info.Version = "Legacy";
				Info.Help = "A spread of xy-slider groups";
				Info.Tags = "EX9, DX9, transform, interaction, mouse, fader";
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
		public override void SetPluginHost(IPluginHost Host)
		{
			
			//assign host
			FHost = Host;

			//create inputs:
			
			//transform
			FHost.CreateTransformInput("Transform In", TSliceMode.Dynamic, TPinVisibility.True, out FTransformIn);
			
			//value
			FHost.CreateValueInput("Value Input ", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out FValueIn);
			FValueIn.SetSubType2D(0, 1, 0.01, 0, 0, false, false, false);
			
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
			
			FHost.CreateValueInput("Size Slider", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSizeSliderIn);
			FSizeSliderIn.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.02, false, false, false);
			
			FHost.CreateColorInput("Slider Color", TSliceMode.Dynamic, TPinVisibility.True, out FSliderColorIn);
			FSliderColorIn.SetSubType(new RGBAColor(1, 1, 1, 1), true);
			
			FHost.CreateValueInput("Slider Speed", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSliderSpeedIn);
			FSliderSpeedIn.SetSubType(0, double.MaxValue, 0.01, 1, false, false, false);


			//create outputs
			FHost.CreateTransformOutput("Transform Out", TSliceMode.Dynamic, TPinVisibility.True, out FTransformOut);
			
			FHost.CreateColorOutput("Color", TSliceMode.Dynamic, TPinVisibility.True, out FColorOut);
			FColorOut.SetSubType(new RGBAColor(0.2, 0.2, 0.2, 1), true);
			
			FHost.CreateValueOutput("Value Output ", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out FValueOut);
			FValueOut.SetSubType2D(0, 1, 0.01, 0, 0, false, false, false);
			
			FHost.CreateValueOutput("Active", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FActiveOut);
			FActiveOut.SetSubType(0, 1, 1, 0, false, true, false);
			
			FHost.CreateValueOutput("Hit", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FHitOut);
			FHitOut.SetSubType(0, 1, 1, 0, true, false, false);
			
			FHost.CreateValueOutput("Mouse Over", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMouseOverOut);
			FMouseOverOut.SetSubType(0, 1, 1, 0, false, true, false);
			
			FHost.CreateValueOutput("Spread Counts", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSpreadCountsOut);
			FSpreadCountsOut.SetSubType(0, double.MaxValue, 0.01, 0, false, false, true);
			
			//create config pin
			FHost.CreateValueConfig("Internal Value ", 2, null, TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out FInternalValueConfig);
			FInternalValueConfig.SetSubType2D(0, 1, 0.01, 0, 0, false, false, false);
			
			FControllerGroups = new ArrayList();
			
		}
		
		#endregion pin creation
		
		#region mainloop
		
		public override void Evaluate(int SpreadMax)
		{
			
			//calc input spreadcount
			int inputSpreadCount = GetSpreadMax();
			
			//create or delete button groups
			int diff = inputSpreadCount - FControllerGroups.Count;
			if (diff > 0)
			{
				for (int i=0; i<diff; i++)
				{
					FControllerGroups.Add(new SliderXYGroup());
				}
			}
			else if (diff < 0)
			{
				for (int i=0; i< -diff; i++)
				{
					FControllerGroups.RemoveAt(FControllerGroups.Count-1-i);
				}
			}
			
			//update parameters
			int slice;
			if (   FCountXIn.PinIsChanged
			    || FCountYIn.PinIsChanged
			    || FSizeXIn.PinIsChanged
			    || FSizeYIn.PinIsChanged
			    || FSizeSliderIn.PinIsChanged
			    || FTransformIn.PinIsChanged
			    || FColorIn.PinIsChanged
			    || FOverColorIn.PinIsChanged
			    || FActiveColorIn.PinIsChanged
			    || FSliderColorIn.PinIsChanged
			    || FSliderSpeedIn.PinIsChanged)
			{
				
				for (slice = 0; slice < inputSpreadCount; slice++)
				{
					SliderXYGroup group = (SliderXYGroup) FControllerGroups[slice];
					
					Matrix4x4 trans;
					Vector2D count, size;
					double sizeSlider, sliderSpeed;
					RGBAColor col, over, active, slider;
					
					FTransformIn.GetMatrix(slice, out trans);
					FCountXIn.GetValue(slice, out count.x);
					FCountYIn.GetValue(slice, out count.y);
					FSizeXIn.GetValue(slice, out size.x);
					FSizeYIn.GetValue(slice, out size.y);
					FSizeSliderIn.GetValue(slice, out sizeSlider);
					FColorIn.GetColor(slice, out col);
					FOverColorIn.GetColor(slice, out over);
					FActiveColorIn.GetColor(slice, out active);
					FSliderColorIn.GetColor(slice, out slider);
					FSliderSpeedIn.GetValue(slice, out sliderSpeed);

					group.UpdateTransform(trans, count, size, sizeSlider, col, over, active, slider, sliderSpeed);
					
				}
			}
			
			//get spread counts
			int outcount = 0;
			FSpreadCountsOut.SliceCount = inputSpreadCount;
			
			for (slice = 0; slice < inputSpreadCount; slice++)
			{
				SliderXYGroup group = (SliderXYGroup) FControllerGroups[slice];
				
				outcount += group.FControllers.Length;
				FSpreadCountsOut.SetValue(slice, group.FControllers.Length);
				
			}
			
			//update mouse and colors
			bool valueSet = false;
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
				
				bool mouseDown = mouseLeft >= 0.5;
				bool mousDownEdge = mouseDown && !FLastMouseLeft;
				
				for (slice = 0; slice < inputSpreadCount; slice++)
				{
					
					SliderXYGroup group = (SliderXYGroup) FControllerGroups[slice];
					valueSet |= group.UpdateMouse(mouse, mousDownEdge, mouseDown);
						
				}
				
				FLastMouseLeft = mouseDown;
			}
			
			//set value
			slice = 0;
			if (   FValueIn.PinIsChanged
			    || FSetValueIn.PinIsChanged)
			{
				
				for (int i = 0; i < inputSpreadCount; i++)
				{
					SliderXYGroup group = (SliderXYGroup) FControllerGroups[i];
					int pcount = group.FControllers.Length;
					
					for (int j = 0; j < pcount; j++)
					{
						
						Vector2D val;
						
						FSetValueIn.GetValue(slice, out val.x);
						
						if (val.x >= 0.5)
						{
							//update value
							FValueIn.GetValue2D(slice, out val.x, out val.y);
							group.UpdateValue((SliderXY)group.FControllers[j], val);
							
							valueSet = true;
						}
						else if (FFirstframe) 
						{
							//load from config pin on first frame
							FInternalValueConfig.GetValue2D(slice, out val.x, out val.y);
							group.UpdateValue((SliderXY)group.FControllers[j], val);
							
						}
						
						slice++;
					}
				}
			}

			
			//write output to pins
			FValueOut.SliceCount = outcount;
			if (outcount != FInternalValueConfig.SliceCount) FInternalValueConfig.SliceCount = outcount;
			FTransformOut.SliceCount = outcount * 2;
			FColorOut.SliceCount = outcount * 2;
			FHitOut.SliceCount = outcount;
			FActiveOut.SliceCount = outcount;
			FMouseOverOut.SliceCount = outcount;
			
			slice = 0;
			for (int i = 0; i < inputSpreadCount; i++)
			{
				SliderXYGroup group = (SliderXYGroup) FControllerGroups[i];
				int pcount = group.FControllers.Length;
				
				for (int j = 0; j < pcount; j++)
				{
					SliderXY s = (SliderXY) group.FControllers[j];
					
					FTransformOut.SetMatrix(slice * 2, s.Transform);
					FTransformOut.SetMatrix(slice * 2 + 1, s.SliderTransform);
					FColorOut.SetColor(slice * 2, s.CurrentCol);
					FColorOut.SetColor(slice * 2 + 1, s.ColorSlider);
					FValueOut.SetValue2D(slice, s.Value.x, s.Value.y);
					FMouseOverOut.SetValue(slice, s.MouseOver ? 1 : 0);
					FHitOut.SetValue(slice, s.Hit ? 1 : 0);
					FActiveOut.SetValue(slice, s.Active ? 1 : 0);
					
					//update config pin
					if (valueSet)
					{
						Vector2D val;
						FInternalValueConfig.GetValue2D(slice, out val.x, out val.y);
						
						if (VMath.Abs(s.Value - val) > 0.00000001)
							FInternalValueConfig.SetValue2D(slice, s.Value.x, s.Value.y);
					}
					
					slice++;
				}
			}

			//end of frame
			FFirstframe = false;
		}
		
		//calc how many groups are required
		private int GetSpreadMax()
		{
			
			int max = 0;
			
			max = Math.Max(max, FCountXIn.SliceCount);
			max = Math.Max(max, FCountYIn.SliceCount);
			
			max = Math.Max(max, FSizeXIn.SliceCount);
			max = Math.Max(max, FSizeYIn.SliceCount);
			
			max = Math.Max(max, FSizeSliderIn.SliceCount);
			max = Math.Max(max, FSliderSpeedIn.SliceCount);
			
			max = Math.Max(max, FTransformIn.SliceCount);
			max = Math.Max(max, FColorIn.SliceCount);
			max = Math.Max(max, FActiveColorIn.SliceCount);
			max = Math.Max(max, FOverColorIn.SliceCount);
			max = Math.Max(max, FSliderColorIn.SliceCount);
		
			return max;
			
		}
		
		#endregion mainloop
		
	}
	


}



