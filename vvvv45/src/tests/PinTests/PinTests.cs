using System;
using System.Collections.Generic;
using Hoster;
using NUnit.Framework;
using SlimDX;
using VVVV.Hosting.Pins;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using System.Linq;

namespace PinTests
{
	[TestFixture]
	public class PinTests
	{
		static double[] SampleDataDouble = new double[] { 0.0, -1.0, 1.0 };
		static float[] SampleDataFloat = new float[] { 0.0f, -1.0f, 1.0f };
		static string[] SampleDataString = new string[] { "bar", "", "foo" };
		
		protected IPluginHost2 FPluginHost;
		
		[SetUp]
		public void Init()
		{
			FPluginHost = new PluginHost();
		}
		
		#region Input
		
		[Test]
		public void TestInputPinDouble()
		{
			TestInputPin<double>(SampleDataDouble);
		}
		
		[Test]
		public void TestInputPinFloat()
		{
			TestInputPin<float>(SampleDataFloat);
		}
		
		[Test]
		public void TestInputPinInt()
		{
			TestInputPin<int>(new int[] { 0, -1, 1 });
		}
		
		[Test]
		public void TestInputPinBool()
		{
			TestInputPin<bool>(new bool[] { false, true, false });
		}
		
		[Test]
		public void TestInputPinString()
		{
			TestInputPin<string>(SampleDataString);
		}
		
		[Test]
		public void TestInputPinColor()
		{
			TestInputPin<RGBAColor>(new RGBAColor[] { new RGBAColor() });
		}
		
		[Test]
		public void TestInputPinEnum()
		{
			TestInputPin<SliceMode>(new SliceMode[] { SliceMode.Dynamic, SliceMode.Single });
		}
		
		[Test]
		public void TestInputPinMatrix4x4()
		{
			TestInputPin<Matrix4x4>(new Matrix4x4[] { new Matrix4x4() });
		}
		
		[Test]
		public void TestInputPinVector2D()
		{
			TestInputPin<Vector2D>(new Vector2D[] { new Vector2D() });
		}
		
		[Test]
		public void TestInputPinVector3D()
		{
			TestInputPin<Vector3D>(new Vector3D[] { new Vector3D() });
		}
		
		[Test]
		public void TestInputPinVector4D()
		{
			TestInputPin<Vector4D>(new Vector4D[] { new Vector4D() });
		}
		
		[Test]
		public void TestInputPinMatrix()
		{
			TestInputPin<Matrix>(new Matrix[] { new Matrix() });
		}
		
		[Test]
		public void TestInputPinVector2()
		{
			TestInputPin<Vector2>(new Vector2[] { new Vector2() });
		}
		
		[Test]
		public void TestInputPinVector3()
		{
			TestInputPin<Vector3>(new Vector3[] { new Vector3() });
		}
		
		[Test]
		public void TestInputPinVector4()
		{
			TestInputPin<Vector4>(new Vector4[] { new Vector4() });
		}
		
		#endregion
		
		#region Output
		
		[Test]
		public void TestOutputPinDouble()
		{
			TestOutputPin<double>(SampleDataDouble);
		}
		
		[Test]
		public void TestOutputPinFloat()
		{
			TestOutputPin<float>(SampleDataFloat);
		}
		
		[Test]
		public void TestOutputPinInt()
		{
			TestOutputPin<int>(new int[] { 0, -1, 1 });
		}
		
		[Test]
		public void TestOutputPinBool()
		{
			TestOutputPin<bool>(new bool[] { false, true, false });
		}
		
		[Test]
		public void TestOutputPinString()
		{
			TestOutputPin<string>(SampleDataString);
		}
		
		[Test]
		public void TestOutputPinColor()
		{
			TestOutputPin<RGBAColor>(new RGBAColor[] { new RGBAColor() });
		}
		
		[Test]
		public void TestOutputPinEnum()
		{
			TestOutputPin<SliceMode>(new SliceMode[] { SliceMode.Dynamic, SliceMode.Single });
		}
		
		[Test]
		public void TestOutputPinMatrix4x4()
		{
			TestOutputPin<Matrix4x4>(new Matrix4x4[] { new Matrix4x4() });
		}
		
		[Test]
		public void TestOutputPinVector2D()
		{
			TestOutputPin<Vector2D>(new Vector2D[] { new Vector2D() });
		}
		
		[Test]
		public void TestOutputPinVector3D()
		{
			TestOutputPin<Vector3D>(new Vector3D[] { new Vector3D() });
		}
		
		[Test]
		public void TestOutputPinVector4D()
		{
			TestOutputPin<Vector4D>(new Vector4D[] { new Vector4D() });
		}
		
		[Test]
		public void TestOutputPinMatrix()
		{
			TestOutputPin<Matrix>(new Matrix[] { new Matrix() });
		}
		
		[Test]
		public void TestOutputPinVector2()
		{
			TestOutputPin<Vector2>(new Vector2[] { new Vector2() });
		}
		
		[Test]
		public void TestOutputPinVector3()
		{
			TestOutputPin<Vector3>(new Vector3[] { new Vector3() });
		}
		
		[Test]
		public void TestOutputPinVector4()
		{
			TestOutputPin<Vector4>(new Vector4[] { new Vector4() });
		}
		
		#endregion
		
		#region Config
		
