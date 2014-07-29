#region usings
using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using System.ComponentModel;
using System.Diagnostics;
#endregion usings

namespace VVVV.Nodes.Table
{
	#region PluginInfo
	[PluginInfo(Name = "TableView", Category = "Table", Help = "View and edit the data of a Table", Tags = "", Author="elliotwoods", AutoEvaluate = true)]
	#endregion PluginInfo
	public class SpreadTableViewNode : UserControl, IPluginEvaluate
	{
		#region fields & pins
		[Input("Up", IsSingle = true, IsBang = true)]
		ISpread<bool> FUp;

		[Input("Down", IsSingle = true, IsBang = true)]
		ISpread<bool> FDown;

		[Input("Table", IsSingle = true)]
		ISpread<Table> FPinInTable;

		[Output("Index")]
		ISpread<int> FCurrentIndex;

		DataGridView FDataGridView;
		Table FData;
		bool FNeedsUpdate = false;
		#endregion fields & pins

		#region constructor and init

		public SpreadTableViewNode()
		{
			//setup the gui
			InitializeComponent();
			FDataGridView.CellValueChanged += FDataGridView_CellValueChanged;
			FDataGridView.RowsRemoved += FDataGridView_RowsRemoved;
		}

		void FDataGridView_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
		{
			OnDataChanged();
		}

		void FDataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			OnDataChanged();
		}

		void FDataGridView_Validated(object sender, EventArgs e)
		{
			OnDataChanged();
		}

		void OnDataChanged()
		{
			if (this.FData != null)
			{
				FData.OnDataChange(this);
			}
		}

		void InitializeComponent()
		{
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.FDataGridView = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.FDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // FDataGridView
            // 
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            dataGridViewCellStyle1.Format = "N4";
            dataGridViewCellStyle1.NullValue = "0.0000";
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.Gray;
            this.FDataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.FDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.FDataGridView.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.FDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.FDataGridView.Cursor = System.Windows.Forms.Cursors.Default;
            this.FDataGridView.Location = new System.Drawing.Point(0, 0);
            this.FDataGridView.Name = "FDataGridView";
            dataGridViewCellStyle2.Format = "N4";
            dataGridViewCellStyle2.NullValue = "0.0000";
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.Gray;
            this.FDataGridView.RowsDefaultCellStyle = dataGridViewCellStyle2;
            this.FDataGridView.Size = this.Size;
            this.FDataGridView.TabIndex = 0;
            this.FDataGridView.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.dataGridView1_CellValidating);
            this.FDataGridView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.FDataGridView_MouseMove);
            // 
            // SpreadTableViewNode
            // 
            this.Controls.Add(this.FDataGridView);
            this.Name = "SpreadTableViewNode";
            this.Size = new System.Drawing.Size(344, 368);
            this.Resize += new System.EventHandler(this.ValueTableBufferNode_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.FDataGridView)).EndInit();
            this.ResumeLayout(false);
		}
	
		Point FMouseLast;
		bool FMouseDragging = false;
		void FDataGridView_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Button.HasFlag(System.Windows.Forms.MouseButtons.Right))
			{
				if (FMouseDragging)
				{
					int stepOrder = 0;
					stepOrder += Form.ModifierKeys.HasFlag(Keys.Shift) ? 1 : 0;
					stepOrder += Form.ModifierKeys.HasFlag(Keys.Control)? 1 : 0;
					stepOrder *= Form.ModifierKeys.HasFlag(Keys.Alt) ? 1 : -1;
					double step = 0.01 * Math.Pow(10, stepOrder);

					double delta = - step * (double)(e.Y - FMouseLast.Y);
					foreach (DataGridViewCell cell in FDataGridView.SelectedCells)
						if (cell.Value.GetType() == typeof(System.Double) && cell.RowIndex < FData.Rows.Count) //avoids selection of the 'new row' at bottom or invalid cells
							cell.Value = (double)cell.Value + delta;
					FData.OnDataChange(this);
				}
				else
				{
					FMouseDragging = true;
				}
				FMouseLast = e.Location;
			}
			else
				FMouseDragging = false;
		}

		#endregion constructor and init

		public void Evaluate(int SpreadMax)
		{
			if (FPinInTable[0] != FData)
			{
				if (FData != null)
				{
					FData.DataChanged -= new Table.DataChangedHandler(FData_DataChanged);
				}
				FData = FPinInTable[0];
				FDataGridView.DataSource = FData;
				if (FData != null)
				{
					FData.DataChanged += new Table.DataChangedHandler(FData_DataChanged);
					foreach(DataGridViewColumn column in FDataGridView.Columns)
					{
						column.SortMode = DataGridViewColumnSortMode.NotSortable;
					}
				}
			}

			if (FData == null)
				return;

			if (FData.Rows.Count > 0)
			{
				bool moveRow = false;
				int selectedRow = 0;

				if (FDataGridView.SelectedCells.Count > 0)
					selectedRow = FDataGridView.SelectedCells[0].RowIndex;
				else
					selectedRow = 0;

				if (FUp[0])
				{
					selectedRow++;
					selectedRow %= FData.Rows.Count;
					moveRow = true;
				}

				if (FDown[0])
				{
					selectedRow--;
					if (selectedRow < 0)
						selectedRow += FData.Rows.Count;
					moveRow = true;
				}

				if (moveRow)
				{
					FDataGridView.ClearSelection();
					FDataGridView.Rows[selectedRow].Selected = true;
				}
			}

			if (FNeedsUpdate)
			{
				foreach (DataGridViewColumn column in FDataGridView.Columns)
				{
					column.SortMode = DataGridViewColumnSortMode.NotSortable;
				}
				FDataGridView.Refresh();
				FNeedsUpdate = false;
			}

			if (FData.Rows.Count == 0)
			{
				FCurrentIndex.SliceCount = 1;
				FCurrentIndex[0] = 0;
			}
			else
			{
				var rows = FDataGridView.SelectedRows;
				if (rows.Count > 0)
				{
					FCurrentIndex.SliceCount = 0;
					foreach (DataGridViewRow row in rows)
					{
						FCurrentIndex.Add(row.Index);
					}
				}
				else
				{
					int row = FDataGridView.CurrentCellAddress.Y;
					FCurrentIndex.SliceCount = 1;
					FCurrentIndex[0] = row;
				}
			}

			if (this.FDataGridView.Rows.Count != this.FData.Rows.Count + 1)
			{
				//this.FDataGridView.Rows.RemoveAt(this.FDataGridView.Rows.Count - 2);
			}
		}

		void FData_DataChanged(Object sender, EventArgs e)
		{
			//pretty hacky. this clears the 'udpate' flag if the last instruction is from itself
			FNeedsUpdate = true;// (sender != this);
		}

		private void ValueTableBufferNode_Resize(object sender, EventArgs e)
		{
			this.FDataGridView.Size = this.Size;
		}

		private void dataGridView1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			try
			{
				System.Convert.ToDouble(e.FormattedValue);
			}
			catch
			{
				e.Cancel = true;
			}			
		}
	}
}
