/*
 * Created by SharpDevelop.
 * User: tgallery
 * Date: 16.07.2010
 * Time: 15:14
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace VVVV.HDE.Viewer.WinFormsViewer
{
    partial class TableViewer
    {
        /// <summary>
        /// Designer variable used to keep track of non-visual components.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        
        /// <summary>
        /// This method is required for Windows Forms designer support.
        /// Do not change the method contents inside the source code editor. The Forms designer might
        /// not be able to load this method if it was changed manually.
        /// </summary>
        private void InitializeComponent()
        {
        	System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
        	System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
        	this.FDataGridView = new System.Windows.Forms.DataGridView();
        	((System.ComponentModel.ISupportInitialize)(this.FDataGridView)).BeginInit();
        	this.SuspendLayout();
        	// 
        	// FDataGridView
        	// 
        	this.FDataGridView.AllowUserToAddRows = false;
        	this.FDataGridView.AllowUserToDeleteRows = false;
        	this.FDataGridView.AllowUserToResizeRows = false;
        	this.FDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        	this.FDataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
        	this.FDataGridView.BackgroundColor = System.Drawing.Color.DimGray;
        	this.FDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
        	this.FDataGridView.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
        	this.FDataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
        	dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
        	dataGridViewCellStyle1.BackColor = System.Drawing.Color.DarkGray;
        	dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
        	dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
        	dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
        	dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
        	this.FDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
        	this.FDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        	this.FDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.FDataGridView.GridColor = System.Drawing.Color.Gray;
        	this.FDataGridView.Location = new System.Drawing.Point(0, 0);
        	this.FDataGridView.Name = "FDataGridView";
        	this.FDataGridView.RowHeadersVisible = false;
        	dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
        	dataGridViewCellStyle2.BackColor = System.Drawing.Color.Silver;
        	dataGridViewCellStyle2.ForeColor = System.Drawing.Color.Black;
        	dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
        	dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.Black;
        	this.FDataGridView.RowsDefaultCellStyle = dataGridViewCellStyle2;
        	this.FDataGridView.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
        	this.FDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        	this.FDataGridView.Size = new System.Drawing.Size(556, 430);
        	this.FDataGridView.TabIndex = 0;
        	this.FDataGridView.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.FDataGridViewCellMouseClick);
        	this.FDataGridView.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.FDataGridViewCellMouseDoubleClick);
        	// 
        	// TableViewer
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.Controls.Add(this.FDataGridView);
        	this.Margin = new System.Windows.Forms.Padding(0);
        	this.Name = "TableViewer";
        	this.Size = new System.Drawing.Size(556, 430);
        	this.BackColorChanged += new System.EventHandler(this.TableViewer_BackColorChanged);
        	((System.ComponentModel.ISupportInitialize)(this.FDataGridView)).EndInit();
        	this.ResumeLayout(false);
        }
        private System.Windows.Forms.DataGridView FDataGridView;
        
        
    }
}
