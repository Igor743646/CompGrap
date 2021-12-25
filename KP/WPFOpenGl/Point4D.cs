using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Geometry
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Point4D
    {
        public readonly double X, Y, Z, W;
        public readonly double R, G, B;

        public Point4D(double x, double y, double z, double w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;

            R = 0.5;
            G = 0.5;
            B = 0.5;
        }
        public Point4D(double x, double y, double z) : this(x, y, z, 1.0d) { }
        public Point4D(Point4D p) : this(p.X, p.Y, p.Z, p.W) { }
        public Point4D(Point4D p, int index, double value) : this(p)
        {
            switch(index)
            {
                case 0: 
                    X = value;
                    break;
                case 1: 
                    Y = value;
                    break;
                case 2:
                    Z = value;
                    break;
                case 3:
                    W = value;
                    break;
                default:
                    throw new ArgumentNullException($"В конструктор класса Point4D index должен находится в пределе от 0 до 3");
            }
        }
        public Point4D(double[] array) : this(array[0], array[1], array[2], array[3])
        {
            if (array == null)
            {
                throw new ArgumentNullException($"В конструктор класса Point4D не должен подаваться неинициализированный (null) объект");
            }

            if (array.Length != 4)
            {
                throw new ArgumentException($"В конструктор класса Point4D должен подаваться массив длины 4");
            }
        }
        public static Point4D operator *(Point4D p, double s)
        {
            return new Point4D(p.X * s, p.Y * s, p.Z * s, p.W * s);
        }
        public static Point4D operator +(Point4D p, Point4D s)
        {
            return new Point4D(p.X + s.X, p.Y + s.Y, p.Z + s.Z, p.W + s.W);
        }
        public static Point4D operator *(Point4D p, Point4D s)
        {
            return new Point4D(p.X * s.X, p.Y * s.Y, p.Z * s.Z, p.W * s.W);
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
        }
        public static Vector4D operator -(Point4D A, Point4D B) => new Vector4D(A.X - B.X, A.Y - B.Y, A.Z - B.Z, A.W - B.W);

        public double[] ToDoubleArray() => new double[3] { X, Y, Z };
        public float[] ToFloatArray() => new float[3] { (float)X, (float)Y, (float)Z };
        public override string ToString() => $"({X}, {Y}, {Z}, {W})";
    }
}
