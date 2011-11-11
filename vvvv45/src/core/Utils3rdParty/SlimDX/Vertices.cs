using System;
using System.Runtime.InteropServices;
using SlimDX;
using SlimDX.Direct3D9;

namespace VVVV.Utils.SlimDX
{
	#region vertices
	
	/// <summary>
	/// Simple vertex struct with position
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
    public struct SimpleVertex
    {
        /// <summary>
        /// Gets or sets the position of the vertex.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleVertex"/> struct.
        /// </summary>
        /// <param name="position">The position.</param>
        public SimpleVertex(Vector3 position)
        {
        	Position = position;
        }
        
        /// <summary>
        /// The vertex format for the device
        /// </summary>
        public static VertexFormat Format = VertexFormat.Position;
        
        /// <summary>
        /// The VertexElements to set up a VertexDeclaration
        /// </summary>
        public static VertexElement[] VertexElements = 
        {
        	new VertexElement(0,  0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0),
        	VertexElement.VertexDeclarationEnd
        };
        
        /// <summary>
        /// The data size in bytes
        /// </summary>
        public static int ByteSize = Marshal.SizeOf(typeof(SimpleVertex));
    }
    
    /// <summary>
    /// Vertex struct with position and color
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
    public struct ColoredVertex
    {
        /// <summary>
        /// Gets or sets the position of the vertex.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Gets or sets the color of the vertex.
        /// </summary>
        public int Color;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColoredVertex"/> struct.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="color">The color.</param>
        public ColoredVertex(Vector3 position, int color)
        {
            Position = position;
            Color = color;
        }
        
        /// <summary>
        /// The vertex format for the device
        /// </summary>
        public static VertexFormat Format = VertexFormat.Position | VertexFormat.Diffuse;
        
        /// <summary>
        /// The VertexElements to set up a VertexDeclaration
        /// </summary>
        public static VertexElement[] VertexElements = 
        {
        	new VertexElement(0,  0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0),
        	new VertexElement(0, 12, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 0),
        	VertexElement.VertexDeclarationEnd
        };
        
        /// <summary>
        /// The data size in bytes
        /// </summary>
        public static int ByteSize = Marshal.SizeOf(typeof(ColoredVertex));
    }
    
    /// <summary>
	/// Vertex struct with position and normal
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
    public struct NormalVertex
    {
        /// <summary>
        /// Gets or sets the position of the vertex.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Gets or sets the normal of the vertex.
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// Initializes a new instance of the <see cref="TexturedVertex"/> struct.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="normal">The normal.</param>
        public NormalVertex(Vector3 position, Vector3 normal)
        {
            Position = position;
            Normal = normal;
        }
        
        /// <summary>
        /// The vertex format for the device
        /// </summary>
        public static VertexFormat Format = VertexFormat.Position | VertexFormat.Normal;
        
        /// <summary>
        /// The VertexElements to set up a VertexDeclaration
        /// </summary>
        public static VertexElement[] VertexElements = 
        {
        	new VertexElement(0,  0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0),
        	new VertexElement(0, 12, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Normal, 0),
        	VertexElement.VertexDeclarationEnd
        };
        
        /// <summary>
        /// The data size in bytes
        /// </summary>
        public static int ByteSize = Marshal.SizeOf(typeof(NormalVertex));
    }
    
    /// <summary>
	/// Vertex struct with position and texture coordinate
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
    public struct TexturedVertex
    {
        /// <summary>
        /// Gets or sets the position of the vertex.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Gets or sets the teture coordinate of the vertex.
        /// </summary>
        public Vector2 TextureCoordinate;

        /// <summary>
        /// Initializes a new instance of the <see cref="TexturedVertex"/> struct.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="texCd">The texture coordinate.</param>
        public TexturedVertex(Vector3 position, Vector2 texCd)
        {
            Position = position;
            TextureCoordinate = texCd;
        }
        
        /// <summary>
        /// The vertex format for the device
        /// </summary>
        public static VertexFormat Format = VertexFormat.Position | VertexFormat.Texture1;
        
