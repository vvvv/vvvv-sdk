#include "StdAfx.h"
#include "VTriangle.h"

#include "../Math/Vector3.h"


namespace SyntopiaCore {
	namespace Extras {	

		VTriangle::VTriangle(void)
		{
		}

		VTriangle::VTriangle(int num,Vector3f v1,Vector3f v2,Vector3f v3) {
			this->v1 = v1;
			this->v2 = v2;
			this->v3 = v3;
			this->id1 = num;
			this->id2 = num+1;
			this->id3 = num+2;
		}

		VTriangle::~VTriangle(void)
		{
		}


	}
}
