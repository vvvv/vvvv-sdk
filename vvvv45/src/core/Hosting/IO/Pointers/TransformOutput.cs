using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Utils.Streams;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Hosting.IO.Pointers
{
    public unsafe class TransformOutput : IOutStream
    {
        private readonly ITransformOut FPin;
        float** dataptr;

        float* data;

        public TransformOutput(ITransformOut pin)
        {
            FPin = pin;
            pin.GetMatrixPointer(out dataptr);
        }

        public int Length
        {
            get
            {
                return this.FPin.SliceCount;
            }
            set
            {
                this.FPin.SliceCount = value;
                data = *dataptr;
            }
        }

        public object Clone()
        {
            return null;
        }

        public void Flush()
        {

        }

        public float* Data { get { return this.data; } }
    }
}
