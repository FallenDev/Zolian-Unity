using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using Assets.Scripts.Network.PacketHandling;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.ReceiveFromServer
{
    public sealed class CharacterFinalizedConverter : PacketConverterBase<CharacterFinalizedArgs>
    {
        protected override CharacterFinalizedArgs Deserialize(ref SpanReader reader)
        {
            var finalized = reader.ReadBoolean();

            return new CharacterFinalizedArgs
            {
                Finalized = finalized
            };
        }
    }
}