using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using System.Security.Authentication;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Scripts.Network.PacketArgs.SendToServer;
using Assets.Scripts.Network.PacketHandling;
using Assets.Scripts.Network.Span;
using UnityEngine.SceneManagement;
using Assets.Scripts.Managers;
using Assets.Scripts.Models;
using Assets.Scripts.Network.Converters.ReceiveFromServer;
using Assets.Scripts.Network.Converters.SendToServer;
using Assets.Scripts.Network.OpCodes;

namespace Assets.Scripts.Network
{
    public class NetworkBase : MonoBehaviour
    {
        private SslStream _sslStream;
        private TcpClient _tcpClient;
        private StreamReader _reader;
        private StreamWriter _writer;
        private readonly object _sendLock = new();

        protected bool IsConnected;
        protected readonly Dictionary<byte, IPacketConverter> ServerConverters = new();
        protected readonly Dictionary<byte, IPacketConverter> ClientConverters = new();
        protected string CentralServerIp = "127.0.0.1";
        protected ushort LobbyPort = 4200;
        protected ushort LoginPort = 4201;
        protected ushort WorldPort = 4202;

        protected bool LobbySceneTransfer;
        protected bool LoginSceneTransfer;


        private void Start()
        {
            IndexServerConverters();
            IndexClientConverters();
        }

        private void Update()
        {
            if (LobbySceneTransfer)
            {
                LobbySceneTransfer = false;
                SceneManager.LoadSceneAsync("Login");
            }

            if (LoginSceneTransfer)
            {
                LoginSceneTransfer = false;
                SceneManager.LoadSceneAsync("World");
            }
        }

        private void OnApplicationQuit() => Cleanup();

        private void Cleanup()
        {
            IsConnected = false;

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

        /// <summary>
        /// Sends a packet to the server, using the provided OpCode and arguments.
        /// </summary>
        protected void SendPacket<T>(byte opCode, T args) where T : IPacketSerializable
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

        private async Task BeginReceive()
        {
            try
            {
                var buffer = new byte[ushort.MaxValue]; // Adjust size as needed
                while (IsConnected)
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

        protected virtual void HandlePacket(byte opCode, object args)
        {

        }

        #region Core Packet Handling

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

        private void SendHeartBeatReply()
        {

        }

        #endregion

        private void IndexServerConverters()
        {
            // Add heartbeat converter
        }

        private void IndexClientConverters()
        {
            ClientConverters.Add((byte)ClientOpCode.ClientRedirected, new ConfirmConnectionConverter());
            // Add heartbeat reply converter
        }

        protected void ConnectToServer(ushort port)
        {
            try
            {
                // Initialize the TCP client and SSL stream
                _tcpClient = new TcpClient(CentralServerIp, port);
                ConfigureTcpSocket(_tcpClient.Client);
                _sslStream = new SslStream(_tcpClient.GetStream(), false, ValidateServerCertificate);

                // Perform SSL handshake
                _sslStream.AuthenticateAsClient("localhost", null, SslProtocols.Tls12, false);

                // Initialize reader and writer for the stream
                _writer = new StreamWriter(_sslStream) { AutoFlush = true };
                _reader = new StreamReader(_sslStream);
                IsConnected = true;
                if (port == LobbyPort)
                    LobbyClient.LobbyInstance.SendVersionNumber();
                else
                    SendConnectionConfirmation();
            }
            catch (AuthenticationException ex)
            {
                MainThreadDispatcher.RunOnMainThread(() =>
                {
                    PopupManager.Instance.ShowMessage($"SSL Authentication failed: {ex.Message}", PopupMessageType.System);
                });
                Cleanup();
            }
            catch (SocketException ex)
            {
                MainThreadDispatcher.RunOnMainThread(() =>
                {
                    PopupManager.Instance.ShowMessage("Server Offline", PopupMessageType.Screen);
                });
                Cleanup();
            }
            catch (Exception ex)
            {
                MainThreadDispatcher.RunOnMainThread(() =>
                {
                    PopupManager.Instance.ShowMessage($"Connection error: {ex.Message}", PopupMessageType.System);
                });
                Cleanup();
            }
            finally
            {
                _ = BeginReceive();
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
    }
}
