#region Copyright notice
/*
A Firmata Plugin for VVVV - v 1.0
----------------------------------
Encoding control and configuration messages for Firmata enabled MCUs. This
Plugin encodes to a ANSI string and a byte array, so you can send via any
interface, most likely RS-232 a.k.a. Comport to a - most likely - Arduino.

For more information on Firmata see: http://firmata.org
Get the source from: https://github.com/jens-a-e/VVVVirmata
Any issues & feature requests should be posted to: https://github.com/jens-a-e/VVVVirmata/issues

Copyleft 2011
Jens Alexander Ewald, http://ififelse.net
Chris Engler, http://wirmachenbunt.de
Both: http://www.muthesius-kunsthochschule.de

Inspired by the Sharpduino project by Tasos Valsamidis (LSB and MSB operations)
See http://code.google.com/p/sharpduino if interested.


Copyright notice
----------------

This is free and unencumbered software released into the public domain.

Anyone is free to copy, modify, publish, use, compile, sell, or
distribute this software, either in source code form or as a compiled
binary, for any purpose, commercial or non-commercial, and by any
means.

In jurisdictions that recognize copyright laws, the author or authors
of this software dedicate any and all copyright interest in the
software to the public domain. We make this dedication for the benefit
of the public at large and to the detriment of our heirs and
successors. We intend this dedication to be an overt act of
relinquishment in perpetuity of all present and future rights to this
software under copyright law.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.

For more information, please refer to <http://unlicense.org/>
 */
#endregion

#region usings
using System;
using System.ComponentModel.Composition;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using Firmata;
using System.Collections.Generic;

