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
	public class RotarySliderGroup : BasicGui2dGroup<RotarySlider>
	{
		//fields
		private Vector2D FLastMouse;
		private RGBAColor ColSlider;
		private double FSliderSpeed;
		
		//constructor
		public RotarySliderGroup()
		{
		}
		
		//update data
		public void UpdateTransform(Matrix4x4 Transform,
		                            Vector2D Count,
		                            Vector2D Size,
		                            RGBAColor Col,
		                            RGBAColor Over,
		                            RGBAColor Active,
		                            RGBAColor SliderCol,
		                            double sliderSpeed)
		{
			//copy to fields
			ColSlider = SliderCol;
			FSliderSpeed = sliderSpeed;
			
			base.UpdateTransform(Transform, Count, Size, Col, Over, Active);
			
		}
		
		//update mouse
		public override bool UpdateMouse(Vector2D Mouse, 
		                          		 bool MouseLeftDownEdge,
		                        		 bool MouseLeftPressed)
		{
			
			bool upEdgeHit = base.UpdateMouse(Mouse, MouseLeftDownEdge, MouseLeftPressed);
					
			//update slider
			for (int i = 0; i < FControllers.Length; i++)
			{
				//get current slider
				RotarySlider s = FControllers[i];
				
				
				//set selected slice number and color
				if (FMouseHit && i == SelectedSlice)
				{
					
					Vector2D invMouse = (s.InvTransform * Mouse).xy;
					Vector2D invLastMouse = (s.InvTransform * FLastMouse).xy;
					
					s.Value = VMath.Clamp(s.Value + (invMouse.y - invLastMouse.y) * FSliderSpeed, 0, 1);

				}
				
				s.ColorSlider = ColSlider;
				
			}
			
			FLastMouse = Mouse;
			
			return upEdgeHit;
		}
		
		//set value
		public void UpdateValue(RotarySlider s, double val)
		{
			s.Value = VMath.Clamp(val, 0, 1);
		}
	}
}


