namespace ServerModule.Network
{
    public class PacketBase<PacketIndex>
    {
        public PacketBase()
        {
            Index = default(PacketIndex)!;
        }

        public PacketIndex Index { get; set; }
    }
}
