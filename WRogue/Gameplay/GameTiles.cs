using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;

namespace djack.RogueSurvivor.Gameplay
{
    class GameTiles : TileModelDB
    {
        #region IDs
        public enum IDs
        {
            _FIRST = 0,

            UNDEF = _FIRST,

            FLOOR_ASPHALT,
            FLOOR_CONCRETE,
            FLOOR_GRASS,
            FLOOR_OFFICE,
            FLOOR_PLANKS,
            FLOOR_SEWER_WATER,
            FLOOR_TILES,
            FLOOR_WALKWAY,

            ROAD_ASPHALT_EW,
            ROAD_ASPHALT_NS,
            RAIL_EW,

            WALL_BRICK,
            WALL_CHAR_OFFICE,
            WALL_HOSPITAL,
            WALL_POLICE_STATION,
            WALL_SEWER,
            WALL_STONE,
            WALL_SUBWAY,

            _COUNT
        }
        #endregion

        #region Colors
        static readonly Color DRK_GRAY1 = Color.DimGray;
        static readonly Color DRK_GRAY2 = Color.DarkGray;
        static readonly Color DRK_RED = Color.FromArgb(128, 0, 0);
        static readonly Color LIT_GRAY1 = Color.Gray;
        static readonly Color LIT_GRAY2 = Color.LightGray;
        static readonly Color LIT_GRAY3 = Color.FromArgb(230, 230, 230);
        static readonly Color LIT_BROWN = Color.BurlyWood;
        #endregion

        #region Fields
        TileModel[] m_Models = new TileModel[(int) IDs._COUNT];
        #endregion

        #region Properties
        public override TileModel this[int id]
        {
            get { return m_Models[id]; }
        }

        public TileModel this[IDs id]
        {
            get { return this[(int)id]; }
            set
            {
                m_Models[(int)id] = value;
                m_Models[(int)id].ID = (int)id;
            }
        }

        public TileModel FLOOR_ASPHALT { get { return this[IDs.FLOOR_ASPHALT]; } }
        public TileModel FLOOR_CONCRETE { get { return this[IDs.FLOOR_CONCRETE]; } }
        public TileModel FLOOR_GRASS { get { return this[IDs.FLOOR_GRASS]; } }
        public TileModel FLOOR_OFFICE { get { return this[IDs.FLOOR_OFFICE]; } }
        public TileModel FLOOR_PLANKS { get { return this[IDs.FLOOR_PLANKS]; } }
        public TileModel FLOOR_SEWER_WATER { get { return this[IDs.FLOOR_SEWER_WATER]; } }
        public TileModel FLOOR_TILES { get { return this[IDs.FLOOR_TILES]; } }
        public TileModel FLOOR_WALKWAY { get { return this[IDs.FLOOR_WALKWAY]; } }
        public TileModel ROAD_ASPHALT_EW { get { return this[IDs.ROAD_ASPHALT_EW]; } }
        public TileModel ROAD_ASPHALT_NS { get { return this[IDs.ROAD_ASPHALT_NS]; } }
        public TileModel RAIL_EW { get { return this[IDs.RAIL_EW]; } }

        public TileModel WALL_BRICK { get { return this[IDs.WALL_BRICK]; } }
        public TileModel WALL_CHAR_OFFICE { get { return this[IDs.WALL_CHAR_OFFICE]; } }
        public TileModel WALL_POLICE_STATION { get { return this[IDs.WALL_POLICE_STATION]; } }
        public TileModel WALL_HOSPITAL { get { return this[IDs.WALL_HOSPITAL]; } }
        public TileModel WALL_SEWER { get { return this[IDs.WALL_SEWER]; } }
        public TileModel WALL_STONE { get { return this[IDs.WALL_STONE]; } }
        public TileModel WALL_SUBWAY { get { return this[IDs.WALL_SUBWAY]; } }
        #endregion

        #region Init
        public GameTiles()
        {
            // bind.
            Models.Tiles = this;

            this[IDs.UNDEF] = TileModel.UNDEF;

            #region Floors
            this[IDs.FLOOR_ASPHALT] = new TileModel(GameImages.TILE_FLOOR_ASPHALT, LIT_GRAY1, true, true);
            this[IDs.FLOOR_CONCRETE] = new TileModel(GameImages.TILE_FLOOR_CONCRETE, LIT_GRAY2, true, true);
            this[IDs.FLOOR_GRASS] = new TileModel(GameImages.TILE_FLOOR_GRASS, Color.Green, true, true);
            this[IDs.FLOOR_OFFICE] = new TileModel(GameImages.TILE_FLOOR_OFFICE, LIT_GRAY3, true, true);
            this[IDs.FLOOR_PLANKS] = new TileModel(GameImages.TILE_FLOOR_PLANKS, LIT_BROWN, true, true);
            this[IDs.FLOOR_SEWER_WATER] = new TileModel(GameImages.TILE_FLOOR_SEWER_WATER, Color.Blue, true, true) { IsWater = true, WaterCoverImageID = GameImages.TILE_FLOOR_SEWER_WATER_COVER };
            this[IDs.FLOOR_TILES] = new TileModel(GameImages.TILE_FLOOR_TILES, LIT_GRAY2, true, true);
            this[IDs.FLOOR_WALKWAY] = new TileModel(GameImages.TILE_FLOOR_WALKWAY, LIT_GRAY2, true, true);
            this[IDs.ROAD_ASPHALT_EW] = new TileModel(GameImages.TILE_ROAD_ASPHALT_EW, LIT_GRAY1, true, true);
            this[IDs.ROAD_ASPHALT_NS] = new TileModel(GameImages.TILE_ROAD_ASPHALT_NS, LIT_GRAY1, true, true);
            this[IDs.RAIL_EW] = new TileModel(GameImages.TILE_RAIL_ES, LIT_GRAY1, true, true);
            #endregion

            #region Walls
            this[IDs.WALL_BRICK] = new TileModel(GameImages.TILE_WALL_BRICK, DRK_GRAY1, false, false);
            this[IDs.WALL_CHAR_OFFICE] = new TileModel(GameImages.TILE_WALL_CHAR_OFFICE, DRK_RED, false, false);
            this[IDs.WALL_HOSPITAL] = new TileModel(GameImages.TILE_WALL_HOSPITAL, Color.White, false, false);
            this[IDs.WALL_POLICE_STATION] = new TileModel(GameImages.TILE_WALL_STONE, Color.CadetBlue, false, false);
            this[IDs.WALL_SEWER] = new TileModel(GameImages.TILE_WALL_SEWER, Color.DarkGreen, false, false);
            this[IDs.WALL_STONE] = new TileModel(GameImages.TILE_WALL_STONE, DRK_GRAY1, false, false);
            this[IDs.WALL_SUBWAY] = new TileModel(GameImages.TILE_WALL_STONE, Color.Blue, false, false);
            #endregion
        }

        #endregion

        #region Helpers
        public bool IsRoadModel(TileModel model)
        {
            return model == this[IDs.ROAD_ASPHALT_EW] || model == this[IDs.ROAD_ASPHALT_NS];
        }
        #endregion
    }
}

