namespace Assets.Scripts.Network.OpCodes
{
    public enum ServerOpCode : byte
    {
        ConnectionInfo = 0x00,
        PlayerList = 0x01,
        LoginMessage = 0x02,
        CreateCharacterFinalized = 0x03,
        ServerMessage = 0x0A,
        RemoveEntity = 0x0E,
        Sound = 0x19,
        AcceptConnection = 0x7E
    }

    public enum ClientOpCode : byte
    {
        Login = 0x01,
        Version = 0x0A,
        ClientRedirected = 0x0B,
        CreateCharacter = 0x0C,
        DeleteCharacter = 0x0D,
    }
}
