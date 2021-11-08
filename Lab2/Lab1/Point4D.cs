using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geometry
{
    class Point4D
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double W { get; set; }

        public Point4D(double x, double y, double z, double w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
        public Point4D(double x, double y, double z) : this(x, y, z, 1.0d) { }
        public Point4D(Point4D p) : this(p.X, p.Y, p.Z, p.W) { }
        public Point4D(double[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException($"В конструктор класса Point4D не должен подаваться неинициализированный (null) объект");
            }

            if (array.Length != 4)
            {
                throw new ArgumentException($"В конструктор класса Point4D должен подаваться массив длины 4");
            }

            X = array[0];
            Y = array[1];
            Z = array[2];
            W = array[3];
        }

        public void NormalizeW()
        {
            X /= W;
            Y /= W;
            Z /= W;
            W /= W;
        }

        public static Point4D operator *(Point4D p, double s)
        {
            return new Point4D(p.X * s, p.Y * s, p.Z * s, p.W * s);
        }
        public static Point4D operator /(Point4D p, double s)
        {
            return new Point4D(p.X / s, p.Y / s, p.Z / s, p.W / s);
        }
        public static Point4D operator *(Point4D p, Matrix4D m)
        {
            double[] array = new double[4] { 0, 0, 0, 0 };

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    array[i] += p[j] * m[j, i];
                }
            }

            return new Point4D(array);
        }

        public double this[int column]
        {
            get
            {
                if (column < 0 || column > 3)
                {
                    throw new ArgumentOutOfRangeException("Индексы класса Point4D должны быть в диапозоне от 0 до 3");
                }

                double result = double.NaN;

                if (column == 0) result = X;
                if (column == 1) result = Y;
                if (column == 2) result = Z;
                if (column == 3) result = W;

                return result;
            }

            set
            {
                if (column < 0 || column > 3)
                {
                    throw new ArgumentOutOfRangeException("Индексы класса Point4D должны быть в диапозоне от 0 до 3");
                }

                if (column == 0) X = value;
                if (column == 1) Y = value;
                if (column == 2) Z = value;
                if (column == 3) W = value;
            }
        }

        public static Vector4D operator -(Point4D A, Point4D B) => new Vector4D(A.X - B.X, A.Y - B.Y, A.Z - B.Z, A.W - B.W);
        public override string ToString() => $"({X}, {Y}, {Z}, {W})";
    }
}
