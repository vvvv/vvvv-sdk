using System;
using System.Collections;
using System.Collections.Generic;

using VVVV.Hosting.Pins.Config;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins
{
	/// <summary>
	/// base class for diff spread lists
	/// </summary>
	public abstract class DiffSpreadList<T> : SpreadListBase, IDiffSpread<ISpread<T>>, IDisposable
	{
		protected IDiffSpread<T>[] FPins;
		protected IPluginHost FHost;
		protected PinAttribute FAttribute;
		protected IntConfigPin FConfigPin;
		protected int FOffsetCounter;
		protected int FUpdateCounter;
		
		public DiffSpreadList(IPluginHost host, PinAttribute attribute)
		{
			//store fields
			FHost = host;
			FAttribute = attribute;
			FPins = new IDiffSpread<T>[0];
			
			//create config pin
			var att = new ConfigAttribute(FAttribute.Name + " Count");
			att.DefaultValue = 2;
			
			//increment instance Counter and store it as pin offset
			FOffsetCounter = FInstanceCounter++;
			
			FConfigPin = new IntConfigPin(FHost, att);
			FConfigPin.Updated += UpdatePins;
			
			FConfigPin.Update();
		}
		
		public virtual void Dispose()
		{
		    FConfigPin.Updated -= UpdatePins;
		}
		
		//pin management
		protected void UpdatePins(object sender, EventArgs args)
		{
			var count = FConfigPin[0];
			var diff = count - FPins.Length;
			
			if (count > FPins.Length)
			{
				//store old pins
				var oldPins = FPins;
				
				//create new array
				FPins = new IDiffSpread<T>[count];
				
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
				FPins = new IDiffSpread<T>[count];
				
				//copy/delete pins
				for (int i = 0; i<oldPins.Length; i++)
				{
					if (i < FPins.Length)
						FPins[i] = oldPins[i];
					else	
						DeletePin((oldPins[i] as DiffPin<T>));
				}
			}
		}

		protected void Pin_Updated(object sender, EventArgs args)
		{
			FUpdateCounter++;
			if(FUpdateCounter >= FPins.Length)
			{
				if (IsChanged && Changed != null) 
					Changed(this);
				
				FUpdateCounter = 0;
			}
			
		}
		
		//the actual pin creation
		protected abstract IDiffSpread<T> CreatePin(int pos);
		
		//delete a specific pin
		protected void DeletePin(DiffPin<T> pin)
		{
			pin.Updated -= Pin_Updated;
			FHost.DeletePin(pin.PluginIO);
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
			for (int i=0; i<FPins.Length; i++)
				yield return FPins[i];
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		
		public event SpreadChangedEventHander<ISpread<T>> Changed;
		
		public bool IsChanged 
		{
			get 
			{
				for (int i = 0; i<FPins.Length; i++)
				{
					if (FPins[i].IsChanged) return true;
				}
				
				return false;
			}
		}
	}
}
