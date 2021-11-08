using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Geometry;

namespace View
{
    public enum EProjectionMode
    {
        SideView = 0,
        FrontView = 1,
        AboveView = 2,
        Isometry = 3,
        Free = 4,
    }

    public enum EDrawMode
    {
        Polyline = 1,
        PolylineVisible = 2,
        Polygon = 4,
        PolygonRandom = 8,
        FlatShading = 16,
        GouraudShading = 32,
    }

    class Camera
    {
        public Point4D Position { get; set; }
        public Vector4D Forward { get; set; }
        public Vector4D Up { get; set; }
        public Vector4D Right { get; set; }
        private Lab1.MainWindow MW { get; set; }
        public Camera(Point4D position, Lab1.MainWindow mw)
        {
            Position = position;
            Forward = new Vector4D(0, 0, -1, 1);
            Up = new Vector4D(0, 1, 0, 1);
            Right = new Vector4D(1, 0, 0, 1);
            MW = mw;
        }

        public void AddLineInCanvas(double _X1, double _X2, double _Y1, double _Y2, SolidColorBrush scb)
        {
            _ = MW.Canvas.Children.Add(
                new Line
                {
                    X1 = _X1,
                    X2 = _X2,
                    Y1 = _Y1,
                    Y2 = _Y2,
                    Stroke = scb,
                }
            );
        }

        public void AddLineInCanvas(double _X1, double _X2, double _Y1, double _Y2, SolidColorBrush scb, int i)
        {
            AddLineInCanvas(_X1, _X2, _Y1, _Y2, scb);

            TextBlock t = new TextBlock()
            {
                Text = $"({i})",
                Foreground = scb,
            };

            Canvas.SetLeft(t, _X1 - 2);
            Canvas.SetTop(t, _Y1 - 2);

            _ = MW.Canvas.Children.Add(
                t
            );
        }

        public void AddPolyLineInCanvas(Polyline polyline, int i = -1, Point4D p = null)
        {
            _ = MW.Canvas.Children.Add(polyline);

            if (i != -1 && p != null)
            {
                TextBlock t = new TextBlock()
                {
                    Text = $"({i})",
                    Foreground = polyline.Stroke,
                };

                Canvas.SetLeft(t, p.X - 2);
                Canvas.SetTop(t, p.Y - 2);

                _ = MW.Canvas.Children.Add(
                    t
                );
            }
        }

        public void AddPolyGonInCanvas(Polygon polygon, int i = -1)
        {
            _ = MW.Canvas.Children.Add(polygon);

            if (i != -1)
            {
                TextBlock t = new TextBlock()
                {
                    Text = $"({i})",
                    Foreground = polygon.Stroke,
                };

                Canvas.SetLeft(t, polygon.ActualWidth / 2);
                Canvas.SetTop(t, polygon.ActualHeight / 2);

                _ = MW.Canvas.Children.Add(
                    t
                );
            }
        }

        public void AddPolyGonInCanvas(Polyline polyline, int i = -1)
        {

            /*foreach (Point p in polyline.Points)
            {
                TextBlock t = new TextBlock()
                {
                    Text = $"({p.X.ToString("F")}, {p.Y.ToString("F")})",
                    Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                };

                Canvas.SetLeft(t, p.X - 2);
                Canvas.SetTop(t, p.Y - 2);

                _ = MW.Canvas.Children.Add(
                    t
                );
            }*/

            Polygon polygon = new Polygon()
            {
                Points = polyline.Points,
                Stroke = polyline.Stroke,
                Fill = polyline.Stroke,
            };

            polygon.MouseLeftButtonUp += MW.CanvasGrid_MouseLeftButtonUp;
            polygon.MouseRightButtonUp += MW.CanvasGrid_MouseRightButtonUp;

            AddPolyGonInCanvas(polygon, i);
        }

        public void AddPolyGonInCanvas(Polyline polyline, SolidColorBrush randcolor, int i = -1)
        {
            Polygon polygon = new Polygon()
            {
                Points = polyline.Points,
                Stroke = polyline.Stroke,
                Fill = randcolor,
            };

            polygon.MouseLeftButtonUp += MW.CanvasGrid_MouseLeftButtonUp;
            polygon.MouseRightButtonUp += MW.CanvasGrid_MouseRightButtonUp;

            AddPolyGonInCanvas(polygon, i);
        }

