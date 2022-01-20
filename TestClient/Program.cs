using System.Text;

using ServerModule.Network;

using Shared;

namespace TestClient
{
    public class Event : ClientSocketEvent
    {
        public Event()
            : base()
        {
            Converter = new JsonConverter();
            m_dispatcher = new Dispatcher<ConnectSocket, int>();
        }

        public void OnError(SocketErrorType _error_type, Exception _exception, ConnectSocket? _connectsocket)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]OnError. ErrorType={_error_type.ToString()}, Msg={_exception.Message}, StackTrace={_exception.StackTrace}, Source={_exception.Source}");
        }

        public void OnConnect(ConnectSocket _connectsocket)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]Connect");
        }

        public void OnDisconnect(ConnectSocket _connectsocket)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]OnDisconnect");
        }

        public void OnSend(ConnectSocket _connectsocket)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]OnSend");
        }

        public void OnReceive(ConnectSocket _connectsocket, byte[] _data)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]OnReceive Data={Encoding.UTF8.GetString(_data)}");

            PacketBase<int>? packet_base = null;
            if(false == Converter.Deserialize(_data, out packet_base))
            {
                return;
            }

            if(null == packet_base)
            {
                return;
            }

            FunctionBase<ConnectSocket, int>? func_handler;
            Type? packet_type = null;
            if (false == m_dispatcher.GetFunction(packet_base.Index, out func_handler, out packet_type))
            {
                return;
            }

            if (null == func_handler)
            {
                return;
            }

            func_handler.ExecuteFunction(_connectsocket, packet_base);
        }

        public bool Send<PacketObject>(ConnectSocket _socket, PacketObject _packet_object)
        {
            if(null == _packet_object)
            {
                return false;
            }

            byte[]? byte_data = null;
            if(false == Converter.Serialize(_packet_object!, typeof(PacketObject), out byte_data))
            {
                return false;
            }

            if(null == byte_data)
            {
                return false;
            }

            return _socket.Send(byte_data);
        }

        public void ServerToClient(ConnectSocket _socket, ServerToClient _packet)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]Message={_packet.Message}");
        }

        public IConverter Converter;
        private Dispatcher<ConnectSocket, int> m_dispatcher;
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Server IP:");
            string? ip = Console.ReadLine();
            Console.Write("Server Port:");
            int port = int.Parse(Console.ReadLine()!);

            Event _event = new Event(); 
            ConnectSocket connectsocket = new ConnectSocket(_event);
            if (false == connectsocket.Connect(ip!, port))
            {
                Console.WriteLine("Connect Fail");
            }

            while (true)
            {
                Console.Write("Data Input : ");
                string? data = Console.ReadLine();

                ClientToServer packet = new ClientToServer() { Message = data! };
                _event.Send<ClientToServer>(connectsocket, packet);
            }
        }
    }
}
