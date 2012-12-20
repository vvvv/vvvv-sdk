using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Reflection;

namespace VVVV.Hosting
{
	static class ExportProviderUtils
	{
		public static IEnumerable<Attribute> GetImportDefinitionAttributes(ImportDefinition definition)
        {
            if (ReflectionModelServices.IsImportingParameter(definition))
            {
                var parameter = ReflectionModelServices.GetImportingParameter(definition);
                
                foreach (var attribute in Attribute.GetCustomAttributes(parameter.Value, true))
                {
                    yield return attribute;
                }
            }
            else
            {
                var member = ReflectionModelServices.GetImportingMember(definition);
                
                MemberInfo attributedMember = null;
                switch (member.MemberType)
                {
                    case MemberTypes.Property:
                        attributedMember = FindProperty(member);
                        break;
                    case MemberTypes.Field:
                        attributedMember = member.GetAccessors()[0];
                        break;
                }
                
                if (attributedMember != null)
                {
                    foreach (var attribute in Attribute.GetCustomAttributes(attributedMember, true))
                    {
                        yield return attribute;
                    }
                }
            }
        }
        
        public static PropertyInfo FindProperty(LazyMemberInfo member)
        {
            var accessor = member.GetAccessors()[0];
            var declaringType = accessor.DeclaringType;
            
            foreach (var property in declaringType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var getter = property.GetGetMethod(true);
                var setter = property.GetSetMethod(true);
                if (getter == accessor || setter == accessor)
                {
                    return property;
                }
            }
            
            return null;
        }
        
        public static Type GetImportDefinitionType(ImportDefinition definition)
        {
            Type importDefinitionType = null;
            if (ReflectionModelServices.IsImportingParameter(definition))
                importDefinitionType = GetParameterType(definition);
            else
                importDefinitionType = GetMethodType(definition, importDefinitionType);
            return importDefinitionType;
        }

        public static Type GetMethodType(ImportDefinition definition, Type importDefinitionType)
        {
            var memberInfos = ReflectionModelServices.GetImportingMember(definition).GetAccessors();
            var memberInfo = memberInfos[0];

            if (memberInfo.MemberType == MemberTypes.Method)
            {
                var methodInfo = (MethodInfo) memberInfo;
                importDefinitionType = methodInfo.ReturnType;
            }
            else if (memberInfo.MemberType == MemberTypes.Field)
            {
                var fieldInfo = (FieldInfo) memberInfo;
                importDefinitionType = fieldInfo.FieldType;
            }
            return importDefinitionType;
        }

        public static Type GetParameterType(ImportDefinition definition)
        {
            Type importDefinitionType;
            var importingParameter = ReflectionModelServices.GetImportingParameter(definition);
            var parameterInfo = importingParameter.Value;
            importDefinitionType = parameterInfo.ParameterType;
            return importDefinitionType;
        }
	}
}
