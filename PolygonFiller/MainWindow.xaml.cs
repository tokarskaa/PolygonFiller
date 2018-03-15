using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PolygonFiller
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WriteableBitmap wbmap;
        bool IsDrawing;
        bool VertexDrag;
        bool PolygonDrag;
        List<Point> line;
        ViewModel vm;
        bool polygonFinished;
        bool polygonStarted;
        List<Polygon> ToIntersect;
        public MainWindow()
        {
            wbmap = new WriteableBitmap(1067, 657, 300, 300, PixelFormats.Bgra32, null);
            vm = new ViewModel(wbmap.PixelWidth * wbmap.PixelHeight * wbmap.Format.BitsPerPixel / 8, new Point((int)wbmap.PixelWidth/2, (int)wbmap.PixelHeight/2), wbmap);
            wbmap.CopyPixels(vm.BitmapArraySource, wbmap.BackBufferStride, 0);
            InitializeComponent();
            double width = MainGrid.ColumnDefinitions[1].ActualWidth;
            IsDrawing = false;
            VertexDrag = false;
            PolygonDrag = false;
            polygonFinished = false;
            ToIntersect = new List<Polygon>();
            MainImage.Source = wbmap;
            this.DataContext = vm;
            InitializePolygon();

        }

       
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (vm.Polygons.Count == 0)
                return;
            polygonFinished = false;
            vm.Polygons.RemoveAt(vm.Polygons.Count - 1);
            polygonStarted = false;
            polygonFinished = false;
            vm.DrawPolygons(wbmap);
        }

        private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point mouse = e.GetPosition(MainImage);
            double actheight = MainImage.ActualHeight;
            double actwidth = MainImage.ActualWidth;
            mouse.X = (mouse.X / actwidth) * 1067;
            mouse.Y = (mouse.Y / actheight) * 657;
            if (!polygonFinished && polygonStarted)
            {
                Vertex v = new Vertex(mouse);
                if (!IsDrawing)
                {
                    vm.Start = mouse;
                    IsDrawing = true;
                }
                else
                {
                    if (vm.Polygon.IsStartPoint(mouse))
                    {
                        polygonFinished = true;
                        IsDrawing = false; 
                        Bresenham.CalculateBresenhamLine((int)vm.NewLine.X, (int)vm.NewLine.Y, (int)vm.Polygon.Edges[0].Vertices[0].GetX(), (int)vm.Polygon.Edges[0].Vertices[0].GetY(), out line);
                    }
                    vm.Polygon.AddLine(new Edge(new Vertex(line[0]), new Vertex(line[1])));
                    vm.DrawPolygons(wbmap);
                }
                vm.NewLine = mouse;
            }
            else
            {
                if (vm.IsPointInPolygonVertices(mouse))
                {
                    vm.DraggedVertex = vm.Polygon.GetVertexFromPoint(mouse);
                    vm.Adjacent = vm.Polygon.GetAdjacentEdges(mouse);
                    VertexDrag = true;
                }

                else if (vm.IsPointInPolygonLines(mouse))
                {
                    PolygonDrag = true;
                    vm.Waypoint = mouse;
                }
            }

        }

        private void MainBorder_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
            Point mouse = e.GetPosition(MainImage);
            double actheight = MainImage.ActualHeight;
            double actwidth = MainImage.ActualWidth;
            mouse.X = (mouse.X / actwidth) * 1067;
            mouse.Y = (mouse.Y / actheight) * 657;
            if (mouse.X > 1067 || mouse.X <= 0 || mouse.Y > 657 || mouse.Y <= 0)
                return;
            if (!polygonFinished && polygonStarted)
            {
                if (IsDrawing)
                {
                    vm.DrawPolygons(wbmap);
                    line = wbmap.DrawBresenhamLine((int)vm.NewLine.X, (int)vm.NewLine.Y, (int)mouse.X, (int)mouse.Y, Colors.Black);
                }
            }
            if (polygonFinished)
            {
                if (vm.IsPointInPolygonVertices(mouse))
                    Cursor = Cursors.Hand;
                else if (VertexDrag && e.LeftButton == MouseButtonState.Pressed)
                {
                    Cursor = Cursors.Hand;
                    vm.Polygon.MoveVertex(vm.DraggedVertex, mouse, out vm.DraggedVertex);
                    vm.DrawPolygons(wbmap);
                }
                else if (PolygonDrag && e.LeftButton == MouseButtonState.Pressed)
                {
                    Cursor = Cursors.Hand;
                    vm.MovePolygon(vm.Waypoint, mouse, out vm.Waypoint);
                    vm.DrawPolygons(wbmap);
                }
            }
        }

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point mouse = new Point((int)e.GetPosition(MainImage).X, (int)e.GetPosition(MainImage).Y);
            if (VertexDrag)
            {
                VertexDrag = false;
            }
            if (PolygonDrag)
            {
                PolygonDrag = false;
            }
        }

        private void Grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point mouse = e.GetPosition(MainImage);
            double actheight = MainImage.ActualHeight;
            double actwidth = MainImage.ActualWidth;
            mouse.X = (mouse.X / actwidth) * 1067;
            mouse.Y = (mouse.Y / actheight) * 657;
            if (vm.IsPointInPolygonVertices(mouse))
            {
                vm.ToBeDeleted = vm.Polygon.GetVertexFromPoint(mouse);
                ContextMenu cm = this.FindResource("vertexMenu") as ContextMenu;
                cm.IsOpen = true;
            }
            else if (vm.IsPointInPolygonLines(mouse))
            {
                vm.SelectedLine = vm.Polygon.GetEdgeFromPoint(mouse);
                ContextMenu cm = this.FindResource("lineMenu") as ContextMenu;
                cm.IsOpen = true;
            }
        }

        private void DeleteVertex_Click(object sender, RoutedEventArgs e)
        {
            vm.Polygon.DeleteVertex(vm.ToBeDeleted);
            vm.DrawPolygons(wbmap);
        }

        private void AddVertex_Click(object sender, RoutedEventArgs e)
        {
            vm.Polygon.AddVertexOnLine(vm.SelectedLine);
            vm.DrawPolygons(wbmap);
        }

        private void FillPolygon_Click(object sender, RoutedEventArgs e)
        {
            vm.Polygon.ColorOpt = vm.ObjectColorOpt;
            vm.Polygon.SetCoefficient(vm.Coefficient);
            if (vm.ObjectColorOpt == RadioButtonsOptions.constant)
                vm.Polygon.SetColor(vm.ObjectColor);
            else if (vm.ObjectColorOpt == RadioButtonsOptions.fromTexture)
                vm.Polygon.SetTexture(vm.ChosenTexture);
            else vm.Polygon.UnsetColorAndTexture();

            if (vm.NormalMapOpt == RadioButtonsOptions.fromTexture)
                vm.Polygon.SetNormalMap(vm.ChosenNormalMapTexture);
            else vm.Polygon.UnsetNormalMap();
            if (vm.DistMapOpt == RadioButtonsOptions.fromTexture)
                vm.Polygon.SetHeightMap(vm.ChosenDistTexture);
            else vm.Polygon.UnsetHeightMap();
            vm.DrawPolygons(wbmap);      
        }
        private void Draw_Click(object sender, RoutedEventArgs e)
        {
            polygonStarted = true;
            polygonFinished = false;
            vm.AddPolygon();
        }

      
        private void ChangeTexture_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            string directory = Directory.GetCurrentDirectory() + "\\textures";
            dlg.InitialDirectory = directory;
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                vm.ChosenTexture = new BitmapImage(new Uri(filename));
            }
        }

        private void ChangeNormalMapTexture_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            string directory = Directory.GetCurrentDirectory() + "\\textures";
            dlg.InitialDirectory = directory;
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                vm.ChosenNormalMapTexture = new BitmapImage(new Uri(filename));
            }
        }
        private void ChangeDistTexture_Click(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            string directory = Directory.GetCurrentDirectory() + "\\textures";
            dlg.InitialDirectory = directory;
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                vm.ChosenDistTexture = new BitmapImage(new Uri(filename));
            }
        }
        private void Intersection_Click(object sender, RoutedEventArgs e)
        {
            if (ToIntersect.Count == 0)
            {
                ToIntersect.Add(vm.Polygon);
                MessageBox.Show("Choose second polygon to get an intersection");
            }
            else if(ToIntersect.Count == 1)
            {
                if (vm.Polygon == ToIntersect[0])
                    MessageBox.Show("Choose another polygon");
                else
                {
                    ToIntersect.Add(vm.Polygon);
                    List<Polygon> newPolygons = WeilerAtherton.FindClippingResult(ToIntersect[0], ToIntersect[1]);
                    vm.Polygons.Remove(ToIntersect[0]);
                    vm.Polygons.Remove(ToIntersect[1]);
                    vm.Polygons.AddRange(newPolygons);
                    ToIntersect.Clear();
                    vm.DrawPolygons(wbmap);
                }
            }
        }

        private void ApplyLight_Click(object sender, RoutedEventArgs e)
        {
            vm.DrawPolygons(wbmap);
        }


        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (vm.IlluminationModel.LightVectorOpt == RadioButtonsOptions.animated)
                vm.DrawPolygons(wbmap);
        }

        private void InitializePolygon()
        {
            Polygon p = new Polygon();
            var directory = Directory.GetCurrentDirectory();
            p.AddLine(new Edge(new Vertex(150, 150), new Vertex(200, 150)));
            p.AddLine(new Edge(new Vertex(200, 150), new Vertex(400, 200)));
            p.AddLine(new Edge(new Vertex(200, 400), new Vertex(300, 350)));
            p.AddLine(new Edge(new Vertex(300, 350), new Vertex(150, 150)));
            Polygon p1 = new Polygon();
            p1.AddLine(new Edge(new Vertex(400, 400), new Vertex(600, 400)));
            p1.AddLine(new Edge(new Vertex(600, 400), new Vertex(600, 600)));
            p1.AddLine(new Edge(new Vertex(600, 600), new Vertex(400, 600)));
            p1.AddLine(new Edge(new Vertex(400, 600), new Vertex(400, 400)));
            vm.IlluminationModel.LightColor = Colors.White;
            vm.IlluminationModel.LightVectorOpt = RadioButtonsOptions.constant;
            vm.ChosenTexture = new BitmapImage(new Uri(directory + "/textures/bricks.jpg"));
            vm.ChosenNormalMapTexture = new BitmapImage(new Uri(directory + "/textures/stones.jpg"));
            vm.ChosenDistTexture = new BitmapImage(new Uri(directory + "/textures/stones.jpg"));
            p.SetTexture(vm.ChosenTexture);
            p.SetHeightMap(vm.ChosenDistTexture);
            vm.DistMapOpt = RadioButtonsOptions.fromTexture;
            vm.ObjectColorOpt = RadioButtonsOptions.fromTexture;
            vm.NormalMapOpt = RadioButtonsOptions.fromTexture;
            p.ColorOpt = vm.ObjectColorOpt;
            p.SetNormalMap(vm.ChosenNormalMapTexture);
            vm.Polygons.Add(p);
            vm.Polygons.Add(p1);
            vm.DrawPolygons(wbmap);
            polygonFinished = true;
        }
    }
}
