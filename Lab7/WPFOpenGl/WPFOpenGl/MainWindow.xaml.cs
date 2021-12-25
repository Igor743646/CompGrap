using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Geometry;
using SharpGL;
using SharpGL.Enumerations;
using SharpGL.SceneGraph;
using System.Text;
using System.Windows.Media;

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
        FongShading = 4,
    }

    public enum ESettingChanges : int
    {
        MatrixModelView = 1,
        MatrixProjection = 2,
        PolygonMode = 4,
        ColorMode = 8,
    }

    public partial class MainWindow : Window
    {
        #region Настройки приложения
        private readonly double BasicHeight = 490.00d;
        private readonly double BasicWidth = 600.00d;
        private bool ChangeSettings { get; set; }
        private bool ChangeObject { get; set; }
        private bool PressedMouseLeftButton { get; set; }
        private bool PressedMouseRightButton { get; set; }
        private Point LastPosition { get; set; }
        #endregion

        #region Переменные вращения, перемещения и изменения размера
        public double RotateAngle { get; set; }
        public double ScaleXValue { get; set; }
        public double ScaleYValue { get; set; }
        public double TranslationXValue { get; set; }
        public double TranslationYValue { get; set; }
        #endregion

        #region Переменные для отрисовки объекта
        public double UserScale { get; set; }
        public EProjectionMode FProjectionMode { get; set; }
        public EDrawMode FDrawMode { get; set; }
        public ESettingChanges FSettingChanges { get; set; }
        public Matrix4D AllTransformations { get; set; }
        public Matrix4D ProjectionMatrix { get; set; }
        public Matrix4D ViewMatrix { get; set; }
        public double AproximationValue { get; set; }
        #endregion

        #region Сплайн и его параметры
        public Object3D Spline { get; set; }

        public Point3D[] P;
        public int index_of_selected_point = -1;
        #endregion

        public MainWindow()
        {
            InitializeFields();
            InitializeComponent();
        }

        private void InitializeFields()
        {
            RotateAngle = 0;
            ScaleXValue = 1;
            ScaleYValue = 1;
            TranslationXValue = 0;
            TranslationYValue = 0;

            UserScale = 1;

            FProjectionMode = EProjectionMode.Isometry;
            FDrawMode = EDrawMode.Polyline;
            AproximationValue = 0.01;

            ChangeSettings = false;
            ChangeObject = false;
            PressedMouseLeftButton = false;
            PressedMouseRightButton = false;

            AllTransformations = Matrix4D.ScaleMatrix(1, 1, 0);

            P = new Point3D[] { new Point3D(0, 0), new Point3D(1, 5), new Point3D(5, 1), new Point3D(7, 12), new Point3D(12, 7), new Point3D(18, 8) };

            Spline = GetSpline();
        }

        private Object3D GetSpline()
        {
            if (AproximationValue == 0)
            {
                AproximationValue = 0.01;
            }
            int length = (int)(1 / AproximationValue) + 1;
            Point3D[] points = new Point3D[length];

            double t = 0;
            for (int i = 0; i < length; i++, t += AproximationValue)
            {
                (double x, double y) = (0, 0);

                for (int j = 0; j < 6; j++)
                {
                    x += HelpUtils.Comb(5, j) * P[j].X * Math.Pow(1-t, 5 - j) * Math.Pow(t, j);
                    y += HelpUtils.Comb(5, j) * P[j].Y * Math.Pow(1 - t, 5 - j) * Math.Pow(t, j);
                }

                points[i] = new Point3D(x, y);
            }

            uint[] points_indexes = new uint[length];

            for (uint i = 0; i < (uint)length; i++)
            {
                points_indexes[i] = i;
            }

            return new Object3D(points, points_indexes);
        }

        private Object3D GetDopLines()
        {
            uint[] points_indexes = new uint[6];

            for (uint i = 0; i < 6; i++)
            {
                points_indexes[i] = i;
            }

            return new Object3D(P, points_indexes);
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
                Matrix4D.ScaleMatrix(ScaleXValue * UserScale, ScaleYValue * UserScale, 0) *
                Matrix4D.RotateMatrixZ(RotateAngle)) *
                Matrix4D.TranslationMatrix(TranslationXValue, TranslationYValue, 0);
        }

        private void OpenGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            OpenGL gl = args.OpenGL;
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT | OpenGL.GL_STENCIL_BUFFER_BIT);
            
            /// Изменение параметров отрисовки
            if (ChangeSettings)
            {
                if (((int)FSettingChanges & (int)ESettingChanges.MatrixModelView) != 0)
                {
                    ViewMatrix = NewAllTransformationsMatrix();
                    gl.MatrixMode(MatrixMode.Modelview);
                    gl.LoadMatrix(ViewMatrix.ToArray());
                }
                if (((int)FSettingChanges & (int)ESettingChanges.MatrixProjection) != 0)
                {
                    ProjectionMatrix = Matrix4D.ScaleMatrix(BasicWidth / (ActualWidth - 200), BasicHeight / ActualHeight, 0);

                    gl.MatrixMode(MatrixMode.Projection);
                    gl.LoadMatrix(ProjectionMatrix.ToArray());
                }

                FSettingChanges = 0;
                ChangeSettings = false;
            }

            /// Изменение параметров объекта
            if (ChangeObject)
            {
                Spline = GetSpline();
                ChangeObject = false;
            }

            /// Отрисовка
            DrawObject3D(Spline, gl);
            DrawObject3D(GetDopLines(), gl);

            if (index_of_selected_point != -1)
            {
                int ind0 = index_of_selected_point - 1;
                int ind1 = index_of_selected_point;
                int ind2 = index_of_selected_point + 1;

                if (ind0 < 0) ind0 = 0;
                if (ind2 > 5) ind2 = 5;

                gl.Begin(OpenGL.GL_LINE_STRIP);
                gl.Color(1.0, 0.0, 0.0);
                gl.Vertex(P[ind0].X, P[ind0].Y);
                gl.Color(0.0, 1.0, 0.0);
                gl.Vertex(P[ind1].X, P[ind1].Y);
                gl.Color(1.0, 0.0, 0.0);
                gl.Vertex(P[ind2].X, P[ind2].Y);
                gl.End();
            }
        }


        private void DrawObject3D(Object3D o, OpenGL gl, double r = 1, double g = 0, double b = 0)
        {
            gl.Color(r, g, b);
            unsafe
            {
                gl.EnableClientState(OpenGL.GL_VERTEX_ARRAY);

                fixed (Point3D* p = o.Points)
                {
                    gl.VertexPointer(2, OpenGL.GL_DOUBLE, sizeof(Point3D), (IntPtr)(&p->X));
                }

                fixed (uint* p1 = o.PolygonsIndexes)
                {
                    gl.DrawElements(OpenGL.GL_LINE_STRIP, o.PolygonsIndexes.Length, OpenGL.GL_UNSIGNED_INT, (IntPtr)p1);
                }

                gl.DisableClientState(OpenGL.GL_VERTEX_ARRAY);
            }
        }

        private void OpenGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            var gl = args.OpenGL;

            #region Вывод Версии и справочной информации
            #if DEBUG
            Trace.WriteLine($"Renderer: {gl.GetString(OpenGL.GL_RENDERER)}");
            Trace.WriteLine($"using OpenGL version: {gl.GetString(OpenGL.GL_VERSION)}");
            Trace.WriteLine($"supported GLSL version: {gl.GetString(OpenGL.GL_SHADING_LANGUAGE_VERSION)}");
            #endif
            #endregion

            gl.ClearColor(0.16015625f, 0.01421875f, 0.1796875f, 1f);
            gl.PolygonMode(FaceMode.FrontAndBack, PolygonMode.Lines);
            gl.MatrixMode(MatrixMode.Modelview);
            gl.LoadMatrix((Matrix4D.ScaleMatrix(0.5, 0.5, 0) * Matrix4D.RotateMatrix(0, 0, 0)).ToArray());
            gl.MatrixMode(MatrixMode.Projection);
            gl.LoadMatrix(Matrix4D.ScaleMatrix(1, 1, 0).ToArray());

            gl.PolygonMode(FaceMode.FrontAndBack, PolygonMode.Lines);
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

        #region Функции ввода
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

                if (index_of_selected_point == -1)
                {
                    TranslationXValue -= divx / OpenGLGrid.ActualWidth * 2;
                    TranslationYValue += divy / OpenGLGrid.ActualHeight * 2;
                    TranslationX.Text = TranslationXValue.ToString();
                    TranslationY.Text = TranslationYValue.ToString();

                    MarkChangeSettings();
                    FSettingChanges |= ESettingChanges.MatrixModelView;
                }
                else
                {
                    int iosp = index_of_selected_point;
                    P[iosp] = new Point3D(P[iosp].X - divx / BasicWidth / UserScale * 2, P[iosp].Y + divy / BasicHeight / UserScale * 2);
                    WriteCoordInTextBox(P[iosp].X, P[iosp].Y, iosp);
                    MarkChangeObject();
                }

                LastPosition = new Point(e.GetPosition(OpenGLGrid).X, e.GetPosition(OpenGLGrid).Y);
                
            }
            if (PressedMouseRightButton)
            {
                Point CurrentPosition = new Point(e.GetPosition(OpenGLGrid).X, e.GetPosition(OpenGLGrid).Y);

                double alpha = LastPosition.X - CurrentPosition.X;

                RotateAngle += alpha;
                Rotate.Text = $"{RotateAngle % 360}";

                LastPosition = new Point(e.GetPosition(OpenGLGrid).X, e.GetPosition(OpenGLGrid).Y);
                MarkChangeSettings();
                FSettingChanges = ESettingChanges.MatrixModelView;
            }
            else
            {
                double x = 2 * (e.GetPosition(OpenGLGrid).X / OpenGLGrid.ActualWidth) - 1;
                double y = 1 - 2 * (e.GetPosition(OpenGLGrid).Y / OpenGLGrid.ActualHeight);

                double minLength = double.MaxValue;
                int index = -1;

                for (int i = 0; i < 6; i++)
                {
                    Point3D p = (P[i] * ViewMatrix) * ProjectionMatrix;
                    double length = Math.Sqrt((p.X - x) * (p.X - x) + (p.Y - y) * (p.Y - y));
                    if (length < 0.1)
                    {
                        if (minLength > length)
                        {
                            minLength = length;
                            index = i;
                        }
                    }
                }

                index_of_selected_point = index;

                if (index_of_selected_point == -1)
                {
                    WriteInDebugTextBlock("нет ближайшей точки");
                } else
                {
                    WriteInDebugTextBlock($"ближайшая точка - {index_of_selected_point}");
                }
            }
            
        }
        private void CanvasGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double scale = e.Delta > 0 ? 1.1d : 1d / 1.1d;
            UserScale *= scale;
            MarkChangeSettings();
            FSettingChanges = ESettingChanges.MatrixModelView;
        }
        #endregion

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
        private void Rotate_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_RotateXAngle = ReadTextBlock<double>(Rotate, 0);

            RotateAngle = new_RotateXAngle;
            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.MatrixModelView;
        }
        private void ScaleX_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_ScaleXValue = ReadTextBlock<double>(ScaleX, 0);

            ScaleXValue = new_ScaleXValue;
            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.MatrixModelView;
        }
        private void ScaleY_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_ScaleYValue = ReadTextBlock<double>(ScaleY, 0);

            ScaleYValue = new_ScaleYValue;
            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.MatrixModelView;
        }
        private void TranslationX_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_TranslationXValue = ReadTextBlock<double>(TranslationX, 0);

            TranslationXValue = new_TranslationXValue;
            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.MatrixModelView;
        }
        private void TranslationY_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_TranslationYValue = ReadTextBlock<double>(TranslationY, 0);

            TranslationYValue = new_TranslationYValue;
            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.MatrixModelView;
        }
        private void SetRotationValues(double alpha)
        {
            RotateAngle = alpha;
            Rotate.Text = RotateAngle.ToString();
        }
        #endregion

        #region Функции считывания параметров сплайна

        private void P1x_TextChanged(object sender, TextChangedEventArgs e)
        {
            double px = ReadTextBlock<double>(P1x, 1);

            P[0] = new Point3D(px, P[0].Y);
            MarkChangeObject();
        }
        private void P1y_TextChanged(object sender, TextChangedEventArgs e)
        {
            double py = ReadTextBlock<double>(P1y, 1);

            P[0] = new Point3D(P[0].X, py);
            MarkChangeObject();
        }
        private void P2x_TextChanged(object sender, TextChangedEventArgs e)
        {
            double px = ReadTextBlock<double>(P2x, 1);

            P[1] = new Point3D(px, P[1].Y);
            MarkChangeObject();
        }
        private void P2y_TextChanged(object sender, TextChangedEventArgs e)
        {
            double py = ReadTextBlock<double>(P2y, 1);

            P[1] = new Point3D(P[1].X, py);
            MarkChangeObject();
        }
        private void P3x_TextChanged(object sender, TextChangedEventArgs e)
        {
            double px = ReadTextBlock<double>(P3x, 1);

            P[2] = new Point3D(px, P[2].Y);
            MarkChangeObject();
        }
        private void P3y_TextChanged(object sender, TextChangedEventArgs e)
        {
            double py = ReadTextBlock<double>(P3y, 1);

            P[2] = new Point3D(P[2].X, py);
            MarkChangeObject();
        }
        private void P4x_TextChanged(object sender, TextChangedEventArgs e)
        {
            double px = ReadTextBlock<double>(P4x, 1);

            P[3] = new Point3D(px, P[3].Y);
            MarkChangeObject();
        }
        private void P4y_TextChanged(object sender, TextChangedEventArgs e)
        {
            double py = ReadTextBlock<double>(P4y, 1);

            P[3] = new Point3D(P[3].X, py);
            MarkChangeObject();
        }
        private void P5x_TextChanged(object sender, TextChangedEventArgs e)
        {
            double px = ReadTextBlock<double>(P5x, 1);

            P[4] = new Point3D(px, P[4].Y);
            MarkChangeObject();
        }
        private void P5y_TextChanged(object sender, TextChangedEventArgs e)
        {
            double py = ReadTextBlock<double>(P5y, 1);

            P[4] = new Point3D(P[4].X, py);
            MarkChangeObject();
        }
        private void P6x_TextChanged(object sender, TextChangedEventArgs e)
        {
            double px = ReadTextBlock<double>(P6x, 1);

            P[5] = new Point3D(px, P[5].Y);
            MarkChangeObject();
        }
        private void P6y_TextChanged(object sender, TextChangedEventArgs e)
        {
            double py = ReadTextBlock<double>(P6y, 1);

            P[5] = new Point3D(P[5].X, py);
            MarkChangeObject();
        }
        private void WriteCoordInTextBox(double x, double y, int index)
        {
            switch (index)
            {
                case 0:
                    P1x.Text = x.ToString();
                    P1y.Text = y.ToString();
                    break;
                case 1:
                    P2x.Text = x.ToString(); 
                    P2y.Text = y.ToString(); 
                    break;
                case 2:
                    P3x.Text = x.ToString();
                    P3y.Text = y.ToString();
                    break;
                case 3:
                    P4x.Text = x.ToString();
                    P4y.Text = y.ToString();
                    break;
                case 4:
                    P5x.Text = x.ToString();
                    P5y.Text = y.ToString();
                    break;
                case 5:
                    P6x.Text = x.ToString();
                    P6y.Text = y.ToString();
                    break;
                default:
                    break;
            }
        }
        private void Aproximation_TextChanged(object sender, TextChangedEventArgs e)
        {
            AproximationValue = ReadTextBlock<double>(Aproximation, 0.01);

            MarkChangeObject();
        }

        #endregion
    }
}