using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Geometry;
using SharpGL;
using SharpGL.Enumerations;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Lighting;

namespace WPFOpenGl
{
    public enum EProjectionMode
    {
        SideView = 0,
        FrontView = 1,
        AboveView = 2,
        Isometry = 3,
        Free = 4,
        Perspective = 5,
    }

    public enum EDrawMode
    {
        Polyline = 0,
        PolylineVisible = 1,
        Polygon = 2,
        PolygonRandom = 3,
    }

    public enum ESettingChanges : int
    {
        MatrixModelView = 1,
        MatrixProjection = 2,
        PolygonMode = 4,
    }

    public partial class MainWindow : Window
    {
        #region Настройки приложения

        private readonly double BasicHeight = 750.00d;
        private readonly double BasicWidth = 700.00d;
        private bool ChangeSettings { get; set; }
        private bool ChangeObject { get; set; }
        private bool PressedMouseLeftButton { get; set; }
        private bool PressedMouseRightButton { get; set; }
        private Point LastPosition { get; set; }

        #endregion

        #region Переменные вращения, перемещения и изменения размера
        public double RotateXAngle { get; set; }
        public double RotateYAngle { get; set; }
        public double RotateZAngle { get; set; }
        public double ScaleXValue { get; set; }
        public double ScaleYValue { get; set; }
        public double ScaleZValue { get; set; }
        public double TranslationXValue { get; set; }
        public double TranslationYValue { get; set; }
        public double TranslationZValue { get; set; }
        #endregion

        public bool BoolDirects { get; set; }
        public double UserScale { get; set; }
        public EDrawMode FDrawMode { get; set; }
        public ESettingChanges FSettingChanges { get; set; }
        public Matrix4D AllTransformations { get; set; }

        #region Параболоид и его параметры

        public double Approximation = 0.5;

        public readonly Matrix4D Hermit_Matrix = new Matrix4D(new double[4,4] {
            { 1, 0, -3,  2 },
            { 0, 0,  3, -2 },
            { 0, 1, -2,  1 },
            { 0, 0, -1,  1 }
        });

        public Matrix4D_p surface_data;

        public Object4D Surface = null;

        #endregion

        public MainWindow()
        {
            InitializeFields();
            InitializeComponent();
        }

        private void InitializeFields()
        {
            RotateXAngle = -30;
            RotateYAngle = -45;
            RotateZAngle = -20;
            ScaleXValue = 1;
            ScaleYValue = 1;
            ScaleZValue = 1;
            TranslationXValue = 0;
            TranslationYValue = 0;
            TranslationZValue = 0;

            UserScale = 1;

            FDrawMode = EDrawMode.Polyline;

            BoolDirects = false;
            ChangeSettings = false;
            ChangeObject = false;
            PressedMouseLeftButton = false;
            PressedMouseRightButton = false;

            AllTransformations = Matrix4D.ScaleMatrix(1, 1, 1);

            Approximation = 0.01;
            surface_data = new Matrix4D_p(new Point4D[4,4]
            {
                {
                    new Point4D(19.1887108187929, -7.441939017372807, -24.530802477413577),
                    new Point4D(10.382882040847122, -2.0003981274221587, 14.089931441921912),
                    new Point4D(-2.290474137805968, 1.4038572075397937, 4.665706306726495),
                    new Point4D(-3.9188107239901147, 3.7068486783840697, 2.457938420991587),
                     },
                    {
                    new Point4D(-24.501367373174602, 13.051072424383108, -6.21392450357628),
                    new Point4D(-16.151915008938, -23.79053480421897, -6.315376086935565),
                    new Point4D(1.148336739502148, -2.554510369368294, 4.2517053879173545),
                    new Point4D(-2.3956064843645484, 0.7202084963296551, -0.9884553052273546),
                     },
                    {
                    new Point4D(0.26840926703574475, 1.9289885647657137, 4.042542674996092),
                    new Point4D(2.4208334566047753, -4.408508054924708, -3.9874332844694296),
                    new Point4D(1.3839226892097116, 0.8395876366166295, -0.5295247806818546),
                    new Point4D(0.8942706771433029, 0.7584381441568855, 0.219307719026002),
                     },
                    {
                    new Point4D(-2.130322840707486, -3.4907451600417994, 1.8317649881495521),
                    new Point4D(-4.552641846091307, 1.094072770649717, 0.7218938498113081),
                    new Point4D(3.9222603937971883, -1.0194697706581959, 0.8354798178916152),
                    new Point4D(-3.137472884597633, -3.762013117356485, 2.8307195265504266),
                     }
            });
            Surface = GetSurface();
        }

