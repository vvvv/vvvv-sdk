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

	//the slider group
	public class ToggleButtonGroup : BasicGui2dGroup<ToggleButton>
	{
		
		//constructor
		public ToggleButtonGroup()
		{
		}
		
		
		//update mouse
        public override void UpdateTouches(TouchList touches)
		{

            base.UpdateTouches(touches);

			for (int slice = 0; slice < FControllers.Length ; slice++)
			{
				//get current button
				ToggleButton s = FControllers[slice];

                if (s.NewHit)
                {
                    s.Value = !s.Value;
                }		
			}
		}
		
		//set value
		public void UpdateValue(ToggleButton s, bool val)
		{
			s.Value = val;
		}
	}
}




