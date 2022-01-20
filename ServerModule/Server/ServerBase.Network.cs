using System;

using ServerModule.Network;

namespace ServerModule.Server
{
    public class ServerBase_Network_Server<PacketIndex> : ServerSocketEvent where PacketIndex : notnull
    {
        internal ServerBase_Network_Server()
        {
            EventError = null;
            EventAccept = null;
            EventDisconnect = null;
            EventSend = null;

            m_accept_socket = new AcceptSocket(this);
            m_dispatcher = new Dispatcher<SessionSocket, PacketIndex>();
            m_converter = null;
        }

        public virtual void OnError(SocketErrorType _error_type, Exception _exception, SessionSocket? _sessionsocket)
        {
            if (null != EventError)
            {
                EventError(_error_type, _exception, _sessionsocket);
            }
        }

        public virtual void OnAccept(SessionSocket _sessionsocket)
        {
            if (null != EventAccept)
            {
                EventAccept(_sessionsocket);
            }
        }

        public virtual void OnDisconnect(SessionSocket _sessionsocket)
        {
            if (null != EventDisconnect)
            {
                EventDisconnect(_sessionsocket);
            }
        }

        public virtual void OnSend(SessionSocket _sessionsocket)
        {
            if (null != EventSend)
            {
                EventSend(_sessionsocket);
            }
        }

        public virtual void OnReceive(SessionSocket _sessionsocket, byte[] _data)
        {
            if (null == m_converter)
            {
                return;
            }

            PacketBase<PacketIndex>? packet_base;
            if (false == m_converter.Deserialize<PacketBase<PacketIndex>>(_data, out packet_base))
            {
                return;
            }

            if (null == packet_base)
            {
                return;
            }

            FunctionBase<SessionSocket, PacketIndex>? func_handler;
            Type? packet_type = null;
            if (false == m_dispatcher.GetFunction(packet_base.Index, out func_handler, out packet_type))
            {
                return;
            }

            if (null == func_handler ||
                null == packet_type)
            {
                return;
            }

            object? packet_object = null;
            if(false == m_converter.Deserialize(_data, packet_type!, out packet_object))
            {
                return;
            }

            if(null == packet_object)
            {
                return;
            }

            func_handler.ExecuteFunction(_sessionsocket, (packet_object as PacketBase<PacketIndex>)!);
        }

        public bool Start(IConverter _converter, int _port, int _listen_count = 1000, uint _buf_size = 512, uint _total_buf_size = 5120)
        {
            m_converter = _converter;
            return m_accept_socket.Start(_port, _listen_count, _buf_size, _total_buf_size);
        }

        public void RegistHandler<PacketObject>(PacketIndex _packet_index, Action<SessionSocket, PacketObject> _func) where PacketObject : PacketBase<PacketIndex>
        {
            m_dispatcher.RegistFunction(_packet_index, _func);
        }

        public bool Send<PacketObject>(SessionSocket _sessionsocket, PacketObject _packet_object)
        {
            if (null == m_converter ||
                null == _packet_object)
            {
                return false;
            }

            byte[]? byte_data = null;
            if (false == m_converter.Serialize(_packet_object!, typeof(PacketObject), out byte_data))
            {
                return false;
            }

            if (null == byte_data)
            {
                return false;
            }

            return _sessionsocket.Send(byte_data);
        }

        public Action<SocketErrorType, Exception, SessionSocket?>? EventError { private get; set; }
        public Action<SessionSocket>? EventAccept { private get; set; }
        public Action<SessionSocket>? EventDisconnect { private get; set; }
        public Action<SessionSocket>? EventSend { private get; set; }

        public IConverter? m_converter;

        private AcceptSocket m_accept_socket;
        private Dispatcher<SessionSocket, PacketIndex> m_dispatcher;
    }

    public class ServerBase_Network_Client<PacketIndex> : ClientSocketEvent where PacketIndex : notnull
    {
        internal ServerBase_Network_Client()
        {
            EventError = null;
            EventAccept = null;
            EventDisconnect = null;
            EventSend = null;

            m_connect_socket = new ConnectSocket(this);
            m_dispatcher = new Dispatcher<ConnectSocket, PacketIndex>();
            m_converter = null;
        }

        public virtual void OnError(SocketErrorType _error_type, Exception _exception, ConnectSocket? _connectsocket)
        {
            if (null != EventError)
            {
                EventError(_error_type, _exception, _connectsocket);
            }
        }

        public virtual void OnConnect(ConnectSocket _connectsocket)
        {
            if (null != EventAccept)
            {
                EventAccept(_connectsocket);
            }
        }

        public virtual void OnDisconnect(ConnectSocket _connectsocket)
        {
            if (null != EventDisconnect)
            {
                EventDisconnect(_connectsocket);
            }
        }

        public virtual void OnSend(ConnectSocket _connectsocket)
        {
            if (null != EventSend)
            {
                EventSend(_connectsocket);
            }
        }

        public virtual void OnReceive(ConnectSocket _connectsocket, byte[] _data)
        {
            if (null == m_converter)
            {
                return;
            }

            PacketBase<PacketIndex>? packet_base;
            if (false == m_converter.Deserialize<PacketBase<PacketIndex>>(_data, out packet_base))
            {
                return;
            }

            if (null == packet_base)
            {
                return;
            }

            FunctionBase<ConnectSocket, PacketIndex>? func_handler;
            Type? packet_type = null;
            if (false == m_dispatcher.GetFunction(packet_base.Index, out func_handler, out packet_type))
            {
                return;
            }

            if (null == func_handler ||
                null == packet_type)
            {
                return;
            }

            object? packet_object = null;
            if (false == m_converter.Deserialize(_data, packet_type!, out packet_object))
            {
                return;
            }

            if (null == packet_object)
            {
                return;
            }

            func_handler.ExecuteFunction(_connectsocket, (packet_object as PacketBase<PacketIndex>)!);
        }

        public bool Connect(IConverter _converter, string _ip, int _port, uint _buf_size = 512, uint _total_buf_size = 5120)
        {
            m_converter = _converter;
            return m_connect_socket.Connect(_ip, _port, _buf_size, _total_buf_size);
        }

        public void RegistHandler<PacketObject>(PacketIndex _packet_index, Action<ConnectSocket, PacketObject> _func) where PacketObject : PacketBase<PacketIndex>
        {
            m_dispatcher.RegistFunction(_packet_index, _func);
        }

        public bool Send<PacketObject>(PacketObject _packet_object)
        {
            if(null == m_converter ||
                null == _packet_object)
            {
                return false;
            }

            byte[]? byte_data = null;
            if(false == m_converter.Serialize(_packet_object, typeof(PacketObject), out byte_data))
            {
                return false;
            }

            if(null == byte_data)
            {
                return false;
            }

            return m_connect_socket.Send(byte_data);
        }

        public Action<SocketErrorType, Exception, ConnectSocket?>? EventError { private get; set; }
        public Action<ConnectSocket>? EventAccept { private get; set; }
        public Action<ConnectSocket>? EventDisconnect { private get; set; }
        public Action<ConnectSocket>? EventSend { private get; set; }

        public IConverter? m_converter;

        private ConnectSocket m_connect_socket;
        private Dispatcher<ConnectSocket, PacketIndex> m_dispatcher;
    }
}
