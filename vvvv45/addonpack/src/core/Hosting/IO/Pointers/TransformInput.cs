using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Utils.Streams;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Hosting.IO.Pointers
{
    public unsafe class TransformInput : ISynchronizable, IInStream
    {
        private readonly ITransformIn FPin;
        int* lenptr;
        float** dataptr;

        int len;
        float* data;

        public TransformInput(ITransformIn pin)
        {
            FPin = pin;
            pin.GetMatrixPointer(out lenptr, out dataptr);
        }

        public float* Data { get { return this.data; } }

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
