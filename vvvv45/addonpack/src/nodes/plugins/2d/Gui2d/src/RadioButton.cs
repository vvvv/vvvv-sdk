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
	public class RadioButton : BasicGui2dController
	{
		
		public RadioButton()
		{
			Transform = VMath.IdentityMatrix;
			Hit = false;
			MouseOver = false;
			Active = false;
			CurrentCol = new RGBAColor(0.2, 0.2, 0.2, 1);
		}
	
	}
}

