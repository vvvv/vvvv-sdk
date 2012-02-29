#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Linq;
using System.Timers;
using System.Net;

using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.Core.Collections;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.OSC;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Kontrolleur",
	            Category = "VVVV",
	            Help = "Communicates with the Kontrolleur Android app")]
	#endregion PluginInfo
	public class StringOSC2PatchNode: IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
	{
		#region fields & pins
		[Config("Kontrolleur IP", IsSingle=true, DefaultString="")]
		IDiffSpread<string> FKontrolleurIP;
		
		[Config("Kontrolleur Port", IsSingle=true, DefaultValue=0)]
		IDiffSpread<int> FKontrolleurPort;
		
		[Input("OSC Message")]
		ISpread<string> FOSCIn;
		
		[Input("Prefix", IsSingle=true)]
		IDiffSpread<string> FPrefix;
		
		[Output("Kontrolleur Resolution")]
		ISpread<Vector2D> FKontrolleurResolution;
		
		private INode2 FRoot;
		private OSCTransmitter FOSCTransmitter;
		private IPAddress FTargetIP;
		private int FTargetPort;
		private IPAddress FAutoIP;
		private int FAutoPort;
		private IPAddress FManualIP;
		private int FManualPort;
		private bool FFirstFrame = true;
		private List<string> FPrefixes = new List<string>();
		// Track whether Dispose has been called.
		private bool FDisposed = false;
		
		[Import]
		ILogger FLogger;
		
		private Timer FTimer = new Timer(2000);
		private bool FAllowUpdates = true;
		private XmlDocument FXML = new XmlDocument();
		private List<IPin2> FBangs = new List<IPin2>();
		private Dictionary<string, RemoteValue> FTargets = new Dictionary<string, RemoteValue>();
		#endregion fields & pins
		
		#region constructor/destructor
		[ImportingConstructor]
		public StringOSC2PatchNode(IHDEHost host)
		{
			FRoot = host.RootNode;

			FTimer.Enabled = true;
			FTimer.AutoReset = false;
			FTimer.Elapsed += new ElapsedEventHandler(FTimer_Elapsed);
		}
		
		~StringOSC2PatchNode()
		{
			Dispose(false);
		}
		
		public void Dispose()
		{
			Dispose(true);
		}
		
		protected void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if(!FDisposed)
			{
				if(disposing)
				{
					// Dispose managed resources.
					FPrefix.Changed -= PrefixChangedCB;
					UnRegisterPatch(FRoot);
				}
				// Release unmanaged resources. If disposing is false,
				// only the following code is executed.
				
			}
			FDisposed = true;
		}
		#endregion constructor/destructor
		
		#region events
		public void OnImportsSatisfied()
		{
			FPrefix.Changed += PrefixChangedCB;
		}
		
		private void PrefixChangedCB(IDiffSpread spread)
		{
			FPrefixes.Clear();
			foreach (string prefix in spread)
				FPrefixes.Add(prefix);
			
			UnRegisterPatch(FRoot);
			RegisterPatch(FRoot);
		}
		
		private void FTimer_Elapsed(object Sender, ElapsedEventArgs e)
		{
			FAllowUpdates = true;
		}
		#endregion events
		
		#region network
		private void InitNetwork()
		{
			IPAddress newIP = null;
			int newPort = 0;
			
			if (FAutoIP != null)
				newIP = FAutoIP;
			
			if (FAutoPort > 0)
				newPort = FAutoPort;
			
			//manual settings take precedence
			if (FManualIP != null)
				newIP = FManualIP;
			
			if (FManualPort > 0)
				newPort = FManualPort;
			
			if (!newIP.Equals(FTargetIP) || newPort != FTargetPort)
			{
				FTargetIP = newIP;
				FTargetPort = newPort;
				
				if (FTargetIP != null)
					try
				{
					if (FOSCTransmitter != null)
						FOSCTransmitter.Close();
					FOSCTransmitter = new OSCTransmitter(FTargetIP.ToString(), FTargetPort);
					FOSCTransmitter.Connect();

					FLogger.Log(LogType.Debug, "connected to Kontrolleur on: " + FTargetIP.ToString() + ":" + FTargetPort.ToString());
				}
				catch (Exception e)
				{
					FLogger.Log(LogType.Warning, "Kontrolleur: failed to open port " + FTargetPort.ToString());
					FLogger.Log(LogType.Warning, "Kontrolleur: " + e.Message);
				}
			}
		}
		#endregion network
		
		#region exposing IOBoxes
		private void RegisterPatch(INode2 patch)
		{
			foreach (var node in patch)
			{
				if (node.HasPatch)
				{
					node.Added += new CollectionDelegate<INode2>(NodeAddedCB);
					node.Removed += NodeRemovedCB;
					RegisterPatch(node);
				}
				else if (node.Name.Contains("IOBox"))
					RegisterIOBox(node);
			}
		}
		
		private void RegisterIOBox(INode2 io)
		{
			//for now only accepts value-IOBoxes
			if (!io.Name.Contains("Value"))
				return;
			
			//now see if this box already wants to be exposed
			CheckExposeIOBox(io);
			
			//register for labelchanges
			io.LabelPin.Changed += LabelChangedCB;
		}
		
		private void UnRegisterPatch(INode2 patch)
		{
			foreach (INode2 node in patch)
			{
				if (node.HasPatch)
				{
					node.Added -= NodeAddedCB;
					node.Removed -= NodeRemovedCB;
					UnRegisterPatch(node);
				}
				else if (node.Name.Contains("IOBox"))
					UnRegisterIOBox(node);
			}
		}
		
		private void UnRegisterIOBox(INode2 io)
		{
			//for now only accepts value-IOBoxes
			if (!io.Name.Contains("Value"))
				return;
			
			UnExposeIOBox(io);
			
			//unregister from labelchanges
			io.LabelPin.Changed -= LabelChangedCB;
		}
		
		private void NodeAddedCB(IViewableCollection collection, object item)
		{
			var node = item as INode2;
			
			if (node.HasPatch)
				RegisterPatch(node);
			else if (node.Name.Contains("IOBox"))
				RegisterIOBox(node);
		}
		
		private void NodeRemovedCB(IViewableCollection collection, object item)
		{
			var node = item as INode2;

			if (node.HasPatch)
				UnRegisterPatch(node);
			else if (node.Name.Contains("IOBox"))
				UnRegisterIOBox(node);
		}
		
		private void CheckExposeIOBox(INode2 node)
		{
			bool unexpose = false;
			
			if (string.IsNullOrEmpty(node.LabelPin.GetSlice(0)))
				unexpose = true;
			else
			{
				var name = node.LabelPin.GetSlice(0);
				
				if ((FPrefixes.Count == 0)
				    || ((FPrefixes.Count > 0) && name.StartsWith(FPrefix[0])))
				{
					var target = new RemoteValue(node, FPrefixes);
					
					if (!FTargets.ContainsKey(target.Address))
						FTargets.Add(target.Address, target);
					else
						target.Kill();
				}
				else
					unexpose = true;
			}
			
			if (unexpose)
				UnExposeIOBox(node);
		}
		
		private void UnExposeIOBox(INode2 node)
		{
			var address = "/" + node.Parent.NodeInfo.Filename + "/" + node.ID;
			
			if (FTargets.ContainsKey(address))
				FTargets[address].State = RemoteValueState.Remove;
		}
		
		private void LabelChangedCB(object Sender, EventArgs e)
		{
			var labelPin = Sender as IPin2;
			CheckExposeIOBox(labelPin.ParentNode);
		}
		#endregion exposing IOBoxes
		
		#region OSC
		private void ProcessOSCMessage(OSCMessage message)
		{
			if (message.Address == "/k/init")
			{
				foreach (var target in FTargets.ToArray())
					target.Value.Kill();
				FTargets.Clear();
				
				FAutoIP = IPAddress.Parse((string)message.Values[0]);
				FAutoPort = (int) message.Values[1];
				FKontrolleurResolution[0] = new Vector2D((int) message.Values[2], (int) message.Values[3]);
				
				InitNetwork();
				
				UnRegisterPatch(FRoot);
				RegisterPatch(FRoot);
				
				return;
			}
			else if (!message.Address.StartsWith("/k/"))
				return;
			
			var address = "/" + message.Address.Trim('/','k');
			if (FTargets.ContainsKey(address))
			{
				var pin = FTargets[address].Node.FindPin("Y Input Value");
				
				//var values = string.Join(",", message.Values.ToArray(typeof(string)));
				var values = "";
				foreach(var v in message.Values)
					values += v.ToString().Replace(',', '.') + ",";
				values = values.TrimEnd(',');
				
				if (values == "bang")
				{
					values = "1";
					//save pin for sending 0 next frame
					FBangs.Add(pin);
				}
				
				pin.SetSpread(values, false);
			}
		}
		#endregion OSC
		
		#region MainLoop
		public void Evaluate(int SpreadMax)
		{
			//re/init udp
			if (FKontrolleurIP.IsChanged || FKontrolleurPort.IsChanged)
			{
				FManualIP = IPAddress.Parse(FKontrolleurIP[0]);
				FManualPort = FKontrolleurPort[0];
				InitNetwork();
			}
			
			if (FFirstFrame)
			{
				//go down recursively and register with every patch and IOBox
				RegisterPatch(FRoot);
				FFirstFrame = false;
			}
			
			//convert the incoming oscmessage to vvvv xml patch messages
			//return multiple slices, one per patch addressed
			//ie. multiple nodes addressed in one patch go to the same slice
			var message = FOSCIn[0];
			//special treatment for bangs
			//which cause 2 patch messages in consecutive frames to be sent
			foreach (var pin in FBangs)
			{
				//set all the bangs values to 0
				pin.SetSpread("0", false);
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
						osc.Append(target.Default);
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
						osc.Append(target.Default);
						osc.Append(target.Minimum);
						osc.Append(target.Maximum);
						osc.Append(target.Stepsize);
						osc.Append(target.Value);
						bundle.Append(osc);
					}
				}
				
				if (FOSCTransmitter != null)
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
				{
					target.Value.Kill();
					FTargets.Remove(target.Key);
				}
					
					else
						target.Value.InvalidateState();
			}
		}
		#endregion MainLoop
	}
}