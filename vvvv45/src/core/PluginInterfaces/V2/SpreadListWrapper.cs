using System;
using System.Diagnostics;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2.Input;
using VVVV.PluginInterfaces.V2.Output;

namespace VVVV.PluginInterfaces.V2
{
	/// <summary>
	/// Intermediate type to delegate the pin group creation
	/// </summary>
	public class SpreadListWrapper<T> : ISpreadList<T>
	{
		protected ISpreadList<T> FSpreadList;
		
		public SpreadListWrapper(IPluginHost host, PinAttribute attribute)
		{
			//input pin group
			if (attribute is InputAttribute)
			{
				Debug.WriteLine(string.Format("Creating input Pin Group '{0}'.", attribute.Name));

				if (KnownTypes.IsKnown(typeof(T)))
					FSpreadList = new InputSpreadList<T>(host, attribute as InputAttribute) as ISpreadList<T>;
				else
					FSpreadList = new GenericInputSpreadList<T>(host, attribute as InputAttribute) as ISpreadList<T>;
			}
			
			//output pin group
//			else if(attribute is OutputAttribute)
//			{
//				if (KnownTypes.IsKnown(typeof(T)))
//					FPinGroup = new OutputPinGroup<T>(host, attribute) as IPinGroup<T>;
//				else
//					FPinGroup = new GenericOutputPinGroup<T>(host, attribute) as IPinGroup<T>;
//			}
		}
		
		//the spreads
		public ISpread<T>[] Spreads
		{
			get
			{
				return FSpreadList.Spreads;
			}
		}
	}
}
