using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FocalLengthAnalyzer.Readers
{
    public class LittleEndianBinaryReader : IUniversalBinaryReader
    {
        private BinaryReader _binaryReader;

        public LittleEndianBinaryReader(BinaryReader binaryReader)
        {
            _binaryReader = binaryReader;
        }

        public ushort ReadUInt16() => _binaryReader.ReadUInt16();
        public uint ReadUInt32() => _binaryReader.ReadUInt32();
        public string ReadString(int length) => System.Text.Encoding.ASCII.GetString(_binaryReader.ReadBytes(length)).Trim('\0');
        public decimal ReadUnsignedRational() => (decimal)ReadUInt32() / ReadUInt32();

    }
}
