using System;
using System.Collections.Generic;
using System.Text;

namespace ACGLab.Model
{
    public class Polygon
    {
        public List<Vertex> Vertices;

        public Polygon (List<Vertex> vertices)
        {
            Vertices = vertices;
        }

    }
}
