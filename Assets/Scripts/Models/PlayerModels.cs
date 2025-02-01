using System;

namespace Assets.Scripts.Models
{
    [Flags]
    public enum BaseClass : byte
    {
        Peasant = 0,
        Berserker = 1,
        Defender = 2,
        Assassin = 3,
        Cleric = 4,
        Arcanus = 5,
        Monk = 6,
        DualBash = 7,
        DualCast = 8,
        Racial = 9,
        Monster = 10,
        Quest = 11
    }

    [Flags]
    public enum JobClass
    {
        None = 0,
        Thief = 1,
        DarkKnight = 1 << 1,
        Templar = 1 << 2,
        Knight = 1 << 3,
        Ninja = 1 << 4,
        SharpShooter = 1 << 5,
        Oracle = 1 << 6,
        Bard = 1 << 7,
        Summoner = 1 << 8,
        Samurai = 1 << 9,
        ShaolinMonk = 1 << 10,
        Necromancer = 1 << 11,
        Dragoon = 1 << 12
    }

    public static class PlayerModels
    {
        public static bool BaseClassFlagIsSet(this BaseClass self, BaseClass flag) => (self & flag) == flag;
        public static bool JobClassFlagIsSet(this JobClass self, JobClass flag) => (self & flag) == flag;
    }
}
