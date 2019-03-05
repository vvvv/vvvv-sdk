using System;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using Path = VL.Lib.IO.Path;

namespace VVVV.VL.Hosting.IO.Streams
{
    class PathOutStream : MemoryIOStream<Path>, IDisposable
    {
        readonly IIOContainer<IOutStream<string>> FContainer;

        public PathOutStream(IIOFactory factory, OutputAttribute attribute)
        {
            attribute.StringType = StringType.Filename;
            FContainer = factory.CreateIOContainer<IOutStream<string>>(attribute, false);
        }

        public void Dispose()
        {
            FContainer.Dispose();
        }

        public override void Flush(bool force = false)
        {
            if (IsChanged)
            {
                var stream = FContainer.IOObject;
                using (var writer = stream.GetDynamicWriter())
                    foreach (var p in this)
                        writer.Write(p);
                stream.Flush();
            }
            base.Flush(force);
        }
    }
}
