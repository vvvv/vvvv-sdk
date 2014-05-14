using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CefGlue;

namespace VVVV.Nodes.Texture.HTML
{
    class App : CefApp
    {
        public const string CEF_SCHEME_NAME = "cef";

        protected override void OnRegisterCustomSchemes(CefSchemeRegistrar registrar)
        {
            if (!registrar.AddCustomScheme(CEF_SCHEME_NAME, false, true, false)) throw new Exception(string.Format("Couldn't register custom scheme '{0}'.", CEF_SCHEME_NAME));
        }
    }
}
