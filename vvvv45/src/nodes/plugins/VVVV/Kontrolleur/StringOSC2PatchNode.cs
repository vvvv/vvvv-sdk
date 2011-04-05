#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Linq;
using System.Timers;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;
using VVVV.Utils.OSC;
#endregion usings

namespace VVVV.Nodes
{	
	#region PluginInfo
	[PluginInfo(Name = "OSC2Patch",
	Category = "String",
	Help = "Converts OSC messages to vvvv patch messages",
	Tags = "remote")]
	#endregion PluginInfo
	public class StringOSC2PatchNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("OSC Message")]
		ISpread<string> FOSCIn;
		
		[Input("Prefix", IsSingle=true)]
		ISpread<string> FPrefix;
		
		[Output("Patch")]
		ISpread<string> FPatchOut;
		
		[Output("Patch Message")]
		ISpread<string> FMessageOut;
		
		[Input("Kontrolleur Host", IsSingle=true, DefaultString="192.168.1.255")]
		IDiffSpread<string> FKontrolleurHost;
		
		[Input("Kontrolleur Port", IsSingle=true, DefaultValue=4444)]
		IDiffSpread<int> FKontrolleurPort;
		
		private IHDEHost FHost;
		private INode2 FRoot;
		private OSCTransmitter FOSCTransmitter;
		
		[Import()]
		ILogger FLogger;
		
		private Timer FTimer = new Timer(2000);
		private bool FAllowUpdates = true;
		private XmlDocument FXML = new XmlDocument();
		private Dictionary<string, string> FMessages = new Dictionary<string, string>();
		private Dictionary<string, string> FBangs = new Dictionary<string, string>();
		private Dictionary<string, RemoteValue> FTargets = new Dictionary<string, RemoteValue>();
		#endregion fields & pins
		
		[ImportingConstructor]
		public StringOSC2PatchNode(IHDEHost host)
		{
			FRoot = host.RootNode;
			FTimer.Enabled = true;
			FTimer.AutoReset = false;
			FTimer.Elapsed += new ElapsedEventHandler(FTimer_Elapsed);
		}
		
		private void FTimer_Elapsed(object Sender, ElapsedEventArgs e)
		{
			FAllowUpdates = true;
		}
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			//re/init udp
			if (FKontrolleurHost.IsChanged || FKontrolleurPort.IsChanged)
			{
				try
				{
					if (FOSCTransmitter != null)
						FOSCTransmitter.Close();
					FOSCTransmitter = new OSCTransmitter(FKontrolleurHost[0], FKontrolleurPort[0]);
					FOSCTransmitter.Connect();
				}
				catch (Exception e)
				{
					FLogger.Log(LogType.Warning, "Kontrolleur: failed to open port " + FKontrolleurPort[0].ToString());
					FLogger.Log(LogType.Warning, "Kontrolleur: " + e.Message);
				}
			}
			
			//convert the incoming oscmessage to vvvv xml patch messages
			//return multiple slices, one per patch addressed
			//ie. multiple nodes addressed in one patch go to the same slice
			var message = FOSCIn[0];
			FMessages.Clear();
			foreach (var bang in FBangs)
			{
				//set all the bangs values to 0
				var b = bang.Value.Replace("values=\"1\"", "values=\"0\"");
				FMessages.Add(bang.Key, b);
			}	
			FBangs.Clear();
						
			if (!string.IsNullOrEmpty(message))
			{
				FAllowUpdates = false;
				
				var bundlePos = message.IndexOf("#bundle");
				if (bundlePos == -1)
				return;
				
				while ((bundlePos = message.IndexOf("#bundle")) >= 0)
				{
					var nextpos = message.IndexOf("#bundle", bundlePos + 1);
					var bundle = "";
					if (nextpos == -1)
					{
						bundle = message;
						message = "";
					}
					else
					{
						bundle = message.Substring(bundlePos, nextpos - bundlePos);
						message = message.Substring(nextpos);
					}
					
					var packet = OSCPacket.Unpack(Encoding.Default.GetBytes(bundle));
					if (packet.IsBundle())
					{
						ArrayList messages = packet.Values;
						for (int i = 0; i < messages.Count; i++)
						{
							ProcessOSCMessage((OSCMessage)messages[i]);
						}
					}
					else
						ProcessOSCMessage((OSCMessage)packet);
				}
				
				FTimer.Start();
			}
			else 
			{
				//send targets to kontrolleur
				//invalidate targets
				foreach (var target in FTargets.Values)
					target.InvalidateState();
				
				//update targets
				GetTargetNodes(FRoot);
				
				//writeout targets to pins
				OSCBundle bundle = new OSCBundle();
				foreach (var target in FTargets.Values)
				{
					if (target.State == RemoteValueState.Idle)
						continue;
					
					OSCMessage osc;
					if (target.State == RemoteValueState.Remove)
					{
						osc = new OSCMessage("/k/remove");
						osc.Append(target.Address);
						bundle.Append(osc);
						continue;
					}
					else if (target.State == RemoteValueState.Add)
					{
						osc = new OSCMessage("/k/add");
						osc.Append(target.Address);
						osc.Append(target.Name);
						osc.Append(target.Type);
						osc.Append(target.Minimum);
						osc.Append(target.Maximum);
						osc.Append(target.Stepsize);
						osc.Append(target.Value);
						bundle.Append(osc);
					}	
					else if (FAllowUpdates && target.State == RemoteValueState.Update)
					{
						osc = new OSCMessage("/k/update");
						osc.Append(target.Address);
						osc.Append(target.Name);
						osc.Append(target.Type);
						osc.Append(target.Minimum);
						osc.Append(target.Maximum);
						osc.Append(target.Stepsize);
						osc.Append(target.Value);
						bundle.Append(osc);
					}
				}
				
				try
				{
					FOSCTransmitter.Send(bundle);
				}
				catch (Exception ex)
				{
					FLogger.Log(LogType.Warning, "Kontrolleur: " + ex.Message);
				}
				
				//remove unused targets
				foreach (var target in FTargets.ToArray())
					if (target.Value.State == RemoteValueState.Remove)
						FTargets.Remove(target.Key);
			}
			
			//return patch messages
			FPatchOut.AssignFrom(FMessages.Keys);
			FMessageOut.AssignFrom(FMessages.Values);
		}
		
		private void GetTargetNodes(INode2 node)
		{
			foreach(var n in node)
			{
				if (n.Name == "IOBox (Value Advanced)")
				{
					foreach(var p in n.Pins)
					if ((p.Name == "Descriptive Name") && (!string.IsNullOrEmpty(p[0]) && p[0].StartsWith(FPrefix[0])))
					{
						var address = "/" + node.NodeInfo.Filename + "/" + n.ID;
						var name = p[0];
						if (!String.IsNullOrEmpty(FPrefix[0]))
							name = name.Replace(FPrefix[0], "");
					    float v = 0;
						string t = "Slider";
						foreach(var pn in n.Pins)
						{
							//todo: minimum, maximum, stepsize
							if (pn.Name == "Slider Behavior")
								t = pn[0];
							else if (pn.Name == "Y Input Value")
							{
								v = float.Parse( pn[0].Replace('.', ','));
								break;
							}
						}
						
						//update
						if (FTargets.ContainsKey(address))
						{
							FTargets[address].Update(name, t, 0, 1, 0.01f, v);
						}
						else //add
						{
							var target = new RemoteValue(address, name, t, 0, 1, 0.01f, v);
							FTargets.Add(address, target);
						}
						
						break;
					}	
				}
				GetTargetNodes(n);
			}
		}
		
		#region OSC
		private void ProcessOSCMessage(OSCMessage message)
		{
			if (message.Address == "/k/reset")
			{
				FTargets.Clear();
				return;
			}
			else if (!message.Address.StartsWith("/k/"))
				return;
			
			var address = message.Address.Split('/');
			if (!FMessages.ContainsKey(address[2]))
				FMessages.Add(address[2], "<PATCH></PATCH>");
			
			var m = FMessages[address[2]];
			FXML.LoadXml(m);
			var patch = FXML.SelectSingleNode(@"//PATCH");
			
			var node = FXML.CreateElement("NODE");
			patch.AppendChild(node);
			var id = FXML.CreateAttribute("id");
			id.Value = address[3];
			node.Attributes.Append(id);
			
			//var values = string.Join(",", message.Values.ToArray(typeof(string)));
			var values = "";
			foreach(var v in message.Values)
				values += v.ToString().Replace(',', '.') + ",";
			values = values.TrimEnd(',');
			
			if (values == "bang")
			{
				node.InnerXml = "<PIN pinname=\"Y Input Value\" values=\"1\" />";
				FBangs.Add(address[2], patch.OuterXml);
			}	
			else
				node.InnerXml = "<PIN pinname=\"Y Input Value\" values=\"" + values + "\" />";
			
			FMessages[address[2]] = patch.OuterXml;
		}
		#endregion OSC
	}
	
	enum RemoteValueState {Idle, Add, Update, Remove}
	
	class RemoteValue
	{
		public string Address;
		public string Name;
		public string Type;
		public float Minimum;
		public float Maximum;
		public float Stepsize;
		public float Value;
		public RemoteValueState State;
	
		public RemoteValue(string Address, string Name, string Type, float Minimum, float Maximum, float Stepsize, float Value)
		{
			this.Address = Address;
			this.Name = Name;
			this.Type = Type;
			this.Minimum = Minimum;
			this.Maximum = Maximum;
			this.Stepsize = Stepsize;
			this.Value = Value;
			State = RemoteValueState.Add;
		}
		
		public void Update(string Name, string Type, float Minimum, float Maximum, float Stepsize, float Value)
		{
			if ((this.Name != Name)
			|| (this.Type != Type)
			|| (this.Minimum != Minimum)
			|| (this.Maximum != Maximum)
			|| (this.Stepsize != Stepsize)
			|| (this.Value != Value))
			{
				State = RemoteValueState.Update;
				this.Name = Name;
				this.Type = Type;
				this.Minimum = Minimum;
				this.Maximum = Maximum;
				this.Stepsize = Stepsize;
				this.Value = Value;
			}
			else
				State = RemoteValueState.Idle;
		}
		
		public void InvalidateState()
		{
			State = RemoteValueState.Remove;
		}
	}
}
