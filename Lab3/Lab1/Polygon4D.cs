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

namespace Geometry
{
    public class Polygon4D : IComparable
    {
        public Point4D[] points;
        public int[] pointsIndexes;
        private Vector4D _normal = null;
        private Point4D _barycenter = null;
        public Vector4D Normal 
        { 
            get {
                if (_normal == null)
                {
                    if (pointsIndexes.Length <= 2)
                    {
                        _normal = new Vector4D(0, 0, 0, 0);
                    }
                    else
                    {
                        _normal = Vector4D.Normalize(Vector4D.CrossProduct(points[pointsIndexes[0]] - points[pointsIndexes[1]], points[pointsIndexes[2]] - points[pointsIndexes[1]]));
                    }
                    return _normal;
                }
                else
                {
                    return _normal;
                }
            }
        }
        public Point4D Barycenter
        {
            get
            {
                if (_barycenter == null)
                {
                    _barycenter = new Point4D(0, 0, 0, 0);

                    for (int i = 0; i < pointsIndexes.Length; i++)
                    {
                        _barycenter.X += points[pointsIndexes[i]].X;
                        _barycenter.Y += points[pointsIndexes[i]].Y;
                        _barycenter.Z += points[pointsIndexes[i]].Z;
                        _barycenter.W += points[pointsIndexes[i]].W;
                    }

                    _barycenter /= pointsIndexes.Length;

                    return _barycenter;
                }
                else
                {
                    return _barycenter;
                }
                
            }
        }
        public SolidColorBrush Brush { get; set; }

        public Polygon4D(Point4D[] p, int[] pi)
        {
            points = p;
            pointsIndexes = pi;
        }
        public Polygon4D(Point4D[] p, int[] pi, SolidColorBrush brush) : this(p, pi)
        {
            Brush = brush;
        }
        public Polygon4D(Point4D[] p, int[] pi, byte r, byte g, byte b) : this(p, pi, new SolidColorBrush(Color.FromRgb(r, g, b))) { }

        public int CompareTo(object o)
        {
            if (o == null)
            {
                return 1;
            }

            if (o is Polygon4D O)
            {
                double min1 = double.MaxValue;
                double min2 = double.MaxValue;

                for (int i = 0; i < pointsIndexes.Length; i++)
                {
                    double z = points[pointsIndexes[i]].Z;

                    if (z < min1)
                    {
                        min1 = z;
                    }
                }

                for (int i = 0; i < O.pointsIndexes.Length; i++)
                {
                    double z = O.points[O.pointsIndexes[i]].Z;

                    if (z < min2)
                    {
                        min2 = z;
                    }
                }

                return min1 < min2 ? -1 : min1 == min2 ? 0 : 1;
            }
            else
            {
                throw new ArgumentException("Object is not a Polygon4D");
            }
        }
        public override string ToString()
        {
            string output = "{";

            for (int i = 0; i < pointsIndexes.Length; i++)
            {
                output += $"{pointsIndexes[i]}";

                if (i != pointsIndexes.Length - 1)
                {
                    output += ", ";
                }
            }

            /*for (int i = 0; i < pointsIndexes.Length; i++)
            {
                output += $"{points[pointsIndexes[i]]}";

                if (i != pointsIndexes.Length - 1)
                {
                    output += ", ";
                }
            }*/

            return output + "}";
        }
    }
}
