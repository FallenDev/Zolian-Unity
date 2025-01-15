namespace Assets.Scripts.Network.OpCodes
{
    public enum ServerOpCode : byte
    {
        /// <summary>
        ///     OpCode used to send the encryption details and checksum of the details of available login servers
        ///     <br />
        ///     Hex value: 0x00
        /// </summary>
        ConnectionInfo = 0,

        /// <summary>
        ///     OpCode used to send a message to a client on the login server
        ///     <br />
        ///     Hex value: 0x02
        /// </summary>
        LoginMessage = 2,

        /// <summary>
        ///     OpCode used to redirect a client to another server
        ///     <br />
        ///     Hex value: 0x03
        /// </summary>
        Redirect = 3,

        /// <summary>
        ///     OpCode used to send a client it's location
        ///     <br />
        ///     Hex value: 0x04
        /// </summary>
        Location = 4,

        /// <summary>
        ///     OpCode used to send a client it's id
        ///     <br />
        ///     Hex value: 0x05
        /// </summary>
        UserId = 5,

        /// <summary>
        ///     OpCode used to send a client all non-aisling objects in it's viewport
        ///     <br />
        ///     Hex value: 0x07
        /// </summary>
        DisplayVisibleEntities = 7,

        /// <summary>
        ///     OpCode used to send a client it's attributes
        ///     <br />
        ///     Hex value: 0x08
        /// </summary>
        Attributes = 8,

        /// <summary>
        ///     OpCode used to send a client a non-public message
        ///     <br />
        ///     Hex value: 0x0A
        /// </summary>
        ServerMessage = 10,

        /// <summary>
        ///     OpCode used to respond to a client's request to walk
        ///     <br />
        ///     Hex value: 0x0B
        /// </summary>
        ClientWalkResponse = 11,

        /// <summary>
        ///     OpCode used to send a client another creature's walk
        ///     <br />
        ///     Hex value: 0x0C
        /// </summary>
        CreatureWalk = 12,

        /// <summary>
        ///     OpCode used to send a client a public message
        ///     <br />
        ///     Hex value: 0x0D
        /// </summary>
        DisplayPublicMessage = 13,

        /// <summary>
        ///     OpCode used to remove an object from the client's viewport
        ///     <br />
        ///     Hex value: 0x0E
        /// </summary>
        RemoveEntity = 14,

        /// <summary>
        ///     OpCode used to add an item to the client's inventory
        ///     <br />
        ///     Hex value: 0x0F
        /// </summary>
        AddItemToPane = 15,

        /// <summary>
        ///     OpCode used to remove an item from the client's inventory
        ///     <br />
        ///     Hex value: 0x10
        /// </summary>
        RemoveItemFromPane = 16,

        /// <summary>
        ///     OpCode used to send a client another creature's turn
        ///     <br />
        ///     Hex value: 0x11
        /// </summary>
        CreatureTurn = 17,

        /// <summary>
        ///     OpCode used to display a creature's health bar
        ///     <br />
        ///     Hex value: 0x13
        /// </summary>
        HealthBar = 19,

        /// <summary>
        ///     OpCode used to send map details to a client
        ///     <br />
        ///     Hex value: 0x15
        /// </summary>
        MapInfo = 21,

        /// <summary>
        ///     OpCode used to add a spell to the client's spellbook
        ///     <br />
        ///     Hex value: 0x17
        /// </summary>
        AddSpellToPane = 23,

        /// <summary>
        ///     OpCode used to remove a spell from the client's spellbook
        ///     <br />
        ///     Hex value: 0x18
        /// </summary>
        RemoveSpellFromPane = 24,

        /// <summary>
        ///     OpCode used to tell the client to play a sound
        ///     <br />
        ///     Hex value: 0x19
        /// </summary>
        Sound = 25,

        /// <summary>
        ///     OpCode used to animate a creature's body
        ///     <br />
        ///     Hex value: 0x1A
        /// </summary>
        BodyAnimation = 26,

        /// <summary>
        ///     OpCode used to associate text with an object
        ///     <br />
        ///     Hex value: 0x1B
        /// </summary>
        Notepad = 27,

        /// <summary>
        ///     OpCode used to signal to the client that a map change operation has completed
        ///     <br />
        ///     Hex value: 0x1F
        /// </summary>
        MapChangeComplete = 31,

        /// <summary>
        ///     OpCode used to change the light level of the map
        ///     <br />
        ///     Hex value: 0x20
        /// </summary>
        LightLevel = 32,

        /// <summary>
        ///     OpCode used to respond to a client's request to refresh the viewport
        ///     <br />
        ///     Hex value: 0x22
        /// </summary>
        RefreshResponse = 34,

        /// <summary>
        ///     OpCode used to play an animation on a point or object
        ///     <br />
        ///     Hex value: 0x29
        /// </summary>
        Animation = 41,

        /// <summary>
        ///     OpCode used to add a skill to a client's skillbook
        ///     <br />
        ///     Hex value: 0x2C
        /// </summary>
        AddSkillToPane = 44,

        /// <summary>
        ///     OpCode used to remove a skill from a client's skillbook
        ///     <br />
        ///     Hex value: 0x2D
        /// </summary>
        RemoveSkillFromPane = 45,

        /// <summary>
        ///     OpCode used to transition a client to the world map
        ///     <br />
        ///     Hex value: 0x2E
        /// </summary>
        WorldMap = 46,

        /// <summary>
        ///     OpCode used to display a merchant menu to a client
        ///     <br />
        ///     Hex value: 0x2F
        /// </summary>
        DisplayMenu = 47,

        /// <summary>
        ///     OpCode used to display a dialog to a client
        ///     <br />
        ///     Hex value: 0x30
        /// </summary>
        DisplayDialog = 48,

        /// <summary>
        ///     OpCode used to display a board to a client
        ///     <br />
        ///     Hex value: 0x31
        /// </summary>
        DisplayBoard = 49,

        /// <summary>
        ///     OpCode used to give details of nearby doors to a client
        ///     <br />
        ///     Hex value: 0x32
        /// </summary>
        Door = 50,

        /// <summary>
        ///     OpCode used to display an aisling to a client
        ///     <br />
        ///     Hex value: 0x33
        /// </summary>
        DisplayAisling = 51,

        /// <summary>
        ///     OpCode used to display an aisling's profile to a client
        ///     <br />
        ///     Hex value: 0x34
        /// </summary>
        OtherProfile = 52,

        /// <summary>
        ///     OpCode used to display the world list to a client
        ///     <br />
        ///     Hex value: 0x36
        /// </summary>
        WorldList = 54,

        /// <summary>
        ///     OpCode used to send a client a change in an equipment slot
        ///     <br />
        ///     Hex value: 0x37
        /// </summary>
        Equipment = 55,

        /// <summary>
        ///     OpCode used to send a client a removal from an equipment slot
        ///     <br />
        ///     Hex value: 0x38
        /// </summary>
        DisplayUnequip = 56,

        /// <summary>
        ///     OpCode used to send a client it's own profile
        ///     <br />
        ///     Hex value: 0x39
        /// </summary>
        SelfProfile = 57,

        /// <summary>
        ///     OpCode used to display an effect on the bar on the right hand side of the viewport
        ///     <br />
        ///     Hex value: 0x3A
        /// </summary>
        Effect = 58,

        /// <summary>
        ///     OpCode used to respond to a client's heartbeat
        ///     <br />
        ///     Hex value: 0x3B
        /// </summary>
        HeartBeatResponse = 59,

        /// <summary>
        ///     OpCode used to send a client tile data for a map
        ///     <br />
        ///     Hex value: 0x3C
        /// </summary>
        MapData = 60,

        /// <summary>
        ///     OpCode used to send a skill or spell cooldown to a client
        ///     <br />
        ///     Hex value: 0x3F
        /// </summary>
        Cooldown = 63,

        /// <summary>
        ///     OpCode used to send data displayed int an exchange window to a client
        ///     <br />
        ///     Hex value: 0x42
        /// </summary>
        DisplayExchange = 66,

        /// <summary>
        ///     OpCode used to tell a client to cancel a spellcast
        ///     <br />
        ///     Hex value: 0x48
        /// </summary>
        CancelCasting = 72,

        /// <summary>
        ///     OpCode used to request profile details from a client
        ///     <br />
        ///     Hex value: 0x49
        /// </summary>
        EditableProfileRequest = 73,

        /// <summary>
        ///     OpCode used to force a client to send back a specified packet
        ///     <br />
        ///     Hex value: 0x4B
        /// </summary>
        ForceClientPacket = 75,

        /// <summary>
        ///     OpCode used to send a client confirmation of a request to exit to the login server
        ///     <br />
        ///     Hex value: 0x4C
        /// </summary>
        ExitResponse = 76,

        /// <summary>
        ///     OpCode used to send a client a list of available servers
        ///     <br />
        ///     Hex value: 0x56
        /// </summary>
        ServerTableResponse = 86,

        /// <summary>
        ///     OpCode used to signal a client that it has finished sending map data
        ///     <br />
        ///     Hex value: 0x58
        /// </summary>
        MapLoadComplete = 88,

        /// <summary>
        ///     OpCode used to send a client the EULA / login notice
        ///     <br />
        ///     Hex value: 0x60
        /// </summary>
        LoginNotice = 96,

        /// <summary>
        ///     OpCode used to send a client a group request
        ///     <br />
        ///     Hex value: 0x63
        /// </summary>
        DisplayGroupInvite = 99,

        /// <summary>
        ///     OpCode used to send a client data for the login screen
        ///     <br />
        ///     Hex value: 0x66
        /// </summary>
        LoginControl = 102,

        /// <summary>
        ///     OpCode used to signal a client that it is changing maps
        ///     <br />
        ///     Hex value: 0x67
        /// </summary>
        MapChangePending = 103,

        /// <summary>
        ///     OpCode used to synchronize the server's ticks with a client's
        ///     <br />
        ///     Hex value: 0x68
        /// </summary>
        SynchronizeTicksResponse = 104,

        /// <summary>
        ///     OpCode used to send metadata data to a client
        ///     <br />
        ///     Hex value: 0x6F
        /// </summary>
        MetaData = 111,

        /// <summary>
        ///     OpCode sent to a client to confirm their initial connection
        ///     <br />
        ///     Hex value: 0x7E
        /// </summary>
        AcceptConnection = 126
    }
}
