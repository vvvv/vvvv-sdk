using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace VVVV.Nodes
{
	/// <summary>
	/// Description of ProcessControl2.
	/// </summary>
	public partial class TaskControl : UserControl
	{
		public event ButtonUpHandler OnXButton;
		public event ButtonUpHandler OnExecute;
		
		private bool FEditing = false;
		
		private string FTaskName;
		public string TaskName
		{
			get {return FTaskName;}
			set 
			{
				NameLabel.Text = value;
				FTaskName = value;
			}
		}
		
		private TTaskType FTaskType;
		public TTaskType TaskType
		{
			get {return FTaskType;}
		}
		
		public System.Windows.Forms.ComboBox SelectionDrop
		{
			get {return SelectionBox;}
		}
		
		public System.Windows.Forms.ComboBox ProcessDrop
		{
			get {return ProcessBox;}
		}
		
		public TaskControl()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			Height = 25;
		}
		
		void DeleteButtonClick(object sender, EventArgs e)
		{
			OnXButton.Invoke(this);
		}
		
		void EditButtonClick(object sender, EventArgs e)
		{
			FEditing = !FEditing;
			
			if (FEditing)
			{
				EditButton.Text = "S";
				Height = 135;
				NameEdit.Text = FTaskName;
			}
			else
			{
				EditButton.Text = "E";
				Height = 25;
				TaskName = NameEdit.Text;
			}			
		}
		
		void StartButtonClick(object sender, EventArgs e)
		{
			FTaskType = TTaskType.Start;
			OnExecute(this);
		}
		
		void RestartButtonClick(object sender, EventArgs e)
		{
			FTaskType = TTaskType.Restart;
			OnExecute(this);
		}
		
		void KillButtonClick(object sender, EventArgs e)
		{
			FTaskType = TTaskType.Kill;
			OnExecute(this);
		}
	}
}
