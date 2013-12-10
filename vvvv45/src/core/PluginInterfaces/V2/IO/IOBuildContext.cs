
using System;
using System.Linq;
using System.Runtime.InteropServices;
using VVVV.Utils.Streams;

namespace VVVV.PluginInterfaces.V2
{
    /// <summary>
    /// An io build context is used during the build process of an io object.
    /// It contains all the necessary information of how to configure
    /// an io object and its container object when creating it.
    /// </summary>
    [ComVisible(false)]
    public abstract class IOBuildContext
    {
        #region static factory methods
        
        public static IOBuildContext<TAttribute> Create<TAttribute>(Type ioType, TAttribute ioAttribute, bool subscribe = true)
            where TAttribute : IOAttribute
        {
            var dataType = ioType.GetGenericArguments().FirstOrDefault();
            return Create(ioType, dataType, ioAttribute);
        }
        
        public static IOBuildContext<TAttribute> Create<TAttribute>(Type ioType, Type dataType, TAttribute ioAttribute, bool subscribe = true)
            where TAttribute : IOAttribute
        {
            return new IOBuildContext<TAttribute>()
            {
                IOType = ioType,
                DataType = dataType,
                IOAttribute = ioAttribute,
                SubscribeToIOEvents = subscribe
            };
        }
        
        public static IOBuildContext Create(Type ioType, IOAttribute attribute, bool subscribe = true)
        {
            var dataType = ioType.GetGenericArguments().FirstOrDefault();
            return Create(ioType, dataType, attribute, subscribe);
        }
        
        public static IOBuildContext Create(Type ioType, Type dataType, IOAttribute attribute, bool subscribe = true)
        {
            var inputAttribute = attribute as InputAttribute;
            if (inputAttribute != null)
                return IOBuildContext.Create(ioType, dataType, inputAttribute, subscribe);
            var outputAttribute = attribute as OutputAttribute;
            if (outputAttribute != null)
                return IOBuildContext.Create(ioType, dataType, outputAttribute, subscribe);
            var configAttribute = attribute as ConfigAttribute;
            if (configAttribute != null)
                return IOBuildContext.Create(ioType, dataType, configAttribute, subscribe);
            return null;
        }
        
        #endregion
        
        /// <summary>
        /// Gets the io attribute.
        /// </summary>
        public IOAttribute IOAttribute
        {
            get;
            internal set;
        }
        
        /// <summary>
        /// Gets the type of the io object to build.
        /// For example: ISpread{double} or IValueIn.
        /// </summary>
        public Type IOType
        {
            get;
            internal set;
        }
        
        /// <summary>
        /// Gets the data type which the io object should handle.
        /// For example: double or T.
        /// </summary>
        public Type DataType
        {
            get;
            internal set;
        }
        
        /// <summary>
        /// Gets whether or not the io container should subscribe
        /// to the sync, flush and dispose events of the io factory.
        /// In most cases only the outer most container will subscribe
        /// to those events as its containing io object will trigger
        /// the sync and flush methods of its inner io objects manually.
        /// </summary>
        public bool SubscribeToIOEvents
        {
            get;
            internal set;
        }

        public IIOContainer BinSizeIOContainer { get; set; }
        
        public IOBuildContext ReplaceIOType(Type ioType)
        {
            return Create(ioType, this.DataType, this.IOAttribute, false);
        }
        
        public IOBuildContext ReplaceDataType(Type dataType)
        {
            return Create(this.IOType, dataType, this.IOAttribute, false);
        }
        
        public PinDirection Direction
        {
            get
            {
                if (IOAttribute is InputAttribute)
                    return PinDirection.Input;
                else if (IOAttribute is OutputAttribute)
                    return PinDirection.Output;
                else
                    return PinDirection.Configuration;
            }
        }
    }
    
    [ComVisible(false)]
    public class IOBuildContext<TAttribute> : IOBuildContext
        where TAttribute : IOAttribute
    {
        internal IOBuildContext()
        {
        }
        
        /// <summary>
        /// Gets or sets the io attribute.
        /// </summary>
        public new TAttribute IOAttribute
        {
            get
            {
                return (TAttribute) base.IOAttribute;
            }
            internal set
            {
                base.IOAttribute = value;
            }
        }
        
        public new IOBuildContext<TAttribute> ReplaceIOType(Type ioType)
        {
            return (IOBuildContext<TAttribute>) base.ReplaceIOType(ioType);
        }
        
        public new IOBuildContext<TAttribute> ReplaceDataType(Type dataType)
        {
            return (IOBuildContext<TAttribute>) base.ReplaceDataType(dataType);
        }
    }
}
