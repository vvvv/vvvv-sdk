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
	public class SpreadListWrapper<T, TSub> : ISpread<T>
	{
		protected ISpread<T> FSpreadList;
		
		public SpreadListWrapper(IPluginHost host, PinAttribute attribute)
		{
			//input pin group
			if (attribute is InputAttribute)
			{
				Debug.WriteLine(string.Format("Creating input spread list '{0}'.", attribute.Name));

				FSpreadList = new InputSpreadList<T, TSub>(host, attribute as InputAttribute) as ISpread<T>;
			}
			
			//output pin group
			else if(attribute is OutputAttribute)
			{
				Debug.WriteLine(string.Format("Creating output spread list '{0}'.", attribute.Name));

				FSpreadList = new OutputSpreadList<T, TSub>(host, attribute as OutputAttribute) as ISpread<T>;
			}
		}
		
		public T this[int index]
		{
			get
			{
				return FSpreadList[index];
			}
			set 
			{

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

			}
		}
		
		public IEnumerator<T> GetEnumerator()
		{
			return FSpreadList.GetEnumerator();
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
