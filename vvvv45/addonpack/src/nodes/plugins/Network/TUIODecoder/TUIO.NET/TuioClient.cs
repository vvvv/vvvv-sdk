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
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace TUIO.NET
{
	public class TuioClient 
	{
		private Dictionary<long,TuioObject> objectList = new Dictionary<long,TuioObject>(32);
		private List<long> aliveObjectList = new List<long>(32);
		private List<long> newObjectList = new List<long>(32);
		private Dictionary<long,TuioCursor> cursorList = new Dictionary<long,TuioCursor>(32);
		private List<long> aliveCursorList = new List<long>(32);
		private List<long> newCursorList = new List<long>(32);
		
		private List<TuioCursor> freeCursorList = new List<TuioCursor>();
		private int maxFingerID = -1;

		private int currentFrame = 0;
		private int lastFrame = 0;
		private DateTime startTime;
		private long lastTime;
		
		private readonly int UNDEFINED = -1;
		
		private List<TuioListener> listenerList = new List<TuioListener>();
		
		public TuioClient() {
            startTime = DateTime.Now;
        }

		public void ProcessMessage(OSC.NET.OSCMessage message) {
            string address = message.Address;
            ArrayList args = message.Values;
			string command = (string)args[0];

			if (address == "/tuio/2Dobj") {
				if ((command == "set") && (currentFrame>=lastFrame)) {
					long s_id  = (int)args[1];
					int f_id  = (int)args[2];
					float x = (float)args[3];
					float y = (float)args[4];
					float a = (float)args[5];
					float X = (float)args[6];
					float Y = (float)args[7];
					float A = (float)args[8];
					float m = (float)args[9];
					float r = (float)args[10];

					
					if (!objectList.ContainsKey(s_id)) {
						TuioObject addObject  = new TuioObject(s_id,f_id,x,y,a);		
						objectList.Add(s_id, addObject);

						for (int i=0;i<listenerList.Count;i++) {
							TuioListener listener = (TuioListener)listenerList[i];
							if (listener!=null) listener.addTuioObject(addObject);
						}
					} else {
						TuioObject updateObject = objectList[s_id];

						if((updateObject.getX()!=x) || (updateObject.getY()!=y) || (updateObject.getAngle()!=a)) {
							updateObject.update(x,y,a,X,Y,A,m,r);
							for (int i=0;i<listenerList.Count;i++) {
								TuioListener listener = (TuioListener)listenerList[i];
								if (listener!=null) listener.updateTuioObject(updateObject);
							}
							//objectList[s_id] = tobj;
						}
					}
							
				} else if ((command == "alive") && (currentFrame>=lastFrame)) {
		
					for (int i=1;i<args.Count;i++) {
						// get the message content
						long s_id = (int)args[i];
						newObjectList.Add(s_id);
						// reduce the object list to the lost objects
						if (aliveObjectList.Contains(s_id))
							 aliveObjectList.Remove(s_id);
					}
					
					// remove the remaining objects
					for (int i=0;i<aliveObjectList.Count;i++) {
						long s_id = aliveObjectList[i];
						TuioObject removeObject = objectList[s_id];
						removeObject.remove();
						objectList.Remove(s_id);
						

						for (int j=0;j<listenerList.Count;j++) {
							TuioListener listener = (TuioListener)listenerList[j];
							if (listener!=null) listener.removeTuioObject(removeObject);
						}
					}

					List<long> buffer = aliveObjectList;
					aliveObjectList = newObjectList;
					
					// recycling of the List
					newObjectList = buffer;
					newObjectList.Clear();
						
				} else if (command=="fseq") {
					lastFrame = currentFrame;
					currentFrame = (int)args[1];
					if(currentFrame==-1) currentFrame=lastFrame;
										
					if (currentFrame>=lastFrame) {
						
						long currentTime = lastTime;
						if (currentFrame>lastFrame) {

							TimeSpan span = DateTime.Now - startTime;
							currentTime = span.Milliseconds;
							lastTime = currentTime;
						}


							
						IEnumerator<TuioObject> refreshList = objectList.Values.GetEnumerator();

						while(refreshList.MoveNext()) {

							TuioObject refreshObject = refreshList.Current;
							if (refreshObject.getUpdateTime()==UNDEFINED) refreshObject.setUpdateTime(currentTime);
						}

							
						for (int i=0;i<listenerList.Count;i++) {
							TuioListener listener = (TuioListener)listenerList[i];						
							if (listener!=null) listener.refresh(currentTime);
						}
					}
				}

			} else if (address == "/tuio/2Dcur") {

				if ((command == "set") && (currentFrame>=lastFrame)) {
					long s_id  = (int)args[1];
					float x = (float)args[2];
                    TUIODecoder.TUIODecoder.instance.FHost.Log(VVVV.PluginInterfaces.V1.TLogType.Debug, x.ToString() + " - " + message.BinaryData.Length);
					float y = (float)args[3];
					float X = (float)args[4];
					float Y = (float)args[5];
					float m = (float)args[6];
					
					if (!cursorList.ContainsKey(s_id)) {
						
						int f_id = cursorList.Count;
						if (cursorList.Count<=maxFingerID) {
							TuioCursor closestCursor = freeCursorList[0];
							IEnumerator<TuioCursor> testList = freeCursorList.GetEnumerator();
							while(testList.MoveNext()) {
								TuioCursor testCursor = testList.Current;
								if (testCursor.getDistance(x,y)<closestCursor.getDistance(x,y)) closestCursor = testCursor;
							}
							f_id = closestCursor.getFingerID();
							freeCursorList.Remove(closestCursor);
						} else maxFingerID = f_id;		
			
						TuioCursor addCursor  = new TuioCursor(s_id,f_id,x,y);		
						cursorList.Add(s_id, addCursor);

						for (int i=0;i<listenerList.Count;i++) {
							TuioListener listener = (TuioListener)listenerList[i];
							if (listener!=null) listener.addTuioCursor(addCursor);
						}
					} else {
						TuioCursor updateCursor = (TuioCursor)cursorList[s_id];
						if((updateCursor.getX()!=x) || (updateCursor.getY()!=y)) {	
							updateCursor.update(x,y,X,Y,m);
							for (int i=0;i<listenerList.Count;i++) {
								TuioListener listener = (TuioListener)listenerList[i];
								if (listener!=null) listener.updateTuioCursor(updateCursor);
							}


							//cursorList[s_id] = tcur;
						}
					}
					
				} else if ((command == "alive") && (currentFrame>=lastFrame)) {
		
					for (int i=1;i<args.Count;i++) {
						// get the message content
						long s_id = (int)args[i];
						newCursorList.Add(s_id);
						// reduce the cursor list to the lost cursors
						if (aliveCursorList.Contains(s_id)) 
							aliveCursorList.Remove(s_id);
					}
					
					// remove the remaining cursors
					for (int i=0;i<aliveCursorList.Count;i++) {
						long s_id = aliveCursorList[i];
						if (!cursorList.ContainsKey(s_id)) continue;
						TuioCursor removeCursor = cursorList[s_id];
                        int c_id = removeCursor.getFingerID();
						cursorList.Remove(s_id);

                        if (c_id == maxFingerID)
                        {
                            maxFingerID = -1;


                            if (cursorList.Count > 0)
                            {

                                IEnumerator<KeyValuePair<long, TuioCursor>> clist = cursorList.GetEnumerator();
                                while (clist.MoveNext())
                                {
                                    int f_id = clist.Current.Value.getFingerID();
                                    if (f_id > maxFingerID) maxFingerID = f_id;
                                }
								
							   List<TuioCursor> freeCursorBuffer = new List<TuioCursor>();
							   IEnumerator<TuioCursor> flist = freeCursorList.GetEnumerator();
                                while (flist.MoveNext())
                                {
								   TuioCursor testCursor = flist.Current;

                                    if (testCursor.getFingerID() < maxFingerID) freeCursorBuffer.Add(testCursor);
                                }
								freeCursorList = freeCursorBuffer;
                            }
                        } else  {
							removeCursor.remove();
							freeCursorList.Add(removeCursor);
						}
						

						for (int j=0;j<listenerList.Count;j++) {
							TuioListener listener = (TuioListener)listenerList[j];
							if (listener!=null) listener.removeTuioCursor(removeCursor);
						}
					}
					
					List<long> buffer = aliveCursorList;
					aliveCursorList = newCursorList;
					
					// recycling of the List
					newCursorList = buffer;
					newCursorList.Clear();
				} else if (command=="fseq") {
					lastFrame = currentFrame;
					currentFrame = (int)args[1];
					if(currentFrame==-1) currentFrame=lastFrame;
										
					if (currentFrame>=lastFrame) {

						
						long currentTime = lastTime;
						if (currentFrame>lastFrame) {

							TimeSpan span = DateTime.Now - startTime;
							currentTime = span.Milliseconds;
							lastTime = currentTime;
						}			
						
						IEnumerator<TuioCursor> refreshList = cursorList.Values.GetEnumerator();

						while(refreshList.MoveNext()) {

							TuioCursor refreshCursor = refreshList.Current;
							if (refreshCursor.getUpdateTime()==UNDEFINED) refreshCursor.setUpdateTime(currentTime);
						}						
						
						for (int i=0;i<listenerList.Count;i++) {
							TuioListener listener = (TuioListener)listenerList[i];
							if (listener!=null) listener.refresh(currentTime);
						}
					}
				}

			}
		}
		
		public void addTuioListener(TuioListener listener) {
			listenerList.Add(listener);
		}
		
		public void removeTuioListener(TuioListener listener) {	
			listenerList.Remove(listener);
		}
		

		public List<TuioObject> getTuioObjects() {
			return new List<TuioObject>(objectList.Values);
		}
	
		public List<TuioCursor> getTuioCursors() {
			return new List<TuioCursor>(cursorList.Values);
		}	

	
		public TuioObject getTuioObject(long s_id) {
			TuioObject tobject = null;
			objectList.TryGetValue(s_id,out tobject);
			return tobject;
		}
		
		public TuioCursor getTuioCursor(long s_id) {
			TuioCursor tcursor = null;
			cursorList.TryGetValue(s_id, out tcursor);
			return tcursor;
		}		 

	}
}
