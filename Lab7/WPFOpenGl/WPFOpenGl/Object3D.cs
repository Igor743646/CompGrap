using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Geometry
{
    public class Object3D
    {
        private Point3D[] _points;
        private uint[] _polygons_indexes;

        public Point3D[] Points
        {
            get => _points;
            set => _points = value;
        }
        public uint[] PolygonsIndexes
        {
            get => _polygons_indexes;
            set => _polygons_indexes = value;
        }

        public Object3D(Point3D[] points, uint[] polygons_indexes)
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
