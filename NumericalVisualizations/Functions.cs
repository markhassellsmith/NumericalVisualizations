using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NumericalVisualizations
{
    /// <summary>
    ///
    /// </summary>
    public class Functions
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="zin"></param>
        /// <returns></returns>
        public static Complex F(Complex zin)
        {
            // Complex zout = zin^7 + zin^6 - zin^5 + zin^4 - zin^3 + zin*zin - zin + 4.0;
            //Complex zout = zin * zin * zin * zin * zin * zin * zin + zin * zin * zin * zin * zin * zin - zin * zin * zin * zin * zin + zin * zin * zin * zin - zin * zin * zin + zin * zin - zin + 4.0;
            //Complex zout = zin * (zin * (zin * (zin * (zin * (zin * (zin + 1.0) - 1.0) + 1.0) - 1.0) + 1.0) - 1.0) + 4.0;
            // Complex zout = (((((zin + 1.0) * zin + 1.0) * zin - 1.0) * zin + 1.0) * zin - 1.0) * zin + 4.0;
            //Complex zout = Complex.Sin(zin);
            //Complex zout = (4.0 * zin * zin - 3.0 * zin + 7.0) / (zin + 4.0);  //a rational function in Complex domain
            //Complex zout = Complex.Exp(-zin) * Complex.Sin(zin);
            //Complex zout =zin *zin*zin - zin*zin  +  zin;

            Complex zout = 6.0 * zin * zin * zin * zin + 4.0 * zin * zin - zin + 1.0;

            return zout;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="zin"></param>
        /// <returns></returns>
        public static Complex FP(Complex zin)
        {
            // Complex zout = 6.0*zin^5 - 5.0*zin^4 + 4.0*zin^3 - 3.0*zin^2 + 2.0*zin - 1.0;
            // Complex zout = 7*zin*zin*zin*zin*zin*zin + 6.0 * zin * zin * zin * zin * zin - 5.0 * zin * zin * zin * zin + 4.0 * zin * zin * zin - 3.0 * zin * zin + 2.0 * zin - 1.0;
            //  Complex zout =Complex.Cos(zin);
            //Complex zout = ((zin + 4.0) * (8.0 * zin - 3.0) - (4.0 * zin * zin - 3.0 * zin + 7.0)) / (zin + 4.0) * (zin + 4.0);
            //Complex zout = -Complex.Exp(-zin) * Complex.Sin(zin) + Complex.Exp(-zin) * Complex.Cos(zin);
            //Complex zout = 3.0*zin * zin - 2.0*zin + 1.0;

            Complex zout = 24.0 * zin * zin * zin + 8.0 * zin - 1.0;

            return zout;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="zin"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Complex FMandelbrot(Complex zin, Complex c)
        {
            //Complex zout = Complex.Tan(zin) + c ;
            //Complex zout = zin*Complex.Sin(zin);
            Complex zout = zin * zin * zin * zin + c;
            return zout;
        }

        public static int FHailStone(int x)
        {
            if (x % 2 == 0) return x / 2; else return 3 * x - 1;
        }

        //public static int FHailStoneNextX(int xin, int yin)
        //{
        //    if (xin % 2 == 0 & yin % 2 == 0)
        //        return xin / 2  + 1;
        //    else if (xin % 2 == 0 & yin % 2 != 0)
        //        return xin / 2 - 1;
        //    else if (xin % 2 != 0 & yin % 2 == 0)
        //        return 3 * xin + 1;
        //    else
        //        return 3 * xin - 1;
        //}

        //public static int FHailStoneNextY(int xin, int yin)
        //{
        //    if (xin % 2 == 0 & yin % 2 == 0)
        //        return  yin / 2 + 3;
        //    else if (xin % 2 == 0 & yin % 2 != 0)
        //        return 3 * yin + -1;
        //    else if (xin % 2 != 0 & yin % 2 == 0)
        //        return yin / 2 + 1;
        //    else
        //        return 3 * yin + 3;
        //}

        /// <summary>
        ///
        /// </summary>
        /// <param name="xin"></param>
        /// <param name="yin"></param>
        /// <returns></returns>
        public static int FHailStoneNextX(int xin, int yin)
        {
            if (xin % 2 == 0 & yin % 2 == 0)
                return xin / 2;
            else if (xin % 2 == 0 & yin % 2 != 0)
                return xin / 2 + 1;
            else if (xin % 2 != 0 & yin % 2 == 0)
                return 3 * xin - 1;
            else
                return 3 * xin + 1;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="xin"></param>
        /// <param name="yin"></param>
        /// <returns></returns>
        public static int FHailStoneNextY(int xin, int yin)
        {
            if (xin % 2 == 0 & yin % 2 == 0)
                return yin / 2;
            else if (xin % 2 == 0 & yin % 2 != 0)
                return 3 * yin - 1;
            else if (xin % 2 != 0 & yin % 2 == 0)
                return yin / 2 - 1;
            else
                return 3 * yin - 3;
        }
    }
}