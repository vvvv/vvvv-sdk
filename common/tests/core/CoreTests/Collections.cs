using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using NUnit.Framework;
using VVVV.Core;
using VVVV.Core.Collections;
using VVVV.Core.Collections.Sync;

namespace CoreTests
{
    [TestFixture]
    public class Collections
    {
        public IEditableList<int> Numbers
        {
            get;
            private set;
        }

        private EditableList<int> FResults;
        public IViewableList<int> Results
        {
            get;
            private set;
        }

        [Test]
        public void TestMethod1()
        {
            Numbers = new EditableList<int>();
            
            FResults = new EditableList<int>();
            using (FResults.SyncWith(Numbers, x => x * x))
            {
                Results = FResults.AsViewableList();
                
                // when doing more than one change: 
                // use beginupdate/endupdate to reduce syncing events
                Numbers.BeginUpdate();
                try
                {
                    Numbers.Add(4);
                    Numbers.Add(7);
                }
                finally
                {
                    Numbers.EndUpdate();
                }

                // Magically results (x*x) are already added to the Results list
                foreach (var r in Results)
                    Debug.WriteLine(r);

                // You can't add a result to the public Results list
                // Results.Add(17);

                // again: change source collection:
                Numbers.Add(8);

                // synced results collection is already updated.
                foreach (var r in Results)
                    Debug.WriteLine(r);
            }
        }
    }
}