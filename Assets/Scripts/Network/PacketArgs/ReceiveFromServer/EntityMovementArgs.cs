using System;
using Assets.Scripts.Models;
using Assets.Scripts.Network.PacketHandling;
using UnityEngine;

namespace Assets.Scripts.Network.PacketArgs.ReceiveFromServer
{
    public sealed record EntityMovementArgs : IPacketSerializable
    {
        public EntityType EntityType { get; set; }
        public Guid Serial { get; set; }
        public Vector3 Position { get; set; }
        public float CameraYaw { get; set; }
    }
}