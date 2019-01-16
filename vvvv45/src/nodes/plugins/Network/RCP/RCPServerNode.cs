#region usings
using System;
using System.IO;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;

using V2 = System.Numerics.Vector2;
using V3 = System.Numerics.Vector3;
using V4 = System.Numerics.Vector4;

using RCP;
using RCP.Transporter;
using RCP.Parameters;
using RCP.Protocol;

using Kaitai;
using RCP.Types;

#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Rabbit",
	Category = "RCP",
	AutoEvaluate = true,
	Help = "An RCP Server",
	Tags = "remote, server")]
	#endregion PluginInfo
	public class RCPRabbitNode : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
	{
		#region fields & pins
		[Input("Host", IsSingle=true, DefaultString = "0.0.0.0")]
		public IDiffSpread<string> FHost; 
		
		[Input("Port", IsSingle=true, DefaultValue = 10000)]
		public IDiffSpread<int> FPort; 
		
		[Input("Update Enums", IsSingle=true, IsBang=true)]
		public ISpread<bool> FUpdateEnums; 
		
		[Output("Client Count")]
		public ISpread<int> FConnectionCount;
		//public ISpread<byte> FOutput;
		
		[Import()]
		public ILogger FLogger;
		
		[Import()]
		public IHDEHost FHDEHost;
		
		RCPServer FRCPServer;
		WebsocketServerTransporter FTransporter;
		
		//address -> IGroupParameter
		Dictionary<string, GroupParameter> FGroups = new Dictionary<string, GroupParameter>();
		//userid -> IPin2
		Dictionary<string, IPin2> FCachedPins = new Dictionary<string, IPin2>();
		//userid -> IParameter
		Dictionary<string, Parameter> FCachedParams = new Dictionary<string, Parameter>();
		readonly Dictionary<IPin2, IOBox> FWatchedIOBoxes = new Dictionary<IPin2, IOBox>();
		#endregion fields & pins
		  
		public RCPRabbitNode()
		{ 
			//initialize the RCP Server
			FRCPServer = new RCPServer();
		}
		
		public void OnImportsSatisfied()
		{
			FHDEHost.ExposedNodeService.NodeAdded += NodeAddedCB;
			FHDEHost.ExposedNodeService.NodeRemoved += NodeRemovedCB;
			
			GroupMap.GroupAdded += GroupAdded; 
			GroupMap.GroupRemoved += GroupRemoved; 
			
			//FRCPServer.Log = (s) => FLogger.Log(LogType.Debug, "server: " + s);
			 
			//get initial list of exposed ioboxes
			foreach (var node in FHDEHost.ExposedNodeService.Nodes)
				NodeAddedCB(node);
		}
		
		public void Dispose()
		{
			//unscubscribe from nodeservice
			FHDEHost.ExposedNodeService.NodeAdded -= NodeAddedCB;
			FHDEHost.ExposedNodeService.NodeRemoved -= NodeRemovedCB;
			
			GroupMap.GroupAdded -= GroupAdded; 
			GroupMap.GroupRemoved -= GroupRemoved; 
			
			//dispose the RCP server
			FLogger.Log(LogType.Debug, "Disposing the RCP Server");
			FRCPServer.Dispose();
			
			//clear cached pins
			FGroups.Clear();
			FCachedPins.Clear();
			FCachedParams.Clear();
			FWatchedIOBoxes.Clear();
		}
		 
		private void GroupAdded(string address)
		{
			if (!FGroups.ContainsKey(address))
			{
				var group = FRCPServer.CreateGroup(GroupMap.GetName(address));
				FGroups.Add(address, group);
				
				//move all ioboxes of the groups patch to the group
				var ps = GetGroupParameters(address);
				foreach (var param in ps)
					FRCPServer.AddParameter(param, group);
				
				FRCPServer.Update();
			}
		}
		
		private void GroupRemoved(string address)
		{
			if (FGroups.ContainsKey(address))
			{
				var group = FGroups[address];
				
				//move all params of the group to the root group
				var paramsOfGroup = GetGroupParameters(address);
				foreach (var param in paramsOfGroup)
					FRCPServer.AddParameter(param, null);
				
				//then remove the actual group-param
				FRCPServer.RemoveParameter(group);
				FGroups.Remove(address);
								
				FRCPServer.Update();
			}
		}
		
		//TODO: remove this when rcpserver has GetParamByUserId
		private IEnumerable<Parameter> GetGroupParameters(string address)
		{
			var paramIds = FCachedParams.Values.Select(p => p.Id);
			foreach (var id in paramIds)
			{
				var param = FRCPServer.GetParameter(id);
				
				var lastSlash = param.UserId.LastIndexOf('/');
				var temp = param.UserId.Substring(0, lastSlash);
				lastSlash = temp.LastIndexOf('/');
				var parentPath = param.UserId.Substring(0, lastSlash);
				
				if (parentPath == address)
					yield return param;
			}
		}
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FTransporter == null)
			{
				FTransporter = new WebsocketServerTransporter(FHost[0], FPort[0]);
				FRCPServer.AddTransporter(FTransporter);
			}
			
			if (FHost.IsChanged || FPort.IsChanged)
				FTransporter.Bind(FHost[0], FPort[0]);
			
			//TODO: subscribe to enum-changes on the host and update all related
			//parameters as changes happen, so a client can update its gui accordingly
			if (FUpdateEnums[0])
			{
				var enumPins = FCachedPins.Values.Where(v => v.Type == "Enumeration");
				
				foreach (var enumPin in enumPins)
					PinValueChanged(enumPin, null);
			}

			var anyOutBoxChanged = false;
			foreach (var pin in FWatchedIOBoxes.Keys)
            {
            	if (pin.IsConnected())
            	{
            		var ioBox = FWatchedIOBoxes[pin];
            		if (ioBox.Sync())
                	{
                		anyOutBoxChanged = true;
						var userId = IdFromPin(pin);
						var param = FCachedParams[userId];
						//in case of enum pin we also update the full definition here
						//which may have changed in the meantime
						//TODO: subscribe to enum-changes on the host and update all related
						//parameters as changes happen, so a client can update its gui accordingly
						if (pin.Type == "Enumeration")
						{
							var subtype = pin.SubType.Split(',').Select(s => s.Trim()).ToArray();
							var enumName = subtype[1].Trim();
							var dflt = subtype[2].Trim();
							var newDef = GetEnumDefinition(enumName, dflt);
							IEnumDefinition paramDef;
							if (pin.SliceCount == 1)
								paramDef = param.TypeDefinition as IEnumDefinition;
							else
								paramDef = (param.TypeDefinition as IArrayDefinition).ElementDefinition as IEnumDefinition;
							paramDef.Default = newDef.Default;
							paramDef.Entries = newDef.Entries;
							//FLogger.Log(LogType.Debug, "count: " + pin.Spread);
						}
						RCP.Helpers.StringToValue(param, pin.Spread);
                	}
                }
            	
            	if (anyOutBoxChanged)
	            	FRCPServer.Update();
            }
			
			FConnectionCount[0] = FTransporter.ConnectionCount;
		}
		
		void TryWrap(INode2 node, IPin2 pin)
        {
            var ioBox = IOBox.Wrap(node);
            if (ioBox != null)
                FWatchedIOBoxes.Add(pin, ioBox);
            else
                FLogger.Log(LogType.Error, "Wrapper for IO box " + node + " not implemented.");
        }

        void Remove(INode2 node)
        {
        	var pinName = PinNameFromNode(node);
			var pin = node.FindPin(pinName);
            FWatchedIOBoxes.Remove(pin);
        }
		
		private void NodeAddedCB(INode2 node)
		{
			var pinName = PinNameFromNode(node);
			var pin = node.FindPin(pinName);
			TryWrap(node, pin);
			
			pin.Changed += PinValueChanged;
			pin.SubtypeChanged += SubtypeChanged;
			node.LabelPin.Changed += LabelChanged;
			var tagPin = node.FindPin("Tag");
			tagPin.Changed += TagChanged;
			
			//TODO: subscribe to subtype-pins here as well
			//default, min, max, ...
			
			var userId = IdFromPin(pin);
			FCachedPins.Add(userId, pin);
			
			var parentId = ParentIdFromNode(node);
			var param = ParameterFromNode(node, userId, parentId);
			param.ValueUpdated += ParameterUpdated;
			FCachedParams.Add(userId, param);
			
			//group
			var parentPath = node.Parent.GetNodePath(false);
			if (FGroups.ContainsKey(parentPath))
			{
				var group = FGroups[parentPath];	
				FRCPServer.AddParameter(param, group);
				//FLogger.Log(LogType.Debug, "added to: " + group.Label);
			}
			
			FRCPServer.Update();
		}
		
		private void NodeRemovedCB(INode2 node)
		{
			Remove(node);
			
			var pinName = PinNameFromNode(node);
			var pin = node.FindPin(pinName);
			pin.Changed -= PinValueChanged;
			pin.SubtypeChanged -= SubtypeChanged;
			node.LabelPin.Changed -= LabelChanged;
			var tagPin = node.FindPin("Tag");
			tagPin.Changed -= TagChanged;
			
			var userId = IdFromPin(pin);
			FCachedPins.Remove(userId);
			var param = FCachedParams[userId];
			param.ValueUpdated -= ParameterUpdated;
			FRCPServer.RemoveParameter(param);
			FCachedParams.Remove(userId);

			FRCPServer.Update();
		}
		
		private string IdFromPin(IPin2 pin)
		{
			var pinname = PinNameFromNode(pin.ParentNode);
			var pinpath = pin.ParentNode.GetNodePath(false) + "/" + pinname;
			return pinpath;
		}
		
		private string ParentIdFromNode(INode2 node)
		{
			var path = node.GetNodePath(false);
			var ids = path.Split('/'); 
			return string.Join("/", ids.Take(ids.Length-1));
		}
		
		private string PinNameFromNode(INode2 node)
		{ 
			string pinName = "";
			if (node.NodeInfo.Systemname == "IOBox (Value Advanced)")
			pinName = "Y Input Value";
			else if (node.NodeInfo.Systemname == "IOBox (String)")
			pinName = "Input String";
			else if (node.NodeInfo.Systemname == "IOBox (Color)")
			pinName = "Color Input";
			else if (node.NodeInfo.Systemname == "IOBox (Enumerations)")
			pinName = "Input Enum";
			else if (node.NodeInfo.Systemname == "IOBox (Node)")
			pinName = "Input Node";
			
			return pinName;
		}
		
		private Parameter ParameterFromNode(INode2 node, string userId, string parentId)
		{
			var pinName = PinNameFromNode(node);
			var pin = node.FindPin(pinName);
			var id = IdFromPin(pin);
			
			Parameter parameter = null;
			
			var subtype = new string[0];
			if (!string.IsNullOrEmpty(pin.SubType))
				subtype = pin.SubType.Split(',').Select(s => s.Trim()).ToArray();
			var sliceCount = pin.SliceCount;
			var label = pin.ParentNode.LabelPin.Spread.Trim('|');
			
			switch(pin.Type)
			{
				case "Value": 
				{
					var dimensions = int.Parse(subtype[1]);
					//figure out the actual spreadcount
					//taking dimensions (ie. vectors) of value-spreads into account
					sliceCount /= dimensions;
					
					if (dimensions == 1)
					{
						int intStep = 0;
						float floatStep = 0;
						
						if (int.TryParse(subtype[5], out intStep)) //integer
						{
	                        var isbool = (subtype[3] == "0") && (subtype[4] == "1");
	                        if (isbool)
	                        {
	                        	var def = subtype[2] == "1";
	                        	parameter = GetBoolParameter(label, sliceCount, def, pin, (p,i) => {return p[i] == "1";});
	                        }
							else
							{
								int def, min, max, mul;
								ParseIntSubtype(subtype, out def, out min, out max, out mul);
								parameter = GetNumberParameter<int>(label, sliceCount, 1, def, min, max, intStep, pin, (p,i) => RCP.Helpers.GetInt(p,i));
							}
						}
						else if (float.TryParse(subtype[5], NumberStyles.Float, CultureInfo.InvariantCulture, out floatStep))
						{
							float def, min, max, mul;
							ParseFloatSubtype(subtype, out def, out min, out max, out mul);
							parameter = GetNumberParameter<float>(label, sliceCount, 1, def, min, max, mul, pin, (p,i) => RCP.Helpers.GetFloat(p,i));
						}
						
						switch (subtype[0])
						{
							case "Bang": parameter.Widget = new BangWidget(); break;
							case "Press": parameter.Widget = new PressWidget(); break;
							case "Toggle": parameter.Widget = new ToggleWidget(); break;
							case "Slider": parameter.Widget = new SliderWidget(); break;
							case "Endless": parameter.Widget = new NumberboxWidget(); break;
						}
						
						//widget display precision
						//int.TryParse(subtype[7], out precision);
					}
					else if (dimensions == 2)
					{
						//TODO: parse 2d subtype when pin.Subtype supports it
						//var comps = subtype[2].Split(',');
						//FLogger.Log(LogType.Debug, subtype[2]);
						float def, min, max, mul;
						ParseFloatSubtype(subtype, out def, out min, out max, out mul);
						var def2 = new V2(def);
						var min2 = new V2(min);
						var max2 = new V2(max);
						var mul2 = new V2(mul);
						parameter = GetNumberParameter<V2>(label, sliceCount, 2, def2, min2, max2, mul2, pin, (p,i) => RCP.Helpers.GetVector2(p,i));
					}
					else if (dimensions == 3)
					{
						//TODO: parse 3d subtype when pin.Subtype supports it
						//var comps = subtype[2].Split(',');
						//FLogger.Log(LogType.Debug, subtype[2]);
						float def, min, max, mul;
						ParseFloatSubtype(subtype, out def, out min, out max, out mul);
						var def3 = new V3(def);
						var min3 = new V3(min);
						var max3 = new V3(max);
						var mul3 = new V3(RCP.Helpers.ParseFloat(subtype[5]));
						parameter = GetNumberParameter<V3>(label, sliceCount, 3, def3, min3, max3, mul3, pin, (p,i) => RCP.Helpers.GetVector3(p,i));
					}
					else if (dimensions == 4)
					{
						//TODO: parse 3d subtype when pin.Subtype supports it
						//var comps = subtype[2].Split(',');
						//FLogger.Log(LogType.Debug, subtype[2]);
						float def, min, max, mul;
						ParseFloatSubtype(subtype, out def, out min, out max, out mul);
						var def4 = new V4(def);
						var min4 = new V4(min);
						var max4 = new V4(max);
						var mul4 = new V4(mul);
						parameter = GetNumberParameter<V4>(label, sliceCount, 4, def4, min4, max4, mul4, pin, (p,i) => RCP.Helpers.GetVector4(p,i));
					}
					break;
				}
				
				case "String": 
				{
					var s = subtype[0].ToLower();
					var def = subtype[1];
					if (s == "filename" || s == "directory")
					{
						var schema = "file";
						var filter = "";
						if (s == "filename")
							filter = subtype[2];
						
						//var v = pin[0].TrimEnd('\\').Replace("\\", "/");
//						if (schema == "directory")
//							v += "/";
						
						parameter = GetUriParameter(label, sliceCount, def, schema, filter, pin, (p,i) => p[i]);
					}
					else if (s == "url")
					{
						var schema = "http";
						parameter = GetUriParameter(label, sliceCount, def, schema, "", pin, (p,i) => p[i]);
					}
					else 
					{
						var maxChars = -1;
						int.TryParse(subtype[3], out maxChars);
						parameter = GetStringParameter(label, sliceCount, def, maxChars, pin, (p,i) => p[i]);
					}
					
					break;
				}
				case "Color":
	            {
		            /// colors: guiType, default, hasAlpha
	                bool hasAlpha = subtype[2].Trim() == "HasAlpha";
	            	//TODO: implement default for color IOBoxes
	            	var def = Color.Red;
	            	parameter = GetRGBAParameter(label, sliceCount, def, pin, (p,i) => RCP.Helpers.ParseColor(pin[i]));
	            	break;
	            }
				case "Enumeration":
	            {
		            /// enums: guiType, enumName, default
	                var enumName = subtype[1].Trim();
	            	var def = subtype[2].Trim();
	            	parameter = GetEnumParameter(label, sliceCount, enumName, def, pin, (p,i) => p[i]);
	            	break;
	            }
			}
			
			//no suitable parameter found?
			if (parameter == null)
			{
				parameter = FRCPServer.CreateStringParameter(label);
				(parameter as StringParameter).Value = "Unknown Type";
				//parameter.Readonly = true;
			}

			//FLogger.Log(LogType.Debug, address + " - " + ParentMap.GetName(address));
			
			//order
			var bounds = node.GetBounds(BoundsType.Box);
			parameter.Order = bounds.X;
			
			//userid
			parameter.UserId = userId;
			
			//userdata
			var tag = node.FindPin("Tag");
            if (tag != null)
                parameter.Userdata = Encoding.UTF8.GetBytes(tag.Spread.Trim('|'));
			
			return parameter;
		}
		
		private void ParseFloatSubtype(string[] subtype, out float def, out float min, out float max, out float mul)
		{
			def = RCP.Helpers.ParseFloat(subtype[2]);
			min	= RCP.Helpers.ParseFloat(subtype[3]);
			max = RCP.Helpers.ParseFloat(subtype[4]);
			mul = RCP.Helpers.ParseFloat(subtype[5]);
		}

		private void ParseIntSubtype(string[] subtype, out int def, out int min, out int max, out int mul)
		{
			def = RCP.Helpers.ParseInt(subtype[2]);
			min	= RCP.Helpers.ParseInt(subtype[3]);
			max = RCP.Helpers.ParseInt(subtype[4]);
			mul = RCP.Helpers.ParseInt(subtype[5]);
		}
		
		private void ParameterUpdated(object sender, EventArgs e)
        {
        	IPin2 pin;
        	if (FCachedPins.TryGetValue((sender as Parameter).UserId, out pin))
        	{
				pin.Spread = RCP.Helpers.ValueToString(sender as Parameter);
        		//FLogger.Log(LogType.Debug, "remote: " + pin.Spread);
        	}
        }
		
		#region GetParameter
		private Parameter GetBoolParameter(string label, int sliceCount, bool def, IPin2 pin, Func<IPin2, int, bool> parse)
		{
			if (sliceCount == 1)
			{
				var param = FRCPServer.CreateValueParameter<bool>(label);
				param.Default = def;
				param.Value = parse(pin, 0);
				return param;
			}
			else
			{
				var param = FRCPServer.CreateArrayParameter<bool>(label, sliceCount);
				var values = new List<bool>();
				var defs = new List<bool>();
				for (int i=0; i<sliceCount; i++)
				{
					values.Add(parse(pin, i));
					defs.Add(def);
				}
				param.Value = values.ToArray(); 
				param.Default = defs.ToArray();
				return param;
			}
		}
		
		private Parameter GetNumberParameter<T>(string label, int sliceCount, int dimensions, T def, T min, T max, T multiple, IPin2 pin, Func<IPin2, int, T> parse) where T: struct
		{
			if (sliceCount == 1)
			{
				var param = FRCPServer.CreateNumberParameter<T>(label);
				param.Default = def;
				param.Minimum = min;
				param.Maximum = max;
				param.MultipleOf = multiple;
				param.Value = parse(pin, 0);
				return param;
			}
			else
			{
				var param = FRCPServer.CreateArrayParameter<T>(label, sliceCount);
				var values = new List<T>();
				var defs = new List<T>();
				//TODO:set multiple, min, max
//				var mins = new List<T>();
//				var maxs = new List<T>();
//				var mults = new List<T>();
				for (int i=0; i<sliceCount; i++)
				{
					values.Add(parse(pin, i*dimensions));
					defs.Add(def);					
				}
				param.Value = values.ToArray();
				param.Default = defs.ToArray();
				return param;
			}
		}
			
		private Parameter GetStringParameter(string label, int sliceCount, string def, int maxChars, IPin2 pin, Func<IPin2, int, string> parse)
		{
			if (sliceCount == 1)
			{
				var param = FRCPServer.CreateStringParameter(label);
				param.Default = def;
//				if (maxChars > -1)
//					param.RegularExpression = ".{0," + maxChars.ToString() + "}";
				param.Value = parse(pin, 0);
				return param;
			}
			else
			{
				var param = FRCPServer.CreateArrayParameter<string>(label, sliceCount);
				var values = new List<string>();
				var defs = new List<string>();
				for (int i=0; i<sliceCount; i++)
				{
					values.Add(parse(pin, i));
					defs.Add(def);
				}
				param.Value = values.ToArray(); 
				param.Default = defs.ToArray();
				return param;
			}
		}
		
		private Parameter GetUriParameter(string label, int sliceCount, string def, string schema, string filter, IPin2 pin, Func<IPin2, int, string> parse)
		{
			if (sliceCount == 1)
			{
				var param = FRCPServer.CreateUriParameter(label);
				param.Default = def;
				param.Schema = schema;
				param.Filter = filter;
				param.Value = parse(pin, 0);
				return param;
			}
			else
			{
				var param = FRCPServer.CreateUriArrayParameter(label, sliceCount);
				//TODO:set schema, filter
				var values = new List<string>();
				var defs = new List<string>();
				for (int i=0; i<sliceCount; i++)
				{
					values.Add(parse(pin, i));
					defs.Add(def);
				}	
				param.Value = values.ToArray();
				param.Default = defs.ToArray();
				return param;
			}
		}
		
		private Parameter GetRGBAParameter(string label, int sliceCount, Color def, IPin2 pin, Func<IPin2, int, Color> parse)
		{
			if (sliceCount == 1)
			{
				var param = FRCPServer.CreateValueParameter<Color>(label);
				param.Default = def;
				param.Value = parse(pin, 0);
				return param;
			}
			else
			{
				var param = FRCPServer.CreateArrayParameter<Color>(label, sliceCount);
				var values = new List<Color>();
				var defs = new List<Color>();
				for (int i=0; i<sliceCount; i++)
				{
					values.Add(parse(pin, i));
					defs.Add(def);
				}
				param.Value = values.ToArray(); 
				param.Default = defs.ToArray(); 
				return param;
			}
		}
		
		private Parameter GetEnumParameter(string label, int sliceCount, string name, string def, IPin2 pin, Func<IPin2, int, string> parse)
		{
			var definition = GetEnumDefinition(name, def);
			if (sliceCount == 1)
			{
				var param = FRCPServer.CreateEnumParameter(label);
				param.Default = def;
				param.Entries = definition.Entries;
				param.Value = parse(pin, 0);
				return param;
			}
			else
			{
				var param = FRCPServer.CreateEnumArrayParameter(label, sliceCount);
				//TODO:set entries
				var values = new List<string>();
				var defs = new List<string>();
				for (int i=0; i<sliceCount; i++)
				{
					values.Add(parse(pin, i));
					defs.Add(def);
				}
				param.Value = values.ToArray(); 
				param.Default = defs.ToArray();
				return param;
			}
		}
		
		private EnumDefinition GetEnumDefinition(string enumName, string deflt)
		{
			var entryCount = EnumManager.GetEnumEntryCount(enumName);
            var entries = new List<string>();
            for (int i = 0; i < entryCount; i++)
                entries.Add(EnumManager.GetEnumEntryString(enumName, i));

            var def = new EnumDefinition();
            def.Default = deflt; //(ushort) entries.IndexOf(deflt);
        	def.Entries = entries.ToArray();
			
			return def;
		}
		#endregion
		
		#region PinsChanged
		//the application updated a value
		private void PinValueChanged(object sender, EventArgs e)
		{
			//here it coult make sense to think about a
			//beginframe/endframe bracket to not send every changed pin directly
			//but collect them and send them per frame in a bundle
			var pin = sender as IPin2;
			var userId = IdFromPin(pin);
			
			//FLogger.Log(LogType.Debug, "id: " + userId);
			var param = FCachedParams[userId];
			//in case of enum pin we also update the full definition here
			//which may have changed in the meantime
			//TODO: subscribe to enum-changes on the host and update all related
			//parameters as changes happen, so a client can update its gui accordingly
			if (pin.Type == "Enumeration")
			{
				var subtype = pin.SubType.Split(',').Select(s => s.Trim()).ToArray();
				var enumName = subtype[1].Trim();
				var dflt = subtype[2].Trim();
				var newDef = GetEnumDefinition(enumName, dflt);
				IEnumDefinition paramDef;
				if (pin.SliceCount == 1)
					paramDef = param.TypeDefinition as IEnumDefinition;
				else
					paramDef = (param.TypeDefinition as IArrayDefinition).ElementDefinition as IEnumDefinition;
				paramDef.Default = newDef.Default;
				paramDef.Entries = newDef.Entries;
				//FLogger.Log(LogType.Debug, "count: " + pin.Spread);
			}
			RCP.Helpers.StringToValue(param, pin.Spread);
			
			FRCPServer.Update();
		}
		
		private void SubtypeChanged(object sender, EventArgs e)
		{
			var pin = sender as IPin2;
			var labelPin = sender as IPin2;
			var userId = IdFromPin(labelPin);
			
			var param = FCachedParams[userId];
			var subtype = pin.SubType.Split(',').Select(s => s.Trim()).ToArray();
			
			switch (param.TypeDefinition.Datatype)
			{
				case RcpTypes.Datatype.Boolean:
				{
					var p = (param as ValueParameter<bool>);
					p.Default = subtype[2] == "1";
					break;
				}
				case RcpTypes.Datatype.Float32:
				{
					var p = (param as NumberParameter<float>);
					float def, min, max, mul;
					ParseFloatSubtype(subtype, out def, out min, out max, out mul);
					p.Default = def;
					p.Minimum = min;
					p.Maximum = max;
					p.MultipleOf = mul;
					p.Unit = subtype[6];
					break;
				}
				case RcpTypes.Datatype.Int32:
				{
					var p = (param as NumberParameter<int>);
					int def, min, max, mul;
					ParseIntSubtype(subtype, out def, out min, out max, out mul);
					p.Default = def;
					p.Minimum = min;
					p.Maximum = max;
					p.MultipleOf = mul;
					p.Unit = subtype[6];
					break;
				}
				case RcpTypes.Datatype.Vector2f32:
				{
					var p = (param as NumberParameter<V2>);
					float def, min, max, mul;
					ParseFloatSubtype(subtype, out def, out min, out max, out mul);
					p.Default = new V2(def);
					p.Minimum = new V2(min);
					p.Maximum = new V2(max);
					p.MultipleOf = new V2(mul);
					p.Unit = subtype[6];
					FLogger.Log(LogType.Debug, min.ToString());
					break;
				}
				case RcpTypes.Datatype.Vector3f32:
				{
					var p = (param as NumberParameter<V3>);
					float def, min, max, mul;
					ParseFloatSubtype(subtype, out def, out min, out max, out mul);
					p.Default = new V3(def);
					p.Minimum = new V3(min);
					p.Maximum = new V3(max);
					p.MultipleOf = new V3(mul);
					p.Unit = subtype[6];
					break;
				}
				case RcpTypes.Datatype.Vector4f32:
				{
					var p = (param as NumberParameter<V4>);
					float def, min, max, mul;
					ParseFloatSubtype(subtype, out def, out min, out max, out mul);
					p.Default = new V4(def);
					p.Minimum = new V4(min);
					p.Maximum = new V4(max);
					p.MultipleOf = new V4(mul);
					p.Unit = subtype[6];
					break;
				}
				
				//TODO: subtypes for string, uri, vectors
			}
			FRCPServer.Update();
		}
		
		private void LabelChanged(object sender, EventArgs e)
		{
			var labelPin = sender as IPin2;
			var userId = IdFromPin(labelPin);
			
			FCachedParams[userId].Label = labelPin.Spread.Trim('|');
			FRCPServer.Update();
		}
		
		private void TagChanged(object sender, EventArgs e)
		{
			var tagPin = sender as IPin2;
			var userId = IdFromPin(tagPin);
			
			FCachedParams[userId].Userdata = Encoding.UTF8.GetBytes(tagPin.Spread.Trim('|'));
			FRCPServer.Update();
		}
		#endregion
	}
}

