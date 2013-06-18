/*
 * Created by SharpDevelop.
 * User: Joreg
 * Date: 31.10.2008
 * Time: 19:02
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace VVVV.HDE.GraphicalEditing
{
	partial class GraphEditor
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the control.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				// Piccolo has some static fields which reference the current canvas.
				// Canvas references us through events -> we never get cleanup.
				// Therefor we must unsubscribe from all events.
				FCanvas.RemoveInputEventListener(FSelectionEventHandler);
				FCanvas.RemoveInputEventListener(FDragDropEventHandler);
				FCanvas.RemoveInputEventListener(FMyPanEventHandler);
				FCanvas.RemoveInputEventListener(FMyZoomEventHandler);
				FCanvas.RemoveInputEventListener(FPathEventHandler);
				FCanvas.RemoveInputEventListener(FEventPassThrougHandler);
				
				FCanvas.KeyPress -= FCanvas_KeyPress;
            	FCanvas.KeyDown -= FCanvas_KeyDown;
            	FCanvas.KeyUp -= FCanvas_KeyUp;
				
				if (components != null) 
				{
					components.Dispose();
				}
			}
			
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.FCanvas = new Piccolo.NET.PCanvas();
			this.FToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.SuspendLayout();
			// 
			// FCanvas
			// 
			this.FCanvas.AllowDrop = true;
			this.FCanvas.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
			this.FCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FCanvas.GridFitText = false;
			this.FCanvas.Location = new System.Drawing.Point(0, 0);
			this.FCanvas.Name = "FCanvas";
			this.FCanvas.RegionManagement = true;
			this.FCanvas.Size = new System.Drawing.Size(721, 336);
			this.FCanvas.TabIndex = 1;
			this.FCanvas.Text = "pCanvas1";
			// 
			// FToolTip
			// 
			this.FToolTip.BackColor = System.Drawing.Color.Gray;
			this.FToolTip.ForeColor = System.Drawing.Color.White;
			// 
			// GraphEditor
			// 
			this.Controls.Add(this.FCanvas);
			this.Name = "GraphEditor";
			this.Size = new System.Drawing.Size(721, 336);
			this.ResumeLayout(false);
        }
		private System.Windows.Forms.ToolTip FToolTip;
		private Piccolo.NET.PCanvas FCanvas;
	}
}