        private Object4D GetSurface()
        {
            int number_of_points_by_one_coord = (int)(1 / Approximation) + 1;
            int number_of_points = number_of_points_by_one_coord * number_of_points_by_one_coord;

            Point4D[] points = new Point4D[number_of_points];
            uint[] points_indexes = new uint[(number_of_points_by_one_coord - 1) * (number_of_points_by_one_coord - 1) * 4];

            Matrix4D_p MGM = Matrix4D.Transpose(Hermit_Matrix) * surface_data * Hermit_Matrix;

            // Создание точек поверхности
            double u = 0.0d;
            for (int i = 0; i < number_of_points_by_one_coord; i++, u += Approximation)
            {
                double v = 0.0d;
                for (int j = 0; j < number_of_points_by_one_coord; j++, v += Approximation)
                {
                    Vector4D pu = new Vector4D(1, u, u * u, u * u * u);
                    Vector4D pv = new Vector4D(1, v, v * v, v * v * v);

                    points[i * number_of_points_by_one_coord + j] = pu * MGM * pv;
                }
            }

            // создание индексов полигонов
            uint k = 0;
            for (uint i = 0; i < number_of_points_by_one_coord - 1; i++)
            {
                for (uint j = 0; j < number_of_points_by_one_coord - 1; j++)
                {
                    uint l = i * (uint)number_of_points_by_one_coord + j;
                    points_indexes[k] = l;
                    points_indexes[k + 1] = l + 1;
                    points_indexes[k + 2] = l + (uint)number_of_points_by_one_coord + 1;
                    points_indexes[k + 3] = l + (uint)number_of_points_by_one_coord;
                    k += 4;
                }
            }

            Object4D surface = new Object4D(points, points_indexes);

            return surface;
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

        private Matrix4D NewAllTransformationsMatrix()
        {
            return (
                Matrix4D.ScaleMatrix(ScaleXValue * UserScale, ScaleYValue * UserScale, ScaleZValue * UserScale) *
                Matrix4D.RotateMatrix(RotateXAngle, RotateYAngle, RotateZAngle)) *
                Matrix4D.TranslationMatrix(TranslationXValue, TranslationYValue, TranslationZValue);
        }

        private void OpenGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            OpenGL gl = args.OpenGL;
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT | OpenGL.GL_STENCIL_BUFFER_BIT);
            
            if (ChangeSettings)
            {
                if (((int)FSettingChanges & (int)ESettingChanges.MatrixModelView) != 0)
                {
                    gl.MatrixMode(MatrixMode.Modelview);
                    gl.LoadMatrix(NewAllTransformationsMatrix().ToArray());
                }
                if (((int)FSettingChanges & (int)ESettingChanges.MatrixProjection) != 0)
                {
                    double koefHeight = ActualHeight / BasicHeight;
                    double koefWidth = (ActualWidth - 200) / BasicWidth;

                    gl.MatrixMode(MatrixMode.Projection);
                    gl.LoadMatrix(new double[] {
                        1/koefWidth, 0, 0, 0,
                        0, 1/koefHeight, 0, 0,
                        0, 0, 0, 0,
                        0, 0, 0, 1,
                    });
                }
                if (((int)FSettingChanges & (int)ESettingChanges.PolygonMode) != 0)
                {
                    if (FDrawMode == EDrawMode.Polyline)
                    {
                        if (gl.IsEnabled(OpenGL.GL_CULL_FACE))
                        {
                            gl.Disable(OpenGL.GL_CULL_FACE);
                        }

                        gl.PolygonMode(FaceMode.FrontAndBack, PolygonMode.Lines);
                    }
                    else if (FDrawMode == EDrawMode.PolylineVisible)
                    {
                        gl.Enable(OpenGL.GL_CULL_FACE);
                        gl.CullFace(OpenGL.GL_FRONT);
                        gl.PolygonMode(FaceMode.FrontAndBack, PolygonMode.Lines);
                    }
                    else if (FDrawMode == EDrawMode.Polygon)
                    {
                        if (gl.IsEnabled(OpenGL.GL_CULL_FACE))
                        {
                            gl.Disable(OpenGL.GL_CULL_FACE);
                        }

                        gl.PolygonMode(FaceMode.FrontAndBack, PolygonMode.Filled);
                    }
                }

                FSettingChanges = 0;
                ChangeSettings = false;
            }

