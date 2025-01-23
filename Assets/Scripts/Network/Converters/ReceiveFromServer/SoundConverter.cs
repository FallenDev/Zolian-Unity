using Assets.Scripts.Network.OpCodes;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.ReceiveFromServer
{
    /// <summary>
    ///     Provides deserialization logic for <see cref="SoundArgs" />
    /// </summary>
    public sealed class SoundConverter : PacketConverterBase<SoundArgs>
    {
        /// <inheritdoc />
        public override byte OpCode => (byte)ServerOpCode.Sound;

        /// <inheritdoc />
        public override SoundArgs Deserialize(ref SpanReader reader)
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