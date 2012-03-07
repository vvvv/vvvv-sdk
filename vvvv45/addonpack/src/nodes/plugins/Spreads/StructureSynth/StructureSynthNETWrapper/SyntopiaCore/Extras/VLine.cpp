#include "StdAfx.h"
#include "Vline.h"

#include "../Math/Vector3.h"


namespace SyntopiaCore {
	namespace Extras {	

		VLine::VLine(void)
		{
		}

		VLine::VLine(Vector3f v1,Vector3f v2) {
			this->v1 = v1;
			this->v2 = v2;
		}

		VLine::~VLine(void)
		{
		}


	}
}