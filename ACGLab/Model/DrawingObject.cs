using System;
using System.Collections.Generic;
using System.Text;

namespace ACGLab.Model
{
    public class DrawingObject
    {
        public List<Polygon> Instance;

        public DrawingObject(List<Polygon> instance)
        {
            Instance = instance;
        }
    }
}