namespace RCP
{
	public static class Helpers
	{
		//vvvv string/enum escaping rules:
		//if a slice contains either a space " ", a pipe "|" or a comma ","
		//the slice is quoted with pipes "|like so|"
		//and also every pipe is escaped with another pipe "|like||so|" to encode a string like "like|so"
		
		private static string PipeEscape(string input)
		{
			if (input.Contains(",") || input.Contains("|") || input.Contains(" "))
			{
				input = input.Replace("|", "||");
				input = "|" + input + "|";
			}
			return input;
		}
		
		public static string ValueToString(Parameter param)
		{
			try
			{
				switch (param.TypeDefinition.Datatype)
				{
					case RcpTypes.Datatype.Boolean: return RCP.Helpers.BoolToString((param as ValueParameter<bool>).Value);
					case RcpTypes.Datatype.String: return PipeEscape((param as StringParameter).Value);
					case RcpTypes.Datatype.Uri: return PipeEscape((param as UriParameter).Value);
					case RcpTypes.Datatype.Enum: return PipeEscape((param as EnumParameter).Value);
					case RcpTypes.Datatype.Float32: return RCP.Helpers.Float32ToString((param as NumberParameter<float>).Value);
					case RcpTypes.Datatype.Int32: return RCP.Helpers.Int32ToString((param as NumberParameter<int>).Value);
					case RcpTypes.Datatype.Vector2f32: return RCP.Helpers.Vector2f32ToString((param as NumberParameter<Vector2>).Value);
					case RcpTypes.Datatype.Vector3f32: return RCP.Helpers.Vector3f32ToString((param as NumberParameter<Vector3>).Value);
					case RcpTypes.Datatype.Vector4f32: return RCP.Helpers.Vector4f32ToString((param as NumberParameter<Vector4>).Value);
					case RcpTypes.Datatype.Rgba: return RCP.Helpers.ColorToString((param as ValueParameter<Color>).Value);
					case RcpTypes.Datatype.Group: return "";
					case RcpTypes.Datatype.Array:
					{
						switch ((param.TypeDefinition as IArrayDefinition).ElementType)
						{
							case RcpTypes.Datatype.Boolean:
							{
								var val = ((ArrayParameter<bool>)param).Value;
								return string.Join(",", val.Select(v => BoolToString(v)));
							}
							case RcpTypes.Datatype.Enum:
							{
								//TODO; accessing the subtypes entries fails
								var val = ((ArrayParameter<string>)param).Value;
								return string.Join(",", val.Select(v => PipeEscape(v)));
							}						
							case RcpTypes.Datatype.Int32:
							{
								var val = ((ArrayParameter<int>)param).Value;
								return string.Join(",", val.Select(v => Int32ToString(v)));
							}
							case RcpTypes.Datatype.Float32:
							{
								var val = ((ArrayParameter<float>)param).Value;
								return string.Join(",", val.Select(v => Float32ToString(v)));
							}
							case RcpTypes.Datatype.Vector2f32:
							{
								var val = ((ArrayParameter<V2>)param).Value;
								return string.Join(",", val.Select(v => Vector2f32ToString(v)));
							}		
							case RcpTypes.Datatype.Vector3f32:
							{
								var val = ((ArrayParameter<V3>)param).Value;
								return string.Join(",", val.Select(v => Vector3f32ToString(v)));
							}	
							case RcpTypes.Datatype.Vector4f32:
							{
								var val = ((ArrayParameter<V4>)param).Value;
								return string.Join(",", val.Select(v => Vector4f32ToString(v)));
							}	
							case RcpTypes.Datatype.String:
							{
								var val = ((ArrayParameter<string>)param).Value;
								return string.Join(",", val.Select(v => PipeEscape(v)));
							}
							case RcpTypes.Datatype.Uri:
							{
								var val = ((ArrayParameter<string>)param).Value;
								return string.Join(",", val.Select(v => PipeEscape(v)));
							}
							case RcpTypes.Datatype.Rgba:
							{
								var val = ((ArrayParameter<Color>)param).Value;
								return string.Join(",", val.Select(v => ColorToString(v)));
							}
							
							default: return ""; //param.Value.ToString();
						}
					}
					default: return "null";
				}
			}
			catch (Exception e)
			{
				return e.Message;
			}
		}
		
