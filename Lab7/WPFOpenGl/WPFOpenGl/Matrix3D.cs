using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geometry
{
    public class Matrix3D
    {
        public double[,] matrix;
        public int Dim => 3;

        public Matrix3D()
        {
            matrix = new double[3, 3];

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    matrix[i, j] = 0;
                }
            }
        }
        public Matrix3D(double[,] input)
        {
            if (input.GetLength(0) != 3 || input.GetLength(1) != 3)
            {
                throw new ArgumentException($"В конструктор класса Matrix3D должен подаваться массив 3*3");
            }

            if (input == null)
            {
                throw new ArgumentNullException($"В конструктор класса Matrix3D не должен подаваться неинициализированный (null) объект");
            }

            matrix = new double[3, 3];

            for (int i = 0; i < input.GetLength(0); i++)
            {
                for (int j = 0; j < input.GetLength(1); j++)
                {
                    matrix[i, j] = input[i, j];
                }
            }
        }

        public double this[int row, int column]
        {
            get
            {
                if (row < 0 || row > 2 || column < 0 || column > 2)
                {
                    throw new ArgumentOutOfRangeException("Индексы класса Matrix3D должны быть в диапозоне от 0 до 2");
                }

                return matrix[row, column];
            }

            set
            {
                if (row < 0 || row > 2 || column < 0 || column > 2)
                {
                    throw new ArgumentOutOfRangeException("Индексы класса Matrix3D должны быть в диапозоне от 0 до 2");
                }

                matrix[row, column] = value;
            }
        }

        public static Matrix3D operator +(Matrix3D m1, Matrix3D m2)
        {
            Matrix3D result = new Matrix3D();

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    result[i, j] = m1[i, j] + m2[i, j];
                }
            }

            return result;
        }

        public static Matrix3D operator *(Matrix3D m1, Matrix3D m2)
        {
            Matrix3D result = new Matrix3D();

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        result[i, j] += m1[i, k] * m2[k, j];
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
                    output += matrix[i, j].ToString("F2");

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

        public double[] ToArray()
        {
            double[] result = new double[9];

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    result[i * 3 + j] = matrix[i, j];
                }
            }

            return result;
        }
        public float[] ToFloatArray()
        {
            float[] result = new float[9];

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    result[i * 3 + j] = (float)matrix[i, j];
                }
            }

            return result;
        }

        #region Типовые матрицы
        public static Matrix3D Zero()
        {
            return new Matrix3D(
                new double[,] {
                { 0, 0, 0 },
                { 0, 0, 0 },
                { 0, 0, 0 },
            });
        }

        /// <summary>
        /// a b 0 <br/>
        /// c d 0 <br/>
        /// 0 0 1
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static Matrix3D ProjectionMatrix(double a, double b, double c, double d)
        {
            return new Matrix3D(
                new double[,] {
                { a, b, 0 },
                { c, d, 0 },
                { 0, 0, 1 },
            });
        }

        public static Matrix3D ScaleMatrix(double S_x, double S_y)
        {
            return new Matrix3D(
                new double[,] {
                    { S_x, 0, 0 },
                    { 0, S_y, 0 },
                    { 0,  0,  1 },
                }
            );
        }

        public static Matrix3D RotateMatrix(double alpha)
        {
            alpha = alpha / 180 * Math.PI;
            return new Matrix3D(
                new double[,] {
                    { Math.Cos(alpha), -Math.Sin(alpha), 0 },
                    { Math.Sin(alpha), Math.Cos(alpha), 0 },
                    { 0, 0, 1},
                }
            );
        }

        public static Matrix3D TranslationMatrix(double dx, double dy)
        {
            return new Matrix3D(
                new double[,]
                {
                    { 1,  0,  0 },
                    { 0,  1,  0 },
                    { dx, dy, 1 },
                }
            );
        }
        public static Matrix3D ShiftMatrix(double a, double b)
        {
            return new Matrix3D(
                new double[,]
                {
                    { 1, a, 0 },
                    { b, 1, 0 },
                    { 0, 0, 1},
                }
            );
        }
        #endregion
    }
}
