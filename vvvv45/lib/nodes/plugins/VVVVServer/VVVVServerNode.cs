#region usings
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;
using VVVV.Utils.OSC;
using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	enum AcceptMode {None, OnlyExposed, OnlyCached, Any};
	
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
		[Input("UDP Port", IsSingle=true, DefaultValue = 44444)]
		IDiffSpread<int> FUDPPort;
		
		[Input("Clear Cache", IsSingle=true, IsBang=true)]
		IDiffSpread<bool> FClearCache;
		
		[Input("Accept", IsSingle=true, DefaultEnumEntry="Any")]
		IDiffSpread<AcceptMode> FAcceptMode;

		[Output("Exposed Pins")]
		ISpread<string> FExposedPinsOut;
		
		[Output("Cached Pins")]
		ISpread<string> FCachedPinsOut;

		[Import()]
		ILogger FLogger;
		
		[Import()]
		IHDEHost FHDEHost;
		
		private OSCReceiver FOSCReceiver;
		private bool FListening;
		private Thread FThread;
		private bool FDisposed;
		private INode2 FRoot;
		
		private Dictionary<string, IPin2> FCachedPins = new Dictionary<string, IPin2>();
		private List<OSCMessage> FMessageQueue = new List<OSCMessage>();
		#endregion fields & pins
		
		#region constructor/destructor
		[ImportingConstructor]
		public VVVVServerNode(IHDEHost host)
		{
			FRoot = host.RootNode;
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
					StopListeningOSC();
				}
				// Release unmanaged resources. If disposing is false,
				// only the following code is executed.
				
			}
			FDisposed = true;
		}
		#endregion

		#region OSC
		private void StartListeningOSC()
		{
			FOSCReceiver = new OSCReceiver(FUDPPort[0]);
			FListening = true;
			FThread = new Thread(new ThreadStart(ListenToOSC));
			FThread.Start();
		}
		
		private void StopListeningOSC()
		{
			FListening = false;
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
					if (packet!=null)
					{
						if (packet.IsBundle())
						{
							ArrayList messages = packet.Values;
							for (int i=0; i<messages.Count; i++)
								FMessageQueue.Add((OSCMessage)messages[i]);
						}
						else
							FMessageQueue.Add((OSCMessage)packet);
					}
					else
						FLogger.Log(LogType.Debug, "UDP: null packet received!");
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
									FCachedPins.Add(message.Address, pin);
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
					values += v.ToString().Replace(',', '.') + ",";
				values = values.TrimEnd(',');
				
				pin.Spread = values;
			}
		}
		#endregion OSC
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FUDPPort.IsChanged)
			{
				StopListeningOSC();
				StartListeningOSC();
			}
			
			if (FClearCache[0])
				FCachedPins.Clear();
			
			//process messagequeue 
			//in order to handle all messages from main thread
			//since all COM-access is single threaded
			foreach (var message in FMessageQueue)
				ProcessOSCMessage(message);
			FMessageQueue.Clear();
			
			FCachedPinsOut.AssignFrom(FCachedPins.Keys);
		}
	}
}
