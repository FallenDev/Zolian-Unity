using System;
using Assets.Scripts.Models;
using UnityEngine;

namespace Assets.Scripts.Entity
{
    public class Entity : MonoBehaviour
    {
        public Guid Serial { get; set; }
        public uint CurrentZoneId { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }
        public Vector3 Position { get; set; }
        public uint EntityLevel { get; set; }

        #region Stats

        // Health
        public long BaseHp { get; set; }
        public long BonusHp { get; set; }
        public long GearedHp { get; set; }
        public long CalculatedMaxHp => Math.Clamp(BaseHp + BonusHp + GearedHp, 0, long.MaxValue);
        public long CurrentHp { get; set; } // Live Value

        // Mana
        public long BaseMp { get; set; }
        public long BonusMp { get; set; }
        public long GearedMp { get; set; }
        public long CalculatedMaxMp => Math.Clamp(BaseMp + BonusMp + GearedMp, 0, long.MaxValue);
        public long CurrentMp { get; set; } // Live Value

        // Stamina
        public uint BaseStamina { get; set; }
        public uint BonusStamina { get; set; }
        public uint GearedStamina { get; set; }
        public uint CalculatedMaxStamina => Math.Clamp(BaseStamina + BonusStamina + GearedStamina, 0, uint.MaxValue);
        public uint CurrentStamina { get; set; } // Live Value

        // Rage
        public uint BaseRage { get; set; }
        public uint BonusRage { get; set; }
        public uint GearedRage { get; set; }
        public uint CalculatedMaxRage => Math.Clamp(BaseRage + BonusRage + GearedRage, 0, uint.MaxValue);
        public uint CurrentRage { get; set; } // Live Value

        // Regeneration
        public uint BaseRegen { get; set; }
        public uint BonusRegen { get; set; }
        public uint GearedRegen { get; set; }
        public uint Regen => Math.Clamp(BaseRegen + BonusRegen + GearedRegen, 0, uint.MaxValue);

        // Damage
        public uint BaseDmg { get; set; }
        public uint BonusDmg { get; set; }
        public uint GearedDmg { get; set; }
        public uint Dmg => Math.Clamp(BaseDmg + BonusDmg + GearedDmg, 0, uint.MaxValue);

        // Hit (Reflex Saving Throw)
        private uint BaseHit { get; set; } = 5;
        public int BonusHit { get; set; }
        public double Reflex => Math.Min(Math.Round((BaseHit + Dex + BonusHit) * 0.08, 2), 70); // Hard Cap 70%

        // Fortitude (Fortitude Saving Throw)
        private uint BaseFortitude { get; set; } = 5;
        public int BonusFortitude { get; set; }
        public double Fortitude => Math.Min(Math.Round((BaseFortitude + Con + BonusFortitude) * 0.1, 2), 85); // Hard Cap 85%

        // Will (Will Saving Throw)
        private uint BaseMagicResistance { get; set; } = 5;
        public int BonusMr { get; set; }
        public double Will => Math.Min(Math.Round((BaseMagicResistance + Wis + BonusMr) * 0.1, 2), 80); // Hard Cap 80%

        // Armor - How much damage you resist from all attacks (Physical, Magical, etc)
        private int AcFromStats => Math.Clamp((Wis / 20 + Con / 20), 0, int.MaxValue);
        public int BonusAc { get; set; }
        public int GearedAc { get; set; }
        private int CalculatedAc => AcFromStats + GearedAc + BonusAc;
        private int Ac => Math.Clamp(CalculatedAc, -100, int.MaxValue);

        // Sealed Logic - SealedModifier is a double value that can be set to increase or decrease the Ac value
        public double SealedModifier { get; set; }
        public int ArmorClass
        {
            get
            {
                switch (Ac)
                {
                    case > 0:
                        if (SealedModifier == 0) return Ac;
                        return (int)(Ac * SealedModifier);
                    case <= 0:
                        if (SealedModifier == 0) return Ac;
                        return Ac - (int)(Math.Abs(Ac) * SealedModifier);
                }
            }
        }

        // Amplified Damage - Buff that amplifies Entities elemental status
        public double Amplified { get; set; }
        public Element OffenseElement { get; set; }
        public Element SecondaryOffensiveElement { get; set; }
        public Element DefenseElement { get; set; }
        public Element SecondaryDefensiveElement { get; set; }

        // Strength
        public int BaseStr { get; set; }
        public int CarryingStr => BaseStr;
        public int BonusStr { get; set; }
        public int GearedStr { get; set; }
        public int Str => BaseStr + BonusStr + GearedStr;

        // Intelligence
        public int BaseInt { get; set; }
        public int BonusInt { get; set; }
        public int GearedInt { get; set; }
        public int Int => BaseInt + BonusInt + GearedInt;

        // Wisdom
        public int BaseWis { get; set; }
        public int BonusWis { get; set; }
        public int GearedWis { get; set; }
        public int Wis => BaseWis + BonusWis + GearedWis;

        // Constitution
        public int BaseCon { get; set; }
        public int BonusCon { get; set; }
        public int GearedCon { get; set; }
        public int Con => BaseCon + BonusCon + GearedCon;

        // Dexterity
        public int BaseDex { get; set; }
        public int BonusDex { get; set; }
        public int GearedDex { get; set; }
        public int Dex => BaseDex + BonusDex + GearedDex;

        // Luck
        public int BaseLuck { get; set; }
        public int BonusLuck { get; set; }
        public int GearedLuck { get; set; }
        public int Luck => BaseLuck + BonusLuck + GearedLuck;

        #endregion

        private void Start()
        {

        }

        private void Update()
        {
            Position = new Vector3(PositionX, PositionY, PositionZ);
        }

        public Entity()
        {

        }
    }
}

