using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Gui.CompletionWindow;

namespace VVVV.HDE.CodeEditor.Gui
{
	public class SearchBar : UserControl
	{
		private TextEditorControl FTextEditorControl;
		private TextBox FSearchTextBox;
		private IList<TextMarker> FSearchMarkers;
		
		public SearchBar(TextEditorControl textEditorControl)
		{
			// Setup private fiels
			FTextEditorControl  = textEditorControl;
			FSearchMarkers = new List<TextMarker>();
			
			// Setup GUI
			FSearchTextBox = new TextBox();
			Controls.Add(FSearchTextBox);
			
			// Setup event callbacks
			FSearchTextBox.Enter += FSearchTextBox_Enter;
			FSearchTextBox.Leave += FSearchTextBox_Leave;
			FSearchTextBox.KeyDown += FSearchTextBox_KeyDown;
			FSearchTextBox.TextChanged += FSearchTextBox_TextChanged;
			
			FTextEditorControl.ActiveTextAreaControl.Resize += FTextEditorControl_ActiveTextAreaControl_Resize;
		}

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					FSearchTextBox.Enter -= FSearchTextBox_Enter;
					FSearchTextBox.Leave -= FSearchTextBox_Leave;
					FSearchTextBox.KeyDown -= FSearchTextBox_KeyDown;
					FSearchTextBox.TextChanged -= FSearchTextBox_TextChanged;
					FTextEditorControl.ActiveTextAreaControl.Resize -= FTextEditorControl_ActiveTextAreaControl_Resize;
					FTextEditorControl.Controls.Remove(this);
				}
			}
			
			base.Dispose(disposing);
		}

		protected void UpdateControlBounds()
		{
			int leftOffset = 0;
			foreach (var margin in FTextEditorControl.ActiveTextAreaControl.TextArea.LeftMargins)
			{
				if (margin.IsVisible)
					leftOffset += margin.Size.Width;
			}
			
			var maxWidth = FTextEditorControl.Width - leftOffset - SystemInformation.HorizontalScrollBarArrowWidth;
			var location = new Point(leftOffset + maxWidth / 2, 0);
			var size = new Size(maxWidth / 2, 20);
			Bounds = new Rectangle(location, size);
			
			FSearchTextBox.Bounds = new Rectangle(0, 0, Width, Height);
		}
		
		public bool CaseSensitive
		{
			get;
			set;
		}

		public void ShowSearchBar()
		{
			UpdateControlBounds();
			
			Show();
			BringToFront();
			
			FSearchTextBox.Focus();
		}
		
		public void CloseSearchBar()
		{
            this.ClearMarkers();
			Hide();
		}
		
		void FSearchTextBox_Enter(object sender, EventArgs e)
		{
			FSearchTextBox.SelectAll();
		}
		
		void FSearchTextBox_Leave(object sender, EventArgs e)
		{
			CloseSearchBar();
		}
		
		void FSearchTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				CloseSearchBar();
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.F3)
			{
				// Find marker for current caret
				var doc = FTextEditorControl.Document;
				var textAreaControl = FTextEditorControl.ActiveTextAreaControl;
				var caret = textAreaControl.Caret;
				
				// Search backward if SHIFT is pressed
				bool backward = e.Shift;
				
				IEnumerable<TextMarker> markers = FSearchMarkers;
				if (backward)
					markers = FSearchMarkers.Reverse();
				
				bool foundNextMarker = false;
				foreach (var marker in markers)
				{
					var result = marker.Offset > caret.Offset;
					if (backward)
						result = marker.Offset < caret.Offset;
					
					if (result)
					{
						foundNextMarker = true;
						JumpToOffset(textAreaControl, doc, marker.Offset);
						break;
					}
				}
				
				if (!foundNextMarker && FSearchMarkers.Count > 0)
				{
					// Try from beginning
					var marker = FSearchMarkers[0];
					if (backward)
						marker = FSearchMarkers[FSearchMarkers.Count - 1];
					
					JumpToOffset(textAreaControl, doc, marker.Offset);
				}
				
				e.Handled = true;
			}
		}
		
		void FSearchTextBox_TextChanged(object sender, EventArgs e)
		{
			HighlightSearchText();
		}
		
		void FTextEditorControl_ActiveTextAreaControl_Resize(object sender, EventArgs e)
		{
			UpdateControlBounds();
		}

        private void ClearMarkers()
        {
            var doc = FTextEditorControl.Document;

            foreach (var marker in FSearchMarkers)
            {
                var markerLocation = doc.OffsetToPosition(marker.Offset);

                doc.MarkerStrategy.RemoveMarker(marker);

                doc.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.PositionToLineEnd, markerLocation));
            }
            FSearchMarkers.Clear();
        }
		
		protected void JumpToOffset(TextAreaControl textAreaControl, IDocument doc, int offset)
		{
			var location = doc.OffsetToPosition(offset);
			textAreaControl.Caret.Line = location.Line;
			textAreaControl.Caret.Column = location.Column;
			textAreaControl.ScrollToCaret();
		}
		
		protected void HighlightSearchText()
		{
			var doc = FTextEditorControl.Document;
			var textArea = FTextEditorControl.ActiveTextAreaControl.TextArea;
			var markerStrategy = doc.MarkerStrategy;
			
			// Clear all previous markers
            this.ClearMarkers();

			var searchText = FSearchTextBox.Text;
			var searchTextLength = searchText.Length;
			var stringComparison = CaseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
			
			if (searchText != string.Empty)
			{
				for (int line = 0; line < doc.TotalNumberOfLines; line++)
				{
					var lineText = TextUtilities.GetLineAsString(doc, line);
					var startIndex = lineText.IndexOf(searchText, stringComparison);
					if (startIndex >= 0)
					{
						var lineSegment = doc.GetLineSegment(line);
						var offset = lineSegment.Offset + startIndex;
						var location = doc.OffsetToPosition(offset);

                        var marker = new TextMarker(offset, searchTextLength, TextMarkerType.SolidBlock, doc.HighlightingStrategy.GetColorFor("SearchResult").BackgroundColor);
						
						FSearchMarkers.Add(marker);
						markerStrategy.AddMarker(marker);
						
						doc.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.PositionToLineEnd, location));
					}
				}
			}
			
			doc.CommitUpdate();
		}
	}
}
