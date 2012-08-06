//-----------------------------------------------------------------------------
// File: CreateDevice.cpp
//
// Desc: This is the first tutorial for using Direct3D. In this tutorial, all
//       we are doing is creating a Direct3D device and using it to clear the
//       window.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#include <d3d9.h>
#pragma warning( disable : 4996 ) // disable deprecated warning 
#include <strsafe.h>
#pragma warning( default : 4996 )

#pragma comment( lib ,"fantastiqui.lib" )
#include "windows.h"
#include "windowsx.h"
#include "fantastiqui.h"
#include "FUIMain.h"
#include "FUIFlashPlayer.h"

//-----------------------------------------------------------------------------
// Global variables
//-----------------------------------------------------------------------------
LPDIRECT3D9         g_pD3D = NULL; // Used to create the D3DDevice
LPDIRECT3DDEVICE9   g_pd3dDevice = NULL; // Our rendering device


//main fantastiqui gui class
FUIMain* maingui;
//first flash player instance
FUIFlashPlayer* player;

//textures that we will use
LPDIRECT3DTEXTURE9 pTexture[2];

void DrawSquare(int x,int y,int x1,int y1,float fu1,float fv1, float fu2,float fv2,int c1,int c2,int c3,int c4);
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
	return 0;
}

/// Requests from you a pointer to a surface to which flash has to be written
/// for texture pTexture, so here we lock the texture and return the surface pointer
void* __stdcall _GetTextureSurfacePointer(void* p,void* pTexture)
{
	LPDIRECT3DTEXTURE9 tex = (LPDIRECT3DTEXTURE9)pTexture;
	D3DLOCKED_RECT r;
	tex->LockRect(0,&r,NULL,0);//D3DLOCK_NO_DIRTY_UPDATE
	return r.pBits;
}        

/// Fantastiqui calls this function when texture editing is complete, and the
/// texture can be unlocked again
int __stdcall _ReleaseTextureSurfacePointer(void* p,void* pTexture,void* pPointer)
{
	LPDIRECT3DTEXTURE9 tex = (LPDIRECT3DTEXTURE9)pTexture;
	tex->UnlockRect(0);
	//tex->
	return S_OK;
}

/// DirtyRect is a callback that passes us regions of an updated part
/// of the texture, we pass this to the ditry texture class as
/// dirty rectangles so directx updates them
void __stdcall _DirtyRect(void* p,void* pTexture,int x,int y, int x1,int y1)
{
	LPDIRECT3DTEXTURE9 tex = (LPDIRECT3DTEXTURE9)pTexture;
	RECT r;
	r.top = y;r.left = x;
	r.right= x1;r.bottom = y1;
	tex->AddDirtyRect(&r);
	return;
}
//-----------------------------------------------------------------------------
// Name: InitD3D()
// Desc: Initializes Direct3D
//-----------------------------------------------------------------------------
int iSX,iSY,iNumFrames;float fFPS;

