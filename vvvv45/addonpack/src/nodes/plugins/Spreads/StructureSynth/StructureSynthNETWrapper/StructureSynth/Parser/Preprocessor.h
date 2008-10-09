#pragma once

#include <QString>
#include <QList>

#include "../../SyntopiaCore/Exceptions/Exception.h"

namespace StructureSynth {
	namespace Parser {	

		/// The preprocessor is responsible for removing comments and resolving '#include'
		/// reference to other EisenScript files.
		///
		/// UPDATE: the preprocessor does nothing more than normalizing the line endings as of now.
		/// It turned out that it was easier to remove comments in the tokenizer, since
		/// we need to keep track of the original text position when highlighting errors in the GUI.
		class Preprocessor {

		public:
			
			static QString Process(QString input);

		private:
			Preprocessor() {};
		};

	}
}

