#region licence/info
// OSC.NET - Open Sound Control for .NET
// http://luvtechno.net/
//
// Copyright (c) 2006, Yoshinori Kawasaki 
// All rights reserved.
//
// Changes and improvements:
// Copyright (c) 2005-2008 Martin Kaltenbrunner <mkalten@iua.upf.edu>
// As included with    
// http://reactivision.sourceforge.net/
//
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
// * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
// * Neither the name of "luvtechno.net" nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS 
// OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY 
// AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR 
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY 
// WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion licence/info

using System;
using System.Collections;
using System.IO;
using System.Text;
using VVVV.Utils.VColor;

namespace VVVV.Utils.OSC
{
	/// <summary>
	/// OSCPacket
	/// </summary>
	abstract public class OSCPacket
	{
		public static readonly Encoding ASCIIEncoding8Bit;

        static OSCPacket()
        {
            ASCIIEncoding8Bit = Encoding.GetEncoding(1252);
        }
        
		public OSCPacket()
		{
			this.values = new ArrayList();
		}

		protected static void addBytes(ArrayList data, byte[] bytes)
		{
			foreach(byte b in bytes)
			{
				data.Add(b);
			}
		}

		protected static void padNull(ArrayList data)
		{
			byte zero = 0;
			int pad = 4 - (data.Count % 4);
			for (int i = 0; i < pad; i++)
			{
				data.Add(zero);
			}
		}

		internal static byte[] swapEndian(byte[] data)
		{
			byte[] swapped = new byte[data.Length];
			for(int i = data.Length - 1, j = 0 ; i >= 0 ; i--, j++)
			{
				swapped[j] = data[i];
			}
			return swapped;
		}

		protected static byte[] packInt(int value)
		{
			byte[] data = BitConverter.GetBytes(value);
			if(BitConverter.IsLittleEndian)	data = swapEndian(data);
			return data;
		}

		protected static byte[] packLong(long value)
		{
			byte[] data = BitConverter.GetBytes(value);
			if(BitConverter.IsLittleEndian) data = swapEndian(data);
			return data;
		}

		protected static byte[] packFloat(float value)
		{
			byte[] data = BitConverter.GetBytes(value);
			if(BitConverter.IsLittleEndian) data = swapEndian(data);
			return data;
		}

		protected static byte[] packDouble(double value)
		{
			byte[] data = BitConverter.GetBytes(value);
			if(BitConverter.IsLittleEndian) data = swapEndian(data);
			return data;
		}

		protected static byte[] packString(string value)
		{
			return ASCIIEncoding8Bit.GetBytes(value);
		}


        protected static byte[] packChar(char value)
        {
            byte[] data = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) data = swapEndian(data);
            return data;
        }

        protected static byte[] packStream(MemoryStream value)
        {
            byte[] data = value.ToArray();
            return data;
        }

        protected static byte[] packTimetag(OscTimeTag value)
        {
            return value.ToByteArray(); ;
        }

        protected static byte[] packColor(RGBAColor value)
        {
            double[] rgba = {value.R, value.G, value.B, value.A};

            byte[] data = new byte[rgba.Length];
            for (int i = 0; i < rgba.Length; i++) data[i] = (byte) Math.Round(rgba[i]*255);
            if (BitConverter.IsLittleEndian) data = swapEndian(data);
            return data;
        }

		abstract protected void pack();
		protected byte[] binaryData;
		public byte[] BinaryData
		{
			get
			{
				pack();
				return binaryData;
			}
		}

		protected static int unpackInt(byte[] bytes, ref int start)
		{
			byte[] data = new byte[4];
			for(int i = 0 ; i < 4 ; i++, start++) data[i] = bytes[start];
			if(BitConverter.IsLittleEndian) data = swapEndian(data);
			return BitConverter.ToInt32(data, 0);
		}

		protected static long unpackLong(byte[] bytes, ref int start)
		{
			byte[] data = new byte[8];
			for(int i = 0 ; i < 8 ; i++, start++) data[i] = bytes[start];
			if(BitConverter.IsLittleEndian) data = swapEndian(data);
			return BitConverter.ToInt64(data, 0);
		}

		protected static float unpackFloat(byte[] bytes, ref int start)
		{
			byte[] data = new byte[4];
			for(int i = 0 ; i < 4 ; i++, start++) data[i] = bytes[start];
			if(BitConverter.IsLittleEndian) data = swapEndian(data);
			return BitConverter.ToSingle(data, 0);
		}

		protected static double unpackDouble(byte[] bytes, ref int start)
		{
			byte[] data = new byte[8];
			for(int i = 0 ; i < 8 ; i++, start++) data[i] = bytes[start];
			if(BitConverter.IsLittleEndian) data = swapEndian(data);
			return BitConverter.ToDouble(data, 0);
		}

		protected static string unpackString(byte[] bytes, ref int start)
		{
			int count= 0;
			for(int index = start ; bytes[index] != 0 ; index++, count++) ;
			string s = ASCIIEncoding8Bit.GetString(bytes, start, count);
			start += count+1;
			start = (start + 3) / 4 * 4;
			return s;
		}

        protected static char unpackChar(byte[] bytes, ref int start)
        {
            byte[] data = {bytes[start]};
            return BitConverter.ToChar(data, 0);
        }

        protected static MemoryStream unpackBlob(byte[] bytes, ref int start)
        {
            int length = unpackInt(bytes, ref start);

            byte[] buffer = new byte[length];
            Array.Copy(bytes, start, buffer, 0, length);


            return new MemoryStream(buffer);
        }



        protected static RGBAColor unpackColor(byte[] bytes, ref int start)
        {
            byte[] data = new byte[4];
            for (int i = 0; i < 4; i++, start++) data[i] = bytes[start];
            if (BitConverter.IsLittleEndian) data = swapEndian(data);
            
            var col = new RGBAColor();
            col.R = (double)data[0] / 255.0;
            col.G = (double)data[1] / 255.0;
            col.B = (double)data[2] / 255.0;
            col.A = (double)data[3] / 255.0;

            return col;
        }

        protected static OscTimeTag unpackTimeTag(byte[] bytes, ref int start)
        {
            byte[] data = new byte[8];
            for (int i = 0; i < 8; i++, start++) data[i] = bytes[start];
            return new OscTimeTag(data);
        }

		public static OSCPacket Unpack(byte[] bytes)
		{
			int start = 0;
			return Unpack(bytes, ref start, bytes.Length);
		}

		public static OSCPacket Unpack(byte[] bytes, ref int start, int end)
		{
			if(bytes[start] == '#') return OSCBundle.Unpack(bytes, ref start, end);
			else return OSCMessage.Unpack(bytes, ref start);
		}


		protected string address;
		public string Address
		{
			get { return address; }
			set 
			{
				// TODO: validate
				address = value;
			}
		}

		protected ArrayList values;
		public ArrayList Values
		{
			get { return (ArrayList)values.Clone(); }
		}
		abstract public void Append(object value);

		abstract public bool IsBundle();
	}
}
