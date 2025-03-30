using Assets.Scripts.Network.OpCodes;
using Assets.Scripts.Network.PacketArgs.SendToServer;
using Assets.Scripts.Network.PacketHandling;
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
            writer.WriteInt16(args.Hair);
            writer.WriteInt16(args.HairColor);
            writer.WriteInt16(args.HairHighlightColor);
            writer.WriteInt16(args.SkinColor);
            writer.WriteInt16(args.EyeColor);
            writer.WriteInt16(args.Beard);
            writer.WriteInt16(args.Mustache);
            writer.WriteInt16(args.Bangs);
        }
    }
}