        public void AddPointInCanvas(double _X, double _Y, SolidColorBrush scb)
        {
            Ellipse el = new Ellipse
            {
                Width = 4,
                Height = 4,
                Fill = scb,
                Stroke = scb,
                StrokeThickness = 3
            };

            Canvas.SetLeft(el, _X-2);
            Canvas.SetTop(el, _Y-2);

            _ = MW.Canvas.Children.Add(
                el
            );
        }
        public void AddPointInCanvas(Point4D p, SolidColorBrush scb) => AddPointInCanvas(p.X, p.Y, scb);
        public void AddPointInCanvas(Point4D p, SolidColorBrush scb, int i)
        {
            AddPointInCanvas(p, scb);

            TextBlock t = new TextBlock()
            {
                Text = $"({i})",
                Foreground = scb,
            };

            Canvas.SetLeft(t, p.X - 2);
            Canvas.SetTop(t, p.Y - 2);

            _ = MW.Canvas.Children.Add(
                t
            );
        }

        public void NewPoint(Point4D[] no, Point4D[] o, int i, Matrix4D AllTransformations)
        {
            no[i] = o[i] * AllTransformations;
        }

        /// <summary>
        /// Расчет освещенности в точке point
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="m"></param>
        /// <param name="point"></param>
        /// <param name="normal"></param>
        /// <returns>Возвращает суммарную освещенность</returns>
        public Vector4D Illumination(PointLight pl, Material m, Point4D point, Vector4D normal)
        {
            Vector4D Ia = m.Ka * pl.Ia;

            Vector4D L = pl.Coordinates - point;
            double cos_alpha = Vector4D.ScalarProduct(normal, Vector4D.Normalize(L));
            Vector4D Id = cos_alpha >= 0 ?
                m.Kd * pl.Il * cos_alpha :
                new Vector4D(0, 0, 0, 0);

            Vector4D R = (normal * (2.0d * L.Length * cos_alpha)) - L;
            Vector4D S = new Vector4D(0, 0, 1, 0);
            double cos_tetha = Vector4D.ScalarProduct(Vector4D.Normalize(R), Vector4D.Normalize(S));
            Vector4D Is = cos_tetha >= 0 && cos_alpha >= 0 ?
                m.Ks * pl.Il * Math.Pow(cos_tetha, m.P) :
                new Vector4D(0, 0, 0, 0);

            Vector4D I = Ia + ((Id + Is) / (MW.LightDistanceCoefMK + MW.LightDistanceCoefMD * (pl.Coordinates - point).Length));

            return I;
        }

        public void AddGradientPoligon(Polygon4D poly, Polyline polyline, PointLight pl, Material m, Matrix4D pointTransformations, Matrix4D normalsTransformations, int t = -1)
        {
            Polygon[] polygons = new Polygon[poly.points.Length];

            double max_x = polyline.Points[0].X,
                min_x = polyline.Points[0].X,
                max_y = polyline.Points[0].Y,
                min_y = polyline.Points[0].Y;

            for (int i = 0; i < polyline.Points.Count; i++)
            {
                double _x = polyline.Points[i].X,
                       _y = polyline.Points[i].Y;

                if (_x > max_x) max_x = _x;
                if (_x < min_x) min_x = _x;
                if (_y > max_y) max_y = _y;
                if (_y < min_y) min_y = _y;
            }

            for (int i = 0; i < poly.pointsIndexes.Length; i++)
            {
                Vector4D I = Illumination(pl, m, poly.points[poly.pointsIndexes[i]] * pointTransformations, Vector4D.Normalize(poly.points[poly.pointsIndexes[i]].Normal * normalsTransformations));
                Color colorbegin = Color.FromScRgb(m.Color.Color.ScA, (float)I[0] * m.Color.Color.ScR, (float)I[1] * m.Color.Color.ScG, (float)I[2] * m.Color.Color.ScB);
                
                Color colorend = i == 0 ? Color.FromScRgb(255, (float)I[0] * m.Color.Color.ScR, (float)I[1] * m.Color.Color.ScG, (float)I[2] * m.Color.Color.ScB) :
                    Color.FromScRgb(0, (float)I[0] * m.Color.Color.ScR, (float)I[1] * m.Color.Color.ScG, (float)I[2] * m.Color.Color.ScB);

                polygons[i] = new Polygon
                {
                    Points = polyline.Points,
                    Fill = new RadialGradientBrush(colorbegin, colorend)
                    {
                        GradientOrigin = new Point((polyline.Points[i].X - min_x) / (max_x - min_x), (polyline.Points[i].Y - min_y) / (max_y - min_y))
                    }
                };
                polygons[i].MouseLeftButtonUp += new MouseButtonEventHandler(MW.CanvasGrid_MouseLeftButtonUp);
                polygons[i].MouseRightButtonUp += new MouseButtonEventHandler(MW.CanvasGrid_MouseRightButtonUp);
            }

            polyline.Stroke = polygons[0].Fill.Clone();
            AddPolyGonInCanvas(polyline);

            for (int i = 0; i < poly.pointsIndexes.Length; i++)
            {
                AddPolyGonInCanvas(polygons[i], t);
            }
        }

