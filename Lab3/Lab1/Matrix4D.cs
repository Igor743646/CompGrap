using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geometry
{
    public class Matrix4D
    {
        public double[,] matrix;
        public int Dim => 4;

        public Matrix4D()
        {
            matrix = new double[4, 4];

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    matrix[i, j] = 0;
                }
            }
        }
        public Matrix4D(double[,] input)
        {
            if (input.GetLength(0) != 4 || input.GetLength(1) != 4)
            {
                throw new ArgumentException($"В конструктор класса Matrix4D должен подаваться массив 4*4");
            }

            if (input == null)
            {
                throw new ArgumentNullException($"В конструктор класса Matrix4D не должен подаваться неинициализированный (null) объект");
            }

            matrix = new double[4, 4];

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

        public static Matrix4D operator +(Matrix4D m1, Matrix4D m2)
        {
            Matrix4D result = new Matrix4D();

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    result[i, j] += m1[i, j] + m2[i, j];
                }
            }

            return result;
        }

        public static Matrix4D operator *(Matrix4D m1, Matrix4D m2)
        {
            Matrix4D result = new Matrix4D();

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

        public static Matrix4D ProjectionMatrixX()
        {
            return new Matrix4D( 
                new double[,] {
                { 0, 0, 1, 0 },
                { 0, -1, 0, 0 },
                { 1, 0, 0, 0 },
                { 0, 0, 0, 1 },
            });
        }
        public static Matrix4D ProjectionMatrixY()
        {
            return new Matrix4D(
                new double[,] {
                { 1, 0, 0, 0 },
                { 0, 0, 1, 0 },
                { 0, -1, 0, 0 },
                { 0, 0, 0, 1 },
            });
        }
        public static Matrix4D ProjectionMatrixZ()
        {
            return new Matrix4D(
                new double[,] {
                { 1, 0, 0, 0 },
                { 0, -1, 0, 0 },
                { 0, 0, 1, 0 },
                { 0, 0, 0, 1 },
            });
        }

        public static Matrix4D ScaleMatrix(double S_x, double S_y, double S_z)
        {
            return new Matrix4D(
                new double[,] {
                    { S_x, 0, 0, 0 },
                    { 0, S_y, 0, 0 },
                    { 0, 0, S_z, 0 },
                    { 0,  0,  0,  1},
                }
            );
        }

        public static Matrix4D RotateMatrixX(double alpha)
        {
            alpha = alpha / 180 * Math.PI;
            return new Matrix4D(
                new double[,] {
                    { 1, 0, 0, 0 },
                    { 0, Math.Cos(alpha), -Math.Sin(alpha), 0 },
                    { 0, Math.Sin(alpha), Math.Cos(alpha), 0 },
                    { 0, 0, 0, 1},
                }
            );
        }

        public static Matrix4D RotateMatrixY(double alpha)
        {
            alpha = alpha / 180 * Math.PI;
            return new Matrix4D(
                new double[,] {
                    { Math.Cos(alpha), 0, Math.Sin(alpha), 0 },
                    { 0, 1, 0, 0 },
                    { -Math.Sin(alpha), 0, Math.Cos(alpha), 0 },
                    { 0, 0, 0, 1},
                }
            );
        }

        public static Matrix4D RotateMatrixZ(double alpha)
        {
            alpha = alpha / 180 * Math.PI;
            return new Matrix4D(
                new double[,] {
                    { Math.Cos(alpha), -Math.Sin(alpha), 0, 0 },
                    { Math.Sin(alpha), Math.Cos(alpha), 0, 0 },
                    { 0, 0, 1, 0 },
                    { 0, 0, 0, 1},
                }
            );
        }

        public static Matrix4D RotateMatrix(double alpha, double betha, double gamma)
        {
            return RotateMatrixX(alpha) * RotateMatrixY(betha) * RotateMatrixZ(gamma);
        }

        public static Matrix4D TranslationMatrix(double dx, double dy, double dz)
        {
            return new Matrix4D(
                new double[,]
                {
                    { 1, 0, 0, 0 },
                    { 0, 1, 0, 0 },
                    { 0, 0, 1, 0 },
                    { dx, dy, dz, 1},
                }    
            );
        }

        public static Matrix4D ShiftMatrix(double b, double c, double d, double f, double h, double i)
        {
            return new Matrix4D(
                new double[,]
                {
                    { 1, b, c, 0 },
                    { d, 1, f, 0 },
                    { h, i, 1, 0 },
                    { 0, 0, 0, 1},
                }
            );
        }
    }
}
