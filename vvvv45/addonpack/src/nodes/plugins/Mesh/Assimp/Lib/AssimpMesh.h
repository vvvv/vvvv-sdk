#pragma once

namespace VVVV { namespace Assimp { namespace Lib {

public ref class AssimpMesh
{
public:
	AssimpMesh(void);
	property int MaterialIndex { int get(); }
	property int VerticesCount { int get(); }

	property List<int>^ Indices { List<int>^ get(); }

	property DataStream^ Vertices { DataStream^ get(); }

	void Write(DataStream^ vertices);

	List<SlimDX::Direct3D9::VertexElement>^ GetVertexBinding();
internal:
	AssimpMesh(aiMesh* mesh);

private:
	aiMesh* m_pMesh;

	//Cached Data
	List<int>^ m_indices;
	DataStream^ m_vertices;

	int CalculateVertexSize();
};

}}}