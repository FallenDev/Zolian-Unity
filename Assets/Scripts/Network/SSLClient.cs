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
using Assets.Scripts.Network.Converters;
using Assets.Scripts.Network.OpCodes;
using Assets.Scripts.Network.PacketArgs;

namespace Assets.Scripts.Network
{
    public class SSLClient : MonoBehaviour
    {
        private bool isConnected;
        private SslStream sslStream;
        private TcpClient tcpClient;
        private StreamReader reader;
        private StreamWriter writer;
        private readonly Dictionary<byte, IPacketConverter> Converters = new();

        public string serverIp = "127.0.0.1"; // Server IP address
        public int serverPort = 4200; // Server port

        private void Start()
        {
            Debug.Log("Attempting to connect...");
            IndexConverters();
            ConnectToServer();
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

        #region Client -> Server

        private void SendTestData()
        {
            // Send a byte
            SendDataToServer(new byte[] { 0xFF }, 0x02); // Example OpCode: 0x02

            // Send an int
            SendDataToServer(BitConverter.GetBytes(123456), 0x03); // Example OpCode: 0x03

            // Send a long
            SendDataToServer(BitConverter.GetBytes(123456789012345L), 0x04); // Example OpCode: 0x04

            // Send a ulong
            SendDataToServer(BitConverter.GetBytes(9876543210987654321UL), 0x05); // Example OpCode: 0x05

            // Send a float
            SendDataToServer(BitConverter.GetBytes(123.45f), 0x06); // Example OpCode: 0x06

            // Send a double
            SendDataToServer(BitConverter.GetBytes(12345.6789), 0x07); // Example OpCode: 0x07

            // Send a bool (as a byte)
            SendDataToServer(new byte[] { true ? (byte)1 : (byte)0 }, 0x08); // Example OpCode: 0x08
        }

        private void SendPacket(byte opCode, Span<byte> payload)
        {
            try
            {
                var packet = new Packet(opCode, payload);
                var data = packet.ToArray();
                sslStream.Write(data, 0, data.Length);
                Debug.Log($"Packet sent to server: {BitConverter.ToString(data).Replace("-", " ")}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to send packet: {ex.Message}");
            }
        }

        private void SendMessageToServer(string message)
        {
            var payload = Encoding.UTF8.GetBytes(message);
            SendPacket(0x01, payload); // OpCode for string messages
        }

        private void SendDataToServer(byte[] data, byte opCode)
        {
            SendPacket(opCode, data);
        }

        #endregion

        #region Server -> Client

        private async void BeginReceive()
        {
            try
            {
                byte[] buffer = new byte[ushort.MaxValue]; // Adjust size as needed
                while (isConnected)
                {
                    int bytesRead = await sslStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);

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

                if (Converters.TryGetValue(packet.OpCode, out var converter))
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
                    var acceptArgs = (AcceptConnectionArgs)args;
                    Debug.Log($"Accept connection message: {acceptArgs.Message}");
                    break;
                case (byte)ServerOpCode.LoginMessage: // Login message
                    var loginArgs = (LoginMessageArgs)args;
                    Debug.Log($"Login message: {loginArgs.LoginMessageType} - {loginArgs.Message}");
                    break;
                default:
                    Debug.LogWarning($"Unhandled OpCode: {opCode}");
                    break;
            }
        }

        #endregion

        #region Configuration

        private void IndexConverters()
        {
            Converters.Add((byte)ServerOpCode.AcceptConnection, new AcceptConnectionConverter());
            Converters.Add((byte)ServerOpCode.LoginMessage, new LoginMessageConverter());
            Converters.Add((byte)ServerOpCode.RemoveEntity, new RemoveEntityConverter());
            Converters.Add((byte)ServerOpCode.ServerMessage, new ServerMessageConverter());
            Converters.Add((byte)ServerOpCode.Sound, new SoundConverter());
        }

        private void ConnectToServer()
        {
            try
            {
                // Initialize the TCP client and SSL stream
                tcpClient = new TcpClient(serverIp, serverPort);
                ConfigureTcpSocket(tcpClient.Client);
                sslStream = new SslStream(tcpClient.GetStream(), false, ValidateServerCertificate);

                // Perform SSL handshake
                sslStream.AuthenticateAsClient("localhost", null, SslProtocols.Tls12, false);

                Debug.Log("Connected securely to the server.");

                // Initialize reader and writer for the stream
                writer = new StreamWriter(sslStream) { AutoFlush = true };
                reader = new StreamReader(sslStream);

                isConnected = true;

                // Send an initial message to the server
                SendMessageToServer("Decoding string messages still works, now attempting to send test data byte arrays!");
                SendTestData();
            }
            catch (AuthenticationException ex)
            {
                Debug.LogError($"SSL Authentication failed: {ex.Message}");
                Cleanup();
            }
            catch (SocketException ex)
            {
                Debug.LogError($"SocketException: {ex.Message}");
                Debug.LogError($"Ensure the server is running at {serverIp}:{serverPort}.");
                Cleanup();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Connection error: {ex.Message}");
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

                Debug.LogError($"Certificate validation failed: {sslPolicyErrors}");
                return false;
            }

            Debug.LogError("The provided certificate is not an X509Certificate2.");
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
