#include "StdAfx.h"
#include "AssimpMesh.h"

namespace VVVV { namespace Assimp { namespace Lib {

AssimpMesh::AssimpMesh(void)
{
}

AssimpMesh::AssimpMesh(aiMesh* mesh)
{
	this->m_pMesh = mesh;
}


int AssimpMesh::MaterialIndex::get()
{
	return this->m_pMesh->mMaterialIndex;
}

int AssimpMesh::VerticesCount::get()
{
	return this->m_pMesh->mNumVertices;
}



List<int>^ AssimpMesh::Indices::get()
{
	if (this->m_indices == nullptr)
	{
		this->m_indices = gcnew List<int>();
		for (int i = 0; i < this->m_pMesh->mNumFaces ; i++)
		{
			aiFace f = this->m_pMesh->mFaces[i];
			
			if (f.mNumIndices == 3)
			{
				this->m_indices->Add(f.mIndices[0]);
				this->m_indices->Add(f.mIndices[1]);
				this->m_indices->Add(f.mIndices[2]);
			}
		}
	}
	return this->m_indices;
}

DataStream^ AssimpMesh::Vertices::get()
{
	if (this->m_vertices == nullptr)
	{
		int vertexsize = this->CalculateVertexSize();
		int totalsize = vertexsize * this->m_pMesh->mNumVertices;
		this->m_vertices = gcnew DataStream(totalsize,true,true);
	}
	
	return this->m_vertices;
}

void AssimpMesh::Write(DataStream^ vertices)
{
	aiColor4D col;
	aiVector3D v3;
	
	for (int i = 0; i < this->m_pMesh->mNumVertices; i++)
	{
		v3 = this->m_pMesh->mVertices[i];
		vertices->Write(v3.x);
		vertices->Write(v3.y);
		vertices->Write(v3.z);

		//Normal
		if (this->m_pMesh->HasNormals())
		{
			v3 = this->m_pMesh->mNormals[i];
			vertices->Write(v3.x);
			vertices->Write(v3.y);
			vertices->Write(v3.z);
		}

		//Tangents / Bitan
		if (this->m_pMesh->HasTangentsAndBitangents())
		{
			v3 = this->m_pMesh->mTangents[i];
			vertices->Write(v3.x);
			vertices->Write(v3.y);
			vertices->Write(v3.z);

			v3 = this->m_pMesh->mBitangents[i];
			vertices->Write(v3.x);
			vertices->Write(v3.y);
			vertices->Write(v3.z);
		}

		//Color
		for (int j = 0; j < m_pMesh->GetNumColorChannels(); j++)
		{
			col = this->m_pMesh->mColors[j][i];
			vertices->Write(col.r);
			vertices->Write(col.g);
			vertices->Write(col.b);
			vertices->Write(col.a);
		}

		//UV
		for (int j = 0; j < m_pMesh->GetNumUVChannels(); j++)
		{
			int numcomp = this->m_pMesh->mNumUVComponents[j];
			if (numcomp == 1)
			{
				v3 = this->m_pMesh->mTextureCoords[j][i];
				vertices->Write(v3.x);
			}
			if (numcomp == 2)
			{
				v3 = this->m_pMesh->mTextureCoords[j][i];
				vertices->Write(v3.x);
				vertices->Write(v3.y);
			}
			if (numcomp == 3)
			{
				v3 = this->m_pMesh->mTextureCoords[j][i];
				vertices->Write(v3.x);
				vertices->Write(v3.y);
				vertices->Write(v3.z);
			}
		}

	}
}

List<VertexElement>^ AssimpMesh::GetVertexBinding()
{
	List<VertexElement>^ result = gcnew List<SlimDX::Direct3D9::VertexElement>();

	int offset = 0;

	//Position
	VertexElement pos(0,offset,DeclarationType::Float3,DeclarationMethod::Default, DeclarationUsage::Position,0);
	offset += 12;
	result->Add(pos);

	//Normal
	if (this->m_pMesh->HasNormals())
	{
		VertexElement norm(0,offset,DeclarationType::Float3,DeclarationMethod::Default, DeclarationUsage::Normal,0);
		offset += 12;
		result->Add(norm);
	}

	//Tangents / Bitan
	if (this->m_pMesh->HasTangentsAndBitangents())
	{
		VertexElement tang(0,offset,DeclarationType::Float3,DeclarationMethod::Default, DeclarationUsage::Tangent,0);
		offset += 12;
		result->Add(tang);
		VertexElement bitan(0,offset,DeclarationType::Float3,DeclarationMethod::Default, DeclarationUsage::Binormal,0);
		offset += 12;
		result->Add(bitan);
	}

	for (int i = 0; i < m_pMesh->GetNumColorChannels(); i++)
	{
		VertexElement col(0,offset,DeclarationType::Float4,DeclarationMethod::Default, DeclarationUsage::Color,i);
		offset += 16;
		result->Add(col);
	}

	for (int i = 0; i < m_pMesh->GetNumUVChannels(); i++)
	{
		int numcomp = this->m_pMesh->mNumUVComponents[i];
		DeclarationType type = DeclarationType::Float1;
		int stride = 4;
		if (numcomp == 2)
		{
			type = DeclarationType::Float2;
			stride = 8;
		}
		if (numcomp == 3)
		{
			type = DeclarationType::Float3;
			stride = 12;
		}

		VertexElement tc(0,offset,type,DeclarationMethod::Default, DeclarationUsage::TextureCoordinate,i);
		offset += stride;
		result->Add(tc);
	}

	result->Add(VertexElement::VertexDeclarationEnd);

	return result;
}

int AssimpMesh::CalculateVertexSize()
{
	//Position
	int result = 3 * sizeof(float);

	//Add normals
	if (this->m_pMesh->HasNormals()) { result += 3 * sizeof(float); }

	//Tangent and bitangents (goes together)
	if (this->m_pMesh->HasTangentsAndBitangents()) { result += 6 * sizeof(float); }

	//Color
	result += this->m_pMesh->GetNumColorChannels() * 4 * sizeof(float);

	//Texture coordinates, need to iterate since can be 1/2/3d
	for (int i = 0; i < this->m_pMesh->GetNumUVChannels() ; i++)
	{
		result += this->m_pMesh->mNumUVComponents[i] * sizeof(float);
	}

	return result;
}


}}}