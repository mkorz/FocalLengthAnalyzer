using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FocalLengthAnalyzer.Readers
{
    interface IUniversalBinaryReader
    {
        ushort ReadUInt16();
        uint ReadUInt32();
        string ReadString(int length);
        decimal ReadUnsignedRational();
    }
}
