using Assets.Scripts.Entity.Entities;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;

using UnityEngine;

namespace Assets.Scripts.Entity.Behaviors
{
    public static class PlayerMethods
    {
        public static void UpdateMovement(Player player, EntityMovementArgs args)
        {
            if (player == null) return;
            if (player.IsLocalPlayer) return;
            if (player.RemoteMotor == null)
            {
                player.RemoteMotor = player.GetComponent<RemoteRPGMotor>();
                if (player.RemoteMotor == null)
                {
                    Debug.LogError("Remote motor not found on player.");
                    return;
                }
            }

            player.RemoteMotor.ApplyRemoteState(
                args.Position,
                args.InputDirection,
                args.CameraYaw,
                args.Speed,
                args.VerticalVelocity);
        }
    }
}