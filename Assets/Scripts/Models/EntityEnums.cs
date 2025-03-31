using System;

namespace Assets.Scripts.Models
{
    [Flags]
    public enum Element
    {
        None = 0x00,
        Fire = 0x01,
        Water = 0x02,
        Wind = 0x03,
        Earth = 0x04,
        Holy = 0x05,
        Void = 0x06,
        Rage = 0x07,
        Terror = 0x08,
        Sorrow = 0x09,
        Chaos = 0x0A
    }

    public static class ElementEnums
    {
        public static bool ElementFlagIsSet(this Element self, Element flag) => (self & flag) == flag;
    }
}