HRESULT InitD3D( HWND hWnd )
{
	char path[256];
	char srchpath[256];
	wchar_t srchpathw[256];
	wchar_t srchpathw1[256];

	GetModuleFileNameA(NULL,path,255); 
	AppPath(path,srchpath);	
	for(int i =0;i < 256;i++){
		srchpathw[i] = srchpath[i];
	}
	memcpy(srchpathw1,srchpathw,512);

    // Create the D3D object, which is needed to create the D3DDevice.
    if( NULL == ( g_pD3D = Direct3DCreate9( D3D_SDK_VERSION ) ) )
        return E_FAIL;

    // Set up the structure used to create the D3DDevice. Most parameters are
    // zeroed out. We set Windowed to TRUE, since we want to do D3D in a
    // window, and then set the SwapEffect to "discard", which is the most
    // efficient method of presenting the back buffer to the display.  And 
    // we request a back buffer format that matches the current desktop display 
    // format.
    D3DPRESENT_PARAMETERS d3dpp;
    ZeroMemory( &d3dpp, sizeof( d3dpp ) );
    d3dpp.Windowed = TRUE;
    d3dpp.SwapEffect = D3DSWAPEFFECT_DISCARD;
    d3dpp.BackBufferFormat = D3DFMT_UNKNOWN;
	//d3dpp.m
    // Create the Direct3D device. Here we are using the default adapter (most
    // systems only have one, unless they have multiple graphics hardware cards
    // installed) and requesting the HAL (which is saying we want the hardware
    // device rather than a software one). Software vertex processing is 
    // specified since we know it will work on all cards. On cards that support 
    // hardware vertex processing, though, we would see a big performance gain 
    // by specifying hardware vertex processing.
    if( FAILED( g_pD3D->CreateDevice( D3DADAPTER_DEFAULT, D3DDEVTYPE_HAL, hWnd,
                                      D3DCREATE_SOFTWARE_VERTEXPROCESSING | D3DCREATE_MULTITHREADED,
                                      &d3dpp, &g_pd3dDevice ) ) )
    {
        return E_FAIL;
    }

    // Device state would normally be set here


	////////////////////////////////////////////////////////////////////////////
	// Fantastiqui oode
	////////////////////////////////////////////////////////////////////////////

	//create the main gui class
	maingui = CreateFantastiqUI();
	//provide the callback pointers to the main gui class
	maingui->CreateUI(&_ResizeTexture,&_GetTextureSurfacePointer,
				    &_ReleaseTextureSurfacePointer,0,&_DirtyRect);

	maingui->LoadFlashHeader(strcat(srchpath,"\\vt.swf"),&iSX,&iSY,&fFPS,&iNumFrames);
	for(int i = 0;i < 2;i++)
		g_pd3dDevice->CreateTexture(iSX,iSY,1,0,D3DFMT_X8R8G8B8,D3DPOOL_MANAGED,&pTexture[i],NULL);

/*
	//create 2 textures for the first flash player
	int t1 = pTexture->CreateTexture(512,256,false,0);
	int t2 = pTexture->CreateTexture(512,256,false,0);*/

	//create the flashplayer and load the movie
	player = maingui->CreateFlashPlayer();
	player->SetFlashSettings(false,0,true,0x00000000);
	player->CreateFlashControl(0,iSX,iSY,(void*)pTexture[0],(void*)pTexture[1],false);
	player->LoadMovie(wcscat(srchpathw,L"\\vt.swf"));
	player->SetFrameTime(1000.0f/30.0f);

    return S_OK;
}




//-----------------------------------------------------------------------------
// Name: Cleanup()
// Desc: Releases all previously initialized objects
//-----------------------------------------------------------------------------
VOID Cleanup()
{
	maingui->DeleteFlashPlayer(player);
	DeleteFantastiqUI(maingui);

    if( g_pd3dDevice != NULL )
        g_pd3dDevice->Release();

    if( g_pD3D != NULL )
        g_pD3D->Release();
}




//-----------------------------------------------------------------------------
// Name: Render()
// Desc: Draws the scene
//-----------------------------------------------------------------------------
VOID Render()
{
    if( NULL == g_pd3dDevice )
        return;

    // Clear the backbuffer to a blue color
    g_pd3dDevice->Clear( 0, NULL, D3DCLEAR_TARGET, D3DCOLOR_XRGB( 0, 0, 255 ), 1.0f, 0 );

    // Begin the scene
    if( SUCCEEDED( g_pd3dDevice->BeginScene() ) )
    {
        // Rendering of scene objects can happen here
		g_pd3dDevice->SetTexture(0,(LPDIRECT3DTEXTURE9)player->GetTexture());
		DrawSquare(0,0,iSX-1,iSY-1,0,0,1,1,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF);
		g_pd3dDevice->SetTexture(0,NULL);
		player->ReleaseTexture();

        // End the scene
        g_pd3dDevice->EndScene();
    }

    // Present the backbuffer contents to the display
    g_pd3dDevice->Present( NULL, NULL, NULL, NULL );
}

//2d vertex
struct TLVERTEX
{
    float x;
    float y;
    float z;
    float rhw;
    D3DCOLOR colour;
    float u;
    float v;
};
const DWORD D3DFVF_TLVERTEX = D3DFVF_XYZRHW | D3DFVF_DIFFUSE | D3DFVF_TEX1;

