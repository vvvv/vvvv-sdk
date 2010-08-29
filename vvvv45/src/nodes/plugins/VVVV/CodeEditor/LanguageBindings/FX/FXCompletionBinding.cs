
using System;
using VVVV.Core.Logging;

namespace VVVV.HDE.CodeEditor.LanguageBindings.FX
{
	/// <summary>
	/// Description of FXCompletionBinding.
	/// </summary>
	public class FXCompletionBinding : ICompletionBinding
	{
		private FXCompletionProvider FCompletionProvider;
		
		public FXCompletionBinding(IDocumentLocator documentLocator, ILogger logger)
		{
			FCompletionProvider = new FXCompletionProvider(documentLocator, logger);
		}
		
		public void HandleKeyPress(CodeEditor editor, char key)
		{
			if (char.IsLetter(key))
			{
				editor.ShowCompletionWindow(FCompletionProvider, key);
			}
		}
	}
}
