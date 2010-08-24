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
	public class SpreadListWrapper<T> : ISpread<ISpread<T>>
	{
		protected ISpread<ISpread<T>> FSpreadList;
		
		public SpreadListWrapper(IPluginHost host, PinAttribute attribute)
		{
			if(attribute.IsPinGroup)
			{
				//input pin group
				if (attribute is InputAttribute)
				{
					Debug.WriteLine(string.Format("Creating input spread list '{0}'.", attribute.Name));

					FSpreadList = new InputSpreadList<T>(host, attribute as InputAttribute) as ISpread<ISpread<T>>;
				}
				
				//output pin group
				else if(attribute is OutputAttribute)
				{
					Debug.WriteLine(string.Format("Creating output spread list '{0}'.", attribute.Name));

					FSpreadList = new OutputSpreadList<T>(host, attribute as OutputAttribute) as ISpread<ISpread<T>>;
				}
			}
			else
			{
				//input bin spread
				if (attribute is InputAttribute)
				{
					Debug.WriteLine(string.Format("Creating input bin spread '{0}'.", attribute.Name));

					FSpreadList = new InputBinSpread<T>(host, attribute as InputAttribute) as ISpread<ISpread<T>>;
				}
				
				//output bin spread
				else if(attribute is OutputAttribute)
				{
					Debug.WriteLine(string.Format("Creating output bin spread '{0}'.", attribute.Name));

					FSpreadList = new OutputBinSpread<T>(host, attribute as OutputAttribute) as ISpread<ISpread<T>>;
				}
			}
			
		}
		
		public ISpread<T> this[int index]
		{
			get
			{
				return FSpreadList[index];
			}
			set 
			{
				FSpreadList[index] = value;
			}
		}
		
		public int SliceCount 
		{
			get 
			{
				return FSpreadList.SliceCount;
			}
			set 
			{
				FSpreadList.SliceCount = value;
			}
		}
		
		public IEnumerator<ISpread<T>> GetEnumerator()
		{
			return FSpreadList.GetEnumerator();
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
