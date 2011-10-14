using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Bullet
{
	public class PinReflectionUtils
	{
		public static void RegisterNodeGuids<T>(INodeOut node, string friendlyname)
		{
			List<Guid> result = new List<Guid>();
			AddType(result, typeof(T));
			node.SetSubType(result.ToArray(), friendlyname);
		}

		private static void AddType(List<Guid> uids, Type t)
		{
			uids.Add(t.GUID);
			foreach (Type iface in t.GetInterfaces())
			{
				if (!uids.Contains(iface.GUID))
				{
					uids.Add(iface.GUID);
				}
			}
			if (t.BaseType != null)
			{
				AddType(uids,t.BaseType);
			}
		}
	}
}