		public static string PipeUnEscape(string input)
		{
			if (string.IsNullOrEmpty(input))
				return input;
			
			if (input[0] == '|' && input[input.Length-1] == '|')
				input = input.Substring(1, input.Length-2);
			return input.Replace("||", "|");
		}
		
		//sets the value given as string on the given parameter
		public static Parameter StringToValue(Parameter param, string input)
		{
			try
			{
				switch(param.TypeDefinition.Datatype)
				{
					case RcpTypes.Datatype.Boolean:
					{
						var p = (ValueParameter<bool>)param;
						p.Value = ParseBool(input);
						return p;
					}
					case RcpTypes.Datatype.Enum:
					{
						var p = (EnumParameter)param;
						p.Value = PipeUnEscape(input);
						return p;
					}
					case RcpTypes.Datatype.Int32:
					{
						var p = (NumberParameter<int>)param;
						p.Value = ParseInt(input);
						return p;
					}
					case RcpTypes.Datatype.Float32:
					{
						var p = (NumberParameter<float>)param;
						p.Value = ParseFloat(input);
						return p;
					}
					case RcpTypes.Datatype.String:
					{
						var p = (StringParameter)param;
						p.Value = PipeUnEscape(input);
						return p;
					}
					case RcpTypes.Datatype.Uri:
					{
						var p = (UriParameter)param;
						p.Value = PipeUnEscape(input);
						return p;
					}
					case RcpTypes.Datatype.Rgba:
					{
						var p = (ValueParameter<Color>)param;
						p.Value = ParseColor(input);
						return p;
					}
					case RcpTypes.Datatype.Vector2f32:
					{
						var p = (NumberParameter<V2>)param;
						p.Value = ParseVector2(input);
						return p;
					}
					case RcpTypes.Datatype.Vector3f32:
					{
						var p = (NumberParameter<V3>)param;
						p.Value = ParseVector3(input);
						return p;
					}
					case RcpTypes.Datatype.Vector4f32:
					{
						var p = (NumberParameter<V4>)param;
						p.Value = ParseVector4(input);
						return p;
					}
					case RcpTypes.Datatype.Array:
					{
						switch ((param.TypeDefinition as IArrayDefinition).ElementType)
						{
							case RcpTypes.Datatype.Boolean:
							{
								var p = (ArrayParameter<bool>)param;
								p.Value = input.Split(',').Select(s => ParseBool(s)).ToArray();
								return p;
							}
							case RcpTypes.Datatype.Enum:
							{
								var p = (ArrayParameter<string>)param;
								p.Value = SplitToSlices(input).Select(s => PipeUnEscape(s)).ToArray();
								return p;
							}
							case RcpTypes.Datatype.Int32:
							{
								var p = (ArrayParameter<int>)param;
								p.Value = input.Split(',').Select(s => ParseInt(s)).ToArray();
								return p;
							}
							case RcpTypes.Datatype.String:
							{
								var p = (ArrayParameter<string>)param;
								p.Value = SplitToSlices(input).Select(s => PipeUnEscape(s)).ToArray();
								return p;
							}
							case RcpTypes.Datatype.Uri:
							{
								var p = (ArrayParameter<string>)param;
								p.Value = SplitToSlices(input).Select(s => PipeUnEscape(s)).ToArray();
								return p;
							}
							case RcpTypes.Datatype.Float32:
							{
								var p = (ArrayParameter<float>)param;
								p.Value = input.Split(',').Select(s => ParseFloat(s)).ToArray();
								return p;
							}
							case RcpTypes.Datatype.Vector2f32:
							{
								var p = (ArrayParameter<V2>)param;
								var v = input.Split(',');
								for (int i=0; i<v.Count()/2; i++)
									p.Value[i] = new V2(ParseFloat(v[i*2]), ParseFloat(v[i*2+1]));
								return p;
							}
							case RcpTypes.Datatype.Vector3f32:
							{
								var p = (ArrayParameter<V3>)param;
								var v = input.Split(',');
								for (int i=0; i<v.Count()/3; i++)
									p.Value[i] = new V3(ParseFloat(v[i*3]), ParseFloat(v[i*3+1]), ParseFloat(v[i*3+2]));
								return p;
							}
							case RcpTypes.Datatype.Vector4f32:
							{
								var p = (ArrayParameter<V4>)param;
								var v = input.Split(',');
								for (int i=0; i<v.Count()/4; i++)
									p.Value[i] = new V4(ParseFloat(v[i*4]), ParseFloat(v[i*4+1]), ParseFloat(v[i*4+2]), ParseFloat(v[i*4+3]));
								return p;
							}
							case RcpTypes.Datatype.Rgba:
							{
								var p = (ArrayParameter<Color>)param;
								//split at commas outside of pipes
								p.Value = SplitToSlices(input).Select(s => ParseColor(s)).ToArray();
								return p;
							}
						}
						break;
					}
				}
			}
			catch
			{
				//string parsing went wrong...						
			}
			
			return param;
		}
		
