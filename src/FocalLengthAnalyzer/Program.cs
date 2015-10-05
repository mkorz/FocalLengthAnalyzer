using System;
using System.IO;
using System.Linq;


namespace FocalLengthAnalyzer
{
    public class Program
    {
        public void Main(string[] args)
        {      
           if (args.Count()<1) {
                Console.WriteLine("Please pass the path to the directory with jpeg files as the first argument.");
                return;
            }
           if (!Directory.Exists(args[0])) {
                Console.WriteLine($"Error: directory {args[0]} does not exits.");
                return;
            }
            var analyzer = new Analyzer();
            analyzer.AnalyzeDirectoryContent(args[0]);
        }
    }
}
