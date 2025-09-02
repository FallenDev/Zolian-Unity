using System;

using FishNet.Broadcast;

namespace Assets.Scripts.Network.FishNetPro.Auth
{
    /// <summary>
    /// Sent immediately after connecting to the server, before any player exists. 
    /// </summary>
    public struct AuthTicketBroadcast : IBroadcast
    {
        public string Ticket;      // Steam (or other) ticket from your Auth server, base64-encoded
        public long SteamId;       // 64-bit SteamID
        public Guid AccountId;     // optional
        public Guid CharacterId;   // optional (selected character to spawn)
    }

    /// <summary>
    /// Sent by the client after auth passes, to enter the world.
    /// </summary>
    public struct EnterGameRequest : IBroadcast
    {
        public Guid Serial;
        public string UserName;
    }
}