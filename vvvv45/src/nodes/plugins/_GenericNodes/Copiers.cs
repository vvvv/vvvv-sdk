using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes
{
    public static class Copiers
    {
        public static readonly Copier<Stream> Raw = new RawCopier();
        public static readonly Copier<EnumEntry> EnumEntry = new EnumEntryCopier();

        class RawCopier : Copier<Stream>
        {
            public override Stream Copy(Stream value)
            {
                if (value != null)
                {
                    var result = new MemoryStream((int)value.Length);
                    value.Position = 0;
                    value.CopyTo(result);
                    return result;
                }
                return null;
            }
        }

        class EnumEntryCopier : Copier<EnumEntry>
        {
            public override EnumEntry Copy(EnumEntry value)
            {
                if (value != null)
                    return new EnumEntry(value.EnumName, value.Index);
                return null;
            }
        }
    }
}
