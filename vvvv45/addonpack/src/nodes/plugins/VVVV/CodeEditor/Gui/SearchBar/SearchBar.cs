using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Gui.CompletionWindow;

namespace VVVV.HDE.CodeEditor.Gui.SearchBar
{
	public class SearchBar : UserControl
	{
		private TextEditorControl FTextEditorControl;
		private TextBox FSearchTextBox;
		private IList<TextMarker> FSearchMarkers;
		private int FLastSelectedMarkerIndex;
		
		public SearchBar(TextEditorControl textEditorControl)
		{
			FTextEditorControl  = textEditorControl;
			
			// Setup private fiels
			FSearchMarkers = new List<TextMarker>();
			
			// Setup GUI
			FSearchTextBox = new TextBox();
			FSearchTextBox.Dock = DockStyle.Fill;
			Controls.Add(FSearchTextBox);
			
			SetLocation();
			
			FSearchTextBox.Leave += FSearchTextBox_Leave;
			FSearchTextBox.KeyDown += FSearchTextBox_KeyDown;
			FSearchTextBox.TextChanged += FSearchTextBox_TextChanged;
			
			FTextEditorControl.Controls.Add(this);
		}

		public void ShowSearchBar()
		{
			SetLocation();
			
			Show();
			BringToFront();
			
			FSearchTextBox.Focus();
		}
		
		public void CloseSearchBar()
		{
			Hide();
		}
		
		protected void SetLocation()
		{
			var location = new Point(0, 0);
			var size = new Size(FTextEditorControl.Width, 20);
			Bounds = new Rectangle(location, size);
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
			else if (e.KeyCode == Keys.Enter)
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
						var location = doc.OffsetToPosition(marker.Offset);
						textAreaControl.Caret.Line = location.Line;
						textAreaControl.Caret.Column = location.Column;
						textAreaControl.ScrollToCaret();
						
						foundNextMarker = true;
						break;
					}
				}
				
				
				if (!foundNextMarker && FSearchMarkers.Count > 0)
				{
					// Try from beginning
					var marker = FSearchMarkers[0];
					if (backward)
						marker = FSearchMarkers[FSearchMarkers.Count - 1];
					
					var location = doc.OffsetToPosition(marker.Offset);
					textAreaControl.Caret.Line = location.Line;
					textAreaControl.Caret.Column = location.Column;
					textAreaControl.ScrollToCaret();
				}
				
				e.Handled = true;
			}
		}
		
		void FSearchTextBox_TextChanged(object sender, EventArgs e)
		{
			HighlightSearchText();
		}
		
		protected void HighlightSearchText()
		{
			var doc = FTextEditorControl.Document;
			var textArea = FTextEditorControl.ActiveTextAreaControl.TextArea;
			var markerStrategy = doc.MarkerStrategy;
			
			// Clear all previous markers
			foreach (var marker in FSearchMarkers)
			{
				var markerLocation = doc.OffsetToPosition(marker.Offset);
				
				markerStrategy.RemoveMarker(marker);
				
				doc.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.PositionToLineEnd, markerLocation));
			}
			FSearchMarkers.Clear();
			
			var searchText = FSearchTextBox.Text;
			var searchTextLength = searchText.Length;
			
			if (searchText != string.Empty)
			{
				for (int line = 0; line < doc.TotalNumberOfLines; line++)
				{
					var lineText = TextUtilities.GetLineAsString(doc, line);
					var startIndex = lineText.IndexOf(searchText);
					if (startIndex >= 0)
					{
						var lineSegment = doc.GetLineSegment(line);
						var offset = lineSegment.Offset + startIndex;
						var location = doc.OffsetToPosition(offset);
						
						var marker = new TextMarker(offset, searchTextLength, TextMarkerType.SolidBlock, Color.Beige);
						
						FSearchMarkers.Add(marker);
						markerStrategy.AddMarker(marker);
						
						doc.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.PositionToLineEnd, location));
					}
				}
			}
			
			doc.CommitUpdate();
		}
		
		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					FSearchTextBox.Leave -= FSearchTextBox_Leave;
					FSearchTextBox.KeyDown -= FSearchTextBox_KeyDown;
					FSearchTextBox.TextChanged -= FSearchTextBox_TextChanged;
					FTextEditorControl.Controls.Remove(this);
				}
			}
			
			base.Dispose(disposing);
		}
	}
}
