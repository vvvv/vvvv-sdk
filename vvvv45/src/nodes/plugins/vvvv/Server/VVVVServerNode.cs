#region usings
using System;
using System.Net;
using System.Globalization;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;
using VVVV.Utils.OSC;
using VVVV.Core.Logging;
using VVVV.Core;
#endregion usings

namespace VVVV.Nodes
{
	enum AcceptMode {None, OnlyExposed, OnlyCached, ExposedAndCached, Any};
	
	#region PluginInfo
	[PluginInfo(Name = "Server",
	            Category = "VVVV",
	            Help = "Accepts values for pins via OSC",
	            Tags = "remote",
	            AutoEvaluate = true)]
	#endregion PluginInfo
	public class VVVVServerNode: IPluginEvaluate, IDisposable
	{
		#region fields & pins
#pragma warning disable 0649
        [Input("Listening UDP Port", IsSingle = true, DefaultValue = 44444)]
        IDiffSpread<int> FUDPPort;

        [Input("Accept", IsSingle = true, DefaultEnumEntry = "OnlyExposed")]
        IDiffSpread<AcceptMode> FAcceptMode;

        [Input("Clear Cache", IsSingle = true, IsBang = true)]
        IDiffSpread<bool> FClearCache;

        [Input("Feedback Accepted", IsSingle = true, DefaultValue = 0)]
        ISpread<bool> FFeedback;

        [Input("Feedback Target IP", IsSingle = true, DefaultString = "127.0.0.1")]
        IDiffSpread<string> FTargetIP;

        [Input("Feedback Target UDP Port", IsSingle = true, DefaultValue = 55555)]
        IDiffSpread<int> FTargetPort;

        [Output("Exposed Pins")]
        ISpread<string> FExposedPinsOut;

        [Output("Cached Pins")]
        ISpread<string> FCachedPinsOut;

        [Import()]
        ILogger FLogger;

        [Import()] 
#pragma warning restore
		IHDEHost FHDEHost;
		
		private OSCReceiver FOSCReceiver;
		private bool FListening;
		private Thread FThread;
		
		private OSCTransmitter FOSCTransmitter;
		private IPAddress FIP;
		
		private Dictionary<string, IPin2> FCachedPins = new Dictionary<string, IPin2>();
		private Dictionary<string, IPin2> FExposedPins = new Dictionary<string, IPin2>();
		private List<OSCMessage> FMessageQueue = new List<OSCMessage>();
		
		private bool FDisposed;
		#endregion fields & pins
		
		#region constructor/destructor
		[ImportingConstructor]
		public VVVVServerNode(IHDEHost host)
		{
			FHDEHost = host;
			FHDEHost.ExposedNodeService.NodeAdded += NodeAddedCB;
			FHDEHost.ExposedNodeService.NodeRemoved += NodeRemovedCB;
			
			//get initial list of exposed ioboxes
			foreach (var node in FHDEHost.ExposedNodeService.Nodes)
			{
				var pinName = PinNameFromNode(node);
				var pin = node.FindPin(pinName);
				pin.Changed += PinChanged;
				FExposedPins.Add(node.GetNodePath(false) + "/" + pinName, pin);
			}
		}
		
		~VVVVServerNode()
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
					FHDEHost.ExposedNodeService.NodeAdded -= NodeAddedCB;
					FHDEHost.ExposedNodeService.NodeRemoved -= NodeRemovedCB;
					
