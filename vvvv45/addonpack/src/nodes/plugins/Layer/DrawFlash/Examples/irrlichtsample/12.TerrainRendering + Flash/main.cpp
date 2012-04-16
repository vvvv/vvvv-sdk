/*
This tutorial will briefly show how to use the terrain renderer of Irrlicht. It will also
show the terrain renderer triangle selector to be able to do collision detection with
terrain.

Note that the Terrain Renderer in Irrlicht is based on Spintz' GeoMipMapSceneNode, lots 
of thanks go to him.
DeusXL provided a new elegant simple solution for building larger area on small heightmaps
-> terrain smoothing.
In the beginning there is nothing special. We include the needed header files and create
an event listener to listen if the user presses the 'W' key so we can switch to wireframe
mode and if he presses 'D' we toggle to material between solid and detail mapped.
*/
#include <irrlicht.h>
#include <iostream>
#include <windows.h>

#pragma comment( lib ,"fantastiqui.lib" )
#include "fantastiqui.h"
#include "FUIMain.h"
#include "FUIFlashPlayer.h"

using namespace irr;

#pragma comment(lib, "Irrlicht.lib")

//main fantastiqui gui class
FUIMain* maingui;
//first flash player instance
FUIFlashPlayer* player = NULL;
irr::video::ITexture* tex1;
irr::video::ITexture* tex2;

int sx,sy,numframes;
float fps;

char freepath[256];
char path[256];
char srchpath[256];

void AppPath(char* PathOfFile, char* ret_Path)
{							
	char* found = strrchr(PathOfFile, '\\');
	if(!found)
	{
		// check with '/' path format
		found = strrchr(PathOfFile, '/');
		if(!found)
		{
			// no path herre it's just a file.
			// so just blank output
			ret_Path[0] = 0;
		}
		else
		{
			// copy just a part of the string
			int size = (int)found - (int)PathOfFile + 1;
			strncpy(ret_Path, PathOfFile, size);
			ret_Path[size] = 0;
		}

	}
	else
	{
		// copy just a part of the string
		int size = (int)found - (int)PathOfFile + 1;
		strncpy(ret_Path, PathOfFile, size);
		ret_Path[size] = 0;
	}		
}
/// In case you resize the flash player, this function is called to tell you to
/// actually resize the textures used, as return value you can provide a new
/// texture pointer that will be used from that moment
void* __stdcall _ResizeTexture(void* p,void* _pTexture,int iSizeX,int iSizeY,int iReserved)
{
	return NULL;
}

/// Requests from you a pointer to a surface to which flash has to be written
/// for texture pTexture, so here we lock the texture and return the surface pointer
void* __stdcall _GetTextureSurfacePointer(void* p,void* pTexture)
{
	void* pData = ((irr::video::ITexture*)pTexture)->lock();
	return pData;
}        

/// Fantastiqui calls this function when texture editing is complete, and the
/// texture can be unlocked again
int __stdcall _ReleaseTextureSurfacePointer(void* p,void* pTexture,void* pPointer)
{
	((irr::video::ITexture*)pTexture)->unlock();
	return S_OK;
}

/// DirtyRect is a callback that passes us regions of an updated part
/// of the texture, we pass this to the ditry texture class as
/// dirty rectangles so directx updates them
void __stdcall _DirtyRect(void* p,void* pTexture,int x,int y, int x1,int y1)
{
}
class MyEventReceiver : public IEventReceiver
{
public:

	MyEventReceiver(scene::ISceneNode* terrain)
	{
		// store pointer to terrain so we can change its drawing mode
		Terrain = terrain;
	}

