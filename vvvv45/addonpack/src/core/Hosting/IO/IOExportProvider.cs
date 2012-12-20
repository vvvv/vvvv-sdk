using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.IO
{
	[ComVisible(false)]
	class IOExportProvider: ExportProvider
	{
		private readonly IOFactory FIOFactory;
		
		public IOExportProvider(IOFactory ioFactory)
		{
			FIOFactory = ioFactory;
		}
		
		protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
		{
			var contractName = definition.ContractName;
			
			if (contractName == typeof(IIOFactory).FullName)
			{
				yield return new Export(contractName, () => FIOFactory);
			}
			else
			{
				var typeToExport = ExportProviderUtils.GetImportDefinitionType(definition);

				foreach (var attribute in ExportProviderUtils.GetImportDefinitionAttributes(definition))
				{
					var ioAttribute = attribute as IOAttribute;
					if (ioAttribute == null) continue;
					
					var context = IOBuildContext.Create(typeToExport, ioAttribute);
					if (FIOFactory.CanCreateIOContainer(context))
					{
					    yield return new Export(contractName, () => FIOFactory.CreateIO(context));
						yield break;
					}
				}
			}
		}
	}
}
