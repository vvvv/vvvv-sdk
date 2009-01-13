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
	//parent class for controllers
	public class BasicController
	{
		public Matrix4x4 Transform;
		public Matrix4x4 InvTransform;
		public bool Hit;
		public bool MouseOver;
		public bool Active;
		public RGBAColor CurrentCol;
		
		public BasicController()
		{
			
			Transform = VMath.IdentityMatrix;
			Hit = false;
			MouseOver = false;
			Active = false;
			CurrentCol = new RGBAColor(0.2, 0.2, 0.2, 1);
		}
		
	}

	//parent class for controller groups
	public class BasicGui2dGroup
	{
		//fields
		
		//controllers
		public BasicController[] FControllers;
		
		//which slice is selected
		public int SelectedSlice = 0;
		
		//is mouse pressed and a controller hit
		protected bool FMouseHit = false;
		
		//colors
		protected RGBAColor ColNorm, ColOver, ColActive;
		
		
		public BasicGui2dGroup()
		{
		}
		
		public virtual void UpdateMouse(Vector2D Mouse, 
		                        		bool MouseLeftDownEdge,
		                        		bool MouseLeftPressed)
		{
			
			//update state
			bool anythingHit = false;
			for (int i = 0; i < FControllers.Length && !FMouseHit; i++)
			{
				//get current button
				BasicController b = FControllers[i];
				
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
			
			FMouseHit = anythingHit ? true : FMouseHit;
			FMouseHit = FMouseHit && MouseLeftPressed;
			
			
		}
	}
}
