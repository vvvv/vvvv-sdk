
using System;
using System.Linq;

namespace VVVV.PluginInterfaces.V2
{
    public abstract class IOBuildContext
    {
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
        
        /// <summary>
        /// Gets or sets the io factory which is used during the build process.
        /// </summary>
        public IIOFactory Factory
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets the io attribute.
        /// </summary>
        public IOAttribute IOAttribute
        {
            get;
            internal set;
        }
        
        /// <summary>
        /// Gets or sets the type of the io object to build.
        /// For example: ISpread{double} or IValueIn.
        /// </summary>
        public Type IOType
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets the data type which the io object should handle.
        /// For example: double or T.
        /// </summary>
        public Type DataType
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets whether or not the io container should subscribe
        /// to the sync and flush events of the io factory.
        /// In most cases only the outer most container will subscribe
        /// to those events as its containing io object will trigger
        /// the sync and flush methods of its inner io objects manually.
        /// </summary>
        public bool SubscribeToIOEvents
        {
            get;
            set;
        }
    }
    
    /// <summary>
    /// An io build context is used during the build process of an io object.
    /// It contains all the necessary information of how to configure
    /// an io object and its container object when creating it.
    /// </summary>
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
            set
            {
                base.IOAttribute = value;
            }
        }
        
        public IOBuildContext<TAttribute> ReplaceIOType(Type ioType)
        {
            return new IOBuildContext<TAttribute>()
            {
                DataType = this.DataType,
                Factory = this.Factory,
                IOAttribute = this.IOAttribute,
                IOType = ioType,
                SubscribeToIOEvents = this.SubscribeToIOEvents
            };
        }
        
        public IOBuildContext<TAttribute> ReplaceDataType(Type dataType)
        {
            return new IOBuildContext<TAttribute>()
            {
                DataType = dataType,
                Factory = this.Factory,
                IOAttribute = this.IOAttribute,
                IOType = this.IOType,
                SubscribeToIOEvents = this.SubscribeToIOEvents
            };
        }
    }
}
