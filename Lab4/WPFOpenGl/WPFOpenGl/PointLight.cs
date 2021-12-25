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
        public Point4D Coordinates { get; set; }
        public Vector4D Ia { get; set; }
        public Vector4D Il { get; set; }
    }
}
