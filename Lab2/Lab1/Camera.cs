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
        Polyline = 0,
        PolylineVisible = 1,
        Polygon = 2,
        PolygonRandom = 3,
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

        public void AddPolyLineInCanvas(Polyline polyline)
        {
            _ = MW.Canvas.Children.Add(polyline);
        }

        public void AddPolyGonInCanvas(Polygon polygon)
        {
            _ = MW.Canvas.Children.Add(polygon);
        }

        public void AddPolyGonInCanvas(Polyline polyline)
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
                Fill = new SolidColorBrush(Color.FromRgb(112, 118, 224)),
            };

            polygon.MouseLeftButtonUp += MW.CanvasGrid_MouseLeftButtonUp;
            polygon.MouseRightButtonUp += MW.CanvasGrid_MouseRightButtonUp;

            AddPolyGonInCanvas(polygon);
        }

        public void AddPolyGonInCanvas(Polyline polyline, SolidColorBrush randcolor)
        {
            Polygon polygon = new Polygon()
            {
                Points = polyline.Points,
                Stroke = polyline.Stroke,
                Fill = randcolor,
            };

            polygon.MouseLeftButtonUp += MW.CanvasGrid_MouseLeftButtonUp;
            polygon.MouseRightButtonUp += MW.CanvasGrid_MouseRightButtonUp;

            AddPolyGonInCanvas(polygon);
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

        public void NewPoint(Point4D[] no, Point4D[] o, int i, Matrix4D AllTransformations)
        {
            no[i] = o[i] * AllTransformations;
        }

        public void Project(List<Object4D> objects, EProjectionMode FProjectionMode)
        {
            if (objects == null)
            {
                return;
            }

            MW.Canvas.Children.Clear();

            double WholeScaleXValue = MW.ScaleXValue * MW.UserScale * MW.WindowScale;
            double WholeScaleYValue = MW.ScaleYValue * MW.UserScale * MW.WindowScale;
            double WholeScaleZValue = MW.ScaleZValue * MW.UserScale * MW.WindowScale;

            Matrix4D AllTransformations = Matrix4D.ScaleMatrix(WholeScaleXValue, WholeScaleYValue, WholeScaleZValue)
                * Matrix4D.RotateMatrix(MW.RotateXAngle, MW.RotateYAngle, MW.RotateZAngle);

            Matrix4D NormalsTransformations = Matrix4D.ScaleMatrix(WholeScaleYValue * WholeScaleZValue, WholeScaleXValue * WholeScaleZValue, WholeScaleXValue * WholeScaleYValue)
                * Matrix4D.RotateMatrix(MW.RotateXAngle, MW.RotateYAngle, MW.RotateZAngle);

            if (FProjectionMode == EProjectionMode.Isometry || FProjectionMode == EProjectionMode.Free || FProjectionMode == EProjectionMode.FrontView)
            {
                AllTransformations *= Matrix4D.ProjectionMatrixZ();
                NormalsTransformations *= Matrix4D.ProjectionMatrixZ();
            }
            else if (FProjectionMode == EProjectionMode.AboveView)
            {
                AllTransformations *= Matrix4D.ProjectionMatrixY();
                NormalsTransformations *= Matrix4D.ProjectionMatrixY();
            }
            else if (FProjectionMode == EProjectionMode.SideView)
            {
                AllTransformations *= Matrix4D.ProjectionMatrixX();
                NormalsTransformations *= Matrix4D.ProjectionMatrixX();
            }

            AllTransformations *= Matrix4D.TranslationMatrix(MW.TranslationXValue, MW.TranslationYValue, MW.TranslationZValue);
            NormalsTransformations *= Matrix4D.TranslationMatrix(MW.TranslationXValue, MW.TranslationYValue, MW.TranslationZValue);

            MW.WriteInDebugTextBlock(AllTransformations);

            foreach (Object4D o in objects)
            {
                Point4D[] objectPoints = new Point4D[o.Points.Length];

                _ = Parallel.For(0, o.Points.Length,
                    (i, state) =>
                    NewPoint(objectPoints, o.Points, i, AllTransformations)
                );

                Array.Sort(o.Polygons.Select(x => {
                    double sum_z = 0;

                    for (int i = 0; i < x.pointsIndexes.Length; i++)
                    {
                        sum_z += objectPoints[x.pointsIndexes[i]].Z;
                    }

                    return sum_z / x.pointsIndexes.Length;
                }).ToArray(), o.Polygons);

                foreach (Polygon4D poly in o.Polygons)
                {
                    Polyline polyline = new Polyline() { Stroke = o.Color };

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
                        //AddPolyGonInCanvas(polyline);

                        Vector4D normal_vector = poly.Normal * NormalsTransformations;

                        if (normal_vector.Dest.Z >= 0)
                        {
                            AddPolyGonInCanvas(polyline);
                        }
                    }
                    else if (MW.FDrawMode == EDrawMode.PolygonRandom)
                    {
                        //AddPolyGonInCanvas(polyline, poly.Brush);

                        Vector4D normal_vector = poly.Normal * NormalsTransformations;

                        if (normal_vector.Dest.Z >= 0)
                        {
                            AddPolyGonInCanvas(polyline, poly.Brush);
                        }
                    }
                    else if (MW.FDrawMode == EDrawMode.PolylineVisible)
                    {
                        Vector4D normal_vector = poly.Normal * NormalsTransformations;

                        if (normal_vector.Dest.Z >= 0)
                        {
                            AddPolyLineInCanvas(polyline);
                        }
                    }


                    if (MW.ViewNormals)
                    {
                        Point4D barycenter = poly.Barycenter * AllTransformations;
                        Vector4D normal_vector = Vector4D.Normalize(poly.Normal * NormalsTransformations) * Matrix4D.ScaleMatrix(100 * MW.UserScale, 100 * MW.UserScale, 100 * MW.UserScale);

                        if (MW.FDrawMode == EDrawMode.PolylineVisible)
                        {
                            if (normal_vector.Dest.Z >= 0)
                            {
                                AddPointInCanvas(barycenter.X, barycenter.Y, Brushes.Red);
                                AddLineInCanvas(barycenter.X, barycenter.X + normal_vector.Dest.X, barycenter.Y, barycenter.Y + normal_vector.Dest.Y, new SolidColorBrush(Color.FromRgb(255, 0, 0)));
                            }
                        } 
                        else
                        {
                            AddPointInCanvas(barycenter.X, barycenter.Y, Brushes.Red);
                            AddLineInCanvas(barycenter.X, barycenter.X + normal_vector.Dest.X, barycenter.Y, barycenter.Y + normal_vector.Dest.Y, new SolidColorBrush(Color.FromRgb(255, 0, 0)));
                        }
                    }
                }
            }
        }
    }
}
