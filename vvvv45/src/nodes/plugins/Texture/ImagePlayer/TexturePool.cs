/*
 * Created by SharpDevelop.
 * User: nsynk
 * Date: 10.01.2012
 * Time: 16:51
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Concurrent;
using SlimDX.Direct3D9;

namespace VVVV.Nodes.ImagePlayer
{
	public class TexturePool : IDisposable
	{
		private readonly ConcurrentDictionary<int, ObjectPool<Texture>> FTexturePools = new ConcurrentDictionary<int, ObjectPool<Texture>>();
		
		public Texture GetTexture(
			Device device, 
			int width, 
			int height, 
			int levelCount,
			Usage usage,
			Format format,
			Pool pool)
		{
			int key = GetKey(device, width, height, levelCount, usage, format, pool);
			
			ObjectPool<Texture> texturePool = null;
			if (!FTexturePools.TryGetValue(key, out texturePool))
			{
				texturePool = new ObjectPool<Texture>(() => new Texture(device, width, height, levelCount, usage, format, pool));
				FTexturePools[key] = texturePool;
			}
			
			return texturePool.GetObject();
		}
		
		public void PutTexture(Texture texture)
		{
			var desc = texture.GetLevelDescription(0);
			int key = GetKey(
				texture.Device,
				desc.Width,
				desc.Height,
				texture.LevelCount,
				desc.Usage,
				desc.Format,
				desc.Pool);
			
			var texturePool = FTexturePools[key];
			texturePool.PutObject(texture);
		}
		
		private int GetKey(
			Device device, 
			int width, 
			int height, 
			int levelCount,
			Usage usage,
			Format format,
			Pool pool)
		{
			return 
				device.GetHashCode() ^ 
				width.GetHashCode() ^ 
				height.GetHashCode() ^ 
				levelCount.GetHashCode() ^ 
				usage.GetHashCode() ^
				format.GetHashCode() ^
				pool.GetHashCode();
		}
		
		public void Dispose()
		{
			foreach (var texturePool in FTexturePools.Values)
			{
				foreach (var texture in texturePool.ToArrayAndClear())
				{
					texture.Dispose();
				}
			}
		}
	}
}
