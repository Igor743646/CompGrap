using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geometry
{
    public struct Point3D
    {
        public double X, Y, W;

        public Point3D(double x, double y, double w)
        {
            X = x;
            Y = y;
            W = w;
        }

        public Point3D(double x, double y) : this(x, y, 1.0d) { }
        public Point3D(Point3D p) : this(p.X, p.Y, p.W) { }
        public Point3D(double[] array) : this(array[0], array[1], array[2])
        {
            if (array == null)
            {
                throw new ArgumentNullException($"В конструктор класса Point3D не должен подаваться неинициализированный (null) объект");
            }

            if (array.Length != 3)
            {
                throw new ArgumentException($"В конструктор класса Point3D должен подаваться массив длины 3");
            }
        }

        public static Point3D operator *(Point3D p, double s)
        {
            return new Point3D(p.X * s, p.Y * s, p.W * s);
        }
        public static Point3D operator /(Point3D p, double s)
        {
            return new Point3D(p.X / s, p.Y / s, p.W / s);
        }
        public static Point3D operator *(Point3D p, Matrix3D m)
        {
            double[] array = new double[3] { 0, 0, 0 };

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    array[i] += p[j] * m[j, i];
                }
            }

            return new Point3D(array);
        }

        public static Point3D operator *(Point3D p, Matrix4D m)
        {
            double[] result = new double[3] { 0, 0, 0 };

            for (int i = 0; i < 3; i++)
            {
                int d = i;
                if (i == 2) d = i + 1;

                for (int j = 0; j < 3; j++)
                {
                    int k = j;
                    if (j == 2) k = j + 1;

                    result[j] += m[d, k] * p[i];
                }
            }
            
            return new Point3D(result);
        }

        public double this[int column]
        {
            get
            {
                if (column < 0 || column > 2)
                {
                    throw new ArgumentOutOfRangeException("Индексы класса Point3D должны быть в диапозоне от 0 до 2");
                }

                double result = double.NaN;

                if (column == 0) result = X;
                if (column == 1) result = Y;
                if (column == 2) result = W;

                return result;
            }
        }
        public static Vector3D operator -(Point3D A, Point3D B) => new Vector3D(A.X - B.X, A.Y - B.Y, A.W - B.W);
        public override string ToString() => $"({X}, {Y}, {W})";
        public double[] ToDoubleArray() => new double[3] { X, Y, W };
        public float[] ToFloatArray() => new float[3] { (float)X, (float)Y, (float)W };
        public float[] ToFloat2Array() => new float[2] { (float)X, (float)Y };
    }
}
