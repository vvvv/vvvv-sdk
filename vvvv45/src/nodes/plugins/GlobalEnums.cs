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
	
	public enum ColorChannels
	{
		Red,
		Green,
		Blue,
		Alpha
	}

    public enum FirmataPinMode
    {
        Input,
        Output,
        Analog,
        PWM,
        Servo,
        Shift,
        I2C
    }

    public enum FirmataI2CReadMode
    {
        READ_ONCE = 0x08, // B00001000

        READ_CONTINUOUSLY = 0x10, // B00010000

        STOP_READING = 0x18  // B00011000
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

            //Firmata Pin Modes
            EnumManager.UpdateEnum("FirmataPinModes", "Input", new string[] { "Input", "Output", "Analog In", "PWM", "Servo", "Shift", "I2C" });

            //Firmata I2C Read Modes
            EnumManager.UpdateEnum("FirmataI2CReadModes", "READ_ONCE", Enum.GetNames(typeof(FirmataI2CReadMode)));

            //Firmata I2C Address Modes
            EnumManager.UpdateEnum("FirmataI2CAddressModes", "7 bit", new string[] {"7 bit", "10 bit"});

            //Color Channels
            EnumManager.UpdateEnum("ColorChannels", "Red", Enum.GetNames(typeof(ColorChannels)));

            //Cursor Type
            EnumManager.UpdateEnum("CursorType", "Pointer", new string[] { "Pointer", "Vert", "Hor", "Left Bottom", "Right Bottom", "Left Top", "Right Top" });

            //Point Type
            EnumManager.UpdateEnum("PointType", "Circle", new string[] { "Rectangle", "Triangle", "Circle"});

        }
		public void Shutdown()
		{
		}
		
	}
}
