#include "StdAfx.h"
#include "VBox.h"

#include "../Math/Vector3.h"


namespace SyntopiaCore {
	namespace Extras {	

		VBox::VBox(void)
		{
		}

		VBox::VBox(Vector3f base,Vector3f dir1,Vector3f dir2,Vector3f dir3) {
			this->base = base;
			this->dir1 = dir1;
			this->dir2 = dir2;
			this->dir3 = dir3;
		}

		VBox::~VBox(void)
		{
		}


	}
}

