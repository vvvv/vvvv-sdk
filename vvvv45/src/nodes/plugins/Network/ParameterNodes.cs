#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Linq;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;

using RCP;
using RCP.Parameters;
using RCP.Protocol;
using RCP.Transporter;

using Kaitai;
#endregion usings

namespace VVVV.Nodes
{
	public class RcpParameter
	{
		string FId, FDatatype, FTypeDefinition, FValue, FLabel, FGroup, FWidget, FUserdata;
		
		public RcpParameter ()
		{}
		
		public RcpParameter (short id, string datatype, string typeDefinition, string value, string label, string group, Widget widget, string userdata)
		{
			FId = id.ToString();
			FDatatype = datatype;
			FTypeDefinition = typeDefinition;
			FValue = value;
			FLabel = label;
			FGroup = group;
			FUserdata = userdata;
			
			if (widget is BangWidget)
				FWidget = "bang";
			else if (widget is PressWidget)
				FWidget = "press";
			else if (widget is ToggleWidget)
				FWidget = "toggle";
			else if (widget is SliderWidget)
				FWidget = "slider";
			else if (widget is NumberboxWidget)
				FWidget = "endless";
			else 
				FWidget = "default";
		}

		public string Id => FId;
		public string Datatype => FDatatype;
		public string TypeDefinition => FTypeDefinition;
		public string Value => FValue;
		public string Label => FLabel;
		public string Group => FGroup;	
		public string Widget => FWidget;	
		public string Userdata => FUserdata;
	}
	
	#region PluginInfo
	[PluginInfo(Name = "ParameterByGroup", 
				Category = "RCP", 
				Version = "",
				Help = "Filter parameters by group")]
	#endregion PluginInfo
	public class RCPParameterByGroupNode : IPluginEvaluate
	{ 
		#region fields & pins
		[Input("Input")]
		public ISpread<RcpParameter> FParameters;
		
		[Input("Group")]
		public ISpread<string> FGroup;
		
		[Output("Output")]
		public ISpread<RcpParameter> FParametersOut;
		#endregion

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FParameters.SliceCount == 0 || FParameters[0] == null)
				FParametersOut.SliceCount = 0;
			else if (string.IsNullOrWhiteSpace(FGroup[0]))
				FParametersOut.AssignFrom(FParameters);
			else
			{
				var groups = FParameters.Where(p => p.Datatype == "Group");
				var groupsToFilter = FGroup.ToList();
				FParametersOut.AssignFrom(FParameters.Where(p => groupsToFilter.Contains(p.Group)));
			}
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Parameter", 
				Category = "RCP", 
				Version = "Split",
				Help = "An RCP Parameter")]
	#endregion PluginInfo
	public class RCPParameterSplitNode : IPluginEvaluate
	{ 
		#region fields & pins
		[Input("Input")]
		public ISpread<RcpParameter> FParameter;
		
		[Output("ID")]
		public ISpread<string> FId;
		
		[Output("Datatype")]
		public ISpread<string> FDatatype;
		
		[Output("Type Definition")]
		public ISpread<string> FTypeDefinition;
		
		[Output("Value")]
		public ISpread<string> FValue;
		
		[Output("Label")]
		public ISpread<string> FLabel;
		
//		[Output("Order")]
//		public ISpread<int> FOrder;
		
		[Output("Group")]
		public ISpread<string> FGroup;
		
		[Output("Widget")]
		public ISpread<string> FWidget;
		
		[Output("Userdata")]
		public ISpread<string> FUserdata;
		#endregion fields & pins
		
		public RCPParameterSplitNode()
		{ }
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			try
			{
				FId.AssignFrom(FParameter.Select(p => p.Id));
				FDatatype.AssignFrom(FParameter.Select(p => p.Datatype));
				FTypeDefinition.AssignFrom(FParameter.Select(p => p.TypeDefinition));
				FValue.AssignFrom(FParameter.Select(p => p.Value));
				FLabel.AssignFrom(FParameter.Select(p => p.Label));
				FWidget.AssignFrom(FParameter.Select(p => p.Widget));
				FGroup.AssignFrom(FParameter.Select(p => p.Group));
				FUserdata.AssignFrom(FParameter.Select(p => p.Userdata));
			}
			catch {}
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Parameter", 
				Category = "RCP", 
				Version = "Join",
				Help = "An RCP Parameter")]
	#endregion PluginInfo
	public class RCPParameterJoinNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("ID")]
		public ISpread<string> FId;
		
		[Input("Datatype")]
		public ISpread<string> FDatatype;
		
		[Input("Type Definition")]
		public ISpread<string> FTypeDefinition;
		
		[Input("Value")]
		public ISpread<string> FValue;
		
		[Input("Label")]
		public ISpread<string> FLabel;
		
//		[Output("Order")]
//		public ISpread<int> FOrder;
		
//		[Input("Parent")]
//		public ISpread<string> FParent;
		
		[Input("Widget")]
		public ISpread<string> FWidget;
		
		[Input("Userdata")]
		public ISpread<string> FUserdata;
		
		[Output("Output")]
		public ISpread<RcpParameter> FParameter;
		
		List<RcpParameter> FParams = new List<RcpParameter>();
		
		#endregion fields & pins
		
		public RCPParameterJoinNode()
		{ }
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FParams.Clear();
			for (int i=0; i<SpreadMax; i++)
			{
				Widget widget = null;
				switch (FWidget[i].ToLower())
				{
					case "bang": widget = new BangWidget(); break;
					case "press": widget = new PressWidget(); break;
					case "toggle": widget = new ToggleWidget(); break;
					case "slider": widget = new SliderWidget(); break;
					case "endless": widget = new NumberboxWidget(); break;
				}
				
//				var param = new Parameter(FId[i].ToRCPId(), FDatatype[i], FTypeDefinition[i], FValue[i], FLabel[i], new byte[0]{}/*FParent[i].ToRCPId()*/, widget, FUserdata[i]);
//				FParams.Add(param);
			}	
			
			FParameter.AssignFrom(FParams);
		}
	}
}