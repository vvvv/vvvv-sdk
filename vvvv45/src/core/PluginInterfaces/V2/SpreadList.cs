using System;
using System.Collections;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2.Config;

namespace VVVV.PluginInterfaces.V2
{
	/// <summary>
	/// base class for spread lists
	/// </summary>
	public abstract class SpreadList<T> : ISpread<ISpread<T>>
	{
		protected ISpread<T>[] FPins;
		protected IPluginHost FHost;
		protected PinAttribute FAttribute;
		protected IntConfigPin FConfigPin;
		
		public SpreadList(IPluginHost host, PinAttribute attribute)
		{
			//store fields
			FHost = host;
			FAttribute = attribute;
			FPins = new ISpread<T>[0];
			
			//create config pin
			var att = new ConfigAttribute(FAttribute.Name + " Pin Count");
			att.DefaultValue = 2;
			
			FConfigPin = new IntConfigPin(FHost, att);
			FConfigPin.Updated += new PinUpdatedEventHandler<int>(UpdatePins);
			
		}
		
		//pin management
		protected void UpdatePins(ObservablePin<int> pin)
		{
			var count = FConfigPin[0];
			var diff = count - FPins.Length;
			
			if (count > FPins.Length)
			{
				//store old pins
				var oldPins = FPins;
				
				//create new array
				FPins = new ISpread<T>[count];
				
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
				FPins = new ISpread<T>[count];
				
				//copy/delete pins
				for (int i = 0; i<oldPins.Length; i++)
				{
					if (i < FPins.Length)
						FPins[i] = oldPins[i];
					else	
						DeletePin((oldPins[i] as IPluginIOProvider).PluginIO);
				}
			}
		}
		
		//the actual pin creation
		protected abstract ISpread<T> CreatePin(int pos);
		
		//delete a specific pin
		protected void DeletePin(IPluginIO pin)
		{
			FHost.DeletePin(pin);
		}
		
		public ISpread<T> this[int index]
		{
			get
			{
				return FPins[index];
			}
			set 
			{
				
			}
		}
		
		public int SliceCount 
		{
			get 
			{
				return FPins.Length;
			}
			set 
			{
				
			}
		}

		public IEnumerator<ISpread<T>> GetEnumerator()
		{
			return (IEnumerator<ISpread<T>>)FPins.GetEnumerator();
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
