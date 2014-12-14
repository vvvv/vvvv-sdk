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

using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Nodes
{
	[PluginInfo(Name = "RadioButton",
	            Category = "GUI",
	            Help = "A spread of radio buttons",
	            Tags = "multitouch, MT, EX9, DX9, transform, interaction, mouse, bang",
	            Author = "tonfilm")]
	public class RadioButtonNode : BasicGui2dNode
	{
		
		[Input("Value Input")]
		IDiffSpread<int> FValueIn;

        //autosave
        [Input("Autosave", IsSingle = true)]
        ISpread<bool> FAutoSave;
		
		[Output("Value Output")]
		ISpread<int> FValueOut;
		
		//this is to store the values with the patch
		[Config("Internal Value")]
		IDiffSpread<int> FInternalValueConfig;
		
		#region mainloop
		
		public override void Evaluate(int SpreadMax)
		{
			
			//calc input spreadcount
			int inputSpreadCount = SpreadMax;
			
			//create or delete button groups
			int diff = inputSpreadCount - FControllerGroups.Count;
			if (diff > 0)
			{
				for (int i=0; i<diff; i++)
				{
					FControllerGroups.Add(new RadioButtonGroup());
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
					RadioButtonGroup group = (RadioButtonGroup) FControllerGroups[slice];
					
					group.UpdateTransform(FTransformIn[slice], FCountIn[slice], FSizeIn[slice], FColorIn[slice], FOverColorIn[slice], FActiveColorIn[slice]);
				}
			}
			
			//update mouse and colors
			bool valueSet = false;
			if ( AnyMouseUpdatePinChanged() )
			{
				var mouseCount = FMouseIn.SliceCount;
				FLastMouseLeft.SliceCount = mouseCount;
				for (int mouseSlice = 0; mouseSlice < mouseCount; mouseSlice++)
				{
					var mouse = FMouseIn[0];
					
					for (slice = 0; slice < inputSpreadCount; slice++)
					{
						RadioButtonGroup group = (RadioButtonGroup) FControllerGroups[slice];
						group.IsMultiTouch = mouseCount > 1;
						valueSet = group.UpdateMouse(mouse.Position, mouse.IsLeft && !FLastMouseLeft[mouseSlice], mouse.IsLeft);
					}
					
					valueSet = FLastMouseLeft[mouseSlice] && mouse.IsLeft;
					FLastMouseLeft[mouseSlice] = mouse.IsLeft;
				}
			}
			
			if (   FValueIn.IsChanged
			    || FSetValueIn.IsChanged)
			{
				
				for (slice = 0; slice < inputSpreadCount; slice++)
				{
					RadioButtonGroup group = (RadioButtonGroup) FControllerGroups[slice];
					
					if (FSetValueIn[slice])
					{
					   //update value
					   group.UpdateValue(FValueIn[slice]);
					   valueSet = true;
					}
					else if (FFirstframe) 
					{
						//load from config pin on first frame
						group.UpdateValue(FInternalValueConfig[slice]);
					}
				}
			}

			//get spread counts
			int outcount = 0;
			FSpreadCountsOut.SliceCount = inputSpreadCount;
			
			for (slice = 0; slice < inputSpreadCount; slice++)
			{
				RadioButtonGroup group = (RadioButtonGroup) FControllerGroups[slice];
				
				outcount += group.FControllers.Length;
				FSpreadCountsOut[slice] = group.FControllers.Length;
				
			}
			
			//write output to pins
			FValueOut.SliceCount = inputSpreadCount;
			if (outcount != FInternalValueConfig.SliceCount) FInternalValueConfig.SliceCount = outcount;
			FTransformOut.SliceCount = outcount;
			FColorOut.SliceCount = outcount;
			FHitOut.SliceCount = outcount;
			FActiveOut.SliceCount = outcount;
			FMouseOverOut.SliceCount = outcount;
			
			slice = 0;
			for (int i = 0; i < inputSpreadCount; i++)
			{
				RadioButtonGroup group = (RadioButtonGroup) FControllerGroups[i];
				int pcount = group.FControllers.Length;
				
				FValueOut[i] = group.SelectedSlice;
                if (valueSet && FAutoSave[0]) FInternalValueConfig[i] = group.SelectedSlice;
				
				for (int j = 0; j < pcount; j++)
				{
					RadioButton b = (RadioButton)group.FControllers[j];
					
					FTransformOut[slice] = b.Transform;
					FColorOut[slice] = b.CurrentCol;
					FMouseOverOut[slice] = b.GetAndResetMouseOver();
					FHitOut[slice] = b.Hit;
					FActiveOut[slice] = b.Active;
					
					slice++;
				}
			}
			
			//end of frame
			FFirstframe = false;
		}
		
		#endregion mainloop
	}
}
