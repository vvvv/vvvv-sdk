/*
 * 
 * the c# vvvv math library
 * 
 * definitions:
 * structs contain only constructors, data fields, properties, indexer and operators
 * higher dimensional datatypes contain operators for lower dimensional ones
 * all functions of the structs are static methods of the VMath class
 * 
 */
 
using System;
using System.Diagnostics.Contracts;

/// <summary>
/// VVVV Math Utilities 
/// </summary>
namespace VVVV.Utils.VMath
{
	
	#region enums
	
	/// <summary>
	/// vvvv like modi for the Map function
	/// </summary>
	public enum TMapMode {
		/// <summary>
		/// Maps the value continously
		/// </summary>
		Float,
		/// <summary>
		/// Maps the value, but clamps it at the min/max borders of the output range
		/// </summary>
		Clamp, 
		/// <summary>
		/// Maps the value, but repeats it into the min/max range, like a modulo function
		/// </summary>
		Wrap,
		/// <summary>
		/// Maps the value, but mirrors it into the min/max range, always against either start or end, whatever is closer
		/// </summary>
		Mirror
    };
	
	#endregion enums
	
	#region VMath class
	
	/// <summary>
	/// The vvvv c# math routines library
	/// </summary>
	public sealed class VMath
	{
		#region constants

        /// <summary>
        /// Pi, as you know it
        /// </summary>
        public const double Pi = 3.1415926535897932384626433832795;

        /// <summary>
        /// Pi * 2
        /// </summary>
        public const double TwoPi = 6.283185307179586476925286766559;

        /// <summary>
        /// 1 / Pi, multiply by this if you have to divide by Pi
        /// </summary>
        public const double PiRez = 0.31830988618379067153776752674503;

        /// <summary>
        /// 2 / Pi, multiply by this if you have to divide by 2*Pi
        /// </summary>
        public const double TwoPiRez = 0.15915494309189533576888376337251;
		
		/// <summary>
		/// Conversion factor from cycles to radians, (2 * Pi)
		/// </summary>
		public const double CycToRad = 6.28318530717958647693;
		/// <summary>
		/// Conversion factor from radians to cycles, 1/(2 * Pi)
		/// </summary>
		public const double RadToCyc = 0.159154943091895335769;
		/// <summary>
		/// Conversion factor from degree to radians, (2 * Pi)/360
		/// </summary>
		public const double DegToRad = 0.0174532925199432957692;
		/// <summary>
		/// Conversion factor from radians to degree, 360/(2 * Pi)
		/// </summary>
		public const double RadToDeg = 57.2957795130823208768;
		/// <summary>
		/// Conversion factor from degree to radians, 1/360
		/// </summary>
		public const double DegToCyc = 0.00277777777777777777778;
		/// <summary>
		/// Conversion factor from radians to degree, 360
		/// </summary>
		public const double CycToDeg = 360.0;
		
		/// <summary>
		/// Identity matrix 
		/// 1000 
		/// 0100
		/// 0010
		/// 0001
		/// </summary>
		public static readonly Matrix4x4 IdentityMatrix = new Matrix4x4(1, 0, 0, 0,
		                                                                0, 1, 0, 0,
		                                                                0, 0, 1, 0,
		                                                                0, 0, 0, 1);
		
		#endregion constants
		
		#region random
		/// <summary>
		/// A random object for conveninece
		/// </summary>
		public static Random Random = new Random(4444);
		
		/// <summary>
		/// Creates a random 2d vector.
		/// </summary>
		/// <returns>Random vector with its components in the range [-1..1].</returns>
		public static Vector2D RandomVector2D()
		{
			return new Vector2D(VMath.Random.NextDouble() * 2 - 1,
			                    VMath.Random.NextDouble() * 2 - 1);
		}
		
		/// <summary>
		/// Creates a random 3d vector.
		/// </summary>
		/// <returns>Random vector with its components in the range [-1..1].</returns>
		public static Vector3D RandomVector3D()
		{
			return new Vector3D(VMath.Random.NextDouble() * 2 - 1,
			                    VMath.Random.NextDouble() * 2 - 1,
			                    VMath.Random.NextDouble() * 2 - 1);
		}
		
		/// <summary>
		/// Creates a random 4d vector.
		/// </summary>
		/// <returns>Random vector with its components in the range [-1..1].</returns>
		public static Vector4D RandomVector4D()
		{
			return new Vector4D(VMath.Random.NextDouble() * 2 - 1,
			                    VMath.Random.NextDouble() * 2 - 1,
			                    VMath.Random.NextDouble() * 2 - 1,
			                    VMath.Random.NextDouble() * 2 - 1);
		}
		
		#endregion random

        #region numeric functions

        /// <summary>
        /// Factorial function, DON'T FEED ME WITH LARGE NUMBERS !!! (n>10 can be huge)
        /// </summary>
        /// <param name="n"></param>
        /// <returns>The product n * n-1 * n-2 * n-3 * .. * 3 * 2 * 1</returns>
        public static int Factorial(int n) 
        {
            if (n == 0)
            {
                return 1;
            }
            if (n < 0) { n = -n; }
            return n*Factorial(n - 1);
        }

        /// <summary>
        /// Binomial function
        /// </summary>
        /// <param name="n"></param>
        /// <param name="k"></param>
        /// <returns>The number of k-tuples of n items</returns>
        public static long Binomial(int n,int k)
        {
            if (n < 0) { n = -n; }
            return Factorial(n) / (Factorial(k) * Factorial(n - k));
        }
        
        /// <summary>
        /// Raises x to the power of y.
        /// </summary>
        /// <param name="x">The base.</param>
        /// <param name="y">The exponent.</param>
        /// <returns>Returns x raised to the power of y.</returns>
        /// <remarks>This method should be considerably faster than Math.Pow for small y.</remarks>
        public static double Pow(double x, int y)
        {
            Contract.Requires(y >= 0);
            var result = 1.0;
            for (int i = 0; i < y; i++)
            {
                result *= x;
            }
            return result;
        }
        
        /// <summary>
        /// Solves a quadratic equation a*x^2 + b*x + c for x
        /// </summary>
        /// <param name="a">Coefficient of x^2</param>
        /// <param name="b">Coefficient of x</param>
        /// <param name="c">Constant</param>
        /// <param name="x1">First solution</param>
        /// <param name="x2">Second solution</param>
        /// <returns>Number of solution, 0, 1, 2 or int.MaxValue</returns>
        public int SolveQuadratic(double a, double b, double c, out double x1, out double x2)
        {
        	x1 = 0;
        	x2 = 0;
        	
        	if (a==0)
        	{
        		if ((b==0) && (c==0))
        		{
        			return int.MaxValue;
        		}
        		else
        		{
        			x1 = - c / b;
        			x2 = x1;
        			return 1;
        		}
        	}
        	else
        	{
        		double D = b*b - 4 * a * c;

        		if (D > 0)
        		{
        			
        			D = Math.Sqrt(D);
        			x1 = (-b + D) / (2*a);
        			x2 = (-b - D) / (2*a);
        			return 2;
        		}
        		else
        		{
        			if (D == 0)
        			{
        				x1 = -b / (2*a);
        				x2 = x1;
        				return 1;
        			}
        			else
        			{
        				return 0;
        			}
        		}
        	}
        }

        #endregion numeric functions

        #region range functions


        /// <summary>
		/// Min function
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
        /// <returns>Smaller value of the two input parameters</returns>
		public static double Min(double a, double b)
		{
		    return a < b ? a : b;
		}
		
		/// <summary>
		/// Max function
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns>Greater value of the two input parameters</returns>
		public static double Max(double a, double b)
		{
			 return a > b ? a : b;
		}
		
