using Assets.Scripts.Models;

namespace Assets.Scripts.Network.PacketArgs.ReceiveFromServer
{
    public sealed record ServerMessageArgs : IPacketSerializable
    {
        public PopupMessageType ServerMessageType { get; set; }
        public string Message { get; set; } = null!;
    }
}