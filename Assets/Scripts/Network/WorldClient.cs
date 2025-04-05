using UnityEngine;
using Assets.Scripts.Entity.Entities;
using Assets.Scripts.Network.Converters.ReceiveFromServer;
using Assets.Scripts.Network.Converters.SendToServer;
using Assets.Scripts.Network.OpCodes;
using Assets.Scripts.Network.PacketArgs.SendToServer;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using Assets.Scripts.Managers;
using Assets.Scripts.Models;
using Steamworks;
using System;

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
        private void SendEnterGame(Guid serial, string userName)
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

                        // Character data based on packet enumeration type
                        Player.PlayerInstance.Serial = playerArgs.Serial;
                        Player.PlayerInstance.CurrentZoneId = 0;
                        Player.PlayerInstance.Position = playerArgs.Position;
                        Player.PlayerInstance.EntityLevel = playerArgs.EntityLevel;
                        Player.PlayerInstance.CurrentHp = playerArgs.CurrentHealth;
                        Player.PlayerInstance.MaxHp = playerArgs.MaxHealth;
                        Player.PlayerInstance.CurrentMp = playerArgs.CurrentMana;
                        Player.PlayerInstance.MaxMp = playerArgs.MaxMana;
                        Player.PlayerInstance.CurrentStamina = playerArgs.CurrentStamina;
                        Player.PlayerInstance.MaxStamina = playerArgs.MaxStamina;
                        Player.PlayerInstance.CurrentRage = playerArgs.CurrentRage;
                        Player.PlayerInstance.MaxRage = playerArgs.MaxRage;
                        Player.PlayerInstance.Reflex = playerArgs.Reflex;
                        Player.PlayerInstance.Fortitude = playerArgs.Fortitude;
                        Player.PlayerInstance.Will = playerArgs.Will;
                        Player.PlayerInstance.ArmorClass = playerArgs.ArmorClass;
                        Player.PlayerInstance.OffenseElement = playerArgs.OffenseElement;
                        Player.PlayerInstance.SecondaryOffensiveElement = playerArgs.SecondaryOffenseElement;
                        Player.PlayerInstance.DefenseElement = playerArgs.DefenseElement;
                        Player.PlayerInstance.SecondaryDefensiveElement = playerArgs.SecondaryDefenseElement;
                        Player.PlayerInstance.Str = playerArgs.Str;
                        Player.PlayerInstance.Int = playerArgs.Int;
                        Player.PlayerInstance.Wis = playerArgs.Wis;
                        Player.PlayerInstance.Con = playerArgs.Con;
                        Player.PlayerInstance.Dex = playerArgs.Dex;
                        Player.PlayerInstance.Luck = playerArgs.Luck;
                        Player.PlayerInstance.Regen = playerArgs.Regen;
                        Player.PlayerInstance.Dmg = playerArgs.Dmg;
                        Player.PlayerInstance.LastLogged = DateTime.Now;
                        Player.PlayerInstance.UserName = playerArgs.UserName;
                        Player.PlayerInstance.Stage = playerArgs.Stage;
                        Player.PlayerInstance.Job = playerArgs.Job;
                        Player.PlayerInstance.JobLevel = playerArgs.JobLevel;
                        Player.PlayerInstance.FirstClass = playerArgs.FirstClass;
                        Player.PlayerInstance.SecondClass = playerArgs.SecondClass;
                        Player.PlayerInstance.GameMaster = playerArgs.GameMaster;
                        Player.PlayerInstance.Race = playerArgs.Race;
                        Player.PlayerInstance.Gender = playerArgs.Sex;
                        Player.PlayerInstance.Hair = playerArgs.Hair;
                        Player.PlayerInstance.HairColor = playerArgs.HairColor;
                        Player.PlayerInstance.HairHighlightColor = playerArgs.HairHighlightColor;
                        Player.PlayerInstance.SkinColor = playerArgs.SkinColor;
                        Player.PlayerInstance.EyeColor = playerArgs.EyeColor;
                        Player.PlayerInstance.Beard = playerArgs.Beard;
                        Player.PlayerInstance.Mustache = playerArgs.Mustache;
                        Player.PlayerInstance.Bangs = playerArgs.Bangs;

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
            ServerConverters.Add((byte)ServerOpCode.ServerMessage, new ServerMessageConverter());
        }

        private void IndexClientConverters()
        {
            ClientConverters.Add((byte)ClientOpCode.ClientRedirected, new ConfirmConnectionConverter());
            ClientConverters.Add((byte)ClientOpCode.EnterGame, new EnterGameConverter());
        }
    }
}