					StopListeningOSC();
				}
				// Release unmanaged resources. If disposing is false,
				// only the following code is executed.
				
			}
			FDisposed = true;
		}
		#endregion

		private void NodeAddedCB(INode2 node)
		{
			var pinName = PinNameFromNode(node);
			var pin = node.FindPin(pinName);
			
			FExposedPins.Add(node.GetNodePath(false) + "/" + pinName, pin);
			if (pin != null)
				pin.Changed += PinChanged;
		}
		
		private void NodeRemovedCB(INode2 node)
		{
			var pinName = PinNameFromNode(node);
			var pin = node.FindPin(pinName);
			
			FExposedPins.Remove(node.GetNodePath(false) + "/" + pinName);
			if (pin != null)
				pin.Changed -= PinChanged;
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
		
		private void PinChanged(object sender, EventArgs e)
		{
			if ((FOSCTransmitter != null) && FFeedback[0])
			{
				var pin = sender as IPin2;
				var pinPath = pin.ParentNode.GetNodePath(false) + "/" + pin.Name;
				
				var bundle = new OSCBundle();
				var message = new OSCMessage(pinPath);
				
				for (int i = 0; i < pin.SliceCount; i++)
					message.Append(pin[i]);
				
				bundle.Append(message);

				try
				{
					FOSCTransmitter.Send(bundle);
				}
				catch (Exception ex)
				{
					FLogger.Log(LogType.Warning, "PinServer: " + ex.Message);
				}
			}
		}
		
		#region Network Input
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
						if (packet.IsBundle())
						{
							ArrayList messages = packet.Values;
							lock(FMessageQueue)
								for (int i=0; i<messages.Count; i++)
									FMessageQueue.Add((OSCMessage)messages[i]);
						}
						else
							lock(FMessageQueue)
								FMessageQueue.Add((OSCMessage)packet);
					}
				}
				catch (Exception e)
				{
					FLogger.Log(LogType.Debug, "UDP: " + e.Message);
				}
			}
		}
		
		private void ProcessOSCMessage(OSCMessage message)
		{
			IPin2 pin = null;
			switch (FAcceptMode[0])
			{
				case AcceptMode.OnlyCached:
					{
						if (FCachedPins.ContainsKey(message.Address))
							pin = FCachedPins[message.Address];
						break;
					}
					
				case AcceptMode.OnlyExposed:
					{
						if (FExposedPins.ContainsKey(message.Address))
							pin = FExposedPins[message.Address];
						break;
					}
					
				case AcceptMode.ExposedAndCached:
					{
						if (FExposedPins.ContainsKey(message.Address))
							pin = FExposedPins[message.Address];
						else if (FCachedPins.ContainsKey(message.Address))
							pin = FCachedPins[message.Address];
						break;
					}
					
				case AcceptMode.Any:
					{
						if (FCachedPins.ContainsKey(message.Address))
							pin = FCachedPins[message.Address];
						else
						{
							//check if address is in the format of a nodepath
							//ie. /4/545/46/pinname
							var path = message.Address.Split('/');
							var pinName = path[path.Length - 1];
							
							int id = 0;
							//last entry in path has to be a string (ie. pinname) not a number
							var validAddress = !string.IsNullOrEmpty(pinName) && !int.TryParse(pinName, out id);
							
							if (validAddress)
							{
								var pathIDs = new int[path.Length - 1];
								for (int i = 1; i < pathIDs.Length; i++)
								{
									
									if (int.TryParse(path[i], out id))
										pathIDs[i] = id;
									else
									{
										validAddress = false;
										break;
									}
								}
							}
							
							if (validAddress)
							{
								//check if a pin is available under this address
								var nodePath = message.Address.Substring(0, message.Address.LastIndexOf('/'));
								var node = FHDEHost.GetNodeFromPath(nodePath);
								if (node != null)
								{
									//FLogger.Log(LogType.Warning, node.Name);
									pin = node.FindPin(pinName.Trim());
									
									if (pin != null)
									{
										FCachedPins.Add(message.Address, pin);
										pin.Changed += PinChanged;
									}
									else
										FLogger.Log(LogType.Warning, "No pin available under: \"" + message.Address + "\"!");
								}
								else
									FLogger.Log(LogType.Warning, "No node available under: \"" + nodePath + "\"!");
							}
							else
								FLogger.Log(LogType.Warning, "\"" + message.Address + "\" is not a valid pin address!");
						}
						break;
					}
			}

			//send values to pin
			if (pin != null)
			{
				var values = "";
				foreach(var v in message.Values)
				{
                    if (v is float)
                        values += ((float)v).ToString(System.Globalization.CultureInfo.InvariantCulture) + ",";
                    else if (v is double)
                        values += ((double)v).ToString(System.Globalization.CultureInfo.InvariantCulture) + ",";
                    else
                    {
                        //pipes are used in pin values only to surround slices that have a "space", "comma" or "pipe" in them
                        //therefore they only make sense in string and color values in the first place 
                        //but also it does no harm if a slice is surrounded by pipes anyway
                        //so lets check if first and last character is a pipe, then assume user knows those rules and don't do anything
                        //otherwise:
                        //- always escape a single pipe with an extra pipe
                        //- always surround the string with pipes
                        string s = v.ToString();
                        if (s.StartsWith("|") && s.EndsWith("|"))
                            values += s + ",";
                        else
                        {
                            s = s.Replace("|", "||"); //escape a single pipe with a double pipe
                            values += "|" + s + "|,"; //quote the string with pipes in order for it to be treated as a single slice
                        }
                    }
				}
				values = values.TrimEnd(',');
				
				pin.Spread = values;
			}
		}
		#endregion OSC
		
		#region Network Output
		private void InitNetwork()
		{
			if (FIP != null)
				try
			{
				if (FOSCTransmitter != null)
					FOSCTransmitter.Close();
				FOSCTransmitter = new OSCTransmitter(FIP.ToString(), FTargetPort[0]);
				FOSCTransmitter.Connect();
			}
			catch (Exception e)
			{
				FLogger.Log(LogType.Warning, "PinServer: failed to open port " + FTargetPort.ToString());
				FLogger.Log(LogType.Warning, "PinServer: " + e.Message);
			}
		}
		#endregion network
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FUDPPort.IsChanged)
			{
				StopListeningOSC();
				StartListeningOSC();
			}
			
			//re/init udp
			if (FTargetIP.IsChanged || FTargetPort.IsChanged)
			{
				try
				{
					FIP = IPAddress.Parse(FTargetIP[0]);
					InitNetwork();
				}
				catch
				{}
			}
			
			if (FClearCache[0])
			{
				foreach (var pin in FCachedPins)
					pin.Value.Changed -= PinChanged;
				FCachedPins.Clear();
			}
			
			//process messagequeue
			//in order to handle all messages from main thread
			//since all COM-access is single threaded
			lock(FMessageQueue)
			{
				foreach (var message in FMessageQueue)
					ProcessOSCMessage(message);
				FMessageQueue.Clear();
			}
			
			FExposedPinsOut.AssignFrom(FExposedPins.Keys);
			FCachedPinsOut.AssignFrom(FCachedPins.Keys);
		}
	}
}
