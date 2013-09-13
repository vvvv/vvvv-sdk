#pragma once
#include "../Math/Vector3.h"

using namespace SyntopiaCore::Math;

namespace SyntopiaCore {
	namespace Extras {	
		class VLine
		{
		public:
			Vector3f v1,v2;
			VLine(void);
			VLine(Vector3f v1,Vector3f v2);
		public:
			~VLine(void);
		};
	}
}

