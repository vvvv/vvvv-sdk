using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.IO
{
    [ComVisible(false)]
    class IOExportProvider : ExportProvider
    {
        private readonly IOFactory FIOFactory;
        readonly Dictionary<ImportDefinition, Export> FExports = new Dictionary<ImportDefinition, Export>();
        readonly Dictionary<MemberInfo, Export> FAccessorToExportMap = new Dictionary<MemberInfo, Export>();

        public IOExportProvider(IOFactory ioFactory)
        {
            FIOFactory = ioFactory;
        }

        protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
        {
            Export export;
            if (!FExports.TryGetValue(definition, out export))
            {
                var contractName = definition.ContractName;
                if (contractName == typeof(IIOFactory).FullName)
                    export = new Export(contractName, () => FIOFactory);
                else
                {
                    var typeToExport = ExportProviderUtils.GetImportDefinitionType(definition);

                    foreach (var attribute in ExportProviderUtils.GetImportDefinitionAttributes(definition))
                    {
                        var ioAttribute = attribute as IOAttribute;
                        if (ioAttribute == null) continue;

                        if (!ReflectionModelServices.IsImportingParameter(definition))
                        {
                            var member = ReflectionModelServices.GetImportingMember(definition);
                            if (member.MemberType == MemberTypes.Property)
                            {
                                foreach (var accessor in member.GetAccessors())
                                    if (FAccessorToExportMap.TryGetValue(accessor, out export))
                                        break;
                            }
                        }
                        if (export == null)
                        {
                            var context = IOBuildContext.Create(typeToExport, ioAttribute);
                            if (FIOFactory.CanCreateIOContainer(context))
                            {
                                export = new Export(contractName, () => FIOFactory.CreateIO(context));
                                // Now register the export for all the base members
                                if (!ReflectionModelServices.IsImportingParameter(definition))
                                {
                                    var member = ReflectionModelServices.GetImportingMember(definition);
                                    if (member.MemberType == MemberTypes.Property)
                                        foreach (var accessor in member.GetAccessors().OfType<MethodInfo>())
                                            RegisterExport(accessor, export);
                                }
                            }
                        }
                        if (export != null)
                            break;
                    }
                }
                FExports.Add(definition, export);
            }
            if (export != null)
                yield return export;
        }

        void RegisterExport(MethodInfo methodInfo, Export export)
        {
            FAccessorToExportMap.Add(methodInfo, export);
            var baseMethod = methodInfo.GetBaseDefinition();
            if (baseMethod != null && baseMethod != methodInfo)
                RegisterExport(baseMethod, export);
        }
    }
}
