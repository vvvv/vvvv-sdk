#include "StdAfx.h"
#include "BodyCustomData.h"

BodyCustomData::BodyCustomData(void)
{
	this->MarkedForDeletion = false;
}

BodyCustomData::~BodyCustomData(void)
{
	delete Custom;
}
