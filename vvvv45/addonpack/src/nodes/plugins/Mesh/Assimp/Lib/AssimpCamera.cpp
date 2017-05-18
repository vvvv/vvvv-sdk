#include "StdAfx.h"
#include "AssimpCamera.h"

namespace VVVV { namespace Assimp { namespace Lib {

AssimpCamera::AssimpCamera(void)
{
}

AssimpCamera::AssimpCamera(aiCamera* cam)
{
	this->m_pCam = cam;
	
}

String^ AssimpCamera::Name::get()
{
	return gcnew String(this->m_pCam->mName.data);
}

Vector3 AssimpCamera::Position::get()
{
	aiVector3D pos = this->m_pCam->mPosition;
	return Vector3(pos.x,pos.y,pos.z);
}

Vector3 AssimpCamera::UpVector::get()
{
	aiVector3D pos = this->m_pCam->mUp;
	return Vector3(pos.x,pos.y,pos.z);
}

Vector3 AssimpCamera::LookAt::get()
{
	aiVector3D pos = this->m_pCam->mLookAt;
	return Vector3(pos.x,pos.y,pos.z);
}

float AssimpCamera::HFOV::get()
{
	return this->m_pCam->mHorizontalFOV;
}

float AssimpCamera::NearPlane::get()
{
	return this->m_pCam->mClipPlaneNear;
}

float AssimpCamera::FarPlane::get()
{
	return this->m_pCam->mClipPlaneFar;
}

float AssimpCamera::AspectRatio::get()
{
	return this->m_pCam->mAspect;
}

}}}
