using System;
using System.IO.Ports;
using System.Linq;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes
{
	public enum Ps
	{
		A,
		B,
		C,
		D
	}
	
	/// <summary>
	/// Description of GlobalEnums.
	/// </summary>
	[Startable]
	public class GlobalEnumManager : IStartable
	{
		public const string COM_PORT_ENUM_NAME = "Rs232Node.ComPort";
		
		public static void UpdatePortList()
        {
            var portNames = SerialPort.GetPortNames()
                .Where(n => n.StartsWith("com", StringComparison.InvariantCultureIgnoreCase))
                .ToArray();
            EnumManager.UpdateEnum(COM_PORT_ENUM_NAME, portNames.Length > 0 ? portNames[0] : string.Empty, portNames);
        }
		
		#region IStartable implementation
		public void Start()
		{
			EnumManager.UpdateEnum("aaaName", "", Enum.GetNames(typeof(Ps)));
			UpdatePortList();
		}
		public void Shutdown()
		{
		}
		#endregion
	}
}
