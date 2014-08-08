using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Raw
{
    [PluginInfo(Name = "Change", Category = "Raw", 
        Help = "Outputs 1 when the input has changed in this frame and 0 if the input was equal to the one in the last frame")]
    public class RawChangeNode : ChangeNode<Stream>
    {
        public RawChangeNode()
            : base(StreamEqualityComparer.Instance)
        {
        }

        protected override Stream CopySlice(Stream slice)
        {
            var copy = new MemoryStream((int)slice.Length);
            slice.Position = 0;
            slice.CopyTo(copy);
            return copy;
        }

        protected override void CopySlices(Stream[] slices)
        {
            for (int i = 0; i < slices.Length; i++)
                slices[i] = CopySlice(slices[i]);
        }
    }
}
