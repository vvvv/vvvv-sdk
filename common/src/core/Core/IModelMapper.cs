using System;

namespace VVVV.Core
{
    /// <summary>
    /// A IModelMapper provides mappings for a model element.
    /// </summary>
    public interface IModelMapper : IDisposable
    {
        /// <summary>
        /// The model element this mapper provides mappings for.
        /// </summary>
        object Model { get; }

        /// <summary>
        /// Maps the containing model element to TTo.
        /// </summary>
        TTo Map<TTo>();
        
        /// <summary>
        /// Whether the containing model element can be mapped to TTo.
        /// </summary>
        bool CanMap<TTo>();
        
        /// <summary>
        /// Creates a new mapper for the specified model element which inherits
        /// all mappings from this mapper.
        /// </summary>
        /// <param name="model">The model element to create a new mapper for.</param>
        /// <returns>A new model mapper which wraps itself around the specified model element.</returns>
        IModelMapper CreateChildMapper(object model);
        
        /// <summary>
        /// Registers mapping from Type fromType to instance toInstance.
        /// The mapping so only valid for this model class and subclasses.
        /// </summary>
        void RegisterMapping(Type fromType, object toInstance);
        
        /// <summary>
        /// Registers mapping from Type fromType to instance toInstance.
        /// The mapping is valid for this model object and child objects.
        /// </summary>
        void RegisterDefault(Type fromType, object toInstance);
        
        /// <summary>
        /// Registers mapping for Type forType from Type fromType to Type toType.
        /// </summary>
        void RegisterMapping(Type forType, Type fromType, Type toType);
    }
    
    public static class ModelMapperExtensions
    {
        /// <summary>
        /// Registers mapping from TInterface to instance.
        /// </summary>
        public static void RegisterMapping<TInterface>(this IModelMapper mapper, TInterface instance)
        {
            mapper.RegisterMapping(typeof(TInterface), instance);
        }
        
        public static void RegisterDefault<TInterface>(this IModelMapper mapper, TInterface instance)
        {
            mapper.RegisterDefault(typeof(TInterface), instance);
        }
        
        public static void RegisterMapping<TFor, TFrom, TTo>(this IModelMapper mapper)
        {
        	mapper.RegisterMapping(typeof(TFor), typeof(TFrom), typeof(TTo));
        }
    }
}
