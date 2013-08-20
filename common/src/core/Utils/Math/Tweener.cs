/*
 * 
 * the c# vvvv math library
 * 
 * 
 */

using System;

namespace VVVV.Utils.VMath
{
	/// <summary>
	/// Tweener routines, interpolation functions for a value in the range [0..1] in various shapes
	/// 
	/// Code by west
	/// </summary>
	public sealed class Tweener
	{	
		
		// -= QUADRATIC EASING =-
		
		/// <summary>
		/// QUADRATIC EASE IN
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double QuadEaseIn(double X)
		{
			return X*X;
		}
		
		/// <summary>
		/// QUADRATIC EASE OUT
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double QuadEaseOut(double X)
		{
			return -(X * (X - 2));
		}
		
		/// <summary>
		/// QUADRATIC EASE IN/OUT
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double QuadEaseInOut(double X)
		{
			if (X <= 0.5)
			{
				X = X * 2;
				X = Tweener.QuadEaseIn (X);
				return X / 2;
			}
			else
			{
				X = (X - 0.5) * 2;
				X = Tweener.QuadEaseOut (X);
				return (X / 2) + 0.5;
			}
		}
		
		/// <summary>
		/// QUADRATIC EASE OUT/IN
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double QuadEaseOutIn(double X)
		{
			if (X <= 0.5)
			{
				X = X * 2;
				X = Tweener.QuadEaseOut (X);
				return X / 2;
			}
			else
			{
				X = (X - 0.5) * 2;
				X = Tweener.QuadEaseIn (X);
				return (X / 2) + 0.5;
			}
		}
		
		// -= CUBIC EASING =-
		
		/// <summary>
		/// CUBIC EASE IN
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double CubicEaseIn (double X)
		{
			return X * X * X;
		}
		
		/// <summary>
		/// CUBIC EASE OUT
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double CubicEaseOut (double X)
		{
			X = X - 1;
			return (X * X * X) + 1;
		}
		
		/// <summary>
		/// CUBIC EASE IN/OUT
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double CubicEaseInOut (double X)
		{			
			if (X <= 0.5)
			{
				X = X * 2;
				X = Tweener.CubicEaseIn (X);
				return X / 2;
			}
			else
			{
				X = (X - 0.5) * 2;
				X = Tweener.CubicEaseOut (X);
				return (X / 2) + 0.5;
			}
		}
		
		/// <summary>
		/// CUBIC EASE OUT/IN
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double CubicEaseOutIn (double X)
		{ 
			if (X <= 0.5)
			{
				X = X * 2;
				X = Tweener.CubicEaseOut (X);
				return  X / 2;
			 }
			 else  
			 {
			   	X = (X - 0.5) * 2;
				X = Tweener.CubicEaseIn (X);
				return  (X / 2) + 0.5;
			 }		
		}

		// -= QUARTIC EASING =-
		
		/// <summary>
		/// QUARTIC EASE IN
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double QuarticEaseIn (double X)
		{
			return X * X * X * X;
		}
		
		/// <summary>
		/// QUARTIC EASE OUT
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double QuarticEaseOut (double X)
		{
			X = X - 1; 
			X = (X * X * X * X) - 1; 
			return  X * -1;
		}
		
		/// <summary>
		/// QUARTIC EASE IN/OUT
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double QuarticEaseInOut (double X)
		{			
			if (X <= 0.5)
			{
				X = X * 2;
				X = Tweener.QuarticEaseIn (X);
				return X / 2;
			}
			else
			{
				X = (X - 0.5) * 2;
				X = Tweener.QuarticEaseOut (X);
				return (X / 2) + 0.5;
			}
		}
		
		/// <summary>
		/// QUARTIC EASE OUT/IN
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>		
		public static double QuarticEaseOutIn (double X)
		{ 
			if (X <= 0.5)
			{
				X = X * 2;
				X = Tweener.QuarticEaseOut (X);
				return  X / 2;
			 }
			 else  
			 {
			   	X = (X - 0.5) * 2;
				X = Tweener.QuarticEaseIn (X);
				return  (X / 2) + 0.5;
			 }	
		}
		
		// -= QUINTYIC EASING =-

	   	/// <summary>
		/// QUINTYIC EASE IN
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double QuinticEaseIn (double X) 
		{
		   	return X * X * X * X * X;
		}

		/// <summary>
		/// QUINTYIC EASE OUT
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double QuinticEaseOut (double X)
		{
			X = X - 1; 
			X = X * X * X * X * X; 
			return X + 1;
		}		  
		
		/// <summary>
		/// QUINTYIC EASE IN/OUT
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double QuinticEaseInOut (double X)
		{
			if (X <= 0.5)
			{
			 	X = X * 2;
			 	X = Tweener.QuinticEaseIn (X);
				return X / 2;
	  		}
			else
			{
		 		X = (X - 0.5) * 2;
		 		X = Tweener.QuinticEaseOut (X);
				return (X / 2) + 0.5;
		  	}
		}
		
