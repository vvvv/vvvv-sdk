using System;
using System.IO.Ports;
using System.Linq;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes
{
	
	// Photoshop Enums
	// Source:
	// https://github.com/adobe-photoshop/generator-core/wiki/Photoshop-Kevlar-API-Additions-for-Generator
	//
	
	public enum PhotoshopEvents
	{
		imageChanged,
		generatorMenuChanged,
		generatorDocActivated,
		foregroundColorChanged,
		backgroundColorChanged,
		currentDocumentChanged,
		activeViewChanged,
		newDocumentViewCreated,
		closedDocument,
		documentChanged,
		colorSettingsChanged,
		keyboardShortcutsChanged,
		quickMaskStateChanged,
		toolChanged,
		workspaceChanged,
		Asrt,
		idle
	}
	
	public enum PhotoshopImageFormat
	{
		JPEG,
		Pixmap
	}
	
	[Startable]
	public class GlobalEnumManager : IStartable
	{
		
		//RS232.ComPort ENUM
		public const string COM_PORT_ENUM_NAME = "Rs232Node.ComPort";
		
		public static void UpdatePortList()
        {
            var portNames = SerialPort.GetPortNames()
                .Where(n => n.StartsWith("com", StringComparison.InvariantCultureIgnoreCase))
                .ToArray();
            EnumManager.UpdateEnum(COM_PORT_ENUM_NAME, portNames.Length > 0 ? portNames[0] : string.Empty, portNames);
        }
		
		
		public void Start()
		{
			//Photoshop
			EnumManager.UpdateEnum("PhotoshopEvents", "documentChanged", Enum.GetNames(typeof(PhotoshopEvents)));
			EnumManager.UpdateEnum("PhotoshopImageFormat", "JPEG", Enum.GetNames(typeof(PhotoshopImageFormat)));
			
			//RS232
			UpdatePortList();
		}
		public void Shutdown()
		{
		}
		
	}
}
