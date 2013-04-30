#region Copyright notice
/*
A Firmata Plugin for VVVV - v 1.2
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
  [PluginInfo(Name = "FirmataEncode",
              Version = "2.x",
              Category = "Devices",
              Author = "jens a. ewald",
              Help = "Encodes pins, values and commands for Firmata protocol version 2.x",
              Tags = "Arduino")]
  #endregion PluginInfo

  public class FirmataEncode : IPluginEvaluate
  {
    #region Pin Definitions
    ///
    /// INPUT
    ///
    [Input("Input")]
    IDiffSpread<double> FPinValues;

    [Input("Pin Modes", DefaultEnumEntry = "INPUT")]
    IDiffSpread<PinMode> FPinModeSetup;

    [Input("Report Analog Pins",IsSingle = true, DefaultValue = 1)]
    IDiffSpread<bool> FReportAnalogPins;

    [Input("Report Digital Pins",IsSingle = true, DefaultValue = 1)]
    IDiffSpread<bool> FReportDigitalPins;

    [Input("Reset",IsSingle = true, IsBang=true, DefaultValue = 0)]
    IDiffSpread<bool> FResetSystem;

    [Input("Samplerate", MinValue = 0, DefaultValue = Default.SampleRate,
                         Visibility = PinVisibility.Hidden, IsSingle = true)]
    IDiffSpread<int> FSamplerate;

    [Input("Report Firmware Version", IsBang = true,
                                      Visibility = PinVisibility.OnlyInspector, IsSingle=true)]
    IDiffSpread<bool> FReportFirmwareVersion;

    [Input("Report Capabilities", IsSingle = true,
                    Visibility = PinVisibility.OnlyInspector, IsBang=true, DefaultValue = 0)]
    IDiffSpread<bool> FReportCapabilities;

    [Input("Analog Input Count",  DefaultValue = 6, MaxValue = Default.MaxAnalogPins, MinValue = 0,
                                  Visibility = PinVisibility.OnlyInspector, IsSingle = true)]
    IDiffSpread<int> FAnalogInputCount;

    [Input("Digital Input Count", DefaultValue = 20, MaxValue = Default.MaxDigitalPins, MinValue = 0,
                                  Visibility = PinVisibility.OnlyInspector, IsSingle = true)]
    IDiffSpread<int> FDigitalInputCount;


    ///
    /// OUTPUT
    ///
    [Output("Firmata Message")]
    IOutStream<Stream> FFirmataOut;

    [Output("On Change")]
    ISpread<bool> FChangedOut;

    #endregion Pin Definitions

    /// Use a Queue for a command byte buffer:
    Queue<byte> CommandBuffer = new Queue<byte>();

    /// EVALUATE
    public void Evaluate(int SpreadMax)
    {
      // Clear the buffer before everey run
      CommandBuffer.Clear();

      /// How many pins are there to handle
      if (ShouldReset || SpreadUtils.AnyChanged(FDigitalInputCount, FAnalogInputCount, FPinValues, FPinModeSetup)) {
        UpdatePinCount();
      }

      /// Shall we reset?
      if (ShouldReset) {
        GetResetCommand();
        GetCapabilityReport();
      }

      if (FReportCapabilities.IsChanged && FReportCapabilities[0] && !ShouldReset) {
        GetCapabilityReport();
      }

      /// Firmware Version requested?
      if ((FReportFirmwareVersion.IsChanged && FReportFirmwareVersion[0]) || ShouldReset) {
        GetFirmwareVersionCommand();
      }

      // TODO: Find out if we have pull-up configured input pins and if so, update the config too
      if(FPinModeSetup.IsChanged || ShouldReset) {
        UpdatePinConfiguration();
      }

      /// Write the values to the pins
      if (FPinModeSetup.IsChanged || FPinValues.IsChanged || ShouldReset) {
        SetPinStates(FPinValues);
      }

      /// Set sample rate
      if (FSamplerate.IsChanged || ShouldReset) {
        GetSamplerateCommand(FSamplerate[0]);
      }

      /// Set Pinreporting for analog pins
      if (FPinModeSetup.IsChanged || FReportAnalogPins.IsChanged || ShouldReset) {
        SetAnalogPinReportingForRange(FAnalogInputCount[0],FReportAnalogPins[0]);
      }

      /// Set Pinreporting for digital pins
      if (FReportDigitalPins.IsChanged || ShouldReset) {
        for(int port=0; port<NUM_PORTS; port++) {
          GetDigitalPinReportingCommandForState(FReportDigitalPins[0],port);
        }
      }

      bool HasData = CommandBuffer.Count>0;
      FChangedOut[0] = HasData;

      // Spreaded Encoders are not supported at the moment!
      FFirmataOut.Length = 1;

      try{
        if (HasData) {
          Stream outStream = new MemoryStream(CommandBuffer.ToArray());
          using (var outputWriter = FFirmataOut.GetWriter()) {
            outputWriter.Write(outStream);
          }
        }
      } catch(Exception e) {
        // Do nothing on errors. Yes, i am lazy on that one.
      }
      // END Evaluate
    }

    #region Helper Functions

    // Make a shortcut for FResetSystem[0]
    bool hasLaunched = false; // always reset on first launch
    bool ShouldReset {
      get {
        if (!hasLaunched) {
          hasLaunched = true;
          return true;
        } else {
          return FResetSystem.IsChanged && FResetSystem[0];
        }
      }
      set {}
    }

    byte[] OUTPUT_PORT_MASKS  = {}; // empty array

    int NUM_PORTS = 0; // The total number of ports (AVR PORTS) respective to the number of pins
    int NUM_PINS  = 0; // The total number of pins addressed by this node

    /// <summary>
    /// Calculate the total number of pins addressed with this node
    /// </summary>
    void UpdatePinCount()
    {
      /// Find the least count of pins
      NUM_PINS = Math.Min(FPinModeSetup.SliceCount,FPinValues.SliceCount);
      // TODO: If we do this, we need to fill the missing:
      // NUM_PINS = Math.Min(FDigitalInputCount[0],NUM_PINS);
      NUM_PINS = Math.Max(0,NUM_PINS);

      /// calculate the next full divider by 8:
      NUM_PORTS = NUM_PINS/8 + (NUM_PINS%8==0 ? 0 : 1);

      // we have to resize the masks
      if(OUTPUT_PORT_MASKS.Length != NUM_PORTS) {
        Array.Resize(ref OUTPUT_PORT_MASKS, NUM_PORTS);
      }
    }

    PinMode PinModeForPin(int pin)
    {
      return pin < FPinModeSetup.SliceCount ? FPinModeSetup[pin]:Firmata.Default.PINMODE;
    }

    /// <summary>
    /// Updates the pin masks, number of pins ,etc
    /// </summary>
    void UpdatePinConfiguration()
    {
      UpdatePinCount();

      // allocate memory once
      byte output_port;
      for(int i = 0; i<NUM_PORTS; i++)
      {
        // reset temporary port mask
        output_port=0x00;

        // Build the mask
        PinMode mode; int pin;
        for (int bit=0; bit<8; bit++)
        {
          mode = Firmata.Default.PINMODE;
          pin = i*8+bit;
          /// Set the mode and add to the configure command
          if(pin<FPinModeSetup.SliceCount)
          {
            mode = FPinModeSetup[pin];
            SetPinModeCommand(mode,pin);
          }
          // using both both modes, enables configuring
          // of pullup mode (thx! to motzi)
          output_port |= (byte)(((mode == PinMode.OUTPUT || mode == PinMode.INPUT) ? 1:0)<<bit);
        }
        OUTPUT_PORT_MASKS[i] = output_port;
      }
    }

    void SetPinModeCommand(PinMode mode, int pin)
    {
      CommandBuffer.Enqueue(Command.SETPINMODE);
      CommandBuffer.Enqueue((byte) pin);
      CommandBuffer.Enqueue((byte) mode);
    }

    static int PortIndexForPin(int pin)
    {
      return pin/8;
    }

    void SetPinStates(ISpread<double> values)
    {
      // get the number of output ports
      // FIXME: Get only those ports, whos values have changed
      int[] digital_out = new int[NUM_PORTS];
      Queue<byte> AnalogCommandBuffer = new Queue<byte>();
      int analogOutCount = 0;
      int pinCount = Math.Min(Default.MaxDigitalPins,Math.Min(NUM_PINS,values.SliceCount));
      for(int pin=0; pin<pinCount; pin++)
      {
        double value = values[pin];
        PinMode mode = PinModeForPin(pin);
        switch(mode)
        {
          case PinMode.PWM:
          case PinMode.SERVO:
            byte LSB,MSB;
            value *= mode==PinMode.SERVO ? 180 : 255; // servo is in degrees
            FirmataUtils.GetBytesFromValue((int)value,out MSB,out LSB);
            if (pin <= 0x0F) {
              AnalogCommandBuffer.Enqueue((byte)(Command.ANALOGMESSAGE | pin ));
              AnalogCommandBuffer.Enqueue(LSB);
              AnalogCommandBuffer.Enqueue(MSB);
            } else {
              AnalogCommandBuffer.Enqueue(Command.SYSEX_START);
              AnalogCommandBuffer.Enqueue(Command.EXTENDED_ANALOG);
              AnalogCommandBuffer.Enqueue((byte)(pin & 0x7F)); // mask 7 Bit
              AnalogCommandBuffer.Enqueue(LSB);
              AnalogCommandBuffer.Enqueue(MSB);
              AnalogCommandBuffer.Enqueue(Command.SYSEX_END);
            }
            break;

          case PinMode.OUTPUT:
          case PinMode.INPUT:   // fixes PullUp enabling issue, thx to motzi!
            int port = PortIndexForPin(pin);
            // Break, if we have no outputports we can get
            if (port < NUM_PORTS) {
              int state = ( value >= 0.5 ? 0x01 : 0x00 ) << pin%8;
              state |= digital_out[port];
              state &= OUTPUT_PORT_MASKS[port];
              digital_out[port] = (byte) state;
            }
            break;
        }
      }

      /// Write all the output ports to the command buffer
      for(int port=0; port < digital_out.Length; port++)
      {
        byte LSB,MSB;
        FirmataUtils.GetBytesFromValue(digital_out[port],out MSB,out LSB);
        CommandBuffer.Enqueue((byte)(Command.DIGITALMESSAGE | port));
        CommandBuffer.Enqueue(LSB);
        CommandBuffer.Enqueue(MSB);
      }

      /// Append the Commands for the analog messages
      if(AnalogCommandBuffer.Count > 0) {
        foreach (byte b in AnalogCommandBuffer) CommandBuffer.Enqueue(b);
      }
    }

    void GetSamplerateCommand(int rate)
    {
      byte lsb,msb;
      FirmataUtils.GetBytesFromValue(rate,out msb,out lsb);
      CommandBuffer.Enqueue(Command.SYSEX_START);
      CommandBuffer.Enqueue(Command.SAMPLING_INTERVAL);
      CommandBuffer.Enqueue(lsb);
      CommandBuffer.Enqueue(msb);
      CommandBuffer.Enqueue(Command.SYSEX_END);
    }

    /// Query Firmware Name and Version
    void GetFirmwareVersionCommand()
    {
      CommandBuffer.Enqueue(Command.SYSEX_START);
      CommandBuffer.Enqueue(Command.REPORT_FIRMWARE);
      CommandBuffer.Enqueue(Command.SYSEX_END);
    }

    void GetAnalogPinReportingCommandForState(bool state,int pin)
    {
      byte val = (byte) (state ? 0x01 : 0x00);
      CommandBuffer.Enqueue((byte)(Command.TOGGLEANALOGREPORT|pin));
      CommandBuffer.Enqueue(val);
    }

    void SetAnalogPinReportingForRange(int range, bool state)
    {
      for(int i = 0; i<range; i++)
        GetAnalogPinReportingCommandForState(state,i);
    }

    void GetDigitalPinReportingCommandForState(bool state,int port)
    {
      byte val = (byte) (state ? 0x01 : 0x00);
      CommandBuffer.Enqueue((byte)(Command.TOGGLEDIGITALREPORT|port));
      CommandBuffer.Enqueue(val);
    }

    void GetResetCommand()
    {
      CommandBuffer.Enqueue(Command.RESET);
    }

    void GetCapabilityReport() {
      CommandBuffer.Enqueue(Command.SYSEX_START);
      CommandBuffer.Enqueue(Command.CAPABILITY_QUERY);
      CommandBuffer.Enqueue(Command.SYSEX_END);
    }

    #endregion

  }

}
