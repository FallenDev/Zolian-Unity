using Assets.Scripts.Managers;
using Assets.Scripts.Models;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;

using UnityEngine;

namespace Assets.Scripts.Network
{
    public static class WorldServerHandlers
    {
        public static void LoginMessageReceived(object args)
        {
            var loginArgs = (LoginMessageArgs)args;
            if (loginArgs.Message == "Established")
                WorldClient.Instance.SendEnterGame(CharacterGameManager.Instance.Serial, CharacterGameManager.Instance.UserName);
        }

        public static void ServerMessageReceived(object args)
        {
            var messageArgs = (ServerMessageArgs)args;
            MainThreadDispatcher.RunOnMainThread(() =>
            {
                PopupManager.Instance.ShowMessage(messageArgs.Message, messageArgs.ServerMessageType);
            });
        }

        public static void CharacterDataReceived(object args)
        {
            var playerArgs = (CharacterDataArgs)args;
            MainThreadDispatcher.RunOnMainThread(() =>
            {
                CharacterGameManager.Instance.SpawnPlayerPrefab(playerArgs);
            });
        }

        public static void EntityMovementReceived(object args)
        {
            var entityArgs = (EntityMovementArgs)args;

            switch (entityArgs.EntityType)
            {
                case EntityType.Player:
                    {
                        if (CharacterGameManager.Instance.CachedPlayers.TryGetValue(entityArgs.Serial, out var player))
                            player.UpdateMovement(entityArgs);
                    }
                    break;
                case EntityType.NPC:
                    {
                        if (CharacterGameManager.Instance.CachedNpcs.TryGetValue(entityArgs.Serial, out var npc))
                        {
                            //npc.UpdateMovement(entityArgs);
                        }
                    }
                    break;
                case EntityType.Monster:
                    {
                        if (CharacterGameManager.Instance.CachedMobs.TryGetValue(entityArgs.Serial, out var mob))
                        {
                            //mob.UpdateMovement(entityArgs);
                        }
                    }
                    break;
                case EntityType.Item:
                    {
                        if (CharacterGameManager.Instance.CachedItems.TryGetValue(entityArgs.Serial, out var item))
                        {
                            //item.UpdateMovement(entityArgs);
                        }
                    }
                    break;
                case EntityType.Pet:
                case EntityType.Mount:
                case EntityType.Summon:
                case EntityType.Unknown:
                    break;
            }
        }
    }
}
