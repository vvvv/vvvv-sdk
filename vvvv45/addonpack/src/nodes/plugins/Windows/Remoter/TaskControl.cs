using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace VVVV.Nodes
{
	public partial class TaskControl : UserControl
	{
		public event ButtonUpHandler OnXButton;
		public event ButtonUpHandler OnExecute;
		public event ButtonUpHandler OnSave;
		
		private bool FEditing = false;
		
		private string FDescription;
		public string Description
		{
			get {return FDescription;}
			set
			{
				DescriptionLabel.Text = value;
				FDescription = value;
			}
		}
		
		public bool Watch
		{
			get {return WatchCheckBox.Checked;}
			set {WatchCheckBox.Checked = value;}
		}
		
		private int FGroupID;
		public int GroupID
		{
			get {return FGroupID;}
			set 
			{
				FGroupID = value;
				GroupDrop.SelectedIndex = value;
			}
		}
		
		private int FProcessID;
		public int ProcessID
		{
			get {return FProcessID;}
			set 
			{
				FProcessID = value;
				ProcessDrop.SelectedIndex = value;
			}
		}
		
		public int Timeout
		{
			get {return (int) TimeOutUpDown.Value;}
			set {TimeOutUpDown.Value = value;}
		}
		
		public TWatchMode WatchMode
		{
			get
			{
				if (WatchRestart.Checked)
					return TWatchMode.Restart;
				else if (WatchReboot.Checked)
					return TWatchMode.Reboot;
				else
					return TWatchMode.Off;
			}
			set
			{
				switch(value)
				{
					case TWatchMode.Off:
						{
							WatchDoNothing.Checked = true;
							break;
						}
					case TWatchMode.Restart:
						{
							WatchRestart.Checked = true;
							break;
						}
					case TWatchMode.Reboot:
						{
							WatchReboot.Checked = true;
							break;
						}
				}
			}
		}
		
		private TTaskType FTaskType;
		public TTaskType TaskType
		{
			get {return FTaskType;}
		}
		
		public System.Windows.Forms.ComboBox GroupDrop
		{
			get {return GroupBox;}
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
			
			Height = 28;
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
				DescriptionEdit.Text = FDescription;
			}
			else
			{
				EditButton.Text = "E";
				Height = 28;
				Description = DescriptionEdit.Text;
				OnSave.Invoke(this);
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
		
		void WatchCheckBoxClick(object sender, EventArgs e)
		{
			OnSave.Invoke(this);
		}
		
		void GroupBoxSelectedIndexChanged(object sender, EventArgs e)
		{
			FGroupID = GroupDrop.SelectedIndex;
		}
		
		void ProcessBoxSelectedIndexChanged(object sender, EventArgs e)
		{
			FProcessID = ProcessDrop.SelectedIndex;
		}
	}
}
