using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blend
{
    public static class ColorHelper
    {
        // <summary>
        /// Converts HSB to RGB.
        /// </summary>
        public static RGB HSBtoRGB(double h, double s, double br)
        {
            double r = 0;
            double g = 0;
            double b = 0;

            if (s == 0)
            {
                r = g = b = br;
            }
            else
            {
                // the color wheel consists of 6 sectors. Figure out which sector
                // you're in.
                double sectorPos = h / 60.0;
                int sectorNumber = (int)(Math.Floor(sectorPos));
                // get the fractional part of the sector
                double fractionalSector = sectorPos - sectorNumber;

                // calculate values for the three axes of the color.
                double p = br * (1.0 - s);
                double q = br * (1.0 - (s * fractionalSector));
                double t = br * (1.0 - (s * (1 - fractionalSector)));

                // assign the fractional colors to r, g, and b based on the sector
                // the angle is in.
                switch (sectorNumber)
                {
                    case 0:
                        r = br;
                        g = t;
                        b = p;
                        break;
                    case 1:
                        r = q;
                        g = br;
                        b = p;
                        break;
                    case 2:
                        r = p;
                        g = br;
                        b = t;
                        break;
                    case 3:
                        r = p;
                        g = q;
                        b = br;
                        break;
                    case 4:
                        r = t;
                        g = p;
                        b = br;
                        break;
                    case 5:
                        r = br;
                        g = p;
                        b = q;
                        break;
                }
            }

            return new RGB(
                Convert.ToInt32(Double.Parse(String.Format("{0:0.00}", r * 255.0))),
                Convert.ToInt32(Double.Parse(String.Format("{0:0.00}", g * 255.0))),
                Convert.ToInt32(Double.Parse(String.Format("{0:0.00}", b * 255.0)))
            );
        }

        /// <summary>
        /// Converts RGB to HSB.
        /// </summary>
        public static HSB RGBtoHSB(int red, int green, int blue)
        {
            // normalize red, green and blue values
            double r = ((double)red / 255.0);
            double g = ((double)green / 255.0);
            double b = ((double)blue / 255.0);

            // conversion start
            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));

            double h = 0.0;
            if (max == r && g >= b)
            {
                h = 60 * (g - b) / (max - min);
            }
            else if (max == r && g < b)
            {
                h = 60 * (g - b) / (max - min) + 360;
            }
            else if (max == g)
            {
                h = 60 * (b - r) / (max - min) + 120;
            }
            else if (max == b)
            {
                h = 60 * (r - g) / (max - min) + 240;
            }
            if (double.IsNaN(h))
                h = 0.0;
            double s = (max == 0) ? 0.0 : (1.0 - (min / max));

            if (Double.IsNaN(s) || Double.IsNaN(max))
                h = 0.5;

            return new HSB(h, s, (double)max);
        }
    }
}