void DrawSquare(int x,int y,int x1,int y1,float fu1,float fv1, float fu2,float fv2,int c1,int c2,int c3,int c4)
{
	//create the quad vertices
	TLVERTEX vertices[4];
	vertices[0].colour = c1;
	vertices[0].x = (float) x - 0.5f;
	vertices[0].y = (float) y - 0.5f;
	vertices[0].z = 0.0f;
	vertices[0].rhw = 1.0f;
	vertices[0].u = fu1;
	vertices[0].v = fv1;

	vertices[1].colour = c2;
	vertices[1].x = (float) x1 - 0.5f;
	vertices[1].y = (float) y - 0.5f;
	vertices[1].z = 0.0f;
	vertices[1].rhw = 1.0f;
	vertices[1].u = fu2;
	vertices[1].v = fv1;

	vertices[2].colour = c3;
	vertices[2].x = (float) x1 - 0.5f;
	vertices[2].y = (float) y1 - 0.5f;
	vertices[2].z = 0.0f;
	vertices[2].rhw = 1.0f;
	vertices[2].u = fu2;
	vertices[2].v = fv2;

	vertices[3].colour = c4;
	vertices[3].x = (float) x - 0.5f;
	vertices[3].y = (float) y1 - 0.5f;
	vertices[3].z = 0.0f;
	vertices[3].rhw = 1.0f;
	vertices[3].u = fu1;
	vertices[3].v = fv2;

	//draw the quad
	g_pd3dDevice->SetRenderState(D3DRS_CULLMODE, D3DCULL_NONE);

	g_pd3dDevice->SetRenderState(D3DRS_FOGENABLE,false);
	g_pd3dDevice->SetRenderState(D3DRS_ALPHABLENDENABLE,true);		//alpha blending enabled

	// use alpha channel in texture for alpha
	g_pd3dDevice->SetTextureStageState(0, D3DTSS_ALPHAARG1, D3DTA_TEXTURE);
	g_pd3dDevice->SetTextureStageState(0, D3DTSS_ALPHAOP, D3DTOP_SELECTARG1);

	g_pd3dDevice->SetRenderState(D3DRS_SRCBLEND ,D3DBLEND_SRCALPHA);
	g_pd3dDevice->SetRenderState(D3DRS_DESTBLEND ,D3DBLEND_INVSRCALPHA);		//alpha blending enabled

	g_pd3dDevice->SetFVF(D3DFVF_TLVERTEX);
	g_pd3dDevice->DrawPrimitiveUP(D3DPT_TRIANGLEFAN,2,&vertices[0],sizeof(TLVERTEX));
}

//-----------------------------------------------------------------------------
// Name: MsgProc()
// Desc: The window's message handler
//-----------------------------------------------------------------------------
LRESULT WINAPI MsgProc( HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam )
{
    switch( msg )
    {
        case WM_DESTROY:
            Cleanup();
            PostQuitMessage( 0 );
            return 0;

        case WM_PAINT:
//            Render();
//            ValidateRect( hWnd, NULL );
            return 0;
		case WM_MOUSEMOVE:
			player->UpdateMousePosition(GET_X_LPARAM(lParam),GET_Y_LPARAM(lParam));
			return 0;
		case WM_LBUTTONDOWN:
			player->UpdateMouseButton(0,true);
			return 0;
		case WM_LBUTTONUP:
			player->UpdateMouseButton(0,false);
			return 0;

    }

    return DefWindowProc( hWnd, msg, wParam, lParam );
}




//-----------------------------------------------------------------------------
// Name: wWinMain()
// Desc: The application's entry point
//-----------------------------------------------------------------------------
INT WINAPI wWinMain( HINSTANCE hInst, HINSTANCE, LPWSTR, INT )
{
    // Register the window class
    WNDCLASSEX wc =
    {
        sizeof( WNDCLASSEX ), CS_CLASSDC, MsgProc, 0L, 0L,
        GetModuleHandle( NULL ), NULL, NULL, NULL, NULL,
        L"D3D Tutorial", NULL
    };
    RegisterClassEx( &wc );

    // Create the application's window
    HWND hWnd = CreateWindow( L"D3D Tutorial", L"D3D Tutorial 01: CreateDevice",
                              WS_OVERLAPPEDWINDOW, 100, 100, 800, 600,
                              NULL, NULL, wc.hInstance, NULL );

    // Initialize Direct3D
    if( SUCCEEDED( InitD3D( hWnd ) ) )
    {
        // Show the window
        ShowWindow( hWnd, SW_SHOWDEFAULT );
        UpdateWindow( hWnd );

        // Enter the message loop
        MSG msg;
        while( GetMessage( &msg, NULL, 0, 0 ) )
        {
			Render();
            TranslateMessage( &msg );
            DispatchMessage( &msg );
        }
    }

    UnregisterClass( L"D3D Tutorial", wc.hInstance );
    return 0;
}



