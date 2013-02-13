﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Windows.Forms;
using VVVV.Hosting.Interfaces;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.IO
{
    public class IOFactory : IIOFactory, IInternalPluginHostListener, IDisposable
    {
        private readonly IInternalPluginHost FPluginHost;
        private readonly IIORegistry FIORegistry;
        
        public IOFactory(IInternalPluginHost pluginHost, IIORegistry streamRegistry)
        {
            FPluginHost = pluginHost;
            FIORegistry = streamRegistry;
            // HACK
            if (FPluginHost != null)
            {
                FPluginHost.Subscribe(this);
            }
        }
        
        public void Dispose()
        {
            // HACK
            if (FPluginHost != null)
            {
                FPluginHost.Unsubscribe(this);
            }
            OnDisposing(EventArgs.Empty);
        }
        
        public IPluginHost2 PluginHost
        {
            get
            {
                return FPluginHost;
            }
        }
        
        public IIOContainer CreateIOContainer(IOBuildContext context)
        {
            var io = FIORegistry.CreateIOContainer(this, context);
            if (io == null)
            {
                throw new NotSupportedException(string.Format("Can't create container for build context '{1}'.", context));
            }
            return io;
        }

        public bool CanCreateIOContainer(IOBuildContext context)
        {
            if (!FIORegistry.CanCreate(context))
            {
                var type = context.IOType;
                if (type.IsGenericType)
                {
                    var openGenericType = type.GetGenericTypeDefinition();
                    return FIORegistry.CanCreate(context.ReplaceIOType(openGenericType));
                }
                
                return false;
            }
            
            return true;
        }
        
        public event EventHandler Synchronizing;
        
        internal void OnSynchronizing(EventArgs e)
        {
            if (Synchronizing != null) 
            {
                Synchronizing(this, e);
            }
        }
        
        public event EventHandler Flushing;
        
        internal void OnFlushing(EventArgs e)
        {
            if (Flushing != null) 
            {
                Flushing(this, e);
            }
        }
        
        public event EventHandler<ConfigEventArgs> Configuring;
        
        internal void OnConfiguring(ConfigEventArgs e)
        {
            if (Configuring != null) 
            {
                Configuring(this, e);
            }
        }
        
        public event EventHandler<ConnectionEventArgs> Connected;
        
        protected virtual void OnConnected(ConnectionEventArgs e)
        {
            if (Connected != null) 
            {
                Connected(this, e);
            }
        }
        
        public event EventHandler<ConnectionEventArgs> Disconnected;
        
        protected virtual void OnDisconnected(ConnectionEventArgs e)
        {
            if (Disconnected != null) 
            {
                Disconnected(this, e);
            }
        }
        
        public event EventHandler Created;
        
        internal virtual void OnCreated(EventArgs e)
        {
            if (Created != null) 
            {
                Created(this, e);
            }
        }
        
        public event EventHandler Disposing;
        
        protected virtual void OnDisposing(EventArgs e)
        {
            if (Disposing != null) 
            {
                Disposing(this, e);
            }
        }
        
        void IInternalPluginHostListener.ConnectCB(IPluginIO pluginIO, IPin otherPin)
        {
            OnConnected(new ConnectionEventArgs(pluginIO, otherPin));
        }
        
        void IInternalPluginHostListener.DisconnectCB(IPluginIO pluginIO, IPin otherPin)
        {
            OnDisconnected(new ConnectionEventArgs(pluginIO, otherPin));
        }
    }
}
