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
	            Category = "Network",
	            Help = "Communicates with the Kontrolleur Android app",
	            AutoEvaluate = true)]
	#endregion PluginInfo
	public class KontrolleurNode: IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
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
		
		[Output("Touch ID")]
		ISpread<int> FTouchID;
		
		[Output("Touch")]
		ISpread<Vector2D> FTouchXY;
		
		[Output("Touch Pressure")]
		ISpread<double> FTouchPressure;
		
		[Output("Acceleration")]
		ISpread<Vector3D> FAcceleration;
		
		[Output("Orientation")]
		ISpread<Vector3D> FOrientation;
		
		[Output("Magnetism")]
		ISpread<Vector3D> FMagnetism;
		
		private Vector2D FResolution;
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
		
		[Import]
		IHDEHost FHDEHost;
		
		private Timer FTimer = new Timer(2000);
		private bool FAllowUpdates = true;
		private XmlDocument FXML = new XmlDocument();
		private List<IPin2> FBangs = new List<IPin2>();
		private Dictionary<string, RemoteValue> FTargets = new Dictionary<string, RemoteValue>();
		private Dictionary<string, string> FSaveTargets = new Dictionary<string, string>();
		private Dictionary<string, PatchMessage> FPatchMessages = new Dictionary<string, PatchMessage>();
		#endregion fields & pins
		
		#region constructor/destructor
		[ImportingConstructor]
		public KontrolleurNode(IHDEHost host)
		{
			FRoot = host.RootNode;

			FTimer.Enabled = true;
			FTimer.AutoReset = false;
			FTimer.Elapsed += new ElapsedEventHandler(FTimer_Elapsed);
		}
		
		~KontrolleurNode()
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
					FTimer.Enabled = false;
					FTimer.Dispose();
					
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
			
			if (string.IsNullOrEmpty(node.LabelPin[0]))
				unexpose = true;
			else
			{
				var name = node.LabelPin[0];
				
				if ((FPrefixes.Count == 0)
				    || ((FPrefixes.Count > 0) && name.StartsWith(FPrefix[0])))
				{
					var target = new RemoteValue(node, FPrefixes);
					
					if (!FTargets.ContainsKey(target.RuntimeNodePath))
						FTargets.Add(target.RuntimeNodePath, target);
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
			var address = node.GetNodePath(false);
			
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
				FSaveTargets.Clear();
				
				FAutoIP = IPAddress.Parse((string)message.Values[0]);
				FAutoPort = (int) message.Values[1];
				FResolution = new Vector2D((int) message.Values[2], (int) message.Values[3]);

				InitNetwork();
				
				UnRegisterPatch(FRoot);
				RegisterPatch(FRoot);
				
				return;
			}
			else if (message.Address == "/k/save")
			{
				//save all savetargets to patch by sending a patchmessage
				//savetargets can be scattered over many patches so potentially this needs to send multiple patchmessages
				
				FPatchMessages.Clear();
				foreach (var target in FSaveTargets)
				{
					var patchClass = target.Key.Split('/');
					PatchMessage pm;
					if (FPatchMessages.ContainsKey(patchClass[0]))
						pm = FPatchMessages[patchClass[0]];
					else
					{
						pm = new PatchMessage(patchClass[0]);
						FPatchMessages.Add(patchClass[0], pm);
					}
					
					var node = pm.AddNode(int.Parse(patchClass[1]));
					var pin = node.AddPin("Y Input Value");
					pin.AddAttribute("values", target.Value);
				}
				
				foreach (var pm in FPatchMessages)
					FHDEHost.SendXMLSnippet(pm.Key, pm.Value.ToString(), true);
			}
			else if (message.Address == "/acceleration")
			{
				FAcceleration[0] = new Vector3D((float)message.Values[0], (float)message.Values[1], (float)message.Values[2]);
			}
			else if (message.Address == "/magnetism")
			{
				FMagnetism[0] = new Vector3D((float)message.Values[0], (float)message.Values[1], (float)message.Values[2]);
			}
			else if (message.Address == "/orientation")
			{
				FOrientation[0] = new Vector3D((float)message.Values[0], (float)message.Values[1], (float)message.Values[2]);
			}
			else if (message.Address == "/touches")
			{
				int touchCount = message.Values.Count / 4;
				int t = 0;
				FTouchID.SliceCount = touchCount;
				FTouchXY.SliceCount = touchCount;
				FTouchPressure.SliceCount = touchCount;
				
				for (int i = 0; i < touchCount; i++)
				{
					FTouchID[i] = (int) message.Values[t];
					FTouchXY[i] = new Vector2D((float)message.Values[t + 1] / FResolution.x * 2 - 1, (float)message.Values[t + 2] / FResolution.y * -2 + 1);
					FTouchPressure[i] = (float) message.Values[t + 3];
					t += 4;
				}
			}
			else if (message.Address.StartsWith("/k/"))
			{
				FAllowUpdates = false;
				
				var address = "/" + message.Address.Trim('/','k');
				if (FTargets.ContainsKey(address))
				{
					var pin = FTargets[address].Pin;
					
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
					
					//save last value sent per path (for optional later saving to patch)
					if (FSaveTargets.ContainsKey(FTargets[address].SourceNodePath))
						FSaveTargets[FTargets[address].SourceNodePath] = values;
					else
						if (!(FTargets[address].Type == "Bang"))
							FSaveTargets.Add(FTargets[address].SourceNodePath, values);
					
					pin.Spread = values;
				}
			}
		}
		#endregion OSC
		
		#region MainLoop
		public void Evaluate(int SpreadMax)
		{
			FTouchID.SliceCount = 0;
			FTouchXY.SliceCount = 0;
			FTouchPressure.SliceCount = 0;
			
			//re/init udp
			if (FKontrolleurIP.IsChanged || FKontrolleurPort.IsChanged)
			{
				try
				{
					FManualIP = IPAddress.Parse(FKontrolleurIP[0]);
					FManualPort = FKontrolleurPort[0];
					InitNetwork();
				}
				catch
				{}
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
				pin.Spread = "0";
			}
			FBangs.Clear();
			
			//parse incoming OSC
			if (!string.IsNullOrEmpty(message))
			{
				var bundlePos = message.IndexOf("#bundle");
				if (bundlePos == -1)
					return;
				
				while ((bundlePos = message.IndexOf("#bundle")) >= 0)
				{
					var nextpos = message.IndexOf("#bundle", bundlePos + 1);
					var bundleMessage = "";
					if (nextpos == -1)
					{
						bundleMessage = message;
						message = "";
					}
					else
					{
						bundleMessage = message.Substring(bundlePos, nextpos - bundlePos);
						message = message.Substring(nextpos);
					}
					
					var packet = OSCPacket.Unpack(Encoding.Default.GetBytes(bundleMessage));
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
			
			//send targets to kontrolleur
			var bundle = new OSCBundle();
			foreach (var target in FTargets.Values)
			{
				if (target.State == RemoteValueState.Idle)
					continue;
				
				OSCMessage osc;
				if (target.State == RemoteValueState.Remove)
				{
					osc = new OSCMessage("/k/remove");
					osc.Append(target.RuntimeNodePath);
					bundle.Append(osc);
					continue;
				}
				else if (target.State == RemoteValueState.Add)
				{
					osc = new OSCMessage("/k/add");
					osc.Append(target.RuntimeNodePath);
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
					osc.Append(target.RuntimeNodePath);
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
