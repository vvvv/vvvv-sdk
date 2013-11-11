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
using VVVV.Utils.VMath; 
using VVVV.Utils.VColor;

namespace VVVV.Nodes
{
	
	//the radio button group, uses basic controller
	public class RadioButtonGroup : BasicGui2dGroup<RadioButton>
	{
	
		//constructor
		public RadioButtonGroup()
		{
		}
		
		//update transform
		public override void UpdateTransform(Matrix4x4 Transform,
		                                     Vector2D Count,
		                                     Vector2D Size,
		                                     RGBAColor Col,
		                                     RGBAColor Over,
		                                     RGBAColor Active)
		{

			base.UpdateTransform(Transform, Count, Size, Col, Over, Active);
			
		}
		
		//update mouse
		public override bool UpdateMouse(Vector2D Mouse,
		                                 bool MouseLeftDownEdge,
		                                 bool MouseLeftPressed)
		{
			
			int lastSelected = SelectedSlice;
			
			base.UpdateMouse(Mouse, MouseLeftDownEdge, MouseLeftPressed);
			
			return lastSelected	!= SelectedSlice;
		}
		
		public void UpdateValue(int val)
		{
			//disable old slice
			FControllers[SelectedSlice].Active = false;
			FControllers[SelectedSlice].CurrentCol = ColNorm;
			
			//set new selection
			SelectedSlice = val % FControllers.Length;
			
			//enable new slice
			FControllers[SelectedSlice].Active = true;
			FControllers[SelectedSlice].CurrentCol = ColActive;
		}
	}
	
}

