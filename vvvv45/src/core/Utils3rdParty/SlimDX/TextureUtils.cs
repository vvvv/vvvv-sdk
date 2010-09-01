using System;
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
	/// Method delegate for pixelshader like in place texture fill functions.
	/// </summary>
	public unsafe delegate void TextureFillFunctionInPlace(uint* data, int row, int col, int width, int height);
	
	/// <summary>
	/// Provides some utils to work with DX textures
	/// </summary>
	public static class TextureUtils
	{
		public static Texture CreateTexture(Device device, int width, int height)
		{
			return new Texture(device, width, height, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
		}
		
		public static Texture CreateTextureNoAlpha(Device device, int width, int height)
		{
			return new Texture(device, width, height, 1, Usage.None, Format.X8R8G8B8, Pool.Managed);
		}
		
		//pixel access via pointers
		public unsafe static uint GetPtrVal2D(uint* data, int row, int col, int width)
		{
			return data[row * width + col];
		}
		
		public unsafe static void SetPtrVal2D(uint* data, uint value, int row, int col, int width)
		{
			data[row * width + col] = value;
		}
		
		public unsafe static uint GetArrayVal2D(this uint[] data, int row, int col, int width)
		{
			return data[row * width + col];
		}
		
		public unsafe static void SetArrayVal2D(this uint[] data, uint value, int row, int col, int width)
		{
			data[row * width + col] = value;
		}
		
		//copy texture pixels to an array
		public static uint[] Copy32BitTexToArray(IntPtr src, int size)
		{
			var ret = new uint[size];
			Marshal.Copy(src, (int[])((object)ret), 0, size);
			return ret;
		}
		
		public unsafe static void Fill32BitTex(Texture tex, TextureFillFunction fillFunc)
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
			var oldData = Copy32BitTexToArray(rect.Data.DataPointer, pixelCount);

			//call the given function for each pixel
			for(int i=0; i<height; i++)
				for(int j=0; j<width; j++)
					fillFunc(oldData, data, i, j, width, height);
			
			//unlock texture
			tex.UnlockRectangle(0);
		}
		
		public unsafe static void Fill32BitTexParallel(Texture tex, TextureFillFunction fillFunc)
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
			var oldData = Copy32BitTexToArray(rect.Data.DataPointer, pixelCount);

			//call the given function for each pixel
			Parallel.For(0, height, i =>
            {
            	for(int j=0; j<width; j++)
            		fillFunc(oldData, data, i, j, width, height);
            });
			
			//unlock texture
			tex.UnlockRectangle(0);
			
		}
		
		public unsafe static void Fill32BitTexInPlace(Texture tex, TextureFillFunctionInPlace fillFunc)
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
					fillFunc(data, i, j, width, height);
			
			//unlock texture
			tex.UnlockRectangle(0);
			
		}
	}
}
