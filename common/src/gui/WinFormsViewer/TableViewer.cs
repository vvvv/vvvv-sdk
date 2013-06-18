using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.Core.View.Table;
using VVVV.Core.Collections.Sync;

namespace VVVV.HDE.Viewer.WinFormsViewer
{
    /// <summary>
    /// TableViewer works with the following interfaces:
    /// - IEnumerable<Column> to get column header information.
    /// - IEnumerable to get rows.
    /// - IEnumerable<ICell> to get cells for a row.
    /// </summary>
    public partial class TableViewer : Viewer
    {
        private ModelMapper FMapper;
        private Synchronizer<object, object> FRowSynchronizer;
        
        public new event ClickHandler Click;
        protected void OnClick(ModelMapper sender, MouseEventArgs e)
        {
            if (Click != null)
                Click(sender, e);
        }
        
        public new event ClickHandler DoubleClick;
        protected void OnDoubleClick(ModelMapper sender, MouseEventArgs e)
        {
            if (DoubleClick != null)
                DoubleClick(sender, e);
        }
        
        public TableViewer()
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();
            
            FDataGridView.CellValidating += HandleCellValidating;
            FDataGridView.CellValueChanged += HandleCellValueChanged;
        }

        void HandleCellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            DebugHelpers.CatchAndLogNeverStop(() =>
            {
                var gridViewCell = FDataGridView[e.ColumnIndex, e.RowIndex];
                var cell = gridViewCell.Tag as ICell;
                var value = Convert.ChangeType(gridViewCell.FormattedValue, cell.ValueType);

                if (!cell.ReadOnly)
                {
                    if (cell.AcceptsValue(value))
                    {
                        cell.Value = value;
                    }
                }
            }, "Cell Value Changed");
        }

        void HandleCellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            DebugHelpers.CatchAndLogNeverStop(() =>
            {            
                var gridViewCell = FDataGridView[e.ColumnIndex, e.RowIndex];
                var cell = gridViewCell.Tag as ICell;
                var value = Convert.ChangeType(gridViewCell.FormattedValue, cell.ValueType);

                if (!cell.ReadOnly)
                {
                    if (!cell.AcceptsValue(value))
                    {
                        FDataGridView.CancelEdit();
                    }
                }
            }, "Cell Validating");
        }

        public int RowCount
        {
            get
            {
                return FDataGridView.RowCount;
            }
        }
        
        public int ColumnCount
        {
            get
            {
                return FDataGridView.ColumnCount;
            }
        }
        
        public DataGridViewSelectionMode SelectMode
        {
            get
            {
                return FDataGridView.SelectionMode;
            }
            set
            {
                FDataGridView.SelectionMode = value;
            }
        }
        
        public override void Reload()
        {
            FDataGridView.Rows.Clear();
            FDataGridView.Columns.Clear();
            
            if (FMapper != null)
                FMapper.Dispose();
            
            if (FRowSynchronizer != null)
            {
                FRowSynchronizer.Dispose();
            }
            
            FMapper = new ModelMapper(Model, Registry);
            
            if (FMapper.CanMap<IEnumerable<Column>>())
            {
                try
                {
                    var columns = FMapper.Map<IEnumerable<Column>>();
                    
                    foreach (var col in columns) {
                        var column = new DataGridViewColumn(new DataGridViewTextBoxCell());
                        column.Name = col.Name;
                        switch (col.AutoSizeMode)
                        {
                            case AutoSizeColumnMode.AllCells:
                                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                                break;
                            case AutoSizeColumnMode.ColumnHeader:
                                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                                break;
                            case AutoSizeColumnMode.DisplayedCells:
                                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                                break;
                            case AutoSizeColumnMode.Fill:
                                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                                break;
                            default:
                                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.NotSet;
                                break;
                        }
                        FDataGridView.Columns.Add(column);
                    }
                    
                    if (FDataGridView.Columns.Count > 0)
                    {
                        var entries = FMapper.Map<IEnumerable>();
                        FRowSynchronizer = FDataGridView.Rows.SyncWith(entries, CreateRow, (r) => { }, null, FRowSynchronizer_Synced);
                    }
                }
                catch (Exception e)
                {
                    Shell.Instance.Logger.Log(e);
                }
            }
        }

        void FRowSynchronizer_Synced(object sender, SyncEventArgs<object, object> args)
        {
            switch (args.Action)
            {
                case CollectionAction.Added:
                    break;
                case CollectionAction.Removed:
                    break;
                case CollectionAction.Cleared:
                    break;
                case CollectionAction.OrderChanged:
                    break;
                case CollectionAction.Updating:
                    ((ISupportInitialize)FDataGridView).BeginInit();
                    break;
                case CollectionAction.Updated:
                    ((ISupportInitialize)FDataGridView).EndInit();
                    break;
                default:
                    break;
            }
        }

        void FDataGridViewCellMouseDoubleClick(object sender, System.Windows.Forms.DataGridViewCellMouseEventArgs e)
        {
        	if (e.RowIndex >= 0 && e.RowIndex < FDataGridView.Rows.Count)
            {
                var mapper = new ModelMapper(FDataGridView.Rows[e.RowIndex].Tag, Registry);
                OnDoubleClick(mapper, null);
                if (e.Button == MouseButtons.Left)
	        	{
	      			FDataGridView.CurrentCell = FDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
	        		FDataGridView.BeginEdit(true);
	        	}
            }
        }
        
        void FDataGridViewCellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
        	if (e.Button == MouseButtons.Right)
        	{
      			FDataGridView.CurrentCell = FDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
        		FDataGridView.BeginEdit(true);
        	}
        }
        
        object CreateRow(object entry)
        {
            var row = new DataGridViewRow();
            
            var childMapper = FMapper.CreateChildMapper(entry);
            if (childMapper.CanMap<IEnumerable<ICell>>())
            {
                row.Tag = entry;
                
                var cells = childMapper.Map<IEnumerable<ICell>>();
                
                int cellCount = 0;
                foreach (var cell in cells)
                {
                    var gridViewCell = CreateGridViewCell(cell);
                    
                    cell.ValueChanged += HandleCellValueChanged;
                    
                    if (cell.WrapContent)
                    {
                        gridViewCell.Style.WrapMode = DataGridViewTriState.True;
                    }
                    
                    gridViewCell.Value = cell.Value;
                    
                    row.Cells.Add(gridViewCell);
                    
                    // Following properties can only be set after cell has been added to a row.
                    gridViewCell.ReadOnly = cell.ReadOnly;
                    gridViewCell.Tag = cell;
                    
                    cellCount++;
                }
                
                if (row.Cells.Count > 0)
                    return row;
            }
            
            var emptyGridViewCell = new DataGridViewTextBoxCell();
            emptyGridViewCell.Value = entry.ToString();
            row.Tag = entry;
            row.Cells.Add(emptyGridViewCell);
            
            return row;
        }
        
        DataGridViewCell CreateGridViewCell(ICell cell)
        {
            // TODO: Handle different data types here.
            var valueType = cell.ValueType;
            if (valueType == typeof(bool))
            {
                return new DataGridViewCheckBoxCell();
            }
            else
            {
                return new DataGridViewTextBoxCell();
            }
        }

        void HandleCellValueChanged(object sender, EventArgs e)
        {
            var cell = sender as ICell;
            var gridViewCell = FDataGridView.Rows.Cast<DataGridViewRow>().SelectMany(
                row => row.Cells.Cast<DataGridViewCell>()).First(c => c.Tag == cell);
            
            FDataGridView.CellValueChanged -= HandleCellValueChanged;
            gridViewCell.Value = cell.Value;
            FDataGridView.CellValueChanged += HandleCellValueChanged;
        }
        
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    if (components != null)
                        components.Dispose();
                    
                    if (FMapper != null)
                        FMapper.Dispose();
                    
                    if (FRowSynchronizer != null)
                        FRowSynchronizer.Dispose();
                }
            }
            base.Dispose(disposing);
        }


        private void TableViewer_BackColorChanged(object sender, EventArgs e)
        {
            FDataGridView.BackgroundColor = BackColor;
        }
        
        
    }
}
