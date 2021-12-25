using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Geometry;

/*
    C:\Users\Igor\source\repos\WPFOpenGl\packages\SharpGL.2.4.0.0\lib\net40\SharpGL.dll
    C:\Users\Igor\source\repos\WPFOpenGl\packages\SharpGL.2.4.0.0\lib\net40\SharpGL.SceneGraph.dll
    C:\Users\Igor\source\repos\WPFOpenGl\packages\SharpGL.WPF.2.4.0.0\lib\net40\SharpGL.WPF.dll
 */
using SharpGL;
using SharpGL.Enumerations;
using SharpGL.SceneGraph;

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
        private readonly double BasicHeight = 450.00d;
        private readonly double BasicWidth = 450.00d;
        private bool ChangeSettings { get; set; }
        private bool ChangeObject { get; set; }
        private bool PressedMouseLeftButton { get; set; }
        private bool PressedMouseRightButton { get; set; }
        private bool UseVBO { get; set; }
        private Point LastPosition { get; set; }

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

        public double UserScale { get; set; }
        public EProjectionMode FProjectionMode { get; set; }
        public EDrawMode FDrawMode { get; set; }
        public ESettingChanges FSettingChanges { get; set; }
        public Matrix4D AllTransformations { get; set; }

        private uint[] BUFFERS = new uint[2];

        #region Параболоид и его параметры
        public double ParaboloidHeight { get; set; }
        public double ParaboloidK { get; set; }
        public int ParaboloidDPhi { get; set; }
        public int ParaboloidDH { get; set; }
        public Object4D Parabaloid { get; set; }
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

            FProjectionMode = EProjectionMode.Isometry;
            FDrawMode = EDrawMode.Polyline;

            ChangeSettings = false;
            ChangeObject = false;
            PressedMouseLeftButton = false;
            PressedMouseRightButton = false;
            UseVBO = false;

            AllTransformations = Matrix4D.ScaleMatrix(1, 1, 1);

            ParaboloidHeight = 2;
            ParaboloidK = 1;
            ParaboloidDPhi = 4;
            ParaboloidDH = 3;
            Parabaloid = GetParaboloid();
        }

        private Object4D GetParaboloid()
        {
            Point4D[] paraboloid_points = new Point4D[(ParaboloidDH * ParaboloidDPhi) + 2];
            uint[] paraboloid_polygons_indexes = new uint[6 * ParaboloidDH * ParaboloidDPhi];

            // calculate points
            int iter = 0;
            double z = 0;
            for (int h = 0; h <= ParaboloidDH; z += ParaboloidHeight / ParaboloidDH, h++)
            {
                if (h == 0)
                {
                    paraboloid_points[iter] = new Point4D(0.0d, 0.0d, 0.0d, 1);
                    iter++;
                    continue;
                }

                double p = 0.0;
                for (int phi = 0; phi < ParaboloidDPhi; phi++, p += 2 * Math.PI / ParaboloidDPhi)
                {
                    paraboloid_points[iter] = new Point4D(Math.Sqrt(z / ParaboloidK) * Math.Cos(p), z, Math.Sqrt(z / ParaboloidK) * Math.Sin(p), 1);
                    iter++;
                }

                if (h == ParaboloidDH)
                {
                    paraboloid_points[iter] = new Point4D(0.0d, z, 0.0d, 1);
                    iter++;
                }
            }

            // calculate polygones indexes
            iter = 0;
            for (uint n = 1; n <= ParaboloidDPhi; n++)
            {
                paraboloid_polygons_indexes[iter] = 0;
                paraboloid_polygons_indexes[iter + 1] = (n % (uint)ParaboloidDPhi) + 1;
                paraboloid_polygons_indexes[iter + 2] = n;
                iter += 3;
            }

            for (uint n = 1; n <= (ParaboloidDH - 1) * ParaboloidDPhi; n++)
            {
                if (n % ParaboloidDPhi == 0)
                {
                    paraboloid_polygons_indexes[iter] = n;
                    paraboloid_polygons_indexes[iter + 1] = n - (uint)ParaboloidDPhi + 1;
                    paraboloid_polygons_indexes[iter + 2] = n + (uint)ParaboloidDPhi;

                    paraboloid_polygons_indexes[iter + 3] = n - (uint)ParaboloidDPhi + 1;
                    paraboloid_polygons_indexes[iter + 4] = n + 1;
                    paraboloid_polygons_indexes[iter + 5] = n + (uint)ParaboloidDPhi;
                    iter += 6;
                    continue;
                }

                paraboloid_polygons_indexes[iter] = n;
                paraboloid_polygons_indexes[iter + 1] = n + 1;
                paraboloid_polygons_indexes[iter + 2] = n + (uint)ParaboloidDPhi;

                paraboloid_polygons_indexes[iter + 3] = n + 1;
                paraboloid_polygons_indexes[iter + 4] = n + 1 + (uint)ParaboloidDPhi;
                paraboloid_polygons_indexes[iter + 5] = n + (uint)ParaboloidDPhi;
                iter += 6;
            }

            for (uint n = (uint)((ParaboloidDPhi * (ParaboloidDH - 1)) + 1); n <= ParaboloidDH * ParaboloidDPhi; n++)
            {
                paraboloid_polygons_indexes[iter] = n;
                paraboloid_polygons_indexes[iter + 1] = n + 1 != (uint)(ParaboloidDH * ParaboloidDPhi + 1) ? n + 1 : (uint)(ParaboloidDPhi * (ParaboloidDH - 1) + 1);
                paraboloid_polygons_indexes[iter + 2] = (uint)(ParaboloidDH * ParaboloidDPhi + 1);
                iter += 3;
            }

            Object4D paraboloid = new Object4D(paraboloid_points, paraboloid_polygons_indexes);

            return paraboloid;
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

        private static Matrix4D Perspective(double verticalAngle, double aspectRatio, double nearPlane, double farPlane)
        {
            var radians = (verticalAngle / 2) * Math.PI / 180;
            var sine = Math.Sin(radians);
            if (nearPlane == farPlane || aspectRatio == 0 || sine == 0)
                return Matrix4D.Zero();
            var cotan = Math.Cos(radians) / sine;
            var clip = farPlane - nearPlane;
            return new Matrix4D(new double[,] {
                { cotan / aspectRatio, 0, 0, 0 },
                { 0, cotan, 0, 0 },
                { 0, 0, -farPlane / clip, nearPlane * farPlane / clip },
                { 0, 0, 1, 2 },
            });
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
                    if (FProjectionMode == EProjectionMode.Isometry || FProjectionMode == EProjectionMode.Free)
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
                    else if (FProjectionMode == EProjectionMode.Perspective)
                    {
                        gl.MatrixMode(MatrixMode.Projection);
                        gl.LoadMatrix(Perspective(90, OpenGLGrid.ActualWidth / OpenGLGrid.ActualHeight, -1, 1).ToArray());
                    }
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
                        gl.Enable(OpenGL.GL_CULL_FACE);
                        gl.CullFace(OpenGL.GL_FRONT);
                        gl.PolygonMode(FaceMode.FrontAndBack, PolygonMode.Filled);
                    }
                }

                FSettingChanges = 0;
                ChangeSettings = false;
            }

            if (ChangeObject)
            {
                Parabaloid = GetParaboloid();

                gl.DeleteBuffers(2, BUFFERS);
                gl.GenBuffers(2, BUFFERS);

                unsafe
                {
                    fixed (Point4D* p = Parabaloid.Points)
                    {
                        gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, BUFFERS[0]);
                        gl.BufferData(OpenGL.GL_ARRAY_BUFFER, sizeof(double) * 7 * Parabaloid.Points.Length, (IntPtr)p, OpenGL.GL_STATIC_DRAW);
                        gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, 0);
                    }

                    fixed (uint* p = Parabaloid.PolygonsIndexes)
                    {
                        gl.BindBuffer(OpenGL.GL_ELEMENT_ARRAY_BUFFER, BUFFERS[1]);
                        gl.BufferData(OpenGL.GL_ELEMENT_ARRAY_BUFFER, sizeof(uint) * Parabaloid.PolygonsIndexes.Length, (IntPtr)p, OpenGL.GL_STATIC_DRAW);
                        gl.BindBuffer(OpenGL.GL_ELEMENT_ARRAY_BUFFER, 0);
                    }
                }

                ChangeObject = false;
            }

            unsafe
            {
                gl.EnableClientState(OpenGL.GL_VERTEX_ARRAY);
                gl.EnableClientState(OpenGL.GL_COLOR_ARRAY);

                if (UseVBO)
                {
                    gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, BUFFERS[0]);
                    gl.VertexPointer(4, OpenGL.GL_DOUBLE, sizeof(Point4D), (IntPtr)0);
                    gl.ColorPointer(3, OpenGL.GL_DOUBLE, sizeof(Point4D), (IntPtr)(sizeof(double) * 4));

                    gl.BindBuffer(OpenGL.GL_ELEMENT_ARRAY_BUFFER, BUFFERS[1]);
                    gl.DrawElements(OpenGL.GL_TRIANGLES, Parabaloid.PolygonsIndexes.Length, OpenGL.GL_UNSIGNED_INT, (IntPtr)0);

                    gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, 0);
                    gl.BindBuffer(OpenGL.GL_ELEMENT_ARRAY_BUFFER, 0);
                }
                else
                {
                    fixed (Point4D* p = Parabaloid.Points)
                    {
                        gl.VertexPointer(4, OpenGL.GL_DOUBLE, sizeof(Point4D), (IntPtr)(&p->X));
                        gl.ColorPointer(3, OpenGL.GL_DOUBLE, sizeof(Point4D), (IntPtr)(&p->R));
                    }

                    fixed (uint* p1 = Parabaloid.PolygonsIndexes)
                    {
                        gl.DrawElements(OpenGL.GL_TRIANGLES, Parabaloid.PolygonsIndexes.Length, OpenGL.GL_UNSIGNED_INT, (IntPtr)p1);
                    }
                }

                gl.DisableClientState(OpenGL.GL_VERTEX_ARRAY);
                gl.DisableClientState(OpenGL.GL_COLOR_ARRAY);
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
        private void ProjectionMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = ((TextBlock)((ComboBox)sender).SelectedItem).Text;

            if (text == "Изометрическая")
            {
                FProjectionMode = EProjectionMode.Isometry;
                SetRotationValues(-45, 35, 30);
            }
            if (text == "Свободная")
            {
                FProjectionMode = EProjectionMode.Free;
            }
            if (text == "Перспективная")
            {
                FProjectionMode = EProjectionMode.Perspective;
            }

            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.MatrixProjection;
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
            if (text == "Полигоны")
            {
                FDrawMode = EDrawMode.Polygon;
            }

            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.PolygonMode;
        }
        private void VBO_Checked(object sender, RoutedEventArgs e)
        {
            UseVBO = true;
        }
        private void VBO_Unchecked(object sender, RoutedEventArgs e)
        {
            UseVBO = false;
        }

        #region Функции считывания параметров параболоида
        private void H_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_ParaboloidHeight = ReadTextBlock<double>(H, 1);

            ParaboloidHeight = new_ParaboloidHeight;
            MarkChangeObject();
        }
        private void K_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_ParaboloidK = ReadTextBlock<double>(K, 1);

            ParaboloidK = new_ParaboloidK;
            MarkChangeObject();
        }
        private void Dphi_TextChanged(object sender, TextChangedEventArgs e)
        {
            int new_ParaboloidDPhi = ReadTextBlock<int>(dphi, 3);

            ParaboloidDPhi = new_ParaboloidDPhi;
            MarkChangeObject();
        }
        private void Dh_TextChanged(object sender, TextChangedEventArgs e)
        {
            int new_ParaboloidDH = ReadTextBlock<int>(dh, 3);

            ParaboloidDH = new_ParaboloidDH;
            MarkChangeObject();
        }
        #endregion
    }
}