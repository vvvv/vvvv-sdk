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

	public interface TuioListener
	{
		void addTuioObject(TuioObject tuioObject);
		void updateTuioObject(TuioObject tuioObject);
		void removeTuioObject(TuioObject tuioObject);

		void addTuioCursor(TuioCursor tuioCursor);
		void updateTuioCursor(TuioCursor tuioCursor);
		void removeTuioCursor(TuioCursor tuioCursor);

        void addTuioBlob(TuioBlob tuioBlob);
        void updateTuioBlob(TuioBlob tuioBlob);
        void removeTuioBlob(TuioBlob tuioBlob);

		void refresh(long timestamp);
	}
}
