using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace PolygonFiller
{
    static class WeilerAtherton
    {
        public static List<Polygon> FindClippingResult(Polygon clipper, Polygon subject)
        {
            List<Polygon> result = new List<Polygon>();
          //  if (Disjunctive(clipper, subject))
            //{
              //  result.Add(clipper);
               // result.Add(subject);
                //return result;
            //}
            List<Point>[] vertexLists = AddIntersections(clipper, subject);
            List<Polygon> check = CheckIfIsInbound(clipper, subject);
            if (check != null) return check;

            int subjectStartPoint = GetOutPointIndex(clipper, vertexLists[1]);
            int index = subjectStartPoint;
            Point subjectPoint = vertexLists[1][subjectStartPoint];
            bool outside = true;
            Polygon p = new Polygon();
            List<Point> polygonPoints = new List<Point>();
            List<List<Point>> polygons = new List<List<Point>>();
            do
            {
                index = (index + 1 >= vertexLists[1].Count) ? 0 : index + 1;
                subjectPoint = vertexLists[1][index];
                if (vertexLists[0].Contains(subjectPoint) && outside)
                {
                    if (polygonPoints.Count > 0 && polygonPoints[0] == subjectPoint)
                    {
                        polygons.Add(CopyList(polygonPoints));
                        polygonPoints.Clear();
                    }
                    polygonPoints.Add(subjectPoint);
                    outside = false;
                    continue;
                }
                else if (vertexLists[0].Contains(subjectPoint) && !outside)
                {
                    outside = true;
                    if (polygonPoints.Count > 0 && polygonPoints[0] == subjectPoint)
                    {
                        polygons.Add(CopyList(polygonPoints));
                        polygonPoints.Clear();
                    }
                    polygonPoints.Add(subjectPoint);
                    int index2 = vertexLists[0].FindIndex(x => x == subjectPoint);
                    int oldIndex = index;
                    do
                    {
                        index2 = (index2 + 1) >= vertexLists[0].Count ? 0 : index2 + 1;
                        if (polygonPoints.Count > 0 && polygonPoints[0] == vertexLists[0][index2])
                        {
                            polygons.Add(CopyList(polygonPoints));
                            polygonPoints.Clear();
                            index = oldIndex;
                            outside = true;
                        }
                        else
                        {
                            index = vertexLists[1].FindIndex(x => x == vertexLists[0][index2]);
                            outside = false;
                            polygonPoints.Add(vertexLists[0][index2]);
                        }
                    } while (!vertexLists[1].Contains(vertexLists[0][index2]));
                    //index = vertexLists[1].FindIndex(x => x == vertexLists[0][index2]);
                }
                else if (!outside)
                {
                    if (polygonPoints.Count > 0 && polygonPoints[0] == subjectPoint)
                    {
                        polygons.Add(CopyList(polygonPoints));
                        polygonPoints.Clear();
                    }
                    polygonPoints.Add(subjectPoint);
                }

            } while (subjectPoint != vertexLists[1][subjectStartPoint]);
            return result = GetPolygonsFromList(polygons);
        }

        private static bool Disjunctive(Polygon p1, Polygon p2)
        {
            List<Point> points = GetVertexList(p1);
            foreach (var point in points)
            {
                if (IsPointInPolygon(point, p2))
                    return false;
            }
            return true;
        }
        private static List<Polygon> GetPolygonsFromList(List<List<Point>> polygons)
        {
            List<Polygon> list = new List<Polygon>();
            foreach (var item in polygons)
                list.Add(GetPolygonFromPointList(item));
            return list;
        }

        private static Polygon GetPolygonFromPointList(List<Point> list)
        {
            Polygon p = new Polygon();
            for (int i = 0; i < list.Count - 1; i++)
                p.AddLine(new Edge(new Vertex(new Point(list[i].X, list[i].Y)), new Vertex(new Point(list[i + 1].X, list[i + 1].Y))));
            p.AddLine(new Edge(new Vertex(new Point(list[list.Count-1].X, list[list.Count-1].Y)), new Vertex(new Point(list[0].X, list[0].Y))));
            return p;

        }
        private static List<Point> CopyList(List<Point> l)
        {
            List<Point> newList = new List<Point>();
            foreach (Point p in l)
            {
                newList.Add(new Point(p.X, p.Y));
            
            }
            return newList;
        }
        private static List<Polygon> CheckIfIsInbound(Polygon clipper, Polygon subject)
        {
            List<Polygon> result = new List<Polygon>();
            List<Point>[] vertexLists = AddIntersections(clipper, subject);
            Point? subjectOutPoint = GetSubjectStartPoint(clipper, vertexLists[1]);
            Point? clipperOutPoint = GetSubjectStartPoint(subject, vertexLists[0]);
            if (subjectOutPoint == null)
            {
                result.Add(subject);
                return result;
            }
            else if (clipperOutPoint == null)
            {
                result.Add(clipper);
                return result;
            }
            return null;
        }
        private static Point? GetSubjectStartPoint(Polygon clipper, List<Point> subjectPoints)
        {
            for (int i = 0; i < subjectPoints.Count; i++)
            {
                if (!IsPointInPolygon(subjectPoints[i], clipper))
                    return subjectPoints[i];
            }
            return null;
        }

       
        private static int GetOutPointIndex(Polygon clipper, List<Point> subjectPoints)
        {
            for (int i = 0; i < subjectPoints.Count; i++)
            {
                if (!IsPointInPolygon(subjectPoints[i], clipper))
                    return i;
            }
            return -1;
        }
        public static List<Point>[] AddIntersections(Polygon p1, Polygon p2)
        {
            List<Point> vertexList1 = GetVertexList(p1);
            List<Point> vertexList2 = GetVertexList(p2);
           // PrintList(vertexList1);
          //  PrintList(vertexList2);
            List<Tuple<Point, Edge>> intersections1 = new List<Tuple<Point, Edge>>();
            List<Tuple<Point, Edge>> intersections2 = new List<Tuple<Point, Edge>>();
            foreach (Edge e1 in p1.Edges)
                foreach (Edge e2 in p2.Edges)
                    if (DoIntersect(e1.Vertices[0].GetPoint(), e1.Vertices[1].GetPoint(), e2.Vertices[0].GetPoint(), e2.Vertices[1].GetPoint()))
                    {
                        intersections1.Add(new Tuple<Point, Edge>(FindIntersection(e1, e2), e1));
                        intersections2.Add(new Tuple<Point, Edge>(FindIntersection(e1, e2), e2));
                    }
            InsertIntersections(vertexList1, intersections1);
            InsertIntersections(vertexList2, intersections2);
            List<Point>[] result = new List<Point>[2];
            result[0] = vertexList1;
            result[1] = vertexList2;
            return result;
          //  PrintList(vertexList1);
           // PrintList(vertexList2);

        }

        private static void PrintList(List<Point> l)
        {
            Console.WriteLine("Vertex list: ");
            foreach (var item in l)
            {
                Console.WriteLine("x: " + item.X + " y: " + item.Y);
            }
        }

        private static void InsertIntersections(List<Point> list, List<Tuple<Point, Edge>> intersections)
        {
            while (intersections.Count > 0)
            {
                List<Point> sameEdge = new List<Point>();
                Edge e = intersections[0].Item2;
                foreach (var tuple in intersections)
                    if (tuple.Item2 == e)
                        sameEdge.Add(tuple.Item1);
                intersections.RemoveAll(x => x.Item2 == e);
                InsertIntersectionsOnEdge(list, e, sameEdge);
            }
        }

        private static void InsertIntersectionsOnEdge(List<Point> list, Edge e, List<Point> intersections)
        {
            int index = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == e.Vertices[0].GetPoint())
                {
                    index = i+1;
                    break; 
                }
            }
            intersections.Sort(new PointLengthComparer(e.Vertices[0].GetPoint()));
            list.InsertRange(index, intersections);
        }

       
        private static List<Point> GetVertexList(Polygon p)
        {
            List<Point> vertexList = new List<Point>();
            foreach (Edge e in p.Edges)
                vertexList.Add(e.Vertices[0].GetPoint());
            return vertexList;
        }
        private static bool OnEdge(Point p, Point q, Point r)
        {
            if (q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) &&
                q.Y <= Math.Max(p.Y, r.Y) && q.Y >= Math.Min(p.Y, r.Y))
                return true;

            return false;
        }

        private static int Orientation(Point p, Point q, Point r)
        {
            int val = (int)((q.Y - p.Y) * (r.X - q.X) - (q.X - p.X) * (r.Y - q.Y));

            if (val == 0) return 0; 

            return (val > 0) ? 1 : 2; 
        }

        private static bool DoIntersect(Point p1, Point q1, Point p2, Point q2)
        {
            int o1 = Orientation(p1, q1, p2);
            int o2 = Orientation(p1, q1, q2);
            int o3 = Orientation(p2, q2, p1);
            int o4 = Orientation(p2, q2, q1);

            if (o1 != o2 && o3 != o4)
                return true;

            if (o1 == 0 && OnEdge(p1, p2, q1)) return true;

            if (o2 == 0 && OnEdge(p1, q2, q1)) return true;

            if (o3 == 0 && OnEdge(p2, p1, q2)) return true;

            if (o4 == 0 && OnEdge(p2, q1, q2)) return true;

            return false; 
        }

        private static Point FindIntersection(Edge e1, Edge e2)
        {
            double?[] f1 = GetLinearFunction(e1);
            double?[] f2 = GetLinearFunction(e2);
            double x, y;
            if (f1[0] == null)
            {
                x = e1.Vertices[0].GetX();
                y = f2[0].Value * x + f2[1].Value;
            }
            else if (f2[0] == null)
            {
                x = e2.Vertices[0].GetX();
                y = f1[0].Value * x + f1[1].Value;
            }
            else
            {
                x = (f2[1].Value - f1[1].Value) / (f1[0].Value - f2[0].Value);
                y = f1[0].Value * x + f1[1].Value;
            }
            return new Point(x, y);
        }

        private static double?[] GetLinearFunction(Edge e1)
        {
            double?[] result = new double?[2];
            double x1 = e1.Vertices[0].GetX(); double y1 = e1.Vertices[0].GetY();
            double x2 = e1.Vertices[1].GetX(); double y2 = e1.Vertices[1].GetY();
            if (x1-x2 == 0)
                result[0] = result[1] = null;
            else
            {
                result[0] = (y1 - y2) / (x1 - x2);
                result[1] = y1 - x1 * result[0];
            }
            return result;
        }

        private static bool IsPointInPolygon(Point p, Polygon polygon)
        {
            int count = 0;
            foreach (Edge e in polygon.Edges)
                if (DoIntersect(new Point(0, p.Y), p, e.Vertices[0].GetPoint(), e.Vertices[1].GetPoint()))
                    count++;
            return count % 2 != 0;
        }
    }

    class PointLengthComparer : IComparer<Point>
    {
        private Point p3;
        public PointLengthComparer(Point p3)
        {
            this.p3 = p3;
        }
        public int Compare(Point x, Point y)
        {
            if (GetLenght(x, p3) == GetLenght(y, p3))
                return 0;
            if(GetLenght(x, p3) > GetLenght(y, p3))
                return 1;
            return -1;
        }

        private static double GetLenght(Point p1, Point p2)
        {
            double a = Math.Abs(p1.X - p2.X);
            double b = Math.Abs(p1.Y - p2.Y);
            return Math.Sqrt(a * a + b * b);
        }
    }
}