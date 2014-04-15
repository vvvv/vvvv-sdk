using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace VVVV.PluginInterfaces.V2
{
   
    /// <summary>
    /// Interface to allow some code to be executed on startup.
    /// </summary>
    [ComVisible(false)]
    public interface IStartable
    {
        /// <summary>
        /// Code to execute once library is loaded.
        /// </summary>
        void Start();

        /// <summary>
        /// Code to execute when vvvv quits.
        /// </summary>
        void Shutdown();
    }
}