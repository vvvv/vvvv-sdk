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

	//parent class for controller groups
    public class BasicGui2dGroup<T> where T : AbstractMTGui2dController, new()
	{
		//controllers
		public T[] FControllers;
		
		//which slice is selected
		public int SelectedSlice = 0;
			
		public BasicGui2dGroup()
		{
			FControllers = new T[1];
			FControllers[0] = new T();
		}

		
		//update transform
		public virtual void UpdateTransform(Matrix4x4 Transform,
		                                    Vector2D Count,
		                                    Vector2D Size)

		{
			
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
                    AbstractMTGui2dController b = FControllers[slice];
					
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
		public virtual void UpdateTouches(TouchList touches)
		{
			//update state
			for (int i = 0; i < FControllers.Length; i++)
			{
				//get current button
                AbstractMTGui2dController b = FControllers[i];

                b.NewHit = false;

                if (b.AssignedTouch == null)
                {
                    //No touch, do hit test
                    TouchList newtouches = touches.NewTouches;

                    Touch hit = null;
                    foreach (Touch t in newtouches)
                    {
                        Vector2D v2d = new Vector2D(t.X, t.Y);

                        //put the mouse into the inverse button space
                        Vector2D invMouse = (b.InvTransform * v2d).xy;

                        if (invMouse > -0.5 && invMouse < 0.5)
                        {
                            hit = t;
                        }      
                    }

                    //check mouse over and hit
                    b.Hit = hit != null;
                    b.AssignedTouch = hit;
                    b.LastTouchPos = hit;
                    b.NewHit = b.Hit;
                }
                else
                {
                    //Touch, update assigned id or remove if not there anymore
                    if (touches.ContainsId(b.AssignedTouch.Id))
                    {
                        b.LastTouchPos = b.AssignedTouch;
                        b.AssignedTouch = touches.GetById(b.AssignedTouch.Id);
                        b.Hit = true;
                    }
                    else
                    {
                        b.AssignedTouch = null;
                        b.Hit = false;
                    }
                }			
			}
		}
	}
}
