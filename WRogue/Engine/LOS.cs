using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine
{
    /// <summary>
    /// Line Of Sight & Field Of View computing, line tracing and assorted utilities.
    /// </summary>
    static class LOS
    {
        #region Line tracing

#if false
        /// <summary>
        /// Ensure symetric results by ordering coordinates.
        /// </summary>
        /// <param name="maxSteps"></param>
        /// <param name="map"></param>
        /// <param name="xFrom"></param>
        /// <param name="yFrom"></param>
        /// <param name="xTo"></param>
        /// <param name="yTo"></param>
        /// <param name="line"></param>
        /// <param name="fn"></param>
        /// <returns></returns>
        public static bool SymetricBresenhamTrace(int maxSteps, Map map, int xFrom, int yFrom, int xTo, int yTo, List<Point> line, Func<int, int, bool> fn)
        {         
            ///////////////////////////////////////////////
            // Ensure symetry by arbitrary ordering points
            ///////////////////////////////////////////////
            if (xFrom + yFrom > xTo + yTo)
            {
                // swap from and to
                int swap;

                swap = xFrom;
                xFrom = xTo;
                xTo = swap;

                swap = yFrom;
                yFrom = yTo;
                yTo = swap;
            }

            /////////////////////
            // Then do bresenham
            /////////////////////
            return AsymetricBresenhamTrace(maxSteps, map, xFrom, yFrom, xTo, yTo, line, fn);
        }
#endif

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxSteps"></param>
        /// <param name="map"></param>
        /// <param name="xFrom"></param>
        /// <param name="yFrom"></param>
        /// <param name="xTo"></param>
        /// <param name="yTo"></param>
        /// <param name="line">if not null, the method adds the points forming the segment, source position included.</param>
        /// <param name="fn"></param>
        /// <returns></returns>
        public static bool AsymetricBresenhamTrace(int maxSteps, Map map, int xFrom, int yFrom, int xTo, int yTo, List<Point> line, Func<int, int, bool> fn)
        {
            // From Roguebasin
            // http://roguebasin.roguelikedevelopment.org/index.php?title=Bresenham%27s_Line_Algorithm

            int delta_x = Math.Abs(xTo - xFrom) << 1;
            int delta_y = Math.Abs(yTo - yFrom) << 1;

            // if xFrom == xTo or yFrom == yTo, then it does not matter what we set here
            int ix = xTo > xFrom ? 1 : -1;
            int iy = yTo > yFrom ? 1 : -1;

            // plot
            if (line != null)
                line.Add(new Point(xFrom, yFrom));

            int stepCount = 0;

            if (delta_x >= delta_y)
            {
                // error may go below zero
                int error = delta_y - (delta_x >> 1);

                while (xFrom != xTo)
                {
                    if (error >= 0)
                    {
                        if (error != 0 || ix > 0)
                        {
                            yFrom += iy;
                            error -= delta_x;
                        }
                        // else do nothing
                    }
                    // else do nothing

                    xFrom += ix;
                    error += delta_y;

                    if (++stepCount > maxSteps)
                        return false;

                    // plot
                    if (!fn(xFrom, yFrom))
                        return false;
                    if (line != null)
                        line.Add(new Point(xFrom, yFrom));
                }
            }
            else
            {
                // error may go below zero
                int error = delta_x - (delta_y >> 1);

                while (yFrom != yTo)
                {
                    if (error >= 0)
                    {
                        if (error != 0 || iy > 0)
                        {
                            xFrom += ix;
                            error -= delta_y;
                        }
                        // else do nothing
                    }
                    // else do nothing

                    yFrom += iy;
                    error += delta_x;

                    if (++stepCount > maxSteps)
                        return false;

                    // plot
                    if (!fn(xFrom, yFrom))
                        return false;
                    if (line != null)
                        line.Add(new Point(xFrom, yFrom));
                }
            }

            // all clear.
            return true;
        }

#if false
        public static bool SymetricBresenhamTrace(Map map, int xFrom, int yFrom, int xTo, int yTo, List<Point> line, Func<int, int, bool> fn)
        {
            return SymetricBresenhamTrace(Int32.MaxValue, map, xFrom, yFrom, xTo, yTo, line, fn);
        }
#endif

        public static bool AsymetricBresenhamTrace(Map map, int xFrom, int yFrom, int xTo, int yTo, List<Point> line, Func<int, int, bool> fn)
        {
            return AsymetricBresenhamTrace(Int32.MaxValue, map, xFrom, yFrom, xTo, yTo, line, fn);
        }

        public static Direction DirectionTo(Map map, int xFrom, int yFrom, int xTo, int yTo)
        {
            List<Point> line = new List<Point>();
            AsymetricBresenhamTrace(1, map, xFrom, yFrom, xTo, yTo, line,
                (x, y) => true);

            return Direction.FromVector(line[0]);
        }

        public static bool CanTraceViewLine(Location fromLocation, Point toPosition, int maxRange)
        {
            Map map = fromLocation.Map;
            Point goal = toPosition;

            return AsymetricBresenhamTrace(maxRange,
                map,
                fromLocation.Position.X, fromLocation.Position.Y,
                toPosition.X, toPosition.Y,
                null,
                (x, y) =>
                {
                    if (map.IsTransparent(x, y)) return true;
                    if (x == goal.X && y == goal.Y) return true;
                    return false;
                });
        }

        public static bool CanTraceViewLine(Location fromLocation, Point toPosition)
        {
            return CanTraceViewLine(fromLocation, toPosition, Int32.MaxValue);
        }

        /// <summary>
        /// Checks if can fire from a position to another.
        /// </summary>
        /// <param name="fromLocation"></param>
        /// <param name="toPosition"></param>
        /// <param name="maxRange"></param>
        /// <param name="line">if not null will contains the entire fire line, even if obstructed.</param>
        /// <returns>true if fire line is clear.</returns>
        public static bool CanTraceFireLine(Location fromLocation, Point toPosition, int maxRange, List<Point> line)
        {
            Map map = fromLocation.Map;
            Point start = fromLocation.Position;
            Point goal = toPosition;
            bool fireLineClear = true;

            AsymetricBresenhamTrace(maxRange,
                map,
                fromLocation.Position.X, fromLocation.Position.Y,
                toPosition.X, toPosition.Y,
                line,
                (x, y) =>
                {
                    if (x == start.X && y == start.Y) return true;
                    if (x == goal.X && y == goal.Y) return true;
                    if (map.IsBlockingFire(x, y)) fireLineClear = false;
                    return true;
                });

            return fireLineClear;
        }

        public static bool CanTraceThrowLine(Location fromLocation, Point toPosition, int maxRange, List<Point> line)
        {
            Map map = fromLocation.Map;
            Point start = fromLocation.Position;
            Point goal = toPosition;
            bool throwLineClear = true;

            // check line.
            AsymetricBresenhamTrace(maxRange,
                map,
                fromLocation.Position.X, fromLocation.Position.Y,
                toPosition.X, toPosition.Y,
                line,
                (x, y) =>
                {
                    if (x == start.X && y == start.Y) return true;
                    if (x == goal.X && y == goal.Y) return true;
                    if (map.IsBlockingThrow(x, y)) throwLineClear = false;
                    return true;
                });

            // we can't throw on something blocking, no matter the rest of the line is clear.
            if (map.IsBlockingThrow(toPosition.X, toPosition.Y))
                throwLineClear = false;

            // done.
            return throwLineClear;
        }
        #endregion

        #region Computing FOV
        static bool FOVSub(Location fromLocation, Point toPosition, int maxRange, ref HashSet<Point> visibleSet)
        {
#if false
            return CanTraceViewLine(fromLocation, toPosition, maxRange);
#endif

            // Asymetric bresenham : use the fact we are tracing FROM to TO to addvisible tiles on the fly.
            // Pros: fixed "holes" in fov : if you can see a tile you can see everything in its line too.
            // Cons: rare cases of asymetry in FOV : i can see you, but you can't see me.
            Map map = fromLocation.Map;
            HashSet<Point> visibleSetRef = visibleSet;  // necessary to have a local variable in lambda call.
            Point goal = toPosition;

            return AsymetricBresenhamTrace(maxRange,
                map,
                fromLocation.Position.X, fromLocation.Position.Y,
                toPosition.X, toPosition.Y,
                null,
                (x, y) =>
                {
                    bool viewThrough =
                        (x == goal.X && y == goal.Y) ? true :
                        map.IsTransparent(x, y) ? true :
                        false;

                    if (viewThrough)
                        visibleSetRef.Add(new Point(x, y));

                    return viewThrough;
                });
        }

        public static HashSet<Point> ComputeFOVFor(Rules rules, Actor actor, WorldTime time, Weather weather)
        {
            Location fromLocation = actor.Location;
            HashSet<Point> visibleSet = new HashSet<Point>();
            Point from = fromLocation.Position;
            Map map = fromLocation.Map;
            int maxRange = rules.ActorFOV(actor, time, weather);

            //////////////////////////////////////////////
            // Brute force ray-casting with wall fix pass
            //////////////////////////////////////////////
            int xmin = from.X - maxRange;
            int xmax = from.X + maxRange;
            int ymin = from.Y - maxRange;
            int ymax = from.Y + maxRange;
            map.TrimToBounds(ref xmin, ref ymin);
            map.TrimToBounds(ref xmax, ref ymax);
            Point to = new Point();
            List<Point> wallsToFix = new List<Point>();

            // 1st pass : trace line and remember walls that are not visible for 2nd pass.
            for (int x = xmin; x <= xmax; x++)
            {
                to.X = x;
                for (int y = ymin; y <= ymax; y++)
                {
                    to.Y = y;

                    // Distance check.
                    if (rules.LOSDistance(from, to) > maxRange)
                        continue;

                    // If we already know tile is visible, pass.
                    if (visibleSet.Contains(to))
                        continue;

                    // Trace line.
                    if(!FOVSub(fromLocation, to, maxRange, ref visibleSet))
                    {                        
                        // if its a wall (in FoV terms), remember.
                        bool isFovWall = false;
                        Tile tile = map.GetTileAt(x, y);
                        MapObject mapObj = map.GetMapObjectAt(x, y);
                        if (!tile.Model.IsTransparent && !tile.Model.IsWalkable)
                            isFovWall = true;
                        else if (mapObj != null)
                            isFovWall = true;                           
                        if(isFovWall)
                            wallsToFix.Add(to);

                        // next.
                        continue;
                    }

                    // Visible.
                    visibleSet.Add(to);
                }
            }

            // 2nd pass : wall fix.
            List<Point> fixedWalls = new List<Point>(wallsToFix.Count);
            foreach (Point wallP in wallsToFix)
            {
                int count = 0;
                foreach (Direction d in Direction.COMPASS)
                {
                    Point next = wallP + d;
                    if (visibleSet.Contains(next))
                    {
                        Tile tile = map.GetTileAt(next.X, next.Y);
                        if (tile.Model.IsTransparent && tile.Model.IsWalkable)
                            ++count;
                    }
                }
                if (count >= 3)
                    fixedWalls.Add(wallP);
            }
            foreach (Point fixedWall in fixedWalls)
            {
                visibleSet.Add(fixedWall);
            }

            return visibleSet;
        }
        #endregion
    }
}
