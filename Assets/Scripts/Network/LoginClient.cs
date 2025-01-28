using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using System.Security.Authentication;
using System.Text;
using System.Collections.Generic;
using Assets.Scripts.Network.Converters.SendToServer;
using Assets.Scripts.Network.Converters.ReceiveFromServer;
using Assets.Scripts.Network.OpCodes;
using Assets.Scripts.Network.PacketArgs.SendToServer;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network
{
    public class LoginClient : MonoBehaviour
    {
        private bool isConnected;
        private SslStream sslStream;
        private TcpClient tcpClient;
        private StreamReader reader;
        private StreamWriter writer;
        private readonly Dictionary<byte, IPacketConverter> ServerConverters = new();
        private readonly Dictionary<byte, IPacketConverter> ClientConverters = new();

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

        private void Update() { }

        private void OnApplicationQuit() => Cleanup();

        private void Cleanup()
        {
            isConnected = false;

            try
            {
                writer?.Close();
                reader?.Close();
                sslStream?.Close();
                tcpClient?.Close();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during cleanup: {ex.Message}");
            }
        }

        #region Client-Handlers

        private void SendConnectionConfirmation()
        {
            var args = new ConfirmConnectionArgs();
            SendPacket(args.OpCode, args);
        }

        public void SendLoginCredentials(string username, string password)
        {
            var args = new LoginArgs
            {
                Username = username,
                Password = password
            };

            SendPacket(args.OpCode, args);
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
                if (!ClientConverters.TryGetValue(opCode, out var converter))
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
                sslStream.Write(data, 0, data.Length);
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
                while (isConnected)
                {
                    var bytesRead = await sslStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);

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

                if (ServerConverters.TryGetValue(packet.OpCode, out var converter))
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
                        break;
                    }
                case (byte)ServerOpCode.LoginMessage: // Login message
                    {
                        var loginArgs = (LoginMessageArgs)args;
                        MainThreadDispatcher.RunOnMainThread(() =>
                        {
                            PopupManager.Instance.ShowMessage(loginArgs.Message);
                        });
                        break;
                    }
                case (byte)ServerOpCode.ConnectionInfo: // Redirect
                    {
                        var connectionInfoArgs = (ConnectionInfoArgs)args;
                        // Redirect to World Scene
                        break;
                    }
                default:
                    MainThreadDispatcher.RunOnMainThread(() =>
                    {
                        PopupManager.Instance.ShowMessage($"Unhandled OpCode: {opCode}");
                    });
                    break;
            }
        }

        #endregion

        #region Configuration

        private void IndexServerConverters()
        {
            ServerConverters.Add((byte)ServerOpCode.AcceptConnection, new AcceptConnectionConverter());
            ServerConverters.Add((byte)ServerOpCode.LoginMessage, new LoginMessageConverter());
            ServerConverters.Add((byte)ServerOpCode.ServerMessage, new ServerMessageConverter());
            ServerConverters.Add((byte)ServerOpCode.ConnectionInfo, new ConnectionInfoConverter());
        }

        private void IndexClientConverters()
        {
            ClientConverters.Add((byte)ClientOpCode.ClientRedirected, new ConfirmConnectionConverter());
            ClientConverters.Add((byte)ClientOpCode.Login, new LoginConverter());
        }

        public void ConnectToServer(ushort port)
        {
            try
            {
                // Initialize the TCP client and SSL stream
                tcpClient = new TcpClient(_serverIp, port);
                ConfigureTcpSocket(tcpClient.Client);
                sslStream = new SslStream(tcpClient.GetStream(), false, ValidateServerCertificate);

                // Perform SSL handshake
                sslStream.AuthenticateAsClient("localhost", null, SslProtocols.Tls12, false);

                // Initialize reader and writer for the stream
                writer = new StreamWriter(sslStream) { AutoFlush = true };
                reader = new StreamReader(sslStream);
                isConnected = true;
                if (port == _worldPort)
                    SendConnectionConfirmation();
            }
            catch (AuthenticationException ex)
            {
                PopupManager.Instance.ShowMessage($"SSL Authentication failed: {ex.Message}");
                Cleanup();
            }
            catch (SocketException ex)
            {
                PopupManager.Instance.ShowMessage("Server Offline");
                Cleanup();
            }
            catch (Exception ex)
            {
                PopupManager.Instance.ShowMessage($"Connection error: {ex.Message}");
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

                PopupManager.Instance.ShowMessage($"Certificate validation failed: {sslPolicyErrors}");
                return false;
            }

            PopupManager.Instance.ShowMessage("The provided certificate is not an X509Certificate2.");
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
