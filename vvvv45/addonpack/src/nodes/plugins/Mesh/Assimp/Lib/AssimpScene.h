#pragma once

#include "AssimpMesh.h"
#include "AssimpNode.h"
#include "AssimpCamera.h"
#include "AssimpMaterial.h"

namespace VVVV { namespace Assimp { namespace Lib {

	public ref class AssimpScene : IDisposable
{
public:
	AssimpScene(void);
	~AssimpScene(void);
	AssimpScene(String^ path);
	property int MeshCount { int get(); }
	property AssimpNode^ RootNode { AssimpNode^ get(); }
	property List<AssimpMesh^>^ Meshes { List<AssimpMesh^>^ get(); }
	property List<AssimpCamera^>^ Cameras { List<AssimpCamera^>^ get(); }
	property List<AssimpMaterial^>^ Materials { List<AssimpMaterial^>^ get(); }

internal:
	AssimpScene(aiScene* scene);

private:
	const aiScene* m_pScene;
	List<AssimpMesh^>^ m_meshes;
	List<AssimpCamera^>^ m_cameras;
		List<AssimpMaterial^>^ m_materials;
	AssimpNode^ m_rootnode;


};

}}}