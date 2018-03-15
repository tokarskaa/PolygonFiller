﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PolygonFiller
{
    public static class Scanline
    {
        private static EdgeTable PrepareEdgeTable(Polygon polygon)
        {
            EdgeTable edgeTable = new EdgeTable((int)polygon.GetMaxY()+1);
            foreach(Edge e in polygon.Edges)
            {
                if (e.Vertices[0].GetY() == e.Vertices[1].GetY())
                    continue;
                EdgeNode node = new EdgeNode(e);
                int index = (int)e.GetMinY();
                edgeTable.AddAtIndex(index, node);
            }
            return edgeTable;
        }

        public static List<Edge> GetLinesToFill(Polygon polygon)
        {
            EdgeTable edgeTable = PrepareEdgeTable(polygon);
            EdgeList activeEdgeTable = new EdgeList();
            List<Edge> pixelPairs = new List<Edge>();
            int y = (int)polygon.GetMinY();
            while (edgeTable.EdgeCount != 0 || activeEdgeTable.EdgeCount != 0)
            {
                EdgeList bucket = edgeTable.GetListAtIndex(y);
                if (bucket != null)
                {
                    activeEdgeTable.Append(bucket);
                }
                activeEdgeTable.SortByX();
                activeEdgeTable.DeleteFinishedLines(y);
                for (int i = 0; i < activeEdgeTable.EdgeCount; i += 2)
                {
                    pixelPairs.Add(new Edge(new Vertex(new Point(activeEdgeTable[i].Xmin, y)), new Vertex(new Point(activeEdgeTable[i + 1].Xmin, y))));
                }
                y++;
                activeEdgeTable.UpdateXmins();
            }
            return pixelPairs;
        }
    }
}
