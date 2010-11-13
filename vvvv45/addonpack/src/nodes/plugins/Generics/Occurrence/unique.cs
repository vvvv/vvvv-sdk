using System;
using System.Collections.Generic;

namespace VVVV.Nodes
{
	public class UniqueObj<T>
	{
		private T uni;
		public T Unique
		{
			get { return uni;}
		}
		
		private List<int> occId;
		public int FirstOccurred
		{
			get { return occId[0];}
		}
		public int Count
		{
			get { return occId.Count; }
		}
		
		public UniqueObj(T inValue, int id)
		{
			uni = inValue;
			occId = new List<int>();
			occId.Add(id);
		}
		
		public int AddMember(int id)
		{
			occId.Add(id);
			return occId.Count-1;
		}
	}
}
