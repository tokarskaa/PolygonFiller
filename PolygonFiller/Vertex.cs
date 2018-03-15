using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace PolygonFiller
{
    public class Vertex
    {
        public Point MiddlePoint { get; set; }
        public Vertex(Point middlePoint)
        {
            MiddlePoint = middlePoint;
        }
        public Vertex(double x, double y)
        {
            MiddlePoint = new Point(x, y);
        }
        public double GetX()
        {
            return MiddlePoint.X;
        }

        public double GetY()
        {
            return MiddlePoint.Y;
        }
        public Point GetPoint()
        {
            return MiddlePoint;
        }
        public void SetMiddlePoint(Point point)
        {
            MiddlePoint = point;
        }

        public static Vertex GetVertexFromMiddlePoint(Point middlePoint, int r)
        {
            return new Vertex(middlePoint);
        }

        public bool IsPointInVertex(Point point)
        {
            List<List<Point>> Pixels = Bresenham.CalculateBresenhamCircle((int)MiddlePoint.X, (int)MiddlePoint.Y, 6);
            foreach (var pair in Pixels)
            {
                if ((int)point.Y == pair[0].Y && (int)point.X >= pair[0].X && (int)point.X <= pair[1].X)
                    return true;
            }
            return false;
        }

        public override bool Equals(object o)
        {
            Vertex v = o as Vertex;
            return (GetX() == v.GetX() && GetY() == v.GetY());
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
