using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using Assets.Scripts.Network.PacketHandling;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.ReceiveFromServer
{
    public sealed class SoundConverter : PacketConverterBase<SoundArgs>
    {
        protected override SoundArgs Deserialize(ref SpanReader reader)
        {
            var indicatorOrIndex = reader.ReadByte();

            if (indicatorOrIndex == byte.MaxValue)
            {
                var musicIndex = reader.ReadByte();

                return new SoundArgs
                {
                    IsMusic = true,
                    Sound = musicIndex
                };
            }

            return new SoundArgs
            {
                IsMusic = false,
                Sound = indicatorOrIndex
            };
        }
    }
}