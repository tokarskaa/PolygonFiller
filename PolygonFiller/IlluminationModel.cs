using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace PolygonFiller
{
    class IlluminationModel : INotifyPropertyChanged
    {
        private Point center;

        public event PropertyChangedEventHandler PropertyChanged;
        private Color lightColor;
        public Color LightColor
        {
            get { return lightColor; }
            set { lightColor = value; NotifyPropertyChanged("LightColor"); }
        }

        private int lightRadius;
        public int LightRadius
        {
            get { return lightRadius; }
            set { lightRadius = value; SliderMax = 2 * value; NotifyPropertyChanged("LightRadius"); }
        }
        private int sliderMax;
        public int SliderMax
        {
            get { return sliderMax; }
            set { sliderMax = value; NotifyPropertyChanged("SliderMax"); }
        }
        private int lightPoint;
        public int LightPoint
        {
            get { return lightPoint; }
            set { lightPoint = value; NotifyPropertyChanged("LightPoint"); }
        }

        private RadioButtonsOptions lightVectorOpt;
        public RadioButtonsOptions LightVectorOpt
        {
            get { return lightVectorOpt; }
            set { lightVectorOpt = value; NotifyPropertyChanged("LightVectorOpt"); }
        }

        public IlluminationModel(Point center)
        {
            this.center = center;
            //LightPoint = (int)center.X;
        }
        public Color GetScatteredComponent(Color objectColor, Point3D normalVector, Point3D lightVersor)
        {
            // Point3D lightVersor = GetLightVersor(illuminatedPoint, lightPoint, LightRadius);
            Color component = new Color();
            double cosine = lightVersor.X * normalVector.X + lightVersor.Y * normalVector.Y + lightVersor.Z * normalVector.Z;
            component.ScG = (float)(LightColor.ScG * objectColor.ScG * Math.Max(cosine, 0));
            component.ScB = (float)(LightColor.ScB * objectColor.ScB * Math.Max(cosine, 0));
            component.ScR = (float)(LightColor.ScR * objectColor.ScR * Math.Max(cosine, 0));
            component.A = 255;
            
            return component;
        }

        public void ApplyIllumination(Edge pixelPair, List<Color> pixelColors, List<Point3D> normalMapColors)
        {
            if (LightVectorOpt == RadioButtonsOptions.none)
                return;
            List<Point> pixels = Bresenham.CalculateBresenhamLine((int)pixelPair.Vertices[0].GetX(), (int)pixelPair.Vertices[0].GetY(), (int)pixelPair.Vertices[1].GetX(), (int)pixelPair.Vertices[1].GetY(), out List<Point> tmp);
            for (int i = 0; i < pixelColors.Count; i++)
            {
                Point3D lightVersor = (LightVectorOpt == RadioButtonsOptions.constant) ? new Point3D(0,0,1) : GetLightVersor(new Point3D(pixels[i].X, pixels[i].Y, 0));
                Point3D normalVector = (normalMapColors.Count == 0) ? new Point3D(0, 0, 1) : normalMapColors[i];
                pixelColors[i] = GetScatteredComponent(pixelColors[i], normalVector, lightVersor);
            }
        }

        public Point3D GetLightVersor(Point3D illuminatedPoint)
        {
            double t = center.X - LightRadius + LightPoint;
            double m = Math.Abs(LightRadius - LightPoint);
            double x = Math.Abs(t - illuminatedPoint.X);
            double z = Math.Sqrt(LightRadius * LightRadius - m * m);
            double y = Math.Abs(center.Y - illuminatedPoint.Y);
            double norm = 1 / Math.Sqrt(x * x + y * y + z * z);
            return new Point3D(x*norm, y*norm, z*norm);
        }

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