            if (ChangeObject)
            {
                Surface = GetSurface();
                ChangeObject = false;
            }

            unsafe
            {
                gl.EnableClientState(OpenGL.GL_VERTEX_ARRAY);
                gl.EnableClientState(OpenGL.GL_COLOR_ARRAY);

                fixed (Point4D* p = Surface.Points)
                {
                    gl.VertexPointer(3, OpenGL.GL_DOUBLE, sizeof(Point4D), (IntPtr)(&p->X));
                    gl.ColorPointer(3, OpenGL.GL_DOUBLE, sizeof(Point4D), (IntPtr)(&p->R));
                }

                fixed (uint* p1 = Surface.PolygonsIndexes)
                {
                    gl.DrawElements(OpenGL.GL_QUADS, Surface.PolygonsIndexes.Length, OpenGL.GL_UNSIGNED_INT, (IntPtr)p1);
                }

                gl.DisableClientState(OpenGL.GL_VERTEX_ARRAY);
                gl.DisableClientState(OpenGL.GL_COLOR_ARRAY);
            }

            if (BoolDirects)
            {
                #region Отрисовка направляющих

                double koefHeight = ActualHeight / BasicHeight;
                double koefWidth = (ActualWidth - 200) / BasicWidth;

                Matrix4D project = new Matrix4D(new double[,] {
                        { 1/koefWidth, 0, 0, 0, },
                        { 0, 1/koefHeight, 0, 0, },
                        { 0, 0, 0, 0, },
                        { 0, 0, 0, 1, },
                 });

                for (int i = 0; i < 4; i++)
                {
                    int x = i / 2, y = i % 2;
                    Point4D p = surface_data[x, y] * NewAllTransformationsMatrix() * project;
                    int px = (int)(p.X * OpenGLGrid.ActualWidth / 2 + OpenGLGrid.ActualWidth / 2), py = (int)(p.Y * OpenGLGrid.ActualHeight / 2 + OpenGLGrid.ActualHeight / 2);

                    gl.DrawText(px + 9, py + 9, 1f, 1f, 1f, "Arial", 12, $"P{x}{y}");

                    gl.Begin(OpenGL.GL_LINES);
                    gl.Color(1d, 0d, 0d);
                    gl.Vertex(surface_data[x, y].ToDoubleArray());
                    gl.Vertex((surface_data[x, y] + surface_data[x + 2, y]).ToDoubleArray());

                    gl.Color(0d, 1d, 0d);
                    gl.Vertex(surface_data[x, y].ToDoubleArray());
                    gl.Vertex((surface_data[x, y] + surface_data[x, y + 2]).ToDoubleArray());

                    gl.Color(1d, 0d, 1d);
                    gl.Vertex(surface_data[x, y].ToDoubleArray());
                    gl.Vertex((surface_data[x, y] + surface_data[x + 2, y + 2]).ToDoubleArray());
                    gl.End();
                }

                #endregion

                #region Отрисоыка осей

                gl.MatrixMode(MatrixMode.Modelview);

                Matrix4D mm = Matrix4D.ScaleMatrix(0.25, 0.25, 0.25) * 
                    Matrix4D.RotateMatrix(RotateXAngle, RotateYAngle, RotateZAngle) * 
                    Matrix4D.TranslationMatrix(0.8 * OpenGLGrid.ActualWidth / BasicWidth, -0.8 * OpenGLGrid.ActualHeight / BasicHeight, 0);
                gl.LoadMatrix(mm.ToArray());

                gl.Begin(OpenGL.GL_LINES);
                { 
                    gl.Color(0f, 0f, 1f);
                    gl.Vertex(0, 0, 0);
                    gl.Vertex(0, 0, 1);

                    gl.Color(0f, 1f, 0f);
                    gl.Vertex(0, 0, 0);
                    gl.Vertex(0, 1, 0);

                    gl.Color(1f, 0f, 0f);
                    gl.Vertex(0, 0, 0);
                    gl.Vertex(1, 0, 0);
                }
                gl.End();

                gl.MatrixMode(MatrixMode.Modelview);
                gl.LoadMatrix(NewAllTransformationsMatrix().ToArray());
                #endregion
            }
        }

