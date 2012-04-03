using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using VVVV.Core.Viewer.GraphicalEditor;

namespace VVVV.Core.View.GraphicalEditor
{
    public enum ConnectableType
    {
        StartingPoint,
        EndPoint,
        CanBeBoth,
    }
    
    public interface IConnectable
    {
        bool AwaitingConnection
        {
            set;
        }

        bool IsConnectionCandidate
        {
            set;
        }

        ConnectableType ConnectableType
        {
            get;
        }

        bool CanConnectTo(IConnectable aConnectable);

        void ConnectTo(IConnectable aConnectable, IPathHost apathhost);

        void DecorateStartingPath(ITempPath aPath);

        bool CommitAsConnectionCandidate(PointF mousepos, IConnectable bestcandidatesofar);

        void DisconnectFrom(IConnectable iConnectable, IPathHost iPathHost);

        //bool Connected
        //{
        //    get;
        //}
    }
}
