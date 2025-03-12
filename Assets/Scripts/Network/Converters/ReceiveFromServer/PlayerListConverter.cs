using System;
using System.Collections.Generic;
using Assets.Scripts.Models;
using Assets.Scripts.Network.OpCodes;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.ReceiveFromServer
{
    public sealed class PlayerListConverter : PacketConverterBase<PlayerListArgs>
    {
        public override byte OpCode => (byte)ServerOpCode.PlayerList;

        public override PlayerListArgs Deserialize(ref SpanReader reader)
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

                Enum.TryParse(baseClassString, out BaseClass baseClass);
                Enum.TryParse(advClassString, out BaseClass advClass);
                Enum.TryParse(jobString, out JobClass jobClass);

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
                    Mana = mana
                };

                playerList.Add(player);
            }
            
            return new PlayerListArgs { Players = playerList };
        }
    }
}
