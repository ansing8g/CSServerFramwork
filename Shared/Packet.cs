using ServerModule.Network;

namespace Shared
{
    
    public class PacketBase : PacketBase<int>
    {
        public PacketBase(int _index)
        {
            Index = _index;
        }
    }

    public class ClientToServer : PacketBase
    {
        public ClientToServer()
            : base(1)
        {
            Message = "";
        }

        public string Message { get; set; }
    }

    public class ServerToClient : PacketBase
    {
        public ServerToClient()
            : base(2)
        {
            Message = "";
        }

        public string Message { get; set; }
    }
}
