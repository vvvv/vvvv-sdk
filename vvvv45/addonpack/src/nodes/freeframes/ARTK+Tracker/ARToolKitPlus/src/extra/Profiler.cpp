

#include <ARToolKitPlus/extra/Profiler.h>
#include <stdio.h>


namespace ARToolKitPlus {


#ifdef _USE_PROFILING_

#pragma message(">>> Building ARToolKitPlus with PROFILING ENABLED")

int mulCount = 0;
int divCount = 0;
int sqrtCount = 0;
int invSqrtCount = 0;
int trigCount = 0;

#endif

void
Profiler::reset()
{
	_SINGLEMARKER_OVERALL.reset();
	_LABELING.reset();
	_DETECTMARKER2.reset();
	_GETMARKERINFO.reset();
	_GETTRANSMAT.reset();
	_GETINITROT.reset();
	_GETTRANSMAT3.reset();
	_GETTRANSMATSUB.reset();
	_MODIFYMATRIX_LOOP.reset();
	_MODIFYMATRIX.reset();
	_GETNEWMATRIX.reset();
	_GETROT.reset();
	_GETANGLE.reset();
}


const Profiler::Measurement*
Profiler::getMes(MES nMes) const
{
	switch(nMes)
	{
	case SINGLEMARKER_OVERALL:
		return &_SINGLEMARKER_OVERALL;
	case LABELING:
		return &_LABELING;
	case DETECTMARKER2:
		return &_DETECTMARKER2;
	case GETMARKERINFO:
		return &_GETMARKERINFO;
	case GETTRANSMAT:
		return &_GETTRANSMAT;
	case GETINITROT:
		return &_GETINITROT;
	case GETTRANSMAT3:
		return &_GETTRANSMAT3;
	case GETTRANSMATSUB:
		return &_GETTRANSMATSUB;
	case MODIFYMATRIX_LOOP:
		return &_MODIFYMATRIX_LOOP;
	case MODIFYMATRIX:
		return &_MODIFYMATRIX;
	case GETNEWMATRIX:
		return &_GETNEWMATRIX;
	case GETROT:
		return &_GETROT;
	case GETANGLE:
		return &_GETANGLE;
	}

	return NULL;
}


#ifdef _ARTKP_IS_WINDOWS_


void
Profiler::Measurement::reset()
{
	sum.QuadPart = 0;
}


void
Profiler::beginSection(Measurement& nM)
{
	QueryPerformanceCounter(&nM.secBegin);
}


void
Profiler::endSection(Measurement& nM)
{
	QueryPerformanceCounter(&nM.secEnd);
	nM.sum.QuadPart += nM.secEnd.QuadPart - nM.secBegin.QuadPart;
}


float
Profiler::getFraction(const Measurement& nNom, const Measurement& nDenom) const
{
	long double nom = (long double)nNom.sum.QuadPart,
				denom = (long double)nDenom.sum.QuadPart;

	long double fract = nom/denom;
	return (float)fract;
}


float
Profiler::getFraction(MES nNom, MES nDenom) const
{
	const Measurement* nom = getMes(nNom);
	const Measurement* denom = getMes(nDenom);

	if(!nom || !denom)
		return 0.0f;

	return getFraction(*nom, *denom);
}


float
Profiler::getTime(MES nMes) const
{
	const Measurement* mes = getMes(nMes);
	LARGE_INTEGER freq;

	if(!mes)
		return 0.0f;

	QueryPerformanceFrequency(&freq);

	long double ld_sum = (long double)mes->sum.QuadPart,
				ld_freq = (long double)freq.QuadPart;

	long double dt = ld_sum/ld_freq;
	return (float)dt;
}


#else // _ARTKP_IS_WINDOWS_


// supply only stub methods for non-windows systems
//
void
Profiler::Measurement::reset()
{
}


void
Profiler::beginSection(Measurement& /*nM*/)
{
}


void
Profiler::endSection(Measurement& /*nM*/)
{
}


float
Profiler::getFraction(MES /*nNom*/, MES /*nDenom*/) const
{
	return 0.0f;
}


float
Profiler::getTime(MES /*nMes*/) const
{
	return 0.0f;
}


#endif //_ARTKP_IS_WINDOWS_



void
Profiler::writeReport(const char* nFileName, unsigned int nNumRuns) const
{
	FILE* fp = fopen(nFileName, "w");
	if(!fp)
		return;

#ifdef _USE_PROFILING_
#  ifdef _ARTKP_IS_WINDOWS_
	float overall = getTime(SINGLEMARKER_OVERALL);

	if(overall==0.0f)			// prevent division by 0
		overall = 1.0f;			// on non-windows systems

	fprintf(fp, "PROFILER REPORT (%d runs)\n\n", nNumRuns);
	fprintf(fp, "  SINGLEMARKER_OVERALL:                    %.3f msecs\n", 1000.0f*overall/nNumRuns);
	fprintf(fp, "      LABELING:                            %.3f msecs  (%.2f %%)\n", 1000.0f*getTime(LABELING)/nNumRuns, 100.0f*getTime(LABELING)/overall);
	fprintf(fp, "      DETECTMARKER2:                       %.3f msecs  (%.2f %%)\n", 1000.0f*getTime(DETECTMARKER2)/nNumRuns, 100.0f*getTime(DETECTMARKER2)/overall);
	fprintf(fp, "      GETMARKERINFO:                       %.3f msecs  (%.2f %%)\n", 1000.0f*getTime(GETMARKERINFO)/nNumRuns, 100.0f*getTime(GETMARKERINFO)/overall);
	fprintf(fp, "      GETTRANSMAT:                         %.3f msecs  (%.2f %%)\n", 1000.0f*getTime(GETTRANSMAT)/nNumRuns, 100.0f*getTime(GETTRANSMAT)/overall);
	fprintf(fp, "          GETINITROT:                      %.3f msecs  (%.2f %%)\n", 1000.0f*getTime(GETINITROT)/nNumRuns, 100.0f*getTime(GETINITROT)/overall);
	fprintf(fp, "          GETTRANSMAT3:                    %.3f msecs  (%.2f %%)\n", 1000.0f*getTime(GETTRANSMAT3)/nNumRuns, 100.0f*getTime(GETTRANSMAT3)/overall);
	fprintf(fp, "              GETTRANSMATSUB:              %.3f msecs  (%.2f %%)\n", 1000.0f*getTime(GETTRANSMATSUB)/nNumRuns, 100.0f*getTime(GETTRANSMATSUB)/overall);
	fprintf(fp, "                  MODIFYMATRIX:            %.3f msecs  (%.2f %%)\n", 1000.0f*getTime(MODIFYMATRIX)/nNumRuns, 100.0f*getTime(MODIFYMATRIX)/overall);
	fprintf(fp, "                      MODIFYMATRIX_LOOP:   %.3f msecs  (%.2f %%)\n", 1000.0f*getTime(MODIFYMATRIX_LOOP)/nNumRuns, 100.0f*getTime(MODIFYMATRIX_LOOP)/overall);
	fprintf(fp, "                          GETNEWMATRIX:    %.3f msecs  (%.2f %%)\n", 1000.0f*getTime(GETNEWMATRIX)/nNumRuns, 100.0f*getTime(GETNEWMATRIX)/overall);
	fprintf(fp, "                              GETROT:      %.3f msecs  (%.2f %%)\n", 1000.0f*getTime(GETROT)/nNumRuns, 100.0f*getTime(GETROT)/overall);

	fprintf(fp, "\n  GETANGLE:                                 %.3f msecs  (%.2f %%)\n", 1000.0f*getTime(GETANGLE)/nNumRuns, 100.0f*getTime(GETANGLE)/overall);
#  else  // _ARTKP_IS_WINDOWS_
	fprintf(fp, "PROFILER REPORT (%d runs)\n\n", nNumRuns);
	fprintf(fp, "  ERROR: profiling currently only supported under Windows.\n");
#  endif // _ARTKP_IS_WINDOWS_
#else  // _USE_PROFILING_
	fprintf(fp, "PROFILER REPORT (%d runs)\n\n", nNumRuns);
	fprintf(fp, "  ERROR: profiling was disabled at compiletime.\n");
#endif // _USE_PROFILING_

	fclose(fp);
}


bool
Profiler::isProfilingEnabled()
{
#ifdef _USE_PROFILING_
	return true;
#else
	return false;
#endif
}



}  // namespace ARToolKitPlus
