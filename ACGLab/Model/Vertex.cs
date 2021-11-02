using System;
using System.Collections.Generic;
using System.Numerics;
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

        public Vector4 ToVector()
        {
            return new Vector4( (float)X, (float)Y, (float)Z, (float)W );

        }
    }
}
