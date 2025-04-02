using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.CharacterSelection;
using Assets.Scripts.Managers;
using Assets.Scripts.Models;
using Assets.Scripts.Network.Converters.SendToServer;
using Assets.Scripts.Network.Converters.ReceiveFromServer;
using Assets.Scripts.Network.OpCodes;
using Assets.Scripts.Network.PacketArgs.SendToServer;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;

namespace Assets.Scripts.Network
{
    /// <summary>
    /// LoginClient is a singleton class that handles the connection to the login server.
    /// Note: Cannot be Abstract because it inherits a MonoBehaviour
    /// </summary>
    public class LoginClient : NetworkBase
    {
        public List<PlayerSelection> CachedPlayers = new();
        public long SteamId;
        private static LoginClient _instance;

        public static LoginClient Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = FindFirstObjectByType<LoginClient>();
                if (_instance != null) return _instance;

                // If no instance is found, create one
                var singletonObject = new GameObject("LoginClient");
                _instance = singletonObject.AddComponent<LoginClient>();

                return _instance;
            }
        }

        private void Start()
        {
            IndexServerConverters();
            IndexClientConverters();
            ConnectToServer(LoginPort);
        }

        #region Client-Handlers

        private void SendConnectionConfirmation()
        {
            try
            {
                var args = new ConfirmConnectionArgs();
                SendPacket(ConfirmConnectionArgs.OpCode, args);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to send connection confirmation: {e.Message}");
                MainThreadDispatcher.RunOnMainThread(() =>
                {
                    PopupManager.Instance.ShowMessage(e.Message, PopupMessageType.System);
                });
            }
        }

        /// <summary>
        /// Obtained SteamID sent to the server for login
        /// </summary>
        public void SendLoginCredentials(long steamId)
        {
            try
            {
                var args = new LoginArgs
                {
                    SteamId = steamId
                };

                SendPacket(LoginArgs.OpCode, args);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to send login credentials: {e.Message}");
                MainThreadDispatcher.RunOnMainThread(() =>
                {
                    PopupManager.Instance.ShowMessage(e.Message, PopupMessageType.System);
                });
            }
        }

        public void SendEnterGame(Guid serial, string userName)
        {
            try
            {
                var args = new EnterGameArgs
                {
                    Serial = serial,
                    SteamId = SteamId,
                    UserName = userName
                };

                SendPacket(EnterGameArgs.OpCode, args);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to enter world: {e.Message}");
                MainThreadDispatcher.RunOnMainThread(() =>
                {
                    PopupManager.Instance.ShowMessage(e.Message, PopupMessageType.System);
                });
            }
        }

        public void SendCharacterDelete(long steamId, string name)
        {
            try
            {
                var args = new DeleteCharacterArgs
                {
                    SteamId = steamId,
                    Name = name
                };

                SendPacket(DeleteCharacterArgs.OpCode, args);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete character: {e.Message}");
                MainThreadDispatcher.RunOnMainThread(() =>
                {
                    PopupManager.Instance.ShowMessage(e.Message, PopupMessageType.System);
                });
            }
        }

        public void SendCharacterCreation(long steamId, string name, BaseClass className, Race race, Sex sex, 
            short hairIndex, short bangsIndex, short beardIndex, short mustacheIndex, short hairColorIndex,
            short hairHighlightIndex, short eyeColorIndex, short skinToneIndex)
        {
            try
            {
                var args = new CreateCharacterArgs
                {
                    SteamId = steamId,
                    Name = name,
                    Class = className,
                    Race = race,
                    Sex = sex,
                    Hair = hairIndex,
                    Bangs = bangsIndex,
                    Beard = beardIndex,
                    Mustache = mustacheIndex,
                    HairColor = hairColorIndex,
                    HairHighlightColor = hairHighlightIndex,
                    EyeColor = eyeColorIndex,
                    SkinColor = skinToneIndex
                };

                SendPacket(CreateCharacterArgs.OpCode, args);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create character: {e.Message}");
                MainThreadDispatcher.RunOnMainThread(() =>
                {
                    PopupManager.Instance.ShowMessage(e.Message, PopupMessageType.System);
                });
            }
        }

        #endregion

        protected override void HandlePacket(byte opCode, object args)
        {
            switch (opCode)
            {
                case (byte)ServerOpCode.AcceptConnection: // Login message
                    {
                        var acceptArgs = (AcceptConnectionArgs)args;
                        Debug.Log($"Accept connection message: {acceptArgs.Message}");

                        // ToDo: Obtain SteamID and send it here
                        SteamId = 123456;
                        SendLoginCredentials(SteamId);
                        break;
                    }
                case (byte)ServerOpCode.LoginMessage: // Login message
                    {
                        var loginArgs = (LoginMessageArgs)args;
                        MainThreadDispatcher.RunOnMainThread(() =>
                        {
                            PopupManager.Instance.ShowMessage(loginArgs.Message, PopupMessageType.System);
                        });
                        break;
                    }
                case (byte)ServerOpCode.ConnectionInfo: // Redirect
                    {
                        var connectionInfoArgs = (ConnectionInfoArgs)args;
                        // Redirect to World Scene
                        Debug.Log($"Redirecting to World Server at {connectionInfoArgs.PortNumber}");
                        if (connectionInfoArgs.PortNumber == WorldPort)
                        {
                            LoginSceneTransfer = true;
                        }
                        break;
                    }
                case (byte)ServerOpCode.CreateCharacterFinalized: // Character finalized
                    {
                        var characterFinalizedArgs = (CharacterFinalizedArgs)args;
                        if (characterFinalizedArgs.Finalized)
                        {
                            MainThreadDispatcher.RunOnMainThread(() =>
                            {
                                PopupManager.Instance.ShowMessage("Character created successfully!", PopupMessageType.System);
                                CreationAndAuthManager.Instance.CharacterFinalized();
                                SendLoginCredentials(SteamId);
                            });
                        }
                        break;
                    }
                case (byte)ServerOpCode.PlayerList:
                    {
                        var playerListArgs = (PlayerListArgs)args;
                        CachedPlayers.Clear();
                        CachedPlayers = new List<PlayerSelection>();
                        CachedPlayers = playerListArgs.Players.Where(p => p.Disabled != true).ToList();

                        MainThreadDispatcher.RunOnMainThread(() =>
                        {
                            if (CachedPlayers.Count == 0)
                                PopupManager.Instance.ShowMessage("No characters found, create one below!", PopupMessageType.System);
                            else
                            {
                                CharacterSelectionUI.Instance.PopulateCharacterList();
                                CharacterSelectionUI.Instance.SelectCharacter(0);
                            }

                            CreationAndAuthManager.Instance.createButton.gameObject.SetActive(true);
                        });
                        break;
                    }
                default:
                    MainThreadDispatcher.RunOnMainThread(() =>
                    {
                        PopupManager.Instance.ShowMessage($"Unhandled OpCode: {opCode}", PopupMessageType.System);
                    });
                    break;
            }
        }

        private void IndexServerConverters()
        {
            ServerConverters.Add((byte)ServerOpCode.AcceptConnection, new AcceptConnectionConverter());
            ServerConverters.Add((byte)ServerOpCode.LoginMessage, new LoginMessageConverter());
            ServerConverters.Add((byte)ServerOpCode.ServerMessage, new ServerMessageConverter());
            ServerConverters.Add((byte)ServerOpCode.CreateCharacterFinalized, new CharacterFinalizedConverter());
            ServerConverters.Add((byte)ServerOpCode.ConnectionInfo, new ConnectionInfoConverter());
            ServerConverters.Add((byte)ServerOpCode.PlayerList, new PlayerListConverter());
        }

        private void IndexClientConverters()
        {
            ClientConverters.Add((byte)ClientOpCode.ClientRedirected, new ConfirmConnectionConverter());
            ClientConverters.Add((byte)ClientOpCode.Login, new LoginConverter());
            ClientConverters.Add((byte)ClientOpCode.CreateCharacter, new CreateCharacterConverter());
            ClientConverters.Add((byte)ClientOpCode.DeleteCharacter, new DeleteCharacterConverter());
            ClientConverters.Add((byte)ClientOpCode.EnterGame, new EnterGameConverter());
        }
    }
}
