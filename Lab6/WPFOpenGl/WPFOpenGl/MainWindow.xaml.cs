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
        private readonly double BasicHeight = 800.00d;
        private readonly double BasicWidth = 800.00d;
        private bool ChangeSettings { get; set; }
        private bool ChangeObject { get; set; }
        private bool PressedMouseLeftButton { get; set; }
        private bool PressedMouseRightButton { get; set; }
        private bool UseVBO { get; set; }
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

        #region Переменные для отрисовки объекта
        public double UserScale { get; set; }
        public EProjectionMode FProjectionMode { get; set; }
        public EDrawMode FDrawMode { get; set; }
        public ESettingChanges FSettingChanges { get; set; }
        public Matrix4D AllTransformations { get; set; }
        public Matrix4D ProjectionMatrix { get; set; }
        public Matrix4D ViewMatrix { get; set; }

        private readonly uint[] BUFFERS = new uint[2];
        #endregion

        #region Параболоид и его параметры
        public double ParaboloidHeight { get; set; }
        public double ParaboloidK { get; set; }
        public int ParaboloidDPhi { get; set; }
        public int ParaboloidDH { get; set; }
        public Object4D Parabaloid { get; set; }
        public Material ParaboloidMaterial { get; set; }
        public float ParaboloidMK { get; set; }
        public float ParaboloidMD { get; set; }
        #endregion

        #region Переменные шейдеров

        public virtual bool useShader { get; set; }
        private uint prog_shader;
        private uint vert_shader, frag_shader;
        private int attrib_coord, attrib_normal;
        private int uniform_proj, uniform_view, uniform_fragColor, uniform_viewNormal,
            uniform_lightPosition, uniform_time,
            uniform_lightKA, uniform_lightKD, uniform_lightKS, uniform_p,
            uniform_lightIA, uniform_lightID, uniform_md, uniform_mk;
        private float curent_time = 0;

        #endregion

        public PointLight LightSource { get; set; }
        public float ParaboloidR { get; private set; }
        public float ParaboloidG { get; private set; }
        public float ParaboloidB { get; private set; }

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
            ParaboloidR = 1;
            ParaboloidG = 1;
            ParaboloidB = 1;
            ParaboloidMaterial = new Material()
            {
                Color = new SolidColorBrush(Color.FromScRgb(0, ParaboloidR, ParaboloidG, ParaboloidB)),
                Ka = new Vector4D(0.0d, 0.1d, 0.0d),
                Kd = new Vector4D(0.0d, 0.0d, 1.0d),
                Ks = new Vector4D(1.0d, 0.0d, 0.0d),
                P = 1.0d,
            };

            LightSource = new PointLight()
            {
                X = 0,
                Y = 0,
                Z = 1,
                W = 1,
                Ia = new Vector4D(0, 0, 0, 0),
                Il = new Vector4D(0, 0, 0, 0),
            };
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
                    paraboloid_points[iter] = new Point4D(0.0d, 0.0d, 0.0d, 1, 0.0d, -1.0d, 0.0d);
                    iter++;
                    continue;
                }

                double p = 0.0;
                for (int phi = 0; phi < ParaboloidDPhi; phi++, p += 2 * Math.PI / ParaboloidDPhi)
                {
                    (double p_x, double p_y, double p_z) = (Math.Sqrt(z / ParaboloidK) * Math.Cos(p), z, Math.Sqrt(z / ParaboloidK) * Math.Sin(p));
                    double length = Math.Sqrt(9 * p_x * p_x + (p_y - 1 / ParaboloidK) * (p_y - 1 / ParaboloidK) + 9 * p_z * p_z);
                    (double p_nx, double p_ny, double p_nz) = (3 * p_x / length, (p_y - 1/ParaboloidK) / length, 3 * p_z / length);
                    if (h == ParaboloidDH)
                    {
                        length = Math.Sqrt(9 * p_x * p_x + (p_y - 1 / ParaboloidK) * (p_y - 1 / ParaboloidK) + 2 * (p_y - 1 / ParaboloidK) + 1 + 9 * p_z * p_z);
                        (p_nx, p_ny, p_nz) = (3 * p_x / length, (p_y - 1 / ParaboloidK) / length + 1, 3 * p_z / length);
                    }
                    paraboloid_points[iter] = new Point4D(p_x, p_y, p_z, 1, p_nx, p_ny, p_nz);
                    iter++;
                }

                if (h == ParaboloidDH)
                {
                    paraboloid_points[iter] = new Point4D(0.0d, z, 0.0d, 1, 0.0d, 1.0d, 0.0d);
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

        private Matrix4D NewNormalsAllTransformationsMatrix()
        {
            return (
                Matrix4D.ScaleMatrix(1 / (ScaleXValue * UserScale), 1 / (ScaleYValue * UserScale), 1 / (ScaleZValue * UserScale)) *
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
                { 0, 0, 1, 1 },
            });
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
                    gl.UniformMatrix4(uniform_view, 1, false, ViewMatrix.ToFloatArray());
                    gl.UniformMatrix4(uniform_viewNormal, 1, false, NewNormalsAllTransformationsMatrix().ToFloatArray());
                }
                if (((int)FSettingChanges & (int)ESettingChanges.MatrixProjection) != 0)
                {
                    if (FProjectionMode == EProjectionMode.Perspective)
                    {
                        ProjectionMatrix = Perspective(90, OpenGLGrid.ActualWidth / OpenGLGrid.ActualHeight, -1, 1);
                    }
                    else
                    {
                        ProjectionMatrix = Matrix4D.ScaleMatrix(BasicWidth / (ActualWidth - 200), BasicHeight / ActualHeight, 0);
                    }

                    gl.MatrixMode(MatrixMode.Projection);
                    gl.LoadMatrix(ProjectionMatrix.ToArray());
                    gl.UniformMatrix4(uniform_proj, 1, false, ProjectionMatrix.ToFloatArray());
                }
                if (((int)FSettingChanges & (int)ESettingChanges.PolygonMode) != 0)
                {
                    if (FDrawMode == EDrawMode.Polyline)
                    {
                        gl.UseProgram(0);
                        if (gl.IsEnabled(OpenGL.GL_CULL_FACE))
                        {
                            gl.Disable(OpenGL.GL_CULL_FACE);
                        }

                        gl.PolygonMode(FaceMode.FrontAndBack, PolygonMode.Lines);
                    }
                    else if (FDrawMode == EDrawMode.PolylineVisible)
                    {
                        gl.UseProgram(0);
                        gl.Enable(OpenGL.GL_CULL_FACE);
                        gl.CullFace(OpenGL.GL_FRONT);
                        gl.PolygonMode(FaceMode.FrontAndBack, PolygonMode.Lines);
                    }
                    else if (FDrawMode == EDrawMode.Polygon)
                    {
                        gl.UseProgram(0);
                        gl.Enable(OpenGL.GL_CULL_FACE);
                        gl.CullFace(OpenGL.GL_FRONT);
                        gl.PolygonMode(FaceMode.FrontAndBack, PolygonMode.Filled);
                    }
                    else if (FDrawMode == EDrawMode.FongShading)
                    {
                        gl.UseProgram(prog_shader);
                        gl.UniformMatrix4(uniform_view, 1, false, ViewMatrix.ToFloatArray());
                        gl.UniformMatrix4(uniform_viewNormal, 1, false, NewNormalsAllTransformationsMatrix().ToFloatArray());
                        gl.UniformMatrix4(uniform_proj, 1, false, ProjectionMatrix.ToFloatArray());

                        gl.Enable(OpenGL.GL_CULL_FACE);
                        gl.CullFace(OpenGL.GL_FRONT);
                        gl.PolygonMode(FaceMode.FrontAndBack, PolygonMode.Filled);
                    }
                }
                if (((int)FSettingChanges & (int)ESettingChanges.ColorMode) != 0)
                {
                    gl.Uniform3(uniform_fragColor, ParaboloidR, ParaboloidG, ParaboloidB);
                    gl.Uniform3(uniform_lightPosition, (float)LightSource.Coordinates.X, (float)LightSource.Coordinates.Y, (float)LightSource.Coordinates.Z);
                    gl.Uniform3(uniform_lightKA, (float)ParaboloidMaterial.Ka.Dest.X, (float)ParaboloidMaterial.Ka.Dest.Y, (float)ParaboloidMaterial.Ka.Dest.Z);
                    gl.Uniform3(uniform_lightKD, (float)ParaboloidMaterial.Kd.Dest.X, (float)ParaboloidMaterial.Kd.Dest.Y, (float)ParaboloidMaterial.Kd.Dest.Z);
                    gl.Uniform3(uniform_lightKS, (float)ParaboloidMaterial.Ks.Dest.X, (float)ParaboloidMaterial.Ks.Dest.Y, (float)ParaboloidMaterial.Ks.Dest.Z);
                    gl.Uniform3(uniform_lightIA, (float)LightSource.Ia.Dest.X, (float)LightSource.Ia.Dest.Y, (float)LightSource.Ia.Dest.Z);
                    gl.Uniform3(uniform_lightID, (float)LightSource.Il.Dest.X, (float)LightSource.Il.Dest.Y, (float)LightSource.Il.Dest.Z);

                    gl.Uniform1(uniform_p, (float)ParaboloidMaterial.P);
                    gl.Uniform1(uniform_md, ParaboloidMD);
                    gl.Uniform1(uniform_mk, ParaboloidMK);
                }

                FSettingChanges = 0;
                ChangeSettings = false;
            }

            /// Изменение параметров объекта
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
                        gl.BufferData(OpenGL.GL_ARRAY_BUFFER, sizeof(Point4D) * Parabaloid.Points.Length, (IntPtr)p, OpenGL.GL_STATIC_DRAW);
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

            /// Отрисовка
            unsafe
            {

                gl.Begin(OpenGL.GL_LINE_LOOP);
                gl.Vertex((LightSource.Coordinates - new Point4D(0.1, 0.1, 0.1, 1, 0 ,0 ,0)).Dest.ToFloatArray());
                gl.Color(0, 0, 0);
                gl.Vertex((LightSource.Coordinates - new Point4D(0.1, 0.1, -0.1, 1, 0, 0, 0)).Dest.ToFloatArray());
                gl.Color(0, 0, 0);
                gl.Vertex((LightSource.Coordinates - new Point4D(0.1, -0.1, -0.1, 1, 0, 0, 0)).Dest.ToFloatArray());
                gl.Color(0, 0, 0);
                gl.Vertex((LightSource.Coordinates - new Point4D(0.1, -0.1, 0.1, 1, 0, 0, 0)).Dest.ToFloatArray());
                gl.Color(0, 0, 0);
                gl.Vertex((LightSource.Coordinates - new Point4D(-0.1, -0.1, 0.1, 1, 0, 0, 0)).Dest.ToFloatArray());
                gl.Color(0, 0, 0);
                gl.Vertex((LightSource.Coordinates - new Point4D(-0.1, 0.1, 0.1, 1, 0, 0, 0)).Dest.ToFloatArray());
                gl.Color(0, 0, 0);
                gl.Vertex((LightSource.Coordinates - new Point4D(-0.1, 0.1, -0.1, 1, 0, 0, 0)).Dest.ToFloatArray());
                gl.Color(0, 0, 0);
                gl.Vertex((LightSource.Coordinates - new Point4D(-0.1, -0.1, -0.1, 1, 0, 0, 0)).Dest.ToFloatArray());
                gl.Color(0, 0, 0);
                gl.End();

                if (FDrawMode != EDrawMode.FongShading)
                {
                    gl.EnableClientState(OpenGL.GL_VERTEX_ARRAY);
                    gl.EnableClientState(OpenGL.GL_NORMAL_ARRAY);
                    gl.EnableClientState(OpenGL.GL_COLOR_ARRAY);

                    if (UseVBO)
                    {
                        gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, BUFFERS[0]);
                        gl.VertexPointer(4, OpenGL.GL_DOUBLE, sizeof(Point4D), (IntPtr)0);
                        gl.ColorPointer(3, OpenGL.GL_DOUBLE, sizeof(Point4D), (IntPtr)(sizeof(double) * 7));
                        gl.NormalPointer(OpenGL.GL_DOUBLE, sizeof(Point4D), (IntPtr)(sizeof(double) * 4));

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
                            gl.NormalPointer(OpenGL.GL_DOUBLE, sizeof(Point4D), (IntPtr)(&p->nX));
                            gl.ColorPointer(3, OpenGL.GL_DOUBLE, sizeof(Point4D), (IntPtr)(&p->R));
                        }

                        fixed (uint* p1 = Parabaloid.PolygonsIndexes)
                        {
                            gl.DrawElements(OpenGL.GL_TRIANGLES, Parabaloid.PolygonsIndexes.Length, OpenGL.GL_UNSIGNED_INT, (IntPtr)p1);
                        }
                    }

                    gl.DisableClientState(OpenGL.GL_COLOR_ARRAY);
                    gl.DisableClientState(OpenGL.GL_NORMAL_ARRAY);
                    gl.DisableClientState(OpenGL.GL_VERTEX_ARRAY);
                }
                else
                {
                    gl.EnableVertexAttribArray((uint)attrib_coord);
                    gl.EnableVertexAttribArray((uint)attrib_normal);

                    gl.Uniform1(uniform_time, curent_time += 0.1f);

                    if (UseVBO)
                    {
                        gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, BUFFERS[0]);
                        gl.VertexAttribPointer((uint)attrib_coord, 4, OpenGL.GL_DOUBLE, false, sizeof(Point4D), (IntPtr)(0));
                        gl.VertexAttribPointer((uint)attrib_normal, 3, OpenGL.GL_DOUBLE, true, sizeof(Point4D), (IntPtr)(sizeof(double) * 4));

                        gl.BindBuffer(OpenGL.GL_ELEMENT_ARRAY_BUFFER, BUFFERS[1]);
                        gl.DrawElements(OpenGL.GL_TRIANGLES, Parabaloid.PolygonsIndexes.Length, OpenGL.GL_UNSIGNED_INT, (IntPtr)0);

                        gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, 0);
                        gl.BindBuffer(OpenGL.GL_ELEMENT_ARRAY_BUFFER, 0);
                    }
                    else
                    {
                        fixed (Point4D* p = Parabaloid.Points)
                        {
                            gl.VertexAttribPointer((uint)attrib_coord, 4, OpenGL.GL_DOUBLE, false, sizeof(Point4D), (IntPtr)(&p->X));
                            gl.VertexAttribPointer((uint)attrib_normal, 3, OpenGL.GL_DOUBLE, true, sizeof(Point4D), (IntPtr)(&(p->nX)));
                        }

                        fixed (uint* p1 = Parabaloid.PolygonsIndexes)
                        {
                            gl.DrawElements(OpenGL.GL_TRIANGLES, Parabaloid.PolygonsIndexes.Length, OpenGL.GL_UNSIGNED_INT, (IntPtr)p1);
                        }
                    }

                    gl.DisableVertexAttribArray((uint)attrib_normal);
                    gl.DisableVertexAttribArray((uint)attrib_coord);
                }


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
            gl.LoadMatrix((Matrix4D.ScaleMatrix(0.5, 0.5, 0.5) * Matrix4D.RotateMatrix(-45, 35, 30)).ToArray());
            gl.MatrixMode(MatrixMode.Projection);
            gl.LoadMatrix(Matrix4D.ScaleMatrix(0, 0, 0).ToArray());

            #region Компиляция шейдеров

            var parameters = new int[1];
            var load_and_compile = new Func<uint, string, uint>(
                (shader_type, shader_name) =>
                {
                    var shader = gl.CreateShader(shader_type);
                    if (shader == 0)
                        throw new Exception("OpenGL Error: не удалось создать объект шейдера");

                    var source = HelpUtils.GetTextFileFromRes(shader_name);
                    gl.ShaderSource(shader, source);
                    gl.CompileShader(shader);

                    gl.GetShader(shader, OpenGL.GL_COMPILE_STATUS, parameters);
                    if (parameters[0] != OpenGL.GL_TRUE)
                    {
                        gl.GetShader(shader, OpenGL.GL_INFO_LOG_LENGTH, parameters);
                        var stringBuilder = new StringBuilder(parameters[0]);
                        gl.GetShaderInfoLog(shader, parameters[0], IntPtr.Zero, stringBuilder);
                        Debug.WriteLine("\n\n\n\n ====== SHADER GL_COMPILE_STATUS: ======");
                        Debug.WriteLine(stringBuilder);
                        Debug.WriteLine("==================================");
                        throw new Exception("OpenGL Error: ошибка во при компиляции " + (
                            shader_type == OpenGL.GL_VERTEX_SHADER ? "вершиного шейдера"
                            : shader_type == OpenGL.GL_FRAGMENT_SHADER ? "фрагментного шейдера"
                            : "какого-то еще щеёдера"));
                    }

                    gl.AttachShader(prog_shader, shader);
                    return shader;
                });

            prog_shader = gl.CreateProgram();
            if (prog_shader == 0)
                throw new Exception("OpenGL Error: не удалось создать шейдерную программу");

            vert_shader = load_and_compile(OpenGL.GL_VERTEX_SHADER, "sample.vert");
            frag_shader = load_and_compile(OpenGL.GL_FRAGMENT_SHADER, "sample.frag");

            gl.LinkProgram(prog_shader);
            gl.GetProgram(prog_shader, OpenGL.GL_LINK_STATUS, parameters);
            if (parameters[0] != OpenGL.GL_TRUE)
                throw new Exception("OpenGL Error: ошибка линковкой");

            #endregion

            #region Связывание аттрибутов и юниформ

            attrib_coord = gl.GetAttribLocation(prog_shader, "coord");
            if (attrib_coord < 0)
                throw new Exception("OpenGL Error: не удалость связать аттрибут coord");

            attrib_normal = gl.GetAttribLocation(prog_shader, "normal");
            if (attrib_normal < 0)
                throw new Exception("OpenGL Error: не удалость связать аттрибут normal");

            uniform_time = gl.GetUniformLocation(prog_shader, "time");
            if (uniform_time < 0)
                throw new Exception("OpenGL Error: не удалость связать аттрибут time");

            uniform_proj = gl.GetUniformLocation(prog_shader, "proj4d");
            if (uniform_proj < 0)
                throw new Exception("OpenGL Error: не удалость связать аттрибут proj4d");

            uniform_view = gl.GetUniformLocation(prog_shader, "view4d");
            if (uniform_view < 0)
                throw new Exception("OpenGL Error: не удалость связать аттрибут view4d");

            uniform_viewNormal = gl.GetUniformLocation(prog_shader, "view4dnormals");
            if (uniform_viewNormal < 0)
                throw new Exception("OpenGL Error: не удалость связать аттрибут view4dnormals");

            uniform_fragColor = gl.GetUniformLocation(prog_shader, "fragColor");
            if (uniform_fragColor < 0)
                throw new Exception("OpenGL Error: не удалость связать аттрибут fragColor");

            uniform_lightPosition = gl.GetUniformLocation(prog_shader, "lightPosition");
            if (uniform_lightPosition < 0)
                throw new Exception("OpenGL Error: не удалость связать аттрибут lightPosition");

            uniform_lightKA = gl.GetUniformLocation(prog_shader, "lightKA");
            if (uniform_lightKA < 0)
                throw new Exception("OpenGL Error: не удалость связать аттрибут lightKA");

            uniform_lightKD = gl.GetUniformLocation(prog_shader, "lightKD");
            if (uniform_lightKD < 0)
                throw new Exception("OpenGL Error: не удалость связать аттрибут lightKD");

            uniform_lightKS = gl.GetUniformLocation(prog_shader, "lightKS");
            if (uniform_lightKS < 0)
                throw new Exception("OpenGL Error: не удалость связать аттрибут lightKS");

            uniform_lightIA = gl.GetUniformLocation(prog_shader, "lightIA");
            if (uniform_lightIA < 0)
                throw new Exception("OpenGL Error: не удалость связать аттрибут lightIA");

            uniform_lightID = gl.GetUniformLocation(prog_shader, "lightID");
            if (uniform_lightID < 0)
                throw new Exception("OpenGL Error: не удалость связать аттрибут lightID");

            uniform_mk = gl.GetUniformLocation(prog_shader, "mk");
            if (uniform_mk < 0)
                throw new Exception("OpenGL Error: не удалость связать аттрибут mk");

            uniform_md = gl.GetUniformLocation(prog_shader, "md");
            if (uniform_md < 0)
                throw new Exception("OpenGL Error: не удалость связать аттрибут md");

            uniform_p = gl.GetUniformLocation(prog_shader, "p");
            if (uniform_p < 0)
                throw new Exception("OpenGL Error: не удалость связать аттрибут p");
            #endregion
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

                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    TranslationXValue -= divx / OpenGLGrid.ActualWidth * 2;
                    TranslationZValue += divy / OpenGLGrid.ActualHeight * 2;
                    TranslationX.Text = TranslationXValue.ToString();
                    TranslationZ.Text = TranslationZValue.ToString();
                }
                else if (Keyboard.IsKeyDown(Key.LeftAlt))
                {
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        LightSource.X -= divx / 100;
                        LightSource.Z += divy / 100;
                        P_X.Text = LightSource.X.ToString();
                        P_Z.Text = LightSource.Z.ToString();
                    }
                    else
                    {
                        LightSource.X -= divx / 100;
                        LightSource.Y += divy / 100;
                        P_X.Text = LightSource.X.ToString();
                        P_Y.Text = LightSource.Y.ToString();
                    }

                    FSettingChanges |= ESettingChanges.ColorMode;
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
                FSettingChanges |= ESettingChanges.MatrixModelView;
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
        private void RotateX_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_RotateXAngle = ReadTextBlock<double>(RotateX, 0);

            RotateXAngle = new_RotateXAngle;
            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.MatrixModelView;
        }
        private void RotateY_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_RotateYAngle = ReadTextBlock<double>(RotateY, 0);

            RotateYAngle = new_RotateYAngle;
            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.MatrixModelView;
        }
        private void RotateZ_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_RotateZAngle = ReadTextBlock<double>(RotateZ, 0);

            RotateZAngle = new_RotateZAngle;
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
        private void ScaleZ_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_ScaleZValue = ReadTextBlock<double>(ScaleZ, 0);

            ScaleZValue = new_ScaleZValue;
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
        private void TranslationZ_TextChanged(object sender, TextChangedEventArgs e)
        {
            double new_TranslationZValue = ReadTextBlock<double>(TranslationZ, 0);

            TranslationZValue = new_TranslationZValue;
            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.MatrixModelView;
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
            if (text == "Затенение по Фонгу")
            {
                FDrawMode = EDrawMode.FongShading;

                FSettingChanges |= ESettingChanges.ColorMode;
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

        #region Функции считывания параметров цвета
        private void R_TextChanged(object sender, TextChangedEventArgs e)
        {
            ParaboloidR = ReadTextBlock<float>(R, 1);

            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.ColorMode;
        }
        private void G_TextChanged(object sender, TextChangedEventArgs e)
        {
            ParaboloidG = ReadTextBlock<float>(G, 1);

            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.ColorMode;
        }
        private void B_TextChanged(object sender, TextChangedEventArgs e)
        {
            ParaboloidB = ReadTextBlock<float>(B, 1);

            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.ColorMode;
        }
        private void P_X_TextChanged(object sender, TextChangedEventArgs e)
        {
            LightSource.X = ReadTextBlock<double>(P_X, 0);

            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.ColorMode;
        }
        private void P_Y_TextChanged(object sender, TextChangedEventArgs e)
        {
            LightSource.Y = ReadTextBlock<double>(P_Y, 0);

            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.ColorMode;
        }
        private void P_Z_TextChanged(object sender, TextChangedEventArgs e)
        {
            LightSource.Z = ReadTextBlock<double>(P_Z, 0);

            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.ColorMode;
        }
        private void Ia_X_TextChanged(object sender, TextChangedEventArgs e)
        {
            LightSource.Ia.Dest.X = ReadTextBlock<double>(Ia_X, 0);

            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.ColorMode;
        }
        private void Ia_Y_TextChanged(object sender, TextChangedEventArgs e)
        {
            LightSource.Ia.Dest.Y = ReadTextBlock<double>(Ia_Y, 0);

            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.ColorMode;
        }
        private void Ia_Z_TextChanged(object sender, TextChangedEventArgs e)
        {
            LightSource.Ia.Dest.Z = ReadTextBlock<double>(Ia_Z, 0);

            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.ColorMode;
        }
        private void Il_X_TextChanged(object sender, TextChangedEventArgs e)
        {
            LightSource.Il.Dest.X = ReadTextBlock<double>(Il_X, 0);

            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.ColorMode;
        }
        private void Il_Y_TextChanged(object sender, TextChangedEventArgs e)
        {
            LightSource.Il.Dest.Y = ReadTextBlock<double>(Il_Y, 0);

            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.ColorMode;
        }
        private void Il_Z_TextChanged(object sender, TextChangedEventArgs e)
        {
            LightSource.Il.Dest.Z = ReadTextBlock<double>(Il_Z, 0);

            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.ColorMode;
        }
        private void P_TextChanged(object sender, TextChangedEventArgs e)
        {
            ParaboloidMaterial.P = ReadTextBlock<double>(P, 0);

            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.ColorMode;
        }
        private void Ka_X_TextChanged(object sender, TextChangedEventArgs e)
        { 
            ParaboloidMaterial.Ka.Dest.X = ReadTextBlock<double>(Ka_X, 0);

            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.ColorMode;
        }
        private void Ka_Y_TextChanged(object sender, TextChangedEventArgs e) 
        {
            ParaboloidMaterial.Ka.Dest.Y = ReadTextBlock<double>(Ka_Y, 0);

            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.ColorMode;
        }
        private void Ka_Z_TextChanged(object sender, TextChangedEventArgs e) 
        {
            ParaboloidMaterial.Ka.Dest.Z = ReadTextBlock<double>(Ka_Z, 0);

            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.ColorMode;
        }
        private void Kd_X_TextChanged(object sender, TextChangedEventArgs e) 
        {
            ParaboloidMaterial.Kd.Dest.X = ReadTextBlock<double>(Kd_X, 0);

            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.ColorMode;
        }
        private void Kd_Y_TextChanged(object sender, TextChangedEventArgs e) 
        {
            ParaboloidMaterial.Kd.Dest.Y = ReadTextBlock<double>(Kd_Y, 0);

            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.ColorMode;
        }
        private void Kd_Z_TextChanged(object sender, TextChangedEventArgs e) 
        {
            ParaboloidMaterial.Kd.Dest.Z = ReadTextBlock<double>(Kd_Z, 0);

            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.ColorMode;
        }
        private void Ks_X_TextChanged(object sender, TextChangedEventArgs e) 
        {
            ParaboloidMaterial.Ks.Dest.X = ReadTextBlock<double>(Ks_X, 0);

            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.ColorMode;
        }
        private void Ks_Y_TextChanged(object sender, TextChangedEventArgs e) 
        {
            ParaboloidMaterial.Ks.Dest.Y = ReadTextBlock<double>(Ks_Y, 0);

            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.ColorMode;
        }
        private void Ks_Z_TextChanged(object sender, TextChangedEventArgs e) 
        {
            ParaboloidMaterial.Ks.Dest.Z = ReadTextBlock<double>(Ks_Z, 0);

            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.ColorMode;
        }
        private void MD_TextChanged(object sender, TextChangedEventArgs e) 
        {
            ParaboloidMD = ReadTextBlock(MD, 0.0f);

            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.ColorMode;
        }
        private void MK_TextChanged(object sender, TextChangedEventArgs e) 
        {
            ParaboloidMK = ReadTextBlock(MK, 0.0f);

            MarkChangeSettings();
            FSettingChanges |= ESettingChanges.ColorMode;
        }

        #endregion
    }
}