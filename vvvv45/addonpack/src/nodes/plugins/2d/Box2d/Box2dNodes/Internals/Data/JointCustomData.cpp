#include "StdAfx.h"
#include "JointCustomData.h"

JointCustomData::JointCustomData(void)
{
	this->MarkedForDeletion = false;
}

JointCustomData::~JointCustomData(void)
{
	delete Custom;
}