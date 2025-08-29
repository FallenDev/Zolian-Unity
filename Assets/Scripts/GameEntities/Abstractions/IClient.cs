using Assets.Scripts.Network;

namespace Assets.Scripts.GameEntities.Abstractions
{
    /// <summary>
    /// The client instance that is used to send and receive packets.
    /// </summary>
    /// <remarks>
    /// This property is used to access the client instance from other classes.
    /// </remarks>
    /// <value>The client instance.</value>
    /// <returns>The client instance.</returns>
    public interface IClient
    {
        WorldClient Client { get; set; }
    }
}