		private static List<string> SplitToSlices(string input)
		{
			return Regex.Split(input, @",(?=(?:[^\|]*\|[^\|]*\|)*[^\|]*$)").ToList();
		}
		
		public static V2 GetVector2(IPin2 pin, int index)
		{
			var x = ParseFloat(pin[index]);
			var y = ParseFloat(pin[index+1]);
			return new V2(x, y);
		}
		
		public static V2 ParseVector2(string input)
		{
			var comps = input.Split(',');
			return new V2(ParseFloat(comps[0]), ParseFloat(comps[1]));
		}
		
		public static V3 GetVector3(IPin2 pin, int index)
		{
			var x = ParseFloat(pin[index]);
			var y = ParseFloat(pin[index+1]);
			var z = ParseFloat(pin[index+2]);
			return new V3(x, y, z);
		}
		
		public static V3 ParseVector3(string input)
		{
			var comps = input.Split(',');
			return new V3(ParseFloat(comps[0]), ParseFloat(comps[1]), ParseFloat(comps[2]));
		}
		
		public static V4 GetVector4(IPin2 pin, int index)
		{
			var x = ParseFloat(pin[index]);
			var y = ParseFloat(pin[index+1]);
			var z = ParseFloat(pin[index+2]);
			var w = ParseFloat(pin[index+3]);
			return new V4(x, y, z, w);
		}
		
