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
	public class RotarySlider : BasicGui2dController
		{
			//fields
			public Matrix4x4 SliderTransform;
			public double Value;
			public RGBAColor ColorSlider;
			
			public RotarySlider()
			{
				Transform = VMath.IdentityMatrix;
				InvTransform = VMath.IdentityMatrix;
				Value = 0;
				Hit = false;
				MouseOver = false;
				CurrentCol = new RGBAColor(0.2, 0.2, 0.2, 1);
			}
		}
}


