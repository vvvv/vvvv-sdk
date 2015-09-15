using System;
using System.IO;
using System.Runtime.Caching;
using System.Collections.Generic;
using VVVV.Core.Logging;
using System.Text;

namespace VVVV.Core.Model
{
    public class TextDocument : Document, ITextDocument
    {
        public string TextContent
        {
            get
            {
                Content.Position = 0;
                using (var reader = new StreamReader(Content, Encoding.Default, true, 4096, true))
                    return reader.ReadToEnd();
            }
            set
            {
                var newContent = new MemoryStream();
                using (var writer = new StreamWriter(newContent, Encoding.Default, 4096, true))
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
