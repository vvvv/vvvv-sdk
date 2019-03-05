using System;
using System.Collections;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using Path = VL.Lib.IO.Path;

namespace VVVV.VL.Hosting.IO.Streams
{
    class PathInStream : MemoryIOStream<Path>, IDisposable
    {
        readonly IIOContainer<IInStream<string>> FContainer;

        public PathInStream(IIOFactory factory, InputAttribute attribute)
        {
            attribute.StringType = StringType.Filename;
            FContainer = factory.CreateIOContainer<IInStream<string>>(attribute, false);
        }

        public void Dispose()
        {
            FContainer.Dispose();
        }

        public override bool Sync()
        {
            var stream = FContainer.IOObject;
            var result = IsChanged = stream.Sync();
            if (result)
            {
                using (var writer = this.GetDynamicWriter())
                    foreach (var s in stream)
                        writer.Write((Path)s);
            }
            return result;
        }
    }
}
