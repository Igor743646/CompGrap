using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geometry
{
    public class Matrix4D_p
    {
        public Point4D[,] matrix;
        public int Dim => 4;

        public Matrix4D_p()
        {
            matrix = new Point4D[4, 4];

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    matrix[i, j] = default;
                }
            }
        }
        public Matrix4D_p(Point4D[,] input)
        {
            if (input.GetLength(0) != 4 || input.GetLength(1) != 4)
            {
                throw new ArgumentException($"В конструктор класса Matrix4D должен подаваться массив 4*4");
            }

            if (input == null)
            {
                throw new ArgumentNullException($"В конструктор класса Matrix4D не должен подаваться неинициализированный (null) объект");
            }

            matrix = new Point4D[4, 4];

            for (int i = 0; i < input.GetLength(0); i++)
            {
                for (int j = 0; j < input.GetLength(1); j++)
                {
                    matrix[i, j] = input[i, j];
                }
            }
        }

        public static Matrix4D_p Transpose(Matrix4D_p m)
        {
            Matrix4D_p result = new Matrix4D_p();

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    result[j, i] = m[i, j];
                }
            }

            return result;
        } 

        public Point4D this[int row, int column]
        {
            get
            {
                if (row < 0 || row > 3 || column < 0 || column > 3)
                {
                    throw new ArgumentOutOfRangeException("Индексы класса Matrix4D должны быть в диапозоне от 0 до 3");
                }

                return matrix[row, column];
            }

            set
            {
                if (row < 0 || row > 3 || column < 0 || column > 3)
                {
                    throw new ArgumentOutOfRangeException("Индексы класса Matrix4D должны быть в диапозоне от 0 до 3");
                }

                matrix[row, column] = value;
            }
        }
        public static Matrix4D_p operator +(Matrix4D_p m1, Matrix4D_p m2)
        {
            Matrix4D_p result = new Matrix4D_p();

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    result[i, j] = m1[i, j] + m2[i, j];
                }
            }

            return result;
        }
        public static Matrix4D_p operator *(Matrix4D_p m1, Matrix4D_p m2)
        {
            Matrix4D_p result = new Matrix4D_p();

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        result[i, j] += m1[i, k] * m2[k, j];
                    }
                }
            }

            return result;
        }
        public static Vector4D_p operator *(Vector4D m1, Matrix4D_p m2)
        {
            Vector4D_p result = new Vector4D_p(new Point4D(), new Point4D(), new Point4D(), new Point4D());

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    result[i] += m2[j, i] * m1[j];
                }
            }

            return result;
        }
        public static Matrix4D_p operator *(Matrix4D_p m1, Matrix4D m2)
        {
            Matrix4D_p result = new Matrix4D_p();

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        result[i, j] += m1[i, k] * m2[k, j];
                    }
                }
            }

            return result;
        }
        public static Matrix4D_p operator *(Matrix4D m1, Matrix4D_p m2)
        {
            Matrix4D_p result = new Matrix4D_p();

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        result[i, j] +=  m2[k, j] * m1[i, k];
                    }
                }
            }

            return result;
        }
        public override string ToString()
        {
            string output = "[";

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    output += matrix[i, j].ToString();

                    if (j != matrix.GetLength(1) - 1)
                    {
                        output += ", ";
                    }
                }

                if (i != matrix.GetLength(0) - 1)
                {
                    output += "\n";
                }
            }

            return output + "]";
        }
    }
}
