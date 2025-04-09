using Assets.Scripts.Network;
using JohnStairs.RCC.Character;

namespace Assets.Scripts.Entity.Abstractions
{
    public interface IPlayer : ICharacter
    {
        WorldClient Client { get; set; }
        bool LockedOnTarget();
    }
}
