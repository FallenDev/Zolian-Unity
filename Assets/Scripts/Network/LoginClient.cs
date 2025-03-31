using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using System.Security.Authentication;
using System.Collections.Generic;
using Assets.Scripts.CharacterSelection;
using Assets.Scripts.Managers;
using Assets.Scripts.Models;
using Assets.Scripts.Network.Converters.SendToServer;
using Assets.Scripts.Network.Converters.ReceiveFromServer;
using Assets.Scripts.Network.OpCodes;
using Assets.Scripts.Network.PacketArgs.SendToServer;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using Assets.Scripts.Network.PacketHandling;
using Assets.Scripts.Network.Span;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Network
{
    public class LoginClient : MonoBehaviour
    {
        private bool _isConnected;
        private bool _sceneTransfer;
        private SslStream _sslStream;
        private TcpClient _tcpClient;
        private StreamReader _reader;
        private StreamWriter _writer;
        private readonly Dictionary<byte, IPacketConverter> _serverConverters = new();
        private readonly Dictionary<byte, IPacketConverter> _clientConverters = new();
        public List<PlayerSelection> CachedPlayers = new();
        public long SteamId;
        private readonly object _sendLock = new();

        [SerializeField] private string _serverIp = "127.0.0.1";
        [SerializeField] private ushort _loginPort = 4201;
        [SerializeField] private ushort _worldPort = 4202;

        // Static instance of NetworkClient
        private static LoginClient _instance;

        // Public property to access the instance
        public static LoginClient Instance
        {
            get
            {
                if (_instance != null) return _instance;
                // Look for an existing NetworkClient in the scene
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
            ConnectToServer(_loginPort);
        }

        private void Update()
        {
            if (!_sceneTransfer) return;
            _sceneTransfer = false;
            SceneManager.LoadSceneAsync("World");
        }

        private void OnApplicationQuit() => Cleanup();

        private void Cleanup()
        {
            _isConnected = false;

            try
            {
                _writer?.Close();
                _reader?.Close();
                _sslStream?.Close();
                _tcpClient?.Close();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during cleanup: {ex.Message}");
            }
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

        public void SendEnterGame(Guid serial, long steamId, string userName)
        {
            try
            {
                var args = new EnterGameArgs
                {
                    Serial = serial,
                    SteamId = steamId,
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

        /// <summary>
        /// Sends a packet to the server, using the provided OpCode and arguments.
        /// </summary>
        private void SendPacket<T>(byte opCode, T args) where T : IPacketSerializable
        {
            try
            {
                // Fetch the appropriate converter from the indexer
                if (!_clientConverters.TryGetValue(opCode, out var converter))
                {
                    Debug.LogError($"No converter found for OpCode {opCode}.");
                    return;
                }

                // Cast the converter to the appropriate type
                if (converter is not PacketConverterBase<T> typedConverter)
                {
                    Debug.LogError($"Converter for OpCode {opCode} does not match type {typeof(T).Name}.");
                    return;
                }

                // Serialize the arguments into a payload
                Span<byte> buffer = stackalloc byte[ushort.MaxValue]; // Allocate a large enough buffer
                var packetWriter = new SpanWriter(buffer);
                typedConverter.Serialize(ref packetWriter, args);

                // Trim the buffer to the written content
                var payload = packetWriter.ToSpan();

                // Create and send the packet
                var packet = new Packet(opCode, payload);
                var data = packet.ToArray();
                lock (_sendLock)
                {
                    _sslStream.Write(data, 0, data.Length);
                }
                Debug.Log($"Packet with OpCode {opCode} sent successfully.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to send packet: {ex.Message}");
            }
        }

        #region Server -> Client

        private async void BeginReceive()
        {
            try
            {
                var buffer = new byte[ushort.MaxValue]; // Adjust size as needed
                while (_isConnected)
                {
                    var bytesRead = await _sslStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);

                    if (bytesRead > 0)
                    {
                        ProcessServerPacket(buffer.AsSpan(0, bytesRead).ToArray());
                    }
                }
            }
            catch
            {
                Cleanup();
            }
        }

        private void ProcessServerPacket(byte[] buffer)
        {
            try
            {
                var span = buffer.AsSpan();
                if (span[0] != 0x16) // Verify signature
                {
                    Debug.LogError("Invalid packet signature.");
                    return;
                }

                var packet = new Packet(span);
                Debug.Log($"Packet received: OpCode={packet.OpCode}, Sequence={packet.Sequence}, Length={packet.Length}");
                Debug.Log($"Raw Payload: {BitConverter.ToString(packet.Payload)}");

                if (_serverConverters.TryGetValue(packet.OpCode, out var converter))
                {
                    var spanReader = new SpanReader(packet.Payload);
                    var args = converter.Deserialize(ref spanReader);
                    HandlePacket(packet.OpCode, args);
                }
                else
                {
                    Debug.LogWarning($"Unhandled OpCode: {packet.OpCode}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error processing server packet: {ex.Message}");
            }
        }

        private void HandlePacket(byte opCode, object args)
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
                        if (connectionInfoArgs.PortNumber == _worldPort)
                        {
                            _sceneTransfer = true;
                            Cleanup();
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

        #endregion

        #region Configuration

        private void IndexServerConverters()
        {
            _serverConverters.Add((byte)ServerOpCode.AcceptConnection, new AcceptConnectionConverter());
            _serverConverters.Add((byte)ServerOpCode.LoginMessage, new LoginMessageConverter());
            _serverConverters.Add((byte)ServerOpCode.ServerMessage, new ServerMessageConverter());
            _serverConverters.Add((byte)ServerOpCode.CreateCharacterFinalized, new CharacterFinalizedConverter());
            _serverConverters.Add((byte)ServerOpCode.ConnectionInfo, new ConnectionInfoConverter());
            _serverConverters.Add((byte)ServerOpCode.PlayerList, new PlayerListConverter());
        }

        private void IndexClientConverters()
        {
            _clientConverters.Add((byte)ClientOpCode.ClientRedirected, new ConfirmConnectionConverter());
            _clientConverters.Add((byte)ClientOpCode.Login, new LoginConverter());
            _clientConverters.Add((byte)ClientOpCode.CreateCharacter, new CreateCharacterConverter());
            _clientConverters.Add((byte)ClientOpCode.DeleteCharacter, new DeleteCharacterConverter());
        }

        public void ConnectToServer(ushort port)
        {
            try
            {
                // Initialize the TCP client and SSL stream
                _tcpClient = new TcpClient(_serverIp, port);
                ConfigureTcpSocket(_tcpClient.Client);
                _sslStream = new SslStream(_tcpClient.GetStream(), false, ValidateServerCertificate);

                // Perform SSL handshake
                _sslStream.AuthenticateAsClient("localhost", null, SslProtocols.Tls12, false);

                // Initialize reader and writer for the stream
                _writer = new StreamWriter(_sslStream) { AutoFlush = true };
                _reader = new StreamReader(_sslStream);
                _isConnected = true;
                if (port == _worldPort)
                    SendConnectionConfirmation();
            }
            catch (AuthenticationException ex)
            {
                PopupManager.Instance.ShowMessage($"SSL Authentication failed: {ex.Message}", PopupMessageType.System);
                Cleanup();
            }
            catch (SocketException ex)
            {
                PopupManager.Instance.ShowMessage("Server Offline", PopupMessageType.Screen);
                Cleanup();
            }
            catch (Exception ex)
            {
                PopupManager.Instance.ShowMessage($"Connection error: {ex.Message}", PopupMessageType.System);
                Cleanup();
            }
            finally
            {
                BeginReceive();
            }
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate is X509Certificate2 cert2)
            {
                if (sslPolicyErrors == SslPolicyErrors.None)
                {
                    Debug.Log("Server certificate is valid.");
                    return true;
                }

                // Allow self-signed certificates for development/testing
#if DEBUG
                if (sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateChainErrors) && chain.ChainStatus.Any(status => status.Status == X509ChainStatusFlags.UntrustedRoot))
                {
                    Debug.LogWarning("Allowing self-signed certificate.");
                    return true;
                }
#endif

                PopupManager.Instance.ShowMessage($"Certificate validation failed: {sslPolicyErrors}", PopupMessageType.System);
                return false;
            }

            PopupManager.Instance.ShowMessage("The provided certificate is not an X509Certificate2.", PopupMessageType.System);
            return false;
        }

        private static void ConfigureTcpSocket(Socket tcpSocket)
        {
            tcpSocket.LingerState = new LingerOption(false, 0);
            tcpSocket.NoDelay = true;
            tcpSocket.Blocking = true;
            tcpSocket.ReceiveBufferSize = 32768;
            tcpSocket.SendBufferSize = 32768;
            // ToDo: Adjust timeout to be double the expected ping time - for now leave it at 30 seconds
            tcpSocket.ReceiveTimeout = 30000;
            tcpSocket.SendTimeout = 30000;
            tcpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        }

        #endregion
    }
}
