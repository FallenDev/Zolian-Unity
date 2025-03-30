using Assets.Scripts.Models;
using Assets.Scripts.Network.PacketHandling;

namespace Assets.Scripts.Network.PacketArgs.ReceiveFromServer
{
    public sealed record ServerMessageArgs : IPacketSerializable
    {
        public PopupMessageType ServerMessageType { get; set; }
        public string Message { get; set; } = null!;
    }
}