		/// <summary>
		/// Modulo function with the property, that the remainder of a division z / d
		/// and z &lt; 0 is positive. For example: zmod(-2, 30) = 28.
		/// </summary>
		/// <param name="z"></param>
		/// <param name="d"></param>
		/// <returns>Remainder of division z / d.</returns>
		public static int Zmod(int z, int d)
		{
            if (z >= d)
				return z % d;
			else if (z < 0)
			{
				int remainder = z % d;
				return remainder == 0 ? 0 : remainder + d;
			}
			else
				return z;
		}
		
		
		/// <summary>
		/// Clamp function, clamps a floating point value into the range [min..max]
		/// </summary>
		/// <param name="x"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static double Clamp(double x, double min, double max)
		{
			double minTemp = Min(min, max);
		 	double maxTemp = Max(min, max);
		 	return Min(Max(x, minTemp), maxTemp);
		}
		
		/// <summary>
		/// Clamp function, clamps an integer value into the range [min..max]
		/// </summary>
		/// <param name="x"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static int Clamp(int x, int min, int max)
		{
			int minTemp = Math.Min(min, max);
		 	int maxTemp = Math.Max(min, max);
		 	return Math.Min(Math.Max(x, minTemp), maxTemp);
		}

        /// <summary>
        /// Clamp function, clamps a long value into the range [min..max]
        /// </summary>
        /// <param name="x"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static long Clamp(long x, long min, long max)
        {
            var minTemp = Math.Min(min, max);
            var maxTemp = Math.Max(min, max);
            return Math.Min(Math.Max(x, minTemp), maxTemp);
        }
		
		/// <summary>
		/// Clamp function, clamps a 2d-vector into the range [min..max]
		/// </summary>
		/// <param name="v"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static Vector2D Clamp(Vector2D v, double min, double max)
		{
			return new Vector2D(Clamp(v.x, min, max), Clamp(v.y, min, max));
		}
		
		/// <summary>
		/// Clamp function, clamps a 3d-vector into the range [min..max]
		/// </summary>
		/// <param name="v"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static Vector3D Clamp(Vector3D v, double min, double max)
		{
			return new Vector3D(Clamp(v.x, min, max), Clamp(v.y, min, max), Clamp(v.z, min, max));
		}
		
		/// <summary>
		/// Clamp function, clamps a 4d-vector into the range [min..max]
		/// </summary>
		/// <param name="v"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static Vector4D Clamp(Vector4D v, double min, double max)
		{
			return new Vector4D(Clamp(v.x, min, max), Clamp(v.y, min, max), Clamp(v.z, min, max), Clamp(v.w, min, max));
		}
		
		/// <summary>
		/// Clamp function, clamps a 2d-vector into the range [min..max]
		/// </summary>
		/// <param name="v"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static Vector2D Clamp(Vector2D v, Vector2D min, Vector2D max)
		{
			return new Vector2D(Clamp(v.x, min.x, max.x), Clamp(v.y, min.y, max.y));
		}
		
		/// <summary>
		/// Clamp function, clamps a 3d-vector into the range [min..max]
		/// </summary>
		/// <param name="v"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static Vector3D Clamp(Vector3D v, Vector3D min, Vector3D max)
		{
			return new Vector3D(Clamp(v.x, min.x, max.x), Clamp(v.y, min.y, max.y), Clamp(v.z, min.z, max.z));
		}
		
		/// <summary>
		/// Clamp function, clamps a 4d-vector into the range [min..max]
		/// </summary>
		/// <param name="v"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static Vector4D Clamp(Vector4D v, Vector4D min, Vector4D max)
		{
			return new Vector4D(Clamp(v.x, min.x, max.x), Clamp(v.y, min.y, max.y), Clamp(v.z, min.z, max.z), Clamp(v.w, min.w, max.w));
		}
		
		/// <summary>
		/// Abs function for values, just for completeness
		/// </summary>
		/// <param name="a"></param>
		/// <returns>New value with the absolut value of a</returns>
		public static double Abs(double a)
		{
			return Math.Abs(a);
		}
		
		/// <summary>
		/// Abs function for 2d-vectors
		/// </summary>
		/// <param name="a"></param>
		/// <returns>New vector with the absolut values of the components of input vector a</returns>
		public static Vector2D Abs(Vector2D a)
		{
			return new Vector2D(Math.Abs(a.x), Math.Abs(a.y));
		}
		
		/// <summary>
		/// Abs function for 3d-vectors
		/// </summary>
		/// <param name="a"></param>
		/// <returns>New vector with the absolut values of the components of input vector a</returns>
		public static Vector3D Abs(Vector3D a)
		{
			return new Vector3D(Math.Abs(a.x), Math.Abs(a.y), Math.Abs(a.z));
		}
		
		/// <summary>
		/// Abs function for 4d-vectors
		/// </summary>
		/// <param name="a"></param>
		/// <returns>New vector with the absolut values of the components of input vector a</returns>
		public static Vector4D Abs(Vector4D a)
		{
			return new Vector4D(Math.Abs(a.x), Math.Abs(a.y), Math.Abs(a.z), Math.Abs(a.w));
		}
		
		/// <summary>
		/// Calculates the distance between two values
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <returns>Positive distance between p1 and p2</returns>
		public static double Dist(double p1, double p2)
		{
			return Math.Abs(p1 - p2);
		}
		
		/// <summary>
		/// Calculates the distance between two 2d-points
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <returns>Positive distance between p1 and p2</returns>
		public static double Dist(Vector2D p1, Vector2D p2)
		{
			return !(p1 - p2);
		}
		
		/// <summary>
		/// Calculates the distance between two 3d-points
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <returns>Positive distance between p1 and p2</returns>
		public static double Dist(Vector3D p1, Vector3D p2)
		{
			return !(p1 - p2);
		}
		
		/// <summary>
		/// Calculates the distance between two 4d-points
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <returns>Positive distance between p1 and p2</returns>
		public static double Dist(Vector4D p1, Vector4D p2)
		{
			return !(p1 - p2);
		}
		
        /// <summary>
         /// This Method can be seen as an inverse of Lerp (in Mode Float). Additionally it provides the infamous Mapping Modes, author: velcrome
         /// </summary>
         /// <param name="Input">Input value to convert</param>
         /// <param name="start">Minimum of input value range</param>
         /// <param name="end">Maximum of input value range</param>
         /// <param name="mode">Defines the behavior of the function if the input value exceeds the destination range 
         /// <see cref="VVVV.Utils.VMath.TMapMode">TMapMode</see></param>
         /// <returns>Input value mapped from input range into destination range</returns>
         public static double Ratio(double Input, double start, double end, TMapMode mode)
         {
             if (end.CompareTo(start) == 0) return 0;
 
             double range = end - start;
             double ratio = (Input - start) / range;
 
             if (mode == TMapMode.Float) { }
             else if (mode == TMapMode.Clamp)
             {
                 if (ratio < 0) ratio = 0;
                 if (ratio > 1) ratio = 1;
             }
             else
             {
                 if (mode == TMapMode.Wrap)
                 {
                     // includes fix for inconsistent behaviour of old delphi Map 
                     // node when handling integers
                     int rangeCount = (int)Math.Floor(ratio);
                     ratio -= rangeCount;
                 }
                 else if (mode == TMapMode.Mirror)
                 {
                     // merke: if you mirror an input twice it is displaced twice the range. same as wrapping twice really
                     int rangeCount = (int)Math.Floor(ratio);
                     rangeCount -= rangeCount & 1; // if uneven, make it even. bitmask of one is same as mod2
                     ratio -= rangeCount;
 
                     if (ratio > 1) ratio = 2 - ratio; // if on the max side of things now (due to rounding down rangeCount), mirror once against max
                 }
             }
             return ratio;
         }
 
