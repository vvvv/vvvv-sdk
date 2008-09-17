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
using System.Collections.Generic;

namespace VVVV.Utils.VMath
{
	public enum TMapMode {Float, Clamp, Wrap};
	
	//the vvvv math routines library
	public sealed class VMath
	{
		#region constants
		
		//angle conversion
		public const double CycToRad = 6.28318530717958647693;
		public const double RadToCyc = 0.159154943091895335769;
		public const double DegToRad = 0.0174532925199432957692;
		public const double RadToDeg = 57.2957795130823208768;
		public const double DegToCyc = 0.00277777777777777777778;
		public const double CycToDeg = 360.0;
		
		//identity matrix
		public static readonly Matrix4x4 IdentityMatrix = new Matrix4x4(1, 0, 0, 0,
		                                                                0, 1, 0, 0,
		                                                                0, 0, 1, 0,
		                                                                0, 0, 0, 1);
		
		#endregion constants
	
		#region range functions

		//min
		public static double Min(double a, double b)
		{
			if (a < b)
			{
				return a;
			}
			
			return b;
		}
		
		//max
		public static double Max(double a, double b)
		{
			if (a > b)
			{
				return a;
			}
			
			return b;
		}
		
		//clamp
		public static double Clamp(double x, double min, double max)
		{
			double minTemp = Min(min, max);
		 	double maxTemp = Max(min, max);
		 	return Min(Max(x, minTemp), maxTemp);
		}
		
		//abs
		public static Vector2D Abs(Vector2D a)
		{
			return new Vector2D(Math.Abs(a.x), Math.Abs(a.y));
		}
		
		public static Vector3D Abs(Vector3D a)
		{
			return new Vector3D(Math.Abs(a.x), Math.Abs(a.y), Math.Abs(a.z));
		}
		
		public static Vector4D Abs(Vector4D a)
		{
			return new Vector4D(Math.Abs(a.x), Math.Abs(a.y), Math.Abs(a.z), Math.Abs(a.w));
		}
		
		//distance
		public static double Dist(double p1, double p2)
		{
			return Math.Abs(p1 - p2);
		}
		
		public static double Dist(Vector2D p1, Vector2D p2)
		{
			return !(p1 - p2);
		}
		
		public static double Dist(Vector3D p1, Vector3D p2)
		{
			return !(p1 - p2);
		}
		
		public static double Dist(Vector4D p1, Vector4D p2)
		{
			return !(p1 - p2);
		}
		
		//map
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
		
		public static Vector2D Map(Vector2D Input, double InMin, double InMax, double OutMin, double OutMax, TMapMode mode)
		{
			return new Vector2D(Map(Input.x, InMin, InMax, OutMin, OutMax, mode),
			                    Map(Input.y, InMin, InMax, OutMin, OutMax, mode));
		}
		
		public static Vector3D Map(Vector3D Input, double InMin, double InMax, double OutMin, double OutMax, TMapMode mode)
		{
			return new Vector3D(Map(Input.x, InMin, InMax, OutMin, OutMax, mode),
			                    Map(Input.y, InMin, InMax, OutMin, OutMax, mode),
			                    Map(Input.z, InMin, InMax, OutMin, OutMax, mode));
		}
		
		public static Vector4D Map(Vector4D Input, double InMin, double InMax, double OutMin, double OutMax, TMapMode mode)
		{
			return new Vector4D(Map(Input.x, InMin, InMax, OutMin, OutMax, mode),
			                    Map(Input.y, InMin, InMax, OutMin, OutMax, mode),
			                    Map(Input.z, InMin, InMax, OutMin, OutMax, mode),
			                    Map(Input.w, InMin, InMax, OutMin, OutMax, mode));
		}
		
		public static Vector2D Map(Vector2D Input, Vector4D InMin, Vector4D InMax, Vector4D OutMin, Vector4D OutMax, TMapMode mode)
		{
			return new Vector2D(Map(Input.x, InMin.x, InMax.x, OutMin.x, OutMax.x, mode),
			                    Map(Input.y, InMin.y, InMax.y, OutMin.y, OutMax.y, mode));
		}
		
		public static Vector3D Map(Vector3D Input, Vector4D InMin, Vector4D InMax, Vector4D OutMin, Vector4D OutMax, TMapMode mode)
		{
			return new Vector3D(Map(Input.x, InMin.x, InMax.x, OutMin.x, OutMax.x, mode),
			                    Map(Input.y, InMin.y, InMax.y, OutMin.y, OutMax.y, mode),
			                    Map(Input.z, InMin.z, InMax.z, OutMin.z, OutMax.z, mode));
		}
		
		public static Vector4D Map(Vector4D Input, Vector4D InMin, Vector4D InMax, Vector4D OutMin, Vector4D OutMax, TMapMode mode)
		{
			return new Vector4D(Map(Input.x, InMin.x, InMax.x, OutMin.x, OutMax.x, mode),
			                    Map(Input.y, InMin.y, InMax.y, OutMin.y, OutMax.y, mode),
			                    Map(Input.z, InMin.z, InMax.z, OutMin.z, OutMax.z, mode),
			                    Map(Input.w, InMin.w, InMax.w, OutMin.w, OutMax.w, mode));
		}
		
