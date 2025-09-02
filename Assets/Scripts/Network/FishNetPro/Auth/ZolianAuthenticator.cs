using Assets.Scripts.Network.PacketArgs.SendToServer;

using FishNet.Authenticating;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Server;

using System;
using System.Threading.Tasks;
using FishNet.Transporting;
using UnityEngine;

namespace Assets.Scripts.Network.FishNetPro.Auth
{
    /// Custom authenticator:
    /// - Receives AuthTicketBroadcast (pre-spawn), validates against your Auth-Server.
    /// - On success -> PassAuthentication; on failure -> disconnect.
    /// - After auth, handles EnterGameRequest and calls your existing server flow.
    public class ZolianAuthenticator : Authenticator
    {
        private NetworkManager _nm;
        private ZolianWorldServer _worldServer;

        // Your FishNet Pro build requires implementing this abstract event.
        public override event Action<NetworkConnection, bool> OnAuthenticationResult;

        public override void InitializeOnce(NetworkManager networkManager)
        {
            base.InitializeOnce(networkManager);
            _nm = networkManager;

            _worldServer = UnityEngine.Object.FindFirstObjectByType<ZolianWorldServer>();
            if (_worldServer == null)
                Debug.LogWarning("ZolianAuthenticator: ZolianWorldServer not found yet.");

            // Use single-parameter overload for widest FishNet compatibility.
            _nm.ServerManager.RegisterBroadcast<AuthTicketBroadcast>(OnAuthTicketReceived);
            _nm.ServerManager.RegisterBroadcast<EnterGameRequest>(OnEnterGameReceived);
        }

        private async void OnAuthTicketReceived(NetworkConnection conn, AuthTicketBroadcast msg, Channel channel)
        {
            if (conn == null || !conn.IsActive)
                return;

            bool ok = false;
            string reason = null;

            try
            {
                ok = await ValidateWithAuthServerAsync(msg);
                if (!ok) reason = "Invalid or expired ticket.";
            }
            catch (Exception ex)
            {
                reason = "Auth server error: " + ex.Message;
            }

            if (ok)
            {
                //PassAuthentication(conn);
                OnAuthenticationResult?.Invoke(conn, true);
            }
            else
            {
                // Your build doesn’t expose FailAuthentication, so disconnect.
                _nm.ServerManager.StopConnection(true);
                OnAuthenticationResult?.Invoke(conn, false);
                if (!string.IsNullOrEmpty(reason))
                    Debug.LogWarning($"Auth failed: {reason}");
            }
        }

        private void OnEnterGameReceived(NetworkConnection conn, EnterGameRequest req, Channel channel)
        {
            if (conn == null || !conn.IsActive) return;

            if (!conn.Authenticated)
            {
                _nm.ServerManager.StopConnection(true);
                OnAuthenticationResult?.Invoke(conn, false);
                return;
            }

            if (_worldServer == null)
            {
                Debug.LogError("ZolianAuthenticator: WorldServer missing; cannot enter game.");
                return;
            }

            var args = new EnterGameArgs
            {
                Serial = req.Serial,
                UserName = req.UserName
            };

            _worldServer.HandleClientEnterGame(conn, args);
        }

        // TODO: Replace with real call into your .NET Auth-Server (Steam ticket validation).
        private Task<bool> ValidateWithAuthServerAsync(AuthTicketBroadcast msg)
        {
            // Minimal sanity gate until hooked to the Auth-Server.
            return Task.FromResult(!string.IsNullOrWhiteSpace(msg.Ticket) && msg.SteamId != 0);
        }
    }
}
