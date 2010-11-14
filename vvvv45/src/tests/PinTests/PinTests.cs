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
		static RGBAColor[] SampleDataColor = new RGBAColor[] { new RGBAColor(), new RGBAColor(), new RGBAColor() };
		static SliceMode[] SampleDataEnum = new SliceMode[] { SliceMode.Dynamic, SliceMode.Single, SliceMode.Single };
		static Matrix4x4[] SampleDataMatrix4x4 = new Matrix4x4[] { new Matrix4x4(), new Matrix4x4(), new Matrix4x4() };
		static Matrix[] SampleDataMatrix = new Matrix[] { new Matrix(), new Matrix(), new Matrix() };
		static Vector2[] SampleDataVector2 = new Vector2[] { new Vector2(), new Vector2(), new Vector2() };
		static Vector3[] SampleDataVector3 = new Vector3[] { new Vector3(), new Vector3(), new Vector3() };
		static Vector4[] SampleDataVector4 = new Vector4[] { new Vector4(), new Vector4(), new Vector4() };
		static Vector2D[] SampleDataVector2D = new Vector2D[] { new Vector2D(), new Vector2D(), new Vector2D() };
		static Vector3D[] SampleDataVector3D = new Vector3D[] { new Vector3D(), new Vector3D(), new Vector3D() };
		static Vector4D[] SampleDataVector4D = new Vector4D[] { new Vector4D(), new Vector4D(), new Vector4D() };
		
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
			TestInputPin<RGBAColor>(SampleDataColor);
		}
		
		[Test]
		public void TestInputPinEnum()
		{
			TestInputPin<SliceMode>(SampleDataEnum);
		}
		
		[Test]
		public void TestInputPinMatrix4x4()
		{
			TestInputPin<Matrix4x4>(SampleDataMatrix4x4);
		}
		
		[Test]
		public void TestInputPinVector2D()
		{
			TestInputPin<Vector2D>(SampleDataVector2D);
		}
		
		[Test]
		public void TestInputPinVector3D()
		{
			TestInputPin<Vector3D>(SampleDataVector3D);
		}
		
		[Test]
		public void TestInputPinVector4D()
		{
			TestInputPin<Vector4D>(SampleDataVector4D);
		}
		
		[Test]
		public void TestInputPinMatrix()
		{
			TestInputPin<Matrix>(SampleDataMatrix);
		}
		
		[Test]
		public void TestInputPinVector2()
		{
			TestInputPin<Vector2>(SampleDataVector2);
		}
		
		[Test]
		public void TestInputPinVector3()
		{
			TestInputPin<Vector3>(SampleDataVector3);
		}
		
		[Test]
		public void TestInputPinVector4()
		{
			TestInputPin<Vector4>(SampleDataVector4);
		}
		
		#endregion
		
		#region DiffInput
		
		[Test]
		public void TestDiffInputPinDouble()
		{
			TestDiffInputPin<double>(SampleDataDouble);
		}
		
		[Test]
		public void TestDiffInputPinFloat()
		{
			TestDiffInputPin<float>(SampleDataFloat);
		}
		
		[Test]
		public void TestDiffInputPinInt()
		{
			TestDiffInputPin<int>(new int[] { 0, -1, 1 });
		}
		
		[Test]
		public void TestDiffInputPinBool()
		{
			TestDiffInputPin<bool>(new bool[] { false, true, false });
		}
		
		[Test]
		public void TestDiffInputPinString()
		{
			TestDiffInputPin<string>(SampleDataString);
		}
		
		[Test]
		public void TestDiffInputPinColor()
		{
			TestDiffInputPin<RGBAColor>(SampleDataColor);
		}
		
		[Test]
		public void TestDiffInputPinEnum()
		{
			TestDiffInputPin<SliceMode>(SampleDataEnum);
		}
		
//		[Test]
//		public void TestDiffInputPinMatrix4x4()
//		{
//			TestDiffInputPin<Matrix4x4>(SampleDataMatrix4x4);
//		}
		
		[Test]
		public void TestDiffInputPinVector2D()
		{
			TestDiffInputPin<Vector2D>(SampleDataVector2D);
		}
		
		[Test]
		public void TestDiffInputPinVector3D()
		{
			TestDiffInputPin<Vector3D>(SampleDataVector3D);
		}
		
		[Test]
		public void TestDiffInputPinVector4D()
		{
			TestDiffInputPin<Vector4D>(SampleDataVector4D);
		}
		
