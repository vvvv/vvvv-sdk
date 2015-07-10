#include "StdAfx.h"
#include "CVSeqBuilder.h"

CVSeqBuilder::CVSeqBuilder(void)
{
	this->storage = cvCreateMemStorage(0);
	this->points = cvCreateSeq(CV_SEQ_KIND_GENERIC|CV_32FC2, sizeof(CvContour),sizeof(CvPoint2D32f), storage );
}

CVSeqBuilder::~CVSeqBuilder(void)
{
	cvClearSeq(this->points);
	cvClearMemStorage(storage);
}


void CVSeqBuilder::Destroy() 
{
	cvClearSeq(this->points);
	cvClearMemStorage(storage);
}

void CVSeqBuilder::Clear() 
{
	this->storage = cvCreateMemStorage(0);
	this->points = cvCreateSeq(CV_SEQ_KIND_GENERIC|CV_32FC2, sizeof(CvContour),sizeof(CvPoint2D32f), storage );
}

void CVSeqBuilder::AddPoint(float x,float y) 
{
	CvPoint2D32f pt;
	pt.x = x;
	pt.y = y;
	cvSeqPush( points, &pt );
	CvContour ctr;

}

