using Assets.Scripts.Network;

namespace Assets.Scripts.Entity.Abstractions
{
    public interface IPlayer
    {
        WorldClient Client { get; set; }
    }
}
