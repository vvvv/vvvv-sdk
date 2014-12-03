/* Microsoft Public License (MS-PL)

 * 2013, Paul Varcholik
 * http://bespokesoftware.org/
 
This license governs use of the accompanying software. If you use the software, you
accept this license. If you do not accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the
same meaning here as under U.S. copyright law.
A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.

 */

using System;
using System.Collections.Generic;

namespace VVVV.Utils.OSC
{
    public class OscTimeTag
    {
        /// <summary>
        /// Osc Time Epoch (January 1, 1900 00:00:00).
        /// </summary>
        public static readonly DateTime Epoch = new DateTime(1900, 1, 1, 0, 0, 0, 0);

        /// <summary>
        /// Minimum Osc Time Tag.
        /// </summary>
        public static readonly OscTimeTag MinValue = new OscTimeTag(Epoch + TimeSpan.FromMilliseconds(1.0));

        /// <summary>
        /// Gets the first 32 bits of the Osc Time Tag. Specifies the number of seconds since the epoch.
        /// </summary>
        public uint SecondsSinceEpoch
        {
            get
            {
                return (uint)(mTimeStamp - Epoch).TotalSeconds;
            }
        }

        /// <summary>
        /// Gets the last 32 bits of the Osc Time Tag. Specifies the fractional part of a second.
        /// </summary>
        public uint FractionalSecond
        {
            get
            {
                return (uint)((mTimeStamp - Epoch).Milliseconds);
            }
        }

