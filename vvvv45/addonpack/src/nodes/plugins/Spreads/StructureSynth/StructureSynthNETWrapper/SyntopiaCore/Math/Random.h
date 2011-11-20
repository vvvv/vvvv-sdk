#pragma once

#include <QString>
#include <QVector>
#include <QStringList>
#include <cmath>

#include "../../ThirdPartyCode/MersenneTwister/MersenneTwister.h"

namespace SyntopiaCore {
	namespace Math {	

		/// A simple class for generating random numbers
		/// It is possible to have multiple independent streams, if the underlying RNG is the Mersenne Twister.
		/// If set to useStdLib, the CStdLib 'rand' and 'srand' functions are used - these are not independent - not even with multiple instances of this class.
		class RandomNumberGenerator {
		public:
			RandomNumberGenerator(bool useOldLibrary = false) { if (useOldLibrary) { rng = 0; } else { rng = new MTRand(); } setSeed(0); };
			~RandomNumberGenerator() { delete rng; };

			// This is only useful for backward compatibility.
			// The Mersenne Twister is much better since it allows multiple independent streams.
			void useStdLib(bool useOldLibrary) { 
				delete rng; rng = 0;
				if (!useOldLibrary) {
					rng = new MTRand();
				}
				setSeed(lastSeed);
			};

			bool isUsingStdLib() { return (rng == 0); }

			 // Returns a double in the interval [0;1]
			double getDouble() { 
				if (rng) {
					return rng->rand();
				} else {
					return rand()/(double)RAND_MAX;
					/*
					This one would be more correct, but the old cstdlib rand is implemented for backward compatibility:
					return  (double)rand() / ((double)(RAND_MAX)+(double)(1)) ; // There are reasons for the multiple (double) casts, see: http://members.cox.net/srice1/random/crandom.html
					*/
				}
			};    
			
			// Returns an integer between 0 and max (both inclusive).
			int getInt(int max) { 
				if (rng) {
					return rng->randInt(max);
				} else {
					return rand() % (max+1); // Probably not very good, use mersenne instead
				}
			}

			int getInt() { 
				if (rng) {
					return rng->randInt();
				} else {
					return rand();
				}
			}

			void setSeed(int seed) { 
				lastSeed = seed; 
				if (rng) {
					rng->seed(seed);
				} else {
					srand(seed);
				}
			};

		private:
			int lastSeed;
			MTRand* rng;
		};

	}
}

