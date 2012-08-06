//using System;
//using System.Collections.Generic;
//using System.ComponentModel.Composition.Hosting;
//using System.ComponentModel.Composition.Primitives;
//
//using VVVV.Hosting.Streams;
//using VVVV.PluginInterfaces.V2;
//using VVVV.Utils.Streams;
//
//namespace VVVV.Hosting
//{
//	class StreamExportProvider : ExportProvider, IDisposable
//	{
//		private readonly IOFactory FStreamFactory;
//		
//		public StreamExportProvider(IOFactory streamFactory)
//		{
//			FStreamFactory = streamFactory;
//		}
//		
//		protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
//		{
//			var contractName = definition.ContractName;
//			
//			if (contractName.StartsWith("VVVV.Utils.Streams"))
//			{
//				var typeToExport = ExportProviderUtils.GetImportDefinitionType(definition);
//				
//				foreach (var attribute in ExportProviderUtils.GetImportDefinitionAttributes(definition))
//				{
//					if (!(attribute is IOAttribute)) continue;
//					
//					if (typeToExport.IsGenericType)
//					{
//						var genericArgumentType = typeToExport.GetGenericArguments()[0];
//						
//						// ISpread<T>
//						if (typeof(IStream).IsAssignableFrom(typeToExport))
//						{
//							yield return new Export(contractName, () => FStreamFactory.CreateStream(genericArgumentType, attribute as IOAttribute));
//						}
//						
//						yield break;
//					}
//				}
//			}
//		}
//		
//		public void Dispose()
//		{
//			FStreamFactory.Dispose();
//		}
//	}
//}
