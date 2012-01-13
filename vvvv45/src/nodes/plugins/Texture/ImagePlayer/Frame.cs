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
    	private readonly List<Texture> FTextures;
    	private readonly TexturePool FTexturePool;
    	
    	public Frame(FrameInfo frameInfo, IEnumerable<Texture> textures, TexturePool texturePool)
    	{
    		FFrameInfo = frameInfo;
    		FTextures = textures.ToList();
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
        	var texture = FTextures.FirstOrDefault(t => t.Device == device);
        	if (texture == null)
        	{
        	    using (var stream = new FileStream(FFrameInfo.Filename, FileMode.Open, FileAccess.Read))
        	    {
        	        var decoder = FrameDecoder.Create(FFrameInfo, FTexturePool, stream);
        	        texture = decoder.Decode(device);
        	        FTextures.Add(texture);
        	    }
        	}
        	return texture;
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
