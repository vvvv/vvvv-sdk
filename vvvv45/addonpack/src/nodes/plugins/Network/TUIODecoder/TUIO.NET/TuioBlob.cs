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
// 04/12 - bjo:rn  more or less c&p from TuioObject. Added attributes according to Tuio protocol spec.

using System;
using System.Collections.Generic;

namespace TUIO.NET
{
	public class TuioBlob:TuioContainer {

    protected int blob_id;
    protected float angle;
	protected float rotation_speed, rotation_accel;
    protected float width;
    protected float height;
    protected float area;

		

	public TuioBlob (long s_id, int b_id, float xpos, float ypos, float angle, float width, float height, float area):base(s_id,xpos,ypos) {
        this.blob_id = b_id;
        this.width = width;
        this.height = height;
        this.area = area;
        this.angle = angle;
		this.rotation_speed = 0.0f;
		this.rotation_accel = 0.0f;
	}

	
	public TuioBlob (TuioBlob b):base(b) {

        this.blob_id = b.getBlobID();
        this.width = b.getWidth();
        this.height = b.getHeight();
        this.area = b.getArea();
        this.angle = b.getAngle();
		this.rotation_speed = 0.0f;
		this.rotation_accel = 0.0f;
	}
		
	public void update (float xpos, float ypos, float angle, float width, float height, float area, float xspeed, float yspeed, float rspeed, float maccel, float raccel) {
		base.update(xpos,ypos,xspeed,yspeed,maccel);
		this.angle = angle;
        this.width = width;
        this.height = height;
        this.area = area;
		this.rotation_speed = rspeed;
		this.rotation_accel = raccel;

	}
		

	public void update (TuioBlob b) {
			base.update(b);
			this.angle = b.getAngle();
			this.rotation_speed = b.getRotationSpeed();
			this.rotation_accel = b.getRotationAccel();
	}


    public int getBlobID()
    {
        return blob_id;
    }    
        
    public float getWidth() {
		return width;
	}

    public float getHeight()
    {
        return height;
    }

    public float getArea()
    {
        return area;
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
