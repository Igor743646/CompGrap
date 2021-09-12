using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lab1
{
    public partial class MainWindow : Window
    {
        private bool PresedButton { get; set; }
        private Point LastPosition { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            InitializeFields();
        }

        private void InitializeFields()
        {
            PresedButton = false;
            LastPosition = new Point(0, 0);
        }

        private void WriteInDebugTextBlock(string msg)
        {
            DebugTextBlock.Text = msg;
        }

        private void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            double coef_a;
            double dx;

            try
            {
                coef_a = Convert.ToDouble(Coef_A.Text);
                dx = Convert.ToDouble(Dx.Text);

                if (dx <= 0)
                {
                    dx = 0.5;
                    Dx.Text = dx.ToString();
                }

                if (coef_a < 0)
                {
                    coef_a = 200;
                    Coef_A.Text = coef_a.ToString();
                }

            }
            catch (FormatException ee)
            {
                System.Diagnostics.Debug.WriteLine(ee.Message);
                coef_a = 200;
                dx = 0.5;
            }
            catch (OverflowException ee)
            {
                System.Diagnostics.Debug.WriteLine(ee.Message);
                coef_a = 200;
                dx = 0.5;
            }

            Polyline polyline = ReturnLines(ReturnPoints(dx, coef_a));

            Canvas.Children.Clear();

            AddAxises();

            _ = Canvas.Children.Add(polyline);
        }

        private Point[] ReturnPoints(double dx, double coef_a)
        {
            Point[] points = new Point[Convert.ToInt32(2 * Math.PI / dx) + 1];

            // formula: r = a * (1 - cos(alpha))
            double alpha;
            double r;

            for (int i = 0; i < points.Length; i++)
            {
                alpha = i * dx;
                r = coef_a * (1 - Math.Cos(alpha));
                points[i] = new Point(r * Math.Cos(alpha), r * Math.Sin(alpha));
            }

            points[points.Length - 1] = points[0];

            return points;
        }

        private Polyline ReturnLines(Point[] points)
        {
            return new Polyline
            {
                Points = new PointCollection(points),
                Stroke = new SolidColorBrush(Color.FromRgb(228, 239, 240))
            };
        }

        private void FixAxises(Line axis)
        {
            if (axis.Name == "MarkX")
            {
                double midle = axis.X1 + axis.X2;
                axis.X1 = midle / 2 - 5;
                axis.X2 = midle / 2 + 5;
            }
            else if (axis.Name == "MarkY")
            {
                double midle = axis.Y1 + axis.Y2;
                axis.Y1 = midle / 2 - 5;
                axis.Y2 = midle / 2 + 5;
            }
            else if (axis.Name == "AxisX")
            {
                axis.X1 = -ActualWidth / 2;
                axis.X2 = ActualWidth / 2;
            }
            else if (axis.Name == "AxisY")
            {
                axis.Y1 = ActualWidth / 2;
                axis.Y2 = -ActualWidth / 2;
            }
        }

        private void AddAxises()
        {
            _ = Canvas.Children.Add(new Line
            {
                Name = "AxisX",
                Stroke = new SolidColorBrush(Color.FromRgb(228, 239, 240)),
                X1 = -1000,
                Y1 = 0,
                X2 = 1000,
                Y2 = 0,
                StrokeThickness = 1
            });

            _ = Canvas.Children.Add(new Line
            {
                Name = "AxisY",
                Stroke = new SolidColorBrush(Color.FromRgb(228, 239, 240)),
                X1 = 0,
                Y1 = -1000,
                X2 = 0,
                Y2 = 1000,
                StrokeThickness = 1
            });

            _ = Canvas.Children.Add(new Line
            {
                Name = "MarkX",
                Stroke = new SolidColorBrush(Color.FromRgb(228, 239, 240)),
                X1 = -5,
                Y1 = -1,
                X2 = 5,
                Y2 = -1,
                StrokeThickness = 1
            });

            _ = Canvas.Children.Add(new Line
            {
                Name = "MarkY",
                Stroke = new SolidColorBrush(Color.FromRgb(228, 239, 240)),
                X1 = 1,
                Y1 = -5,
                X2 = 1,
                Y2 = 5,
                StrokeThickness = 1
            });
        }

        private void CanvasGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double mouse_X = e.GetPosition(Canvas).X;
            double mouse_Y = e.GetPosition(Canvas).Y;
            double scale = e.Delta > 0 ? 1.1 : 1 / 1.1;

            foreach (object UIelement in Canvas.Children)
            {
                if (UIelement is Line line)
                {
                    line.X1 = (line.X1 - mouse_X) * scale + mouse_X;
                    line.Y1 = (line.Y1 - mouse_Y) * scale + mouse_Y;
                    line.X2 = (line.X2 - mouse_X) * scale + mouse_X;
                    line.Y2 = (line.Y2 - mouse_Y) * scale + mouse_Y;

                    FixAxises(line);
                }
                else if (UIelement is Polyline polyline)
                {
                    for (int i = 0; i < polyline.Points.Count; i++)
                    {
                        Point p = polyline.Points[i];
                        p.X = (p.X - mouse_X) * scale + mouse_X;
                        p.Y = (p.Y - mouse_Y) * scale + mouse_Y;
                        polyline.Points[i] = p;
                    }
                }
                
            }
        }

        private void CanvasGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PresedButton = true;
            LastPosition = new Point(e.GetPosition(Canvas).X, e.GetPosition(Canvas).Y);
        }

        private void CanvasGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PresedButton = false;
        }

        private void CanvasGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (PresedButton)
            {
                Point CurrentPosition = new Point(e.GetPosition(Canvas).X, e.GetPosition(Canvas).Y);

                double divx = LastPosition.X - CurrentPosition.X;
                double divy = LastPosition.Y - CurrentPosition.Y;

                if (divx == 0 && divy == 0) 
                    return;

                foreach (object UIelement in Canvas.Children)
                {
                    if (UIelement is Polyline polyline)
                    {
                        for (int i = 0; i < polyline.Points.Count; i++)
                        {
                            Point p = polyline.Points[i];
                            p.X += -divx;
                            p.Y += -divy;
                            polyline.Points[i] = p;
                        }
                    }
                    else if (UIelement is Line line)
                    {
                        line.X1 += -divx;
                        line.Y1 += -divy;
                        line.X2 += -divx;
                        line.Y2 += -divy;

                        FixAxises(line);
                    }
                }

                LastPosition = new Point(e.GetPosition(Canvas).X, e.GetPosition(Canvas).Y);
            }
        }

        private void CanvasGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (object UIelement in Canvas.Children)
            {
                if (UIelement is Line line)
                {
                    FixAxises(line);
                }
            }
        }
    }
}
