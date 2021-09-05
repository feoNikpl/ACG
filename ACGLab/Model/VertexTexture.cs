using System;
using System.Collections.Generic;
using System.Text;

namespace ACGLab.Model
{
    public class VertexTexture
    {
        double U;
        double V;
        double W;

        public VertexTexture(double u, double v, double w)
        {
            U = u;
            V = v;
            W = w;
        }
    }
}
