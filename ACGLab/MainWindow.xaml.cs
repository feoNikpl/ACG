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

namespace ACGLab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DrawingObject drawingObject = FileParser.FileParser.ParseFile("B.obj");
        float Zoom = 1.0f;
        int X = -10, Y = -10, Z = 0;
        float Ox = 0, Oy = 0, Oz = 0;
        float CameraSpeed = 0.05f;
        Vector3 CamPos = new Vector3(0.0f, 0.0f, 50.0f), CamTarget = new Vector3(0.0f, 0.0f, 1.0f), CamUp = new Vector3(0.0f, 1.0f, 0.0f);

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            drawing.bitmap = new WriteableBitmap((int)Grid1.ActualWidth, (int)Grid1.ActualHeight, 96, 96, PixelFormats.Bgr32, null);
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
            double dx = 0, dy = 0, x1, x2, y1, y2;
            int L;
            Matrix4x4 viewport = Transformation.Transformation.GetViewportMatrix(Grid1.ActualWidth, Grid1.ActualHeight);
            Matrix4x4 transformMatrix = Transformation.Transformation.GetTransformationMatrix(CamPos, CamTarget, CamUp, Grid1.ActualWidth, Grid1.ActualHeight, 1f, 100.0f, Zoom, X, Y, Z, Ox, Oy, Oz);
            try
            {
                // Reserve the back buffer for updates.
                drawing.bitmap.Lock();
                unsafe
                {
                    int backColor = Color.White.ToArgb();
                    for(int i = 0; i< drawing.bitmap.PixelWidth; i++)
                        for (int j = 0; j < drawing.bitmap.PixelHeight; j++)
                        {
                            IntPtr pBackBuffer = drawing.bitmap.BackBuffer + (int)j * drawing.bitmap.BackBufferStride + (int)i * 4;
                            *((int*)pBackBuffer) = backColor;
                        }
                    int color_data = Color.Black.ToArgb();
                    foreach (Polygon polygon in drawingObject.Instance)
                    {
                        Vector4 point1, point2;
                        for (int i = 0; i < polygon.Vertices.Count; i++)
                        {
                            if (i == polygon.Vertices.Count - 1)
                            {
                                point1 = Vector4.Transform(polygon.Vertices[0].ToVector(), transformMatrix);
                            }
                            else
                            {
                                point1 = Vector4.Transform(polygon.Vertices[i+1].ToVector(), transformMatrix);
                            }
                            point2 = Vector4.Transform(polygon.Vertices[i].ToVector(), transformMatrix);
                            point1 /= point1.W;
                            point2 /= point2.W;
                            point1 = Vector4.Transform(point1, viewport);
                            point2 = Vector4.Transform(point2, viewport);


                            x1 = point1.X;
                            y1 = point1.Y;
                            x2 = point2.X;
                            y2 = point2.Y;
                            
                            L = (int)Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1));
                            if (L == 0)
                            {
                                L = 1;
                            }
                            else
                            {
                                dx = (x2 - x1) / L;
                                dy = (y2 - y1) / L;
                            }
                            
                            for (int k = 0; k <= L; k++)
                            {
                                if (x1 > 0 && y1 > 0 && x1 < (int)Grid1.ActualWidth && y1 < (int)Grid1.ActualHeight)
                                {
                                    IntPtr pBackBuffer = drawing.bitmap.BackBuffer + (int)y1 * drawing.bitmap.BackBufferStride + (int)x1 * 4;
                                    *((int*)pBackBuffer) = color_data;
                                }
                                x1 += dx;
                                y1 += dy;
                            }
                            if (x2 > 0 && y2 > 0 && x2 < (int)Grid1.ActualWidth-1 && y2 < (int)Grid1.ActualHeight-1)
                            {
                                IntPtr pBackBuffer = drawing.bitmap.BackBuffer + (int)y2 * drawing.bitmap.BackBufferStride + (int)x2 * 4;
                                *((int*)pBackBuffer) = color_data;
                            }
                        }
                    }
                    drawing.bitmap.AddDirtyRect(new Int32Rect(0, 0, drawing.bitmap.PixelWidth, drawing.bitmap.PixelHeight));
                }
            }
            finally
            {
                drawing.bitmap.Unlock();
            }

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
