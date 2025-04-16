using Assets.Scripts.Entity.Entities;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using UnityEngine;

namespace Assets.Scripts.Entity.Behaviors
{
    public static class PlayerMethods
    {
        public static void UpdateMovement(Player player, EntityMovementArgs args)
        {
            if (args.Serial != player.Serial) return;

            var motor = player.GetComponent<RemoteRPGMotor>();
            if (motor == null)
            {
                Debug.LogError($"RemoteRPGMotor not found on {player.name}");
                return;
            }

            if (!player.IsLocalPlayer)
                motor.ApplyRemoteState(
                    args.Position,
                    args.InputDirection,
                    args.CameraYaw,
                    args.Speed,
                    args.VerticalVelocity
                );
        }
    }
}