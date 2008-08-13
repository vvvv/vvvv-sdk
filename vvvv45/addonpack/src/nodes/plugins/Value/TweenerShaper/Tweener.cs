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
		public static double CubicEaseIn (double X)
		{
			return X * X * X;
		}
		
		public static double CubicEaseOut (double X)
		{
			X = X - 1;
			return (X * X * X) + 1;
		}
		
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
		public static double QuarticEaseIn (double X)
		{
			return X * X * X * X;
		}
		
		public static double QuarticEaseOut (double X)
		{
			X = X - 1; 
			X = (X * X * X * X) - 1; 
			return  X * -1;
		}
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

		public static double QuinticEaseIn (double X) 
		{
		   	return X * X * X * X * X;
		}

		public static double QuinticEaseOut (double X)
		{
			X = X - 1; 
			X = X * X * X * X * X; 
			return X + 1;
		}		  
		
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
			
	  	public static double SinusoidalEaseIn (double X)
		{
			return -1 * Math.Cos(X * (Math.PI / 2)) + 1;
		}

	  	public static double SinusoidalEaseOut (double X)
	  	{
	   		X = Math.Sin(X * (Math.PI / 2));
			return X;		
		}
		
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
		
		public static double ExponentialEaseIn (double X)
		{	 
			return Math.Pow(2, 10 * (X - 1)) - 0.001;
		}
		
		public static double ExponentialEaseOut (double X)
		{
	 		return 1.001 * (-Math.Pow(2, -10 * X) + 1);
		}
		
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
		 
		public static double CirculairEaseIn (double X)
		{
			return -1 * (Math.Sqrt(1 - (X * X)) - 1);
		}
		
		public static double CirculairEaseOut (double X)
	  	{
	   		return Math.Sqrt(1 - (X - 1) * (X - 1));
		}
				public static double CirculairEaseInOut (double X)
		{
			if (X <= 0.5)
			{
			 	X = X * 2;
			 	X = Tweener.CirculairEaseIn (X);
				return X / 2;
	  		}
			else
			{
		 		X = (X - 0.5) * 2;
		 		X = Tweener.CirculairEaseOut (X);
				return (X / 2) + 0.5;
		  	}
		}
		
		public static double CirculairEaseOutIn (double X) 
		{
			if (X <= 0.5)
			{
				X = X * 2;
				X = Tweener.CirculairEaseOut (X);
				return X / 2;
			}
			else
		 	{
				X = (X - 0.5) * 2;
				X = Tweener.CirculairEaseIn (X);
				return (X / 2) + 0.5;
			}
		}	   
		
		// -= ELASTIC EASING =-
		public static double ElasticEaseIn (double X)
		{
			return (-1 * Math.Pow(2, 10 * (X - 1)) * Math.Sin(((X - 1) - 0.075) * (2 * Math.PI) / 0.3));
		}
		
		public static double ElasticEaseOut (double X)
		{
			return 1 * Math.Pow(2, -10 * X) * Math.Sin((X - 0.075) * (2 * Math.PI) / 0.3) + 1;
	   	}
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
	   	public static double BackEaseIn (double X)
	   	{
			return X * X * ((1.7016 + 1) * X - 1.7016);
		}

	   	public static double BackEaseOut (double X)
	   	{
			return (X - 1) * (X - 1) * ((1.7016 + 1) * (X - 1) + 1.7016) + 1;
		}
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
