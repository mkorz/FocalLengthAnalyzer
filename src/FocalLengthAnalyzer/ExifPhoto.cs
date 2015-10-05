using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace FocalLengthAnalyzer
{
    public class ExifPhoto
    {
        [Exif(0x8769, ExifHeader.IFD0)]
        public uint SubIFDPosition { get; set; }

        [Exif(0x010f, ExifHeader.IFD0)]
        public string Manufacturer { get; set; }

        [Exif(0x110, ExifHeader.IFD0)]
        public string Model { get; set; }

        [Exif(0x920a, ExifHeader.SubIFD)]
        public decimal FocalLength { get; set; }
                
        //to avoid iterating through properties on object instantiation 
        private static Dictionary<ExifAttribute, string> _exifAttributesCache;
        private static Dictionary<ExifHeader, int> _unsetPropertiesCache;
        private  Dictionary<ExifHeader, int> _unsetProperties;
                
        static ExifPhoto()
        {           
            _exifAttributesCache = GetTaggedProperties().ToDictionary(k => (ExifAttribute)CustomAttributeExtensions.GetCustomAttribute(k, typeof(ExifAttribute)), v => v.Name);
            _unsetPropertiesCache = GetTaggedProperties().GroupBy(g => ((ExifAttribute)CustomAttributeExtensions.GetCustomAttribute(g, typeof(ExifAttribute))).ExifHeader).ToDictionary(k => k.Key, c => c.Count());
        }

        public ExifPhoto()
        {
            _unsetProperties = _unsetPropertiesCache.ToDictionary(k => k.Key, v=>v.Value);
        }

        public static bool HasMatchingProperty(uint tag, ExifHeader headerType)
        {
            var exifAttribute = new ExifAttribute(tag, headerType);
            return _exifAttributesCache.ContainsKey(exifAttribute);
        }

        private static IEnumerable<PropertyInfo> GetTaggedProperties()
            => typeof(ExifPhoto)
                .GetProperties()
                .Where(p => CustomAttributeExtensions.GetCustomAttribute(p, typeof(ExifAttribute)) != null);
        

        public bool HasUnsetProperties(ExifHeader headerType) => _unsetProperties[headerType] > 0;
        
        
        public void SetMatchingProperty(uint tag, ExifHeader headerType, object value)
        {
            var exifAttribute = new ExifAttribute(tag, headerType);
            if (!_exifAttributesCache.ContainsKey(exifAttribute)) return;
            var propertyName = _exifAttributesCache[exifAttribute];
            var propertyInfo = this.GetType().GetProperty(propertyName);
            propertyInfo.SetValue(this, value, null);
            _unsetProperties[headerType]--; //this is based on the assumption that Exif tags in the jpg are unique
        }          
       
    }
}
