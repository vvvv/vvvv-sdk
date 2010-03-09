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
using VVVV.Nodes.src.Controllers;

namespace VVVV.Nodes
{

	//the xy slider group
	public class SliderXYGroup : BasicGui2dGroup<SliderXY>
	{
		private Matrix4x4 FSliderSize;
		private double FSliderSpeed;
		
		//constructor
		public SliderXYGroup()
		{
		}
		
		//update data
		public void UpdateTransform(Matrix4x4 Transform,
		                            Vector2D Count,
		                            Vector2D Size,
		                            double SizeSlider,
		                            double sliderSpeed)
		{
			//copy fields
			FSliderSpeed = sliderSpeed;
			FSliderSize = VMath.Scale(SizeSlider, SizeSlider, 1);
			
			base.UpdateTransform(Transform, Count, Size);
			
			//update slider control
			for (int slice = 0; slice < FControllers.Length; slice++)
			{
				
				//get current slider
				SliderXY s = FControllers[slice];
				s.SliderTransform = FSliderSize * VMath.Translate(s.Value.x - 0.5, s.Value.y - 0.5, 0) * s.Transform;
				
			}
			
		}
		
		//update mouse
		public override void UpdateTouches(TouchList touches)
		{

            base.UpdateTouches(touches);

            //update slider
            for (int i = 0; i < FControllers.Length; i++)
            {
                //get current slider
                SliderXY s = FControllers[i];

                if (s.Hit)
                {
                    Vector2D invMouse = (s.InvTransform * new Vector2D(s.AssignedTouch.X, s.AssignedTouch.Y)).xy;
                    Vector2D invLastMouse = (s.InvTransform * new Vector2D(s.LastTouchPos.X, s.LastTouchPos.Y)).xy;

                    s.Value = VMath.Clamp(s.Value + (invMouse - invLastMouse) * FSliderSpeed, 0, 1);
                    s.SliderTransform = FSliderSize * VMath.Translate(s.Value.x - 0.5, s.Value.y - 0.5, 0) * s.Transform;
                }
            }

		}
		
		//set value
		public void UpdateValue(SliderXY s, Vector2D val)
		{
			s.Value = VMath.Clamp(val, 0, 1);
			s.SliderTransform = FSliderSize * VMath.Translate(s.Value.x - 0.5, s.Value.y - 0.5, 0) * s.Transform;
		}
	}
}


