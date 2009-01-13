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
	public class RadioButtonGroup : BasicGui2dGroup
	{
	
		//constructor
		public RadioButtonGroup()
		{
			FControllers = new BasicController[1];
			FControllers[0] = new BasicController();
		}
		
		//update transform
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
			ColNorm = Col;
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
			if (countTotal != FControllers.Length)
			{
				FControllers = new BasicController[countTotal];
				for (int i = 0; i < countTotal; i++) 
				{
					FControllers[i] = new BasicController();
				}
					
			}
			
			int slice = 0;
			for (int i = 0; i < countY; i++)
			{
				for(int j = 0; j < countX; j++)
				{
					//get current button
					BasicController b = FControllers[slice];
					
					//calc position in button space
					double posX = ( (j + 0.5) / countX ) - 0.5;
					double posY = 0.5 - ( (i + 0.5) / countY ) ;	
					
					//build particular button space
					b.Transform = buttonSpace * VMath.Translate(posX, posY, 0) * size;
					b.InvTransform = !b.Transform;
						
					slice++;
				}
			}
			
			
		}
		
		//update mouse
		public override void UpdateMouse(Vector2D Mouse, 
		                        bool MouseLeftDownEdge,
		                        bool MouseLeftPressed)
		{
			
			base.UpdateMouse(Mouse, MouseLeftDownEdge, MouseLeftPressed);
			
			//update colors
			for (int i = 0; i < FControllers.Length; i++)
			{
				//get current button
				BasicController b = FControllers[i];
				
				//set selected slice number and color
				if (i == SelectedSlice)
				{
					b.Active = true;
					b.Hit = FMouseHit;
					b.CurrentCol = b.MouseOver ? ColOver : ColActive;
					
				}
				else
				{
					b.Active = false;
					b.Hit = false;
					b.CurrentCol = b.MouseOver ? ColOver : ColNorm;
				}
				
				
			}
				
		}
		
		public void UpdateValue(int val)
		{
			
			FControllers[SelectedSlice].Active = false;
			FControllers[SelectedSlice].CurrentCol = ColNorm;
			SelectedSlice = val % FControllers.Length;
			FControllers[SelectedSlice].Active = true;
			FControllers[SelectedSlice].CurrentCol = ColActive;
		}
	}
	
}

