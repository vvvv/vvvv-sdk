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

	//the slider group
	public class ToggleButtonGroup : BasicGui2dGroup<ToggleButton>
	{
		
		//constructor
		public ToggleButtonGroup()
		{
		}
		
		//update data
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
			
			bool upEdgeHit = base.UpdateMouse(Mouse, MouseLeftDownEdge, MouseLeftPressed);

			for (int slice = 0; slice < FControllers.Length ; slice++)
			{
				//get current button
				ToggleButton s = FControllers[slice];
				
				//set selected slice number and color
				if (MouseLeftDownEdge && FMouseHit && slice == SelectedSlice)
				{
					s.Value = !s.Value;
					s.CurrentCol = s.Value ? ColActive : ColNorm;
				}
				else if (slice == SelectedSlice)
				{
					s.CurrentCol = s.Value ? ColActive : ColNorm;
					s.CurrentCol = s.MouseOver ? ColOver : s.CurrentCol;
					s.CurrentCol = s.Value ? ColActive : s.CurrentCol;
				}
				else
				{
					s.CurrentCol = s.Value ? ColActive : ColNorm;	
					s.CurrentCol = s.MouseOver ? ColOver : s.CurrentCol;
				}
				
			}
			
			return upEdgeHit;
		}
		
		//set value
		public void UpdateValue(ToggleButton s, bool val)
		{
			s.Value = val;
			s.CurrentCol = s.Value ? ColActive : ColNorm;
		}
	}
}




