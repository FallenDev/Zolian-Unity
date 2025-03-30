using Assets.Scripts.Network.OpCodes;
using Assets.Scripts.Network.PacketHandling;
using UnityEngine;

namespace Assets.Scripts.Network.PacketArgs.SendToServer
{
    public sealed record VersionArgs : IPacketSerializable
    {
        public static byte OpCode => (byte)ClientOpCode.Version;
        public static string Version => Application.version;
    }
}