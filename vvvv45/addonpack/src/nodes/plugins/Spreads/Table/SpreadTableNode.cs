#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

using System.Collections.Generic;
using System.IO;
#endregion usings

namespace VVVV.Nodes.Table
{

	#region PluginInfo
	[PluginInfo(Name = "Table", Category = "Spreads", Help = "Create an instance of a Table to be used elsewhere", Tags = "", Author = "elliotwoods", AutoEvaluate = true)]
	#endregion PluginInfo
	public class SpreadTableNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Column Names", DefaultString="x,y,z", IsSingle=true)]
		IDiffSpread<string> FPinInColumnNames;

		[Input("Auto Save", IsSingle = true)]
		IDiffSpread<bool> FPinInAutoSave;

		[Input("Load", IsBang = true, IsSingle = true)]
		ISpread<bool> FPinInLoad;

		[Input("Save", IsBang = true, IsSingle = true)]
		ISpread<bool> FPinInSave;

		[Input("Clear", IsBang = true, IsSingle = true)]
		ISpread<bool> FPinInClear;

		[Input("Filename", IsSingle = true, DefaultString="spreadtable.xml", FileMask="XML File (*.xml)|*.xml", StringType=StringType.Filename)]
		IDiffSpread<string> FPinInFilename;

		[Output("Table")]
		ISpread<Table> FPinOutTable;

		[Output("Status")]
		ISpread<string> FOutStatus;

		Table FTable = new Table();
		#endregion fields & pins

		[ImportingConstructor]
		public SpreadTableNode(IPluginHost host)
		{
			FTable.DataChanged += new Table.DataChangedHandler(FTable_DataChanged);
		}

		void FTable_DataChanged(Object sender, EventArgs e)
		{
			if (sender != this)
			{
				FFreshData = true;
				FTable.SetupColumns(FColumnNames);
			}
		}

		bool FFirstRun = true;
		bool FAutosave = false;
		bool FFreshData = false;
		string FFilename = "spreadtable.xml";
		string FColumnNames = "x,y,z";
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FPinInColumnNames.IsChanged)
			{
				try
				{
					FColumnNames = FPinInColumnNames[0];
					FTable.SetupColumns(FColumnNames);
					FTable.OnDataChange(this);
					FFreshData = true;
					FOutStatus[0] = "OK";
				}
				catch(Exception e)
				{
					FOutStatus[0] = e.Message;
				}
			}

			if (FPinInAutoSave.IsChanged)
			{
				FAutosave = FPinInAutoSave[0];
				if (FAutosave)
					Save();
			}

			if (FPinInFilename.IsChanged)
			{
				FFilename = FPinInFilename[0];
				if (FAutosave)
				{
					this.Load();
				}
			}

			if (FPinInLoad[0])
			{
				Load();
				FTable.OnDataChange(this);
			}

			if (FPinInSave[0])
			{
				Save();
			}

			if (FPinInClear[0])
			{
				FTable.ClearAll();
				FTable.SetupColumns(FColumnNames);
				FTable.OnDataChange(this);
				FFreshData = true;
			}

			if (FFirstRun)
			{
				FPinOutTable[0] = FTable;
				FFirstRun = false;
			}

			if (FFreshData)
			{
				if (FAutosave)
					Save();
				FFreshData = false;
			}
		}

		void Load()
		{
			if (FFilename != "")
			{
				try
				{
					if (!File.Exists(FFilename))
					{
						throw (new Exception("File not found."));
					}
					FTable.Clear();
					FTable.ReadXmlSchema(FFilename);
					FTable.ReadXml(FFilename);
					FOutStatus[0] = "Loaded OK";
					FTable.SetupColumns(FColumnNames);
				}
				catch(Exception e)
				{
					FOutStatus[0] = e.Message;
				}
			}
		}

		void Save()
		{
			if (FFilename != "")
			{
				try
				{
					FTable.WriteXmlSchema(FFilename);
					FTable.WriteXml(FFilename);
					FOutStatus[0] = "Saved OK";
				}
				catch (Exception e)
				{
					FOutStatus[0] = e.Message;
				}
			}
		}
	}
}
