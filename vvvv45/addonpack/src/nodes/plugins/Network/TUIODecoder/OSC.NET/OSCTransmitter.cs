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
using System.Net;
using System.Net.Sockets;

namespace OSC.NET
{
	/// <summary>
	/// OSCTransmitter
	/// </summary>
	public class OSCTransmitter
	{
		protected UdpClient udpClient;
		protected string remoteHost;
		protected int remotePort;

		public OSCTransmitter(string remoteHost, int remotePort)
		{
			this.remoteHost = remoteHost;
			this.remotePort = remotePort;
			Connect();
		}

		public void Connect()
		{
			if(this.udpClient != null) Close();
			this.udpClient = new UdpClient(this.remoteHost, this.remotePort);
		}

		public void Close()
		{
			this.udpClient.Close();
			this.udpClient = null;
		}

		public int Send(OSCPacket packet)
		{
			int byteNum = 0;
			byte[] data = packet.BinaryData;
			try 
			{
				byteNum = this.udpClient.Send(data, data.Length);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e.Message);
				Console.Error.WriteLine(e.StackTrace);
			}

			return byteNum;
		}

	}
}
