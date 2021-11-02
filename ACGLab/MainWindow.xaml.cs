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
        public DrawingObject drawingObject = FileParser.FileParser.ParseFile("radio.obj");
        float Zoom = 1.0f;
        int X = -10, Y = -10, Z = 0;
        float Ox = 0, Oy = 0, Oz = 0;
        float CameraSpeed = 1.0f;
        Vector3 CamPos = new Vector3(0.0f, 0.0f, 50.0f), CamTarget = new Vector3(0.0f, 0.0f, 1.0f), CamUp = new Vector3(0.0f, 1.0f, 0.0f);
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
                        Vector4 point, normal;
                        for (int i = 0; i < polygon.Vertices.Count; i++)
                        {
                            point = Vector4.Transform(polygon.Vertices[i].ToVector(), transformMatrix);
                            point /= point.W;
                            points.Add(Vector4.Transform(point, viewport));

                            normal = Vector4.Transform(polygon.VerticesNormal[i].ToVector(), transformMatrix);
                            normal /= normal.W;
                            normals.Add(normal);
                        }
                        color_data = CalcColor(points, normals, new Vector4(CamPos, 1));
                        if (polygon.Vertices.Count > 3)
                        {
                            DrawTriangle(color_data, points[0], points[1], points[2]);
                            DrawTriangle(color_data, points[0], points[3], points[2]);
                        }
                        else
                        {
                            DrawTriangle(color_data, points[0], points[1], points[2]);
                        }
                    }
                    //DrawTriangle(color_data, new Vector4(0,0,0.5f,0), new Vector4(400, 300, 0.5f, 0), new Vector4(400, 100, 0.5f, 0));
                    //DrawTriangle(Color.Red.ToArgb(), new Vector4(0, 0, 0.5f, 0), new Vector4(400, 300, 0.2f, 0), new Vector4(400, 200, 0.4f, 0));
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

        /*private void DrawLine(int color_data, float x1, float y1,float z1, float x2, float y2, float z2)
        {
            unsafe
            {
                int L = (int)Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1));
                float dx = 0, dy = 0, dz = 0;
                if (L == 0)
                {
                    L = 1;
                }
                else
                {
                    dx = (x2 - x1) / L;
                    dy = (y2 - y1) / L;
                    dz = (z2 - z1) / L;
                }

                for (int k = 0; k <= L; k++)
                {
                    if (x1 > 0 && y1 > 0 && x1 < (int)Grid1.ActualWidth && y1 < (int)Grid1.ActualHeight)
                    {
                        if (ZBuffer[(int)((int)x1 * Grid1.ActualHeight + (int)y1)] > z1)
                        {
                            Bitmap[(int)((int)x1 * Grid1.ActualHeight + (int)y1)] = color_data;
                            ZBuffer[(int)((int)x1 * Grid1.ActualHeight + (int)y1)] = z1;
                        }
                        
                    }
                    x1 += dx;
                    y1 += dy;
                    z1 += dz;
                }
            }
        }*/
        private void DrawTriangle(int color_data, Vector4 point1, Vector4 point2, Vector4 point3)
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
            }
            if (point1.Y > point3.Y)
            {
                temp = point1;
                point1 = point3;
                point3 = temp;
            }
            if (point2.Y > point3.Y)
            {
                temp = point2;
                point2 = point3;
                point3 = temp;
            }
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
                if (a.X > b.X)
                {
                    temp = a;
                    a = b;
                    b = temp;
                }
                for (int j = (int)a.X; j <= b.X; j++)
                {
                    double phi = b.X == a.X
                            ? 1d
                            : (double)(j - a.X) / (double)(b.X - a.X);
                    float z = (float)(a.Z + (b.Z - a.Z) * phi);
                    int idx = (int)(j * Grid1.ActualHeight + point1.Y + i);
                    if (j > 0 && point1.Y + i > 0 && j < (int)Grid1.ActualWidth && point1.Y + i < (int)Grid1.ActualHeight) 
                    {
                        if (ZBuffer[idx] >= z)
                        {
                            Bitmap[idx] = color_data;
                            ZBuffer[idx] = z;
                        }
                    }
                }
            }
            /*float x1 = point1.X, x2 = point2.X, x3 = point3.X, y1 = point1.Y, y2 = point2.Y, y3 = point3.Y, z1 = point1.Z, z2 = point2.Z, z3 = point3.Z;
            int L = (int)Math.Max(Math.Max(Math.Abs(x1 - x2), Math.Abs(x1 - x3)), Math.Max(Math.Abs(y1 - y2), Math.Abs(y1 - y3)));
            float dx2 = 0, dy2 = 0, dx3 = 0, dy3 = 0, dz2 = 0, dz3 = 0;
            dx2 = (x1 - x2) / L;
            dx3 = (x1 - x3) / L;
            dy2 = (y1 - y2) / L;
            dy3 = (y1 - y3) / L;
            dz2 = (z1 - z2) / L;
            dz3 = (z1 - z3) / L;
            for (int i = 0; i <= L; i++)
            {
                DrawLine(color_data,x2, y2, z2, x3, y3, z3);
                x2 += dx2;
                x3 += dx3;
                y2 += dy2;
                y3 += dy3;
                z2 += dz2;
                z3 += dz3;
            }*/
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
                    if (Zoom > 0.5f)
                    {
                        Zoom -= 0.5f;
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
                    X += 10;
                    break;
                case Key.Left:
                    X -= 10;
                    break;
                case Key.Up:
                    Y += 10;
                    break;
                case Key.Down:
                    Y -= 10;
                    break;
                case Key.Q:
                    Z -= 10;
                    break;
                case Key.E:
                    Z += 10;
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
                case Key.W:
                    CamPos.Z += CameraSpeed;
                    break;
                case Key.S:
                    CamPos.Z -= CameraSpeed;
                    break;
                case Key.A:
                    CamPos.X += CameraSpeed;
                    break;
                case Key.D:
                    CamPos.X -= CameraSpeed;
                    break;
                case Key.Space:
                    CamPos.Y += CameraSpeed;
                    break;
                case Key.LeftShift:
                    CamPos.Y -= CameraSpeed;
                    break;
            }
            Draw();
        }

    }

    
}
