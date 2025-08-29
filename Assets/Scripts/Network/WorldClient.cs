using UnityEngine;
using Assets.Scripts.Network.Converters.ReceiveFromServer;
using Assets.Scripts.Network.Converters.SendToServer;
using Assets.Scripts.Network.OpCodes;
using Assets.Scripts.Network.PacketArgs.SendToServer;
using Assets.Scripts.Managers;
using System;
using Assets.Scripts.GameEntities.Behaviors;
using Assets.Scripts.Models;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;

namespace Assets.Scripts.Network
{
    public class WorldClient : NetworkBase
    {
        private static WorldClient _instance;

        public static WorldClient Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = FindFirstObjectByType<WorldClient>();
                if (_instance != null) return _instance;

                // If no instance is found, create one
                var singletonObject = new GameObject("WorldClient");
                _instance = singletonObject.AddComponent<WorldClient>();

                return _instance;
            }
        }

        private void Start()
        {
            IndexServerConverters();
            IndexClientConverters();
            ConnectToServer(CharacterGameManager.Instance.WorldPort);
        }

        #region Client-Handlers

        /// <summary>
        /// Sends basic player information to the server to enter the game and sends back character information
        /// </summary>
        public void SendEnterGame(Guid serial, string userName)
        {
            try
            {
                var args = new EnterGameArgs
                {
                    Serial = serial,
                    SteamId = CharacterGameManager.Instance.SteamId,
                    UserName = userName
                };

                SendPacket(EnterGameArgs.OpCode, args);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to enter world: {e.Message}");
            }
        }

        public void SendMovement(Guid serial, Vector3 position, float verticalVelocity, Vector3 inputDirection, float cameraYaw, float speed)
        {
            try
            {
                var args = new MovementInputArgs
                {
                    Serial = serial,
                    Position = position,
                    VerticalVelocity = verticalVelocity,
                    InputDirection = inputDirection,
                    CameraYaw = cameraYaw,
                    Speed = speed
                };

                SendPacket(MovementInputArgs.OpCode, args);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to send movement: {e.Message}");
            }
        }

        #endregion

        protected override void HandlePacket(byte opCode, object args)
        {
            switch (opCode)
            {
                case (byte)ServerOpCode.LoginMessage:
                    {
                        var loginArgs = (LoginMessageArgs)args;
                        if (loginArgs.Message == "Established")
                            SendEnterGame(CharacterGameManager.Instance.Serial, CharacterGameManager.Instance.UserName);
                        break;
                    }
                case (byte)ServerOpCode.ServerMessage:
                    {
                        var messageArgs = (ServerMessageArgs)args;
                        MainThreadDispatcher.RunOnMainThread(() =>
                        {
                            PopupManager.Instance.ShowMessage(messageArgs.Message, messageArgs.ServerMessageType);
                        });
                        break;
                    }
                case (byte)ServerOpCode.CharacterData:
                    {
                        var playerArgs = (CharacterDataArgs)args;
                        MainThreadDispatcher.RunOnMainThread(() =>
                        {
                            CharacterGameManager.Instance.SpawnPlayerPrefab(playerArgs);
                        });
                        break;
                    }
                case (byte)ServerOpCode.EntityMovement:
                    {
                        var entityArgs = (EntityMovementArgs)args;
                        switch (entityArgs.EntityType)
                        {
                            case EntityType.Player:
                                {
                                    MainThreadDispatcher.RunOnMainThread(() =>
                                    {
                                        if (CharacterGameManager.Instance.CachedPlayers.TryGetValue(entityArgs.Serial, out var player))
                                            PlayerMethods.UpdateMovement(player, entityArgs);
                                    });
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
                        }
                        break;
                    }
                case (byte)ServerOpCode.AddEntity:
                    {
                        var entityArgs = (EntitySpawnArgs)args;
                        switch (entityArgs.EntityType)
                        {
                            case EntityType.Player:
                                {
                                    if (CharacterGameManager.Instance.CachedPlayers.ContainsKey(entityArgs.Serial)) return;

                                    MainThreadDispatcher.RunOnMainThread(() =>
                                    {
                                        var player = CharacterGameManager.Instance.SpawnOtherPlayerPrefab(entityArgs);
                                        // Cache for future updates
                                        CharacterGameManager.Instance.CachedPlayers[entityArgs.Serial] = player;
                                    });
                                }
                                break;

                            case EntityType.NPC:
                                break;
                            case EntityType.Monster:
                                break;
                            case EntityType.Pet:
                                break;
                            case EntityType.Mount:
                                break;
                            case EntityType.Summon:
                                break;
                            case EntityType.Item:
                                break;
                            case EntityType.Unknown:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        break;
                    }
                default:
                    Debug.LogWarning($"Unhandled OpCode: {opCode}");
                    break;
            }
        }

        private void IndexServerConverters()
        {
            ServerConverters.Add((byte)ServerOpCode.LoginMessage, new LoginMessageConverter());
            ServerConverters.Add((byte)ServerOpCode.CharacterData, new CharacterDataConverter());
            ServerConverters.Add((byte)ServerOpCode.EntityMovement, new EntityMovementConverter());
            ServerConverters.Add((byte)ServerOpCode.AddEntity, new EntitySpawnConveter());
            ServerConverters.Add((byte)ServerOpCode.ServerMessage, new ServerMessageConverter());
        }

        private void IndexClientConverters()
        {
            ClientConverters.Add((byte)ClientOpCode.ClientRedirected, new ConfirmConnectionConverter());
            ClientConverters.Add((byte)ClientOpCode.EnterGame, new EnterGameConverter());
            ClientConverters.Add((byte)ClientOpCode.MovementInput, new MovementInputConverter());
        }
    }
}
