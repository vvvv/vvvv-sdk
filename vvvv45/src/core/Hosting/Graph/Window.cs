using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;
using VVVV.Utils;

namespace VVVV.Hosting.Graph
{
    // TODO: Move this somewhere else.
    public abstract class WrapperBase : MarshalByRefObject
    {
        private readonly object m_comObject;
        
        protected WrapperBase(object value)
        {
            Debug.Assert(value != null);
            m_comObject = value;
        }
        
        public override bool Equals(object obj)
        {
            WrapperBase other = obj as WrapperBase;
            if (other == null)
                return false;
            return object.Equals(this.m_comObject, other.m_comObject);
        }
        
        public override int GetHashCode()
        {
            return m_comObject.GetHashCode();
        }
        
        public static bool operator ==(WrapperBase lhs, WrapperBase rhs)
        {
            if (ReferenceEquals(lhs, rhs))
                return true;
            if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null))
                return false;
            return lhs.Equals(rhs);
        }
        
        public static bool operator !=(WrapperBase lhs, WrapperBase rhs)
        {
            return !(lhs == rhs);
        }
    }
    
    internal class Window : WrapperBase, IWindow2
    {
        #region factory methods
        static internal Window Create(IWindow internalCOMInterf)
        {
            return new Window(internalCOMInterf);
        }
        #endregion
        
        private readonly IWindow FNativeWindow;
        
        private Window(IWindow internalCOMInterf)
            : base(internalCOMInterf)
        {
            FNativeWindow = internalCOMInterf;
        }
        
        public string Caption
        {
            get
            {
                return FNativeWindow.Caption;
            }
            set
            {
                FNativeWindow.Caption = value;
            }
        }
        
        public WindowType WindowType
        {
            get
            {
                return FNativeWindow.GetWindowType();
            }
        }
        
        public INode2 Node
        {
            get
            {
                return VVVV.Hosting.Graph.Node.Create(FNativeWindow.GetNode(), ProxyNodeInfoFactory.Instance);
            }
        }
        
        public bool IsVisible
        {
            get
            {
                return FNativeWindow.IsVisible();
            }
        }
        
        public Rectangle Bounds
        {
            get
            {
                return new Rectangle(FNativeWindow.Left, FNativeWindow.Top, FNativeWindow.Width, FNativeWindow.Height);
            }
        }
        
        public IntPtr Handle
        {
            get
            {
                return FNativeWindow.Handle;
            }
        }
        
        public override string ToString()
        {
            return Caption;
        }
        
        public bool Equals(IWindow2 other)
        {
            return base.Equals(other);
        }
    }
}
