#include "dump_graph.h"

#include <stdio.h>


void dump_graph( const char *fileName, Segmenter *s )
{
    int i, j;

    FILE *fp = fopen( fileName, "wt" );
    fprintf( fp, "graph G {\n" );

    // we depend on the segmenter initializing certain region fields for us
    // check that here
#ifndef NDEBUG
    sanity_check_region_initial_values( s );
#endif

    // find fiducial roots beginning at leafs
    
    for( i=0; i < s->region_count; ++i ){
        Region *r = LOOKUP_SEGMENTER_REGION( s, i );

        if( !(r->flags & FREE_REGION_FLAG) ){

            fprintf( fp, "\"%p\" [", r );
            
            if( r->colour != 0 )
                fprintf( fp, "color=green ");

            fprintf( fp, "label=_%p_", r );

            if( r->flags & SATURATED_REGION_FLAG )
                fprintf( fp, "S_" );

            if( r->flags & FRAGMENTED_REGION_FLAG )
                fprintf( fp, "F_" );

            if( r->flags & ADJACENT_TO_ROOT_REGION_FLAG )
                fprintf( fp, "AR" );
                
            fprintf( fp, "]\n" );

        
            for( j=0; j < r->adjacent_region_count; ++j ){
                Region *adjacent = r->adjacent_regions[j];

                if( r < adjacent )
                    fprintf( fp, "\"%p\" -- \"%p\"\n", r, adjacent );
            }
        }
    }

    fprintf( fp, "}\n" );
    fclose( fp );
}