//		[Test]
//		public void TestDiffInputPinMatrix()
//		{
//			TestDiffInputPin<Matrix>(SampleDataMatrix);
//		}
		
		[Test]
		public void TestDiffInputPinVector2()
		{
			TestDiffInputPin<Vector2>(SampleDataVector2);
		}
		
		[Test]
		public void TestDiffInputPinVector3()
		{
			TestDiffInputPin<Vector3>(SampleDataVector3);
		}
		
		[Test]
		public void TestDiffInputPinVector4()
		{
			TestDiffInputPin<Vector4>(SampleDataVector4);
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
			TestOutputPin<RGBAColor>(SampleDataColor);
		}
		
		[Test]
		public void TestOutputPinEnum()
		{
			TestOutputPin<SliceMode>(SampleDataEnum);
		}
		
		[Test]
		public void TestOutputPinMatrix4x4()
		{
			TestOutputPin<Matrix4x4>(SampleDataMatrix4x4);
		}
		
		[Test]
		public void TestOutputPinVector2D()
		{
			TestOutputPin<Vector2D>(SampleDataVector2D);
		}
		
		[Test]
		public void TestOutputPinVector3D()
		{
			TestOutputPin<Vector3D>(SampleDataVector3D);
		}
		
		[Test]
		public void TestOutputPinVector4D()
		{
			TestOutputPin<Vector4D>(SampleDataVector4D);
		}
		
		[Test]
		public void TestOutputPinMatrix()
		{
			TestOutputPin<Matrix>(SampleDataMatrix);
		}
		
		[Test]
		public void TestOutputPinVector2()
		{
			TestOutputPin<Vector2>(SampleDataVector2);
		}
		
		[Test]
		public void TestOutputPinVector3()
		{
			TestOutputPin<Vector3>(SampleDataVector3);
		}
		
		[Test]
		public void TestOutputPinVector4()
		{
			TestOutputPin<Vector4>(SampleDataVector4);
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
			TestConfigPin<RGBAColor>(SampleDataColor);
		}
		
		[Test]
		public void TestConfigPinEnum()
		{
			TestConfigPin<SliceMode>(SampleDataEnum);
		}
		
		[Test]
		public void TestConfigPinMatrix4x4()
		{
			TestConfigPin<Matrix4x4>(SampleDataMatrix4x4);
		}
		
		[Test]
		public void TestConfigPinVector2D()
		{
			TestConfigPin<Vector2D>(SampleDataVector2D);
		}
		
		[Test]
		public void TestConfigPinVector3D()
		{
			TestConfigPin<Vector3D>(SampleDataVector3D);
		}
		
		[Test]
		public void TestConfigPinVector4D()
		{
			TestConfigPin<Vector4D>(SampleDataVector4D);
		}
		
		[Test]
		public void TestConfigPinMatrix()
		{
			TestConfigPin<Matrix>(SampleDataMatrix);
		}
		
		[Test]
		public void TestConfigPinVector2()
		{
			TestConfigPin<Vector2>(SampleDataVector2);
		}
		
		[Test]
		public void TestConfigPinVector3()
		{
			TestConfigPin<Vector3>(SampleDataVector3);
		}
		
		[Test]
		public void TestConfigPinVector4()
		{
			TestConfigPin<Vector4>(SampleDataVector4);
		}
		
		#endregion
		
		protected void TestInputPin<T>(T[] sampleData)
		{
			var pinName = string.Format("{0} Input", typeof(T));
			var attribute = new InputAttribute(pinName);
			
			ISpread<T> spread = PinFactory.CreateSpread<T>(FPluginHost, attribute);
			
			Assert.True(spread.SliceCount == 1);
			
			TestSpread(spread, sampleData);
			
			ISpread<ISpread<T>> spreadedSpread = PinFactory.CreateSpread<ISpread<T>>(FPluginHost, attribute);
			
			Assert.True(spreadedSpread.SliceCount == 1);
			
			TestSpread(spreadedSpread, new ISpread<T>[] { new Spread<T>(sampleData.ToList()), new Spread<T>(sampleData.ToList()) });
		}
		
		protected void TestDiffInputPin<T>(T[] sampleData)
		{
			var pinName = string.Format("{0} Input", typeof(T));
			var attribute = new InputAttribute(pinName);
			
			IDiffSpread<T> spread = PinFactory.CreateDiffSpread<T>(FPluginHost, attribute);
			
			Assert.True(spread.SliceCount == 1);
			
			TestSpread(spread, sampleData);
			TestDiffSpread(spread, sampleData);
			
			IDiffSpread<ISpread<T>> spreadedSpread = PinFactory.CreateDiffSpread<ISpread<T>>(FPluginHost, attribute);
			
			Assert.True(spreadedSpread.SliceCount == 1);
			
			var spreadedSampleData = new ISpread<T>[] { new Spread<T>(sampleData.ToList()), new Spread<T>(sampleData.ToList()) };
			TestSpread(spreadedSpread, spreadedSampleData);
			TestDiffSpread(spreadedSpread, spreadedSampleData);
		}
		
		protected void TestConfigPin<T>(T[] sampleData)
		{
			var pinName = string.Format("{0} Input", typeof(T));
			var attribute = new ConfigAttribute(pinName);
			
			ISpread<T> spread = PinFactory.CreateSpread<T>(FPluginHost, attribute);
			
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
			
			ISpread<T> spread = PinFactory.CreateSpread<T>(FPluginHost, attribute);
			
			Assert.True(spread.SliceCount == 1);
			
			TestSpread(spread, sampleData);
			
			ISpread<ISpread<T>> spreadedSpread = PinFactory.CreateSpread<ISpread<T>>(FPluginHost, attribute);
			
			Assert.True(spreadedSpread.SliceCount == 1);
			
			TestSpread(spreadedSpread, new ISpread<T>[] { new Spread<T>(sampleData.ToList()), new Spread<T>(sampleData.ToList()) });
		}
		
		protected void TestSpread<T>(ISpread<T> spread, T[] sampleData)
		{
			spread.SliceCount = 0;
			Assert.True(spread.SliceCount == 0);
			
			spread.AssignFrom(sampleData);
			Assert.True(spread.SliceCount == sampleData.Length);
			for (int i = 0; i < spread.SliceCount; i++)
			{
				Assert.True(spread[i].Equals(sampleData[i]));
				Assert.True(spread[i].Equals(spread[spread.SliceCount + i]));
			}
			
			var spreadAsList = spread.ToList();
			for (int i = 0; i < sampleData.Length; i++)
			{
				Assert.True(sampleData[i].Equals(spreadAsList[i]));
			}
			
			spread.SliceCount = sampleData.Length;
			Assert.True(spread.SliceCount == sampleData.Length);
			
			// Test writing
			for (int i = 0; i < sampleData.Length; i++)
				spread[i] = sampleData[i];
			
			// Test writing with index above SliceCount
			for (int i = 0; i < sampleData.Length; i++)
				spread[spread.SliceCount + i] = sampleData[i];
			
			// Test writing with index below SliceCount
			for (int i = 0; i < sampleData.Length; i++)
				spread[i - spread.SliceCount] = sampleData[i];
			
			// Test reading
			for (int i = 0; i < spread.SliceCount; i++)
			{
				Assert.True(spread[i].Equals(sampleData[i]));
				Assert.True(spread[i].Equals(spread[spread.SliceCount + i]));
			}
			
			// Data should not get lost by increasing SliceCount
			spread.SliceCount++;
			for (int i = 0; i < sampleData.Length; i++)
				Assert.AreEqual(sampleData[i], spread[i]);
			
			// Data should not get lost by decreasing SliceCount
			spread.SliceCount = 1;
			Assert.True(spread.SliceCount == 1);
			Assert.AreEqual(sampleData[0], spread[0]);
			
			bool eventRaised = false;
			
			
			var pin = spread as Pin<T>;
			if (pin != null)
			{
				pin.Updated +=
					delegate(object sender, EventArgs args)
				{
					eventRaised = true;
				};
				
				pin[0] = sampleData[0];
				pin.Update();
				pin[0] = default(T);
				pin.Update();
				pin[0] = sampleData[1];
				pin.Update();
				
				Assert.IsTrue(eventRaised, "Update event was not raised");
			}
		}
		
		protected void TestDiffSpread<T>(IDiffSpread<T> spread, T[] sampleData)
		{
			bool eventRaised = false;
			
			spread.Changed +=
				delegate(IDiffSpread<T> s)
			{
				eventRaised = true;
			};
			
			var pin = spread as DiffPin<T>;
			if (pin != null)
			{
				pin[0] = sampleData[0];
				pin.Update();
				pin[0] = default(T);
				pin.Update();
				pin[0] = sampleData[1];
				pin.Update();
				
				Assert.IsTrue(eventRaised, "Changed event was not raised");
			}
		}
	}
}
