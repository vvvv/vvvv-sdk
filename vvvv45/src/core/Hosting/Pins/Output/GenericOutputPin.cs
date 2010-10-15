using System;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Output
{
	public class GenericOutputPin<T> : Pin<T>, IPinUpdater, IGenericIO<T>
	{
		protected INodeOut FNodeOut;
		protected bool FChanged;
		
		public GenericOutputPin(IPluginHost host, OutputAttribute attribute)
			: base(host, attribute)
		{
			host.CreateNodeOutput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out FNodeOut);
			
			// Register all implemented interfaces and inherited classes of T
			// to support the assignment of ISpread<Apple> output to ISpread<Fruit> input.
			var guids = new List<Guid>();
			var typeT = typeof(T);
			
			foreach (var interf in typeT.GetInterfaces())
				guids.Add(interf.GUID);
			
			while (typeT != null)
			{
				guids.Add(typeT.GUID);
				typeT = typeT.BaseType;
			}
			
			FNodeOut.SetSubType(guids.ToArray(), typeof(T).FullName);
			FNodeOut.SetInterface(this);
			
			base.Initialize(FNodeOut);
		}
		
		public override T this[int index]
		{
			get
			{
				return base[index];
			}
			set
			{
				base[index] = value;
				FChanged = true;
			}
		}
		
		public T GetSlice(int slice)
		{
			return this[slice];
		}
		
		public override void Update()
		{
			base.Update();
			
			if (FChanged) 
			{
				if (FAttribute.SliceMode != SliceMode.Single)
					FNodeOut.SliceCount = FSliceCount;
				
				FNodeOut.MarkPinAsChanged();
			}
			
			FChanged = false;
		}
	}
}