         /// <summary>
         /// The infamous Map function of vvvv for values
         /// </summary>
         /// <param name="Input">Input value to convert</param>
         /// <param name="InMin">Minimum of input value range</param>
         /// <param name="InMax">Maximum of input value range</param>
         /// <param name="OutMin">Minimum of destination value range</param>
         /// <param name="OutMax">Maximum of destination value range</param>
         /// <param name="mode">Defines the behavior of the function if the input value exceeds the destination range 
         /// <see cref="VVVV.Utils.VMath.TMapMode">TMapMode</see></param>
         /// <returns>Input value mapped from input range into destination range</returns>
         public static double Map(double Input, double InMin, double InMax, double OutMin, double OutMax, TMapMode mode)
         {
             double ratio = Ratio(Input, InMin, InMax, mode);
             return Lerp(OutMin, OutMax, ratio);
         }
	
		/// <summary>
		/// The infamous Map function of vvvv for 2d-vectors and value range bounds
		/// </summary>
		/// <param name="Input">Input value to convert</param>
		/// <param name="InMin">Minimum of input value range</param>
		/// <param name="InMax">Maximum of input value range</param>
		/// <param name="OutMin">Minimum of destination value range</param>
		/// <param name="OutMax">Maximum of destination value range</param>
		/// <param name="mode">Defines the behavior of the function if the input value exceeds the destination range 
		/// <see cref="VVVV.Utils.VMath.TMapMode">TMapMode</see></param>
		/// <returns>Input vector mapped from input range into destination range</returns>
		public static Vector2D Map(Vector2D Input, double InMin, double InMax, double OutMin, double OutMax, TMapMode mode)
		{
			return new Vector2D(Map(Input.x, InMin, InMax, OutMin, OutMax, mode),
			                    Map(Input.y, InMin, InMax, OutMin, OutMax, mode));
		}
		
		/// <summary>
		/// The infamous Map function of vvvv for 3d-vectors and value range bounds
		/// </summary>
		/// <param name="Input">Input value to convert</param>
		/// <param name="InMin">Minimum of input value range</param>
		/// <param name="InMax">Maximum of input value range</param>
		/// <param name="OutMin">Minimum of destination value range</param>
		/// <param name="OutMax">Maximum of destination value range</param>
		/// <param name="mode">Defines the behavior of the function if the input value exceeds the destination range 
		/// <see cref="VVVV.Utils.VMath.TMapMode">TMapMode</see></param>
		/// <returns>Input vector mapped from input range into destination range</returns>
		public static Vector3D Map(Vector3D Input, double InMin, double InMax, double OutMin, double OutMax, TMapMode mode)
		{
			return new Vector3D(Map(Input.x, InMin, InMax, OutMin, OutMax, mode),
			                    Map(Input.y, InMin, InMax, OutMin, OutMax, mode),
			                    Map(Input.z, InMin, InMax, OutMin, OutMax, mode));
		}
		
		/// <summary>
		/// The infamous Map function of vvvv for 4d-vectors and value range bounds
		/// </summary>
		/// <param name="Input">Input value to convert</param>
		/// <param name="InMin">Minimum of input value range</param>
		/// <param name="InMax">Maximum of input value range</param>
		/// <param name="OutMin">Minimum of destination value range</param>
		/// <param name="OutMax">Maximum of destination value range</param>
		/// <param name="mode">Defines the behavior of the function if the input value exceeds the destination range 
		/// <see cref="VVVV.Utils.VMath.TMapMode">TMapMode</see></param>
		/// <returns>Input vector mapped from input range into destination range</returns>
		public static Vector4D Map(Vector4D Input, double InMin, double InMax, double OutMin, double OutMax, TMapMode mode)
		{
			return new Vector4D(Map(Input.x, InMin, InMax, OutMin, OutMax, mode),
			                    Map(Input.y, InMin, InMax, OutMin, OutMax, mode),
			                    Map(Input.z, InMin, InMax, OutMin, OutMax, mode),
			                    Map(Input.w, InMin, InMax, OutMin, OutMax, mode));
		}
		
		/// <summary>
		/// The infamous Map function of vvvv for 2d-vectors and range bounds given as vectors
		/// </summary>
		/// <param name="Input">Input value to convert</param>
		/// <param name="InMin">Minimum of input value range</param>
		/// <param name="InMax">Maximum of input value range</param>
		/// <param name="OutMin">Minimum of destination value range</param>
		/// <param name="OutMax">Maximum of destination value range</param>
		/// <param name="mode">Defines the behavior of the function if the input value exceeds the destination range 
		/// <see cref="VVVV.Utils.VMath.TMapMode">TMapMode</see></param>
		/// <returns>Input vector mapped from input range into destination range</returns>
		public static Vector2D Map(Vector2D Input, Vector2D InMin, Vector2D InMax, Vector2D OutMin, Vector2D OutMax, TMapMode mode)
		{
			return new Vector2D(Map(Input.x, InMin.x, InMax.x, OutMin.x, OutMax.x, mode),
			                    Map(Input.y, InMin.y, InMax.y, OutMin.y, OutMax.y, mode));
		}
		
		/// <summary>
		/// The infamous Map function of vvvv for 3d-vectors and range bounds given as vectors
		/// </summary>
		/// <param name="Input">Input value to convert</param>
		/// <param name="InMin">Minimum of input value range</param>
		/// <param name="InMax">Maximum of input value range</param>
		/// <param name="OutMin">Minimum of destination value range</param>
		/// <param name="OutMax">Maximum of destination value range</param>
		/// <param name="mode">Defines the behavior of the function if the input value exceeds the destination range 
		/// <see cref="VVVV.Utils.VMath.TMapMode">TMapMode</see></param>
		/// <returns>Input vector mapped from input range into destination range</returns>
		public static Vector3D Map(Vector3D Input, Vector3D InMin, Vector3D InMax, Vector3D OutMin, Vector3D OutMax, TMapMode mode)
		{
			return new Vector3D(Map(Input.x, InMin.x, InMax.x, OutMin.x, OutMax.x, mode),
			                    Map(Input.y, InMin.y, InMax.y, OutMin.y, OutMax.y, mode),
			                    Map(Input.z, InMin.z, InMax.z, OutMin.z, OutMax.z, mode));
		}
		
		/// <summary>
		/// The infamous Map function of vvvv for 4d-vectors and range bounds given as vectors
		/// </summary>
		/// <param name="Input">Input value to convert</param>
		/// <param name="InMin">Minimum of input value range</param>
		/// <param name="InMax">Maximum of input value range</param>
		/// <param name="OutMin">Minimum of destination value range</param>
		/// <param name="OutMax">Maximum of destination value range</param>
		/// <param name="mode">Defines the behavior of the function if the input value exceeds the destination range 
		/// <see cref="VVVV.Utils.VMath.TMapMode">TMapMode</see></param>
		/// <returns>Input vector mapped from input range into destination range</returns>
		public static Vector4D Map(Vector4D Input, Vector4D InMin, Vector4D InMax, Vector4D OutMin, Vector4D OutMax, TMapMode mode)
		{
			return new Vector4D(Map(Input.x, InMin.x, InMax.x, OutMin.x, OutMax.x, mode),
			                    Map(Input.y, InMin.y, InMax.y, OutMin.y, OutMax.y, mode),
			                    Map(Input.z, InMin.z, InMax.z, OutMin.z, OutMax.z, mode),
			                    Map(Input.w, InMin.w, InMax.w, OutMin.w, OutMax.w, mode));
		}
		
		#endregion range functions
			
		#region interpolation

		//Lerp---------------------------------------------------------------------------------------------
		
		/// <summary>
		/// Linear interpolation (blending) between two values
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="x"></param>
		/// <returns>Linear interpolation between a and b if x in the range ]0..1[ or a if x = 0 or b if x = 1</returns>
		public static double Lerp(double a, double b, double x) 
		{
			return a + x * (b - a);
		}
		
		/// <summary>
		/// Linear interpolation (blending) between two 2d-vectors
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="x"></param>
		/// <returns>Linear interpolation between a and b if x in the range ]0..1[, or a if x = 0, or b if x = 1</returns>
		public static Vector2D Lerp(Vector2D a, Vector2D b, double x)
		{
			return a + x * (b - a);
		}
		
