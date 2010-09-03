using System;
using System.Collections;
using System.Collections.Generic;

using VVVV.Hosting.Pins.Config;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins
{
	public abstract class SpreadListBase
	{
		protected static int FInstanceCounter = 1;
	}
	
	/// <summary>
	/// base class for spread lists
	/// </summary>
	public abstract class SpreadList<T> : SpreadListBase, ISpread<ISpread<T>>
	{
		protected ISpread<T>[] FPins;
		protected IPluginHost FHost;
		protected PinAttribute FAttribute;
		protected IntConfigPin FConfigPin;
		protected int FOffsetCounter;
		
		public SpreadList(IPluginHost host, PinAttribute attribute)
		{
			//store fields
			FHost = host;
			FAttribute = attribute;
			FPins = new ISpread<T>[0];
			
			//create config pin
			var att = new ConfigAttribute(FAttribute.Name + " Count");
			att.DefaultValue = 2;
			
			//increment instance Counter and store it as pin offset
			FOffsetCounter = FInstanceCounter++;
			
			FConfigPin = new IntConfigPin(FHost, att);
			FConfigPin.Updated += new PinUpdatedEventHandler<int>(UpdatePins);
			
			FConfigPin.Update();
		}
		
		//pin management
		protected void UpdatePins(Pin<int> pin)
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
				return FPins[VMath.Zmod(index, FPins.Length)];
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
			for(int i=0; i<FPins.Length; i++)
				yield return FPins[i];
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
