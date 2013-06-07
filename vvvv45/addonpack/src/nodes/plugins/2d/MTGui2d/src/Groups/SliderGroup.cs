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
	public class SliderGroup : BasicGui2dGroup<Slider>
	{
		//fields
		private Matrix4x4 FSliderSize;
		private double FSliderSpeed;
		private bool FIsLong;
		
		//constructor
		public SliderGroup()
		{
		}
		
		//update data
		public void UpdateTransform(Matrix4x4 Transform,
		                            Vector2D Count,
		                            Vector2D Size,
		                            double SizeSlider,
		                            double SliderSpeed,
		                            bool isX,
		                           	bool isLong)
		{
			//copy fields
			FSliderSpeed = SliderSpeed;
			FIsLong = isLong;
			
			if(!FIsLong)
			{
				FSliderSize = VMath.Scale(1.0, SizeSlider, 1);
			}
			
			base.UpdateTransform(Transform, Count, Size);
			
			//update slider control
			for (int slice = 0; slice < FControllers.Length; slice++)
			{
				
				//get current slider
				Slider s = FControllers[slice];
				
				if (isX) 
				{
					s.Transform = VMath.RotateZ(isX ? -0.25 * VMath.CycToRad : 0) * s.Transform;
					s.InvTransform = !s.Transform;
				}
	
				if(!FIsLong)
				{
					s.SliderTransform = FSliderSize * VMath.Translate(0, s.Value - 0.5, 0) * s.Transform;
				}
				else
				{
					s.SliderTransform = VMath.Scale(1, s.Value, 1) * VMath.Translate(0, s.Value*0.5 - 0.5 , 1) * s.Transform;
				}
				
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
				Slider s = FControllers[i];

                if (s.Hit)
                {
                    Vector2D invMouse = (s.InvTransform * new Vector2D(s.AssignedTouch.X,s.AssignedTouch.Y)).xy;
                    Vector2D invLastMouse = (s.InvTransform * new Vector2D(s.LastTouchPos.X, s.LastTouchPos.Y)).xy;
                    s.Value = VMath.Clamp(s.Value + (invMouse.y - invLastMouse.y) * FSliderSpeed, 0, 1);
                    if(!FIsLong)
					{
						s.SliderTransform = FSliderSize * VMath.Translate(0, s.Value - 0.5, 0) * s.Transform;
					}
					else
					{
						s.SliderTransform = VMath.Scale(1, s.Value, 1) * VMath.Translate(0, s.Value*0.5 - 0.5 , 1) * s.Transform;
					}					
                }			
			}
		}
		
		//set value
		public void UpdateValue(Slider s, double val)
		{
			s.Value = VMath.Clamp(val, 0, 1);
			if(!FIsLong)
			{
				s.SliderTransform = FSliderSize * VMath.Translate(0, s.Value - 0.5, 0) * s.Transform;
			}
			else
			{
				s.SliderTransform = VMath.Scale(1, s.Value, 1) * VMath.Translate(0, s.Value*0.5 - 0.5 , 1) * s.Transform;
			}
				
		}
	}
}