        private void OpenGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            var gl = args.OpenGL;
#if DEBUG
            Trace.WriteLine($"Renderer: {gl.GetString(OpenGL.GL_RENDERER)}");
            Trace.WriteLine($"using OpenGL version: {gl.GetString(OpenGL.GL_VERSION)}");
            Trace.WriteLine($"supported GLSL version: {gl.GetString(OpenGL.GL_SHADING_LANGUAGE_VERSION)}");
#endif
            gl.ClearColor(0.16015625f, 0.01421875f, 0.1796875f, 1f);
            gl.PolygonMode(FaceMode.FrontAndBack, PolygonMode.Lines);
            gl.MatrixMode(MatrixMode.Modelview);
            gl.LoadMatrix((Matrix4D.ScaleMatrix(0.5, 0.5, 0.5) * Matrix4D.RotateMatrix(-45, 35, 30)).ToArray());
            gl.MatrixMode(MatrixMode.Projection);
            gl.LoadMatrix(Matrix4D.ScaleMatrix(0, 0, 0).ToArray());
        }

        private void OpenGLControl_Resized(object sender, OpenGLEventArgs args)
        {
            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.MatrixProjection;
        }

        private void MarkChangeSettings()
        {
            ChangeSettings = true;
        }
        private void MarkChangeObject()
        {
            ChangeObject = true;
        }
        private void CanvasGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PressedMouseLeftButton = true;
            LastPosition = new Point(e.GetPosition(OpenGLGrid).X, e.GetPosition(OpenGLGrid).Y);
        }
        private void CanvasGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PressedMouseLeftButton = false;
        }
        private void CanvasGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            PressedMouseRightButton = true;
            LastPosition = new Point(e.GetPosition(OpenGLGrid).X, e.GetPosition(OpenGLGrid).Y);
        }
        private void CanvasGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            PressedMouseRightButton = false;
        }
        private void CanvasGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (PressedMouseLeftButton)
            {
                Point CurrentPosition = new Point(e.GetPosition(OpenGLGrid).X, e.GetPosition(OpenGLGrid).Y);

                double divx = LastPosition.X - CurrentPosition.X;
                double divy = LastPosition.Y - CurrentPosition.Y;

                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    TranslationXValue -= divx / OpenGLGrid.ActualWidth * 2;
                    TranslationZValue += divy / OpenGLGrid.ActualHeight * 2;
                    TranslationX.Text = TranslationXValue.ToString();
                    TranslationZ.Text = TranslationZValue.ToString();
                }
                else
                {
                    TranslationXValue -= divx / OpenGLGrid.ActualWidth * 2;
                    TranslationYValue += divy / OpenGLGrid.ActualHeight * 2;
                    TranslationX.Text = TranslationXValue.ToString();
                    TranslationY.Text = TranslationYValue.ToString();
                }

                LastPosition = new Point(e.GetPosition(OpenGLGrid).X, e.GetPosition(OpenGLGrid).Y);
                MarkChangeSettings();
                FSettingChanges = ESettingChanges.MatrixModelView;
            }
            if (PressedMouseRightButton)
            {
                Point CurrentPosition = new Point(e.GetPosition(OpenGLGrid).X, e.GetPosition(OpenGLGrid).Y);

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

                LastPosition = new Point(e.GetPosition(OpenGLGrid).X, e.GetPosition(OpenGLGrid).Y);
                MarkChangeSettings();
                FSettingChanges = ESettingChanges.MatrixModelView;
            }
        }
        private void CanvasGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double scale = e.Delta > 0 ? 1.1d : 1d / 1.1d;
            UserScale *= scale;
            MarkChangeSettings();
            FSettingChanges = ESettingChanges.MatrixModelView;
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
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                result = defaultValue;
            }

            return result;
        }

        #region Функции считывания переменных вращения, перемещения и изменения размера
        private void RotateX_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_RotateXAngle = ReadTextBlock<double>(RotateX, 0);

            RotateXAngle = new_RotateXAngle;
            MarkChangeSettings();
            FSettingChanges = ESettingChanges.MatrixModelView;
        }
        private void RotateY_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_RotateYAngle = ReadTextBlock<double>(RotateY, 0);

            RotateYAngle = new_RotateYAngle;
            MarkChangeSettings();
            FSettingChanges = ESettingChanges.MatrixModelView;
        }
        private void RotateZ_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_RotateZAngle = ReadTextBlock<double>(RotateZ, 0);

            RotateZAngle = new_RotateZAngle;
            MarkChangeSettings();
            FSettingChanges = ESettingChanges.MatrixModelView;
        }
        private void ScaleX_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_ScaleXValue = ReadTextBlock<double>(ScaleX, 0);

            ScaleXValue = new_ScaleXValue;
            MarkChangeSettings();
            FSettingChanges = ESettingChanges.MatrixModelView;
        }
        private void ScaleY_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_ScaleYValue = ReadTextBlock<double>(ScaleY, 0);

            ScaleYValue = new_ScaleYValue;
            MarkChangeSettings();
            FSettingChanges = ESettingChanges.MatrixModelView;
        }
        private void ScaleZ_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_ScaleZValue = ReadTextBlock<double>(ScaleZ, 0);

            ScaleZValue = new_ScaleZValue;
            MarkChangeSettings();
            FSettingChanges = ESettingChanges.MatrixModelView;
        }
        private void TranslationX_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_TranslationXValue = ReadTextBlock<double>(TranslationX, 0);

            TranslationXValue = new_TranslationXValue;
            MarkChangeSettings();
            FSettingChanges = ESettingChanges.MatrixModelView;
        }
        private void TranslationY_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_TranslationYValue = ReadTextBlock<double>(TranslationY, 0);

            TranslationYValue = new_TranslationYValue;
            MarkChangeSettings();
            FSettingChanges = ESettingChanges.MatrixModelView;
        }
        private void TranslationZ_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_TranslationZValue = ReadTextBlock<double>(TranslationZ, 0);

            TranslationZValue = new_TranslationZValue;
            MarkChangeSettings();
            FSettingChanges = ESettingChanges.MatrixModelView;
        }
