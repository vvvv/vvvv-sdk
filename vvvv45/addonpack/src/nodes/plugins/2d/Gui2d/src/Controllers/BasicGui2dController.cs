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
	public class BasicGui2dController
	{
		public Matrix4x4 Transform;
		public Matrix4x4 InvTransform;
		public bool Hit;
		public bool MouseOver;
		public bool Active;
		public RGBAColor CurrentCol;
		
		public BasicGui2dController()
		{
			Transform = VMath.IdentityMatrix;
			InvTransform = VMath.IdentityMatrix;
			Active = false;
			Hit = false;
			MouseOver = false;
			CurrentCol = new RGBAColor(0.2, 0.2, 0.2, 1);
		}
		
		public virtual void CopyFrom(BasicGui2dController Source)
		{
			Transform = Source.Transform;
			InvTransform = Source.InvTransform;
			Active = Source.Active;
			Hit = Source.Hit;
			MouseOver = Source.MouseOver;
			CurrentCol = Source.CurrentCol;
		}
		
		public bool GetAndResetMouseOver()
		{
			var mo = this.MouseOver;
			this.MouseOver = false;
			return mo;
		}
		
	}
}
