using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geometry
{
    class Vector4D
    {
        public Point4D Dest { get; set; }
        public double Length => Math.Sqrt(Dest.X * Dest.X + Dest.Y * Dest.Y + Dest.Z * Dest.Z + Dest.W * Dest.W);
        public Vector4D(double x, double y, double z, double w)
        {
            Dest = new Point4D(x, y, z, w);
        }
        public Vector4D(double x, double y, double z) : this(x, y, z, 0) { }
        public Vector4D(Point4D dest) : this(dest.X, dest.Y, dest.Z, dest.Z) { }
        public Vector4D(double[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException($"В конструктор класса Vector4D не должен подаваться неинициализированный (null) объект");
            }

            if (array.Length != 4)
            {
                throw new ArgumentException($"В конструктор класса Vector4D должен подаваться массив длины 4");
            }

            Dest = new Point4D(array[0], array[1], array[2], array[3]);
        }

        public static Vector4D CrossProduct(Vector4D A, Vector4D B)
        {
            return new Vector4D(
                A.Dest.Y * B.Dest.Z - A.Dest.Z * B.Dest.Y,
                A.Dest.Z * B.Dest.X - A.Dest.X * B.Dest.Z,
                A.Dest.X * B.Dest.Y - A.Dest.Y * B.Dest.X
            );
        }

        public static Vector4D Normalize(Vector4D A)
        {
            if (A.Length == 0)
                return A;
            return new Vector4D(A.Dest.X / A.Length, A.Dest.Y / A.Length, A.Dest.Z / A.Length, A.Dest.W / A.Length);
        }
        public static Vector4D operator /(Vector4D A, double s)
        {
            return new Vector4D(A.Dest.X / s, A.Dest.Y / s, A.Dest.Z / s, A.Dest.W / s);
        }
        public static Vector4D operator *(Vector4D v, Matrix4D m)
        {
            double[] array = new double[4] { 0, 0, 0, 0 };

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    array[i] += v[j] * m[j, i];
                }
            }

            return new Vector4D(array);
        }

        public double this[int column]
        {
            get
            {
                if (column < 0 || column > 3)
                {
                    throw new ArgumentOutOfRangeException("Индексы класса Vector4D должны быть в диапозоне от 0 до 3");
                }

                double result = double.NaN;

                if (column == 0) result = Dest.X;
                if (column == 1) result = Dest.Y;
                if (column == 2) result = Dest.Z;
                if (column == 3) result = Dest.W;

                return result;
            }

            set
            {
                if (column < 0 || column > 3)
                {
                    throw new ArgumentOutOfRangeException("Индексы класса Vector4D должны быть в диапозоне от 0 до 3");
                }

                if (column == 0) Dest.X = value;
                if (column == 1) Dest.Y = value;
                if (column == 2) Dest.Z = value;
                if (column == 3) Dest.W = value;
            }
        }

        public override string ToString() => $"<{Dest.X}, {Dest.Y}, {Dest.Z}, {Dest.W}>";
    }
}