		/// <summary>
		/// Linear interpolation (blending) between two 3d-vectors
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="x"></param>
		/// <returns>Linear interpolation between a and b if x in the range ]0..1[, or a if x = 0, or b if x = 1</returns>
		public static Vector3D Lerp(Vector3D a, Vector3D b, double x)
		{
			return a + x * (b - a);
		}
		
		/// <summary>
		/// Linear interpolation (blending) between two 4d-vectors
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="x"></param>
		/// <returns>Linear interpolation between a and b if x in the range ]0..1[, or a if x = 0, or b if x = 1</returns>
		public static Vector4D Lerp(Vector4D a, Vector4D b, double x)
		{
			return a + x * (b - a);
		}
		
		
		//Bilerp------------------------------------------------------------------------------------------
		
		/// <summary>
		/// 2d linear interpolation in x and y direction for single values
		/// </summary>
		/// <param name="Input">The position where to interpolate, 0..1</param>
		/// <param name="P1">Upper left value</param>
		/// <param name="P2">Upper right value</param>
		/// <param name="P3">Lower right value</param>
		/// <param name="P4">Lower left value</param>
		/// <returns>Interpolated value between the 4 values of the corners of a unit square</returns>
		public static double Bilerp(Vector2D Input, double P1, double P2, double P3, double P4)
		{
			
			//interpolate upper values in x direction
			P1 = Lerp(P1, P2, Input.x);
			
			//interpolate lower values in x direction
			P3 = Lerp(P4, P3, Input.x);
			
			//interpolate results in y direction
			return Lerp(P3, P1, Input.y);
			
		}
		
		/// <summary>
		/// 2d linear interpolation in x and y direction for 2d-vectors
		/// </summary>
		/// <param name="Input">The position where to interpolate, 0..1</param>
		/// <param name="P1">Upper left vector</param>
		/// <param name="P2">Upper right vector</param>
		/// <param name="P3">Lower right vector</param>
		/// <param name="P4">Lower left vector</param>
		/// <returns>Interpolated vector between the 4 vectors of the corners of a unit square</returns>
		public static Vector2D Bilerp(Vector2D Input, Vector2D P1, Vector2D P2, Vector2D P3, Vector2D P4)
		{
			
			//interpolate upper points in x direction
			P1 = Lerp(P1, P2, Input.x);
			
			//interpolate lower points in x direction
			P3 = Lerp(P4, P3, Input.x);
			
			//interpolate results in y direction
			return Lerp(P3, P1, Input.y);
			
		}
		
		/// <summary>
		/// 2d linear interpolation in x and y direction for 3d-vectors
		/// </summary>
		/// <param name="Input">The position where to interpolate, 0..1</param>
		/// <param name="P1">Upper left vector</param>
		/// <param name="P2">Upper right vector</param>
		/// <param name="P3">Lower right vector</param>
		/// <param name="P4">Lower left vector</param>
		/// <returns>Interpolated vector between the 4 vectors of the corners of a unit square</returns>
		public static Vector3D Bilerp(Vector2D Input, Vector3D P1, Vector3D P2, Vector3D P3, Vector3D P4)
		{
			
			//interpolate upper points in x direction
			P1 = Lerp(P1, P2, Input.x);
			
			//interpolate lower points in x direction
			P3 = Lerp(P4, P3, Input.x);
			
			//interpolate results in y direction
			return Lerp(P3, P1, Input.y);
			
		}
		
		/// <summary>
		/// 2d linear interpolation in x and y direction for 4d-vectors
		/// </summary>
		/// <param name="Input">The position where to interpolate, 0..1</param>
		/// <param name="P1">Upper left vector</param>
		/// <param name="P2">Upper right vector</param>
		/// <param name="P3">Lower right vector</param>
		/// <param name="P4">Lower left vector</param>
		/// <returns>Interpolated vector between the 4 vectors of the corners of a unit square</returns>
		public static Vector4D Bilerp(Vector2D Input, Vector4D P1, Vector4D P2, Vector4D P3, Vector4D P4)
		{
			
			//interpolate upper points in x direction
			P1 = Lerp(P1, P2, Input.x);
			
			//interpolate lower points in x direction
			P3 = Lerp(P4, P3, Input.x);
			
			//interpolate results in y direction
			return Lerp(P3, P1, Input.y);
			
		}
		
		
		//Trilerp-------------------------------------------------------------------------------
		
		/// <summary>
		/// 3d linear interpolation in x, y and z direction for single values
		/// </summary>
		/// <param name="Input">The Interpolation factor, 3d-position inside the unit cube</param>
		/// <param name="V010">Front upper left</param>
		/// <param name="V110">Front upper right</param>
		/// <param name="V100">Front lower right</param>
		/// <param name="V000">Front lower left</param>
		/// <param name="V011">Back upper left</param>
		/// <param name="V111">Back upper right</param>
		/// <param name="V101">Back lower right</param>
		/// <param name="V001">Back lower left</param>
		/// <returns>Interpolated value between the 8 values of the corners of a unit cube</returns>
		public static double Trilerp(Vector3D Input, 
		                             double V010, double V110, double V100, double V000,
		                             double V011, double V111, double V101, double V001)
		{
			//interpolate the front side
			V000 = Bilerp(Input.xy, V010, V110, V100, V000);
			
			//interpolate the back side
			V111 = Bilerp(Input.xy, V011, V111, V101, V001);
			
			//interpolate in z direction
			return Lerp(V000, V111, Input.z);
		}
		
		/// <summary>
		/// 3d linear interpolation in x, y and z direction for 2d-vectors
		/// </summary>
		/// <param name="Input">The Interpolation factor, 3d-position inside the unit cube</param>
		/// <param name="V010">Front upper left</param>
		/// <param name="V110">Front upper right</param>
		/// <param name="V100">Front lower right</param>
		/// <param name="V000">Front lower left</param>
		/// <param name="V011">Back upper left</param>
		/// <param name="V111">Back upper right</param>
		/// <param name="V101">Back lower right</param>
		/// <param name="V001">Back lower left</param>
		/// <returns>Interpolated vector between the 8 vectors of the corners of a unit cube</returns>
		public static Vector2D Trilerp(Vector3D Input, 
		                             Vector2D V010, Vector2D V110, Vector2D V100, Vector2D V000,
		                             Vector2D V011, Vector2D V111, Vector2D V101, Vector2D V001)
		{
			//interpolate the front side
			V000 = Bilerp(Input.xy, V010, V110, V100, V000);
			
			//interpolate the back side
			V111 = Bilerp(Input.xy, V011, V111, V101, V001);
			
			//interpolate in z direction
			return Lerp(V000, V111, Input.z);
		}
		
		/// <summary>
		/// 3d linear interpolation in x, y and z direction for 3d-vectors
		/// </summary>
		/// <param name="Input">The Interpolation factor, 3d-position inside the unit cube</param>
		/// <param name="V010">Front upper left</param>
		/// <param name="V110">Front upper right</param>
		/// <param name="V100">Front lower right</param>
		/// <param name="V000">Front lower left</param>
		/// <param name="V011">Back upper left</param>
		/// <param name="V111">Back upper right</param>
		/// <param name="V101">Back lower right</param>
		/// <param name="V001">Back lower left</param>
		/// <returns>Interpolated vector between the 8 vectors of the corners of a unit cube</returns>
		public static Vector3D Trilerp(Vector3D Input, 
		                             Vector3D V010, Vector3D V110, Vector3D V100, Vector3D V000,
		                             Vector3D V011, Vector3D V111, Vector3D V101, Vector3D V001)
		{
			//interpolate the front side
			V000 = Bilerp(Input.xy, V010, V110, V100, V000);
			
			//interpolate the back side
			V111 = Bilerp(Input.xy, V011, V111, V101, V001);
			
			//interpolate in z direction
			return Lerp(V000, V111, Input.z);
		}
		