        public void Project(List<Object4D> objects, EProjectionMode FProjectionMode, PointLight p_light)
        {
            if (objects == null)
            {
                return;
            }

            MW.Canvas.Children.Clear();

            double WholeScaleXValue = MW.ScaleXValue * MW.UserScale * MW.WindowScale;
            double WholeScaleYValue = MW.ScaleYValue * MW.UserScale * MW.WindowScale;
            double WholeScaleZValue = MW.ScaleZValue * MW.UserScale * MW.WindowScale;

            Matrix4D ProjectionMatrix = (FProjectionMode == EProjectionMode.AboveView) ? Matrix4D.ProjectionMatrixY() :
                (FProjectionMode == EProjectionMode.SideView) ? ProjectionMatrix = Matrix4D.ProjectionMatrixX() : 
                ProjectionMatrix = Matrix4D.ProjectionMatrixZ();

            Matrix4D AllTransformations = Matrix4D.ScaleMatrix(WholeScaleXValue, WholeScaleYValue, WholeScaleZValue)
                * Matrix4D.RotateMatrix(MW.RotateXAngle, MW.RotateYAngle, MW.RotateZAngle)
                * ProjectionMatrix
                * Matrix4D.TranslationMatrix(MW.TranslationXValue, MW.TranslationYValue, MW.TranslationZValue);

            Matrix4D NormalsTransformations = Matrix4D.ScaleMatrix(WholeScaleYValue * WholeScaleZValue, WholeScaleXValue * WholeScaleZValue, WholeScaleXValue * WholeScaleYValue)
                * Matrix4D.RotateMatrix(MW.RotateXAngle, MW.RotateYAngle, MW.RotateZAngle)
                * Matrix4D.TranslationMatrix(MW.TranslationXValue, MW.TranslationYValue, MW.TranslationZValue);

            Matrix4D PointLightTransformations = Matrix4D.ScaleMatrix(WholeScaleXValue, WholeScaleYValue, WholeScaleZValue)
                * Matrix4D.RotateMatrix(MW.RotateXAngle, MW.RotateYAngle, MW.RotateZAngle)
                * Matrix4D.TranslationMatrix(MW.TranslationXValue, MW.TranslationYValue, MW.TranslationZValue);

            PointLight pl = null;

            if (((int)MW.FDrawMode & 48) > 0)
            {
                AddPointInCanvas((p_light.Coordinates * AllTransformations).X, (p_light.Coordinates * AllTransformations).Y, new SolidColorBrush(Color.FromRgb(255, 255, 255)));

                pl = new PointLight
                {
                    Coordinates = p_light.Coordinates * PointLightTransformations,
                    Ia = p_light.Ia,
                    Il = p_light.Il
                };
            }

            foreach (Object4D o in objects)
            {
                Point4D[] objectPoints = new Point4D[o.Points.Length];

                _ = Parallel.For(0, o.Points.Length,
                    (i, state) =>
                    NewPoint(objectPoints, o.Points, i, AllTransformations)
                );

                for (int k = 0; k < o.Polygons.Length; k++)
                {
                    Polygon4D poly = o.Polygons[k];
                    Vector4D normal_vector = Vector4D.Normalize(poly.Normal * NormalsTransformations);
                    Polyline polyline = new Polyline() { Stroke = o.Material.Color };

                    for (int i = 0; i <= poly.pointsIndexes.Length; i++)
                    {
                        Point4D p = objectPoints[poly.pointsIndexes[i % poly.pointsIndexes.Length]];
                        polyline.Points.Add(new Point(p.X, p.Y));
                    }

                    if (MW.FDrawMode == EDrawMode.Polyline)
                    {
                        AddPolyLineInCanvas(polyline);
                    }
                    else if (MW.FDrawMode == EDrawMode.Polygon)
                    {
                        if (normal_vector.Dest.Z >= 0)
                        {
                            AddPolyGonInCanvas(polyline, o.Material.Color);
                        }
                    }
                    else if (MW.FDrawMode == EDrawMode.PolygonRandom)
                    {
                        if (normal_vector.Dest.Z >= 0)
                        {
                            AddPolyGonInCanvas(polyline, poly.Brush);
                        }
                    }
                    else if (MW.FDrawMode == EDrawMode.PolylineVisible)
                    {
                        if (normal_vector.Dest.Z >= 0)
                        {
                            AddPolyLineInCanvas(polyline);
                        }
                    }
                    else if (MW.FDrawMode == EDrawMode.FlatShading)
                    {
                        if (normal_vector.Dest.Z >= 0)
                        {
                            /*Расчет цвета полигона*/
                            Point4D barycenter = poly.Barycenter * PointLightTransformations;
                            Vector4D I = Illumination(pl, o.Material, barycenter, normal_vector);

                            polyline.Stroke = new SolidColorBrush(Color.FromScRgb(o.Material.Color.Color.ScA, (float)I[0] * o.Material.Color.Color.ScR, (float)I[1] * o.Material.Color.Color.ScG, (float)I[2] * o.Material.Color.Color.ScB));
                            AddPolyGonInCanvas(polyline);
                        }
                    }
                    else if (MW.FDrawMode == EDrawMode.GouraudShading)
                    {
                        if (normal_vector.Dest.Z >= 0)
                        {
                            AddGradientPoligon(poly, polyline, pl, o.Material, PointLightTransformations, NormalsTransformations);
                        }
                    }


                    if (MW.ViewNormals)
                    {
                        Point4D barycenter = poly.Barycenter * AllTransformations;
                        normal_vector = normal_vector * ProjectionMatrix * Matrix4D.ScaleMatrix(100 * MW.UserScale, 100 * MW.UserScale, 100 * MW.UserScale);

                        if (((int)MW.FDrawMode & 62) != 0)
                        {
                            if (normal_vector.Dest.Z >= 0)
                            {
                                AddPointInCanvas(barycenter, Brushes.Red);
                                AddLineInCanvas(barycenter.X, barycenter.X + normal_vector.Dest.X, barycenter.Y, barycenter.Y + normal_vector.Dest.Y, new SolidColorBrush(Color.FromRgb(255, 0, 0)));
                            }
                        }
                        else
                        {
                            AddPointInCanvas(barycenter, Brushes.Red);
                            AddLineInCanvas(barycenter.X, barycenter.X + normal_vector.Dest.X, barycenter.Y, barycenter.Y + normal_vector.Dest.Y, new SolidColorBrush(Color.FromRgb(255, 0, 0)));
                        }
                    }
                }

                // отрисовка нормалей вершин
                if (MW.ViewNormals)
                {
                    for (int i = 0; i < o.Points.Length; i++)
                    {
                        Vector4D normal_vector = Vector4D.Normalize(o.Points[i].Normal * NormalsTransformations) * Matrix4D.ScaleMatrix(100 * MW.UserScale, 100 * MW.UserScale, 100 * MW.UserScale);

                        if (((int)MW.FDrawMode & 62) != 0)
                        {
                            if (normal_vector.Dest.Z >= 0)
                            {
                                normal_vector *= ProjectionMatrix;
                                AddPointInCanvas(objectPoints[i], new SolidColorBrush(Color.FromRgb(37, 161, 234)));
                                AddLineInCanvas(objectPoints[i].X, objectPoints[i].X + normal_vector.Dest.X, objectPoints[i].Y, objectPoints[i].Y + normal_vector.Dest.Y, new SolidColorBrush(Color.FromRgb(37, 161, 234)));
                            }
                        }
                        else
                        {
                            normal_vector *= ProjectionMatrix;
                            AddPointInCanvas(objectPoints[i], new SolidColorBrush(Color.FromRgb(37, 161, 234)));
                            AddLineInCanvas(objectPoints[i].X, objectPoints[i].X + normal_vector.Dest.X, objectPoints[i].Y, objectPoints[i].Y + normal_vector.Dest.Y, new SolidColorBrush(Color.FromRgb(37, 161, 234)));
                        }
                    }
                }
            }
        }
    }
}
