#pragma once

#include <QString>
#include "../../../SyntopiaCore/GLEngine/EngineWidget.h"
#include "Renderer.h"

#include "../../../SyntopiaCore/Math/Vector3.h"


namespace StructureSynth {
	namespace Model {	
		namespace Rendering {

			/// A renderer implementation based on the SyntopiaCore openGL widget.
			class OpenGLRenderer : public Renderer {
			public:
				OpenGLRenderer(SyntopiaCore::GLEngine::EngineWidget* engine) : engine(engine) {};
				virtual ~OpenGLRenderer() {};

				/// The primitives
				virtual void drawBox(SyntopiaCore::Math::Vector3f base, 
					          SyntopiaCore::Math::Vector3f dir1 , 
							  SyntopiaCore::Math::Vector3f dir2, 
							  SyntopiaCore::Math::Vector3f dir3,
								const QString& classID);

				
				virtual void drawMesh(  SyntopiaCore::Math::Vector3f startBase, 
										SyntopiaCore::Math::Vector3f startDir1, 
										SyntopiaCore::Math::Vector3f startDir2, 
										SyntopiaCore::Math::Vector3f endBase, 
										SyntopiaCore::Math::Vector3f endDir1, 
										SyntopiaCore::Math::Vector3f endDir2, 
										const QString& classID);

				virtual void drawSphere(SyntopiaCore::Math::Vector3f center, float radius,
								const QString& classID);

				virtual void drawGrid(SyntopiaCore::Math::Vector3f base, 
								SyntopiaCore::Math::Vector3f dir1, 
								SyntopiaCore::Math::Vector3f dir2, 
								SyntopiaCore::Math::Vector3f dir3,
								const QString& classID);

				virtual void drawLine(SyntopiaCore::Math::Vector3f from, 
										SyntopiaCore::Math::Vector3f to,
								const QString& classID);

				virtual void drawDot(SyntopiaCore::Math::Vector3f pos,
								const QString& classID);

				virtual void drawTriangle(SyntopiaCore::Math::Vector3f p1,
										 SyntopiaCore::Math::Vector3f p2,
									     SyntopiaCore::Math::Vector3f p3,
											const QString& classID);

				virtual void begin();
				virtual void end();
				
				virtual void setColor(SyntopiaCore::Math::Vector3f rgb) { this->rgb = rgb; }
				virtual void setBackgroundColor(SyntopiaCore::Math::Vector3f rgb);
				virtual void setAlpha(double alpha) { this->alpha = alpha; }

				virtual void setTranslation(SyntopiaCore::Math::Vector3f /*translation*/);
				virtual void setScale(double /*scale*/);
				virtual void setRotation(SyntopiaCore::Math::Matrix4f /*rotation*/);
				virtual void setPivot(SyntopiaCore::Math::Vector3f /*pivot*/);

				// Issues a command for a specific renderclass such as 'template' or 'opengl'
				virtual void callCommand(const QString& renderClass, const QString& command);
			private:
				SyntopiaCore::GLEngine::EngineWidget* engine;
				SyntopiaCore::Math::Vector3f rgb;
				double alpha;
			};

		}
	}
}