		public static V4 ParseVector4(string input)
		{
			var comps = input.Split(',');
			return new V4(ParseFloat(comps[0]), ParseFloat(comps[1]), ParseFloat(comps[2]), ParseFloat(comps[3]));
		}
		
		public static bool ParseBool(string input)
		{
			return input == "1" ? true : false;
		}
		
		public static ushort ParseEnum(string input, string[] entries)
		{
			return (ushort)entries.ToList().IndexOf(input);
		}
		
		public static float GetFloat(IPin2 pin, int index)
		{
			return ParseFloat(pin[index]);
		}
		
		public static float ParseFloat(string input)
		{
			float v;
			float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out v);
			return v;
		}
		
		public static int GetInt(IPin2 pin, int index)
		{
			return ParseInt(pin[index]);
		}
		
		public static int ParseInt(string input)
		{
			int v;
			int.TryParse(input, out v);
			return v;
		}
		
		public static Color GetColor(IPin2 pin, int index)
		{
			return ParseColor(pin[index]);
		}
		
		public static Color ParseColor(string input)
		{
			var comps = input.Trim('|').Split(',');
	        var r = 255 * float.Parse(comps[0], NumberStyles.Float, CultureInfo.InvariantCulture);
	        var g = 255 * float.Parse(comps[1], NumberStyles.Float, CultureInfo.InvariantCulture);
	        var b = 255 * float.Parse(comps[2], NumberStyles.Float, CultureInfo.InvariantCulture);
	        var a = 255 * float.Parse(comps[3], NumberStyles.Float, CultureInfo.InvariantCulture);
	        var color = Color.FromArgb((int)a, (int)r, (int)g, (int)b);
			return color;
		}
		
