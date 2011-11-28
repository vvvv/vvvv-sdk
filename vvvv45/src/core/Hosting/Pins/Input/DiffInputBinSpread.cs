using System;
using System.Runtime.InteropServices;
using VVVV.Hosting.Streams;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Input
{
    [ComVisible(false)]
	public class DiffInputBinSpread<T> : InputBinSpread<T>, IDiffSpread<ISpread<T>>
	{
		public DiffInputBinSpread(IOFactory ioFactory, InputAttribute attribute)
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
