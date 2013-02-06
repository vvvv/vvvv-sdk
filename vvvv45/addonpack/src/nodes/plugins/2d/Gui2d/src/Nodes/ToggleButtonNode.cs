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
	[PluginInfo(Name = "ToggleButton",
	            Category = "GUI",
	            Help = "A spread of toggle botton groups",
	            Tags = "multitouch, MT, EX9, DX9, transform, interaction, mouse, toggle",
	            Author = "tonfilm")]
	public class ToggleButtonNode: BasicGui2dNode
	{
		[Input("Value Input")]
		ISpread<bool> FValueIn;
		
		[Output("Value Output")]
		ISpread<bool> FValueOut;
		
		//this is to store the values with the patch
		[Config("Internal Value")]
		ISpread<bool> FInternalValueConfig;
		
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
					FControllerGroups.Add(new ToggleButtonGroup());
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
			if ( AnyParameterPinChanged() )
			{
				
				for (slice = 0; slice < inputSpreadCount; slice++)
				{
					ToggleButtonGroup group = (ToggleButtonGroup) FControllerGroups[slice];
					
					group.UpdateTransform(FTransformIn[slice], FCountIn[slice], FSizeIn[slice], FColorIn[slice], FOverColorIn[slice], FActiveColorIn[slice]);
				}
			}
			
			//get spread counts
			int outcount = 0;
			FSpreadCountsOut.SliceCount = inputSpreadCount;
			
			for (slice = 0; slice < inputSpreadCount; slice++)
			{
				ToggleButtonGroup group = (ToggleButtonGroup) FControllerGroups[slice];
				
				outcount += group.FControllers.Length;
				FSpreadCountsOut[slice] = group.FControllers.Length;
				
			}
			
			//update mouse and colors
			bool valueSet = UpdateMouse<ToggleButtonGroup, ToggleButton>(inputSpreadCount);
			
			//set value
			slice = 0;
			if (   FValueIn.IsChanged
			    || FSetValueIn.IsChanged)
			{
				
				for (int i = 0; i < inputSpreadCount; i++)
				{
					ToggleButtonGroup group = (ToggleButtonGroup) FControllerGroups[i];
					int pcount = group.FControllers.Length;
					
					for (int j = 0; j < pcount; j++)
					{

						if (FSetValueIn[slice])
						{
							//update value
							group.UpdateValue((ToggleButton)group.FControllers[j], FValueIn[slice]);
							
							valueSet = true;
						}
						else if (FFirstframe) 
						{
							//load from config pin on first frame
							group.UpdateValue((ToggleButton)group.FControllers[j], FInternalValueConfig[slice]);
						}
						
						slice++;
					}
				}
			}

			
			//write output to pins
			FValueOut.SliceCount = outcount;
			if (outcount != FInternalValueConfig.SliceCount) FInternalValueConfig.SliceCount = outcount;
			FTransformOut.SliceCount = outcount;
			FColorOut.SliceCount = outcount;
			FHitOut.SliceCount = outcount;
			FActiveOut.SliceCount = outcount;
			FMouseOverOut.SliceCount = outcount;
			
			slice = 0;
			for (int i = 0; i < inputSpreadCount; i++)
			{
				ToggleButtonGroup group = (ToggleButtonGroup) FControllerGroups[i];
				int pcount = group.FControllers.Length;
				
				for (int j = 0; j < pcount; j++)
				{
					ToggleButton s = (ToggleButton) group.FControllers[j];
					
					FTransformOut[slice] = s.Transform;
					FColorOut[slice] = s.CurrentCol;
					FValueOut[slice] = s.Value;
					FMouseOverOut[slice] = s.MouseOver;
					FHitOut[slice] = s.Hit;
					FActiveOut[slice] = s.Active;
					
					//update config pin
					if (valueSet)
					{
						if (FInternalValueConfig[slice] != s.Value)
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



