// indexsort.h
#include <iostream>

typedef struct {float x; float y; float width; float height; int lostframes; bool found;} Obj;
typedef struct {int o1n; int o2n; } Highscore;

using namespace std;

int adaptindex(Obj* Objlist1, Obj* Objlist2, int NUMOBS1, int NUMOBS2, int* IDs_old, int* IDs_new, int* Sortlist, int* inc, int DoInc);

// Objlist1 : List with object coordinates from last frame
// Objlist2 : List with object coordinates from current frame
// NUMOBS1  : Nr. of Elements of Objlist1 ( Nr. of Object in last frame )
// NUMOBS2  : Nr. of Elements of Objlist2 ( Nr. of Object in current frame )
// IDs_old  : List of Object IDs from last frame
// IDs_new  : Empty list of Object IDs from this frame ( to be filled by adaptindex )
// Sortlist : List with position every object had in the object list from last frame
// inc      : Highest object index
// DoInc    : Flag for using incremented object IDs