		public static string ColorToString(Color input)
		{
			return "|" + Float32ToString(input.R / 255f) + "," + Float32ToString(input.G / 255f) + "," + Float32ToString(input.B / 255f) + "," + Float32ToString(input.A / 255f) + "|";
		}
		
		public static string BoolToString(bool input)
		{
			return input ? "1" : "0";
		}
		
		public static string Float32ToString(float input)
		{
			return input.ToString(CultureInfo.InvariantCulture);
		}
		
		public static string Int32ToString(int input)
		{
			return input.ToString(CultureInfo.InvariantCulture);
		}
		
		public static string EnumToString(ushort input, string[] entries)
		{
			if (input >= 0 && input < entries.Length)
				return entries[input];
			else
				return "";
		}
		
		public static string Vector2f32ToString(V2 input)
		{
			return Float32ToString(input.X) + "," + Float32ToString(input.Y);
		}
		
		public static string Vector3f32ToString(V3 input)
		{
			return Float32ToString(input.X) + "," + Float32ToString(input.Y) + "," + Float32ToString(input.Z);
		}
		
		public static string Vector4f32ToString(V4 input)
		{
			return Float32ToString(input.X) + "," + Float32ToString(input.Y) + "," + Float32ToString(input.Z)+ "," + Float32ToString(input.W);
		}
		
