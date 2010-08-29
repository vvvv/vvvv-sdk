
using System;
using System.Drawing;
using System.Windows.Forms;

namespace VVVV.HDE.CodeEditor.Gui.Dialogs
{
	public enum SaveOption
	{
		SaveAll,
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
		
		void FSaveAllButtonClick(object sender, EventArgs e)
		{
			SaveOptionResult = SaveOption.SaveAll;
		}
		
		void SaveDialogLoad(object sender, EventArgs e)
		{
			FCloseButton.Select();
		}
	}
}
