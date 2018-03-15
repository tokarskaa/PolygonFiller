using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace PolygonFiller
{
    public class Polygon
    {
        public List<Edge> Edges { get; set; }
        public RadioButtonsOptions ColorOpt { get; set; }
        public ColoringModel ColoringModel { get; set; }
        public Polygon()
        {
            Edges = new List<Edge>();
            ColoringModel = new ColoringModel
            {
                Color = Colors.Black
            };
        }

        public bool IsStartPoint(Point point)
        {
            
            if (Edges.Count == 0)
                return false;
            return Edges[0].Vertices[0].IsPointInVertex(point);
        }

        public void AddLine(Edge e)
        {
            if (Edges.Count != 0)
            {
                e.Vertices[0] = Edges[Edges.Count - 1].Vertices[1];
                if (e.Vertices[1].Equals(Edges[0].Vertices[0]))
                    e.Vertices[1] = Edges[0].Vertices[0];
            }
            Edges.Add(e);
        }
        public Vertex GetVertexFromPoint(Point point)
        {
            if (Edges.Count == 0)
                return null;
            foreach (var edge in Edges)
            {
                if (edge.Vertices[0].IsPointInVertex(point))
                    return edge.Vertices[0];
            }
            return null;
        }

        public Edge GetEdgeFromPoint(Point point)
        {
            if (Edges.Count == 0)
                return null;
            foreach (var edge in Edges)
            {
                if (edge.IsPointInEdge(point))
                    return edge;
            }
            return null;
        }

        public bool IsPointInVertices(Point point)
        {
            if (GetVertexFromPoint(point) == null)
                return false;
            return true;
        }

        public bool IsPointInEdges(Point point)
        {
            if (GetEdgeFromPoint(point) == null)
                return false;
            return true;
        }

        public List<Edge> GetAdjacentEdges(Point point)
        {
            Edge l1 = null, l2 = null;
            List<Edge> adjacent = new List<Edge>();
            foreach (Edge edge in Edges)
            {
                if (edge.Vertices[0].IsPointInVertex(point))
                    l1 = edge;
                if (edge.Vertices[1].IsPointInVertex(point))
                    l2 = edge; 
            }
            adjacent.Add(l1);
            adjacent.Add(l2);
            return adjacent;
        }

        public List<Edge> GetAdjacentEdges(Edge e)
        {
            List<Edge> adjacent = GetAdjacentEdges(e.Vertices[0].MiddlePoint);
            adjacent.AddRange(GetAdjacentEdges(e.Vertices[1].MiddlePoint));
            adjacent.Distinct();
            return adjacent;
        }

        public void MoveVertex(Vertex vertex, Point point, out Vertex newVertex)
        {
            List<Edge> edges = Edges.FindAll(x => x.Vertices[0].Equals(vertex) || x.Vertices[1].Equals(vertex));
            newVertex = Vertex.GetVertexFromMiddlePoint(point, 6);
            foreach (Edge e in edges)
            {
                if (e.Vertices[0].Equals(vertex))
                    e.Vertices[0] = newVertex;
                else
                    e.Vertices[1] = newVertex;
            }
        }

        public void DeleteVertex(Vertex v)
        {
            if (Edges.Count <= 3)
                return;
            List<Edge> adjacent = GetAdjacentEdges(v.MiddlePoint);
            adjacent[0].Vertices[0] = adjacent[1].Vertices[0];
            Edges.Remove(adjacent[1]);
        }

        public void AddVertexOnLine(Edge e)
        {
            Vertex newVertex = new Vertex(e.GetMiddlePoint());
            Edge newEdge = new Edge(e.Vertices[0], newVertex);
            int index = Edges.FindIndex(x => x.Vertices[0].Equals(e.Vertices[0]) && x.Vertices[1].Equals(e.Vertices[1]));
            Edges.Insert(index + 1, newEdge);
            index = Edges.FindIndex(x => x.Vertices[1].Equals(e.Vertices[1]));
            Edges[index].Vertices[0] = newVertex;
        }

        public void MovePolygon(Point vector)
        {
            foreach (var edge in Edges)
                edge.Vertices[0].SetMiddlePoint(new Point(edge.Vertices[0].GetX() + vector.X, edge.Vertices[0].GetY() + vector.Y));
        }

        public void ResizeBitmap()
        {
            if (ColoringModel.Texture == null)
                return;
            double x = ColoringModel.Texture.DecodePixelWidth - Math.Abs(GetMaxX() - GetMinX());
            double y = ColoringModel.Texture.DecodePixelHeight - Math.Abs(GetMaxY() - GetMinY());
            if (x > 0 && y > 0)
                return;
            if (x < y)
                ColoringModel.Texture.DecodePixelWidth = (int)Math.Abs(GetMaxX() - GetMinX()); 
            else
                ColoringModel.Texture.DecodePixelHeight = (int)Math.Abs(GetMaxY() - GetMinY());

        }

        public double GetMinY()
        {
            double min = int.MaxValue;
            foreach (Edge e in Edges)
                if (e.GetMinY() < min)
                    min = e.GetMinY(); 
            return min;
        }

        public double GetMaxY()
        {
            double max = 0;
            foreach (Edge e in Edges)
                if (e.GetMaxY() > max)
                    max = e.GetMaxY();
            return max;
        }

        public double GetMinX()
        {
            double min = int.MaxValue;
            foreach (Edge e in Edges)
                if (e.GetMinX() < min)
                    min = e.GetMinX();
            return min;
        }

        public double GetMaxX()
        {
            double max = 0;
            foreach (Edge e in Edges)
                if (e.GetMaxX() > max)
                    max = e.GetMaxX();
            return max;
        }

        public void SetColor(Color c)
        {
            ColoringModel.Color = c;
            ColoringModel.FilledWithColor = true;
            ColoringModel.FilledWithTexture = false;
        }

        public Color GetColor()
        {
            return ColoringModel.Color;
        }
        public void SetTexture(BitmapImage bmp)
        {
            ColoringModel.SetTexture(bmp);
            ColoringModel.FilledWithColor = false;
            ColoringModel.FilledWithTexture = true;
        }

        public void UnsetColorAndTexture()
        {
            ColoringModel.FilledWithColor = false;
            ColoringModel.FilledWithTexture = false;
        }

        public void SetNormalMap(BitmapImage bmp)
        {
            ColoringModel.ChosenNormalMap = true;
            ColoringModel.SetNormalMap(bmp);
        }

        public void UnsetNormalMap()
        {
            ColoringModel.ChosenNormalMap = false;
        }

        public void SetHeightMap(BitmapImage bmp)
        {
            ColoringModel.SetHeightMap(bmp);
            ColoringModel.UseHeightMap = true;
        }

        public void UnsetHeightMap()
        {
            ColoringModel.UseHeightMap = false;
        }


        public List<List<Color>> GetColorLists(List<Edge> pixelPairs)
        {
            return ColoringModel.GetBresenhamColorLists(pixelPairs);
        }

        public List<List<Point3D>> GetNormalMapLists(List<Edge> pixelPairs)
        {
            return ColoringModel.GetNormalMapColors(pixelPairs);
        }

        public void SetCoefficient(double coeff)
        {
            ColoringModel.DistortionCoefficient = coeff;
        }
    }
}