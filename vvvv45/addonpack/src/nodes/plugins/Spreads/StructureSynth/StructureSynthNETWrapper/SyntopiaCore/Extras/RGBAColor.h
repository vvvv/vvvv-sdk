#pragma once

#include "../Math/Vector3.h"

namespace SyntopiaCore {
	namespace Extras {	
		class VRGBAColor
		{
		public:
			VRGBAColor(void);
			VRGBAColor(float r,float g,float b,float alpha);
			VRGBAColor(SyntopiaCore::Math::Vector3f vector,float alpha);
		public:
			float r,g,b,a;
			~VRGBAColor(void);
		};
	}
}
