#include "StdAfx.h"
#include "RGBAColor.h"
#include "../Math/Vector3.h"


namespace SyntopiaCore {
	namespace Extras {	

		VRGBAColor::VRGBAColor(void)
		{
		}

		VRGBAColor::VRGBAColor(float r,float g,float b,float alpha) 
		{
			this->r = r;
			this->g = g;
			this->b = b;
			this->a = alpha;
		}

		VRGBAColor::VRGBAColor(SyntopiaCore::Math::Vector3f vector,float alpha)
		{
			this->r = vector.x();
			this->g = vector.y();
			this->b = vector.z();
			this->a = alpha;
		}

		VRGBAColor::~VRGBAColor(void)
		{
		}
	}
}
