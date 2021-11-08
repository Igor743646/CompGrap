using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Threading;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using Geometry;

namespace Geometry
{
    class Object4D
    {
        private Point4D[] _points;
        private Polygon4D[] _polygons;
        public SolidColorBrush Color { get; set; }

        public Point4D[] Points
        {
            get => _points;
            set => _points = value;
        }

        public Polygon4D[] Polygons
        {
            get => _polygons;
            set => _polygons = value;
        }

        public Object4D(Point4D[] points, Polygon4D[] polygons)
        {
            _points = points;
            _polygons = polygons;
            Color = new SolidColorBrush(System.Windows.Media.Color.FromRgb(228, 239, 240));

            Random ColorRandomer = new Random(DateTime.Now.Millisecond);
            foreach (Polygon4D p in _polygons)
            {
                p.Brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb((byte)ColorRandomer.Next(0, 256), (byte)ColorRandomer.Next(0, 256), (byte)ColorRandomer.Next(0, 256)));
            }
        }

        public override string ToString()
        {
            string output = "";

            output += "Dots: ";

            for (int i = 0; i < _points.Length; i++)
            {
                output += _points[i].ToString();
                if (i < _points.Length - 1)
                    output += ", ";
            }

            output += "\nPolygones: ";

            for (int i = 0; i < _polygons.Length; i++)
            {
                output += _polygons[i];

                if (i != _polygons.Length - 1)
                {
                    output += "\n";
                }
            }

            return output;
        }
    }
}
