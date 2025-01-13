using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using System.Security.Authentication;
using System.Text;

public class SSLClient : MonoBehaviour
{
    private SslStream sslStream;
    private TcpClient tcpClient;
    private StreamReader reader;
    private StreamWriter writer;

    public string serverIp = "127.0.0.1"; // Server IP address
    public int serverPort = 4200; // Server port
    private bool isConnected;

    void Start()
    {
        Debug.Log("Attempting to connect...");
        ConnectToServer();
    }

    void ConnectToServer()
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
            SendMessageToServer("Hello, secure server!");
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
    }

    void Update()
    {
        if (isConnected && tcpClient.Connected)
        {
            // Attempt to read from the server
            ReceiveMessageFromServer();
        }
    }

    private void SendMessageToServer(string message)
    {
        try
        {
            // Create the payload
            byte[] payload = Encoding.UTF8.GetBytes(message);

            ushort length = (ushort)payload.Length; // Payload size only

            byte[] packet = new byte[5 + length];
            packet[0] = 0x16; // Signature
            packet[1] = (byte)((length >> 8) & 0xFF); // High byte of length (big-endian)
            packet[2] = (byte)(length & 0xFF);        // Low byte of length
            packet[3] = 0x01; // OpCode
            packet[4] = 0x00; // Sequence
            Array.Copy(payload, 0, packet, 5, payload.Length);

            // Send the packet
            sslStream.Write(packet);
            Debug.Log($"Packet sent to server: {BitConverter.ToString(packet).Replace("-", " ")}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to send packet: {ex.Message}");
        }
    }

    private void ReceiveMessageFromServer()
    {
        try
        {
            // Read a single line from the server (blocking call)
            if (tcpClient.GetStream().CanRead)
            {
                string response = reader.ReadLine();
                if (!string.IsNullOrEmpty(response))
                {
                    Debug.Log($"Server: {response}");
                }
            }
        }
        catch (IOException ex)
        {
            Debug.LogError($"Error reading from server: {ex.Message}");
        }
    }

    void OnApplicationQuit()
    {
        Cleanup();
    }

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
        tcpSocket.ReceiveTimeout = 5000;
        tcpSocket.SendTimeout = 5000;
        tcpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
    }
}
