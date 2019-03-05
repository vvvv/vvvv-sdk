using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VVVV.Core.Model
{
    public class ContentChangedEventArgs : EventArgs
    {
        public readonly Stream NewContent;

        public ContentChangedEventArgs(Stream newContent)
        {
            this.NewContent = newContent;
        }
    }
}
