using Microsoft.VisualBasic;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace ImageUnicodeConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            string imagePath;
            if (args.Length < 1)
            {
                Console.WriteLine("Image path: ");
                imagePath = Console.ReadLine();
            }
            else
            {
                imagePath = args[0];
            }

            int lineWidth = 100;
            if (args.Length >= 2)
                lineWidth = int.Parse(args[1]);

            bool textOut = false;
            if (args.Length >= 3)
                textOut = true;

            Bitmap image = new Bitmap(imagePath);
            Console.WriteLine($"The image is {image.Height} x {image.Width}");

            if (lineWidth > image.Width)
                lineWidth = image.Width;
            int lineHeight = lineWidth * image.Height / image.Width;

            Stopwatch timer = new Stopwatch();
            timer.Start();

            ColorInfo[,] tiles = AverageColorInfo(image, lineHeight, lineWidth);
            if (textOut)
                WriteToText(tiles);
            else
                WriteToRex(tiles);

            timer.Stop();
            Console.WriteLine($"Converted image in {timer.Elapsed}");
        }

        private static void WriteToRex(ColorInfo[,] tiles)
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(memStream))
                {
                    // write xp header info
                    bw.Write(1); // version
                    bw.Write(1); // layers
                    bw.Write(tiles.GetLength(0)); // width
                    bw.Write(tiles.GetLength(1)); // height

                    // write pixel information
                    foreach (ColorInfo info in tiles)
                    {
                        // write character code
                        if (info.Brightness > 0.875)
                            bw.Write(0);
                        else if (info.Brightness > 0.625)
                            bw.Write(176);
                        else if (info.Brightness > 0.375)
                            bw.Write(177);
                        else if (info.Brightness > 0.125)
                            bw.Write(178);
                        else
                            bw.Write(219);

                        const double multiplier = 0.05;
                        // calculate a shade for the foreground color
                        bw.Write((byte)Math.Max(info.R - 20, 0));
                        bw.Write((byte)Math.Max(info.G - 20, 0));
                        bw.Write((byte)Math.Max(info.B - 20, 0));

                        // calculate a tint for the background color
                        bw.Write((byte)Math.Min(info.R + 20, 255));
                        bw.Write((byte)Math.Min(info.G + 20, 255));
                        bw.Write((byte)Math.Min(info.B + 20, 255));
                    }
                }

                memStream.Flush();
                byte[] buffer = memStream.GetBuffer();

                using (GZipStream zip = new GZipStream(File.OpenWrite("out.xp"), CompressionMode.Compress))
                {
                    zip.Write(buffer, 0, buffer.Length);
                }
            }
        }

        private static void WriteToText(ColorInfo[,] tiles)
        {
            using (StreamWriter sw = new StreamWriter("out.txt", false, Encoding.UTF8))
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    for (int x = 0; x < tiles.GetLength(0); x++)
                    {
                        ColorInfo info = tiles[x, y];

                        if (info.Brightness > 0.875)
                        {
                            sw.Write(' ');
                        }
                        else if (info.Brightness > 0.625)
                        {
                            sw.Write(Strings.ChrW('\u2591'));
                        }
                        else if (info.Brightness > 0.375)
                        {
                            sw.Write(Strings.ChrW('\u2592'));
                        }
                        else if (info.Brightness > 0.125)
                        {
                            sw.Write(Strings.ChrW('\u2593'));
                        }
                        else
                        {
                            sw.Write(Strings.ChrW('\u2588'));
                        }
                    }

                    sw.WriteLine();
                }
            }
        }

        private static ColorInfo[,] AverageColorInfo(Bitmap image, int lineHeight, int lineWidth)
        {
            int width = image.Width / lineWidth;
            int height = image.Height / lineHeight;
            int pixelCount = width * height;

            // foreach goes through the rightmost index first
            ColorInfo[,] output = new ColorInfo[lineWidth, lineHeight];

            for (int y = 0; y < lineHeight; y++)
            {
                for (int x = 0; x < lineWidth; x++)
                {
                    float totalBright = 0;
                    int totalRed = 0;
                    int totalGreen = 0;
                    int totalBlue = 0;

                    for (int a = x * width; a < (x + 1) * width; a++)
                    {
                        for (int b = y * height; b < (y + 1) * height; b++)
                        {
                            Color pixel = image.GetPixel(a, b);
                            totalBright += pixel.GetBrightness();
                            totalRed += pixel.R;
                            totalGreen += pixel.G;
                            totalBlue += pixel.B;
                        }
                    }

                    float avgBright = totalBright / pixelCount;
                    byte avgRed = (byte)(totalRed / pixelCount);
                    byte avgGreen = (byte)(totalGreen / pixelCount);
                    byte avgBlue = (byte)(totalBlue / pixelCount);

                    output[x, y] = new ColorInfo(avgRed, avgGreen, avgBlue, avgBright);
                }
            }

            return output;
        }
    }

    struct ColorInfo
    {
        internal byte R { get; }
        internal byte G { get; }
        internal byte B { get; }
        internal float Brightness { get; }

        public ColorInfo(byte r, byte g, byte b, float bright)
        {
            R = r;
            G = g;
            B = b;
            Brightness = bright;
        }
    }
}
