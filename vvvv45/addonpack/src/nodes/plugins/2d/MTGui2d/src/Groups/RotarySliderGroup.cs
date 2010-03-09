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



	//the slider group
	public class RotarySliderGroup : BasicGui2dGroup<RotarySlider>
	{
		private double FSliderSpeed;
		
		//constructor
		public RotarySliderGroup()
		{
		}
		
		//update data
		public void UpdateTransform(Matrix4x4 Transform,
		                            Vector2D Count,
		                            Vector2D Size,
		                            double sliderSpeed)
		{
			//copy to fields
			FSliderSpeed = sliderSpeed;
			
			base.UpdateTransform(Transform, Count, Size);
			
		}
		
        public override void UpdateTouches(TouchList touches)
		{			
			base.UpdateTouches(touches);

            //update slider
            for (int i = 0; i < FControllers.Length; i++)
            {
                //get current slider
                RotarySlider s = FControllers[i];

                if (s.Hit)
                {
                    Vector2D invMouse = (s.InvTransform * new Vector2D(s.AssignedTouch.X, s.AssignedTouch.Y)).xy;
                    Vector2D invLastMouse = (s.InvTransform * new Vector2D(s.LastTouchPos.X, s.LastTouchPos.Y)).xy;
                    s.Value = VMath.Clamp(s.Value + (invMouse.y - invLastMouse.y) * FSliderSpeed, 0, 1);
                }
            }
		}
		
		//set value
		public void UpdateValue(RotarySlider s, double val)
		{
			s.Value = VMath.Clamp(val, 0, 1);
		}
	}
}


