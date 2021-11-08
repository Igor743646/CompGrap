using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Geometry;
using View;


namespace Lab1
{
    public partial class MainWindow : Window
    {
        private bool PressedMouseLeftButton { get; set; }
        private bool PressedMouseRightButton { get; set; }
        public bool ViewNormals { get; set; }
        private Point LastPosition { get; set; }
        public double RotateXAngle { get; set; }
        public double RotateYAngle { get; set; }
        public double RotateZAngle { get; set; }
        public double ScaleXValue { get; set; }
        public double ScaleYValue { get; set; }
        public double ScaleZValue { get; set; }
        public double TranslationXValue { get; set; }
        public double TranslationYValue { get; set; }
        public double TranslationZValue { get; set; }
        public double UserScale { get; set; }
        private double BasicWindowScale { get; set; }
        public double WindowScale { get; set; }
        public EProjectionMode FProjectionMode { get; set; }
        public EDrawMode FDrawMode { get; set; }

        private List<Object4D> _objects;
        private Camera _camera;

        public MainWindow()
        {
            InitializeFields();
            InitializeObjects();
            InitializeComponent();
            Draw();
        }

        private void InitializeFields()
        {
            PressedMouseLeftButton = false;
            PressedMouseRightButton = false;
            ViewNormals = false;
            LastPosition = new Point(0, 0);
            RotateXAngle = 0;
            RotateYAngle = 0;
            RotateZAngle = 0;
            ScaleXValue = 100;
            ScaleYValue = 100;
            ScaleZValue = 100;
            TranslationXValue = 0;
            TranslationYValue = 0;
            TranslationZValue = 0;
            UserScale = 1;
            BasicWindowScale = Math.Min(450, 600);
            WindowScale = 1;
            FProjectionMode = EProjectionMode.Isometry;
            FDrawMode = EDrawMode.Polyline;
            _objects = new List<Object4D>();
            _camera = new Camera(new Point4D(0, 0, 30, 1), this);
        }

        private void InitializeObjects()
        {
            Point4D[] cube_points = new Point4D[]
            {
                new Point4D(3, 1, 1, 1),
                new Point4D(1, 3, 1, 1),
                new Point4D(1, 1, 3, 1),

                new Point4D(-3, -1, 1, 1),
                new Point4D(-1, -3, 1, 1),
                new Point4D(-1, -1, 3, 1),

                new Point4D(-3, 1, -1, 1),
                new Point4D(-1, 3, -1, 1),
                new Point4D(-1, 1, -3, 1),

                new Point4D(3, -1, -1, 1),
                new Point4D(1, -3, -1, 1),
                new Point4D(1, -1, -3, 1),
            };

            Object4D cube = new Object4D(
                cube_points,
                new Polygon4D[] {
                    new Polygon4D(cube_points, new int[]{ 2, 1, 0 }),
                    new Polygon4D(cube_points, new int[]{ 5, 4, 3 }),
                    new Polygon4D(cube_points, new int[]{ 8, 7, 6 }),
                    new Polygon4D(cube_points, new int[]{ 11, 10, 9 }),
                    new Polygon4D(cube_points, new int[]{ 0, 1, 7, 8, 11, 9 }),
                    new Polygon4D(cube_points, new int[]{ 1, 2, 5, 3, 6, 7 }),
                    new Polygon4D(cube_points, new int[]{ 3, 4, 10, 11, 8, 6 }),
                    new Polygon4D(cube_points, new int[]{ 0, 9, 10, 4, 5, 2 }),
                }
            );

            Point4D[] oxo = new Point4D[]
            {
                new Point4D(0, 0, 0, 1),
                new Point4D(10, 0, 0, 1),
            };

            Point4D[] oyo = new Point4D[]
            {
                new Point4D(0, 0, 0, 1),
                new Point4D(0, 10, 0, 1),
            };

            Point4D[] ozo = new Point4D[]
            {
                new Point4D(0, 0, 0, 1),
                new Point4D(0, 0, 10, 1),
            };

            Object4D ox = new Object4D(
                oxo,
                new Polygon4D[] {
                    new Polygon4D(oxo, new int[]{ 0, 1 }),
                }
            )
            { Color = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 0, 0)) };

