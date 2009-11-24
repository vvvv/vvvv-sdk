#include "OpenGLRenderer.h"
#include "../../../SyntopiaCore/GLEngine/Sphere.h"
#include "../../../SyntopiaCore/GLEngine/Box.h"
#include "../../../SyntopiaCore/GLEngine/Grid.h"
#include "../../../SyntopiaCore/GLEngine/Dot.h"
#include "../../../SyntopiaCore/GLEngine/Line.h"
#include "../../../SyntopiaCore/GLEngine/Mesh.h"
#include "../../../SyntopiaCore/GLEngine/Triangle.h"
#include "../../../SyntopiaCore/Math/Vector3.h"

using namespace SyntopiaCore::GLEngine;
using namespace SyntopiaCore::Math;

#include "../../../SyntopiaCore/Logging/Logging.h"

using namespace SyntopiaCore::Logging;

namespace StructureSynth {
	namespace Model {	
		namespace Rendering {

			void OpenGLRenderer::drawBox(SyntopiaCore::Math::Vector3f base, 
				SyntopiaCore::Math::Vector3f dir1 , 
				SyntopiaCore::Math::Vector3f dir2, 
				SyntopiaCore::Math::Vector3f dir3, const QString &) {
					Object3D* o =new Box( base, dir1, dir2, dir3);
					o->setColor(rgb, alpha);
				
					engine->addObject(o);
			};

			
			void OpenGLRenderer::drawMesh(  SyntopiaCore::Math::Vector3f startBase, 
										SyntopiaCore::Math::Vector3f startDir1, 
										SyntopiaCore::Math::Vector3f startDir2, 
										SyntopiaCore::Math::Vector3f endBase, 
										SyntopiaCore::Math::Vector3f endDir1, 
										SyntopiaCore::Math::Vector3f endDir2, 
										const QString& /*classID*/) {
					Object3D* o =new Mesh( startBase, startDir1, startDir2, endBase, endDir1, endDir2);
					o->setColor(rgb, alpha);
				
					engine->addObject(o);
			};

			void OpenGLRenderer::drawGrid(SyntopiaCore::Math::Vector3f base, 
				SyntopiaCore::Math::Vector3f dir1 , 
				SyntopiaCore::Math::Vector3f dir2, 
				SyntopiaCore::Math::Vector3f dir3, const QString &) {
					Object3D* o = new Grid( base, dir1, dir2, dir3);
					o->setColor(rgb, alpha);
					engine->addObject(o);
			};

			void OpenGLRenderer::drawLine(SyntopiaCore::Math::Vector3f from, SyntopiaCore::Math::Vector3f to, const QString &) {
					Object3D* o = new Line( from, to);
					o->setColor(rgb, alpha);
					engine->addObject(o);
			};

			void OpenGLRenderer::drawTriangle(SyntopiaCore::Math::Vector3f p1,
										 SyntopiaCore::Math::Vector3f p2,
									     SyntopiaCore::Math::Vector3f p3,
										 const QString& /*classID*/) {
				   Object3D* o = new Triangle(p1, p2,p3);
				   o->setColor(rgb, alpha);
				   engine->addObject(o);
			}

			void OpenGLRenderer::drawDot(SyntopiaCore::Math::Vector3f v, const QString &) {
					Object3D* o = new Dot(v);
					o->setColor(rgb, alpha);
					engine->addObject(o);	
			};

			void OpenGLRenderer::drawSphere(SyntopiaCore::Math::Vector3f center, float radius, const QString &) {
				Object3D* o = new Sphere( center, radius);
				o->setColor(rgb, alpha);
				engine->addObject(o);
			};

			void OpenGLRenderer::begin() {
				engine->clearWorld();
				engine->setBackgroundColor(0,0,0);
				rgb = Vector3f(1,0,0);
				alpha = 1;
			};

			void OpenGLRenderer::end() {
				INFO(QString("Rendering done. Wrote %1 objects.").arg(engine->objectCount()));
				engine->requireRedraw();
			};

			void OpenGLRenderer::setBackgroundColor(SyntopiaCore::Math::Vector3f rgb) {
				engine->setBackgroundColor(rgb.x(),rgb.y(),rgb.z());
			}

			void OpenGLRenderer::callCommand(const QString& /*renderClass*/, const QString& /*command*/) {
			}

			void OpenGLRenderer::setTranslation(SyntopiaCore::Math::Vector3f translation) {
				engine->setTranslation(translation);
			};

			void OpenGLRenderer::setScale(double scale) {
				engine->setScale(scale);
			};

			void OpenGLRenderer::setRotation(SyntopiaCore::Math::Matrix4f rotation) {
				engine->setRotation(rotation);
			};

			
			void OpenGLRenderer::setPivot(SyntopiaCore::Math::Vector3f pivot) {
				engine->setPivot(pivot);
			};

				

		}
	}
}

