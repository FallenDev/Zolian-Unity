using System;

namespace Assets.Scripts.Network.PacketHandling
{
    public class Packet
    {
        private byte Signature { get; }
        public ushort Length { get; }
        public byte OpCode { get; }
        public byte Sequence { get; }
        public byte[] Payload { get; }

        // Constructor for incoming packets
        public Packet(ReadOnlySpan<byte> span)
        {
            if (span.Length < 5)
                throw new ArgumentException("Span is too short to be a valid packet.");

            Signature = span[0];

            // Extract length (big-endian)
            Length = (ushort)((span[1] << 8) | span[2]);
            if (span.Length < 5 + Length)
                throw new ArgumentException("Span does not contain the full packet data.");

            OpCode = span[3];
            Sequence = span[4];
            Payload = span.Slice(5, Length).ToArray(); // Convert to a byte array
        }

        // Constructor for outgoing packets
        public Packet(byte opCode, ReadOnlySpan<byte> payload)
        {
            Signature = 0x16;
            Length = (ushort)payload.Length;
            OpCode = opCode;
            Sequence = 0;
            Payload = payload.ToArray(); // Convert to a byte array
        }

        public byte[] ToArray()
        {
            var buffer = new byte[5 + Payload.Length];
            buffer[0] = Signature;
            buffer[1] = (byte)(Length >> 8);
            buffer[2] = (byte)(Length & 0xFF);
            buffer[3] = OpCode;
            buffer[4] = Sequence;
            Payload.CopyTo(buffer, 5);
            return buffer;
        }
    }
}