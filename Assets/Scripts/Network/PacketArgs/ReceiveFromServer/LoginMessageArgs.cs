using Assets.Scripts.Models;
using Assets.Scripts.Network.PacketHandling;

namespace Assets.Scripts.Network.PacketArgs.ReceiveFromServer
{
    public sealed record LoginMessageArgs : IPacketSerializable
    {
        public PopupMessageType LoginMessageType { get; set; }
        public string Message { get; set; }
    }
}