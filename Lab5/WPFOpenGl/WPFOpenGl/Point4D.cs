using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Geometry
{
    //[StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Point4D
    {
        public double X, Y, Z, W;
        public double nX, nY, nZ;
        public double R, G, B;

        public Point4D(double x, double y, double z, double w,
                       double nx, double ny, double nz,
                       double r, double g, double b)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;

            nX = nx;
            nY = ny;
            nZ = nz;

            R = r;
            G = g;
            B = b;
        }

        public Point4D(double x, double y, double z, double w,
                       double nx, double ny, double nz)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;

            nX = nx;
            nY = ny;
            nZ = nz;

            Random rand = new Random((int)(x * 1000) + (int)(y * 1000) + (int)(z * 1000) + (int)w);
            R = rand.NextDouble();
            G = rand.NextDouble();
            B = rand.NextDouble();
        }
        public Point4D(double x, double y, double z,
                       double nx, double ny, double nz) : this(x, y, z, 1.0d, nx, ny, nz) { }
        public Point4D(Point4D p) : this(p.X, p.Y, p.Z, p.W, p.nX, p.nY, p.nZ) { }
        public Point4D(double[] array, double nx, double ny, double nz) : this(array[0], array[1], array[2], array[3], nx, ny, nz)
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
            return new Point4D(p.X * s, p.Y * s, p.Z * s, p.W * s, p.nX, p.nY, p.nZ);
        }
        public static Point4D operator /(Point4D p, double s)
        {
            return new Point4D(p.X / s, p.Y / s, p.Z / s, p.W / s, p.nX, p.nY, p.nZ);
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

            return new Point4D(array, p.nX, p.nY, p.nZ);
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
        public override string ToString() => $"({X}, {Y}, {Z}, {W})";
        public double[] ToDoubleArray() => new double[4] { X, Y, Z, W };
        public float[] ToFloatArray() => new float[4] { (float)X, (float)Y, (float)Z, (float)W };
        public float[] ToFloat3Array() => new float[3] { (float)X, (float)Y, (float)Z };
    }
}
