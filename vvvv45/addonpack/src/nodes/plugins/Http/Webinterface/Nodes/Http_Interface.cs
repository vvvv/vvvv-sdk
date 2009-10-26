using System;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.InteropServices;
using VVVV.Nodes.HttpGUI;
using VVVV.PluginInterfaces.V1;
using VVVV.Webinterface.Utilities;

namespace VVVV.Nodes.HttpGUI
{

	[Guid("8869A551-6F32-4F0D-9003-27AC990D53D6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IHttpGUIIO: INodeIOBase
	{
        void GetDataObject(int Index, out List<GuiDataObject> GuiDaten);
		bool PinIsChanged();

	}

	public class HttpGUIIO
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

	
		public static string FriendlyName = "HTTP GUI";
	}




    [Guid("55B727DD-0CD6-427b-98AB-4D147B982AD5"),  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IHttpGUIStyleIO : INodeIOBase
    {
        void GetCssProperties(int Index, out SortedList<string,string> CssProperties);
		bool PinIsChanged();
    }

    public class HttpGUIStyleIO
    {

        private static Guid FGuid;

        public static Guid GUID
        {
            get
            {
                if (FGuid == Guid.Empty)

                    FGuid = new Guid("55B727DD-0CD6-427b-98AB-4D147B982AD5");
                return FGuid;
            }

        }

        public static string FriendlyName = "HTTP CSS";
    }




    [Guid("219B6AA6-ECFC-4a01-AF9C-889F23801FB1"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IHttpPageIO : INodeIOBase
    {
        void GetPage(out string PageName, out string FileName);

    }

    public class HttpPageIO
    {

        private static Guid FGuid;

        public static Guid GUID
        {
            get
            {
                if (FGuid == Guid.Empty)

                    FGuid = new Guid("219B6AA6-ECFC-4a01-AF9C-889F23801FB1");
                return FGuid;
            }

        }

        public static string FriendlyName = "HTTP Page";
    }


	[Guid("8EA17FEC-7848-4e0f-81DA-0E055B69BBE3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IJQueryIO : INodeIOBase
	{
		bool PinIsChanged();
	}

	public class JQueryIO
	{
		private static Guid FGuid;

		public static Guid GUID
		{
			get
			{
				if (FGuid == Guid.Empty)

					FGuid = new Guid("8EA17FEC-7848-4e0f-81DA-0E055B69BBE3");
				return FGuid;
			}

		}

		public static string FriendlyName = "JQuery";
	}
}