        /// <summary>
        /// Gets the Osc Time Tag as a DateTime value.
        /// </summary>
        public DateTime DateTime
        {
            get
            {
                return mTimeStamp;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OscTimeTag"/> class.
        /// </summary>
        /// <remarks>Defaults the Osc Time Tag value to DateTime.Now.</remarks>
        public OscTimeTag()
            : this(DateTime.Now)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OscTimeTag"/> class.
        /// </summary>
        /// <param name="timeStamp">The time stamp to use to set the Osc Time Tag.</param>
        public OscTimeTag(DateTime timeStamp)
        {
            Set(timeStamp);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OscTimeTag"/> class.
        /// </summary>
        /// <param name="data">The time stamp to use to set the Osc Time Tag.</param>
        public OscTimeTag(byte[] data)
        {
            byte[] secondsSinceEpochData = new byte[4];
            Array.Copy(data, 0, secondsSinceEpochData, 0, 4);

            byte[] fractionalSecondData = new byte[4];
            Array.Copy(data, 4, fractionalSecondData, 0, 4);

            if (BitConverter.IsLittleEndian) 
            {
                secondsSinceEpochData = OSCPacket.swapEndian(secondsSinceEpochData);
                fractionalSecondData = OSCPacket.swapEndian(fractionalSecondData);
            }

            uint secondsSinceEpoch = BitConverter.ToUInt32(secondsSinceEpochData, 0);
            uint fractionalSecond = BitConverter.ToUInt32(fractionalSecondData, 0);

            DateTime timeStamp = Epoch.AddSeconds(secondsSinceEpoch).AddMilliseconds(fractionalSecond);
            if (!IsValidTime(timeStamp)) throw new Exception("Not a valid OSC Timetag discovered.");
            mTimeStamp = timeStamp;
        }

        /// <summary>
        /// Convert the Osc Time Tag to a byte array.
        /// </summary>
        /// <returns>A byte array containing the Osc Time Tag.</returns>
        public byte[] ToByteArray()
        {
            List<byte> timeStamp = new List<byte>();

            byte[] secondsSinceEpoch = BitConverter.GetBytes(SecondsSinceEpoch);
            byte[] fractionalSecond = BitConverter.GetBytes(FractionalSecond);

            if (BitConverter.IsLittleEndian) // != OscPacket.LittleEndianByteOrder)
            {
                secondsSinceEpoch = OSCPacket.swapEndian(secondsSinceEpoch);
                fractionalSecond = OSCPacket.swapEndian(fractionalSecond);
            }

            timeStamp.AddRange(secondsSinceEpoch);
            timeStamp.AddRange(fractionalSecond);

            return timeStamp.ToArray();
        }

        /// <summary>
        /// Determines whether two specified instances of OscTimeTag are equal.
        /// </summary>
        /// <param name="lhs">An OscTimeTag.</param>
        /// <param name="rhs">An OscTimeTag.</param>
        /// <returns>true if lhs and rhs represent the same time tag; otherwise, false.</returns>
        public static bool Equals(OscTimeTag lhs, OscTimeTag rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Determines whether two specified instances of OscTimeTag are equal.
        /// </summary>
        /// <param name="lhs">An OscTimeTag.</param>
        /// <param name="rhs">An OscTimeTag.</param>
        /// <returns>true if lhs and rhs represent the same time tag; otherwise, false.</returns>
        public static bool operator ==(OscTimeTag lhs, OscTimeTag rhs)
        {
            if (ReferenceEquals(lhs, rhs))
            {
                return true;
            }

            if (((object)lhs == null) || ((object)rhs == null))
            {
                return false;
            }

            return lhs.DateTime == rhs.DateTime;
        }

        /// <summary>
        /// Determines whether two specified instances of OscTimeTag are not equal.
        /// </summary>
        /// <param name="lhs">An OscTimeTag.</param>
        /// <param name="rhs">An OscTimeTag.</param>
        /// <returns>true if lhs and rhs do not represent the same time tag; otherwise, false.</returns>
        public static bool operator !=(OscTimeTag lhs, OscTimeTag rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Determines whether one specified <see cref="OscTimeTag"/> is less than another specified <see cref="OscTimeTag"/>.
        /// </summary>
        /// <param name="lhs">An OscTimeTag.</param>
        /// <param name="rhs">An OscTimeTag.</param>
        /// <returns>true if lhs is less than rhs; otherwise, false.</returns>        
        public static bool operator <(OscTimeTag lhs, OscTimeTag rhs)
        {
            return lhs.DateTime < rhs.DateTime;
        }

        /// <summary>
        /// Determines whether one specified <see cref="OscTimeTag"/> is less than or equal to another specified <see cref="OscTimeTag"/>.
        /// </summary>
        /// <param name="lhs">An OscTimeTag.</param>
        /// <param name="rhs">An OscTimeTag.</param>
        /// <returns>true if lhs is less than or equal to rhs; otherwise, false.</returns>        
        public static bool operator <=(OscTimeTag lhs, OscTimeTag rhs)
        {
            return lhs.DateTime <= rhs.DateTime;
        }

        /// <summary>
        /// Determines whether one specified <see cref="OscTimeTag"/> is greater than another specified <see cref="OscTimeTag"/>.
        /// </summary>
        /// <param name="lhs">An OscTimeTag.</param>
        /// <param name="rhs">An OscTimeTag.</param>
        /// <returns>true if lhs is greater than rhs; otherwise, false.</returns>        
        public static bool operator >(OscTimeTag lhs, OscTimeTag rhs)
        {
            return lhs.DateTime > rhs.DateTime;
        }

        /// <summary>
        /// Determines whether one specified <see cref="OscTimeTag"/> is greater than or equal to another specified <see cref="OscTimeTag"/>.
        /// </summary>
        /// <param name="lhs">An OscTimeTag.</param>
        /// <param name="rhs">An OscTimeTag.</param>
        /// <returns>true if lhs is greater than or equal to rhs; otherwise, false.</returns>        
        public static bool operator >=(OscTimeTag lhs, OscTimeTag rhs)
        {
            return lhs.DateTime >= rhs.DateTime;
        }

        /// <summary>
        /// Validates the time stamp for use in an Osc Time Tag.
        /// </summary>
        /// <param name="timeStamp">The time stamp to validate.</param>
        /// <returns>True if the time stamp is a valid Osc Time Tag; false, otherwise.</returns>
        /// <remarks>Time stamps must be greater-than-or-equal to <see cref="OscTimeTag.MinValue"/>.</remarks>
        public static bool IsValidTime(DateTime timeStamp)
        {
            return (timeStamp >= Epoch + TimeSpan.FromMilliseconds(1.0));
        }

        /// <summary>
        /// Sets the value of the Osc Time Tag.
        /// </summary>
        /// <param name="timeStamp">The time stamp to use to set the Osc Time Tag.</param>
        public void Set(DateTime timeStamp)
        {
            timeStamp = new DateTime(timeStamp.Ticks - (timeStamp.Ticks % TimeSpan.TicksPerMillisecond), timeStamp.Kind);

            if(!IsValidTime(timeStamp)) throw new Exception("Not a valid OSC Timetag.");
            mTimeStamp = timeStamp;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="value">An object to compare to this instance.</param>
        /// <returns>true if value is an instance of System.DateTime and equals the value of this instance; otherwise, false.</returns>
        public override bool Equals(object value)
        {
            if (value == null)
            {
                return false;
            }

            OscTimeTag rhs = value as OscTimeTag;
            if (rhs == null)
            {
                return false;
            }

            return mTimeStamp.Equals(rhs.mTimeStamp);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified OscTimeTag instance.
        /// </summary>
        /// <param name="value">An object to compare to this instance.</param>
        /// <returns>true if value is an instance of System.DateTime and equals the value of this instance; otherwise, false.</returns>
        public bool Equals(OscTimeTag value)
        {
            if ((object)value == null)
            {
                return false;
            }

            return mTimeStamp.Equals(value.mTimeStamp);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return mTimeStamp.GetHashCode();
        }

        /// <summary>
        /// Converts the value of the current <see cref="OscTimeTag"/> object to its equivalent string representation.
        /// </summary>
        /// <returns>A string representation of the value of the current <see cref="OscTimeTag"/> object.</returns>
        public override string ToString()
        {
            return mTimeStamp.ToString();
        }

        private DateTime mTimeStamp;
    }

}
