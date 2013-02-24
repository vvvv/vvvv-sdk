/*
 * Created by SharpDevelop.
 * User: frederik
 * Date: 11/01/2013
 * Time: 15:21
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;

using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.Streams;

using System.Collections.Generic;

namespace InterProcessSendReceiveNodes
{
	public enum MessageTypeEnum {
		stringSpread,
		doubleSpread,
		colorSpread,
		rawSpread //System.IO.Stream
	}
	
	/// <summary>
	/// Description of Utils.
	/// </summary>
	public static class Utils
	{
		public static string GetChannelPrefix(ISpread<string> spread) {
			return "vvvv/InterProcess/StringSpread/";
		}
		public static string GetChannelPrefix(ISpread<double> spread) {
			return "vvvv/InterProcess/DoubleSpread/";
		}
		public static string GetChannelPrefix(ISpread<RGBAColor> spread) {
			return "vvvv/InterProcess/ColorSpread/";
		}
		public static string GetChannelPrefix(IStream spread) {
			return "vvvv/InterProcess/StreamSpread/";
		}
		
		
		
		public static byte[] GenerateMessage(ISpread<string> spread, UInt32 version) {
			List<byte> bytes = new List<byte>();

			//message type
			bytes.AddRange(BitConverter.GetBytes((UInt32)MessageTypeEnum.stringSpread));

			//message version
			bytes.AddRange(BitConverter.GetBytes((UInt32)version));
			
			//nr of slices
			bytes.AddRange(BitConverter.GetBytes((Int32)spread.SliceCount));
			
			for ( Int32 i = 0; i < spread.SliceCount; i++ ) {
				//string length
				bytes.AddRange( BitConverter.GetBytes((Int32)spread[i].Length) );
				
				//string bytes
				bytes.AddRange( new System.Text.UnicodeEncoding().GetBytes(spread[i]) );
			}
			return bytes.ToArray();
		}

		public static void ProcessMessage(byte[] bytes, ISpread<string> spread) {
			//skip message type & version
			Int32 pos = sizeof(UInt32) * 2;
			
			//Read slicecount
			spread.SliceCount = BitConverter.ToInt32(bytes, pos);
			pos += sizeof(Int32);
			
			for ( Int32 i = 0; i < spread.SliceCount; i++ ) {
				//Read string length (nr of characters)
				Int32 len = BitConverter.ToInt32(bytes, pos);
				pos += sizeof(Int32);
				
				//Read string bytes
				//BitConverter.ToChar(
				spread[i] = new System.Text.UnicodeEncoding().GetString(bytes, pos, len * sizeof(char));
				pos += len * sizeof(char);
			}
		}

		
		public static byte[] GenerateMessage(ISpread<double> spread, UInt32 version) {
			List<byte> bytes = new List<byte>();

			//message type
			bytes.AddRange(BitConverter.GetBytes((UInt32)MessageTypeEnum.doubleSpread));

			//message version
			bytes.AddRange(BitConverter.GetBytes((UInt32)version));
			
			//nr of slices
			bytes.AddRange(BitConverter.GetBytes((Int32)spread.SliceCount));
			
			for ( Int32 i = 0; i < spread.SliceCount; i++ ) {
				//float bytes
				bytes.AddRange( BitConverter.GetBytes((Double)spread[i]) );
			}
			return bytes.ToArray();
		}

		public static void ProcessMessage(byte[] bytes, ISpread<double> spread) {
			//skip message type & version
			Int32 pos = sizeof(UInt32) * 2;
			
			//Read slicecount
			spread.SliceCount = BitConverter.ToInt32(bytes, pos);
			pos += sizeof(Int32);
			
			for ( Int32 i = 0; i < spread.SliceCount; i++ ) {
				//Read Double bytes
				spread[i] = BitConverter.ToDouble(bytes, pos);
				pos += sizeof(Double);
			}
		}


		public static byte[] GenerateMessage(ISpread<RGBAColor> spread, UInt32 version) {
			List<byte> bytes = new List<byte>();

			//message type
			bytes.AddRange(BitConverter.GetBytes((UInt32)MessageTypeEnum.colorSpread));

			//message version
			bytes.AddRange(BitConverter.GetBytes((UInt32)version));
			
			//nr of slices
			bytes.AddRange(BitConverter.GetBytes((Int32)spread.SliceCount));
			
			for ( Int32 i = 0; i < spread.SliceCount; i++ ) {			
				//color bytes
				bytes.AddRange( BitConverter.GetBytes((Double)spread[i].R) );
				bytes.AddRange( BitConverter.GetBytes((Double)spread[i].G) );
				bytes.AddRange( BitConverter.GetBytes((Double)spread[i].B) );
				bytes.AddRange( BitConverter.GetBytes((Double)spread[i].A) );
			}
			return bytes.ToArray();
		}

		public static void ProcessMessage(byte[] bytes, ISpread<RGBAColor> spread) {
			//skip message type & version
			Int32 pos = sizeof(UInt32) * 2;
			
			//Read slicecount
			spread.SliceCount = BitConverter.ToInt32(bytes, pos);
			pos += sizeof(Int32);
			
			for ( Int32 i = 0; i < spread.SliceCount; i++ ) {
				//Read RGBAColor bytes
				RGBAColor c = new RGBAColor();
				c.R = BitConverter.ToDouble(bytes, pos);
				pos += sizeof(Double);
				c.G = BitConverter.ToDouble(bytes, pos);
				pos += sizeof(Double);
				c.B = BitConverter.ToDouble(bytes, pos);
				pos += sizeof(Double);
				c.A = BitConverter.ToDouble(bytes, pos);
				pos += sizeof(Double);
				
				spread[i] = c;
			}
		}


        //public static byte[] GenerateMessage(IInStream<Stream> spread, UInt32 version)
        //{
        //    //message type
        //    byte[] typeBytes = BitConverter.GetBytes((UInt32)MessageTypeEnum.rawSpread);

        //    //message version
        //    byte[] versionBytes = BitConverter.GetBytes((UInt32)version);

        //    //first calculate the size
        //    UInt64 size = (UInt64)(typeBytes.Length + versionBytes.Length);

        //    //stream length
        //    UInt64 dataLength = (UInt64)spread.Length;
        //    byte[] dataLengthBytes = BitConverter.GetBytes((UInt64)dataLength);
        //    size += (UInt64)dataLengthBytes.Length;
        //    size += dataLength;

        //    byte[] bytes = new byte[size];

        //    Int32 index = 0;
        //    typeBytes.CopyTo(bytes, index);
        //    index += typeBytes.Length;
        //    versionBytes.CopyTo(bytes, index);
        //    index += versionBytes.Length;
        //    dataLengthBytes.CopyTo(bytes, index);
        //    index += dataLengthBytes.Length;

        //    //stream bytes
        //    spread.ToStream().Buffer.CopyTo(bytes, index);

        //    //using ( var reader = spread.GetReader() ) {
        //    //    while (!reader.Eos)
        //    //    {
        //    //        var numSlicesRead = reader.Read(bytes, index, bytes.Length);
        //    //        for (int i = 0; i < numSlicesRead; i++)
        //    //        {
        //    //            var slice = buffer[i];
        //    //            // Do something with the slice
        //    //        }
        //    //    }
        //    //}

        //    //spread.Read(bytes, index, dataLength);
        //    //spread.GetReader().Read(bytes, index, (Int32)dataLength);

        //    //var memoryStream = new MemoryStream();
        //    //spread.ToStream().Buffer.CopyTo(bytes, index); //.CopyTo(memoryStream);
        //    //return memoryStream.ToArray();

        //    return bytes;
        //}

        //public static void ProcessMessage(byte[] bytes, IOutStream<Stream> spread)
        //{
        //    //skip message type & version
        //    Int32 pos = sizeof(UInt32) * 2;

        //    //Read dataLength
        //    UInt64 dataLength = BitConverter.ToUInt64(bytes, pos);
        //    pos += sizeof(UInt64);

        //    ////Read Stream bytes
        //    //spread.GetWriter().Write<
        //    //spread.GetWriter().Write(bytes, 0,  dataLength);

        //    //var numSlicesToRead = ...;
 

        //    // First set the length of the output stream
        //    spread.Length = (Int32)dataLength;

        //    UInt64 numSlicesToRead = dataLength;
        //    UInt32 chunkSize = 1;
        //    //using (var reader1 = spread.GetCyclicReader())
        //    using (var writer = spread.GetWriter())
        //    {
        //        while (numSlicesToRead > 0) {
        //            // Read chunks of data
        //            // Call your basic op on the data chunks and put results
        //            // in a result buffer
        //            // Write the result buffer to the output
        //            writer.Write(bytes, pos, chunkSize);
        //            // Decrease the number of slices to read by our chunk size
        //            numSlicesToRead -= chunkSize;
        //        }
        //    }
        //}

		
		
		
		public static MessageTypeEnum GetMessageType(byte[] bytes) {
			//Read MessageType
			return (MessageTypeEnum)BitConverter.ToUInt32(bytes, 0);
		}
		
		public static UInt32 GetVersion(byte[] bytes) {
			//Read version
			return BitConverter.ToUInt32(bytes, sizeof(UInt32));
		}
		
		

	}
}
