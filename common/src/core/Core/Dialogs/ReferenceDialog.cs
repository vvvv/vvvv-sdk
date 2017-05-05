using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using VVVV.Core.Model;
using VVVV.Utils;

namespace VVVV.Core.Dialogs
{
    /// <summary>
    /// TODO: Use TableViewer.
    /// </summary>
    public partial class ReferenceDialog : BaseDialog
    {
        protected IList<string[]> FGacAssemblies;
        protected IList<IReference> FReferences;
        protected BackgroundWorker FWorker;

        public IEnumerable<IReference> References
        {
            get
            {
                return FReferences;
            }
        }
        
        public ReferenceDialog()
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();
            
            FGacAssemblies = new List<string[]>();
            FReferences = new List<IReference>();
            
            FWorker = new BackgroundWorker();
            FWorker.WorkerSupportsCancellation = true;
            FWorker.WorkerReportsProgress = false;
            FWorker.DoWork += new DoWorkEventHandler(FWorker_DoWork);
            FWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(FWorker_RunWorkerCompleted);
            FWorker.RunWorkerAsync();
        }
        
        void FWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            foreach (var entry in FGacAssemblies)
            {
                var row = new DataGridViewRow();
                var cell = new DataGridViewTextBoxCell();
                cell.Value = entry[1];
                
                row.Cells.Add(cell);
                
                cell = new DataGridViewTextBoxCell();
                cell.Value = entry[2];
                
                row.Cells.Add(cell);
                
                row.ContextMenuStrip = contextMenuStrip1;
                
                GacDataGridView.Rows.Add(row);
            }
            
            GacDataGridView.Sort(GacDataGridView.Columns[0], ListSortDirection.Ascending);
        }

        void FWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var assemblyCacheEnum = new AssemblyCacheEnum(null);
            var assemblyName = assemblyCacheEnum.GetNextAssembly();
            while (assemblyName != null)
            {
                var details = assemblyName.Split(',');
                
                var shortName = details[0].Trim();
                var version = details[1].Trim().Split('=')[1];
                
                FGacAssemblies.Add(new string[] { assemblyName, shortName, version });
                
                assemblyName = assemblyCacheEnum.GetNextAssembly();
            }
        }
        
		private void AddGacReference(string name)
		{
			var path = AssemblyCache.QueryAssemblyInfo(name);
			var reference = new AssemblyReference(path, true);
			FReferences.Add(reference);
		}
        
        void AddToolStripMenuItemClick(object sender, System.EventArgs e)
        {
            foreach (DataGridViewRow row in GacDataGridView.SelectedRows)
            {
                var newRow = new DataGridViewRow();
                var cell = new DataGridViewTextBoxCell();
                cell.Value = row.Cells[0].Value;
                
                newRow.Cells.Add(cell);
                
                cell = new DataGridViewTextBoxCell();
                cell.Value = "GAC";
                
                newRow.Cells.Add(cell);
                
                newRow.ContextMenuStrip = contextMenuStrip2;
                
                ReferenceDataGridView.Rows.Add(newRow);
                
                AddGacReference(row.Cells[0].Value as string);
            }
            
            ReferenceDataGridView.Sort(ReferenceDataGridView.Columns[0], ListSortDirection.Ascending);
        }
        
        protected void GacDataGridViewSelectionChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewCell cell in GacDataGridView.SelectedCells)
            {
                cell.OwningRow.Selected = true;
            }
        }
        
        void ReferenceDataGridViewSelectionChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewCell cell in ReferenceDataGridView.SelectedCells)
            {
                cell.OwningRow.Selected = true;
            }
        }
        
        void RemoveToolStripMenuItemClick(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in ReferenceDataGridView.SelectedRows)
            {
                FReferences.RemoveAt(row.Index);
                ReferenceDataGridView.Rows.Remove(row);
            }
        }
        
        void ReferenceDialogKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.F))
            {
                SearchTextBox.Visible = true;
                SearchTextBox.Focus();
                e.Handled = true;
            }
        }
        
        void SearchTextBoxLeave(object sender, EventArgs e)
        {
            SearchTextBox.Visible = false;
        }
        
        void SearchTextBoxTextChanged(object sender, EventArgs e)
        {
            var text = SearchTextBox.Text.ToLower();
            foreach (DataGridViewRow row in GacDataGridView.Rows)
            {
                var shortName = row.Cells[0].Value as string;
                row.Visible = shortName.ToLower().Contains(text);
            }
        }
        
        protected void SearchTextBoxPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyData == Keys.Escape)
            {
                e.IsInputKey = true;
                SearchTextBox.Visible = false;
                GacDataGridView.Focus();
            }
        }
        
        void SearchTextBoxEnter(object sender, EventArgs e)
        {
            SearchTextBox.SelectAll();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            foreach (var file in openFileDialog1.FileNames)
            {
                ReferenceDataGridView.Rows.Add(Path.GetFileName(file), Path.GetDirectoryName(file));

                FReferences.Add(new AssemblyReference(file));
            }
        }

        private void GacDataGridView_MouseEnter(object sender, EventArgs e)
        {
            GacDataGridView.Focus();
        }

        private void ReferenceDataGridView_MouseEnter(object sender, EventArgs e)
        {
            ReferenceDataGridView.Focus();
        }
        
        void ReferenceDataGridViewCellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
        	if (e.RowIndex < 0 || e.RowIndex >= ReferenceDataGridView.Rows.Count) return;
            
            if (e.Button == MouseButtons.Left)
            {
                ReferenceDataGridView.Rows.RemoveAt(e.RowIndex);
                FReferences.RemoveAt(e.RowIndex);
            }
        }
        
        void GacDataGridViewCellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
        	if (e.RowIndex < 0 || e.RowIndex >= GacDataGridView.Rows.Count) return;
            
            if (e.Button == MouseButtons.Left)
            {
                var row = GacDataGridView.Rows[e.RowIndex];
                ReferenceDataGridView.Rows.Add(row.Cells[0].Value, "GAC");
                AddGacReference(row.Cells[0].Value as string);
            }
        }
    }
}
