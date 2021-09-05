using System;
using System.Collections.Generic;
using System.Text;

namespace ACGLab.Transformation
{
    public class MatrixMath
    {
        public static double[,] Multiplication(double[,] a, double[,] b)
        {
            if (a.GetLength(1) != b.GetLength(0)) throw new Exception("Матрицы нельзя перемножить");
            double[,] r = new double[a.GetLength(0), b.GetLength(1)];
            for (int i = 0; i < a.GetLength(0); i++)
            {
                for (int j = 0; j < b.GetLength(1); j++)
                {
                    for (int k = 0; k < b.GetLength(0); k++)
                    {
                        r[i, j] += a[i, k] * b[k, j];
                    }
                }
            }
            return r;
        }

        public static double[] Multiplication(double[,] a, double[] b)
        {
            if (a.GetLength(1) != b.GetLength(0)) throw new Exception("Матрицы нельзя перемножить");
            double[] r = new double[b.GetLength(0)];
            for (int i = 0; i < a.GetLength(0); i++)
            {
                for (int k = 0; k < b.GetLength(0); k++)
                {
                    r[i] += a[i, k] * b[k];
                }
            }
            return r;
        }
    }
}
