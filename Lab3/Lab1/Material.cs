using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Geometry;
using View;

namespace Geometry
{
    public class Material
    {
        public SolidColorBrush Color { get; set; }
        public Vector4D Ka { get; set; }
        public Vector4D Kd { get; set; }
        public Vector4D Ks { get; set; }
        public double P { get; set; }
    }
}