	bool OnEvent(const SEvent& event)
	{
		// check if user presses the key 'W' or 'D'
		if (event.EventType == irr::EET_KEY_INPUT_EVENT && !event.KeyInput.PressedDown)
		{
			if(event.KeyInput.Char){
//				maingui->InputChar(event.KeyInput.Char,0);
			}

//			maingui->InputKey(event.KeyInput.Key,0);

			switch (event.KeyInput.Key)
			{
			case irr::KEY_KEY_W: // switch wire frame mode
				//Terrain->setMaterialFlag(video::EMF_WIREFRAME, !Terrain->getMaterial(0).Wireframe);
				//Terrain->setMaterialFlag(video::EMF_POINTCLOUD, false);
				return true;
			case irr::KEY_KEY_P: // switch wire frame mode
				//Terrain->setMaterialFlag(video::EMF_POINTCLOUD, !Terrain->getMaterial(0).PointCloud);
				//Terrain->setMaterialFlag(video::EMF_WIREFRAME, false);
				return true;
			case irr::KEY_KEY_D: // toggle detail map
				//Terrain->setMaterialType(
				//	Terrain->getMaterial(0).MaterialType == video::EMT_SOLID ? 
				//	video::EMT_DETAIL_MAP : video::EMT_SOLID);
				return true;
			}
		}
		if(event.EventType == irr::EET_MOUSE_INPUT_EVENT){
			if(event.MouseInput.Event == irr::EMIE_MOUSE_MOVED){
				if(player)
					player->UpdateMousePosition(event.MouseInput.X,event.MouseInput.Y);
			}
			if(event.MouseInput.Event == irr::EMIE_LMOUSE_PRESSED_DOWN){
				if(player)
					player->UpdateMouseButton(0,true);
			}
			if(event.MouseInput.Event == irr::EMIE_LMOUSE_LEFT_UP){
				if(player)
					player->UpdateMouseButton(0,false);
			}
		}

		return false;
	}

private:
	scene::ISceneNode* Terrain;
};


