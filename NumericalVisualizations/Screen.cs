using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace NumericalVisualizations
{
    public class ScreenStructures
    {
         public struct colorstruct
        {public int red; public int green; public int blue; }
        public struct pixel
        { public int x; public int y; public colorstruct pixcolor; }
    }
}
