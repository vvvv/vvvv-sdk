#pragma once

#include <QString>
#include <QMap>
#include <QStringList>
#include <QVector>
#include "Renderer.h"
#include <QColor>

#include "../../../SyntopiaCore/Extras/VLine.h"
#include "../../../SyntopiaCore/Extras/VBox.h"
#include "../../../SyntopiaCore/Extras/VTriangle.h"
#include "../../../SyntopiaCore/Math/Vector3.h"
#include "../../../SyntopiaCore/Extras/RGBAColor.h"

using namespace System::Collections::Generic;
using namespace System;
using namespace SyntopiaCore::Extras;

namespace StructureSynth {
	namespace Model {	
		namespace Rendering {
			

			/// A renderer implementation based on the SyntopiaCore POV widget.
			class ListRenderer : public Renderer {
			public:
				ListRenderer();
				
				virtual ~ListRenderer();

				virtual QString renderClass() { return "dotnet"; }

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

				virtual void drawTriangle(SyntopiaCore::Math::Vector3f p1,
								SyntopiaCore::Math::Vector3f p2,
								SyntopiaCore::Math::Vector3f p3,
								const QString& classID);

				virtual void begin();
				virtual void end();
				
				virtual void setColor(SyntopiaCore::Math::Vector3f rgb) { this->rgb = rgb; }
				virtual void setBackgroundColor(SyntopiaCore::Math::Vector3f rgb);
				virtual void setAlpha(double alpha) { this->alpha = alpha; }

				
				
				// Issues a command for a specific renderclass such as 'template' or 'opengl'
				virtual void callCommand(const QString& renderClass, const QString& command);

			
				QVector<VRGBAColor> spheres_color;
				QVector<SyntopiaCore::Math::Vector3f> spheres_center;
				QVector<float> spheres_radius;

				QVector<VBox> boxes;
				QVector<VRGBAColor> boxes_color;

				QVector<VBox> grids;
				QVector<VRGBAColor> grids_color;

				QVector<VLine> lines;
				QVector<VRGBAColor> lines_color;

				QVector<VTriangle> triangles;

				QVector<Vector3f> points;
				QVector<VRGBAColor> points_color;
			private:
				int counter;
				SyntopiaCore::Math::Vector3f rgb;
				double alpha;

			};

		}
	}
}

