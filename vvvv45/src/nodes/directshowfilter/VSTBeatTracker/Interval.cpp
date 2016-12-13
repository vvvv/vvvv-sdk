#include "Interval.h"

/******************************************************************************************/

LINK::LINK()
{
  reset();
}

void LINK::reset()
{
  pos   = 0;
  value = 0;
}

/******************************************************************************************/

CHAIN::CHAIN()
{
  reset();
}

void CHAIN::reset()
{
  nLink    = 0;
  value    = 0;
  interval = 0;

  for(int i=0;i<NRESONANCE;i++)
  {
   link[i].pos   = 0;
   link[i].value = 0;
  }

}

/******************************************************************************************/

ChainBuilder::ChainBuilder()
{
  reset();
}

void ChainBuilder::reset()
{
  for(int i=0;i<NRESONANCE;i++)
  peak[i]=0;

  interval = 0;

  minInterval = MININTERVAL;
  maxInterval = MAXINTERVAL;
}

//compares two chains and returns true if the first------// 
//parameter is bigger than the second
bool ChainBuilder::compare(CHAIN a,CHAIN b)
{
  if(a.nLink==0) return false;

  if(b.nLink==0) return true;

  double sumA=0,sumB=0;
  int n=a.nLink;

  //sets the number of peaks which are summed
  //the chain with less links sets it
  if(b.nLink < a.nLink) n=b.nLink;

  for(int i=0;i<n;i++)
  {
    sumA+=a.link[i].value;
	sumB+=b.link[i].value;
  }

  if(sumA>=sumB) return true;

  return false;

}

//copy one chain to another-------------------------------//
void ChainBuilder::copy(CHAIN &a,CHAIN &b)
{
  a.nLink    = b.nLink;
  a.interval = b.interval;
  a.value    = b.value;

  for(int i=0;i<b.nLink;i++)
  {
    a.link[i].pos   = b.link[i].pos;
    a.link[i].value = b.link[i].value;
  }

}

//processing of the resonance-data of one channel---------//
void ChainBuilder::process(double resonance[],int &interval,int minInterval,int maxInterval)
{
  peakpicker.process(resonance,peak,NRESONANCE);
  
  //setTop    ();
  setupChains ();
  sortChains  ();
  calInterval ();

  interval = this->interval; 

  this->minInterval = minInterval;
  this->maxInterval = maxInterval;

}

//the biggest of the peaks. function currently not in use--//
void ChainBuilder::setTop()
{
  top.value = 0;
  top.pos   = 0;

  for(int i=0;i<NRESONANCE;i++)
  if(peak[i]>top.value)
  {
   top.value = peak[i];
   top.pos   = i;
  }
}

//finds peaks and link them to chains----------------------//
void ChainBuilder::setupChains()
{
  int tolerance = 2;
  
  nChain=0;

  //MININTERVAL is about 160 BPM. MAXINTERVAL about 40 BPM------------------//
  //they define the indices of the resonance array between which the 
  //first element of a is allowed to lie
  for(int h=minInterval;h<maxInterval;h++)
  if(peak[h]>0)
  {
	  

	  //if a peak is found it is declared as the first element of a chain 
      chain[nChain].link[0].pos   = h;
	  chain[nChain].link[0].value = peak[h];
	  chain[nChain].nLink = 1;	  
	  chain[nChain].interval = h;
	  
      bool run;

	  do
	  {
	    run=false;

		//the distance of the first element of the chain times x +/- a tolerance is the index which 
		//is checked for a peak. if a peak is found it is linked to the chain
		int posCheck=chain[nChain].link[chain[nChain].nLink-1].pos+chain[nChain].interval;

		if(posCheck>NRESONANCE-1) break; 

		for(int k=0;k<1+tolerance;k++)//-----------------------------------//
		{
		  if(peak[posCheck-k]>0) 
		  { 
			 chain[nChain].link[chain[nChain].nLink].pos   = posCheck-k; 
		     chain[nChain].link[chain[nChain].nLink].value = peak[posCheck-k];
			 chain[nChain].value += peak[posCheck-k];
			 chain[nChain].nLink++;	
			 run=true;
			 break;   
		  }
		  else
          if(peak[posCheck+k]>0 && !(posCheck+k>=NRESONANCE)) 
		  { 
			 chain[nChain].link[chain[nChain].nLink].pos   = posCheck+k; 
		     chain[nChain].link[chain[nChain].nLink].value = peak[posCheck+k];
			 chain[nChain].value += peak[posCheck+k];
			 chain[nChain].nLink++;		
			 run=true;
			 break;
		  }
		 }//end for k------------------------------------------------------// 
	  
	  }while(run);
		
	  if(chain[nChain].nLink+1 >= NRESONANCE/chain[nChain].interval)  nChain++;

  }//end if peak & for h------------------------------------------------------------//

}

//selection-sort of the chains
void ChainBuilder::sortChains()
{
  for(int h=0;h<nChain;h++)
  {
	int posMax =- 1;

    CHAIN tmp,max;

    for(int i=h;i<nChain;i++)
	if(compare(chain[i],max))
	{
      copy(max,chain[i]);
	  posMax = i;
	}

	if(posMax!=-1)
	{
	  copy(chain[posMax],chain[h]);
      copy(chain[h],max);
	}

  }//end for h------------//
}

int ChainBuilder::getNChain()
{
  return nChain;
}

double* ChainBuilder::getPeak()
{
  return peak;
}

int ChainBuilder::getChainNLink(int c)
{
  return chain[c].nLink;
}

int ChainBuilder::getChainLinkPos (int c,int l)
{
  return chain[c].link[l].pos;
}

double ChainBuilder::getChainLinkValue (int c,int l)
{
  return chain[c].link[l].value;
}

//now the first element of the strongest chain is 
//delivers the tempo-hypothesis. maybe there are better solutions.
void ChainBuilder::calInterval()
{
  this->interval = chain[0].interval;

}

double ChainBuilder::getPeak(int index)
{
  return peak[index];
}

int ChainBuilder::getInterval()
{
  return interval;
}

/******************************************************************************************/

//processing of the resonances of the 4 channels
void Interval::process(double resonance[][NRESONANCE],int interval[],int minInterval,int maxInterval,bool optimize)
{
  for(int h=0;h<NCHANNEL;h++)
  {
   if(optimize)
   if((h==1) || (h==3)) continue;

   chainBuilder[h].process(resonance[h],interval[h],minInterval,maxInterval);
  }

}

ChainBuilder* Interval::getChainBuilder(int index)
{
  return &chainBuilder[index];
 
}

void Interval::reset()
{
  for(int i=0;i<NCHANNEL;i++)
  chainBuilder[i].reset();

}

/******************************************************************************************/

