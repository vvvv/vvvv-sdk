using System;
using System.Collections.Generic;
using Hoster;
using NUnit.Framework;
using SlimDX;
using VVVV.Hosting.Pins;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace SpreadTests
{
	[TestFixture]
	public class PinTests
	{
		protected IPluginHost2 FPluginHost;
		
		[SetUp]
		public void Init()
		{
			FPluginHost = new PluginHost();
		}
		
		[Test]
		public void TestInputPins()
		{
			TestInputPin<double>();
			TestInputPin<float>();
			TestInputPin<int>();
			TestInputPin<bool>();
			TestInputPin<string>();
			TestInputPin<RGBAColor>();
			TestInputPin<SliceMode>();
			TestInputPin<Matrix4x4>();
			TestInputPin<Vector2D>();
			TestInputPin<Vector3D>();
			TestInputPin<Vector4D>();
			TestInputPin<Matrix>();
			TestInputPin<Vector2>();
			TestInputPin<Vector3>();
			TestInputPin<Vector4>();
		}
		
		[Test]
		public void TestConfigPins()
		{
			TestConfigPin<double>();
			TestConfigPin<float>();
			TestConfigPin<int>();
			TestConfigPin<bool>();
			TestConfigPin<string>();
			TestConfigPin<RGBAColor>();
			TestConfigPin<SliceMode>();
			TestConfigPin<Vector2D>();
			TestConfigPin<Vector3D>();
			TestConfigPin<Vector4D>();
			TestConfigPin<Vector2>();
			TestConfigPin<Vector3>();
			TestConfigPin<Vector4>();
		}
		
		[Test]
		public void TestOutputPins()
		{
			TestOutputPin<double>();
			TestOutputPin<float>();
			TestOutputPin<int>();
			TestOutputPin<bool>();
			TestOutputPin<string>();
			TestOutputPin<RGBAColor>();
			TestOutputPin<SliceMode>();
			TestOutputPin<Matrix4x4>();
			TestOutputPin<Vector2D>();
			TestOutputPin<Vector3D>();
			TestOutputPin<Vector4D>();
			TestOutputPin<Matrix>();
			TestOutputPin<Vector2>();
			TestOutputPin<Vector3>();
			TestOutputPin<Vector4>();
		}

		protected void TestInputPin<T>()
		{
			var pinName = string.Format("{0} Input", typeof(T));
			var attribute = new InputAttribute(pinName);
			
			ISpread<T> spread = new InputWrapperPin<T>(FPluginHost, attribute);
			
			Assert.True(spread.SliceCount == 1);
			
			TestSpread(spread);
		}
		
		protected void TestConfigPin<T>()
		{
			var pinName = string.Format("{0} Input", typeof(T));
			var attribute = new ConfigAttribute(pinName);
			
			ISpread<T> spread = new ConfigWrapperPin<T>(FPluginHost, attribute);
			
			Assert.True(spread.SliceCount == 1);
			
			TestSpread(spread);
		}
		
		protected void TestOutputPin<T>()
		{
			var pinName = string.Format("{0} Input", typeof(T));
			var attribute = new OutputAttribute(pinName);
			
			ISpread<T> spread = new OutputWrapperPin<T>(FPluginHost, attribute);
			
			Assert.True(spread.SliceCount == 1);
			
			TestSpread(spread);
		}
		
		protected void TestSpread<T>(ISpread<T> spread)
		{
			spread.SliceCount = 10;
			Assert.True(spread.SliceCount == 10);
			
			foreach (var slice in spread)
			{
				Assert.IsAssignableFrom(typeof(T), slice);
			}
			
			spread.SliceCount = 1;
			Assert.True(spread.SliceCount == 1);
		}
	}
}
