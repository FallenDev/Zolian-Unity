using System.Collections.Generic;
using Assets.Scripts.Models;

namespace Assets.Scripts.Network.PacketArgs.ReceiveFromServer
{
    public sealed record PlayerListArgs : IPacketSerializable
    {
        public List<PlayerSelection> Players { get; set; }
    }

    public sealed record PlayerSelection
    {
        public long Serial { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public BaseClass BaseClass { get; set; }
        public BaseClass AdvClass { get; set; }
        public JobClass Job { get; set; }
        public long Health { get; set; }
        public long Mana { get; set; }
    }
}
