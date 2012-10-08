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
using VVVV.Utils.VMath;
using VVVV.Utils.VColor;

namespace VVVV.Nodes
{

	public class RadioButtonNode : BasicGui2dNode, IPlugin
	{
		#region constructor/destructor
		
		public RadioButtonNode()
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
				Info.Name = "RadioButton";
				Info.Category = "GUI";
				Info.Version = "";
				Info.Help = "A spread of radio buttons";
				Info.Tags = "EX9, DX9, transform, interaction, mouse, bang";
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
			if (   FCountXIn.PinIsChanged
			    || FCountYIn.PinIsChanged
			    || FSizeXIn.PinIsChanged
			    || FSizeYIn.PinIsChanged
			    || FTransformIn.PinIsChanged
			    || FColorIn.PinIsChanged
			    || FOverColorIn.PinIsChanged
			    || FActiveColorIn.PinIsChanged)
			{
				
				for (slice = 0; slice < inputSpreadCount; slice++)
				{
					RadioButtonGroup group = (RadioButtonGroup) FControllerGroups[slice];
					
					Matrix4x4 trans;
					Vector2D count, size;
					RGBAColor col, over, active;
					
					FTransformIn.GetMatrix(slice, out trans);
					FCountXIn.GetValue(slice, out count.x);
					FCountYIn.GetValue(slice, out count.y);
					FSizeXIn.GetValue(slice, out size.x);
					FSizeYIn.GetValue(slice, out size.y);
					FColorIn.GetColor(slice, out col);
					FOverColorIn.GetColor(slice, out over);
					FActiveColorIn.GetColor(slice, out active);

					group.UpdateTransform(trans, count, size, col, over, active);
					
				}
			}
			
			
			//update mouse and colors
			bool valueSet = false;
			if (   FMouseXIn.PinIsChanged
			    || FMouseYIn.PinIsChanged
			    || FLeftButtonIn.PinIsChanged
			    || FColorIn.PinIsChanged
			    || FOverColorIn.PinIsChanged
			    || FActiveColorIn.PinIsChanged
			    || FCountXIn.PinIsChanged
			    || FCountYIn.PinIsChanged
			    || FLastMouseLeft
			    || FSetValueIn.PinIsChanged)
			{
				
				Vector2D mouse;
				double mouseLeft;
				
				FMouseXIn.GetValue(0, out mouse.x);
				FMouseYIn.GetValue(0, out mouse.y);
				FLeftButtonIn.GetValue(0, out mouseLeft);
				
				for (slice = 0; slice < inputSpreadCount; slice++)
				{
					
					RadioButtonGroup group = (RadioButtonGroup) FControllerGroups[slice];
					valueSet = group.UpdateMouse(mouse, (mouseLeft >= 0.5) && !FLastMouseLeft, mouseLeft >= 0.5);
					
					
				}
				
				valueSet = FLastMouseLeft && mouseLeft < 0.5;
				FLastMouseLeft = mouseLeft >= 0.5;
			}
			
			if (   FValueIn.PinIsChanged
			    || FSetValueIn.PinIsChanged)
			{
				
				for (slice = 0; slice < inputSpreadCount; slice++)
				{
					RadioButtonGroup group = (RadioButtonGroup) FControllerGroups[slice];
					
					double val;
					
					FSetValueIn.GetValue(slice, out val);
					
					if (val >= 0.5)
					{
					   //update value
					   FValueIn.GetValue(slice, out val);
					   group.UpdateValue((int) Math.Round(val));
					   valueSet = true;
					}
					else if (FFirstframe) 
					{
						//load from config pin on first frame
						FInternalValueConfig.GetValue(slice, out val);
						group.UpdateValue((int) Math.Round(val));
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
				FSpreadCountsOut.SetValue(slice, group.FControllers.Length);
				
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
				
				FValueOut.SetValue(i, group.SelectedSlice);
				if (valueSet) FInternalValueConfig.SetValue(i, group.SelectedSlice);
				
				for (int j = 0; j < pcount; j++)
				{
					RadioButton b = (RadioButton)group.FControllers[j];
					
					FTransformOut.SetMatrix(slice, b.Transform);
					FColorOut.SetColor(slice, b.CurrentCol);
					FMouseOverOut.SetValue(slice, b.MouseOver ? 1 : 0);
					FHitOut.SetValue(slice, b.Hit ? 1 : 0);
					FActiveOut.SetValue(slice, b.Active ? 1 : 0);
					
					slice++;
				}
			}
			
			//end of frame
			FFirstframe = false;

		}
		
		
		#endregion mainloop
	}
}
