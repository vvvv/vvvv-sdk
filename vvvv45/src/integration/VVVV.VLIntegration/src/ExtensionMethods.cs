using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Cci;
using Microsoft.Cci.MutableCodeModel;
using VL.Lang.Platforms.CIL;

namespace VVVV.VL.Hosting
{
    public static class ExtensionMethods
    {
        public static INamedTypeDefinition GetTypeDefinition(this IAssembly assembly, Type type)
        {
            return (
                from t in assembly.GetAllTypes()
                let name = TypeHelper.GetTypeName(t, NameFormattingOptions.UseGenericTypeNameSuffix)
                where name == type.FullName
                select t
               ).First();
        }
        
        public static IMethodDefinition GetMethodDefinition(this ITypeDefinition type, IName name)
        {
            var result = type.Methods.FirstOrDefault(method => method.Name == name);
            if (result != null) return result;
            foreach (var baseType in type.BaseClasses)
            {
                result = baseType.ResolvedType.GetMethodDefinition(name);
                if (result != null) return result;
            }
            if (type.IsInterface)
            {
                foreach (var interf in type.Interfaces)
                {
                    result = interf.ResolvedType.GetMethodDefinition(name);
                    if (result != null) return result;
                }
            }
            throw new Exception(string.Format("Couldn't find method '{0}' in '{1}'.", name, type));
        }
        
        public static IMethodDefinition GetMethodDefinition(this ITypeDefinition type, IName name, int parameterCount)
        {
            var result = type.Methods.FirstOrDefault(method => method.Name == name && method.ParameterCount == parameterCount);
            if (result != null) return result;
            foreach (var baseType in type.BaseClasses)
            {
                result = baseType.ResolvedType.GetMethodDefinition(name, parameterCount);
                if (result != null) return result;
            }
            if (type.IsInterface)
            {
                foreach (var interf in type.Interfaces)
                {
                    result = interf.ResolvedType.GetMethodDefinition(name, parameterCount);
                    if (result != null) return result;
                }
            }
            throw new Exception(string.Format("Couldn't find method '{0}' in '{1}'.", name, type));
        }
        
        public static IMethodDefinition GetMethodDefinition(this ITypeDefinition type, IName name, params ITypeReference[] parameterTypes)
        {
            var result = Helpers.GetMethod(type, name, parameterTypes);
            if (result != null) return result;
            foreach (var baseType in type.BaseClasses)
            {
                result = baseType.ResolvedType.GetMethodDefinition(name, parameterTypes);
                if (result != null) return result;
            }
            if (type.IsInterface)
            {
                foreach (var interf in type.Interfaces)
                {
                    result = interf.ResolvedType.GetMethodDefinition(name, parameterTypes);
                    if (result != null) return result;
                }
            }
            return Dummy.MethodDefinition;
        }
        
        public static IPropertyDefinition GetPropertyDefinition(this ITypeDefinition type, IName name)
        {
            var result = type.Properties.FirstOrDefault(property => property.Name == name);
            if (result != null) return result;
            foreach (var baseType in type.BaseClasses)
            {
                result = baseType.ResolvedType.GetPropertyDefinition(name);
                if (result != null) return result;
            }
            if (type.IsInterface)
            {
                foreach (var interf in type.Interfaces)
                {
                    result = interf.ResolvedType.GetPropertyDefinition(name);
                    if (result != null) return result;
                }
            }
            return Dummy.PropertyDefinition;
        }
        
        public static IFieldDefinition GetFieldDefinition(this ITypeDefinition type, IName name)
        {
            var result = TypeHelper.GetField(type, name);
            if (result != null) return result;
            foreach (var baseType in type.BaseClasses)
            {
                result = baseType.ResolvedType.GetFieldDefinition(name);
                if (result != null) return result;
            }
            if (type.IsInterface)
            {
                foreach (var interf in type.Interfaces)
                {
                    result = interf.ResolvedType.GetFieldDefinition(name);
                    if (result != null) return result;
                }
            }
            return Dummy.FieldDefinition;
        }

        public static IArrayTypeReference MakeArrayType(this ITypeReference elementType, IInternFactory internFactory)
        {
            return Vector45.GetVector(elementType, internFactory);
        }

        public static ITypeReference GetElementType(this ITypeReference type)
        {
            var genericTypeInstanceReference = type as IGenericTypeInstanceReference;
            if (genericTypeInstanceReference != null)
                return genericTypeInstanceReference.GenericArguments.Single();
            var arrayTypeReference = type as IArrayTypeReference;
            if (arrayTypeReference != null)
                return arrayTypeReference.ElementType;
            throw new NotImplementedException();
        }
        
        public static IGenericTypeInstanceReference MakeGenericType(
            this INamedTypeReference genericType,
            IInternFactory internFactory,
            IEnumerable<ITypeReference> genericArguments
           )
        {
            return new Microsoft.Cci.Immutable.GenericTypeInstanceReference(
                genericType,
                genericArguments,
                internFactory
               );
        }
        
        public static IGenericTypeInstanceReference MakeGenericType(
            this INamedTypeReference genericType,
            IInternFactory internFactory,
            params ITypeReference[] genericArguments
           )
        {
            return genericType.MakeGenericType(internFactory, genericArguments as IEnumerable<ITypeReference>);
        }
        
        public static IGenericMethodInstanceReference MakeGenericMethod(
            this IMethodReference genericMethod,
            IInternFactory internFactory,
            IEnumerable<ITypeReference> genericArguments
           )
        {
            return new Microsoft.Cci.Immutable.GenericMethodInstanceReference(
                genericMethod,
                genericArguments,
                internFactory
               );
        }
        
        public static IGenericMethodInstanceReference MakeGenericMethod(
            this IMethodReference genericMethod,
            IInternFactory internFactory,
            params ITypeReference[] genericArguments
           )
        {
            return genericMethod.MakeGenericMethod(internFactory, genericArguments as IEnumerable<ITypeReference>);
        }
        
        public static ILocalDefinition DefineLocal(this IMethodDefinition method, ITypeReference type, IName name)
        {
            return new LocalDefinition()
            {
                MethodDefinition = method,
                Type = type,
                Name = name
            };
        }
    }
}
