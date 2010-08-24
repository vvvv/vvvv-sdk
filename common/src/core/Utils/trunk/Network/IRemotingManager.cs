using System;

namespace VVVV.Utils.Network
{
    //abstraction for remoting object publishing
    public interface IRemotingPublisher
    {
        void PublishObject(MarshalByRefObject instance, string publicName);
        void PublishSingleton<T>(string publicName) where T : MarshalByRefObject;
        void PublishSingleCall<T>(string publicName) where T : MarshalByRefObject;
    }

    //manager for remoting over the LAN or internet using TCP or HTTP
    public interface IRemotingManager : IRemotingPublisher
    {
        void InitializeChannel(string name, int port, bool enableSecurity);
        void InitializeServerChannel(string name, int port, bool enableSecurity);
        void InitializeClientChannel(string name, bool enableSecurity);

        T GetRemoteObject<T>(string publicName, string server, int port);
    }

    //manager for remoting on the same physical machine using IPC
    public interface IRemotingManagerIPC : IRemotingPublisher
    {
        void InitializeIpcChannel(string name, string portName, bool enableSecurity);
        void InitializeIpcServerChannel(string name, string portName, bool enableSecurity);
        void InitializeIpcClientChannel(string name, bool enableSecurity);

        T GetRemoteObjectIpc<T>(string publicName, string portName);
    }
}
