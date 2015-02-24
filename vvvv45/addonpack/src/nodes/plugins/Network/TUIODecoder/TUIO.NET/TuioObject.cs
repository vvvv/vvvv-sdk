/*
	TUIO Java backend - part of the reacTIVision project
	http://reactivision.sourceforge.net/

	Copyright (c) 2005-2008 Martin Kaltenbrunner <mkalten@iua.upf.edu>

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

using System;
using System.Collections.Generic;

namespace TUIO.NET
{
	public class TuioObject:TuioContainer {
	
	protected int fiducial_id;
	protected float angle;
	protected float rotation_speed, rotation_accel;
		

	public TuioObject (long s_id, int f_id, float xpos, float ypos, float angle):base(s_id,xpos,ypos) {
		this.fiducial_id = f_id;
		this.angle = angle;
		this.rotation_speed = 0.0f;
		this.rotation_accel = 0.0f;
	}

	
	public TuioObject (TuioObject o):base(o) {
		this.fiducial_id = o.getFiducialID();
		this.angle = o.getAngle();
		this.rotation_speed = 0.0f;
		this.rotation_accel = 0.0f;
	}
		
	public void update (float xpos, float ypos, float angle, float xspeed, float yspeed, float rspeed, float maccel, float raccel) {
		base.update(xpos,ypos,xspeed,yspeed,maccel);
		this.angle = angle;
		this.rotation_speed = rspeed;
		this.rotation_accel = raccel;

	}
		

	public void update (TuioObject o) {
			base.update(o);
			this.angle = o.getAngle();
			this.rotation_speed = o.getRotationSpeed();
			this.rotation_accel = o.getRotationAccel();
	}
		
	public int getFiducialID() {
		return fiducial_id;
	}
	
	public float getAngle() {
		return angle;
	}
		
	public float getAngleDegrees() {
			return angle/(float)Math.PI*180.0f;

	}
	

	public float getRotationSpeed() {
		return rotation_speed;
	}
		
	public float getRotationAccel() {
		return rotation_accel;
	}
	
}

	
}
