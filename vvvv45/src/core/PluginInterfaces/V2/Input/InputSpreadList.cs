using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2.Config;

namespace VVVV.PluginInterfaces.V2.Input
{
	public class InputSpreadList<T> : SpreadList<T>, IPinUpdater
	{
		protected SpreadListConfigPin FConfigPin;
		
		public InputSpreadList(IPluginHost host, InputAttribute attribute)
			: base(host, attribute)
		{
			var att = new ConfigAttribute(FAttribute.Name + " Pin Count");
			att.DefaultValue = 2;
			
			FConfigPin = new SpreadListConfigPin(FHost, att);
			FConfigPin.Updated += UpdatePins;
			
		}
		
		private void UpdatePins()
		{
			var count = FConfigPin[0];
			var diff = count - FPins.Length;
			
			if (count > FPins.Length)
			{
				//store old pins
				var oldPins = FPins;
				
				//create new array
				FPins = new Pin<T>[count];
				
				//copy/create pins
				for (int i = 0; i<count; i++)
				{
					if (i < oldPins.Length)
						FPins[i] = oldPins[i];
					else	
						FPins[i] = CreatePin(i+1);
				}
				
			}
			else if (count < FPins.Length)
			{
				//store old pins
				var oldPins = FPins;
				
				//create new array
				FPins = new Pin<T>[count];
				
				//copy/delete pins
				for (int i = 0; i<oldPins.Length; i++)
				{
					if (i < FPins.Length)
						FPins[i] = oldPins[i];
					else	
						DeletePin(oldPins[i].PluginIO);
				}
			}
		}
		
		//create a pin at position
		private Pin<T> CreatePin(int pos)
		{
			//create pin name
			var origName = FAttribute.Name;
			FAttribute.Name = origName + " " + pos;
			
			var ret	= new InputWrapperPin<T>(FHost, FAttribute as InputAttribute).Pin;
			
			//set attribute name back
			FAttribute.Name = origName;
			
			return ret;
		}
		
		//delete a specific pin
		private void DeletePin(IPluginIO pin)
		{
			FHost.DeletePin(pin);
		}
	}
}
