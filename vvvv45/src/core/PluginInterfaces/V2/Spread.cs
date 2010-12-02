using System;
using System.Collections;
using System.Collections.Generic;

using VVVV.Utils.VMath;

namespace VVVV.PluginInterfaces.V2
{
	/// <summary>
	/// The spread data type
	/// </summary>
	public class Spread<T> : ISpread<T>
	{
		protected T[] FData;
		protected int FSliceCount;
		
		public Spread(int size)
		{
			FData = new T[size];
			FSliceCount = size;
		}
		
		public Spread(List<T> original)
		{
			FData = new T[original.Count];
			original.CopyTo(FData);	
			FSliceCount = original.Count;
		}
		
		public Spread(ISpread<T> original)
			: this(original.SliceCount)
		{
			for (int i = 0; i < SliceCount; i++)
				FData[i] = original[i];
		}
		
		public virtual T this[int index] 
		{
			get 
			{
				return FData[VMath.Zmod(index, FSliceCount)];
			}
			set 
			{
				FData[VMath.Zmod(index, FSliceCount)] = value;
			}
		}
		
		public virtual int SliceCount 
		{
			get
			{
				return FSliceCount;
			}
			set
			{
				if (FSliceCount != value)
				{
					var old = FData;
					FData = new T[value];
					
					if (old.Length > 0)
					{
						for (int i = 0; i < FData.Length; i++)
						{
							FData[i] = old[i % old.Length];
						}
					}
					
					FSliceCount = value;
				}
			}
		}
		
		public IEnumerator<T> GetEnumerator()
		{
			for (int i=0; i<FSliceCount; i++)
				yield return this[i];
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

    public static class SpreadExtensions_
    {                       
        public static ISpread<T> ToSpread<T>(this List<T> list)
        {
        	return new Spread<T>(list);
        }
     }
}