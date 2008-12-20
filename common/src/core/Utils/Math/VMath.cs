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

namespace VVVV.Utils.VMath
{
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
		Wrap};
	
	/// <summary>
	/// The vvvv c# math routines library
	/// </summary>
	public sealed class VMath
	{
		#region constants
		
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
	
		#region range functions

		
		/// <summary>
		/// Min function
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns>Smaller value of the two input parameters</returns>
		public static double Min(double a, double b)
		{
			if (a < b)
			{
				return a;
			}
			
			return b;
		}
		
		/// <summary>
		/// Max function
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns>Greater value of the two input parameters</returns>
		public static double Max(double a, double b)
		{
			if (a > b)
			{
				return a;
			}
			
			return b;
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
			double output;
			
			if (InMax-InMin == 0)
				output = 0;
			else
			{
				double range = InMax - InMin;
				double normalized = (Input - InMin) / range;
				
				switch (mode) 
				{
					case TMapMode.Float:
						output = OutMin + normalized * (OutMax - OutMin);
						break;
						
					case TMapMode.Clamp:			
						output = OutMin + normalized * (OutMax - OutMin);
						double min = Min(OutMin, OutMax);
						double max = Max(OutMin, OutMax);
						output = Min(Max(output, min), max);
						break;
						
					case TMapMode.Wrap:
						if (normalized < 0)
							normalized = 1 + normalized;
						output = OutMin + (normalized % 1) * (OutMax - OutMin);
						break;
						
					default:
						output = OutMin + normalized * (OutMax - OutMin);
						break;
				}
				
			}

		 	return output;
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

			double cosTheta = a | b;
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

		#endregion interpolation
		
		#region 3D functions
		
		/// <summary>
		/// Convert polar coordinates (pitch, yaw, lenght) to cartesian coordinates (x, y, z)
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
		/// Convert polar coordinates (pitch, yaw, lenght) to cartesian coordinates (x, y, z) exacly like the vvvv node Cartesian
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
		/// Convert cartesian coordinates (x, y, z) to polar coordinates (pitch, yaw, lenght)
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
		/// Convert cartesian coordinates (x, y, z) to polar coordinates (pitch, yaw, lenght)
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
		public static Matrix4x4 Transform(double transX, double transY, double transZ, 
		                                  double scaleX, double scaleY, double scaleZ,
		                                  double   rotX, double   rotY, double   rotZ)
		{
			return  Translate(transX, transY, transZ) * Scale(scaleX, scaleY, scaleZ) * Rotate(rotX, rotY, rotZ); 
		}
		
		/// <summary>
		/// Creates a transform matrix from translation, scaling and rotation parameters given as 3d-vectors
		/// Like the vvvv node Transform (3d Vector)
		/// </summary>
		/// <param name="trans"></param>
		/// <param name="scale"></param>
		/// <param name="rot"></param>
		/// <returns>Transform matrix</returns>
		public static Matrix4x4 Transform(Vector3D trans, Vector3D scale, Vector3D rot)
		{
			return  Translate(trans.x, trans.y, trans.z) * Scale(scale.x, scale.y, scale.z) * Rotate(rot.x, rot.y, rot.z); 
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
		/// takes about 1,8ns to execute on intel core2 duo 2Ghz, the intel reference
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
			

		#endregion transforms
		
	}

}

