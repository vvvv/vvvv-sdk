using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.Hosting.Pins;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting
{
    [ComVisible(false)]
	public class HostExportProvider: ExportProvider
	{
		static Assembly PluginInterfaceAssembly = typeof(IPluginHost).Assembly;
		
		public IPluginHost2 PluginHost { get; set; }
		
		protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
		{
			var contractName = definition.ContractName;
			
			if (contractName == typeof(ILogger).FullName && PluginHost != null)
			{
				yield return new Export(contractName, () => new PluginLogger(PluginHost));
			}
			else if (contractName.StartsWith("VVVV.PluginInterfaces"))
			{
				var typeToExport = GetImportDefinitionType(definition);
				
				if (typeof(IPluginHost).IsAssignableFrom(typeToExport) ||
				    typeof(IPluginHost2).IsAssignableFrom(typeToExport) ||
				    typeof(INode).IsAssignableFrom(typeToExport))
				{
					yield return new Export(contractName, () => PluginHost);
					yield break;
				}
				
				foreach (var attribute in GetImportDefinitionAttributes(definition))
				{
					if (!(attribute is PinAttribute)) continue;
					
					if (typeToExport.IsGenericType)
					{
						var genericArgumentType = typeToExport.GetGenericArguments()[0];
						
						// ISpread<T>
						if (typeof(IDiffSpread<>).MakeGenericType(genericArgumentType).IsAssignableFrom(typeToExport))
						{
							yield return new Export(contractName, () => PinFactory.CreateDiffPin(PluginHost, attribute, genericArgumentType));
						}
						else if (typeof(ISpread<>).MakeGenericType(genericArgumentType).IsAssignableFrom(typeToExport))
						{
							yield return new Export(contractName, () => PinFactory.CreatePin(PluginHost, attribute, genericArgumentType));
						}
						
						yield break;
					}
					else
					{
						var outputAttribute = attribute as OutputAttribute;
						if (outputAttribute != null)
						{
							if (typeof(IDXLayerIO).IsAssignableFrom(typeToExport))
								yield return new Export(
									contractName,
									() =>
									{
										IDXLayerIO pin;
										PluginHost.CreateLayerOutput(outputAttribute.Name, (TPinVisibility)outputAttribute.Visibility, out pin);
										return pin;
									});
							else if (typeof(IDXMeshOut).IsAssignableFrom(typeToExport))
								yield return new Export(
									contractName,
									() =>
									{
										IDXMeshOut pin;
										PluginHost.CreateMeshOutput(outputAttribute.Name, (TSliceMode)outputAttribute.SliceMode, (TPinVisibility)outputAttribute.Visibility, out pin);
										return pin;
									});
							else if (typeof(IDXTextureOut).IsAssignableFrom(typeToExport))
								yield return new Export(
									contractName,
									() =>
									{
										IDXTextureOut pin;
										PluginHost.CreateTextureOutput(outputAttribute.Name, (TSliceMode)outputAttribute.SliceMode, (TPinVisibility)outputAttribute.Visibility, out pin);
										return pin;
									});
							
							yield break;
						}
						
						var inputAttribute = attribute as InputAttribute;
						if (inputAttribute != null)
						{
							if (typeof(IDXRenderStateIn).IsAssignableFrom(typeToExport))
								yield return new Export(
									contractName,
									() =>
									{
										IDXRenderStateIn pin;
										PluginHost.CreateRenderStateInput((TSliceMode)inputAttribute.SliceMode, (TPinVisibility)inputAttribute.Visibility, out pin);
										return pin;
									});
							else if (typeof(IDXSamplerStateIn).IsAssignableFrom(typeToExport))
								yield return new Export(
									contractName,
									() =>
									{
										IDXSamplerStateIn pin;
										PluginHost.CreateSamplerStateInput((TSliceMode)inputAttribute.SliceMode, (TPinVisibility)inputAttribute.Visibility, out pin);
										return pin;
									});
							
							yield break;
						}
					}
				}
			}
		}
		
		private IEnumerable<Attribute> GetImportDefinitionAttributes(ImportDefinition definition)
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
		
		private PropertyInfo FindProperty(LazyMemberInfo member)
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

        private static Type GetMethodType(ImportDefinition definition, Type importDefinitionType)
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

        private static Type GetParameterType(ImportDefinition definition)
        {
            Type importDefinitionType;
            var importingParameter = ReflectionModelServices.GetImportingParameter(definition);
            var parameterInfo = importingParameter.Value;
            importDefinitionType = parameterInfo.ParameterType;
            return importDefinitionType;
        }

	}
}
