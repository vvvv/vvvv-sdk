using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Utils.Streams;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.IO.Pointers
{
    public unsafe class ValueInput : IValuePointerInput, IInStream
    {
        private readonly IValueIn FPin;
        int* lenptr;
        double** dataptr;

        int len;
        double* data;

        public ValueInput(IValueIn pin)
        {
            FPin = pin;
            pin.GetValuePointer(out lenptr, out dataptr);
        }

        public double* Data { get { return this.data; } }


        public bool Sync()
        {
            FPin.Validate();
            len = *lenptr;
            data = *dataptr;

            return true;
        }

        public bool IsChanged
        {
            get { return this.FPin.PinIsChanged; }
        }

        public object Clone()
        {
            return null;
        }

        public int Length
        {
            get { return this.len; }
        }
    }
}
