namespace FocalLengthAnalyzer
{
    public enum ExifHeader
    {
        Uknown=0,
        IFD0=1,
        SubIFD=2
    }

    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class ExifAttribute : System.Attribute
    {
        public uint Tag { get; set; }
        public ExifHeader ExifHeader { get; set; }

        public ExifAttribute(uint tag, ExifHeader exifHeader)
        {
            Tag = tag;
            ExifHeader = exifHeader;
        }
    }
}
