using Assets.Scripts.Entity.Entities;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using UnityEngine;

namespace Assets.Scripts.Entity.Behaviors
{
    public class PlayerMethods
    {
        public void UpdateMovement(Player player, EntityMovementArgs args)
        {
            player.Position = args.Position;
            player.CameraYaw = args.CameraYaw;
            Debug.Log($"{player.UserName} is walking {player.Position} - {player.CameraYaw}");
        }
    }
}
