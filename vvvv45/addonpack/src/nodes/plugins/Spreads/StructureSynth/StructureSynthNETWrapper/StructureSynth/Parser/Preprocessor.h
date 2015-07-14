#pragma once

#include <QString>
#include <QList>
#include <QVector>

#include "../../SyntopiaCore/Exceptions/Exception.h"

namespace StructureSynth {
	namespace Parser {	

		class GuiParameter {
		public:
			GuiParameter(QString name) : name(name) {};
			virtual QString getName() { return name; }
		protected:
			QString name;
		};

		class FloatParameter : public GuiParameter {
		public:
			FloatParameter(QString name, double from, double to, double defaultValue) :
					GuiParameter(name), from(from), to(to), defaultValue(defaultValue) {};
			
			double getFrom() { return from; }
			double getTo() { return to; }
			double getDefaultValue() { return defaultValue; }
		private:
			double from;
			double to;
			double defaultValue;
		};

		/// The preprocessor is responsible for expanding '#define'
		///
		class Preprocessor {

		public:
			Preprocessor() {};
			
			QString Process(QString input);
			QVector<GuiParameter*> getParameters() { return params; }
			
		private:
			QVector<GuiParameter*> params;
		};

	}
}

