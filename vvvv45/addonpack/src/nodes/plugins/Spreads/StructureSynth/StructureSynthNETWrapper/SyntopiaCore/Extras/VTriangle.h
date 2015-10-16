#pragma once
#include "../Math/Vector3.h"

using namespace SyntopiaCore::Math;

namespace SyntopiaCore {
	namespace Extras {	
		class VTriangle
		{
		public:
			Vector3f v1,v2,v3;
			int id1,id2,id3;
			VTriangle(void);
			VTriangle(int num,Vector3f v1,Vector3f v2,Vector3f v3);
		public:
			~VTriangle(void);
		};
	}
}