using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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

            Bitmap image = new Bitmap(imagePath);
            Console.WriteLine($"The image is {image.Height} x {image.Width}");

            if (lineWidth > image.Width)
                lineWidth = image.Width;
            int lineHeight = lineWidth * image.Height / image.Width;

            Stopwatch timer = new Stopwatch();
            timer.Start();

            using (StreamWriter sw = new StreamWriter("out.txt", false, Encoding.UTF8))
            {
                foreach (ColorInfo info in AverageColorInfo(image, lineHeight, lineWidth))
                {
                    if (info.X == 0)
                        sw.WriteLine();

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
            }

            timer.Stop();
            Console.WriteLine($"Converted image in {timer.Elapsed}");
        }

        private static IEnumerable<ColorInfo> AverageColorInfo(Bitmap image, int lineHeight, int lineWidth)
        {
            int width = image.Width / lineWidth;
            int height = image.Height / lineHeight;
            int pixelCount = width * height;

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

                    yield return new ColorInfo(x, y, avgRed, avgGreen, avgBlue, avgBright);
                }
            }
        }
    }

    struct ColorInfo
    {
        internal int X { get; }
        internal int Y { get; }
        internal byte R { get; }
        internal byte G { get; }
        internal byte B { get; }
        internal float Brightness { get; }

        public ColorInfo(int x, int y, byte r, byte g, byte b, float bright)
        {
            X = x;
            Y = y;
            R = r;
            G = g;
            B = b;
            Brightness = bright;
        }
    }
}
