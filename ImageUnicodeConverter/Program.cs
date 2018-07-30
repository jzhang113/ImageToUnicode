using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
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

            if (lineWidth > 2 * image.Width)
                lineWidth = image.Width;
            int lineHeight = lineWidth * image.Height / image.Width;

            Stopwatch timer = new Stopwatch();
            timer.Start();

            using (StreamWriter sw = new StreamWriter(File.OpenWrite("out.txt"), Encoding.UTF8))
            {
                foreach (ColorInfo info in AverageColorInfo(image, lineHeight, lineWidth))
                {
                    if (info.X == 0)
                        sw.WriteLine();

                    if (info.Brightness > 0.875)
                    {
                        sw.Write(Strings.ChrW('\u2588'));
                    }
                    else if (info.Brightness > 0.625)
                    {
                        sw.Write(Strings.ChrW('\u2593'));
                    }
                    else if (info.Brightness > 0.375)
                    {
                        sw.Write(Strings.ChrW('\u2592'));
                    }
                    else if (info.Brightness > 0.125)
                    {
                        sw.Write(Strings.ChrW('\u2591'));
                    }
                    else
                    {
                        sw.Write(' ');
                    }
                }
            }

            timer.Stop();
            Console.WriteLine($"Converted image in {timer.Elapsed}");
        }

        private static IEnumerable<ColorInfo> AverageColorInfo(Bitmap image, int lineHeight, int lineWidth)
        {
            for (int y = 0; y < lineHeight; y++)
            {
                for (int x = 0; x < lineWidth; x++)
                {
                    Rectangle rect = new Rectangle(
                        x * image.Width / lineWidth,
                        y * image.Height / lineHeight,
                        image.Width / lineWidth,
                        image.Height / lineHeight);

                    Bitmap crop = image.Clone(rect, PixelFormat.DontCare);

                    float totalBright = 0;
                    int totalRed = 0;
                    int totalGreen = 0;
                    int totalBlue = 0;
                    int pixelCount = crop.Width * crop.Height;
                    for (int a = 0; a < crop.Width; a++)
                    {
                        for (int b = 0; b < crop.Height; b++)
                        {
                            Color pixel = crop.GetPixel(a, b);
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
