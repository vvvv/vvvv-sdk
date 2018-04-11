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

	//super parent class for controller groups
	//only for the static field, because classes with
	//different generics don't share static variables
	public class MouseLockable
	{
		//static field for all controller groups
		//to disable mouse update when something is hit
		protected static bool FStaticMousHit;
	}

	//parent class for controller groups
	public class BasicGui2dGroup<T> : MouseLockable where T : BasicGui2dController, new()
	{
		//controllers
		public T[] FControllers;
		
		//which slice is selected
		public int SelectedSlice = 0;
		
		//is mouse pressed and a controller hit
		protected bool FMouseHit = false;
		
		//colors
		protected RGBAColor ColNorm, ColOver, ColActive;
		
		public BasicGui2dGroup()
		{
			FControllers = new T[1];
			FControllers[0] = new T();
		}

		
		//update transform
		public virtual void UpdateTransform(Matrix4x4 Transform,
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
			
			//calculate controller space
			Matrix4x4 size = VMath.Scale(Size.x / countX, Size.y / countY, 1);
			Matrix4x4 controllerSpace = Transform;
			
			//create controllers?
			if (countTotal != FControllers.Length)
			{

				//store old controllers
				T[] temp = FControllers;

				//make new array
				FControllers = new T[countTotal];
				for (int i = 0; i < countTotal; i++)
				{
					FControllers[i] = new T();
					
					//copy data from old array
					FControllers[i].CopyFrom(temp[i%temp.Length]);
				}
			}

			int slice = 0;
			for (int i = 0; i < countY; i++)
			{
				for(int j = 0; j < countX; j++)
				{
					//get current controller
					BasicGui2dController b = FControllers[slice];
					
					//calc position in controller space
					double posX = ( (j + 0.5) / countX ) - 0.5;
					double posY = 0.5 - ( (i + 0.5) / countY );	
					
					//build particular controller space
					b.Transform = size * VMath.Translate(posX, posY, 0) * controllerSpace;
					b.InvTransform = !b.Transform;
					
					if (b is RotarySlider) 
					{
						b.Transform = VMath.Scale(-1, 1, 1) * VMath.RotateZ(0.25 * VMath.CycToRad) * b.Transform;
					}
					
					slice++;
				}
			}
			
		}
		
		//mouse
		public virtual bool UpdateMouse(Vector2D Mouse, 
		                        		bool MouseLeftDownEdge,
		                        		bool MouseLeftPressed)
		{
			
			if (FStaticMousHit && MouseLeftPressed) return false;
			
			//update state
			bool anythingHit = false;
			for (int i = 0; i < FControllers.Length && !FMouseHit; i++)
			{
				//get current button
				BasicGui2dController b = FControllers[i];
				
				//put the mouse into the inverse button space
				Vector2D invMouse = (b.InvTransform * Mouse).xy;
				
				//check mouse over and hit
				b.MouseOver = invMouse > -0.5 && invMouse < 0.5;
				
				//set selected slice number
				if (b.MouseOver && MouseLeftDownEdge)
				{
					SelectedSlice = i;
					anythingHit = true;
				}
				
			}
			
			bool lastMouseHit = FMouseHit;
			FMouseHit = anythingHit ? true : FMouseHit;
			FMouseHit = FMouseHit && MouseLeftPressed;
			
			//update colors
			for (int i = 0; i < FControllers.Length; i++)
			{
				//get current button
				BasicGui2dController b = FControllers[i];
				
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
			
			//return if something was hit on mous up edge
			FStaticMousHit = FMouseHit;
			return lastMouseHit && !FMouseHit;
		}
	}
}
