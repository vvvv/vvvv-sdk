using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using SlimDX.Direct3D9;

namespace VVVV.Utils.SlimDX
{
	/// <summary>
	/// Method delegate for pixelshader like texture fill functions.
	/// </summary>
	public unsafe delegate void TextureFillFunction(uint[] oldData, uint* data, int row, int col, int width, int height);
	
	/// <summary>
	/// Method delegate for pixelshader like texture fill functions carrying user defined metadata in its arguments.
	/// </summary>
	public unsafe delegate void TextureFillFunction<TMetadata>(uint[] oldData, uint* data, int row, int col, int width, int height, TMetadata metadata);
	
	/// <summary>
	/// Method delegate for pixelshader like in place texture fill functions.
	/// </summary>
	public unsafe delegate void TextureFillFunctionInPlace(uint* data, int row, int col, int width, int height);
	
	/// <summary>
	/// Method delegate for pixelshader like in place texture fill functions carrying user defined metadata in its arguments.
	/// </summary>
	public unsafe delegate void TextureFillFunctionInPlace<TMetadata>(uint* data, int row, int col, int width, int height, TMetadata metadata);
	
	/// <summary>
	/// Provides some utils to work with DX textures
	/// </summary>
	public static class TextureUtils
	{
		/// <summary>
		/// Create a <see cref="Texture">texture</see> of <paramref name="width" />
		/// and <paramref name="height" /> on <paramref name="device" />.
		/// </summary>
		/// <param name="device">The device to create the texture on.</param>
		/// <param name="width">The width of the texture.</param>
		/// <param name="height">The height of the texture.</param>
		/// <returns>The newly created <see cref="Texture">texture</see>.</returns>
		public static Texture CreateTexture(Device device, int width, int height)
		{
			var pool = Pool.Managed;
			var usage = Usage.None;
			if (device is DeviceEx)
			{
				pool = Pool.Default;
				usage = Usage.Dynamic;
			}				
			
			return new Texture(device, width, height, 1, usage, Format.A8R8G8B8, pool);
		}
		
		/// <summary>
		/// Create a <see cref="Texture">texture</see> without an alpha channel of <paramref name="width" />
		/// and <paramref name="height" /> on <paramref name="device" />.
		/// </summary>
		/// <param name="device">The device to create the texture on.</param>
		/// <param name="width">The width of the texture.</param>
		/// <param name="height">The height of the texture.</param>
		/// <returns>The newly created <see cref="Texture">texture</see>.</returns>
		public static Texture CreateTextureNoAlpha(Device device, int width, int height)
		{
			var pool = Pool.Managed;
			var usage = Usage.None;
			if (device is DeviceEx)
			{
				pool = Pool.Default;
				usage = Usage.Dynamic;
			}
			
			return new Texture(device, width, height, 1, usage, Format.X8R8G8B8, pool);
		}
		
		/// <summary>
		/// Create a <see cref="Texture">texture</see> of <paramref name="width" />
		/// and <paramref name="height" /> on <paramref name="device" />
		/// and fill it with <paramref name="argbColor" />.
		/// </summary>
		/// <param name="device">The device to create the texture on.</param>
		/// <param name="width">The width of the texture.</param>
		/// <param name="height">The height of the texture.</param>
		/// <param name="argbColor">The color to fill the texture with.</param>
		/// <returns>The newly created <see cref="Texture">texture</see>.</returns>
		public static Texture CreateColoredTexture(Device device, int width, int height, uint argbColor)
		{
			var pool = Pool.Managed;
			var usage = Usage.None;
			if (device is DeviceEx)
			{
				pool = Pool.Default;
				usage = Usage.Dynamic;
			}
			
			var t = new Texture(device, width, height, 1, usage, Format.A8R8G8B8, pool);
			var rect = t.LockRectangle(0, LockFlags.None).Data;
			
			for (int i=0; i<(width*height); i++)
			{
				rect.Write(argbColor);
			}
			
			t.UnlockRectangle(0);
			return t;
		}
		
		/// <summary>
		/// Copies all pixels of the bitmap into the texture.
		/// no checks are done, make sure the pixel count of the bitmap and texture matches
		/// </summary>
		/// <param name="bm"></param>
		/// <param name="texture"></param>
		public static void CopyBitmapToTexture(Bitmap bm, Texture texture)
		{
			var data = bm.LockBits(new Rectangle(0, 0, bm.Width, bm.Height), ImageLockMode.ReadOnly, bm.PixelFormat);
			var rect = texture.LockRectangle(0, LockFlags.None);
				
			if(rect.Pitch == data.Stride)
			{
				Memory.Copy(rect.Data.DataPointer, data.Scan0, (uint)(data.Stride * data.Height));
			}
			else
			{
				//copy full lines
				for (int i = 0; i < data.Height; i++) 
				{
                    Memory.Copy(rect.Data.DataPointer.Move(rect.Pitch * i), data.Scan0.Move(data.Stride * i), (uint)data.Stride);
				}
			}
			
			texture.UnlockRectangle(0);
			bm.UnlockBits(data);
		}
		
