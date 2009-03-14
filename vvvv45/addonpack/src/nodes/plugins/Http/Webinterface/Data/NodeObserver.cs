
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace VVVV.Webinterface.Data
{
    
    /// <summary>
    /// abstract Observer class defintion 
    /// </summary>
    abstract class Observer
    {
        /// <summary>
        /// abstract method
        /// </summary>
        public abstract void Updated();


        /// <summary>
        /// abstract method
        /// </summary>
        public abstract void Reload();

        public abstract void UpdatedBrowser(string pData);

    }


    /// <summary>
    /// NodeObserver class definition 
    /// inherit from the class Observer the abstract functions updated() and reload()
    /// </summary>
    class NodeObserver : Observer
    {
        private string mName;
        private string mObserverState;
        private string mToNode;
        private ConcreteSubject mSubject;


        /// <summary>
        /// the aktualle subject instance 
        /// </summary>
        public ConcreteSubject Subject
        {
            get
            {
                return mSubject;
            }
            set
            {
                mSubject = value;
            }
        }


        /// <summary>
        /// obsever ID = node ID
        /// </summary>
        public string ID
        {
            get
            {
                return mName;
            }
        }

        /// <summary>
        /// State of the Observer 
        /// </summary>
        public string ObserverState
        {
            get
            {
                return mObserverState;
            }
        }

        /// <summary>
        /// The Node that recieves the new data
        /// should be expand to a List which is filled up each frame
        /// </summary>
        public string ToNode
        {
            get
            {
                return mToNode;
            }
        }
        
        /// <summary>
        /// NodeObserver constructor. 
        /// </summary>
        /// <param name="pSubject">the subject which works together which updates the observer</param>
        /// <param name="pName">name of the observer = Node ID</param>
        public NodeObserver(ConcreteSubject pSubject, string pName)
        {
            this.mSubject = pSubject;
            this.mName = pName;

        }

        /// <summary>
        /// is called by the Concret Subject Class method NotifyServer()
        /// </summary>
        public override void Updated()
        {
            Debug.WriteLine("Updated " + mName + "; mSubject.ToNode =  " + mSubject.ToNode);
            mObserverState = mSubject.SubjectState;
            mToNode = mSubject.ToNode;
        }

        /// <summary>
        /// is called from the subject class method reload()
        /// </summary>
        public override void Reload()
        {
            Debug.WriteLine("Reload NodeObserver");
        }

        public override void UpdatedBrowser(string pData)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }

    

}
