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
			QRegExp defineCommandWithGUI("^#define\\s([^\\s]+)\\s(.*)\\s\\(float:([^\\s]*)\\)$"); // Look for #define varname value 


			for (QStringList::iterator it = in.begin(); it != in.end(); ++it) {

				if (ppCommand.indexIn(*it) != -1) {
					// Preprocessor command

					if (defineCommandWithGUI.indexIn(*it) != -1) {
						//INFO(QString("Found ppC (%1)->(%2): ").arg(defineCommandWithGUI.cap(1)).arg(defineCommandWithGUI.cap(2)) + *it);
						if (defineCommandWithGUI.cap(2).contains(defineCommandWithGUI.cap(1))) {
							WARNING(QString("#define command is recursive - skipped: %1 -> %2")
								.arg(defineCommandWithGUI.cap(1))
								.arg(defineCommandWithGUI.cap(2)));
						}
						//substitutions[defineCommandWithGUI.cap(1)] = defineCommandWithGUI.cap(2);
						QString defaultValue = defineCommandWithGUI.cap(2);
						QString floatInterval = defineCommandWithGUI.cap(3);
						QStringList fi = floatInterval.split("-");
						if (fi.count() != 2) {
							WARNING("Could not understand #define gui command: " + floatInterval);
							continue;
						}
						bool succes = false;
						double d1 = fi[0].toDouble(&succes);
						bool succes2 = false;
						double d2 = fi[1].toDouble(&succes2);
						if (!succes || !succes2) {
							WARNING("Could not parse float interval in #define gui command: " + floatInterval);
							continue;
						}
						bool succes3 = false;
						double d3 = defineCommandWithGUI.cap(2).toDouble(&succes3);
						if (!succes3) {
							WARNING("Could not parse default argumentin #define gui command: " + defineCommandWithGUI.cap(2));
							continue;
						}
						FloatParameter* fp= new FloatParameter(defineCommandWithGUI.cap(1), d1, d2, d3);
						//INFO(QString("Float: %1, %2").arg(d1).arg(d2));
						params.append(fp);
						
					} else if (defineCommand.indexIn(*it) != -1) {
						//INFO(QString("Found ppC (%1)->(%2): ").arg(defineCommand.cap(1)).arg(defineCommand.cap(2)) + *it);
						if (defineCommand.cap(2).contains(defineCommand.cap(1))) {
							WARNING(QString("#define command is recursive - skipped: %1 -> %2")
								.arg(defineCommand.cap(1))
								.arg(defineCommand.cap(2)));
						}
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
							INFO("Replacing: " + it2.key() + " with " + it2.value());
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

