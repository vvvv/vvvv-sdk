/*  reacTIVision fiducial tracking framework
    FiducialObject.cpp
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

#include "FiducialObject.h"
#include <string>

FiducialObject::FiducialObject (int s_id, int f_id, int width, int height) {

	this->session_id    = s_id,
	this->fiducial_id   = f_id;
	this->width     = (float)width;
	this->height    = (float)height;
	
	updated = false;
	alive   = false;

	unsent = 0;
	lost_frames = 0;
	
	current.xpos = current.ypos = current.raw_xpos = current.raw_ypos = -100.0f;
	current.angle = current.raw_angle = 0.0f;
	current.rotation_speed = current.rotation_accel = 0.0f;
	current.motion_speed = current.motion_accel = 0.0f;
	current.motion_speed_x = current.motion_speed_y = 0.0f; 
	current.time = 0;

	saveLastFrame();
}

FiducialObject::~FiducialObject() {}

void FiducialObject::update (float x, float y, float a) {

	current.time = getCurrentTime();
	
	current.raw_xpos = x;
	current.raw_ypos = y;
	current.raw_angle = (float)(DOUBLEPI-a);
	
	// fix possible position and angle jitter
	positionFilter();
	// calculate movement and rotation speed and acceleration
	computeSpeedAccel();
	
	updated = true;
	alive = true;
	lost_frames = 0;

}

void FiducialObject::saveLastFrame() {

	last.time = current.time;
	last.xpos = current.xpos;
	last.ypos = current.ypos;
	last.angle = current.angle;
	last.motion_speed = current.motion_speed;
	last.rotation_speed = current.rotation_speed;
}

void FiducialObject::positionFilter() {
	
	// TODO 
	// most definitely there is a more sophisticated way
	// to remove position and angle jitter
	// rather than defining a one pixel threshold 

	float threshold = 1.0;
	
	if (fabs(current.raw_xpos-last.xpos)>threshold) current.xpos = current.raw_xpos;
	else current.xpos = last.xpos;

	if (fabs(current.raw_ypos-last.ypos)>threshold) current.ypos = current.raw_ypos;
	else current.ypos = last.ypos;

	if (fabs(current.raw_angle-last.angle)>threshold/20) current.angle = current.raw_angle;
	else current.angle = last.angle;
}

void FiducialObject::computeSpeedAccel() {

	if (last.time==0) return;

	int   dt = current.time - last.time;
	float dx = current.xpos - last.xpos;
	float dy = current.ypos - last.ypos;
	float dist = sqrt(dx*dx+dy*dy);

	current.motion_speed  = dist/dt;
	current.motion_speed_x = fabs(dx/dt);
	current.motion_speed_y = fabs(dy/dt);
	current.rotation_speed  = fabs(current.angle-last.angle)/dt;

	current.motion_accel = (current.motion_speed-last.motion_speed)/dt;
	current.rotation_accel = (current.rotation_speed-last.rotation_speed)/dt;
}

bool FiducialObject::checkRemoved() {

	int frame_threshold  = (int)ceil(sqrt(/*(float)portVideoSDL::current_fps*/25));
	if (lost_frames>frame_threshold) 
    {
	/*	alive = false;
		current.xpos = current.ypos = current.raw_xpos = current.raw_ypos = -100.0f;
		current.angle = current.raw_angle = 0.0f;
		current.rotation_speed = current.rotation_accel = 0.0f;
		current.motion_speed = current.motion_accel = 0.0f;
		current.motion_speed_x = current.motion_speed_y = 0.0f; 
		current.time = 0;
		lost_frames = 0;*/
		return true;
	} 
    else 
    {
		lost_frames++;
		return false; 
	}
}


float FiducialObject::distance(float x, float y) {

	// returns the distance to the current position
	float dx = x - current.xpos;
	float dy = y - current.ypos;
	return sqrt(dx*dx+dy*dy);
}

long FiducialObject::getCurrentTime() {

	#ifdef WIN32
		long timestamp = GetTickCount();
	#else
		struct timeval tv;
		struct timezone tz;
		gettimeofday(&tv,&tz);
		long timestamp = (tv.tv_sec*1000)+(tv.tv_usec/1000);
	#endif
	
	return timestamp;
}
