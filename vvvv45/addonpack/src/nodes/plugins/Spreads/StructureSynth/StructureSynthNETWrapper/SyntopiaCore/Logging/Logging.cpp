#include "stdafx.h"
#include "Logging.h"


#ifdef WIN32
#include "windows.h"
#endif

/// TODO's
/// - Nested log entris
/// - Time
/// - Setting a log view level


namespace SyntopiaCore {
	namespace Logging {	
		QVector<Logger*> Logger::loggers;

		void LOG(QString message, LogLevel priority) {
			
			// On Windows this allows us to see debug in the Output::Debug window while running.
			#ifdef WIN32
				OutputDebugString((LPCWSTR) (message+"\r\n").utf16());
			#endif

			for (int i = 0; i < Logger::loggers.size(); i++) {
				Logger::loggers[i]->log(message, priority);
			}
		}

		/// Useful aliases
		void Debug(QString text) { LOG(text, DebugLevel); }
		void INFO(QString text) { LOG(text, InfoLevel); }
		void WARNING(QString text) { LOG(text, WarningLevel); }
		void CRITICAL(QString text) { LOG(text, CriticalLevel); }
	}
}

