#include "StdAfx.h"
#include "AssimpMaterial.h"

namespace VVVV { namespace Assimp { namespace Lib {

AssimpMaterial::AssimpMaterial(void)
{
}

AssimpMaterial::AssimpMaterial(aiMaterial* material)
{
	this->m_pMaterial = material;

	this->m_texturepath = gcnew List<String^>();
	this->m_texturetype = gcnew List<eAssimpTextureType>();
	this->m_texturemapmode = gcnew List<eAssimpTextureMapMode>();
	this->m_textureop = gcnew List<eAssimpTextureOp>();

	aiString str;
	aiTextureMapMode mapmode;
	aiTextureOp oper;

	Array^ tt = Enum::GetValues(eAssimpTextureType::typeid);

	for (int e = 0 ; e < tt->Length; e++)
	{
		int textype = (int)tt->GetValue(e); 
		aiTextureType etextype =(aiTextureType)textype;
		
		for (int i = 0; i < this->m_pMaterial->GetTextureCount(etextype);i++)
		{
			this->m_pMaterial->GetTexture(etextype,i,&str,NULL,NULL,NULL,&oper,&mapmode);

			this->m_texturetype->Add((eAssimpTextureType)textype);
			this->m_texturemapmode->Add((eAssimpTextureMapMode)mapmode);
			this->m_textureop->Add((eAssimpTextureOp)oper);
			this->m_texturepath->Add(gcnew String(str.data));

		}		
	}

	aiColor3D ambient (0.f,0.f,0.f);
	this->m_pMaterial->Get(AI_MATKEY_COLOR_AMBIENT,ambient);
	this->m_ambient = Color4(1.0f,ambient.r,ambient.g,ambient.b);
	
	aiColor3D diffuse (0.f,0.f,0.f);
	this->m_pMaterial->Get(AI_MATKEY_COLOR_DIFFUSE,diffuse);
	this->m_diffuse = Color4(1.0f,diffuse.r,diffuse.g,diffuse.b);

	aiColor3D specular (0.f,0.f,0.f);
	this->m_pMaterial->Get(AI_MATKEY_COLOR_SPECULAR,specular);
	this->m_specular = Color4(1.0f,specular.r,specular.g,specular.b);

	float specularpower = 1.0f;
	this->m_pMaterial->Get(AI_MATKEY_SHININESS,specularpower);
	this->m_specularpower = specularpower;
}

}}}