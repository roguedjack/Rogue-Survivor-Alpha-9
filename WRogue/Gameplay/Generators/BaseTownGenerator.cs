using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.Engine.MapObjects;
using djack.RogueSurvivor.Gameplay;
using djack.RogueSurvivor.Gameplay.AI;
using djack.RogueSurvivor.UI;

namespace djack.RogueSurvivor.Gameplay.Generators
{
    class BaseTownGenerator : BaseMapGenerator
    {
        #region Types
        public struct Parameters
        {
            #region Fields
            int m_MapWidth;
            int m_MapHeight;
            int m_MinBlockSize;
            int m_WreckedCarChance;
            int m_CHARBuildingChance;
            int m_ShopBuildingChance;
            int m_ParkBuildingChance;
            int m_PostersChance;
            int m_TagsChance;
            int m_ItemInShopShelfChance;
            int m_PolicemanChance;
            #endregion

            #region Properties
            /// <summary>
            /// District the map is currently generated in.
            /// </summary>
            public District District
            {
                get;
                set;
            }

            /// <summary>
            /// Do we need to generate the Police Station in this district?
            /// </summary>
            public bool GeneratePoliceStation
            {
                get;
                set;
            }

            /// <summary>
            /// Do we need to generate the Hospital in this district?
            /// </summary>
            public bool GenerateHospital
            {
                get;
                set;
            }

            public int MapWidth
            {
                get { return m_MapWidth; }
                set
                {
                    if (value <= 0 || value > RogueGame.MAP_MAX_WIDTH) throw new ArgumentOutOfRangeException("MapWidth");
                    m_MapWidth = value;
                }
            }

            public int MapHeight
            {
                get { return m_MapHeight; }
                set
                {
                    if (value <= 0 || value > RogueGame.MAP_MAX_WIDTH) throw new ArgumentOutOfRangeException("MapHeight");
                    m_MapHeight = value;
                }
            }

            public int MinBlockSize
            {
                get { return m_MinBlockSize; }
                set
                {
                    if (value < 4 || value > 32) throw new ArgumentOutOfRangeException("MinBlockSize must be [4..32]");
                    m_MinBlockSize = value;
                }
            }

            public int WreckedCarChance
            {
                get { return m_WreckedCarChance; }
                set
                {
                    if (value < 0 || value > 100) throw new ArgumentOutOfRangeException("WreckedCarChance must be [0..100]");
                    m_WreckedCarChance = value;
                }
            }

            public int ShopBuildingChance
            {
                get { return m_ShopBuildingChance; }
                set
                {
                    if (value < 0 || value > 100) throw new ArgumentOutOfRangeException("ShopBuildingChance must be [0..100]");
                    m_ShopBuildingChance = value;
                }
            }

            public int ParkBuildingChance
            {
                get { return m_ParkBuildingChance; }
                set
                {
                    if (value < 0 || value > 100) throw new ArgumentOutOfRangeException("ParkBuildingChance must be [0..100]");
                    m_ParkBuildingChance = value;

                }
            }

            public int CHARBuildingChance
            {
                get { return m_CHARBuildingChance; }
                set
                {
                    if (value < 0 || value > 100) throw new ArgumentOutOfRangeException("CHARBuildingChance must be [0..100]");
                    m_CHARBuildingChance = value;

                }
            }

            public int PostersChance
            {
                get { return m_PostersChance; }
                set
                {
                    if (value < 0 || value > 100) throw new ArgumentOutOfRangeException("PostersChance must be [0..100]");
                    m_PostersChance = value;

                }
            }

            public int TagsChance
            {
                get { return m_TagsChance; }
                set
                {
                    if (value < 0 || value > 100) throw new ArgumentOutOfRangeException("TagsChance must be [0..100]");
                    m_TagsChance = value;

                }
            }

            public int ItemInShopShelfChance
            {
                get { return m_ItemInShopShelfChance; }
                set
                {
                    if (value < 0 || value > 100) throw new ArgumentOutOfRangeException("ItemInShopShelfChance must be [0..100]");
                    m_ItemInShopShelfChance = value;

                }
            }


            public int PolicemanChance
            {
                get { return m_PolicemanChance; }
                set
                {
                    if (value < 0 || value > 100) throw new ArgumentOutOfRangeException("PolicemanChance must be [0..100]");
                    m_PolicemanChance = value;

                }
            }
            #endregion
        }

        public class Block
        {
            /// <summary>
            /// Rectangle enclosing the whole block.
            /// </summary>
            public Rectangle Rectangle { get; set; }

            /// <summary>
            /// Rectangle enclosing the building : the blocks minus the walkway ring.
            /// </summary>
            public Rectangle BuildingRect { get; set; }

            /// <summary>
            /// Rectangle enclosing the inside of the building : the building minus the walls ring.
            /// </summary>
            public Rectangle InsideRect { get; set; }


            public Block(Rectangle rect)
            {
                ResetRectangle(rect);
            }

            public Block(Block copyFrom)
            {
                this.Rectangle = copyFrom.Rectangle;
                this.BuildingRect = copyFrom.BuildingRect;
                this.InsideRect = copyFrom.InsideRect;
            }

            public void ResetRectangle(Rectangle rect)
            {
                this.Rectangle = rect;
                this.BuildingRect = new Rectangle(rect.Left + 1, rect.Top + 1, rect.Width - 2, rect.Height - 2);
                this.InsideRect = new Rectangle(this.BuildingRect.Left + 1, this.BuildingRect.Top + 1, this.BuildingRect.Width - 2, this.BuildingRect.Height - 2);
            }
        }

        protected enum ShopType : byte
        {
            _FIRST,

            GENERAL_STORE = _FIRST,
            GROCERY,
            SPORTSWEAR,
            PHARMACY,
            CONSTRUCTION,
            GUNSHOP,
            HUNTING,

            _COUNT
        }

        protected enum CHARBuildingType : byte
        {
            NONE,
            AGENCY,
            OFFICE
        }

        // alpha10
        protected enum HouseOutsideRoomType : byte
        {
            _FIRST,

            GARDEN = _FIRST,
            PARKING_LOT,

            _COUNT
        }
        #endregion

        #region Constants
        public static readonly Parameters DEFAULT_PARAMS = new Parameters()
        {
            MapWidth = RogueGame.MAP_MAX_WIDTH,
            MapHeight = RogueGame.MAP_MAX_HEIGHT,
            MinBlockSize = 11, // 12 for 75x75 map size; 10 gives to many small buildings.
            WreckedCarChance = 10,
            ShopBuildingChance = 10,
            ParkBuildingChance = 10,
            CHARBuildingChance = 10,
            PostersChance = 2,
            TagsChance = 2,
            ItemInShopShelfChance = 100,
            PolicemanChance = 15
        };

        const int PARK_TREE_CHANCE = 25;
        const int PARK_BENCH_CHANCE = 5;
        const int PARK_ITEM_CHANCE = 5;
        const int PARK_SHED_CHANCE = 75;  // alpha10.1
        const int PARK_SHED_WIDTH = 5;  // alpha10
        const int PARK_SHED_HEIGHT = 5;  // alpha10

        const int MAX_CHAR_GUARDS_PER_OFFICE = 3;

        const int SEWERS_ITEM_CHANCE = 1;
        const int SEWERS_JUNK_CHANCE = 10;
        const int SEWERS_TAG_CHANCE = 10;
        const int SEWERS_IRON_FENCE_PER_BLOCK_CHANCE = 50; // 8 fences average on std maps size 75x75.
        const int SEWERS_ROOM_CHANCE = 20;

        const int SUBWAY_TAGS_POSTERS_CHANCE = 20;

        const int HOUSE_LIVINGROOM_ITEMS_ON_TABLE = 2;
        const int HOUSE_KITCHEN_ITEMS_ON_TABLE = 2;
        const int HOUSE_KITCHEN_ITEMS_IN_FRIDGE = 3;
        const int HOUSE_BASEMENT_CHANCE = 30;
        const int HOUSE_BASEMENT_OBJECT_CHANCE_PER_TILE = 10;
        const int HOUSE_BASEMENT_PILAR_CHANCE = 20;
        const int HOUSE_BASEMENT_WEAPONS_CACHE_CHANCE = 20;
        const int HOUSE_BASEMENT_ZOMBIE_RAT_CHANCE = 5; // per tile.
        // alpha10 new house stuff
        const int HOUSE_OUTSIDE_ROOM_NEED_MIN_ROOMS = 4;
        const int HOUSE_OUTSIDE_ROOM_CHANCE = 75;
        const int HOUSE_GARDEN_TREE_CHANCE = 10;  // per tile
        const int HOUSE_PARKING_LOT_CAR_CHANCE = 10;  // per tile
        // alpha10.1 new house floorplan: apartements
        const int HOUSE_IS_APARTMENTS_CHANCE = 50;

        const int SHOP_BASEMENT_CHANCE = 30;
        const int SHOP_BASEMENT_SHELF_CHANCE_PER_TILE = 5;
        const int SHOP_BASEMENT_ITEM_CHANCE_PER_SHELF = 33;
        const int SHOP_WINDOW_CHANCE = 30;
        const int SHOP_BASEMENT_ZOMBIE_RAT_CHANCE = 5; // per tile.
        #endregion

        #region Fields
        Parameters m_Params = DEFAULT_PARAMS;
        protected DiceRoller m_DiceRoller;

        /// <summary>
        /// Blocks on surface map since during current generation.
        /// </summary>
        List<Block> m_SurfaceBlocks;
        #endregion

        #region Properties
        public Parameters Params
        {
            get { return m_Params; }
            set { m_Params = value; }
        }
        #endregion

        public BaseTownGenerator(RogueGame game, Parameters parameters)
            : base(game)
        {
            m_Params = parameters;
            m_DiceRoller = new DiceRoller();
        }

        #region Entry Map (Surface)
        public override Map Generate(int seed)
        {
            m_DiceRoller = new DiceRoller(seed);
            Map map = new Map(seed, "Base City", m_Params.MapWidth, m_Params.MapHeight);

            ///////////////////
            // Init with grass
            ///////////////////
            base.TileFill(map, m_Game.GameTiles.FLOOR_GRASS);

            ///////////////
            // Cut blocks
            ///////////////
            List<Block> blocks = new List<Block>();
            Rectangle cityRectangle = new Rectangle(0, 0, map.Width, map.Height);
            MakeBlocks(map, true, ref blocks, cityRectangle);

            ///////////////////////////////////////
            // Make concrete buildings from blocks
            ///////////////////////////////////////
            List<Block> emptyBlocks = new List<Block>(blocks);
            List<Block> completedBlocks = new List<Block>(emptyBlocks.Count);

            // remember blocks.
            m_SurfaceBlocks = new List<Block>(blocks.Count);
            foreach (Block b in blocks)
                m_SurfaceBlocks.Add(new Block(b));

            // Special buildings.
            #region
            // Police Station?
            if (m_Params.GeneratePoliceStation)
            {
                Block policeBlock;
                MakePoliceStation(map, blocks, out policeBlock);
                emptyBlocks.Remove(policeBlock);
            }
            // Hospital?
            if (m_Params.GenerateHospital)
            {
                Block hospitalBlock;
                MakeHospital(map, blocks, out hospitalBlock);
                emptyBlocks.Remove(hospitalBlock);
            }
            #endregion

            // shops.
            completedBlocks.Clear();
            foreach (Block b in emptyBlocks)
            {
                if (m_DiceRoller.RollChance(m_Params.ShopBuildingChance) &&
                    MakeShopBuilding(map, b))
                    completedBlocks.Add(b);
            }
            foreach (Block b in completedBlocks)
                emptyBlocks.Remove(b);

            // CHAR buildings..
            completedBlocks.Clear();
            int charOfficesCount = 0;
            foreach (Block b in emptyBlocks)
            {
                if ((m_Params.District.Kind == DistrictKind.BUSINESS && charOfficesCount == 0) || m_DiceRoller.RollChance(m_Params.CHARBuildingChance))
                {
                    CHARBuildingType btype = MakeCHARBuilding(map, b);
                    if (btype == CHARBuildingType.OFFICE)
                    {
                        ++charOfficesCount;
                        PopulateCHAROfficeBuilding(map, b);
                    }
                    if (btype != CHARBuildingType.NONE)
                        completedBlocks.Add(b);
                }
            }
            foreach (Block b in completedBlocks)
                emptyBlocks.Remove(b);

            // parks.
            completedBlocks.Clear();
            foreach (Block b in emptyBlocks)
            {
                if (m_DiceRoller.RollChance(m_Params.ParkBuildingChance) &&
                    MakeParkBuilding(map, b))
                    completedBlocks.Add(b);
            }
            foreach (Block b in completedBlocks)
                emptyBlocks.Remove(b);

            // all the rest is housings.
            completedBlocks.Clear();
            foreach (Block b in emptyBlocks)
            {
                MakeHousingBuilding(map, b);
                completedBlocks.Add(b);
            }
            foreach (Block b in completedBlocks)
                emptyBlocks.Remove(b);

            ////////////
            // Decorate
            ////////////
            AddWreckedCarsOutside(map, cityRectangle);
            DecorateOutsideWallsWithPosters(map, cityRectangle, m_Params.PostersChance);
            DecorateOutsideWallsWithTags(map, cityRectangle, m_Params.TagsChance);

            // alpha10
            /////////
            // Music
            /////////
            map.BgMusic = GameMusics.SURFACE;

            ////////
            // Done
            ////////
            return map;
        }
        #endregion

        #region Sewers Map
        public virtual Map GenerateSewersMap(int seed, District district)
        {
            // Create.
            m_DiceRoller = new DiceRoller(seed);
            Map sewers = new Map(seed, "sewers", district.EntryMap.Width, district.EntryMap.Height)
            {
                Lighting = Lighting.DARKNESS
            };
            sewers.AddZone(MakeUniqueZone("sewers", sewers.Rect));
            TileFill(sewers, m_Game.GameTiles.WALL_SEWER);

            ///////////////////////////////////////////////////
            // 1. Make blocks.
            // 2. Make tunnels.
            // 3. Link with surface.
            // 4. Additional jobs.
            // 5. Sewers Maintenance Room & Building(surface).
            // 6. Some rooms.
            // 7. Objects.
            // 8. Items.
            // 9. Tags.
            // alpha10
            // 10. Music.
            ///////////////////////////////////////////////////
            Map surface = district.EntryMap;

            // 1. Make blocks.
            List<Block> blocks = new List<Block>(m_SurfaceBlocks.Count);
            MakeBlocks(sewers, false, ref blocks, new Rectangle(0, 0, sewers.Width, sewers.Height));

            // 2. Make tunnels.
            #region
            // Carve tunnels.
            foreach (Block b in blocks)
            {
                TileRectangle(sewers, m_Game.GameTiles.FLOOR_SEWER_WATER, b.Rectangle);
            }
            // Iron Fences blocking some tunnels.
            foreach (Block b in blocks)
            {
                // chance?
                if (!m_DiceRoller.RollChance(SEWERS_IRON_FENCE_PER_BLOCK_CHANCE))
                    continue;

                // fences on a side.
                int fx1, fy1, fx2, fy2;
                bool goodFencePos = false;
                do
                {
                    // roll side.
                    int sideRoll = m_DiceRoller.Roll(0, 4);
                    switch (sideRoll)
                    {
                        case 0: // north.
                        case 1: // south.
                            fx1 = m_DiceRoller.Roll(b.Rectangle.Left, b.Rectangle.Right - 1);
                            fy1 = (sideRoll == 0 ? b.Rectangle.Top : b.Rectangle.Bottom - 1);

                            fx2 = fx1;
                            fy2 = (sideRoll == 0 ? fy1 - 1 : fy1 + 1);
                            break;
                        case 2: // east.
                        case 3: // west.
                            fx1 = (sideRoll == 2 ? b.Rectangle.Left : b.Rectangle.Right - 1);
                            fy1 = m_DiceRoller.Roll(b.Rectangle.Top, b.Rectangle.Bottom - 1);

                            fx2 = (sideRoll == 2 ? fx1 - 1 : fx1 + 1);
                            fy2 = fy1;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("unhandled roll");
                    }

                    // never on border.
                    if (sewers.IsOnMapBorder(fx1, fy1) || sewers.IsOnMapBorder(fx2, fy2))
                        continue;

                    // must have walls.
                    if (CountAdjWalls(sewers, fx1, fy1) != 3)
                        continue;
                    if (CountAdjWalls(sewers, fx2, fy2) != 3)
                        continue;

                    // found!
                    goodFencePos = true;
                }
                while (!goodFencePos);

                // add (both of them)
                MapObjectPlace(sewers, fx1, fy1, MakeObjIronFence(GameImages.OBJ_IRON_FENCE));
                MapObjectPlace(sewers, fx2, fy2, MakeObjIronFence(GameImages.OBJ_IRON_FENCE));
            }
            #endregion

            // 3. Link with surface.
            #region
            // loop until we got at least one link.
            int countLinks = 0;
            do
            {
                for (int x = 0; x < sewers.Width; x++)
                    for (int y = 0; y < sewers.Height; y++)
                    {
                        // link? roll chance. 3%
                        bool doLink = m_DiceRoller.RollChance(3);
                        if (!doLink)
                            continue;

                        // both surface and sewer tile must be walkable.
                        Tile tileSewer = sewers.GetTileAt(x, y);
                        if (!tileSewer.Model.IsWalkable)
                            continue;
                        Tile tileSurface = surface.GetTileAt(x, y);
                        if (!tileSurface.Model.IsWalkable)
                            continue;

                        // no blocking object.
                        if (sewers.GetMapObjectAt(x, y) != null)
                            continue;

                        // surface tile must be outside.
                        if (tileSurface.IsInside)
                            continue;
                        // surface tile must be walkway or grass.
                        if (tileSurface.Model != m_Game.GameTiles.FLOOR_WALKWAY && tileSurface.Model != m_Game.GameTiles.FLOOR_GRASS)
                            continue;
                        // surface tile must not be obstructed by an object.
                        if (surface.GetMapObjectAt(x, y) != null)
                            continue;

                        // must not be adjacent to another exit.
                        Point pt = new Point(x, y);
                        if (sewers.HasAnyAdjacentInMap(pt, (p) => sewers.GetExitAt(p) != null))
                            continue;
                        if (surface.HasAnyAdjacentInMap(pt, (p) => surface.GetExitAt(p) != null))
                            continue;

                        // link with ladder and sewer hole.
                        AddExit(sewers, pt, surface, pt, GameImages.DECO_SEWER_LADDER, true);
                        AddExit(surface, pt, sewers, pt, GameImages.DECO_SEWER_HOLE, true);

                        // - one more link.
                        ++countLinks;
                    }
            }
            while (countLinks < 1);
            #endregion

            // 4. Additional jobs.
            #region
            // Mark all the map as inside.
            for (int x = 0; x < sewers.Width; x++)
                for (int y = 0; y < sewers.Height; y++)
                    sewers.GetTileAt(x, y).IsInside = true;
            #endregion

            // 5. Sewers Maintenance Room & Building(surface).
            #region
            // search a suitable surface blocks.
            List<Block> goodBlocks = null;
            foreach (Block b in m_SurfaceBlocks)
            {
                // surface building must be of minimal size.
                if (b.BuildingRect.Width > m_Params.MinBlockSize + 2 || b.BuildingRect.Height > m_Params.MinBlockSize + 2)
                    continue;

                // must not be a special building or have an exit (eg: houses with basements)
                if (IsThereASpecialBuilding(surface, b.InsideRect))
                    continue;

                // we must carve a room in the sewers.
                bool hasRoom = true;
                for (int x = b.Rectangle.Left; x < b.Rectangle.Right && hasRoom; x++)
                    for (int y = b.Rectangle.Top; y < b.Rectangle.Bottom && hasRoom; y++)
                    {
                        if (sewers.GetTileAt(x, y).Model.IsWalkable)
                            hasRoom = false;
                    }
                if (!hasRoom)
                    continue;

                // found one.
                if (goodBlocks == null)
                    goodBlocks = new List<Block>(m_SurfaceBlocks.Count);
                goodBlocks.Add(b);
                break;
            }

            // if found, make maintenance room in sewers and building on surface.
            if (goodBlocks != null)
            {
                // pick one at random.
                Block surfaceBlock = goodBlocks[m_DiceRoller.Roll(0, goodBlocks.Count)];

                // clear surface building.
                ClearRectangle(surface, surfaceBlock.BuildingRect);
                TileFill(surface, m_Game.GameTiles.FLOOR_CONCRETE, surfaceBlock.BuildingRect);
                m_SurfaceBlocks.Remove(surfaceBlock);

                // make maintenance building on the surface & room in the sewers.
                Block newSurfaceBlock = new Block(surfaceBlock.Rectangle);
                Point ladderHolePos = new Point(newSurfaceBlock.BuildingRect.Left + newSurfaceBlock.BuildingRect.Width / 2, newSurfaceBlock.BuildingRect.Top + newSurfaceBlock.BuildingRect.Height / 2);
                MakeSewersMaintenanceBuilding(surface, true, newSurfaceBlock, sewers, ladderHolePos);
                Block sewersRoom = new Block(surfaceBlock.Rectangle);
                MakeSewersMaintenanceBuilding(sewers, false, sewersRoom, surface, ladderHolePos);
            }
            #endregion

            // 6. Some rooms.
            #region
            foreach (Block b in blocks)
            {
                // chance?
                if (!m_DiceRoller.RollChance(SEWERS_ROOM_CHANCE))
                    continue;

                // must be all walls = not already assigned as a room.
                if (!CheckForEachTile(sewers, b.BuildingRect, (pt) => !sewers.GetTileAt(pt).Model.IsWalkable))
                    continue;

                // carve a room.
                TileFill(sewers, m_Game.GameTiles.FLOOR_CONCRETE, b.InsideRect);

                // 4 entries.
                sewers.SetTileModelAt(b.BuildingRect.Left + b.BuildingRect.Width / 2, b.BuildingRect.Top, m_Game.GameTiles.FLOOR_CONCRETE);
                sewers.SetTileModelAt(b.BuildingRect.Left + b.BuildingRect.Width / 2, b.BuildingRect.Bottom - 1, m_Game.GameTiles.FLOOR_CONCRETE);
                sewers.SetTileModelAt(b.BuildingRect.Left, b.BuildingRect.Top + b.BuildingRect.Height / 2, m_Game.GameTiles.FLOOR_CONCRETE);
                sewers.SetTileModelAt(b.BuildingRect.Right - 1, b.BuildingRect.Top + b.BuildingRect.Height / 2, m_Game.GameTiles.FLOOR_CONCRETE);

                // zone.
                sewers.AddZone(MakeUniqueZone("room", b.InsideRect));
            }
            #endregion

            // 7. Objects.
            #region
            // junk.
            MapObjectFill(sewers, new Rectangle(0, 0, sewers.Width, sewers.Height),
                (pt) =>
                {
                    if (!m_DiceRoller.RollChance(SEWERS_JUNK_CHANCE))
                        return null;
                    if (!sewers.IsWalkable(pt.X, pt.Y))
                        return null;

                    return MakeObjJunk(GameImages.OBJ_JUNK);
                });
            #endregion

            // 8. Items.
            #region
            for (int x = 0; x < sewers.Width; x++)
                for (int y = 0; y < sewers.Height; y++)
                {
                    if (!m_DiceRoller.RollChance(SEWERS_ITEM_CHANCE))
                        continue;
                    if (!sewers.IsWalkable(x, y))
                        continue;

                    // drop item.
                    Item it;
                    int roll = m_DiceRoller.Roll(0, 3);
                    switch (roll)
                    {
                        case 0: it = MakeItemBigFlashlight(); break;
                        case 1: it = MakeItemCrowbar(); break;
                        case 2: it = MakeItemSprayPaint(); break;
                        default:
                            throw new ArgumentOutOfRangeException("unhandled roll");
                    }
                    sewers.DropItemAt(it, x, y);
                }
            #endregion

            // 9. Tags.
            #region
            for (int x = 0; x < sewers.Width; x++)
                for (int y = 0; y < sewers.Height; y++)
                {
                    if (m_DiceRoller.RollChance(SEWERS_TAG_CHANCE))
                    {
                        // must be a wall with walkables around.
                        Tile t = sewers.GetTileAt(x, y);
                        if (t.Model.IsWalkable)
                            continue;
                        if (CountAdjWalkables(sewers, x, y) < 2)
                            continue;

                        // tag.
                        t.AddDecoration(TAGS[m_DiceRoller.Roll(0, TAGS.Length)]);
                    }
                }
            #endregion

            // alpha10
            // 10. Music.
            sewers.BgMusic = GameMusics.SEWERS;

            // Done.
            return sewers;
        }
        #endregion

        #region Subway Map
        public virtual Map GenerateSubwayMap(int seed, District district)
        {
            // Create.
            m_DiceRoller = new DiceRoller(seed);
            Map subway = new Map(seed, "subway", district.EntryMap.Width, district.EntryMap.Height)
            {
                Lighting = Lighting.DARKNESS
            };
            TileFill(subway, m_Game.GameTiles.WALL_BRICK);

            /////////////////////////////////////
            // 1. Trace rail line.
            // 2. Make station linked to surface?
            // 3. Small tools room.
            // 4. Tags & Posters almost everywhere.
            // 5. Additional jobs.
            // alpha10
            // 6. Music
            /////////////////////////////////////
            Map surface = district.EntryMap;

            // 1. Trace rail line.
            #region
            int railStartX = 0;
            int railEndX = subway.Width - 1;
            int railY = subway.Width / 2 - 1;
            int railSize = 4;

            for (int x = railStartX; x <= railEndX; x++)
            {
                for (int y = railY; y < railY + railSize; y++)
                    subway.SetTileModelAt(x, y, m_Game.GameTiles.RAIL_EW);
            }
            subway.AddZone(MakeUniqueZone(RogueGame.NAME_SUBWAY_RAILS, new Rectangle(railStartX, railY, railEndX - railStartX + 1, railSize)));
            #endregion

            // 2. Make station linked to surface.
            #region
            // search a suitable surface blocks.
            List<Block> goodBlocks = null;
            foreach (Block b in m_SurfaceBlocks)
            {
                // surface building must be of minimal size.
                if (b.BuildingRect.Width > m_Params.MinBlockSize + 2 || b.BuildingRect.Height > m_Params.MinBlockSize + 2)
                    continue;

                // must not be a special building or have an exit (eg: houses with basements)
                if (IsThereASpecialBuilding(surface, b.InsideRect))
                    continue;

                // we must carve a room in the subway and must not be to close to rails.
                bool hasRoom = true;
                int minDistToRails = 8;
                for (int x = b.Rectangle.Left - minDistToRails; x < b.Rectangle.Right + minDistToRails && hasRoom; x++)
                    for (int y = b.Rectangle.Top - minDistToRails; y < b.Rectangle.Bottom + minDistToRails && hasRoom; y++)
                    {
                        if (!subway.IsInBounds(x, y))
                            continue;
                        if (subway.GetTileAt(x, y).Model.IsWalkable)
                            hasRoom = false;
                    }
                if (!hasRoom)
                    continue;

                // found one.
                if (goodBlocks == null)
                    goodBlocks = new List<Block>(m_SurfaceBlocks.Count);
                goodBlocks.Add(b);
                break;
            }

            // if found, make station room and building.
            if (goodBlocks != null)
            {
                // pick one at random.
                Block surfaceBlock = goodBlocks[m_DiceRoller.Roll(0, goodBlocks.Count)];

                // clear surface building.
                ClearRectangle(surface, surfaceBlock.BuildingRect);
                TileFill(surface, m_Game.GameTiles.FLOOR_CONCRETE, surfaceBlock.BuildingRect);
                m_SurfaceBlocks.Remove(surfaceBlock);

                // make station building on the surface & room in the subway.
                Block newSurfaceBlock = new Block(surfaceBlock.Rectangle);
                Point stairsPos = new Point(newSurfaceBlock.BuildingRect.Left + newSurfaceBlock.BuildingRect.Width / 2, newSurfaceBlock.InsideRect.Top);
                MakeSubwayStationBuilding(surface, true, newSurfaceBlock, subway, stairsPos);
                Block subwayRoom = new Block(surfaceBlock.Rectangle);
                MakeSubwayStationBuilding(subway, false, subwayRoom, surface, stairsPos);
            }
            #endregion

            // 3.  Small tools room.
            #region
            const int toolsRoomWidth = 5;
            const int toolsRoomHeight = 5;
            Direction toolsRoomDir = m_DiceRoller.RollChance(50) ? Direction.N : Direction.S;
            Rectangle toolsRoom = Rectangle.Empty;
            bool foundToolsRoom = false;
            int toolsRoomAttempt = 0;
            do
            {
                int x = m_DiceRoller.Roll(10, subway.Width - 10);
                int y = (toolsRoomDir == Direction.N ? railY - 1 : railY + railSize);

                if (!subway.GetTileAt(x, y).Model.IsWalkable)
                {
                    // make room rectangle.
                    if (toolsRoomDir == Direction.N)
                        toolsRoom = new Rectangle(x, y - toolsRoomHeight + 1, toolsRoomWidth, toolsRoomHeight);
                    else
                        toolsRoom = new Rectangle(x, y, toolsRoomWidth, toolsRoomHeight);
                    // check room rect is all walls (do not overlap with platform or other rooms)
                    foundToolsRoom = CheckForEachTile(subway, toolsRoom, (pt) => !subway.GetTileAt(pt).Model.IsWalkable);
                }
                ++toolsRoomAttempt;
            }
            while (toolsRoomAttempt < subway.Width * subway.Height && !foundToolsRoom);

            if (foundToolsRoom)
            {
                // room.
                TileFill(subway, m_Game.GameTiles.FLOOR_CONCRETE, toolsRoom);
                TileRectangle(subway, m_Game.GameTiles.WALL_BRICK, toolsRoom);
                PlaceDoor(subway, toolsRoom.Left + toolsRoomWidth / 2, (toolsRoomDir == Direction.N ? toolsRoom.Bottom - 1 : toolsRoom.Top), m_Game.GameTiles.FLOOR_CONCRETE, MakeObjIronDoor());
                subway.AddZone(MakeUniqueZone("tools room", toolsRoom));

                // shelves on walls with construction items.
                DoForEachTile(subway, toolsRoom,
                    (pt) =>
                    {
                        if (!subway.IsWalkable(pt.X, pt.Y))
                            return;
                        if (CountAdjWalls(subway, pt.X, pt.Y) == 0 || CountAdjDoors(subway, pt.X, pt.Y) > 0)
                            return;

                        subway.PlaceMapObjectAt(MakeObjShelf(GameImages.OBJ_SHOP_SHELF), pt);
                        subway.DropItemAt(MakeShopConstructionItem(), pt);
                    });
            }
            #endregion

            // 4. Tags & Posters almost everywhere.
            #region
            for (int x = 0; x < subway.Width; x++)
                for (int y = 0; y < subway.Height; y++)
                {
                    if (m_DiceRoller.RollChance(SUBWAY_TAGS_POSTERS_CHANCE))
                    {
                        // must be a wall with walkables around.
                        Tile t = subway.GetTileAt(x, y);
                        if (t.Model.IsWalkable)
                            continue;
                        if (CountAdjWalkables(subway, x, y) < 2)
                            continue;

                        // poster?
                        if (m_DiceRoller.RollChance(50))
                            t.AddDecoration(POSTERS[m_DiceRoller.Roll(0, POSTERS.Length)]);

                        // tag?
                        if (m_DiceRoller.RollChance(50))
                            t.AddDecoration(TAGS[m_DiceRoller.Roll(0, TAGS.Length)]);
                    }
                }
            #endregion


            // 5. Additional jobs.
            // Mark all the map as inside.
            for (int x = 0; x < subway.Width; x++)
                for (int y = 0; y < subway.Height; y++)
                    subway.GetTileAt(x, y).IsInside = true;

            // alpha10
            // 6. Music.
            subway.BgMusic = GameMusics.SUBWAY;

            // Done.
            return subway;
        }
        #endregion

