using System.Text;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.Network
{
    /// <summary>
    /// A ref struct for reading primitive types, strings, and other data from a <see cref="ReadOnlySpan{T}" />.
    /// </summary>
    public ref struct SpanReader
    {
        private ReadOnlySpan<byte> _buffer;
        private int _position;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpanReader" /> struct.
        /// </summary>
        /// <param name="buffer">The span of bytes to read from.</param>
        public SpanReader(ReadOnlySpan<byte> buffer)
        {
            _buffer = buffer;
            _position = 0;
        }

        /// <summary>
        /// Gets a value indicating whether the reader has reached or exceeded the end of the span.
        /// </summary>
        public bool EndOfSpan => _position >= _buffer.Length;

        /// <summary>
        /// Gets the number of unread bytes remaining in the buffer.
        /// </summary>
        public int Remaining => _buffer.Length - _position;

        /// <summary>
        /// Reads a boolean value from the buffer.
        /// </summary>
        public bool ReadBoolean() => ReadByte() != 0;

        // Read Unsigned Numeric Types
        public ushort ReadUInt16()
        {
            return (ushort)((_buffer[_position++] << 8) | _buffer[_position++]);
        }

        public uint ReadUInt32()
        {
            return (uint)((_buffer[_position++] << 24) | (_buffer[_position++] << 16) |
                          (_buffer[_position++] << 8) | _buffer[_position++]);
        }

        public ulong ReadUInt64()
        {
            ulong value = 0;
            for (var i = 0; i < 8; i++)
            {
                value = (value << 8) | _buffer[_position++];
            }
            return value;
        }

        // Read Signed Numeric Types
        public short ReadInt16() => (short)ReadUInt16();

        public int ReadInt32() => (int)ReadUInt32();

        public long ReadInt64() => (long)ReadUInt64();

        // Read Floating Point Types
        public float ReadFloat() => BitConverter.Int32BitsToSingle(ReadInt32());

        public double ReadDouble() => BitConverter.Int64BitsToDouble(ReadInt64());

        // Read Vectors
        public Vector2 ReadVector2()
        {
            return new Vector2(ReadFloat(), ReadFloat());
        }

        public Vector3 ReadVector3()
        {
            return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
        }

        // Read Points
        public (byte X, byte Y) ReadPoint8()
        {
            return (ReadByte(), ReadByte());
        }

        public (short X, short Y) ReadPoint16()
        {
            return (ReadInt16(), ReadInt16());
        }

        public (ushort X, ushort Y) ReadPoint16U()
        {
            return (ReadUInt16(), ReadUInt16());
        }

        public (int X, int Y) ReadPoint32()
        {
            return (ReadInt32(), ReadInt32());
        }

        public (uint X, uint Y) ReadPoint32U()
        {
            return (ReadUInt32(), ReadUInt32());
        }

        public (long X, long Y) ReadPoint64()
        {
            return (ReadInt64(), ReadInt64());
        }

        public (ulong X, ulong Y) ReadPoint64U()
        {
            return (ReadUInt64(), ReadUInt64());
        }

        // Read Strings with Dynamic Length Prefix
        public string ReadString()
        {
            var lengthType = ReadByte();
            return lengthType switch
            {
                0 => ReadString8(),
                1 => ReadString16(),
                2 => ReadString32(),
                _ => null
            };
        }

        // Read String with 8-bit Length Prefix
        private string ReadString8()
        {
            var length = ReadByte();
            return ReadStringOfLength(length);
        }

        // Read String with 16-bit Length Prefix
        private string ReadString16()
        {
            var length = ReadUInt16();
            return ReadStringOfLength(length);
        }

        // Read String with 32-bit Length Prefix
        private string ReadString32()
        {
            var length = (int)ReadUInt32();
            return ReadStringOfLength(length);
        }

        private string ReadStringOfLength(int length)
        {
            if (_position + length > _buffer.Length)
                throw new IndexOutOfRangeException("String length exceeds available buffer.");

            var result = Encoding.UTF8.GetString(_buffer.Slice(_position, length));
            _position += length;
            return result;
        }

        // Read Bytes
        public byte ReadByte()
        {
            if (_position >= _buffer.Length)
                throw new IndexOutOfRangeException("Cannot read beyond the buffer.");
            return _buffer[_position++];
        }

        public sbyte ReadSByte() => (sbyte)ReadByte();

        public ReadOnlySpan<byte> ReadBytesAsSpan(int length)
        {
            if (_position + length > _buffer.Length)
                throw new IndexOutOfRangeException("Requested length exceeds available buffer.");

            var result = _buffer.Slice(_position, length);
            _position += length;
            return result;
        }

        public byte[] ReadBytes(int length) => ReadBytesAsSpan(length).ToArray();

        // Read Raw Data
        public ReadOnlySpan<byte> ReadData()
        {
            var lengthType = ReadByte();
            return lengthType switch
            {
                0 => ReadData8(),
                1 => ReadData16(),
                2 => ReadData32(),
                _ => null
            };
        }

        private ReadOnlySpan<byte> ReadData8()
        {
            var length = ReadByte();
            return ReadBytesAsSpan(length);
        }

        private ReadOnlySpan<byte> ReadData16()
        {
            var length = ReadUInt16();
            return ReadBytesAsSpan(length);
        }

        private ReadOnlySpan<byte> ReadData32()
        {
            var length = ReadUInt32();
            return ReadBytesAsSpan((int)length);
        }

        // Read Arguments
        public List<string> ReadArgs()
        {
            var args = new List<string>();
            while (!EndOfSpan)
            {
                args.Add(ReadString());
            }
            return args;
        }

        public List<string> ReadArgs8()
        {
            var args = new List<string>();
            while (!EndOfSpan)
            {
                args.Add(ReadString8());
            }
            return args;
        }

        /// <summary>
        /// Returns a span of the unread portion of the buffer.
        /// </summary>
        /// <returns>A span of the unread portion of the buffer.</returns>
        public ReadOnlySpan<byte> ToSpan() => _buffer[_position..];
    }
}
