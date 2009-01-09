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

    //a slider
	public class Slider
	{
		//fields
		public Matrix4x4 Transform;
		public Matrix4x4 InvTransform;
		public Matrix4x4 SliderTransform;
		public double Value;
		public bool Hit;
		public bool MouseOver;
		public bool Active;
		public RGBAColor Color;
		public RGBAColor ColorSlider;
		
		public Slider()
		{
			Transform = VMath.IdentityMatrix;
			InvTransform = VMath.IdentityMatrix;
			SliderTransform = VMath.IdentityMatrix;
			Value = 0;
			Hit = false;
			MouseOver = false;
			Color = new RGBAColor(0.2, 0.2, 0.2, 1);
			ColorSlider = new RGBAColor(1, 1, 1, 1);
		}
	}

	//the slider group
	public class SliderGroup
	{
		//fields
		public int SelectedSlice = 0;
		public Slider[] Sliders;
		private RGBAColor Color, ColOver, ColActive, ColSlider;
		private Vector2D FLastMouse;
		private bool FMouseHit;
		private Matrix4x4 FSliderSize;
		
		//constructor
		public SliderGroup()
		{
			Sliders = new Slider[1];
			Sliders[0] = new Slider();
		}
		
		//update data
		public void UpdateTransform(Matrix4x4 Transform,
		                            Vector2D Position,
		                            Vector2D Scale,
		                            Vector2D Count,
		                            Vector2D Size,
		                            double SizeSlider,
		                            RGBAColor Col,
		                            RGBAColor Over,
		                            RGBAColor Active,
		                            RGBAColor Slider)
		{
			//copy colors
			Color = Col;
			ColOver = Over;
			ColActive = Active;
			ColSlider = Slider;
			
			//get counts
			int countX = VMath.Clamp((int) Math.Round(Count.x), 1, 1000);
			int countY = VMath.Clamp((int) Math.Round(Count.y), 1, 1000);
			int countTotal = countX * countY;
			
			//calculate button space
			Matrix4x4 translate = VMath.Translate(Position.x, Position.y, 0);
			Matrix4x4 size = VMath.Scale(Size.x / countX, Size.y / countY, 1);
			FSliderSize = VMath.Scale(1.0 , SizeSlider, 1);
			Matrix4x4 buttonSpace = Transform * translate * VMath.Scale(Scale.x, Scale.y, 1);
			
			
			//create sliders?
			if (countTotal != Sliders.Length)
			{
				Sliders = new Slider[countTotal];
				for (int i = 0; i < countTotal; i++) 
				{
					Sliders[i] = new Slider();
				}
					
			}
			
			int slice = 0;
			for (int i = 0; i < countY; i++)
			{
				for(int j = 0; j < countX; j++)
				{
					//get current slider
					Slider s = Sliders[slice];
					
					//calc position in slider space
					double posX = ( (j + 0.5) / countX ) - 0.5;
					double posY = 0.5 - ( (i + 0.5) / countY );	
					
					//build particular slider space
					s.Transform = buttonSpace * VMath.Translate(posX, posY, 0) * size;
					s.InvTransform = !s.Transform;
					s.SliderTransform = s.Transform * VMath.Translate(0, s.Value - 0.5, 0) * FSliderSize;
					slice++;
				}
			}
			
		}
		
		//update mouse
		public void UpdateMouse(Vector2D Mouse, 
		                        bool MouseLeftDown,
		                        bool MouseLeftPressed)
		{
			
			//update state
			bool anythingHit = false;
			
			for (int i = 0; i < Sliders.Length && !FMouseHit; i++)
			{
				//get current slider
				Slider s = Sliders[i];
				
				//put the mouse into the inverse slider space
				Vector2D invMouse = (s.InvTransform * Mouse).xy;
				
				//check mouse over and value
	            s.MouseOver = invMouse > -0.5 && invMouse < 0.5;
				s.Hit = s.MouseOver && MouseLeftDown;
				
				//set selected slice number
				if (s.Hit) 
				{
					SelectedSlice = i;
					anythingHit = true;
				} 		
				
			}
			
			FMouseHit = anythingHit ? true : FMouseHit;
			FMouseHit = FMouseHit && MouseLeftPressed;
			
			
			//update colors
			for (int i = 0; i < Sliders.Length; i++)
			{
				//get current slider
				Slider s = Sliders[i];
				
				
				//set selected slice number an color
				if (i == SelectedSlice)
				{
					s.Active = true;
					s.Color = ColActive;
					
					//update Value
					if (FMouseHit)
					{
						
						Vector2D invMouse = (s.InvTransform * Mouse).xy;
						Vector2D invLastMouse = (s.InvTransform * FLastMouse).xy;
						
						s.Value = VMath.Clamp(s.Value + invMouse.y - invLastMouse.y, 0, 1);
						s.SliderTransform = s.Transform * VMath.Translate(0, s.Value - 0.5, 0) * FSliderSize;
						
					}
					
				}
				else
				{
					s.Active = false;
					s.Color = s.MouseOver ? ColOver : Color;
				}
				
				s.ColorSlider = ColSlider;
			}
			
			FLastMouse = Mouse;
		}
		
		//set value
		public void UpdateValue(Slider s, double val)
		{
			s.Value = VMath.Clamp(val, 0, 1);
			s.SliderTransform = s.Transform * VMath.Translate(0, s.Value - 0.5, 0) * FSliderSize;
		}
	}
}

