#region usings
using System;
using System.ComponentModel.Composition;

using System.Net;
using System.Net.Sockets;

using System.Runtime.InteropServices;
using System.IO.Ports;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using System.Collections.Generic;
using System.Collections;
using System.Timers;

using Phidgets;
using Phidgets.Events;

using VVVV.Core.Logging;

#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "RFID", 
	            Category = "Devices",
                Version = "Phidget", 
	            Help = "Manages multiple Phidget RFID", 
	            Tags = "",
	            Author = "velcrome"
	           )]
	#endregion PluginInfo
	public class DevicesRFIDNode : IPluginEvaluate, IDisposable
	{
		#region fields & pins
		[Input("EnablePhidget", DefaultValue = 1)]
		public ISpread<bool> FEnable;

		[Input("EnableDebugLED", DefaultValue = 0)]
		public ISpread<bool> FEnableDebugLED;
		
		[Input("UseAntenna", DefaultValue = 1)]
		public ISpread<bool> FAntenna;

		
		[Output("SerialNumber")]
		public ISpread<string> FSerial;
		
		[Output("Tag")]
		public ISpread<string> FTag;
		
		private bool Enabled = false;

		// Phidget.Manager has its own Device List, however it seemed very slow to use it.
		// Also there were issues of concurrency going on. So I opted for my own management
		private Manager phidgManager;

		private Hashtable tagTable;
		private List<RFID> phidgetList;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins
		
		public DevicesRFIDNode()
		{
			Enabled = false;
			tagTable = new Hashtable();
			phidgetList = new List<RFID>();
			
			try {
				phidgManager = new Manager();
			} catch (Exception ex) {
				FLogger.Log(LogType.Error, "RFID Node Creation: "+ex.Message);
			}
		}
		
		public void Dispose()
		{
			Disable();
		}
		
		#region Phidget Manager Events
		void phidgManager_Attach(object sender, AttachEventArgs e)
		{
			//Console.WriteLine("Attached Device: {0}", e.Device.Class);
			
			//Check if the phidget attached is an RFID reader...
			if(e.Device.Class.ToString() == "RFID")
			{
				
				//Create the RFID reader object for this reader...
				RFID currentReader = new RFID();
				
				//Attach events to the new reader that has been attached...
				currentReader.Attach   += new AttachEventHandler(rfid_Attach);
				currentReader.Detach   += new DetachEventHandler(rfid_Detach);
				currentReader.Error    += new ErrorEventHandler(rfid_Error);
				currentReader.Tag      += new TagEventHandler(rfid_Tag);
				currentReader.TagLost  += new TagEventHandler(rfid_TagLost);
				
				tagTable[e.Device.SerialNumber.ToString()] = "init";
				phidgetList.Add(currentReader);
				
				try {
					currentReader.open(e.Device.SerialNumber);
				} catch (Exception ex) {
//					FLogger.Log(LogType.Error, "Attach RFID: "+ex.Message);
				}
			}
			return;
		}
		
		void phidgManager_Detach(object sender, DetachEventArgs e)
		{
			if (e.Device.Class.ToString() == "RFID")
			{
				
				//Convert the sender object to a RFID object
				RFID currentReader = (RFID)e.Device;
				
				//De-attach events to the new reader that has been attached...
				currentReader.Attach   -= new AttachEventHandler(rfid_Attach);
				currentReader.Detach   -= new DetachEventHandler(rfid_Detach);
				currentReader.Error    -= new ErrorEventHandler(rfid_Error);
				currentReader.Tag      -= new TagEventHandler(rfid_Tag);
				currentReader.TagLost  -= new TagEventHandler(rfid_TagLost);

				
				try {
					tagTable.Remove(currentReader.SerialNumber.ToString());
					
					/*					int index = phidgetList.IndexOf(currentReader);
					while (index != -1) {
						phidgetList.RemoveAt(index);
						index = phidgetList.IndexOf(currentReader);
					}
					 */
					//Close up the RFID object
					// this is necessary because removal of one reader might lead to logical removal of secondary readers.
					currentReader.close();

				} catch (Exception ex) {
//					FLogger.Log(LogType.Error, "Detach in Manager: "+ex.Message);
				}
			}
			return;
		}
		
		void phidgManager_Error(object sender, ErrorEventArgs e)
		{
//			FLogger.Log(LogType.Error, "Phidgetmanager: "+e.Description);
		}
		
		
		#endregion Phidget Manager Events
		
		#region Phidget RFID
		//attach event handler...display the serial number of the attached RFID phidget
		void rfid_Attach(object sender, AttachEventArgs e)
		{
			RFID rfid = (RFID) e.Device;
			rfid.Antenna = true;

			if (FEnableDebugLED[0]) {
				rfid.LED = true;
			} else rfid.LED = false;
		}
		
		//detach event handler...display the serial number of the detached RFID phidget
		void rfid_Detach(object sender, DetachEventArgs e)
		{
		}
		
		//Error event handler...display the error description string
		void rfid_Error(object sender, ErrorEventArgs e)
		{
//			FLogger.Log(LogType.Error, "Phidget: "+e.Description);
		}
		
		//Print the tag code of the scanned tag
		void rfid_Tag(object sender, TagEventArgs e)
		{
			RFID r = (RFID)sender;
			
//			if (FEnableDebugLED[0]) r.LED = false;
			
			tagTable[r.SerialNumber.ToString()] = e.Tag;
		}
		
		//print the tag code for the tag that was just lost
		void rfid_TagLost(object sender, TagEventArgs e)
		{
			RFID r = (RFID)sender;

//			if (FEnableDebugLED[0]) r.LED = true;

			tagTable[r.SerialNumber.ToString()] = "none";
		}
		
		#endregion Phidget RFID
		
		#region PhidgetManager
		
		private void Enable() {
			Enabled = true;
			try {
				phidgManager.open();
				
				phidgManager.Attach += new AttachEventHandler(phidgManager_Attach);
				phidgManager.Detach += new DetachEventHandler(phidgManager_Detach);
				phidgManager.Error  += new ErrorEventHandler(phidgManager_Error);
				
				FLogger.Log(LogType.Debug, "Enabled Phidget Manager...");
			} catch(Exception ex) {
				FLogger.Log(LogType.Debug, "Enable: " + ex.Message);
			}
			
		}
		
		private void Disable() {
			Enabled = false;
			try {
				foreach (RFID rfid in phidgetList) {
					try {
						rfid.LED = false;
						rfid.close();
					} catch (Exception ex) {
						FLogger.Log(LogType.Debug, "Disable: "+ex.Message);
					}
				}

				phidgManager.Attach -= new AttachEventHandler(phidgManager_Attach);
				phidgManager.Detach -= new DetachEventHandler(phidgManager_Detach);
				phidgManager.Error  -= new ErrorEventHandler(phidgManager_Error);
				
				phidgManager.close();
				
				FLogger.Log(LogType.Debug, "Disabled Phidget Manager");
			} catch(Exception ex) {
				FLogger.Log(LogType.Debug, ex.Message);
			}
			
			tagTable.Clear();
			phidgetList.Clear();
		}
		
		private void switchAntenna(RFID currentReader, bool useAntenna) {
			try {
				if (FEnableDebugLED[0])
					currentReader.LED = useAntenna ^ currentReader.TagPresent;

				else currentReader.LED = false;

				currentReader.Antenna = useAntenna;

			} catch (Exception ex) {
				FLogger.Log(LogType.Error, "Antenna: "+ex.Message);
			}
		}
		
		#endregion PhidgetManager
		
		
		#region Main Loop
		public void Evaluate(int SpreadMax)
		{
			if (!Enabled && FEnable[0]) Enable();
			if (Enabled  && !FEnable[0]) Disable();
			
			if (!Enabled) {
				FTag.SliceCount = FSerial.SliceCount = 0;
				return;
			}

			FTag.SliceCount = FSerial.SliceCount = tagTable.Count;
			if (tagTable.Count <= 0) return;

			string msg = "";
			int counter = 0;
			try {
				for (int i=0;i<phidgetList.Count;i++) {
					try {
						
						string serial = phidgetList[i].SerialNumber.ToString();
						string tag = tagTable[serial].ToString();

						FTag[i] = tag;
						FSerial[i] = serial;
						
						msg += serial+" -> "+tag+" | ";

						switchAntenna(phidgetList[i], FAntenna[counter]);
						counter++;
					} catch (Exception ex) {
//						prevents evaluation of disconnected rfid readers, that might still stored in phidgetList
					}
				}
//				FLogger.Log(LogType.Debug, msg);
			}
			catch(Exception ex) {
				
				FLogger.Log(LogType.Error, "Evaluate: "+ex.Message);
			}
			
		}
		
		
		#endregion Main Loop

	}

	#region Auxilliary Debugging Stuff
	class Program {
		static void Main(string[] args) {
			DevicesRFIDNode node = new DevicesRFIDNode();
			
			node.FAntenna = new Spread<bool>(1);
			node.FAntenna[0] = true;
			
			node.FEnable = new Spread<bool>(1);
			node.FEnable[0] = true;

			node.FSerial = new Spread<string>(3);
			node.FTag = new Spread<string>(3);

			node.FLogger = new CLogger();
			while (true) {
				node.Evaluate(1);
			}
			
			return;
		}
	}


	class CLogger : ILogger {
		public void Log(LogType type, string msg) {
			Console.WriteLine("{0}: {1}", type.ToString(), msg);
		}
	}
	#endregion Auxilliary Debugging Stuff
}

