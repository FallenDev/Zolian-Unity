namespace Assets.Scripts.Network.PacketArgs
{
    /// <summary>
    ///     Represents the serialization of the <see cref="ServerOpCode.LoginMessage" /> packet
    /// </summary>
    public sealed record LoginMessageArgs : IPacketSerializable
    {
        /// <summary>
        ///     The type of login message to be used
        /// </summary>
        public LoginMessageType LoginMessageType { get; set; }

        /// <summary>
        ///     If the login message type can have a custom message, this will be the message displayed.
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    ///     Enum for login message types
    /// </summary>
    public enum LoginMessageType : byte
    {
        /// <summary>
        ///     A generic confirmation window with an ok button
        /// </summary>
        Confirm = 0,

        /// <summary>
        ///     Clears the name field during character creation and presents a message with an ok button
        /// </summary>
        ClearNameMessage = 3,

        /// <summary>
        ///     Clears the password field during character creation and presents a message with an ok button
        /// </summary>
        ClearPswdMessage = 5,

        /// <summary>
        ///     Clears the name and password fields on the login screen and presents a message with an ok button
        /// </summary>
        CharacterDoesntExist = 14,

        /// <summary>
        ///     Clears the password fields on the login screen and presents a message with an ok button
        /// </summary>
        WrongPassword = 15
    }
}