        /// <summary>
        /// The VertexElements to set up a VertexDeclaration
        /// </summary>
        public static VertexElement[] VertexElements = 
        {
        	new VertexElement(0,  0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0),
        	new VertexElement(0, 12, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0),
        	VertexElement.VertexDeclarationEnd
        };
        
        /// <summary>
        /// The data size in bytes
        /// </summary>
        public static int ByteSize = Marshal.SizeOf(typeof(TexturedVertex));
    }
    
    /// <summary>
	/// Vertex struct with position, normal and texture coordinate
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
    public struct TexturedNormalVertex
    {
        /// <summary>
        /// Gets or sets the position of the vertex.
        /// </summary>
        public Vector3 Position;
        
        /// <summary>
        /// Gets or sets the normal of the vertex.
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// Gets or sets the teture coordinate of the vertex.
        /// </summary>
        public Vector2 TextureCoordinate;

        /// <summary>
        /// Initializes a new instance of the <see cref="TexturedVertex"/> struct.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="normal">The normal.</param>
		/// <param name="texCd">The texture coordinate.</param>
        public TexturedNormalVertex(Vector3 position, Vector3 normal, Vector2 texCd)
        {
            Position = position;
            Normal = normal;
            TextureCoordinate = texCd;
        }
        
        /// <summary>
        /// The vertex format for the device
        /// </summary>
        public static VertexFormat Format = VertexFormat.Position | VertexFormat.Normal | VertexFormat.Texture1;
        
        /// <summary>
        /// The VertexElements to set up a VertexDeclaration
        /// </summary>
        public static VertexElement[] VertexElements = 
        {
        	new VertexElement(0,  0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0),
        	new VertexElement(0, 12, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Normal, 0),
        	new VertexElement(0, 24, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0),
        	VertexElement.VertexDeclarationEnd
        };
        
        /// <summary>
        /// The data size in bytes
        /// </summary>
        public static int ByteSize = Marshal.SizeOf(typeof(TexturedNormalVertex));
    }
    
    #endregion vertices
    
    #region screen space vertices
	
	/// <summary>
	/// Simple vertex struct with screen space position
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
    public struct TransformedSimpleVertex
    {
        /// <summary>
        /// Gets or sets the position of the vertex.
        /// </summary>
        public Vector4 Position;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformedSimpleVertex"/> struct.
        /// </summary>
        /// <param name="position">The position.</param>
        public TransformedSimpleVertex(Vector4 position)
        {
            Position = position;
        }
        
        /// <summary>
        /// The vertex format for the device
        /// </summary>
        public static VertexFormat Format = VertexFormat.PositionRhw;
        
         /// <summary>
        /// The VertexElements to set up a VertexDeclaration
        /// </summary>
        public static VertexElement[] VertexElements = 
        {
        	new VertexElement(0,  0, DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.PositionTransformed, 0),
        	VertexElement.VertexDeclarationEnd
        };
        
        /// <summary>
        /// The data size in bytes
        /// </summary>
        public static int ByteSize = Marshal.SizeOf(typeof(TransformedSimpleVertex));
    }
	
	/// <summary>
	/// Vertex struct with screen space position and color
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
    public struct TransformedColoredVertex
    {
        /// <summary>
        /// Gets or sets the position of the vertex.
        /// </summary>
        public Vector4 Position;

        /// <summary>
        /// Gets or sets the color of the vertex.
        /// </summary>
        public int Color;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColoredVertex"/> struct.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="color">The color.</param>
        public TransformedColoredVertex(Vector4 position, int color)
        {
            Position = position;
            Color = color;
        }
        
        /// <summary>
        /// The vertex format for the device
        /// </summary>
        public static VertexFormat Format = VertexFormat.PositionRhw | VertexFormat.Diffuse;
        
        /// <summary>
        /// The VertexElements to set up a VertexDeclaration
        /// </summary>
        public static VertexElement[] VertexElements = 
        {
        	new VertexElement(0,  0, DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.PositionTransformed, 0),
        	new VertexElement(0, 16, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 0),
        	VertexElement.VertexDeclarationEnd
        };
        
        /// <summary>
        /// The data size in bytes
        /// </summary>
        public static int ByteSize = Marshal.SizeOf(typeof(TransformedColoredVertex));
    }
    
