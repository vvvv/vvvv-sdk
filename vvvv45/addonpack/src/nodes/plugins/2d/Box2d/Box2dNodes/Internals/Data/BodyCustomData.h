#pragma once

#include <string.h>

class BodyCustomData
{
private:

public:
	BodyCustomData(void);
	~BodyCustomData(void);
	int Id;
	bool MarkedForDeletion;
	char* Custom;
};
