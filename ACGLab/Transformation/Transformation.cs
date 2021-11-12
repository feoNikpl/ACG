using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ACGLab.Transformation
{
    public class Transformation
    {
        public static Matrix4x4 GetTransformationMatrix(Vector3 camPos, Vector3 camTarget, Vector3 camUp, double width, double height, 
                                                        float zNear, float zFar, float zoom, int x, int y, int z, 
                                                        float ox, float oy, float oz)
        {
            var aspect = (float)(width / height);
            var fov = (float)Math.PI * (45) / 180;

            return Matrix4x4.CreateScale(zoom) *
            Matrix4x4.CreateRotationX((float)(ox * Math.PI)) *
            Matrix4x4.CreateRotationY((float)(oy * Math.PI)) *
            Matrix4x4.CreateRotationZ((float)(oz * Math.PI)) *
            Matrix4x4.CreateTranslation(x, y, z) *
            Matrix4x4.CreateLookAt(camPos,camTarget,camUp) *
            Matrix4x4.CreatePerspectiveFieldOfView(fov, aspect,zNear,zFar);
        }

        
        public static Matrix4x4 GetViewportMatrix(double width, double height)
        {
            return Matrix4x4.Transpose(new Matrix4x4((float)width / 2, 0, 0, (float)width / 2, 0, (float)-height / 2, 0, (float)height / 2, 0, 0, 1, 0, 0, 0, 0, 1));
        }

        public static Matrix4x4 GetTranslationMatrix(float zoom, int x, int y, int z, float ox, float oy, float oz)
        {
            return Matrix4x4.CreateScale(zoom) *
            Matrix4x4.CreateRotationX((float)(ox * Math.PI)) *
            Matrix4x4.CreateRotationY((float)(oy * Math.PI)) *
            Matrix4x4.CreateRotationZ((float)(oz * Math.PI)) *
            Matrix4x4.CreateTranslation(x, y, z);
        }
        
        private static Matrix4x4 GetProjectionMatrix(double width, double height,float zNear, float zFar)
        {
            var aspect = (float)(width / height); 
            var fov = (float)Math.PI * (45) / 180; 

            var m00 = 1 / (aspect * (float)Math.Tan(fov / 2));
            var m11 = 1 / (float)Math.Tan(fov / 2);
            var m22 = zFar / (zNear - zFar);
            var m23 = (zNear * zFar) / (zNear - zFar);

            var projectionMatrix = new Matrix4x4(m00, 0, 0, 0,
                                                   0, m11, 0, 0,
                                                   0, 0, m22, m23,
                                                   0, 0, -1, 0);
            projectionMatrix.Translation = new Vector3(0, 0, m23);
            projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
                                            fov,
                                            aspect,
                                            zNear,
                                            zFar);

            return projectionMatrix;
        }
    }
}
