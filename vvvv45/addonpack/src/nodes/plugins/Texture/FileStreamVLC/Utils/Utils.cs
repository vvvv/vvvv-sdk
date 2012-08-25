/*
 * Created by SharpDevelop.
 * User: frederik
 * Date: 29/02/2012
 * Time: 17:35
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
  
//    vvvv Vlc plugin
//    
//    Author:  Frederik Tilkin
//
//    vvvv Vlc plugin is free software: you can redistribute it and/or modify
//    it under the terms of the GNU Lesser General Public License as published by
//    the Free Software Foundation, either version 2.1+ of the License, or
//    (at your option) any later version.
//
//    vvvv Vlc plugin is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Lesser General Public License for more details.
//     
// ========================================================================


using System;
using SlimDX;
using SlimDX.Direct3D9;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Core.Logging;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.SlimDX;

using System.Threading;

namespace VVVV.Nodes.Vlc.Utils
{
	/// <summary>
	/// Description of DoubleTexture.
	/// </summary>
		public class VlcUtils
		{

			public static Texture CreateMemoryTexture(Device device, int width, int height)
			{
				return new Texture(device, width, height, 1, Usage.None, Format.A8R8G8B8, Pool.SystemMemory);
			}
	
			public static Texture CreateManagedTexture(Device device, int width, int height)
			{
				return new Texture(device, width, height, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
			}
	
			public static Texture CreateDefaultTexture(Device device, int width, int height)
			{
				return new Texture(device, width, height, 1, Usage.Dynamic, Format.A8R8G8B8, Pool.Default);
			}
	
			public static Texture CreateSharedTexture(Device device, int width, int height, ref IntPtr sharedHandle)
			{
				return new Texture(device, width, height, 1, Usage.Dynamic, Format.A8R8G8B8, Pool.Default, ref sharedHandle);
			}

		}
}
