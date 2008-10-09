#pragma once

#include "Rule.h"
#include "CustomRule.h"

namespace StructureSynth {
	namespace Model {	

		/// Container for all rules.
		class RuleSet {
			public:
				/// Constructor. Automatically adds built-in rules.
				RuleSet();

				/// Deletes rules
				~RuleSet();

				/// Added rules belong to the RuleSet and will be deleted by the RuleSet destructor.
				void addRule(Rule* rule);

				/// Resolve symbolic names into pointers
				void resolveNames();

				/// TODO: Implement
				QStringList getUnreferencedNames();

				Rule* getStartRule() const ;

				CustomRule* getTopLevelRule() const { return topLevelRule; }

				/// For debug
				void dumpInfo() const;

		private:
			 QList<Rule*> rules;
			 CustomRule* topLevelRule;
		};

	}
}

