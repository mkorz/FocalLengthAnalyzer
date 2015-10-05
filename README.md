# Simple focal length extractor

### A bit of history

Some time ago I bought my first DSLR (Nikon D5000). The stock 18-105mm lens gives some flexibility, but at some point I decided it would be nice to try a prime lens. This idea lead to a bit of dilema - which lens should I go for? Having limited budget I was left with two options - 35mm f/1.8G or 50mm f/1.8G. 

I searched the internet, read number of reviews and posts and decided there is only one way to find the winner - **I should check photos I took and find out which focal length I used more often**.

### The code
I keep my photos on NAS running Linux, so this looked like a perfect opportunity to try [CoreCLR](https://github.com/dotnet/coreclr).
To make this little project more fun, instead of using one of the many available [EXIF](https://en.wikipedia.org/wiki/Exchangeable_image_file_format) readers I decided write my own EXIF header parser (with a limited functionality - I only needed to retrieve the focal length. Oh and camera vendor and type - so I can exclude photos taken with my iPhone).

To add a little extra I created three additional variants of the analyzer:
* first one instantiating the main reader class only once - just to test if it makes any significant difference in speed
* second one using *Parallel.ForEach* method to make processing, well, parallel ;)
* third one which caches the first 4KB of the file (which should contain the complete header) to speed up Binary Reader operations. This obviously is a theory, as the OS has probably already cached it anyway.

Each variant can be selected (or not) by writing correct *DEFINE*s in *project.json*, although they have not been designed to  run in pairs (or triples).

###  So, how do I run it on Linux?
1. Follow the CoreCLR [instructions](https://github.com/dotnet/coreclr/blob/master/Documentation/install/get-dotnetcore-dnx-linux.md) to install CoreCLR
1. Run `dnvm use CORECLR_VERSION -r coreclr` (currently: `dnvm use 1.0.0-beta8-15613 -r coreclr`)
1. Run `dnu restore`
1. Run `dnx FocalLengthAnalyzer DIR_WITH_JPG`

### Sample output
First line contains camera vendor and model name.
Second line gives information about number of photos processed.
Then, each line starts with the focal length, followed by number of photos and graphical representation of the histogram.

```
NIKON CORPORATION NIKON D5000
Photos analyzed: 1278
 18:   699 - **********************************************************************
 21:    48 - ****
 22:    13 - *
 24:    33 - ***
 25:    28 - **
 26:    19 - *
 28:    65 - ******
 30:    40 - ****
 32:    15 - *
 34:     7 -
 35:     8 -
 38:    23 - **
 40:    21 - **
 42:    22 - **
 45:    20 - **
 48:    22 - **
 50:    13 - *
 52:    21 - **
 58:    16 - *
 62:    11 - *
 66:    14 - *
 70:     8 -
 75:     5 -
 80:    12 - *
 85:     7 -
 90:     8 -
 92:     3 -
 98:    12 - *
105:    65 - ******
```

### So what was my result?
It turned out, I mostly use both ends of the spectrum - 18mm and 105mm were chosen for one third of all taken photos. The number of photos take at 35mm and 50mm were nearly identical with 35mm being my favourite - so this one seems to be the better choice. Your mileage may vary. ;)

### Final words
The CoreCLR is defintely fun to work with. In a past I would have written this tool in Perl or create a one liner in shell using one of the available EXIF tools on Linux. And I would probably do the same today had I needed the results fast. However, the point of this project was to evaluate the CoreCLR and I must say I was suprised how easy it was to write a truly multiplatform code.