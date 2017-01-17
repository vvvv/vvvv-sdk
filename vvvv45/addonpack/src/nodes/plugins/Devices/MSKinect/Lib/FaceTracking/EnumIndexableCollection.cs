// -----------------------------------------------------------------------
// <copyright file="EnumIndexableCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.FaceTracking
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Globalization;

    public class EnumIndexableCollection<TIndex, TValue> : IEnumerable<TValue>
    {
        private readonly TValue[] valueArray;

        internal EnumIndexableCollection(TValue[] valueArray)
        {
            this.valueArray = valueArray;
        }

        public int Count
        {
            get
            {
                if (valueArray == null)
                {
                    return 0;
                }

                return valueArray.Count();
            }
        }

        public TValue this[int index]
        {
            get
            {
                if (valueArray == null)
                {
                    throw new InvalidOperationException();
                }

                return valueArray[index];
            }
        }

        public TValue this[TIndex index]
        {
            get
            {
                if (valueArray == null)
                {
                    throw new InvalidOperationException();
                }

                var intIndex = (int)System.Convert.ChangeType(index, typeof(int), CultureInfo.InvariantCulture);
                return valueArray[intIndex];
            }
        }

        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            if (valueArray == null)
            {
                return Enumerable.Empty<TValue>().GetEnumerator();
            }

            return valueArray.AsEnumerable<TValue>().GetEnumerator();
        }
    }
}
