using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Lib
{
    public class DoubleDataHolder : DataHolder<double>
    {
        private static DoubleDataHolder instance;

        private DoubleDataHolder()
        {

        }

        public static DoubleDataHolder Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DoubleDataHolder();
                }
                return instance;
            }
        }
    }
}
