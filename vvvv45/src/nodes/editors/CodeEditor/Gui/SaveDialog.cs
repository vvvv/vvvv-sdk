
using System;
using System.Drawing;
using System.Windows.Forms;

namespace VVVV.HDE.CodeEditor.Gui
{
	public enum SaveOption
	{
		Save,
		SaveAs,
		DontSave,
		Cancel
	}
	
	public partial class SaveDialog : Form
	{
		public SaveDialog()
			: this("Filename")
		{
		}
		
		public SaveDialog(string message)
		{
			InitializeComponent();
			
			Message = message;
			StartPosition = FormStartPosition.CenterParent;
		}
		
		public string Message
		{
			get
			{
				return FMessageLabel.Text;
			}
			set
			{
				FMessageLabel.Text = value;
				Width = Math.Min(800, Math.Max(panel1.Width, 24 + FMessageLabel.Width));
				panel1.Width = Width;
				FSaveButton.Location = new Point((Width - panel1.Controls.Count * FSaveButton.Width) / 2, 0);
				FCloseButton.Location = new Point(FSaveButton.Location.X + FSaveButton.Width, 0);
				FCancelButton.Location = new Point(FCloseButton.Location.X + FCloseButton.Width, 0);
			}
		}
		
		public SaveOption SaveOptionResult
		{
			get;
			private set;
		}
		
		void FCancelButtonClick(object sender, EventArgs e)
		{
			SaveOptionResult = SaveOption.Cancel;
		}
		
		void FCloseButtonClick(object sender, EventArgs e)
		{
			SaveOptionResult = SaveOption.DontSave;
		}
		
		void FSaveButtonClick(object sender, EventArgs e)
		{
			SaveOptionResult = SaveOption.Save;
		}
		
		void SaveDialogLoad(object sender, EventArgs e)
		{
			FCloseButton.Select();
		}
	}
}
