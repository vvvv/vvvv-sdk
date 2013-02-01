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

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

using System.IO;

using Firmata;
using System.Collections.Generic;

#endregion usings

namespace VVVV.Nodes
{
  #region PluginInfo
  [PluginInfo(Name = "FirmataDecode",
              Version = "2.x",
              Category = "Devices",
              Author = "jens a. ewald",
              Help = "Decodes the firmata protocol version 2.x",
              Tags = "Arduino")]
  #endregion PluginInfo
  public class FirmataDecode : IPluginEvaluate
  {
    #region fields & pins
    /// Inputs
    [Input("Firmata Message", IsSingle = true)]
    IInStream<Stream> FirmataIn;
    
    [Input("Analog Input Count",  DefaultValue = 6, MaxValue = Default.MaxAnalogPins, MinValue = 0,
                                  Visibility = PinVisibility.OnlyInspector, IsSingle = true)]
    IDiffSpread<int> FAnalogInputCount;
    
    [Input("Digital Input Count", DefaultValue = 20, MaxValue = Default.MaxDigitalPins, MinValue = 0,
                                  Visibility = PinVisibility.OnlyInspector, IsSingle = true)]
    IDiffSpread<int> FDigitalInputCount;
    
    /// Outputs
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
		IOutStream<Stream> FI2CData;

		[Output("Debug", Visibility = PinVisibility.OnlyInspector)]
		ISpread<string> FDebug;


		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
      /// Configure the output count
      if (FAnalogIns.IsChanged) FAnalogIns.SliceCount  = FAnalogInputCount[0];
      if (FDigitalIns.IsChanged) FDigitalIns.SliceCount = FDigitalInputCount[0];

      /// If there is nothing new to read, there is nothing to parse
      if (!FirmataIn.IsChanged) return;

      /// Read in the stream
      try {
        using (IStreamReader<Stream> InputReader = FirmataIn.GetReader())
        {
          while (!InputReader.Eos) {
            HandleStream(InputReader.Read());
          }
        }
      } catch (Exception e) {
        return;
      }
    }

    #region Plugin Functions
    private Queue<byte> Buffer = new Queue<byte>();

    private void HandleStream(Stream InStream) {
      // Check, if the incoming Stream is usable
      if(InStream == null || InStream.Length == 0 || !InStream.CanRead) return;

      // Read the incoming bytes to the internal stream buffer
      while (InStream.Position < InStream.Length) {
        Buffer.Enqueue((byte)InStream.ReadByte());
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
          FDebug.SliceCount = 1;
          FDebug[0] = data.Count.ToString();
          try {

            MemoryStream i2cStream = new MemoryStream(data.ToArray());

            FI2CData.Length = 1;
            using (var i2cWriter = FI2CData.GetWriter()) {
              i2cWriter.Write(i2cStream);
            }

          } catch (Exception e) {
            FI2CData.Length = 0;
            FDebug.SliceCount = 1;
            FDebug[0] = "Error: " + e.ToString();
          }
          data.Clear();

          break;
					// Todo: Implement Capability reports!
			}
		}
    #endregion Plugin Functions
		
	}
	
	
	
	#region PluginInfo
	[PluginInfo(Name = "I2CDecode",
	            Category = "Devices",
	            Version  = "2.x",
	            Author = "jens a. ewald",
	            Help = "Decodes I2C data from Firmata messages",
	            Tags = "Firmata,Arduino")]
	#endregion PluginInfo
	public class I2CDecode : IPluginEvaluate
	{
    ///
    /// Input
    ///
		[Input("I2CData")]
		IDiffSpread<Stream> Data;

		[Input("Address", IsSingle=true, DefaultValue=0, MinValue = 0, MaxValue = 127)]
		IDiffSpread<int> Address;
		
    ///
    /// Output
    ///
		[Output("Register", DefaultValue=0)]
		ISpread<int> ParsedRegister;

		[Output("Data", DefaultValue=0)]
		ISpread<Stream> ParsedData;

    [Output("Debug", Visibility = PinVisibility.Hidden)]
		ISpread<string> FDebug;

    void clearOutputs(){
      ParsedRegister.SliceCount = 0;
      ParsedData.SliceCount = 0;
    }

    ///
    /// TODO: Make it spreadable with Address spread as size
    ///
    public void Evaluate(int maxSpread){
      if (Data.IsChanged)
      {
        FDebug.SliceCount = 0;

        if(Data.SliceCount <= 0 || Address.SliceCount < 1) {
          clearOutputs();
          return;
        }

        int NumBytes = (int) Data[0].Length-4; // substract Address & Register
        if(NumBytes <= 0) {
          FDebug.Add(NumBytes > 0 ? "No data bytes to process" : "Not enough bytes at all");
          clearOutputs();
          return;
        }

        byte LSB = (byte) Data[0].ReadByte();
        byte MSB = (byte) Data[0].ReadByte();
        int _Address = FirmataUtils.GetValueFromBytes(MSB,LSB);

        if(_Address!=Address[0]) {
          clearOutputs();
          FDebug.Add("Address does not match");
          return;
        }

        LSB = (byte) Data[0].ReadByte();
        MSB = (byte) Data[0].ReadByte();
        ParsedRegister.SliceCount = 1;
        int register = FirmataUtils.GetValueFromBytes(MSB,LSB);
        ParsedRegister[0] = register == 255 ? -1 : register; // Handle REGISTER_NOT_SPECIFIED

        try {
          ParsedData.SliceCount = 1;
          Stream data = new MemoryStream(NumBytes/2); // we deal with 7-bit encoding!
          for (int i=0; i<NumBytes; i+=2) {
            LSB = (byte) Data[0].ReadByte();
            MSB = (byte) Data[0].ReadByte();
            data.WriteByte((byte)FirmataUtils.GetValueFromBytes(MSB,LSB));
          }
          ParsedData[0] = data;
        }
        catch (Exception e) {
          FDebug.Add("Could not read data: "+e.ToString());
        }
      }
    }
  }
}
