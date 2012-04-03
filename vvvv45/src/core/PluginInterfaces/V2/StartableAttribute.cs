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
        public StartableAttribute() : base(typeof(IStartable)) { }

        /// <summary>
        /// Friendly name for Startable Elements
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// If Lazy, it will only start when assemly loads, 
        /// otherwise it will force assmebly to load on startup
        /// </summary>
        public bool Lazy { get; set; }
    }
}