		/// <summary>
		/// 3d linear interpolation in x, y and z direction for 4d-vectors
		/// </summary>
		/// <param name="Input">The Interpolation factor, 3d-position inside the unit cube</param>
		/// <param name="V010">Front upper left</param>
		/// <param name="V110">Front upper right</param>
		/// <param name="V100">Front lower right</param>
		/// <param name="V000">Front lower left</param>
		/// <param name="V011">Back upper left</param>
		/// <param name="V111">Back upper right</param>
		/// <param name="V101">Back lower right</param>
		/// <param name="V001">Back lower left</param>
		/// <returns>Interpolated vector between the 8 vectors of the corners of a unit cube</returns>
		public static Vector4D Trilerp(Vector3D Input, 
		                             Vector4D V010, Vector4D V110, Vector4D V100, Vector4D V000,
		                             Vector4D V011, Vector4D V111, Vector4D V101, Vector4D V001)
		{
			//interpolate the front side
			V000 = Bilerp(Input.xy, V010, V110, V100, V000);
			
			//interpolate the back side
			V111 = Bilerp(Input.xy, V011, V111, V101, V001);
			
			//interpolate in z direction
			return Lerp(V000, V111, Input.z);
		}
		
		//cubic---------------------------------------------------------------------------------------------
		
		/// <summary>
		/// Cubic interpolation curve used in the vvvv timeline
		/// </summary>
		/// <param name="CurrenTime"></param>
		/// <param name="Handle0"></param>
		/// <param name="Handle1"></param>
		/// <param name="Handle2"></param>
		/// <param name="Handle3"></param>
		/// <returns></returns>
		public static double SolveCubic(double CurrenTime, double Handle0, double Handle1, double Handle2, double Handle3)
		{
			return (Handle0 *( System.Math.Pow(( 1 - CurrenTime ), 3)) + ( 3 * Handle1) * (CurrenTime * System.Math.Pow(( 1 - CurrenTime ), 2)) + (3 * Handle2) *( System.Math.Pow(CurrenTime, 2)* ( 1 - CurrenTime )) + Handle3 * System.Math.Pow(CurrenTime, 3));	               
		}
        
		
		//spherical-----------------------------------------------------------------------------------------
		
		/// <summary>
		/// Spherical interpolation between two quaternions (4d-vectors)
		/// The effect is a rotation with uniform angular velocity around a fixed rotation axis from one state of rotation to another
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="x"></param>
		/// <returns>Spherical interpolation between a and b if x in the range ]0..1[, or a if x = 0, or b if x = 1</returns>
		public static Vector4D Slerp(Vector4D a, Vector4D b, double x)
		{
			double w1, w2;

			double cosTheta = a|b; // | is dot product
			double theta    = Math.Acos(cosTheta);
			double sinTheta = Math.Sin(theta);

			if( sinTheta > 0.0001 )
			{
				sinTheta = 1/sinTheta;
				w1 = Math.Sin((1-x) * theta) * sinTheta;
				w2 = Math.Sin(x * theta) * sinTheta;
			}
			else
			{
				w1 = 1 - x;
				w2 = x;
			}

			return a*w1 + b*w2;
		}

        /// <summary>
        /// Spherical interpolation between two points (3d-vectors)
        /// The effect is a rotation with uniform angular velocity around a fixed rotation axis from one state of rotation to another
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="x"></param>
        /// <returns>Spherical interpolation between a and b if x in the range ]0..1[, or a if x = 0, or b if x = 1</returns>
        public static Vector3D Slerp(Vector3D a, Vector3D b, double x)
        {
            double w1, w2;
            double theta, sinTheta;

            double cosTheta = a|b;  // | is dot product
            double len = Math.Sqrt((a|a) * (b|b));   //len = length(A) * length(B)

            if (len > 0.0001)
            {
                theta = Math.Acos(cosTheta / len);
                sinTheta = Math.Sin(theta);

                if (sinTheta > 0.0001)
                {
                    sinTheta = 1 / sinTheta;
                    w1 = Math.Sin((1 - x) * theta) * sinTheta;
                    w2 = Math.Sin(x * theta) * sinTheta;
                }
                else
                {
                    w1 = 1 - x;
                    w2 = x;
                }
            }
            else
            {
                w1 = 1 - x;
                w2 = x;
            }

            return a * w1 + b * w2;
        }

		#endregion interpolation
		
		#region 3D functions
		
		/// <summary>
		/// Convert polar coordinates (pitch, yaw, lenght) in radian to cartesian coordinates (x, y, z).
        /// To convert angles from cycles to radian, multiply them with VMath.CycToDec.
		/// </summary>
		/// <param name="pitch"></param>
		/// <param name="yaw"></param>
		/// <param name="length"></param>
		/// <returns>3d-point in cartesian coordinates</returns>
		public static Vector3D Cartesian(double pitch, double yaw, double length)
		{
			double sinp = length * Math.Sin(pitch);
			
			return new Vector3D(sinp * Math.Cos(yaw), sinp * Math.Sin(yaw), length * Math.Cos(pitch));
		}

        /// <summary>
        /// Convert polar coordinates (pitch, yaw, lenght) in radian to cartesian coordinates (x, y, z).
        /// To convert angles from cycles to radian, multiply them with VMath.CycToDec.
        /// </summary>
        /// <param name="polar">3d-vector containing the polar coordinates as (pitch, yaw, length)</param>
        /// <returns></returns>
        public static Vector3D Cartesian(Vector3D polar)
        {
            double sinp = polar.z * Math.Sin(polar.x);

            return new Vector3D(sinp * Math.Cos(polar.y), sinp * Math.Sin(polar.y), polar.z * Math.Cos(polar.x));
        }
		
		/// <summary>
		/// Convert polar coordinates (pitch, yaw, lenght) in radian to cartesian coordinates (x, y, z) exacly like the vvvv node Cartesian.
        /// To convert angles from cycles to radian, multiply them with VMath.CycToDec.
		/// </summary>
		/// <param name="pitch"></param>
		/// <param name="yaw"></param>
		/// <param name="length"></param>
		/// <returns>3d-point in cartesian coordinates like the vvvv node does it</returns>
		public static Vector3D CartesianVVVV(double pitch, double yaw, double length)
		{
			double cosp = - length * Math.Cos(pitch);
			
			return new Vector3D( cosp * Math.Sin(yaw), length * Math.Sin(pitch), cosp * Math.Cos(yaw));
		}

        /// <summary>
        /// Convert polar coordinates (pitch, yaw, lenght) in radian to cartesian coordinates (x, y, z) exacly like the vvvv node Cartesian.
        /// To convert angles from cycles to radian, multiply them with VMath.CycToDec.
        /// </summary>
        /// <param name="polar">3d-vector containing the polar coordinates as (pitch, yaw, length)</param>
        /// <returns></returns>
        public static Vector3D CartesianVVVV(Vector3D polar)
        {
            double cosp = -polar.z * Math.Cos(polar.x);

            return new Vector3D(cosp * Math.Sin(polar.y), polar.z * Math.Sin(polar.x), cosp * Math.Cos(polar.y));
        }
		
		/// <summary>
		/// Convert cartesian coordinates (x, y, z) to polar coordinates (pitch, yaw, lenght) in radian.
        /// To convert the angles to cycles, multiply them with VMath.DegToCyc.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <returns>3d-point in polar coordinates</returns>
		public static Vector3D Polar(double x, double y, double z)
		{
			double length = x * x + y * y + z * z;
			
			
			if (length > 0) 
			{
				length = Math.Sqrt(length);
				return new Vector3D(Math.Acos(z / length), Math.Atan2(y, x), length);
			} 
			else 
			{
				return new Vector3D(0);
			}
			
		}
		
