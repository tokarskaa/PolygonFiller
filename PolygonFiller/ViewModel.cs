using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace PolygonFiller
{
    class ViewModel : INotifyPropertyChanged
    {
        public byte[] BitmapArraySource;
        int ArrayLenght;
        private WriteableBitmap wbm;
        public List<Polygon> Polygons { get; set; }
        public Polygon Polygon { get; set; }
        public Point Start { get; set; }
        public Point NewLine { get; set; }

        public Vertex DraggedVertex;
        public Vertex ToBeDeleted { get; set; }
        public Edge SelectedLine { get; set; }
        public Point Waypoint;
        public Point center;
        private RadioButtonsOptions objectColorOpt;
        public RadioButtonsOptions ObjectColorOpt
        {
            get { return objectColorOpt; }
            set { objectColorOpt = value; NotifyPropertyChanged("ObjectColorOpt"); }
        }
        private RadioButtonsOptions normalMapOpt;
        public RadioButtonsOptions NormalMapOpt
        {
            get { return normalMapOpt; }
            set { normalMapOpt = value; NotifyPropertyChanged("NormalMapOpt"); }
        }
        private RadioButtonsOptions distMapOpt;
        public RadioButtonsOptions DistMapOpt
        {
            get { return distMapOpt; }
            set { distMapOpt = value; NotifyPropertyChanged("DistMapOpt"); }
        }
        private Color objectColor;
        public Color ObjectColor
        {
            get { return objectColor; }
            set { objectColor = value; NotifyPropertyChanged("ObjectColor"); }
        }
        private BitmapImage chosenTexture;
        public BitmapImage ChosenTexture
        {
            get { return chosenTexture; }
            set { chosenTexture = value; NotifyPropertyChanged("ChosenTexture"); }
        }
        private BitmapImage chosenNormalMapTexture;
        public BitmapImage ChosenNormalMapTexture
        {
            get { return chosenNormalMapTexture; }
            set { chosenNormalMapTexture = value; NotifyPropertyChanged("ChosenNormalMapTexture"); }
        }
        private BitmapImage chosenDistTexture;
        public BitmapImage ChosenDistTexture
        {
            get { return chosenDistTexture; }
            set { chosenDistTexture = value; NotifyPropertyChanged("ChosenDistTexture"); }
        }
        private double coefficient;
        public double Coefficient
        {
            get { return coefficient; }
            set { coefficient = value; NotifyPropertyChanged("Coefficient"); }
        }
        public IlluminationModel IlluminationModel { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public List<Point> Line { get; set; }
        public List<Edge> Adjacent { get; set; }
        public ViewModel(int arrayLength, Point center, WriteableBitmap wbmap)
        {
            wbm = wbmap;
            this.center = center;
            BitmapArraySource = new byte[arrayLength];
            Polygon = new Polygon();
            ArrayLenght = arrayLength;
            Polygons = new List<Polygon>();
            IlluminationModel = new IlluminationModel(center);
            Coefficient = 1;
        }


        public void DrawPolygon(WriteableBitmap wbm, Polygon polygon, Color color)
        {
            foreach (Edge e in polygon.Edges)
            {
                wbm.DrawBresenhamLine((int)e.Vertices[0].GetX(), (int)e.Vertices[0].GetY(), (int)e.Vertices[1].GetX(), (int)e.Vertices[1].GetY(), color);
                wbm.DrawBresenhamCircle((int)e.Vertices[0].GetX(), (int)e.Vertices[0].GetY(), 6);
            }
        }

        private void FillPolygon(WriteableBitmap wbm, Polygon polygon, Color color)
        {
            List<Edge> pixelPairs = Scanline.GetLinesToFill(polygon);
            List<List<Color>> colors = polygon.GetColorLists(pixelPairs);
            List<List<Point3D>> normalMapColors = polygon.GetNormalMapLists(pixelPairs);
            for (int i = 0; i < pixelPairs.Count; i++)
            {
                int y = (int)pixelPairs[i].Vertices[0].GetY();
                List<Point3D> normalColors = normalMapColors?[i];
                IlluminationModel.ApplyIllumination(pixelPairs[i], colors[i], normalColors);
                wbm.DrawBresenhamLine((int)pixelPairs[i].Vertices[0].GetX(), y, (int)pixelPairs[i].Vertices[1].GetX(), y, colors[i]);
            }
        }

        public void DrawPolygons(WriteableBitmap wbm)
        {
            byte[] array = new byte[ArrayLenght];
            wbm.WritePixels(new Int32Rect(0, 0, wbm.PixelWidth, wbm.PixelHeight), array, wbm.BackBufferStride, 0);
            foreach (var p in Polygons)
            {
                DrawPolygon(wbm, p, p.GetColor());
                if (p.ColorOpt == RadioButtonsOptions.constant || p.ColorOpt == RadioButtonsOptions.fromTexture)
                    FillPolygon(wbm, p, p.GetColor());
            }

        }

        public void MovePolygon(Point waypoint, Point point, out Point newWaypoint)
        {
            double XDifference = point.X - waypoint.X;
            double YDifference = point.Y - waypoint.Y;
            newWaypoint = point;
            Polygon.MovePolygon(new Point(XDifference, YDifference));
        }

        public void AddPolygon()
        {
            Polygon p = new Polygon();
            Polygons.Add(p);
            Polygon = p;
        }

        public bool IsPointInPolygonVertices(Point point)
        {
            foreach (var p in Polygons)
            {
                if (p.IsPointInVertices(point))
                {
                    Polygon = p;
                    return true;
                }
            }
            return false;
        }

        public bool IsPointInPolygonLines(Point point)
        {
            foreach (var p in Polygons)
            {
                if (p.IsPointInEdges(point))
                {
                    Polygon = p;
                    return true;
                }
            }
            return false;
        }

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }

    public enum RadioButtonsOptions
    {
        none,
        constant,
        fromTexture,
        animated
    };
}
