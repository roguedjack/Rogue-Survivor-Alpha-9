using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;   // Point

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.MapObjects;
using djack.RogueSurvivor.Gameplay;

namespace djack.RogueSurvivor.Engine
{
    abstract class MapGenerator
    {
        #region Fields
        protected readonly Rules m_Rules;
        #endregion

        #region Init
        public MapGenerator(Rules rules)
        {
            if (rules == null)
                throw new ArgumentNullException("rules");

            m_Rules = rules;
        }
        #endregion

        #region Generating a new map
        public abstract Map Generate(int seed);
        #endregion

        #region Generation helpers

        #region Tile filling
        public void TileFill(Map map, TileModel model)
        {
            TileFill(map, model, null);
        }

        public void TileFill(Map map, TileModel model, Action<Tile, TileModel, int, int> decoratorFn)
        {
            TileFill(map, model, 0, 0, map.Width, map.Height, decoratorFn);
        }

        public void TileFill(Map map, TileModel model, Rectangle rect)
        {
            TileFill(map, model, rect, null);
        }

        public void TileFill(Map map, TileModel model, Rectangle rect, Action<Tile, TileModel, int, int> decoratorFn)
        {
            TileFill(map, model, rect.Left, rect.Top, rect.Width, rect.Height, decoratorFn);
        }

        public void TileFill(Map map, TileModel model, int left, int top, int width, int height)
        {
            TileFill(map, model, left, top, width, height, null);
        }

        public void TileFill(Map map, TileModel model, int left, int top, int width, int height, Action<Tile, TileModel, int, int> decoratorFn)
        {
            if (map == null)
                throw new ArgumentNullException("map");
            if (model == null)
                throw new ArgumentNullException("model");

            for (int x = left; x < left + width; x++)
                for (int y = top; y < top + height; y++)
                {
                    TileModel prevmodel = map.GetTileAt(x, y).Model;
                    map.SetTileModelAt(x, y, model);
                    if (decoratorFn != null)
                        decoratorFn(map.GetTileAt(x, y), prevmodel, x, y);
                }
        }

        public void TileHLine(Map map, TileModel model, int left, int top, int width)
        {
            TileHLine(map, model, left, top, width, null);
        }

        public void TileHLine(Map map, TileModel model, int left, int top, int width, Action<Tile, TileModel, int, int> decoratorFn)
        {
            if (map == null)
                throw new ArgumentNullException("map");
            if (model == null)
                throw new ArgumentNullException("model");

            for (int x = left; x < left + width; x++)
            {
                TileModel prevmodel = map.GetTileAt(x, top).Model;
                map.SetTileModelAt(x, top, model);
                if (decoratorFn != null)
                    decoratorFn(map.GetTileAt(x, top), prevmodel,x, top);
            }
        }

        public void TileVLine(Map map, TileModel model, int left, int top, int height)
        {
            TileVLine(map, model, left, top, height, null);
        }

        public void TileVLine(Map map, TileModel model, int left, int top, int height, Action<Tile, TileModel, int, int> decoratorFn)
        {
            if (map == null)
                throw new ArgumentNullException("map");
            if (model == null)
                throw new ArgumentNullException("model");

            for (int y = top; y < top + height; y++)
            {
                TileModel prevmodel = map.GetTileAt(left, y).Model;
                map.SetTileModelAt(left, y, model);
                if (decoratorFn != null)
                    decoratorFn(map.GetTileAt(left, y), prevmodel, left, y);
            }
        }

        public void TileRectangle(Map map, TileModel model, Rectangle rect)
        {
            TileRectangle(map, model, rect.Left, rect.Top, rect.Width, rect.Height);
        }

        public void TileRectangle(Map map, TileModel model, int left, int top, int width, int height)
        {
            TileRectangle(map, model, left, top, width, height, null);
        }

        public void TileRectangle(Map map, TileModel model, int left, int top, int width, int height, Action<Tile, TileModel, int, int> decoratorFn)
        {
            if (map == null)
                throw new ArgumentNullException("map");
            if (model == null)
                throw new ArgumentNullException("model");

            TileHLine(map, model, left, top, width, decoratorFn);
            TileHLine(map, model, left, top + height - 1, width, decoratorFn);
            TileVLine(map, model, left, top, height, decoratorFn);
            TileVLine(map, model, left + width - 1, top, height, decoratorFn);
        }

        public Point DigUntil(Map map, TileModel model, Point startPos, Direction digDirection, Predicate<Point> stopFn)
        {
            Point digPos = startPos + digDirection;

            while (map.IsInBounds(digPos) && !stopFn(digPos))
            {
                // set tile.
                map.SetTileModelAt(digPos.X, digPos.Y, model);

                // continue digging.
                digPos += digDirection;
            }

            return digPos;
        }

        public void DoForEachTile(Map map, Rectangle rect, Action<Point> doFn)
        {
            if (doFn == null)
                throw new ArgumentNullException("doFn");

            Point p = new Point();
            for (int x = rect.Left; x < rect.Right; x++)
            {
                p.X = x;
                for (int y = rect.Top; y < rect.Bottom; y++)
                {
                    p.Y = y;
                    doFn(p);
                }
            }
        }

