using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace PolygonFiller
{
    public class ColoringModel
    {
        public bool FilledWithColor { get; set; }
        public bool FilledWithTexture { get; set; }
        public bool ChosenNormalMap { get; set; }
        public bool UseHeightMap { get; set; }
        public double DistortionCoefficient { get; set; }
        public Color Color { get; set; }
        private byte[] bitmapPixels;
        private byte[] normalMapPixels;
        private byte[] heightMapPixels;
        private int stride;
        private int mapStride;
        private int heightStride;
        public BitmapImage Texture { get; set; }


        public ColoringModel()
        {
            DistortionCoefficient = 1;
        }

        public void SetTexture(BitmapImage bmp)
        {
            stride = bmp.PixelWidth * 4;
            int size = bmp.PixelHeight * stride;
            bitmapPixels = new byte[size];
            bmp.CopyPixels(bitmapPixels, stride, 0);
        }

        public void SetNormalMap(BitmapImage bmp)
        {
            mapStride = bmp.PixelWidth * 4;
            int size = bmp.PixelHeight * mapStride;
            normalMapPixels = new byte[size];
            bmp.CopyPixels(normalMapPixels, mapStride, 0);
        }

        public void SetHeightMap(BitmapImage bmp)
        {
            heightStride = bmp.PixelWidth * 4;
            int size = bmp.PixelHeight * heightStride;
            heightMapPixels = new byte[size];
            bmp.CopyPixels(heightMapPixels, heightStride, 0);
        }

        public List<List<Color>> GetBresenhamColorLists(List<Edge> pixelPairs)
        {
            if (FilledWithColor)
                return GetSolidColorLists(pixelPairs);
            return GetColorLists(pixelPairs, bitmapPixels, stride);
        }

        public List<List<Point3D>> GetNormalMapColors(List<Edge> pixelPairs)
        {
            List<List<Point3D>> normalPoints = new List<List<Point3D>>();
            int y = 0;
           foreach(Edge pair in pixelPairs)
            {
                normalPoints.Add(new List<Point3D>());
                List<Point> pixels = Bresenham.CalculateBresenhamLine((int)pair.Vertices[0].GetX(), (int)pair.Vertices[0].GetY(), (int)pair.Vertices[1].GetX(), (int)pair.Vertices[1].GetY(), out List<Point> tmp);
                foreach (Point p in pixels)
                {
                    normalPoints[y].Add(GetNPrim((int)p.X, (int)p.Y, y));
                }
                y++;
            }
            return normalPoints;
        }
        private List<List<Color>> GetColorLists(List<Edge> pixelPairs, byte[] bitmapPix, int stride)
        {
            List<List<Color>> colors = new List<List<Color>>();
            int y = 0;
            foreach (Edge pair in pixelPairs)
            {
                colors.Add(new List<Color>());
                List<Point> pixels = Bresenham.CalculateBresenhamLine((int)pair.Vertices[0].GetX(), (int)pair.Vertices[0].GetY(), (int)pair.Vertices[1].GetX(), (int)pair.Vertices[1].GetY(), out List<Point> tmp);
                foreach (Point p in pixels)
                {
                    int index = (int)(y * stride + 4 * p.X);
                    while (index + 3 >= bitmapPix.Length)
                        index -= bitmapPix.Length;
                    Color c = new Color
                    {
                        B = bitmapPix[index],
                        G = bitmapPix[index + 1],
                        R = bitmapPix[index + 2],
                        A = bitmapPix[index + 3]
                    };
                    colors[y].Add(c);
                }
                y++;
            }
            return colors;
        }

        private List<List<Color>> GetSolidColorLists(List<Edge> pixelPairs)
        {
            List<List<Color>> colors = new List<List<Color>>();
            int y = 0;
            foreach (Edge pair in pixelPairs)
            {
                colors.Add(new List<Color>());
                List<Point> pixels = Bresenham.CalculateBresenhamLine((int)pair.Vertices[0].GetX(), (int)pair.Vertices[0].GetY(), (int)pair.Vertices[1].GetX(), (int)pair.Vertices[1].GetY(), out List<Point> tmp);
                foreach (Point p in pixels)
                    colors[y].Add(Color);
                y++;
            }
            return colors;
        }

        private Point3D Normalize(double x, double y, double z)
        {
            var length = Math.Sqrt(x * x + y * y + z * z);
            return new Point3D(x / length, y / length, z / length);
        }


        private Point3D GetNPrim(int xPix, int yPix, int yprim)
        {
            double x = 0;
            double y = 0;
            double z = 1;
            if (ChosenNormalMap)
            {
                var c = GetBitmapPixelColor(normalMapPixels, mapStride, new Point(xPix, yPix), yprim);
                x = (2 * (c.R / 255.0)) - 1;
                y = (2 * (c.G / 255.0)) - 1;
                z = c.B / 255.0;
            }

            var D = new double[3];
            if (UseHeightMap)
            {
                var rightPixel = GetBitmapPixelColor(heightMapPixels, heightStride, new Point(xPix + 1, yPix), yprim);
                var middlePixel = GetBitmapPixelColor(heightMapPixels, heightStride, new Point(xPix, yPix), yprim);
                var upperPixel = GetBitmapPixelColor(heightMapPixels, heightStride, new Point(xPix, yPix+1), yprim);

                D = new double[]{
                    (rightPixel.R - middlePixel.R) * DistortionCoefficient,
                    (upperPixel.G - middlePixel.G) * DistortionCoefficient,
                    (-x * (rightPixel.B - middlePixel.B) + -y * (upperPixel.B - middlePixel.B)) * DistortionCoefficient
                };
            }
            x += D[0];
            y += D[1];
            z += D[2];
            return Normalize(x, y, z);
        }

        private Color GetBitmapPixelColor(byte[] pixels, int stride, Point p, int y)
        {
            int index = (int)(y * stride + 4 * p.X);
            while (index + 3 >= pixels.Length)
                index -= pixels.Length;
            Color c = new Color
            {
                B = pixels[index],
                G = pixels[index + 1],
                R = pixels[index + 2],
                A = pixels[index + 3]
            };
            return c;
        }
    }
}