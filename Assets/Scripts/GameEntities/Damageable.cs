namespace Assets.Scripts.GameEntities
{
    public class Damageable : Movable
    {
        // Immunities
        public bool FireImmunity { get; set; }
        public bool WaterImmunity { get; set; }
        public bool EarthImmunity { get; set; }
        public bool WindImmunity { get; set; }
        public bool LightImmunity { get; set; }
        public bool DarkImmunity { get; set; }

        // Status Immunities
        public bool PoisonImmunity { get; set; }
        public bool CharmImmunity { get; set; }
        public bool StunImmunity { get; set; }
        public bool SleepImmunity { get; set; }

        public bool IsAlive => CurrentHp >= 1;
        public bool IsInvisible { get; set; }
    }
}
