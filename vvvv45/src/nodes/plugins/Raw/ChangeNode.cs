using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Nodes.Raw
{
    [PluginInfo(Name = "Change", Category = "Raw", 
        Help = "Outputs 1 when the input has changed in this frame and 0 if the input was equal to the one in the last frame")]
    public class RawChangeNode : ChangeNode<Stream>
    {
        private static readonly byte[] buffer = new byte[4096];

        public RawChangeNode()
            : base(StreamEqualityComparer.Instance)
        {
        }

        protected override Stream CopySlice(Stream slice)
        {
            var copy = new MemoryComStream((int)slice.Length);
            slice.Position = 0;
            slice.CopyTo(copy, buffer);
            return copy;
        }

        protected override void CopySlices(Stream[] slices, int count)
        {
            for (int i = 0; i < count; i++)
                slices[i] = CopySlice(slices[i]);
        }
    }
}
