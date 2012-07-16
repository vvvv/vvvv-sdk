
using System;
using ICSharpCode.NRefactory;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace VVVV.HDE.CodeEditor
{
	/// <summary>
	/// Used by the CodeEditor to display links (underlined words which can be clicked, like links in a browser).
	/// </summary>
	public interface ILinkDataProvider
	{
		/// <summary>
		/// Returns a struct of type Link which describes where to draw the link and where it points to.
		/// </summary>
		Link GetLink(IDocument document, TextLocation location);
	}
	
	public struct Link
	{
		public static readonly Link Empty = new Link(TextRegion.Empty, null, TextLocation.Empty);
		
		readonly TextRegion hoverRegion;
		readonly string fileName;
		readonly TextLocation location;
		
		public Link(TextRegion hoverRegion, string fileName, TextLocation location)
		{
			this.hoverRegion = hoverRegion;
			this.fileName = fileName;
			this.location = location;
		}
		
		public bool IsEmpty
		{
			get
			{
				return hoverRegion.IsEmpty;
			}
		}
		
		public TextRegion HoverRegion
		{
			get
			{
				return hoverRegion;
			}
		}
		
		public string FileName
		{
			get
			{
				return fileName;
			}
		}
		
		public TextLocation Location
		{
			get
			{
				return location;
			}
		}
	}
	
	public struct TextRegion
	{
		public static readonly TextRegion Empty = new TextRegion(-1, -1, -1, -1);
		
		readonly int beginLine;
		readonly int endLine;
		readonly int beginColumn;
		readonly int endColumn;
		
		public int BeginLine
		{
			get
			{
				return beginLine;
			}
		}
		
		public int EndLine
		{
			get
			{
				return endLine;
			}
		}
		
		public int BeginColumn
		{
			get
			{
				return beginColumn;
			}
		}
		
		public int EndColumn
		{
			get
			{
				return endColumn;
			}
		}
		
		public TextRegion(int beginLine, int beginColumn, int endLine, int endColumn)
		{
			this.beginLine = beginLine;
			this.beginColumn = beginColumn;
			this.endLine = endLine;
			this.endColumn = endColumn;
		}
		
		public bool IsEmpty
		{
			get
			{
				return BeginLine < 0;
			}
		}
	}
	
	public static class RegionExtensions
	{
		public static TextRegion ToTextRegion(this DomRegion domRegion)
		{
			return new TextRegion(domRegion.BeginLine - 1, domRegion.BeginColumn - 1, domRegion.EndLine - 1, domRegion.EndColumn - 1);
		}
		
		public static TextLocation ToTextLocation(this TextRegion textRegion)
		{
			return new TextLocation(textRegion.BeginColumn, textRegion.BeginLine);
		}
		
		public static TextLocation ToTextLocation(this Location location)
		{
			return new TextLocation(location.Column - 1, location.Line - 1);
		}
	}
}
