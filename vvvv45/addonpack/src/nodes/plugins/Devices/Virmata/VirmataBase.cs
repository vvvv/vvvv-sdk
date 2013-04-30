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
using System.Collections.Generic;

#endregion usings

namespace Firmata
{
  #region Static utils
  public static class FirmataUtils {

    public static bool ContainsCommand(byte[] msg, byte cmd) {
      foreach (byte b in msg) {
        if (VerifiyCommand(b,cmd)) return true;
      }
      return false;
    }

    public static bool VerifiyCommand(byte b, byte cmd) {
      return GetCommandFromByte(b) == cmd;
    }

    public static byte GetCommandFromByte (byte data) {
      // Commands with channel data need to be masked
      int masked = data & 0xF0;
      return (
           masked != Command.DIGITALMESSAGE
        && masked != Command.ANALOGMESSAGE
        && masked != Command.TOGGLEDIGITALREPORT
        && masked != Command.TOGGLEANALOGREPORT
      ) ? data :  (byte)(data & 0xF0);
    }

    public static byte[] PortMessage(int port, int[] values) {
      byte LSB,MSB;
      GetBytesFromValue((int)GetPortFromPinValues(values),out MSB, out LSB);
      byte[] command = {(byte)(Command.DIGITALMESSAGE | port),LSB,MSB };
      return command;
    }

    public static bool DecodePortMessage(byte[] data, out int port, out int[] values) {
      if (data.Length<3){
        port = 0;
        values = new int[8];
        return false;
      }
      port = (data[0] & 0x0f);
      int state = GetValueFromBytes(data[2],data[1]);
      values    = GetPinValuesFromPortState(state);
      return true;
    }

    public static bool DecodeAnalogMessage(byte[] data, out int pin, out int value) {
      if (data.Length<3){
        pin = 0;
        value = 0;
        return false;
      }
      pin = (data[0] & 0x0f);
      value = GetValueFromBytes(data[2],data[1]);
      return true;
    }

    /// <summary>
    /// Get the integer value that was sent using the 7-bit messages of the firmata protocol
    /// </summary>
    public static int GetValueFromBytes(byte MSB, byte LSB)
    {
      return ((MSB & 0x7F) << 7) | (LSB & 0x7F);
    }

    /// <summary>
    /// Split an integer value to two 7-bit parts so it can be sent using the firmata protocol
    /// </summary>
    public static void GetBytesFromValue(int value, out byte MSB, out byte LSB)
    {
      LSB = (byte)( value & 0x7F );
      MSB = (byte)((value >> 7) & 0x7F);
    }

    /// <summary>
    /// Send an array of boolean values indicating the state of each individual
    /// pin and get a byte representing a port
    /// </summary>
    public static byte GetPortFromPinValues(int[] pins)
    {
      byte port = 0;
      for (int i = 0; i < pins.Length; i++)
      {
        port |= (byte) ((pins[i]>0 ? 1 : 0) << i);
      }
      return port;
    }
    public static int[] GetPinValuesFromPortState(int state)
    {
      int[] port = new int[8];
      for (int i = 0; i < 8; i++)
      {
        port[i] = (state>>i) & 0x01;
      }
      return port;
    }

