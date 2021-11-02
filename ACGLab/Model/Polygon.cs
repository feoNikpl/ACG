using System;
using System.Collections.Generic;
using System.Text;

namespace ACGLab.Model
{
    public class Polygon
    {
        public List<Vertex> Vertices;
        public List<VertexNormal> VerticesNormal;

        public Polygon (List<Vertex> vertices, List<VertexNormal> verticesNormal)
        {
            Vertices = vertices;
            VerticesNormal = verticesNormal;
        }

    }
}
