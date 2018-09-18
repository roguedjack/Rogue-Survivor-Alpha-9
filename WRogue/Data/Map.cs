using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.Serialization;
using System.Data;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    class Exit
    {
        public Map ToMap { get; set; }
        public Point ToPosition { get; set; }

        /// <summary>
        /// Can be used by AIs.
        /// </summary>
        public bool IsAnAIExit { get; set; }

        public Exit(Map toMap, Point toPosition)
        {
            this.ToMap = toMap;
            this.ToPosition = toPosition;
        }
    }

    [Serializable]
    enum Lighting
    {
        _FIRST,

        DARKNESS = _FIRST,
        OUTSIDE,
        LIT,

        _LAST,
    }

    [Serializable]
    class Map : ISerializable
    {        
        #region Constants
        public const int GROUND_INVENTORY_SLOTS = 10;
        #endregion

        #region Fields

        #region Primary data : Serialize.
        int m_Seed;
        District m_District;
        string m_Name;
        string m_BgMusic;  // alpha10
        Lighting m_Lighting;
        WorldTime m_LocalTime;

        int m_Width;
        int m_Height;
        Rectangle m_Rectangle;

        Tile[,] m_Tiles;

        Dictionary<Point, Exit> m_Exits;

        List<Zone> m_Zones;

        List<Actor> m_ActorsList;
        int m_iCheckNextActorIndex;
        List<MapObject> m_MapObjectsList;
        Dictionary<Point, Inventory> m_GroundItemsByPosition;

        List<Corpse> m_CorpsesList;

        /// <summary>
        /// All scents, there is only one scent by odor possible at each location : identical odors combine their strength.
        /// </summary>
        List<OdorScent> m_Scents;

        List<TimedTask> m_Timers;

        #endregion

        #region Auxiliary data : Don't serialize, Reconstruct at load time AFTER the Session has been deserialized.
        [NonSerialized]
        Dictionary<Point, Actor> m_aux_ActorsByPosition;

        [NonSerialized]
        Dictionary<Point, MapObject> m_aux_MapObjectsByPosition;

        [NonSerialized]
        List<Inventory> m_aux_GroundItemsList;

        [NonSerialized]
        Dictionary<Point, List<Corpse>> m_aux_CorpsesByPosition;

        // scent hash
        [NonSerialized]
        Dictionary<Point, List<OdorScent>> m_aux_ScentsByPosition;
        #endregion

        #endregion

        #region Properties
        public District District
        {
            get { return m_District; }
            set { m_District = value; }
        }

        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public int Seed
        {
            get { return m_Seed; }
        }

        public string BgMusic // alpha10
        {
            get { return m_BgMusic; }
            set { m_BgMusic = value; }
        }

        public bool IsSecret
        {
            get;
            set;
        }

        public Lighting Lighting
        {
            get { return m_Lighting; }
            set { m_Lighting = value; }
        }

        public WorldTime LocalTime
        {
            get { return m_LocalTime; }
        }

        public int Width
        {
            get { return m_Width; }
        }

        public int Height
        {
            get { return m_Height; }
        }

        public Rectangle Rect
        {
            get { return m_Rectangle; }
        }

        public IEnumerable<Zone> Zones
        {
            get { return m_Zones; }
        }

        public IEnumerable<Exit> Exits
        {
            get { return m_Exits.Values; }
        }

        public int CountExits
        {
            get { return m_Exits.Values == null ? 0 : m_Exits.Values.Count; }
        }

        public IEnumerable<Actor> Actors
        {
            get { return m_ActorsList; }
        }

        public int CountActors
        {
            get { return m_ActorsList.Count; }
        }

        /// <summary>
        /// For optimization of actors ordering in rules.
        /// Do not use for anything else.
        /// </summary>
        public int CheckNextActorIndex
        {
            get { return m_iCheckNextActorIndex; }
            set { m_iCheckNextActorIndex = value; }
        }

        public IEnumerable<MapObject> MapObjects
        {
            get { return m_MapObjectsList; }
        }

        public IEnumerable<Inventory> GroundInventories
        {
            get { return m_aux_GroundItemsList; }
        }

        public IEnumerable<Corpse> Corpses
        {
            get { return m_CorpsesList; }
        }

        public int CountCorpses
        {
            get { return m_CorpsesList.Count; }
        }

        public IEnumerable<TimedTask> Timers
        {
            get { return m_Timers; }
        }

        public int CountTimers
        {
            get { return (m_Timers==null ? 0 : m_Timers.Count); }
        }

        public IEnumerable<OdorScent> Scents
        {
            get { return m_Scents; }
        }

        #endregion

        #region Init
        public Map(int seed, string name, int width, int height)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width <=0");
            if (height <= 0)
                throw new ArgumentOutOfRangeException("height <=0");

            m_Seed = seed;
            m_Name = name;
            m_Width = width;
            m_Height = height;
            m_Rectangle = new Rectangle(0, 0, width, height);
            m_LocalTime = new WorldTime();
            this.Lighting = Lighting.OUTSIDE;
            this.IsSecret = false;

            m_Tiles = new Tile[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    m_Tiles[x, y] = new Tile(TileModel.UNDEF);

            m_Exits = new Dictionary<Point, Exit>();

            m_Zones = new List<Zone>(5);

            m_aux_ActorsByPosition = new Dictionary<Point, Actor>(5);
            m_ActorsList = new List<Actor>(5);

            m_aux_MapObjectsByPosition = new Dictionary<Point, MapObject>(5);
            m_MapObjectsList = new List<MapObject>(5);

            m_GroundItemsByPosition = new Dictionary<Point, Inventory>(5);
            m_aux_GroundItemsList = new List<Inventory>(5);

            m_CorpsesList = new List<Corpse>(5);
            m_aux_CorpsesByPosition = new Dictionary<Point, List<Corpse>>(5);

            m_Scents = new List<OdorScent>(128);
            m_aux_ScentsByPosition = new Dictionary<Point, List<OdorScent>>(128);

            m_Timers = new List<TimedTask>(5);
        }
        #endregion

        #region Bounds
        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < m_Width && y >= 0 && y < m_Height;
        }

        public bool IsInBounds(Point p)
        {
            return IsInBounds(p.X, p.Y);
        }

        public void TrimToBounds(ref int x, ref int y)
        {
            if (x < 0) x = 0;
            else if (x > m_Width - 1) x = m_Width - 1;

            if (y < 0) y = 0;
            else if (y > m_Height - 1) y = m_Height - 1;
        }
        
        public void TrimToBounds(ref Point p)
        {
            if (p.X < 0) p.X = 0;
            else if (p.X > m_Width - 1) p.X = m_Width - 1;

            if (p.Y < 0) p.Y = 0;
            else if (p.Y > m_Height - 1) p.Y = m_Height - 1;
        }
        
        public bool IsMapBoundary(int x, int y)
        {
            return x == -1 || x == m_Width || y == -1 || y == m_Height;
        }

        public bool IsOnMapBorder(int x, int y)
        {
            return x == 0 || x == m_Width - 1 || y == 0 || y == m_Height - 1;
        }
        #endregion

        #region Tiles
        public Tile GetTileAt(int x, int y)
        {
            return m_Tiles[x, y];
        }

        public Tile GetTileAt(Point p)
        {
            return m_Tiles[p.X, p.Y];
        }

        public void SetTileModelAt(int x, int y, TileModel model)
        {
            if (!IsInBounds(x, y))
                throw new ArgumentOutOfRangeException("position out of map bounds");
            if (model == null)
                throw new ArgumentNullException("model");

            m_Tiles[x, y].Model = model;
        }
        #endregion

        #region Exits
        public Exit GetExitAt(Point pos)
        {
            Exit exit;
            if (m_Exits.TryGetValue(pos, out exit))
                return exit;
            return null;
        }

        public Exit GetExitAt(int x, int y)
        {
            return GetExitAt(new Point(x, y));
        }

        public void SetExitAt(Point pos, Exit exit)
        {
            m_Exits.Add(pos, exit);
        }

        public void RemoveExitAt(Point pos)
        {
            m_Exits.Remove(pos);
        }

        public bool HasAnExitIn(Rectangle rect)
        {
            for (int x = rect.Left; x < rect.Right; x++)
                for (int y = rect.Top; y < rect.Bottom; y++)
                    if (GetExitAt(x, y) != null)
                        return true;
            return false;
        }

        public Point? GetExitPos(Exit exit)
        {
            if (exit == null) return null;
            foreach (KeyValuePair<Point, Exit> pairs in m_Exits)
            {
                if (pairs.Value == exit)
                    return pairs.Key;
            }
            return null;
        }
        #endregion

        #region Zones
        public void AddZone(Zone zone)
        {
            m_Zones.Add(zone);            
        }

        public void RemoveZone(Zone zone)
        {
            m_Zones.Remove(zone);
        }

        public void RemoveAllZonesAt(int x, int y)
        {
            List<Zone> zones = GetZonesAt(x, y);
            if (zones == null)
                return;
            foreach (Zone z in zones)
                RemoveZone(z);
        }

        public List<Zone> GetZonesAt(int x, int y)
        {
            List<Zone> list = null;

            foreach(Zone zone in m_Zones)
                if (zone.Bounds.Contains(x, y))
                {
                    if (list == null)
                        list = new List<Zone>(m_Zones.Count / 4);
                    list.Add(zone);
                }

            return list;
        }

        public Zone GetZoneByName(string name)
        {
            foreach (Zone zone in m_Zones)
                if (zone.Name == name)
                    return zone;
            return null;
        }

        public Zone GetZoneByPartialName(string partOfname)
        {
            foreach (Zone zone in m_Zones)
                if (zone.Name.Contains(partOfname))
                    return zone;
            return null;
        }

        public bool HasZonePartiallyNamedAt(Point pos, string partOfName)
        {
            List<Zone> zones = GetZonesAt(pos.X, pos.Y);
            if (zones == null)
                return false;

            foreach (Zone z in zones)
                if (z.Name.Contains(partOfName))
                    return true;

            return false;
        }
        #endregion

        #region Actors
        public bool HasActor(Actor actor)
        {
            return m_ActorsList.Contains(actor);
        }

        public Actor GetActor(int index)
        {
            return m_ActorsList[index];
        }

        public Actor GetActorAt(Point position)
        {
            Actor a;
            if (m_aux_ActorsByPosition.TryGetValue(position, out a))
                return a;
            return null;
        }

        public Actor GetActorAt(int x, int y)
        {
            return GetActorAt(new Point(x, y));
        }

        /// <summary>
        /// Add or move an actor to a position.
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="position"></param>
        public void PlaceActorAt(Actor actor, Point position)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            Actor other = GetActorAt(position);
            if (other == actor)
                throw new InvalidOperationException("actor already at position");
            if (other != null)
                throw new InvalidOperationException("another actor already at position");
            if (!IsInBounds(position.X, position.Y))
                throw new ArgumentOutOfRangeException("position out of map bounds");

            if (HasActor(actor))
            {
                m_aux_ActorsByPosition.Remove(actor.Location.Position);
            }
            else
            {
                m_ActorsList.Add(actor);
            }
            m_aux_ActorsByPosition.Add(position, actor);
            actor.Location = new Location(this, position);

            // reset check actor index, as it is now invalidated.
            m_iCheckNextActorIndex = 0;
        }

        public void MoveActorToFirstPosition(Actor actor)
        {
            if (!m_ActorsList.Contains(actor))
                throw new ArgumentException("actor not in map");

            m_ActorsList.Remove(actor);
            if (m_ActorsList.Count == 0)
                m_ActorsList.Add(actor);
            else
                m_ActorsList.Insert(0, actor);

            // reset check actor index, as it is now invalidated.
            m_iCheckNextActorIndex = 0;
        }

        public void RemoveActor(Actor actor)
        {
            if (!m_ActorsList.Contains(actor))
                return;

            m_ActorsList.Remove(actor);
            m_aux_ActorsByPosition.Remove(actor.Location.Position);

            // reset check actor index, as it is now invalidated.
            m_iCheckNextActorIndex = 0;
        }
        #endregion

        #region Map Objects
        public bool HasMapObject(MapObject mapObj)
        {
            return m_MapObjectsList.Contains(mapObj);
        }

        public MapObject GetMapObjectAt(Point position)
        {
            MapObject o;
            if (m_aux_MapObjectsByPosition.TryGetValue(position, out o))
                return o;
            return null;
        }

        public MapObject GetMapObjectAt(int x, int y)
        {
            return GetMapObjectAt(new Point(x, y));
        }

        public void PlaceMapObjectAt(MapObject mapObj, Point position)
        {
            if (mapObj == null)
                throw new ArgumentNullException("actor");
            MapObject other = GetMapObjectAt(position);
            if (other == mapObj)
                return;
            if (other == mapObj)
                throw new InvalidOperationException("mapObject already at position");
            if (other != null)
                throw new InvalidOperationException("another mapObject already at position");
            if (!IsInBounds(position.X, position.Y))
                throw new ArgumentOutOfRangeException("position out of map bounds");
            if (!GetTileAt(position.X, position.Y).Model.IsWalkable)
                throw new InvalidOperationException("cannot place map objects on unwalkable tiles");

            if (HasMapObject(mapObj))
            {
                m_aux_MapObjectsByPosition.Remove(mapObj.Location.Position);
            }
            else
            {
                m_MapObjectsList.Add(mapObj);
            }
            m_aux_MapObjectsByPosition.Add(position, mapObj);
            mapObj.Location = new Location(this, position);
        }

        public void RemoveMapObjectAt(int x, int y)
        {
            MapObject o = GetMapObjectAt(x, y);
            if (o == null)
                return;

            m_MapObjectsList.Remove(o);
            m_aux_MapObjectsByPosition.Remove(new Point(x, y));
        }
        #endregion

        #region Items
        public Inventory GetItemsAt(Point position)
        {
            if (!IsInBounds(position))
                return null;

            Inventory inv;
            if (m_GroundItemsByPosition.TryGetValue(position, out inv))
                return inv;
            return null;
        }

        public Inventory GetItemsAt(int x, int y)
        {
            return GetItemsAt(new Point(x, y));
        }

        public Point? GetGroundInventoryPosition(Inventory groundInv)
        {
            foreach (KeyValuePair<Point,Inventory> pair in m_GroundItemsByPosition)
            {
                if (pair.Value == groundInv)
                    return pair.Key;
            }

            return null;
        }

        public void DropItemAt(Item it, Point position)
        {
            if (it == null)
                throw new ArgumentNullException("item");
            if (!IsInBounds(position))
                throw new ArgumentOutOfRangeException("position out of map bounds");

            Inventory invThere = GetItemsAt(position);
            if (invThere == null)
            {
                invThere = new Inventory(GROUND_INVENTORY_SLOTS);
                m_aux_GroundItemsList.Add(invThere);
                m_GroundItemsByPosition.Add(position, invThere);
                invThere.AddAll(it);
            }
            else
            {
                if (invThere.IsFull)
                {
                    // ground is full, first try to stack as much as possible then if necessary remove oldest item.

                    int prevQuantity = it.Quantity;
                    int quantityAdded;
                    invThere.AddAsMuchAsPossible(it, out quantityAdded);

                    if (quantityAdded < prevQuantity)
                    {
                        // could not stack everything, remove oldest ground item and add the remaining quantity.
                        invThere.RemoveAllQuantity(invThere.BottomItem);
                        invThere.AddAsMuchAsPossible(it, out quantityAdded);
                    }
                }
                else
                {
                    invThere.AddAll(it);
                }
            }

            
        }

        public void DropItemAt(Item it, int x, int y)
        {
            DropItemAt(it, new Point(x, y));
        }

        public void RemoveItemAt(Item it, Point position)
        {
            if (it == null)
                throw new ArgumentNullException("item");
            if (!IsInBounds(position))
                throw new ArgumentOutOfRangeException("position out of map bounds");

            Inventory invThere = GetItemsAt(position);
            if (invThere == null)
                throw new ArgumentException("no items at this position");
            if(!invThere.Contains(it))
                throw new ArgumentException("item not at this position");

            invThere.RemoveAllQuantity(it);

            if (invThere.IsEmpty)            
            {
                m_GroundItemsByPosition.Remove(position);
                m_aux_GroundItemsList.Remove(invThere);
                invThere = null;
            }
        }

        public void RemoveItemAt(Item it, int x, int y)
        {
            RemoveItemAt(it, new Point(x, y));
        }

        public void RemoveAllItemsAt(Point position)
        {
            Inventory invThere = GetItemsAt(position);
            if (invThere == null) return;
            m_GroundItemsByPosition.Remove(position);
            m_aux_GroundItemsList.Remove(invThere);
        }
        #endregion

        #region Corpses
        public List<Corpse> GetCorpsesAt(Point p)
        {
            List<Corpse> listHere;
            if (m_aux_CorpsesByPosition.TryGetValue(p, out listHere))
                return listHere;
            return null;
        }

        public List<Corpse> GetCorpsesAt(int x, int y)
        {
            return GetCorpsesAt(new Point(x, y));
        }

        public bool HasCorpse(Corpse c)
        {
            return m_CorpsesList.Contains(c);
        }

        public void AddCorpseAt(Corpse c, Point p)
        {
            if (m_CorpsesList.Contains(c))
                throw new ArgumentException("corpse already in this map");

            c.Position = p;
            m_CorpsesList.Add(c);
            InsertCorpseAtPos(c);

            // make sure the dead actor follows!
            c.DeadGuy.Location = new Location(this, p);
        }

        public void MoveCorpseTo(Corpse c, Point newPos)
        {
            if (!m_CorpsesList.Contains(c))
                throw new ArgumentException("corpse not in this map");

            RemoveCorpseFromPos(c);
            c.Position = newPos;
            InsertCorpseAtPos(c);

            // make sure the dead actor follows!
            c.DeadGuy.Location = new Location(this, newPos);
        }

        public void RemoveCorpse(Corpse c)
        {
            if (!m_CorpsesList.Contains(c))
                throw new ArgumentException("corpse not in this map");

            m_CorpsesList.Remove(c);
            RemoveCorpseFromPos(c);
        }

        public bool TryRemoveCorpseOf(Actor a)
        {
            foreach (Corpse c in m_CorpsesList)
            {
                if (c.DeadGuy == a)
                {
                    RemoveCorpse(c);
                    return true;
                }
            }
            return false;
        }

        private void RemoveCorpseFromPos(Corpse c)
        {
            List<Corpse> list;
            if (m_aux_CorpsesByPosition.TryGetValue(c.Position, out list))
            {
                list.Remove(c);
                if (list.Count == 0)
                    m_aux_CorpsesByPosition.Remove(c.Position);
            }
        }

        private void InsertCorpseAtPos(Corpse c)
        {
            List<Corpse> list;
            if (m_aux_CorpsesByPosition.TryGetValue(c.Position, out list))
                list.Insert(0, c);
            else
                m_aux_CorpsesByPosition.Add(c.Position, new List<Corpse>(1) { c });
        }
        #endregion

        #region Timers
        public void AddTimer(TimedTask t)
        {
            if (m_Timers == null) m_Timers = new List<TimedTask>(5);
            m_Timers.Add(t);
        }

        public void RemoveTimer(TimedTask t)
        {
            m_Timers.Remove(t);
        }
        #endregion

        #region Odors
        public int GetScentByOdorAt(Odor odor, Point position)
        {
            if (!IsInBounds(position))
                return 0;

            OdorScent scent = GetScentByOdor(odor, position);

            return scent == null ? 0 : scent.Strength;
        }

        OdorScent GetScentByOdor(Odor odor, Point p)
        {
            List<OdorScent> scentsThere;
            if (m_aux_ScentsByPosition.TryGetValue(p, out scentsThere))
            {
                foreach (OdorScent scent in scentsThere)
                    if (scent.Odor == odor)
                        return scent;
                return null;
            }
            else
                return null;
        }

        void AddNewScent(OdorScent scent)
        {
            // list
            if (!m_Scents.Contains(scent))
                m_Scents.Add(scent);

            // hash
            List<OdorScent> scentsThere;
            if (m_aux_ScentsByPosition.TryGetValue(scent.Position, out scentsThere))
            {
                scentsThere.Add(scent);
            }
            else
            {
                scentsThere = new List<OdorScent>(2);
                scentsThere.Add(scent);
                m_aux_ScentsByPosition.Add(scent.Position, scentsThere);
            }
        }

        /// <summary>
        /// Adds or merge a scent.
        /// </summary>
        /// <param name="odor"></param>
        /// <param name="strengthChange"></param>
        /// <param name="position"></param>
        public void ModifyScentAt(Odor odor, int strengthChange, Point position)
        {
            if (!IsInBounds(position))
                throw new ArgumentOutOfRangeException("position");

            OdorScent oldScent = GetScentByOdor(odor, position);
            if (oldScent == null)
            {
                // new odor there.
                OdorScent newScent = new OdorScent(odor, strengthChange, position);
                AddNewScent(newScent);
            }
            else
            {
                // existing odor here.
                oldScent.Change(strengthChange);
            }

        }

        /// <summary>
        /// Sets odor strength to new strength if is it stronger (more "fresh").
        /// </summary>
        /// <param name="odor"></param>
        /// <param name="freshStrength"></param>
        /// <param name="position"></param>
        public void RefreshScentAt(Odor odor, int freshStrength, Point position)
        {
            if (!IsInBounds(position))
                throw new ArgumentOutOfRangeException(String.Format("position; ({0},{1}) map {2} odor {3}", position.X, position.Y, this.m_Name, odor.ToString()));

            OdorScent oldScent = GetScentByOdor(odor, position);
            if (oldScent == null)
            {
                // new odor there.
                OdorScent newScent = new OdorScent(odor, freshStrength, position);
                AddNewScent(newScent);
            }
            else
            {
                // existing odor here.
                if(oldScent.Strength < freshStrength)
                    oldScent.Set(freshStrength);
            }
        }

        public void RemoveScent(OdorScent scent)
        {
            // list.
            m_Scents.Remove(scent);

            // hash.
            List<OdorScent> scentsThere;
            if (m_aux_ScentsByPosition.TryGetValue(scent.Position, out scentsThere))
            {
                scentsThere.Remove(scent);
                if (scentsThere.Count == 0)
                {
                    m_aux_ScentsByPosition.Remove(scent.Position);
                }
            }
        }
        #endregion

        #region View & Visit
        /// <summary>
        /// Set all tiles property visible to false.
        /// </summary>
        public void ClearView()
        {
            for (int x = 0; x < m_Width; x++)
                for (int y = 0; y < m_Height; y++)
                    m_Tiles[x, y].IsInView = false;
        }

        public void SetView(IEnumerable<Point> visiblePositions)
        {
            ClearView();
            foreach (Point pt in visiblePositions)
            {
                if (!IsInBounds(pt.X, pt.Y))
                    throw new ArgumentOutOfRangeException("point " + pt + " not in map bounds");
                m_Tiles[pt.X, pt.Y].IsInView = true;
            }
        }

        public void MarkAsVisited(IEnumerable<Point> positions)
        {
            foreach (Point pt in positions)
            {
                if (!IsInBounds(pt.X, pt.Y))
                    throw new ArgumentOutOfRangeException("point " + pt + " not in map bounds");
                m_Tiles[pt.X, pt.Y].IsVisited = true;
            }
        }

        public void SetViewAndMarkVisited(IEnumerable<Point> visiblePositions)
        {
            ClearView();
            foreach (Point pt in visiblePositions)
            {
                if (!IsInBounds(pt.X, pt.Y))
                    throw new ArgumentOutOfRangeException("point " + pt + " not in map bounds");
                m_Tiles[pt.X, pt.Y].IsInView = true;
                m_Tiles[pt.X, pt.Y].IsVisited = true;
            }
        }

        /// <summary>
        /// Sets all the tiles as visited, usefull cheat for debugging.
        /// </summary>
        public void SetAllAsVisited()
        {
            for (int x = 0; x < m_Width; x++)
                for (int y = 0; y < m_Height; y++)
                    m_Tiles[x, y].IsVisited = true;
        }

        public void SetAllAsUnvisited()
        {
            for (int x = 0; x < m_Width; x++)
                for (int y = 0; y < m_Height; y++)
                    m_Tiles[x, y].IsVisited = false;
        }
        #endregion

        #region Helpers for transparency, walkable && fire line.
        /// <summary>
        /// Helper that checks for tile transparency : check tile model and check map object.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool IsTransparent(int x, int y)
        {
            if(!IsInBounds(x,y))
                return false;

            if (!m_Tiles[x, y].Model.IsTransparent)
                return false;

            MapObject mapObj = GetMapObjectAt(x, y);
            if (mapObj == null)
                return true;
            return mapObj.IsTransparent;
        }

        /// <summary>
        /// Helper that checks if tile is walkable : check tile model and check map object.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool IsWalkable(int x, int y)
        {
            if (!IsInBounds(x, y))
                return false;

            if (!m_Tiles[x, y].Model.IsWalkable)
                return false;

            MapObject mapObj = GetMapObjectAt(x, y);
            if (mapObj == null)
                return true;
            return mapObj.IsWalkable;
        }

        public bool IsWalkable(Point p)
        {
            return IsWalkable(p.X, p.Y);
        }

        /// <summary>
        /// Helper that checks if tile is blocking fire. Checks for tile model, map object and actor.
        /// Blocked by:
        /// - Non transparent tile model (eg: wall).
        /// - Non transparent map object.
        /// - Any actor.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool IsBlockingFire(int x, int y)
        {
            if (!IsInBounds(x, y))
                return true;

            // - Non transparent tile model (eg: wall)
            if (!m_Tiles[x, y].Model.IsTransparent)
                return true;

            // - Non transparent map object.
            MapObject mapObj = GetMapObjectAt(x, y);
            if (mapObj != null && !mapObj.IsTransparent)
                return true;

            // - Any actor.
            Actor actor = GetActorAt(x, y);
            if (actor != null)
                return true;

            // all clear.
            return false;
        }

        /// <summary>
        /// Helper that checks if tile is blocking a throw.
        /// Blocked by :
        /// - Unwalkable tiles (eg: walls).
        /// - Blocking objects : not walkable (eg:closed doors, shelves...) and not jumpables (eg: burning cars...)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool IsBlockingThrow(int x, int y)
        {
            if (!IsInBounds(x, y))
                return true;

            // - Unwalkable tiles (eg: walls).
            if (!m_Tiles[x, y].Model.IsWalkable)
                return true;

            // - Blocking objects : not walkable (eg:closed doors, shelves...) and not jumpables (eg: burning cars...)
            MapObject mapObj = GetMapObjectAt(x, y);
            if (mapObj != null)
            {
                if (!mapObj.IsWalkable && !mapObj.IsJumpable)
                    return true;
            }

            // all clear.
            return false;
        }
        #endregion

        #region Predicates helpers
        /// <summary>
        /// Makes a list of all adjacent positions in map matching a predicate.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="predicateFn"></param>
        /// <returns>null if no match</returns>
        public List<Point> FilterAdjacentInMap(Point position, Predicate<Point> predicateFn)
        {
            if(!IsInBounds(position))
                return null;

            List<Point> list = null;
            Point next;
            foreach (Direction d in Direction.COMPASS)
            {
                next = position + d;

                if (IsInBounds(next) && predicateFn(next))
                {
                    if (list == null)
                        list = new List<Point>(8);
                    list.Add(next);
                }
            }

            return list;
        }

        /// <summary>
        /// Checks if at least one adjacent position in map match a predicate.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="predicateFn"></param>
        /// <returns></returns>
        public bool HasAnyAdjacentInMap(Point position, Predicate<Point> predicateFn)
        {
            if (!IsInBounds(position))
                return false;

            Point next;
            foreach (Direction d in Direction.COMPASS)
            {
                next = position + d;

                if (IsInBounds(next) && predicateFn(next))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Count how many adjacent positions in map match a predicate.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="predicateFn"></param>
        /// <returns></returns>
        public int CountAdjacentInMap(Point position, Predicate<Point> predicateFn)
        {
            if (!IsInBounds(position))
                return 0;

            int count = 0;
            Point next;
            foreach (Direction d in Direction.COMPASS)
            {
                next = position + d;

                if (IsInBounds(next) && predicateFn(next))
                    ++count;
                    
            }

            return count;
        }

        /// <summary>
        /// Apply a fonction on each adjacent tiles in map.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="fn"></param>
        public void ForEachAdjacentInMap(Point position, Action<Point> fn)
        {
            if (!IsInBounds(position))
                return;

            Point next;
            foreach (Direction d in Direction.COMPASS)
            {
                next = position + d;

                if (IsInBounds(next))
                    fn(next);
            }
        }

        public Point? FindFirstInMap(Predicate<Point> predicateFn)
        {
            Point p = new Point();
            for (int x = 0; x < m_Width; x++)
            {
                p.X = x;
                for (int y = 0; y < m_Height; y++)
                {
                    p.Y = y;
                    if (predicateFn(p))
                        return p;
                }
            }

            return null;
        }
        #endregion

        #region Serialization
        /// <summary>
        /// Deserialization contructor.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected Map(SerializationInfo info, StreamingContext context)
        {
            //////////////////
            // Primary fields
            //////////////////
            m_Seed = (int)info.GetValue("m_Seed", typeof(int));
            m_District = (District)info.GetValue("m_District", typeof(District));
            m_Name = (string)info.GetValue("m_Name", typeof(string));
            m_LocalTime = (WorldTime)info.GetValue("m_LocalTime", typeof(WorldTime));
            m_Width = (int)info.GetValue("m_Width", typeof(int));
            m_Height = (int)info.GetValue("m_Height", typeof(int));
            m_Rectangle = (Rectangle)info.GetValue("m_Rectangle", typeof(Rectangle));
            m_Tiles = (Tile[,])info.GetValue("m_Tiles", typeof(Tile[,]));
            m_Exits = (Dictionary<Point, Exit>)info.GetValue("m_Exits", typeof(Dictionary<Point, Exit>));
            m_Zones = (List<Zone>)info.GetValue("m_Zones", typeof(List<Zone>));
            m_ActorsList = (List<Actor>)info.GetValue("m_ActorsList", typeof(List<Actor>));
            m_MapObjectsList = (List<MapObject>)info.GetValue("m_MapObjectsList", typeof(List<MapObject>));
            m_GroundItemsByPosition = (Dictionary<Point, Inventory>)info.GetValue("m_GroundItemsByPosition", typeof(Dictionary<Point, Inventory>));
            m_CorpsesList = (List<Corpse>)info.GetValue("m_CorpsesList", typeof(List<Corpse>));
            m_Lighting = (Lighting)info.GetValue("m_Lighting", typeof(Lighting));
            m_Scents = (List<OdorScent>)info.GetValue("m_Scents", typeof(List<OdorScent>));
            m_Timers = (List<TimedTask>)info.GetValue("m_Timers", typeof(List<TimedTask>));
            // alpha10
            m_BgMusic = (string)info.GetValue("m_BgMusic", typeof(string));
        }

        public void ReconstructAuxiliaryFields()
        {
            ///////////////////////////////
            // Reconstruct auxiliary fields
            ///////////////////////////////
            m_aux_ActorsByPosition = new Dictionary<Point, Actor>();
            foreach (Actor a in m_ActorsList)
                m_aux_ActorsByPosition.Add(a.Location.Position, a);

            m_aux_GroundItemsList = new List<Inventory>();
            foreach (Inventory inv in m_GroundItemsByPosition.Values)
                m_aux_GroundItemsList.Add(inv);

            m_aux_MapObjectsByPosition = new Dictionary<Point, MapObject>();
            foreach (MapObject obj in m_MapObjectsList)
                m_aux_MapObjectsByPosition.Add(obj.Location.Position, obj);

            m_aux_ScentsByPosition = new Dictionary<Point, List<OdorScent>>();
            foreach (OdorScent scent in m_Scents)
            {                
                List<OdorScent> listHere;
                if (m_aux_ScentsByPosition.TryGetValue(scent.Position, out listHere))
                    listHere.Add(scent);
                else
                {
                    listHere = new List<OdorScent>() { scent };
                    m_aux_ScentsByPosition.Add(scent.Position, listHere);
                }
            }

            m_aux_CorpsesByPosition = new Dictionary<Point, List<Corpse>>();
            foreach (Corpse corpse in m_CorpsesList)
            {
                List<Corpse> listHere;
                if (m_aux_CorpsesByPosition.TryGetValue(corpse.Position, out listHere))
                    listHere.Add(corpse);
                else
                    m_aux_CorpsesByPosition.Add(corpse.Position, new List<Corpse>(1) { corpse });
            }
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //////////////////
            // Primary Fields
            //////////////////
            info.AddValue("m_Seed", m_Seed);
            info.AddValue("m_District", m_District);
            info.AddValue("m_Name", m_Name);
            info.AddValue("m_LocalTime", m_LocalTime);
            info.AddValue("m_Width", m_Width);
            info.AddValue("m_Height", m_Height);
            info.AddValue("m_Rectangle", m_Rectangle);
            info.AddValue("m_Tiles", m_Tiles);
            info.AddValue("m_Exits", m_Exits);
            info.AddValue("m_Zones", m_Zones);
            info.AddValue("m_ActorsList", m_ActorsList);
            info.AddValue("m_MapObjectsList", m_MapObjectsList);
            info.AddValue("m_GroundItemsByPosition", m_GroundItemsByPosition);
            info.AddValue("m_CorpsesList", m_CorpsesList);
            info.AddValue("m_Lighting", m_Lighting);
            info.AddValue("m_Scents", m_Scents);
            info.AddValue("m_Timers", m_Timers);
            // alpha10
            info.AddValue("m_BgMusic", m_BgMusic);
        }

        #region Pre-saving
        public void OptimizeBeforeSaving()
        {
            // tiles
            for (int x = 0; x < m_Width; x++)
                for (int y = 0; y < m_Height; y++)
                    m_Tiles[x, y].OptimizeBeforeSaving();

            // alpha10 items stacks
            foreach (Inventory stack in m_GroundItemsByPosition.Values)
                stack.OptimizeBeforeSaving();

            // actors
            foreach (Actor a in m_ActorsList)
                a.OptimizeBeforeSaving();

            // lists.
            m_ActorsList.TrimExcess();
            m_MapObjectsList.TrimExcess();
            m_Scents.TrimExcess();
            m_Zones.TrimExcess();
            m_CorpsesList.TrimExcess();
            m_Timers.TrimExcess();
        }
        #endregion

        #endregion

        #region Hashcode
        public override int GetHashCode()
        {
            return m_Name.GetHashCode() ^ m_District.GetHashCode();
        }
        #endregion
    }
}
