#pragma once

#include <QString>
#include "Rendering/Renderer.h"
#include "RuleSet.h"
#include "State.h"
#include "ExecutionStack.h"

#include "../../SyntopiaCore/Math/Matrix4.h"

namespace StructureSynth {
	namespace Model {	

		

		/// A Builder executes the rule set on a Renderer object
		class Builder {
		public:
			Builder(Rendering::Renderer* renderTarget, RuleSet* ruleSet);
			void build();

			void setCommand(QString command, QString param);
			ExecutionStack& getNextStack();
			State& getState() { return state; };
			Rendering::Renderer* getRenderer() { return renderTarget; };
			void increaseObjectCount() { objects++; };

			// True, if the random seed was changed by the builder (by 'set seed <int>')
			bool seedChanged() { return hasSeedChanged; }
			int getNewSeed() { return newSeed; }

		private:
			State state;
			ExecutionStack stack;
			ExecutionStack nextStack;
			Rendering::Renderer* renderTarget;
			RuleSet* ruleSet;
			int maxGenerations;
			int maxObjects;
			int objects;
			int newSeed;
			bool hasSeedChanged;
		};

	}
}

