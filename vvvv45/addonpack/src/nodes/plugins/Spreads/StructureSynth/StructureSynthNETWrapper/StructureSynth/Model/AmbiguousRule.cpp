#include "stdafx.h"
#include "AmbiguousRule.h"

#include "Builder.h"

#include "../../SyntopiaCore/Logging/Logging.h"

using namespace SyntopiaCore::Logging;


namespace StructureSynth {
	namespace Model {	

		QList<RuleRef*> AmbiguousRule::getRuleRefs() const {
			QList<RuleRef*>  list;
			for (int i = 0; i < rules.size(); i++) {
				for (int j = 0; j < rules[i]->getRuleRefs().size(); j++) {
					list.append(rules[i]->getRuleRefs()[j]);
				}
			}
			return list;
		}

		void AmbiguousRule::apply(Builder* builder) const {
			// Calc sum of weigths
			double totalWeigth = 0;
			for (int i = 0; i < rules.size(); i++) {
				totalWeigth += rules[i]->getWeight();
			}


			int r = rand();
			double random = totalWeigth*(r/(double)RAND_MAX);


			// Choose a random rule according to weigths
			double accWeigth = 0;
			for (int i = 0; i < rules.size(); i++) {
				accWeigth += rules[i]->getWeight();
				if (random <= accWeigth) {
					rules[i]->apply(builder);
					return;
				}
			}
			rules[rules.size()-1]->apply(builder);

			WARNING("Assertion failed: in AmbiguousRule::apply");

		};
	
	}
}

