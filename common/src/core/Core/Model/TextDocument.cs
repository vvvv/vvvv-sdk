using System;
using System.IO;
using System.Runtime.Caching;
using System.Collections.Generic;
using VVVV.Core.Logging;

namespace VVVV.Core.Model
{
    public class TextDocument : Document, ITextDocument
    {
        public string TextContent
        {
            get
            {
                Content.Position = 0;
                using (var reader = new LeaveOpenStreamReader(Content))
                    return reader.ReadToEnd();
            }
            set
            {
                var newContent = new MemoryStream();
                using (var writer = new LeaveOpenStreamWriter(newContent))
                    writer.Write(value);
                Content = newContent;
            }
        }
        
        public TextDocument(string name, string path)
            : base(name, path)
        {
        }
    }
}
