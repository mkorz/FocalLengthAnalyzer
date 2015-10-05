using System;
using System.IO;
using System.Linq;


namespace FocalLengthAnalyzer.Readers
{
    public class BigEndianBinaryReader : IUniversalBinaryReader
    {
        private BinaryReader _binaryReader;

        public BigEndianBinaryReader(BinaryReader binaryReader)
        {
            _binaryReader = binaryReader;            
        }
        
        public ushort ReadUInt16() => BitConverter.ToUInt16(_binaryReader.ReadBytes(2).Reverse().ToArray(), 0);
        public uint ReadUInt32() => BitConverter.ToUInt32(_binaryReader.ReadBytes(4).Reverse().ToArray(), 0);
        public string ReadString(int length) => System.Text.Encoding.ASCII.GetString(_binaryReader.ReadBytes(length)).Trim('\0');
        public decimal ReadUnsignedRational() => (decimal)ReadUInt32() / ReadUInt32();
                
    }
}
