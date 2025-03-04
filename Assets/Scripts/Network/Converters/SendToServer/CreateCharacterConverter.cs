using Assets.Scripts.Network.OpCodes;
using Assets.Scripts.Network.PacketArgs.SendToServer;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.SendToServer
{
    public sealed class CreateCharacterConverter : PacketConverterBase<CreateCharacterArgs>
    {
        public override byte OpCode => (byte)ClientOpCode.CreateCharacter;
        
        public override void Serialize(ref SpanWriter writer, CreateCharacterArgs args)
        {
            writer.WriteInt64(args.SteamId);
            writer.WriteString(args.Name);
            writer.WriteByte((byte)args.Class);
            writer.WriteByte((byte)args.Race);
            writer.WriteByte((byte)args.Sex);
            // ToDo: Create character visuals
        }
    }
}