    public static string CommandBufferToString(Queue<byte> CommandBuffer, string Glue = "\r\n")
    {
      string s = string.Empty;
      while (CommandBuffer.Count > 0) {
        byte b = CommandBuffer.Dequeue();
        int currentSize = s.Length;
        // The Command Switch:
        switch(FirmataUtils.GetCommandFromByte(b)) {
          case Command.SYSEX_START:
            s+="Sysex (";
            byte sb = CommandBuffer.Dequeue();
            while(sb != Command.SYSEX_END) {
              switch(sb){
                case Command.SAMPLING_INTERVAL:
                  s+="SamplingInterval: ";
                  byte _LSB = CommandBuffer.Dequeue();
                  byte _MSB = CommandBuffer.Dequeue();
                  s+=FirmataUtils.GetValueFromBytes(_MSB,_LSB).ToString();
                  break;
                case Command.EXTENDED_ANALOG:
                  int pin  = CommandBuffer.Dequeue() & 0x7f;
                  byte __LSB = CommandBuffer.Dequeue();
                  byte __MSB = CommandBuffer.Dequeue();
                  s+="Extended Analog Message for pin ";
                  s+=pin.ToString() + ": ";
                  s+=FirmataUtils.GetValueFromBytes(__MSB,__LSB).ToString();
                  break;
                case Command.REPORT_FIRMWARE:
                  s+="ReportFirmwareVersion";
                  break;
                default:
                  s+= String.Format("{0:X}", sb);
                break;
              }
              sb = CommandBuffer.Dequeue();
            }
            s+=")";
            break;

          case Command.RESET:
            s+="Reset!";
            break;

          case Command.SETPINMODE:
            s+="Set PinMode of pin ";
            s+=CommandBuffer.Dequeue().ToString();
            s+=" to ";
            switch((PinMode)CommandBuffer.Dequeue()) {
              case PinMode.INPUT:   s+="INPUT";  break;
              case PinMode.OUTPUT:  s+="OUTPUT"; break;
              case PinMode.ANALOG:  s+="ANALOG"; break;
              case PinMode.PWM:     s+="PWM";    break;
              case PinMode.SERVO:   s+="SERVO";  break;
              case PinMode.SHIFT:   s+="SHIFT";  break;
              case PinMode.I2C:     s+="I2C";    break;
            }
            break;

          case Command.TOGGLEDIGITALREPORT:
            s+="DIGITAL Pin Reporting for port ";
            s+=(b&0x0f).ToString();
            s+=" set to: ";
            s+=CommandBuffer.Dequeue().ToString();
            break;

          case Command.TOGGLEANALOGREPORT:
            s+="ANALOG Pin Reporting for pin ";
            s+=(b&0x0f).ToString();
            s+=" set to: ";
            s+=CommandBuffer.Dequeue().ToString();
            break;

          case Command.DIGITALMESSAGE:
            s+="Digital message for port ";
            s+=(b&0x0f).ToString();
            s+=": ";
            s+=Convert.ToString(CommandBuffer.Dequeue(), 2);
            break;

          case Command.ANALOGMESSAGE:
            s+="Analog message for pin ";
            s+=(b&0x0f).ToString();
            s+=": ";
            byte LSB = CommandBuffer.Dequeue();
            byte MSB = CommandBuffer.Dequeue();
            s+=FirmataUtils.GetValueFromBytes(MSB,LSB).ToString();
            break;
        }
        if (s.Length != currentSize) s+=Glue;
      }
      return s;
    }

    public static string PinModeToString(PinMode mode)
    {
      string s = "";
      switch(mode) {
        case PinMode.INPUT:   s="INPUT";  break;
        case PinMode.OUTPUT:  s="OUTPUT"; break;
        case PinMode.ANALOG:  s="ANALOG"; break;
        case PinMode.PWM:     s="PWM";    break;
        case PinMode.SERVO:   s="SERVO";  break;
        case PinMode.SHIFT:   s="SHIFT";  break;
        case PinMode.I2C:     s="I2C";    break;
      }
      return s;
    }
  }

  #endregion

  #region Definitions
  /// For the Specs see: http://firmata.org/wiki/Protocol

  /// Some helpful constants for the NameSpace
  public struct Default {
    public const int SampleRate     = 20;
    public const int MaxAnalogPins  = 16;
    public const int MaxDigitalPins = 128;
    public const PinMode PINMODE = PinMode.OUTPUT;
  }

  public struct Constants {
    public const int BitsPerPort = 8;
  }


  public struct Command
  {
    /// <summary>
    /// The command that toggles the continuous sending of the
    /// analog reading of the specified pin
    /// </summary>
    public const byte TOGGLEANALOGREPORT = 0xC0;
    /// <summary>
    /// The distinctive value that states that this message is an analog message.
    /// It comes as a report for analog in pins, or as a command for PWM
    /// </summary>
    public const byte ANALOGMESSAGE = 0xE0;
    /// <summary>
    /// The command that toggles the continuous sending of the
    /// digital state of the specified port
    /// </summary>
    public const byte TOGGLEDIGITALREPORT = 0xD0;

