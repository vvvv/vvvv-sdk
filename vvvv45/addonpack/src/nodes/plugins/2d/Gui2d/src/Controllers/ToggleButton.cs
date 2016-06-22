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
	//a toggle button
	public class ToggleButton : BasicGui2dController
	{
		//fields
		public bool Value;
		
		public ToggleButton()
		{
			Value = false;
		}
		
		public override void CopyFrom(BasicGui2dController Source)
		{
			base.CopyFrom(Source);
			Value = ((ToggleButton)Source).Value;
		}
		
	}
}




