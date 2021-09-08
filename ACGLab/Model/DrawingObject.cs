using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ACGLab.Model
{
    public class DrawingObject
    {
        public List<Polygon> Instance;
        public Vector3 GeometricCenter;

        public DrawingObject(List<Polygon> instance, Vector3 geometricCenter)
        {
            Instance = instance;
            GeometricCenter = geometricCenter;
        }
    }
}
