﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel.Composition;

namespace VVVV.PluginInterfaces.V2
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    [ComVisible(false)]
    public class StartableAttribute : ExportAttribute
    {
        public bool Lazy { get; set; }
    }
}