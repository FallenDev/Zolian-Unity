using System.Text;
using System;

namespace Assets.Scripts.Network
{
    public ref struct SpanWriter
    {
        private Span<byte> _span;
        private int _position;

        public SpanWriter(Span<byte> span)
        {
            _span = span;
            _position = 0;
        }

        // Write a single byte
        public void WriteByte(byte value)
        {
            if (_position >= _span.Length) throw new IndexOutOfRangeException("Cannot write beyond the end of the span.");
            _span[_position++] = value;
        }

        // Write a 16-bit integer (short)
        public void WriteInt16(short value)
        {
            WriteByte((byte)(value >> 8));
            WriteByte((byte)value);
        }

        // Write a 32-bit integer (int)
        public void WriteInt32(int value)
        {
            WriteByte((byte)(value >> 24));
            WriteByte((byte)(value >> 16));
            WriteByte((byte)(value >> 8));
            WriteByte((byte)value);
        }

        // Write a 64-bit integer (long)
        public void WriteInt64(long value)
        {
            for (int i = 7; i >= 0; i--) WriteByte((byte)(value >> (i * 8)));
        }

        // Write a 32-bit unsigned integer (uint)
        public void WriteUInt32(uint value)
        {
            WriteInt32((int)value);
        }

        // Write a single-precision float
        public void WriteSingle(float value)
        {
            WriteUInt32((uint)BitConverter.SingleToInt32Bits(value));
        }

        // Write a double-precision float
        public void WriteDouble(double value)
        {
            WriteInt64(BitConverter.DoubleToInt64Bits(value));
        }

        // Write a Vector2 (two floats)
        public void WriteVector2(UnityEngine.Vector2 value)
        {
            WriteSingle(value.x);
            WriteSingle(value.y);
        }

        // Write a Vector3 (three floats)
        public void WriteVector3(UnityEngine.Vector3 value)
        {
            WriteSingle(value.x);
            WriteSingle(value.y);
            WriteSingle(value.z);
        }

        // Write a string (prefixed with length as byte)
        public void WriteString(string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            if (bytes.Length > byte.MaxValue)
                throw new ArgumentException("String is too long to write (maximum 255 characters).");

            WriteByte((byte)bytes.Length);
            WriteBytes(bytes);
        }

        // Write raw bytes
        public void WriteBytes(ReadOnlySpan<byte> bytes)
        {
            if (_position + bytes.Length > _span.Length)
                throw new IndexOutOfRangeException("Not enough space in the span.");
            bytes.CopyTo(_span.Slice(_position));
            _position += bytes.Length;
        }
    }
}
