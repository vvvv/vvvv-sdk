using System;
using System.Collections.Generic;

namespace BenTools.Data
{
	/// <summary>
	/// Set für effizienten Zugriff auf Objekte. Objete werden als Key abgelegt, value ist nur ein dummy-Objekt.
	/// </summary>
	[Serializable]
	public class HashSet<T> : IEnumerable<T>, ICollection<T>
	{
		Dictionary<T, object> Core;
		static readonly object Dummy = new object();

		public HashSet(IEnumerable<T> source)
			: this()
		{
			AddRange(source);
		}

		public HashSet(IEqualityComparer<T> eqComp)
		{
			Core = new Dictionary<T, object>(eqComp);
		}
		public HashSet()
		{
			Core = new Dictionary<T, object>();
		}

		public bool Add(T o)
		{
			int count = Core.Count;
			Core[o] = Dummy;
			if (count == Core.Count)
				return false;
			else 
				return true;
		}

		public bool Contains(T o)
		{
			return Core.ContainsKey(o);
		}

		public bool Remove(T o)
		{
			return Core.Remove(o);
		}

		[Obsolete]
		public void AddRange(System.Collections.IEnumerable List)
		{
			foreach(T O in List)
				Add(O);
		}

		public void AddRange(IEnumerable<T> List)
		{
			foreach(T O in List)
				Add(O);
		}

		public void Clear()
		{
			Core.Clear();
		}

		#region IEnumerable<T> Members

		public IEnumerator<T> GetEnumerator()
		{
			return Core.Keys.GetEnumerator();
		}

		#endregion

		#region ICollection<T> Members

		public bool IsSynchronized
		{
			get
			{
				return false;
			}
		}

		public int Count
		{
			get
			{
				
				return Core.Count;
			}
		}

		public void CopyTo(T[] array, int index)
		{
			Core.Keys.CopyTo(array,index);
		}

		public bool IsReadOnly
		{
			get { return false; }
		}
		#endregion


		void ICollection<T>.Add(T item)
		{
			Add(item);
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return Core.Keys.GetEnumerator();
		}
	}
}