		/// <summary>
		/// Retrievies the value at position <paramref name="row" />, <paramref name="col" />
		/// from a data buffer of width <paramref name="width" />.
		/// </summary>
		/// <param name="data">The pointer to the data buffer to retrieve the value from.</param>
		/// <param name="row">The row.</param>
		/// <param name="col">The column.</param>
		/// <param name="width">The width of the data buffer.</param>
		/// <returns>The value at position row, col.</returns>
		public unsafe static uint GetPtrVal2D(uint* data, int row, int col, int width)
		{
			return data[row * width + col];
		}
		
		/// <summary>
		/// Sets the value at position <paramref name="row" />, <paramref name="col" />
		/// in a data buffer of width <paramref name="width" />.
		/// </summary>
		/// <param name="data">The pointer to the data buffer.</param>
		/// <param name="value">The value to set at position row, col.</param>
		/// <param name="row">The row.</param>
		/// <param name="col">The column.</param>
		/// <param name="width">The width of the data buffer.</param>
		public unsafe static void SetPtrVal2D(uint* data, uint value, int row, int col, int width)
		{
			data[row * width + col] = value;
		}
		
		/// <summary>
		/// Retrievies the value at position <paramref name="row" />, <paramref name="col" />
		/// from a data array of width <paramref name="width" />.
		/// </summary>
		/// <param name="data">The data array to retrieve the value from.</param>
		/// <param name="row">The row.</param>
		/// <param name="col">The column.</param>
		/// <param name="width">The width of the data array.</param>
		/// <returns>The value at position row, col.</returns>
		public unsafe static uint GetArrayVal2D(this uint[] data, int row, int col, int width)
		{
			return data[row * width + col];
		}
		
		/// <summary>
		/// Sets the value at position <paramref name="row" />, <paramref name="col" /> in a data array of 
		/// width <paramref name="width" />.
		/// </summary>
		/// <param name="data">The data array.</param>
		/// <param name="value">The value to set at position row, col.</param>
		/// <param name="row">The row.</param>
		/// <param name="col">The column.</param>
		/// <param name="width">The width of the data array.</param>
		public unsafe static void SetArrayVal2D(this uint[] data, uint value, int row, int col, int width)
		{
			data[row * width + col] = value;
		}
		
		/// <summary>
		/// Copy texture pixels to an array.
		/// </summary>
		/// <param name="src">Pointer to the texture.</param>
		/// <param name="dest">Array to fill with the pixel data</param>
		/// <param name="size">The size of the resulting array.</param>
		public static void Copy32BitTexToArray(IntPtr src, uint[] dest, int size)
		{
			Marshal.Copy(src, (int[])((object)dest), 0, size);
		}
		
		/// <summary>
		/// Fill a 32 bit texture with values retrieved from the function fillFunc.
		/// </summary>
		/// <param name="tex">The texture to fill.</param>
		/// <param name="oldData">Array to fill with the old data.</param>
		/// <param name="metadata">User defined metadata to hand over to the fillFunc.</param>
		/// <param name="fillFunc">The function used to fill the texture.</param>
		public unsafe static void Fill32BitTex<TMetadata>(Texture tex, uint[] oldData, TMetadata metadata, TextureFillFunction<TMetadata> fillFunc)
		{
			//lock the texture pixel data
			var rect = tex.LockRectangle(0, LockFlags.None);
			
			//calculate sizes
			var byteLenght = (int)rect.Data.Length;
			var pixelCount = byteLenght/4;
			var width = rect.Pitch/4;
			var height = byteLenght/rect.Pitch;

			//get the pointer to the data
			var data = (uint*)rect.Data.DataPointer.ToPointer();

			//copy data to array, that we can replace the data
			Copy32BitTexToArray(rect.Data.DataPointer, oldData, pixelCount);

			//call the given function for each pixel
			for(int i=0; i<height; i++)
				for(int j=0; j<width; j++)
					fillFunc(oldData, data, i, j, width, height, metadata);
			
			//unlock texture
			tex.UnlockRectangle(0);
		}
		
		/// <summary>
		/// Fill a 32 bit texture with values retrieved from the function fillFunc.
		/// </summary>
		/// <param name="tex">The texture to fill.</param>
		/// <param name="oldData">Array to fill with the old data</param>
		/// <param name="fillFunc">The function used to fill the texture.</param>
		public unsafe static void Fill32BitTex(Texture tex, uint[] oldData, TextureFillFunction fillFunc)
		{
		    Fill32BitTex(tex, oldData, 0, (od, data, row, col, width, height, metadata) => fillFunc(od, data, row, col, width, height));
		}
		
