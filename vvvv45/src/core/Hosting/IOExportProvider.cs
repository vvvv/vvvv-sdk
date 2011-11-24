using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.Hosting.Interfaces;
using VVVV.Hosting.Pins;
using VVVV.Hosting.Streams;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting
{
	[ComVisible(false)]
	public class IOExportProvider: ExportProvider
	{
		private readonly IOFactory FIOFactory;
		
		public IOExportProvider(IOFactory ioFactory)
		{
			FIOFactory = ioFactory;
		}
		
		protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
		{
			var contractName = definition.ContractName;
			
			if (contractName == typeof(IOFactory).FullName)
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
					
					if (FIOFactory.CanCreateIO(typeToExport, ioAttribute))
					{
						yield return new Export(contractName, () => FIOFactory.CreateRawIOObject(typeToExport, ioAttribute));
					}
				}
			}
		}
	}
}
