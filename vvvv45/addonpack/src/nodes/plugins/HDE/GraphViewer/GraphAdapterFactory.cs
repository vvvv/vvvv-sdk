using System;
using System.Reflection;
using System.Collections.Generic;
using VVVV.Utils.Adapter;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Graph.Provider
{
	public class GraphAdapterFactory: IAdapterFactory
	{
		private Dictionary<Type, Dictionary<Type, Type>> FLookupTable;
		
		public GraphAdapterFactory()
		{
			var knownMappings = new Dictionary<Type, Type>();
			
			knownMappings.Add(typeof(INode), typeof(PatchNodeContentProvider));
			
			CreateLookupTable(knownMappings);
		}
		
		public T Adapt<T>(object source) where T: class
		{
			Type sourceType = source.GetType();
			Type targetType = typeof(T);
			
			// Do we know that target type?
			if (FLookupTable.ContainsKey(targetType))
			{
				var table = FLookupTable[targetType];
				if (table.ContainsKey(sourceType))
				{
					return CreateInstance<T>(table[sourceType]);
				}
				
				// Maybe we know how to handle an interface implemented by the source?
				foreach (Type interf in sourceType.GetInterfaces())
				{
					if (table.ContainsKey(interf))
					{
						return CreateInstance<T>(table[interf]);
					}
				}
			}
			
			return default(T);
		}
		
		public bool IsFactoryForType<T>() where T: class
		{
			Type targetType = typeof(T);
			return FLookupTable.ContainsKey(targetType);
		}
		
		private void CreateLookupTable(Dictionary<Type, Type> mappings)
		{
			FLookupTable = new Dictionary<Type, Dictionary<Type, Type>>();
			
			foreach (KeyValuePair<Type, Type> pair in mappings)
			{
				var source = pair.Key;
				var target = pair.Value;
				
				FLookupTable[target] = new Dictionary<Type, Type>();
				FLookupTable[target][source] = target;
				
				foreach (Type interf in target.GetInterfaces())
				{
					if (!FLookupTable.ContainsKey(interf))
						FLookupTable[interf] = new Dictionary<Type, Type>();
					
					var table = FLookupTable[interf];
					table[source] = target;
				}
			}
		}
		
		private T CreateInstance<T>(Type target) where T: class
		{
			var targetClassName = target.FullName;
			return target.Assembly.CreateInstance(targetClassName) as T;
		}
	}
}
