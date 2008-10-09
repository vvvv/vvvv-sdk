#pragma once

#include <QString>
#include "../../../SyntopiaCore/Math/Vector3.h"
#include "../../../SyntopiaCore/Math/Matrix4.h"

namespace StructureSynth {
	namespace Model {	
		namespace Rendering {

		/// Abstract base class for implementing a renderer
		class Renderer {
			public:
				Renderer() {};
				virtual ~Renderer() {};

				/// Flow
				virtual void begin() {};
				virtual void end() {};

				/// This defines the identifier for our renderer.
				virtual QString renderClass() { return ""; }

				/// The primitives
				virtual void drawBox(SyntopiaCore::Math::Vector3f base, 
								SyntopiaCore::Math::Vector3f dir1, 
								SyntopiaCore::Math::Vector3f dir2, 
								SyntopiaCore::Math::Vector3f dir3,
								const QString& classID) = 0;

				virtual void drawMesh(  SyntopiaCore::Math::Vector3f startBase, 
										SyntopiaCore::Math::Vector3f startDir1, 
										SyntopiaCore::Math::Vector3f startDir2, 
										SyntopiaCore::Math::Vector3f endBase, 
										SyntopiaCore::Math::Vector3f endDir1, 
										SyntopiaCore::Math::Vector3f endDir2, 
										const QString& classID) = 0;

				virtual void drawGrid(SyntopiaCore::Math::Vector3f base, 
								SyntopiaCore::Math::Vector3f dir1, 
								SyntopiaCore::Math::Vector3f dir2, 
								SyntopiaCore::Math::Vector3f dir3,
								const QString& classID) = 0;

				virtual void drawLine(SyntopiaCore::Math::Vector3f from, 
										SyntopiaCore::Math::Vector3f to,
								const QString& classID) = 0;

				virtual void drawDot(SyntopiaCore::Math::Vector3f pos,
								const QString& classID) = 0;
				
				virtual void drawSphere(SyntopiaCore::Math::Vector3f center, float radius,
								const QString& classID) = 0;

				// Color
				// RGB in [0;1] intervals.
				virtual void setColor(SyntopiaCore::Math::Vector3f rgb) = 0;
				virtual void setBackgroundColor(SyntopiaCore::Math::Vector3f rgb) = 0;
				virtual void setAlpha(double alpha) = 0;

				// Camera settings
				virtual void setTranslation(SyntopiaCore::Math::Vector3f /*translation*/) {};
				virtual void setScale(double /*scale*/) {};
				virtual void setRotation(SyntopiaCore::Math::Matrix4f /*rotation*/) {};
				virtual void setPivot(SyntopiaCore::Math::Vector3f /*pivot*/) {};

				// Issues a command for a specific renderclass such as 'template' or 'opengl'
				virtual void callCommand(const QString& /*renderClass*/, const QString& /*command*/) {};
		};

		}
	}
}

