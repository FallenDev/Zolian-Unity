using System;
using Assets.Scripts.Network.OpCodes;
using Assets.Scripts.Network.PacketHandling;
using UnityEngine;

namespace Assets.Scripts.Network.PacketArgs.SendToServer
{
    public sealed record MovementInputArgs : IPacketSerializable
    {
        public static byte OpCode => (byte)ClientOpCode.MovementInput;
        public Guid Serial;
        public Vector3 Position; // Player's current position
        public Vector3 InputDirection; // Input direction from the player
        public float CameraYaw; // Orientation of the Camera/Player
        public float Speed; // Current Forward Speed of movement
        public float VerticalVelocity; // Current vertical velocity of the player
    }
}