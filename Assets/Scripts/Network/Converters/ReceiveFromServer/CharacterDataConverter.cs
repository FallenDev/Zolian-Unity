using System;

using Assets.Scripts.Models;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using Assets.Scripts.Network.PacketHandling;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.ReceiveFromServer
{
    public sealed class CharacterDataConverter : PacketConverterBase<CharacterDataArgs>
    {
        protected override CharacterDataArgs Deserialize(ref SpanReader reader)
        {
            var characterData = new CharacterDataArgs();
            var type = reader.ReadString();
            Enum.TryParse(type, out PlayerUpdateType playerType);
            characterData.Type = playerType;

            if (playerType.ServerUpdateTypeFlagIsSet(PlayerUpdateType.FullSend))
            {
                characterData.Serial = reader.ReadGuid();
                characterData.Disabled = reader.ReadBoolean();
                characterData.UserName = reader.ReadString();
                var stage = reader.ReadString();
                Enum.TryParse(stage, out ClassStage playerStage);
                characterData.Stage = playerStage;
                var job = reader.ReadString();
                Enum.TryParse(job, out JobClass playerJob);
                characterData.Job = playerJob;
                var firstClass = reader.ReadString();
                Enum.TryParse(firstClass, out BaseClass playerClassOne);
                characterData.FirstClass = playerClassOne;
                var secondClass = reader.ReadString();
                Enum.TryParse(secondClass, out BaseClass playerClassTwo);
                characterData.SecondClass = playerClassTwo;
                characterData.EntityLevel = reader.ReadUInt32();
                characterData.JobLevel = reader.ReadUInt32();
                characterData.GameMaster = reader.ReadBoolean();
                characterData.Position = reader.ReadVector3();
                characterData.CameraYaw = reader.ReadFloat();
                characterData.CurrentHealth = reader.ReadInt64();
                characterData.MaxHealth = reader.ReadInt64();
                characterData.CurrentMana = reader.ReadInt64();
                characterData.MaxMana = reader.ReadInt64();
                characterData.CurrentStamina = reader.ReadUInt32();
                characterData.MaxStamina = reader.ReadUInt32();
                characterData.CurrentRage = reader.ReadUInt32();
                characterData.MaxRage = reader.ReadUInt32();
                characterData.Regen = reader.ReadUInt32();
                characterData.Dmg = reader.ReadUInt32();
                characterData.Reflex = reader.ReadDouble();
                characterData.Fortitude = reader.ReadDouble();
                characterData.Will = reader.ReadDouble();
                characterData.ArmorClass = reader.ReadInt32();
                var offEle = reader.ReadString();
                Enum.TryParse(offEle, out Element offenseElement);
                characterData.OffenseElement = offenseElement;
                var defEle = reader.ReadString();
                Enum.TryParse(defEle, out Element defenseElement);
                characterData.DefenseElement = defenseElement;
                var secOffEle = reader.ReadString();
                Enum.TryParse(secOffEle, out Element secondaryOffenseElement);
                characterData.SecondaryOffenseElement = secondaryOffenseElement;
                var secDefEle = reader.ReadString();
                Enum.TryParse(secDefEle, out Element secondaryDefenseElement);
                characterData.SecondaryDefenseElement = secondaryDefenseElement;
                characterData.Str = reader.ReadInt32();
                characterData.Int = reader.ReadInt32();
                characterData.Wis = reader.ReadInt32();
                characterData.Con = reader.ReadInt32();
                characterData.Dex = reader.ReadInt32();
                characterData.Luck = reader.ReadInt32();
                var race = reader.ReadString();
                Enum.TryParse(race, out Race playerRace);
                characterData.Race = playerRace;
                var sex = reader.ReadString();
                Enum.TryParse(sex, out Sex playerSex);
                characterData.Sex = playerSex;
                characterData.Hair = reader.ReadInt16();
                characterData.HairColor = reader.ReadInt16();
                characterData.HairHighlightColor = reader.ReadInt16();
                characterData.SkinColor = reader.ReadInt16();
                characterData.EyeColor = reader.ReadInt16();
                characterData.Beard = reader.ReadInt16();
                characterData.Mustache = reader.ReadInt16();
                characterData.Bangs = reader.ReadInt16();
            }

            if (playerType.ServerUpdateTypeFlagIsSet(PlayerUpdateType.HealthManaStaminaRage))
            {
                characterData.CurrentHealth = reader.ReadInt64();
                characterData.MaxHealth = reader.ReadInt64();
                characterData.CurrentMana = reader.ReadInt64();
                characterData.MaxMana = reader.ReadInt64();
                characterData.CurrentStamina = reader.ReadUInt32();
                characterData.MaxStamina = reader.ReadUInt32();
                characterData.CurrentRage = reader.ReadUInt32();
                characterData.MaxRage = reader.ReadUInt32();
            }

            if (playerType.ServerUpdateTypeFlagIsSet(PlayerUpdateType.Position))
            {
                characterData.Position = reader.ReadVector3();
                characterData.CameraYaw = reader.ReadFloat();
            }

            if (playerType.ServerUpdateTypeFlagIsSet(PlayerUpdateType.Stats))
            {
                characterData.Regen = reader.ReadUInt32();
                characterData.Dmg = reader.ReadUInt32();
                characterData.Reflex = reader.ReadDouble();
                characterData.Fortitude = reader.ReadDouble();
                characterData.Will = reader.ReadDouble();
                characterData.ArmorClass = reader.ReadInt32();
                characterData.Str = reader.ReadInt32();
                characterData.Int = reader.ReadInt32();
                characterData.Wis = reader.ReadInt32();
                characterData.Con = reader.ReadInt32();
                characterData.Dex = reader.ReadInt32();
                characterData.Luck = reader.ReadInt32();
            }

            if (playerType.ServerUpdateTypeFlagIsSet(PlayerUpdateType.Elements))
            {
                var offEle = reader.ReadString();
                Enum.TryParse(offEle, out Element offenseElement);
                characterData.OffenseElement = offenseElement;
                var defEle = reader.ReadString();
                Enum.TryParse(defEle, out Element defenseElement);
                characterData.DefenseElement = defenseElement;
                var secOffEle = reader.ReadString();
                Enum.TryParse(secOffEle, out Element secondaryOffenseElement);
                characterData.SecondaryOffenseElement = secondaryOffenseElement;
                var secDefEle = reader.ReadString();
                Enum.TryParse(secDefEle, out Element secondaryDefenseElement);
                characterData.SecondaryDefenseElement = secondaryDefenseElement;
            }

            return characterData;
        }
    }
}