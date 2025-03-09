using Assets.Scripts.Models;

namespace Assets.Scripts.Network.PacketArgs.ReceiveFromServer
{
    public sealed record LoginMessageArgs : IPacketSerializable
    {
        public PopupMessageType LoginMessageType { get; set; }
        public string Message { get; set; }
    }
}