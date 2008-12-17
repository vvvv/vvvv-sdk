using System;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes
{
	[Guid("8869A551-6F32-4F0D-9003-27AC990D53D6"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMyNodeIO: INodeIOBase
	{
		void GetSlice(int Slice, out int Value);
	}
	
	public class MyNodeIO
	{
		private static Guid FGuid;
		public static Guid GUID
		{
			get
			{
				if (FGuid == Guid.Empty)
					FGuid = new Guid("8869A551-6F32-4F0D-9003-27AC990D53D6");
				return FGuid;
			}
		}
		
		public static string FriendlyName = "Simple Plugin NodeType";
	}
}