    /// <summary>
    /// The distinctive value that states that this message is a digital message.
    /// It comes as a report or as a command
    /// </summary>
    public const byte DIGITALMESSAGE = 0x90;

    /// <summary>
    /// A command to change the pin mode for the specified pin
    /// </summary>
    public const byte SETPINMODE = 0xF4;

    /// <summary>
    /// Report Protocol Version
    /// </summary>
    public const byte REPORT_VERSION = 0xF9;

     /// <summary>
    /// Reset System Command
    /// </summary>
    public const byte RESET = 0xFF;


    /// SYSEX Commands:
    ///
    /// <summary>
    /// Sysex start command
    /// </summary>
    public const byte SYSEX_START = 0xF0;

    /// <summary>
    /// Sysex end command
    /// </summary>
    public const byte SYSEX_END = 0xF7;

    /// Subcommands
    public const byte RESERVED_COMMAND        = 0x00; // 2nd SysEx data byte is a chip-specific command (AVR, PIC, TI, etc).
    public const byte ANALOG_MAPPING_QUERY    = 0x69; // ask for mapping of analog to pin numbers
    public const byte ANALOG_MAPPING_RESPONSE = 0x6A; // reply with mapping info
    public const byte CAPABILITY_QUERY        = 0x6B; // ask for supported modes and resolution of all pins
    public const byte CAPABILITY_RESPONSE     = 0x6C; // reply with supported modes and resolution
    public const byte PIN_STATE_QUERY         = 0x6D; // ask for a pin's current mode and value
    public const byte PIN_STATE_RESPONSE      = 0x6E; // reply with a pin's current mode and value
    public const byte EXTENDED_ANALOG         = 0x6F; // analog write (PWM, Servo, etc) to any pin
    public const byte SERVO_CONFIG            = 0x70; // set max angle, minPulse, maxPulse, freq
    public const byte STRING_DATA             = 0x71; // a string message with 14-bits per char
    public const byte SHIFT_DATA              = 0x75; // shiftOut config/data message (34 bits)
    public const byte I2C_REQUEST             = 0x76; // I2C request messages from a host to an I/O board
    public const byte I2C_REPLY               = 0x77; // I2C reply messages from an I/O board to a host
    public const byte I2C_CONFIG              = 0x78; // Configure special I2C settings such as power pins and delay times
    public const byte REPORT_FIRMWARE         = 0x79; // report name and version of the firmware
    public const byte SAMPLING_INTERVAL       = 0x7A; // sampling interval
    public const byte SYSEX_NON_REALTIME      = 0x7E; // MIDI Reserved for non-realtime messages
    public const byte SYSEX_REALTIME          = 0x7F; // MIDI Reserved for realtime messages
    
  }

  public enum PinMode
  {
    /// <summary>
    /// Pinmode INPUT
    /// </summary>
    INPUT = 0x00,

    /// <summary>
    /// Pinmode OUTPUT
    /// </summary>
    OUTPUT = 0x01,

    /// <summary>
    /// Pinmode ANALOG
    /// </summary>
    ANALOG = 0x02,

    /// <summary>
    /// Pinmode PWM
    /// </summary>
    PWM = 0x03,

    /// <summary>
    /// Pinmode SERVO
    /// </summary>

    SERVO = 0x04,

    /// <summary>
    /// Pinmode for Shift Registers
    /// </summary>
    SHIFT = 0x05,

    /// <summary>
    /// Pinmode I2C
    /// </summary>
    I2C = 0x06,
  }

  public enum Port
  {
    /// <summary>
    /// This port represents digital pins 0..7. Pins 0 and 1 are reserved for communication
    /// </summary>
    PORTD = 0x00,

    /// <summary>
    /// This port represents digital pins 8..13. 14 and 15 are for the crystal
    /// </summary>
    PORTB = 0x01,

    /// <summary>
    /// This port represents analog input pins 0..5
    /// </summary>
    PORTC = 0x02,
  }

  #endregion
}
