#pragma once
#include "../Math/Vector3.h"

using namespace SyntopiaCore::Math;

namespace SyntopiaCore {
	namespace Extras {	
		class VBox
		{
		public:
			Vector3f base,dir1,dir2,dir3;
			VBox(void);
			VBox(Vector3f base,Vector3f dir1,Vector3f dir2,Vector3f dir3);
		public:
			~VBox(void);
		};
	}
}

