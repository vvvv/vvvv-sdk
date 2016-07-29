using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Utils.IO;

namespace VVVV.Lib
{

    public class MouseDataHolder : DataHolder<Mouse>
    {
        private static MouseDataHolder instance;

        private MouseDataHolder()
        {

        }

        public static MouseDataHolder Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MouseDataHolder();
                }
                return instance;
            }
        }
    }

    public class KeyboardDataHolder : DataHolder<Keyboard>
    {
        private static KeyboardDataHolder instance;

        private KeyboardDataHolder()
        {

        }

        public static KeyboardDataHolder Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new KeyboardDataHolder();
                }
                return instance;
            }
        }
    }

    public class TouchDeviceDataHolder : DataHolder<TouchDevice>
    {
        private static TouchDeviceDataHolder instance;

        private TouchDeviceDataHolder()
        {

        }

        public static TouchDeviceDataHolder Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TouchDeviceDataHolder();
                }
                return instance;
            }
        }
    }
}
