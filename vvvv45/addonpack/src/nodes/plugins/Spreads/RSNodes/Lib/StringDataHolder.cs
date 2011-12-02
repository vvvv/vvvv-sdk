using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Lib
{
    public class StringDataHolder : DataHolder<string>
    {
        private static StringDataHolder instance;

        private StringDataHolder()
        {

        }

        public static StringDataHolder Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new StringDataHolder();
                }
                return instance;
            }
        }
    }
}
