using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Config
{
	public class SpreadListConfigPin : IntConfigPin
	{
		public SpreadListConfigPin(IPluginHost host, ConfigAttribute attribute)
			:base(host, attribute)
		{
		}
		
		// just add an event to let the spread list create the pins
		public delegate void UpdateEventHandler();
		public event UpdateEventHandler Updated;
		
		unsafe public override void Update()
		{
			base.Update();
			if (Updated != null) Updated();
		}
	}
}
