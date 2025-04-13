using System;
using Assets.Scripts.Models;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using Assets.Scripts.Network.PacketHandling;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.ReceiveFromServer
{
    public sealed class EntityMovementConverter : PacketConverterBase<EntityMovementArgs>
    {
        protected override EntityMovementArgs Deserialize(ref SpanReader reader)
        {
            var type = reader.ReadString();
            Enum.TryParse(type, out EntityType entityType);
            var serial = reader.ReadGuid();
            var position = reader.ReadVector3();
            var cameraYaw = reader.ReadFloat();

            return new EntityMovementArgs
            {
                EntityType = entityType,
                Serial = serial,
                Position = position,
                CameraYaw = cameraYaw
            };
        }
    }
}