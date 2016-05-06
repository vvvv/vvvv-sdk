using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Utils.Streams;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.IO.Pointers
{
    public unsafe class ColorOutput : IColorPointerOutput, IOutStream
    {
        private readonly IColorOut FPin;
        double** dataptr;

        double* data;

        public ColorOutput(IColorOut pin)
        {
            FPin = pin;
            pin.GetColorPointer(out dataptr);
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

        public void Flush(bool force = false)
        {

        }

        public double* Data { get { return this.data; } }
    }
}
