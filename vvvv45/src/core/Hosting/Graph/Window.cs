using System;
using System.Drawing;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;
using System.Collections.Generic;
using VVVV.Utils;

namespace VVVV.Hosting.Graph
{
    internal class Window : Disposable, IWindow2
    {
		#region factory methods
		static private Dictionary<IWindow, Window> FWindows = new Dictionary<IWindow, Window>();
		static internal Window Create(IWindow internalCOMInterf)
		{
			Window window = null;
			if (!FWindows.TryGetValue(internalCOMInterf, out window))
			{
				window = new Window(internalCOMInterf);
				FWindows.Add(internalCOMInterf, window);
			}
			return window;
		}
		
		static internal IEnumerable<Window> Windows
		{
		    get
		    {
		        return FWindows.Values;
		    }
		}
		#endregion
		
        private readonly IWindow FInternalCOMInterf;
		
        private Window(IWindow internalCOMInterf)
        {
			FInternalCOMInterf = internalCOMInterf;
        }
		
		protected override void DisposeManaged ()
		{
			FWindows.Remove(FInternalCOMInterf);
		}
        
        public string Caption 
        {
            get 
            {
                return FInternalCOMInterf.Caption;
            }
            set 
            {
                FInternalCOMInterf.Caption = value;
            }
        }
        
        public WindowType WindowType 
        {
            get 
            {
                return FInternalCOMInterf.GetWindowType();
            }
        }
        
        public INode2 Node 
        {
            get 
            {
                return VVVV.Hosting.Graph.Node.Create(null, FInternalCOMInterf.GetNode(), ProxyNodeInfoFactory.Instance);
            }
        }
        
        public bool IsVisible 
        {
            get 
            {
                return FInternalCOMInterf.IsVisible();
            }
        }
        
        public Rectangle Bounds
        {
            get 
            {
                return new Rectangle(FInternalCOMInterf.Left, FInternalCOMInterf.Top, FInternalCOMInterf.Width, FInternalCOMInterf.Height);
            }
        }
		
		public IWindow InternalCOMInterf
		{
            get 
            {
                return FInternalCOMInterf;
            }
        }
    }
}
