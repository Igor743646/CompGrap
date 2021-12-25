using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geometry
{
    public class Vector3D
    {
        public Point3D Dest;
        public double Length => Math.Sqrt(Dest.X * Dest.X + Dest.Y * Dest.Y);

        public Vector3D(double x, double y, double w)
        {
            Dest = new Point3D(x, y, w);
        }
        public Vector3D(double x, double y) : this(x, y, 0) { }
        public Vector3D(Point3D dest) : this(dest.X, dest.Y, dest.W) { }
        public Vector3D(double[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException($"В конструктор класса Vector3D не должен подаваться неинициализированный (null) объект");
            }

            if (array.Length != 3)
            {
                throw new ArgumentException($"В конструктор класса Vector3D должен подаваться массив длины 3");
            }

            Dest = new Point3D(array[0], array[1], array[2]);
        }

        public static double ScalarProduct(Vector3D A, Vector3D B)
        {
            double result = 0;

            for (int i = 0; i < 3; i++)
            {
                result += A[i] * B[i];
            }

            return result;
        }
        public static Vector3D Normalize(Vector3D A)
        {
            if (A.Length == 0)
                return A;
            return new Vector3D(A.Dest.X / A.Length, A.Dest.Y / A.Length, 0);
        }

        public static Vector3D operator /(Vector3D A, double s)
        {
            return new Vector3D(A.Dest.X / s, A.Dest.Y / s, A.Dest.W / s);
        }
        public static Vector3D operator *(Vector3D A, double s)
        {
            return new Vector3D(A.Dest.X * s, A.Dest.Y * s, A.Dest.W * s);
        }
        public static Vector3D operator *(Vector3D v, Matrix3D m)
        {
            double[] array = new double[3] { 0, 0, 0 };

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    array[i] += v[j] * m[j, i];
                }
            }

            return new Vector3D(array);
        }
        public static Vector3D operator *(Vector3D v, Vector3D m)
        {
            double[] array = new double[3] { 0, 0, 0 };

            for (int i = 0; i < 3; i++)
            {
                array[i] = v[i] * m[i];
            }

            return new Vector3D(array);
        }
        public static Vector3D operator +(Vector3D v, Vector3D m)
        {
            double[] array = new double[3] { 0, 0, 0 };

            for (int i = 0; i < 3; i++)
            {
                array[i] = v[i] + m[i];
            }

            return new Vector3D(array);
        }
        public static Vector3D operator -(Vector3D v, Vector3D m)
        {
            double[] array = new double[3] { 0, 0, 0 };

            for (int i = 0; i < 3; i++)
            {
                array[i] = v[i] - m[i];
            }

            return new Vector3D(array);
        }
        public double this[int column]
        {
            get
            {
                switch (column)
                {
                    case 0:
                        return Dest.X;
                    case 1:
                        return Dest.Y;
                    case 2:
                        return Dest.W;
                    default:
                        throw new ArgumentOutOfRangeException("Индексы класса Vector3D должны быть в диапозоне от 0 до 2");
                }
            }
        }

        public override string ToString() => $"<{Dest.X}, {Dest.Y}, {Dest.W}>";
    }
}
