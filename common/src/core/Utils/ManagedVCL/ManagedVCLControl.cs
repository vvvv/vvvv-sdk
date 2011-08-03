#region usings

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;

using System.Windows.Forms;
using System.Reflection;

#endregion

/// <summary>
/// Utils with handle Delphi ManagedVCL tasks.
/// </summary>
namespace VVVV.Utils.ManagedVCL
{
	/// <summary>
	/// Modified version of http://www.managed-vcl.com/downloads/ManagedVCLControl.zip
	/// Added method ProcessKeyPreview to be able to forward keys like
	/// HOME, INSERT and arrow keys.
	/// Overwrite IsDialogKey to control application specific behavior.
	/// </summary>
	public class TopControl : UserControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		
		private static Keys[] DialogKeys = new Keys[] {
			Keys.Left, Keys.Up, Keys.Right, Keys.Down,
			Keys.Home, Keys.End, Keys.PageUp, Keys.PageDown, Keys.Insert, Keys.Delete};

		public TopControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if( components != null )
					components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// UserControl1
			// 
			this.Name = "UserControl1";
			this.Size = new System.Drawing.Size(352, 312);

		}
		#endregion


		public bool ChildSelectNextControl(ContainerControl ctl, bool forward, bool tabStopOnly, bool nested, bool wrap)
		{
			if (ctl == null || !ctl.CanSelect)
				return false;

			var activeControl = ctl.ActiveControl as ContainerControl;
			if (activeControl != null)
			{
				if (ChildSelectNextControl(activeControl, forward, tabStopOnly, nested, wrap))
					return true;
			}

			return ctl.SelectNextControl(ctl.ActiveControl, forward, tabStopOnly, nested, wrap);
		}

		protected override bool ProcessTabKey(bool forward)
		{
			if (Parent != null)
				return base.ProcessTabKey(forward);

			Control ctrl = ActiveControl;
			var containerControl = ctrl as ContainerControl;

			if (containerControl != null)
			{
				if (ChildSelectNextControl(containerControl, forward, true, true, false))
					return true;
			}
			
			bool result = SelectNextControl(ctrl, forward, true, true, false);
			if (!result)
				ActiveControl = null;
			return result;
		}
		
		private ArrayList GetActiveControls()
		{
			ArrayList ActiveControls = new ArrayList();
			Control act = ActiveControl;
			while (act != null)
			{
				ActiveControls.Add(act);
				var containerControl = act as ContainerControl;
				if (containerControl != null)
					act = containerControl.ActiveControl;
				else
					act = null;
			}
			return ActiveControls;
		}

		private bool m_InProcessDialogKey = false;
		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (m_InProcessDialogKey)
				return false;

			m_InProcessDialogKey = true;
			try
			{
				ArrayList ActiveControls = GetActiveControls();

				for (int i = ActiveControls.Count-1; i >= 0; i--)
				{
					Control ctrl = (Control)ActiveControls[i];
					
					bool result = (bool)(typeof(Control)).InvokeMember("ProcessDialogKey",
					                                                   BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance,
					                                                   null, ctrl, new object[] {keyData});
					if (result)
						return true;
				}

				return base.ProcessDialogKey(keyData);
			}
			finally
			{
				m_InProcessDialogKey = false;
			}
		}
		
		private bool m_ProcessKeyPreview = false;
		protected override bool ProcessKeyPreview (ref Message m)
		{
			if (m_ProcessKeyPreview)
				return false;
			
			m_ProcessKeyPreview = true;
			
			const int WM_KEYDOWN = 0x100;
			
			try
			{
				if (m.Msg == WM_KEYDOWN)
				{
					KeyEventArgs ke = new KeyEventArgs((Keys)m.WParam.ToInt32() | ModifierKeys);
					if (IsDialogKey(ke))
					{
						ArrayList ActiveControls = GetActiveControls();
						
						for (int i = ActiveControls.Count-1; i >= 0; i--)
						{
							Control ctrl = (Control)ActiveControls[i];
							bool result = (bool)(typeof(Control)).InvokeMember("ProcessDialogKey",
							                                                   BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance,
							                                                   null, ctrl, new object[] {ke.KeyData});
							if (result)
								return true;
						}
					}
				}
				
				return base.ProcessKeyPreview(ref m);
			}
			finally
			{
				m_ProcessKeyPreview = false;
			}
		}
		
		private static bool IsDialogKey(KeyEventArgs key)
		{
			if (key.Control)
				return true;
			
			foreach (Keys k in DialogKeys)
			{
				if (key.KeyCode == k)
					return true;
			}
			
			return false;
		}
		
	}
}
