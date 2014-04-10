using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace VVVV.Nodes.Timeliner
{
	public partial class TLEditor : UserControl
	{
		protected bool FTimeChanged = false;
		protected List<TLBaseKeyFrame> FKeyFrames;
		
		public event ExitHandler OnExit;

		public List<TLBaseKeyFrame> KeyFrames
		{
			get {return FKeyFrames;}
		}		
		
		public TLEditor()
		{
			InitializeComponent();
		}

		public TLEditor(List<TLBaseKeyFrame> Keys)
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			FKeyFrames = Keys;
			TimeBox.Minimum = double.MinValue;
			TimeBox.Maximum = double.MaxValue;
		}
		
		public virtual void Exit(bool Accept)
		{
			OnExit(Accept);
		}

		void TimeBoxOnValueChange(double Value)
		{
			FTimeChanged = true;
		}
		
		void TimeBoxKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)13)	//enter
			{
				e.Handled = true;
				Exit(true);			
			}
			else if (e.KeyChar == (char)27)	//escape
			{
				e.Handled = true;
				Exit(false);
			}
		}
	}
	
	public delegate void ExitHandler(bool Accept);
}
