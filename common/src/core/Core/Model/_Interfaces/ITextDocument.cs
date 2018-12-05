using System;

namespace VVVV.Core.Model
{
    public interface ITextDocument : IDocument
    {
        /// <summary>
        /// The content of this document as a string.
        /// </summary>
        string TextContent { get; set; }
    }
}
