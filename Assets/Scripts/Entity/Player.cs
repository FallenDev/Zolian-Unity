using System;
using Assets.Scripts.Models;

namespace Assets.Scripts.Entity
{
    public class Player
    {
        // Character Selection Properties
        public Guid Serial { get; set; }
        public bool Disabled { get; set; }
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
