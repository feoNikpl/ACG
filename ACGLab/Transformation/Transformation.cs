using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ACGLab.Transformation
{
    public class Transformation
    {
        public static double[,] GetTransformationMatrix(Vector3 camPos, Vector3 camTarget, Vector3 camUp, double width, double height, 
                                                        float zNear, float zFar, float zoom, int x, int y, int z, 
                                                        float ox, float oy, float oz)
        {
            return MatrixMath.Multiplication(
                MatrixMath.Multiplication(
                    MatrixMath.Multiplication(
                        GetViewportMatrix(width, height), 
                        GetProjectionMatrix(width, height, zNear, zFar)), 
                    GetViewMatrix(camPos, camTarget,camUp)), 
                GetModelMatrix(zoom, x, y, z, ox, oy, oz));
        }

        private static double[,] GetModelMatrix(float zoom, int x, int y, int z, float ox, float oy, float oz)
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

        private static double[,] GetZoomMatrix(float zoom)
        {
            return new double[4,4]{
                { zoom, 0, 0, 0},
                { 0, zoom, 0, 0 },
                { 0, 0, zoom, 0 },
                { 0, 0, 0, 1 }
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

        private static double[,] GetViewMatrix(Vector3 eye, Vector3 target, Vector3 up)
        {
            Vector3 Zaxis = Vector3.Normalize(eye - target);
            Vector3 Xaxis = Vector3.Normalize(Vector3.Cross(up, Zaxis));
            Vector3 Yaxis = up;
            return new double[4, 4]{ 
                { Xaxis.X, Xaxis.Y, Xaxis.Z, -Vector3.Dot(Xaxis,eye) },
                { Yaxis.X, Yaxis.Y, Yaxis.Z, -Vector3.Dot(Yaxis,eye) },
                { Zaxis.X, Zaxis.Y, Zaxis.Z, -Vector3.Dot(Zaxis,eye) },
                { 0, 0, 0, 1 },};
        }
        private static double[,] GetProjectionMatrix(double width, double height, float zNear, float zFar)
        {
            return new double[4, 4]{
                { 2/width, 0, 0, 0 },
                { 0, 2/height, 0, 0 },
                { 0, 0, 1/(zNear-zFar), zNear/(zNear-zFar) },
                { 0, 0, 0, 1 },};
        }
        private static double[,] GetViewportMatrix(double width, double height)
        {
            return new double[4, 4]{
                { width/2, 0, 0, width/2 },
                { 0, -height/2, 0, height/2 },
                { 0, 0, 1, 0 },
                { 0, 0, 0, 1 },};
        }
    }
}
