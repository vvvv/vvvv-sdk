/*
 * Created by SharpDevelop.
 * User: frederik
 * Date: 11/01/2013
 * Time: 15:21
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using System.Collections.Generic;

namespace InterProcessSendReceiveNodes
{
	/// <summary>
	/// Description of Utils.
	/// </summary>
	public static class Utils
	{

		public static byte[] GenerateMessage(ISpread<string> spread, uint version) {
			List<byte> bytes = new List<byte>();

			//message version			
			bytes.AddRange(BitConverter.GetBytes((UInt32)version));
			
			//nr of slices
			bytes.AddRange(BitConverter.GetBytes((Int32)spread.SliceCount));
			
			for ( int i = 0; i < spread.SliceCount; i++ ) {
				//string length
				bytes.AddRange( BitConverter.GetBytes((Int32)spread[i].Length) );
				
				//string bytes
				bytes.AddRange( new System.Text.UnicodeEncoding().GetBytes(spread[i]) );
			}
			return bytes.ToArray();
		}


		public static uint GetVersion(byte[] bytes) {			
			//Read version
			return BitConverter.ToUInt32(bytes, 0);
		}
		
		public static void ProcessMessage(byte[] bytes, ISpread<string> spread) {
			//skip version
			int pos = sizeof(uint);
			
			//Read slicecount
			spread.SliceCount = BitConverter.ToInt32(bytes, pos);
			pos += sizeof(Int32);
			
			for ( int i = 0; i < spread.SliceCount; i++ ) {
				//Read string length (nr of characters)
				int len = BitConverter.ToInt32(bytes, pos);
				pos += sizeof(Int32);
				
				//Read string bytes
				//BitConverter.ToChar(
				spread[i] = new System.Text.UnicodeEncoding().GetString(bytes, pos, len * sizeof(char));
				pos += len * sizeof(char);
			}
		}
	}
}
