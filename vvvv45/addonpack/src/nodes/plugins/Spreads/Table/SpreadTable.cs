using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using VVVV.PluginInterfaces.V2;
using System.Diagnostics;

namespace VVVV.Nodes.Table
{
	public class Table : DataTable
	{
		public delegate void DataChangedHandler(Object sender, EventArgs e);
		public event DataChangedHandler DataChanged;

		public Table()
		{
			ClearAll();
			this.TableName = "SpreadTable";
		}

		void OnDataChange(object sender, EventArgs e)
		{
			if (DataChanged != null)
				DataChanged(sender, e);
		}

		public void OnDataChange(Object sender)
		{
			OnDataChange(sender, new EventArgs());
		}

		public void SetupColumns(string ColumnNames)
		{
			if (ColumnNames == "")
				return;

			var Columns = this.Columns;
			string[] ColumnNamesSplit = ColumnNames.Split(',');

			int index = 0;
			foreach (var name in ColumnNamesSplit)
			{

				if (index >= Columns.Count)
					this.AddColumn(name);
				else
					Columns[index].ColumnName = name;

				index++;
			}
		}

		public void ClearAll()
		{
			this.Rows.Clear();
			this.Columns.Clear();
			OnDataChange(this);
		}

		public Spread<Spread<double> > Spread
		{
			get
			{
				Spread<Spread<double>> data = new Spread<Spread<double>>(this.Rows.Count);

				int index = 0;
				foreach (DataRow row in this.Rows)
				{
					data[index] = new Spread<double>(this.Columns.Count);

					object cell;
					for (int j = 0; j < data[index].SliceCount; j++)
					{
						cell = row[j];
						if (cell.GetType() == typeof(System.Double))
							data[index][j] = (double)cell;
						else
							data[index][j] = 0.0;
					}
					index++;
				}
				return data;
			}
			set
			{
				//we preserve our own column count, but take new row count
				this.Rows.Clear();
				this.Insert( (ISpread<ISpread<double>>) value, 0);
			}
		}

		public void Insert(ISpread<ISpread<double>> insertSpread, int index)
		{
			foreach (var row in insertSpread)
			{
				Insert(row, index);
			}
		}

		public void Insert(ISpread<double> valueSpread, int index)
		{
			//prepare row for insertion
			DataRow row = this.NewRow();
			Spread<bool> setSpread = new Spread<bool>(1);
			setSpread[0] = true;
			Set(row, valueSpread, setSpread);

			if (this.Rows.Count == 0 || this.Rows.Count == index)
			{
				//if we're blank, or insert index is next slot
				this.Rows.Add(row);
			}
			else
			{
				index = VVVV.Utils.VMath.VMath.Zmod(index, this.Rows.Count + 1);
				this.Rows.InsertAt(row, index); // insert it somewhere inside collection
			}
		}

		public void Set(ISpread<double> valueSpread, int rowIndex, ISpread<bool> setSpread)
		{
			if (this.Rows.Count == 0)
			{
				Insert(valueSpread, 0);
			}
			else
			{
				rowIndex = VVVV.Utils.VMath.VMath.Zmod(rowIndex, this.Rows.Count);
				var dataRow = this.Rows[rowIndex];
				Set(dataRow, valueSpread, setSpread);
			}

		}

		public void Set(DataRow row, ISpread<double> values, ISpread<bool> set)
		{
			//check whether slice count is too big. if soo add columns
			//should generally happen on first row only in a spread of spreads
			while (Math.Max(values.SliceCount, set.SliceCount) > this.Columns.Count)
			{
				this.AddColumn(this.Columns.Count.ToString());
			}

			//set values
			for (int i = 0; i < row.ItemArray.Length; i++)
			{
				if (set[i])
					row[i] = values[i];
			}

			OnDataChange(null);
		}

		public void GetRow(DataRow row, ISpread<double> spread)
		{
			int count = row.ItemArray.Length;
			spread.SliceCount = count;
			for (int i=0; i<count; i++)
				spread[i] = (double) row[i];
		}

		public void GetSpread(ISpread<ISpread<double>> output)
		{
			var spread = this.Spread;
			int slicecount = spread.SliceCount;
			output.SliceCount = slicecount;
			for (int i = 0; i < slicecount; i++)
			{
				int slicecount2 = spread[i].SliceCount;
				output[i].SliceCount = slicecount2;
				for (int j = 0; j < slicecount2; j++)
				{
					output[i][j] = spread[i][j];
				}
			}
		}

		private DataColumn AddColumn(string name)
		{
			DataColumn newCol = Columns.Add(name, typeof(double));
			
			newCol.DefaultValue = 0.0;
			foreach (DataRow testRow in Rows)
			{
				if (testRow[newCol].GetType() == typeof(DBNull))
					testRow[newCol] = newCol.DefaultValue;
			}
			newCol.AllowDBNull = false;

			OnDataChange(null);
			return newCol;
		}
	}
}
