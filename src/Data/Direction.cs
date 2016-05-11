using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    sealed class Direction
    {
        #region The neutral direction
        public static readonly Direction NEUTRAL = new Direction(-1, "neutral", new Point(0, 0));
        #endregion

        #region The eight directions
        /// <summary>
        /// 0 North (0, -1)
        /// </summary>
        public static readonly Direction N = new Direction(0, "N", new Point(0, -1));

        /// <summary>
        /// 1 North-East (+1,-1)
        /// </summary>
        public static readonly Direction NE = new Direction(1, "NE", new Point(+1, -1));

        /// <summary>
        /// 2 East (+1,0)
        /// </summary>
        public static readonly Direction E = new Direction(2, "E", new Point(+1, 0));

        /// <summary>
        /// 3 South-East (+1,+1)
        /// </summary>
        public static readonly Direction SE = new Direction(3, "SE", new Point(+1, +1));

        /// <summary>
        /// 4 South (0,+1)
        /// </summary>
        public static readonly Direction S = new Direction(4, "S", new Point(0, +1));

        /// <summary>
        /// 5 South-West (-1,+1)
        /// </summary>
        public static readonly Direction SW = new Direction(5, "SW", new Point(-1, +1));

        /// <summary>
        /// 6 West (-1,0)
        /// </summary>
        public static readonly Direction W = new Direction(6, "W", new Point(-1, 0));

        /// <summary>
        /// 7 North-West (-1,-1)
        /// </summary>
        public static readonly Direction NW = new Direction(7, "NW", new Point(-1,-1));
        #endregion

        #region The compass containing the directions in clockwise otherwise order, starting north.
        public static readonly Direction[] COMPASS = new Direction[8]
        {
            N, NE, E, SE, S, SW, W, NW
        };

        public static readonly List<Direction> COMPASS_LIST = new List<Direction>()
        {
            N, NE, E, SE, S, SW, W, NW
        };
        #endregion

        #region 4 directions compass
        public static readonly Direction[] COMPASS_4 = new Direction[4]
        {
            N, E, S, W
        };
        #endregion

        #region Directions helpers
        public static Direction FromVector(Point v)
        {
            foreach (Direction d in COMPASS)
                if (d.Vector == v)
                    return d;
            return null;
        }

        public static Direction FromVector(int vx, int vy)
        {
            foreach (Direction d in COMPASS)
                if (d.Vector.X == vx & d.Vector.Y == vy)
                    return d;
            return null;
        }

        public static Direction ApproximateFromVector(Point v)
        {
            // compute normalized vector.
            PointF vF = v;
            float length = (float)Math.Sqrt(vF.X * vF.X + vF.Y * vF.Y);
            if (length == 0)
                return Direction.N;
            vF.X /= length;
            vF.Y /= length;

            // find best matching direction.
            float bestError = float.MaxValue;
            Direction bestDir = Direction.N;
            foreach (Direction d in COMPASS)
            {
                float error = Math.Abs(vF.X - d.NormalizedVector.X) + Math.Abs(vF.Y - d.NormalizedVector.Y);
                if (error < bestError)
                {
                    bestDir = d;
                    bestError = error;
                }
            }

            //Console.Out.WriteLine(String.Format("v={0} vF={1} bestError={2}", v.ToString(), vF.ToString(), bestError));

            return bestDir;
        }

        public static Direction Right(Direction d)
        {
            return COMPASS[(d.m_Index + 1) % 8];
        }

        public static Direction Left(Direction d)
        {
            return COMPASS[(d.m_Index - 1) % 8];
        }

        public static Direction Opposite(Direction d)
        {
            return COMPASS[(d.m_Index + 4) % 8];
        }
        #endregion

        #region Operators
        public static Point operator +(Point lhs, Direction rhs)
        {
            return new Point(lhs.X + rhs.Vector.X, lhs.Y + rhs.Vector.Y);
        }
        #endregion

        #region Fields
        int m_Index;
        string m_Name;
        Point m_Vector;
        PointF m_NormalizedVector;
        #endregion

        #region Properties
        public int Index
        {
            get { return m_Index; }
        }

        public string Name
        {
            get { return m_Name; }
        }

        public Point Vector
        {
            get { return m_Vector; }
        }

        public PointF NormalizedVector
        {
            get { return m_NormalizedVector; }
        }
        #endregion

        #region Init
        Direction(int index, string name, Point vector)
        {
            m_Index = index;
            m_Name = name;
            m_Vector = vector;

            float length = (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
            if (length != 0)
            {
                m_NormalizedVector = new PointF(vector.X / length, vector.Y / length);
            }
            else
                m_NormalizedVector = PointF.Empty;
        }
        #endregion

        #region ToString
        public override string ToString()
        {
            return m_Name;
        }
        #endregion
    }
}

