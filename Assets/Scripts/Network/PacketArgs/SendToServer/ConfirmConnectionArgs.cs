using Assets.Scripts.Network.OpCodes;
using Assets.Scripts.Network.PacketHandling;

namespace Assets.Scripts.Network.PacketArgs.SendToServer
{
    public sealed record ConfirmConnectionArgs : IPacketSerializable
    {
        public static byte OpCode => (byte)ClientOpCode.ClientRedirected;
        public static string Message => "Redirect Successful";
    }
}