using System;
using System.Drawing;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;

namespace VVVV.Hosting.Graph
{
    internal class Window : IWindow2
    {
        private readonly INode2 FOwnerNode;
        private readonly IWindow FInternalCOMInterf;
        
        internal Window(INode2 ownerNode, IWindow internalCOMInterf)
        {
            FOwnerNode = ownerNode;
            FInternalCOMInterf = internalCOMInterf;
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
                return FOwnerNode;
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
    }
}
