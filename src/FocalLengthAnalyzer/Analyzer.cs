
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Concurrent;

namespace FocalLengthAnalyzer {
    public class Analyzer {

        private ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<decimal, int>>> _focalLengths = new ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<decimal, int>>>();
        private int unrecognisedPhotos = 0;

        public void AnalyzeDirectoryContent(string path)
        {
            var fileList = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
            .Where(f => f.ToLowerInvariant().EndsWith(".jpg") || f.ToLowerInvariant().EndsWith(".jpeg"));
#if PERSISTENTREADER
            var exifReader=new ExifReader();
#endif

#if PARALLEL
            Parallel.ForEach<string>(fileList, (string file) => {
#else
            foreach (var file in fileList) {
#endif
                Console.WriteLine($"Processing file: {file}");
                using (var stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
#if MEMORYREADER
                using (var streamReader = new BinaryReader(stream)) {
                    byte[] buffer = new byte[4096];
                    stream.Read(buffer, 0, buffer.Length);
                    using (var memory = new MemoryStream(buffer))
                    using (var reader = new BinaryReader(memory)) {
#else
                using (var reader = new BinaryReader(stream)) {
#endif
#if PERSISTENTREADER
                    exifReader.SetReader(reader);
#else
                    var exifReader = new ExifReader(reader);
#endif
                    try {
                        AddFocalLength(exifReader.GetPhotoInfo());
                    } catch (InvalidDataException ex) {
                        Console.Error.WriteLine($"Error while processing jpg file {file}: {ex.Message}");
                        unrecognisedPhotos++;
                    }
                }
#if MEMORYREADER
                }
#endif
            }
#if PARALLEL
            );
#endif
            PrintFocalLenght();
        }

        private void AddFocalLength(ExifPhoto photo)
        {
            var manufacturerElement = _focalLengths.GetOrAdd(photo.Manufacturer, new ConcurrentDictionary<string, ConcurrentDictionary<decimal, int>>());
            var modelElement = manufacturerElement.GetOrAdd(photo.Model, new ConcurrentDictionary<decimal, int>());
            var focalLengthElement = modelElement.AddOrUpdate(photo.FocalLength, 1, (x, y) => y + 1);
        }

        private void PrintFocalLenght()
        {
            Console.WriteLine($"Photos with errors or no data {unrecognisedPhotos}");
            foreach (var manufacturer in _focalLengths)
                foreach (var model in manufacturer.Value) {
                    Console.WriteLine($"{manufacturer.Key} {model.Key}");
                    Console.WriteLine($"Photos analyzed: {model.Value.Sum(x => x.Value)}");
                    var maxPhotos = model.Value.Max(s => s.Value);
                    foreach (var focalLength in model.Value.OrderBy(f => f.Key))
                        Console.WriteLine($"{focalLength.Key,3}: {focalLength.Value,5} - {new string('*', (int)Math.Floor(70.0 * focalLength.Value / maxPhotos))}");
                }
        }
    }
}