		/// <summary>
		/// QUINTYIC EASE OUT/IN
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double QuinticEaseOutIn (double X) 
		{
			if (X <= 0.5)
			{
				X = X * 2;
				X = Tweener.QuinticEaseOut (X);
				return X / 2;
			}
			else
		 	{
				X = (X - 0.5) * 2;
				X = Tweener.QuinticEaseIn (X);
				return (X / 2) + 0.5;
			}
	  	}
			   
		// -= SINUSOIDAL EASING =-
		
		/// <summary>
		/// SINUSOIDAL EASE IN
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
	  	public static double SinusoidalEaseIn (double X)
		{
			return -1 * Math.Cos(X * (Math.PI / 2)) + 1;
		}

	  	/// <summary>
		/// SINUSOIDAL EASE OUT
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
	  	public static double SinusoidalEaseOut (double X)
	  	{
	   		X = Math.Sin(X * (Math.PI / 2));
			return X;		
		}
		
	  	/// <summary>
		/// SINUSOIDAL EASE IN/OUT
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double SinusoidalEaseInOut (double X)
		{
			if (X <= 0.5)
			{
			 	X = X * 2;
			 	X = Tweener.SinusoidalEaseIn (X);
				return X / 2;
	  		}
			else
			{
		 		X = (X - 0.5) * 2;
		 		X = Tweener.SinusoidalEaseOut (X);
				return (X / 2) + 0.5;
		  	}
		}
		
		/// <summary>
		/// SINUSOIDAL EASE OUT/IN
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double SinusoidalEaseOutIn (double X) 
		{
			if (X <= 0.5)
			{
				X = X * 2;
				X = Tweener.SinusoidalEaseOut (X);
				return X / 2;
			}
			else
		 	{
				X = (X - 0.5) * 2;
				X = Tweener.SinusoidalEaseIn (X);
				return (X / 2) + 0.5;
			}
	  	}
		
		// -= Exponential Easing =-
		
		/// <summary>
		/// EXPONENTIAL EASE IN
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double ExponentialEaseIn (double X)
		{	 
			return Math.Pow(2, 10 * (X - 1)) - 0.001;
		}
		
		/// <summary>
		/// EXPONENTIAL EASE OUT
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double ExponentialEaseOut (double X)
		{
	 		return 1.001 * (-Math.Pow(2, -10 * X) + 1);
		}
		
		/// <summary>
		/// EXPONENTIAL EASE IN/OUT
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double ExponentialEaseInOut (double X)
		{
			if (X <= 0.5)
			{
			 	X = X * 2;
			 	X = Tweener.ExponentialEaseIn (X);
				return X / 2;
	  		}
			else
			{
		 		X = (X - 0.5) * 2;
		 		X = Tweener.ExponentialEaseOut (X);
				return (X / 2) + 0.5;
		  	}
		}
		
		/// <summary>
		/// EXPONENTIAL EASE OUT/IN
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double ExponentialEaseOutIn (double X) 
		{
			if (X <= 0.5)
			{
				X = X * 2;
				X = Tweener.ExponentialEaseOut (X);
				return X / 2;
			}
			else
		 	{
				X = (X - 0.5) * 2;
				X = Tweener.ExponentialEaseIn (X);
				return (X / 2) + 0.5;
			}
		}
		
		// -= CIRCULAR EASING =-
		 
		/// <summary>
		/// CIRCULAR EASE IN
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double CircularEaseIn (double X)
		{
			return -1 * (Math.Sqrt(1 - (X * X)) - 1);
		}
		
		/// <summary>
		/// CIRCULAR EASE OUT
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double CircularEaseOut (double X)
	  	{
	   		return Math.Sqrt(1 - (X - 1) * (X - 1));
		}
		
		/// <summary>
		/// CIRCULAR EASE IN/OUT
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double CircularEaseInOut (double X)
		{
			if (X <= 0.5)
			{
			 	X = X * 2;
			 	X = Tweener.CircularEaseIn (X);
				return X / 2;
	  		}
			else
			{
		 		X = (X - 0.5) * 2;
		 		X = Tweener.CircularEaseOut (X);
				return (X / 2) + 0.5;
		  	}
		}
		
		/// <summary>
		/// CIRCULAR EASE OUT/IN
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>	
		/// <returns>Shaped value</returns>			
		public static double CircularEaseOutIn (double X) 
		{
			if (X <= 0.5)
			{
				X = X * 2;
				X = Tweener.CircularEaseOut (X);
				return X / 2;
			}
			else
		 	{
				X = (X - 0.5) * 2;
				X = Tweener.CircularEaseIn (X);
				return (X / 2) + 0.5;
			}
		}	   
		
		// -= ELASTIC EASING =-
		
		/// <summary>
		/// ELASTIC EASE IN
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double ElasticEaseIn (double X)
		{
			return (-1 * Math.Pow(2, 10 * (X - 1)) * Math.Sin(((X - 1) - 0.075) * (2 * Math.PI) / 0.3));
		}
		
