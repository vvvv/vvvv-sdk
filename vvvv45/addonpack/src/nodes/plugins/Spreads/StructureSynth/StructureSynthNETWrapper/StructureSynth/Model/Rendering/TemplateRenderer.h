#pragma once

#include <QString>
#include <QMap>
#include <QStringList>
#include "Renderer.h"

#include "../../../SyntopiaCore/Math/Vector3.h"


namespace StructureSynth {
	namespace Model {	
		namespace Rendering {
			
			class Template; // Forward...

			/// A renderer implementation based on the SyntopiaCore POV widget.
			class TemplateRenderer : public Renderer {
			public:
				TemplateRenderer();
				TemplateRenderer(QString xmlDefinitionFile);
				
				virtual ~TemplateRenderer();

				virtual QString renderClass() { return "template"; }

				/// The primitives
				virtual void drawBox(SyntopiaCore::Math::Vector3f base, 
					          SyntopiaCore::Math::Vector3f dir1 , 
							  SyntopiaCore::Math::Vector3f dir2, 
							  SyntopiaCore::Math::Vector3f dir3,
								const QString& classID);

				virtual void drawSphere(SyntopiaCore::Math::Vector3f center, float radius,
								const QString& classID);

				
				virtual void drawMesh(  SyntopiaCore::Math::Vector3f startBase, 
										SyntopiaCore::Math::Vector3f startDir1, 
										SyntopiaCore::Math::Vector3f startDir2, 
										SyntopiaCore::Math::Vector3f endBase, 
										SyntopiaCore::Math::Vector3f endDir1, 
										SyntopiaCore::Math::Vector3f endDir2, 
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

				virtual void begin();
				virtual void end();
				
				virtual void setColor(SyntopiaCore::Math::Vector3f rgb) { this->rgb = rgb; }
				virtual void setBackgroundColor(SyntopiaCore::Math::Vector3f rgb);
				virtual void setAlpha(double alpha) { this->alpha = alpha; }

				QString getOutput() { return output.join(""); }
				
				// Issues a command for a specific renderclass such as 'template' or 'opengl'
				virtual void callCommand(const QString& renderClass, const QString& command);

				void assertTemplateExists(QString templateName);
			

			private:
				
				SyntopiaCore::Math::Vector3f rgb;
				double alpha;
				QMap<QString, Template> templates;
				QStringList output;
				int counter;
			};

		}
	}
}

