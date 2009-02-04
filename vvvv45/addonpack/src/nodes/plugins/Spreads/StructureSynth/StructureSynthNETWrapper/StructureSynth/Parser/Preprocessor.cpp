#include "stdafx.h"
#include "Preprocessor.h"

#include <QStringList>
#include <QRegExp>
#include <QMap>

#include "../../SyntopiaCore/Exceptions/Exception.h"
#include "../../SyntopiaCore/Logging/Logging.h"

using namespace SyntopiaCore::Exceptions;
using namespace SyntopiaCore::Logging;


namespace StructureSynth {
	namespace Parser {	

		QString Preprocessor::Process(QString input) {
			QStringList in = input.split(QRegExp("\r\n|\r|\n"));

			QMap<QString, QString> substitutions;
			QRegExp ppCommand("^#"); // Look for #define varname value
			QRegExp defineCommand("^#define\\s([^\\s]+)\\s(.*)*$"); // Look for #define varname value


			for (QStringList::iterator it = in.begin(); it != in.end(); ++it) {

				if (ppCommand.indexIn(*it) != -1) {
					// Preprocessor command
					if (defineCommand.indexIn(*it) != -1) {
						//INFO(QString("Found ppC (%1)->(%2): ").arg(defineCommand.cap(1)).arg(defineCommand.cap(2)) + *it);
						substitutions[defineCommand.cap(1)] = defineCommand.cap(2);
					} else {
						WARNING("Could not understand preprocessor command: " + *it);
					}
				} else {
					// Non-preprocessor command
					// Check for substitutions.
					QMap<QString, QString>::const_iterator it2 = substitutions.constBegin();
					int subst = 0;
					while (it2 != substitutions.constEnd()) {
						if (subst>100) {
							WARNING("More than 100 recursive preprocessor substitutions... breaking.");
							break;
						}
						if ((*it).contains(it2.key())) {
							(*it).replace(it2.key(), it2.value());
							it2 = substitutions.constBegin();
							subst++;
						} else {
							it2++;
						}
				    }
				}
			}

			QStringList out = in;
			return out.join("\r\n");
		}
	}
}

