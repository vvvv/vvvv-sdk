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
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.Utils.VColor;


namespace VVVV.Nodes
{
	[PluginInfo(Name = "RotarySlider",
	            Category = "GUI",
	            Help = "A spread of rotary knob groups",
	            Tags = "multitouch, MT, EX9, DX9, transform, interaction, mouse, fader",
	            Author = "tonfilm")]
	public class RotarySliderNode: BasicGui2dSliderNode
	{

		[Input("Value Input")]
		IDiffSpread<double> FValueIn;

        //autosave
        [Input("Autosave", IsSingle = true)]
        ISpread<bool> FAutoSave;
		
		[Output("Value Output")]
		ISpread<double> FValueOut;
		
		//this is to store the values with the patch
		[Config("Internal Value")]
		IDiffSpread<double> FInternalValueConfig;

        
		
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
					FControllerGroups.Add(new RotarySliderGroup());
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
			if (   AnyParameterPinChanged()
			    || FSliderColorIn.IsChanged
			    || FSliderSpeedIn.IsChanged)
			{
				
				for (slice = 0; slice < inputSpreadCount; slice++)
				{
					RotarySliderGroup group = (RotarySliderGroup) FControllerGroups[slice];
					group.UpdateTransform(FTransformIn[slice], FCountIn[slice], FSizeIn[slice], FColorIn[slice], FOverColorIn[slice], FActiveColorIn[slice], FSliderColorIn[slice], FSliderSpeedIn[slice]);
				}
			}
			
			//get spread counts
			int outcount = 0;
			FSpreadCountsOut.SliceCount = inputSpreadCount;
			
			for (slice = 0; slice < inputSpreadCount; slice++)
			{
				RotarySliderGroup group = (RotarySliderGroup) FControllerGroups[slice];
				
				outcount += group.FControllers.Length;
				FSpreadCountsOut[slice] = group.FControllers.Length;
				
			}
			
			//update mouse and colors
			bool valueSet = UpdateMouse<RotarySliderGroup, RotarySlider>(inputSpreadCount);
			
			//set value
			slice = 0;
			if (   FValueIn.IsChanged
			    || FSetValueIn.IsChanged)
			{
				
				for (int i = 0; i < inputSpreadCount; i++)
				{
					RotarySliderGroup group = (RotarySliderGroup) FControllerGroups[i];
					int pcount = group.FControllers.Length;
					
					for (int j = 0; j < pcount; j++)
					{

						if (FSetValueIn[slice])
						{
							//update value
							group.UpdateValue((RotarySlider)group.FControllers[j], FValueIn[slice]);
							valueSet = true;
						}
						else if (FFirstframe) 
						{
							//load from config pin on first frame
							group.UpdateValue((RotarySlider)group.FControllers[j], FInternalValueConfig[slice]);
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
				RotarySliderGroup group = (RotarySliderGroup) FControllerGroups[i];
				int pcount = group.FControllers.Length;
				
				for (int j = 0; j < pcount; j++)
				{
					RotarySlider s = (RotarySlider) group.FControllers[j];
					
					FTransformOut[slice * 2] = s.Transform;
					FTransformOut[slice * 2 + 1] = s.Transform;
					FColorOut[slice * 2] = s.CurrentCol;
					FColorOut[slice * 2 + 1] = s.ColorSlider;
					FValueOut[slice] = s.Value;
					FMouseOverOut[slice] = s.GetAndResetMouseOver();
					FHitOut[slice] = s.Hit;
					FActiveOut[slice] = s.Active;
					
					//update config pin
                    if (valueSet && FAutoSave[0])
					{
						if (Math.Abs(s.Value - FInternalValueConfig[slice]) > 0.000000001)
							FInternalValueConfig[slice] = s.Value;
					}
					
					slice++;
				}
			}

			//end of frame
			FFirstframe = false;
		}
		
		#endregion mainloop
		
	}
	


}