    /// <summary>
	/// Vertex struct with screen space position and normal
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
    public struct TransformedNormalVertex
    {
        /// <summary>
        /// Gets or sets the position of the vertex.
        /// </summary>
        public Vector4 Position;

        /// <summary>
        /// Gets or sets the normal of the vertex.
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// Initializes a new instance of the <see cref="TexturedVertex"/> struct.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="normal">The normal.</param>
        public TransformedNormalVertex(Vector4 position, Vector3 normal)
        {
            Position = position;
            Normal = normal;
        }
        
        /// <summary>
        /// The vertex format for the device
        /// </summary>
        public static VertexFormat Format = VertexFormat.PositionRhw | VertexFormat.Normal;
        
        /// <summary>
        /// The VertexElements to set up a VertexDeclaration
        /// </summary>
        public static VertexElement[] VertexElements = 
        {
        	new VertexElement(0,  0, DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.PositionTransformed, 0),
        	new VertexElement(0, 16, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Normal, 0),
        	VertexElement.VertexDeclarationEnd
        };
        
        /// <summary>
        /// The data size in bytes
        /// </summary>
        public static int ByteSize = Marshal.SizeOf(typeof(TransformedNormalVertex));
    }
    
    /// <summary>
	/// Vertex struct with screen space position and texture coordinate
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
    public struct TransformedTexturedVertex
    {
        /// <summary>
        /// Gets or sets the position of the vertex.
        /// </summary>
        public Vector4 Position;

        /// <summary>
        /// Gets or sets the teture coordinate of the vertex.
        /// </summary>
        public Vector2 TextureCoordinate;

        /// <summary>
        /// Initializes a new instance of the <see cref="TexturedVertex"/> struct.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="texCd">The texture coordinate.</param>
        public TransformedTexturedVertex(Vector4 position, Vector2 texCd)
        {
            Position = position;
            TextureCoordinate = texCd;
        }
        
        /// <summary>
        /// The vertex format for the device
        /// </summary>
        public static VertexFormat Format = VertexFormat.PositionRhw | VertexFormat.Texture1;
        
        /// <summary>
        /// The VertexElements to set up a VertexDeclaration
        /// </summary>
        public static VertexElement[] VertexElements = 
        {
        	new VertexElement(0,  0, DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.PositionTransformed, 0),
        	new VertexElement(0, 16, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0),
        	VertexElement.VertexDeclarationEnd
        };
        
        /// <summary>
        /// The data size in bytes
        /// </summary>
        public static int ByteSize = Marshal.SizeOf(typeof(TransformedTexturedVertex));
    }
    
    /// <summary>
	/// Vertex struct with screen space position, normal and texture coordinate
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
    public struct TransformedTexturedNormalVertex
    {
        /// <summary>
        /// Gets or sets the position of the vertex.
        /// </summary>
        public Vector4 Position;
        
        /// <summary>
        /// Gets or sets the normal of the vertex.
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// Gets or sets the teture coordinate of the vertex.
        /// </summary>
        public Vector2 TextureCoordinate;

        /// <summary>
        /// Initializes a new instance of the <see cref="TexturedVertex"/> struct.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="normal">The normal.</param>
		/// <param name="texCd">The texture coordinate.</param>
        public TransformedTexturedNormalVertex(Vector4 position, Vector3 normal, Vector2 texCd)
        {
            Position = position;
            Normal = normal;
            TextureCoordinate = texCd;
        }
        
        /// <summary>
        /// The vertex format for the device
        /// </summary>
        public static VertexFormat Format = VertexFormat.PositionRhw | VertexFormat.Normal | VertexFormat.Texture1;
        
         /// <summary>
        /// The VertexElements to set up a VertexDeclaration
        /// </summary>
        public static VertexElement[] VertexElements = 
        {
        	new VertexElement(0,  0, DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.PositionTransformed, 0),
        	new VertexElement(0, 16, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Normal, 0),
        	new VertexElement(0, 28, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0),
        	VertexElement.VertexDeclarationEnd
        };
        
        /// <summary>
        /// The data size in bytes
        /// </summary>
        public static int ByteSize = Marshal.SizeOf(typeof(TransformedTexturedNormalVertex));
    }
    
    #endregion screen space vertices
}
