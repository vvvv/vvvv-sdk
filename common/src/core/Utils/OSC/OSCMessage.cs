#region licence/info
// OSC.NET - Open Sound Control for .NET
// http://luvtechno.net/
//
// Copyright (c) 2006, Yoshinori Kawasaki 
// All rights reserved.
//
// Changes and improvements:
// Copyright (c) 2005-2008 Martin Kaltenbrunner <mkalten@iua.upf.edu>
// As included with    
// http://reactivision.sourceforge.net/
//
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
// * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
// * Neither the name of "luvtechno.net" nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS 
// OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY 
// AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR 
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY 
// WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion licence/info

using System;
using System.Collections;
using System.IO;
using System.Text;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Utils.OSC
{
	/// <summary>
	/// OSCMessage
	/// 
	/// Contains an address, a comma followed by one or more type identifiers. then the data itself follows in binary encoding.
	/// </summary>
	public class OSCMessage : OSCPacket
	{
//      These Attributes adhere to the OSC Specs 1.0
        protected const char INTEGER = 'i'; // int32 8byte
		protected const char FLOAT	  = 'f'; //float32 8byte
		protected const char LONG	  = 'h';  //int64 16byte
		protected const char DOUBLE  = 'd'; // float64 16byte
		protected const char STRING  = 's'; // padded by zeros
		protected const char SYMBOL  = 'S'; // same as STRING really
        protected const char BLOB	  = 'b'; // bytestream, starts with an int that tells the total length of th stream
        protected const char TIMETAG = 't'; // fixed point floating number with 32bytes (16bytes for totaldays after 1.1.1900 and 16bytes for fractionOfDay)
        protected const char CHAR	  = 'c'; // bit
        protected const char COLOR  = 'r'; // 4x8bit -> rgba

        //protected const char TRUE	  = 'T';
        //protected const char FALSE = 'F';
        protected const char NIL = 'N';
        //protected const char INFINITUM = 'I';

        //protected const char ALL     = '*';

//      These Attributes are added for convenience within vvvv. They are NOT part of the OSC Specs, but are VERY useful if you want to make vvvv talk to another instance of vvvv
//      Using them requires to set the ExtendedVVVVMethod property to true (with the constructor or with the Unpack methods, depending if you want to send or receive)
        protected const char VECTOR2D = 'v'; // synonym to dd
        protected const char VECTOR3D = 'V'; // synonym to ddd
        protected const char QUATERNION = 'q'; // synonym to dddd
        protected const char MATRIX4 = 'M';  // for 4x4 Matrices with float, so synonym to ffffffffffffffff


		public OSCMessage(string address, bool extendedMode = false) : base(extendedMode)
		{
            this.typeTag = ",";
			this.Address = address;
		}
		public OSCMessage(string address, object value, bool extendedMode =  false) : base(extendedMode)
		{
            this.typeTag = ",";
			this.Address = address;
			Append(value);
		}

		override protected void pack()
		{
			ArrayList data = new ArrayList();

			addBytes(data, packString(this.address));
			padNull(data);
			addBytes(data, packString(this.typeTag));
			padNull(data);
			
			foreach(object value in this.Values)
			{
				if(value is int) addBytes(data, packInt((int)value));
				else if(value is long) addBytes(data, packLong((long)value));
				else if(value is float) addBytes(data, packFloat((float)value));
				else if(value is double) addBytes(data, packDouble((double)value));
				else if(value is string) {
					addBytes(data, packString((string)value));
					padNull(data);
				}
                else if (value is Stream) {
                    addBytes(data, packBlob((Stream)value));
                    padNull(data);
                }
                else if (value is RGBAColor) addBytes(data, packColor((RGBAColor)value));
                else if (value is char) addBytes(data, packChar((char)value));
                else if (value is DateTime)
                {
                    addBytes(data, packTimeTag((DateTime)value));
                }
                else if (ExtendedVVVVMode)
				{
                    if (value is Vector2D ) addBytes(data, packVector2D((Vector2D)value));
                    else if (value is Vector3D ) addBytes(data, packVector3D((Vector3D)value));
                    else if (value is Vector4D) addBytes(data, packVector4D((Vector4D)value));
                    else if (value is Matrix4x4) addBytes(data, packMatrix((Matrix4x4)value));
				}
			}
			
			this.binaryData = (byte[])data.ToArray(typeof(byte));
		}


		public static OSCMessage Unpack(byte[] bytes, ref int start, bool extendedMode = false)
		{
			string address = unpackString(bytes, ref start);
			//Console.WriteLine("address: " + address);
			OSCMessage msg = new OSCMessage(address, extendedMode);

			char[] tags = unpackString(bytes, ref start).ToCharArray();
			//Console.WriteLine("tags: " + new string(tags));
			foreach(char tag in tags)
			{
				//Console.WriteLine("tag: " + tag + " @ "+start);
				if(tag == ',') continue;
				else if(tag == INTEGER) msg.Append(unpackInt(bytes, ref start));
				else if(tag == LONG) msg.Append(unpackLong(bytes, ref start));
				else if(tag == DOUBLE) msg.Append(unpackDouble(bytes, ref start));
				else if(tag == FLOAT) msg.Append(unpackFloat(bytes, ref start));
                else if (tag == STRING || tag == SYMBOL) msg.Append(unpackString(bytes, ref start));
                
                else if (tag == CHAR) msg.Append(unpackChar(bytes, ref start));
                else if (tag == BLOB) msg.Append(unpackBlob(bytes, ref start));
                else if (tag == COLOR) msg.Append(unpackColor(bytes, ref start));
                else if (tag == TIMETAG) msg.Append(unpackTimeTag(bytes, ref start));

//              here come the custom vvvv datatypes
                else if (extendedMode)
                {
                    if (tag == VECTOR2D) msg.Append(unpackVector2D(bytes, ref start));
                    else if (tag == VECTOR3D) msg.Append(unpackVector3D(bytes, ref start));
                    else if (tag == QUATERNION) msg.Append(unpackVector4D(bytes, ref start));
                    else if (tag == MATRIX4) msg.Append(unpackMatrix(bytes, ref start));
                } else Console.WriteLine("unknown tag: " + tag);
			}
			return msg;
		}

		override public void Append(object value)
		{
			if(value is int)
			{
				AppendTag(INTEGER);
			}
			else if(value is long)
			{
				AppendTag(LONG);
			}
			else if(value is float)
			{
				AppendTag(FLOAT);
			}
			else if(value is double)
			{
				AppendTag(DOUBLE);
			}
			else if(value is string)
			{
				AppendTag(STRING);
			}
            else if (value is char)
            {
                AppendTag(CHAR);
            }
            else if (value is Stream)
            {
                AppendTag(BLOB);
            }
            else if (value is DateTime)
            {
                AppendTag(TIMETAG);
            }
            else if (value is RGBAColor)
            {
                AppendTag(COLOR);
            }
            else if (ExtendedVVVVMode)
            {

                if (value is Vector2D)
                {
                    AppendTag(VECTOR2D);
                }
                else if (value is Vector3D)
                {
                    AppendTag(VECTOR3D);
                }
                else if (value is Vector4D)
                {
                    AppendTag(QUATERNION);
                }
                else if (value is Matrix4x4)
                {
                    AppendTag(MATRIX4);
                }
                else
                {
                    Fallback();
                    return;
                }
            }
            else
            {
                Fallback();
                return;
            }
			values.Add(value);
		}

	    private void Fallback()
	    {
	        AppendTag(NIL);
//	        values.Add("undefined");
	    }

	    protected string typeTag;
		protected void AppendTag(char type)
		{
			typeTag += type;
		}

		override public bool IsBundle() { return false; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.Address + " ");
            for(int i = 0; i < values.Count; i++)
                sb.Append(values[i].ToString() + " ");
            return sb.ToString();
        }
	}
}
