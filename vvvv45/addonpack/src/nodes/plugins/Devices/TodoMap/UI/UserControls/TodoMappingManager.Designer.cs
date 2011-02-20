namespace VVVV.TodoMap.UI.UserControls
{
    partial class TodoMappingManager
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tblMain = new System.Windows.Forms.TableLayoutPanel();
            this.tblIO = new System.Windows.Forms.TableLayoutPanel();
            this.grpInputs = new System.Windows.Forms.GroupBox();
            this.grpOutputs = new System.Windows.Forms.GroupBox();
            this.grpVariables = new System.Windows.Forms.GroupBox();
            this.lvVariables = new ListViewEx.ListViewEx();
            this.lvInputs = new ListViewEx.ListViewEx();
            this.lvOutputs = new ListViewEx.ListViewEx();
            this.tblMain.SuspendLayout();
            this.tblIO.SuspendLayout();
            this.grpInputs.SuspendLayout();
            this.grpOutputs.SuspendLayout();
            this.grpVariables.SuspendLayout();
            this.SuspendLayout();
            // 
            // tblMain
            // 
            this.tblMain.ColumnCount = 2;
            this.tblMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblMain.Controls.Add(this.tblIO, 1, 0);
            this.tblMain.Controls.Add(this.grpVariables, 0, 0);
            this.tblMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblMain.Location = new System.Drawing.Point(0, 0);
            this.tblMain.Name = "tblMain";
            this.tblMain.RowCount = 1;
            this.tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblMain.Size = new System.Drawing.Size(611, 346);
            this.tblMain.TabIndex = 0;
            // 
            // tblIO
            // 
            this.tblIO.ColumnCount = 1;
            this.tblIO.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblIO.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblIO.Controls.Add(this.grpInputs, 0, 0);
            this.tblIO.Controls.Add(this.grpOutputs, 0, 1);
            this.tblIO.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblIO.Location = new System.Drawing.Point(308, 3);
            this.tblIO.Name = "tblIO";
            this.tblIO.RowCount = 2;
            this.tblIO.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblIO.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblIO.Size = new System.Drawing.Size(300, 340);
            this.tblIO.TabIndex = 0;
            // 
            // grpInputs
            // 
            this.grpInputs.Controls.Add(this.lvInputs);
            this.grpInputs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpInputs.Location = new System.Drawing.Point(3, 3);
            this.grpInputs.Name = "grpInputs";
            this.grpInputs.Size = new System.Drawing.Size(294, 164);
            this.grpInputs.TabIndex = 0;
            this.grpInputs.TabStop = false;
            this.grpInputs.Text = "Inputs";
            // 
            // grpOutputs
            // 
            this.grpOutputs.Controls.Add(this.lvOutputs);
            this.grpOutputs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpOutputs.Location = new System.Drawing.Point(3, 173);
            this.grpOutputs.Name = "grpOutputs";
            this.grpOutputs.Size = new System.Drawing.Size(294, 164);
            this.grpOutputs.TabIndex = 1;
            this.grpOutputs.TabStop = false;
            this.grpOutputs.Text = "Outputs";
            // 
            // grpVariables
            // 
            this.grpVariables.Controls.Add(this.lvVariables);
            this.grpVariables.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpVariables.Location = new System.Drawing.Point(3, 3);
            this.grpVariables.Name = "grpVariables";
            this.grpVariables.Size = new System.Drawing.Size(299, 340);
            this.grpVariables.TabIndex = 1;
            this.grpVariables.TabStop = false;
            this.grpVariables.Text = "Variables";
            // 
            // lvVariables
            // 
            this.lvVariables.AllowColumnReorder = true;
            this.lvVariables.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvVariables.DoubleClickActivation = true;
            this.lvVariables.FullRowSelect = true;
            this.lvVariables.Location = new System.Drawing.Point(3, 16);
            this.lvVariables.Name = "lvVariables";
            this.lvVariables.Size = new System.Drawing.Size(293, 321);
            this.lvVariables.TabIndex = 0;
            this.lvVariables.UseCompatibleStateImageBehavior = false;
            this.lvVariables.View = System.Windows.Forms.View.Details;
            // 
            // lvInputs
            // 
            this.lvInputs.AllowColumnReorder = true;
            this.lvInputs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvInputs.DoubleClickActivation = true;
            this.lvInputs.FullRowSelect = true;
            this.lvInputs.Location = new System.Drawing.Point(3, 16);
            this.lvInputs.Name = "lvInputs";
            this.lvInputs.Size = new System.Drawing.Size(288, 145);
            this.lvInputs.TabIndex = 0;
            this.lvInputs.UseCompatibleStateImageBehavior = false;
            this.lvInputs.View = System.Windows.Forms.View.Details;
            // 
            // lvOutputs
            // 
            this.lvOutputs.AllowColumnReorder = true;
            this.lvOutputs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvOutputs.DoubleClickActivation = true;
            this.lvOutputs.FullRowSelect = true;
            this.lvOutputs.Location = new System.Drawing.Point(3, 16);
            this.lvOutputs.Name = "lvOutputs";
            this.lvOutputs.Size = new System.Drawing.Size(288, 145);
            this.lvOutputs.TabIndex = 0;
            this.lvOutputs.UseCompatibleStateImageBehavior = false;
            this.lvOutputs.View = System.Windows.Forms.View.Details;
            // 
            // TodoMappingManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tblMain);
            this.Name = "TodoMappingManager";
            this.Size = new System.Drawing.Size(611, 346);
            this.tblMain.ResumeLayout(false);
            this.tblIO.ResumeLayout(false);
            this.grpInputs.ResumeLayout(false);
            this.grpOutputs.ResumeLayout(false);
            this.grpVariables.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tblMain;
        private System.Windows.Forms.TableLayoutPanel tblIO;
        private System.Windows.Forms.GroupBox grpInputs;
        private System.Windows.Forms.GroupBox grpOutputs;
        private System.Windows.Forms.GroupBox grpVariables;
        private ListViewEx.ListViewEx lvInputs;
        private ListViewEx.ListViewEx lvOutputs;
        private ListViewEx.ListViewEx lvVariables;
    }
}
