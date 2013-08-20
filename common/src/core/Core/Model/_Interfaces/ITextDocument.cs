using System;

namespace VVVV.Core.Model
{
    public delegate void TextDocumentHandler(ITextDocument doc, string content);
    
    public interface ITextDocument : IDocument
    {
        /// <summary>
        /// The content of this document as a string.
        /// </summary>
        string TextContent { get; set; }
        
        /// <summary>
        /// This event occurs each time the content of this document changes.
        /// </summary>
        event TextDocumentHandler ContentChanged;

        bool IsDirty { get; }

        bool IsReadOnly { get; set; }
    }
}
