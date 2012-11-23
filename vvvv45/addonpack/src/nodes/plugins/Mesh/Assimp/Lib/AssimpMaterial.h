#pragma once

namespace VVVV { namespace Assimp { namespace Lib {

	public enum class eAssimpTextureType { Diffuse = aiTextureType_DIFFUSE , Specular = aiTextureType_SPECULAR , Ambient = aiTextureType_AMBIENT, 
		Emissive =aiTextureType_EMISSIVE , Height = aiTextureType_HEIGHT , Normals = aiTextureType_NORMALS , Shininesss = aiTextureType_SHININESS ,
		Opacity = aiTextureType_OPACITY , Displacement = aiTextureType_DISPLACEMENT , LightMap = aiTextureType_LIGHTMAP , Reflection = aiTextureType_REFLECTION ,
		Unknown = aiTextureType_UNKNOWN 
	};

	public enum class eAssimpTextureMapMode
	{
		Wrap = aiTextureMapMode_Wrap , Clamp = aiTextureMapMode_Clamp , Decal = aiTextureMapMode_Decal , Mirror = aiTextureMapMode_Mirror 
	};

	public enum class eAssimpTextureOp
	{
		Multiply = aiTextureOp_Multiply , Add = aiTextureOp_Add , Substract = aiTextureOp_Subtract , Divide = aiTextureOp_Divide ,
		SmoothAdd = aiTextureOp_SmoothAdd , SignedAdd = aiTextureOp_SignedAdd 
	};

	public ref class AssimpMaterial
	{
	public:
		AssimpMaterial(void);
		property List<String^>^ TexturePath { List<String^>^ get() { return this->m_texturepath; } }
		property List<eAssimpTextureType>^ TextureType { List<eAssimpTextureType>^ get() { return this->m_texturetype; } }
		property List<eAssimpTextureMapMode>^ TextureMapMode { List<eAssimpTextureMapMode>^ get() { return this->m_texturemapmode; } }
		property List<eAssimpTextureOp>^ TextureOperation { List<eAssimpTextureOp>^ get() { return this->m_textureop; } }
		property SlimDX::Color4 AmbientColor { SlimDX::Color4 get() { return this->m_ambient; } }
		property SlimDX::Color4 DiffuseColor { SlimDX::Color4 get() { return this->m_diffuse; } }
		property SlimDX::Color4 SpecularColor { SlimDX::Color4 get() { return this->m_specular; } }
		property float SpecularPower { float get() { return this->m_specularpower; } }
	internal:
		AssimpMaterial(aiMaterial* material);
	private:
		aiMaterial* m_pMaterial;
		
		List<String^>^ m_texturepath;
		List<eAssimpTextureType>^ m_texturetype;
		List<eAssimpTextureMapMode>^ m_texturemapmode;
		List<eAssimpTextureOp>^ m_textureop;
		SlimDX::Color4 m_ambient;
		SlimDX::Color4 m_diffuse;
		SlimDX::Color4 m_specular;
		float m_specularpower;
	};

}}}