		/// <summary>
		/// ELASTIC EASE OUT
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double ElasticEaseOut (double X)
		{
			return 1 * Math.Pow(2, -10 * X) * Math.Sin((X - 0.075) * (2 * Math.PI) / 0.3) + 1;
	   	}
		
		/// <summary>
		/// ELASTIC EASE IN/OUT
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
	  	public static double ElasticEaseInOut (double X)
		{
	   		if (X <= 0.5)
			{
			 	X = X * 2;
			 	X = Tweener.ElasticEaseIn (X);
				return X / 2;
	  		}
			else
			{
		 		X = (X - 0.5) * 2;
		 		X = Tweener.ElasticEaseOut (X);
				return (X / 2) + 0.5;
		  	}
		}
		
	  	/// <summary>
		/// ELASTIC EASE OUT/IN
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double ElasticEaseOutIn (double X) 
		{
			if (X <= 0.5)
			{
				X = X * 2;
				X = Tweener.ElasticEaseOut (X);
				return X / 2;
			}
			else
		 	{
				X = (X - 0.5) * 2;
				X = Tweener.ElasticEaseIn (X);
				return (X / 2) + 0.5;
			}
		}	  
		
	   	// -= BACK EASING =-
	   	
	   	/// <summary>
		/// BACK EASE IN
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
	   	public static double BackEaseIn (double X)
	   	{
			return X * X * ((1.7016 + 1) * X - 1.7016);
		}

	   	/// <summary>
		/// BACK EASE OUT
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
	   	public static double BackEaseOut (double X)
	   	{
			return (X - 1) * (X - 1) * ((1.7016 + 1) * (X - 1) + 1.7016) + 1;
		}
	   	
	   	/// <summary>
		/// BACK EASE IN/OUT
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
	 	public static double BackEaseInOut (double X)
		{
	   		if (X <= 0.5)
			{
			 	X = X * 2;
			 	X = Tweener.BackEaseIn (X);
				return X / 2;
	  		}
			else
			{
		 		X = (X - 0.5) * 2;
		 		X = Tweener.BackEaseOut (X);
				return (X / 2) + 0.5;
		  	}
		}
		
	 	/// <summary>
		/// BACK EASE OUT/IN
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double BackEaseOutIn (double X) 
		{
			if (X <= 0.5)
			{
				X = X * 2;
				X = Tweener.BackEaseOut (X);
				return X / 2;
			}
			else
		 	{
				X = (X - 0.5) * 2;
				X = Tweener.BackEaseIn (X);
				return (X / 2) + 0.5;
			}
		}   
		
		// -= BOUNCE EASING =- 
		
		/// <summary>
		/// BOUNCE EASE IN
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double BounceEaseIn (double X)
		{
			X = 1 - X;
			if (X < 1 / 2.75)
				X = 7.5625 * X * X;
			else if (X < 2 / 2.75)
			{
		   		X = X - (1.5 / 2.75);
				X = 7.5625 * X * X + 0.75;
			}
			else if (X < 2.5 / 2.75)
			{
				X = X - (2.25 / 2.75);
				X = 7.5625 * X * X + 0.9375;
			}
			else
			{
				X = X - (2.625 / 2.75);
				X = 7.5625 * X * X + 0.984375;
			}
			return 1 - X;
	   	}
		
		/// <summary>
		/// BOUNCE EASE OUT
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double BounceEaseOut (double X)
		{
			if (X < 1/2.75)
				X = 7.5625 * X * X;
			else if (X < 2/2.75)
			{
				X = X - (1.5 / 2.75);
				X = 7.5625 * X * X + 0.75;
		  	}
			else if (X < 2.5/2.75)
			{
				X = X - (2.25 / 2.75);
				X = 7.5625 * X * X + 0.9375;
		  	}   
			else 
			{ 
				X = X - (2.625 / 2.75);
				X = 7.5625 * X * X + 0.984375;
			}
			return X;
	 	}
		
		/// <summary>
		/// BOUNCE EASE IN/OUT
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double BounceEaseInOut (double X)
		{
	   		if (X <= 0.5)
			{
			 	X = X * 2;
			 	X = Tweener.BounceEaseIn (X);
				return X / 2;
	  		}
			else
			{
		 		X = (X - 0.5) * 2;
		 		X = Tweener.BounceEaseOut (X);
				return (X / 2) + 0.5;
		  	}
		}
		
		/// <summary>
		/// BOUNCE EASE OUT/IN
		/// </summary>
		/// <param name="X">Value in the range [0..1]</param>
		/// <returns>Shaped value</returns>
		public static double BounceEaseOutIn (double X) 
		{
			if (X <= 0.5)
			{
				X = X * 2;
				X = Tweener.BounceEaseOut (X);
				return X / 2;
			}
			else
		 	{
				X = (X - 0.5) * 2;
				X = Tweener.BounceEaseIn (X);
				return (X / 2) + 0.5;
			}
		}   



					

	}
}
