using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SlimDX.Direct3D9;

namespace VVVV.Nodes.ImagePlayer
{
    public class Frame : IDisposable
    {
    	private readonly FrameInfo FFrameInfo;
    	private readonly Texture[] FTextures;
    	private readonly TexturePool FTexturePool;
    	
    	public Frame(FrameInfo frameInfo, IEnumerable<Texture> textures, TexturePool texturePool)
    	{
    		FFrameInfo = frameInfo;
    		FTextures = textures.ToArray();
    		FTexturePool = texturePool;
    	}
    	
    	public FrameInfo Info
    	{
    		get
    		{
    			return FFrameInfo;
    		}
    	}
        
        public Texture GetTexture(Device device)
        {
        	return FTextures.FirstOrDefault(texture => texture.Device == device);
        }
        
        public void Dispose()
        {
        	FFrameInfo.Dispose();
        	
        	foreach (var texture in FTextures)
        	{
        		FTexturePool.PutTexture(texture);
        	}
        }
    }
}