		#endregion range functions
			
		#region interpolation

		//linear interpolation
		public static double Lerp(double a, double b, double x)
		{
			return a + x * (b - a);
		}
		
		public static Vector2D Lerp(Vector2D a, Vector2D b, double x)
		{
			return a + x * (b - a);
		}
		
		public static Vector3D Lerp(Vector3D a, Vector3D b, double x)
		{
			return a + x * (b - a);
		}
		
		public static Vector4D Lerp(Vector4D a, Vector4D b, double x)
		{
			return a + x * (b - a);
		}
		
		//cubic interpolation
		public static double SolveCubic(double CurrenTime, double Handle0, double Handle1, double Handle2, double Handle3)
		{
			return (Handle0 *( System.Math.Pow(( 1 - CurrenTime ), 3)) + ( 3 * Handle1) * (CurrenTime * System.Math.Pow(( 1 - CurrenTime ), 2)) + (3 * Handle2) *( System.Math.Pow(CurrenTime, 2)* ( 1 - CurrenTime )) + Handle3 * System.Math.Pow(CurrenTime, 3));	               
		}
		
		//spherical quaternion interpolation
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
		
		public static Vector3D Cartesian(double pitch, double yaw, double length)
		{
			double sinp = length * Math.Sin(pitch);
			
			return new Vector3D(sinp * Math.Cos(yaw), sinp * Math.Sin(yaw), length * Math.Cos(pitch));
		}
		
		public static Vector3D CartesianVVVV(double pitch, double yaw, double length)
		{
			double cosp = - length * Math.Cos(pitch);
			
			return new Vector3D( cosp * Math.Sin(yaw), length * Math.Sin(pitch), cosp * Math.Cos(yaw));
		}
		
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
		
		//translation
		public static Matrix4x4 Translate(double x, double y, double z)
		{
			return new Matrix4x4(1, 0, 0, 0,
			                     0, 1, 0, 0,
			                     0, 0, 1, 0,
			                     x, y, z, 1);
		}
		
		public static Matrix4x4 Translate(Vector3D v)
		{
			return new Matrix4x4(1, 0, 0, 0,
			                     0, 1, 0, 0,
			                     0, 0, 1, 0,
			                     v.x, v.y, v.z, 1);
		}
		
		//scaling
		public static Matrix4x4 Scale(double x, double y, double z)
		{
			return new Matrix4x4(x, 0, 0, 0,
			                     0, y, 0, 0,
			                     0, 0, z, 0,
			                     0, 0, 0, 1);
		}
		
		public static Matrix4x4 Scale(Vector3D v)
		{
			return new Matrix4x4(v.x,   0,   0, 0,
			                       0, v.y,   0, 0,
			                       0,   0, v.z, 0,
			                       0,   0,   0, 1);
		}
		
		//rotation
		public static Matrix4x4 RotateX(double rotX)
		{
			double s = Math.Sin(rotX);
			double c = Math.Cos(rotX);

			return new Matrix4x4(1,  0, 0, 0,
			                     0,  c, s, 0,
			                     0, -s, c, 0,
			                     0,  0, 0, 1);
		}
		
		public static Matrix4x4 RotateY(double rotY)
		{
			double s = Math.Sin(rotY);
			double c = Math.Cos(rotY);

			return new Matrix4x4(c, 0, -s, 0,
			                     0, 1,  0, 0,
			                     s, 0,  c, 0,
			                     0, 0,  0, 1);
		}
		
		public static Matrix4x4 RotateZ(double rotZ)
		{
			double s = Math.Sin(rotZ);
			double c = Math.Cos(rotZ);

			return new Matrix4x4( c, s, 0, 0,
			                     -s, c, 0, 0,
			                      0, 0, 1, 0,
			                      0, 0, 0, 1);
		}
		
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
		
		//transform
		public static Matrix4x4 Transform(double transX, double transY, double transZ, 
		                                  double scaleX, double scaleY, double scaleZ,
		                                  double   rotX, double   rotY, double   rotZ)
		{
			return  Translate(transX, transY, transZ) * Scale(scaleX, scaleY, scaleZ) * Rotate(rotX, rotY, rotZ); 
		}
		
		public static Matrix4x4 Transform(Vector3D trans, Vector3D scale, Vector3D rot)
		{
			return  Translate(trans.x, trans.y, trans.z) * Scale(scale.x, scale.y, scale.z) * Rotate(rot.x, rot.y, rot.z); 
		}
		
		public static Matrix4x4 Transpose(Matrix4x4 A)
		{
			return new Matrix4x4(A.m11, A.m21, A.m31, A.m41,
			                     A.m12, A.m22, A.m32, A.m42,
			                     A.m13, A.m23, A.m33, A.m43,
			                     A.m14, A.m24, A.m34, A.m44);
		}

		public static Matrix4x4 Inverse(Matrix4x4 A)
		{
			
			// optimized 4x4 matrix inversion using cramer's rule, found in the game engine http://www.ogre3d.org
			// takes about 1,8ns to execute on intel core2 duo 2Ghz, the intel reference
			// implementation (not assembly optimized) was about 2,2ns.
			// http://www.intel.com/design/pentiumiii/sml/24504301.pdf
			
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

