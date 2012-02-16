// indexsort.cpp
#include "indexsort.h"

int adaptindex(Obj* Objlist1, Obj* Objlist2, int NUMOBS1, int NUMOBS2, int* IDs_old, int* IDs_new, int* Sortlist, int* inc, int DoInc)
{
    int o1, o2, hs, hs_rev;
    int maxindex;
    float diff_curr=0.0, diff_hs=0.0;
    for (int i=0; i< NUMOBS2; i++) Sortlist[i] = -1;

    ///////////////////////////////////////////////
    // -> THE ARRAYs                             //
    // -> Objlist_temp, contains corrected indeces of objects //
    // -> 2D Diff Array, contains differences (squares of spacial euclidian distances) of Object pairs
    float ** Diff     = (float**) malloc (NUMOBS1 * sizeof(float*)); //new float *[NUMOBS1];

    for (o1=0; o1<NUMOBS1; o1++)
        {
         Diff[o1] = (float*) malloc (NUMOBS2 * sizeof(float));
         for (o2=0; o2<NUMOBS2; o2++)
             {Diff[o1][o2]=0;}
        }

    // -> Highscore Array , contains index pairs sorted by their difference value (low diff -> top rank) //
    Highscore* High;
    High = (Highscore*) malloc (NUMOBS1*NUMOBS2*2*sizeof(Highscore));

    // -> Vacant arrays No. 1 and 2, needed to re-sort object list
    int * Vacant2     = (int*) malloc (NUMOBS2*sizeof(int)); for (o2=0; o2<NUMOBS2; o2++)     Vacant2[o2] = 1;
    int * Vacant_temp = (int*) malloc (NUMOBS1*sizeof(int)); for (o1=0; o1<NUMOBS1; o1++) Vacant_temp[o1] = 1;

    ///////////////////////////////////////////////
    // -> CALCULATING DISTANCES                  //
    // -> loop over all objects: calculating distances & highscoring //
    for (o1=0; o1<NUMOBS1; o1++)
        {
         for (o2=0; o2<NUMOBS2; o2++)
             {
              maxindex = o1*NUMOBS2+o2;

              // -> calculate (square of) euclidian distance //
              Diff[o1][o2] =((float)Objlist2[o2].x - (float)Objlist1[o1].x) * ((float)Objlist2[o2].x - (float)Objlist1[o1].x) +
                            ((float)Objlist2[o2].y - (float)Objlist1[o1].y) * ((float)Objlist2[o2].y - (float)Objlist1[o1].y) ;

              diff_curr = Diff[o1][o2];

              // -> at startup, pair gets lowest hightscore entry //
              High[maxindex].o1n = o1;
              High[maxindex].o2n = o2;

              // -> global 'highscore' sorting by comparing spacial distance //
              // -> loop over highscore entries //
              for (hs=0; hs<maxindex; hs++)
                  {
                   diff_hs = Diff[High[hs].o1n][High[hs].o2n];

                   // -> if spacial distance of current pair is less than or equal to that of current highscore position //
                   if ((diff_curr<diff_hs) || (diff_curr==diff_hs))
                      {
                        // -> update lower highscore entries starting from bottom //
                        for (hs_rev=maxindex; hs_rev>hs; hs_rev--)
                            {
                             High[hs_rev].o1n = High[hs_rev-1].o1n;

                             High[hs_rev].o2n = High[hs_rev-1].o2n;
                            }
                        // -> set current pair in highscore position //
                        High[hs].o1n = o1;
                        High[hs].o2n = o2;
                        hs=maxindex;      // -> (eq. break) //
                      }
                  }
             }
        }

    ///////////////////////////////////////////////
    // -> SORTING                                //
    // -> sorting the new object list : loop over highscore entries, darwin-style: best entries get transferred  //
    // -> Array2 > Array1 //
    if (NUMOBS2>NUMOBS1 || NUMOBS2==NUMOBS1)
       {
        for (hs=0; hs<NUMOBS1*NUMOBS2; hs++)
            {
             if (Vacant_temp[High[hs].o1n] && Vacant2[High[hs].o2n])
                {
                 IDs_new[High[hs].o2n]     = IDs_old[High[hs].o1n];
                 Sortlist[High[hs].o2n]    = High[hs].o1n;
                 Vacant_temp[High[hs].o1n] = 0;
                     Vacant2[High[hs].o2n] = 0;
                }
            }
        // -> fill up the rest of the object array //
        if (NUMOBS2>NUMOBS1)
            {
             int newid     = 0;
             for (int ni=0; ni< NUMOBS1; ni++) newid = (IDs_old[ni]>newid || IDs_old[ni]==newid)? IDs_old[ni] : newid;
             if (DoInc) newid = (*inc>newid)? *inc : newid;
             else       *inc  =  newid;

             for (o2=0; o2<NUMOBS2; o2++)
                 {
                  if (Vacant2[o2])
                     {
                      newid++;
                      IDs_new[o2]  = newid;
                      *inc         = newid;
                      Vacant2[o2]  = 0;
                     }
                 }
            }
       }

    // -> Array1 > Array2 // TODO
    else
       {
        for (hs=0; hs<NUMOBS1*NUMOBS2; hs++)
            {
             if (Vacant_temp[High[hs].o1n] && Vacant2[High[hs].o2n] )
                {
                 IDs_new[High[hs].o2n]     = IDs_old[High[hs].o1n];
                 Sortlist[High[hs].o2n]    = High[hs].o1n;
                 Vacant_temp[High[hs].o1n] = 0;
                 Vacant2[High[hs].o2n]     = 0;
                }
            }
       }
    ///////////////////////////////////////////////
    // -> FREEdom to the masses:                 //

    for (o1=0; o1<NUMOBS1; o1++) free(Diff[o1]);

    free(High);
    free(Diff);
    free(Vacant2);
    free(Vacant_temp);

    return 1;
}
