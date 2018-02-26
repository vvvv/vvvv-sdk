#region Copyright notice
/*
A Firmata Plugin for VVVV - v 1.3
----------------------------------
Encoding control and configuration messages for Firmata enabled MCUs. This
Plugin encodes to a ANSI string and a byte array, so you can send via any
interface, most likely RS-232 a.k.a. Comport to a - most likely - Arduino.

For more information on Firmata see: http://firmata.org
Get the source from: https://github.com/jens-a-e/VVVVirmata
Any issues & feature requests should be posted to: https://github.com/jens-a-e/VVVVirmata/issues

Copyleft 2011-2013
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
using System.Collections.Generic;
using System.IO;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

using Firmata;

#endregion usings

namespace VVVV.Nodes
{
  #region PluginInfo
  [PluginInfo(Name = "FirmataDecode",
              Version = "2.x Legacy2",
              Category = "Devices",
              Author = "jens a. ewald",
              Help = "Decodes the firmata protocol version 2.x",
              Tags = "arduino")]
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

    [Output("Capabilities", Visibility = PinVisibility.Hidden)]
    ISpread<string> FCapabilities;

    [Output("String",Visibility = PinVisibility.Hidden)]
    ISpread<string> StringOut;

    [Output("I2C Data", Visibility = PinVisibility.OnlyInspector)]
    IOutStream<Stream> FI2CData;

    #endregion fields & pins


    //called when data for any output pin is requested
    public void Evaluate(int SpreadMax)
    {
      /// Configure the output count
      if (FAnalogIns.IsChanged) FAnalogIns.SliceCount  = FAnalogInputCount[0];
      if (FDigitalIns.IsChanged) FDigitalIns.SliceCount = FDigitalInputCount[0];

      StringOut.SliceCount = 0;

      /// If there is nothing new to read, there is nothing to parse
      if (!FirmataIn.IsChanged) return;

      /// Read in the stream
      try {
        using (IStreamReader<Stream> InputReader = FirmataIn.GetReader())
        {
          while (!InputReader.Eos) {
            Stream InStream = InputReader.Read();
            if(InStream == null || InStream.Length == 0 || !InStream.CanRead) continue;

            // Read the incoming bytes to the internal stream buffer
            for (int i=InStream.ReadByte(); i != -1; i = InStream.ReadByte())
              Decode((byte) i);
            
          }
        }
      } catch (Exception e) {
        return;
      }
    }

    #region Plugin Functions

    private Queue<byte> buffer = new Queue<byte>();
    private int remaining = 0;
    private byte lastCommand = Command.RESERVED_COMMAND;

    private void Decode(byte data) {

      if ((data & 0x80) > 0) {
        byte cmd = FirmataUtils.GetCommandFromByte( data );

        lastCommand = cmd;
        switch (cmd) {
          case Command.RESET:
            buffer.Clear();
            break;
          case Command.DIGITALMESSAGE:
          case Command.ANALOGMESSAGE:
          case Command.REPORT_VERSION:
          case Command.SETPINMODE:
            remaining = 2;
            buffer.Clear();
            buffer.Enqueue(data);
            break;
          case Command.TOGGLEDIGITALREPORT:
          case Command.TOGGLEANALOGREPORT:
            remaining = 1;
            buffer.Clear();
            buffer.Enqueue(data);
            break;
          case Command.SYSEX_START:
            buffer.Clear();
            break;
          case Command.SYSEX_END:
            // Fire Sysex event
            ProcessSysex(buffer);
            break;
          default:
            // unknown command
            break;
        }
      }
      else {
        buffer.Enqueue(data);
        if (--remaining==0) {
          // process the message
          switch (lastCommand) {
            case Command.ANALOGMESSAGE:
              int pin,value;
              FirmataUtils.DecodeAnalogMessage(buffer.ToArray(),out pin,out value);
              if (pin < FAnalogInputCount[0]) FAnalogIns[pin] = value; // assign the found value to the spread
              break;
            case Command.DIGITALMESSAGE:
              int port; int[] vals;
              // Decode the values from the bytes:
              FirmataUtils.DecodePortMessage(buffer.ToArray(),out port, out vals);
              // Fill the spread with parsed pinstates
              int pinNum;
              for (int i=0; i<Constants.BitsPerPort; i++) {
                pinNum = i+Constants.BitsPerPort*port;
                if ( pinNum < FDigitalIns.SliceCount) FDigitalIns[pinNum] = vals[i];
              }
              break;
            case Command.REPORT_VERSION:
              int major = (int) buffer.Dequeue();
              int minor = (int) buffer.Dequeue();
              FFirmwareMajorVersion[0] = major;
              FFirmwareMinorVersion[0] = minor;
              break;
            default:
              // unkown byte...
              break;
          }
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
          string name = FirmataUtils.GetStringFromBytes(data);
          string the_name = major + "." + minor;
          if (name.Length > 0) the_name += " " + name.ToString();

          FFirmwareMajorVersion[0] = major;
          FFirmwareMinorVersion[0] = minor;
          FFirmwareName[0] = name.ToString();
          FFirmwareVersion[0] = the_name;
        break;
        
        /// Handle String Data            
        case Command.STRING_DATA:
          string message = FirmataUtils.GetStringFromBytes(data);
          StringOut.Add(message);
          StringOut.Flush(true); // Signal change, regardless of the message
          break;
        
        /// Handle I2C replies
        case Command.I2C_REPLY:
          try {

            MemoryStream i2cStream = new MemoryStream(data.ToArray());

            FI2CData.Length = 1;
            using (var i2cWriter = FI2CData.GetWriter()) {
              i2cWriter.Write(i2cStream);
            }

          } catch (Exception e) {
            FI2CData.Length = 0;
          }
          data.Clear();

          break;
          
        case Command.CAPABILITY_RESPONSE:
          string report = "";
          int pinCount = 0;

          int digitalPins = 0;
          int analogPins = 0;
          int servoPins = 0;
          int pwmPins = 0;
          int shiftPins = 0;
          int i2cPins = 0;
          byte[] buffer = data.ToArray();

          for(int a=0; a<buffer.Length; a++) {
            pinCount++;
            report += "Pin "+pinCount.ToString()+":";

            if (buffer[a]==0x7f) {
              report += "\tNot available!";
            }

            while(buffer[a]!=0x7f) {
              PinMode mode = (PinMode) buffer[a++];
              switch(mode) {
                case PinMode.ANALOG:
                  analogPins++;
                  break;
                case PinMode.INPUT:
                case PinMode.OUTPUT:
                  digitalPins++;
                  break;
                case PinMode.SERVO:
                  servoPins++;
                  break;
                case PinMode.PWM:
                  pwmPins++;
                  break;
                case PinMode.I2C:
                  i2cPins++;
                  break;
                case PinMode.SHIFT:
                  shiftPins++;
                  break;
              }

              int resolution = buffer[a++];

              report += "\t"+FirmataUtils.PinModeToString(mode);
              report += " ("+resolution.ToString()+" bit) ";
            }
            
            report += "\r\n";
          }
          digitalPins /= 2;
          report += "Total number of pins: "+pinCount.ToString()+"\r\n";
          report += string.Format("{0} digital, {1} analog, {2} servo, {3} pwm and {4} i2c pins\r\n",digitalPins,analogPins,servoPins,pwmPins,i2cPins);
          FCapabilities.SliceCount = 1;
          FCapabilities[0] = report;
          break;
      }
    }
    #endregion Plugin Functions

  }



  #region PluginInfo
  [PluginInfo(Name = "I2CDecode",
              Category = "Firmata",
              Version = "2.x Legacy2",
              Author = "jens a. ewald",
              Help = "Decodes I2C data from Firmata messages",
              Tags = "arduino")]
  #endregion PluginInfo
  public class I2CDecode : IPluginEvaluate
  {
    ///
    /// Input
    ///
    [Input("I2C Data")]
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
