using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace PolygonFiller
{
    public class Edge
    {
        public List<Vertex> Vertices { get; set; }
        public Edge(Vertex v1, Vertex v2)
        {
            Vertices = new List<Vertex> { v1, v2 };
        }


        public bool IsPointInEdge(Point point)
        {
            List<Point> pixels = Bresenham.CalculateBresenhamLine((int)Vertices[0].GetX(), (int)Vertices[0].GetY(), (int)Vertices[1].GetX(), (int)Vertices[1].GetY(), out List<Point> v);
            foreach (var pixel in pixels)
            {
                if (point.X >= pixel.X - 5 && point.X <= pixel.X + 5 && point.Y >= pixel.Y - 5 && point.Y <= pixel.Y + 5)
                    return true;
            }
            return false;
        }

        public Point GetMiddlePoint()
        {
            double x = Math.Abs((Vertices[0].GetX() + Vertices[1].GetX()) / 2);
            double y = Math.Abs((Vertices[0].GetY() + Vertices[1].GetY()) / 2);
            return new Point(x, y);
        }

        public int GetLength()
        {
            double a = Math.Abs(Vertices[0].GetX() - Vertices[1].GetX());
            double b = Math.Abs(Vertices[0].GetY() - Vertices[1].GetY());
            return (int)Math.Sqrt(a * a + b * b);
        }

        public double GetMinY()
        {
            return (Vertices[0].GetY() > Vertices[1].GetY() ? Vertices[1].GetY() : Vertices[0].GetY());
        }

        public double GetMaxY()
        {
            return (Vertices[0].GetY() > Vertices[1].GetY() ? Vertices[0].GetY() : Vertices[1].GetY());
        }

        public double GetMinX()
        {
            return (Vertices[0].GetX() > Vertices[1].GetX() ? Vertices[1].GetX() : Vertices[0].GetX());
        }

        public double GetMaxX()
        {
            return (Vertices[0].GetX() > Vertices[1].GetX() ? Vertices[0].GetX() : Vertices[1].GetX());
        }

        public Vertex GetMinYVertex()
        {
            return (Vertices[0].GetY() > Vertices[1].GetY() ? Vertices[1] : Vertices[0]);
        }
    }
}