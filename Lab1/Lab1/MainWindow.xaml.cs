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

namespace MyFigure 
{
    public class MyPoint
    {
        public double x, y;

        public MyPoint() { x = 0; y = 0; }

        public MyPoint(double _x, double _y)
        {
            x = _x;
            y = _y;
        }
    }
}

namespace Lab1
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            InitializeComponent();
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

            Line[] lines = ReturnLines(ReturnPoints(dx, coef_a));

            CanvasGrid.Children.Clear();

            for (int i = 0; i < lines.Length; i++)
            {
                _ = CanvasGrid.Children.Add(lines[i]);
            }
        }

        public MyFigure.MyPoint[] ReturnPoints(double dx, double coef_a)
        {
            MyFigure.MyPoint[] result = new MyFigure.MyPoint[Convert.ToInt32(2 * Math.PI / dx)];

            double alpha = 0;
            for (int i = 0; i < result.Length; i++)
            {
                double r = coef_a * (1 - Math.Cos(alpha));

                result[i] = new MyFigure.MyPoint(r * Math.Cos(alpha), r * Math.Sin(alpha));

                alpha += dx;
            }

            return result;
        }

        public Line[] ReturnLines(MyFigure.MyPoint[] points)
        {
            Line[] lines = new Line[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                lines[i] = new Line
                {
                    Stroke = Brushes.Black,
                    X1 = points[i].x,
                    Y1 = -points[i].y,
                    X2 = points[(i + 1) % points.Length].x,
                    Y2 = -points[(i + 1) % points.Length].y,
                    StrokeThickness = 1
                };
            }

            return lines;
        }

    }
}
