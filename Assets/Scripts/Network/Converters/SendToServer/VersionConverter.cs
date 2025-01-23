using Assets.Scripts.Network.OpCodes;
using Assets.Scripts.Network.PacketArgs.SendToServer;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.SendToServer
{
    /// <summary>
    ///     Provides packet serialization and deserialization logic for <see cref="VersionArgs" />
    /// </summary>
    public sealed class VersionConverter : PacketConverterBase<VersionArgs>
    {
        /// <inheritdoc />
        public override byte OpCode => (byte)ClientOpCode.Version;
        
        /// <inheritdoc />
        public override void Serialize(ref SpanWriter writer, VersionArgs args)
        {
            writer.WriteString(args.Version);
        }
    }
}