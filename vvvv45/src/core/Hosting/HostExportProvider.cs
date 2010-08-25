using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Diagnostics;
using System.Reflection;

using MefContrib.Hosting.Generics;
using VVVV.Core;
using VVVV.Hosting.Pins;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting
{
	public class HostExportProvider: ExportProvider
	{
		public IPluginHost PluginHost { get; set; }
		
		protected override System.Collections.Generic.IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
		{
			var contractName = definition.ContractName;
			
			//pins
			if (contractName.StartsWith("VVVV.PluginInterfaces.V2.ISpread") || contractName.StartsWith("VVVV.PluginInterfaces.V2.IDiffSpread"))
			{
				var type = TypeHelper.GetImportDefinitionType(definition);
				var spreadType = type.GetGenericArguments()[0];
				Type subSpreadType = null;
				if (spreadType.IsGenericType) subSpreadType = spreadType.GetGenericArguments()[0];
				var attributes = GetImportDefinitionAttributes(definition);
				
				var spreadTypeName = spreadType.Name;
				
				foreach (var attribute in attributes)
				{
					if (attribute is PinAttribute)
					{
						Type pinType = null;
						
						if (spreadTypeName.StartsWith("ISpread`1"))
						{
							if (type.Name.StartsWith("ISpread"))
								pinType = typeof(SpreadListWrapper<>).MakeGenericType(subSpreadType);
							else if (type.Name.StartsWith("IDiffSpread"))
								pinType = typeof(DiffSpreadListWrapper<>).MakeGenericType(subSpreadType);
						}
						else
						{
							if (attribute is InputAttribute)
							{
								if (contractName.StartsWith("VVVV.PluginInterfaces.V2.ISpread"))
									pinType = typeof(InputWrapperPin<>).MakeGenericType(spreadType);
								else
									pinType = typeof(DiffInputWrapperPin<>).MakeGenericType(spreadType);
							}
							else if (attribute is OutputAttribute)
							{
								pinType = typeof(OutputWrapperPin<>).MakeGenericType(spreadType);
							}
							else if (attribute is ConfigAttribute)
							{
								pinType = typeof(ConfigWrapperPin<>).MakeGenericType(spreadType);
							}
						}
						
						if (pinType != null)
							yield return new Export(definition.ContractName, () => Activator.CreateInstance(pinType, new object[] { PluginHost, attribute }));
					}
				}
			}
			else if (contractName == typeof(IPluginHost).FullName)
				yield return new Export(typeof(IPluginHost).FullName, () => PluginHost);
			else if (contractName.StartsWith("VVVV.PluginInterfaces.V1"))
			{
				var attributes = GetImportDefinitionAttributes(definition);
				
				InputAttribute inputAttribute = null;
				OutputAttribute outputAttribute = null;
				ConfigAttribute configAttribute = null;
				
				foreach (var attribute in attributes)
				{
					if (attribute is InputAttribute)
						inputAttribute = attribute as InputAttribute;
					else if (attribute is OutputAttribute)
						outputAttribute = attribute as OutputAttribute;
					else if (attribute is ConfigAttribute)
						configAttribute = attribute as ConfigAttribute;
				}
				
				if (contractName == typeof(IDXLayerIO).FullName && outputAttribute != null)
					yield return new Export(definition.ContractName, () =>
					                        {
					                        	IDXLayerIO pin;
					                        	PluginHost.CreateLayerOutput(outputAttribute.Name, (TPinVisibility)outputAttribute.Visibility, out pin);
					                        	return pin;
					                        });
				else if (contractName == typeof(IDXMeshOut).FullName && outputAttribute != null)
					yield return new Export(definition.ContractName, () =>
					                        {
					                        	IDXMeshOut pin;
					                        	PluginHost.CreateMeshOutput(outputAttribute.Name, (TSliceMode)outputAttribute.SliceMode, (TPinVisibility)outputAttribute.Visibility, out pin);
					                        	return pin;
					                        });
				else if (contractName == typeof(IDXTextureOut).FullName && outputAttribute != null)
					yield return new Export(definition.ContractName, () =>
					                        {
					                        	IDXTextureOut pin;
					                        	PluginHost.CreateTextureOutput(outputAttribute.Name, (TSliceMode)outputAttribute.SliceMode, (TPinVisibility)outputAttribute.Visibility, out pin);
					                        	return pin;
					                        });
				else if (contractName == typeof(IDXRenderStateIn).FullName && inputAttribute != null)
					yield return new Export(definition.ContractName, () =>
					                        {
					                        	IDXRenderStateIn pin;
					                        	PluginHost.CreateRenderStateInput((TSliceMode)inputAttribute.SliceMode, (TPinVisibility)inputAttribute.Visibility, out pin);
					                        	return pin;
					                        });
				else if (contractName == typeof(IDXSamplerStateIn).FullName && inputAttribute != null)
					yield return new Export(definition.ContractName, () =>
					                        {
					                        	IDXSamplerStateIn pin;
					                        	PluginHost.CreateSamplerStateInput((TSliceMode)inputAttribute.SliceMode, (TPinVisibility)inputAttribute.Visibility, out pin);
					                        	return pin;
					                        });
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
	}
}