		public static string DatatypeToString(ITypeDefinition definition)
		{
			var result = "";
			if (definition.Datatype == RcpTypes.Datatype.Array)
			{
				var def = definition as IArrayDefinition;
				result = "Array<" + def.ElementType.ToString() + ">";
			}
			else
				result = definition.Datatype.ToString();
			
			return result;
		}
		
		public static string TypeDefinitionToString(ITypeDefinition definition)
		{
			try
			{
				switch(definition.Datatype)
				{
					case RcpTypes.Datatype.Boolean:
					{
						var def = (IBoolDefinition)definition;
						return def.Default ? "1" : "0";
					}
					case RcpTypes.Datatype.Enum:
					{
						var def = (IEnumDefinition)definition;
						return def.Default; //, ((IEnumDefinition)def).Entries) + ", [" + string.Join(",", def.Entries) + "]";
					}
					case RcpTypes.Datatype.Int32:
					{
						var def = (INumberDefinition<int>)definition;
						return Int32ToString(def.Default) + ", " + Int32ToString((int)def.Minimum) + ", " + Int32ToString((int)def.Maximum) + ", " + Int32ToString((int)def.MultipleOf) + ", " + def.Unit;
					}
					case RcpTypes.Datatype.Float32:
					{
						var def = (INumberDefinition<float>)definition;
						return Float32ToString(def.Default) + ", " + Float32ToString((float)def.Minimum) + ", " + Float32ToString((float)def.Maximum) + ", " + Float32ToString((float)def.MultipleOf) + ", " + def.Unit;
					}
					case RcpTypes.Datatype.Vector2f32:
					{
						var def = (INumberDefinition<V2>)definition;
						return Vector2f32ToString(def.Default) + ", " + Vector2f32ToString((V2)def.Minimum) + ", " + Vector2f32ToString((V2)def.Maximum) + ", " + Vector2f32ToString((V2)def.MultipleOf) + ", " + def.Unit;
					}
					case RcpTypes.Datatype.Vector3f32:
					{
						var def = (INumberDefinition<V3>)definition;
						return Vector3f32ToString(def.Default) + ", " + Vector3f32ToString((V3)def.Minimum) + ", " + Vector3f32ToString((V3)def.Maximum) + ", " + Vector3f32ToString((V3)def.MultipleOf) + ", " + def.Unit;
					}
					case RcpTypes.Datatype.Vector4f32:
					{
						var def = (INumberDefinition<V4>)definition;
						return Vector4f32ToString(def.Default) + ", " + Vector4f32ToString((V4)def.Minimum) + ", " + Vector4f32ToString((V4)def.Maximum) + ", " + Vector4f32ToString((V4)def.MultipleOf) + ", " + def.Unit;
					}
					case RcpTypes.Datatype.String:
					{
						var def = (IStringDefinition)definition;
						return def.Default;
					}
					case RcpTypes.Datatype.Uri:
					{
						var def = (IUriDefinition)definition;
						return def.Default + ", " + def.Schema + ", " + def.Filter;
					}
					case RcpTypes.Datatype.Rgba:
					{
						var def = (IRGBADefinition)definition;
						return ColorToString(def.Default);
					}
					case RcpTypes.Datatype.Array:
					{
						var def = definition as IArrayDefinition;
						switch(def.ElementType)
						{
							case RcpTypes.Datatype.Boolean: return TypeDefinitionToString((IBoolDefinition)def.ElementDefinition);
							case RcpTypes.Datatype.Float32: return TypeDefinitionToString((INumberDefinition<float>)def.ElementDefinition);
							case RcpTypes.Datatype.Int32: return TypeDefinitionToString((INumberDefinition<int>)def.ElementDefinition);
							case RcpTypes.Datatype.Vector2f32: return TypeDefinitionToString((INumberDefinition<V2>)def.ElementDefinition);
							case RcpTypes.Datatype.Vector3f32: return TypeDefinitionToString((INumberDefinition<V3>)def.ElementDefinition);
							case RcpTypes.Datatype.Vector4f32: return TypeDefinitionToString((INumberDefinition<V4>)def.ElementDefinition);
							case RcpTypes.Datatype.String: return TypeDefinitionToString((IStringDefinition)def.ElementDefinition);
							case RcpTypes.Datatype.Uri: return TypeDefinitionToString((IUriDefinition)def.ElementDefinition);
							case RcpTypes.Datatype.Rgba: return TypeDefinitionToString((IRGBADefinition)def.ElementDefinition);
							case RcpTypes.Datatype.Enum: return TypeDefinitionToString((IEnumDefinition)def.ElementDefinition);
							
							default: return "Unknown Type";
						}
					}
					case RcpTypes.Datatype.Group: return "";
					
					default: return "Unknown Type";
				}
			}
			catch (Exception e)
			{
				return e.Message;
			}
		}
	}
	
	/// <summary>
    /// Little wrapper around the native IO boxes so we've a unified view on them.
    /// </summary>
    abstract class IOBox
    {
        public static IOBox Wrap(INode2 node)
        {
            return Wrap(node.InternalCOMInterf, node.NodeInfo);
        }

        public static IOBox Wrap(INode node, INodeInfo nodeInfo)
        {
            var name = nodeInfo.ToString();
            switch (name)
            {
                case "IOBox (Value Advanced)":
                    return new ValueIOBox(node, nodeInfo);
                case "IOBox (Color)":
                    return new ColorIOBox(node, nodeInfo);
                case "IOBox (String)":
                    return new StringIOBox(node, nodeInfo);
            	case "IOBox (Enumerations)":
                    return new EnumIOBox(node, nodeInfo);
                default:
                    break;
            }
            return null;
        }

        public IOBox(INode node, INodeInfo nodeInfo)
        {
            Id = node.GetNodePath(useDescriptiveNames: false);
            Name = nodeInfo.ToString();
            Node = node;
            InputPin = GetInputPin(node);
        }

        /// <summary>
        /// Pointer to the native IO box node.
        /// </summary>
        public INode Node { get; private set; }

        /// <summary>
        /// Pointer to the native input pin of the IO box.
        /// </summary>
        public IPin InputPin { get; private set; }

        /// <summary>
        /// The ID of the IO box.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// The name of the IO box.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Retrieve the input pin with which to sync.
        /// </summary>
        protected abstract IPin GetInputPin(INode node);

        /// <summary>
        /// Sync our spread with the native pin.
        /// </summary>
        /// <returns>True if the data changed.</returns>
        public abstract bool Sync();

        public override string ToString()
        {
            return Name + " " + Id;
        }
    }

    abstract class IOBox<T> : IOBox
    {
        public IOBox(INode node, INodeInfo nodeInfo) : base(node, nodeInfo)
        {
            Spread = new Spread<T>();
        }

        public Spread<T> Spread { get; private set; }
    }

    sealed class ValueIOBox : IOBox<double>
    {
        readonly IValueData FData;

        public ValueIOBox(INode node, INodeInfo nodeInfo) : base(node, nodeInfo)
        {
            FData = InputPin as IValueData;
        }

        protected override IPin GetInputPin(INode node)
        {
            return node.GetPin("Y Input Value");
        }

        public override bool Sync()
        {
            var changed = false;
            Spread.SliceCount = InputPin.SliceCount;
            for (int i = 0; i < Spread.SliceCount; i++)
            {
                double value;
                FData.GetValue(i, out value);
                if (value != Spread[i])
                {
                    Spread[i] = value;
                    changed = true;
                }
            }
            return changed;
        }
    }

    sealed class StringIOBox : IOBox<string>
    {
        readonly IStringData FData;

        public StringIOBox(INode node, INodeInfo nodeInfo) : base(node, nodeInfo)
        {
            FData = InputPin as IStringData;
        }

        protected override IPin GetInputPin(INode node)
        {
            return node.GetPin("Input String");
        }

        public override bool Sync()
        {
            var changed = false;
            Spread.SliceCount = InputPin.SliceCount;
            for (int i = 0; i < Spread.SliceCount; i++)
            {
                string value;
                FData.GetString(i, out value);
                if (value != Spread[i])
                {
                    Spread[i] = value;
                    changed = true;
                }
            }
            return changed;
        }
    }

    sealed class ColorIOBox : IOBox<RGBAColor>
    {
        readonly IColorData FData;

        public ColorIOBox(INode node, INodeInfo nodeInfo) : base(node, nodeInfo)
        {
            FData = InputPin as IColorData;
        }

        protected override IPin GetInputPin(INode node)
        {
            return node.GetPin("Color Input");
        }

        public override bool Sync()
        {
            var changed = false;
            Spread.SliceCount = InputPin.SliceCount;
            for (int i = 0; i < Spread.SliceCount; i++)
            {
                RGBAColor value;
                FData.GetColor(i, out value);
                if (value != Spread[i])
                {
                    Spread[i] = value;
                    changed = true;
                }
            }
            return changed;
        }
    }
	
	sealed class EnumIOBox : IOBox<string>
    {
        readonly IStringData FData;

        public EnumIOBox(INode node, INodeInfo nodeInfo) : base(node, nodeInfo)
        {
            FData = InputPin as IStringData;
        }

        protected override IPin GetInputPin(INode node)
        {
            return node.GetPin("Input Enum");
        }

        public override bool Sync()
        {
            var changed = false;
            Spread.SliceCount = InputPin.SliceCount;
            for (int i = 0; i < Spread.SliceCount; i++)
            {
                string value;
                FData.GetString(i, out value);
                if (value != Spread[i])
                {
                    Spread[i] = value;
                    changed = true;
                }
            }
            return changed;
        }
    }
}