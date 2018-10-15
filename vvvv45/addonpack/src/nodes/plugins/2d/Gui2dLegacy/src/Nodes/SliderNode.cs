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
	public class SliderNode: BasicGui2dNode, IPlugin
	{
		#region field declaration

		//additional slider size pin
		private IValueIn FIsXSliderIn;
		private IValueIn FSizeSliderIn;
		private IValueIn FSliderSpeedIn;
		
		//additional slider color pin
		private IColorIn FSliderColorIn;
		
		#endregion field declaration
		
		#region constructor/destructor
    	
        public SliderNode()
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
				Info.Name = "Slider";
				Info.Category = "GUI";
				Info.Version = "Legacy";
				Info.Help = "A spread of slider groups";
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
			
			base.SetPluginHost(Host);

			//create inputs:

			//value
			//correct subtype of value pin
			FValueIn.SetSubType(0, 1, 0.01, 0, false, false, false);
			FInternalValueConfig.SetSubType(0, 1, 0.01, 0, false, false, false);
			
			FHost.CreateValueInput("Is X Slider", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FIsXSliderIn);
			FIsXSliderIn.SetSubType(0, 1, 1, 0, false, true, false);
			
			FHost.CreateValueInput("Size Slider", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSizeSliderIn);
			FSizeSliderIn.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.02, false, false, false);
			
			//color
			FHost.CreateColorInput("Slider Color", TSliceMode.Dynamic, TPinVisibility.True, out FSliderColorIn);
			FSliderColorIn.SetSubType(new RGBAColor(1, 1, 1, 1), true);
			
			FHost.CreateValueInput("Slider Speed", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSliderSpeedIn);
			FSliderSpeedIn.SetSubType(0, double.MaxValue, 0.01, 1, false, false, false);
			
			
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
					FControllerGroups.Add(new SliderGroup());
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
			    || FIsXSliderIn.PinIsChanged
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
					SliderGroup group = (SliderGroup) FControllerGroups[slice];
					
					Matrix4x4 trans;
					Vector2D count, size;
					double sizeSlider, sliderSpeed, isX;
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
					FIsXSliderIn.GetValue(slice, out isX);

					group.UpdateTransform(trans, count, size, sizeSlider, col, over, active, slider, sliderSpeed, isX >= 0.5);
					
				}
			}
			
			//get spread counts
			int outcount = 0;
			FSpreadCountsOut.SliceCount = inputSpreadCount;
			
			for (slice = 0; slice < inputSpreadCount; slice++)
			{
				SliderGroup group = (SliderGroup) FControllerGroups[slice];
				
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
					
					SliderGroup group = (SliderGroup) FControllerGroups[slice];
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
					SliderGroup group = (SliderGroup) FControllerGroups[i];
					int pcount = group.FControllers.Length;
					
					for (int j = 0; j < pcount; j++)
					{
						
						double val;
						
						FSetValueIn.GetValue(slice, out val);
						
						if (val >= 0.5)
						{
							//update value
							FValueIn.GetValue(slice, out val);
							group.UpdateValue((Slider)group.FControllers[j], val);
							
							valueSet = true;
						}
						else if (FFirstframe) 
						{
							//load from config pin on first frame
							FInternalValueConfig.GetValue(slice, out val);
							group.UpdateValue((Slider)group.FControllers[j], val);
							
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
				SliderGroup group = (SliderGroup) FControllerGroups[i];
				int pcount = group.FControllers.Length;
				
				for (int j = 0; j < pcount; j++)
				{
					Slider s = (Slider) group.FControllers[j];
					
					FTransformOut.SetMatrix(slice * 2, s.Transform);
					FTransformOut.SetMatrix(slice * 2 + 1, s.SliderTransform);
					FColorOut.SetColor(slice * 2, s.CurrentCol);
					FColorOut.SetColor(slice * 2 + 1, s.ColorSlider);
					FValueOut.SetValue(slice, s.Value);
					FMouseOverOut.SetValue(slice, s.MouseOver ? 1 : 0);
					FHitOut.SetValue(slice, s.Hit ? 1 : 0);
					FActiveOut.SetValue(slice, s.Active ? 1 : 0);
					
					//update config pin
					if (valueSet)
					{
						double val;
						FInternalValueConfig.GetValue(slice, out val);
						
						if (Math.Abs(s.Value - val) > 0.00000001)
							FInternalValueConfig.SetValue(slice, s.Value);
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
			max = Math.Max(max, FIsXSliderIn.SliceCount);
			
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