        #region Blocks generation

        void QuadSplit(Rectangle rect, int minWidth, int minHeight, out int splitX, out int splitY, out Rectangle topLeft, out Rectangle topRight, out Rectangle bottomLeft, out Rectangle bottomRight)
        {
            // Choose a random split point.
            int leftWidthSplit = m_DiceRoller.Roll(rect.Width / 3, (2 * rect.Width) / 3);
            int topHeightSplit = m_DiceRoller.Roll(rect.Height / 3, (2 * rect.Height) / 3);

            // Ensure splitting does not produce rects below minima.
            if (leftWidthSplit < minWidth)
                leftWidthSplit = minWidth;
            if (topHeightSplit < minHeight)
                topHeightSplit = minHeight;

            int rightWidthSplit = rect.Width - leftWidthSplit;
            int bottomHeightSplit = rect.Height - topHeightSplit;

            bool doSplitX, doSplitY;
            doSplitX = doSplitY = true;

            if (rightWidthSplit < minWidth)
            {
                leftWidthSplit = rect.Width;
                rightWidthSplit = 0;
                doSplitX = false;
            }
            if (bottomHeightSplit < minHeight)
            {
                topHeightSplit = rect.Height;
                bottomHeightSplit = 0;
                doSplitY = false;
            }

            // Split point.
            splitX = rect.Left + leftWidthSplit;
            splitY = rect.Top + topHeightSplit;

            // Make the quads.
            topLeft = new Rectangle(rect.Left, rect.Top, leftWidthSplit, topHeightSplit);

            if (doSplitX)
                topRight = new Rectangle(splitX, rect.Top, rightWidthSplit, topHeightSplit);
            else
                topRight = Rectangle.Empty;

            if (doSplitY)
                bottomLeft = new Rectangle(rect.Left, splitY, leftWidthSplit, bottomHeightSplit);
            else
                bottomLeft = Rectangle.Empty;

            if (doSplitX && doSplitY)
                bottomRight = new Rectangle(splitX, splitY, rightWidthSplit, bottomHeightSplit);
            else
                bottomRight = Rectangle.Empty;
        }

        void MakeBlocks(Map map, bool makeRoads, ref List<Block> list, Rectangle rect)
        {
            const int ring = 1; // dont change, keep to 1 (0=no roads, >1 = out of map)

            ////////////
            // 1. Split
            ////////////
            int splitX, splitY;
            Rectangle topLeft, topRight, bottomLeft, bottomRight;
            // +N to account for the road ring.
            QuadSplit(rect, m_Params.MinBlockSize + ring, m_Params.MinBlockSize + ring, out splitX, out splitY, out topLeft, out topRight, out bottomLeft, out bottomRight);

            ///////////////////
            // 2. Termination?
            ///////////////////
            if (topRight.IsEmpty && bottomLeft.IsEmpty && bottomRight.IsEmpty)
            {
                // Make road ring?
                if (makeRoads)
                {
                    MakeRoad(map, m_Game.GameTiles[GameTiles.IDs.ROAD_ASPHALT_EW], new Rectangle(rect.Left, rect.Top, rect.Width, ring));        // north side
                    MakeRoad(map, m_Game.GameTiles[GameTiles.IDs.ROAD_ASPHALT_EW], new Rectangle(rect.Left, rect.Bottom - 1, rect.Width, ring)); // south side
                    MakeRoad(map, m_Game.GameTiles[GameTiles.IDs.ROAD_ASPHALT_NS], new Rectangle(rect.Left, rect.Top, ring, rect.Height));       // west side
                    MakeRoad(map, m_Game.GameTiles[GameTiles.IDs.ROAD_ASPHALT_NS], new Rectangle(rect.Right - 1, rect.Top, ring, rect.Height));       // east side

                    // Adjust rect.
                    topLeft.Width -= 2 * ring;
                    topLeft.Height -= 2 * ring;
                    topLeft.Offset(ring, ring);
                }

                // Add block.
                list.Add(new Block(topLeft));
                return;
            }

            //////////////
            // 3. Recurse
            //////////////
            // always top left.
            MakeBlocks(map, makeRoads, ref list, topLeft);
            // then recurse in non empty quads.
            if (!topRight.IsEmpty)
            {
                MakeBlocks(map, makeRoads, ref list, topRight);
            }
            if (!bottomLeft.IsEmpty)
            {
                MakeBlocks(map, makeRoads, ref list, bottomLeft);
            }
            if (!bottomRight.IsEmpty)
            {
                MakeBlocks(map, makeRoads, ref list, bottomRight);
            }
        }

        protected virtual void MakeRoad(Map map, TileModel roadModel, Rectangle rect)
        {
            base.TileFill(map, roadModel, rect,
                (tile, prevmodel, x, y) =>
                {
                    // don't overwrite roads!
                    if (m_Game.GameTiles.IsRoadModel(prevmodel))
                        map.SetTileModelAt(x, y, prevmodel);
                });
            map.AddZone(base.MakeUniqueZone("road", rect));
        }
        #endregion

        #region Door/Window placement
        protected virtual void PlaceDoor(Map map, int x, int y, TileModel floor, DoorWindow door)
        {
            map.SetTileModelAt(x, y, floor);
            base.MapObjectPlace(map, x, y, door);
        }

        protected virtual void PlaceDoorIfNoObject(Map map, int x, int y, TileModel floor, DoorWindow door)
        {
            if (map.GetMapObjectAt(x, y) != null)
                return;
            PlaceDoor(map, x, y, floor, door);
        }

        protected virtual bool PlaceDoorIfAccessible(Map map, int x, int y, TileModel floor, int minAccessibility, DoorWindow door)
        {
            int countWalkable = 0;

            Point p = new Point(x, y);
            foreach (Direction d in Direction.COMPASS)
            {
                Point next = p + d;
                if (map.IsWalkable(next.X, next.Y))
                    ++countWalkable;
            }

            if (countWalkable >= minAccessibility)
            {
                PlaceDoorIfNoObject(map, x, y, floor, door);
                return true;
            }
            else
                return false;
        }

        protected virtual bool PlaceDoorIfAccessibleAndNotAdjacent(Map map, int x, int y, TileModel floor, int minAccessibility, DoorWindow door)
        {
            int countWalkable = 0;

            Point p = new Point(x, y);
            foreach (Direction d in Direction.COMPASS)
            {
                Point next = p + d;
                if (map.IsWalkable(next.X, next.Y))
                    ++countWalkable;
                if (map.GetMapObjectAt(next.X, next.Y) is DoorWindow)
                    return false;
            }

            if (countWalkable >= minAccessibility)
            {
                PlaceDoorIfNoObject(map, x, y, floor, door);
                return true;
            }
            else
                return false;
        }
        #endregion

        #region Cars
        protected virtual void AddWreckedCarsOutside(Map map, Rectangle rect)
        {
            //////////////////////////////////////
            // Add random cars (+ on fire effect)
            //////////////////////////////////////
            base.MapObjectFill(map, rect,
                (pt) =>
                {
                    if (m_DiceRoller.RollChance(m_Params.WreckedCarChance))
                    {
                        Tile tile = map.GetTileAt(pt.X, pt.Y);
                        if (!tile.IsInside && tile.Model.IsWalkable && tile.Model != m_Game.GameTiles.FLOOR_GRASS)
                        {
                            MapObject car = base.MakeObjWreckedCar(m_DiceRoller);
                            if (m_DiceRoller.RollChance(50))
                            {
                                m_Game.ApplyOnFire(car);
                            }
                            return car;
                        }
                    }
                    return null;
                });
        }
        #endregion

        #region Concrete buildings
        protected bool IsThereASpecialBuilding(Map map, Rectangle rect)
        {
            // must not be a special building.
            List<Zone> zonesUpThere = map.GetZonesAt(rect.Left, rect.Top);
            if (zonesUpThere != null)
            {
                bool special = false;
                foreach (Zone z in zonesUpThere)
                    if (z.Name.Contains(RogueGame.NAME_SEWERS_MAINTENANCE) || z.Name.Contains(RogueGame.NAME_SUBWAY_STATION) || z.Name.Contains("office") || z.Name.Contains("shop"))
                    {
                        special = true;
                        break;
                    }
                if (special)
                    return true;
            }

            // must not have an exit.
            if (map.HasAnExitIn(rect))
                return true;

            // all clear.
            return false;
        }


