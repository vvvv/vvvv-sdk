using System;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V1;

using ColladaSlimDX.ColladaModel;

namespace VVVV.Nodes
{
	[Guid("77e7e5b0-cd35-11dd-ad8b-0800200c9a66"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IColladaModelNodeIO: INodeIOBase
	{
		void GetSlice(int Slice, out Model ColladaModel);
	}
	
	public class ColladaModelNodeIO
	{
		private static Guid FGuid;
		public static Guid GUID
		{
			get
			{
				if (FGuid == Guid.Empty)
					FGuid = new Guid("77e7e5b0-cd35-11dd-ad8b-0800200c9a66");
				return FGuid;
			}
		}
		
		public static string FriendlyName = "COLLADA Model";
	}
}
