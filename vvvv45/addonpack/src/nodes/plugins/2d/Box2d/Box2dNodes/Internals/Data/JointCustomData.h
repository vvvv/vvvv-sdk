#pragma once

#include <string.h>

class JointCustomData
{
private:

public:
	JointCustomData(void);
	~JointCustomData(void);
	int Id;
	bool MarkedForDeletion;
	char* Custom;
};