/*
The start of the main function starts like in most other example. We ask the user
for the desired renderer and start it up.
*/
int main()
{
	// let user select driver type

	video::E_DRIVER_TYPE driverType = video::EDT_DIRECT3D9;

	printf("Please select the driver you want for this example:\n"\
		" (a) Direct3D 9.0c\n (b) Direct3D 8.1\n (c) OpenGL 1.5\n"\
		" (d) Software Renderer\n (e) Burning's Software Renderer\n"\
		" (f) NullDevice\n (otherKey) exit\n\n");

	char i;
	std::cin >> i;

	switch(i)
	{
		case 'a': driverType = video::EDT_DIRECT3D9;break;
		case 'b': driverType = video::EDT_DIRECT3D8;break;
		case 'c': driverType = video::EDT_OPENGL;   break;
		case 'd': driverType = video::EDT_SOFTWARE; break;
		case 'e': driverType = video::EDT_BURNINGSVIDEO;break;
		case 'f': driverType = video::EDT_NULL;     break;
		default: return 1;
	}	

	// create device

	IrrlichtDevice* device = createDevice(driverType, core::dimension2d<s32>(640, 480));

	if (device == 0)
		return 1; // could not create selected driver.

	
	/*
	First, we add standard stuff to the scene: A nice irrlicht engine
	logo, a small help text, a user controlled camera, and we disable
	the mouse cursor.
	*/   

	video::IVideoDriver* driver = device->getVideoDriver();
	scene::ISceneManager* smgr = device->getSceneManager();
	gui::IGUIEnvironment* env = device->getGUIEnvironment();

	driver->setTextureCreationFlag(video::ETCF_ALWAYS_32_BIT, true);

	// add irrlicht logo
	//env->addImage(driver->getTexture("media/irrlichtlogo2.png"),
	//	core::position2d<s32>(10,10));

	//set other font
	//env->getSkin()->setFont(env->getFont("media/fontlucida.png"));

	// add some help text
	gui::IGUIStaticText* text = env->addStaticText(
		L"Press 'W' to change wireframe mode\nPress 'D' to toggle detail map",
		core::rect<s32>(10,440,250,475), true, true, 0, -1, true);

	// add camera
	scene::ICameraSceneNode* camera = 
		smgr->addCameraSceneNodeFPS(0,100.0f,1200.f);

	camera->setInputReceiverEnabled(false);
	camera->setPosition(core::vector3df(1900*2,255*2,3700*2));
	camera->setTarget(core::vector3df(2397*2,343*2,2700*2));
	camera->setFarValue(12000.0f);

	// disable mouse cursor
	device->getCursorControl()->setVisible(true);
	//device->getCursorControl()->
	/*
	Here comes the terrain renderer scene node: We add it just like any 
	other scene node to the scene using ISceneManager::addTerrainSceneNode(). 
	The only parameter we use is a file name to the heightmap we use. A heightmap
	is simply a gray scale texture. The terrain renderer loads it and creates 
	the 3D terrain from it.
	To make the terrain look more big, we change the scale factor of it to (40, 4.4, 40).
	Because we don't have any dynamic lights in the scene, we switch off the lighting,
	and we set the file terrain-texture.jpg as texture for the terrain and 
	detailmap3.jpg as second texture, called detail map. At last, we set
	the scale values for the texture: The first texture will be repeated only one time over 
	the whole terrain, and the second one (detail map) 20 times. 
	*/

	// add terrain scene node
	/*
	scene::ITerrainSceneNode* terrain = smgr->addTerrainSceneNode( 
		"media/terrain-heightmap.bmp",
		0,										// parent node
		-1,										// node id
		core::vector3df(0.f, 0.f, 0.f),			// position
		core::vector3df(0.f, 0.f, 0.f),			// rotation
		core::vector3df(40.f, 4.4f, 40.f),		// scale
		video::SColor ( 255, 255, 255, 255 ),	// vertexColor,
		5,										// maxLOD
		scene::ETPS_17,							// patchSize
		4										// smoothFactor
		);

	terrain->setMaterialFlag(video::EMF_LIGHTING, false);*/

	//terrain->setMaterialTexture(0, driver->getTexture("media/terrain-texture.jpg"));
	//terrain->setMaterialTexture(1, driver->getTexture("media/detailmap3.jpg"));
	
//	terrain->setMaterialType(video::EMT_DETAIL_MAP);

//	terrain->scaleTexture(1.0f, 20.0f);
	//terrain->setDebugDataVisible ( true );

	/*
	To be able to do collision with the terrain, we create a triangle selector.
	If you want to know what triangle selectors do, just take a look into the 
	collision tutorial. The terrain triangle selector works together with the
	terrain. To demonstrate this, we create a collision response animator 
	and attach it to the camera, so that the camera will not be able to fly 
	through the terrain.
	*/

	// create triangle selector for the terrain	
	/*
	scene::ITriangleSelector* selector
		= smgr->createTerrainTriangleSelector(terrain, 0);
	terrain->setTriangleSelector(selector);

	// create collision response animator and attach it to the camera
	scene::ISceneNodeAnimator* anim = smgr->createCollisionResponseAnimator(
		selector, camera, core::vector3df(60,100,60),
		core::vector3df(0,0,0), 
		core::vector3df(0,50,0));
	selector->drop();
	camera->addAnimator(anim);
	anim->drop();*/

	/*
	To make the user be able to switch between normal and wireframe mode, we create
	an instance of the event reciever from above and let Irrlicht know about it. In 
	addition, we add the skybox which we already used in lots of Irrlicht examples.
	*/

	// create event receiver
	MyEventReceiver receiver(NULL);
	device->setEventReceiver(&receiver);

   	// create skybox
	driver->setTextureCreationFlag(video::ETCF_CREATE_MIP_MAPS, false);

	/*
	smgr->addSkyBoxSceneNode(
		driver->getTexture("media/irrlicht2_up.jpg"),
		driver->getTexture("media/irrlicht2_dn.jpg"),
		driver->getTexture("media/irrlicht2_lf.jpg"),
		driver->getTexture("media/irrlicht2_rt.jpg"),
		driver->getTexture("media/irrlicht2_ft.jpg"),
		driver->getTexture("media/irrlicht2_bk.jpg"));*/

	driver->setTextureCreationFlag(video::ETCF_CREATE_MIP_MAPS, true);


	
		//////////////////////////////////////////////////////////////////////////
		//Flash code
		//////////////////////////////////////////////////////////////////////////
		GetModuleFileName(NULL,path,255); 
		AppPath(path,srchpath);	
		wchar_t pathw[512];
		for(int i = 0;i < 512;i++)
			pathw[i] = srchpath[i];

	//create the main gui class
	maingui = CreateFantastiqUI();
	//provide the callback pointers to the main gui class
	maingui->CreateUI(&_ResizeTexture,&_GetTextureSurfacePointer,
				    &_ReleaseTextureSurfacePointer,0,&_DirtyRect);

	maingui->LoadFlashHeader(strcat(srchpath,"\\vt.swf"),&sx,&sy,&fps,&numframes);
	//sx = 512;sy = 512;
	//create 2 textures for the first flash player
	tex1 = driver->addTexture(irr::core::dimension2d<int>(sx,sy),"flash1");
	tex2 = driver->addTexture(irr::core::dimension2d<int>(sx,sy),"flash2");

	//create the flashplayer and load the movie
	player = maingui->CreateFlashPlayer();
	player->SetFlashSettings(false,0,true,0x000000FF);
	player->CreateFlashControl(2,sx,sy,tex1,tex2,false);
	player->LoadMovie(wcscat(pathw,L"\\vt.swf"));
	player->SetFrameTime(1000.0f / 25.0f);

	//irr::core::dimension2d<int> texdim;
	//texdim.set(512,512);
		/*
	irr::video::ITexture* tex = driver->addTexture(irr::core::dimension2d<int>(512,512),"flash1");
	tex->lock();
	tex->unlock();*/
	//tex->

	//env->addImage((irr::video::ITexture*)flashplayer->GetTexture(),irr::core::position2d<int>(25,25),false);
	//env->addImage((irr::video::ITexture*)flashplayer2->GetTexture(),irr::core::position2d<int>(350,25),true);

	//const irr::core::position2d<s32> pos1(25,25);
	//const irr::core::rect<s32> des1(0,0,512,256);
	//driver->draw2DImage((irr::video::ITexture*)flashplayer->GetTexture(),irr::core::position2d<s32>(25,25),
	//	irr::core::rect<s32>(0,0,512,256),NULL,irr::video::SColor(255,255,255,255),false);
	//tex->
	/*
	That's it, draw everything. Now you know how to use terrain in Irrlicht.
	*/

	int lastFPS = -1;

	while(device->run())
	if (device->isWindowActive())
	{
		driver->beginScene(true, true, 0 );

		smgr->drawAll();
		env->drawAll();

		driver->draw2DImage((irr::video::ITexture*)player->GetTexture(),irr::core::position2d<s32>(0,0),
			irr::core::rect<s32>(0,0,sx-1,sy-1),NULL,irr::video::SColor(210,255,255,255),true);

/*
		driver->draw2DImage((irr::video::ITexture*)flashplayer2->GetTexture(),irr::core::position2d<s32>(0,355),
			irr::core::rect<s32>(0,0,512,128),NULL,irr::video::SColor(255,255,255,255),true);
*/
		driver->endScene();
		player->ReleaseTexture();

		// display frames per second in window title
		int fps = driver->getFPS();
		if (lastFPS != fps)
		{
			core::stringw str = L"Terrain Renderer - Irrlicht Engine [";
			str += driver->getName();
			str += "] FPS:";
			str += fps;
			// Also print terrain height of current camera position
			// We can use camera position because terrain is located at coordinate origin
			str += " Height: ";
//			str += terrain->getHeight(camera->getAbsolutePosition().X, camera->getAbsolutePosition().Z);

			device->setWindowCaption(str.c_str());
			lastFPS = fps;
		}
	}

	maingui->DeleteFlashPlayer(player);
	DeleteFantastiqUI(maingui);

	device->drop();
	
	return 0;
}

