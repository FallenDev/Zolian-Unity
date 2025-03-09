using Assets.Scripts.Network.OpCodes;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.ReceiveFromServer
{
    public sealed class CharacterFinalizedConverter : PacketConverterBase<CharacterFinalizedArgs>
    {
        public override byte OpCode => (byte)ServerOpCode.CreateCharacterFinalized;

        public override CharacterFinalizedArgs Deserialize(ref SpanReader reader)
        {
            var finalized = reader.ReadBoolean();

            return new CharacterFinalizedArgs
            {
                Finalized = finalized
            };
        }
    }
}