using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Utils.IO;

namespace VVVV.Lib
{

    public class MouseStateDataHolder : DataHolder<MouseState>
    {
        private static MouseStateDataHolder instance;

        private MouseStateDataHolder()
        {

        }

        public static MouseStateDataHolder Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MouseStateDataHolder();
                }
                return instance;
            }
        }
    }

    public class KeyStateDataHolder : DataHolder<KeyboardState>
    {
        private static KeyStateDataHolder instance;

        private KeyStateDataHolder()
        {

        }

        public static KeyStateDataHolder Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new KeyStateDataHolder();
                }
                return instance;
            }
        }
    }
}
