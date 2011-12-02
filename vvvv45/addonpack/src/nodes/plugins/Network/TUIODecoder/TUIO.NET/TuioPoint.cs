/*
    TUIO C# Library - part of the reacTIVision project
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

namespace TUIO.NET
{

public class TuioPoint {

	public float xpos, ypos;
		
	public TuioPoint (float xpos, float ypos) {
		this.xpos = xpos;
		this.ypos = ypos;
	}


	
	public TuioPoint (TuioPoint p) {
		this.xpos = p.getX();
		this.ypos = p.getY();			
	}
		
	public void update (float xpos, float ypos) {
		this.xpos = xpos;
		this.ypos = ypos;
	}
	
	public void update (TuioPoint p) {
		this.xpos = p.getX();
		this.ypos = p.getY();			
	}
		
	public float getX() {
		return xpos;
	}
	
	public float getY() {
		return ypos;
	}
		
	public float getDistance(float x, float y) {
		float dx = xpos-x;
		float dy = ypos-y;
		return (float)Math.Sqrt(dx*dx+dy*dy);
	}
	
	public float getDistance(TuioPoint pt) {
		float dx = xpos-pt.getX();
		float dy = ypos-pt.getY();
		return (float)Math.Sqrt(dx*dx+dy*dy);
	}
		

	public float getAngle(TuioPoint tuioPoint) {
		
		float side = tuioPoint.getX()-xpos;
		float height = tuioPoint.getY()-ypos;
		float distance = tuioPoint.getDistance(xpos,ypos);

			
		float angle = (float)(Math.Asin(side/distance)+Math.PI/2);
		if (height<0) angle = 2.0f*(float)Math.PI-angle;
				
		return angle;
	}
		
	public float getScreenX(int w) {
		return (int)(xpos*w);
	}
	
	public int getScreenY(int h) {
		return (int)(ypos*h);
	}
		
	public TuioPoint getPosition() {
			return new TuioPoint(xpos,ypos);
	}
    }
}