		/// <summary>
		/// Convert cartesian coordinates (x, y, z) to polar coordinates (pitch, yaw, lenght) in radian.
        /// To convert the angles to cycles, multiply them with VMath.DegToCyc.
		/// </summary>
		/// <param name="a"></param>
		/// <returns>Point in polar coordinates</returns>
		public static Vector3D Polar(Vector3D a)
		{
			double length = a.x * a.x + a.y * a.y + a.z * a.z;
			
			
			if (length > 0) 
			{
				length = Math.Sqrt(length);
				return new Vector3D(Math.Acos(a.z / length), Math.Atan2(a.y, a.x), length);
			} 
			else 
			{
				return new Vector3D(0);
			}
			
		}

        /// <summary>
        /// Convert cartesian coordinates (x, y, z) to VVVV style polar coordinates (pitch, yaw, lenght) in radian.
        /// To convert the angles to cycles, multiply them with VMath.DegToCyc.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns>3d-point in polar coordinates</returns>
        public static Vector3D PolarVVVV(double x, double y, double z)
        {
            double length = x * x + y * y + z * z;


            if (length > 0)
            {
                length = Math.Sqrt(length);
                var pitch = Math.Asin(y / length);
                var yaw = 0.0;
                if (z != 0)
                    yaw = Math.Atan2(-x, -z);
                else if (x > 0)
                    yaw = -Math.PI / 2;
                else
                    yaw = Math.PI / 2;

                return new Vector3D(pitch, yaw, length);
            }
            else
            {
                return new Vector3D(0);
            }

        }

        /// <summary>
        /// Convert cartesian coordinates (x, y, z) to polar VVVV style coordinates (pitch, yaw, lenght) in radian.
        /// To convert the angles to cycles, multiply them with VMath.DegToCyc.
        /// </summary>
        /// <param name="a"></param>
        /// <returns>Point in polar coordinates</returns>
        public static Vector3D PolarVVVV(Vector3D a)
        {
            double length = a.x * a.x + a.y * a.y + a.z * a.z;


            if (length > 0)
            {
                length = Math.Sqrt(length);
                var pitch = Math.Asin(a.y / length);
                var yaw = 0.0;
                if(a.z != 0)
                    yaw = Math.Atan2(-a.x, -a.z);
                else if (a.x > 0)
                    yaw = -Math.PI / 2;
                else 
                    yaw = Math.PI / 2;

                return new Vector3D(pitch, yaw, length);
            }
            else
            {
                return new Vector3D(0);
            }

        }

        /// <summary>
        /// Converts a quaternion into euler angles, assuming that the euler angle multiplication to create the quaternion was yaw*pitch*roll.
        /// All angles in radian.
        /// </summary>
        /// <param name="q">A quaternion, can be non normalized</param>
        /// <param name="pitch"></param>
        /// <param name="yaw"></param>
        /// <param name="roll"></param>
        public static void QuaternionToEulerYawPitchRoll(Vector4D q, out double pitch, out double yaw, out double roll)
        {
            double sqw = q.w * q.w;
            double sqx = q.x * q.x;
            double sqy = q.y * q.y;
            double sqz = q.z * q.z;
            double unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
            double test = q.x * q.y + q.z * q.w;

            if (test > 0.49999 * unit)
            { // singularity at north pole
                pitch = 0;
                yaw = 2 * Math.Atan2(q.y, q.w);
                roll = Math.PI / 2;
                return;
            }

            if (test < -0.49999 * unit)
            { // singularity at south pole
                pitch = 0;
                yaw = -2 * Math.Atan2(q.y, q.w);
                roll = -Math.PI / 2;
                return;
            }

            pitch = Math.Asin(2 * (q.w * q.x - q.y * q.z) / unit);
            yaw = Math.Atan2(2 * (q.w * q.y + q.x * q.z), 1 - 2 * (sqy + sqx));
            roll = Math.Atan2(2 * (q.w * q.z + q.y * q.x), 1 - 2 * (sqx + sqz));
        }

        /// <summary>
        /// Converts a quaternion into euler angles, assuming that the euler angle multiplication to create the quaternion was yaw*pitch*roll.
        /// All angles in radian.
        /// </summary>
        /// <param name="q">A quaternion, can be non normalized</param>
        /// <returns>3d-vector with x=pitch, y=yaw, z=roll</returns>
        public static Vector3D QuaternionToEulerYawPitchRoll(Vector4D q)
        {
            Vector3D ret;

            QuaternionToEulerYawPitchRoll(q, out ret.x, out ret.y, out ret.z);

            return ret;
        }
        
        /// <summary>
        /// Intersaction of 3 Spheres
        /// </summary>
        /// <param name="P1">Center sphere 1</param>
        /// <param name="P2">Center sphere 2</param>
        /// <param name="P3">Center sphere 3</param>
        /// <param name="r1">Radius sphere 1</param>
        /// <param name="r2">Radius sphere 2</param>
        /// <param name="r3">Radius sphere 3</param>
        /// <param name="S1">Intersection Point 1</param>
        /// <param name="S2">Intersection Point 2</param>
        /// <returns>Number of intersections</returns>
        public static int Trilateration(Vector3D P1, Vector3D P2, Vector3D P3, double r1, double r2, double r3, out Vector3D S1, out Vector3D S2)
        {
        
        	//P1 to P2 vector
        	var P1toP2 = P2 - P1;
        	
        	//distance P1 to P2
        	var dsqr = P1toP2 | P1toP2; // d^2 needed later
        	var d = Math.Sqrt(dsqr);
        	
        	//assume, that sphere 1 and 2 intersect
        	if((d - r1 <= r2) && (r2 <= d + r1))
        	{
        		
        		//P1 to P3 vector
        		var P1toP3 = P3 - P1;
        		
        		//normal base x
        		var ex = P1toP2 / d;
        		
        		//distance P1 to P3 in direction to P2
        		var i = ex | P1toP3;
        		
        		//normal base y
        		var ey = ~(P3 - P1 - i*ex);
        		
        		//distance P1P2 orthoganl to P3
        		var j = ey | P1toP3;
        		
        		//normal base z
        		var ez = ex.CrossRH(ey);
        		
        		//calc x
        		var r1sqr = r1 * r1;
        		var x = (r1sqr - r2*r2 + dsqr) / (2*d);
        		
        		//calc y
        		var xmini = x - i;
        		var xsqr = x*x;
        		var y = (r1sqr - r3*r3 - xsqr + xmini*xmini + j*j) / (2*j);
        		
        		//calc z
        		var z = Math.Sqrt(r1sqr - xsqr - y*y);
        		
        		var zez = z*ez;
        		S1 = P1 + x*ex + y*ey + zez;
        		S2 = S1 - 2*zez;
        		
        		return 2;
        	}
        	else
        	{
        		S1 = new Vector3D();
        		S2 = new Vector3D();
        		return 0;
        	}
        	
        }
                                     
		
		#endregion 3D functions
		
		#region transforms
		
		/// <summary>
		/// Creates a translation matrix from 3 given values
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <returns>Translation matrix</returns>
		public static Matrix4x4 Translate(double x, double y, double z)
		{
			return new Matrix4x4(1, 0, 0, 0,
			                     0, 1, 0, 0,
			                     0, 0, 1, 0,
			                     x, y, z, 1);
		}
		
		/// <summary>
		/// Creates a translation matrix from a given 3d-vector
		/// </summary>
		/// <param name="v"></param>
		/// <returns>Translation matrix</returns>
		public static Matrix4x4 Translate(Vector3D v)
		{
			return new Matrix4x4(1, 0, 0, 0,
			                     0, 1, 0, 0,
			                     0, 0, 1, 0,
			                     v.x, v.y, v.z, 1);
		}
		
		/// <summary>
		/// Creates a scaling matrix from 3 given values
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <returns>Scaling matrix</returns>
		public static Matrix4x4 Scale(double x, double y, double z)
		{
			return new Matrix4x4(x, 0, 0, 0,
			                     0, y, 0, 0,
			                     0, 0, z, 0,
			                     0, 0, 0, 1);
		}
		