            Object4D oy = new Object4D(
                oyo,
                new Polygon4D[] {
                    new Polygon4D(oyo, new int[]{ 0, 1 }),
                }
            )
            { Color = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 255, 0)) };

            Object4D oz = new Object4D(
                ozo,
                new Polygon4D[] {
                    new Polygon4D(ozo, new int[]{ 0, 1 }),
                }
            )
            { Color = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 255)) };

            _objects.Add(ox);
            _objects.Add(oy);
            _objects.Add(oz);
            _objects.Add(cube);
        }

        public void WriteInDebugTextBlock(object msg, bool flag = false)
        {
            if (msg != null)
            {
                if (flag)
                {
                    try
                    {
                        DebugTextBlock.Text += msg.ToString();
                    }
                    catch (Exception ee)
                    {
                        DebugTextBlock.Text += ee.Message;
                    }
                }
                else
                {
                    try
                    {
                        DebugTextBlock.Text = msg.ToString();
                    }
                    catch (Exception ee)
                    {
                        DebugTextBlock.Text = ee.Message;
                    }
                }
            }
        }

        private void Draw()
        {
            _camera.Project(_objects, FProjectionMode);
        }

        private void DropRotationValues()
        {
            RotateXAngle = 0;
            RotateYAngle = 0;
            RotateZAngle = 0;
            RotateX.Text = RotateXAngle.ToString();
            RotateY.Text = RotateYAngle.ToString();
            RotateZ.Text = RotateZAngle.ToString();
        }

        private void SetRotationValues(double alpha, double betha, double gamma)
        {
            RotateXAngle = alpha;
            RotateYAngle = betha;
            RotateZAngle = gamma;
            RotateX.Text = RotateXAngle.ToString();
            RotateY.Text = RotateYAngle.ToString();
            RotateZ.Text = RotateZAngle.ToString();
        }

        private void CanvasGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        { 
            double scale = e.Delta > 0 ? 1.1 : 1 / 1.1;

            UserScale *= scale;

            Draw();
        }

        private void CanvasGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PressedMouseLeftButton = true;
            LastPosition = new Point(e.GetPosition(Canvas).X, e.GetPosition(Canvas).Y);
        }

        public void CanvasGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PressedMouseLeftButton = false;
        }

        private void CanvasGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            PressedMouseRightButton = true;
            LastPosition = new Point(e.GetPosition(Canvas).X, e.GetPosition(Canvas).Y);
        }

        public void CanvasGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            PressedMouseRightButton = false;
        }

        private void CanvasGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (PressedMouseLeftButton)
            {
                Point CurrentPosition = new Point(e.GetPosition(Canvas).X, e.GetPosition(Canvas).Y);

                double divx = LastPosition.X - CurrentPosition.X;
                double divy = LastPosition.Y - CurrentPosition.Y;

                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    TranslationXValue -= divx;
                    TranslationZValue -= divy;
                    TranslationX.Text = TranslationXValue.ToString();
                    TranslationZ.Text = TranslationZValue.ToString();
                }
                else
                {
                    TranslationXValue -= divx;
                    TranslationYValue -= divy;
                    TranslationX.Text = TranslationXValue.ToString();
                    TranslationY.Text = TranslationYValue.ToString();
                }

                Draw();

                LastPosition = new Point(e.GetPosition(Canvas).X, e.GetPosition(Canvas).Y);
            } if (PressedMouseRightButton)
            {
                Point CurrentPosition = new Point(e.GetPosition(Canvas).X, e.GetPosition(Canvas).Y);

                double alpha = LastPosition.X - CurrentPosition.X;
                double betha = LastPosition.Y - CurrentPosition.Y;

                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    RotateZAngle += betha;
                    RotateYAngle += alpha;
                    RotateZ.Text = $"{RotateZAngle % 360}";
                    RotateY.Text = $"{RotateYAngle % 360}";
                }
                else
                {
                    RotateXAngle += betha;
                    RotateYAngle += alpha;
                    RotateX.Text = $"{RotateXAngle % 360}";
                    RotateY.Text = $"{RotateYAngle % 360}";
                }
                
                Draw();

                LastPosition = new Point(e.GetPosition(Canvas).X, e.GetPosition(Canvas).Y);
            }
        }

        private void CanvasGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double new_window_scale = Math.Min(ActualHeight, ActualWidth);

            WindowScale = new_window_scale / BasicWindowScale;
            Draw();
        }

        private void RotateX_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_RotateXAngle;

            try
            {
                new_RotateXAngle = Convert.ToDouble(RotateX.Text);
            }
            catch (FormatException ee)
            {
                System.Diagnostics.Debug.WriteLine(ee.Message);
                new_RotateXAngle = 0;
            }
            catch (OverflowException ee)
            {
                System.Diagnostics.Debug.WriteLine(ee.Message);
                new_RotateXAngle = 0;
            }

            RotateXAngle = new_RotateXAngle;
            Draw();
        }

        private void RotateY_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_RotateYAngle;

            try
            {
                new_RotateYAngle = Convert.ToDouble(RotateY.Text);
            }
            catch (FormatException ee)
            {
                System.Diagnostics.Debug.WriteLine(ee.Message);
                new_RotateYAngle = 0;
            }
            catch (OverflowException ee)
            {
                System.Diagnostics.Debug.WriteLine(ee.Message);
                new_RotateYAngle = 0;
            }

            RotateYAngle = new_RotateYAngle;
            Draw();
        }

        private void RotateZ_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_RotateZAngle;

            try
            {
                new_RotateZAngle = Convert.ToDouble(RotateZ.Text);
            }
            catch (FormatException ee)
            {
                System.Diagnostics.Debug.WriteLine(ee.Message);
                new_RotateZAngle = 0;
            }
            catch (OverflowException ee)
            {
                System.Diagnostics.Debug.WriteLine(ee.Message);
                new_RotateZAngle = 0;
            }

            RotateZAngle = new_RotateZAngle;
            Draw();
        }

        private void ScaleX_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_ScaleXValue;

            try
            {
                new_ScaleXValue = Convert.ToDouble(ScaleX.Text);
            }
            catch (FormatException ee)
            {
                System.Diagnostics.Debug.WriteLine(ee.Message);
                new_ScaleXValue = 0;
            }
            catch (OverflowException ee)
            {
                System.Diagnostics.Debug.WriteLine(ee.Message);
                new_ScaleXValue = 0;
            }

            ScaleXValue = new_ScaleXValue;
            ScaleX.Text = ScaleXValue.ToString();
            Draw();
        }

        private void ScaleY_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_ScaleYValue;

            try
            {
                new_ScaleYValue = Convert.ToDouble(ScaleY.Text);
            }
            catch (FormatException ee)
            {
                System.Diagnostics.Debug.WriteLine(ee.Message);
                new_ScaleYValue = 0;
            }
            catch (OverflowException ee)
            {
                System.Diagnostics.Debug.WriteLine(ee.Message);
                new_ScaleYValue = 0;
            }

            ScaleYValue = new_ScaleYValue;
            ScaleY.Text = ScaleYValue.ToString();
            Draw();
        }

        private void ScaleZ_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_ScaleZValue;

            try
            {
                new_ScaleZValue = Convert.ToDouble(ScaleZ.Text);
            }
            catch (FormatException ee)
            {
                System.Diagnostics.Debug.WriteLine(ee.Message);
                new_ScaleZValue = 0;
            }
            catch (OverflowException ee)
            {
                System.Diagnostics.Debug.WriteLine(ee.Message);
                new_ScaleZValue = 0;
            }

            ScaleZValue = new_ScaleZValue;
            ScaleZ.Text = ScaleZValue.ToString();
            Draw();
        }

        private void ViewNormalsBox_Checked(object sender, RoutedEventArgs e)
        {
            ViewNormals = !ViewNormals;
            Draw();
        }

        private void TranslationX_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_TranslationXValue;

            try
            {
                new_TranslationXValue = Convert.ToDouble(TranslationX.Text);
            }
            catch (FormatException ee)
            {
                System.Diagnostics.Debug.WriteLine(ee.Message);
                new_TranslationXValue = 0;
            }
            catch (OverflowException ee)
            {
                System.Diagnostics.Debug.WriteLine(ee.Message);
                new_TranslationXValue = 0;
            }

            TranslationXValue = new_TranslationXValue;
            TranslationX.Text = TranslationXValue.ToString();
            Draw();
        }

        private void TranslationY_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_TranslationYValue;

            try
            {
                new_TranslationYValue = Convert.ToDouble(TranslationY.Text);
            }
            catch (FormatException ee)
            {
                System.Diagnostics.Debug.WriteLine(ee.Message);
                new_TranslationYValue = 0;
            }
            catch (OverflowException ee)
            {
                System.Diagnostics.Debug.WriteLine(ee.Message);
                new_TranslationYValue = 0;
            }

            TranslationYValue = new_TranslationYValue;
            TranslationY.Text = TranslationYValue.ToString();
            Draw();
        }

        private void TranslationZ_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_TranslationZValue;

            try
            {
                new_TranslationZValue = Convert.ToDouble(TranslationZ.Text);
            }
            catch (FormatException ee)
            {
                System.Diagnostics.Debug.WriteLine(ee.Message);
                new_TranslationZValue = 0;
            }
            catch (OverflowException ee)
            {
                System.Diagnostics.Debug.WriteLine(ee.Message);
                new_TranslationZValue = 0;
            }

            TranslationZValue = new_TranslationZValue;
            TranslationZ.Text = TranslationZValue.ToString();
            Draw();
        }

        private void ProjectionMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = ((TextBlock)((ComboBox)sender).SelectedItem).Text;

            if (text == "Вид сбоку")
            {
                FProjectionMode = EProjectionMode.SideView;
                DropRotationValues();
            }
            if (text == "Вид спереди")
            {
                FProjectionMode = EProjectionMode.FrontView;
                DropRotationValues();
            }
            if (text == "Вид сверху")
            {
                FProjectionMode = EProjectionMode.AboveView;
                DropRotationValues();
            }
            if (text == "Изометрическая")
            {
                FProjectionMode = EProjectionMode.Isometry;
                SetRotationValues(-45, 35, 30);
            }
            if (text == "Свободная")
            {
                FProjectionMode = EProjectionMode.Free;
                SetRotationValues(-30, 75, 30);
            }

            Draw();
        }

        private void DrawMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = ((TextBlock)((ComboBox)sender).SelectedItem).Text;

            if (text == "Каркас")
            {
                FDrawMode = EDrawMode.Polyline;
            }
            if (text == "Каркас с удалением невидимых линий")
            {
                FDrawMode = EDrawMode.PolylineVisible;
            }
            if (text == "Полигоны с сплошной заливкой")
            {
                FDrawMode = EDrawMode.Polygon;
            }
            if (text == "Полигоны с случайной заливкой")
            {
                FDrawMode = EDrawMode.PolygonRandom;
            }

            Draw();
        }
    }
}
