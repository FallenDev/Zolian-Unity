using Assets.Scripts.Network.OpCodes;
using Assets.Scripts.Network.PacketArgs.SendToServer;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.SendToServer
{
    public sealed class DeleteCharacterConverter : PacketConverterBase<DeleteCharacterArgs>
    {
        public override byte OpCode => (byte)ClientOpCode.DeleteCharacter;
        
        public override void Serialize(ref SpanWriter writer, DeleteCharacterArgs args)
        {
            writer.WriteInt64(args.SteamId);
            writer.WriteString(args.Name);
        }
    }
}