		/// <summary>
		/// Fill a 32 bit texture in parallel with values retrieved from the function fillFunc.
		/// </summary>
		/// <param name="tex">The texture to fill.</param>
		/// <param name="oldData">Array to fill with the old data</param>
		/// <param name="metadata">User defined metadata to hand over to the fillFunc.</param>
		/// <param name="fillFunc">The function used to fill the texture.</param>
		public unsafe static void Fill32BitTexParallel<TMetadata>(Texture tex, uint[] oldData, TMetadata metadata, TextureFillFunction<TMetadata> fillFunc)
		{
			//lock the texture pixel data
			var rect = tex.LockRectangle(0, LockFlags.None);
			
			//calculate sizes
			var byteLenght = (int)rect.Data.Length;
			var pixelCount = byteLenght/4;
			var width = rect.Pitch/4;
			var height = byteLenght/rect.Pitch;

			//get the pointer to the data
			var data = (uint*)rect.Data.DataPointer.ToPointer();

			//copy data to array, that we can replace the data
			Copy32BitTexToArray(rect.Data.DataPointer, oldData, pixelCount);

			//call the given function for each pixel
			Parallel.For(0, height, i =>
            {
            	for(int j=0; j<width; j++)
            		fillFunc(oldData, data, i, j, width, height, metadata);
            });
			
			//unlock texture
			tex.UnlockRectangle(0);
			
		}
		
		/// <summary>
		/// Fill a 32 bit texture in parallel with values retrieved from the function fillFunc.
		/// </summary>
		/// <param name="tex">The texture to fill.</param>
		/// <param name="oldData">Array to fill with the old data</param>
		/// <param name="fillFunc">The function used to fill the texture.</param>
		public unsafe static void Fill32BitTexParallel(Texture tex, uint[] oldData, TextureFillFunction fillFunc)
		{
			Fill32BitTexParallel(tex, oldData, 0, (od, data, row, col, width, height, metadata) => fillFunc(od, data, row, col, width, height));
		}
		
		/// <summary>
		/// Fill a 32 bit texture in place with values retrieved from the function fillFunc.
		/// </summary>
		/// <param name="tex">The texture to fill.</param>
		/// <param name="metadata">User defined metadata to hand over to the fillFunc.</param>
		/// <param name="fillFunc">The function used to fill the texture.</param>
		public unsafe static void Fill32BitTexInPlace<TMetadata>(Texture tex, TMetadata metadata, TextureFillFunctionInPlace<TMetadata> fillFunc)
		{
			//lock the texture pixel data
			var rect = tex.LockRectangle(0, LockFlags.None);
			
			//calculate sizes
			var byteLenght = (int)rect.Data.Length;
			var width = rect.Pitch/4;
			var height = byteLenght/rect.Pitch;

			//get the pointer to the data
			var data = (uint*)rect.Data.DataPointer.ToPointer();

			//call the given function for each pixel
			for(int i=0; i<height; i++)
				for(int j=0; j<width; j++)
					fillFunc(data, i, j, width, height, metadata);
			
			//unlock texture
			tex.UnlockRectangle(0);
			
		}
		
		/// <summary>
		/// Fill a 32 bit texture in place with values retrieved from the function fillFunc.
		/// </summary>
		/// <param name="tex">The texture to fill.</param>
		/// <param name="fillFunc">The function used to fill the texture.</param>
		public unsafe static void Fill32BitTexInPlace(Texture tex, TextureFillFunctionInPlace fillFunc)
		{
			Fill32BitTexInPlace(tex, 0, (data, row, col, width, height, metadata) => fillFunc(data, row, col, width, height));
		}
		
		/// <summary>
		/// Fill a 32 bit texture parallel in place with values retrieved from the function fillFunc.
		/// </summary>
		/// <param name="tex">The texture to fill.</param>
		/// <param name="metadata">User defined metadata to hand over to the fillFunc.</param>
		/// <param name="fillFunc">The function used to fill the texture.</param>
		public unsafe static void Fill32BitTexInPlaceParallel<TMetadata>(Texture tex, TMetadata metadata, TextureFillFunctionInPlace<TMetadata> fillFunc)
		{
			//lock the texture pixel data
			var rect = tex.LockRectangle(0, LockFlags.None);
			
			//calculate sizes
			var byteLenght = (int)rect.Data.Length;
			var width = rect.Pitch/4;
			var height = byteLenght/rect.Pitch;

			//get the pointer to the data
			var data = (uint*)rect.Data.DataPointer.ToPointer();

			//call the given function for each pixel
			Parallel.For(0, height, i =>
            {
				for(int j=0; j<width; j++)
					fillFunc(data, i, j, width, height, metadata);
			});
			
			//unlock texture
			tex.UnlockRectangle(0);
			
		}
		
		/// <summary>
		/// Fill a 32 bit texture parallel in place with values retrieved from the function fillFunc.
		/// </summary>
		/// <param name="tex">The texture to fill.</param>
		/// <param name="fillFunc">The function used to fill the texture.</param>
		public unsafe static void Fill32BitTexInPlaceParallel(Texture tex, TextureFillFunctionInPlace fillFunc)
		{
			Fill32BitTexInPlaceParallel(tex, 0, (data, row, col, width, height, metadata) => fillFunc(data, row, col, width, height));
		}
	}
}
