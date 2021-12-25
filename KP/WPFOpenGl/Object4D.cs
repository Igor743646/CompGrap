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
    public class Object4D
    {
        private Point4D[] _points;
        private uint[] _polygons_indexes;

        public Point4D[] Points
        {
            get => _points;
            set => _points = value;
        }
        public uint[] PolygonsIndexes
        {
            get => _polygons_indexes;
            set => _polygons_indexes = value;
        }

        public Object4D(Point4D[] points, uint[] polygons_indexes)
        {
            _points = points;
            _polygons_indexes = polygons_indexes;
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

            for (int i = 0; i < _polygons_indexes.Length; i++)
            {
                output += _polygons_indexes[i].ToString();

                if (i != _polygons_indexes.Length - 1)
                {
                    output += "\n";
                }
            }

            return output;
        }
    }
}
