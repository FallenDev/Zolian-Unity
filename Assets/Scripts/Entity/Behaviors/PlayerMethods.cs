using Assets.Scripts.Entity.Entities;
using Assets.Scripts.Managers;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using UnityEngine;

namespace Assets.Scripts.Entity.Behaviors
{
    public static class PlayerMethods
    {
        public static void UpdateMovement(Player player, EntityMovementArgs args)
        {
            if (args.Serial != player.Serial) return;

            var bridge = player.GetComponent<RPGMotorNetworkBridge>();
            if (bridge == null)
            {
                Debug.LogError($"RPGMotorNetworkBridge not found on {player.name}");
                return;
            }
            bridge.ApplyRemoteState(args);
        }
    }
}
