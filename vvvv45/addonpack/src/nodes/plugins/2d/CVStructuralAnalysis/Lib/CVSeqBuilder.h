#pragma once

#include "cv.h"

public class CVSeqBuilder
{
public:
	CVSeqBuilder(void);
	~CVSeqBuilder(void);
	void AddPoint(float x, float y);
	void Clear();
	void Destroy();
	CvSeq* points;
protected:
	CvMemStorage* storage;
};