        public bool CheckForEachTile(Map map, Rectangle rect, Predicate<Point> predFn)
        {
            if (predFn == null)
                throw new ArgumentNullException("predFn");

            Point p = new Point();
            for (int x = rect.Left; x < rect.Right; x++)
            {
                p.X = x;
                for (int y = rect.Top; y < rect.Bottom; y++)
                {
                    p.Y = y;
                    if (!predFn(p))
                        return false;
                }
            }

            return true;
        }

        public void DoForEachAdjacentInMap(Map map, Point pt, Action<Point> doFn)
        {
            if (doFn == null)
                throw new ArgumentNullException("doFn");

            Point adjPt;
            foreach (Direction d in Direction.COMPASS)
            {
                adjPt = pt + d;
                if (!map.IsInBounds(adjPt))
                    continue;
                doFn(adjPt);

            }
        }
        #endregion

        #region Placing actors
        public bool ActorPlace(DiceRoller roller, int maxTries, Map map, Actor actor)
        {
            return ActorPlace(roller, maxTries, map, actor, null);
        }

        public bool ActorPlace(DiceRoller roller, int maxTries, Map map, Actor actor, int left, int top, int width, int height)
        {
            return ActorPlace(roller, maxTries, map, actor, left, top, width, height, null);
        }

        public bool ActorPlace(DiceRoller roller, int maxTries, Map map, Actor actor, Predicate<Point> goodPositionFn)
        {
            return ActorPlace(roller, maxTries, map, actor, 0, 0, map.Width, map.Height, goodPositionFn);
        }

        public bool ActorPlace(DiceRoller roller, int maxTries, Map map, Actor actor, int left, int top, int width, int height, Predicate<Point> goodPositionFn)
        {
            if (map == null)
                throw new ArgumentNullException("map");
            if (actor == null)
                throw new ArgumentNullException("actor");

            // try <maxTries> times to find a walkable position and place the actor there.
            Point position = new Point();
            for (int i = 0; i < maxTries; i++)
            {
                position.X = roller.Roll(left, left + width);
                position.Y = roller.Roll(top, top + height);

                if (m_Rules.IsWalkableFor(actor, map, position.X, position.Y) &&
                    (goodPositionFn == null || goodPositionFn(position)))
                {
                    map.PlaceActorAt(actor, position);
                    return true;
                }
            }

            // failed.
            return false;
        }
        #endregion

        #region Map Objects
        public void MapObjectPlace(Map map, int x, int y, MapObject mapObj)
        {
            if (map.GetMapObjectAt(x, y) == null)
                map.PlaceMapObjectAt(mapObj, new Point(x, y));
        }

        public void MapObjectFill(Map map, Rectangle rect, Func<Point, MapObject> createFn)
        {
            MapObjectFill(map, rect.Left, rect.Top, rect.Width, rect.Height, createFn);
        }

        public void MapObjectFill(Map map, int left, int top, int width, int height, Func<Point, MapObject> createFn)
        {
            Point p = new Point();
            for (int x = left; x < left + width; x++)
            {
                p.X = x;
                for (int y = top; y < top + height; y++)
                {
                    p.Y = y;
                    MapObject newMapObject = createFn(p);
                    if (newMapObject != null && map.GetMapObjectAt(x, y) == null)
                    {
                        map.PlaceMapObjectAt(newMapObject, new Point(x, y));
                    }
                }
            }
        }

        public void MapObjectPlaceInGoodPosition(Map map, Rectangle rect, Func<Point, bool> isGoodPosFn, DiceRoller roller, Func<Point, MapObject> createFn)
        {
            MapObjectPlaceInGoodPosition(map, rect.Left, rect.Top, rect.Width, rect.Height, isGoodPosFn, roller, createFn);
        }

        public void MapObjectPlaceInGoodPosition(Map map, int left, int top, int width, int height, Func<Point, bool> isGoodPosFn, DiceRoller roller, Func<Point, MapObject> createFn)
        {
            // find all good positions.
            List<Point> goodList = null;

            Point p = new Point();
            for (int x = left; x < left + width; x++)
            {
                p.X = x;
                for (int y = top; y < top + height; y++)
                {
                    p.Y = y;

                    if (isGoodPosFn(p) && map.GetMapObjectAt(x, y) == null)
                    {
                        if (goodList == null)
                            goodList = new List<Point>();
                        goodList.Add(p);
                    }
                }
            }

            // pick a good position at random and put the object there.
            if (goodList == null)
                return;
            int iValid = roller.Roll(0, goodList.Count);
            MapObject mapObj = createFn(goodList[iValid]);
            if (mapObj != null)
                map.PlaceMapObjectAt(mapObj, goodList[iValid]);
        }
        #endregion

