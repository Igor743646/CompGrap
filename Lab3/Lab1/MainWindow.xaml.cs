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

        public double ParaboloidHeight { get; set; }
        public double ParaboloidK { get; set; }
        public int ParaboloidDPhi { get; set; }
        public int ParaboloidDH { get; set; }

        public float ParaboloidAlpha { get; set; }
        public float ParaboloidR { get; set; }
        public float ParaboloidG { get; set; }
        public float ParaboloidB { get; set; }

        public double LightDistanceCoefMK { get; set; }
        public double LightDistanceCoefMD { get; set; }
        public PointLight LightSource { get; set; }
        public Material ParaboloidMaterial { get; set; }

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

            ParaboloidHeight = 2;
            ParaboloidK = 1;
            ParaboloidDPhi = 8;
            ParaboloidDH = 8;

            ParaboloidAlpha = 1;
            ParaboloidR = 1;
            ParaboloidG = 1;
            ParaboloidB = 1;

            LightDistanceCoefMD = 0.34d;
            LightDistanceCoefMK = 0.35d;

            LightSource = new PointLight {
                Coordinates = new Point4D(0.0d, 0.0d, 1.0d),
                Ia = new Vector4D(1d, 1d, 1d),
                Il = new Vector4D(1d, 1d, 1d)
            };
            ParaboloidMaterial = new Material
            {
                Color = new SolidColorBrush(Color.FromScRgb(0, ParaboloidR, ParaboloidG, ParaboloidB)),
                Ka = new Vector4D(0.0d, 0.1d, 0.0d),
                Kd = new Vector4D(0.0d, 0.0d, 1.0d),
                Ks = new Vector4D(1.0d, 0.0d, 0.0d),
                P = 1.0d,
            }; ;

            _objects = new List<Object4D>();
            _camera = new Camera(new Point4D(0, 0, 30, 1), this);
        }

        private Object4D GetParaboloid()
        {
            Point4D[] paraboloid_points = new Point4D[(ParaboloidDH - 1) * ParaboloidDPhi + 1];
            Polygon4D[] paraboloid_polygons = new Polygon4D[(ParaboloidDH - 1) * ParaboloidDPhi + 1];

            // calculate points
            int iter = 0;
            double z = 0;
            for (int h = 0; h < ParaboloidDH; z += ParaboloidHeight / ParaboloidDH, h++)
            {
                if (h == 0)
                {
                    paraboloid_points[iter] = new Point4D(0.0d, 0.0d, 0.0d, 1, paraboloid_polygons);
                    iter++;
                    continue;
                }

                double p = 0.0;
                for (int phi = 0; phi < ParaboloidDPhi; phi++, p += 2 * Math.PI / ParaboloidDPhi)
                {
                    paraboloid_points[iter] = new Point4D(Math.Sqrt(z / ParaboloidK) * Math.Cos(p), z, Math.Sqrt(z / ParaboloidK) * Math.Sin(p), 1, paraboloid_polygons);
                    iter++;
                }
            }

            // calculate polygones
            iter = 0;
            for (int n = 1; n < ParaboloidDPhi + 1; n++, iter++)
            {
                paraboloid_polygons[iter] = new Polygon4D(
                    paraboloid_points,
                    new int[] { 0, (n % ParaboloidDPhi) + 1, n }
                );
            }

            for (int n = 1; n < paraboloid_points.Length - ParaboloidDPhi; n++, iter++)
            {
                if (n % ParaboloidDPhi == 0)
                {
                    paraboloid_polygons[iter] = new Polygon4D(
                        paraboloid_points, 
                        new int[] { n, n - ParaboloidDPhi + 1, n + 1, n + ParaboloidDPhi }
                    );
                    continue;
                }

                paraboloid_polygons[iter] = new Polygon4D(
                    paraboloid_points, 
                    new int[] { n, n + 1, n + 1 + ParaboloidDPhi, n + ParaboloidDPhi }
                );

            }

            int[] up = new int[ParaboloidDPhi];

            for (int i = 0; i < ParaboloidDPhi; i++)
            {
                up[i] = i + paraboloid_points.Length - ParaboloidDPhi;
            }

            paraboloid_polygons[iter] = new Polygon4D(paraboloid_points, up);

            //calculate poligones indexes for points
            for (int i = 0; i < paraboloid_points.Length; i++)
            {
                if (i == 0)
                {
                    int[] idx = new int[ParaboloidDPhi];
                    for (int j = 0; j < ParaboloidDPhi; j++)
                    {
                        idx[j] = j;
                    }
                    paraboloid_points[i].polygonesIndexes = idx;
                }
                else if (i == ((ParaboloidDH - 2) * ParaboloidDPhi) + 1)
                {
                    paraboloid_points[i].polygonesIndexes = new int[] { i - 1, i - 2 + ParaboloidDPhi, ParaboloidDPhi * (ParaboloidDH - 1)};
                }
                else if (i > ((ParaboloidDH - 2) * ParaboloidDPhi) + 1)
                {
                    paraboloid_points[i].polygonesIndexes = new int[] { i - 1, i - 2, ParaboloidDPhi * (ParaboloidDH - 1) };
                }
                else
                {
                    paraboloid_points[i].polygonesIndexes = (i - 1) % ParaboloidDPhi == 0
                        ? (new int[] { i - 1, i - 2 + ParaboloidDPhi, i - 1 + ParaboloidDPhi, i - 2 + (2 * ParaboloidDPhi) })
                        : (new int[] { i - 1, i - 2, i - 1 + ParaboloidDPhi, i - 2 + ParaboloidDPhi });
                }
            }

            Object4D paraboloid = new Object4D(paraboloid_points, paraboloid_polygons, ParaboloidMaterial);

            return paraboloid;
        }

        private void InitializeObjects()
        {
            _objects.Add(GetParaboloid());
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
            DebugTextBlock.Text += '\n';
        }

        private void Draw()
        {
            _camera.Project(_objects, FProjectionMode, LightSource);

            /*Point[] ps = new Point[3];
            ps[0] = new Point(0, 0);
            ps[1] = new Point(50, 90);
            ps[2] = new Point(200, 100);
            PointCollection myPointCollection = new PointCollection(ps);

            double max_x = Math.Max(ps[0].X, Math.Max(ps[1].X, ps[2].X))
                , max_y = Math.Max(ps[0].Y, Math.Max(ps[1].Y, ps[2].Y))
                , min_x = Math.Min(ps[0].X, Math.Min(ps[1].X, ps[2].X))
                , min_y = Math.Min(ps[0].Y, Math.Min(ps[1].Y, ps[2].Y));

            Polygon pol1 = new Polygon
            {
                Points = myPointCollection,
                Fill = new RadialGradientBrush(Colors.Red, Color.FromArgb(0, Colors.Red.R, Colors.Red.G, Colors.Red.B)) {
                    GradientOrigin = new Point((ps[0].X - min_x)/(max_x - min_x), (ps[0].Y - min_y) / (max_y - min_y))
                }
            };

            Polygon pol2 = new Polygon
            {
                Points = myPointCollection,
                Fill = new RadialGradientBrush(Colors.Blue, Color.FromArgb(0, Colors.Blue.R, Colors.Blue.G, Colors.Blue.B))
                {
                    GradientOrigin = new Point((ps[1].X - min_x) / (max_x - min_x), (ps[1].Y - min_y) / (max_y - min_y))
                }
            };

            Polygon pol3 = new Polygon
            {
                Points = myPointCollection,
                Fill = new RadialGradientBrush(Colors.Lime, Color.FromArgb(0, Colors.Lime.R, Colors.Lime.G, Colors.Lime.B))
                {
                    GradientOrigin = new Point((ps[2].X - min_x) / (max_x - min_x), (ps[2].Y - min_y) / (max_y - min_y))
                }
            };


            Canvas.Children.Add(pol1);
            Canvas.Children.Add(pol2);
            Canvas.Children.Add(pol3);*/
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

                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    TranslationXValue -= divx;
                    TranslationZValue -= divy;
                    TranslationX.Text = TranslationXValue.ToString();
                    TranslationZ.Text = TranslationZValue.ToString();
                }
                else if (Keyboard.IsKeyDown(Key.LeftAlt))
                {
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        LightSource.Coordinates.X -= divx * WindowScale / 100;
                        LightSource.Coordinates.Z += divy * WindowScale / 100;
                        P_X.Text = LightSource.Coordinates.X.ToString();
                        P_Z.Text = LightSource.Coordinates.Z.ToString();
                    }
                    else
                    {
                        LightSource.Coordinates.X -= divx * WindowScale / 100;
                        LightSource.Coordinates.Y += divy * WindowScale / 100;
                        P_X.Text = LightSource.Coordinates.X.ToString();
                        P_Y.Text = LightSource.Coordinates.Y.ToString();
                    }
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

        /// <summary>
        /// Возвращает значение типа T из переданного TextBox. В случае ошибки возвращает значение по умолчанию.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tb"></param>
        /// <param name="defaultValue"></param>
        /// <returns>Объект типа T, сконвертированный из tb</returns>
        private T ReadTextBlock<T>(TextBox tb, T defaultValue)
        {
            T result;

            try
            {
                result = (T)Convert.ChangeType(tb.Text, typeof(T));
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                result = defaultValue;
            }

            return result;
        }

        private void RotateX_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_RotateXAngle = ReadTextBlock<double>(RotateX, 0);

            RotateXAngle = new_RotateXAngle;
            Draw();
        }

        private void RotateY_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_RotateYAngle = ReadTextBlock<double>(RotateY, 0);

            RotateYAngle = new_RotateYAngle;
            Draw();
        }

        private void RotateZ_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_RotateZAngle = ReadTextBlock<double>(RotateZ, 0);

            RotateZAngle = new_RotateZAngle;
            Draw();
        }

        private void ScaleX_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_ScaleXValue = ReadTextBlock<double>(ScaleX, 0);

            ScaleXValue = new_ScaleXValue;
            Draw();
        }

        private void ScaleY_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_ScaleYValue = ReadTextBlock<double>(ScaleY, 0);

            ScaleYValue = new_ScaleYValue;
            Draw();
        }

        private void ScaleZ_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_ScaleZValue = ReadTextBlock<double>(ScaleZ, 0);

            ScaleZValue = new_ScaleZValue;
            Draw();
        }

        private void ViewNormalsBox_Checked(object sender, RoutedEventArgs e)
        {
            ViewNormals = !ViewNormals;
            Draw();
        }

        private void TranslationX_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_TranslationXValue = ReadTextBlock<double>(TranslationX, 0);

            TranslationXValue = new_TranslationXValue;
            Draw();
        }

        private void TranslationY_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_TranslationYValue = ReadTextBlock<double>(TranslationY, 0);

            TranslationYValue = new_TranslationYValue;
            Draw();
        }

        private void TranslationZ_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_TranslationZValue = ReadTextBlock<double>(TranslationZ, 0);

            TranslationZValue = new_TranslationZValue;
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
            if (text == "Плоская модель затенения")
            {
                FDrawMode = EDrawMode.FlatShading;
            }
            if (text == "Модель Гуро")
            {
                FDrawMode = EDrawMode.GouraudShading;
            }

            Draw();
        }

        private void H_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_ParaboloidHeight = ReadTextBlock<double>(H, 1);

            ParaboloidHeight = new_ParaboloidHeight;

            _objects.Clear();
            _objects.Add(GetParaboloid());

            Draw();
        }

        private void K_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_ParaboloidK = ReadTextBlock<double>(K, 1);

            ParaboloidK = new_ParaboloidK;

            _objects.Clear();
            _objects.Add(GetParaboloid());

            Draw();
        }

        private void Dphi_TextChanged(object sender, TextChangedEventArgs e)
        {
            int new_ParaboloidDPhi = ReadTextBlock<int>(dphi, 8);

            ParaboloidDPhi = new_ParaboloidDPhi;

            _objects.Clear();
            _objects.Add(GetParaboloid());

            Draw();
        }

        private void Dh_TextChanged(object sender, TextChangedEventArgs e)
        {
            int new_ParaboloidDH = ReadTextBlock<int>(dh, 5);

            if (new_ParaboloidDH >= 2)
            {
                ParaboloidDH = new_ParaboloidDH;

                _objects.Clear();
                _objects.Add(GetParaboloid());

                Draw();
            }

        }

        private void R_TextChanged(object sender, TextChangedEventArgs e)
        {
            float new_ParaboloidR = ReadTextBlock<float>(R, 1);

            ParaboloidR = new_ParaboloidR;
            _objects[0].Material.Color = new SolidColorBrush(Color.FromScRgb(ParaboloidAlpha, ParaboloidR, ParaboloidG, ParaboloidB));

            Draw();
        }

        private void G_TextChanged(object sender, TextChangedEventArgs e)
        {
            float new_ParaboloidG = ReadTextBlock<float>(G, 1);

            ParaboloidG = new_ParaboloidG;
            _objects[0].Material.Color = new SolidColorBrush(Color.FromScRgb(ParaboloidAlpha, ParaboloidR, ParaboloidG, ParaboloidB));

            Draw();
        }

        private void B_TextChanged(object sender, TextChangedEventArgs e)
        {
            float new_ParaboloidB = ReadTextBlock<float>(B, 1);

            ParaboloidB = new_ParaboloidB;
            _objects[0].Material.Color = new SolidColorBrush(Color.FromScRgb(ParaboloidAlpha, ParaboloidR, ParaboloidG, ParaboloidB));

            Draw();
        }

        private void P_X_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_p_x = ReadTextBlock<double>(P_X, 0);

            LightSource.Coordinates.X = new_p_x;

            Draw();
        }

        private void P_Y_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_p_y = ReadTextBlock<double>(P_Y, 0);

            LightSource.Coordinates.Y = new_p_y;

            Draw();
        }

        private void P_Z_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_p_z = ReadTextBlock<double>(P_Z, 0);

            LightSource.Coordinates.Z = new_p_z;

            Draw();
        }

        private void Ia_X_TextChanged(object sender, TextChangedEventArgs e)
        {
            LightSource.Ia.Dest.X = ReadTextBlock<double>(Ia_X, 1.0d);
            Draw();
        }

        private void Ia_Y_TextChanged(object sender, TextChangedEventArgs e)
        {
            LightSource.Ia.Dest.Y = ReadTextBlock(Ia_Y, 1.0d);
            Draw();
        }

        private void Ia_Z_TextChanged(object sender, TextChangedEventArgs e)
        {
            LightSource.Ia.Dest.Z = ReadTextBlock(Ia_Z, 1.0d);
            Draw();
        }

        private void Il_X_TextChanged(object sender, TextChangedEventArgs e)
        {
            LightSource.Il.Dest.X = ReadTextBlock(Il_X, 1.0d);
            Draw();
        }

        private void Il_Y_TextChanged(object sender, TextChangedEventArgs e)
        {
            LightSource.Il.Dest.Y = ReadTextBlock(Il_Y, 1.0d);
            Draw();
        }

        private void Il_Z_TextChanged(object sender, TextChangedEventArgs e)
        {
            LightSource.Il.Dest.Z = ReadTextBlock(Il_Z, 1.0d);
            Draw();
        }

        private void Ka_X_TextChanged(object sender, TextChangedEventArgs e)
        {
            _objects[0].Material.Ka.Dest.X = ReadTextBlock(Ka_X, 0.0d);
            Draw();
        }

        private void Ka_Y_TextChanged(object sender, TextChangedEventArgs e)
        {
            _objects[0].Material.Ka.Dest.Y = ReadTextBlock(Ka_Y, 0.0d);
            Draw();
        }

        private void Ka_Z_TextChanged(object sender, TextChangedEventArgs e)
        {
            _objects[0].Material.Ka.Dest.Z = ReadTextBlock(Ka_Z, 0.0d);
            Draw();
        }

        private void Kd_X_TextChanged(object sender, TextChangedEventArgs e)
        {
            _objects[0].Material.Kd.Dest.X = ReadTextBlock(Kd_X, 0.0d);
            Draw();
        }

        private void Kd_Y_TextChanged(object sender, TextChangedEventArgs e)
        {
            _objects[0].Material.Kd.Dest.Y = ReadTextBlock(Kd_Y, 0.0d);
            Draw();
        }

        private void Kd_Z_TextChanged(object sender, TextChangedEventArgs e)
        {
            _objects[0].Material.Kd.Dest.Z = ReadTextBlock(Kd_Z, 0.0d);
            Draw();
        }

        private void Ks_X_TextChanged(object sender, TextChangedEventArgs e)
        {
            _objects[0].Material.Ks.Dest.X = ReadTextBlock(Ks_X, 0.0d);
            Draw();
        }

        private void Ks_Y_TextChanged(object sender, TextChangedEventArgs e)
        {
            _objects[0].Material.Ks.Dest.Y = ReadTextBlock(Ks_Y, 0.0d);
            Draw();
        }

        private void Ks_Z_TextChanged(object sender, TextChangedEventArgs e)
        {
            _objects[0].Material.Ks.Dest.Z = ReadTextBlock(Ks_Z, 0.0d);
            Draw();
        }

        private void P_TextChanged(object sender, TextChangedEventArgs e)
        {
            _objects[0].Material.P = ReadTextBlock(P, 0.0d);
            Draw();
        }

        private void MK_TextChanged(object sender, TextChangedEventArgs e)
        {
            LightDistanceCoefMK = ReadTextBlock<double>(MK, 1);
            Draw();
        }

        private void MD_TextChanged(object sender, TextChangedEventArgs e)
        {
            LightDistanceCoefMD = ReadTextBlock<double>(MD, 1);
            Draw();
        }
    }
}
