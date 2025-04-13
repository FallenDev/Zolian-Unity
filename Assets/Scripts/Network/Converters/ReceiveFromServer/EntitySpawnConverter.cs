using System;
using Assets.Scripts.Models;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using Assets.Scripts.Network.PacketHandling;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.ReceiveFromServer
{
    public sealed class EntitySpawnConveter : PacketConverterBase<EntitySpawnArgs>
    {
        protected override EntitySpawnArgs Deserialize(ref SpanReader reader)
        {
            var type = reader.ReadString();
            Enum.TryParse(type, out EntityType entityType);
            var serial = reader.ReadGuid();
            var position = reader.ReadVector3();
            var cameraYaw = reader.ReadFloat();
            var level = reader.ReadUInt32();
            var currentHealth = reader.ReadInt64();
            var maxHealth = reader.ReadInt64();
            var currentMana = reader.ReadInt64();
            var maxMana = reader.ReadInt64();

            // Return player args
            if (entityType.EntityTypeFlagIsSet(EntityType.Player))
            {
                var userName = reader.ReadString();
                var job = reader.ReadString();
                var firstClass = reader.ReadString();
                var secondClass = reader.ReadString();
                var jobLevel = reader.ReadUInt32();
                var race = reader.ReadString();
                Enum.TryParse(race, out Race playerRace);
                var sex = reader.ReadString();
                Enum.TryParse(sex, out Sex playerSex);
                var hair = reader.ReadInt16();
                var hairColor = reader.ReadInt16();
                var hairHighlightColor = reader.ReadInt16();
                var skinColor = reader.ReadInt16();
                var eyeColor = reader.ReadInt16();
                var beard = reader.ReadInt16();
                var mustache = reader.ReadInt16();
                var bangs = reader.ReadInt16();

                return new EntitySpawnArgs
                {
                    EntityType = entityType,
                    Serial = serial,
                    Position = position,
                    CameraYaw = cameraYaw,
                    EntityLevel = level,
                    CurrentHealth = currentHealth,
                    MaxHealth = maxHealth,
                    CurrentMana = currentMana,
                    MaxMana = maxMana,
                    UserName = userName,
                    Job = job,
                    FirstClass = firstClass,
                    SecondClass = secondClass,
                    JobLevel = jobLevel,
                    Race = playerRace,
                    Sex = playerSex,
                    Hair = hair,
                    HairColor = hairColor,
                    HairHighlightColor = hairHighlightColor,
                    SkinColor = skinColor,
                    EyeColor = eyeColor,
                    Beard = beard,
                    Mustache = mustache,
                    Bangs = bangs
                };
            }

            // Return common args
            return new EntitySpawnArgs
            {
                EntityType = entityType,
                Serial = serial,
                Position = position,
                CameraYaw = cameraYaw,
                EntityLevel = level,
                CurrentHealth = currentHealth,
                MaxHealth = maxHealth,
                CurrentMana = currentMana,
                MaxMana = maxMana,
            };
        }
    }
}