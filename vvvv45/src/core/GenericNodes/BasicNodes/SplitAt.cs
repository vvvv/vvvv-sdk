using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.Utils.VMath;

namespace VVVV.Nodes.Generic
{
    public class SplitAtNode<T> : IPluginEvaluate
    {
        [Input("Input")]
        public IInStream<T> InputStream;

        [Input("Index")]
        public IInStream<int> IndexStream;

        [Output("Left")]
        public IOutStream<T> LeftOutStream;

        [Output("Right")]
        public IOutStream<T> RightOutStream;

        public void Evaluate(int spreadMax)
        {
            // Should any of the inputs do not have any data exit early
            if (spreadMax == 0)
            {
                LeftOutStream.Length = 0;
                RightOutStream.Length = 0;
                return;
            }

            // Grab buffers and reader/writer
            using (var buffer = MemoryPool<T>.GetBuffer())
            using (var reader = InputStream.GetReader())
            using (var leftWriter = LeftOutStream.GetDynamicWriter())
            using (var rightWriter = RightOutStream.GetDynamicWriter())
            {
                foreach (var index in IndexStream)
                {
                    // Set reader to its initial position
                    reader.Position = 0;
                    // Write everything up to the given index to the left output
                    int numSlicesToRead;
                    // split at 0 could mean all to the left or all to the right
                    // for now let's put all to the right
                    if (index >= 0)
                        numSlicesToRead = Math.Min(index, InputStream.Length);
                    else
                        numSlicesToRead = Math.Max(InputStream.Length - index, 0);
                    while (numSlicesToRead > 0)
                    {
                        var numSlicesRead = reader.Read(buffer, 0, Math.Min(numSlicesToRead, buffer.Length));
                        leftWriter.Write(buffer, 0, numSlicesRead);
                        numSlicesToRead -= numSlicesRead;
                    }
                    // Write whatever remains to the right output
                    while (!reader.Eos)
                    {
                        var numSlicesRead = reader.Read(buffer, 0, buffer.Length);
                        rightWriter.Write(buffer, 0, numSlicesRead);
                    }
                }
            }
        }
    }
}
