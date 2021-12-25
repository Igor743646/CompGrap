using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geometry
{
    public class Vector4D_p
    {
        public Point4D X, Y, Z, W;

        public Vector4D_p(Point4D x, Point4D y, Point4D z, Point4D w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
        public Vector4D_p(Point4D x, Point4D y, Point4D z) : this(x, y, z, new Point4D(0, 0, 0, 0)) { }
        public Vector4D_p(Point4D[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException($"В конструктор класса Vector4D не должен подаваться неинициализированный (null) объект");
            }

            if (array.Length != 4)
            {
                throw new ArgumentException($"В конструктор класса Vector4D должен подаваться массив длины 4");
            }

            X = array[0];
            Y = array[1];
            Z = array[2];
            W = array[3];
        }

        public static Point4D operator *(Vector4D_p v, Vector4D m)
        {
            Point4D p = new Point4D();

            for (int i = 0; i < 4; i++)
            {
                p += v[i] * m[i];
            }

            return p;
        }

        public Point4D this[int column]
        {
            get
            {
                switch (column)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    case 2:
                        return Z;
                    case 3:
                        return W;
                    default:
                        throw new ArgumentOutOfRangeException("Индексы класса Vector4D должны быть в диапозоне от 0 до 3");
                }
            }
            set
            {
                switch (column)
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
                        throw new ArgumentOutOfRangeException("Индексы класса Vector4D должны быть в диапозоне от 0 до 3");
                }
            }
        }

        public override string ToString() => $"<{X}, {Y}, {Z}, {W}>";
    }
}
