using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


	public class ArrayMax
	{
		public static int Max(params int[] slices)
		{
			int max = int.MinValue;
			for (int i = 0; i < slices.Length; i++)
			{
				if (slices[i] > max)
				{
					max = slices[i];
				}
			}
			return max;
		}
	}

