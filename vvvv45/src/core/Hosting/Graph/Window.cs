using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;
using VVVV.Utils;
using VVVV.Hosting.Interfaces;
using VVVV.Hosting.Factories;
using VVVV.Hosting.IO;

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
    
    public class Window : WrapperBase, IWindow2
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

        public IWindow InternalCOMInterf
        {
            get { return FNativeWindow; }
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
                var nativeNode = FNativeWindow.GetNode();
                if (nativeNode != null)
                    return VVVV.Hosting.Graph.Node.Create(nativeNode, ProxyNodeInfoFactory.Instance);
                else
                    return null;
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

        public IUserInputWindow UserInputWindow
        {
            get
            {
                var window = InternalCOMInterf;
                var inputWindow = window as IUserInputWindow;
                if (inputWindow != null)
                    return inputWindow;
                // Special treatment for plugins
                var pluginHost = window.GetNode() as IInternalPluginHost;
                if (pluginHost != null)
                {
                    inputWindow = pluginHost.Plugin as IUserInputWindow;
                    if (inputWindow != null)
                        return inputWindow;
                    var pluginContainer = pluginHost.Plugin as PluginContainer;
                    if (pluginContainer != null)
                        return pluginContainer.PluginBase as IUserInputWindow;
                }
                return null;
            }
        }
    }
}
