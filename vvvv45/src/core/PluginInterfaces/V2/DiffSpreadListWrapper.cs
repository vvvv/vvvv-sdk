using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2.Input;
using VVVV.PluginInterfaces.V2.Output;

namespace VVVV.PluginInterfaces.V2
{
	/// <summary>
	/// Intermediate type to delegate the pin group creation
	/// </summary>
	public class DiffSpreadListWrapper<T> : IDiffSpread<ISpread<T>>
	{
		protected IDiffSpread<ISpread<T>> FSpreadListDiff;
		
		public DiffSpreadListWrapper(IPluginHost host, PinAttribute attribute)
		{
			if(attribute.IsPinGroup)
			{
				Debug.WriteLine(string.Format("Creating input diff spread list '{0}'.", attribute.Name));

				FSpreadListDiff = new DiffInputSpreadList<T>(host, attribute as InputAttribute) as IDiffSpread<ISpread<T>>;
				
			}
			else
			{
				Debug.WriteLine(string.Format("Creating input diff bin spread '{0}'.", attribute.Name));

				FSpreadListDiff = new DiffInputBinSpread<T>(host, attribute as InputAttribute) as IDiffSpread<ISpread<T>>;
			}
				
		}
		
		public ISpread<T> this[int index]
		{
			get
			{
				return FSpreadListDiff[index];
			}
			set 
			{
				FSpreadListDiff[index] = value;
			}
		}
		
		public int SliceCount 
		{
			get 
			{
				return FSpreadListDiff.SliceCount;
			}
			set 
			{
				FSpreadListDiff.SliceCount = value;
			}
		}
		
		public IEnumerator<ISpread<T>> GetEnumerator()
		{
			return FSpreadListDiff.GetEnumerator();
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		
		public event SpreadChangedEventHander<ISpread<T>> Changed
		{
			add
			{
				FSpreadListDiff.Changed += value;
			}
			remove
			{
				FSpreadListDiff.Changed -= value;
			}
		}
		
		public bool IsChanged 
		{
			get 
			{
				return FSpreadListDiff.IsChanged;
			}
		}
	}
}
