using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ACGLab.Model;
using ACGLab.Transformation;
using System.IO;
using System.Drawing;
using Color = System.Drawing.Color;
using System.Numerics;
using ACGLab.View;
using System.Collections.Generic;
using System.Linq;

namespace ACGLab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DrawingObject drawingObject = FileParser.FileParser.ParseFile("C.obj");
        float Zoom = 0.5f;
        int X = 0, Y = 0, Z = 0;
        float Ox = 0, Oy = 0, Oz = 0;
        float CameraSpeed = 100.0f;
        Vector3 LightTarget = new Vector3(1000.0f, 1000.0f, 1000.0f);
        Vector3 CamPos = new Vector3(0.0f, 10.0f, 45.0f), CamTarget = new Vector3(0.0f, 0.0f, 1.0f), CamUp = new Vector3(0.0f, 1.0f, 0.0f);
        Vector3 origlightPoint = new Vector3(-1.08f, -1.26f, 45f);
        int[] Bitmap;
        float[] ZBuffer;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            drawing.bitmap = new WriteableBitmap((int)Grid1.ActualWidth, (int)Grid1.ActualHeight, 96, 96, PixelFormats.Bgr32, null);
            Bitmap = new int[drawing.bitmap.PixelWidth* drawing.bitmap.PixelWidth];
            ZBuffer = new float[drawing.bitmap.PixelWidth * drawing.bitmap.PixelWidth];
            DataContext = drawing;
            Draw();
        }

        BitmapDrawing drawing = new BitmapDrawing();

        public MainWindow()
        {
            InitializeComponent();
        }


        private void Draw()
        {
            int backColor = Color.White.ToArgb();
            Bitmap = Enumerable.Repeat(backColor, Bitmap.Length).ToArray();
            ZBuffer = Enumerable.Repeat(float.MaxValue, ZBuffer.Length).ToArray();
            Matrix4x4 trnaslationMatrix = Transformation.Transformation.GetTranslationMatrix(Zoom, X, Y, Z, Ox, Oy, Oz);
            Matrix4x4 camMatrix =Transformation.Transformation.GetCamMatrix(CamPos, CamTarget, CamUp, Zoom, X, Y, Z, Ox, Oy, Oz);
            Matrix4x4 viewport = Transformation.Transformation.GetViewportMatrix(Grid1.ActualWidth, Grid1.ActualHeight);
            Matrix4x4 transformMatrix = Transformation.Transformation.GetTransformationMatrix(CamPos, CamTarget, CamUp, Grid1.ActualWidth, Grid1.ActualHeight, 1f, 100.0f, Zoom, X, Y, Z, Ox, Oy, Oz);
            try
            {
                // Reserve the back buffer for updates.
                drawing.bitmap.Lock();
                unsafe
                {
                    int color_data = Color.Black.ToArgb();
                    foreach (Polygon polygon in drawingObject.Instance)
                    {
                        List<Vector4> points = new List<Vector4>();
                        List<Vector4> normals = new List<Vector4>();
                        List<Vector4> pointsCam = new List<Vector4>();
                        Vector4 point, normal;
                        for (int i = 0; i < polygon.Vertices.Count; i++)
                        {
                            point = Vector4.Transform(polygon.Vertices[i].ToVector(), transformMatrix);
                            point /= point.W;
                            points.Add(Vector4.Transform(point, viewport));

                            normals.Add(Vector4.Transform(polygon.VerticesNormal[i].ToVector(), trnaslationMatrix));

                            pointsCam.Add(Vector4.Transform(polygon.Vertices[i].ToVector(), camMatrix));
                        }
                        //color_data = CalcColor(points, normals, new Vector4(LightTarget, 1));
                        Vector4 Camr = new Vector4(CamPos, 1);
                        if (polygon.Vertices.Count > 3)
                        {
                            DrawTriangle(color_data, points[0], points[1], points[2], Camr, polygon, trnaslationMatrix, normals, pointsCam);
                            DrawTriangle(color_data, points[0], points[3], points[2], Camr, polygon, trnaslationMatrix, new List<Vector4> { normals[0], normals[3], normals[2]}, new List<Vector4> { pointsCam[0], pointsCam[3], pointsCam[2] });
                        }
                        else
                        {
                            DrawTriangle(color_data, points[0], points[1], points[2], Camr, polygon, trnaslationMatrix, normals, pointsCam);
                        }
                    }
                    for (int i = 0; i < drawing.bitmap.PixelWidth; i++)
                        for (int j = 0; j < drawing.bitmap.PixelHeight; j++)
                        {
                            IntPtr pBackBuffer = drawing.bitmap.BackBuffer + (int)j * drawing.bitmap.BackBufferStride + (int)i * 4;
                            *((int*)pBackBuffer) = Bitmap[i * drawing.bitmap.PixelHeight + j];
                        }
                    //
                    Grid1.Width = drawing.bitmap.PixelWidth;
                    Grid1.Height = drawing.bitmap.PixelHeight;
                    drawing.bitmap.AddDirtyRect(new Int32Rect(0, 0, drawing.bitmap.PixelWidth, drawing.bitmap.PixelHeight));
                }
            }
            finally
            {
                drawing.bitmap.Unlock();
            }

        }
        private int CalcColor(List<Vector4> points, List<Vector4> normals, Vector4 light)
        {
            float diff=0;
            for(int i=0; i < points.Count; i++)
            {
                Vector4 lightDirection = light - points[i];
                Vector3 L = Vector3.Normalize(new Vector3(lightDirection.X, lightDirection.Y, lightDirection.Z));
                Vector3 N = Vector3.Normalize(new Vector3(normals[i].X, normals[i].Y, normals[i].Z));
                diff += Math.Max(Vector3.Dot(N, L), 0);
            }
            diff /= points.Count;
            int color = (int)(255 * diff);
            return (Color.FromArgb((int)(255 * diff), 0,0).ToArgb());
        }

        private void DrawTriangle(int color_data, Vector4 point1, Vector4 point2, Vector4 point3, Vector4 camr, Polygon pol, Matrix4x4 matr,List <Vector4> normals, List<Vector4> pointsCam)
        {
            var a0 = Vector4.Transform(pol.Vertices[0].ToVector(), matr) -
                 Vector4.Transform(pol.Vertices[1].ToVector(), matr);
            var b0 = Vector4.Transform(pol.Vertices[2].ToVector(), matr) -
                Vector4.Transform(pol.Vertices[0].ToVector(), matr);
            Vector3 vector1 = new Vector3(a0.X, a0.Y, a0.Z);
            Vector3 vector2 = new Vector3(b0.X, b0.Y, b0.Z);
            var norm = Vector3.Cross(vector1, vector2);
            Vector4 cam = Vector4.Transform(pol.Vertices[0].ToVector(), matr) - camr;
            Vector3 vector = new Vector3(cam.X, cam.Y, cam.Z);
            var d0 = Vector3.Dot(norm, vector);
            if (d0 > 0)
            {
                Vector4 temp;
                if (point1.Y == point2.Y && point1.Y == point3.Y)
                {
                    return;
                }
                if (point1.Y > point2.Y)
                {
                    temp = point1;
                    point1 = point2;
                    point2 = temp;
                    temp = normals[0];
                    normals[0] = normals[1];
                    normals[1] = temp;
                    temp = pointsCam[0];
                    pointsCam[0] = pointsCam[1];
                    pointsCam[1] = temp;
                }
                if (point1.Y > point3.Y)
                {
                    temp = point1;
                    point1 = point3;
                    point3 = temp;
                    temp = normals[0];
                    normals[0] = normals[2];
                    normals[2] = temp;
                    temp = pointsCam[0];
                    pointsCam[0] = pointsCam[2];
                    pointsCam[2] = temp;
                }
                if (point2.Y > point3.Y)
                {
                    temp = point2;
                    point2 = point3;
                    point3 = temp;
                    temp = normals[1];
                    normals[1] = normals[2];
                    normals[2] = temp;
                    temp = pointsCam[1];
                    pointsCam[1] = pointsCam[2];
                    pointsCam[2] = temp;
                }
                point3.Y += 1;
                point1.Y -= 1;
                float totalHeight = point3.Y - point1.Y;
                for (int i = 0; i < totalHeight; i++)
                {
                    bool secondHalf = i > point2.Y - point1.Y || point2.Y == point1.Y;
                    float segmentHeight = secondHalf ? point3.Y - point2.Y : point2.Y - point1.Y;
                    double alpha = (double)i / totalHeight;
                    double beta = (double)(i - (secondHalf ? point2.Y - point1.Y : 0)) / segmentHeight;
                    Vector4 a = point1 + (point3 - point1) * (float)alpha;
                    Vector4 b = secondHalf
                            ? point2 + (point3 - point2) * (float)beta
                            : point1 + (point2 - point1) * (float)beta;
                    Vector4 aN = normals[0] + (normals[2] - normals[0]) * (float)alpha;
                    Vector4 bN = secondHalf
                            ? normals[1] + (normals[2] - normals[1]) * (float)beta
                            : normals[0] + (normals[1] - normals[0]) * (float)beta;
                    Vector4 aP = pointsCam[0] + (pointsCam[2] - pointsCam[0]) * (float)alpha;
                    Vector4 bP = secondHalf
                            ? pointsCam[1] + (pointsCam[2] - pointsCam[1]) * (float)beta
                            : pointsCam[0] + (pointsCam[1] - pointsCam[0]) * (float)beta;

                    if (a.X > b.X)
                    {
                        temp = a;
                        a = b;
                        b = temp;
                        temp = aN;
                        aN = bN;
                        bN = temp;
                        temp = aP;
                        aP = bP;
                        bP = temp;
                    }
                    for (int j = (int)a.X; j <= b.X; j++)
                    {
                        double phi = b.X == a.X
                                ? 1d
                                : (double)(j - a.X) / (double)(b.X - a.X);
                        float z = (float)(a.Z + (b.Z - a.Z) * phi);
                        float zN = (float)(aN.Z + (bN.Z - aN.Z) * phi);
                        float xN = (float)(aN.X + (bN.X - aN.X) * phi);
                        float zP = (float)(aP.Z + (bP.Z - aP.Z) * phi);
                        float xP = (float)(aP.X + (bP.X - aP.X) * phi);
                        int idx = (int)(j * Grid1.ActualHeight + point1.Y + i);
                        if (j > 0 && point1.Y + i > 0 && j < (int)Grid1.ActualWidth && point1.Y + i < (int)Grid1.ActualHeight)
                        {
                            if (ZBuffer[idx] >= z)
                            {
                                //Bitmap[idx] = color_data;
                                if(new Vector3(j, point1.Y + i, z) == new Vector3(a.X, a.Y, a.Z) || new Vector3(j, point1.Y + i, z) == new Vector3(b.X, b.Y, b.Z))
                                    Bitmap[idx] = (int)(0.5f*CalcColorByFongo(new Vector3(j, point1.Y + i, z),
                                                                    new Vector3 (xN, aN.Y, zN),
                                                                    LightTarget,
                                                                    new Vector3(xP, aP.Y, zP)));
                                else
                                    Bitmap[idx] = CalcColorByFongo(new Vector3(j, point1.Y + i, z),
                                                                    new Vector3(xN, aN.Y, zN),
                                                                    LightTarget,
                                                                    new Vector3(xP, aP.Y, zP));

                                /*Bitmap[idx] = CalcColorByFongo(new Vector3(j, point1.Y + i, z),
                                                                norm,
                                                                LightTarget);*/
                                ZBuffer[idx] = z;
                            }
                        }
                    }
                }
            }
        }
        private Vector3 GetPointNormal (Vector4 point, Vector4 A, Vector4 B)
        {
            A -= point;
            B -= point;
            return Vector3.Cross(new Vector3(A.X, A.Y, A.Z), new Vector3(B.X, B.Y, B.Z));
            //return new Vector3(A.Y*B.Z - A.Z*B.Y, A.Z*B.X -A.X*B.Z, A.X*B.Y-A.Y*B.X);
        }

        private int CalcColorByFongo (Vector3 point, Vector3 pointNormal, Vector3 light, Vector3 pointCam)
        {

            var MColor = new Vector3(255f, 255f, 255f);
            Vector3 ambientLightColor = 0.2f  * MColor;
            Vector3 lightDirection = light - point;
            Vector3 L = Vector3.Normalize(lightDirection);
            Vector3 N = Vector3.Normalize(pointNormal);
            Vector3 diffuseLight = 0.2f *Math.Max(Vector3.Dot(N, L),-Vector3.Dot(N, L)) * MColor;

            var specularColor = new Vector3(255F, 255F, 255F);
            Vector3 eyeDirection = new Vector3(0, 0, 0) - pointCam;
            Vector3 eyeVector = Vector3.Normalize(eyeDirection);

            Vector3 E = Vector3.Normalize(eyeVector);
            Vector3 R = Vector3.Reflect(-L, N);
            if (Vector3.Dot(R, E) > 0)
            {
                MColor = new Vector3(255f, 255f, 255f);
            }
            float specular = 1f * (float)Math.Pow(Math.Max(Vector3.Dot(R, E), 0f), 50f);
            Vector3 specularLight = specularColor * specular;
            Vector3 sumColor;
            sumColor = ambientLightColor;
            sumColor += diffuseLight;
            sumColor += specularLight;

            return Color.FromArgb((byte)Math.Min(sumColor.X, 255), (byte)Math.Min(sumColor.Y, 255), (byte)Math.Min(sumColor.Z, 255)).ToArgb();
           
        }
       
        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                Zoom += 0.5f;
            }
            else
            {
                if (e.Delta < 0)
                {
                    if (Zoom > 0.1f)
                    {
                        Zoom -= 0.2f;
                    }
                }
            }
            Draw();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Right:
                    CamTarget.X += 10;
                    break;
                case Key.Left:
                    CamTarget.X -= 10;
                    break;
                case Key.Up:
                    CamTarget.Y += 10;
                    break;
                case Key.Down:
                    CamTarget.Y -= 10;
                    break;
                case Key.Q:
                    CamTarget.Z += 10;
                    break;
                case Key.E:
                    CamTarget.Z -= 10;
                    break;
                case Key.Z:
                    if (Ox < 2f)
                    {
                        Ox += 0.05f;
                    }
                    else
                    {
                        Ox = 0;
                    }
                    break;
                case Key.X:
                    if (Oy < 2f)
                    {
                        Oy += 0.05f;
                    }
                    else
                    {
                        Oy = 0;
                    }
                    break;
                case Key.C:
                    if (Oz < 2f)
                    {
                        Oz += 0.05f;
                    }
                    else
                    {
                        Oz = 0;
                    }
                    break;
            }
            Draw();
        }

    }

    
}
