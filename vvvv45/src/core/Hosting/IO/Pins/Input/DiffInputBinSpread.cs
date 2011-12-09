using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Input
{
    [ComVisible(false)]
	class DiffInputBinSpread<T> : InputBinSpread<T>, IDiffSpread<ISpread<T>>
	{
		public DiffInputBinSpread(IIOFactory ioFactory, InputAttribute attribute)
			: base(ioFactory, attribute)
		{
		}
		
		protected override InputAttribute ManipulateAttribute(InputAttribute attribute)
		{
			attribute.CheckIfChanged = true;
			return attribute;
		}
		
		public override bool Sync()
		{
			IsChanged = base.Sync();
			
			if (IsChanged)
			{
				OnChanged();
			}
			
			return IsChanged;
		}
		
		protected virtual void OnChanged()
		{
		    if (Changed != null)
		        Changed(this);
		    if (FChanged != null)
		        FChanged(this);
		}

		public event SpreadChangedEventHander<ISpread<T>> Changed;
		
		protected SpreadChangedEventHander FChanged;
        event SpreadChangedEventHander IDiffSpread.Changed
        {
            add
            {
                FChanged += value;
            }
            remove
            {
                FChanged -= value;
            }
        }
		
		public bool IsChanged 
		{
			get;
			private set;
		}
	}
}
