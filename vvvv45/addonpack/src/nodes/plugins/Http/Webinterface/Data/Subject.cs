using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Diagnostics;

using VVVV.Webinterface.HttpServer;

namespace VVVV.Webinterface.Data
{

    /// <summary>
    /// abstract class as base for the Subject classes 
    /// </summary>
    abstract class Subject
    {
        private ArrayList mNodeObservers = new ArrayList();
        private ArrayList mServerhandlingObserver = new ArrayList();


        /// <summary>
        /// ArrayList of existing observers
        /// </summary>
        public ArrayList Observer
        {
            get
            {
                return mNodeObservers;
            }
        }

        /// <summary>
        /// ArrayList of existing Server
        /// </summary>
        public ArrayList ObserverServerhandling
        {
            get
            {
                return mServerhandlingObserver;
            }
        }

        /// <summary>
        /// attaches an NodeObservr to the ArrayList mNodeObservers
        /// </summary>
        /// <param name="pObserver">NodeObserer instance</param>
        public void AttachNode(NodeObserver pObserver)
        {
            mNodeObservers.Add(pObserver);
            Debug.WriteLine("Node Observer: " + pObserver.ID + " is attached / Anzahl Node Observer: " + mNodeObservers.Count.ToString() ); 
        }


        /// <summary>
        /// deletes an NodeObserver from the ArrayList mNodeObservers
        /// </summary>
        /// <param name="pObserver">NodeObserver instance</param>
        public void DetachNode(NodeObserver pObserver)
        {
            mNodeObservers.Remove(pObserver);
            Debug.WriteLine("Node Observer: " + pObserver.ID + " is detached / Anzahl Node Observer: " + mNodeObservers.Count.ToString()); 
        }

        /// <summary>
        /// calles each observer method updated() from the ArrayList mNodeObservers
        /// </summary>
        public void NotifyNode()
        {

            foreach (NodeObserver o in mNodeObservers)
            {
                o.Updated();
            }
        }


        /// <summary>
        /// attach an Server instance to the ArrayList mServerhandlingObserver
        /// </summary>
        /// <param name="pServer">Server instance</param>
        public void AttachServerhandling(Server pServer)
        {
            mServerhandlingObserver.Add(pServer);
            Debug.WriteLine("Serverhandling Observer: " + pServer.Name + " is attached / Anzahl Serverhandling Observer: " + mServerhandlingObserver.Count.ToString()); 
        }

        /// <summary>
        /// detach an Server instance from the ArrayList mServerhandlingObserver
        /// </summary>
        /// <param name="pServer"></param>
        public void DetachServhandling(Server pServer)
        {
            mServerhandlingObserver.Remove(pServer);
            Debug.WriteLine("Serverhandling Observer: " + pServer.Name + " is detached / Anzahl Serverhandling Observer: " + mServerhandlingObserver.Count.ToString()); 
        }

        /// <summary>
        /// calles the updated() method of each server instance in the ArrayList mServerhandlingObserver
        /// </summary>
        public void NotifyServer(string pData)
        {
            foreach (Server o in mServerhandlingObserver)
            {
                o.UpdatedBrowser(pData);
            }                
        }

        /// <summary>
        /// calles the reload() method of each server instance in the ArrayList mServerhandlingObserver
        /// </summary>
        public void Reload()
        {
            foreach (Server o in mServerhandlingObserver)
            {
                o.Reload();
            }   
        }
    }


    /// <summary>
    /// class definition of the ConcretSubject class
    /// has some more properties than the subject class
    /// </summary>
    class ConcreteSubject : Subject
    {
        private string mSubjectState;
        private string mToNode;
        private string mToHtmlForm;
        private SortedList<string, string> mNewServerDaten;


        /// <summary>
        /// State of the Subject
        /// </summary>
        public string SubjectState
        {
            get
            {
                return mSubjectState;
            }
            set
            {
                mSubjectState = value;
            }
        }


        /// <summary>
        /// to which node the new data should be send
        /// should be extended to List
        /// </summary>
        public string ToNode
        {
            get
            {
                return mToNode;
            }
            set
            {
                mToNode = value;
            }
        }


        /// <summary>
        /// to which HTML form the new data should be send
        /// should be extended to List
        /// </summary>
        public string ToHtmlForm
        {
            get
            {
                return mToHtmlForm;
            }
            set
            {
                mToHtmlForm = value; 
            }
        }


        /// <summary>
        /// List of new server data
        /// </summary>
        public SortedList<string, string> NewServerDaten
        {
            get
            {
                return mNewServerDaten;
            }
            set
            {
                mNewServerDaten = value;
            }
        }


    }
   
}
