using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geometry;

namespace Geometry
{
    public class PointLight
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double W { get; set; }
        public Vector4D Ia { get; set; }
        public Vector4D Il { get; set; }
        public Point4D Coordinates => new Point4D(X, Y, Z, W, 0, 0, 0);
    }
}
