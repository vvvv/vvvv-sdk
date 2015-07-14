using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Runtime.InteropServices;

using VVVV.Core.Logging;
using VVVV.Hosting.Interfaces;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting
{
    [ComVisible(false)]
    public class HostExportProvider: ExportProvider
    {
        public IInternalPluginHost PluginHost
        {
        	get;
        	set;
        }
        
        protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
        {
            var contractName = definition.ContractName;
            
            if (contractName == typeof(ILogger).FullName)
            {
                var logger = new DefaultLogger();
                logger.AddLogger(new PluginLogger(PluginHost));
                yield return new Export(contractName, () => logger);
            }
            else if (contractName.StartsWith("VVVV.PluginInterfaces"))
            {
                var typeToExport = ExportProviderUtils.GetImportDefinitionType(definition);
                
                if (typeof(IPluginHost).IsAssignableFrom(typeToExport) ||
                    typeof(IPluginHost2).IsAssignableFrom(typeToExport) ||
                    typeof(INode).IsAssignableFrom(typeToExport))
                {
                    yield return new Export(contractName, () => PluginHost);
                    yield break;
                }
            }
        }
    }
}
