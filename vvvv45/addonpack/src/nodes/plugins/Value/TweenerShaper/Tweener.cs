/*
 * Created by SharpDevelop.
 * User: TF
 * Date: 06.08.2008
 * Time: 00:42
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;

namespace VVVV.Nodes
{
	/// <summary>
	/// Description of Tweener.
	/// </summary>
	public sealed class Tweener
	{	
		
		// -= QUADRATIC EASING =-
		public static double QuadEaseIn(double X)
		{
			return X*X;
		}
		
		public static double QuadEaseOut(double X)
		{
			return -(X * (X - 2));
		}
		
		public static double QuadEaseInOut(double X)
		{
			if (X <= 0.5)
			{
				X = X * 2;
				X = (X * X);
				return X / 2;
			}
			else
			{
				X = (X - 0.5) * 2;
				X = -(X * (X - 2));
				return (X / 2) + 0.5;
			}
		}
		
		public static double QuadEaseOutIn(double X)
		{
			if (X <= 0.5)
			{
				X = X * 2;
				X = -(X * (X - 2));
				return X / 2;
			}
			else
			{
				X = (X - 0.5) * 2;
				X = X * X;
				return (X / 2) + 0.5;
			}
		}
		
		//...cubic
		
	}
}
