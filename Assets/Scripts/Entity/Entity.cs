using System;
using Assets.Scripts.Models;
using UnityEngine;

namespace Assets.Scripts.Entity
{
    public class Entity : MonoBehaviour
    {
        public Guid Serial { get; set; }
        public uint CurrentZoneId { get; set; }
        public Vector3 Position { get; set; }
        public uint EntityLevel { get; set; }

        // Health & Mana
        public long CurrentHp { get; set; }
        public long MaxHp { get; set; }
        public long CurrentMp { get; set; }
        public long MaxMp { get; set; }

        // Resources
        public uint CurrentStamina { get; set; }
        public uint MaxStamina { get; set; }
        public uint CurrentRage { get; set; }
        public uint MaxRage { get; set; }

        // Saving Throws
        public double Reflex { get; set; }
        public double Fortitude { get; set; }
        public double Will { get; set; }

        // Armor - How much damage you resist from all attacks (Physical, Magical, etc)
        public int ArmorClass { get; set; }
        public Element OffenseElement { get; set; }
        public Element SecondaryOffensiveElement { get; set; }
        public Element DefenseElement { get; set; }
        public Element SecondaryDefensiveElement { get; set; }

        // Stats
        public int Str { get; set; }
        public int Int { get; set; }
        public int Wis { get; set; }
        public int Con { get; set; }
        public int Dex { get; set; }
        public int Luck { get; set; }
        public uint Regen { get; set; }
        public uint Dmg { get; set; }

        private void Start()
        {

        }

        private void Update()
        {

        }

        public Entity()
        {

        }
    }
}

