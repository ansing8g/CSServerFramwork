
namespace ServerModule.Network
{
    public enum SocketErrorType
    {
        Accept,
        Connect,
        Disconnect,
        Send,
        Receive,
    }

    public interface ServerSocketEvent
    {
        void OnError(SocketErrorType _error_type, System.Exception _exception, SessionSocket _sessionsocket);

        void OnAccept(SessionSocket _sessionsocket);
        void OnDisconnect(SessionSocket _sessionsocket);
        void OnSend(SessionSocket _sessionsocket);
        void OnReceive(SessionSocket _sessionsocket, byte[] _data);
    }

    public interface ClientSocketEvent
    {
        void OnError(SocketErrorType _error_type, System.Exception _exception, ConnectSocket _connectsocket);

        void OnConnect(ConnectSocket _connectsocket);
        void OnDisconnect(ConnectSocket _connectsocket);
        void OnSend(ConnectSocket _connectsocket);
        void OnReceive(ConnectSocket _connectsocket, byte[] _data);
    }
}
