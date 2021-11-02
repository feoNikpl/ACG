using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ACGLab.Model
{
    public class VertexNormal
    {
        double I;
        double J;
        double K;

        public VertexNormal(double i, double j, double k)
        {
            I = i;
            J = j;
            K = k;
        }

        public Vector4 ToVector()
        {
            return new Vector4((float)I, (float)J, (float)K, 1);

        }
    }
}
