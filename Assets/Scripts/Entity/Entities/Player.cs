using Assets.Scripts.Models;
using Assets.Scripts.Network;

namespace Assets.Scripts.Entity.Entities
{
    public class Player : Damageable
    {
        public WorldClient Client { get; set; }
        public string Name { get; set; }
        public uint Level { get; set; }
        public BaseClass BaseClass { get; set; }
        public BaseClass AdvClass { get; set; }
        public JobClass Job { get; set; }
        public long Health { get; set; }
        public long Mana { get; set; }

        // In-Game Properties
        // Add any additional properties needed for in-game representation


        private Player()
        {
            // Initialize any default values if needed
        }
    }
}
