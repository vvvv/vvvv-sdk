using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;

namespace VVVV.Hosting.Pins.Input
{
    [ComVisible(false)]
	class DiffInputSpreadList<T> : SpreadList<ISpread<T>>, IDiffSpread<ISpread<T>>
	{
		public DiffInputSpreadList(IIOFactory ioFactory, InputAttribute attribute)
			: base(ioFactory, attribute)
		{
		}

        protected override IOAttribute CreateAttribute(int position)
        {
            return new InputAttribute(FAttribute.Name + " " + position)
            {
                IsPinGroup = false,
                CheckIfChanged = true
            };
        }

        public override bool Sync()
        {
            if (base.Sync())
            {
                OnChanged();
                return true;
            }
            return false;
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

        protected virtual void OnChanged()
        {
            if (Changed != null)
                Changed(this);
            if (FChanged != null)
                FChanged(this);
        }
    }
}
