using System;
using System.Collections.Generic;
using System.Text;
using VVVV.Utils.VColor;

namespace VVVV.Lib
{
    public class ColorDataHolder : DataHolder<RGBAColor>
    {
        private static ColorDataHolder instance;

        private ColorDataHolder()
        {

        }

        public static ColorDataHolder Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ColorDataHolder();
                }
                return instance;
            }
        }
    }
}
