/*  reacTIVision fiducial tracking framework
    FiducialObject.h
    Copyright (C) 2006 Martin Kaltenbrunner <mkalten@iua.upf.es>

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/

#ifndef FIDOBJECT_H
#define FIDOBJECT_H

#ifdef WIN32
#include <windows.h>
#else
#include <sys/time.h>
#endif

#include "math.h"
#include "stdio.h"
#include "stdlib.h"

#define DOUBLEPI 6.283185307179586

struct frame {
	float xpos,ypos,angle;
	float raw_xpos, raw_ypos, raw_angle;
	float rotation_speed, rotation_accel;
	float motion_speed, motion_accel;
	float motion_speed_x, motion_speed_y;
	long time;
};

class FiducialObject {
  public:
	bool alive;
	int unsent;
	int session_id;
	int fiducial_id;
	frame current, last;
	
  private:
	bool updated;
	int lost_frames;
	
	float width;
	float height;
	
	void positionFilter();
	void computeSpeedAccel();
	void saveLastFrame();
	long getCurrentTime();

	char message[128];

  public:
	FiducialObject(int s_id, int f_id, int width, int height);
	~FiducialObject();
	void update(float x, float y, float a);

	bool checkRemoved();
	float distance(float x, float y);
	//void che
};


#endif