        protected virtual bool MakeShopBuilding(Map map, Block b)
        {
            ////////////////////////
            // 0. Check suitability
            ////////////////////////
            if (b.InsideRect.Width < 5 || b.InsideRect.Height < 5)
                return false;

            /////////////////////////////
            // 1. Walkway, floor & walls
            /////////////////////////////
            base.TileRectangle(map, m_Game.GameTiles.FLOOR_WALKWAY, b.Rectangle);
            base.TileRectangle(map, m_Game.GameTiles.WALL_STONE, b.BuildingRect);
            base.TileFill(map, m_Game.GameTiles.FLOOR_TILES, b.InsideRect, (tile, prevmodel, x, y) => tile.IsInside = true);

            ///////////////////////
            // 2. Decide shop type
            ///////////////////////
            ShopType shopType = (ShopType)m_DiceRoller.Roll((int)ShopType._FIRST, (int)ShopType._COUNT);

            //////////////////////////////////////////
            // 3. Make sections alleys with displays.
            //////////////////////////////////////////            
            #region
            int alleysStartX = b.InsideRect.Left;
            int alleysStartY = b.InsideRect.Top;
            int alleysEndX = b.InsideRect.Right;
            int alleysEndY = b.InsideRect.Bottom;
            bool horizontalAlleys = b.Rectangle.Width >= b.Rectangle.Height;
            int centralAlley;

            if (horizontalAlleys)
            {
                ++alleysStartX;
                --alleysEndX;
                centralAlley = b.InsideRect.Left + b.InsideRect.Width / 2;
            }
            else
            {
                ++alleysStartY;
                --alleysEndY;
                centralAlley = b.InsideRect.Top + b.InsideRect.Height / 2;
            }
            Rectangle alleysRect = Rectangle.FromLTRB(alleysStartX, alleysStartY, alleysEndX, alleysEndY);

            base.MapObjectFill(map, alleysRect,
                (pt) =>
                {
                    bool addShelf;

                    if (horizontalAlleys)
                        addShelf = ((pt.Y - alleysRect.Top) % 2 == 1) && pt.X != centralAlley;
                    else
                        addShelf = ((pt.X - alleysRect.Left) % 2 == 1) && pt.Y != centralAlley;

                    if (addShelf)
                        return base.MakeObjShelf(GameImages.OBJ_SHOP_SHELF);
                    else
                        return null;
                });
            #endregion

            ///////////////////////////////
            // 4. Entry door with shop ids
            //    Might add window(s).
            ///////////////////////////////
            #region
            int midX = b.Rectangle.Left + b.Rectangle.Width / 2;
            int midY = b.Rectangle.Top + b.Rectangle.Height / 2;

            // make doors on one side.
            if (horizontalAlleys)
            {
                bool west = m_DiceRoller.RollChance(50);

                if (west)
                {

                    // west
                    PlaceDoor(map, b.BuildingRect.Left, midY, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                    if (b.InsideRect.Height >= 8)
                    {
                        PlaceDoor(map, b.BuildingRect.Left, midY - 1, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                        if (b.InsideRect.Height >= 12)
                            PlaceDoor(map, b.BuildingRect.Left, midY + 1, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                    }
                }
                else
                {
                    // east
                    PlaceDoor(map, b.BuildingRect.Right - 1, midY, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                    if (b.InsideRect.Height >= 8)
                    {
                        PlaceDoor(map, b.BuildingRect.Right - 1, midY - 1, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                        if (b.InsideRect.Height >= 12)
                            PlaceDoor(map, b.BuildingRect.Right - 1, midY + 1, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                    }
                }
            }
            else
            {
                bool north = m_DiceRoller.RollChance(50);

                if (north)
                {
                    // north
                    PlaceDoor(map, midX, b.BuildingRect.Top, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                    if (b.InsideRect.Width >= 8)
                    {
                        PlaceDoor(map, midX - 1, b.BuildingRect.Top, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                        if (b.InsideRect.Width >= 12)
                            PlaceDoor(map, midX + 1, b.BuildingRect.Top, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                    }
                }
                else
                {
                    // south
                    PlaceDoor(map, midX, b.BuildingRect.Bottom - 1, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                    if (b.InsideRect.Width >= 8)
                    {
                        PlaceDoor(map, midX - 1, b.BuildingRect.Bottom - 1, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                        if (b.InsideRect.Width >= 12)
                            PlaceDoor(map, midX + 1, b.BuildingRect.Bottom - 1, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                    }
                }
            }

            // add shop image next to doors.
            string shopImage;
            string shopName;
            switch (shopType)
            {
                case ShopType.CONSTRUCTION:
                    shopImage = GameImages.DECO_SHOP_CONSTRUCTION;
                    shopName = "Construction";
                    break;
                case ShopType.GENERAL_STORE:
                    shopImage = GameImages.DECO_SHOP_GENERAL_STORE;
                    shopName = "GeneralStore";
                    break;
                case ShopType.GROCERY:
                    shopImage = GameImages.DECO_SHOP_GROCERY;
                    shopName = "Grocery";
                    break;
                case ShopType.GUNSHOP:
                    shopImage = GameImages.DECO_SHOP_GUNSHOP;
                    shopName = "Gunshop";
                    break;
                case ShopType.PHARMACY:
                    shopImage = GameImages.DECO_SHOP_PHARMACY;
                    shopName = "Pharmacy";
                    break;
                case ShopType.SPORTSWEAR:
                    shopImage = GameImages.DECO_SHOP_SPORTSWEAR;
                    shopName = "Sportswear";
                    break;
                case ShopType.HUNTING:
                    shopImage = GameImages.DECO_SHOP_HUNTING;
                    shopName = "Hunting Shop";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("unhandled shoptype");
            }
            DecorateOutsideWalls(map, b.BuildingRect, (x, y) => map.GetMapObjectAt(x, y) == null && CountAdjDoors(map, x, y) >= 1 ? shopImage : null);

            // window?
            if (m_DiceRoller.RollChance(SHOP_WINDOW_CHANCE))
            {
                // pick a random side.
                int side = m_DiceRoller.Roll(0, 4);
                int wx, wy;
                switch (side)
                {
                    case 0: wx = b.BuildingRect.Left + b.BuildingRect.Width / 2; wy = b.BuildingRect.Top; break;
                    case 1: wx = b.BuildingRect.Left + b.BuildingRect.Width / 2; wy = b.BuildingRect.Bottom - 1; break;
                    case 2: wx = b.BuildingRect.Left; wy = b.BuildingRect.Top + b.BuildingRect.Height / 2; break;
                    case 3: wx = b.BuildingRect.Right - 1; wy = b.BuildingRect.Top + b.BuildingRect.Height / 2; break;
                    default: throw new ArgumentOutOfRangeException("unhandled side");
                }
                // check it is ok to make a window there.
                bool isGoodWindowPos = true;
                if (map.GetTileAt(wx, wy).Model.IsWalkable) isGoodWindowPos = false;
                // do it?
                if (isGoodWindowPos)
                {
                    PlaceDoor(map, wx, wy, m_Game.GameTiles.FLOOR_TILES, base.MakeObjWindow());
                }
            }

            // barricade certain shops types.
            if (shopType == ShopType.GUNSHOP)
            {
                BarricadeDoors(map, b.BuildingRect, Rules.BARRICADING_MAX);
            }

            #endregion

            ///////////////////////////
            // 5. Add items to shelves.
            ///////////////////////////
            #region
            base.ItemsDrop(map, b.InsideRect,
                (pt) =>
                {
                    MapObject mapObj = map.GetMapObjectAt(pt);
                    if (mapObj == null)
                        return false;
                    return mapObj.ImageID == GameImages.OBJ_SHOP_SHELF &&
                        m_DiceRoller.RollChance(m_Params.ItemInShopShelfChance);
                },
                (pt) => MakeRandomShopItem(shopType));
            #endregion

            ///////////
            // 6. Zone
            ///////////
            // shop building.
            map.AddZone(MakeUniqueZone(shopName, b.BuildingRect));
            // walkway zones.
            MakeWalkwayZones(map, b);

            ////////////////
            // 7. Basement?
            ////////////////
            #region
            if (m_DiceRoller.RollChance(SHOP_BASEMENT_CHANCE))
            {
                // shop basement map:                
                // - a single dark room.
                // - some shop items.

                // - a single dark room.
                Map shopBasement = new Map((map.Seed << 1) ^ shopName.GetHashCode(), "basement-" + shopName, b.BuildingRect.Width, b.BuildingRect.Height)
                {
                    Lighting = Lighting.DARKNESS
                };
                DoForEachTile(shopBasement, shopBasement.Rect, (pt) => shopBasement.GetTileAt(pt).IsInside = true);
                TileFill(shopBasement, m_Game.GameTiles.FLOOR_CONCRETE);
                TileRectangle(shopBasement, m_Game.GameTiles.WALL_BRICK, shopBasement.Rect);
                shopBasement.AddZone(MakeUniqueZone("basement", shopBasement.Rect));

                // - some shelves with shop items.
                // - some rats.
                DoForEachTile(shopBasement, shopBasement.Rect,
                    (pt) =>
                    {
                        if (!shopBasement.IsWalkable(pt.X, pt.Y))
                            return;
                        if (shopBasement.GetExitAt(pt) != null)
                            return;

                        if (m_DiceRoller.RollChance(SHOP_BASEMENT_SHELF_CHANCE_PER_TILE))
                        {
                            shopBasement.PlaceMapObjectAt(MakeObjShelf(GameImages.OBJ_SHOP_SHELF), pt);
                            if (m_DiceRoller.RollChance(SHOP_BASEMENT_ITEM_CHANCE_PER_SHELF))
                            {
                                Item it = MakeRandomShopItem(shopType);
                                if (it != null)
                                    shopBasement.DropItemAt(it, pt);
                            }
                        }

                        if (Rules.HasZombiesInBasements(m_Game.Session.GameMode))
                        {
                            if (m_DiceRoller.RollChance(SHOP_BASEMENT_ZOMBIE_RAT_CHANCE))
                                shopBasement.PlaceActorAt(CreateNewBasementRatZombie(0), pt);
                        }
                    });

                // alpha10 music
                shopBasement.BgMusic = GameMusics.SEWERS;

                // link maps, stairs in one corner.
                Point basementCorner = new Point();
                basementCorner.X = m_DiceRoller.RollChance(50) ? 1 : shopBasement.Width - 2;
                basementCorner.Y = m_DiceRoller.RollChance(50) ? 1 : shopBasement.Height - 2;
                Point shopCorner = new Point(basementCorner.X - 1 + b.InsideRect.Left, basementCorner.Y - 1 + b.InsideRect.Top);
                AddExit(shopBasement, basementCorner, map, shopCorner, GameImages.DECO_STAIRS_UP, true);
                AddExit(map, shopCorner, shopBasement, basementCorner, GameImages.DECO_STAIRS_DOWN, true);

                // remove any blocking object in the shop.
                MapObject blocker = map.GetMapObjectAt(shopCorner);
                if (blocker != null)
                    map.RemoveMapObjectAt(shopCorner.X, shopCorner.Y);

                // add map.
                m_Params.District.AddUniqueMap(shopBasement);
            }
            #endregion

            // Done
            return true;
        }

        /// <summary>
        /// Either an Office (for large enough buildings) or an Agency (for small buildings).
        /// </summary>
        /// <param name="map"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        protected virtual CHARBuildingType MakeCHARBuilding(Map map, Block b)
        {
            ///////////////////////////////
            // Offices are large buildings.
            // Agency are small ones.
            ///////////////////////////////
            if (b.InsideRect.Width < 8 || b.InsideRect.Height < 8)
            {
                // small, make it an Agency.
                if (MakeCHARAgency(map, b))
                    return CHARBuildingType.AGENCY;
                else
                    return CHARBuildingType.NONE;
            }
            else
            {
                if (MakeCHAROffice(map, b))
                    return CHARBuildingType.OFFICE;
                else
                    return CHARBuildingType.NONE;
            }
        }

        static string[] CHAR_POSTERS = { GameImages.DECO_CHAR_POSTER1, GameImages.DECO_CHAR_POSTER2, GameImages.DECO_CHAR_POSTER3 };

        protected virtual bool MakeCHARAgency(Map map, Block b)
        {
            /////////////////////////////
            // 1. Walkway, floor & walls
            /////////////////////////////
            base.TileRectangle(map, m_Game.GameTiles.FLOOR_WALKWAY, b.Rectangle);
            base.TileRectangle(map, m_Game.GameTiles.WALL_CHAR_OFFICE, b.BuildingRect);
            base.TileFill(map, m_Game.GameTiles.FLOOR_OFFICE, b.InsideRect,
                (tile, prevmodel, x, y) =>
                {
                    tile.IsInside = true;
                    tile.AddDecoration(GameImages.DECO_CHAR_FLOOR_LOGO);
                });

            //////////////////////////
            // 2. Decide orientation.
            //////////////////////////          
            bool horizontalCorridor = (b.InsideRect.Width >= b.InsideRect.Height);

            /////////////////
            // 3. Entry door 
            /////////////////
            #region
            int midX = b.Rectangle.Left + b.Rectangle.Width / 2;
            int midY = b.Rectangle.Top + b.Rectangle.Height / 2;

            // make doors on one side.
            #region
            if (horizontalCorridor)
            {
                bool west = m_DiceRoller.RollChance(50);

                if (west)
                {
                    // west
                    PlaceDoor(map, b.BuildingRect.Left, midY, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                    if (b.InsideRect.Height >= 8)
                    {
                        PlaceDoor(map, b.BuildingRect.Left, midY - 1, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                        if (b.InsideRect.Height >= 12)
                            PlaceDoor(map, b.BuildingRect.Left, midY + 1, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                    }
                }
                else
                {
                    // east
                    PlaceDoor(map, b.BuildingRect.Right - 1, midY, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                    if (b.InsideRect.Height >= 8)
                    {
                        PlaceDoor(map, b.BuildingRect.Right - 1, midY - 1, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                        if (b.InsideRect.Height >= 12)
                            PlaceDoor(map, b.BuildingRect.Right - 1, midY + 1, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                    }
                }
            }
            else
            {
                bool north = m_DiceRoller.RollChance(50);

                if (north)
                {
                    // north
                    PlaceDoor(map, midX, b.BuildingRect.Top, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                    if (b.InsideRect.Width >= 8)
                    {
                        PlaceDoor(map, midX - 1, b.BuildingRect.Top, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                        if (b.InsideRect.Width >= 12)
                            PlaceDoor(map, midX + 1, b.BuildingRect.Top, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                    }
                }
                else
                {
                    // south
                    PlaceDoor(map, midX, b.BuildingRect.Bottom - 1, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                    if (b.InsideRect.Width >= 8)
                    {
                        PlaceDoor(map, midX - 1, b.BuildingRect.Bottom - 1, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                        if (b.InsideRect.Width >= 12)
                            PlaceDoor(map, midX + 1, b.BuildingRect.Bottom - 1, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                    }
                }
            }
            #endregion

            // add office image next to doors.
            string officeImage = GameImages.DECO_CHAR_OFFICE;
            DecorateOutsideWalls(map, b.BuildingRect, (x, y) => map.GetMapObjectAt(x, y) == null && CountAdjDoors(map, x, y) >= 1 ? officeImage : null);
            #endregion

            ////////////////
            // 4. Furniture
            ////////////////
            #region
            // chairs on the sides.
            // alpha10.1 chance to add book/magazines
            MapObjectFill(map, b.InsideRect,
                (pt) =>
                {
                    if (CountAdjWalls(map, pt.X, pt.Y) < 3)
                        return null;

                    // alpha10.1 book/magazines on chair?
                    if (m_DiceRoller.RollChance(25))
                        map.DropItemAt(m_DiceRoller.RollChance(20) ? MakeItemBook() : MakeItemMagazines(), pt);

                    return MakeObjChair(GameImages.OBJ_CHAR_CHAIR);
                });
            // walls/pilars in the middle.
            TileFill(map, m_Game.GameTiles.WALL_CHAR_OFFICE, new Rectangle(b.InsideRect.Left + b.InsideRect.Width / 2 - 1, b.InsideRect.Top + b.InsideRect.Height / 2 - 1, 3, 2),
                (tile, model, x, y) =>
                {
                    tile.AddDecoration(CHAR_POSTERS[m_DiceRoller.Roll(0, CHAR_POSTERS.Length)]);
                });
            #endregion

            //////////////
            // 5. Posters
            //////////////
            #region
            // outside.
            DecorateOutsideWalls(map, b.BuildingRect,
                (x, y) =>
                {
                    if (CountAdjDoors(map, x, y) > 0)
                        return null;
                    else
                    {
                        if (m_DiceRoller.RollChance(25))
                            return CHAR_POSTERS[m_DiceRoller.Roll(0, CHAR_POSTERS.Length)];
                        else
                            return null;
                    }
                });
            #endregion

            ////////////
            // 6. Zones.
            ////////////
            map.AddZone(MakeUniqueZone("CHAR Agency", b.BuildingRect));
            MakeWalkwayZones(map, b);

            // Done
            return true;
        }

        protected virtual bool MakeCHAROffice(Map map, Block b)
        {

            /////////////////////////////
            // 1. Walkway, floor & walls
            /////////////////////////////
            base.TileRectangle(map, m_Game.GameTiles.FLOOR_WALKWAY, b.Rectangle);
            base.TileRectangle(map, m_Game.GameTiles.WALL_CHAR_OFFICE, b.BuildingRect);
            base.TileFill(map, m_Game.GameTiles.FLOOR_OFFICE, b.InsideRect, (tile, prevmodel, x, y) => tile.IsInside = true);

            //////////////////////////
            // 2. Decide orientation.
            //////////////////////////          
            bool horizontalCorridor = (b.InsideRect.Width >= b.InsideRect.Height);

            /////////////////
            // 3. Entry door 
            /////////////////
            #region
            int midX = b.Rectangle.Left + b.Rectangle.Width / 2;
            int midY = b.Rectangle.Top + b.Rectangle.Height / 2;
            Direction doorSide;

            // make doors on one side.
            #region
            if (horizontalCorridor)
            {
                bool west = m_DiceRoller.RollChance(50);

                if (west)
                {
                    doorSide = Direction.W;
                    // west
                    PlaceDoor(map, b.BuildingRect.Left, midY, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                    if (b.InsideRect.Height >= 8)
                    {
                        PlaceDoor(map, b.BuildingRect.Left, midY - 1, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                        if (b.InsideRect.Height >= 12)
                            PlaceDoor(map, b.BuildingRect.Left, midY + 1, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                    }
                }
                else
                {
                    doorSide = Direction.E;
                    // east
                    PlaceDoor(map, b.BuildingRect.Right - 1, midY, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                    if (b.InsideRect.Height >= 8)
                    {
                        PlaceDoor(map, b.BuildingRect.Right - 1, midY - 1, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                        if (b.InsideRect.Height >= 12)
                            PlaceDoor(map, b.BuildingRect.Right - 1, midY + 1, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                    }
                }
            }
            else
            {
                bool north = m_DiceRoller.RollChance(50);

                if (north)
                {
                    doorSide = Direction.N;
                    // north
                    PlaceDoor(map, midX, b.BuildingRect.Top, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                    if (b.InsideRect.Width >= 8)
                    {
                        PlaceDoor(map, midX - 1, b.BuildingRect.Top, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                        if (b.InsideRect.Width >= 12)
                            PlaceDoor(map, midX + 1, b.BuildingRect.Top, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                    }
                }
                else
                {
                    doorSide = Direction.S;
                    // south
                    PlaceDoor(map, midX, b.BuildingRect.Bottom - 1, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                    if (b.InsideRect.Width >= 8)
                    {
                        PlaceDoor(map, midX - 1, b.BuildingRect.Bottom - 1, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                        if (b.InsideRect.Width >= 12)
                            PlaceDoor(map, midX + 1, b.BuildingRect.Bottom - 1, m_Game.GameTiles.FLOOR_WALKWAY, base.MakeObjGlassDoor());
                    }
                }
            }
            #endregion

            // add office image next to doors.
            string officeImage = GameImages.DECO_CHAR_OFFICE;
            DecorateOutsideWalls(map, b.BuildingRect, (x, y) => map.GetMapObjectAt(x, y) == null && CountAdjDoors(map, x, y) >= 1 ? officeImage : null);

            // barricade entry doors.
            BarricadeDoors(map, b.BuildingRect, Rules.BARRICADING_MAX);
            #endregion

            ///////////////////////
            // 4. Make entry hall.
            ///////////////////////
            #region
            const int hallDepth = 3;
            if (doorSide == Direction.N)
            {
                base.TileHLine(map, m_Game.GameTiles.WALL_CHAR_OFFICE, b.InsideRect.Left, b.InsideRect.Top + hallDepth, b.InsideRect.Width);
            }
            else if (doorSide == Direction.S)
            {
                base.TileHLine(map, m_Game.GameTiles.WALL_CHAR_OFFICE, b.InsideRect.Left, b.InsideRect.Bottom - 1 - hallDepth, b.InsideRect.Width);
            }
            else if (doorSide == Direction.E)
            {
                base.TileVLine(map, m_Game.GameTiles.WALL_CHAR_OFFICE, b.InsideRect.Right - 1 - hallDepth, b.InsideRect.Top, b.InsideRect.Height);
            }
            else if (doorSide == Direction.W)
            {
                base.TileVLine(map, m_Game.GameTiles.WALL_CHAR_OFFICE, b.InsideRect.Left + hallDepth, b.InsideRect.Top, b.InsideRect.Height);
            }
            else
                throw new InvalidOperationException("unhandled door side");
            #endregion

            /////////////////////////////////////
            // 5. Make central corridor & wings
            /////////////////////////////////////
            #region
            Rectangle corridorRect;
            Point corridorDoor;
            if (doorSide == Direction.N)
            {
                corridorRect = new Rectangle(midX - 1, b.InsideRect.Top + hallDepth, 3, b.BuildingRect.Height - 1 - hallDepth);
                corridorDoor = new Point(corridorRect.Left + 1, corridorRect.Top);
            }
            else if (doorSide == Direction.S)
            {
                corridorRect = new Rectangle(midX - 1, b.BuildingRect.Top, 3, b.BuildingRect.Height - 1 - hallDepth);
                corridorDoor = new Point(corridorRect.Left + 1, corridorRect.Bottom - 1);
            }
            else if (doorSide == Direction.E)
            {
                corridorRect = new Rectangle(b.BuildingRect.Left, midY - 1, b.BuildingRect.Width - 1 - hallDepth, 3);
                corridorDoor = new Point(corridorRect.Right - 1, corridorRect.Top + 1);
            }
            else if (doorSide == Direction.W)
            {
                corridorRect = new Rectangle(b.InsideRect.Left + hallDepth, midY - 1, b.BuildingRect.Width - 1 - hallDepth, 3);
                corridorDoor = new Point(corridorRect.Left, corridorRect.Top + 1);
            }
            else
                throw new InvalidOperationException("unhandled door side");

            base.TileRectangle(map, m_Game.GameTiles.WALL_CHAR_OFFICE, corridorRect);
            PlaceDoor(map, corridorDoor.X, corridorDoor.Y, m_Game.GameTiles.FLOOR_OFFICE, base.MakeObjCharDoor());
            #endregion

            /////////////////////////
            // 6. Make office rooms.
            /////////////////////////
            #region
            // make wings.
            Rectangle wingOne;
            Rectangle wingTwo;
            if (horizontalCorridor)
            {
                // top side.
                wingOne = new Rectangle(corridorRect.Left, b.BuildingRect.Top, corridorRect.Width, 1 + corridorRect.Top - b.BuildingRect.Top);
                // bottom side.
                wingTwo = new Rectangle(corridorRect.Left, corridorRect.Bottom - 1, corridorRect.Width, 1 + b.BuildingRect.Bottom - corridorRect.Bottom);
            }
            else
            {
                // left side
                wingOne = new Rectangle(b.BuildingRect.Left, corridorRect.Top, 1 + corridorRect.Left - b.BuildingRect.Left, corridorRect.Height);
                // right side
                wingTwo = new Rectangle(corridorRect.Right - 1, corridorRect.Top, 1 + b.BuildingRect.Right - corridorRect.Right, corridorRect.Height);
            }

            // make rooms in each wing with doors leaving toward corridor.
            const int officeRoomsSize = 4;

            List<Rectangle> officesOne = new List<Rectangle>();
            MakeRoomsPlan(map, ref officesOne, wingOne, officeRoomsSize, officeRoomsSize);

            List<Rectangle> officesTwo = new List<Rectangle>();
            MakeRoomsPlan(map, ref officesTwo, wingTwo, officeRoomsSize, officeRoomsSize);

            List<Rectangle> allOffices = new List<Rectangle>(officesOne.Count + officesTwo.Count);
            allOffices.AddRange(officesOne);
            allOffices.AddRange(officesTwo);

            foreach (Rectangle roomRect in officesOne)
            {
                base.TileRectangle(map, m_Game.GameTiles.WALL_CHAR_OFFICE, roomRect);
                map.AddZone(MakeUniqueZone("Office room", roomRect));
            }
            foreach (Rectangle roomRect in officesTwo)
            {
                base.TileRectangle(map, m_Game.GameTiles.WALL_CHAR_OFFICE, roomRect);
                map.AddZone(MakeUniqueZone("Office room", roomRect));
            }

            foreach (Rectangle roomRect in officesOne)
            {
                if (horizontalCorridor)
                {
                    PlaceDoor(map, roomRect.Left + roomRect.Width / 2, roomRect.Bottom - 1, m_Game.GameTiles.FLOOR_OFFICE, base.MakeObjCharDoor());
                }
                else
                {
                    PlaceDoor(map, roomRect.Right - 1, roomRect.Top + roomRect.Height / 2, m_Game.GameTiles.FLOOR_OFFICE, base.MakeObjCharDoor());
                }
            }
            foreach (Rectangle roomRect in officesTwo)
            {
                if (horizontalCorridor)
                {
                    PlaceDoor(map, roomRect.Left + roomRect.Width / 2, roomRect.Top, m_Game.GameTiles.FLOOR_OFFICE, base.MakeObjCharDoor());
                }
                else
                {
                    PlaceDoor(map, roomRect.Left, roomRect.Top + roomRect.Height / 2, m_Game.GameTiles.FLOOR_OFFICE, base.MakeObjCharDoor());
                }
            }

            // tables with chairs.
            foreach (Rectangle roomRect in allOffices)
            {
                // table.
                Point tablePos = new Point(roomRect.Left + roomRect.Width / 2, roomRect.Top + roomRect.Height / 2);
                map.PlaceMapObjectAt(base.MakeObjTable(GameImages.OBJ_CHAR_TABLE), tablePos);

                // try to put chairs around.
                int nbChairs = 2;
                Rectangle insideRoom = new Rectangle(roomRect.Left + 1, roomRect.Top + 1, roomRect.Width - 2, roomRect.Height - 2);
                if (!insideRoom.IsEmpty)
                {
                    for (int i = 0; i < nbChairs; i++)
                    {
                        Rectangle adjTableRect = new Rectangle(tablePos.X - 1, tablePos.Y - 1, 3, 3);
                        adjTableRect.Intersect(insideRoom);
                        MapObjectPlaceInGoodPosition(map, adjTableRect,
                            (pt) => pt != tablePos,
                            m_DiceRoller,
                            (pt) => MakeObjChair(GameImages.OBJ_CHAR_CHAIR));
                    }
                }
            }
            #endregion

            ////////////////
            // 7. Add items.
            ////////////////
            #region
            // drop goodies in rooms.
            foreach (Rectangle roomRect in allOffices)
            {
                base.ItemsDrop(map, roomRect,
                    (pt) =>
                    {
                        Tile tile = map.GetTileAt(pt.X, pt.Y);
                        if (tile.Model != m_Game.GameTiles.FLOOR_OFFICE)
                            return false;
                        MapObject mapObj = map.GetMapObjectAt(pt);
                        if (mapObj != null)
                            return false;
                        return true;
                    },
                    (pt) => MakeRandomCHAROfficeItem());
            }
            #endregion

            ///////////
            // 8. Zone
            ///////////
            Zone zone = base.MakeUniqueZone("CHAR Office", b.BuildingRect);
            zone.SetGameAttribute<bool>(ZoneAttributes.IS_CHAR_OFFICE, true);
            map.AddZone(zone);
            MakeWalkwayZones(map, b);

            // Done
            return true;
        }

        protected virtual bool MakeParkBuilding(Map map, Block b)
        {
            ////////////////////////
            // 0. Check suitability
            ////////////////////////
            if (b.InsideRect.Width < 3 || b.InsideRect.Height < 3)
                return false;

            /////////////////////////////
            // 1. Grass, walkway & fence
            /////////////////////////////
            base.TileRectangle(map, m_Game.GameTiles.FLOOR_WALKWAY, b.Rectangle);
            base.TileFill(map, m_Game.GameTiles.FLOOR_GRASS, b.InsideRect);
            base.MapObjectFill(map, b.BuildingRect,
                (pt) =>
                {
                    bool placeFence = (pt.X == b.BuildingRect.Left || pt.X == b.BuildingRect.Right - 1 || pt.Y == b.BuildingRect.Top || pt.Y == b.BuildingRect.Bottom - 1);
                    if (placeFence)
                        return base.MakeObjFence(GameImages.OBJ_FENCE);
                    else
                        return null;
                });

            ///////////////////////////////
            // 2. Random trees and benches
            ///////////////////////////////
            base.MapObjectFill(map, b.InsideRect,
                (pt) =>
                {
                    bool placeTree = m_DiceRoller.RollChance(PARK_TREE_CHANCE);
                    if (placeTree)
                        return base.MakeObjTree(GameImages.OBJ_TREE);
                    else
                        return null;
                });

            base.MapObjectFill(map, b.InsideRect,
                (pt) =>
                {
                    bool placeBench = m_DiceRoller.RollChance(PARK_BENCH_CHANCE);
                    if (placeBench)
                        return base.MakeObjBench(GameImages.OBJ_BENCH);
                    else
                        return null;
                });

            ///////////////
            // 3. Entrance
            ///////////////
            int entranceFace = m_DiceRoller.Roll(0, 4);
            int ex, ey;
            switch (entranceFace)
            {
                case 0: // west
                    ex = b.BuildingRect.Left;
                    ey = b.BuildingRect.Top + b.BuildingRect.Height / 2;
                    break;
                case 1: // east
                    ex = b.BuildingRect.Right - 1;
                    ey = b.BuildingRect.Top + b.BuildingRect.Height / 2;
                    break;
                case 3: // north
                    ex = b.BuildingRect.Left + b.BuildingRect.Width / 2;
                    ey = b.BuildingRect.Top;
                    break;
                default: // south
                    ex = b.BuildingRect.Left + b.BuildingRect.Width / 2;
                    ey = b.BuildingRect.Bottom - 1;
                    break;
            }
            map.RemoveMapObjectAt(ex, ey);
            map.SetTileModelAt(ex, ey, m_Game.GameTiles.FLOOR_WALKWAY);

            ////////////
            // 4. Items
            ////////////
            base.ItemsDrop(map, b.InsideRect,
                (pt) => map.GetMapObjectAt(pt) == null && m_DiceRoller.RollChance(PARK_ITEM_CHANCE),
                (pt) => MakeRandomParkItem());

            ///////////
            // 5. Zone
            ///////////
            Zone parkZone = MakeUniqueZone("Park", b.BuildingRect);
            map.AddZone(parkZone);
            MakeWalkwayZones(map, b);

            // alpha10
            ////////////
            // 5. Shed?
            ////////////
            if (b.InsideRect.Width > PARK_SHED_WIDTH + 2 && b.InsideRect.Height > PARK_SHED_HEIGHT + 2)
            {
                if (m_DiceRoller.RollChance(PARK_SHED_CHANCE))
                {
                    // roll shed pos - dont put next to park fences!
                    int shedX = m_DiceRoller.Roll(b.InsideRect.Left + 1, b.InsideRect.Right - PARK_SHED_WIDTH);
                    int shedY = m_DiceRoller.Roll(b.InsideRect.Top + 1, b.InsideRect.Bottom - PARK_SHED_HEIGHT);
                    Rectangle shedRect = new Rectangle(shedX, shedY, PARK_SHED_WIDTH, PARK_SHED_HEIGHT);
                    Rectangle shedInsideRect = new Rectangle(shedX + 1, shedY + 1, PARK_SHED_WIDTH - 2, PARK_SHED_HEIGHT - 2);

                    // clear everything but zones in shed location
                    ClearRectangle(map, shedRect, false);

                    // build it
                    MakeParkShedBuilding(map, "Shed", shedRect);
                }
            }

            // Done.
            return true;
        }

        protected virtual void MakeParkShedBuilding(Map map, string baseZoneName, Rectangle shedBuildingRect)
        {
            Rectangle shedInsideRect = new Rectangle(shedBuildingRect.X + 1, shedBuildingRect.Y + 1, shedBuildingRect.Width - 2, shedBuildingRect.Height - 2);

            // build building & zone
            TileRectangle(map, m_Game.GameTiles.WALL_BRICK, shedBuildingRect);
            TileFill(map, m_Game.GameTiles.FLOOR_PLANKS, shedInsideRect, (tile, prevTileModel, x, y) => tile.IsInside = true);
            map.AddZone(MakeUniqueZone(baseZoneName, shedBuildingRect));

            // place shed door and make sure door front is cleared of objects (trees).
            int doorDir = m_DiceRoller.Roll(0, 4);
            int doorX, doorY;
            int doorFrontX, doorFrontY;
            switch (doorDir)
            {
                case 0: // west
                    doorX = shedBuildingRect.Left;
                    doorY = shedBuildingRect.Top + shedBuildingRect.Height / 2;
                    doorFrontX = doorX - 1;
                    doorFrontY = doorY;
                    break;
                case 1: // east
                    doorX = shedBuildingRect.Right - 1;
                    doorY = shedBuildingRect.Top + shedBuildingRect.Height / 2;
                    doorFrontX = doorX + 1;
                    doorFrontY = doorY;
                    break;
                case 3: // north
                    doorX = shedBuildingRect.Left + shedBuildingRect.Width / 2;
                    doorY = shedBuildingRect.Top;
                    doorFrontX = doorX;
                    doorFrontY = doorY - 1;
                    break;
                default: // south
                    doorX = shedBuildingRect.Left + shedBuildingRect.Width / 2;
                    doorY = shedBuildingRect.Bottom - 1;
                    doorFrontX = doorX;
                    doorFrontY = doorY + 1;
                    break;
            }
            PlaceDoor(map, doorX, doorY, m_Game.GameTiles.FLOOR_TILES, MakeObjWoodenDoor());
            map.RemoveMapObjectAt(doorFrontX, doorFrontY);

            // mark as inside and add shelves with tools
            DoForEachTile(map, shedInsideRect, (pt) =>
            {
                if (!map.IsWalkable(pt))
                    return;

                if (CountAdjDoors(map, pt.X, pt.Y) > 0)
                    return;

                if (CountAdjWalls(map, pt.X, pt.Y) == 0)
                    return;

                // shelf.
                map.PlaceMapObjectAt(MakeObjShelf(GameImages.OBJ_SHOP_SHELF), pt);

                // construction item (tools, lights)
                Item it = MakeShopConstructionItem();
                if (it.Model.IsStackable)
                    it.Quantity = it.Model.StackingLimit;
                map.DropItemAt(it, pt);
            });
        }

        // alpha10.1 makes apartements or vanilla house
        protected virtual bool MakeHousingBuilding(Map map, Block b)
        {
            // alpha10.1 decide floorplan
            // apartment?
            if (m_DiceRoller.RollChance(HOUSE_IS_APARTMENTS_CHANCE))
                if (MakeApartmentsBuilding(map, b))
                    return true;

            // vanilla house?
            return MakeVanillaHousingBuilding(map, b);
        }

        // alpha10.1 apartment houses
        protected virtual bool MakeApartmentsBuilding(Map map, Block b)
        {
            ////////////////////////
            // 0. Check suitability
            ////////////////////////
            if (b.InsideRect.Width < 9 || b.InsideRect.Height < 9)
                return false;
            if (b.InsideRect.Width > 17 || b.InsideRect.Height > 17)
                return false;

            // I pretty much copied and edited the char office algorithm. lame but i'm lazy.

            /////////////////////////////
            // 1. Walkway, floor & walls
            /////////////////////////////
            base.TileRectangle(map, m_Game.GameTiles.FLOOR_WALKWAY, b.Rectangle);
            base.TileRectangle(map, m_Game.GameTiles.WALL_BRICK, b.BuildingRect);
            base.TileFill(map, m_Game.GameTiles.FLOOR_PLANKS, b.InsideRect, (tile, prevmodel, x, y) => tile.IsInside = true);

            //////////////////////////
            // 2. Decide orientation.
            //////////////////////////          
            bool horizontalCorridor = (b.InsideRect.Width >= b.InsideRect.Height);

            /////////////////////////////////////
            // 3. Entry door and opposite window
            /////////////////////////////////////
            #region
            int midX = b.Rectangle.Left + b.Rectangle.Width / 2;
            int midY = b.Rectangle.Top + b.Rectangle.Height / 2;
            Direction doorSide;

            if (horizontalCorridor)
            {
                bool west = m_DiceRoller.RollChance(50);

                if (west)
                {
                    doorSide = Direction.W;
                    // west
                    PlaceDoor(map, b.BuildingRect.Left, midY, m_Game.GameTiles.FLOOR_PLANKS, base.MakeObjWoodenDoor());
                    PlaceDoor(map, b.BuildingRect.Right - 1, midY, m_Game.GameTiles.FLOOR_PLANKS, base.MakeObjWindow());
                }
                else
                {
                    doorSide = Direction.E;
                    // east
                    PlaceDoor(map, b.BuildingRect.Right - 1, midY, m_Game.GameTiles.FLOOR_PLANKS, base.MakeObjWoodenDoor());
                    PlaceDoor(map, b.BuildingRect.Left, midY, m_Game.GameTiles.FLOOR_PLANKS, base.MakeObjWindow());
                }
            }
            else
            {
                bool north = m_DiceRoller.RollChance(50);

                if (north)
                {
                    doorSide = Direction.N;
                    // north
                    PlaceDoor(map, midX, b.BuildingRect.Top, m_Game.GameTiles.FLOOR_PLANKS, base.MakeObjWoodenDoor());
                    PlaceDoor(map, midX, b.BuildingRect.Bottom - 1, m_Game.GameTiles.FLOOR_PLANKS, base.MakeObjWindow());
                }
                else
                {
                    doorSide = Direction.S;
                    // south
                    PlaceDoor(map, midX, b.BuildingRect.Bottom - 1, m_Game.GameTiles.FLOOR_PLANKS, base.MakeObjWoodenDoor());
                    PlaceDoor(map, midX, b.BuildingRect.Top, m_Game.GameTiles.FLOOR_PLANKS, base.MakeObjWindow());
                }
            }
            #endregion

            //////////////////////////////////////////////
            // 4. Make central corridor & side apartments
            //////////////////////////////////////////////
            #region
            Rectangle corridorRect;
            if (doorSide == Direction.N)
                corridorRect = new Rectangle(midX, b.InsideRect.Top, 1, b.BuildingRect.Height - 1);
            else if (doorSide == Direction.S)
                corridorRect = new Rectangle(midX, b.BuildingRect.Top, 1, b.BuildingRect.Height - 1);
            else if (doorSide == Direction.E)
                corridorRect = new Rectangle(b.BuildingRect.Left, midY, b.BuildingRect.Width - 1, 1);
            else if (doorSide == Direction.W)
                corridorRect = new Rectangle(b.InsideRect.Left, midY, b.BuildingRect.Width - 1, 1);
            else
                throw new InvalidOperationException("apartment: unhandled door side");
            #endregion

            //////////////////////
            // 5. Make apartments
            //////////////////////
            #region
            // make wings.
            Rectangle wingOne;
            Rectangle wingTwo;
            if (horizontalCorridor)
            {
                // top side.
                wingOne = Rectangle.FromLTRB(b.BuildingRect.Left, b.BuildingRect.Top, b.BuildingRect.Right, corridorRect.Top);
                // bottom side.
                wingTwo = Rectangle.FromLTRB(b.BuildingRect.Left, corridorRect.Bottom, b.BuildingRect.Right, b.BuildingRect.Bottom);
            }
            else
            {
                // left side
                wingOne = Rectangle.FromLTRB(b.BuildingRect.Left, b.BuildingRect.Top, corridorRect.Left, b.BuildingRect.Bottom);
                // right side
                wingTwo = Rectangle.FromLTRB(corridorRect.Right, b.BuildingRect.Top, b.BuildingRect.Right, b.BuildingRect.Bottom);
            }

            // make apartements in each wing with doors leaving toward corridor and windows to the outside
            // pick sizes so the apartements are not cut into multiple rooms by MakeRoomsPlan
            int apartmentMinXSize, apartmentMinYSize;
            if (horizontalCorridor)
            {
                apartmentMinXSize = 4;
                apartmentMinYSize = b.BuildingRect.Height / 2;
            }
            else
            {
                apartmentMinXSize = b.BuildingRect.Width / 2;
                apartmentMinYSize = 4;
            }

            List<Rectangle> apartementsWingOne = new List<Rectangle>();
            MakeRoomsPlan(map, ref apartementsWingOne, wingOne, apartmentMinXSize, apartmentMinYSize);
            List<Rectangle> apartementsWingTwo = new List<Rectangle>();
            MakeRoomsPlan(map, ref apartementsWingTwo, wingTwo, apartmentMinXSize, apartmentMinYSize);

            List<Rectangle> allApartments = new List<Rectangle>(apartementsWingOne.Count + apartementsWingTwo.Count);
            allApartments.AddRange(apartementsWingOne);
            allApartments.AddRange(apartementsWingTwo);

            foreach (Rectangle apartRect in apartementsWingOne)
                base.TileRectangle(map, m_Game.GameTiles.WALL_BRICK, apartRect);
            foreach (Rectangle roomRect in apartementsWingTwo)
                base.TileRectangle(map, m_Game.GameTiles.WALL_BRICK, roomRect);

            // put door leading to corridor; and an opposite window if outer wall / a door if inside
            foreach (Rectangle apartRect in apartementsWingOne)
            {
                if (horizontalCorridor)
                {
                    PlaceDoor(map, apartRect.Left + apartRect.Width / 2, apartRect.Bottom - 1, m_Game.GameTiles.FLOOR_PLANKS, base.MakeObjWoodenDoor());
                    PlaceDoor(map, apartRect.Left + apartRect.Width / 2, apartRect.Top, m_Game.GameTiles.FLOOR_PLANKS, base.MakeObjWindow());
                }
                else
                {
                    PlaceDoor(map, apartRect.Right - 1, apartRect.Top + apartRect.Height / 2, m_Game.GameTiles.FLOOR_PLANKS, base.MakeObjWoodenDoor());
                    PlaceDoor(map, apartRect.Left, apartRect.Top + apartRect.Height / 2, m_Game.GameTiles.FLOOR_PLANKS, base.MakeObjWindow());
                }
            }
            foreach (Rectangle apartRect in apartementsWingTwo)
            {
                if (horizontalCorridor)
                {
                    PlaceDoor(map, apartRect.Left + apartRect.Width / 2, apartRect.Top, m_Game.GameTiles.FLOOR_PLANKS, base.MakeObjWoodenDoor());
                    PlaceDoor(map, apartRect.Left + apartRect.Width / 2, apartRect.Bottom - 1, m_Game.GameTiles.FLOOR_PLANKS, base.MakeObjWindow());
                }
                else
                {
                    PlaceDoor(map, apartRect.Left, apartRect.Top + apartRect.Height / 2, m_Game.GameTiles.FLOOR_PLANKS, base.MakeObjWoodenDoor());
                    PlaceDoor(map, apartRect.Right - 1, apartRect.Top + apartRect.Height / 2, m_Game.GameTiles.FLOOR_PLANKS, base.MakeObjWindow());
                }
            }

            // fill appartements with furniture and items
            // an "apartement" is one big room that fits all the housing roles: bedroom, kitchen and living room.
            foreach (Rectangle apartRect in allApartments)
            {
                // bedroom
                FillHousingRoomContents(map, apartRect, 0);
                // kitchen
                FillHousingRoomContents(map, apartRect, 8);
                // living room
                FillHousingRoomContents(map, apartRect, 5);
            }
            #endregion

            ///////////
            // 6. Zone
            ///////////
            Zone zone = base.MakeUniqueZone("Apartements", b.BuildingRect);
            map.AddZone(zone);
            MakeWalkwayZones(map, b);

            // done
            return true;
        }

        // alpha10.1 pre alpha10.1 regular houses
        protected virtual bool MakeVanillaHousingBuilding(Map map, Block b)
        {
            ////////////////////////
            // 0. Check suitability
            ////////////////////////
            if (b.InsideRect.Width < 4 || b.InsideRect.Height < 4)
                return false;

            /////////////////////////////
            // 1. Walkway, floor & walls
            /////////////////////////////
            base.TileRectangle(map, m_Game.GameTiles.FLOOR_WALKWAY, b.Rectangle);
            base.TileRectangle(map, m_Game.GameTiles.WALL_BRICK, b.BuildingRect);
            base.TileFill(map, m_Game.GameTiles.FLOOR_PLANKS, b.InsideRect, (tile, prevmodel, x, y) => tile.IsInside = true);

            ///////////////////////
            // 2. Rooms floor plan
            ///////////////////////
            List<Rectangle> roomsList = new List<Rectangle>();
            MakeRoomsPlan(map, ref roomsList, b.BuildingRect, 5, 5);

            /////////////////
            // 3. Make rooms
            /////////////////
            // alpha10 make some housings floor plan non rectangular by randomly chosing not to place one border room
            // and replace it with a special "outside" room : a garden, a parking lot.

            int iOutsideRoom = -1;
            HouseOutsideRoomType outsideRoom = HouseOutsideRoomType._FIRST;
            if (roomsList.Count >= HOUSE_OUTSIDE_ROOM_NEED_MIN_ROOMS && m_DiceRoller.RollChance(HOUSE_OUTSIDE_ROOM_CHANCE))
            {
                for (; ; )
                {
                    iOutsideRoom = m_DiceRoller.Roll(0, roomsList.Count);
                    Rectangle r = roomsList[iOutsideRoom];
                    if (r.Left == b.BuildingRect.Left || r.Right == b.BuildingRect.Right || r.Top == b.BuildingRect.Top || r.Bottom == b.BuildingRect.Bottom)
                        break;
                }
                outsideRoom = (HouseOutsideRoomType)m_DiceRoller.Roll((int)HouseOutsideRoomType._FIRST, (int)HouseOutsideRoomType._COUNT);
            }

            for (int i = 0; i < roomsList.Count; i++)
            {
                Rectangle roomRect = roomsList[i];
                if (iOutsideRoom == i)
                {
                    // make sure all tiles are marked as outside
                    base.DoForEachTile(map, roomRect, (pt) => map.GetTileAt(pt).IsInside = false);

                    // then shrink it properly so we dont overlap with tiles from other rooms and mess things up.
                    if (roomRect.Left != b.BuildingRect.Left)
                    {
                        roomRect.X++;
                        roomRect.Width--;
                    }
                    if (roomRect.Right != b.BuildingRect.Right)
                    {
                        roomRect.Width--;
                    }
                    if (roomRect.Top != b.BuildingRect.Top)
                    {
                        roomRect.Y++;
                        roomRect.Height--;
                    }
                    if (roomRect.Bottom != b.BuildingRect.Bottom)
                    {
                        roomRect.Height--;
                    }

                    // then fill the outside room
                    switch (outsideRoom)
                    {
                        case HouseOutsideRoomType.GARDEN:
                            base.TileFill(map, m_Game.GameTiles.FLOOR_GRASS, roomRect);
                            base.DoForEachTile(map, roomRect,
                                (pos) =>
                                {
                                    if (map.GetTileAt(pos).Model == m_Game.GameTiles.FLOOR_GRASS && m_DiceRoller.RollChance(HOUSE_GARDEN_TREE_CHANCE))
                                        map.PlaceMapObjectAt(MakeObjTree(GameImages.OBJ_TREE), pos);
                                });
                            break;

                        case HouseOutsideRoomType.PARKING_LOT:
                            base.TileFill(map, m_Game.GameTiles.FLOOR_ASPHALT, roomRect);
                            base.DoForEachTile(map, roomRect,
                                (pos) =>
                                {
                                    if (map.GetTileAt(pos).Model == m_Game.GameTiles.FLOOR_ASPHALT && m_DiceRoller.RollChance(HOUSE_PARKING_LOT_CAR_CHANCE))
                                        map.PlaceMapObjectAt(MakeObjWreckedCar(m_DiceRoller), pos);
                                });
                            break;
                    }
                }
                else
                {
                    MakeHousingRoom(map, roomRect, m_Game.GameTiles.FLOOR_PLANKS, m_Game.GameTiles.WALL_BRICK);
                    FillHousingRoomContents(map, roomRect);
                }
            }

            // once all rooms are done, enclose the outside room
            if (iOutsideRoom != -1)
            {
                Rectangle roomRect = roomsList[iOutsideRoom];
                switch (outsideRoom)
                {
                    case HouseOutsideRoomType.GARDEN:
                        base.DoForEachTile(map, roomRect,
                            (pos) =>
                            {
                                if ((pos.X == roomRect.Left || pos.X == roomRect.Right - 1 || pos.Y == roomRect.Top || pos.Y == roomRect.Bottom - 1) && map.GetTileAt(pos).Model == m_Game.GameTiles.FLOOR_GRASS)
                                {
                                    map.RemoveMapObjectAt(pos.X, pos.Y); // make sure trees are removed
                                    map.PlaceMapObjectAt(MakeObjFence(GameImages.OBJ_GARDEN_FENCE, MapObject.Fire.BURNABLE, DoorWindow.BASE_HITPOINTS / 2), pos);
                                }
                            });
                        break;

                    case HouseOutsideRoomType.PARKING_LOT:
                        base.DoForEachTile(map, roomRect,
                            (pos) =>
                            {
                                bool isLotEntry = (pos.X == roomRect.Left + roomRect.Width / 2) || (pos.Y == roomRect.Top + roomRect.Height / 2);
                                if (!isLotEntry && ((pos.X == roomRect.Left || pos.X == roomRect.Right - 1 || pos.Y == roomRect.Top || pos.Y == roomRect.Bottom - 1) && map.GetTileAt(pos).Model == m_Game.GameTiles.FLOOR_ASPHALT))
                                {
                                    map.RemoveMapObjectAt(pos.X, pos.Y); // make sure cars are removed
                                    map.PlaceMapObjectAt(MakeObjWireFence(GameImages.OBJ_WIRE_FENCE), pos);
                                }
                            });
                        break;
                }
            }

            ///////////////////////////////////////
            // 5. Fix buildings with no door exits
            ///////////////////////////////////////
            #region
            bool hasOutsideDoor = false;
            for (int x = b.BuildingRect.Left; x < b.BuildingRect.Right && !hasOutsideDoor; x++)
                for (int y = b.BuildingRect.Top; y < b.BuildingRect.Bottom && !hasOutsideDoor; y++)
                {
                    if (!map.GetTileAt(x, y).IsInside)
                    {
                        DoorWindow door = map.GetMapObjectAt(x, y) as DoorWindow;
                        if (door != null && !door.IsWindow)
                            hasOutsideDoor = true;
                    }
                }
            if (!hasOutsideDoor)
            {
                // replace a random window with a door.
                // alpha10 list all the exit windows, pick one and replace with a door.

                // list all exit windows
                List<Point> buildingExits = new List<Point>(8);
                for (int x = b.BuildingRect.Left; x < b.BuildingRect.Right; x++)
                    for (int y = b.BuildingRect.Top; y < b.BuildingRect.Bottom; y++)
                    {
                        if (!map.GetTileAt(x, y).IsInside)
                        {
                            DoorWindow window = map.GetMapObjectAt(x, y) as DoorWindow;
                            if (window != null && window.IsWindow)
                            {
                                buildingExits.Add(new Point(x, y));
                            }
                        }
                    }

                // replace an exit window with a door
                if (buildingExits.Count > 0)
                {
                    Point newDoorPos = buildingExits[m_DiceRoller.Roll(0, buildingExits.Count)];
                    map.RemoveMapObjectAt(newDoorPos.X, newDoorPos.Y);
                    map.PlaceMapObjectAt(MakeObjWoodenDoor(), newDoorPos);
                    hasOutsideDoor = true;
                }

                // if we did not found an exit window to replace this is a bug, it should never happen.
                // i'm lazy and assume this never happens and throw an exception.
                if (hasOutsideDoor == false)
                {
                    Logger.WriteLine(Logger.Stage.RUN_MAIN, "ERROR: house has no exit, should never happen; sector@" + map.District.WorldPosition + " house@" + b.BuildingRect);
                    throw new Exception("house has not exit, should never happen. read the log.");
                }
            }
            #endregion

            ////////////////
            // 6. Basement?
            ////////////////
            #region
            if (m_DiceRoller.RollChance(HOUSE_BASEMENT_CHANCE))
            {
                Map basementMap = GenerateHouseBasementMap(map, b);
                m_Params.District.AddUniqueMap(basementMap);
            }
            #endregion

            ///////////
            // 7. Zone
            ///////////
            #region
            map.AddZone(MakeUniqueZone("Housing", b.BuildingRect));
            MakeWalkwayZones(map, b);
            #endregion

            // Done
            return true;
        }

        protected virtual void MakeSewersMaintenanceBuilding(Map map, bool isSurface, Block b, Map linkedMap, Point exitPosition)
        {
            ///////////////
            // Outer walls.
            ///////////////
            // if sewers dig room.
            if (!isSurface)
                TileFill(map, m_Game.GameTiles.FLOOR_CONCRETE, b.InsideRect);
            // outer walls.
            TileRectangle(map, m_Game.GameTiles.WALL_SEWER, b.BuildingRect);
            // make sure its marked as inside (in case we replace a park for instance)
            for (int x = b.InsideRect.Left; x < b.InsideRect.Right; x++)
                for (int y = b.InsideRect.Top; y < b.InsideRect.Bottom; y++)
                    map.GetTileAt(x, y).IsInside = true;

            //////////////////
            // Entrance door.
            //////////////////
            // pick door side and put tags.
            #region
            int doorX, doorY;
            Direction digDirection;
            int sideRoll = m_DiceRoller.Roll(0, 4);
            switch (sideRoll)
            {
                case 0: // north.
                    digDirection = Direction.N;
                    doorX = b.BuildingRect.Left + b.BuildingRect.Width / 2;
                    doorY = b.BuildingRect.Top;

                    map.GetTileAt(doorX - 1, doorY).AddDecoration(GameImages.DECO_SEWERS_BUILDING);
                    map.GetTileAt(doorX + 1, doorY).AddDecoration(GameImages.DECO_SEWERS_BUILDING);
                    break;

                case 1: // south.
                    digDirection = Direction.S;
                    doorX = b.BuildingRect.Left + b.BuildingRect.Width / 2;
                    doorY = b.BuildingRect.Bottom - 1;

                    map.GetTileAt(doorX - 1, doorY).AddDecoration(GameImages.DECO_SEWERS_BUILDING);
                    map.GetTileAt(doorX + 1, doorY).AddDecoration(GameImages.DECO_SEWERS_BUILDING);
                    break;

                case 2: // west.
                    digDirection = Direction.W;
                    doorX = b.BuildingRect.Left;
                    doorY = b.BuildingRect.Top + b.BuildingRect.Height / 2;


                    map.GetTileAt(doorX, doorY - 1).AddDecoration(GameImages.DECO_SEWERS_BUILDING);
                    map.GetTileAt(doorX, doorY + 1).AddDecoration(GameImages.DECO_SEWERS_BUILDING);
                    break;

                case 3: // east.
                    digDirection = Direction.E;
                    doorX = b.BuildingRect.Right - 1;
                    doorY = b.BuildingRect.Top + b.BuildingRect.Height / 2;


                    map.GetTileAt(doorX, doorY - 1).AddDecoration(GameImages.DECO_SEWERS_BUILDING);
                    map.GetTileAt(doorX, doorY + 1).AddDecoration(GameImages.DECO_SEWERS_BUILDING);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("unhandled roll");
            }
            // add the door.
            PlaceDoor(map, doorX, doorY, m_Game.GameTiles.FLOOR_CONCRETE, MakeObjIronDoor());
            BarricadeDoors(map, b.BuildingRect, Rules.BARRICADING_MAX);
            #endregion

            /////////////////////////////////
            // Hole/Ladder to sewers/surface.
            /////////////////////////////////
            // add exit.
            map.GetTileAt(exitPosition.X, exitPosition.Y).AddDecoration(isSurface ? GameImages.DECO_SEWER_HOLE : GameImages.DECO_SEWER_LADDER);
            map.SetExitAt(exitPosition, new Exit(linkedMap, exitPosition) { IsAnAIExit = true });

            ///////////////////////////////////////////////////
            // If sewers, dig corridor until we reach a tunnel.
            ///////////////////////////////////////////////////
            if (!isSurface)
            {
                Point digPos = new Point(doorX, doorY) + digDirection;
                while (map.IsInBounds(digPos) && !map.GetTileAt(digPos.X, digPos.Y).Model.IsWalkable)
                {
                    // corridor.
                    map.SetTileModelAt(digPos.X, digPos.Y, m_Game.GameTiles.FLOOR_CONCRETE);
                    // continue digging.
                    digPos += digDirection;
                }
            }

            /////////////////////
            // Furniture & Items.
            /////////////////////
            // bunch of tables near walls with construction items on them.
            int nbTables = m_DiceRoller.Roll(Math.Max(b.InsideRect.Width, b.InsideRect.Height), 2 * Math.Max(b.InsideRect.Width, b.InsideRect.Height));
            for (int i = 0; i < nbTables; i++)
            {
                MapObjectPlaceInGoodPosition(map, b.InsideRect,
                    (pt) => CountAdjWalls(map, pt.X, pt.Y) >= 3 && CountAdjDoors(map, pt.X, pt.Y) == 0,
                    m_DiceRoller,
                    (pt) =>
                    {
                        // add item.
                        map.DropItemAt(MakeShopConstructionItem(), pt);

                        // add table.
                        return MakeObjTable(GameImages.OBJ_TABLE);
                    });
            }
            // a bed and a fridge with food if lucky.
            if (m_DiceRoller.RollChance(33))
            {
                // bed.
                MapObjectPlaceInGoodPosition(map, b.InsideRect,
                    (pt) => CountAdjWalls(map, pt.X, pt.Y) >= 3 && CountAdjDoors(map, pt.X, pt.Y) == 0,
                    m_DiceRoller,
                    (pt) => MakeObjBed(GameImages.OBJ_BED));

                // fridge + food.
                MapObjectPlaceInGoodPosition(map, b.InsideRect,
                    (pt) => CountAdjWalls(map, pt.X, pt.Y) >= 3 && CountAdjDoors(map, pt.X, pt.Y) == 0,
                    m_DiceRoller,
                    (pt) =>
                    {
                        // add food.
                        map.DropItemAt(MakeItemCannedFood(), pt);

                        // add fridge.
                        return MakeObjFridge(GameImages.OBJ_FRIDGE);
                    });
            }

            ////////////////////////////////////
            // Add the poor maintenance guy/gal.
            ////////////////////////////////////
            Actor poorGuy = CreateNewCivilian(0, 3, 1);
            ActorPlace(m_DiceRoller, b.Rectangle.Width * b.Rectangle.Height, map, poorGuy, b.InsideRect.Left, b.InsideRect.Top, b.InsideRect.Width, b.InsideRect.Height);

            //////////////
            // Make zone.
            //////////////
            map.AddZone(MakeUniqueZone(RogueGame.NAME_SEWERS_MAINTENANCE, b.BuildingRect));

            // Done...
        }

        protected virtual void MakeSubwayStationBuilding(Map map, bool isSurface, Block b, Map linkedMap, Point exitPosition)
        {
            ///////////////
            // Outer walls.
            ///////////////
            #region
            // if sewers dig room.
            if (!isSurface)
                TileFill(map, m_Game.GameTiles.FLOOR_CONCRETE, b.InsideRect);
            // outer walls.
            TileRectangle(map, m_Game.GameTiles.WALL_SUBWAY, b.BuildingRect);
            // make sure its marked as inside (in case we replace a park for instance)
            for (int x = b.InsideRect.Left; x < b.InsideRect.Right; x++)
                for (int y = b.InsideRect.Top; y < b.InsideRect.Bottom; y++)
                    map.GetTileAt(x, y).IsInside = true;
            #endregion

            ////////////
            // Entrance
            ////////////
            #region
            // pick door/corridor side and put tags.
            // if not surface, we must dig toward the rails.
            int entryFenceX, entryFenceY;
            Direction digDirection;
            int sideRoll;
            if (isSurface)
                sideRoll = m_DiceRoller.Roll(0, 4);
            else
                sideRoll = b.Rectangle.Bottom < map.Width / 2 ? 1 : 0;
            switch (sideRoll)
            {
                case 0: // north.
                    digDirection = Direction.N;
                    entryFenceX = b.BuildingRect.Left + b.BuildingRect.Width / 2;
                    entryFenceY = b.BuildingRect.Top;

                    if (isSurface)
                    {
                        map.GetTileAt(entryFenceX - 1, entryFenceY).AddDecoration(GameImages.DECO_SUBWAY_BUILDING);
                        map.GetTileAt(entryFenceX + 1, entryFenceY).AddDecoration(GameImages.DECO_SUBWAY_BUILDING);
                    }
                    break;

                case 1: // south.
                    digDirection = Direction.S;
                    entryFenceX = b.BuildingRect.Left + b.BuildingRect.Width / 2;
                    entryFenceY = b.BuildingRect.Bottom - 1;

                    if (isSurface)
                    {
                        map.GetTileAt(entryFenceX - 1, entryFenceY).AddDecoration(GameImages.DECO_SUBWAY_BUILDING);
                        map.GetTileAt(entryFenceX + 1, entryFenceY).AddDecoration(GameImages.DECO_SUBWAY_BUILDING);
                    }
                    break;

                case 2: // west.
                    digDirection = Direction.W;
                    entryFenceX = b.BuildingRect.Left;
                    entryFenceY = b.BuildingRect.Top + b.BuildingRect.Height / 2;

                    if (isSurface)
                    {
                        map.GetTileAt(entryFenceX, entryFenceY - 1).AddDecoration(GameImages.DECO_SUBWAY_BUILDING);
                        map.GetTileAt(entryFenceX, entryFenceY + 1).AddDecoration(GameImages.DECO_SUBWAY_BUILDING);
                    }
                    break;

                case 3: // east.
                    digDirection = Direction.E;
                    entryFenceX = b.BuildingRect.Right - 1;
                    entryFenceY = b.BuildingRect.Top + b.BuildingRect.Height / 2;

                    if (isSurface)
                    {
                        map.GetTileAt(entryFenceX, entryFenceY - 1).AddDecoration(GameImages.DECO_SUBWAY_BUILDING);
                        map.GetTileAt(entryFenceX, entryFenceY + 1).AddDecoration(GameImages.DECO_SUBWAY_BUILDING);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("unhandled roll");
            }
            // add door if surface.
            if (isSurface)
            {
                map.SetTileModelAt(entryFenceX, entryFenceY, m_Game.GameTiles.FLOOR_CONCRETE);
                map.PlaceMapObjectAt(MakeObjGlassDoor(), new Point(entryFenceX, entryFenceY));
            }
            #endregion

            ///////////////////////////
            // Stairs to the other map.
            ///////////////////////////
            #region
            // add exits.
            for (int ex = exitPosition.X - 1; ex <= exitPosition.X + 1; ex++)
            {
                Point thisExitPos = new Point(ex, exitPosition.Y);
                map.GetTileAt(thisExitPos.X, thisExitPos.Y).AddDecoration(isSurface ? GameImages.DECO_STAIRS_DOWN : GameImages.DECO_STAIRS_UP);
                map.SetExitAt(thisExitPos, new Exit(linkedMap, thisExitPos) { IsAnAIExit = true });
            }
            #endregion

            ///////////////////////////////////////////////////
            // If subway : 
            // - dig corridor until we reach the rails.
            // - dig platform and make corridor zone.
            // - add closed iron fences between corridor and platform.
            // - make power room.
            ///////////////////////////////////////////////////
            #region
            if (!isSurface)
            {
                // - dig corridor until we reach the rails.
                #region
                map.SetTileModelAt(entryFenceX, entryFenceY, m_Game.GameTiles.FLOOR_CONCRETE);
                map.SetTileModelAt(entryFenceX + 1, entryFenceY, m_Game.GameTiles.FLOOR_CONCRETE);
                map.SetTileModelAt(entryFenceX - 1, entryFenceY, m_Game.GameTiles.FLOOR_CONCRETE);
                map.SetTileModelAt(entryFenceX - 2, entryFenceY, m_Game.GameTiles.WALL_STONE);
                map.SetTileModelAt(entryFenceX + 2, entryFenceY, m_Game.GameTiles.WALL_STONE);

                Point digPos = new Point(entryFenceX, entryFenceY) + digDirection;
                while (map.IsInBounds(digPos) && !map.GetTileAt(digPos.X, digPos.Y).Model.IsWalkable)
                {
                    // corridor.
                    map.SetTileModelAt(digPos.X, digPos.Y, m_Game.GameTiles.FLOOR_CONCRETE);
                    map.SetTileModelAt(digPos.X - 1, digPos.Y, m_Game.GameTiles.FLOOR_CONCRETE);
                    map.SetTileModelAt(digPos.X + 1, digPos.Y, m_Game.GameTiles.FLOOR_CONCRETE);
                    map.SetTileModelAt(digPos.X - 2, digPos.Y, m_Game.GameTiles.WALL_STONE);
                    map.SetTileModelAt(digPos.X + 2, digPos.Y, m_Game.GameTiles.WALL_STONE);

                    // continue digging.
                    digPos += digDirection;
                }
                #endregion

                // - dig platform and make corridor zone.
                #region
                const int platformExtend = 10;
                const int platformWidth = 3;
                Rectangle platformRect;
                int platformLeft = Math.Max(0, b.BuildingRect.Left - platformExtend);
                int platformRight = Math.Min(map.Width - 1, b.BuildingRect.Right + platformExtend);
                int benchesLine;
                if (digDirection == Direction.S)
                {
                    platformRect = Rectangle.FromLTRB(platformLeft, digPos.Y - platformWidth, platformRight, digPos.Y);
                    benchesLine = platformRect.Top;
                    map.AddZone(MakeUniqueZone("corridor", Rectangle.FromLTRB(entryFenceX - 1, entryFenceY, entryFenceX + 1 + 1, platformRect.Top)));
                }
                else
                {
                    platformRect = Rectangle.FromLTRB(platformLeft, digPos.Y + 1, platformRight, digPos.Y + 1 + platformWidth);
                    benchesLine = platformRect.Bottom - 1;
                    map.AddZone(MakeUniqueZone("corridor", Rectangle.FromLTRB(entryFenceX - 1, platformRect.Bottom, entryFenceX + 1 + 1, entryFenceY + 1)));
                }
                TileFill(map, m_Game.GameTiles.FLOOR_CONCRETE, platformRect);

                // - iron benches in platform.
                for (int bx = platformRect.Left; bx < platformRect.Right; bx++)
                {
                    if (CountAdjWalls(map, bx, benchesLine) < 3)
                        continue;
                    map.PlaceMapObjectAt(MakeObjIronBench(GameImages.OBJ_IRON_BENCH), new Point(bx, benchesLine));
                }

                // - platform zone.
                map.AddZone(MakeUniqueZone("platform", platformRect));
                #endregion

                // - add closed iron gates between corridor and platform.
                #region
                Point ironFencePos;
                if (digDirection == Direction.S)
                    ironFencePos = new Point(entryFenceX, platformRect.Top - 1);
                else
                    ironFencePos = new Point(entryFenceX, platformRect.Bottom);
                map.PlaceMapObjectAt(MakeObjIronGate(GameImages.OBJ_GATE_CLOSED), new Point(ironFencePos.X, ironFencePos.Y));
                map.PlaceMapObjectAt(MakeObjIronGate(GameImages.OBJ_GATE_CLOSED), new Point(ironFencePos.X + 1, ironFencePos.Y));
                map.PlaceMapObjectAt(MakeObjIronGate(GameImages.OBJ_GATE_CLOSED), new Point(ironFencePos.X - 1, ironFencePos.Y));
                #endregion

                // - make power room.
                #region
                // access in the corridor, going toward the center of the map.
                Point powerRoomEntry;
                Rectangle powerRoomRect;
                const int powerRoomWidth = 4;
                const int powerRoomHalfHeight = 2;
                if (entryFenceX > map.Width / 2)
                {
                    // west.
                    powerRoomEntry = new Point(entryFenceX - 2, entryFenceY + powerRoomHalfHeight * digDirection.Vector.Y);
                    powerRoomRect = Rectangle.FromLTRB(powerRoomEntry.X - powerRoomWidth, powerRoomEntry.Y - powerRoomHalfHeight, powerRoomEntry.X + 1, powerRoomEntry.Y + powerRoomHalfHeight + 1);
                }
                else
                {
                    // east.
                    powerRoomEntry = new Point(entryFenceX + 2, entryFenceY + powerRoomHalfHeight * digDirection.Vector.Y);
                    powerRoomRect = Rectangle.FromLTRB(powerRoomEntry.X, powerRoomEntry.Y - powerRoomHalfHeight, powerRoomEntry.X + powerRoomWidth, powerRoomEntry.Y + powerRoomHalfHeight + 1);
                }

                // carve power room.
                TileFill(map, m_Game.GameTiles.FLOOR_CONCRETE, powerRoomRect);
                TileRectangle(map, m_Game.GameTiles.WALL_STONE, powerRoomRect);

                // add door with signs.
                PlaceDoor(map, powerRoomEntry.X, powerRoomEntry.Y, m_Game.GameTiles.FLOOR_CONCRETE, MakeObjIronDoor());
                map.GetTileAt(powerRoomEntry.X, powerRoomEntry.Y - 1).AddDecoration(GameImages.DECO_POWER_SIGN_BIG);
                map.GetTileAt(powerRoomEntry.X, powerRoomEntry.Y + 1).AddDecoration(GameImages.DECO_POWER_SIGN_BIG);

                // add power generators along wall.
                MapObjectFill(map, powerRoomRect,
                    (pt) =>
                    {
                        if (!map.GetTileAt(pt).Model.IsWalkable)
                            return null;
                        if (CountAdjWalls(map, pt.X, pt.Y) < 3 || CountAdjDoors(map, pt.X, pt.Y) > 0)
                            return null;
                        return MakeObjPowerGenerator(GameImages.OBJ_POWERGEN_OFF, GameImages.OBJ_POWERGEN_ON);
                    });

                #endregion
            }
            #endregion

            /////////////////////
            // Furniture & Items.
            /////////////////////
            // iron benches in station.
            #region
            for (int bx = b.InsideRect.Left; bx < b.InsideRect.Right; bx++)
                for (int by = b.InsideRect.Top + 1; by < b.InsideRect.Bottom - 1; by++)
                {
                    // next to walls and no doors.
                    if (CountAdjWalls(map, bx, by) < 2 || CountAdjDoors(map, bx, by) > 0)
                        continue;

                    // not next to stairs.
                    if (m_Game.Rules.GridDistance(new Point(bx, by), new Point(entryFenceX, entryFenceY)) < 2)
                        continue;

                    // bench.
                    map.PlaceMapObjectAt(MakeObjIronBench(GameImages.OBJ_IRON_BENCH), new Point(bx, by));
                }
            #endregion

            /////////////////////////////////////
            // Add subway police guy on surface.
            /////////////////////////////////////
            if (isSurface)
            {
                Actor policeMan = CreateNewPoliceman(0);
                ActorPlace(m_DiceRoller, b.Rectangle.Width * b.Rectangle.Height, map, policeMan, b.InsideRect.Left, b.InsideRect.Top, b.InsideRect.Width, b.InsideRect.Height);
            }

            //////////////
            // Make zone.
            //////////////
            map.AddZone(MakeUniqueZone(RogueGame.NAME_SUBWAY_STATION, b.BuildingRect));
        }

        #endregion

        #region Rooms
        // alpha10.1 allow different x and y min size
        protected virtual void MakeRoomsPlan(Map map, ref List<Rectangle> list, Rectangle rect, int minRoomsXSize, int minRoomsYSize)
        {
            ////////////
            // 1. Split
            ////////////
            int splitX, splitY;
            Rectangle topLeft, topRight, bottomLeft, bottomRight;
            QuadSplit(rect, minRoomsXSize, minRoomsYSize, out splitX, out splitY, out topLeft, out topRight, out bottomLeft, out bottomRight);

            ///////////////////
            // 2. Termination?
            ///////////////////
            if (topRight.IsEmpty && bottomLeft.IsEmpty && bottomRight.IsEmpty)
            {
                list.Add(rect);
                return;
            }

            //////////////
            // 3. Recurse
            //////////////
            // always top left.
            MakeRoomsPlan(map, ref list, topLeft, minRoomsXSize, minRoomsYSize);
            // then recurse in non empty quads.
            // we shift and inflante the quads cause we want rooms walls and doors to overlap.
            if (!topRight.IsEmpty)
            {
                topRight.Offset(-1, 0);
                ++topRight.Width;
                MakeRoomsPlan(map, ref list, topRight, minRoomsXSize, minRoomsYSize);
            }
            if (!bottomLeft.IsEmpty)
            {
                bottomLeft.Offset(0, -1);
                ++bottomLeft.Height;
                MakeRoomsPlan(map, ref list, bottomLeft, minRoomsXSize, minRoomsYSize);
            }
            if (!bottomRight.IsEmpty)
            {
                bottomRight.Offset(-1, -1);
                ++bottomRight.Width;
                ++bottomRight.Height;
                MakeRoomsPlan(map, ref list, bottomRight, minRoomsXSize, minRoomsYSize);
            }
        }

        protected virtual void MakeHousingRoom(Map map, Rectangle roomRect, TileModel floor, TileModel wall)
        {
            ////////////////////
            // 1. Floor & Walls
            ////////////////////
            base.TileFill(map, floor, roomRect);
            base.TileRectangle(map, wall, roomRect.Left, roomRect.Top, roomRect.Width, roomRect.Height,
                (tile, prevmodel, x, y) =>
                {
                    // if we have a door there, don't put a wall!
                    if (map.GetMapObjectAt(x, y) != null)
                        map.SetTileModelAt(x, y, floor);
                });

            //////////////////////
            // 2. Doors & Windows
            //////////////////////
            int midX = roomRect.Left + roomRect.Width / 2;
            int midY = roomRect.Top + roomRect.Height / 2;
            const int outsideDoorChance = 25;

            PlaceIf(map, midX, roomRect.Top, floor,
                (x, y) => HasNoObjectAt(map, x, y) && IsAccessible(map, x, y) && CountAdjDoors(map, x, y) == 0,
                (x, y) => IsInside(map, x, y) || m_DiceRoller.RollChance(outsideDoorChance) ? MakeObjWoodenDoor() : MakeObjWindow());
            PlaceIf(map, midX, roomRect.Bottom - 1, floor,
                (x, y) => HasNoObjectAt(map, x, y) && IsAccessible(map, x, y) && CountAdjDoors(map, x, y) == 0,
                (x, y) => IsInside(map, x, y) || m_DiceRoller.RollChance(outsideDoorChance) ? MakeObjWoodenDoor() : MakeObjWindow());
            PlaceIf(map, roomRect.Left, midY, floor,
                (x, y) => HasNoObjectAt(map, x, y) && IsAccessible(map, x, y) && CountAdjDoors(map, x, y) == 0,
                (x, y) => IsInside(map, x, y) || m_DiceRoller.RollChance(outsideDoorChance) ? MakeObjWoodenDoor() : MakeObjWindow());
            PlaceIf(map, roomRect.Right - 1, midY, floor,
                (x, y) => HasNoObjectAt(map, x, y) && IsAccessible(map, x, y) && CountAdjDoors(map, x, y) == 0,
                (x, y) => IsInside(map, x, y) || m_DiceRoller.RollChance(outsideDoorChance) ? MakeObjWoodenDoor() : MakeObjWindow());
        }

        // alpha10.1 can force room role (optional param)
        // FIXME -- room role should be an enum and not hardcoded numbers -_-
        /// <summary>
        /// 
        /// </summary>
        /// <param name="map"></param>
        /// <param name="roomRect"></param>
        /// <param name="role">-1 roll at random; 0-4 bedroom, 5-7 living room, 8-9 kitchen</param>
        protected virtual void FillHousingRoomContents(Map map, Rectangle roomRect, int role = -1)
        {
            Rectangle insideRoom = new Rectangle(roomRect.Left + 1, roomRect.Top + 1, roomRect.Width - 2, roomRect.Height - 2);

            // alpha10.1 roll room role if not set
            if (role == -1)
                role = m_DiceRoller.Roll(0, 10);

            // alpha10.1 added restriction to not place a mapobj if adj to at least 5 mapobj as to not cramp apartements

            switch (role)
            {
                // 1. Bedroom? 0-4 = 50%
                #region
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                    {
                        #region
                        // beds with night tables.
                        int nbBeds = m_DiceRoller.Roll(1, 3);
                        for (int i = 0; i < nbBeds; i++)
                        {
                            MapObjectPlaceInGoodPosition(map, insideRoom,
                                (pt) => CountAdjWalls(map, pt.X, pt.Y) >= 3 && CountAdjDoors(map, pt.X, pt.Y) == 0 && CountAdjMapObjects(map, pt.X, pt.Y) < 5,  // alpha10.1 not cramped
                                m_DiceRoller,
                                (pt) =>
                                {
                                    // one night table around with item.
                                    Rectangle adjBedRect = new Rectangle(pt.X - 1, pt.Y - 1, 3, 3);
                                    adjBedRect.Intersect(insideRoom);
                                    MapObjectPlaceInGoodPosition(map, adjBedRect,
                                        (pt2) => pt2 != pt && CountAdjDoors(map, pt2.X, pt2.Y) == 0 && CountAdjWalls(map, pt2.X, pt2.Y) > 0 && CountAdjMapObjects(map, pt.X, pt.Y) < 5,  // alpha10.1 not cramped,
                                        m_DiceRoller,
                                        (pt2) =>
                                        {
                                            // item.
                                            Item it = MakeRandomBedroomItem();
                                            if (it != null)
                                                map.DropItemAt(it, pt2);

                                            // night table.
                                            return MakeObjNightTable(GameImages.OBJ_NIGHT_TABLE);
                                        });

                                    // bed.
                                    MapObject bed = MakeObjBed(GameImages.OBJ_BED);
                                    return bed;
                                });
                        }

                        // wardrobe/drawer with items
                        int nbWardrobeOrDrawer = m_DiceRoller.Roll(1, 4);
                        for (int i = 0; i < nbWardrobeOrDrawer; i++)
                        {
                            MapObjectPlaceInGoodPosition(map, insideRoom,
                                                (pt) => CountAdjWalls(map, pt.X, pt.Y) >= 2 && CountAdjDoors(map, pt.X, pt.Y) == 0 && CountAdjMapObjects(map, pt.X, pt.Y) < 5,  // alpha10.1 not cramped,
                                                m_DiceRoller,
                                                (pt) =>
                                                {
                                                    // item.
                                                    Item it = MakeRandomBedroomItem();
                                                    if (it != null)
                                                        map.DropItemAt(it, pt);

                                                    // wardrobe or drawer
                                                    if (m_DiceRoller.RollChance(50))
                                                        return MakeObjWardrobe(GameImages.OBJ_WARDROBE);
                                                    else
                                                        return MakeObjDrawer(GameImages.OBJ_DRAWER);
                                                });
                        }
                        break;
                        #endregion
                    }
                #endregion

                // 2. Living room? 5-6-7 = 30%
                #region
                case 5:
                case 6:
                case 7:
                    {
                        #region
                        // tables with chairs.
                        int nbTables = m_DiceRoller.Roll(1, 3);

                        for (int i = 0; i < nbTables; i++)
                        {
                            MapObjectPlaceInGoodPosition(map, insideRoom,
                                (pt) => CountAdjWalls(map, pt.X, pt.Y) == 0 && CountAdjDoors(map, pt.X, pt.Y) == 0 && CountAdjMapObjects(map, pt.X, pt.Y) < 5,  // alpha10.1 not cramped,
                                m_DiceRoller,
                                (pt) =>
                                {
                                    // items.
                                    for (int ii = 0; ii < HOUSE_LIVINGROOM_ITEMS_ON_TABLE; ii++)
                                    {
                                        Item it = MakeRandomKitchenItem();
                                        if (it != null)
                                            map.DropItemAt(it, pt);
                                    }

                                    // one chair around.
                                    Rectangle adjTableRect = new Rectangle(pt.X - 1, pt.Y - 1, 3, 3);
                                    adjTableRect.Intersect(insideRoom);
                                    MapObjectPlaceInGoodPosition(map, adjTableRect,
                                        (pt2) => pt2 != pt && CountAdjDoors(map, pt2.X, pt2.Y) == 0 && CountAdjMapObjects(map, pt.X, pt.Y) < 5,  // alpha10.1 not cramped,
                                        m_DiceRoller,
                                        (pt2) => MakeObjChair(GameImages.OBJ_CHAIR));

                                    // table.
                                    MapObject table = MakeObjTable(GameImages.OBJ_TABLE);
                                    return table;
                                });
                        }

                        // drawers.
                        int nbDrawers = m_DiceRoller.Roll(1, 3);
                        for (int i = 0; i < nbDrawers; i++)
                        {
                            MapObjectPlaceInGoodPosition(map, insideRoom,
                                                (pt) => CountAdjWalls(map, pt.X, pt.Y) >= 2 && CountAdjDoors(map, pt.X, pt.Y) == 0 && CountAdjMapObjects(map, pt.X, pt.Y) < 5,  // alpha10.1 not cramped,
                                                m_DiceRoller,
                                                (pt) => MakeObjDrawer(GameImages.OBJ_DRAWER));
                        }
                        break;
                        #endregion
                    }
                #endregion

                // 3. Kitchen? 8-9 = 20%
                #region
                case 8:
                case 9:
                    {
                        #region
                        // table with item & chair.
                        MapObjectPlaceInGoodPosition(map, insideRoom,
                            (pt) => CountAdjWalls(map, pt.X, pt.Y) == 0 && CountAdjDoors(map, pt.X, pt.Y) == 0 && CountAdjMapObjects(map, pt.X, pt.Y) < 5,  // alpha10.1 not cramped,
                            m_DiceRoller,
                            (pt) =>
                            {
                                // items.
                                for (int ii = 0; ii < HOUSE_KITCHEN_ITEMS_ON_TABLE; ii++)
                                {
                                    Item it = MakeRandomKitchenItem();
                                    if (it != null)
                                        map.DropItemAt(it, pt);
                                }

                                // one chair around.
                                Rectangle adjTableRect = new Rectangle(pt.X - 1, pt.Y - 1, 3, 3);
                                MapObjectPlaceInGoodPosition(map, adjTableRect,
                                    (pt2) => pt2 != pt && CountAdjDoors(map, pt2.X, pt2.Y) == 0,
                                    m_DiceRoller,
                                    (pt2) => MakeObjChair(GameImages.OBJ_CHAIR));

                                // table.
                                return MakeObjTable(GameImages.OBJ_TABLE);
                            });

                        // fridge with items
                        MapObjectPlaceInGoodPosition(map, insideRoom,
                                            (pt) => CountAdjWalls(map, pt.X, pt.Y) >= 2 && CountAdjDoors(map, pt.X, pt.Y) == 0 && CountAdjMapObjects(map, pt.X, pt.Y) < 5,  // alpha10.1 not cramped,
                                            m_DiceRoller,
                                            (pt) =>
                                            {
                                                // items.
                                                for (int ii = 0; ii < HOUSE_KITCHEN_ITEMS_IN_FRIDGE; ii++)
                                                {
                                                    Item it = MakeRandomKitchenItem();
                                                    if (it != null)
                                                        map.DropItemAt(it, pt);
                                                }

                                                // fridge
                                                return MakeObjFridge(GameImages.OBJ_FRIDGE);
                                            });
                        break;
                        #endregion
                    }

                default:
                    throw new ArgumentOutOfRangeException("unhandled roll");
                    #endregion
            }
        }

        #endregion

        #region Items
        protected Item MakeRandomShopItem(ShopType shop)
        {
            switch (shop)
            {
                case ShopType.CONSTRUCTION:
                    return MakeShopConstructionItem();
                case ShopType.GENERAL_STORE:
                    return MakeShopGeneralItem();
                case ShopType.GROCERY:
                    return MakeShopGroceryItem();
                case ShopType.GUNSHOP:
                    return MakeShopGunshopItem();
                case ShopType.PHARMACY:
                    return MakeShopPharmacyItem();
                case ShopType.SPORTSWEAR:
                    return MakeShopSportsWearItem();
                case ShopType.HUNTING:
                    return MakeHuntingShopItem();
                default:
                    throw new ArgumentOutOfRangeException("unhandled shoptype");

            }
        }

        public Item MakeShopGroceryItem()
        {
            if (m_DiceRoller.RollChance(50))
                return MakeItemCannedFood();
            else
                return MakeItemGroceries();
        }

        public Item MakeShopPharmacyItem()
        {
            int randomItem = m_DiceRoller.Roll(0, 6);
            switch (randomItem)
            {
                case 0: return MakeItemBandages();
                case 1: return MakeItemMedikit();
                case 2: return MakeItemPillsSLP();
                case 3: return MakeItemPillsSTA();
                case 4: return MakeItemPillsSAN();
                case 5: return MakeItemStenchKiller();

                default:
                    throw new ArgumentOutOfRangeException("unhandled roll");
            }
        }

        public Item MakeShopSportsWearItem()
        {
            int roll = m_DiceRoller.Roll(0, 10);

            switch (roll)
            {
                case 0:
                    if (m_DiceRoller.RollChance(30))
                        return MakeItemHuntingRifle();
                    else
                        return MakeItemLightRifleAmmo();
                case 1:
                    if (m_DiceRoller.RollChance(30))
                        return MakeItemHuntingCrossbow();
                    else
                        return MakeItemBoltsAmmo();
                case 2:
                case 3:
                case 4:
                case 5: return MakeItemBaseballBat();       // 40%

                case 6:
                case 7: return MakeItemIronGolfClub();      // 20%

                case 8:
                case 9: return MakeItemGolfClub();          // 20%
                default:
                    throw new ArgumentOutOfRangeException("unhandled roll");
            }
        }

        public Item MakeShopConstructionItem()
        {
            int roll = m_DiceRoller.Roll(0, 24);
            switch (roll)
            {
                case 0:
                case 1:
                case 2: return m_DiceRoller.RollChance(50) ? MakeItemShovel() : MakeItemShortShovel();

                case 3:
                case 4:
                case 5: return MakeItemCrowbar();

                case 6:
                case 7:
                case 8: return m_DiceRoller.RollChance(50) ? MakeItemHugeHammer() : MakeItemSmallHammer();

                case 9:
                case 10:
                case 11: return MakeItemWoodenPlank();

                case 12:
                case 13:
                case 14: return MakeItemFlashlight();

                case 15:
                case 16:
                case 17: return MakeItemBigFlashlight();

                case 18:
                case 19:
                case 20: return MakeItemSpikes();

                case 21:
                case 22:
                case 23: return MakeItemBarbedWire();

                default:
                    throw new ArgumentOutOfRangeException("unhandled roll");
            }
        }

        public Item MakeShopGunshopItem()
        {
            // Weapons (40%) vs Ammo (60%)
            if (m_DiceRoller.RollChance(40))
            {
                int roll = m_DiceRoller.Roll(0, 4);

                switch (roll)
                {
                    case 0: return MakeItemRandomPistol();
                    case 1: return MakeItemShotgun();
                    case 2: return MakeItemHuntingRifle();
                    case 3: return MakeItemHuntingCrossbow();

                    default:
                        return null;
                }
            }
            else
            {
                int roll = m_DiceRoller.Roll(0, 4);

                switch (roll)
                {
                    case 0: return MakeItemLightPistolAmmo();
                    case 1: return MakeItemShotgunAmmo();
                    case 2: return MakeItemLightRifleAmmo();
                    case 3: return MakeItemBoltsAmmo();

                    default:
                        return null;
                }
            }
        }

        public Item MakeHuntingShopItem()
        {
            // Weapons/Ammo (50%) Outfits&Traps (50%)
            if (m_DiceRoller.RollChance(50))
            {
                // Weapons(40) Ammo(60)
                if (m_DiceRoller.RollChance(40))
                {
                    int roll = m_DiceRoller.Roll(0, 2);

                    switch (roll)
                    {
                        case 0: return MakeItemHuntingRifle();
                        case 1: return MakeItemHuntingCrossbow();
                        default:
                            return null;
                    }
                }
                else
                {
                    int roll = m_DiceRoller.Roll(0, 2);

                    switch (roll)
                    {
                        case 0: return MakeItemLightRifleAmmo();
                        case 1: return MakeItemBoltsAmmo();
                        default:
                            return null;
                    }
                }
            }
            else
            {
                // Outfits&Traps
                int roll = m_DiceRoller.Roll(0, 2);
                switch (roll)
                {
                    case 0: return MakeItemHunterVest();
                    case 1: return MakeItemBearTrap();
                    default:
                        return null;
                }
            }
        }

        public Item MakeShopGeneralItem()
        {
            int roll = m_DiceRoller.Roll(0, 6);
            switch (roll)
            {
                case 0: return MakeShopPharmacyItem();
                case 1: return MakeShopSportsWearItem();
                case 2: return MakeShopConstructionItem();
                case 3: return MakeShopGroceryItem();
                case 4: return MakeHuntingShopItem();
                case 5: return MakeRandomBedroomItem();
                default:
                    throw new ArgumentOutOfRangeException("unhandled roll");
            }
        }

        public Item MakeHospitalItem()
        {
            int randomItem = m_DiceRoller.Roll(0, 7);
            switch (randomItem)
            {
                case 0: return MakeItemBandages();
                case 1: return MakeItemMedikit();
                case 2: return MakeItemPillsSLP();
                case 3: return MakeItemPillsSTA();
                case 4: return MakeItemPillsSAN();
                case 5: return MakeItemStenchKiller();
                case 6: return MakeItemPillsAntiviral();

                default:
                    throw new ArgumentOutOfRangeException("unhandled roll");
            }
        }

        public Item MakeRandomBedroomItem()
        {
            int randomItem = m_DiceRoller.Roll(0, 24);

            switch (randomItem)
            {
                case 0:
                case 1: return MakeItemBandages();
                case 2: return MakeItemPillsSTA();
                case 3: return MakeItemPillsSLP();
                case 4: return MakeItemPillsSAN();

                case 5:
                case 6:
                case 7:
                case 8: return MakeItemBaseballBat();

                case 9: return MakeItemRandomPistol();

                case 10: // rare fire weapon
                    if (m_DiceRoller.RollChance(30))
                    {
                        if (m_DiceRoller.RollChance(50))
                            return MakeItemShotgun();
                        else
                            return MakeItemHuntingRifle();
                    }
                    else
                    {
                        if (m_DiceRoller.RollChance(50))
                            return MakeItemShotgunAmmo();
                        else
                            return MakeItemLightRifleAmmo();
                    }
                case 11:
                case 12:
                case 13: return MakeItemCellPhone();

                case 14:
                case 15: return MakeItemFlashlight();

                case 16:
                case 17: return MakeItemLightPistolAmmo();

                case 18:
                case 19: return MakeItemStenchKiller();

                case 20: return MakeItemHunterVest();

                case 21:
                case 22:
                case 23:
                    if (m_DiceRoller.RollChance(50))
                        return MakeItemBook();
                    else
                        return MakeItemMagazines();

                default: throw new ArgumentOutOfRangeException("unhandled roll");
            }
        }

        public Item MakeRandomKitchenItem()
        {
            if (m_DiceRoller.RollChance(50))
                return MakeItemCannedFood();
            else
                return MakeItemGroceries();
        }

        public Item MakeRandomCHAROfficeItem()
        {
            int randomItem = m_DiceRoller.Roll(0, 10);
            switch (randomItem)
            {
                case 0:
                    // weapons:
                    // - grenade (rare).
                    // - shotgun/ammo
                    if (m_DiceRoller.RollChance(10))
                    {
                        // grenade!
                        return MakeItemGrenade();
                    }
                    else
                    {
                        // shotgun/ammo
                        if (m_DiceRoller.RollChance(30))
                            return MakeItemShotgun();
                        else
                            return MakeItemShotgunAmmo();
                    }

                case 1:
                case 2:
                    if (m_DiceRoller.RollChance(50))
                        return MakeItemBandages();
                    else
                        return MakeItemMedikit();

                case 3:
                    return MakeItemCannedFood();

                case 4: // rare tracker items
                    if (m_DiceRoller.RollChance(50))
                    {
                        if (m_DiceRoller.RollChance(50))
                            return MakeItemZTracker();
                        else
                            return MakeItemBlackOpsGPS();
                    }
                    else
                        return null;

                default: return null; // 50% chance to find nothing.
            }
        }

        public Item MakeRandomParkItem()
        {
            int randomItem = m_DiceRoller.Roll(0, 8);
            switch (randomItem)
            {
                case 0: return MakeItemSprayPaint();
                case 1: return MakeItemBaseballBat();
                case 2: return MakeItemPillsSLP();
                case 3: return MakeItemPillsSTA();
                case 4: return MakeItemPillsSAN();
                case 5: return MakeItemFlashlight();
                case 6: return MakeItemCellPhone();
                case 7: return MakeItemWoodenPlank();
                default: throw new ArgumentOutOfRangeException("unhandled item roll");
            }
        }
        #endregion

        #region Decorations

        static readonly string[] POSTERS = { GameImages.DECO_POSTERS1, GameImages.DECO_POSTERS2 };
        protected virtual void DecorateOutsideWallsWithPosters(Map map, Rectangle rect, int chancePerWall)
        {
            base.DecorateOutsideWalls(map, rect,
                (x, y) =>
                {
                    if (m_DiceRoller.RollChance(chancePerWall))
                    {
                        return POSTERS[m_DiceRoller.Roll(0, POSTERS.Length)];
                    }
                    else
                        return null;
                });
        }

        static readonly string[] TAGS = { GameImages.DECO_TAGS1, GameImages.DECO_TAGS2, GameImages.DECO_TAGS3, GameImages.DECO_TAGS4, GameImages.DECO_TAGS5, GameImages.DECO_TAGS6, GameImages.DECO_TAGS7 };

        protected virtual void DecorateOutsideWallsWithTags(Map map, Rectangle rect, int chancePerWall)
        {
            base.DecorateOutsideWalls(map, rect,
                (x, y) =>
                {
                    if (m_DiceRoller.RollChance(chancePerWall))
                    {
                        return TAGS[m_DiceRoller.Roll(0, TAGS.Length)];
                    }
                    else
                        return null;
                });
        }
        #endregion

        #region Populating buildings
        protected virtual void PopulateCHAROfficeBuilding(Map map, Block b)
        {
            //////////
            // Guards
            //////////
            for (int i = 0; i < MAX_CHAR_GUARDS_PER_OFFICE; i++)
            {
                Actor newGuard = CreateNewCHARGuard(0);
                ActorPlace(m_DiceRoller, 100, map, newGuard, b.InsideRect.Left, b.InsideRect.Top, b.InsideRect.Width, b.InsideRect.Height);
            }

        }
        #endregion

        #region Special Locations

        #region House Basement
        Map GenerateHouseBasementMap(Map map, Block houseBlock)
        {
            // make map.
            #region
            Rectangle rect = houseBlock.BuildingRect;
            int seed = map.Seed << 1 + rect.Left * map.Height + rect.Top;
            Map basement = new Map(seed, String.Format("basement{0}{1}@{2}-{3}", m_Params.District.WorldPosition.X, m_Params.District.WorldPosition.Y, rect.Left + rect.Width / 2, rect.Top + rect.Height / 2), rect.Width, rect.Height)
            {
                Lighting = Lighting.DARKNESS
            };
            basement.AddZone(MakeUniqueZone("basement", basement.Rect));
            #endregion

            // enclose.
            #region
            TileFill(basement, m_Game.GameTiles.FLOOR_CONCRETE, (tile, model, x, y) => tile.IsInside = true);
            TileRectangle(basement, m_Game.GameTiles.WALL_BRICK, new Rectangle(0, 0, basement.Width, basement.Height));
            #endregion

            // link to house with stairs.
            #region
            Point surfaceStairs = new Point();
            for (; ; )
            {
                // roll.
                surfaceStairs.X = m_DiceRoller.Roll(rect.Left, rect.Right);
                surfaceStairs.Y = m_DiceRoller.Roll(rect.Top, rect.Bottom);

                // valid if walkable & no blocking object.
                // alpha10 and inside
                if (!map.GetTileAt(surfaceStairs.X, surfaceStairs.Y).Model.IsWalkable)
                    continue;
                if (map.GetMapObjectAt(surfaceStairs.X, surfaceStairs.Y) != null)
                    continue;
                if (!map.GetTileAt(surfaceStairs.X, surfaceStairs.Y).IsInside)
                    continue;

                // good post.
                break;
            }
            Point basementStairs = new Point(surfaceStairs.X - rect.Left, surfaceStairs.Y - rect.Top);
            AddExit(map, surfaceStairs, basement, basementStairs, GameImages.DECO_STAIRS_DOWN, true);
            AddExit(basement, basementStairs, map, surfaceStairs, GameImages.DECO_STAIRS_UP, true);
            #endregion

            // random pilars/walls.
            #region
            DoForEachTile(basement, basement.Rect,
                (pt) =>
                {
                    if (!m_DiceRoller.RollChance(HOUSE_BASEMENT_PILAR_CHANCE))
                        return;
                    if (pt == basementStairs)
                        return;
                    basement.SetTileModelAt(pt.X, pt.Y, m_Game.GameTiles.WALL_BRICK);
                });
            #endregion

            // fill with ome furniture/crap and items.
            #region
            MapObjectFill(basement, basement.Rect,
                (pt) =>
                {
                    if (!m_DiceRoller.RollChance(HOUSE_BASEMENT_OBJECT_CHANCE_PER_TILE))
                        return null;

                    if (basement.GetExitAt(pt) != null)
                        return null;
                    if (!basement.IsWalkable(pt.X, pt.Y))
                        return null;

                    int roll = m_DiceRoller.Roll(0, 5);
                    switch (roll)
                    {
                        case 0: // junk
                            return MakeObjJunk(GameImages.OBJ_JUNK);
                        case 1: // barrels.
                            return MakeObjBarrels(GameImages.OBJ_BARRELS);
                        case 2: // table with random item.
                            {
                                Item it = MakeShopConstructionItem();
                                basement.DropItemAt(it, pt);
                                return MakeObjTable(GameImages.OBJ_TABLE);
                            };
                        case 3: // drawer with random item.
                            {
                                Item it = MakeShopConstructionItem();
                                basement.DropItemAt(it, pt);
                                return MakeObjDrawer(GameImages.OBJ_DRAWER);
                            };
                        case 4: // bed.
                            return MakeObjBed(GameImages.OBJ_BED);

                        default:
                            throw new ArgumentOutOfRangeException("unhandled roll");
                    }
                });
            #endregion

            // rats!
            #region
            if (Rules.HasZombiesInBasements(m_Game.Session.GameMode))
            {
                DoForEachTile(basement, basement.Rect,
                    (pt) =>
                    {
                        if (!basement.IsWalkable(pt.X, pt.Y))
                            return;
                        if (basement.GetExitAt(pt) != null)
                            return;

                        if (m_DiceRoller.RollChance(SHOP_BASEMENT_ZOMBIE_RAT_CHANCE))
                            basement.PlaceActorAt(CreateNewBasementRatZombie(0), pt);
                    });
            }
            #endregion

            // weapons cache?
            #region
            if (m_DiceRoller.RollChance(HOUSE_BASEMENT_WEAPONS_CACHE_CHANCE))
            {
                MapObjectPlaceInGoodPosition(basement, basement.Rect,
                    (pt) =>
                    {
                        if (basement.GetExitAt(pt) != null)
                            return false;
                        if (!basement.IsWalkable(pt.X, pt.Y))
                            return false;
                        if (basement.GetMapObjectAt(pt) != null)
                            return false;
                        if (basement.GetItemsAt(pt) != null)
                            return false;
                        return true;
                    },
                    m_DiceRoller,
                    (pt) =>
                    {
                        // two grenades...
                        basement.DropItemAt(MakeItemGrenade(), pt);
                        basement.DropItemAt(MakeItemGrenade(), pt);

                        // and a handfull of gunshop items.
                        for (int i = 0; i < 5; i++)
                        {
                            Item it = MakeShopGunshopItem();
                            basement.DropItemAt(it, pt);
                        }

                        // shelf.
                        MapObject shelf = MakeObjShelf(GameImages.OBJ_SHOP_SHELF);
                        return shelf;
                    });
            }
            #endregion

            // alpha10
            // music.
            basement.BgMusic = GameMusics.SEWERS;

            // done.
            return basement;
        }
        #endregion

        #region CHAR Underground Facility
        // alpha10 added entry pos
        public Map GenerateUniqueMap_CHARUnderground(Map surfaceMap, Zone officeZone, out Point baseEntryPos)
        {
            /////////////////////////
            // 1. Create basic secret map.
            // 2. Link to office.
            // 3. Create rooms.
            // 4. Furniture & Items.
            // 5. Posters & Blood.
            // 6. Populate.
            // 7. Add uniques.
            // alpha10
            // 8. Music
            /////////////////////////

            // 1. Create basic secret map.
            #region
            // huge map.
            Map underground = new Map((surfaceMap.Seed << 3) ^ surfaceMap.Seed, "CHAR Underground Facility", RogueGame.MAP_MAX_WIDTH, RogueGame.MAP_MAX_HEIGHT)
            {
                Lighting = Lighting.DARKNESS,
                IsSecret = true
            };
            // fill & enclose.
            TileFill(underground, m_Game.GameTiles.FLOOR_OFFICE, (tile, model, x, y) => tile.IsInside = true);
            TileRectangle(underground, m_Game.GameTiles.WALL_CHAR_OFFICE, new Rectangle(0, 0, underground.Width, underground.Height));
            #endregion

            // 2. Link to office.
            #region
            // find surface point in office:
            // - in a random office room.
            // - set exit somewhere walkable inside.
            // - iron door, barricade the door.
            Zone roomZone = null;
            Point surfaceExit = new Point();
            while (true)    // loop until found.
            {
                // find a random room.
                do
                {
                    int x = m_DiceRoller.Roll(officeZone.Bounds.Left, officeZone.Bounds.Right);
                    int y = m_DiceRoller.Roll(officeZone.Bounds.Top, officeZone.Bounds.Bottom);
                    List<Zone> zonesHere = surfaceMap.GetZonesAt(x, y);
                    if (zonesHere == null || zonesHere.Count == 0)
                        continue;
                    foreach (Zone z in zonesHere)
                        if (z.Name.Contains("room"))
                        {
                            roomZone = z;
                            break;
                        }
                }
                while (roomZone == null);

                // find somewhere walkable inside.
                bool foundSurfaceExit = false;
                int attempts = 0;
                do
                {
                    surfaceExit.X = m_DiceRoller.Roll(roomZone.Bounds.Left, roomZone.Bounds.Right);
                    surfaceExit.Y = m_DiceRoller.Roll(roomZone.Bounds.Top, roomZone.Bounds.Bottom);
                    foundSurfaceExit = surfaceMap.IsWalkable(surfaceExit.X, surfaceExit.Y);
                    ++attempts;
                }
                while (attempts < 100 && !foundSurfaceExit);

                // failed?
                if (foundSurfaceExit == false)
                    continue;

                // found everything, good!
                break;
            }

            // alpha10
            // remember position
            baseEntryPos = surfaceExit;

            // barricade the rooms door.
            DoForEachTile(surfaceMap, roomZone.Bounds,
                (pt) =>
                {
                    DoorWindow door = surfaceMap.GetMapObjectAt(pt) as DoorWindow;
                    if (door == null)
                        return;
                    surfaceMap.RemoveMapObjectAt(pt.X, pt.Y);
                    door = MakeObjIronDoor();
                    door.BarricadePoints = Rules.BARRICADING_MAX;
                    surfaceMap.PlaceMapObjectAt(door, pt);
                });

            // stairs.
            // underground : in the middle of the map.
            Point undergroundStairs = new Point(underground.Width / 2, underground.Height / 2);
            underground.SetExitAt(undergroundStairs, new Exit(surfaceMap, surfaceExit));
            underground.GetTileAt(undergroundStairs.X, undergroundStairs.Y).AddDecoration(GameImages.DECO_STAIRS_UP);
            surfaceMap.SetExitAt(surfaceExit, new Exit(underground, undergroundStairs));
            surfaceMap.GetTileAt(surfaceExit.X, surfaceExit.Y).AddDecoration(GameImages.DECO_STAIRS_DOWN);
            // floor logo.
            ForEachAdjacent(underground, undergroundStairs.X, undergroundStairs.Y, (pt) => underground.GetTileAt(pt).AddDecoration(GameImages.DECO_CHAR_FLOOR_LOGO));
            #endregion

            // 3. Create floorplan & rooms.
            #region
            // make 4 quarters, splitted by a crossed corridor.
            const int corridorHalfWidth = 1;
            Rectangle qTopLeft = Rectangle.FromLTRB(0, 0, underground.Width / 2 - corridorHalfWidth, underground.Height / 2 - corridorHalfWidth);
            Rectangle qTopRight = Rectangle.FromLTRB(underground.Width / 2 + 1 + corridorHalfWidth, 0, underground.Width, qTopLeft.Bottom);
            Rectangle qBotLeft = Rectangle.FromLTRB(0, underground.Height / 2 + 1 + corridorHalfWidth, qTopLeft.Right, underground.Height);
            Rectangle qBotRight = Rectangle.FromLTRB(qTopRight.Left, qBotLeft.Top, underground.Width, underground.Height);

            // split all the map in rooms.
            const int minRoomSize = 6;
            List<Rectangle> roomsList = new List<Rectangle>();
            MakeRoomsPlan(underground, ref roomsList, qBotLeft, minRoomSize, minRoomSize);
            MakeRoomsPlan(underground, ref roomsList, qBotRight, minRoomSize, minRoomSize);
            MakeRoomsPlan(underground, ref roomsList, qTopLeft, minRoomSize, minRoomSize);
            MakeRoomsPlan(underground, ref roomsList, qTopRight, minRoomSize, minRoomSize);

            // make the rooms walls.
            foreach (Rectangle roomRect in roomsList)
            {
                TileRectangle(underground, m_Game.GameTiles.WALL_CHAR_OFFICE, roomRect);
            }

            // add room doors.
            // quarters have door side preferences to lead toward the central corridors.
            foreach (Rectangle roomRect in roomsList)
            {
                Point westEastDoorPos = roomRect.Left < underground.Width / 2 ?
                    new Point(roomRect.Right - 1, roomRect.Top + roomRect.Height / 2) :
                    new Point(roomRect.Left, roomRect.Top + roomRect.Height / 2);
                if (underground.GetMapObjectAt(westEastDoorPos) == null)
                {
                    DoorWindow door = MakeObjCharDoor();
                    PlaceDoorIfAccessibleAndNotAdjacent(underground, westEastDoorPos.X, westEastDoorPos.Y, m_Game.GameTiles.FLOOR_OFFICE, 6, door);
                }

                Point northSouthDoorPos = roomRect.Top < underground.Height / 2 ?
                    new Point(roomRect.Left + roomRect.Width / 2, roomRect.Bottom - 1) :
                    new Point(roomRect.Left + roomRect.Width / 2, roomRect.Top);
                if (underground.GetMapObjectAt(northSouthDoorPos) == null)
                {
                    DoorWindow door = MakeObjCharDoor();
                    PlaceDoorIfAccessibleAndNotAdjacent(underground, northSouthDoorPos.X, northSouthDoorPos.Y, m_Game.GameTiles.FLOOR_OFFICE, 6, door);
                }
            }

            // add iron doors closing each corridor.
            for (int x = qTopLeft.Right; x < qBotRight.Left; x++)
            {
                PlaceDoor(underground, x, qTopLeft.Bottom - 1, m_Game.GameTiles.FLOOR_OFFICE, MakeObjIronDoor());
                PlaceDoor(underground, x, qBotLeft.Top, m_Game.GameTiles.FLOOR_OFFICE, MakeObjIronDoor());
            }
            for (int y = qTopLeft.Bottom; y < qBotLeft.Top; y++)
            {
                PlaceDoor(underground, qTopLeft.Right - 1, y, m_Game.GameTiles.FLOOR_OFFICE, MakeObjIronDoor());
                PlaceDoor(underground, qTopRight.Left, y, m_Game.GameTiles.FLOOR_OFFICE, MakeObjIronDoor());
            }
            #endregion

            // 4. Rooms, furniture & items.
            #region
            // furniture + items in rooms.
            // room roles with zones:
            // - corners room : Power Room.
            // - top left quarter : armory.
            // - top right quarter : storage.
            // - bottom left quarter : living.
            // - bottom right quarter : pharmacy.
            foreach (Rectangle roomRect in roomsList)
            {
                Rectangle insideRoomRect = new Rectangle(roomRect.Left + 1, roomRect.Top + 1, roomRect.Width - 2, roomRect.Height - 2);
                string roomName = "<noname>";

                // special room?
                // one power room in each corner.
                bool isPowerRoom = (roomRect.Left == 0 && roomRect.Top == 0) ||
                    (roomRect.Left == 0 && roomRect.Bottom == underground.Height) ||
                    (roomRect.Right == underground.Width && roomRect.Top == 0) ||
                    (roomRect.Right == underground.Width && roomRect.Bottom == underground.Height);
                if (isPowerRoom)
                {
                    roomName = "Power Room";
                    MakeCHARPowerRoom(underground, roomRect, insideRoomRect);
                }
                else
                {
                    // common room.
                    int roomRole = (roomRect.Left < underground.Width / 2 && roomRect.Top < underground.Height / 2) ? 0 :
                        (roomRect.Left >= underground.Width / 2 && roomRect.Top < underground.Height / 2) ? 1 :
                        (roomRect.Left < underground.Width / 2 && roomRect.Top >= underground.Height / 2) ? 2 :
                        3;
                    switch (roomRole)
                    {
                        case 0: // armory room.
                            {
                                roomName = "Armory";
                                MakeCHARArmoryRoom(underground, insideRoomRect);
                                break;
                            }
                        case 1: // storage room.
                            {
                                roomName = "Storage";
                                MakeCHARStorageRoom(underground, insideRoomRect);
                                break;
                            }
                        case 2: // living room.
                            {
                                roomName = "Living";
                                MakeCHARLivingRoom(underground, insideRoomRect);
                                break;
                            }
                        case 3: // pharmacy.
                            {
                                roomName = "Pharmacy";
                                MakeCHARPharmacyRoom(underground, insideRoomRect);
                                break;
                            }
                        default:
                            throw new ArgumentOutOfRangeException("unhandled role");
                    }
                }

                underground.AddZone(MakeUniqueZone(roomName, insideRoomRect));
            }
            #endregion

            // 5. Posters & Blood.
            #region
            // char propaganda posters & blood almost everywhere.
            for (int x = 0; x < underground.Width; x++)
                for (int y = 0; y < underground.Height; y++)
                {
                    // poster on wall?
                    if (m_DiceRoller.RollChance(25))
                    {
                        Tile tile = underground.GetTileAt(x, y);
                        if (tile.Model.IsWalkable)
                            continue;
                        tile.AddDecoration(CHAR_POSTERS[m_DiceRoller.Roll(0, CHAR_POSTERS.Length)]);
                    }

                    // blood?
                    if (m_DiceRoller.RollChance(20))
                    {
                        Tile tile = underground.GetTileAt(x, y);
                        if (tile.Model.IsWalkable)
                            tile.AddDecoration(GameImages.DECO_BLOODIED_FLOOR);
                        else
                            tile.AddDecoration(GameImages.DECO_BLOODIED_WALL);
                    }
                }
            #endregion

            // 6. Populate.
            // don't block exits!
            #region
            // leveled up undeads!
            int nbZombies = underground.Width;  // 100 for 100.
            for (int i = 0; i < nbZombies; i++)
            {
                Actor undead = CreateNewUndead(0);
                for (; ; )
                {
                    GameActors.IDs upID = m_Game.NextUndeadEvolution((GameActors.IDs)undead.Model.ID);
                    if (upID == (GameActors.IDs)undead.Model.ID)
                        break;
                    undead.Model = m_Game.GameActors[upID];
                }
                ActorPlace(m_DiceRoller, underground.Width * underground.Height, underground, undead, (pt) => underground.GetExitAt(pt) == null);
            }

            // CHAR Guards.
            int nbGuards = underground.Width / 10; // 10 for 100.
            for (int i = 0; i < nbGuards; i++)
            {
                Actor guard = CreateNewCHARGuard(0);
                ActorPlace(m_DiceRoller, underground.Width * underground.Height, underground, guard, (pt) => underground.GetExitAt(pt) == null);
            }
            #endregion

            // 7. Add uniques.
            // TODO...
            #region
            #endregion

            // alpha10
            // 8. Music
            underground.BgMusic = GameMusics.CHAR_UNDERGROUND_FACILITY;

            // done.
            return underground;
        }

        void MakeCHARArmoryRoom(Map map, Rectangle roomRect)
        {
            // Shelves with weapons/ammo along walls.
            MapObjectFill(map, roomRect,
                (pt) =>
                {
                    if (CountAdjWalls(map, pt.X, pt.Y) < 3)
                        return null;
                    // dont block exits!
                    if (map.GetExitAt(pt) != null)
                        return null;

                    // table + tracker/armor/weapon.
                    if (m_DiceRoller.RollChance(20))
                    {
                        Item it;
                        if (m_DiceRoller.RollChance(20))
                            it = MakeItemCHARLightBodyArmor();
                        else if (m_DiceRoller.RollChance(20))
                        {
                            it = m_DiceRoller.RollChance(50) ? MakeItemZTracker() : MakeItemBlackOpsGPS();
                        }
                        else
                        {
                            // rare grenades.
                            if (m_DiceRoller.RollChance(20))
                            {
                                it = MakeItemGrenade();
                            }
                            else
                            {
                                // weapon vs ammo.
                                if (m_DiceRoller.RollChance(30))
                                {
                                    it = m_DiceRoller.RollChance(50) ? MakeItemShotgun() : MakeItemHuntingRifle();
                                }
                                else
                                {
                                    it = m_DiceRoller.RollChance(50) ? MakeItemShotgunAmmo() : MakeItemLightRifleAmmo();
                                }
                            }
                        }
                        map.DropItemAt(it, pt);

                        MapObject shelf = MakeObjShelf(GameImages.OBJ_SHOP_SHELF);
                        return shelf;
                    }
                    else
                        return null;
                });
        }

        void MakeCHARStorageRoom(Map map, Rectangle roomRect)
        {
            // Replace floor with concrete.
            TileFill(map, m_Game.GameTiles.FLOOR_CONCRETE, roomRect);

            // Objects.
            // Barrels & Junk in the middle of the room.
            MapObjectFill(map, roomRect,
                (pt) =>
                {
                    if (CountAdjWalls(map, pt.X, pt.Y) > 0)
                        return null;
                    // dont block exits!
                    if (map.GetExitAt(pt) != null)
                        return null;

                    // barrels/junk?
                    if (m_DiceRoller.RollChance(50))
                        return m_DiceRoller.RollChance(50) ? MakeObjJunk(GameImages.OBJ_JUNK) : MakeObjBarrels(GameImages.OBJ_BARRELS);
                    else
                        return null;
                });

            // Items.
            // Construction items in this mess.
            for (int x = roomRect.Left; x < roomRect.Right; x++)
                for (int y = roomRect.Top; y < roomRect.Bottom; y++)
                {
                    if (CountAdjWalls(map, x, y) > 0)
                        continue;
                    if (map.GetMapObjectAt(x, y) != null)
                        continue;

                    map.DropItemAt(MakeShopConstructionItem(), x, y);
                }
        }

        void MakeCHARLivingRoom(Map map, Rectangle roomRect)
        {
            // Replace floor with wood with painted logo.
            TileFill(map, m_Game.GameTiles.FLOOR_PLANKS, roomRect, (tile, model, x, y) => tile.AddDecoration(GameImages.DECO_CHAR_FLOOR_LOGO));

            // Objects.
            // Beds/Fridges along walls.
            MapObjectFill(map, roomRect,
                (pt) =>
                {
                    if (CountAdjWalls(map, pt.X, pt.Y) < 3)
                        return null;
                    // dont block exits!
                    if (map.GetExitAt(pt) != null)
                        return null;

                    // bed/fridge?
                    if (m_DiceRoller.RollChance(30))
                    {
                        if (m_DiceRoller.RollChance(50))
                            return MakeObjBed(GameImages.OBJ_BED);
                        else
                            return MakeObjFridge(GameImages.OBJ_FRIDGE);
                    }
                    else
                        return null;
                });
            // Tables(with canned food) & Chairs in the middle.
            MapObjectFill(map, roomRect,
                (pt) =>
                {
                    if (CountAdjWalls(map, pt.X, pt.Y) > 0)
                        return null;
                    // dont block exits!
                    if (map.GetExitAt(pt) != null)
                        return null;

                    // tables/chairs.
                    if (m_DiceRoller.RollChance(30))
                    {
                        if (m_DiceRoller.RollChance(30))
                        {
                            MapObject table = MakeObjTable(GameImages.OBJ_CHAR_TABLE);
                            map.DropItemAt(MakeItemCannedFood(), pt);
                            return table;
                        }
                        else
                            return MakeObjChair(GameImages.OBJ_CHAR_CHAIR);
                    }
                    else
                        return null;
                });
        }

        void MakeCHARPharmacyRoom(Map map, Rectangle roomRect)
        {
            // Shelves with medicine along walls.
            MapObjectFill(map, roomRect,
                (pt) =>
                {
                    if (CountAdjWalls(map, pt.X, pt.Y) < 3)
                        return null;
                    // dont block exits!
                    if (map.GetExitAt(pt) != null)
                        return null;

                    // table + meds.
                    if (m_DiceRoller.RollChance(20))
                    {
                        Item it = MakeHospitalItem();
                        map.DropItemAt(it, pt);

                        MapObject shelf = MakeObjShelf(GameImages.OBJ_SHOP_SHELF);
                        return shelf;
                    }
                    else
                        return null;
                });
        }

        void MakeCHARPowerRoom(Map map, Rectangle wallsRect, Rectangle roomRect)
        {
            // Replace floor with concrete.
            TileFill(map, m_Game.GameTiles.FLOOR_CONCRETE, roomRect);

            // add deco power sign next to doors.
            DoForEachTile(map, wallsRect,
                (pt) =>
                {
                    if (!(map.GetMapObjectAt(pt) is DoorWindow))
                        return;
                    DoForEachAdjacentInMap(map, pt, (
                        ptAdj) =>
                        {
                            Tile tile = map.GetTileAt(ptAdj);
                            if (tile.Model.IsWalkable)
                                return;
                            tile.RemoveAllDecorations();
                            tile.AddDecoration(GameImages.DECO_POWER_SIGN_BIG);
                        });
                });

            // add power generators along walls.
            DoForEachTile(map, roomRect,
                (pt) =>
                {
                    if (!map.GetTileAt(pt).Model.IsWalkable)
                        return;
                    if (map.GetExitAt(pt) != null)
                        return;
                    if (CountAdjWalls(map, pt.X, pt.Y) < 3)
                        return;

                    PowerGenerator powGen = MakeObjPowerGenerator(GameImages.OBJ_POWERGEN_OFF, GameImages.OBJ_POWERGEN_ON);
                    map.PlaceMapObjectAt(powGen, pt);
                });
        }
        #endregion

        #region Police Station
        void MakePoliceStation(Map map, List<Block> freeBlocks, out Block policeBlock)
        {
            ////////////////////////////////
            // 1. Pick a block.
            // 2. Generate surface station.
            // 3. Generate level -1.
            // 4. Generate level -2.
            // 5. Link maps.
            // 6. Add maps to district.
            // 7. Set unique maps.
            ////////////////////////////////

            // 1. Pick a block.
            // any random block will do.
            policeBlock = freeBlocks[m_DiceRoller.Roll(0, freeBlocks.Count)];

            // 2. Generate surface station.
            Point surfaceStairsPos;

            GeneratePoliceStation(map, policeBlock, out surfaceStairsPos);

            // 3. Generate Offices level (-1).
            Map officesLevel = GeneratePoliceStation_OfficesLevel(map, policeBlock, surfaceStairsPos);

            // 4. Generate Jails level (-2).
            Map jailsLevel = GeneratePoliceStation_JailsLevel(officesLevel);

            // alpha10 music
            officesLevel.BgMusic = jailsLevel.BgMusic = GameMusics.SURFACE;

            // 5. Link maps.
            // surface <-> offices level
            AddExit(map, surfaceStairsPos, officesLevel, new Point(1, 1), GameImages.DECO_STAIRS_DOWN, true);
            AddExit(officesLevel, new Point(1, 1), map, surfaceStairsPos, GameImages.DECO_STAIRS_UP, true);

            // offices <-> jails
            AddExit(officesLevel, new Point(1, officesLevel.Height - 2), jailsLevel, new Point(1, 1), GameImages.DECO_STAIRS_DOWN, true);
            AddExit(jailsLevel, new Point(1, 1), officesLevel, new Point(1, officesLevel.Height - 2), GameImages.DECO_STAIRS_UP, true);

            // 6. Add maps to district.
            m_Params.District.AddUniqueMap(officesLevel);
            m_Params.District.AddUniqueMap(jailsLevel);

            // 7. Set unique maps.
            m_Game.Session.UniqueMaps.PoliceStation_OfficesLevel = new UniqueMap() { TheMap = officesLevel };
            m_Game.Session.UniqueMaps.PoliceStation_JailsLevel = new UniqueMap() { TheMap = jailsLevel };

            // done!
        }

        void GeneratePoliceStation(Map surfaceMap, Block policeBlock, out Point stairsToLevel1)
        {
            // Fill & Enclose Building.
            TileFill(surfaceMap, m_Game.GameTiles.FLOOR_TILES, policeBlock.InsideRect);
            TileRectangle(surfaceMap, m_Game.GameTiles.WALL_POLICE_STATION, policeBlock.BuildingRect);
            TileRectangle(surfaceMap, m_Game.GameTiles.FLOOR_WALKWAY, policeBlock.Rectangle);
            DoForEachTile(surfaceMap, policeBlock.InsideRect, (pt) => surfaceMap.GetTileAt(pt).IsInside = true);

            // Entrance to the south with police signs.
            Point entryDoorPos = new Point(policeBlock.BuildingRect.Left + policeBlock.BuildingRect.Width / 2, policeBlock.BuildingRect.Bottom - 1);
            surfaceMap.GetTileAt(entryDoorPos.X - 1, entryDoorPos.Y).AddDecoration(GameImages.DECO_POLICE_STATION);
            surfaceMap.GetTileAt(entryDoorPos.X + 1, entryDoorPos.Y).AddDecoration(GameImages.DECO_POLICE_STATION);

            // Entry hall.
            Rectangle entryHall = Rectangle.FromLTRB(policeBlock.BuildingRect.Left, policeBlock.BuildingRect.Top + 2, policeBlock.BuildingRect.Right, policeBlock.BuildingRect.Bottom);
            TileRectangle(surfaceMap, m_Game.GameTiles.WALL_POLICE_STATION, entryHall);
            PlaceDoor(surfaceMap, entryHall.Left + entryHall.Width / 2, entryHall.Top, m_Game.GameTiles.FLOOR_TILES, MakeObjIronDoor());
            PlaceDoor(surfaceMap, entryDoorPos.X, entryDoorPos.Y, m_Game.GameTiles.FLOOR_TILES, MakeObjGlassDoor());
            DoForEachTile(surfaceMap, entryHall,
                (pt) =>
                {
                    if (!surfaceMap.IsWalkable(pt.X, pt.Y))
                        return;
                    if (CountAdjWalls(surfaceMap, pt.X, pt.Y) == 0 || CountAdjDoors(surfaceMap, pt.X, pt.Y) > 0)
                        return;
                    surfaceMap.PlaceMapObjectAt(MakeObjBench(GameImages.OBJ_BENCH), pt);
                });

            // Place stairs, north side.
            stairsToLevel1 = new Point(entryDoorPos.X, policeBlock.InsideRect.Top);

            // Zone.
            surfaceMap.AddZone(MakeUniqueZone("Police Station", policeBlock.BuildingRect));
            MakeWalkwayZones(surfaceMap, policeBlock);
        }

        Map GeneratePoliceStation_OfficesLevel(Map surfaceMap, Block policeBlock, Point exitPos)
        {
            //////////////////
            // 1. Create map.
            // 2. Floor plan.
            // 3. Populate.
            //////////////////

            // 1. Create map.
            int seed = (surfaceMap.Seed << 1) ^ surfaceMap.Seed;
            Map map = new Map(seed, "Police Station - Offices", 20, 20)
            {
                Lighting = Lighting.DARKNESS
            };
            DoForEachTile(map, map.Rect, (pt) => map.GetTileAt(pt).IsInside = true);

            // 2. Floor plan.
            TileFill(map, m_Game.GameTiles.FLOOR_TILES);
            TileRectangle(map, m_Game.GameTiles.WALL_POLICE_STATION, map.Rect);
            // - offices rooms on the east side, doors leading west.
            Rectangle officesRect = Rectangle.FromLTRB(3, 0, map.Width, map.Height);
            List<Rectangle> roomsList = new List<Rectangle>();
            MakeRoomsPlan(map, ref roomsList, officesRect, 5, 5);
            foreach (Rectangle roomRect in roomsList)
            {
                Rectangle inRoomRect = Rectangle.FromLTRB(roomRect.Left + 1, roomRect.Top + 1, roomRect.Right - 1, roomRect.Bottom - 1);
                // 2 kind of rooms.
                // - farthest east from corridor : security.
                // - others : offices.
                if (roomRect.Right == map.Width)
                {
                    // Police Security Room.
                    #region
                    // make room with door.
                    TileRectangle(map, m_Game.GameTiles.WALL_POLICE_STATION, roomRect);
                    PlaceDoor(map, roomRect.Left, roomRect.Top + roomRect.Height / 2, m_Game.GameTiles.FLOOR_CONCRETE, MakeObjIronDoor());

                    // shelves with weaponry & armor next to the walls.
                    DoForEachTile(map, inRoomRect,
                        (pt) =>
                        {
                            if (!map.IsWalkable(pt.X, pt.Y) || CountAdjWalls(map, pt.X, pt.Y) == 0 || CountAdjDoors(map, pt.X, pt.Y) > 0)
                                return;

                            // shelf.
                            map.PlaceMapObjectAt(MakeObjShelf(GameImages.OBJ_SHOP_SHELF), pt);

                            // weaponry/armor/radios.
                            Item it = null;
                            int roll = m_DiceRoller.Roll(0, 10);
                            switch (roll)
                            {
                                // 20% armors
                                case 0:
                                case 1:
                                    it = m_DiceRoller.RollChance(50) ? MakeItemPoliceJacket() : MakeItemPoliceRiotArmor();
                                    break;

                                // 20% light/radio
                                case 2:
                                case 3:
                                    it = m_DiceRoller.RollChance(50) ? (m_DiceRoller.RollChance(50) ? MakeItemFlashlight() : MakeItemBigFlashlight()) : MakeItemPoliceRadio();
                                    break;

                                // 20% truncheon
                                case 4:
                                case 5:
                                    it = MakeItemTruncheon();
                                    break;

                                // 20% pistol/ammo - 30% pistol 70% amo
                                case 6:
                                case 7:
                                    it = m_DiceRoller.RollChance(30) ? MakeItemPistol() : MakeItemLightPistolAmmo();
                                    break;

                                // 20% shotgun/ammo - 30% shotgun 70% amo
                                case 8:
                                case 9:
                                    it = m_DiceRoller.RollChance(30) ? MakeItemShotgun() : MakeItemShotgunAmmo();
                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException("unhandled roll");

                            }

                            map.DropItemAt(it, pt);

                        });

                    // zone.
                    map.AddZone(MakeUniqueZone("security", inRoomRect));
                    #endregion
                }
                else
                {
                    // Police Office Room.
                    #region
                    // make room with door.
                    TileFill(map, m_Game.GameTiles.FLOOR_PLANKS, roomRect);
                    TileRectangle(map, m_Game.GameTiles.WALL_POLICE_STATION, roomRect);
                    PlaceDoor(map, roomRect.Left, roomRect.Top + roomRect.Height / 2, m_Game.GameTiles.FLOOR_PLANKS, MakeObjWoodenDoor());

                    // add furniture : 1 table, 2 chairs.
                    MapObjectPlaceInGoodPosition(map, inRoomRect,
                        (pt) => map.IsWalkable(pt.X, pt.Y) && CountAdjDoors(map, pt.X, pt.Y) == 0,
                        m_DiceRoller,
                        (pt) => MakeObjTable(GameImages.OBJ_TABLE));
                    MapObjectPlaceInGoodPosition(map, inRoomRect,
                        (pt) => map.IsWalkable(pt.X, pt.Y) && CountAdjDoors(map, pt.X, pt.Y) == 0,
                        m_DiceRoller,
                        (pt) => MakeObjChair(GameImages.OBJ_CHAIR));
                    MapObjectPlaceInGoodPosition(map, inRoomRect,
                        (pt) => map.IsWalkable(pt.X, pt.Y) && CountAdjDoors(map, pt.X, pt.Y) == 0,
                        m_DiceRoller,
                        (pt) => MakeObjChair(GameImages.OBJ_CHAIR));

                    // zone.
                    map.AddZone(MakeUniqueZone("office", inRoomRect));
                    #endregion
                }
            }
            // - benches in corridor.
            DoForEachTile(map, new Rectangle(1, 1, 1, map.Height - 2),
                (pt) =>
                {
                    if (pt.Y % 2 == 1)
                        return;
                    if (!map.IsWalkable(pt))
                        return;
                    if (CountAdjWalls(map, pt) != 3)
                        return;

                    map.PlaceMapObjectAt(MakeObjIronBench(GameImages.OBJ_IRON_BENCH), pt);
                });

            // 3. Populate.
            // - cops.
            const int nbCops = 5;
            for (int i = 0; i < nbCops; i++)
            {
                Actor cop = CreateNewPoliceman(0);
                ActorPlace(m_DiceRoller, map.Width * map.Height, map, cop);
            }

            // done.
            return map;
        }

        Map GeneratePoliceStation_JailsLevel(Map surfaceMap)
        {
            //////////////////
            // 1. Create map.
            // 2. Floor plan.
            // 3. Populate.
            //////////////////

            // 1. Create map.
            int seed = (surfaceMap.Seed << 1) ^ surfaceMap.Seed;
            Map map = new Map(seed, "Police Station - Jails", 22, 6)
            {
                Lighting = Lighting.DARKNESS
            };
            DoForEachTile(map, map.Rect, (pt) => map.GetTileAt(pt).IsInside = true);

            // 2. Floor plan.
            TileFill(map, m_Game.GameTiles.FLOOR_TILES);
            TileRectangle(map, m_Game.GameTiles.WALL_POLICE_STATION, map.Rect);
            // - small cells.
            const int cellWidth = 3;
            const int cellHeight = 3;
            const int yCells = 3;
            List<Rectangle> cells = new List<Rectangle>();
            for (int x = 0; x + cellWidth <= map.Width; x += cellWidth - 1)
            {
                // room.
                Rectangle cellRoom = new Rectangle(x, yCells, cellWidth, cellHeight);
                cells.Add(cellRoom);
                TileFill(map, m_Game.GameTiles.FLOOR_CONCRETE, cellRoom);
                TileRectangle(map, m_Game.GameTiles.WALL_POLICE_STATION, cellRoom);

                // couch.
                Point couchPos = new Point(x + 1, yCells + 1);
                map.PlaceMapObjectAt(MakeObjIronBench(GameImages.OBJ_IRON_BENCH), couchPos);

                // gate.
                Point gatePos = new Point(x + 1, yCells);
                map.SetTileModelAt(gatePos.X, gatePos.Y, m_Game.GameTiles.FLOOR_CONCRETE);
                map.PlaceMapObjectAt(MakeObjIronGate(GameImages.OBJ_GATE_CLOSED, false), gatePos);  // alpha10.1 made unbreakable because civ ai can now bash their way out when trapped :p
                // zone.
                map.AddZone(MakeUniqueZone(RogueGame.NAME_POLICE_STATION_JAILS_CELL, cellRoom));
            }
            // - corridor.
            Rectangle corridor = Rectangle.FromLTRB(1, 1, map.Width, yCells);
            map.AddZone(MakeUniqueZone("cells corridor", corridor));
            // - the switch to open/close the cells.
            map.PlaceMapObjectAt(MakeObjPowerGenerator(GameImages.OBJ_POWERGEN_OFF, GameImages.OBJ_POWERGEN_ON), new Point(map.Width - 2, 1));

            // 3. Populate.
            // a prisoner in each cell.
            // alph10.1 the prisoner who should not be is now in one of the cell at random instead of always the last one
            int prisonnerCell = m_DiceRoller.Roll(0, cells.Count);
            for (int i = 0; i < cells.Count; i++)
            {
                Rectangle cell = cells[i];

                // prisonner who should not be or regular civilian
                Actor prisoner;
                if (i == prisonnerCell)
                {
                    // prisoner who should not be
                    prisoner = CreateNewCivilian(0, 0, 1);
                    prisoner.Name = "The Prisoner Who Should Not Be";

                    // plenty of food
                    for (int j = 0; j < prisoner.Inventory.MaxCapacity; j++)
                        prisoner.Inventory.AddAll(MakeItemArmyRation());

                    // register unique
                    m_Game.Session.UniqueActors.PoliceStationPrisoner = new UniqueActor()
                    {
                        TheActor = prisoner,
                        IsSpawned = true
                    };
                }
                else
                {
                    // jailed. Civilian.
                    prisoner = CreateNewCivilian(0, 0, 1);

                    // make sure he is stripped of all default items!
                    while (!prisoner.Inventory.IsEmpty)
                        prisoner.Inventory.RemoveAllQuantity(prisoner.Inventory[0]);

                    // give him some food.
                    prisoner.Inventory.AddAll(MakeItemGroceries());
                }

                // drop him.
                map.PlaceActorAt(prisoner, new Point(cell.Left + 1, cell.Top + 1));
            }

            // done.
            return map;
        }
        #endregion

        #region Hospital
        /// <summary>
        /// Layout :
        ///  0 floor: Entry Hall.
        /// -1 floor: Admissions (short term patients).
        /// -2 floor: Offices. (doctors)
        /// -3 floor: Patients. (nurses, injured patients)
        /// -4 floor: Storage. (bunch of meds & pills; blocked by closed gates, need power on)
        /// -5 floor: Power. (restore power to the whole building = lights, open storage gates)
        /// </summary>
        /// <param name="map"></param>
        /// <param name="freeBlocks"></param>
        /// <param name="hospitalBlock"></param>
        void MakeHospital(Map map, List<Block> freeBlocks, out Block hospitalBlock)
        {
            ////////////////////////////////
            // 1. Pick a block.
            // 2. Generate surface building.
            // 3. Generate other levels maps.
            // 5. Link maps.
            // 6. Add maps to district.
            // 7. Set unique maps.
            ////////////////////////////////

            // 1. Pick a block.
            // any random block will do.
            hospitalBlock = freeBlocks[m_DiceRoller.Roll(0, freeBlocks.Count)];

            // 2. Generate surface.
            GenerateHospitalEntryHall(map, hospitalBlock);

            // 3. Generate other levels maps.
            Map admissions = GenerateHospital_Admissions((map.Seed << 1) ^ map.Seed);
            Map offices = GenerateHospital_Offices((map.Seed << 2) ^ map.Seed);
            Map patients = GenerateHospital_Patients((map.Seed << 3) ^ map.Seed);
            Map storage = GenerateHospital_Storage((map.Seed << 4) ^ map.Seed);
            Map power = GenerateHospital_Power((map.Seed << 5) ^ map.Seed);

            // alpha10 music
            admissions.BgMusic = offices.BgMusic = patients.BgMusic = storage.BgMusic = power.BgMusic = GameMusics.HOSPITAL;

            // 5. Link maps.
            // entry <-> admissions
            Point entryStairs = new Point(hospitalBlock.InsideRect.Left + hospitalBlock.InsideRect.Width / 2, hospitalBlock.InsideRect.Top);
            Point admissionsUpStairs = new Point(admissions.Width / 2, 1);
            AddExit(map, entryStairs, admissions, admissionsUpStairs, GameImages.DECO_STAIRS_DOWN, true);
            AddExit(admissions, admissionsUpStairs, map, entryStairs, GameImages.DECO_STAIRS_UP, true);

            // admissions <-> offices
            Point admissionsDownStairs = new Point(admissions.Width / 2, admissions.Height - 2);
            Point officesUpStairs = new Point(offices.Width / 2, 1);
            AddExit(admissions, admissionsDownStairs, offices, officesUpStairs, GameImages.DECO_STAIRS_DOWN, true);
            AddExit(offices, officesUpStairs, admissions, admissionsDownStairs, GameImages.DECO_STAIRS_UP, true);

            // offices <-> patients
            Point officesDownStairs = new Point(offices.Width / 2, offices.Height - 2);
            Point patientsUpStairs = new Point(patients.Width / 2, 1);
            AddExit(offices, officesDownStairs, patients, patientsUpStairs, GameImages.DECO_STAIRS_DOWN, true);
            AddExit(patients, patientsUpStairs, offices, officesDownStairs, GameImages.DECO_STAIRS_UP, true);

            // patients <-> storage
            Point patientsDownStairs = new Point(patients.Width / 2, patients.Height - 2);
            Point storageUpStairs = new Point(1, 1);
            AddExit(patients, patientsDownStairs, storage, storageUpStairs, GameImages.DECO_STAIRS_DOWN, true);
            AddExit(storage, storageUpStairs, patients, patientsDownStairs, GameImages.DECO_STAIRS_UP, true);

            // storage <-> power
            Point storageDownStairs = new Point(storage.Width - 2, 1);
            Point powerUpStairs = new Point(1, 1);
            AddExit(storage, storageDownStairs, power, powerUpStairs, GameImages.DECO_STAIRS_DOWN, true);
            AddExit(power, powerUpStairs, storage, storageDownStairs, GameImages.DECO_STAIRS_UP, true);

            // 6. Add maps to district.
            m_Params.District.AddUniqueMap(admissions);
            m_Params.District.AddUniqueMap(offices);
            m_Params.District.AddUniqueMap(patients);
            m_Params.District.AddUniqueMap(storage);
            m_Params.District.AddUniqueMap(power);

            // 7. Set unique maps.
            m_Game.Session.UniqueMaps.Hospital_Admissions = new UniqueMap() { TheMap = admissions };
            m_Game.Session.UniqueMaps.Hospital_Offices = new UniqueMap() { TheMap = offices };
            m_Game.Session.UniqueMaps.Hospital_Patients = new UniqueMap() { TheMap = patients };
            m_Game.Session.UniqueMaps.Hospital_Storage = new UniqueMap() { TheMap = storage };
            m_Game.Session.UniqueMaps.Hospital_Power = new UniqueMap() { TheMap = power };

            // done!
        }

        void GenerateHospitalEntryHall(Map surfaceMap, Block block)
        {
            // Fill & Enclose Building.
            TileFill(surfaceMap, m_Game.GameTiles.FLOOR_TILES, block.InsideRect);
            TileRectangle(surfaceMap, m_Game.GameTiles.WALL_HOSPITAL, block.BuildingRect);
            TileRectangle(surfaceMap, m_Game.GameTiles.FLOOR_WALKWAY, block.Rectangle);
            DoForEachTile(surfaceMap, block.InsideRect, (pt) => surfaceMap.GetTileAt(pt).IsInside = true);

            // 2 entrances to the south with signs.
            Point entryRightDoorPos = new Point(block.BuildingRect.Left + block.BuildingRect.Width / 2, block.BuildingRect.Bottom - 1);
            Point entryLeftDoorPos = new Point(entryRightDoorPos.X - 1, entryRightDoorPos.Y);
            surfaceMap.GetTileAt(entryLeftDoorPos.X - 1, entryLeftDoorPos.Y).AddDecoration(GameImages.DECO_HOSPITAL);
            surfaceMap.GetTileAt(entryRightDoorPos.X + 1, entryRightDoorPos.Y).AddDecoration(GameImages.DECO_HOSPITAL);

            // Entry hall = whole building.
            Rectangle entryHall = Rectangle.FromLTRB(block.BuildingRect.Left, block.BuildingRect.Top, block.BuildingRect.Right, block.BuildingRect.Bottom);
            PlaceDoor(surfaceMap, entryRightDoorPos.X, entryRightDoorPos.Y, m_Game.GameTiles.FLOOR_TILES, MakeObjGlassDoor());
            PlaceDoor(surfaceMap, entryLeftDoorPos.X, entryLeftDoorPos.Y, m_Game.GameTiles.FLOOR_TILES, MakeObjGlassDoor());
            DoForEachTile(surfaceMap, entryHall,
                (pt) =>
                {
                    // benches only on west & east sides.
                    if (pt.Y == block.InsideRect.Top || pt.Y == block.InsideRect.Bottom - 1)
                        return;
                    if (!surfaceMap.IsWalkable(pt.X, pt.Y))
                        return;
                    if (CountAdjWalls(surfaceMap, pt.X, pt.Y) == 0 || CountAdjDoors(surfaceMap, pt.X, pt.Y) > 0)
                        return;
                    surfaceMap.PlaceMapObjectAt(MakeObjIronBench(GameImages.OBJ_IRON_BENCH), pt);
                });

            // Zone.
            surfaceMap.AddZone(MakeUniqueZone("Hospital", block.BuildingRect));
            MakeWalkwayZones(surfaceMap, block);
        }

        Map GenerateHospital_Admissions(int seed)
        {
            //////////////////
            // 1. Create map.
            // 2. Floor plan.
            // 3. Populate.
            //////////////////

            // 1. Create map.
            Map map = new Map(seed, "Hospital - Admissions", 13, 33)
            {
                Lighting = Lighting.DARKNESS
            };
            DoForEachTile(map, map.Rect, (pt) => map.GetTileAt(pt).IsInside = true);
            TileFill(map, m_Game.GameTiles.FLOOR_TILES);
            TileRectangle(map, m_Game.GameTiles.WALL_HOSPITAL, map.Rect);

            // 2. Floor plan.
            // One central south->north corridor with admission rooms on each sides.
            const int roomSize = 5;

            // 1. Central corridor.
            Rectangle corridor = new Rectangle(roomSize - 1, 0, 5, map.Height);
            TileRectangle(map, m_Game.GameTiles.WALL_HOSPITAL, corridor);
            map.AddZone(MakeUniqueZone("corridor", corridor));

            // 2. Admission rooms, all similar 5x5 rooms (3x3 inside)            
            Rectangle leftWing = new Rectangle(0, 0, roomSize, map.Height);
            for (int roomY = 0; roomY <= map.Height - roomSize; roomY += roomSize - 1)
            {
                Rectangle room = new Rectangle(leftWing.Left, roomY, roomSize, roomSize);
                MakeHospitalPatientRoom(map, "patient room", room, true);
            }

            Rectangle rightWing = new Rectangle(map.Rect.Right - roomSize, 0, roomSize, map.Height);
            for (int roomY = 0; roomY <= map.Height - roomSize; roomY += roomSize - 1)
            {
                Rectangle room = new Rectangle(rightWing.Left, roomY, roomSize, roomSize);
                MakeHospitalPatientRoom(map, "patient room", room, false);
            }

            // 3. Populate.
            // patients in rooms.
            const int nbPatients = 10;
            for (int i = 0; i < nbPatients; i++)
            {
                // create.
                Actor patient = CreateNewHospitalPatient(0);
                // place.
                ActorPlace(m_DiceRoller, map.Width * map.Height, map, patient, (pt) => map.HasZonePartiallyNamedAt(pt, "patient room"));
            }

            // nurses & doctor in corridor.
            const int nbNurses = 4;
            for (int i = 0; i < nbNurses; i++)
            {
                // create.
                Actor nurse = CreateNewHospitalNurse(0);
                // place.
                ActorPlace(m_DiceRoller, map.Width * map.Height, map, nurse, (pt) => map.HasZonePartiallyNamedAt(pt, "corridor"));
            }
            const int nbDoctor = 1;
            for (int i = 0; i < nbDoctor; i++)
            {
                // create.
                Actor nurse = CreateNewHospitalDoctor(0);
                // place.
                ActorPlace(m_DiceRoller, map.Width * map.Height, map, nurse, (pt) => map.HasZonePartiallyNamedAt(pt, "corridor"));
            }

            // done.
            return map;
        }

        Map GenerateHospital_Offices(int seed)
        {
            //////////////////
            // 1. Create map.
            // 2. Floor plan.
            // 3. Populate.
            //////////////////

            // 1. Create map.
            Map map = new Map(seed, "Hospital - Offices", 13, 33)
            {
                Lighting = Lighting.DARKNESS
            };
            DoForEachTile(map, map.Rect, (pt) => map.GetTileAt(pt).IsInside = true);
            TileFill(map, m_Game.GameTiles.FLOOR_TILES);
            TileRectangle(map, m_Game.GameTiles.WALL_HOSPITAL, map.Rect);

            // 2. Floor plan.
            // One central south->north corridor with offices rooms on each sides.
            const int roomSize = 5;

            // 1. Central corridor.
            Rectangle corridor = new Rectangle(roomSize - 1, 0, 5, map.Height);
            TileRectangle(map, m_Game.GameTiles.WALL_HOSPITAL, corridor);
            map.AddZone(MakeUniqueZone("corridor", corridor));

            // 2. Offices rooms, all similar 5x5 rooms (3x3 inside)
            Rectangle leftWing = new Rectangle(0, 0, roomSize, map.Height);
            for (int roomY = 0; roomY <= map.Height - roomSize; roomY += roomSize - 1)
            {
                Rectangle room = new Rectangle(leftWing.Left, roomY, roomSize, roomSize);
                MakeHospitalOfficeRoom(map, "office", room, true);
            }

            Rectangle rightWing = new Rectangle(map.Rect.Right - roomSize, 0, roomSize, map.Height);
            for (int roomY = 0; roomY <= map.Height - roomSize; roomY += roomSize - 1)
            {
                Rectangle room = new Rectangle(rightWing.Left, roomY, roomSize, roomSize);
                MakeHospitalOfficeRoom(map, "office", room, false);
            }

            // 3. Populate.
            // nurses & doctor in offices.
            const int nbNurses = 5;
            for (int i = 0; i < nbNurses; i++)
            {
                // create.
                Actor nurse = CreateNewHospitalNurse(0);
                // place.
                ActorPlace(m_DiceRoller, map.Width * map.Height, map, nurse, (pt) => map.HasZonePartiallyNamedAt(pt, "office"));
            }
            const int nbDoctor = 2;
            for (int i = 0; i < nbDoctor; i++)
            {
                // create.
                Actor nurse = CreateNewHospitalDoctor(0);
                // place.
                ActorPlace(m_DiceRoller, map.Width * map.Height, map, nurse, (pt) => map.HasZonePartiallyNamedAt(pt, "office"));
            }

            // done.
            return map;
        }

        Map GenerateHospital_Patients(int seed)
        {
            //////////////////
            // 1. Create map.
            // 2. Floor plan.
            // 3. Populate.
            //////////////////

            // 1. Create map.
            Map map = new Map(seed, "Hospital - Patients", 13, 49)
            {
                Lighting = Lighting.DARKNESS
            };
            DoForEachTile(map, map.Rect, (pt) => map.GetTileAt(pt).IsInside = true);
            TileFill(map, m_Game.GameTiles.FLOOR_TILES);
            TileRectangle(map, m_Game.GameTiles.WALL_HOSPITAL, map.Rect);

            // 2. Floor plan.
            // One central south->north corridor with admission rooms on each sides.
            const int roomSize = 5;

            // 1. Central corridor.
            Rectangle corridor = new Rectangle(roomSize - 1, 0, 5, map.Height);
            TileRectangle(map, m_Game.GameTiles.WALL_HOSPITAL, corridor);
            map.AddZone(MakeUniqueZone("corridor", corridor));

            // 2. Patients rooms, all similar 5x5 rooms (3x3 inside)            
            Rectangle leftWing = new Rectangle(0, 0, roomSize, map.Height);
            for (int roomY = 0; roomY <= map.Height - roomSize; roomY += roomSize - 1)
            {
                Rectangle room = new Rectangle(leftWing.Left, roomY, roomSize, roomSize);
                MakeHospitalPatientRoom(map, "patient room", room, true);
            }

            Rectangle rightWing = new Rectangle(map.Rect.Right - roomSize, 0, roomSize, map.Height);
            for (int roomY = 0; roomY <= map.Height - roomSize; roomY += roomSize - 1)
            {
                Rectangle room = new Rectangle(rightWing.Left, roomY, roomSize, roomSize);
                MakeHospitalPatientRoom(map, "patient room", room, false);
            }

            // 3. Populate.
            // patients in rooms.
            const int nbPatients = 20;
            for (int i = 0; i < nbPatients; i++)
            {
                // create.
                Actor patient = CreateNewHospitalPatient(0);
                // place.
                ActorPlace(m_DiceRoller, map.Width * map.Height, map, patient, (pt) => map.HasZonePartiallyNamedAt(pt, "patient room"));
            }

            // nurses & doctor in corridor.
            const int nbNurses = 8;
            for (int i = 0; i < nbNurses; i++)
            {
                // create.
                Actor nurse = CreateNewHospitalNurse(0);
                // place.
                ActorPlace(m_DiceRoller, map.Width * map.Height, map, nurse, (pt) => map.HasZonePartiallyNamedAt(pt, "corridor"));
            }
            const int nbDoctor = 2;
            for (int i = 0; i < nbDoctor; i++)
            {
                // create.
                Actor nurse = CreateNewHospitalDoctor(0);
                // place.
                ActorPlace(m_DiceRoller, map.Width * map.Height, map, nurse, (pt) => map.HasZonePartiallyNamedAt(pt, "corridor"));
            }

            // done.
            return map;
        }

        Map GenerateHospital_Storage(int seed)
        {
            //////////////////
            // 1. Create map.
            // 2. Floor plan.
            //////////////////

            // 1. Create map.
            Map map = new Map(seed, "Hospital - Storage", 51, 16)
            {
                Lighting = Lighting.DARKNESS
            };
            DoForEachTile(map, map.Rect, (pt) => map.GetTileAt(pt).IsInside = true);
            TileFill(map, m_Game.GameTiles.FLOOR_TILES);
            TileRectangle(map, m_Game.GameTiles.WALL_HOSPITAL, map.Rect);

            // 2. Floor plan.
            // 1 north corridor linking stairs.
            // 1 central corridor to storage rooms, locked by an iron gate.
            // 1 south corridor to other storage rooms.

            // 1 north corridor linking stairs.
            const int northCorridorHeight = 4;
            Rectangle northCorridorRect = Rectangle.FromLTRB(0, 0, map.Width, northCorridorHeight);
            TileRectangle(map, m_Game.GameTiles.WALL_HOSPITAL, northCorridorRect);
            map.AddZone(MakeUniqueZone("north corridor", northCorridorRect));

            // 1 corridor to storage rooms, locked by an iron gate.
            const int corridorHeight = 4;
            Rectangle centralCorridorRect = Rectangle.FromLTRB(0, northCorridorRect.Bottom - 1, map.Width, northCorridorRect.Bottom - 1 + corridorHeight);
            TileRectangle(map, m_Game.GameTiles.WALL_HOSPITAL, centralCorridorRect);
            map.SetTileModelAt(1, centralCorridorRect.Top, m_Game.GameTiles.FLOOR_TILES);
            map.PlaceMapObjectAt(MakeObjIronGate(GameImages.OBJ_GATE_CLOSED), new Point(1, centralCorridorRect.Top));
            map.AddZone(MakeUniqueZone("central corridor", centralCorridorRect));
            // storage rooms.
            const int storageWidth = 5;
            const int storageHeight = 4;
            Rectangle storageCentral = new Rectangle(2, centralCorridorRect.Bottom - 1, map.Width - 2, storageHeight);
            for (int roomX = storageCentral.Left; roomX <= map.Width - storageWidth; roomX += storageWidth - 1)
            {
                Rectangle room = new Rectangle(roomX, storageCentral.Top, storageWidth, storageHeight);
                MakeHospitalStorageRoom(map, "storage", room);
            }
            map.SetTileModelAt(1, storageCentral.Top, m_Game.GameTiles.FLOOR_TILES);

            // 1 south corridor to other storage rooms.
            Rectangle southCorridorRect = Rectangle.FromLTRB(0, storageCentral.Bottom - 1, map.Width, storageCentral.Bottom - 1 + corridorHeight);
            TileRectangle(map, m_Game.GameTiles.WALL_HOSPITAL, southCorridorRect);
            map.SetTileModelAt(1, southCorridorRect.Top, m_Game.GameTiles.FLOOR_TILES);
            map.AddZone(MakeUniqueZone("south corridor", southCorridorRect));
            // storage rooms.
            Rectangle storageSouth = new Rectangle(2, southCorridorRect.Bottom - 1, map.Width - 2, storageHeight);
            for (int roomX = storageSouth.Left; roomX <= map.Width - storageWidth; roomX += storageWidth - 1)
            {
                Rectangle room = new Rectangle(roomX, storageSouth.Top, storageWidth, storageHeight);
                MakeHospitalStorageRoom(map, "storage", room);
            }
            map.SetTileModelAt(1, storageSouth.Top, m_Game.GameTiles.FLOOR_TILES);

            // alpha10.1 moved Jason Myers out of power room to storage north corridor
            // also upped high stamina to 5 (was 3).
            // Jason Myers
            ActorModel model = m_Game.GameActors.JasonMyers;
            Actor jason = model.CreateNamed(m_Game.GameFactions.ThePsychopaths, "Jason Myers", false, 0);
            jason.IsUnique = true;
            jason.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_JASON_MYERS);
            GiveStartingSkillToActor(jason, Skills.IDs.TOUGH);
            GiveStartingSkillToActor(jason, Skills.IDs.TOUGH);
            GiveStartingSkillToActor(jason, Skills.IDs.TOUGH);
            GiveStartingSkillToActor(jason, Skills.IDs.STRONG);
            GiveStartingSkillToActor(jason, Skills.IDs.STRONG);
            GiveStartingSkillToActor(jason, Skills.IDs.STRONG);
            GiveStartingSkillToActor(jason, Skills.IDs.AGILE);
            GiveStartingSkillToActor(jason, Skills.IDs.AGILE);
            GiveStartingSkillToActor(jason, Skills.IDs.AGILE);
            GiveStartingSkillToActor(jason, Skills.IDs.HIGH_STAMINA);
            GiveStartingSkillToActor(jason, Skills.IDs.HIGH_STAMINA);
            GiveStartingSkillToActor(jason, Skills.IDs.HIGH_STAMINA);
            GiveStartingSkillToActor(jason, Skills.IDs.HIGH_STAMINA);
            GiveStartingSkillToActor(jason, Skills.IDs.HIGH_STAMINA);
            jason.Inventory.AddAll(MakeItemJasonMyersAxe());
            map.PlaceActorAt(jason, new Point(map.Width / 2, 1));
            m_Game.Session.UniqueActors.JasonMyers = new UniqueActor()
            {
                TheActor = jason,
                IsSpawned = true
            };

            // done.
            return map;
        }

        Map GenerateHospital_Power(int seed)
        {
            //////////////////
            // 1. Create map.
            // 2. Floor plan.
            // 3. Populate.
            //////////////////

            // 1. Create map.
            Map map = new Map(seed, "Hospital - Power", 10, 10)
            {
                Lighting = Lighting.DARKNESS
            };
            DoForEachTile(map, map.Rect, (pt) => map.GetTileAt(pt).IsInside = true);
            TileFill(map, m_Game.GameTiles.FLOOR_CONCRETE);
            TileRectangle(map, m_Game.GameTiles.WALL_BRICK, map.Rect);

            // 2. Floor plan.
            // one narrow corridor separated from the power gen room by iron fences.
            // barricade room for the Enraged Patient.

            // corridor with fences.
            Rectangle corridor = Rectangle.FromLTRB(1, 1, 3, map.Height);
            map.AddZone(MakeUniqueZone("corridor", corridor));
            for (int yFence = 1; yFence < map.Height - 2; yFence++)
                map.PlaceMapObjectAt(MakeObjIronFence(GameImages.OBJ_IRON_FENCE), new Point(2, yFence));

            // power room.
            Rectangle room = Rectangle.FromLTRB(3, 0, map.Width, map.Height);
            map.AddZone(MakeUniqueZone("power room", room));

            // power generators.
            DoForEachTile(map, room,
                (pt) =>
                {
                    if (pt.X == room.Left)
                        return;
                    if (!map.IsWalkable(pt))
                        return;
                    if (CountAdjWalls(map, pt) < 3)
                        return;

                    map.PlaceMapObjectAt(MakeObjPowerGenerator(GameImages.OBJ_POWERGEN_OFF, GameImages.OBJ_POWERGEN_ON), pt);
                });

            // alpha10.1 moved Jason Myers out of power room to storage north corridor
            /*
            // 3. Populate.
            // enraged patient!
            ActorModel model = m_Game.GameActors.JasonMyers;
            Actor jason = model.CreateNamed(m_Game.GameFactions.ThePsychopaths, "Jason Myers", false, 0);
            jason.IsUnique = true;
            jason.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_JASON_MYERS);
            GiveStartingSkillToActor(jason, Skills.IDs.TOUGH);
            GiveStartingSkillToActor(jason, Skills.IDs.TOUGH);
            GiveStartingSkillToActor(jason, Skills.IDs.TOUGH);
            GiveStartingSkillToActor(jason, Skills.IDs.STRONG);
            GiveStartingSkillToActor(jason, Skills.IDs.STRONG);
            GiveStartingSkillToActor(jason, Skills.IDs.STRONG);
            GiveStartingSkillToActor(jason, Skills.IDs.AGILE);
            GiveStartingSkillToActor(jason, Skills.IDs.AGILE);
            GiveStartingSkillToActor(jason, Skills.IDs.AGILE);
            GiveStartingSkillToActor(jason, Skills.IDs.HIGH_STAMINA);
            GiveStartingSkillToActor(jason, Skills.IDs.HIGH_STAMINA);
            GiveStartingSkillToActor(jason, Skills.IDs.HIGH_STAMINA);
            jason.Inventory.AddAll(MakeItemJasonMyersAxe());
            map.PlaceActorAt(jason, new Point(map.Width / 2, map.Height / 2));
            m_Game.Session.UniqueActors.JasonMyers = new UniqueActor()
            {
                TheActor = jason,
                IsSpawned = true
            };
            */

            // done.
            return map;
        }

        Actor CreateNewHospitalPatient(int spawnTime)
        {
            // decide model.
            ActorModel model = m_Rules.Roll(0, 2) == 0 ? m_Game.GameActors.MaleCivilian : m_Game.GameActors.FemaleCivilian;

            // create.
            Actor patient = model.CreateNumberedName(m_Game.GameFactions.TheCivilians, 0);
            SkinNakedHuman(m_DiceRoller, patient);
            GiveNameToActor(m_DiceRoller, patient);
            patient.Name = "Patient " + patient.Name;
            //patient.Controller = new CivilianAI();  // alpha10.1 defined by model like other actors

            // skills.
            GiveRandomSkillsToActor(m_DiceRoller, patient, 1);

            // add patient uniform.
            patient.Doll.AddDecoration(DollPart.TORSO, GameImages.HOSPITAL_PATIENT_UNIFORM);

            // done.
            return patient;
        }

        Actor CreateNewHospitalNurse(int spawnTime)
        {
            // create.
            Actor nurse = m_Game.GameActors.FemaleCivilian.CreateNumberedName(m_Game.GameFactions.TheCivilians, 0);
            SkinNakedHuman(m_DiceRoller, nurse);
            GiveNameToActor(m_DiceRoller, nurse);
            nurse.Name = "Nurse " + nurse.Name;
            //nurse.Controller = new CivilianAI(); // alpha10.1 defined by model like other actors

            // add uniform.
            nurse.Doll.AddDecoration(DollPart.TORSO, GameImages.HOSPITAL_NURSE_UNIFORM);

            // skills : 1 + 1-Medic.
            GiveRandomSkillsToActor(m_DiceRoller, nurse, 1);
            GiveStartingSkillToActor(nurse, Skills.IDs.MEDIC);

            // items : bandages.
            nurse.Inventory.AddAll(MakeItemBandages());

            // done.
            return nurse;
        }

        Actor CreateNewHospitalDoctor(int spawnTime)
        {
            // create.
            Actor doctor = m_Game.GameActors.MaleCivilian.CreateNumberedName(m_Game.GameFactions.TheCivilians, 0);
            SkinNakedHuman(m_DiceRoller, doctor);
            GiveNameToActor(m_DiceRoller, doctor);
            doctor.Name = "Doctor " + doctor.Name;
            //doctor.Controller = new CivilianAI(); // alpha10.1 defined by model like other actors

            // add uniform.
            doctor.Doll.AddDecoration(DollPart.TORSO, GameImages.HOSPITAL_DOCTOR_UNIFORM);

            // skills : 1 + 3-Medic + 1-Leadership.
            GiveRandomSkillsToActor(m_DiceRoller, doctor, 1);
            GiveStartingSkillToActor(doctor, Skills.IDs.MEDIC);
            GiveStartingSkillToActor(doctor, Skills.IDs.MEDIC);
            GiveStartingSkillToActor(doctor, Skills.IDs.MEDIC);
            GiveStartingSkillToActor(doctor, Skills.IDs.LEADERSHIP);

            // items : medikit + bandages.
            doctor.Inventory.AddAll(MakeItemMedikit());
            doctor.Inventory.AddAll(MakeItemBandages());

            // done.
            return doctor;
        }

        void MakeHospitalPatientRoom(Map map, string baseZoneName, Rectangle room, bool isFacingEast)
        {
            TileRectangle(map, m_Game.GameTiles.WALL_HOSPITAL, room);
            map.AddZone(MakeUniqueZone(baseZoneName, room));

            int xDoor = (isFacingEast ? room.Right - 1 : room.Left);

            // door in the corner.
            PlaceDoor(map, xDoor, room.Top + 1, m_Game.GameTiles.FLOOR_TILES, MakeObjHospitalDoor());

            // bed in the middle in the south.
            Point bedPos = new Point(room.Left + room.Width / 2, room.Bottom - 2);
            map.PlaceMapObjectAt(MakeObjBed(GameImages.OBJ_HOSPITAL_BED), bedPos);

            // chair and nighttable on either side of the bed.
            map.PlaceMapObjectAt(MakeObjChair(GameImages.OBJ_HOSPITAL_CHAIR), new Point(isFacingEast ? bedPos.X + 1 : bedPos.X - 1, bedPos.Y));
            Point tablePos = new Point(isFacingEast ? bedPos.X - 1 : bedPos.X + 1, bedPos.Y);
            map.PlaceMapObjectAt(MakeObjNightTable(GameImages.OBJ_HOSPITAL_NIGHT_TABLE), tablePos);

            // chance of some meds/food/book on nightable.
            if (m_DiceRoller.RollChance(50))
            {
                int roll = m_DiceRoller.Roll(0, 3);
                Item it = null;
                switch (roll)
                {
                    case 0: it = MakeShopPharmacyItem(); break;
                    case 1: it = MakeItemGroceries(); break;
                    case 2: it = MakeItemBook(); break;
                }
                if (it != null)
                    map.DropItemAt(it, tablePos);
            }

            // wardrobe in the corner.
            map.PlaceMapObjectAt(MakeObjWardrobe(GameImages.OBJ_HOSPITAL_WARDROBE), new Point(isFacingEast ? room.Left + 1 : room.Right - 2, room.Top + 1));
        }

        void MakeHospitalOfficeRoom(Map map, string baseZoneName, Rectangle room, bool isFacingEast)
        {
            TileFill(map, m_Game.GameTiles.FLOOR_PLANKS, room);
            TileRectangle(map, m_Game.GameTiles.WALL_HOSPITAL, room);
            map.AddZone(MakeUniqueZone(baseZoneName, room));

            int xDoor = (isFacingEast ? room.Right - 1 : room.Left);
            int yDoor = room.Top + 2;

            // door in the middle.
            PlaceDoor(map, xDoor, yDoor, m_Game.GameTiles.FLOOR_TILES, MakeObjWoodenDoor());

            // chairs and table facing the door.
            int xTable = (isFacingEast ? room.Left + 2 : room.Right - 3);
            map.PlaceMapObjectAt(MakeObjTable(GameImages.OBJ_TABLE), new Point(xTable, yDoor));
            map.PlaceMapObjectAt(MakeObjChair(GameImages.OBJ_CHAIR), new Point(xTable - 1, yDoor));
            map.PlaceMapObjectAt(MakeObjChair(GameImages.OBJ_CHAIR), new Point(xTable + 1, yDoor));
        }

        void MakeHospitalStorageRoom(Map map, string baseZoneName, Rectangle room)
        {
            TileRectangle(map, m_Game.GameTiles.WALL_HOSPITAL, room);
            map.AddZone(MakeUniqueZone(baseZoneName, room));

            // door.
            PlaceDoor(map, room.Left + 2, room.Top, m_Game.GameTiles.FLOOR_TILES, MakeObjHospitalDoor());

            // shelves with meds or food.
            DoForEachTile(map, room,
                (pt) =>
                {
                    if (!map.IsWalkable(pt))
                        return;

                    if (CountAdjDoors(map, pt.X, pt.Y) > 0)
                        return;

                    // shelf.
                    map.PlaceMapObjectAt(MakeObjShelf(GameImages.OBJ_SHOP_SHELF), pt);

                    // full stacks of meds or canned food.
                    Item it;
                    it = m_DiceRoller.RollChance(80) ? MakeHospitalItem() : MakeItemCannedFood();
                    if (it.Model.IsStackable)
                        it.Quantity = it.Model.StackingLimit;
                    map.DropItemAt(it, pt);
                });

            // alpha10 
            // chance to spawn a nurse
            if (m_DiceRoller.RollChance(20))  // alpha10.1 increased to 20% (avg 5 nurses for 24 storage rooms)
            {
                bool spawnedActor = false;
                DoForEachTile(map, room,
                    (pt) =>
                    {
                        if (spawnedActor)
                            return;
                        if (!map.IsWalkable(pt))
                            return;
                        if (map.GetMapObjectAt(pt) != null)
                            return;
                        map.PlaceActorAt(CreateNewHospitalNurse(0), pt);
                        spawnedActor = true;
                    });
            }
        }

        #endregion
        #endregion

        #region Actors

        public void GiveRandomItemToActor(DiceRoller roller, Actor actor, int spawnTime)
        {
            Item it = null;

            // rare item chance after Day X
            int day = new WorldTime(spawnTime).Day;
            if (day > Rules.GIVE_RARE_ITEM_DAY && roller.RollChance(Rules.GIVE_RARE_ITEM_CHANCE))
            {
                int roll = roller.Roll(0, 6);
                switch (roll)
                {
                    case 0: it = MakeItemGrenade(); break;
                    case 1: it = MakeItemArmyBodyArmor(); break;
                    case 2: it = MakeItemHeavyPistolAmmo(); break;
                    case 3: it = MakeItemHeavyRifleAmmo(); break;
                    case 4: it = MakeItemPillsAntiviral(); break;
                    case 5: it = MakeItemCombatKnife(); break;
                    default: it = null; break;
                }
            }
            else
            {
                // standard item.
                int roll = roller.Roll(0, 10);
                switch (roll)
                {
                    case 0: it = MakeRandomShopItem(ShopType.CONSTRUCTION); break;
                    case 1: it = MakeRandomShopItem(ShopType.GENERAL_STORE); break;
                    case 2: it = MakeRandomShopItem(ShopType.GROCERY); break;
                    case 3: it = MakeRandomShopItem(ShopType.GUNSHOP); break;
                    case 4: it = MakeRandomShopItem(ShopType.PHARMACY); break;
                    case 5: it = MakeRandomShopItem(ShopType.SPORTSWEAR); break;
                    case 6: it = MakeRandomShopItem(ShopType.HUNTING); break;
                    case 7: it = MakeRandomParkItem(); break;
                    case 8: it = MakeRandomBedroomItem(); break;
                    case 9: it = MakeRandomKitchenItem(); break;
                    default: it = null; break;
                }
            }

            if (it != null)
                actor.Inventory.AddAll(it);
        }

        public Actor CreateNewRefugee(int spawnTime, int itemsToCarry)
        {
            Actor newRefugee;

            // civilian, policeman?
            if (m_DiceRoller.RollChance(Params.PolicemanChance))
            {
                newRefugee = CreateNewPoliceman(spawnTime);
                // add random items.
                for (int i = 0; i < itemsToCarry && newRefugee.Inventory.CountItems < newRefugee.Inventory.MaxCapacity; i++)
                    GiveRandomItemToActor(m_DiceRoller, newRefugee, spawnTime);
            }
            else
            {
                newRefugee = CreateNewCivilian(spawnTime, itemsToCarry, 1);
            }

            // give skills : 1 per day + 1 for starting.
            int nbSkills = 1 + new WorldTime(spawnTime).Day;
            base.GiveRandomSkillsToActor(m_DiceRoller, newRefugee, nbSkills);

            // done.
            return newRefugee;
        }

        public Actor CreateNewSurvivor(int spawnTime)
        {
            // decide model.
            bool isMale = m_Rules.Roll(0, 2) == 0;
            ActorModel model = isMale ? m_Game.GameActors.MaleCivilian : m_Game.GameActors.FemaleCivilian;

            // create.
            Actor survivor = model.CreateNumberedName(m_Game.GameFactions.TheSurvivors, spawnTime);

            // setup.
            base.GiveNameToActor(m_DiceRoller, survivor);
            base.DressCivilian(m_DiceRoller, survivor);
            survivor.Doll.AddDecoration(DollPart.HEAD, isMale ? GameImages.SURVIVOR_MALE_BANDANA : GameImages.SURVIVOR_FEMALE_BANDANA);

            // give items, good survival gear (7 items).
            #region
            // 1,2   1 can of food, 1 amr.
            survivor.Inventory.AddAll(MakeItemCannedFood());
            survivor.Inventory.AddAll(MakeItemArmyRation());
            // 3,4. 1 fire weapon with 1 ammo box or grenade.
            if (m_DiceRoller.RollChance(50))
            {
                survivor.Inventory.AddAll(MakeItemArmyRifle());
                if (m_DiceRoller.RollChance(50))
                    survivor.Inventory.AddAll(MakeItemHeavyRifleAmmo());
                else
                    survivor.Inventory.AddAll(MakeItemGrenade());
            }
            else
            {
                survivor.Inventory.AddAll(MakeItemShotgun());
                if (m_DiceRoller.RollChance(50))
                    survivor.Inventory.AddAll(MakeItemShotgunAmmo());
                else
                    survivor.Inventory.AddAll(MakeItemGrenade());
            }
            // 5    1 healing item.
            survivor.Inventory.AddAll(MakeItemMedikit());

            // 6    1 pill item.
            switch (m_DiceRoller.Roll(0, 3))
            {
                case 0: survivor.Inventory.AddAll(MakeItemPillsSLP()); break;
                case 1: survivor.Inventory.AddAll(MakeItemPillsSTA()); break;
                case 2: survivor.Inventory.AddAll(MakeItemPillsSAN()); break;
            }
            // 7    1 armor.
            survivor.Inventory.AddAll(MakeItemArmyBodyArmor());
            #endregion

            // give skills : 1 per day + 5 as bonus.
            int nbSkills = 3 + new WorldTime(spawnTime).Day;
            base.GiveRandomSkillsToActor(m_DiceRoller, survivor, nbSkills);

            // AI.
            //survivor.Controller = new CivilianAI(); // alpha10.1 defined by model like other actors

            // slightly randomize Food and Sleep - 0..25%.
            int foodDeviation = (int)(0.25f * survivor.FoodPoints);
            survivor.FoodPoints = survivor.FoodPoints - m_Rules.Roll(0, foodDeviation);
            int sleepDeviation = (int)(0.25f * survivor.SleepPoints);
            survivor.SleepPoints = survivor.SleepPoints - m_Rules.Roll(0, sleepDeviation);

            // done.
            return survivor;
        }

        public Actor CreateNewNakedHuman(int spawnTime, int itemsToCarry, int skills)
        {
            // decide model.
            ActorModel model = m_Rules.Roll(0, 2) == 0 ? m_Game.GameActors.MaleCivilian : m_Game.GameActors.FemaleCivilian;

            // create.
            Actor civilian = model.CreateNumberedName(m_Game.GameFactions.TheCivilians, spawnTime);

            // done.
            return civilian;
        }

        public Actor CreateNewCivilian(int spawnTime, int itemsToCarry, int skills)
        {
            // decide model.
            ActorModel model = m_Rules.Roll(0, 2) == 0 ? m_Game.GameActors.MaleCivilian : m_Game.GameActors.FemaleCivilian;

            // create.
            Actor civilian = model.CreateNumberedName(m_Game.GameFactions.TheCivilians, spawnTime);

            // setup.
            base.DressCivilian(m_DiceRoller, civilian);
            base.GiveNameToActor(m_DiceRoller, civilian);
            for (int i = 0; i < itemsToCarry; i++)
                GiveRandomItemToActor(m_DiceRoller, civilian, spawnTime);
            base.GiveRandomSkillsToActor(m_DiceRoller, civilian, skills);
            //civilian.Controller = new CivilianAI();  // alpha10.1 defined by model like other actors

            // slightly randomize Food and Sleep - 0..25%.
            int foodDeviation = (int)(0.25f * civilian.FoodPoints);
            civilian.FoodPoints = civilian.FoodPoints - m_Rules.Roll(0, foodDeviation);
            int sleepDeviation = (int)(0.25f * civilian.SleepPoints);
            civilian.SleepPoints = civilian.SleepPoints - m_Rules.Roll(0, sleepDeviation);

            // done.
            return civilian;
        }

        public Actor CreateNewPoliceman(int spawnTime)
        {
            // model.
            ActorModel model = m_Game.GameActors.Policeman;

            // create.
            Actor newCop = model.CreateNumberedName(m_Game.GameFactions.ThePolice, spawnTime);

            // setup.
            base.DressPolice(m_DiceRoller, newCop);
            base.GiveNameToActor(m_DiceRoller, newCop);
            newCop.Name = "Cop " + newCop.Name;
            base.GiveRandomSkillsToActor(m_DiceRoller, newCop, 1);
            base.GiveStartingSkillToActor(newCop, Skills.IDs.FIREARMS);
            base.GiveStartingSkillToActor(newCop, Skills.IDs.LEADERSHIP);
            //newCop.Controller = new CivilianAI(); // alpha10.1 defined by model like other actors

            // give items.
            if (m_DiceRoller.RollChance(50))
            {
                // pistol
                newCop.Inventory.AddAll(MakeItemPistol());
                newCop.Inventory.AddAll(MakeItemLightPistolAmmo());
            }
            else
            {
                // shoty
                newCop.Inventory.AddAll(MakeItemShotgun());
                newCop.Inventory.AddAll(MakeItemShotgunAmmo());
            }
            newCop.Inventory.AddAll(MakeItemTruncheon());
            newCop.Inventory.AddAll(MakeItemFlashlight());
            newCop.Inventory.AddAll(MakeItemPoliceRadio());
            if (m_DiceRoller.RollChance(50))
            {
                if (m_DiceRoller.RollChance(80))
                    newCop.Inventory.AddAll(MakeItemPoliceJacket());
                else
                    newCop.Inventory.AddAll(MakeItemPoliceRiotArmor());
            }

            // done.
            return newCop;
        }

        public Actor CreateNewUndead(int spawnTime)
        {
            Actor newUndead;

            if (Rules.HasAllZombies(m_Game.Session.GameMode))
            {
                // decide model.
                ActorModel undeadModel;
                int chance = m_Rules.Roll(0, 100);
                undeadModel = (chance < RogueGame.Options.SpawnSkeletonChance ? m_Game.GameActors.Skeleton :
                    chance < RogueGame.Options.SpawnSkeletonChance + RogueGame.Options.SpawnZombieChance ? m_Game.GameActors.Zombie :
                    chance < RogueGame.Options.SpawnSkeletonChance + RogueGame.Options.SpawnZombieChance + RogueGame.Options.SpawnZombieMasterChance ? m_Game.GameActors.ZombieMaster :
                     m_Game.GameActors.Skeleton);

                // create.
                newUndead = undeadModel.CreateNumberedName(m_Game.GameFactions.TheUndeads, spawnTime);
            }
            else
            {
                // zombified.
                newUndead = MakeZombified(null, CreateNewCivilian(spawnTime, 0, 0), spawnTime);
                // skills?
                WorldTime time = new WorldTime(spawnTime);
                int nbSkills = time.Day / 2;
                if (nbSkills > 0)
                {
                    for (int i = 0; i < nbSkills; i++)
                    {
                        Skills.IDs? zombifiedSkill = m_Game.ZombifySkill((Skills.IDs)m_Rules.Roll(0, (int)Skills.IDs._COUNT));
                        if (zombifiedSkill.HasValue)
                            m_Game.SkillUpgrade(newUndead, zombifiedSkill.Value);
                    }
                    RecomputeActorStartingStats(newUndead);
                }
            }

            // done.
            return newUndead;
        }

        public Actor MakeZombified(Actor zombifier, Actor deadVictim, int turn)
        {
            // create actor.
            string zombiefiedName = String.Format("{0}'s zombie", deadVictim.UnmodifiedName);
            ActorModel zombiefiedModel = deadVictim.Doll.Body.IsMale ? m_Game.GameActors.MaleZombified : m_Game.GameActors.FemaleZombified;
            Faction zombieFaction = (zombifier == null ? m_Game.GameFactions.TheUndeads : zombifier.Faction);
            Actor newZombie = zombiefiedModel.CreateNamed(zombieFaction, zombiefiedName, deadVictim.IsPluralName, turn);

            // dress as victim.
            for (DollPart p = DollPart._FIRST; p < DollPart._COUNT; p++)
            {
                List<string> partDecos = deadVictim.Doll.GetDecorations(p);
                if (partDecos != null)
                {
                    foreach (string deco in partDecos)
                        newZombie.Doll.AddDecoration(p, deco);
                }
            }

            // add blood.
            newZombie.Doll.AddDecoration(DollPart.TORSO, GameImages.BLOODIED);

            return newZombie;
        }

        public Actor CreateNewSewersUndead(int spawnTime)
        {
            if (!Rules.HasAllZombies(m_Game.Session.GameMode))
                return CreateNewUndead(spawnTime);

            // decide model. 
            ActorModel undeadModel = m_DiceRoller.RollChance(80) ? m_Game.GameActors.RatZombie : m_Game.GameActors.Zombie;

            // create.
            Actor newUndead = undeadModel.CreateNumberedName(m_Game.GameFactions.TheUndeads, spawnTime);

            // done.
            return newUndead;
        }

        public Actor CreateNewBasementRatZombie(int spawnTime)
        {
            if (!Rules.HasAllZombies(m_Game.Session.GameMode))
                return CreateNewUndead(spawnTime);

            return m_Game.GameActors.RatZombie.CreateNumberedName(m_Game.GameFactions.TheUndeads, spawnTime);
        }

        public Actor CreateNewSubwayUndead(int spawnTime)
        {
            if (!Rules.HasAllZombies(m_Game.Session.GameMode))
                return CreateNewUndead(spawnTime);

            // standard zombies.
            ActorModel undeadModel = m_Game.GameActors.Zombie;

            // create.
            Actor newUndead = undeadModel.CreateNumberedName(m_Game.GameFactions.TheUndeads, spawnTime);

            // done.
            return newUndead;
        }

        public Actor CreateNewCHARGuard(int spawnTime)
        {
            // model.
            ActorModel model = m_Game.GameActors.CHARGuard;

            // create.
            Actor newGuard = model.CreateNumberedName(m_Game.GameFactions.TheCHARCorporation, spawnTime);

            // setup.
            base.DressCHARGuard(m_DiceRoller, newGuard);
            base.GiveNameToActor(m_DiceRoller, newGuard);
            newGuard.Name = "Gd. " + newGuard.Name;

            // give items.
            newGuard.Inventory.AddAll(MakeItemShotgun());
            newGuard.Inventory.AddAll(MakeItemShotgunAmmo());
            newGuard.Inventory.AddAll(MakeItemCHARLightBodyArmor());

            // done.
            return newGuard;
        }

        public Actor CreateNewArmyNationalGuard(int spawnTime, string rankName)
        {
            // model.
            ActorModel model = m_Game.GameActors.NationalGuard;

            // create.
            Actor newNat = model.CreateNumberedName(m_Game.GameFactions.TheArmy, spawnTime);

            // setup.
            base.DressArmy(m_DiceRoller, newNat);
            base.GiveNameToActor(m_DiceRoller, newNat);
            newNat.Name = rankName + " " + newNat.Name;

            // give items 6/7.
            newNat.Inventory.AddAll(MakeItemArmyRifle());
            newNat.Inventory.AddAll(MakeItemHeavyRifleAmmo());
            newNat.Inventory.AddAll(MakeItemArmyPistol());
            newNat.Inventory.AddAll(MakeItemHeavyPistolAmmo());
            newNat.Inventory.AddAll(MakeItemArmyBodyArmor());
            ItemBarricadeMaterial planks = MakeItemWoodenPlank();
            planks.Quantity = m_Game.GameItems.WOODENPLANK.StackingLimit;
            newNat.Inventory.AddAll(planks);

            // skills : carpentry for building small barricades.
            // alpha10 and firearms
            GiveStartingSkillToActor(newNat, Skills.IDs.CARPENTRY);
            GiveStartingSkillToActor(newNat, Skills.IDs.FIREARMS);

            // give skills : 1 per day after min arrival date.
            int nbSkills = new WorldTime(spawnTime).Day - RogueGame.NATGUARD_DAY;
            if (nbSkills > 0)
                base.GiveRandomSkillsToActor(m_DiceRoller, newNat, nbSkills);

            // done.
            return newNat;
        }

        public Actor CreateNewBikerMan(int spawnTime, GameGangs.IDs gangId)
        {
            // decide model.
            ActorModel model = m_Game.GameActors.BikerMan;

            // create.
            Actor newBiker = model.CreateNumberedName(m_Game.GameFactions.TheBikers, spawnTime);

            // setup.
            newBiker.GangID = (int)gangId;
            base.DressBiker(m_DiceRoller, newBiker);
            base.GiveNameToActor(m_DiceRoller, newBiker);
            newBiker.Controller = new GangAI();

            // give items.
            newBiker.Inventory.AddAll(m_DiceRoller.RollChance(50) ? MakeItemCrowbar() : MakeItemBaseballBat());
            newBiker.Inventory.AddAll(MakeItemBikerGangJacket(gangId));

            // give skills : 1 per day after min arrival date.
            int nbSkills = new WorldTime(spawnTime).Day - RogueGame.BIKERS_RAID_DAY;
            if (nbSkills > 0)
                base.GiveRandomSkillsToActor(m_DiceRoller, newBiker, nbSkills);

            // done.
            return newBiker;
        }

        public Actor CreateNewGangstaMan(int spawnTime, GameGangs.IDs gangId)
        {
            // decide model.
            ActorModel model = m_Game.GameActors.GangstaMan;

            // create.
            Actor newGangsta = model.CreateNumberedName(m_Game.GameFactions.TheGangstas, spawnTime);

            // setup.
            newGangsta.GangID = (int)gangId;
            base.DressGangsta(m_DiceRoller, newGangsta);
            base.GiveNameToActor(m_DiceRoller, newGangsta);
            newGangsta.Controller = new GangAI();

            // give items.
            newGangsta.Inventory.AddAll(m_DiceRoller.RollChance(50) ? MakeItemRandomPistol() : MakeItemBaseballBat());


            // give skills : 1 per day after min arrival date.
            int nbSkills = new WorldTime(spawnTime).Day - RogueGame.GANGSTAS_RAID_DAY;
            if (nbSkills > 0)
                base.GiveRandomSkillsToActor(m_DiceRoller, newGangsta, nbSkills);

            // done.
            return newGangsta;
        }

        public Actor CreateNewBlackOps(int spawnTime, string rankName)
        {
            // model.
            ActorModel model = m_Game.GameActors.BlackOps;

            // create.
            Actor newBO = model.CreateNumberedName(m_Game.GameFactions.TheBlackOps, spawnTime);

            // setup.
            base.DressBlackOps(m_DiceRoller, newBO);
            base.GiveNameToActor(m_DiceRoller, newBO);
            newBO.Name = rankName + " " + newBO.Name;

            // give items.
            newBO.Inventory.AddAll(MakeItemPrecisionRifle());
            newBO.Inventory.AddAll(MakeItemHeavyRifleAmmo());
            newBO.Inventory.AddAll(MakeItemArmyPistol());
            newBO.Inventory.AddAll(MakeItemHeavyPistolAmmo());
            newBO.Inventory.AddAll(MakeItemBlackOpsGPS());

            // done.
            return newBO;
        }

        public Actor CreateNewFeralDog(int spawnTime)
        {
            Actor newDog;

            // model
            newDog = m_Game.GameActors.FeralDog.CreateNumberedName(m_Game.GameFactions.TheFerals, spawnTime);

            // skin
            SkinDog(m_DiceRoller, newDog);

            // done.
            return newDog;
        }
        #endregion

        #region Exits
        /// <summary>
        /// Add the Exit with the decoration.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="fromPosition"></param>
        /// <param name="to"></param>
        /// <param name="toPosition"></param>
        /// <param name="exitImageID"></param>
        void AddExit(Map from, Point fromPosition, Map to, Point toPosition, string exitImageID, bool isAnAIExit)
        {
            from.SetExitAt(fromPosition, new Exit(to, toPosition) { IsAnAIExit = isAnAIExit });
            from.GetTileAt(fromPosition).AddDecoration(exitImageID);
        }
        #endregion

        #region Zones
        protected void MakeWalkwayZones(Map map, Block b)
        {
            /*
             *  NNNE
             *  W  E
             *  W  E
             *  WSSS
             *
             */
            Rectangle r = b.Rectangle;

            // N
            map.AddZone(MakeUniqueZone("walkway", new Rectangle(r.Left, r.Top, r.Width - 1, 1)));
            // S
            map.AddZone(MakeUniqueZone("walkway", new Rectangle(r.Left + 1, r.Bottom - 1, r.Width - 1, 1)));
            // E
            map.AddZone(MakeUniqueZone("walkway", new Rectangle(r.Right - 1, r.Top, 1, r.Height - 1)));
            // W
            map.AddZone(MakeUniqueZone("walkway", new Rectangle(r.Left, r.Top + 1, 1, r.Height - 1)));
        }
        #endregion
    }
}
