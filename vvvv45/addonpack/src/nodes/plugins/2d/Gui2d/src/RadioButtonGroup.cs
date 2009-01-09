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
	//a button
	public class RadioButton
	{
		public Matrix4x4 Transform;
		public bool Hit;
		public bool MouseOver;
		public bool Active;
		public RGBAColor Color;
		
		public RadioButton()
		{
			Transform = VMath.IdentityMatrix;
			Hit = false;
			MouseOver = false;
			Color = new RGBAColor(0.2, 0.2, 0.2, 1);
		}
	}
	
	//the button group
	public class RadioButtonGroup
	{
		//fields
		public int SelectedSlice = 0;
		public RadioButton[] Buttons;
		private RGBAColor Color, ColOver, ColActive;
		private static bool FMouseHit;
		
		//constructor
		public RadioButtonGroup()
		{
			Buttons = new RadioButton[1];
			Buttons[0] = new RadioButton();
		}
		
		//update data
		public void UpdateTransform(Matrix4x4 Transform,
		                            Vector2D Position,
		                            Vector2D Scale,
		                            Vector2D Count,
		                            Vector2D Size,
		                            RGBAColor Col,
		                            RGBAColor Over,
		                            RGBAColor Active)
		{
			//copy colors
			Color = Col;
			ColOver = Over;
			ColActive = Active;
			
			//get counts
			int countX = VMath.Clamp((int) Math.Round(Count.x), 1, 1000);
			int countY = VMath.Clamp((int) Math.Round(Count.y), 1, 1000);
			int countTotal = countX * countY;
			
			//calculate button space
			Matrix4x4 translate = VMath.Translate(Position.x, Position.y, 0);
			Matrix4x4 size = VMath.Scale(Size.x / countX, Size.y / countY, 1);
			Matrix4x4 buttonSpace = Transform * translate * VMath.Scale(Scale.x, Scale.y, 1);
			
			
			//create buttons?
			if (countTotal != Buttons.Length)
			{
				Buttons = new RadioButton[countTotal];
				for (int i = 0; i < countTotal; i++) 
				{
					Buttons[i] = new RadioButton();
				}
					
			}
			
			int slice = 0;
			for (int i = 0; i < countY; i++)
			{
				for(int j = 0; j < countX; j++)
				{
					//get current button
					RadioButton b = Buttons[slice];
					
					//calc position in button space
					double posX = ( (j + 0.5) / countX ) - 0.5;
					double posY = 0.5 - ( (i + 0.5) / countY ) ;	
					
					//build particular button space
					b.Transform = buttonSpace * VMath.Translate(posX, posY, 0) * size;
						
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
			for (int i = 0; i < Buttons.Length && !FMouseHit; i++)
			{
				//get current button
				RadioButton b = Buttons[i];
				
				//put the mouse into the inverse button space
				Vector2D invMouse = (!b.Transform * Mouse).xy;
				
				//check mouse over and hit
                b.MouseOver = invMouse > -0.5 && invMouse < 0.5;
				b.Hit = b.MouseOver && MouseLeftDown;
				
				//set selected slice number
				if (b.Hit) 
				{
					SelectedSlice = i;
					anythingHit = true;
				} 		
				
			}
			
			FMouseHit = anythingHit ? true : FMouseHit;
			FMouseHit = FMouseHit && MouseLeftPressed;
			
			//update colors
			for (int i = 0; i < Buttons.Length; i++)
			{
				//get current button
				RadioButton b = Buttons[i];
				
				
				//set selected slice number an color
				if (i == SelectedSlice) 
				{
					b.Active = true;
					b.Color = ColActive;
				} 
				else
				{
					b.Active = false;
					b.Color = b.MouseOver ? ColOver : Color;
				}
					
				
			}
			
			
		}
		
		public void UpdateValue(int val)
		{
			
			Buttons[SelectedSlice].Active = false;
			SelectedSlice = val % Buttons.Length;
			Buttons[SelectedSlice].Active = true;
			Buttons[SelectedSlice].Hit = true;
		}
	}
	
}