		/// <summary>
		/// Creates a scaling matrix from a given 3d-vector
		/// </summary>
		/// <param name="v"></param>
		/// <returns>Scaling matrix</returns>
		public static Matrix4x4 Scale(Vector3D v)
		{
			return new Matrix4x4(v.x,   0,   0, 0,
			                       0, v.y,   0, 0,
			                       0,   0, v.z, 0,
			                       0,   0,   0, 1);
		}
		
		/// <summary>
		/// Creates a rotation matrix from a given angle around the x-axis
		/// </summary>
		/// <param name="rotX"></param>
		/// <returns>Rotation matrix</returns>
		public static Matrix4x4 RotateX(double rotX)
		{
			double s = Math.Sin(rotX);
			double c = Math.Cos(rotX);

			return new Matrix4x4(1,  0, 0, 0,
			                     0,  c, s, 0,
			                     0, -s, c, 0,
			                     0,  0, 0, 1);
		}
		
		/// <summary>
		/// Creates a rotation matrix from a given angle around the y-axis
		/// </summary>
		/// <param name="rotY"></param>
		/// <returns>Rotation matrix</returns>
		public static Matrix4x4 RotateY(double rotY)
		{
			double s = Math.Sin(rotY);
			double c = Math.Cos(rotY);

			return new Matrix4x4(c, 0, -s, 0,
			                     0, 1,  0, 0,
			                     s, 0,  c, 0,
			                     0, 0,  0, 1);
		}
		
		/// <summary>
		/// Creates a rotation matrix from a given angle around the z-axis
		/// </summary>
		/// <param name="rotZ"></param>
		/// <returns>Rotation matrix</returns>
		public static Matrix4x4 RotateZ(double rotZ)
		{
			double s = Math.Sin(rotZ);
			double c = Math.Cos(rotZ);

			return new Matrix4x4( c, s, 0, 0,
			                     -s, c, 0, 0,
			                      0, 0, 1, 0,
			                      0, 0, 0, 1);
		}
		
		/// <summary>
		/// Creates a rotation matrix from 3 angles
		/// </summary>
		/// <param name="rotX"></param>
		/// <param name="rotY"></param>
		/// <param name="rotZ"></param>
		/// <returns>Rotation matrix</returns>
		public static Matrix4x4 Rotate(double rotX, double rotY, double rotZ)
		{
			double sx = Math.Sin(rotX);
			double cx = Math.Cos(rotX);
			double sy = Math.Sin(rotY);
			double cy = Math.Cos(rotY);
			double sz = Math.Sin(rotZ);
			double cz = Math.Cos(rotZ);

			return new Matrix4x4( cz * cy + sz * sx * sy, sz * cx, cz * -sy + sz * sx * cy, 0,
			                     -sz * cy + cz * sx * sy, cz * cx,  sz * sy + cz * sx * cy, 0,
			                                     cx * sy,     -sx,                 cx * cy, 0,
			                                           0,       0,                       0, 1);
		}
		
		/// <summary>
		///  Creates a rotation matrix from 3 angles given as 3d-vector
		/// </summary>
		/// <param name="rot"></param>
		/// <returns>Rotation matrix</returns>
		public static Matrix4x4 Rotate(Vector3D rot)
		{
			double sx = Math.Sin(rot.x);
			double cx = Math.Cos(rot.x);
			double sy = Math.Sin(rot.y);
			double cy = Math.Cos(rot.y);
			double sz = Math.Sin(rot.z);
			double cz = Math.Cos(rot.z);

			return new Matrix4x4( cz * cy + sz * sx * sy, sz * cx, cz * -sy + sz * sx * cy, 0,
			                     -sz * cy + cz * sx * sy, cz * cx,  sz * sy + cz * sx * cy, 0,
			                                     cx * sy,     -sx,                 cx * cy, 0,
			                                           0,       0,                       0, 1);
		}
		
		/// <summary>
		/// Creates a transform matrix from translation, scaling and rotation parameters
		/// </summary>
		/// <param name="transX"></param>
		/// <param name="transY"></param>
		/// <param name="transZ"></param>
		/// <param name="scaleX"></param>
		/// <param name="scaleY"></param>
		/// <param name="scaleZ"></param>
		/// <param name="rotX"></param>
		/// <param name="rotY"></param>
		/// <param name="rotZ"></param>
		/// <returns>Transform matrix</returns>
		public static Matrix4x4 Transform(double transX, double transY, double transZ, 
		                                  double scaleX, double scaleY, double scaleZ,
		                                  double   rotX, double   rotY, double   rotZ)
		{
			return Rotate(rotX, rotY, rotZ) * Scale(scaleX, scaleY, scaleZ) * Translate(transX, transY, transZ);
		}
		
		/// <summary>
		/// Creates a transform matrix from translation, scaling and rotation parameters given as 3d-vectors
		/// </summary>
		/// <param name="trans"></param>
		/// <param name="scale"></param>
		/// <param name="rot"></param>
		/// <returns>Transform matrix</returns>
		public static Matrix4x4 Transform(Vector3D trans, Vector3D scale, Vector3D rot)
		{
			return Rotate(rot.x, rot.y, rot.z) * Scale(scale.x, scale.y, scale.z) * Translate(trans.x, trans.y, trans.z);
		}

        /// <summary>
		/// Creates a transform matrix from translation, scaling and rotation parameters
		/// Like the vvvv node Transform (3d)
		/// </summary>
		/// <param name="transX"></param>
		/// <param name="transY"></param>
		/// <param name="transZ"></param>
		/// <param name="scaleX"></param>
		/// <param name="scaleY"></param>
		/// <param name="scaleZ"></param>
		/// <param name="rotX"></param>
		/// <param name="rotY"></param>
		/// <param name="rotZ"></param>
		/// <returns>Transform matrix</returns>
		public static Matrix4x4 TransformVVVV(double transX, double transY, double transZ,
                                              double scaleX, double scaleY, double scaleZ,
                                              double rotX, double rotY, double rotZ)
        {
            return Scale(scaleX, scaleY, scaleZ) * Rotate(rotX, rotY, rotZ) * Translate(transX, transY, transZ);
        }

        /// <summary>
        /// Creates a transform matrix from translation, scaling and rotation parameters given as 3d-vectors
        /// Like the vvvv node Transform (3d Vector)
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="scale"></param>
        /// <param name="rot"></param>
        /// <returns>Transform matrix</returns>
        public static Matrix4x4 TransformVVVV(Vector3D trans, Vector3D scale, Vector3D rot)
        {
            return  Scale(scale.x, scale.y, scale.z) * Rotate(rot.x, rot.y, rot.z) * Translate(trans.x, trans.y, trans.z);
        }

        /// <summary>
        /// Builds a left-handed perspective projection matrix based on a field of view.
        /// </summary>
        /// <param name="FOV">Camera angle in cycles, [0..0.5]</param>
        /// <param name="Near">Near Plane z</param>
        /// <param name="Far">Far Plane z</param>
        /// <param name="Aspect">Aspect Ratio</param>
        /// <returns>Projection matrix</returns>
        public static Matrix4x4 PerspectiveLH(double FOV, double Near, double Far, double Aspect)
		{
			double scaleY = 1.0/Math.Tan(FOV * Math.PI);
			double scaleX = scaleY / Aspect;
			double fn = Far / (Far - Near);
			
			return new Matrix4x4(scaleX,      0,        0, 0,
			                          0, scaleY,        0, 0,
			                          0,      0,       fn, 1,
			                          0,      0, -Near*fn, 0);
			
		}
		
