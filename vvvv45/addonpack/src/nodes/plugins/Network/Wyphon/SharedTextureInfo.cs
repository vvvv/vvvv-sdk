/*
 * Created by SharpDevelop.
 * User: frederik
 * Date: 8/01/2013
 * Time: 9:52
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace VVVV.Nodes.Network.Wyphon
{
	/// <summary>
	/// Description of SharedTextureInfo.
	/// </summary>
	public class SharedTextureInfo
	{
		public uint partnerId;
		public uint textureHandle;
		public uint width;
		public uint height;
		public uint format;
		public uint usage;
		public string description;
				
		public SharedTextureInfo(uint partnerId, uint textureHandle, uint width, uint height, uint format, uint usage, string description) {
			this.partnerId = partnerId;
			this.textureHandle = textureHandle;
			this.width = width;
			this.height = height;
			this.format = format;
			this.usage = usage;
			this.description = description;
		}
	}
}
