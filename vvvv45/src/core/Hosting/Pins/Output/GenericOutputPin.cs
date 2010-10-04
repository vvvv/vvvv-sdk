using System;
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
			FNodeOut.SetSubType(new Guid[] { typeof(T).GUID }, typeof(T).FullName);
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
