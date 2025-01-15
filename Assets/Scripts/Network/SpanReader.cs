using System.Text;
using System;

namespace Assets.Scripts.Network
{
    public ref struct SpanReader
    {
        private readonly Span<byte> _span;
        private int _position;

        public SpanReader(Span<byte> span)
        {
            _span = span;
            _position = 0;
        }

        // Read a single byte
        public byte ReadByte()
        {
            if (_position >= _span.Length) throw new IndexOutOfRangeException("Cannot read beyond the end of the span.");
            return _span[_position++];
        }

        // Read a 16-bit integer (short)
        public short ReadInt16()
        {
            return (short)((ReadByte() << 8) | ReadByte());
        }

        // Read a 32-bit integer (int)
        public int ReadInt32()
        {
            return (ReadByte() << 24) | (ReadByte() << 16) | (ReadByte() << 8) | ReadByte();
        }

        // Read a 64-bit integer (long)
        public long ReadInt64()
        {
            long value = 0;
            for (int i = 0; i < 8; i++) value = (value << 8) | ReadByte();
            return value;
        }

        // Read a 32-bit unsigned integer (uint)
        public uint ReadUInt32()
        {
            return (uint)((ReadByte() << 24) | (ReadByte() << 16) | (ReadByte() << 8) | ReadByte());
        }

        // Read a single-precision float
        public float ReadSingle()
        {
            uint value = ReadUInt32();
            return BitConverter.Int32BitsToSingle((int)value);
        }

        // Read a double-precision float
        public double ReadDouble()
        {
            long value = ReadInt64();
            return BitConverter.Int64BitsToDouble(value);
        }

        // Read a Vector2 (two floats)
        public UnityEngine.Vector2 ReadVector2()
        {
            return new UnityEngine.Vector2(ReadSingle(), ReadSingle());
        }

        // Read a Vector3 (three floats)
        public UnityEngine.Vector3 ReadVector3()
        {
            return new UnityEngine.Vector3(ReadSingle(), ReadSingle(), ReadSingle());
        }

        // Read a string (prefixed with length as byte)
        public string ReadString()
        {
            int length = ReadByte(); // Length of the string
            if (length < 0 || _position + length > _span.Length)
            {
                throw new IndexOutOfRangeException($"String length exceeds span length. Length={length}, Available={_span.Length - _position}");
            }
            var result = Encoding.ASCII.GetString(_span.Slice(_position, length));
            _position += length;
            return result;
        }
    }
}
