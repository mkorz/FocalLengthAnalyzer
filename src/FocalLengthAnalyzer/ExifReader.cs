using System.IO;
using System.Linq;
using FocalLengthAnalyzer.Readers;

namespace FocalLengthAnalyzer {
    public enum ExifFieldType {
        UnsignedByte = 1,
        AsciiStrings = 2,
        UnsignedShort = 3,
        UnsignedLong = 4,
        UnsignedRational = 5,
        SignedByte = 6,
        Undefined = 7,
        SignedShort = 8,
        SignedLong = 9,
        SignedRational = 10,
        SingleFloat = 11,
        DoubleFloat = 12
    }


    public class ExifReader {

        private long TiffHeaderOffset;
        static readonly byte[] JpegHeader = { 0xff, 0xd8 };
        static readonly byte[] JfifMarker = { 0xff, 0xe0 };
        static readonly byte[] ExifMarker = { 0xff, 0xe1 };

        static readonly byte[] JpegHeaderWithJfifSegment = new byte[] { 0xff, 0xd8, 0xff, 0xe0 };
        static readonly byte[] JpegHeaderWithExifSegment = new byte[] { 0xff, 0xd8, 0xff, 0xe1 };

        private BinaryReader _jpegFileReader;
        private IUniversalBinaryReader _binaryReader;


        public ExifReader()
        {
        }

        public ExifReader(BinaryReader jpegFileReader)
        {
            SetReader(jpegFileReader);
        }

        public void SetReader(BinaryReader jpegFileReader)
        {
            TiffHeaderOffset = 12;
            _jpegFileReader = jpegFileReader;
            _binaryReader = new BigEndianBinaryReader(_jpegFileReader);
            
        }

        public bool IsJpeg(byte[] header) => header.Take(2).SequenceEqual(JpegHeader);
        public bool IsExif(byte[] header) => header.Take(2).SequenceEqual(ExifMarker);
        public bool IsJfif(byte[] header) => header.Take(2).SequenceEqual(JfifMarker);


        public void ReadTag(ushort tag, ExifHeader headerType, ExifPhoto photo)
        {
            var currentPos = _jpegFileReader.BaseStream.Position;
            var fieldtype = (ExifFieldType)_binaryReader.ReadUInt16();
            var length = _binaryReader.ReadUInt32();
            var value = _binaryReader.ReadUInt32();
            switch (fieldtype) {

                case ExifFieldType.AsciiStrings:
                    SeekToTiffHeaderRelativeOffset(value);
                    photo.SetMatchingProperty(tag, headerType, _binaryReader.ReadString((int)length));
                    break;

                case ExifFieldType.UnsignedRational:
                    SeekToTiffHeaderRelativeOffset(value);
                    photo.SetMatchingProperty(tag, headerType, _binaryReader.ReadUnsignedRational());
                    break;

                case ExifFieldType.UnsignedLong:
                    photo.SetMatchingProperty(tag, headerType, value);
                    break;

            }
            _jpegFileReader.BaseStream.Seek(currentPos + 2 + 4 + 4, SeekOrigin.Begin);
        }


        public void ReadIFDHeader(ExifHeader headerType, uint headerPosition, ExifPhoto photo)
        {
            SeekToTiffHeaderRelativeOffset(headerPosition);
            var numberOfTags = _binaryReader.ReadUInt16();
            for (var i = 0; i < numberOfTags; i++) {
                var tag = _binaryReader.ReadUInt16();
                if (ExifPhoto.HasMatchingProperty(tag, headerType)) {
                    ReadTag(tag, headerType, photo);
                    if (!photo.HasUnsetProperties(headerType)) break;
                } else
                    MoveToTheNextTag();
            }
        }

        public bool IsBigEndian()
        {
            SeekToTiffHeaderRelativeOffset(0);
            var format = _binaryReader.ReadUInt16();
            if (format == 0x4d4d) return true;
            if (format == 0x4949) return false;
            throw new InvalidDataException("Could not recognize bitness");
        }

        public ExifPhoto GetPhotoInfo()
        {
            var fileHeader = _jpegFileReader.ReadBytes(2);
            if (!IsJpeg(fileHeader)) throw new InvalidDataException("File is not a recognisable jpeg file");
            var nextHeader = _jpegFileReader.ReadBytes(2);
            if (!IsExif(nextHeader)) {
                if (!IsJfif(nextHeader))
                    throw new InvalidDataException("File does not start with JPEG Exif header");
                var jfifLength = _binaryReader.ReadUInt16();
                _jpegFileReader.BaseStream.Seek(jfifLength - 2, SeekOrigin.Current);
                nextHeader = _jpegFileReader.ReadBytes(2);
                if (!IsExif(nextHeader)) throw new InvalidDataException("Could not find Exif header");
                TiffHeaderOffset = _jpegFileReader.BaseStream.Position + 8;
            }

            if (!IsBigEndian()) _binaryReader = new LittleEndianBinaryReader(_jpegFileReader);

            var photo = new ExifPhoto();
            ReadIFDHeader(ExifHeader.IFD0, 8, photo);

            if (photo.SubIFDPosition == 0)
                throw new InvalidDataException("Have not found ExifSubIdTag inside the file");

            ReadIFDHeader(ExifHeader.SubIFD, photo.SubIFDPosition, photo);

            if (photo.Manufacturer == null || photo.Model == null)
                throw new InvalidDataException("Could not read photo manufacturer or model from the Exif header");

            return photo;
        }

        private void MoveToTheNextTag() => _jpegFileReader.BaseStream.Seek(10, SeekOrigin.Current);

        private void SeekToTiffHeaderRelativeOffset(uint pos) => _jpegFileReader.BaseStream.Seek(TiffHeaderOffset + pos, SeekOrigin.Begin);

    }
}