#endregion

        private void SetRotationValues(double alpha, double betha, double gamma)
        {
            RotateXAngle = alpha;
            RotateYAngle = betha;
            RotateZAngle = gamma;
            RotateX.Text = RotateXAngle.ToString();
            RotateY.Text = RotateYAngle.ToString();
            RotateZ.Text = RotateZAngle.ToString();
        }
        private void DropRotationValues() => SetRotationValues(0, 0, 0);
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
            if (text == "Полигоны")
            {
                FDrawMode = EDrawMode.Polygon;
            }

            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.PolygonMode;
        }

        #region Функции считывания параметров поверхности

        private void Approximation_TextChanged(object sender, TextChangedEventArgs e)
        {
            Approximation = ReadTextBlock(Approx, 0.05d);

            MarkChangeObject();
        }

        private void SurfaceData_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            double x = ReadTextBlock<double>(tb, 0);

            (char a, int b, int c, char d) = (tb.Name[0], Convert.ToInt32(tb.Name[1]) - 48, Convert.ToInt32(tb.Name[2]) - 48, tb.Name[3]);

            int i, j, index;

            switch (a)
            {
                case 'P':
                    i = 0; j = 0;
                    break;
                case 'X':
                    i = 2; j = 0;
                    break;
                case 'Y':
                    i = 0; j = 2;
                    break;
                case 'Z':
                    i = 2; j = 2;
                    break;
                default:
                    throw new Exception("Bad textbox reading");
            }

            switch(d)
            {
                case 'x':
                    index = 0;
                    break;
                case 'y':
                    index = 1;
                    break;
                case 'z':
                    index = 2;
                    break;
                default:
                    throw new Exception("Bad textbox reading");
            }

            Point4D p = surface_data[i + b, j + c];
            surface_data[i + b, j + c] = new Point4D(p, index, x);

            MarkChangeObject();

            //WriteInDebugTextBlock(surface_data);
        }

        #endregion

        private void Directs_Checked(object sender, RoutedEventArgs e) => BoolDirects = !BoolDirects;

        private void Foo(object sender, RoutedEventArgs e)
        {
            foreach (var child in Parameters.Children)
            {
                if (child is TextBox)
                {
                    ((TextBox)child).Text = "0";
                }
                
            }
        }
    }
}