#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "FirmataDecode",
				Version = "2.x String Legacy",
	            Category = "Devices",
				Author = "jens a. ewald",
	            Help = "Decodes the firmata protocol version 2.x",
	            Tags = "Arduino")]
	#endregion PluginInfo
	public class FirmataDecodeString : IPluginEvaluate
	{
		#region fields & pins
		[Input("Firmata Message")]
		IDiffSpread<String> FAnsiMessage;
		
		[Input("Analog Input Count",DefaultValue = 6, Visibility = PinVisibility.OnlyInspector, IsSingle = true)]
		ISpread<int> FAnalogInputCount;
		
		[Input("Digital Input Count",DefaultValue = 14, Visibility = PinVisibility.OnlyInspector, IsSingle = true)]
		ISpread<int> FDigitalInputCount;
		
		[Output("Analog In")]
		ISpread<int> FAnalogIns;
		
		[Output("Digital In")]
		ISpread<int> FDigitalIns;
		
		[Output("Firmware Major Version",Visibility = PinVisibility.Hidden)]
		ISpread<int> FFirmwareMajorVersion;
		
		[Output("Firmware Minor Version",Visibility = PinVisibility.Hidden)]
		ISpread<int> FFirmwareMinorVersion;
		
		[Output("Firmware Name",Visibility = PinVisibility.OnlyInspector)]
		ISpread<string> FFirmwareName;
		
		[Output("Firmware Version")]
		ISpread<string> FFirmwareVersion;
		
		[Output("I2C Data",Visibility = PinVisibility.OnlyInspector)]
		ISpread<byte> FI2CData;
		
		#endregion fields & pins
		
		private Queue<byte> Buffer = new Queue<byte>();
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FAnalogIns.SliceCount  = FAnalogInputCount[0];
			FDigitalIns.SliceCount = FDigitalInputCount[0];
			
			/// Using a Queue and iterate over it (nice to handle and inexpensive)
			foreach (byte b in Encoding.GetEncoding(1252).GetBytes(FAnsiMessage[0])) {
				// we should check for max buffer size and not constantly enque things...
				Buffer.Enqueue(b);
			}
			
			// A cache for sysex data
			Queue<byte> cache = new Queue<byte>();
			// A flag if parsing sysex data
			bool bIsSysex = false;
			
			// PARSE:
			while (Buffer.Count > 0) {
				byte current = Buffer.Dequeue();
				switch(current){
					case Command.SYSEX_START:
					case Command.SYSEX_END:
						if (current == Command.SYSEX_START)
							bIsSysex = true;
						else if(current == Command.SYSEX_END) {
							// Process the Sysexdata:
							ProcessSysex(cache);
							bIsSysex = false;
						}
						cache.Clear();
						break;
					default:
						if (bIsSysex) { // Collect bytes for the SysSex message cache
							cache.Enqueue(current);
						} else {
							// Treat Ananlog & Digital Messages:
							bool hasDigitalMessage = FirmataUtils.VerifiyCommand(current,Command.DIGITALMESSAGE);
							bool hasAnalogMessage  = FirmataUtils.VerifiyCommand(current,Command.ANALOGMESSAGE);
							// We have a data for commands
							if(Buffer.Count >= 2 && (hasDigitalMessage || hasAnalogMessage))
							{
								// Reihenfolge matters!
								byte[] data = {current, Buffer.Dequeue(),Buffer.Dequeue()};
								// Check for Analog Command
								if (hasAnalogMessage) {
									int pinNum,value;
									FirmataUtils.DecodeAnalogMessage(data,out pinNum,out value);
									if (pinNum < FAnalogInputCount[0]) FAnalogIns[pinNum] = value; // assign the found value to the spread
								}
								else if (hasDigitalMessage) {
									int port; int[] vals;
									// Decode the values from the bytes:
									FirmataUtils.DecodePortMessage(data,out port, out vals);
									// Fill the spread with parsed pinstates
									for (int i=0; i<Constants.BitsPerPort; i++) {
										int pinNum = i+Constants.BitsPerPort*port;
										if ( pinNum < FDigitalIns.SliceCount) FDigitalIns[pinNum] = vals[i];
									}
								}
							}
						}
						break;
				}
			}
		}
		
		void ProcessSysex(Queue<byte> data) {
			if(data.Count == 0 ) return;
			
			switch(data.Dequeue()){
					/// Handle Firmwareversion replies:
				case Command.REPORT_FIRMWARE:
					if (data.Count < 2) break;
					int major = data.Dequeue();
					int minor = data.Dequeue();
					// Read the name, of the Version
					StringBuilder name = new StringBuilder();
					while(data.Count >= 2){
						byte lsb = (byte)(data.Dequeue() & 0x7F);
						byte msb = (byte)((data.Dequeue() & 0x7F) << 7);
						byte[] both = {(byte)(lsb|msb)};
						if(lsb!=0 || msb!=0)
							name.Append(Encoding.ASCII.GetString(both));
					}
					string the_name = major+"."+minor;
					if(name.Length>0) the_name += " "+name.ToString();
					
					FFirmwareMajorVersion[0] = major;
					FFirmwareMinorVersion[0] = minor;
					FFirmwareName[0] = name.ToString();
					FFirmwareVersion[0] = the_name;
					break;
					
					/// Handle I2C replies
				case Command.I2C_REPLY:
					FI2CData.AssignFrom(data);
					break;
					
				// Todo: Implement Capability reports!
			}
		}
		
	}
	
	
	
	#region PluginInfo
	[PluginInfo(Name = "I2CDecode",
	            Category = "Devices String",
            	Version  = "2.x",
				Author = "jens a. ewald",
	            Help = "Decodes I2C data from Firmata messages",
	            Tags = "Firmata,Arduino")]
	#endregion PluginInfo
	public class I2CDecodeString : IPluginEvaluate
	{
		[Input("I2CData")]
		IDiffSpread<byte> Data;

		[Input("Address")]
		IDiffSpread<int> Address;
		
		[Output("Register",DefaultValue = 0)]
		ISpread<int> ParsedRegister;

		[Output("Data",DefaultValue = 0)]
		ISpread<int> ParsedData;
		
		public void Evaluate(int maxSpread){
			if (Data.IsChanged)
			{
				if(Data.SliceCount<2) return;
				
				int _address = FirmataUtils.GetValueFromBytes(Data[1],Data[0]);
				if(_address!=Address[0] && Data.SliceCount<4) return;
				
				int[] empty = {};
				ParsedRegister.AssignFrom(empty);
				ParsedData.AssignFrom(empty);
				
				for(int i=2; i<Data.SliceCount-4; i+=4){
					ParsedRegister.Add(FirmataUtils.GetValueFromBytes(Data[i+1],Data[i]));
					ParsedData.Add(FirmataUtils.GetValueFromBytes(Data[i+3],Data[i+2]));
				}
			}
		}
	}

}