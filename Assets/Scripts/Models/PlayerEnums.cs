using System;

namespace Assets.Scripts.Models
{
    [Flags]
    public enum BaseClass : byte
    {
        Berserker = 1,
        Defender = 2,
        Assassin = 3,
        Cleric = 4,
        Arcanus = 5,
        Monk = 6,
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

    [Flags]
    public enum Race
    {
        UnDecided = 0,
        Human = 1,
        HalfElf = 2,
        HighElf = 3,
        DarkElf = 4,
        WoodElf = 5,
        Orc = 6,
        Dwarf = 7,
        Halfling = 8,
        Dragonkin = 9,
        HalfBeast = 10,
        Merfolk = 11
    }

    [Flags]
    public enum Sex
    {
        Male = 0,
        Female = 1
    }

    public static class PlayerEnums
    {
        public static bool BaseClassFlagIsSet(this BaseClass self, BaseClass flag) => (self & flag) == flag;
        public static bool JobClassFlagIsSet(this JobClass self, JobClass flag) => (self & flag) == flag;
        public static bool RaceFlagIsSet(this Race self, Race flag) => (self & flag) == flag;
        public static bool SexFlagIsSet(this Sex self, Sex flag) => (self & flag) == flag;
    }
}
