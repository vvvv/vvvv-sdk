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
		public UInt32 partnerId;
		public UInt32 textureHandle;
		public UInt32 width;
		public UInt32 height;
		public UInt32 format;
		public UInt32 usage;
		public string description;
				
		public SharedTextureInfo(UInt32 partnerId, UInt32 textureHandle, UInt32 width, UInt32 height, UInt32 format, UInt32 usage, string description) {
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
