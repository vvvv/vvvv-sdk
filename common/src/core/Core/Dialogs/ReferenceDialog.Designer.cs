/*
 * Created by SharpDevelop.
 * User: tgallery
 * Date: 22.07.2010
 * Time: 20:46
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace VVVV.Core.Dialogs
{
    partial class ReferenceDialog
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
            if (disposing) {
                if (components != null) {
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
        	System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
        	System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
        	this.splitContainer1 = new System.Windows.Forms.SplitContainer();
        	this.tabControl1 = new System.Windows.Forms.TabControl();
        	this.tabPage1 = new System.Windows.Forms.TabPage();
        	this.SearchTextBox = new System.Windows.Forms.TextBox();
        	this.GacDataGridView = new System.Windows.Forms.DataGridView();
        	this.Reference = new System.Windows.Forms.DataGridViewTextBoxColumn();
        	this.VersionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
        	this.tabPage2 = new System.Windows.Forms.TabPage();
        	this.button1 = new System.Windows.Forms.Button();
        	this.ReferenceDataGridView = new System.Windows.Forms.DataGridView();
        	this.SelectedReference = new System.Windows.Forms.DataGridViewTextBoxColumn();
        	this.SelectedReferenceLocationColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
        	this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
        	this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
        	this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
        	this.splitContainer1.Panel1.SuspendLayout();
        	this.splitContainer1.Panel2.SuspendLayout();
        	this.splitContainer1.SuspendLayout();
        	this.tabControl1.SuspendLayout();
        	this.tabPage1.SuspendLayout();
        	((System.ComponentModel.ISupportInitialize)(this.GacDataGridView)).BeginInit();
        	this.tabPage2.SuspendLayout();
        	((System.ComponentModel.ISupportInitialize)(this.ReferenceDataGridView)).BeginInit();
        	this.contextMenuStrip1.SuspendLayout();
        	this.contextMenuStrip2.SuspendLayout();
        	this.SuspendLayout();
        	// 
        	// splitContainer1
        	// 
        	this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
        	this.splitContainer1.Location = new System.Drawing.Point(0, 0);
        	this.splitContainer1.Name = "splitContainer1";
        	this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
        	// 
        	// splitContainer1.Panel1
        	// 
        	this.splitContainer1.Panel1.Controls.Add(this.tabControl1);
        	// 
        	// splitContainer1.Panel2
        	// 
        	this.splitContainer1.Panel2.Controls.Add(this.ReferenceDataGridView);
        	this.splitContainer1.Size = new System.Drawing.Size(511, 603);
        	this.splitContainer1.SplitterDistance = 419;
        	this.splitContainer1.TabIndex = 3;
        	// 
        	// tabControl1
        	// 
        	this.tabControl1.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
        	this.tabControl1.Controls.Add(this.tabPage1);
        	this.tabControl1.Controls.Add(this.tabPage2);
        	this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.tabControl1.Location = new System.Drawing.Point(0, 0);
        	this.tabControl1.Name = "tabControl1";
        	this.tabControl1.SelectedIndex = 0;
        	this.tabControl1.Size = new System.Drawing.Size(511, 419);
        	this.tabControl1.TabIndex = 0;
        	// 
        	// tabPage1
        	// 
        	this.tabPage1.Controls.Add(this.SearchTextBox);
        	this.tabPage1.Controls.Add(this.GacDataGridView);
        	this.tabPage1.Location = new System.Drawing.Point(4, 25);
        	this.tabPage1.Name = "tabPage1";
        	this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
        	this.tabPage1.Size = new System.Drawing.Size(503, 390);
        	this.tabPage1.TabIndex = 0;
        	this.tabPage1.Text = "GAC";
        	this.tabPage1.UseVisualStyleBackColor = true;
        	// 
        	// SearchTextBox
        	// 
        	this.SearchTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
        	this.SearchTextBox.Dock = System.Windows.Forms.DockStyle.Bottom;
        	this.SearchTextBox.Location = new System.Drawing.Point(3, 367);
        	this.SearchTextBox.Name = "SearchTextBox";
        	this.SearchTextBox.Size = new System.Drawing.Size(497, 20);
        	this.SearchTextBox.TabIndex = 1;
        	this.SearchTextBox.Visible = false;
        	this.SearchTextBox.TextChanged += new System.EventHandler(this.SearchTextBoxTextChanged);
        	this.SearchTextBox.Enter += new System.EventHandler(this.SearchTextBoxEnter);
        	this.SearchTextBox.Leave += new System.EventHandler(this.SearchTextBoxLeave);
        	this.SearchTextBox.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.SearchTextBoxPreviewKeyDown);
        	// 
        	// GacDataGridView
        	// 
        	this.GacDataGridView.AllowUserToAddRows = false;
        	this.GacDataGridView.AllowUserToDeleteRows = false;
        	this.GacDataGridView.AllowUserToResizeColumns = false;
        	this.GacDataGridView.AllowUserToResizeRows = false;
        	this.GacDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
        	this.GacDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        	this.GacDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
        	        	        	this.Reference,
        	        	        	this.VersionColumn});
        	dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
        	dataGridViewCellStyle1.BackColor = System.Drawing.Color.Silver;
        	dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
        	dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(216)))), ((int)(((byte)(216)))));
        	dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.Black;
        	dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
        	this.GacDataGridView.DefaultCellStyle = dataGridViewCellStyle1;
        	this.GacDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.GacDataGridView.Location = new System.Drawing.Point(3, 3);
        	this.GacDataGridView.Name = "GacDataGridView";
        	this.GacDataGridView.ReadOnly = true;
        	this.GacDataGridView.RowHeadersVisible = false;
        	this.GacDataGridView.RowTemplate.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(216)))), ((int)(((byte)(216)))));
        	this.GacDataGridView.RowTemplate.Height = 16;
        	this.GacDataGridView.Size = new System.Drawing.Size(497, 384);
        	this.GacDataGridView.StandardTab = true;
        	this.GacDataGridView.TabIndex = 0;
        	this.GacDataGridView.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.GacDataGridViewCellMouseDoubleClick);
        	this.GacDataGridView.SelectionChanged += new System.EventHandler(this.GacDataGridViewSelectionChanged);
        	this.GacDataGridView.MouseEnter += new System.EventHandler(this.GacDataGridView_MouseEnter);
        	// 
        	// Reference
        	// 
        	this.Reference.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
        	this.Reference.HeaderText = "Reference";
        	this.Reference.Name = "Reference";
        	this.Reference.ReadOnly = true;
        	// 
        	// VersionColumn
        	// 
        	this.VersionColumn.HeaderText = "Version";
        	this.VersionColumn.Name = "VersionColumn";
        	this.VersionColumn.ReadOnly = true;
        	// 
        	// tabPage2
        	// 
        	this.tabPage2.BackColor = System.Drawing.SystemColors.ControlDark;
        	this.tabPage2.Controls.Add(this.button1);
        	this.tabPage2.Location = new System.Drawing.Point(4, 25);
        	this.tabPage2.Name = "tabPage2";
        	this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
        	this.tabPage2.Size = new System.Drawing.Size(503, 390);
        	this.tabPage2.TabIndex = 1;
        	this.tabPage2.Text = "Browser";
        	// 
        	// button1
        	// 
        	this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.button1.Location = new System.Drawing.Point(8, 6);
        	this.button1.Name = "button1";
        	this.button1.Size = new System.Drawing.Size(147, 68);
        	this.button1.TabIndex = 0;
        	this.button1.Text = "Browse...";
        	this.button1.UseVisualStyleBackColor = true;
        	this.button1.Click += new System.EventHandler(this.button1_Click);
        	// 
        	// ReferenceDataGridView
        	// 
        	this.ReferenceDataGridView.AllowUserToAddRows = false;
        	this.ReferenceDataGridView.AllowUserToResizeColumns = false;
        	this.ReferenceDataGridView.AllowUserToResizeRows = false;
        	this.ReferenceDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
        	this.ReferenceDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        	this.ReferenceDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
        	        	        	this.SelectedReference,
        	        	        	this.SelectedReferenceLocationColumn});
        	dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
        	dataGridViewCellStyle2.BackColor = System.Drawing.Color.Silver;
        	dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
        	dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(216)))), ((int)(((byte)(216)))));
        	dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.Black;
        	dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
        	this.ReferenceDataGridView.DefaultCellStyle = dataGridViewCellStyle2;
        	this.ReferenceDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.ReferenceDataGridView.Location = new System.Drawing.Point(0, 0);
        	this.ReferenceDataGridView.Name = "ReferenceDataGridView";
        	this.ReferenceDataGridView.ReadOnly = true;
        	this.ReferenceDataGridView.RowHeadersVisible = false;
        	this.ReferenceDataGridView.RowTemplate.Height = 16;
        	this.ReferenceDataGridView.Size = new System.Drawing.Size(511, 180);
        	this.ReferenceDataGridView.TabIndex = 0;
        	this.ReferenceDataGridView.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.ReferenceDataGridViewCellMouseDoubleClick);
        	this.ReferenceDataGridView.SelectionChanged += new System.EventHandler(this.ReferenceDataGridViewSelectionChanged);
        	this.ReferenceDataGridView.MouseEnter += new System.EventHandler(this.ReferenceDataGridView_MouseEnter);
        	// 
        	// SelectedReference
        	// 
        	this.SelectedReference.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
        	this.SelectedReference.HeaderText = "Reference";
        	this.SelectedReference.Name = "SelectedReference";
        	this.SelectedReference.ReadOnly = true;
        	this.SelectedReference.Width = 82;
        	// 
        	// SelectedReferenceLocationColumn
        	// 
        	this.SelectedReferenceLocationColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
        	this.SelectedReferenceLocationColumn.HeaderText = "Location";
        	this.SelectedReferenceLocationColumn.Name = "SelectedReferenceLocationColumn";
        	this.SelectedReferenceLocationColumn.ReadOnly = true;
        	// 
        	// contextMenuStrip1
        	// 
        	this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        	        	        	this.addToolStripMenuItem});
        	this.contextMenuStrip1.Name = "contextMenuStrip1";
        	this.contextMenuStrip1.Size = new System.Drawing.Size(105, 26);
        	// 
        	// addToolStripMenuItem
        	// 
        	this.addToolStripMenuItem.Name = "addToolStripMenuItem";
        	this.addToolStripMenuItem.Size = new System.Drawing.Size(104, 22);
        	this.addToolStripMenuItem.Text = "Add";
        	this.addToolStripMenuItem.Click += new System.EventHandler(this.AddToolStripMenuItemClick);
        	// 
        	// contextMenuStrip2
        	// 
        	this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        	        	        	this.removeToolStripMenuItem});
        	this.contextMenuStrip2.Name = "contextMenuStrip2";
        	this.contextMenuStrip2.Size = new System.Drawing.Size(125, 26);
        	// 
        	// removeToolStripMenuItem
        	// 
        	this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
        	this.removeToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
        	this.removeToolStripMenuItem.Text = "Remove";
        	this.removeToolStripMenuItem.Click += new System.EventHandler(this.RemoveToolStripMenuItemClick);
        	// 
        	// openFileDialog1
        	// 
        	this.openFileDialog1.Filter = ".NET Assemblies (*.dll)|*.dll";
        	this.openFileDialog1.Multiselect = true;
        	this.openFileDialog1.RestoreDirectory = true;
        	// 
        	// ReferenceDialog
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.ClientSize = new System.Drawing.Size(511, 632);
        	this.Controls.Add(this.splitContainer1);
        	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
        	this.KeyPreview = true;
        	this.Name = "ReferenceDialog";
        	this.Text = "Select references";
        	this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ReferenceDialogKeyDown);
        	this.Controls.SetChildIndex(this.splitContainer1, 0);
        	this.splitContainer1.Panel1.ResumeLayout(false);
        	this.splitContainer1.Panel2.ResumeLayout(false);
        	this.splitContainer1.ResumeLayout(false);
        	this.tabControl1.ResumeLayout(false);
        	this.tabPage1.ResumeLayout(false);
        	this.tabPage1.PerformLayout();
        	((System.ComponentModel.ISupportInitialize)(this.GacDataGridView)).EndInit();
        	this.tabPage2.ResumeLayout(false);
        	((System.ComponentModel.ISupportInitialize)(this.ReferenceDataGridView)).EndInit();
        	this.contextMenuStrip1.ResumeLayout(false);
        	this.contextMenuStrip2.ResumeLayout(false);
        	this.ResumeLayout(false);
        }
        private System.Windows.Forms.TextBox SearchTextBox;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.DataGridViewTextBoxColumn VersionColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn SelectedReferenceLocationColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn SelectedReference;
        private System.Windows.Forms.DataGridView ReferenceDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn Reference;
        private System.Windows.Forms.DataGridView GacDataGridView;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        
    }
}
