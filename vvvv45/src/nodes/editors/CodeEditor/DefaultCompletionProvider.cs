using System;
using System.Windows.Forms;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Gui.CompletionWindow;

namespace VVVV.HDE.CodeEditor
{
	/// <summary>
	/// Used by the CompletionWindow.
	/// </summary>
	public class DefaultCompletionProvider : ICompletionDataProvider
	{
		public DefaultCompletionProvider()
		{
		}
		
		public virtual ImageList ImageList 
		{
			get 
			{
				return CodeEditorPlugin.CompletionIcons;
			}
		}
		
		private string FPreSelection;
		
		/// <summary>
		/// From: http://community.sharpdevelop.net/forums/p/9921/27514.aspx#27514
		/// The "PreSelection" is the string before the caret that should be included 
		/// in the 'active word' logic in completion window (without the character typed).
		/// Returning null from PreSelection disables the feature, so nothing will be 
		/// initially selected (unless you also use DefaultIndex).
		/// If you return string.Empty from PreSelection, the character being typed 
		/// will be included in the 'active word' and an entry starting with that 
		/// character will be selected.
		/// </summary>
		public string PreSelection {
			get 
			{
				return FPreSelection;
			}
			protected set
			{
				FPreSelection = value;
			}
		}
		
		private int FDefaultIndex = -1;
		public int DefaultIndex 
		{
			get 
			{
				return FDefaultIndex;
			}
			protected set
			{
				FDefaultIndex = value;
			}
		}
		
		public virtual CompletionDataProviderKeyResult ProcessKey(char key)
        {
			if (char.IsLetterOrDigit(key) || key == '_')
            {
                return CompletionDataProviderKeyResult.NormalKey;
            }
            else
            {
                // key triggers insertion of selected items
                return CompletionDataProviderKeyResult.InsertionKey;
            }
        }
        
        /// <summary>
        /// Called when entry should be inserted. Forward to the insertion action of the completion data.
        /// </summary>
        public virtual bool InsertAction(ICompletionData data, TextArea textArea, int insertionOffset, char key)
        {
        	textArea.Caret.Position = textArea.Document.OffsetToPosition(insertionOffset);
            return data.InsertAction(textArea, key);
        }
		
		public virtual ICompletionData[] GenerateCompletionData(string fileName, TextArea textArea, char charTyped)
		{
			return new ICompletionData[0];
		}
	}
}