		[Test]
		public void TestConfigPinDouble()
		{
			TestConfigPin<double>(SampleDataDouble);
		}
		
		[Test]
		public void TestConfigPinFloat()
		{
			TestConfigPin<float>(SampleDataFloat);
		}
		
		[Test]
		public void TestConfigPinInt()
		{
			TestConfigPin<int>(new int[] { 0, -1, 1 });
		}
		
		[Test]
		public void TestConfigPinBool()
		{
			TestConfigPin<bool>(new bool[] { false, true, false });
		}
		
		[Test]
		public void TestConfigPinString()
		{
			TestConfigPin<string>(SampleDataString);
		}
		
		[Test]
		public void TestConfigPinColor()
		{
			TestConfigPin<RGBAColor>(new RGBAColor[] { new RGBAColor() });
		}
		
		[Test]
		public void TestConfigPinEnum()
		{
			TestConfigPin<SliceMode>(new SliceMode[] { SliceMode.Dynamic, SliceMode.Single });
		}
		
		[Test]
		public void TestConfigPinMatrix4x4()
		{
			TestConfigPin<Matrix4x4>(new Matrix4x4[] { new Matrix4x4() });
		}
		
		[Test]
		public void TestConfigPinVector2D()
		{
			TestConfigPin<Vector2D>(new Vector2D[] { new Vector2D() });
		}
		
		[Test]
		public void TestConfigPinVector3D()
		{
			TestConfigPin<Vector3D>(new Vector3D[] { new Vector3D() });
		}
		
		[Test]
		public void TestConfigPinVector4D()
		{
			TestConfigPin<Vector4D>(new Vector4D[] { new Vector4D() });
		}
		
		[Test]
		public void TestConfigPinMatrix()
		{
			TestConfigPin<Matrix>(new Matrix[] { new Matrix() });
		}
		
		[Test]
		public void TestConfigPinVector2()
		{
			TestConfigPin<Vector2>(new Vector2[] { new Vector2() });
		}
		
		[Test]
		public void TestConfigPinVector3()
		{
			TestConfigPin<Vector3>(new Vector3[] { new Vector3() });
		}
		
		[Test]
		public void TestConfigPinVector4()
		{
			TestConfigPin<Vector4>(new Vector4[] { new Vector4() });
		}
		
		#endregion
		
		protected void TestInputPin<T>(T[] sampleData)
		{
			var pinName = string.Format("{0} Input", typeof(T));
			var attribute = new InputAttribute(pinName);
			
			ISpread<T> spread = new InputWrapperPin<T>(FPluginHost, attribute);
			
			Assert.True(spread.SliceCount == 1);
			
			TestSpread(spread, sampleData);
			
			ISpread<ISpread<T>> spreadedSpread = new InputWrapperPin<ISpread<T>>(FPluginHost, attribute);
			
			Assert.True(spreadedSpread.SliceCount == 1);
			
			TestSpread(spreadedSpread, new ISpread<T>[] { new Spread<T>(sampleData.ToList()), new Spread<T>(sampleData.ToList()) });
		}
		
		protected void TestConfigPin<T>(T[] sampleData)
		{
			var pinName = string.Format("{0} Input", typeof(T));
			var attribute = new ConfigAttribute(pinName);
			
			ISpread<T> spread = new ConfigWrapperPin<T>(FPluginHost, attribute);
			
			Assert.True(spread.SliceCount == 1);
			
			TestSpread(spread, sampleData);
			
//			ISpread<ISpread<T>> spreadedSpread = new ConfigWrapperPin<ISpread<T>>(FPluginHost, attribute);
//			
//			Assert.True(spreadedSpread.SliceCount == 1);
//			
//			TestSpread(spreadedSpread, new ISpread<T>[] { new Spread<T>(sampleData.ToList()), new Spread<T>(sampleData.ToList()) });
		}
		
		protected void TestOutputPin<T>(T[] sampleData)
		{
			var pinName = string.Format("{0} Input", typeof(T));
			var attribute = new OutputAttribute(pinName);
			
			ISpread<T> spread = new OutputWrapperPin<T>(FPluginHost, attribute);
			
			Assert.True(spread.SliceCount == 1);
			
			TestSpread(spread, sampleData);
			
			ISpread<ISpread<T>> spreadedSpread = new OutputWrapperPin<ISpread<T>>(FPluginHost, attribute);
			
			Assert.True(spreadedSpread.SliceCount == 1);
			
			TestSpread(spreadedSpread, new ISpread<T>[] { new Spread<T>(sampleData.ToList()), new Spread<T>(sampleData.ToList()) });
		}
		
		protected void TestSpread<T>(ISpread<T> spread, T[] sampleData)
		{
			spread.SliceCount = sampleData.Length;
			Assert.True(spread.SliceCount == sampleData.Length);
			
			// Test writing
			for (int i = 0; i < sampleData.Length; i++)
				spread[i] = sampleData[i];
			
			// Test writing with index above SliceCount
			for (int i = 0; i < sampleData.Length; i++)
				spread[spread.SliceCount + i] = sampleData[i];
			
			// Test reading
			for (int i = 0; i < spread.SliceCount; i++)
				Assert.True(spread[i].Equals(spread[spread.SliceCount + i]));
			
			spread.SliceCount = 1;
			Assert.True(spread.SliceCount == 1);
		}
	}
}
