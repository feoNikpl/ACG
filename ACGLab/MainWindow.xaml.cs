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

namespace ACGLab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DrawingObject drawingObject = FileParser.FileParser.ParseFile("C.obj");
        float Zoom = 1.0f;
        int X = 0, Y = 0, Z = 0;
        float Ox = 0, Oy = 0, Oz = 0;
        float CameraSpeed = 0.05f;
        Vector3 CamPos = new Vector3(1.0f, 1.0f, 1.0f), CamTarget = new Vector3(0.0f, 0.0f, 0.0f), CamUp = new Vector3(0.0f, 1.0f, 0.0f);
        Vector3 SpeedVecOx = new Vector3(1f, 0f, 0f);
        Vector3 SpeedVecOy = new Vector3(0f, 1f, 0f);
        Vector3 SpeedVecOz = new Vector3(0f, 0f, 1f);

        Bitmap bitmap;
        public MainWindow()
        {
            InitializeComponent();
            bitmap = new Bitmap((int)Width, (int)Height);
            Draw();

        }


        private void Draw()
        {
            bitmap = ClearBitmap(bitmap, Color.White);
            double[,] a = Transformation.Transformation.GetTransformationMatrix(CamPos, CamTarget, CamUp, Width, Height, 0.0f, 1.0f, Zoom, X, Y, Z, Ox, Oy, Oz);
            foreach (Polygon p in drawingObject.Instance)
            {
                bitmap = DrawPolygon(bitmap, p, a);
            }
            Canvas.Source = BitmapToImageSource(bitmap);


        }
        private Bitmap ClearBitmap(Bitmap bitmap, Color color)
        {
            for (int i = 0; i < bitmap.Width; i++)
                for (int j = 0; j < bitmap.Height; j++)
                    bitmap.SetPixel(i, j, color);
            return bitmap;
        }
        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        public Bitmap DrawLine(Bitmap bitmap, double x1, double y1, double x2, double y2)
        {
            int L = (int)Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1));
            double dx = (x2 - x1) / L;
            double dy = (y2 - y1) / L;
            for (int k = 0; k <= L; k++)
            {
                if (x1 > 0 && y1 > 0 && x1 < Width && y1 < Height)
                    bitmap.SetPixel((int)x1, (int)y1, Color.Black);
                x1 += dx;
                y1 += dy;
            }
            if (x2 > 0 && y2 > 0 && x2 < Width && y2 < Height)
                bitmap.SetPixel((int)x2, (int)y2, Color.Black);
            return bitmap;
        }

        public Bitmap DrawPolygon(Bitmap bitmap, Polygon polygon, double[,] transformMatrix)
        {
            double[] point1, point2 = new double[4];
            for (int i = 0; i < polygon.Vertices.Count; i++)
            {
                if (i == polygon.Vertices.Count - 1)
                {
                    point1 = MatrixMath.Multiplication(transformMatrix, polygon.Vertices[0].ToArray());
                }
                else
                {
                    point1 = MatrixMath.Multiplication(transformMatrix, polygon.Vertices[i].ToArray());
                    point2 = MatrixMath.Multiplication(transformMatrix, polygon.Vertices[i + 1].ToArray());
                }
                DrawLine(bitmap, point1[0], point1[1], point2[0], point2[1]);
            }
            return bitmap;
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
                case Key.Z:
                    if (Ox < 2f)
                    {
                        Ox += 0.1f;
                    }
                    else
                    {
                        Ox = 0;
                    }
                    break;
                case Key.X:
                    if (Oy < 2f)
                    {
                        Oy += 0.1f;
                    }
                    else
                    {
                        Oy = 0;
                    }
                    break;
                case Key.C:
                    if (Oz < 2f)
                    {
                        Oz += 0.1f;
                    }
                    else
                    {
                        Oz = 0;
                    }
                    break;
                case Key.W:
                    CamPos += SpeedVecOz;
                    break;
                case Key.S:
                    CamPos -= SpeedVecOz;
                    break;
                case Key.A:
                    CamPos -= SpeedVecOx;
                    break;
                case Key.D:
                    CamPos += SpeedVecOx;
                    break;
            }
            Draw();
        }
    }

    
}
