using System;
using System.Collections.Generic;
using System.Text;

namespace ACGLab.Model
{
    public class Vertex
    {
        public double X;
        public double Y;
        public double Z;
        public double W;

        public Vertex(double x, double y, double z, double w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public double[] ToArray()
        {
            return new double[4] { X, Y, Z, W };

        }

        public void FromArray(double[] arr)
        {
            X = arr[0];
            Y = arr[1];
            Z = arr[2];
            W = arr[3];
        }
    }
}
