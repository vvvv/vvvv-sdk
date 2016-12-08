#include "stdafx.h"
#include "ListRenderer.h"
#include "../../../SyntopiaCore/Math/Vector3.h"
#include "../../../SyntopiaCore/Extras/RGBAColor.h"
#include "../../../SyntopiaCore/Extras/VBox.h"
#include "../../../SyntopiaCore/Logging/Logging.h"
#include "../../../SyntopiaCore/Exceptions/Exception.h"

#include <QDomDocument>
#include <QIODevice>
#include <QFile>
#include <QMap>

using namespace SyntopiaCore::Math;
using namespace SyntopiaCore::Logging;
using namespace System::Collections::Generic;
using namespace SyntopiaCore::Extras;

namespace StructureSynth {
	namespace Model {	
		namespace Rendering {


			ListRenderer::ListRenderer() 
			{		

			}


			ListRenderer::~ListRenderer() {
			}


			void ListRenderer::drawBox(SyntopiaCore::Math::Vector3f base, 
				SyntopiaCore::Math::Vector3f dir1 , 
				SyntopiaCore::Math::Vector3f dir2, 
				SyntopiaCore::Math::Vector3f dir3,
				const QString& classID) 
			{
				this->boxes.append(VBox(base,dir1,dir2,dir3));
				this->boxes_color.append(VRGBAColor(rgb,alpha));
			};

			void ListRenderer::drawGrid(SyntopiaCore::Math::Vector3f base, 
				SyntopiaCore::Math::Vector3f dir1, 
				SyntopiaCore::Math::Vector3f dir2, 
				SyntopiaCore::Math::Vector3f dir3,
								const QString& classID) 
			{
				this->grids.append(VBox(base,dir1,dir2,dir3));
				this->grids_color.append(VRGBAColor(rgb,alpha));
			};

			void ListRenderer::drawLine(SyntopiaCore::Math::Vector3f from, SyntopiaCore::Math::Vector3f to,const QString& classID) 
			{
				this->lines.append(VLine(from,to));
				this->lines_color.append(VRGBAColor(rgb,alpha));
			};

			void ListRenderer::drawDot(SyntopiaCore::Math::Vector3f v,const QString& classID) 
			{
				this->points.append(v);
				this->points_color.append(VRGBAColor(rgb,alpha));
			};

			void ListRenderer::drawSphere(SyntopiaCore::Math::Vector3f center, float radius,const QString& classID) 
			{
				this->spheres_color.append(VRGBAColor(rgb,alpha));
				this->spheres_center.append(center);
				this->spheres_radius.append(radius);
			};

			void ListRenderer::drawTriangle(SyntopiaCore::Math::Vector3f p1,
								SyntopiaCore::Math::Vector3f p2,
								SyntopiaCore::Math::Vector3f p3,
								const QString& classID) 
			{	
				this->triangles.append(VTriangle(this->triangles.count(),p1,p2,p3));
			};

			void ListRenderer::begin() 
			{
				this->spheres_color.clear();
				this->spheres_center.clear();
				this->spheres_radius.clear();

				this->boxes.clear();
				this->boxes_color.clear();

				this->grids.clear();
				this->grids_color.clear();

				this->lines.clear();
				this->lines_color.clear();
			};

			void ListRenderer::end() {

			};

			void ListRenderer::setBackgroundColor(SyntopiaCore::Math::Vector3f /*rgb*/) {
				// TODO
			}

			void ListRenderer::drawMesh(  SyntopiaCore::Math::Vector3f /*startBase*/, 
										SyntopiaCore::Math::Vector3f /*startDir1*/, 
										SyntopiaCore::Math::Vector3f /*startDir2*/, 
										SyntopiaCore::Math::Vector3f /*endBase*/, 
										SyntopiaCore::Math::Vector3f /*endDir1*/, 
										SyntopiaCore::Math::Vector3f /*endDir2*/, 
										const QString& /*classID*/) {
			};

			void ListRenderer::callCommand(const QString& renderClass, const QString& /*command*/) {
				if (renderClass != this->renderClass()) return;
				
			}


		}
	}
}