		/// <summary>
		/// Builds a right-handed perspective projection matrix based on a field of view.
		/// </summary>
		/// <param name="FOV">Camera angle in cycles, [0..0.5]</param>
		/// <param name="Near">Near Plane z</param>
		/// <param name="Far">Far Plane z</param>
		/// <param name="Aspect">Aspect Ratio</param>
		/// <returns>Projection matrix</returns>
		public static Matrix4x4 PerspectiveRH(double FOV, double Near, double Far, double Aspect)
		{
			double scaleY = 1.0/Math.Tan(FOV * Math.PI);
			double scaleX = scaleY / Aspect;
			double fn = Far / (Far - Near);
			
			return new Matrix4x4(scaleX,      0,       0,  0,
			                          0, scaleY,       0,  0,
			                          0,      0,      fn, -1,
			                          0,      0, Near*fn,  0);
			
		}
		
		/// <summary>
		/// Transpose a 4x4 matrix
		/// </summary>
		/// <param name="A"></param>
		/// <returns>Transpose of input matrix A</returns>
		public static Matrix4x4 Transpose(Matrix4x4 A)
		{
			return new Matrix4x4(A.m11, A.m21, A.m31, A.m41,
			                     A.m12, A.m22, A.m32, A.m42,
			                     A.m13, A.m23, A.m33, A.m43,
			                     A.m14, A.m24, A.m34, A.m44);
		}
		
		/// <summary>
		/// Optimized 4x4 matrix inversion using cramer's rule, found in the game engine http://www.ogre3d.org
		/// Note that the unary ! operator of Matrix4x4 does the same
		/// 
		/// Code takes about 1,8ns to execute on intel core2 duo 2Ghz, the intel reference
		/// implementation (not assembly optimized) was about 2,2ns.
		/// http://www.intel.com/design/pentiumiii/sml/24504301.pdf
		/// </summary>
		/// <param name="A"></param>
		/// <returns>Inverse matrix of input matrix A</returns>
		public static Matrix4x4 Inverse(Matrix4x4 A)
		{
			double a11 = A.m11, a12 = A.m12, a13 = A.m13, a14 = A.m14;
			double a21 = A.m21, a22 = A.m22, a23 = A.m23, a24 = A.m24;
			double a31 = A.m31, a32 = A.m32, a33 = A.m33, a34 = A.m34;
			double a41 = A.m41, a42 = A.m42, a43 = A.m43, a44 = A.m44;

			double term1 = a31 * a42 - a32 * a41;
			double term2 = a31 * a43 - a33 * a41;
			double term3 = a31 * a44 - a34 * a41;
			double term4 = a32 * a43 - a33 * a42;
			double term5 = a32 * a44 - a34 * a42;
			double term6 = a33 * a44 - a34 * a43;

			double subterm1 = + (term6 * a22 - term5 * a23 + term4 * a24);
			double subterm2 = - (term6 * a21 - term3 * a23 + term2 * a24);
			double subterm3 = + (term5 * a21 - term3 * a22 + term1 * a24);
			double subterm4 = - (term4 * a21 - term2 * a22 + term1 * a23);

			double invDet = 1 / (subterm1 * a11 + subterm2 * a12 + subterm3 * a13 + subterm4 * a14);

			double ret11 = subterm1 * invDet;
			double ret21 = subterm2 * invDet;
			double ret31 = subterm3 * invDet;
			double ret41 = subterm4 * invDet;

			double ret12 = - (term6 * a12 - term5 * a13 + term4 * a14) * invDet;
			double ret22 = + (term6 * a11 - term3 * a13 + term2 * a14) * invDet;
			double ret32 = - (term5 * a11 - term3 * a12 + term1 * a14) * invDet;
			double ret42 = + (term4 * a11 - term2 * a12 + term1 * a13) * invDet;

			term1 = a21 * a42 - a22 * a41;
			term2 = a21 * a43 - a23 * a41;
			term3 = a21 * a44 - a24 * a41;
			term4 = a22 * a43 - a23 * a42;
			term5 = a22 * a44 - a24 * a42;
			term6 = a23 * a44 - a24 * a43;

			double ret13 = + (term6 * a12 - term5 * a13 + term4 * a14) * invDet;
			double ret23 = - (term6 * a11 - term3 * a13 + term2 * a14) * invDet;
			double ret33 = + (term5 * a11 - term3 * a12 + term1 * a14) * invDet;
			double ret43 = - (term4 * a11 - term2 * a12 + term1 * a13) * invDet;

			term1 = a32 * a21 - a31 * a22;
			term2 = a33 * a21 - a31 * a23;
			term3 = a34 * a21 - a31 * a24;
			term4 = a33 * a22 - a32 * a23;
			term5 = a34 * a22 - a32 * a24;
			term6 = a34 * a23 - a33 * a24;

			double ret14 = - (term6 * a12 - term5 * a13 + term4 * a14) * invDet;
			double ret24 = + (term6 * a11 - term3 * a13 + term2 * a14) * invDet;
			double ret34 = - (term5 * a11 - term3 * a12 + term1 * a14) * invDet;
			double ret44 = + (term4 * a11 - term2 * a12 + term1 * a13) * invDet;

			return new Matrix4x4(ret11, ret12, ret13, ret14,
								 ret21, ret22, ret23, ret24,
								 ret31, ret32, ret33, ret34,
								 ret41, ret42, ret43, ret44);
		}

		/// <summary>
		/// Calculates the determinat of a 4x4 matrix
		/// Note that the unary ~ operator of Matrix4x4 does the same
		/// </summary>
		/// <param name="A"></param>
		/// <returns>Determinat of input matrix A</returns>
		public static double Det(Matrix4x4 A)
		{
			double a11 = A.m11, a12 = A.m12, a13 = A.m13, a14 = A.m14;
			double a21 = A.m21, a22 = A.m22, a23 = A.m23, a24 = A.m24;
			double a31 = A.m31, a32 = A.m32, a33 = A.m33, a34 = A.m34;
			double a41 = A.m41, a42 = A.m42, a43 = A.m43, a44 = A.m44;

			double term1 = a31 * a42 - a32 * a41;
			double term2 = a31 * a43 - a33 * a41;
			double term3 = a31 * a44 - a34 * a41;
			double term4 = a32 * a43 - a33 * a42;
			double term5 = a32 * a44 - a34 * a42;
			double term6 = a33 * a44 - a34 * a43;

			double subterm1 =   (term6 * a22 - term5 * a23 + term4 * a24);
			double subterm2 = - (term6 * a21 - term3 * a23 + term2 * a24);
			double subterm3 =   (term5 * a21 - term3 * a22 + term1 * a24);
			double subterm4 = - (term4 * a21 - term2 * a22 + term1 * a23);

			return subterm1 * a11 + subterm2 * a12 + subterm3 * a13 + subterm4 * a14;
		}
		
		/// <summary>
		/// Builds a matrix that interpolates 4d-vectors like a 2d bilinear interpolation in x and y direction
		/// 
		/// Should be used to transform 4d vectors with interpolation foacors in the 4d-form (x, y, x*y, 1) 
		/// </summary>
		/// <param name="P1">Upper left vector</param>
		/// <param name="P2">Upper right vector</param>
		/// <param name="P3">Lower right vector</param>
		/// <param name="P4">Lower left vector</param>
		/// <returns>Linear interpolation matrix, can be used to interpolate 4d vectors with interpolation factors in the 4d-form (x, y, x*y, 1)</returns>
		public static Matrix4x4 BilerpMatrix(Vector4D P1, Vector4D P2, Vector4D P3, Vector4D P4)
		{
			return new Matrix4x4(P4.x - P3.x               , P4.y - P3.y               , P4.z - P3.z               , P4.w - P3.w               ,
			                     P1.x - P3.x               , P1.y - P3.y               , P1.z - P3.z               , P1.w - P3.w               ,
			                     P3.x + P2.x - P4.x - P1.x , P3.y + P2.y - P4.y - P1.y , P3.z + P2.z - P4.z - P1.z , P3.w + P2.w - P4.w - P1.w ,
			                     P3.x                      , P3.y                      , P3.z                      , P3.w);
			
		}
			

		#endregion transforms
		
	}
	
	#endregion VMath class

}

