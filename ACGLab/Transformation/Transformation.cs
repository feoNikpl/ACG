using System;
using System.Collections.Generic;
using System.Text;

namespace ACGLab.Transformation
{
    public class Transformation
    {
        public static double[,] GetTransformationMatrix(float zoom, int x, int y, int z, float ox, float oy, float oz)
        {
            return MatrixMath.Multiplication(
                MatrixMath.Multiplication(GetZoomMatrix(zoom), GetTranslationMatrix(x, y, z)),
                GetTurnMatrix(ox, oy, oz));
        }
        
        private static double[,] GetTurnMatrix(float ox, float oy, float oz)
        {
            return MatrixMath.Multiplication
                (GetTurnOXMatrix(ox), MatrixMath.Multiplication(GetTurnOYMatrix(oy),GetTurnOZMatrix(oz)));
        }
        /*
                Vector3 Eye = new Vector3(0.0f, 0.0f, 3.0f);
                Vector3 Target = new Vector3(0.0f, 0.0f, 0.0f);
                Vector3 Up = new Vector3(0.0f, 1.0f, 0.0f);
                Vector3 Zaxis = Vector3.Normalize(Eye - Target);
                Vector3 Xaxis = Vector3.Multiply(Up, Zaxis);
                Vector3 Yaxis = Up;
                double[,] View = { { Xaxis.X, Xaxis.Y, Xaxis.Z, -Vector3.Dot(Xaxis,Eye) },
                { Yaxis.X, Yaxis.Y, Yaxis.Z, -Vector3.Dot(Yaxis,Eye) },
                { Zaxis.X, Zaxis.Y, Zaxis.Z, -Vector3.Dot(Zaxis,Eye) },
                { 0, 0, 0, 1 },};
         */
        private static double[,] GetZoomMatrix(float zoom)
        {
            return new double[4,4]{
                { zoom, 0, 0, 0},
                { 0, -zoom, 0, 0 },
                { 0, 0, zoom, 0 },
                { 0, 0, 0, zoom }
            };
        }
        private static double[,] GetTranslationMatrix(int x, int y, int z)
        {
            return new double[4, 4]{
                { 1, 0, 0, x},
                { 0, 1, 0, y },
                { 0, 0, 1, z },
                { 0, 0, 0, 1 }
            };
        }
        private static double[,] GetTurnOXMatrix(float ox)
        {
            return new double[4, 4]{
                { 1, 0, 0, 0},
                { 0, Math.Cos(ox * Math.PI), -Math.Sin(ox * Math.PI), 0 },
                { 0, Math.Sin(ox * Math.PI), Math.Cos(ox * Math.PI), 0},
                { 0, 0, 0, 1 }
            };
        }
        private static double[,] GetTurnOYMatrix(float oy)
        {
            return new double[4, 4]{
                { Math.Cos(oy * Math.PI), 0, Math.Sin(oy * Math.PI), 0 },
                { 0, 1, 0, 0},
                { -Math.Sin(oy * Math.PI), 0, Math.Cos(oy * Math.PI), 0 },
                { 0, 0, 0, 1 }
            };
        }
        private static double[,] GetTurnOZMatrix(float oz)
        {
            return new double[4, 4]{
                { Math.Cos(oz * Math.PI), -Math.Sin(oz * Math.PI), 0, 0 },
                { Math.Sin(oz * Math.PI), Math.Cos(oz * Math.PI), 0, 0 },
                { 0, 0, 1, 0},
                { 0, 0, 0, 1 }
            };
        }
    }
}
