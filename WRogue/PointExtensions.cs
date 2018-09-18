using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace djack.RogueSurvivor
{
    public static class PointExtensions
    {
        public static Point Add(this Point pt, int x, int y)
        {
            return new Point(pt.X + x, pt.Y + y);
        }

        public static Point Add(this Point pt, Point other)
        {
            return new Point(pt.X + other.X, pt.Y + other.Y);
        }
    }
}
