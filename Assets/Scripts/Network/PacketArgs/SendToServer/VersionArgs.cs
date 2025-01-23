using Assets.Scripts.Network.OpCodes;
using UnityEngine;

namespace Assets.Scripts.Network.PacketArgs.SendToServer
{
    public sealed record VersionArgs : IPacketSerializable
    {
        public byte OpCode => (byte)ClientOpCode.Version;
        public string Version => Application.version;
    }
}