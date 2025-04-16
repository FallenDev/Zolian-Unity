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
            var type = (EntityType)reader.ReadByte();
            var serial = reader.ReadGuid();
            var position = reader.ReadVector3();
            var inputDir = reader.ReadVector3();
            var cameraYaw = reader.ReadFloat();
            var speed = reader.ReadFloat();
            var velocity = reader.ReadFloat();

            return new EntityMovementArgs
            {
                EntityType = type,
                Serial = serial,
                Position = position,
                InputDirection = inputDir,
                CameraYaw = cameraYaw,
                Speed = speed,
                VerticalVelocity = velocity
            };
        }
    }
}