        #region Items
        public void ItemsDrop(Map map, Rectangle rect, Func<Point, bool> isGoodPositionFn, Func<Point, Item> createFn)
        {
            Point p = new Point();
            for (int x = rect.Left; x < rect.Left + rect.Width; x++)
            {
                p.X = x;
                for (int y = rect.Top; y < rect.Top + rect.Height; y++)
                {
                    p.Y = y;

                    if (isGoodPositionFn(p))
                    {
                        Item it = createFn(p);
                        if (it != null)
                            map.DropItemAt(it, p);
                    }
                }
            }
        }
        #endregion

        #region Clearing whole areas
        /// <summary>
        /// Remove all Actors, MapOjects, Items, Decorations and Zones in a rect.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="clearZones"></param>  // alpha10
        protected void ClearRectangle(Map map, Rectangle rect, bool clearZones=true)
        {
            for(int x = rect.Left; x <rect.Right;x++)
                for (int y = rect.Top; y < rect.Bottom; y++)
                {
                    map.RemoveMapObjectAt(x, y);

                    Inventory stack = map.GetItemsAt(x,y);
                    if (stack != null)
                    {
                        while (!stack.IsEmpty)
                            map.RemoveItemAt(stack[0], x, y);
                    }

                    map.GetTileAt(x, y).RemoveAllDecorations();

                    if (clearZones)
                        map.RemoveAllZonesAt(x, y);

                    Actor actorThere = map.GetActorAt(x, y);
                    if (actorThere != null)
                        map.RemoveActor(actorThere);
                }
        }
        #endregion

        #region Predicates and Actions
        /// <summary>
        /// Apply an action on each adjacent tiles within map bounds.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="doFn">action to apply at each point</param>
        public void ForEachAdjacent(Map map, int x, int y, Action<Point> doFn)
        {
            Point p = new Point(x,y);

            foreach (Direction d in Direction.COMPASS)
            {
                Point adj = p + d;
                if (!(map.IsInBounds(adj)))
                    continue;

                doFn(adj);
            }
        }

        /// <summary>
        /// Count how many adjacent tiles within map bounds match a predicate.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="checkFn">predicate to check at each point</param>
        /// <returns></returns>
        public int CountForEachAdjacent(Map map, int x, int y, Func<Point, bool> checkFn)
        {
            int count = 0;
            Point p = new Point(x, y);

            foreach (Direction d in Direction.COMPASS)
            {
                Point adj = p + d;
                if (!(map.IsInBounds(adj)))
                    continue;
                if (checkFn(adj))
                    ++count;
            }

            return count;
        }

        public int CountAdjWalls(Map map, int x, int y)
        {
            return CountForEachAdjacent(map, x, y, (pt) => !map.GetTileAt(pt.X, pt.Y).Model.IsWalkable);
        }

        public int CountAdjWalls(Map map, Point p)
        {
            return CountAdjWalls(map, p.X, p.Y);
        }

        public int CountAdjWalkables(Map map, int x, int y)
        {
            return CountForEachAdjacent(map, x, y, (pt) => map.GetTileAt(pt.X, pt.Y).Model.IsWalkable);
        }

        public int CountAdjDoors(Map map, int x, int y)
        {
            return CountForEachAdjacent(map, x, y, (pt) => map.GetMapObjectAt(pt.X, pt.Y) as DoorWindow != null);
        }

        // alpha10.1
        public int CountAdjMapObjects(Map map, int x, int y)
        {
            return CountForEachAdjacent(map, x, y, (pt) => map.GetMapObjectAt(pt.X, pt.Y) != null);
        }

        public void PlaceIf(Map map, int x, int y, TileModel floor, Func<int, int, bool> predicateFn, Func<int, int, MapObject> createFn)
        {
            if (predicateFn(x, y))
            {
                MapObject mapObj = createFn(x, y);
                if (mapObj == null)
                    return;
                map.SetTileModelAt(x, y, floor);
                MapObjectPlace(map, x, y, mapObj);
            }
        }

        public bool IsAccessible(Map map, int x, int y)
        {
            return CountForEachAdjacent(map, x, y, (pt) => map.IsWalkable(pt.X, pt.Y)) >= 6;
        }

        public bool HasNoObjectAt(Map map, int x, int y)
        {
            return map.GetMapObjectAt(x, y) == null;
        }

        public bool IsInside(Map map, int x, int y)
        {
            return map.GetTileAt(x, y).IsInside;
        }

        public bool HasInRange(Map map, Point from, int maxDistance, Predicate<Point> predFn)
        {
            int xmin, xmax;
            int ymin, ymax;
            xmin = from.X - maxDistance;
            ymin = from.Y - maxDistance;
            xmax = from.X + maxDistance;
            ymax = from.Y + maxDistance;
            map.TrimToBounds(ref xmin, ref ymin);
            map.TrimToBounds(ref xmax, ref ymax);
            Point pos = new Point();
            for (int x = xmin; x <= xmax; x++)
            {
                pos.X = x;
                for (int y = ymin; y <= ymax; y++)
                {
                    pos.Y = y;
                    if (x == from.X && y == from.Y)
                        continue;
                    if (predFn(pos))
                        return true;
                }
            }

            return false;
        }
        #endregion

        #endregion
    }
}
