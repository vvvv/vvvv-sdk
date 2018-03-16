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
using System.Threading;

using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.Core.Collections;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.OSC;
using VVVV.PluginInterfaces.V2.NonGeneric;
using VVVV.Hosting;
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
#pragma warning disable 0649
        [Input("Listening UDP Port", IsSingle = true, DefaultValue = 44444)]
        IDiffSpread<int> FUDPPort;

        [Input("Patch Filter", IsSingle = true)]
        IDiffSpread<string> FPatchFilter;

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

        [Import]
        ILogger FLogger;

        [Import]
        IHDEHost FHDEHost; 
#pragma warning restore
		
		private Vector2D FResolution;
		private INode2 FRoot;
		private OSCTransmitter FOSCTransmitter;
		private OSCReceiver FOSCReceiver;
		private bool FListening;
		private Thread FThread;
		
		private IPAddress FTargetIP;
		private int FTargetPort;
		private IPAddress FAutoIP;
		private int FAutoPort;
		private List<string> FPrefixes = new List<string>();
		// Track whether Dispose has been called.
		private bool FDisposed = false;
		
		private System.Timers.Timer FTimer = new System.Timers.Timer(2000);
		private bool FAllowUpdates = true;
		private XmlDocument FXML = new XmlDocument();
		private List<IPin2> FBangs = new List<IPin2>();
		private Dictionary<string, RemoteValue> FTargets = new Dictionary<string, RemoteValue>();
		private Dictionary<string, string> FSaveTargets = new Dictionary<string, string>();
		private Dictionary<string, PatchMessage> FPatchMessages = new Dictionary<string, PatchMessage>();
		private Dictionary<string, IPin2> FExposedPins = new Dictionary<string, IPin2>();
		private List<OSCMessage> FMessageQueue = new List<OSCMessage>();
		#endregion fields & pins
		
		#region constructor/destructor
		[ImportingConstructor]
		public KontrolleurNode(IHDEHost host)
		{
			FHDEHost = host;
			FHDEHost.ExposedNodeService.NodeAdded += NodeAddedCB;
			FHDEHost.ExposedNodeService.NodeRemoved += NodeRemovedCB;
			
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
					
					StopListeningOSC();
					
					FPatchFilter.Changed -= PrefixChangedCB;
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
			FPatchFilter.Changed += PrefixChangedCB;
		}
		
		private void PrefixChangedCB(IDiffSpread spread)
		{
			FPrefixes.Clear();
			foreach (string prefix in spread)
				if (!string.IsNullOrEmpty(prefix))
					FPrefixes.Add(prefix);
			
			ReExposeIOBoxes();
		}
		
		private void FTimer_Elapsed(object Sender, ElapsedEventArgs e)
		{
			FAllowUpdates = true;
		}
		
		private void NodeAddedCB(INode2 node)
		{
			ExposeIOBox(node);
		}
		
		private void NodeRemovedCB(INode2 node)
		{
			UnExposeIOBox(node);
		}
		#endregion events
		
		#region network
		private void InitNetwork()
		{
			if (!FAutoIP.Equals(FTargetIP) || FAutoPort != FTargetPort)
			{
				FTargetIP = FAutoIP;
				FTargetPort = FAutoPort;
				
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
		
		private void StartListeningOSC()
		{
			FOSCReceiver = new OSCReceiver(FUDPPort[0]);
			FListening = true;
			FThread = new Thread(new ThreadStart(ListenToOSC));
			FThread.Start();
		}
		
		private void StopListeningOSC()
		{
			if (FThread != null && FThread.IsAlive)
			{
				FListening = false;
				//FOSCReceiver is blocking the thread
				//so waiting would freeze
				//shouldn't be necessary here anyway...
				//FThread.Join();
			}	

			if (FOSCReceiver != null)
				FOSCReceiver.Close();
			FOSCReceiver = null;
		}
		
		private void ListenToOSC()
		{
			while(FListening)
			{
				try
				{
					OSCPacket packet = FOSCReceiver.Receive();
					if (packet != null)
					{ 
						lock(FMessageQueue)
							if (packet.IsBundle())
							{
								ArrayList messages = packet.Values;
								for (int i=0; i<messages.Count; i++)
									FMessageQueue.Add((OSCMessage)messages[i]);
							}
							else
								FMessageQueue.Add((OSCMessage)packet);
					}
				}
				catch (Exception e)
				{
					FLogger.Log(LogType.Debug, "UDP: " + e.Message);
				}
			}
		}
		
		#endregion network
		
		#region exposing IOBoxes
		private void ExposeIOBox(INode2 node)
		{
			var target = new RemoteValue(node, FPrefixes);

			if (!FTargets.ContainsKey(target.RuntimeNodePath))
			{
				if (FPrefixes.Count == 0
				    || (FPrefixes.Count > 0 && FPrefixes.Contains(target.Node.Parent.NodeInfo.Filename)))
				FTargets.Add(target.RuntimeNodePath, target);
			}
			else if (FTargets.ContainsKey(target.RuntimeNodePath))
			{
				if (FPrefixes.Count == 0
				    || (FPrefixes.Count > 0 && FPrefixes.Contains(target.Node.Parent.NodeInfo.Filename)))
				FTargets[target.RuntimeNodePath].State = RemoteValueState.Add;
				
				target.Kill();
			}
			else
				target.Kill();					
		}
		
		private void UnExposeIOBox(INode2 node)
		{
			var address = node.GetNodePath(false);
			
			if (FTargets.ContainsKey(address))
				FTargets[address].State = RemoteValueState.Remove;
		}

		private void ReExposeIOBoxes()
		{
			foreach (var target in FTargets.ToArray())
				target.Value.State = RemoteValueState.Remove;
				
			foreach (var node in FHDEHost.ExposedNodeService.Nodes)
				ExposeIOBox(node);
		}
		#endregion exposing IOBoxes
		
		#region OSC
		private void ProcessOSCMessage(OSCMessage message)
		{
			if (message.Address == "/k/init")
			{
				FSaveTargets.Clear();
				
				FAutoIP = IPAddress.Parse((string)message.Values[0]);
				FAutoPort = (int) message.Values[1];
				FResolution = new Vector2D((int) message.Values[2], (int) message.Values[3]);

				InitNetwork();
				
				//get initial list of exposed ioboxes
				ReExposeIOBoxes();
				
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
                        pm.AddSaveMe();
						FPatchMessages.Add(patchClass[0], pm);
					}
					
					var node = pm.AddNode(int.Parse(patchClass[1]));
					var pin = node.AddPin("Y Input Value");
					pin.Spread = target.Value;
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
				FTimer.Start();
				
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
			if (FUDPPort.IsChanged)
			{
				StopListeningOSC();
				StartListeningOSC();
			}
			
			FTouchID.SliceCount = 0;
			FTouchXY.SliceCount = 0;
			FTouchPressure.SliceCount = 0;
			
			//special treatment for bangs
			//which cause 2 patch messages in consecutive frames to be sent
			foreach (var pin in FBangs)
			{
				//set all the bangs values to 0
				pin.Spread = "0";
			}
			FBangs.Clear();
			
			//process messagequeue 
			//in order to handle all messages from main thread
			//since all COM-access is single threaded
			lock(FMessageQueue)
			{
				foreach (var message in FMessageQueue)
					ProcessOSCMessage(message);
				FMessageQueue.Clear();
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
