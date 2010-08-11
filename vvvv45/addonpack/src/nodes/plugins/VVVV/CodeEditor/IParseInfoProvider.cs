
using System;
using ICSharpCode.SharpDevelop.Dom;
using VVVV.Core.Model;
using SD = ICSharpCode.TextEditor.Document;

namespace VVVV.HDE.CodeEditor
{
	public interface IParseInfoProvider
	{
		/// <summary>
		/// Returns the project content (contains parse information) for a IProject.
		/// </summary>
		DefaultProjectContent GetProjectContent(IProject project);
		
		/// <summary>
		/// Returns the parse information for a ITextDocument.
		/// </summary>
		ParseInformation GetParseInfo(ITextDocument doc);
		
		/// <summary>
		/// Returns the parse information for a SharpDevelop document.
		/// </summary>
		ParseInformation GetParseInfo(SD.IDocument doc);
	}
}
