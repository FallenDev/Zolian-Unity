using System;
using System.Collections.Generic;
using Assets.Scripts.Models;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using Assets.Scripts.Network.PacketHandling;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.ReceiveFromServer
{
    public sealed class PlayerListConverter : PacketConverterBase<PlayerListArgs>
    {
        protected override PlayerListArgs Deserialize(ref SpanReader reader)
        {
            var numberOfCharacters = reader.ReadByte();
            var playerList = new List<PlayerSelection>();

            for (var i = 0; i < numberOfCharacters; i++)
            {
                var serial = reader.ReadGuid();
                var disabled = reader.ReadBoolean();
                var name = reader.ReadString();
                var level = reader.ReadUInt32();
                var baseClassString = reader.ReadString();
                var advClassString = reader.ReadString();
                var jobString = reader.ReadString();
                var health = reader.ReadInt64();
                var mana = reader.ReadInt64();
                var raceString = reader.ReadString();
                var genderString = reader.ReadString();
                var hair = reader.ReadInt16();
                var hairColor = reader.ReadInt16();
                var hairHighlightColor = reader.ReadInt16();
                var skinColor = reader.ReadInt16();
                var eyeColor = reader.ReadInt16();
                var beard = reader.ReadInt16();
                var mustache = reader.ReadInt16();
                var bangs = reader.ReadInt16();

                Enum.TryParse(baseClassString, out BaseClass baseClass);
                Enum.TryParse(advClassString, out BaseClass advClass);
                Enum.TryParse(jobString, out JobClass jobClass);
                Enum.TryParse(raceString, out Race race);
                Enum.TryParse(genderString, out Sex sex);

                var player = new PlayerSelection
                {
                    Serial = serial,
                    Disabled = disabled,
                    Name = name,
                    Level = level,
                    BaseClass = baseClass,
                    AdvClass = advClass,
                    Job = jobClass,
                    Health = health,
                    Mana = mana,
                    Race = race,
                    Sex = sex,
                    Hair = hair,
                    HairColor = hairColor,
                    HairHighlightColor = hairHighlightColor,
                    SkinColor = skinColor,
                    EyeColor = eyeColor,
                    Beard = beard,
                    Mustache = mustache,
                    Bangs = bangs
                };

                playerList.Add(player);
            }
            
            return new PlayerListArgs { Players = playerList };
        }
    }
}
