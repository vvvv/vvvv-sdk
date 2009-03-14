using System;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.InteropServices;
using VVVV.Nodes.HttpGUI.Datenobjekte;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes.HttpGUI
{
	[Guid("8869A551-6F32-4F0D-9003-27AC990D53D6"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IHttpGUIIO: INodeIOBase
	{

        void GetDatenObjekt(int Index, out BaseDatenObjekt GuiDaten);

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

	
		public static string FriendlyName = "HTTP GUI Element";
	}





    [Guid("FDFEC9C0-8049-4fb6-A5C8-FE04F26F4CA6"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IHttpGUIFunktionIO : INodeIOBase
    {
        void GetFunktionObjekt(int Index, out JsFunktion GuiDaten);

    }

    public class HttpGUIFunktionIO
    {
        private static Guid FGuid;


        public static Guid GUID
        {
            get
            {
                if (FGuid == Guid.Empty)

                    FGuid = new Guid("FDFEC9C0-8049-4fb6-A5C8-FE04F26F4CA6");
                return FGuid;
            }

        }


        public static string FriendlyName = "HTTP GUI Funktion";
    }




    [Guid("55B727DD-0CD6-427b-98AB-4D147B982AD5"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IHttpGUIStyleIO : INodeIOBase
    {
        void GetCssProperties(int Index, out SortedList<string,string> CssProperties);
        void GetInputChanged(out bool ChangedValue);

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

        public static string FriendlyName = "HTTP GUI CSS";
    }
}
