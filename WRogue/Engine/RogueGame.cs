using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.IO;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.Engine.MapObjects;
using djack.RogueSurvivor.Gameplay;
using djack.RogueSurvivor.Gameplay.AI;
using djack.RogueSurvivor.Gameplay.Generators;

using Message = djack.RogueSurvivor.Data.Message;
using djack.RogueSurvivor.Engine.Tasks;
using ItemRating = djack.RogueSurvivor.Gameplay.AI.BaseAI.ItemRating;
using TradeRating = djack.RogueSurvivor.Gameplay.AI.BaseAI.TradeRating;

namespace djack.RogueSurvivor.Engine
{
    class RogueGame
    {
        #region Constants
        public const int MAP_MAX_HEIGHT = 100;
        public const int MAP_MAX_WIDTH = 100;

        public const int TILE_SIZE = 32;
        public const int ACTOR_SIZE = 32;
        public const int ACTOR_OFFSET = (TILE_SIZE - ACTOR_SIZE) / 2;
        public const int TILE_VIEW_WIDTH = 21;
        public const int TILE_VIEW_HEIGHT = 21;
        const int HALF_VIEW_WIDTH = 10;
        const int HALF_VIEW_HEIGHT = 10;

        public const int CANVAS_WIDTH = 1024;
        public const int CANVAS_HEIGHT = 768;

        const int DAMAGE_DX = 10;
        const int DAMAGE_DY = 10;

        #region UI elements
        const int RIGHTPANEL_X = TILE_SIZE * TILE_VIEW_WIDTH + 4;
        const int RIGHTPANEL_Y = 0;
        const int RIGHTPANEL_TEXT_X = RIGHTPANEL_X + 4;
        const int RIGHTPANEL_TEXT_Y = RIGHTPANEL_Y + 4;

        const int INVENTORYPANEL_X = RIGHTPANEL_TEXT_X;
        const int INVENTORYPANEL_Y = RIGHTPANEL_TEXT_Y + 170; // alpha10  156;//142
        const int GROUNDINVENTORYPANEL_Y = INVENTORYPANEL_Y + 64;
        const int CORPSESPANEL_Y = GROUNDINVENTORYPANEL_Y + 64;
        const int INVENTORY_SLOTS_PER_LINE = 10;

        const int SKILLTABLE_Y = CORPSESPANEL_Y + 64;
        const int SKILLTABLE_LINES = 8;  // alpha10 10

        const int LOCATIONPANEL_X = RIGHTPANEL_X;
        const int LOCATIONPANEL_Y = MESSAGES_Y;
        const int LOCATIONPANEL_TEXT_X = LOCATIONPANEL_X + 4;
        const int LOCATIONPANEL_TEXT_Y = LOCATIONPANEL_Y + 4;

        const int MESSAGES_X = 4;
        const int MESSAGES_Y = TILE_VIEW_HEIGHT * TILE_SIZE + 4;
        const int MESSAGES_SPACING = 12;
        const int MESSAGES_FADEOUT = 25;
        const int MAX_MESSAGES = 7;
        const int MESSAGES_HISTORY = 59;

        public const int MINITILE_SIZE = 2;
        const int MINIMAP_X = RIGHTPANEL_X + (CANVAS_WIDTH - RIGHTPANEL_X - MAP_MAX_WIDTH * MINITILE_SIZE) / 2;
        const int MINIMAP_Y = MESSAGES_Y - MINITILE_SIZE * MAP_MAX_HEIGHT - 1;
        const int MINI_TRACKER_OFFSET = 1;

        const int DELAY_SHORT = 250;
        const int DELAY_NORMAL = 500;
        const int DELAY_LONG = 1000;

        readonly Color POPUP_FILLCOLOR = Color.FromArgb(192, Color.CornflowerBlue);

        readonly string[] CLOSE_DOOR_MODE_TEXT = new string[] { "CLOSE MODE - directions to close, ESC cancels" };
        readonly string[] BARRICADE_MODE_TEXT = new string[] { "BARRICADE/REPAIR MODE - directions to barricade/repair, ESC cancels" };
        readonly string[] BREAK_MODE_TEXT = new string[] { "BREAK MODE - directions/wait to break an object, ESC cancels" };
        readonly string[] BUILD_LARGE_FORT_MODE_TEXT = new string[] { "BUILD LARGE FORTIFICATION MODE - directions to build, ESC cancels" };
        readonly string[] BUILD_SMALL_FORT_MODE_TEXT = new string[] { "BUILD SMALL FORTIFICATION MODE - directions to build, ESC cancels" };
        readonly string[] TRADE_MODE_TEXT = new string[] { "TRADE MODE - Y to accept the deal, N to refuse" };
        readonly string[] NEGOCIATE_TRADE_MODE_TEXT = new string[] { "NEGOCIATE TRADE MODE - directions to start negociating with someone, ESC cancels" }; // alpha10
        readonly string[] ASK_NEGOCIATE_TEXT = new string[] { "DO YOU WANT TO NEGOCIATE TRADE WITH {0} - Y to negociate, N to cancel" }; // alpha10.1
        readonly string[] UPGRADE_MODE_TEXT = new string[] { "UPGRADE MODE - follow instructions in the message panel" };
        readonly string[] FIRE_MODE_TEXT = new string[] { "FIRE MODE - F to fire, T next target, M toggle mode, ESC cancels" };
        readonly string[] SWITCH_PLACE_MODE_TEXT = new string[] { "SWITCH PLACE MODE - directions to switch place with a follower, ESC cancels" };
        readonly string[] TAKE_LEAD_MODE_TEXT = new string[] { "TAKE LEAD MODE - directions to recruit a follower, ESC cancels" };
        readonly string[] PULL_MODE_TEXT = new string[] { "PULL MODE - directions to select object, ESC cancels" }; // alpha10
        readonly string[] PUSH_MODE_TEXT = new string[] { "PUSH/SHOVE MODE - directions to push/shove, ESC cancels" };
        readonly string[] TAG_MODE_TEXT = new string[] { "TAG MODE - directions to tag a wall or on the floor, ESC cancels" };
        readonly string[] SPRAY_MODE_TEXT = new string[] { "SPRAY MODE - directions to spray or wait key to spray on yourself, ESC cancels" };
        readonly string PULL_OBJECT_MODE_TEXT = "PULLING {0} - directions to walk to, ESC cancels";  // alpha10
        readonly string PULL_ACTOR_MODE_TEXT = "PULLING {0} - directions to walk to, ESC cancels";  // alpha10
        readonly string PUSH_OBJECT_MODE_TEXT = "PUSHING {0} - directions to push, ESC cancels";
        readonly string SHOVE_ACTOR_MODE_TEXT = "SHOVING {0} - directions to shove, ESC cancels";
        readonly string[] ORDER_MODE_TEXT = new string[] { "ORDER MODE - follow instructions in the message panel, ESC cancels" };
        readonly string[] GIVE_MODE_TEXT = new string[] { "GIVE MODE - directions to give item to someone, ESC cancels" };
        readonly string[] THROW_GRENADE_MODE_TEXT = new string[] { "THROW GRENADE MODE - directions to select, F to fire,  ESC cancels" };
        readonly string[] MARK_ENEMIES_MODE = new string[] { "MARK ENEMIES MODE - E to make enemy, T next actor, ESC cancels" };
        readonly string[] TRADING_DIALOG_MODE_TEXT = new string[] { "TRADING MODE - TAB switch mode, 0..9 select, ESC cancels" }; // alpha10
        readonly Color MODE_TEXTCOLOR = Color.Yellow;
        readonly Color MODE_BORDERCOLOR = Color.Yellow;
        readonly Color MODE_FILLCOLOR = Color.FromArgb(192, Color.Gray);

        // alpha10
        readonly Color TRADE_COLOR_SELECTED_ITEM = Color.LightBlue;
        readonly Color TRADE_COLOR_ACCEPT = Color.LightGreen;
        readonly Color TRADE_COLOR_REFUSE = Color.DarkRed;
        readonly Color TRADE_COLOR_MAYBE_SUCCESS = Color.Green;
        readonly Color TRADE_COLOR_MAYBE_FAILED = Color.Red;

        readonly Color PLAYER_ACTION_COLOR = Color.White;
        readonly Color OTHER_ACTION_COLOR = Color.Gray;
        readonly Color SAYOREMOTE_DANGER_COLOR = Color.Brown; // alpha10
        readonly Color SAYOREMOTE_NORMAL_COLOR = Color.DarkCyan; // alpha10
        readonly Color PLAYER_AUDIO_COLOR = Color.Green;

        const int LINE_SPACING = 12;
        const int BOLD_LINE_SPACING = 14;
        const int CREDIT_CHAR_SPACING = 8;
        const int CREDIT_LINE_SPACING = LINE_SPACING;

        readonly Color NIGHT_COLOR = Color.Cyan;
        readonly Color DAY_COLOR = Color.Gold;

        const int TEXTFILE_CHARS_PER_LINE = 120;
        const int TEXTFILE_LINES_PER_PAGE = 50;
        #endregion

        #region Notable Zone names
        public const string NAME_SUBWAY_STATION = "Subway Station";
        public const string NAME_SEWERS_MAINTENANCE = "Sewers Maintenance";
        public const string NAME_SUBWAY_RAILS = "rails";
        public const string NAME_POLICE_STATION_JAILS_CELL = "jail";
        #endregion

        #region Events
        const int SPAWN_DISTANCE_TO_PLAYER = 10;

        #region Zombie invasion

        #endregion

        #region Sewers invasion
        const int SEWERS_INVASION_CHANCE = 1;
        public const float SEWERS_UNDEADS_FACTOR = 0.50f;  // 1.0 for as much as surface undead spawning.
        #endregion

        #region DISABLED Subway invasion 
#if false 
        const int SUBWAY_INVASION_CHANCE = 1;
        public const float SUBWAY_UNDEADS_FACTOR = 0.25f;  // 1.0 for as much as surface undead spawning.
#endif
        #endregion

        #region Refugees
        /// <summary>
        /// How many refugees in each wave, as ratio of max civilians.
        /// </summary>
        const float REFUGEES_WAVE_SIZE = 0.20f;

        /// <summary>
        /// How many random items each new refugee will carry.
        /// </summary>
        const int REFUGEES_WAVE_ITEMS = 3;


        /// <summary>
        /// Chance to spawn on the surface vs sewers/subway.
        /// </summary>
        const int REFUGEE_SURFACE_SPAWN_CHANCE = 80;
        #endregion

        #region Unique NPC refugees
        const int UNIQUE_REFUGEE_CHECK_CHANCE = 10;
        #endregion

        #region National Guard Squad
        /// <summary>
        /// Date at which natguard can intervene.
        /// </summary>
        public const int NATGUARD_DAY = 3;

        /// <summary>
        /// Date at which natguard will stop coming.
        /// </summary>
        const int NATGUARD_END_DAY = 10;

        /// <summary>
        /// Date at which the natguard leader will bring Z-Trackers.
        /// </summary>
        const int NATGUARD_ZTRACKER_DAY = NATGUARD_DAY + 3;

        /// <summary>
        /// How many soldiers in each national guard squad.
        /// </summary>
        const int NATGUARD_SQUAD_SIZE = 5;

        /// <summary>
        /// By how many times the undeads must outnumber the livings for the nat guard to intervene.
        /// Factored by option.
        /// </summary>
        const float NATGUARD_INTERVENTION_FACTOR = 5;

        /// <summary>
        /// How many chance per turn the nat guard intervene (if other conditions are met).
        /// </summary>
        const int NATGUARD_INTERVENTION_CHANCE = 1;
        #endregion

        #region Army drop supplies
        /// <summary>
        /// Date at which army can drop supplies.
        /// </summary>
        const int ARMY_SUPPLIES_DAY = 4;

        /// <summary>
        /// Ratio total map food items nutrition / livings below which the army drop supplies event can fire.
        /// Factored by option.
        /// </summary>
        const float ARMY_SUPPLIES_FACTOR = 0.20f * Rules.FOOD_BASE_POINTS;

        /// <summary>
        /// Chances per turn the army will drop supply (if other conditions are met).
        /// </summary>
        const int ARMY_SUPPLIES_CHANCE = 2;

        /// <summary>
        /// Radius in which supplies items are dropped.
        /// One item is dropped per suitable tile in radius.
        /// </summary>
        const int ARMY_SUPPLIES_SCATTER = 1;

        #endregion

        #region Bikers raid
        /// <summary>
        /// Date at which bikers will start to raid.
        /// </summary>
        public const int BIKERS_RAID_DAY = 2;

        /// <summary>
        /// Date at which bikers will stop coming.
        /// </summary>
        const int BIKERS_END_DAY = 14;

        /// <summary>
        /// Number of bikers in the raid.
        /// </summary>
        const int BIKERS_RAID_SIZE = 6;

        /// <summary>
        /// Raid chance per turn (if others conditions are met).
        /// </summary>
        const int BIKERS_RAID_CHANCE_PER_TURN = 1;

        /// <summary>
        /// Number of days between each bikers raid.
        /// </summary>
        const int BIKERS_RAID_DAYS_GAP = 2;
        #endregion

        #region Gangstas raid
        /// <summary>
        /// Date at which gangsta will start to raid.
        /// </summary>
        public const int GANGSTAS_RAID_DAY = 7;

        /// <summary>
        /// Date at which gangstas will stop coming.
        /// </summary>
        const int GANGSTAS_END_DAY = 21;

        /// <summary>
        /// Number of gangstas in the raid.
        /// </summary>
        const int GANGSTAS_RAID_SIZE = 6;

        /// <summary>
        /// Raid chance per turn (if others conditions are met).
        /// </summary>
        const int GANGSTAS_RAID_CHANCE_PER_TURN = 1;

        /// <summary>
        /// Number of days between each gangsta raid.
        /// </summary>
        const int GANGSTAS_RAID_DAYS_GAP = 3;
        #endregion

        #region BlackOps raid
        /// <summary>
        /// Date at which blackops will start to raid.
        /// </summary>
        const int BLACKOPS_RAID_DAY = 14;

        /// <summary>
        /// Number of blackops in the raid.
        /// </summary>
        const int BLACKOPS_RAID_SIZE = 3;

        /// <summary>
        /// Raid chances per turn (if others conditions are met).
        /// </summary>
        const int BLACKOPS_RAID_CHANCE_PER_TURN = 1;

        /// <summary>
        /// Delay between each raid.
        /// </summary>
        const int BLACKOPS_RAID_DAY_GAP = 5;
        #endregion

        #region Band of Survivors 
        const int SURVIVORS_BAND_DAY = 21;
        const int SURVIVORS_BAND_SIZE = 5;
        const int SURVIVORS_BAND_CHANCE_PER_TURN = 1;
        const int SURVIVORS_BAND_DAY_GAP = 5;
        #endregion

        #endregion

        #region Undeads evolution
        const int ZOMBIE_LORD_EVOLUTION_MIN_DAY = 7;
        const int DISCIPLE_EVOLUTION_MIN_DAY = 7;
        #endregion

        #region Map color tints for day phases
        readonly Color TINT_DAY = Color.White;
        readonly Color TINT_SUNSET = Color.FromArgb(235, 235, 235);
        readonly Color TINT_EVENING = Color.FromArgb(215, 215, 215);
        readonly Color TINT_MIDNIGHT = Color.FromArgb(195, 195, 195);
        readonly Color TINT_NIGHT = Color.FromArgb(205, 205, 205);
        readonly Color TINT_SUNRISE = Color.FromArgb(225, 225, 225);
        #endregion

        #region Hearing chances - avoid spamming messages.
        const int PLAYER_HEAR_FIGHT_CHANCE = 25;
        const int PLAYER_HEAR_SCREAMS_CHANCE = 10;
        const int PLAYER_HEAR_PUSHPULL_CHANCE = 25;  // alpha10 also for pulls
        const int PLAYER_HEAR_BASH_CHANCE = 25;
        const int PLAYER_HEAR_BREAK_CHANCE = 50;
        const int PLAYER_HEAR_EXPLOSION_CHANCE = 100;
        #endregion

        #region Blood splatting
        const int BLOOD_WALL_SPLAT_CHANCE = 20;
        #endregion

        #region NPC player sleeping snoring message chance
        public const int MESSAGE_NPC_SLEEP_SNORE_CHANCE = 10;
        #endregion

        #region Weather
        // alpha10
        // weather stays from 1h to 3 days and then change
        public const int WEATHER_MIN_DURATION = 1 * WorldTime.TURNS_PER_HOUR;
        public const int WEATHER_MAX_DURATION = 3 * WorldTime.TURNS_PER_DAY;
        #endregion

        // alpha10
        #region Music
        // check bg music every Nth game hours
        const int BGMUSIC_UPDATE_TURNS = 4 * WorldTime.TURNS_PER_HOUR;
        #endregion

        #region World Gen
        const int DISTRICT_EXIT_CHANCE_PER_TILE = 15;
        #endregion

        #region Common verbs
        readonly Verb VERB_ACCEPT_THE_DEAL = new Verb("accept the deal", "accepts the deal");
        readonly Verb VERB_ACTIVATE = new Verb("activate");
        readonly Verb VERB_AVOID = new Verb("avoid");
        readonly Verb VERB_BARRICADE = new Verb("barricade");
        readonly Verb VERB_BASH = new Verb("bash", "bashes");
        readonly Verb VERB_BE = new Verb("are", "is");
        readonly Verb VERB_BUILD = new Verb("build");
        readonly Verb VERB_BREAK = new Verb("break");
        readonly Verb VERB_BUTCHER = new Verb("butcher");
        readonly Verb VERB_CATCH = new Verb("catch", "catches");
        readonly Verb VERB_CHAT_WITH = new Verb("chat with", "chats with");
        readonly Verb VERB_CLOSE = new Verb("close");
        readonly Verb VERB_COLLAPSE = new Verb("collapse");
        readonly Verb VERB_CRUSH = new Verb("crush", "crushes");
        readonly Verb VERB_DESACTIVATE = new Verb("desactivate");
        readonly Verb VERB_DESTROY = new Verb("destroy");
        readonly Verb VERB_DIE = new Verb("die");
        readonly Verb VERB_DIE_FROM_STARVATION = new Verb("die from starvation", "dies from starvation");
        readonly Verb VERB_DISARM = new Verb("disarm");  // alpha10
        readonly Verb VERB_DISCARD = new Verb("discard");
        readonly Verb VERB_DRAG = new Verb("drag");
        readonly Verb VERB_DROP = new Verb("drop");
        readonly Verb VERB_EAT = new Verb("eat");
        readonly Verb VERB_ENJOY = new Verb("enjoy");
        readonly Verb VERB_ENTER = new Verb("enter");
        readonly Verb VERB_ESCAPE = new Verb("escape");
        readonly Verb VERB_FAIL = new Verb("fail");
        readonly Verb VERB_FEAST_ON = new Verb("feast on", "feasts on");
        readonly Verb VERB_FEEL = new Verb("feel");
        readonly Verb VERB_GET = new Verb("get");
        readonly Verb VERB_GIVE = new Verb("give");
        readonly Verb VERB_GRAB = new Verb("grab");
        readonly Verb VERB_EQUIP = new Verb("equip");
        readonly Verb VERB_HAVE = new Verb("have", "has");
        readonly Verb VERB_HELP = new Verb("help");
        readonly Verb VERB_HEAL_WITH = new Verb("heal with", "heals with");
        readonly Verb VERB_JUMP_ON = new Verb("jump on", "jumps on");
        readonly Verb VERB_KILL = new Verb("kill");
        readonly Verb VERB_LEAVE = new Verb("leave");
        readonly Verb VERB_MISS = new Verb("miss", "misses");
        readonly Verb VERB_MURDER = new Verb("murder");
        readonly Verb VERB_OFFER = new Verb("offer");
        readonly Verb VERB_OPEN = new Verb("open");
        readonly Verb VERB_ORDER = new Verb("order");
        readonly Verb VERB_PERSUADE = new Verb("persuade");
        readonly Verb VERB_PULL = new Verb("pull", "pulls");  // alpha10
        readonly Verb VERB_PUSH = new Verb("push", "pushes");
        readonly Verb VERB_RAISE_ALARM = new Verb("raise the alarm", "raises the alarm");
        readonly Verb VERB_REFUSE_THE_DEAL = new Verb("refuse the deal", "refuses the deal");
        readonly Verb VERB_RELOAD = new Verb("reload");
        readonly Verb VERB_RECHARGE = new Verb("recharge");
        readonly Verb VERB_REPAIR = new Verb("repair");
        readonly Verb VERB_REVIVE = new Verb("revive");
        readonly Verb VERB_SEE = new Verb("see");
        readonly Verb VERB_SHOUT = new Verb("shout");
        readonly Verb VERB_SHOVE = new Verb("shove");
        readonly Verb VERB_SNORE = new Verb("snore");
        readonly Verb VERB_SPRAY = new Verb("spray");
        readonly Verb VERB_START = new Verb("start");
        readonly Verb VERB_STOP = new Verb("stop");
        readonly Verb VERB_STUMBLE = new Verb("stumble");
        readonly Verb VERB_SWITCH = new Verb("switch", "switches");
        readonly Verb VERB_SWITCH_PLACE_WITH = new Verb("switch place with", "switches place with");
        readonly Verb VERB_TAKE = new Verb("take");
        readonly Verb VERB_THROW = new Verb("throw");
        readonly Verb VERB_TRADE = new Verb("trade");  // alpha10
        readonly Verb VERB_TRANSFORM_INTO = new Verb("transform into", "transforms into");
        readonly Verb VERB_UNEQUIP = new Verb("unequip");
        readonly Verb VERB_VOMIT = new Verb("vomit");
        readonly Verb VERB_WAIT = new Verb("wait");
        readonly Verb VERB_WAKE_UP = new Verb("wake up", "wakes up");
        #endregion

        #endregion

        #region Types
        #region Overlays
        abstract class Overlay
        {
            public abstract void Draw(IRogueUI ui);
        }

        class OverlayImage : Overlay
        {
            public Point ScreenPosition { get; set; }
            public string ImageID { get; set; }

            public OverlayImage(Point screenPosition, string imageID)
            {
                this.ScreenPosition = screenPosition;
                this.ImageID = imageID;
            }

            public override void Draw(IRogueUI ui)
            {
                ui.UI_DrawImage(ImageID, ScreenPosition.X, ScreenPosition.Y);
            }
        }

        class OverlayTransparentImage : Overlay
        {
            public float Alpha { get; set; }
            public Point ScreenPosition { get; set; }
            public string ImageID { get; set; }

            public OverlayTransparentImage(float alpha, Point screenPosition, string imageID)
            {
                this.Alpha = alpha;
                this.ScreenPosition = screenPosition;
                this.ImageID = imageID;
            }

            public override void Draw(IRogueUI ui)
            {
                ui.UI_DrawTransparentImage(Alpha, ImageID, ScreenPosition.X, ScreenPosition.Y);
            }
        }

        class OverlayText : Overlay
        {
            public Point ScreenPosition { get; set; }
            public string Text { get; set; }
            public Color Color { get; set; }
            public Color? ShadowColor { get; set; }

            public OverlayText(Point screenPosition, Color color, string text)
                : this(screenPosition, color, text, null)
            {
            }

            public OverlayText(Point screenPosition, Color color, string text, Color? shadowColor)
            {
                this.ScreenPosition = screenPosition;
                this.Color = color;
                this.ShadowColor = shadowColor;
                this.Text = text;
            }

            public override void Draw(IRogueUI ui)
            {
                if (ShadowColor.HasValue)
                    ui.UI_DrawString(ShadowColor.Value, Text, ScreenPosition.X + 1, ScreenPosition.Y + 1);
                ui.UI_DrawString(Color, Text, ScreenPosition.X, ScreenPosition.Y);
            }
        }

        class OverlayLine : Overlay
        {
            public Point ScreenFrom { get; set; }
            public Point ScreenTo { get; set; }
            public Color Color { get; set; }

            public OverlayLine(Point screenFrom, Color color, Point screenTo)
            {
                ScreenFrom = screenFrom;
                ScreenTo = screenTo;
                Color = color;
            }

            public override void Draw(IRogueUI ui)
            {
                ui.UI_DrawLine(Color, ScreenFrom.X, ScreenFrom.Y, ScreenTo.X, ScreenTo.Y);
            }
        }

        class OverlayRect : Overlay
        {
            public Rectangle Rectangle { get; set; }
            public Color Color { get; set; }

            public OverlayRect(Color color, Rectangle rect)
            {
                this.Rectangle = rect;
                this.Color = color;
            }

            public override void Draw(IRogueUI ui)
            {
                ui.UI_DrawRect(this.Color, this.Rectangle);
            }
        }

        class OverlayPopup : Overlay
        {
            public Point ScreenPosition { get; set; }
            public Color TextColor { get; set; }
            public Color BoxBorderColor { get; set; }
            public Color BoxFillColor { get; set; }
            public string[] Lines { get; set; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="lines">can be null if want to set text property later</param>
            /// <param name="textColor"></param>
            /// <param name="boxBorderColor"></param>
            /// <param name="boxFillColor"></param>
            /// <param name="screenPos"></param>
            public OverlayPopup(string[] lines, Color textColor, Color boxBorderColor, Color boxFillColor, Point screenPos)
            {
                this.ScreenPosition = screenPos;
                this.TextColor = textColor;
                this.BoxBorderColor = boxBorderColor;
                this.BoxFillColor = boxFillColor;
                this.Lines = lines;
            }

            public override void Draw(IRogueUI ui)
            {
                ui.UI_DrawPopup(Lines, TextColor, BoxBorderColor, BoxFillColor, ScreenPosition.X, ScreenPosition.Y);
            }
        }

        // alpha10

        class OverlayPopupTitle : Overlay
        {
            public Point ScreenPosition { get; set; }
            public string Title { get; set; }
            public Color TitleColor { get; set; }
            public string[] Lines { get; set; }
            public Color TextColor { get; set; }
            public Color BoxBorderColor { get; set; }
            public Color BoxFillColor { get; set; }

            public OverlayPopupTitle(string title, Color titleColor, string[] lines, Color textColor, Color boxBorderColor, Color boxFillColor, Point screenPos)
            {
                this.ScreenPosition = screenPos;
                this.Title = title;
                this.TitleColor = titleColor;
                this.TextColor = textColor;
                this.BoxBorderColor = boxBorderColor;
                this.BoxFillColor = boxFillColor;
                this.Lines = lines;
            }

            public override void Draw(IRogueUI ui)
            {
                ui.UI_DrawPopupTitle(Title, TitleColor, Lines, TextColor, BoxBorderColor, BoxFillColor, ScreenPosition.X, ScreenPosition.Y);
            }
        }

        class OverlayPopupTitleColors : Overlay
        {
            public Point ScreenPosition { get; set; }
            public string Title { get; set; }
            public Color TitleColor { get; set; }
            public string[] Lines { get; set; }
            public Color[] Colors { get; set; }
            public Color BoxBorderColor { get; set; }
            public Color BoxFillColor { get; set; }

            public OverlayPopupTitleColors(string title, Color titleColor, string[] lines, Color[] colors, Color boxBorderColor, Color boxFillColor, Point screenPos)
            {
                this.ScreenPosition = screenPos;
                this.Title = title;
                this.TitleColor = titleColor;
                this.Colors = colors;
                this.BoxBorderColor = boxBorderColor;
                this.BoxFillColor = boxFillColor;
                this.Lines = lines;
            }

            public override void Draw(IRogueUI ui)
            {
                ui.UI_DrawPopupTitleColors(Title, TitleColor, Lines, Colors, BoxBorderColor, BoxFillColor, ScreenPosition.X, ScreenPosition.Y);
            }
        }
        #endregion

        #region Character generation
        struct CharGen
        {
            public bool IsUndead { get; set; }
            public GameActors.IDs UndeadModel { get; set; }
            public bool IsMale { get; set; }
            public Skills.IDs StartingSkill { get; set; }
        }
        #endregion

        #region Simulation
        [Flags]
        enum SimFlags
        {
            NOT_SIMULATING = 0,
            HIDETAIL_TURN = (1 << 0),
            LODETAIL_TURN = (1 << 1)
        }
        #endregion

        #endregion

        #region Fields
        readonly IRogueUI m_UI;
        Rules m_Rules;
        Session m_Session;
        HiScoreTable m_HiScoreTable;
        MessageManager m_MessageManager;
        bool m_IsGameRunning = true;
        bool m_HasLoadedGame = false;
        List<Overlay> m_Overlays = new List<Overlay>();
        Actor m_Player;
        HashSet<Point> m_PlayerFOV = new HashSet<Point>();
        Rectangle m_MapViewRect;

        static GameOptions s_Options;
        static Keybindings s_KeyBindings;
        static GameHintsStatus s_Hints;

        OverlayPopup m_HintAvailableOverlay;  // alpha10

        BaseTownGenerator m_TownGenerator;

        bool m_PlayedIntro;
        IMusicManager m_MusicManager;

        CharGen m_CharGen;

        TextFile m_Manual;
        int m_ManualLine;

        GameFactions m_GameFactions;
        GameActors m_GameActors;
        GameItems m_GameItems;
        GameTiles m_GameTiles;

        bool m_IsPlayerLongWait;
        bool m_IsPlayerLongWaitForcedStop;
        WorldTime m_PlayerLongWaitEnd;

        // alpha10 new sim thread management
        //Object m_SimMutex = new Object();  // alpha10 obsolete
        Thread m_SimThread;
        readonly Object m_SimStateLock = new Object(); // alpha10 lock when reading sim thread state flags
        bool m_SimThreadDoRun;  // alpha10 sim thread state: set by main thread to false to ask sim thread to stop.
        bool m_SimThreadIsWorking;  // alpha10 sim thread state: set by sim thread to false when has exited loop. 
        #endregion

        #region Properties
        public Session Session
        {
            get { return m_Session; }
        }

        public Rules Rules
        {
            get { return m_Rules; }
        }

        public IRogueUI UI
        {
            get { return m_UI; }
        }

        public bool IsGameRunning
        {
            get { return m_IsGameRunning; }
            set { m_IsGameRunning = value; }
        }

        public static GameOptions Options
        {
            get { return s_Options; }
        }

        public static Keybindings KeyBindings
        {
            get { return s_KeyBindings; }
        }

        public GameFactions GameFactions
        {
            get { return m_GameFactions; }
        }

        public GameActors GameActors
        {
            get { return m_GameActors; }
        }

        public GameItems GameItems
        {
            get { return m_GameItems; }
        }

        public GameTiles GameTiles
        {
            get { return m_GameTiles; }
        }

        public Actor Player
        {
            get { return m_Player; }
        }
        #endregion

        // alpha10
        #region Debug
        // Looping ai detection code: 
        // detect cases where an ai is proably performing an infinite sequence of ap free actions.
        Actor m_DEBUG_prevAiActor;
        int m_DEBUG_sameAiActorCount;
        const int DEBUG_AI_ACTOR_LOOP_COUNT_WARNING = 10;
        #endregion

        #region Init
        public RogueGame(IRogueUI UI)
        {
            Logger.WriteLine(Logger.Stage.INIT_MAIN, "RogueGame()");

            m_UI = UI;

            Logger.WriteLine(Logger.Stage.INIT_MAIN, "creating MusicManager");
            switch (SetupConfig.Sound)
            {
                case SetupConfig.eSound.SOUND_MANAGED_DIRECTX:
                    m_MusicManager = new MDXSoundManager();
                    break;
                case SetupConfig.eSound.SOUND_SFML:
                    m_MusicManager = new SFMLSoundManager();
                    break;
                default:
                    m_MusicManager = new NullSoundManager();
                    break;
            }

            Logger.WriteLine(Logger.Stage.INIT_MAIN, "creating MessageManager");
            m_MessageManager = new MessageManager(MESSAGES_SPACING, MESSAGES_FADEOUT, MESSAGES_HISTORY);

            m_Session = Session.Get;
            Logger.WriteLine(Logger.Stage.INIT_MAIN, "creating Rules");
            m_Rules = new Rules(new DiceRoller(m_Session.Seed));

            BaseTownGenerator.Parameters genParams = BaseTownGenerator.DEFAULT_PARAMS;
            genParams.MapWidth = genParams.MapHeight = 100;
            Logger.WriteLine(Logger.Stage.INIT_MAIN, "creating Generator");
            m_TownGenerator = new StdTownGenerator(this, genParams);

            Logger.WriteLine(Logger.Stage.INIT_MAIN, "creating options, keys, hints.");
            s_Options = new GameOptions();
            s_Options.ResetToDefaultValues();
            s_KeyBindings = new Keybindings();
            s_KeyBindings.ResetToDefaults();
            s_Hints = new GameHintsStatus();
            s_Hints.ResetAllHints();

            Logger.WriteLine(Logger.Stage.INIT_MAIN, "creating dbs");
            m_GameFactions = new GameFactions();
            m_GameActors = new GameActors();
            m_GameItems = new GameItems();
            m_GameTiles = new GameTiles();

            Logger.WriteLine(Logger.Stage.INIT_MAIN, "RogueGame() done.");
        }
        #endregion

        #region Messaging & Grammar helpers
        public void AddMessage(Message msg)
        {
            // ignore empty messages
            if (msg.Text.Length == 0)
                return;

            // Clear if too much messages.
            if (m_MessageManager.Count >= MAX_MESSAGES)
                m_MessageManager.Clear();

            // Format message: <turn> <Text>           
            msg.Text = String.Format("{0} {1}", m_Session.WorldTime.TurnCounter, Capitalize(msg.Text));

            // Add.
            m_MessageManager.Add(msg);
        }

        /// <summary>
        /// Adds the message if it is audible by the player and redraws the screen.
        /// </summary>
        public void AddMessageIfAudibleForPlayer(Location location, Message msg)
        {
            if (msg == null)
                throw new ArgumentNullException("msg");

            // 1. Audible to player?
            if (m_Player != null)
            {
                // if sleeping can't hear.
                if (m_Player.IsSleeping)
                    return;

                // can't hear if not same map.
                if (location.Map != m_Player.Location.Map)
                    return;

                // can hear if close enough.
                if (m_Rules.StdDistance(m_Player.Location.Position, location.Position) <= m_Player.AudioRange)
                {
                    // hear.
                    msg.Color = PLAYER_AUDIO_COLOR;
                    AddMessage(msg);

                    // if waiting, interupt.
                    if (m_IsPlayerLongWait)
                        m_IsPlayerLongWaitForcedStop = true;

                    // redraw.
                    RedrawPlayScreen();
                }
            }
        }

        /// <summary>
        /// Make a message with the text: [eventText] DISTANCE tiles to the DIRECTION.
        /// </summary>
        /// <param name="eventText"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        Message MakePlayerCentricMessage(string eventText, Point position)
        {
            Point vDir = new Point(position.X - m_Player.Location.Position.X, position.Y - m_Player.Location.Position.Y);
            string text = String.Format("{0} {1} tiles to the {2}.", eventText, (int)m_Rules.StdDistance(vDir), Direction.ApproximateFromVector(vDir));
            return new Message(text, m_Session.WorldTime.TurnCounter);
        }

        Message MakeErrorMessage(string text)
        {
            return new Message(text, m_Session.WorldTime.TurnCounter, Color.Red);
        }

        Message MakeYesNoMessage(string question)
        {
            return new Message(String.Format("{0}? Y to confirm, N to cancel", question), m_Session.WorldTime.TurnCounter, Color.Yellow);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="actor"></param>
        /// <returns>"someone" if not visible to the player; TheName if visible.</returns>
        string ActorVisibleIdentity(Actor actor)
        {
            return IsVisibleToPlayer(actor) ? actor.TheName : "someone";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapObj"></param>
        /// <returns>"someone" if not visible to the player; TheName if visible.</returns>
        string ObjectVisibleIdentity(MapObject mapObj)
        {
            return IsVisibleToPlayer(mapObj) ? mapObj.TheName : "something";
        }

        Message MakeMessage(Actor actor, string doWhat)
        {
            return MakeMessage(actor, doWhat, OTHER_ACTION_COLOR);
        }

        Message MakeMessage(Actor actor, string doWhat, Color color)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(ActorVisibleIdentity(actor));
            sb.Append(" ");
            sb.Append(doWhat);

            Message msg = new Message(sb.ToString(), m_Session.WorldTime.TurnCounter);
            if (actor.IsPlayer)
                msg.Color = PLAYER_ACTION_COLOR;
            else
                msg.Color = color;

            return msg;
        }

        Message MakeMessage(Actor actor, string doWhat, Actor target)
        {
            return MakeMessage(actor, doWhat, target, ".");
        }

        Message MakeMessage(Actor actor, string doWhat, Actor target, string phraseEnd)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(ActorVisibleIdentity(actor));
            sb.Append(" ");
            sb.Append(doWhat);
            sb.Append(" ");
            sb.Append(ActorVisibleIdentity(target));
            sb.Append(phraseEnd);

            Message msg = new Message(sb.ToString(), m_Session.WorldTime.TurnCounter);
            if (actor.IsPlayer || target.IsPlayer)
                msg.Color = PLAYER_ACTION_COLOR;
            else
                msg.Color = OTHER_ACTION_COLOR;

            return msg;
        }

        Message MakeMessage(Actor actor, string doWhat, MapObject target)
        {
            return MakeMessage(actor, doWhat, target, ".");
        }

        Message MakeMessage(Actor actor, string doWhat, MapObject target, string phraseEnd)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(ActorVisibleIdentity(actor));
            sb.Append(" ");
            sb.Append(doWhat);
            sb.Append(" ");
            sb.Append(ObjectVisibleIdentity(target));
            sb.Append(phraseEnd);

            Message msg = new Message(sb.ToString(), m_Session.WorldTime.TurnCounter);
            if (actor.IsPlayer)
                msg.Color = PLAYER_ACTION_COLOR;
            else
                msg.Color = OTHER_ACTION_COLOR;

            return msg;
        }

        Message MakeMessage(Actor actor, string doWhat, Item target)
        {
            return MakeMessage(actor, doWhat, target, ".");
        }

        Message MakeMessage(Actor actor, string doWhat, Item target, string phraseEnd)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(ActorVisibleIdentity(actor));
            sb.Append(" ");
            sb.Append(doWhat);
            sb.Append(" ");
            sb.Append(target.TheName);
            sb.Append(phraseEnd);

            Message msg = new Message(sb.ToString(), m_Session.WorldTime.TurnCounter);
            if (actor.IsPlayer)
                msg.Color = PLAYER_ACTION_COLOR;
            else
                msg.Color = OTHER_ACTION_COLOR;

            return msg;
        }

        void ClearMessages()
        {
            m_MessageManager.Clear();
        }

        void ClearMessagesHistory()
        {
            m_MessageManager.ClearHistory();
        }

        void RemoveLastMessage()
        {
            m_MessageManager.RemoveLastMessage();
        }

        void DrawMessages()
        {
            m_MessageManager.Draw(m_UI, m_Session.LastTurnPlayerActed, MESSAGES_X, MESSAGES_Y);
        }

        // alpha10.1 caller handle bot : check for IsBotPlayer and dont call this
        void AddMessagePressEnter()
        {
            AddMessage(new Message("<press ENTER>", m_Session.WorldTime.TurnCounter, Color.Yellow));
            RedrawPlayScreen();
            WaitEnter();
            RemoveLastMessage();
            RedrawPlayScreen();
        }

        string Conjugate(Actor actor, string verb)
        {
            return actor.IsProperName && !actor.IsPluralName ? verb + "s" : verb;
        }

        string Conjugate(Actor actor, Verb verb)
        {
            return actor.IsProperName && !actor.IsPluralName ? verb.HeForm : verb.YouForm;
        }

        string Capitalize(string text)
        {
            if (text == null)
                return "";
            if (text.Length == 1)
                return String.Format("{0}", Char.ToUpper(text[0]));

            return String.Format("{0}{1}", Char.ToUpper(text[0]), text.Substring(1));
        }

        string HisOrHer(Actor actor)
        {
            return actor.Model.DollBody.IsMale ? "his" : "her";
        }

        string HeOrShe(Actor actor)
        {
            return actor.Model.DollBody.IsMale ? "he" : "she";
        }

        string HimOrHer(Actor actor)
        {
            return actor.Model.DollBody.IsMale ? "him" : "her";
        }

        // alpha10
        string HimselfOrHerself(Actor actor)
        {
            return actor.Model.DollBody.IsMale ? "himself" : "herself";
        }

        /// <summary>
        /// </summary>
        /// <returns>"a/an name"</returns>
        string AorAn(string name)
        {
            char c = name[0];
            return (c == 'a' || c == 'e' || c == 'i' || c == 'u' ? "an " : "a ") + name;
        }

        string TruncateString(string s, int maxLength)
        {
            return s.Length <= maxLength ? s : s.Substring(0, maxLength);
        }
        #endregion

        #region Anim Delay
        void AnimDelay(int msecs)
        {
            if (s_Options.IsAnimDelayOn)
                m_UI.UI_Wait(msecs);
        }
        #endregion

        #region Game flow

        /// <summary>
        /// Main loop.
        /// </summary>
        public void Run()
        {
            // first run inits.
            InitDirectories();

            // load data.
            LoadData();

            // load options.
            LoadOptions();

            // load hints.
            LoadHints();

            // apply options.
            ApplyOptions(false);

            // load keys.
            LoadKeybindings();

            // load music & sfxs.
            m_UI.UI_Clear(Color.Black);
            m_UI.UI_DrawStringBold(Color.White, "Loading music...", 0, 0);
            m_UI.UI_Repaint();

            m_MusicManager.Load(GameMusics.ARMY, GameMusics.ARMY_FILE);
            m_MusicManager.Load(GameMusics.BIGBEAR_THEME_SONG, GameMusics.BIGBEAR_THEME_SONG_FILE);
            m_MusicManager.Load(GameMusics.BIKER, GameMusics.BIKER_FILE);
            m_MusicManager.Load(GameMusics.CHAR_UNDERGROUND_FACILITY, GameMusics.CHAR_UNDERGROUND_FACILITY_FILE);
            m_MusicManager.Load(GameMusics.DUCKMAN_THEME_SONG, GameMusics.DUCKMAN_THEME_SONG_FILE);
            m_MusicManager.Load(GameMusics.FAMU_FATARU_THEME_SONG, GameMusics.FAMU_FATARU_THEME_SONG_FILE);
            m_MusicManager.Load(GameMusics.FIGHT, GameMusics.FIGHT_FILE);
            m_MusicManager.Load(GameMusics.GANGSTA, GameMusics.GANGSTA_FILE);
            m_MusicManager.Load(GameMusics.HANS_VON_HANZ_THEME_SONG, GameMusics.HANS_VON_HANZ_THEME_SONG_FILE);
            m_MusicManager.Load(GameMusics.HEYTHERE, GameMusics.HEYTHERE_FILE);
            m_MusicManager.Load(GameMusics.HOSPITAL, GameMusics.HOSPITAL_FILE);
            m_MusicManager.Load(GameMusics.INSANE, GameMusics.INSANE_FILE);
            m_MusicManager.Load(GameMusics.INTERLUDE, GameMusics.INTERLUDE_FILE);
            m_MusicManager.Load(GameMusics.INTRO, GameMusics.INTRO_FILE);
            m_MusicManager.Load(GameMusics.LIMBO, GameMusics.LIMBO_FILE);
            m_MusicManager.Load(GameMusics.PLAYER_DEATH, GameMusics.PLAYER_DEATH_FILE);
            m_MusicManager.Load(GameMusics.REINCARNATE, GameMusics.REINCARNATE_FILE);
            m_MusicManager.Load(GameMusics.ROGUEDJACK_THEME_SONG, GameMusics.ROGUEDJACK_THEME_SONG_FILE);
            m_MusicManager.Load(GameMusics.SANTAMAN_THEME_SONG, GameMusics.SANTAMAN_THEME_SONG_FILE);
            m_MusicManager.Load(GameMusics.SEWERS, GameMusics.SEWERS_FILE);
            m_MusicManager.Load(GameMusics.SLEEP, GameMusics.SLEEP_FILE);
            m_MusicManager.Load(GameMusics.SUBWAY, GameMusics.SUBWAY_FILE);
            m_MusicManager.Load(GameMusics.SURVIVORS, GameMusics.SURVIVORS_FILE);
            // alpha10
            m_MusicManager.Load(GameMusics.SURFACE, GameMusics.SURFACE_FILE);

            m_UI.UI_Clear(Color.Black);
            m_UI.UI_DrawStringBold(Color.White, "Loading music... done!", 0, 0);
            m_UI.UI_Repaint();

            m_UI.UI_Clear(Color.Black);
            m_UI.UI_DrawStringBold(Color.White, "Loading sfxs...", 0, 0);
            m_UI.UI_Repaint();

            m_MusicManager.Load(GameSounds.UNDEAD_EAT, GameSounds.UNDEAD_EAT_FILE);
            m_MusicManager.Load(GameSounds.UNDEAD_RISE, GameSounds.UNDEAD_RISE_FILE);
            m_MusicManager.Load(GameSounds.NIGHTMARE, GameSounds.NIGHTMARE_FILE);

            m_UI.UI_Clear(Color.Black);
            m_UI.UI_DrawStringBold(Color.White, "Loading sfxs... done!", 0, 0);
            m_UI.UI_Repaint();


            // load and parse manual.
            LoadManual();

            // load hi score table.
            LoadHiScoreTable();

            // loop.
            while (m_IsGameRunning)
            {
                GameLoop();
            }

            // stop & dispose music.
            m_MusicManager.Stop();
            m_MusicManager.Dispose();

            // quit.
            m_UI.UI_DoQuit();
        }

        void GameLoop()
        {
            // main menu.
            HandleMainMenu();

            // play until player dies or quits.
            while (m_Player != null && !m_Player.IsDead && m_IsGameRunning)
            {
                // timer.
                DateTime timeBefore = DateTime.Now;

                // alpha10
                // roll player charisma for this turn
                m_Session.Player_TurnCharismaRoll = m_Rules.Roll(0, 100);

                // play.
                m_HasLoadedGame = false;
                AdvancePlay(m_Session.CurrentMap.District, SimFlags.NOT_SIMULATING);

                // if quit, don't bother.
                if (!m_IsGameRunning)
                    break;

                // timer.
                DateTime timeAfter = DateTime.Now;
                m_Session.Scoring.RealLifePlayingTime = m_Session.Scoring.RealLifePlayingTime.Add(timeAfter - timeBefore);

                // alpha10
                // check background music every N game hours
                if (m_Session.WorldTime.TurnCounter % BGMUSIC_UPDATE_TURNS == 0)
                    UpdateBgMusic();
            }
        }

        void InitDirectories()
        {
            int gy = 0;
            m_UI.UI_Clear(Color.Black);
            m_UI.UI_DrawStringBold(Color.Yellow, "Checking user game directories...", 0, gy);
            gy += BOLD_LINE_SPACING;
            m_UI.UI_Repaint();

            ///////////////////////
            // Create directories.
            //////////////////////
            bool created = false;
            created |= CheckDirectory(GetUserBasePath(), "base user", ref gy);
            created |= CheckDirectory(GetUserConfigPath(), "config", ref gy);
            created |= CheckDirectory(GetUserDocsPath(), "docs", ref gy);
            created |= CheckDirectory(GetUserGraveyardPath(), "graveyard", ref gy);
            created |= CheckDirectory(GetUserSavesPath(), "saves", ref gy);
            created |= CheckDirectory(GetUserScreenshotsPath(), "screenshots", ref gy);

            //////////////////
            // Copying manual.
            //////////////////
            created |= CheckCopyOfManual();

            if (created)
            {
                m_UI.UI_DrawStringBold(Color.Yellow, "Directories and game manual created.", 0, gy);
                gy += BOLD_LINE_SPACING;
                m_UI.UI_DrawStringBold(Color.Yellow, "Your game data directory is in the game folder:", 0, gy);
                gy += BOLD_LINE_SPACING;
                m_UI.UI_DrawString(Color.LightGreen, GetUserBasePath(), 0, gy);
                gy += BOLD_LINE_SPACING;
                m_UI.UI_DrawStringBold(Color.Yellow, "When you uninstall the game you can delete this directory.", 0, gy);
                gy += BOLD_LINE_SPACING;
                DrawFootnote(Color.White, "press ENTER");
                m_UI.UI_Repaint();
                WaitEnter();
            }
        }

        void HandleMainMenu()
        {
            bool loop = true;
            bool isLoadEnabled = File.Exists(GetUserSave());

            string[] menuEntries = new string[] {
                "New Game",                                     // 0 
                isLoadEnabled ?  "Load Game" : "(Load Game)",   // 1
                "Redefine keys",                                // 2
                "Options",                                      // 3
                "Game Manual",                                  // 4
                "All Hints",                                    // 5
                "Hi Scores",                                    // 6
                "Credits",                                      // 7
                "Quit Game" };                                  // 8
            int selected = 0;
            do
            {
                // music.
                if (!m_PlayedIntro)
                {
                    m_MusicManager.Stop();
                    m_MusicManager.Play(GameMusics.INTRO, MusicPriority.PRIORITY_EVENT);
                    m_PlayedIntro = true;
                }

                // display.
                int gx, gy;
                gx = gy = 0;
                m_UI.UI_Clear(Color.Black);
                DrawHeader();
                gy += BOLD_LINE_SPACING;
                m_UI.UI_DrawStringBold(Color.Yellow, "Main Menu", 0, gy);
                gy += 2 * BOLD_LINE_SPACING;
                DrawMenuOrOptions(selected, Color.White, menuEntries, Color.White, null, gx, ref gy);
                DrawFootnote(Color.White, "cursor to move, ENTER to select");

                // christmas special.
                DateTime dateNow = DateTime.Now;
                if (dateNow.Month == 12 && dateNow.Day >= 24 && dateNow.Day <= 26)
                {
                    const int NB_SANTAS = 10;
                    for (int i = 0; i < NB_SANTAS; i++)
                    {
                        int santax = m_Rules.Roll(0, 1024);
                        int santay = m_Rules.Roll(0, 768);
                        m_UI.UI_DrawImage(GameImages.ACTOR_SANTAMAN, santax, santay);
                        m_UI.UI_DrawStringBold(Color.Snow, "* Merry Christmas *", santax - 60, santay - 10);
                    }
                }

                // repaint.
                m_UI.UI_Repaint();

                // get menu action.
                KeyEventArgs key = m_UI.UI_WaitKey();
                switch (key.KeyCode)
                {
                    case Keys.Up:       // move up
                        if (selected > 0) --selected;
                        else selected = menuEntries.Length - 1;
                        break;
                    case Keys.Down:     // move down
                        selected = (selected + 1) % menuEntries.Length;
                        break;

                    case Keys.Enter:    // validate
                        {
                            switch (selected)
                            {
                                case 0:
                                    if (HandleNewCharacter())
                                    {
                                        StartNewGame();
                                        loop = false;
                                    }
                                    break;

                                case 1:
                                    if (!isLoadEnabled)
                                        break;
                                    gy += 2 * BOLD_LINE_SPACING;
                                    m_UI.UI_DrawStringBold(Color.Yellow, "Loading game, please wait...", gx, gy);
                                    m_UI.UI_Repaint();
                                    LoadGame(GetUserSave());
                                    loop = false;
                                    // alpha10
                                    if (s_Options.IsSimON && s_Options.SimThread)
                                        StartSimThread();
                                    break;

                                case 2:
                                    HandleRedefineKeys();
                                    break;

                                case 3:
                                    HandleOptions(false);
                                    ApplyOptions(false);
                                    break;

                                case 4:
                                    HandleHelpMode();
                                    break;

                                case 5:
                                    HandleHintsScreen();
                                    break;

                                case 6:
                                    HandleHiScores(true);
                                    break;

                                case 7:
                                    HandleCredits();
                                    break;

                                case 8:
                                    m_IsGameRunning = false;
                                    loop = false;
                                    break;

                                default:
                                    break;
                            } // switch selected
                            break;
                        }
                }
            }
            while (loop);
        }

        #region Character creation
        bool HandleNewCharacter()
        {
            DiceRoller roller = new DiceRoller();

            /////////////////
            // Reset session
            /////////////////
            m_Session.Reset();

            ///////////////
            // Game Mode //
            ///////////////
            if (!HandleNewGameMode())
                return false;

            ////////////////////////
            // Choose living/undead 
            ////////////////////////
            bool isUndead;
            if (!HandleNewCharacterRace(roller, out isUndead))
                return false;
            m_CharGen.IsUndead = isUndead;

            /////////////////////////////
            // Choose gender/undead type
            /////////////////////////////
            if (isUndead)
            {
                GameActors.IDs modelID;
                if (!HandleNewCharacterUndeadType(roller, out modelID))
                    return false;
                m_CharGen.UndeadModel = modelID;
            }
            else
            {
                bool isMale;
                if (!HandleNewCharacterGender(roller, out isMale))
                    return false;
                m_CharGen.IsMale = isMale;
            }

            /////////////////////////////
            // Choose skill (living only)
            /////////////////////////////
            if (!isUndead)
            {
                Skills.IDs skID;
                if (!HandleNewCharacterSkill(roller, out skID))
                    return false;
                m_CharGen.StartingSkill = skID;
                // scoring : starting skill.
                m_Session.Scoring.StartingSkill = skID;
            }
            else
            {
                // udead.
            }

            // done
            return true;
        }

        bool HandleNewGameMode()
        {
            string[] menuEntries = new string[]
            {
                Session.DescGameMode(GameMode.GM_STANDARD),
                Session.DescGameMode(GameMode.GM_CORPSES_INFECTION),
                Session.DescGameMode(GameMode.GM_VINTAGE)
            };
            string[] descs = new string[]
            {
                "Rogue Survivor standard game.",
                "Don't get a cold. Keep an eye on your deceased diseased friends.",
                "The classic zombies next door."
            };

            bool loop = true;
            bool choiceDone = false;
            int selected = 0;
            do
            {
                // display.
                m_UI.UI_Clear(Color.Black);
                int gx, gy;
                gx = gy = 0;
                m_UI.UI_DrawStringBold(Color.Yellow, "New Game - Choose Game Mode", gx, gy);
                gy += 2 * BOLD_LINE_SPACING;
                DrawMenuOrOptions(selected, Color.White, menuEntries, Color.LightGray, descs, gx, ref gy);
                gy += 2 * BOLD_LINE_SPACING;

                string[] descMode = { };
                switch (selected)
                {
                    case 0:
                        descMode = new string[] {
                            "This is the standard game setting.",
                            "Recommended for beginners.",
                            "- All the kinds of undeads.",
                            "- Undeads can evolve to stronger forms.",
                            "- Livings can zombify instantly when dead.",
                            "- No infection.",
                            "- No corpses."
                        };
                        break;
                    case 1:
                        descMode = new string[] {
                            "This is the standard game setting plus corpses and infection.",
                            "Recommended to experience all the features of the game.",
                            "- All the kinds of undeads.",
                            "- Undeads can evolve to stronger forms.",
                            "- Infection:",
                            "  - some undeads can infect livings when biting them.",
                            "  - infected livings can become ill and die.",
                            "  - infected corpses have more chances to rise as zombies.",
                            "- Corpses:",
                            "  - livings that die drop corpses that will rot away.",
                            "  - corpses may rise as zombies.",
                            "  - undeads can eat corpses.",
                            "  - livings can eat corpses if desperate."
                        };
                        break;
                    case 2:
                        descMode = new string[] {
                            "This is the classic zombies for hardcore zombie fans.",
                            "Recommended if you want classic movies zombies.",
                            "- Undeads are only zombified men and women.",
                            "- Undeads don't evolve to stronger forms.",
                            "- Infection:",
                            "  - some undeads can infect livings when biting them.",
                            "  - infected livings can become ill and die.",
                            "  - infected corpses have more chances to rise as zombies.",
                            "- Corpses:",
                            "  - livings that die drop corpses that will rot away.",
                            "  - corpses may rise as zombies.",
                            "  - undeads can eat corpses.",
                            "  - livings can eat corpses if desperate.",
                            "",
                            "NOTE:",
                            "This mode force some options OFF.",
                            "Remember to set them back ON again when you play other modes!"
                        };
                        break;
                }
                foreach (String str in descMode)
                {
                    m_UI.UI_DrawStringBold(Color.Gray, str, gx, gy);
                    gy += BOLD_LINE_SPACING;
                }


                DrawFootnote(Color.White, "cursor to move, ENTER to select, ESC to cancel");
                m_UI.UI_Repaint();

                // get menu action.
                KeyEventArgs key = m_UI.UI_WaitKey();
                switch (key.KeyCode)
                {
                    case Keys.Up:       // move up
                        if (selected > 0) --selected;
                        else selected = menuEntries.Length - 1;
                        break;
                    case Keys.Down:     // move down
                        selected = (selected + 1) % menuEntries.Length;
                        break;

                    case Keys.Escape:
                        choiceDone = false;
                        loop = false;
                        break;

                    case Keys.Enter:    // validate
                        {
                            switch (selected)
                            {
                                case 0: // standard
                                    m_Session.GameMode = GameMode.GM_STANDARD;
                                    choiceDone = true;
                                    loop = false;
                                    break;

                                case 1: // corpses & infection
                                    m_Session.GameMode = GameMode.GM_CORPSES_INFECTION;
                                    choiceDone = true;
                                    loop = false;
                                    break;

                                case 2: // vintage
                                    m_Session.GameMode = GameMode.GM_VINTAGE;

                                    // force some options off.
                                    s_Options.AllowUndeadsEvolution = false;
                                    s_Options.ShamblersUpgrade = false;
                                    s_Options.RatsUpgrade = false;
                                    s_Options.SkeletonsUpgrade = false;
                                    ApplyOptions(false);

                                    choiceDone = true;
                                    loop = false;
                                    break;
                            }
                            break;
                        }
                }

            }
            while (loop);

            // done.
            return choiceDone;
        }

        bool HandleNewCharacterRace(DiceRoller roller, out bool isUndead)
        {
            string[] menuEntries = new string[]
            {
                "*Random*",
                "Living",
                "Undead"
            };
            string[] descs = new string[]
            {
                "(picks a race at random for you)",
                "Try to survive.",
                "Eat brains and die again."
            };

            isUndead = false;
            bool loop = true;
            bool choiceDone = false;
            int selected = 0;
            do
            {
                // display.
                m_UI.UI_Clear(Color.Black);
                int gx, gy;
                gx = gy = 0;
                m_UI.UI_DrawStringBold(Color.Yellow, String.Format("[{0}] New Character - Choose Race", Session.DescGameMode(m_Session.GameMode)), gx, gy);
                gy += 2 * BOLD_LINE_SPACING;
                DrawMenuOrOptions(selected, Color.White, menuEntries, Color.LightGray, descs, gx, ref gy);
                gy += 2 * BOLD_LINE_SPACING;

                DrawFootnote(Color.White, "cursor to move, ENTER to select, ESC to cancel");
                m_UI.UI_Repaint();

                // get menu action.
                KeyEventArgs key = m_UI.UI_WaitKey();
                switch (key.KeyCode)
                {
                    case Keys.Up:       // move up
                        if (selected > 0) --selected;
                        else selected = menuEntries.Length - 1;
                        break;
                    case Keys.Down:     // move down
                        selected = (selected + 1) % menuEntries.Length;
                        break;

                    case Keys.Escape:
                        choiceDone = false;
                        loop = false;
                        break;

                    case Keys.Enter:    // validate
                        {
                            switch (selected)
                            {
                                case 0: // random
                                    isUndead = roller.RollChance(50);

                                    gy += BOLD_LINE_SPACING;
                                    m_UI.UI_DrawStringBold(Color.White, String.Format("Race : {0}.", isUndead ? "Undead" : "Living"), gx, gy);
                                    gy += BOLD_LINE_SPACING;
                                    m_UI.UI_DrawStringBold(Color.Yellow, "Is that OK? Y to confirm, N to cancel.", gx, gy);
                                    m_UI.UI_Repaint();
                                    if (WaitYesOrNo())
                                    {
                                        choiceDone = true;
                                        loop = false;
                                    }
                                    break;

                                case 1: // living
                                    isUndead = false;
                                    choiceDone = true;
                                    loop = false;
                                    break;

                                case 2: // undead
                                    isUndead = true;
                                    choiceDone = true;
                                    loop = false;
                                    break;
                            }
                            break;
                        }
                }

            }
            while (loop);

            // done.
            return choiceDone;
        }

        bool HandleNewCharacterGender(DiceRoller roller, out bool isMale)
        {
            ActorModel maleModel = GameActors.MaleCivilian;
            ActorModel femaleModel = GameActors.FemaleCivilian;

            string[] menuEntries = new string[]
            {
                "*Random*",
                "Male",
                "Female"
            };
            string[] descs = new string[]
            {
                "(picks a gender at random for you)",
                String.Format("HP:{0:D2}  Def:{1:D2}  Dmg:{2:D1}", maleModel.StartingSheet.BaseHitPoints, maleModel.StartingSheet.BaseDefence.Value,  maleModel.StartingSheet.UnarmedAttack.DamageValue),
                String.Format("HP:{0:D2}  Def:{1:D2}  Dmg:{2:D1}", femaleModel.StartingSheet.BaseHitPoints, femaleModel.StartingSheet.BaseDefence.Value, femaleModel.StartingSheet.UnarmedAttack.DamageValue),
            };

            isMale = true;
            bool loop = true;
            bool choiceDone = false;
            int selected = 0;
            do
            {
                // display.
                m_UI.UI_Clear(Color.Black);
                int gx, gy;
                gx = gy = 0;
                m_UI.UI_DrawStringBold(Color.Yellow, String.Format("[{0}] New Living - Choose Gender", Session.DescGameMode(m_Session.GameMode)), gx, gy);
                gy += 2 * BOLD_LINE_SPACING;
                DrawMenuOrOptions(selected, Color.White, menuEntries, Color.LightGray, descs, gx, ref gy);
                DrawFootnote(Color.White, "cursor to move, ENTER to select, ESC to cancel");
                m_UI.UI_Repaint();

                // get menu action.
                KeyEventArgs key = m_UI.UI_WaitKey();
                switch (key.KeyCode)
                {
                    case Keys.Up:       // move up
                        if (selected > 0) --selected;
                        else selected = menuEntries.Length - 1;
                        break;
                    case Keys.Down:     // move down
                        selected = (selected + 1) % menuEntries.Length;
                        break;

                    case Keys.Escape:
                        choiceDone = false;
                        loop = false;
                        break;

                    case Keys.Enter:    // validate
                        {
                            switch (selected)
                            {
                                case 0: // random
                                    isMale = roller.RollChance(50);

                                    gy += BOLD_LINE_SPACING;
                                    m_UI.UI_DrawStringBold(Color.White, String.Format("Gender : {0}.", isMale ? "Male" : "Female"), gx, gy);
                                    gy += BOLD_LINE_SPACING;
                                    m_UI.UI_DrawStringBold(Color.Yellow, "Is that OK? Y to confirm, N to cancel.", gx, gy);
                                    m_UI.UI_Repaint();
                                    if (WaitYesOrNo())
                                    {
                                        choiceDone = true;
                                        loop = false;
                                    }
                                    break;

                                case 1: // male
                                    isMale = true;
                                    choiceDone = true;
                                    loop = false;
                                    break;

                                case 2: // female
                                    isMale = false;
                                    choiceDone = true;
                                    loop = false;
                                    break;
                            }
                            break;
                        }
                }

            }
            while (loop);

            // done.
            return choiceDone;
        }

        string DescribeUndeadModelStatLine(ActorModel m)
        {
            return String.Format("HP:{0:D3}  Spd:{1:F2}  Atk:{2:D2}  Def:{3:D2}  Dmg:{4:D2}  FoV:{5:D1}  Sml:{6:F2}",
                m.StartingSheet.BaseHitPoints, m.DollBody.Speed / 100f,
                m.StartingSheet.UnarmedAttack.HitValue, m.StartingSheet.BaseDefence.Value, m.StartingSheet.UnarmedAttack.DamageValue,
                m.StartingSheet.BaseViewRange, m.StartingSheet.BaseSmellRating);
        }

        bool HandleNewCharacterUndeadType(DiceRoller roller, out GameActors.IDs modelID)
        {
            ActorModel skeletonModel = GameActors.Skeleton;
            ActorModel shamblerModel = GameActors.Zombie;
            ActorModel maleModel = GameActors.MaleZombified;
            ActorModel femaleModel = GameActors.FemaleZombified;
            ActorModel masterModel = GameActors.ZombieMaster;

            string[] menuEntries = new string[]
            {
                "*Random*",
                skeletonModel.Name,
                shamblerModel.Name,
                maleModel.Name,
                femaleModel.Name,
                masterModel.Name,
            };
            string[] descs = new string[]
            {
                "(picks a type at random for you)",
                DescribeUndeadModelStatLine(skeletonModel),
                DescribeUndeadModelStatLine(shamblerModel),
                DescribeUndeadModelStatLine(maleModel),
                DescribeUndeadModelStatLine(femaleModel),
                DescribeUndeadModelStatLine(masterModel)
            };

            modelID = GameActors.IDs.UNDEAD_MALE_ZOMBIFIED;
            bool loop = true;
            bool choiceDone = false;
            int selected = 0;
            do
            {
                // display.
                m_UI.UI_Clear(Color.Black);
                int gx, gy;
                gx = gy = 0;
                m_UI.UI_DrawStringBold(Color.Yellow, String.Format("[{0}] New Undead - Choose Type", Session.DescGameMode(m_Session.GameMode)), gx, gy);
                gy += 2 * BOLD_LINE_SPACING;
                DrawMenuOrOptions(selected, Color.White, menuEntries, Color.LightGray, descs, gx, ref gy);
                DrawFootnote(Color.White, "cursor to move, ENTER to select, ESC to cancel");
                m_UI.UI_Repaint();

                // get menu action.
                KeyEventArgs key = m_UI.UI_WaitKey();
                switch (key.KeyCode)
                {
                    case Keys.Up:       // move up
                        if (selected > 0) --selected;
                        else selected = menuEntries.Length - 1;
                        break;
                    case Keys.Down:     // move down
                        selected = (selected + 1) % menuEntries.Length;
                        break;

                    case Keys.Escape:
                        choiceDone = false;
                        loop = false;
                        break;

                    case Keys.Enter:    // validate
                        {
                            switch (selected)
                            {
                                case 0: // random
                                    selected = roller.Roll(0, 5);
                                    switch (selected)
                                    {
                                        case 0: modelID = GameActors.IDs.UNDEAD_SKELETON; break;
                                        case 1: modelID = GameActors.IDs.UNDEAD_ZOMBIE; break;
                                        case 2: modelID = GameActors.IDs.UNDEAD_MALE_ZOMBIFIED; break;
                                        case 3: modelID = GameActors.IDs.UNDEAD_FEMALE_ZOMBIFIED; break;
                                        case 4: modelID = GameActors.IDs.UNDEAD_ZOMBIE_MASTER; break;
                                        default:
                                            throw new ArgumentOutOfRangeException("unhandled select " + selected);
                                    }

                                    gy += BOLD_LINE_SPACING;
                                    m_UI.UI_DrawStringBold(Color.White, String.Format("Type : {0}.", GameActors[modelID].Name), gx, gy);
                                    gy += BOLD_LINE_SPACING;
                                    m_UI.UI_DrawStringBold(Color.Yellow, "Is that OK? Y to confirm, N to cancel.", gx, gy);
                                    m_UI.UI_Repaint();
                                    if (WaitYesOrNo())
                                    {
                                        choiceDone = true;
                                        loop = false;
                                    }
                                    break;

                                case 1: // skeleton
                                    modelID = GameActors.IDs.UNDEAD_SKELETON;
                                    choiceDone = true;
                                    loop = false;
                                    break;

                                case 2: // shambler
                                    modelID = GameActors.IDs.UNDEAD_ZOMBIE;
                                    choiceDone = true;
                                    loop = false;
                                    break;

                                case 3: // male zombified
                                    modelID = GameActors.IDs.UNDEAD_MALE_ZOMBIFIED;
                                    m_CharGen.IsMale = true;
                                    choiceDone = true;
                                    loop = false;
                                    break;

                                case 4: // female zombified
                                    modelID = GameActors.IDs.UNDEAD_FEMALE_ZOMBIFIED;
                                    m_CharGen.IsMale = false;
                                    choiceDone = true;
                                    loop = false;
                                    break;

                                case 5: // zm
                                    modelID = GameActors.IDs.UNDEAD_ZOMBIE_MASTER;
                                    choiceDone = true;
                                    loop = false;
                                    break;
                            }
                            break;
                        }
                }

            }
            while (loop);

            // done.
            return choiceDone;
        }

        bool HandleNewCharacterSkill(DiceRoller roller, out Skills.IDs skID)
        {
            /////////////////////////////
            // Make table of all skills.
            /////////////////////////////
            Skills.IDs[] allSkills = new Skills.IDs[(int)Skills.IDs._LAST_LIVING + 1];
            string[] menuEntries = new string[allSkills.Length + 1];
            string[] skillDesc = new string[allSkills.Length + 1];
            menuEntries[0] = "*Random*";
            skillDesc[0] = "(picks a skill at random for you)";
            for (int i = (int)Skills.IDs._FIRST_LIVING; i < (int)Skills.IDs._LAST_LIVING + 1; i++)
            {
                allSkills[i] = (Skills.IDs)i;
                menuEntries[i + 1] = Skills.Name(allSkills[i]);
                skillDesc[i + 1] = String.Format("{0} max - {1}", Skills.MaxSkillLevel(i), DescribeSkillShort(allSkills[i]));
            }

            //////////////////////////
            // Loop until choice done
            //////////////////////////
            skID = Skills.IDs._FIRST;
            bool loop = true;
            bool choiceDone = false;
            int selected = 0;
            do
            {
                // display.
                m_UI.UI_Clear(Color.Black);
                int gx, gy;
                gx = gy = 0;
                m_UI.UI_DrawStringBold(Color.Yellow, String.Format("[{0}] New {1} Character - Choose Starting Skill",
                    Session.DescGameMode(m_Session.GameMode),
                    m_CharGen.IsMale ? "Male" : "Female"), gx, gy);
                gy += 2 * BOLD_LINE_SPACING;
                DrawMenuOrOptions(selected, Color.White, menuEntries, Color.LightGray, skillDesc, gx, ref gy);
                DrawFootnote(Color.White, "cursor to move, ENTER to select, ESC to cancel");
                m_UI.UI_Repaint();

                // get menu action.
                KeyEventArgs key = m_UI.UI_WaitKey();
                switch (key.KeyCode)
                {
                    case Keys.Up:       // move up
                        if (selected > 0) --selected;
                        else selected = menuEntries.Length - 1;
                        break;
                    case Keys.Down:     // move down
                        selected = (selected + 1) % menuEntries.Length;
                        break;

                    case Keys.Escape:
                        choiceDone = false;
                        loop = false;
                        break;

                    case Keys.Enter:    // validate
                        if (selected == 0) // random
                            skID = Skills.RollLiving(roller);
                        else
                            skID = (Skills.IDs)(selected - 1 + (int)Skills.IDs._FIRST);

                        gy += BOLD_LINE_SPACING;
                        m_UI.UI_DrawStringBold(Color.White, String.Format("Skill : {0}.", Skills.Name(skID)), gx, gy);
                        gy += BOLD_LINE_SPACING;
                        m_UI.UI_DrawStringBold(Color.Yellow, "Is that OK? Y to confirm, N to cancel.", gx, gy);
                        m_UI.UI_Repaint();
                        if (WaitYesOrNo())
                        {
                            choiceDone = true;
                            loop = false;
                        }
                        break;
                }
            }
            while (loop);

            // done.
            return choiceDone;
        }
        #endregion

        #region Manual
        void LoadManual()
        {
            m_UI.UI_Clear(Color.Black);
            int gy = 0;
            m_UI.UI_DrawStringBold(Color.White, "Loading game manual...", 0, 0);
            gy += BOLD_LINE_SPACING;
            m_UI.UI_Repaint();

            m_Manual = new TextFile();
            m_ManualLine = 0;
            if (!m_Manual.Load(GetUserManualFilePath()))
            {
                // error.
                m_UI.UI_DrawStringBold(Color.Red, "Error while loading the manual.", 0, gy);
                gy += BOLD_LINE_SPACING;
                m_UI.UI_DrawStringBold(Color.Red, "The manual won't be available ingame.", 0, gy);
                gy += BOLD_LINE_SPACING;
                m_UI.UI_Repaint();
                DrawFootnote(Color.White, "press ENTER");
                WaitEnter();

                // delete manual.
                m_Manual = null;
                return;
            }

            m_UI.UI_DrawStringBold(Color.White, "Parsing game manual...", 0, gy);
            gy += BOLD_LINE_SPACING;
            m_UI.UI_Repaint();
            m_Manual.FormatLines(TEXTFILE_CHARS_PER_LINE);

            m_UI.UI_Clear(Color.Black);
            m_UI.UI_DrawStringBold(Color.White, "Game manual... done!", 0, gy);
            m_UI.UI_Repaint();
        }
        #endregion

        #region HiScore
        void HandleHiScores(bool saveToTextfile)
        {
            TextFile file = null;
            if (saveToTextfile)
                file = new TextFile();

            m_UI.UI_Clear(Color.Black);
            int gy = 0;
            DrawHeader();
            gy += BOLD_LINE_SPACING;
            m_UI.UI_DrawStringBold(Color.Yellow, "Hi Scores", 0, gy);
            gy += BOLD_LINE_SPACING;
            m_UI.UI_DrawStringBold(Color.White, "---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+", 0, gy);
            gy += BOLD_LINE_SPACING;

            // display.
            m_UI.UI_DrawStringBold(Color.White, "Rank | Name, Skills, Death       |  Score |Difficulty|Survival|  Kills |Achievm.|      Game Time | Playing time", 0, gy);
            gy += BOLD_LINE_SPACING;

            // text.
            if (saveToTextfile)
            {
                file.Append(String.Format("ROGUE SURVIVOR {0}", SetupConfig.GAME_VERSION));
                file.Append("Hi Scores");
                file.Append("Rank | Name, Skills, Death       |  Score |Difficulty|Survival|  Kills |Achievm.|      Game Time | Playing time");
            }

            // individual entries.
            for (int i = 0; i < m_HiScoreTable.Count; i++)
            {
                // display.
                Color rankColor = (i == 0 ? Color.LightYellow : i == 1 ? Color.LightCyan : i == 2 ? Color.LightGreen : Color.DimGray);
                m_UI.UI_DrawStringBold(rankColor, "------------------------------------------------------------------------------------------------------------------------", 0, gy);
                gy += BOLD_LINE_SPACING;
                HiScore hi = m_HiScoreTable[i];
                string line = String.Format("{0,3}. | {1,-25} | {2,6} |     {3,3}% | {4,6} | {5,6} | {6,6} | {7,14} | {8}",
                    i + 1, TruncateString(hi.Name, 25),
                    hi.TotalPoints, hi.DifficultyPercent, hi.SurvivalPoints, hi.KillPoints, hi.AchievementPoints,
                    new WorldTime(hi.TurnSurvived).ToString(), TimeSpanToString(hi.PlayingTime));
                m_UI.UI_DrawStringBold(rankColor, line, 0, gy);
                gy += BOLD_LINE_SPACING;
                m_UI.UI_DrawStringBold(rankColor, String.Format("     | {0}.", hi.SkillsDescription), 0, gy);
                gy += BOLD_LINE_SPACING;
                m_UI.UI_DrawStringBold(rankColor, String.Format("     | {0}.", hi.Death), 0, gy);
                gy += BOLD_LINE_SPACING;

                // text.
                if (saveToTextfile)
                {
                    file.Append("------------------------------------------------------------------------------------------------------------------------");
                    file.Append(line);
                    file.Append(String.Format("     | {0}", hi.SkillsDescription));
                    file.Append(String.Format("     | {0}", hi.Death));
                }
            }

            // save.
            string textfilePath = GetUserHiScoreTextFilePath();
            if (saveToTextfile)
                file.Save(textfilePath);

            // display.
            m_UI.UI_DrawStringBold(Color.White, "---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+", 0, gy);
            gy += BOLD_LINE_SPACING;
            if (saveToTextfile)
            {
                m_UI.UI_DrawStringBold(Color.White, textfilePath, 0, gy);
                gy += BOLD_LINE_SPACING;
            }
            DrawFootnote(Color.White, "press ESC to leave");
            m_UI.UI_Repaint();
            WaitEscape();
        }

        void LoadHiScoreTable()
        {
            m_UI.UI_Clear(Color.Black);
            m_UI.UI_DrawStringBold(Color.White, "Loading hiscores table...", 0, 0);
            m_UI.UI_Repaint();

            m_HiScoreTable = HiScoreTable.Load(GetUserHiScoreFilePath());
            if (m_HiScoreTable == null)
            {
                m_HiScoreTable = new HiScoreTable(HiScoreTable.DEFAULT_MAX_ENTRIES);
                m_HiScoreTable.Clear();
            }

            m_UI.UI_Clear(Color.Black);
            m_UI.UI_DrawStringBold(Color.White, "Loading hiscores table... done!", 0, 0);
            m_UI.UI_Repaint();
        }

        void SaveHiScoreTable()
        {
            m_UI.UI_Clear(Color.Black);
            m_UI.UI_DrawStringBold(Color.White, "Saving hiscores table...", 0, 0);
            m_UI.UI_Repaint();

            HiScoreTable.Save(m_HiScoreTable, GetUserHiScoreFilePath());

            m_UI.UI_Clear(Color.Black);
            m_UI.UI_DrawStringBold(Color.White, "Saving hiscores table... done!", 0, 0);
            m_UI.UI_Repaint();
        }
        #endregion

        void StartNewGame()
        {
            bool isUndead = m_CharGen.IsUndead;

            // generate world.
            GenerateWorld(true, s_Options.CitySize);

            // scoring : hello there.
            m_Session.Scoring.AddVisit(m_Session.WorldTime.TurnCounter, m_Player.Location.Map);
            m_Session.Scoring.AddEvent(m_Session.WorldTime.TurnCounter, String.Format(isUndead ? "Rose in {0}." : "Woke up in {0}.", m_Player.Location.Map.Name));

            // setup proper scoring mode.
            m_Session.Scoring.Side = (isUndead ? DifficultySide.FOR_UNDEAD : DifficultySide.FOR_SURVIVOR);

            // alpha10.1
            // schedule first autosave.
            ScheduleNextAutoSave();

            // advisor on?
            // alpha10 not if undead
            if (s_Options.IsAdvisorEnabled)
            {
                ClearMessages();
                ClearMessagesHistory();
                if (m_Player.Model.Abilities.IsUndead)
                {
                    AddMessage(new Message("The Advisor is enabled but you will get no hint when playing undead.", 0, Color.Red));
                }
                else
                {
                    AddMessage(new Message("The Advisor is enabled and will give you hints during the game.", 0, Color.LightGreen));
                    AddMessage(new Message("The hints help a beginner learning the basic controls.", 0, Color.LightGreen));
                    AddMessage(new Message("You can disable the Advisor by going to the Options screen.", 0, Color.LightGreen));
                }
                AddMessage(new Message(String.Format("Press {0} during the game to change the options.", s_KeyBindings.Get(PlayerCommand.OPTIONS_MODE)), 0, Color.LightGreen));
                AddMessage(new Message("<press ENTER>", 0, Color.Yellow));
                RedrawPlayScreen();
                WaitEnter();
            }

            // welcome banner.
            ClearMessages();
            ClearMessagesHistory();
            AddMessage(new Message("*****************************", 0, Color.LightGreen));
            AddMessage(new Message("* Welcome to Rogue Survivor *", 0, Color.LightGreen));
            AddMessage(new Message("* We hope you like Zombies  *", 0, Color.LightGreen));
            AddMessage(new Message("*****************************", 0, Color.LightGreen));
            AddMessage(new Message(String.Format("Press {0} for help", s_KeyBindings.Get(PlayerCommand.HELP_MODE)), 0, Color.LightGreen));
            AddMessage(new Message(String.Format("Press {0} to redefine keys", s_KeyBindings.Get(PlayerCommand.KEYBINDING_MODE)), 0, Color.LightGreen));
            AddMessage(new Message("<press ENTER>", 0, Color.Yellow));
            RefreshPlayer();
            RedrawPlayScreen();
            WaitEnter();

            // wake up!
            ClearMessages();
            AddMessage(new Message(String.Format(isUndead ? "{0} rises..." : "{0} wakes up.", m_Player.Name), 0, Color.White));
            RedrawPlayScreen();

            // alpha10.1 reset/cleanup bot from previous session
#if DEBUG
            if (m_isBotMode)
                BotReleaseControl();
#endif

            // start simulation thread.
            StopSimThread(false);  // alpha10 stop-start
            StartSimThread();
        }

        void HandleCredits()
        {
            const int left = 0;
            const int right = 256;
            int gy = 0;

            // music.
            m_MusicManager.Stop();
            m_MusicManager.Play(GameMusics.SLEEP, MusicPriority.PRIORITY_BGM);

            // draw.
            m_UI.UI_Clear(Color.Black);
            DrawHeader();
            gy += BOLD_LINE_SPACING;
            m_UI.UI_DrawStringBold(Color.Yellow, "Credits", 0, gy);
            gy += 2 * BOLD_LINE_SPACING;
            m_UI.UI_DrawStringBold(Color.White, "Programming, Graphics & Music by Jacques Ruiz (roguedjack) 2018", 0, gy);
            gy += 2 * BOLD_LINE_SPACING;

            m_UI.UI_DrawStringBold(Color.White, "Programming", left, gy); m_UI.UI_DrawString(Color.White, "- C# NET 3.5, Microsoft Visual Studio Community 2017", right, gy);
            gy += BOLD_LINE_SPACING;
            m_UI.UI_DrawStringBold(Color.White, "Graphic softwares", left, gy); m_UI.UI_DrawString(Color.White, "- Inkscape, Paint.NET", right, gy);
            gy += BOLD_LINE_SPACING;
            m_UI.UI_DrawStringBold(Color.White, "Sound & Music softwares", left, gy); m_UI.UI_DrawString(Color.White, "- GuitarPro 7, Audacity", right, gy);
            gy += BOLD_LINE_SPACING;
            m_UI.UI_DrawStringBold(Color.White, "Sound samples", left, gy); m_UI.UI_DrawString(Color.White, @"- http://www.sound-fishing.net  http://www.soundsnap.com/", right, gy);
            //gy += BOLD_LINE_SPACING;
            //m_UI.UI_DrawStringBold(Color.White, "Installer", left, gy); m_UI.UI_DrawString(Color.White, @"- NSIS", right, gy);

            gy += 2 * BOLD_LINE_SPACING;
            m_UI.UI_DrawStringBold(Color.White, "Contact", 0, gy);
            gy += BOLD_LINE_SPACING;
            m_UI.UI_DrawString(Color.White, @"Email      : roguedjack@yahoo.fr", 0, gy);
            gy += BOLD_LINE_SPACING;
            m_UI.UI_DrawString(Color.White, @"Blog       : http://roguesurvivor.blogspot.com/", 0, gy);
            gy += BOLD_LINE_SPACING;
            m_UI.UI_DrawString(Color.White, @"Fans Forum : http://roguesurvivor.proboards.com/", 0, gy);
            gy += BOLD_LINE_SPACING;
            m_UI.UI_DrawStringBold(Color.White, "Thanks to the players for their feedback and eagerness to die!", 0, gy);
            gy += BOLD_LINE_SPACING;

            DrawFootnote(Color.White, "ESC to leave");
            m_UI.UI_Repaint();
            WaitEscape();
        }

        // alpha10 removed mention of mode and mentioned which options are always off in certain modes (vintage)
        void HandleOptions(bool ingame)
        {
            GameOptions prevOptions = s_Options;

            #region
            GameOptions.IDs[] list = new GameOptions.IDs[]
            {
                   // autosave
                   GameOptions.IDs.GAME_AUTOSAVE_PERIOD,  // alpha10.1
                   // display & sounds
                   GameOptions.IDs.UI_MUSIC,
                   GameOptions.IDs.UI_MUSIC_VOLUME,
                   GameOptions.IDs.UI_ANIM_DELAY,
                   GameOptions.IDs.UI_SHOW_MINIMAP,
                   GameOptions.IDs.UI_SHOW_PLAYER_TAG_ON_MINIMAP,
                   // helpers
                   GameOptions.IDs.UI_ADVISOR,
                   GameOptions.IDs.UI_COMBAT_ASSISTANT,
                   GameOptions.IDs.UI_SHOW_PLAYER_TARGETS,
                   GameOptions.IDs.UI_SHOW_TARGETS,
                   // sim
                   GameOptions.IDs.GAME_SIMULATE_DISTRICTS,
                   GameOptions.IDs.GAME_SIM_THREAD,
                   GameOptions.IDs.GAME_SIMULATE_SLEEP,
                   // death
                   GameOptions.IDs.GAME_DEATH_SCREENSHOT,
                   GameOptions.IDs.GAME_PERMADEATH,
                   // maps
                   GameOptions.IDs.GAME_CITY_SIZE,
                   GameOptions.IDs.GAME_DISTRICT_SIZE,
                   GameOptions.IDs.GAME_REVEAL_STARTING_DISTRICT,
                   // living
                   GameOptions.IDs.GAME_MAX_CIVILIANS,
                   // GameOptions.IDs.GAME_MAX_DOGS,
                   GameOptions.IDs.GAME_ZOMBIFICATION_CHANCE,
                   GameOptions.IDs.GAME_AGGRESSIVE_HUNGRY_CIVILIANS,
                   GameOptions.IDs.GAME_NPC_CAN_STARVE_TO_DEATH,
                   GameOptions.IDs.GAME_STARVED_ZOMBIFICATION_CHANCE,
                   // undeads
                   GameOptions.IDs.GAME_MAX_UNDEADS,
                   GameOptions.IDs.GAME_ALLOW_UNDEADS_EVOLUTION,
                   GameOptions.IDs.GAME_DAY_ZERO_UNDEADS_PERCENT,
                   GameOptions.IDs.GAME_ZOMBIE_INVASION_DAILY_INCREASE,
                   GameOptions.IDs.GAME_UNDEADS_UPGRADE_DAYS,
                   GameOptions.IDs.GAME_SHAMBLERS_UPGRADE,
                   GameOptions.IDs.GAME_SKELETONS_UPGRADE,
                   GameOptions.IDs.GAME_RATS_UPGRADE,
                   // events
                   GameOptions.IDs.GAME_NATGUARD_FACTOR,
                   GameOptions.IDs.GAME_SUPPLIESDROP_FACTOR,
                   // reinc
                   GameOptions.IDs.GAME_MAX_REINCARNATIONS,
                   GameOptions.IDs.GAME_REINC_LIVING_RESTRICTED,
                   GameOptions.IDs.GAME_REINCARNATE_AS_RAT,
                   GameOptions.IDs.GAME_REINCARNATE_TO_SEWERS
            };
            #endregion

            string[] menuEntries = new string[list.Length];
            string[] values = new string[list.Length];
            for (int i = 0; i < list.Length; i++)
            {
                menuEntries[i] = GameOptions.Name(list[i]);
                // alpha10 special mode notes
                GameOptions.IDs id = list[i];
                if (id == GameOptions.IDs.GAME_ALLOW_UNDEADS_EVOLUTION ||
                    id == GameOptions.IDs.GAME_RATS_UPGRADE ||
                    id == GameOptions.IDs.GAME_SKELETONS_UPGRADE ||
                    id == GameOptions.IDs.GAME_SHAMBLERS_UPGRADE)
                    menuEntries[i] += " -V";
                else if (id == GameOptions.IDs.GAME_ZOMBIFICATION_CHANCE ||
                    id == GameOptions.IDs.GAME_STARVED_ZOMBIFICATION_CHANCE)
                    menuEntries[i] += " =S";
            }

            bool loop = true;
            int selected = 0;
            char[] newlines = { '\n' };  // alpha10
            char[] spaces = { ' ' }; // alpha10
            do
            {
                for (int i = 0; i < list.Length; i++)
                    values[i] = s_Options.DescribeValue(m_Session.GameMode, list[i]);

                int gx, gy;
                gx = gy = 0;
                m_UI.UI_Clear(Color.Black);
                DrawHeader();
                gy += BOLD_LINE_SPACING;
                m_UI.UI_DrawStringBold(Color.Yellow, "Options", 0, gy);  // alpha10 dont mention current mode
                gy += 2 * BOLD_LINE_SPACING;
                DrawMenuOrOptions(selected, Color.White, menuEntries, Color.LightGreen, values, gx, ref gy, false, 400);

                // alpha10
                // describe current option.
                gy += BOLD_LINE_SPACING;
                m_UI.UI_DrawStringBold(Color.White, menuEntries[selected].TrimStart(spaces), gx, gy);
                gy += BOLD_LINE_SPACING;
                string desc = GameOptions.Describe(list[selected]);
                string[] descLines = desc.Split(newlines);
                foreach (string d in descLines)
                {
                    m_UI.UI_DrawString(Color.White, "  " + d, gx, gy);
                    gy += BOLD_LINE_SPACING;
                }

                // legend.
                gy += BOLD_LINE_SPACING;
                m_UI.UI_DrawStringBold(Color.Red, "* Caution : increasing these values makes the game runs slower and saving/loading longer.", gx, gy);
                gy += BOLD_LINE_SPACING;
                m_UI.UI_DrawStringBold(Color.White, "-V : option always OFF when playing VTG-Vintage", gx, gy);
                gy += BOLD_LINE_SPACING;
                m_UI.UI_DrawStringBold(Color.White, "=S : option used only when playing STD-Standard", gx, gy);
                gy += BOLD_LINE_SPACING;

                // difficulty rating.               
                gy += BOLD_LINE_SPACING;
                int diffForSurvivor = (int)(100 * Scoring.ComputeDifficultyRating(s_Options, DifficultySide.FOR_SURVIVOR, 0));
                int diffforUndead = (int)(100 * Scoring.ComputeDifficultyRating(s_Options, DifficultySide.FOR_UNDEAD, 0));
                m_UI.UI_DrawStringBold(Color.Yellow, String.Format("Difficulty Rating : {0}% as survivor / {1}% as undead.", diffForSurvivor, diffforUndead), gx, gy);
                gy += BOLD_LINE_SPACING;
                m_UI.UI_DrawStringBold(Color.White, "Difficulty used for scoring automatically decrease with each reincarnation.", gx, gy);
                gy += 2 * BOLD_LINE_SPACING;

                // footnote.
                DrawFootnote(Color.White, "cursor to move and change values, R to restore previous values, ESC to save and leave");
                m_UI.UI_Repaint();

                // handle
                // get menu action.
                KeyEventArgs key = m_UI.UI_WaitKey();
                switch (key.KeyCode)
                {
                    case Keys.Up:       // move up
                        if (selected > 0) --selected;
                        else selected = menuEntries.Length - 1;
                        break;
                    case Keys.Down:     // move down
                        selected = (selected + 1) % menuEntries.Length;
                        break;

                    case Keys.R:        // restore previous.
                        s_Options = prevOptions;
                        break;

                    case Keys.Escape:   // validate and leave
                        loop = false;
                        break;

                    case Keys.Left:
                        switch ((GameOptions.IDs)list[selected])
                        {
                            case GameOptions.IDs.GAME_DISTRICT_SIZE: s_Options.DistrictSize -= 5; break;
                            case GameOptions.IDs.UI_MUSIC: s_Options.PlayMusic = !s_Options.PlayMusic; break;
                            case GameOptions.IDs.UI_MUSIC_VOLUME: s_Options.MusicVolume -= 5; break;
                            case GameOptions.IDs.UI_ANIM_DELAY: s_Options.IsAnimDelayOn = !s_Options.IsAnimDelayOn; break;
                            case GameOptions.IDs.UI_SHOW_MINIMAP: s_Options.IsMinimapOn = !s_Options.IsMinimapOn; break;
                            case GameOptions.IDs.UI_SHOW_PLAYER_TAG_ON_MINIMAP: s_Options.ShowPlayerTagsOnMinimap = !s_Options.ShowPlayerTagsOnMinimap; break;
                            case GameOptions.IDs.UI_ADVISOR: s_Options.IsAdvisorEnabled = !s_Options.IsAdvisorEnabled; break;
                            case GameOptions.IDs.UI_COMBAT_ASSISTANT: s_Options.IsCombatAssistantOn = !s_Options.IsCombatAssistantOn; break;
                            case GameOptions.IDs.UI_SHOW_TARGETS: s_Options.ShowTargets = !s_Options.ShowTargets; break;
                            case GameOptions.IDs.UI_SHOW_PLAYER_TARGETS: s_Options.ShowPlayerTargets = !s_Options.ShowPlayerTargets; break;
                            case GameOptions.IDs.GAME_MAX_CIVILIANS: s_Options.MaxCivilians -= 5; break;
                            case GameOptions.IDs.GAME_MAX_DOGS: --s_Options.MaxDogs; break;
                            case GameOptions.IDs.GAME_MAX_UNDEADS: s_Options.MaxUndeads -= 10; break;
                            case GameOptions.IDs.GAME_DAY_ZERO_UNDEADS_PERCENT: s_Options.DayZeroUndeadsPercent -= 5; break;
                            case GameOptions.IDs.GAME_ZOMBIE_INVASION_DAILY_INCREASE: --s_Options.ZombieInvasionDailyIncrease; break;
                            case GameOptions.IDs.GAME_CITY_SIZE: s_Options.CitySize -= 1; break;
                            case GameOptions.IDs.GAME_NPC_CAN_STARVE_TO_DEATH: s_Options.NPCCanStarveToDeath = !s_Options.NPCCanStarveToDeath; break;
                            case GameOptions.IDs.GAME_STARVED_ZOMBIFICATION_CHANCE: s_Options.StarvedZombificationChance -= 5; break;
                            case GameOptions.IDs.GAME_SIMULATE_DISTRICTS:
                                if (s_Options.SimulateDistricts != GameOptions.SimRatio.OFF)
                                    s_Options.SimulateDistricts = (GameOptions.SimRatio)(s_Options.SimulateDistricts - 1);
                                break;
                            case GameOptions.IDs.GAME_SIMULATE_SLEEP: s_Options.SimulateWhenSleeping = !s_Options.SimulateWhenSleeping; break;
                            case GameOptions.IDs.GAME_SIM_THREAD: s_Options.SimThread = !s_Options.SimThread; break;
                            case GameOptions.IDs.GAME_ZOMBIFICATION_CHANCE: s_Options.ZombificationChance -= 5; break;
                            case GameOptions.IDs.GAME_REVEAL_STARTING_DISTRICT: s_Options.RevealStartingDistrict = !s_Options.RevealStartingDistrict; break;
                            case GameOptions.IDs.GAME_ALLOW_UNDEADS_EVOLUTION: s_Options.AllowUndeadsEvolution = !s_Options.AllowUndeadsEvolution; break;
                            case GameOptions.IDs.GAME_UNDEADS_UPGRADE_DAYS:
                                if (s_Options.ZombifiedsUpgradeDays != GameOptions.ZupDays._FIRST)
                                    s_Options.ZombifiedsUpgradeDays = (GameOptions.ZupDays)(s_Options.ZombifiedsUpgradeDays - 1);
                                break;
                            case GameOptions.IDs.GAME_MAX_REINCARNATIONS: --s_Options.MaxReincarnations; break;
                            case GameOptions.IDs.GAME_REINCARNATE_AS_RAT: s_Options.CanReincarnateAsRat = !s_Options.CanReincarnateAsRat; break;
                            case GameOptions.IDs.GAME_REINCARNATE_TO_SEWERS: s_Options.CanReincarnateToSewers = !s_Options.CanReincarnateToSewers; break;
                            case GameOptions.IDs.GAME_REINC_LIVING_RESTRICTED: s_Options.IsLivingReincRestricted = !s_Options.IsLivingReincRestricted; break;
                            case GameOptions.IDs.GAME_PERMADEATH: s_Options.IsPermadeathOn = !s_Options.IsPermadeathOn; break;
                            case GameOptions.IDs.GAME_DEATH_SCREENSHOT: s_Options.IsDeathScreenshotOn = !s_Options.IsDeathScreenshotOn; break;
                            case GameOptions.IDs.GAME_AGGRESSIVE_HUNGRY_CIVILIANS: s_Options.IsAggressiveHungryCiviliansOn = !s_Options.IsAggressiveHungryCiviliansOn; break;
                            case GameOptions.IDs.GAME_NATGUARD_FACTOR: s_Options.NatGuardFactor -= 10; break;
                            case GameOptions.IDs.GAME_SUPPLIESDROP_FACTOR: s_Options.SuppliesDropFactor -= 10; break;
                            case GameOptions.IDs.GAME_RATS_UPGRADE:
                                s_Options.RatsUpgrade = !s_Options.RatsUpgrade;
                                break;
                            case GameOptions.IDs.GAME_SHAMBLERS_UPGRADE:
                                s_Options.ShamblersUpgrade = !s_Options.ShamblersUpgrade;
                                break;
                            case GameOptions.IDs.GAME_SKELETONS_UPGRADE:
                                s_Options.SkeletonsUpgrade = !s_Options.SkeletonsUpgrade;
                                break;
                            case GameOptions.IDs.GAME_AUTOSAVE_PERIOD: // alpha10.1
                                s_Options.AutoSavePeriodInHours -= 12;
                                break;
                        }
                        break;
                    case Keys.Right:
                        switch ((GameOptions.IDs)list[selected])
                        {
                            case GameOptions.IDs.GAME_DISTRICT_SIZE: s_Options.DistrictSize += 5; break;
                            case GameOptions.IDs.UI_MUSIC: s_Options.PlayMusic = !s_Options.PlayMusic; break;
                            case GameOptions.IDs.UI_MUSIC_VOLUME: s_Options.MusicVolume += 5; break;
                            case GameOptions.IDs.UI_ANIM_DELAY: s_Options.IsAnimDelayOn = !s_Options.IsAnimDelayOn; break;
                            case GameOptions.IDs.UI_SHOW_MINIMAP: s_Options.IsMinimapOn = !s_Options.IsMinimapOn; break;
                            case GameOptions.IDs.UI_SHOW_PLAYER_TAG_ON_MINIMAP: s_Options.ShowPlayerTagsOnMinimap = !s_Options.ShowPlayerTagsOnMinimap; break;
                            case GameOptions.IDs.UI_ADVISOR: s_Options.IsAdvisorEnabled = !s_Options.IsAdvisorEnabled; break;
                            case GameOptions.IDs.UI_COMBAT_ASSISTANT: s_Options.IsCombatAssistantOn = !s_Options.IsCombatAssistantOn; break;
                            case GameOptions.IDs.UI_SHOW_TARGETS: s_Options.ShowTargets = !s_Options.ShowTargets; break;
                            case GameOptions.IDs.UI_SHOW_PLAYER_TARGETS: s_Options.ShowPlayerTargets = !s_Options.ShowPlayerTargets; break;
                            case GameOptions.IDs.GAME_MAX_CIVILIANS: s_Options.MaxCivilians += 5; break;
                            case GameOptions.IDs.GAME_MAX_DOGS: ++s_Options.MaxDogs; break;
                            case GameOptions.IDs.GAME_MAX_UNDEADS: s_Options.MaxUndeads += 10; break;
                            case GameOptions.IDs.GAME_DAY_ZERO_UNDEADS_PERCENT: s_Options.DayZeroUndeadsPercent += 5; break;
                            case GameOptions.IDs.GAME_ZOMBIE_INVASION_DAILY_INCREASE: ++s_Options.ZombieInvasionDailyIncrease; break;
                            case GameOptions.IDs.GAME_CITY_SIZE: s_Options.CitySize += 1; break;
                            case GameOptions.IDs.GAME_NPC_CAN_STARVE_TO_DEATH: s_Options.NPCCanStarveToDeath = !s_Options.NPCCanStarveToDeath; break;
                            case GameOptions.IDs.GAME_STARVED_ZOMBIFICATION_CHANCE: s_Options.StarvedZombificationChance += 5; break;
                            case GameOptions.IDs.GAME_SIMULATE_DISTRICTS:
                                if (s_Options.SimulateDistricts != GameOptions.SimRatio.FULL)
                                {
                                    s_Options.SimulateDistricts = (GameOptions.SimRatio)(s_Options.SimulateDistricts + 1);
                                }
                                break;
                            case GameOptions.IDs.GAME_SIMULATE_SLEEP: s_Options.SimulateWhenSleeping = !s_Options.SimulateWhenSleeping; break;
                            case GameOptions.IDs.GAME_SIM_THREAD: s_Options.SimThread = !s_Options.SimThread; break;
                            case GameOptions.IDs.GAME_ZOMBIFICATION_CHANCE: s_Options.ZombificationChance += 5; break;
                            case GameOptions.IDs.GAME_REVEAL_STARTING_DISTRICT: s_Options.RevealStartingDistrict = !s_Options.RevealStartingDistrict; break;
                            case GameOptions.IDs.GAME_ALLOW_UNDEADS_EVOLUTION:
                                s_Options.AllowUndeadsEvolution = !s_Options.AllowUndeadsEvolution;
                                break;
                            case GameOptions.IDs.GAME_UNDEADS_UPGRADE_DAYS:
                                if (s_Options.ZombifiedsUpgradeDays != GameOptions.ZupDays._COUNT - 1)
                                    s_Options.ZombifiedsUpgradeDays = (GameOptions.ZupDays)(s_Options.ZombifiedsUpgradeDays + 1);
                                break;
                            case GameOptions.IDs.GAME_MAX_REINCARNATIONS: ++s_Options.MaxReincarnations; break;
                            case GameOptions.IDs.GAME_REINCARNATE_AS_RAT: s_Options.CanReincarnateAsRat = !s_Options.CanReincarnateAsRat; break;
                            case GameOptions.IDs.GAME_REINCARNATE_TO_SEWERS: s_Options.CanReincarnateToSewers = !s_Options.CanReincarnateToSewers; break;
                            case GameOptions.IDs.GAME_REINC_LIVING_RESTRICTED: s_Options.IsLivingReincRestricted = !s_Options.IsLivingReincRestricted; break;
                            case GameOptions.IDs.GAME_PERMADEATH: s_Options.IsPermadeathOn = !s_Options.IsPermadeathOn; break;
                            case GameOptions.IDs.GAME_DEATH_SCREENSHOT: s_Options.IsDeathScreenshotOn = !s_Options.IsDeathScreenshotOn; break;
                            case GameOptions.IDs.GAME_AGGRESSIVE_HUNGRY_CIVILIANS: s_Options.IsAggressiveHungryCiviliansOn = !s_Options.IsAggressiveHungryCiviliansOn; break;
                            case GameOptions.IDs.GAME_NATGUARD_FACTOR: s_Options.NatGuardFactor += 10; break;
                            case GameOptions.IDs.GAME_SUPPLIESDROP_FACTOR: s_Options.SuppliesDropFactor += 10; break;
                            case GameOptions.IDs.GAME_RATS_UPGRADE: s_Options.RatsUpgrade = !s_Options.RatsUpgrade; break;
                            case GameOptions.IDs.GAME_SHAMBLERS_UPGRADE: s_Options.ShamblersUpgrade = !s_Options.ShamblersUpgrade; break;
                            case GameOptions.IDs.GAME_SKELETONS_UPGRADE: s_Options.SkeletonsUpgrade = !s_Options.SkeletonsUpgrade; break;
                            case GameOptions.IDs.GAME_AUTOSAVE_PERIOD: // alpha10.1
                                s_Options.AutoSavePeriodInHours += 12;
                                break;
                        }
                        break;
                }

                // force some options combinations.
                if (s_Options.SimThread)
                    s_Options.SimulateWhenSleeping = false;
                // apply options.
                ApplyOptions(false);
            }
            while (loop);

            // save.
            SaveOptions();
        }

        void HandleRedefineKeys()
        {
            bool loop = true;
            int selected = 0;
            bool conflict = false;
            do
            {
                // check for conflict.
                conflict = s_KeyBindings.CheckForConflict();

                // draw
                string[] menuEntries = new string[]
                {
                    "Move N",
                    "Move NE",
                    "Move E",
                    "Move SE",
                    "Move S",
                    "Move SW",
                    "Move W",
                    "Move NW",
                    "Wait",
                    "Wait 1 hour",
                    "Abandon Game",
                    "Advisor Hint",
                    "Barricade",
                    "Break",
                    "Build Large Fortification",
                    "Build Small Fortification",
                    "City Info",
                    "Close",
                    "Fire",
                    "Give",
                    "Help",
                    "Hints screen",
                    "Negociate Trade",
                    "Item 1 slot",
                    "Item 2 slot",
                    "Item 3 slot",
                    "Item 4 slot",
                    "Item 5 slot",
                    "Item 6 slot",
                    "Item 7 slot",
                    "Item 8 slot",
                    "Item 9 slot",
                    "Item 10 slot",
                    "Lead",
                    "Load Game",
                    "Mark Enemies",
                    "Messages Log",
                    "Options",
                    "Order",
                    "Pull",  // alpha10
                    "Push",
                    "Quit Game",
                    "Redefine Keys",
                    "Run",
                    "Save Game",
                    "Screenshot",
                    "Shout",
                    "Sleep",
                    "Switch Place",
                    "Use Exit",
                    "Use Spray",
                };
                const int O_MOVE_N = 0;
                const int O_MOVE_NE = 1;
                const int O_MOVE_E = 2;
                const int O_MOVE_SE = 3;
                const int O_MOVE_S = 4;
                const int O_MOVE_SW = 5;
                const int O_MOVE_W = 6;
                const int O_MOVE_NW = 7;
                const int O_WAIT = 8;
                const int O_WAIT_LONG = 9;
                const int O_ABANDON = 10;
                const int O_ADVISOR = 11;
                const int O_BARRICADE = 12;
                const int O_BREAK = 13;
                const int O_BUILD_LARGE_F = 14;
                const int O_BUILD_SMALL_F = 15;
                const int O_CITYINFO = 16;
                const int O_CLOSE = 17;
                const int O_FIRE = 18;
                const int O_GIVE = 19;
                const int O_HELP = 20;
                const int O_HINTS_SCREEN = 21;
                const int O_INIT_TRADE = 22;
                const int O_ITEM_1 = 23;
                const int O_ITEM_2 = 24;
                const int O_ITEM_3 = 25;
                const int O_ITEM_4 = 26;
                const int O_ITEM_5 = 27;
                const int O_ITEM_6 = 28;
                const int O_ITEM_7 = 29;
                const int O_ITEM_8 = 30;
                const int O_ITEM_9 = 31;
                const int O_ITEM_10 = 32;
                const int O_LEAD = 33;
                const int O_LOAD = 34;
                const int O_MARKENEMY = 35;
                const int O_LOG = 36;
                const int O_OPTIONS = 37;
                const int O_ORDER = 38;
                const int O_PULL = 39;  // alpha10 inserted and shifted all below
                const int O_PUSH = 40;
                const int O_QUIT = 41;
                const int O_REDEFKEYS = 42;
                const int O_RUN = 43;
                const int O_SAVE = 44;
                const int O_SCREENSHOT = 45;
                const int O_SHOUT = 46;
                const int O_SLEEP = 47;
                const int O_SWITCH = 48;
                const int O_USE_EXIT = 49;
                const int O_USE_SPRAY = 50;
                string[] values = new string[]
                {
                    s_KeyBindings.Get(PlayerCommand.MOVE_N).ToString(),
                    s_KeyBindings.Get(PlayerCommand.MOVE_NE).ToString(),
                    s_KeyBindings.Get(PlayerCommand.MOVE_E).ToString(),
                    s_KeyBindings.Get(PlayerCommand.MOVE_SE).ToString(),
                    s_KeyBindings.Get(PlayerCommand.MOVE_S).ToString(),
                    s_KeyBindings.Get(PlayerCommand.MOVE_SW).ToString(),
                    s_KeyBindings.Get(PlayerCommand.MOVE_W).ToString(),
                    s_KeyBindings.Get(PlayerCommand.MOVE_NW).ToString(),
                    s_KeyBindings.Get(PlayerCommand.WAIT_OR_SELF).ToString(),
                    s_KeyBindings.Get(PlayerCommand.WAIT_LONG).ToString(),
                    s_KeyBindings.Get(PlayerCommand.ABANDON_GAME).ToString(),
                    s_KeyBindings.Get(PlayerCommand.ADVISOR).ToString(),
                    s_KeyBindings.Get(PlayerCommand.BARRICADE_MODE).ToString(),
                    s_KeyBindings.Get(PlayerCommand.BREAK_MODE).ToString(),
                    s_KeyBindings.Get(PlayerCommand.BUILD_LARGE_FORTIFICATION).ToString(),
                    s_KeyBindings.Get(PlayerCommand.BUILD_SMALL_FORTIFICATION).ToString(),
                    s_KeyBindings.Get(PlayerCommand.CITY_INFO).ToString(),
                    s_KeyBindings.Get(PlayerCommand.CLOSE_DOOR).ToString(),
                    s_KeyBindings.Get(PlayerCommand.FIRE_MODE).ToString(),
                    s_KeyBindings.Get(PlayerCommand.GIVE_ITEM).ToString(),
                    s_KeyBindings.Get(PlayerCommand.HELP_MODE).ToString(),
                    s_KeyBindings.Get(PlayerCommand.HINTS_SCREEN_MODE).ToString(),
                    s_KeyBindings.Get(PlayerCommand.NEGOCIATE_TRADE).ToString(),
                    s_KeyBindings.Get(PlayerCommand.ITEM_SLOT_0).ToString(),
                    s_KeyBindings.Get(PlayerCommand.ITEM_SLOT_1).ToString(),
                    s_KeyBindings.Get(PlayerCommand.ITEM_SLOT_2).ToString(),
                    s_KeyBindings.Get(PlayerCommand.ITEM_SLOT_3).ToString(),
                    s_KeyBindings.Get(PlayerCommand.ITEM_SLOT_4).ToString(),
                    s_KeyBindings.Get(PlayerCommand.ITEM_SLOT_5).ToString(),
                    s_KeyBindings.Get(PlayerCommand.ITEM_SLOT_6).ToString(),
                    s_KeyBindings.Get(PlayerCommand.ITEM_SLOT_7).ToString(),
                    s_KeyBindings.Get(PlayerCommand.ITEM_SLOT_8).ToString(),
                    s_KeyBindings.Get(PlayerCommand.ITEM_SLOT_9).ToString(),
                    s_KeyBindings.Get(PlayerCommand.LEAD_MODE).ToString(),
                    s_KeyBindings.Get(PlayerCommand.LOAD_GAME).ToString(),
                    s_KeyBindings.Get(PlayerCommand.MARK_ENEMIES_MODE).ToString(),
                    s_KeyBindings.Get(PlayerCommand.MESSAGE_LOG).ToString(),
                    s_KeyBindings.Get(PlayerCommand.OPTIONS_MODE).ToString(),
                    s_KeyBindings.Get(PlayerCommand.ORDER_MODE).ToString(),
                    s_KeyBindings.Get(PlayerCommand.PULL_MODE).ToString(), // alpha10
                    s_KeyBindings.Get(PlayerCommand.PUSH_MODE).ToString(),
                    s_KeyBindings.Get(PlayerCommand.QUIT_GAME).ToString(),
                    s_KeyBindings.Get(PlayerCommand.KEYBINDING_MODE).ToString(),
                    s_KeyBindings.Get(PlayerCommand.RUN_TOGGLE).ToString(),
                    s_KeyBindings.Get(PlayerCommand.SAVE_GAME).ToString(),
                    s_KeyBindings.Get(PlayerCommand.SCREENSHOT).ToString(),
                    s_KeyBindings.Get(PlayerCommand.SHOUT).ToString(),
                    s_KeyBindings.Get(PlayerCommand.SLEEP).ToString(),
                    s_KeyBindings.Get(PlayerCommand.SWITCH_PLACE).ToString(),
                    s_KeyBindings.Get(PlayerCommand.USE_EXIT).ToString(),
                    s_KeyBindings.Get(PlayerCommand.USE_SPRAY).ToString(),
                };

                int gx, gy;
                gx = gy = 0;
                m_UI.UI_Clear(Color.Black);
                DrawHeader();
                gy += BOLD_LINE_SPACING;
                m_UI.UI_DrawStringBold(Color.Yellow, "Redefine keys", 0, gy);
                gy += BOLD_LINE_SPACING;
                DrawMenuOrOptions(selected, Color.White, menuEntries, Color.LightGreen, values, gx, ref gy);
                if (conflict)
                {
                    m_UI.UI_DrawStringBold(Color.Red, "Conflicting keys. Please redefine the keys so the commands don't overlap.", gx, gy);
                    gy += BOLD_LINE_SPACING;
                }
                DrawFootnote(Color.White, "cursor to move, ENTER to rebind a key, ESC to save and leave");
                m_UI.UI_Repaint();

                // handle
                // get menu action.
                KeyEventArgs key = m_UI.UI_WaitKey();
                switch (key.KeyCode)
                {
                    case Keys.Up:       // move up
                        if (selected > 0) --selected;
                        else selected = menuEntries.Length - 1;
                        break;
                    case Keys.Down:     // move down
                        selected = (selected + 1) % menuEntries.Length;
                        break;

                    case Keys.Escape:   // leave.
                        if (!conflict)
                        {
                            loop = false;
                        }
                        break;

                    case Keys.Enter: // rebind
                        // say.
                        m_UI.UI_DrawStringBold(Color.Yellow, String.Format("rebinding {0}, press the new key.", menuEntries[selected]), gx, gy);
                        m_UI.UI_Repaint();

                        // read new key.
                        bool loopNewKey = true;
                        Keys newKeyData = Keys.None;
                        do
                        {
                            KeyEventArgs newKey = m_UI.UI_WaitKey();
                            // ignore Shift and Control alone.
                            if (newKey.KeyCode == Keys.ShiftKey || newKey.KeyCode == Keys.ControlKey)
                                continue;
                            // always ignore Alt.
                            if (newKey.Alt)
                                continue;
                            // done!
                            newKeyData = newKey.KeyData;
                            loopNewKey = false;
                        }
                        while (loopNewKey);

                        // get command.
                        PlayerCommand command;
                        switch (selected)
                        {
                            case O_MOVE_N: command = PlayerCommand.MOVE_N; break;
                            case O_MOVE_NE: command = PlayerCommand.MOVE_NE; break;
                            case O_MOVE_E: command = PlayerCommand.MOVE_E; break;
                            case O_MOVE_SE: command = PlayerCommand.MOVE_SE; break;
                            case O_MOVE_S: command = PlayerCommand.MOVE_S; break;
                            case O_MOVE_SW: command = PlayerCommand.MOVE_SW; break;
                            case O_MOVE_W: command = PlayerCommand.MOVE_W; break;
                            case O_MOVE_NW: command = PlayerCommand.MOVE_NW; break;
                            case O_WAIT: command = PlayerCommand.WAIT_OR_SELF; break;
                            case O_WAIT_LONG: command = PlayerCommand.WAIT_LONG; break;
                            case O_ABANDON: command = PlayerCommand.ABANDON_GAME; break;
                            case O_ADVISOR: command = PlayerCommand.ADVISOR; break;
                            case O_BARRICADE: command = PlayerCommand.BARRICADE_MODE; break;
                            case O_BREAK: command = PlayerCommand.BREAK_MODE; break;
                            case O_BUILD_LARGE_F: command = PlayerCommand.BUILD_LARGE_FORTIFICATION; break;
                            case O_BUILD_SMALL_F: command = PlayerCommand.BUILD_SMALL_FORTIFICATION; break;
                            case O_CITYINFO: command = PlayerCommand.CITY_INFO; break;
                            case O_CLOSE: command = PlayerCommand.CLOSE_DOOR; break;
                            case O_FIRE: command = PlayerCommand.FIRE_MODE; break;
                            case O_GIVE: command = PlayerCommand.GIVE_ITEM; break;
                            case O_HELP: command = PlayerCommand.HELP_MODE; break;
                            case O_HINTS_SCREEN: command = PlayerCommand.HINTS_SCREEN_MODE; break;
                            case O_INIT_TRADE: command = PlayerCommand.NEGOCIATE_TRADE; break;
                            case O_ITEM_1: command = PlayerCommand.ITEM_SLOT_0; break;
                            case O_ITEM_2: command = PlayerCommand.ITEM_SLOT_1; break;
                            case O_ITEM_3: command = PlayerCommand.ITEM_SLOT_2; break;
                            case O_ITEM_4: command = PlayerCommand.ITEM_SLOT_3; break;
                            case O_ITEM_5: command = PlayerCommand.ITEM_SLOT_4; break;
                            case O_ITEM_6: command = PlayerCommand.ITEM_SLOT_5; break;
                            case O_ITEM_7: command = PlayerCommand.ITEM_SLOT_6; break;
                            case O_ITEM_8: command = PlayerCommand.ITEM_SLOT_7; break;
                            case O_ITEM_9: command = PlayerCommand.ITEM_SLOT_8; break;
                            case O_ITEM_10: command = PlayerCommand.ITEM_SLOT_9; break;
                            case O_LEAD: command = PlayerCommand.LEAD_MODE; break;
                            case O_LOAD: command = PlayerCommand.LOAD_GAME; break;
                            case O_MARKENEMY: command = PlayerCommand.MARK_ENEMIES_MODE; break;
                            case O_LOG: command = PlayerCommand.MESSAGE_LOG; break;
                            case O_OPTIONS: command = PlayerCommand.OPTIONS_MODE; break;
                            case O_ORDER: command = PlayerCommand.ORDER_MODE; break;
                            case O_PULL: command = PlayerCommand.PULL_MODE; break;  // alpha10
                            case O_PUSH: command = PlayerCommand.PUSH_MODE; break;
                            case O_QUIT: command = PlayerCommand.QUIT_GAME; break;
                            case O_REDEFKEYS: command = PlayerCommand.KEYBINDING_MODE; break;
                            case O_RUN: command = PlayerCommand.RUN_TOGGLE; break;
                            case O_SAVE: command = PlayerCommand.SAVE_GAME; break;
                            case O_SCREENSHOT: command = PlayerCommand.SCREENSHOT; break;
                            case O_SHOUT: command = PlayerCommand.SHOUT; break;
                            case O_SLEEP: command = PlayerCommand.SLEEP; break;
                            case O_SWITCH: command = PlayerCommand.SWITCH_PLACE; break;
                            case O_USE_EXIT: command = PlayerCommand.USE_EXIT; break;
                            case O_USE_SPRAY: command = PlayerCommand.USE_SPRAY; break;
                            default:
                                throw new InvalidOperationException("unhandled selected");
                        }

                        // bind it.                      
                        s_KeyBindings.Set(command, newKeyData);

                        break;

                }
            }
            while (loop);

            // Save.
            SaveKeybindings();
        }

        /// <summary>
        /// Advance play in district : could be player district (live district) or simulated district.
        /// </summary>
        /// <param name="district"></param>
        /// <param name="sim"></param>
        void AdvancePlay(District district, SimFlags sim)
        {
            lock (district)  // alpha10 lock district
            {
#if DEBUG_STATS
            Session.UpdateStats(district);
#endif

                // 0. Remember if current district.
                bool wasNight = m_Session.WorldTime.IsNight;
                DayPhase prevPhase = m_Session.WorldTime.Phase;

                // 1. Advance all maps.
                // if player quit/loaded at any time, don't bother!
                #region
                foreach (Map map in district.Maps)
                {
                    int prevLocalTurn = map.LocalTime.TurnCounter;
                    do
                    {
                        // play this map.
                        AdvancePlay(map, sim);
                        // check for reincarnation.
                        if (m_Player.IsDead)
                            HandleReincarnation();
                        // check stopping game.
                        if (!m_IsGameRunning || m_HasLoadedGame || m_Player.IsDead)
                            return;
                    }
                    while (map.LocalTime.TurnCounter == prevLocalTurn);
                }
                #endregion

                // 2. Advance district.
                #region
                // 2.1. Advance world time if current district.
                // alpha10 also check weather change
                #region
                if (district == m_Session.CurrentMap.District)
                {
                    ++m_Session.WorldTime.TurnCounter;

                    // sunrise/sunset.
                    bool canSeeSky = m_Rules.CanActorSeeSky(m_Player);  // alpha10 message ony if can see sky
                    bool isNight = m_Session.WorldTime.IsNight;
                    DayPhase newPhase = m_Session.WorldTime.Phase;
                    if (wasNight && !isNight)
                    {
                        if (canSeeSky) AddMessage(new Message("The sun is rising again for you...", m_Session.WorldTime.TurnCounter, DAY_COLOR));
                        OnNewDay();
                    }
                    else if (!wasNight && isNight)
                    {
                        if (canSeeSky) AddMessage(new Message("Night is falling upon you...", m_Session.WorldTime.TurnCounter, NIGHT_COLOR));
                        OnNewNight();
                    }
                    else if (prevPhase != newPhase)
                    {
                        if (canSeeSky) AddMessage(new Message(String.Format("Time passes, it is now {0}...", DescribeDayPhase(newPhase)), m_Session.WorldTime.TurnCounter, isNight ? NIGHT_COLOR : DAY_COLOR));
                    }


                    // alpha10
                    // if time to change weather do it and roll next change time.
                    if (m_Session.WorldTime.TurnCounter >= m_Session.World.NextWeatherCheckTurn)
                    {
                        ChangeWeather();
                        m_Session.World.NextWeatherCheckTurn = m_Session.WorldTime.TurnCounter + m_Rules.Roll(WEATHER_MIN_DURATION, WEATHER_MAX_DURATION);
                    }
                }
                #endregion

                // 2.2. Check for events.

                #region Entry/Surface map
                // 1 Invasion?
                if (CheckForEvent_ZombieInvasion(district.EntryMap))
                {
                    FireEvent_ZombieInvasion(district.EntryMap);
                }
                // 2 Refugees?
                if (CheckForEvent_RefugeesWave(district.EntryMap))
                {
                    FireEvent_RefugeesWave(district);
                }
                // 3 National guard?
                if (CheckForEvent_NationalGuard(district.EntryMap))
                {
                    FireEvent_NationalGuard(district.EntryMap);
                }
                // 4 Army drop supplies?
                if (CheckForEvent_ArmySupplies(district.EntryMap))
                {
                    FireEvent_ArmySupplies(district.EntryMap);
                }
                // 5 Bikers raid?
                if (CheckForEvent_BikersRaid(district.EntryMap))
                {
                    FireEvent_BikersRaid(district.EntryMap);
                }
                // 6 Gangsta raid?
                if (CheckForEvent_GangstasRaid(district.EntryMap))
                {
                    FireEvent_GangstasRaid(district.EntryMap);
                }
                // 7 Blackops raid?
                if (CheckForEvent_BlackOpsRaid(district.EntryMap))
                {
                    FireEvent_BlackOpsRaid(district.EntryMap);
                }
                // 8 Band of Survivors?
                if (CheckForEvent_BandOfSurvivors(district.EntryMap))
                {
                    FireEvent_BandOfSurvivors(district.EntryMap);
                }
                #endregion

                #region Sewers
                // 1 Sewers Invasion?
                if (CheckForEvent_SewersInvasion(district.SewersMap))
                {
                    FireEvent_SewersInvasion(district.SewersMap);
                }
                #endregion

                #region DISABLED Subway
#if false
            if (district.SubwayMap != null)
            {
                // 1 Subway Invasion?
                if (CheckForEvent_SubwayInvasion(district.SubwayMap))
                {
                    FireEvent_SubwayInvasion(district.SubwayMap);
                }
            }
#endif
                #endregion
                #endregion

                // 3. Simulate nearby districts?
                #region
                // if player is sleeping in this map and option enabled.
                if (s_Options.IsSimON && m_Player != null && m_Player.IsSleeping && s_Options.SimulateWhenSleeping && m_Player.Location.Map.District == district)
                {
                    SimulateNearbyDistricts(district);
                }
                #endregion
            }  // end lock district
        }

        void NotifyOrderablesAI(Map map, RaidType raid, Point position)
        {
            foreach (Actor a in map.Actors)
            {
                OrderableAI oAI = a.Controller as OrderableAI;
                if (oAI == null)
                    continue;
                oAI.OnRaid(raid, new Location(map, position), map.LocalTime.TurnCounter);
            }
        }

        void AdvancePlay(Map map, SimFlags sim)
        {
            //////////////////////////////////////////////////////////
            // 0. Secret maps.
            // 1. Get next actor to Act.
            // 2. If none move to next turn and return.
            // 3. Ask actor to act. Handle player and AI differently.
            //////////////////////////////////////////////////////////

            // 0. Secret maps.
            #region
            if (map.IsSecret)
            {
                // don't play the map at all, jump in time.
                ++map.LocalTime.TurnCounter;
                return;
            }
            #endregion

            // 1. Get next actor to Act.
            #region
            Actor actor = m_Rules.GetNextActorToAct(map, map.LocalTime.TurnCounter);

            // alpha10 ai loop bug detection 
            if (actor != null && !actor.IsPlayer)
            {
                if (actor == m_DEBUG_prevAiActor)
                {
                    if (++m_DEBUG_sameAiActorCount >= DEBUG_AI_ACTOR_LOOP_COUNT_WARNING)
                    {
                        // TO DEVS: you might want to add a debug breakpoint here ->
                        Logger.WriteLine(Logger.Stage.RUN_MAIN, "WARNING: AI actor " + actor.Name + " is probably looping!!");
#if DEBUG
                        // in debug keep going to let us debug the ai
#else
                        // alpha10.1 in release, instead of "crashing" the game by throwing an exception force the AI to do
                        // spend a turn and emote. its better than crashing the game!
                        DoWait(actor);
                        DoEmote(actor, "My AI is looping, I'll  wait instead of crashing your game :)", true);
#endif
                    }
                }
                else
                {
                    m_DEBUG_sameAiActorCount = 0;
                    m_DEBUG_prevAiActor = actor;
                }
            }
            #endregion

            // 2. If none move to next turn and return.
            #region
            if (actor == null)
            {
                NextMapTurn(map, sim);
                return;
            }
            #endregion

            // 3. Ask actor to act. Handle player and AI differently.
            #region
            actor.PreviousStaminaPoints = actor.StaminaPoints;
            if (actor.Controller == null)
            {
                SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
            }
            else if (actor.IsPlayer)
            {
                HandlePlayerActor(actor);
                // if quit, dead or loaded, don't bother.
                if (!m_IsGameRunning || m_HasLoadedGame || m_Player.IsDead)
                    return;
                // Check special player events
                CheckSpecialPlayerEventsAfterAction(actor);
            }
            else
            {
                HandleAiActor(actor);
            }
            actor.PreviousHitPoints = actor.HitPoints;
            actor.PreviousFoodPoints = actor.FoodPoints;
            actor.PreviousSleepPoints = actor.SleepPoints;
            actor.PreviousSanity = actor.Sanity;
            #endregion
        }

        void SpendActorActionPoints(Actor actor, int actionCost)
        {
            actor.ActionPoints -= actionCost;
            actor.LastActionTurn = actor.Location.Map.LocalTime.TurnCounter;
        }

        void SpendActorStaminaPoints(Actor actor, int staminaCost)
        {
            if (actor.Model.Abilities.CanTire)
            {
                // night penalty?
                if (actor.Location.Map.LocalTime.IsNight && staminaCost > 0)
                    staminaCost += m_Rules.NightStaminaPenalty(actor);

                // exhausted?
                if (m_Rules.IsActorExhausted(actor))
                    staminaCost *= 2;

                // apply.
                actor.StaminaPoints -= staminaCost;
            }
            else
                actor.StaminaPoints = Rules.STAMINA_INFINITE;
        }

        void RegenActorStaminaPoints(Actor actor, int staminaRegen)
        {
            if (actor.Model.Abilities.CanTire)
                actor.StaminaPoints = Math.Min(m_Rules.ActorMaxSTA(actor), actor.StaminaPoints + staminaRegen);
            else
                actor.StaminaPoints = Rules.STAMINA_INFINITE;
        }

        void RegenActorHitPoints(Actor actor, int hpRegen)
        {
            actor.HitPoints = Math.Min(m_Rules.ActorMaxHPs(actor), actor.HitPoints + hpRegen);
        }

        void RegenActorSleep(Actor actor, int sleepRegen)
        {
            actor.SleepPoints = Math.Min(m_Rules.ActorMaxSleep(actor), actor.SleepPoints + sleepRegen);
        }

        void SpendActorSanity(Actor actor, int sanCost)
        {
            actor.Sanity -= sanCost;
            if (actor.Sanity < 0) actor.Sanity = 0;
        }

        void RegenActorSanity(Actor actor, int sanRegen)
        {
            actor.Sanity = Math.Min(m_Rules.ActorMaxSanity(actor), actor.Sanity + sanRegen);
        }

        void NextMapTurn(Map map, SimFlags sim)
        {
            bool isLoDetail = (sim & SimFlags.LODETAIL_TURN) != 0;

            ////////////////////////////////////////
            // (the following are skipped in lodetail turns)
            // 0. Raise the deads; Check infections (non STD)
            // 1. Update odors.
            //      alpha10 obsolete 1.1 Odor suppression/generation.
            //      1.2 Odors decay.
            //      1.3 Actors scents.  // alpha10
            // 2. Regen actors AP & STA
            // 3. Stop tired actors from running.
            // 4. Actor gauges & states :
            //    Hunger, Sleep, Sanity, Leader Trust.
            //      4.1 May kill starved actors.
            //      4.2 Handle sleeping actors.
            //      4.3 Exhausted actors might collapse.
            // 5. Check batteries : lights, trackers.
            // 6. Check explosives.
            // 7. Check fires.
            // (the following are always performed)
            // - Check timers.
            // - Advance local time.
            // - Check for NPC upgrade.
            ////////////////////////////////////////

            // TEST CORPSES   
#if false
            if (map.LocalTime.TurnCounter < 2 && map == m_Player.Location.Map)
            {
                for (int i = 0; i < 10; i++)
                {
                    Actor deadGuy = m_TownGenerator.CreateNewCivilian(0, 0, 0);
                    Point p = new Point(1, 1);
                    if (map.GetActorAt(p.Add(m_Player.Location.Position)) == null)
                    {
                        map.PlaceActorAt(deadGuy, p.Add(m_Player.Location.Position));
                        KillActor(null, deadGuy, "TEST CORPSES");
                    }
                }
            }
            
            foreach (Actor a in map.Actors)
            {
                if (a.IsPlayer) continue;
                if (a.Model.Abilities.IsUndead) continue;
                KillActor(null, a, "TEST CORPSES");
                break;
            }
#endif
            if (!isLoDetail)
            {
                // 0. Raise the deads; Check infections (non STD)
                #region
                bool hasCorpses = Rules.HasCorpses(m_Session.GameMode);
                bool hasInfection = Rules.HasInfection(m_Session.GameMode);
                if (hasCorpses || hasInfection)
                {
                    #region Corpses
                    if (hasCorpses && map.CountCorpses > 0)
                    {
                        // decide who zombify or rots.
                        List<Corpse> tryZombifyCorpses = new List<Corpse>(map.CountCorpses);
                        List<Corpse> rottenCorpses = new List<Corpse>(map.CountCorpses);
                        foreach (Corpse c in map.Corpses)
                        {
                            // zombify?
                            int chanceZombify = m_Rules.CorpseZombifyChance(c, map.LocalTime);
                            if (m_Rules.RollChance(chanceZombify))
                            {
                                // zombify this one.
                                tryZombifyCorpses.Add(c);
                                continue;
                            }
                            // or rot away?
                            InflictDamageToCorpse(c, Rules.CorpseDecayPerTurn(c));
                            if (c.HitPoints <= 0)
                            {
                                rottenCorpses.Add(c);
                                continue;
                            }
                        }
                        // zombify!
                        if (tryZombifyCorpses.Count > 0)
                        {
                            List<Corpse> zombifiedCorpses = new List<Corpse>(tryZombifyCorpses.Count);
                            foreach (Corpse c in tryZombifyCorpses)
                            {
                                // only one actor per tile!
                                if (map.GetActorAt(c.Position) == null)
                                {
                                    float corpseState = (float)c.HitPoints / (float)c.MaxHitPoints;
                                    int zombifiedHP = (int)(corpseState * m_Rules.ActorMaxHPs(c.DeadGuy));
                                    zombifiedCorpses.Add(c);
                                    Actor zombified = Zombify(null, c.DeadGuy, false);

                                    if (IsVisibleToPlayer(map, c.Position))
                                    {
                                        AddMessage(new Message(String.Format("The corpse of {0} rise again!!", c.DeadGuy.Name), map.LocalTime.TurnCounter, Color.Red));
                                        // FIXME -- 
                                        // alpha10 this will be a sfx not music
                                        m_MusicManager.Play(GameSounds.UNDEAD_RISE, MusicPriority.PRIORITY_EVENT);
                                    }
                                }
                            }
                            foreach (Corpse c in zombifiedCorpses)
                                DestroyCorpse(c, map);
                        }
                        // rot! (message only)
                        if (m_Player != null && m_Player.Location.Map == map)
                        {
                            foreach (Corpse c in rottenCorpses)
                            {
                                DestroyCorpse(c, map);
                                if (IsVisibleToPlayer(map, c.Position))
                                    AddMessage(new Message(String.Format("The corpse of {0} turns into dust.", c.DeadGuy.Name), map.LocalTime.TurnCounter, Color.Purple));
                            }
                        }
                    }
                    #endregion

                    #region Infection effects
                    if (hasInfection)
                    {
                        List<Actor> infectedToKill = null;
                        foreach (Actor a in map.Actors)
                        {
                            if (a.Infection >= Rules.INFECTION_LEVEL_1_WEAK && !a.Model.Abilities.IsUndead)
                            {
                                int infectionP = m_Rules.ActorInfectionPercent(a);

                                #region
                                if (m_Rules.Roll(0, 1000) < m_Rules.InfectionEffectTriggerChance1000(infectionP))
                                {
                                    bool isVisible = IsVisibleToPlayer(a);
                                    bool isPlayer = a.IsPlayer;  // alpha10.1 consistency fix
                                    bool isBot = a.IsBotPlayer;  // alpha10.1 handle bot

                                    // if sleeping, wake up.
                                    if (a.IsSleeping)
                                        DoWakeUp(a);

                                    // apply effect.
                                    bool killHim = false;
                                    if (infectionP >= Rules.INFECTION_LEVEL_5_DEATH)
                                    {
                                        killHim = true;
                                    }
                                    else if (infectionP >= Rules.INFECTION_LEVEL_4_BLEED)
                                    {
                                        DoVomit(a);
                                        a.HitPoints -= Rules.INFECTION_LEVEL_4_BLEED_HP;
                                        if (isVisible)
                                        {
                                            if (isPlayer) ClearMessages();
                                            AddMessage(MakeMessage(a, String.Format("{0} blood.", Conjugate(a, VERB_VOMIT)), Color.Purple));
                                            if (isPlayer && !isBot)
                                            {
                                                AddMessagePressEnter();
                                                ClearMessages();
                                            }
                                        }
                                        if (a.HitPoints <= 0)
                                            killHim = true;
                                    }
                                    else if (infectionP >= Rules.INFECTION_LEVEL_3_VOMIT)
                                    {
                                        DoVomit(a);
                                        if (isVisible)
                                        {
                                            if (isPlayer) ClearMessages();
                                            AddMessage(MakeMessage(a, String.Format("{0}.", Conjugate(a, VERB_VOMIT)), Color.Purple));
                                            if (isPlayer && !isBot)
                                            {
                                                AddMessagePressEnter();
                                                ClearMessages();
                                            }
                                        }
                                    }
                                    else if (infectionP >= Rules.INFECTION_LEVEL_2_TIRED)
                                    {
                                        SpendActorStaminaPoints(a, Rules.INFECTION_LEVEL_2_TIRED_STA);
                                        a.SleepPoints -= Rules.INFECTION_LEVEL_2_TIRED_SLP;
                                        if (a.SleepPoints < 0) a.SleepPoints = 0;
                                        if (isVisible)
                                        {
                                            if (isPlayer) ClearMessages();
                                            AddMessage(MakeMessage(a, String.Format("{0} sick and tired.", Conjugate(a, VERB_FEEL)), Color.Purple));
                                            if (isPlayer && !isBot)
                                            {
                                                AddMessagePressEnter();
                                                ClearMessages();
                                            }
                                        }
                                    }
                                    else if (infectionP >= Rules.INFECTION_LEVEL_1_WEAK)
                                    {
                                        SpendActorStaminaPoints(a, Rules.INFECTION_LEVEL_1_WEAK_STA);
                                        if (isVisible)
                                        {
                                            if (isPlayer) ClearMessages();
                                            AddMessage(MakeMessage(a, String.Format("{0} sick and weak.", Conjugate(a, VERB_FEEL)), Color.Purple));
                                            if (isPlayer && !isBot)
                                            {
                                                AddMessagePressEnter();
                                                ClearMessages();
                                            }
                                        }
                                    }

                                    // if it kills him, remember.
                                    if (killHim)
                                    {
                                        if (infectedToKill == null) infectedToKill = new List<Actor>(map.CountActors);
                                        infectedToKill.Add(a);
                                    }
                                } // trigged effect
                                #endregion
                            } // is infected
                        } // each actor

                        // kill infected to kill (duh)
                        if (infectedToKill != null)
                        {
                            foreach (Actor a in infectedToKill)
                            {
                                if (IsVisibleToPlayer(a))
                                    AddMessage(MakeMessage(a, String.Format("{0} of infection!", Conjugate(a, VERB_DIE))));
                                KillActor(null, a, "infection");
                                // if player, force zombify NOW.
                                if (a.IsPlayer)
                                {
                                    // remove player corpse!
                                    map.TryRemoveCorpseOf(a);
                                    // zombify player!
                                    Zombify(null, a, false);

                                    // show
                                    AddMessage(MakeMessage(a, Conjugate(a, "turn") + " into a Zombie!"));
                                    RedrawPlayScreen();
                                    AnimDelay(DELAY_LONG);
                                }
                            }
                        }
                    }
                    #endregion
                }  // non STD game.
                #endregion

                // 1. Update odors.
                // alpha10 obsolete     1.1 Odor suppression/generation.
                #region
                //List<OdorScent> scentGenerated = new List<OdorScent>();
                //foreach (OdorScent scent in map.Scents)
                //{
                //    switch (scent.Odor)
                //    {
                //        case Odor.PERFUME_LIVING_SUPRESSOR:
                //            // remember to suppress living scent.
                //            scentGenerated.Add(new OdorScent(Odor.LIVING, -scent.Strength, scent.Position));
                //            break;

                //        case Odor.PERFUME_LIVING_GENERATOR:
                //            // remember to generate living scent here.
                //            scentGenerated.Add(new OdorScent(Odor.LIVING, scent.Strength, scent.Position));
                //            break;
                //    }
                //}
                //foreach (OdorScent genScent in scentGenerated)
                //    map.ModifyScentAt(genScent.Odor, genScent.Strength, genScent.Position);
                #endregion

                //      1.2 Odors decay.
                #region
                List<OdorScent> scentGarbage = null;

                // decay map scents
                foreach (OdorScent scent in map.Scents)
                {
                    // alpha10
                    int decay = m_Rules.OdorsDecay(map, scent.Position, m_Session.World.Weather);

                    // decay.
                    map.ModifyScentAt(scent.Odor, -decay, scent.Position);

                    // garbage?
                    if (scent.Strength < OdorScent.MIN_STRENGTH)
                    {
                        if (scentGarbage == null) scentGarbage = new List<OdorScent>(1);
                        scentGarbage.Add(scent);
                    }
                }
                if (scentGarbage != null)
                {
                    foreach (OdorScent scent in scentGarbage)
                        map.RemoveScent(scent);
                    scentGarbage = null;
                }
                #endregion

                //      1.3 Actors scents.
                #region
                foreach (Actor actor in map.Actors)
                {
                    // alpha10
                    DropActorScents(actor);
                    DecayActorScents(actor);
                }
                #endregion

                // 2. Regen actors AP & STA
                #region
                // regen.
                foreach (Actor actor in map.Actors)
                {
                    if (!actor.IsSleeping)
                        actor.ActionPoints += m_Rules.ActorSpeed(actor);

                    if (actor.StaminaPoints < m_Rules.ActorMaxSTA(actor))
                        RegenActorStaminaPoints(actor, Rules.STAMINA_REGEN_PER_TURN);
                }
                // reset actor index.
                map.CheckNextActorIndex = 0;
                #endregion

                // 3. Stop tired actors from running.
                #region
                foreach (Actor actor in map.Actors)
                {
                    if (actor.IsRunning)
                    {
                        if (actor.StaminaPoints < Rules.STAMINA_MIN_FOR_ACTIVITY)
                        {
                            actor.IsRunning = false;
                            if (actor == m_Player)
                            {
                                AddMessage(MakeMessage(actor, String.Format("{0} too tired to continue running!", Conjugate(actor, VERB_BE))));
                                RedrawPlayScreen();
                            }
                        }
                    }
                }
                #endregion

                // 4. Actor gauges & states
                #region
                List<Actor> actorsStarvedToDeath = null;
                foreach (Actor actor in map.Actors)
                {
                    // hunger && rot.
                    #region
                    if (actor.Model.Abilities.HasToEat)
                    {
                        // food points loss.
                        --actor.FoodPoints;
                        if (actor.FoodPoints < 0) actor.FoodPoints = 0;

                        // May kill starved actors.
                        if (m_Rules.IsActorStarving(actor))
                        {
                            // kill him?
                            if (m_Rules.RollChance(Rules.FOOD_STARVING_DEATH_CHANCE))
                            {
                                if (actor.IsPlayer || s_Options.NPCCanStarveToDeath)
                                {
                                    if (actorsStarvedToDeath == null)
                                        actorsStarvedToDeath = new List<Actor>();
                                    actorsStarvedToDeath.Add(actor);
                                }
                            }
                        }
                    }
                    else if (actor.Model.Abilities.IsRotting)
                    {
                        // rot.
                        --actor.FoodPoints;
                        if (actor.FoodPoints < 0) actor.FoodPoints = 0;

                        // rot effects.
                        if (m_Rules.IsRottingActorStarving(actor))
                        {
                            // loose 1 HP.
                            if (m_Rules.Roll(0, 1000) < Rules.ROT_STARVING_HP_CHANCE)
                            {
                                if (IsVisibleToPlayer(actor))
                                {
                                    AddMessage(MakeMessage(actor, "is rotting away."));
                                }
                                if (--actor.HitPoints <= 0)
                                {
                                    if (actorsStarvedToDeath == null)
                                        actorsStarvedToDeath = new List<Actor>();
                                    actorsStarvedToDeath.Add(actor);
                                }
                            }
                        }
                        else if (m_Rules.IsRottingActorHungry(actor))
                        {
                            // loose a skill.
                            if (m_Rules.Roll(0, 1000) < Rules.ROT_HUNGRY_SKILL_CHANCE)
                                DoLooseRandomSkill(actor);
                        }
                    }
                    #endregion

                    // sleep.
                    #region
                    if (actor.Model.Abilities.HasToSleep)
                    {
                        // sleep vs sleep points loss.
                        if (actor.IsSleeping)
                        {
                            // sleeping.
                            // nightmare?
                            if (m_Rules.IsActorDisturbed(actor) && m_Rules.RollChance(Rules.SANITY_NIGHTMARE_CHANCE))
                            {
                                // wake up, shout, lose sleep and sta.
                                DoWakeUp(actor);
                                DoShout(actor, "NO! LEAVE ME ALONE!");
                                actor.SleepPoints -= Rules.SANITY_NIGHTMARE_SLP_LOSS;
                                if (actor.SleepPoints < 0) actor.SleepPoints = 0;
                                SpendActorSanity(actor, Rules.SANITY_NIGHTMARE_SAN_LOSS);
                                SpendActorStaminaPoints(actor, Rules.SANITY_NIGHTMARE_STA_LOSS);
                                // msg.
                                if (IsVisibleToPlayer(actor))
                                    AddMessage(MakeMessage(actor, String.Format("{0} from a horrible nightmare!", Conjugate(actor, VERB_WAKE_UP))));
                                // if player, sfx.
                                if (actor.IsPlayer)
                                {
                                    // FIXME replace with sfx
                                    // alpha10 
                                    m_MusicManager.Stop();
                                    m_MusicManager.Play(GameSounds.NIGHTMARE, MusicPriority.PRIORITY_EVENT);
                                }
                            }
                        }
                        else
                        {
                            // awake.
                            --actor.SleepPoints;
                            if (map.LocalTime.IsNight)
                                --actor.SleepPoints;
                            if (actor.SleepPoints < 0) actor.SleepPoints = 0;
                        }

                        //      4.2 Handle sleeping actors.
                        #region
                        if (actor.IsSleeping)
                        {
                            bool isOnCouch = m_Rules.IsOnCouch(actor);
                            // activity.
                            actor.Activity = Activity.SLEEPING;

                            // regen sleep pts.
                            int sleepRegen = m_Rules.ActorSleepRegen(actor, isOnCouch);
                            actor.SleepPoints += sleepRegen;
                            actor.SleepPoints = Math.Min(actor.SleepPoints, m_Rules.ActorMaxSleep(actor));

                            // heal?
                            if (actor.HitPoints < m_Rules.ActorMaxHPs(actor))
                            {
                                int healChance = (isOnCouch ? Rules.SLEEP_ON_COUCH_HEAL_CHANCE : 0);
                                healChance += m_Rules.ActorHealChanceBonus(actor);
                                if (m_Rules.RollChance(healChance))
                                    RegenActorHitPoints(actor, Rules.SLEEP_HEAL_HITPOINTS);
                            }

                            // wake up?
                            // wake up if hungry or fully slept.
                            bool wakeUp = m_Rules.IsActorHungry(actor) || actor.SleepPoints >= m_Rules.ActorMaxSleep(actor);
                            if (wakeUp)
                            {
                                DoWakeUp(actor);
                            }
                            else
                            {
                                if (actor.IsPlayer)
                                {
                                    // check music.
                                    if (m_MusicManager.Music != GameMusics.SLEEP)
                                    {
                                        m_MusicManager.Stop();
                                        m_MusicManager.PlayLooping(GameMusics.SLEEP, MusicPriority.PRIORITY_EVENT);
                                    }
                                    // message.
                                    AddMessage(new Message("...zzZZZzzZ...", map.LocalTime.TurnCounter, Color.DarkCyan));
                                    RedrawPlayScreen();
                                    // give some time to sim thread.
                                    if (s_Options.SimThread)
                                        Thread.Sleep(10);
                                }
                                else if (m_Rules.RollChance(MESSAGE_NPC_SLEEP_SNORE_CHANCE) && IsVisibleToPlayer(actor))
                                {
                                    AddMessage(MakeMessage(actor, String.Format("{0}.", Conjugate(actor, VERB_SNORE))));
                                    RedrawPlayScreen();
                                }
                            }
                        }
                        #endregion

                        //      4.3 Exhausted actors might collapse.
                        #region
                        if (m_Rules.IsActorExhausted(actor))
                        {
                            if (m_Rules.RollChance(Rules.SLEEP_EXHAUSTION_COLLAPSE_CHANCE))
                            {
                                // do it
                                DoStartSleeping(actor);

                                // message.
                                if (IsVisibleToPlayer(actor))
                                {
                                    AddMessage(MakeMessage(actor, String.Format("{0} from exhaustion !!", Conjugate(actor, VERB_COLLAPSE))));
                                    RedrawPlayScreen();
                                }

                                // player?
                                if (actor == m_Player)
                                {
                                    UpdatePlayerFOV(m_Player);
                                    ComputeViewRect(m_Player.Location.Position);
                                    RedrawPlayScreen();
                                }
                            }
                        }
                        #endregion
                    }
                    #endregion

                    // sanity.
                    #region
                    if (actor.Model.Abilities.HasSanity)
                    {
                        // sanity loss.
                        if (--actor.Sanity <= 0) actor.Sanity = 0;
                    }
                    #endregion

                    // leader trust & leader/follower bond.
                    #region
                    if (actor.HasLeader)
                    {
                        // trust.
                        ModifyActorTrustInLeader(actor, m_Rules.ActorTrustIncrease(actor.Leader), false);
                        // bond with leader.
                        if (m_Rules.HasActorBondWith(actor, actor.Leader) && m_Rules.RollChance(Rules.SANITY_RECOVER_BOND_CHANCE))
                        {
                            RegenActorSanity(actor, Rules.SANITY_RECOVER_BOND);
                            RegenActorSanity(actor.Leader, Rules.SANITY_RECOVER_BOND);
                            if (IsVisibleToPlayer(actor))
                                AddMessage(MakeMessage(actor, String.Format("{0} reassured knowing {1} is with {2}.",
                                            Conjugate(actor, VERB_FEEL), actor.Leader.Name, HimOrHer(actor))));
                            if (IsVisibleToPlayer(actor.Leader))
                                AddMessage(MakeMessage(actor.Leader, String.Format("{0} reassured knowing {1} is with {2}.",
                                            Conjugate(actor.Leader, VERB_FEEL), actor.Name, HimOrHer(actor.Leader))));
                        }
                    }
                    #endregion

                }
                #endregion
                #region Kill (zombify) starved actors.
                if (actorsStarvedToDeath != null)
                {
                    foreach (Actor actor in actorsStarvedToDeath)
                    {
                        // message.
                        if (IsVisibleToPlayer(actor))
                        {
                            AddMessage(MakeMessage(actor, String.Format("{0} !!", Conjugate(actor, VERB_DIE_FROM_STARVATION))));
                            RedrawPlayScreen();
                        }

                        // kill.
                        KillActor(null, actor, "starvation");

                        // zombify?
                        if (!actor.Model.Abilities.IsUndead && Rules.HasImmediateZombification(m_Session.GameMode) && m_Rules.RollChance(s_Options.StarvedZombificationChance))
                        {
                            // remove morpse!
                            map.TryRemoveCorpseOf(actor);
                            // zombify!
                            Zombify(null, actor, false);
                            // show.
                            if (IsVisibleToPlayer(actor))
                            {
                                AddMessage(MakeMessage(actor, String.Format("{0} into a Zombie!", Conjugate(actor, "turn"))));
                                RedrawPlayScreen();
                                AnimDelay(DELAY_LONG);
                            }
                        }
                    }
                }
                #endregion

                // 5. Check batteries : lights, trackers.
                #region
                foreach (Actor actor in map.Actors)
                {
                    Item leftItem = actor.GetEquippedItem(DollPart.LEFT_HAND);
                    if (leftItem == null)
                        continue;

                    // light?
                    ItemLight light = leftItem as ItemLight;
                    if (light != null)
                    {
                        if (light.Batteries > 0)
                        {
                            --light.Batteries;
                            if (light.Batteries <= 0)
                            {
                                if (IsVisibleToPlayer(actor))
                                {
                                    AddMessage(MakeMessage(actor, String.Format(": {0} light goes off.", light.TheName)));
                                }
                            }
                        }
                        continue;
                    }

                    // tracker?
                    ItemTracker tracker = leftItem as ItemTracker;
                    if (tracker != null)
                    {
                        if (tracker.Batteries > 0)
                        {
                            --tracker.Batteries;
                            if (tracker.Batteries <= 0)
                            {
                                if (IsVisibleToPlayer(actor))
                                {
                                    AddMessage(MakeMessage(actor, String.Format(": {0} goes off.", tracker.TheName)));
                                }
                            }
                        }
                        continue;
                    }
                }
                #endregion

                // 6. Check explosives.
                #region
                // 6.1 Update fuses.
                bool hasExplosivesToExplode = false;
                #region
                // on ground.
                foreach (Inventory groundInv in map.GroundInventories)
                {
                    // update each explosive fuse there,
                    // remember which should explode.
                    foreach (Item it in groundInv.Items)
                    {
                        ItemPrimedExplosive primed = it as ItemPrimedExplosive;
                        if (primed == null)
                            continue;

                        // primed explosive, burn fuse.
                        --primed.FuseTimeLeft;
                        if (primed.FuseTimeLeft <= 0)
                            hasExplosivesToExplode = true;
                    }
                }

                // on actors.
                foreach (Actor actor in map.Actors)
                {
                    Inventory inv = actor.Inventory;
                    if (inv == null || inv.IsEmpty)
                        continue;

                    // update each explosive fuse there,
                    // remember which should explode.
                    foreach (Item it in inv.Items)
                    {
                        ItemPrimedExplosive primed = it as ItemPrimedExplosive;
                        if (primed == null)
                            continue;

                        // primed explosive, burn fuse.
                        --primed.FuseTimeLeft;
                        if (primed.FuseTimeLeft <= 0)
                            hasExplosivesToExplode = true;
                    }
                }
                #endregion

                // 6.2 Explode.
                #region
                if (hasExplosivesToExplode)
                {
                    bool hasExplodedSomething = false;
                    do
                    {
                        // nothing exploded by default.
                        hasExplodedSomething = false;

                        // on ground.
                        if (!hasExplodedSomething)
                        {
                            foreach (Inventory groundInv in map.GroundInventories)
                            {
                                Point? pos = map.GetGroundInventoryPosition(groundInv);
                                if (pos == null)
                                    throw new InvalidOperationException("explosives : GetGroundInventoryPosition returned null point");

                                foreach (Item it in groundInv.Items)
                                {
                                    ItemPrimedExplosive primed = it as ItemPrimedExplosive;
                                    if (primed == null)
                                        continue;

                                    if (primed.FuseTimeLeft <= 0)
                                    {
                                        // boom!
                                        map.RemoveItemAt(primed, pos.Value);
                                        DoBlast(new Location(map, pos.Value), (primed.Model as ItemExplosiveModel).BlastAttack);
                                        hasExplodedSomething = true;
                                        break;
                                    }
                                }

                                if (hasExplodedSomething)
                                    break;
                            }
                        }

                        // on actors.
                        if (!hasExplodedSomething)
                        {
                            foreach (Actor actor in map.Actors)
                            {
                                Inventory inv = actor.Inventory;
                                if (inv == null || inv.IsEmpty)
                                    continue;

                                foreach (Item it in inv.Items)
                                {
                                    ItemPrimedExplosive primed = it as ItemPrimedExplosive;
                                    if (primed == null)
                                        continue;

                                    if (primed.FuseTimeLeft <= 0)
                                    {
                                        // boom!
                                        actor.Inventory.RemoveAllQuantity(primed);
                                        DoBlast(new Location(map, actor.Location.Position), (primed.Model as ItemExplosiveModel).BlastAttack);
                                        hasExplodedSomething = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    while (hasExplodedSomething);
                }
                #endregion
                #endregion

                // 7. Check fires.
                #region
                // 7.1 Rain has a chance to put out fires.
                // FIXME there still the weather bug when simulating = weather used is current world weather, not map weather.
                #region
                // Check?
                if (m_Rules.IsWeatherRain(m_Session.World.Weather) && m_Rules.RollChance(Rules.FIRE_RAIN_TEST_CHANCE))
                {
                    // 7.1.1 Burning objects?
                    foreach (MapObject obj in map.MapObjects)
                    {
                        if (obj.IsOnFire && m_Rules.RollChance(Rules.FIRE_RAIN_PUT_OUT_CHANCE))
                        {
                            // do it.
                            UnapplyOnFire(obj);
                            // tell.
                            if (IsVisibleToPlayer(obj))
                            {
                                AddMessage(new Message("The rain has put out a fire.", map.LocalTime.TurnCounter));
                            }
                        }
                    }
                }
                #endregion
                #endregion
            }   // skipped in lodetail turns.

            // -- Check timers.
            #region
            if (map.CountTimers > 0)
            {
                List<TimedTask> timersGarbage = null;
                foreach (TimedTask t in map.Timers)
                {
                    t.Tick(map);
                    if (t.IsCompleted)
                    {
                        if (timersGarbage == null) timersGarbage = new List<TimedTask>(map.CountTimers);
                        timersGarbage.Add(t);
                    }
                }
                if (timersGarbage != null)
                {
                    foreach (TimedTask t in timersGarbage)
                        map.RemoveTimer(t);
                }
            }
            #endregion

            // -- Advance local time.
            #region
            bool wasLocalNight = map.LocalTime.IsNight;
            ++map.LocalTime.TurnCounter;
            bool isLocalDay = !map.LocalTime.IsNight;
            #endregion

            // -- Check for NPC upgrade.
            #region
            if (wasLocalNight && isLocalDay)
            {
                HandleLivingNPCsUpgrade(map);
            }
            else if (s_Options.ZombifiedsUpgradeDays != GameOptions.ZupDays.OFF && !wasLocalNight && !isLocalDay && GameOptions.IsZupDay(s_Options.ZombifiedsUpgradeDays, map.LocalTime.Day))
            {
                HandleUndeadNPCsUpgrade(map);
            }
            #endregion
        }

        // alpha10
        void DropActorScents(Actor actor)
        {
            // alpha10 dont drop if odor suppressed
            if (actor.OdorSuppressorCounter > 0)
                return;

            if (actor.Model.Abilities.IsUndead)
            {
                // ZM scent?
                if (actor.Model.Abilities.IsUndeadMaster)
                    actor.Location.Map.RefreshScentAt(Odor.UNDEAD_MASTER, Rules.UNDEAD_MASTER_SCENT_DROP, actor.Location.Position);
            }
            else
            {
                // Living scent.
                actor.Location.Map.RefreshScentAt(Odor.LIVING, Rules.LIVING_SCENT_DROP, actor.Location.Position);
            }
        }

        // alpha10
        void DecayActorScents(Actor actor)
        {
            // decay suppressor
            if (actor.OdorSuppressorCounter > 0)
            {
                int decay = m_Rules.OdorsDecay(actor.Location.Map, actor.Location.Position, m_Session.World.Weather);
                actor.OdorSuppressorCounter -= decay;
                if (actor.OdorSuppressorCounter < 0) actor.OdorSuppressorCounter = 0;
            }
        }

        void ModifyActorTrustInLeader(Actor a, int mod, bool addMessage)
        {
            // do it.
            a.TrustInLeader += mod;
            if (a.TrustInLeader > Rules.TRUST_MAX)
                a.TrustInLeader = Rules.TRUST_MAX;
            else if (a.TrustInLeader < Rules.TRUST_MIN)
                a.TrustInLeader = Rules.TRUST_MIN;

            // if leader is player, message.
            if (addMessage && a.Leader.IsPlayer)
                AddMessage(new Message(String.Format("({0} trust with {1})", mod, a.TheName), m_Session.WorldTime.TurnCounter, Color.White));
        }
        #endregion

        #region Stats
        int CountLivings(Map map)
        {
            if (map == null)
                throw new ArgumentNullException("map");

            int count = 0;
            foreach (Actor a in map.Actors)
                if (!a.Model.Abilities.IsUndead)
                    ++count;

            return count;
        }

        int CountActors(Map map, Predicate<Actor> predFn)
        {
            if (map == null)
                throw new ArgumentNullException("map");

            int count = 0;
            foreach (Actor a in map.Actors)
                if (predFn(a))
                    ++count;

            return count;
        }

        int CountFaction(Map map, Faction f)
        {
            if (map == null)
                throw new ArgumentNullException("map");

            int count = 0;
            foreach (Actor a in map.Actors)
                if (a.Faction == f)
                    ++count;

            return count;
        }

        int CountUndeads(Map map)
        {
            if (map == null)
                throw new ArgumentNullException("map");

            int count = 0;
            foreach (Actor a in map.Actors)
                if (a.Model.Abilities.IsUndead)
                    ++count;

            return count;
        }

        int CountFoodItemsNutrition(Map map)
        {
            if (map == null)
                throw new ArgumentNullException("map");

            // food items on ground.
            int groundNutrition = 0;
            foreach (Inventory inv in map.GroundInventories)
            {
                if (inv.IsEmpty)
                    continue;
                foreach (Item it in inv.Items)
                {
                    if (it is ItemFood)
                        groundNutrition += m_Rules.FoodItemNutrition(it as ItemFood, map.LocalTime.TurnCounter);
                }
            }
            // food items carried by actors.
            int carriedNutrition = 0;
            foreach (Actor a in map.Actors)
            {
                Inventory inv = a.Inventory;
                if (inv == null || inv.IsEmpty)
                    continue;
                foreach (Item it in inv.Items)
                {
                    if (it is ItemFood)
                        carriedNutrition += m_Rules.FoodItemNutrition(it as ItemFood, map.LocalTime.TurnCounter);
                }
            }

            return groundNutrition + carriedNutrition;
        }

        bool HasActorOfModelID(Map map, GameActors.IDs actorModelID)
        {
            if (map == null)
                throw new ArgumentNullException("map");

            foreach (Actor a in map.Actors)
                if (a.Model.ID == (int)actorModelID)
                    return true;

            return false;
        }
        #endregion

        #region Events
        #region Zombie Invasion
        bool CheckForEvent_ZombieInvasion(Map map)
        {
            // when midnight strikes only.
            if (!map.LocalTime.IsStrikeOfMidnight)
                return false;

            // if not enough zombies only.
            int undeads = CountUndeads(map);
            if (undeads >= s_Options.MaxUndeads)
                return false;

            // clear.
            return true;
        }

        void FireEvent_ZombieInvasion(Map map)
        {
            // announce.
            if (map == m_Player.Location.Map && !m_Player.IsSleeping && !m_Player.Model.Abilities.IsUndead)
            {
                // message.
                AddMessage(new Message("It is Midnight! Zombies are invading!", m_Session.WorldTime.TurnCounter, Color.Crimson));
                RedrawPlayScreen();
            }

            // do it.
            int undeads = CountUndeads(map);
            float invasionRatio = Math.Min(1.0f, (map.LocalTime.Day * s_Options.ZombieInvasionDailyIncrease + s_Options.DayZeroUndeadsPercent) / 100.0f);
            int targetUndeadsCount = 1 + (int)(invasionRatio * s_Options.MaxUndeads);
            int undeadsToSpawn = targetUndeadsCount - undeads;
            for (int i = 0; i < undeadsToSpawn; i++)
                SpawnNewUndead(map, map.LocalTime.Day);

        }
        #endregion

        #region Sewers Invasion
        bool CheckForEvent_SewersInvasion(Map map)
        {
            // check game mode.
            if (!Rules.HasZombiesInSewers(m_Session.GameMode))
                return false;

            // randomly.
            if (!m_Rules.RollChance(SEWERS_INVASION_CHANCE))
                return false;

            // if not enough zombies only.
            int undeads = CountUndeads(map);
            if (undeads >= s_Options.MaxUndeads * SEWERS_UNDEADS_FACTOR)
                return false;

            // clear.
            return true;
        }

        void FireEvent_SewersInvasion(Map map)
        {
            // do it silently.
            int undeads = CountUndeads(map);
            float invasionRatio = Math.Min(1.0f, (map.LocalTime.Day * s_Options.ZombieInvasionDailyIncrease + s_Options.DayZeroUndeadsPercent) / 100.0f);
            int targetUndeadsCount = 1 + (int)(invasionRatio * s_Options.MaxUndeads * SEWERS_UNDEADS_FACTOR);
            int undeadsToSpawn = targetUndeadsCount - undeads;
            for (int i = 0; i < undeadsToSpawn; i++)
                SpawnNewSewersUndead(map, map.LocalTime.Day);

        }
        #endregion

        #region DISABLED Subway Invasion
#if false
        bool CheckForEvent_SubwayInvasion(Map map)
        {
            // randomly.
            if (!m_Rules.RollChance(SUBWAY_INVASION_CHANCE))
                return false;

            // if not enough zombies only.
            int undeads = CountUndeads(map);
            if (undeads >= s_Options.MaxUndeads * SUBWAY_UNDEADS_FACTOR)
                return false;

            // clear.
            return true;
        }

        void FireEvent_SubwayInvasion(Map map)
        {
            // do it silently.
            int undeads = CountUndeads(map);
            float invasionRatio = Math.Min(1.0f, (map.LocalTime.Day * s_Options.ZombieInvasionDailyIncrease + s_Options.DayZeroUndeadsPercent) / 100.0f);
            int targetUndeadsCount = 1 + (int)(invasionRatio * s_Options.MaxUndeads * SUBWAY_UNDEADS_FACTOR);
            int undeadsToSpawn = targetUndeadsCount - undeads;
            for (int i = 0; i < undeadsToSpawn; i++)
                SpawnNewSubwayUndead(map, map.LocalTime.Day);

        }
#endif
        #endregion

        #region Refugees wave
        bool CheckForEvent_RefugeesWave(Map map)
        {
            // when midday strikes only.
            if (!map.LocalTime.IsStrikeOfMidday)
                return false;

            // if not enough civs only.
#if false // TEST: disabled
            int civs = CountActors(map, (a) => a.Faction == GameFactions.TheCivilians || a.Faction == GameFactions.ThePolice);
            if (civs >= Options.MaxCivilians)
                return false;
#endif

            // clear.
            return true;
        }

        /// <summary>
        /// Double factor on city borders, half factor in city center, normal factor in all other districts.
        /// </summary> 
        /// <param name="d"></param>
        /// <returns></returns>
        float RefugeesEventDistrictFactor(District d)
        {
            int dx = d.WorldPosition.X;
            int dy = d.WorldPosition.Y;
            int border = m_Session.World.Size - 1;
            int center = border / 2;

            return (dx == 0 || dy == 0 || dx == border || dy == border ? 2f :
                dx == center && dy == center ? 0.5f :
                1f);
        }

        void FireEvent_RefugeesWave(District district)
        {
            // announce.
            if (district == m_Player.Location.Map.District && !m_Player.IsSleeping && !m_Player.Model.Abilities.IsUndead)
            {
                // message.
                AddMessage(new Message("A new wave of refugees has arrived!", m_Session.WorldTime.TurnCounter, Color.Pink));
                RedrawPlayScreen();
            }

            // Spawn most on the surface and a small number in sewers and subway.
            int civilians = CountActors(district.EntryMap, (a) => a.Faction == GameFactions.TheCivilians || a.Faction == GameFactions.ThePolice);
            int size = 1 + (int)(REFUGEES_WAVE_SIZE * RefugeesEventDistrictFactor(district) * s_Options.MaxCivilians);
            int civiliansToSpawn = Math.Min(size, s_Options.MaxCivilians - civilians);
            Map spawnMap = null;
            for (int i = 0; i < civiliansToSpawn; i++)
            {
                // map: surface or sewers/subway.
                if (m_Rules.RollChance(REFUGEE_SURFACE_SPAWN_CHANCE))
                    spawnMap = district.EntryMap;
                else
                {
                    // 50% sewers and 50% subway, but some districts have no subway.
                    if (district.HasSubway)
                        spawnMap = m_Rules.RollChance(50) ? district.SubwayMap : district.SewersMap;
                    else
                        spawnMap = district.SewersMap;
                }
                // do it.
                SpawnNewRefugee(spawnMap);
            }

            // check for uniques, always in surface.
            if (m_Rules.RollChance(UNIQUE_REFUGEE_CHECK_CHANCE))
            {
                lock (m_Session) // thread safe
                {
                    UniqueActor[] array = m_Session.UniqueActors.ToArray();
                    UniqueActor[] mayArrive = Array.FindAll(array,
                        (UniqueActor a) =>
                        {
                            return a.IsWithRefugees && !a.IsSpawned && !a.TheActor.IsDead;
                        });
                    if (mayArrive != null && mayArrive.Length > 0)
                    {
                        int iArrive = m_Rules.Roll(0, mayArrive.Length);
                        FireEvent_UniqueActorArrive(district.EntryMap, mayArrive[iArrive]);
                    }
                }
            }
        }

        void FireEvent_UniqueActorArrive(Map map, UniqueActor unique)
        {
            // try to find a spot.
            bool spawned = SpawnActorOnMapBorder(map, unique.TheActor, SPAWN_DISTANCE_TO_PLAYER, true);

            // if failed, cancel.
            if (!spawned)
                return;

            // mark as spawned.
            unique.IsSpawned = true;

            // announce.
            if (map == m_Player.Location.Map && !m_Player.IsSleeping && !m_Player.Model.Abilities.IsUndead)
            {
                // alpha factorized to PlayUniqueActorMusicAndMessage
                //// message and music.
                //if (unique.EventMessage != null)
                //{
                //    if (unique.EventThemeMusic != null)
                //    {
                //        m_MusicManager.Stop();
                //        m_MusicManager.Play(unique.EventThemeMusic, MusicPriority.PRIORITY_EVENT);
                //    }
                //    // message.
                //    ClearMessages();
                //    AddMessage(new Message(unique.EventMessage, m_Session.WorldTime.TurnCounter, Color.Pink));
                //    AddMessage(MakePlayerCentricMessage("Seems to come from", unique.TheActor.Location.Position));
                //    AddMessagePressEnter();
                //    ClearMessages();
                //}
                PlayUniqueActorMusicAndMessage(unique, true);
                // scoring event.
                m_Session.Scoring.AddEvent(m_Session.WorldTime.TurnCounter, unique.TheActor.Name + " arrived.");
            }
        }

        // alpha10
        void PlayUniqueActorMusicAndMessage(UniqueActor unique, bool hasArrived)
        {
            if (unique.EventMessage != null)
            {
                Overlay highlightOverlay = null;

                if (unique.EventThemeMusic != null)
                {
                    m_MusicManager.Stop();
                    m_MusicManager.Play(unique.EventThemeMusic, MusicPriority.PRIORITY_EVENT);
                }

                ClearMessages();
                AddMessage(new Message(unique.EventMessage, m_Session.WorldTime.TurnCounter, Color.Pink));
                if (hasArrived)
                    AddMessage(MakePlayerCentricMessage("Seems to come from", unique.TheActor.Location.Position));
                else
                {
                    highlightOverlay = new OverlayRect(Color.Pink, new Rectangle(MapToScreen(unique.TheActor.Location.Position), new Size(TILE_SIZE, TILE_SIZE)));
                    AddOverlay(highlightOverlay);
                }
                if (!m_Player.IsBotPlayer)
                {
                    AddMessagePressEnter();
                    ClearMessages();
                }
                if (highlightOverlay != null)
                    RemoveOverlay(highlightOverlay);
            }
        }
        #endregion

        #region National guard
        bool CheckForEvent_NationalGuard(Map map)
        {
            // if option zeroed, don't bother.
            if (s_Options.NatGuardFactor == 0)
                return false;

            // during day only.
            if (map.LocalTime.IsNight)
                return false;

            // date.
            if (map.LocalTime.Day < NATGUARD_DAY)
                return false;
            if (map.LocalTime.Day >= NATGUARD_END_DAY)
                return false;

            // check chance.
            if (!m_Rules.RollChance(NATGUARD_INTERVENTION_CHANCE))
                return false;

            // if zombies significantly outnumber livings only (army count as 2 livings).
            int livings = CountLivings(map) + CountFaction(map, GameFactions.TheArmy);
            int undeads = CountUndeads(map);
            float undeadsPerLiving = (float)undeads / (float)livings;
            if (undeadsPerLiving * (s_Options.NatGuardFactor / 100f) < NATGUARD_INTERVENTION_FACTOR)
                return false;

            // clear.
            return true;
        }

        void FireEvent_NationalGuard(Map map)
        {
            // do it.
            // spawn squad leader then troopers.
            Actor squadLeader = SpawnNewNatGuardLeader(map);
            if (squadLeader != null)
            {
                for (int i = 0; i < NATGUARD_SQUAD_SIZE - 1; i++)
                {
                    // spawn trooper.
                    Actor trooper = SpawnNewNatGuardTrooper(map, squadLeader.Location.Position);
                    // add to leader squad.
                    if (trooper != null)
                        squadLeader.AddFollower(trooper);
                }
            }
            if (squadLeader == null)
                return;

            // notify AI.
            NotifyOrderablesAI(map, RaidType.NATGUARD, squadLeader.Location.Position);

            // announce.
            if (map == m_Player.Location.Map && !m_Player.IsSleeping && !m_Player.Model.Abilities.IsUndead)
            {
                // music.
                m_MusicManager.Stop();
                m_MusicManager.Play(GameMusics.ARMY, MusicPriority.PRIORITY_EVENT);

                // message.
                ClearMessages();
                AddMessage(new Message("A National Guard squad has arrived!", m_Session.WorldTime.TurnCounter, Color.LightGreen));
                AddMessage(MakePlayerCentricMessage("Soldiers seem to come from", squadLeader.Location.Position));
                if (!m_Player.IsBotPlayer)
                {
                    AddMessagePressEnter();
                    ClearMessages();
                }
            }

            // scoring event.
            if (map == m_Player.Location.Map)
            {
                m_Session.Scoring.AddEvent(m_Session.WorldTime.TurnCounter, "A National Guard squad arrived.");
            }

        }
        #endregion

        #region Army drop supplies
        bool CheckForEvent_ArmySupplies(Map map)
        {
            // if option zeroed, don't bother.
            if (s_Options.SuppliesDropFactor == 0)
                return false;

            // during day only.
            if (map.LocalTime.IsNight)
                return false;

            // date.
            if (map.LocalTime.Day < ARMY_SUPPLIES_DAY)
                return false;

            // check chance.
            if (!m_Rules.RollChance(ARMY_SUPPLIES_CHANCE))
                return false;

            // count food items vs livings.
            int livingsNeedFood = 1 + CountActors(map, (a) => !a.Model.Abilities.IsUndead && a.Model.Abilities.HasToEat && a.Faction == GameFactions.TheCivilians);
            int food = 1 + CountFoodItemsNutrition(map);
            float foodPerLiving = (float)food / (float)livingsNeedFood;
            if (foodPerLiving >= (s_Options.SuppliesDropFactor / 100f) * ARMY_SUPPLIES_FACTOR)
                return false;

            // clear.
            return true;
        }

        void FireEvent_ArmySupplies(Map map)
        {
            ////////////////////////////
            // Do it.
            // 1. Pick drop point.
            // 2. Drop scattered items.
            ////////////////////////////

            // 1. Pick drop point.
            Point dropPoint;
            bool dropped = FindDropSuppliesPoint(map, out dropPoint);
            if (!dropped)
                return;

            // 2. Drop scattered items.
            // only outside and free of actor and objects.
            int xmin = dropPoint.X - ARMY_SUPPLIES_SCATTER;
            int xmax = dropPoint.X + ARMY_SUPPLIES_SCATTER;
            int ymin = dropPoint.Y - ARMY_SUPPLIES_SCATTER;
            int ymax = dropPoint.Y + ARMY_SUPPLIES_SCATTER;
            map.TrimToBounds(ref xmin, ref ymin);
            map.TrimToBounds(ref xmax, ref ymax);
            for (int sx = xmin; sx <= xmax; sx++)
                for (int sy = ymin; sy <= ymax; sy++)
                {
                    if (!IsSuitableDropSuppliesPoint(map, sx, sy))
                        continue;

                    // drop stuff.
                    Item it = m_Rules.RollChance(80) ? m_TownGenerator.MakeItemArmyRation() : m_TownGenerator.MakeItemMedikit();
                    map.DropItemAt(it, sx, sy);
                }

            // notify AI.
            NotifyOrderablesAI(map, RaidType.ARMY_SUPLLIES, dropPoint);

            // announce.
            if (map == m_Player.Location.Map && !m_Player.IsSleeping && !m_Player.Model.Abilities.IsUndead)
            {
                // music.
                m_MusicManager.Stop();
                m_MusicManager.Play(GameMusics.ARMY, MusicPriority.PRIORITY_EVENT);

                // message.
                ClearMessages();
                AddMessage(new Message("An Army chopper has dropped supplies!", m_Session.WorldTime.TurnCounter, Color.LightGreen));
                AddMessage(MakePlayerCentricMessage("The drop point seems to be", dropPoint));
                if (!m_Player.IsBotPlayer)
                {
                    AddMessagePressEnter();
                    ClearMessages();
                }
            }

            // scoring event.
            if (map == m_Player.Location.Map)
            {
                m_Session.Scoring.AddEvent(m_Session.WorldTime.TurnCounter, "An army chopper dropped supplies.");
            }
        }

        bool IsSuitableDropSuppliesPoint(Map map, int x, int y)
        {
            //////////////////////////////
            // Must be:
            // 1. In bounds.
            // 2. Outside & walkable.
            // 3. No actor nor object.
            // 4. Far enough from player.
            //////////////////////////////

            // 1. In bounds.
            if (!map.IsInBounds(x, y))
                return false;

            // 2. Outside & walkable.
            Tile tile = map.GetTileAt(x, y);
            if (tile.IsInside || !tile.Model.IsWalkable)
                return false;

            // 3. No actor nor object.
            if (map.GetActorAt(x, y) != null || map.GetMapObjectAt(x, y) != null)
                return false;

            // 4. Far enough from player.
            if (DistanceToPlayer(map, x, y) < SPAWN_DISTANCE_TO_PLAYER)
                return false;

            // all clear.
            return true;
        }

        bool FindDropSuppliesPoint(Map map, out Point dropPoint)
        {
            dropPoint = new Point();

            // try to find a suitable point.
            int maxAttempts = 4 * map.Width;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // roll.
                dropPoint.X = m_Rules.RollX(map);
                dropPoint.Y = m_Rules.RollY(map);

                // suitable?
                if (!IsSuitableDropSuppliesPoint(map, dropPoint.X, dropPoint.Y))
                    continue;

                // we're good.
                return true;
            }

            // failed
            return false;
        }
        #endregion

        #region Timed raids
        bool HasRaidHappenedSince(RaidType raid, District district, WorldTime mapTime, int sinceNTurns)
        {
            return m_Session.HasRaidHappened(raid, district) && mapTime.TurnCounter - m_Session.LastRaidTime(raid, district) < sinceNTurns;
        }

        #region Bikers raid
        bool CheckForEvent_BikersRaid(Map map)
        {
            // date.
            if (map.LocalTime.Day < BIKERS_RAID_DAY)
                return false;
            if (map.LocalTime.Day >= BIKERS_END_DAY)
                return false;

            // last time : at least N day
            if (HasRaidHappenedSince(RaidType.BIKERS, map.District, map.LocalTime, BIKERS_RAID_DAYS_GAP * WorldTime.TURNS_PER_DAY))
                return false;

            // check chance.
            if (!m_Rules.RollChance(BIKERS_RAID_CHANCE_PER_TURN))
                return false;

            // if no bikers.
#if false
            disabled to take advantage of the new rival gang feature.
            if(HasActorOfModelID(map, GameActors.IDs.BIKER_MAN))
                return false;
#endif

            // clear.
            return true;
        }

        void FireEvent_BikersRaid(Map map)
        {
            // remember time.
            m_Session.SetLastRaidTime(RaidType.BIKERS, map.District, map.LocalTime.TurnCounter);

            // roll a random gang.
            GameGangs.IDs gangId = GameGangs.BIKERS[m_Rules.Roll(0, GameGangs.BIKERS.Length)];

            // do it.
            // spawn raid leader then squadies.
            Actor raidLeader = SpawnNewBikerLeader(map, gangId);
            if (raidLeader != null)
            {
                for (int i = 0; i < BIKERS_RAID_SIZE - 1; i++)
                {
                    // spawn squadie.
                    Actor squadie = SpawnNewBiker(map, gangId, raidLeader.Location.Position);
                    // add to leader squad.
                    if (squadie != null)
                        raidLeader.AddFollower(squadie);
                }
            }
            if (raidLeader == null)
                return;

            // notify AI.
            NotifyOrderablesAI(map, RaidType.BIKERS, raidLeader.Location.Position);

            // announce.
            if (map == m_Player.Location.Map && !m_Player.IsSleeping && !m_Player.Model.Abilities.IsUndead)
            {
                // music.
                m_MusicManager.Stop();
                m_MusicManager.Play(GameMusics.BIKER, MusicPriority.PRIORITY_EVENT);

                // message.
                ClearMessages();
                AddMessage(new Message("You hear the sound of roaring engines!", m_Session.WorldTime.TurnCounter, Color.LightGreen));
                AddMessage(MakePlayerCentricMessage("Motorbikes seem to come from", raidLeader.Location.Position));
                if (!m_Player.IsBotPlayer)
                {
                    AddMessagePressEnter();
                    ClearMessages();
                }
            }

            // scoring event.
            if (map == m_Player.Location.Map)
            {
                m_Session.Scoring.AddEvent(m_Session.WorldTime.TurnCounter, "Bikers raided the district.");
            }
        }
        #endregion

        #region Gangsta raid
        bool CheckForEvent_GangstasRaid(Map map)
        {
            // date.
            if (map.LocalTime.Day < GANGSTAS_RAID_DAY)
                return false;
            if (map.LocalTime.Day >= GANGSTAS_END_DAY)
                return false;

            // last time : at least N day
            if (HasRaidHappenedSince(RaidType.GANGSTA, map.District, map.LocalTime, GANGSTAS_RAID_DAYS_GAP * WorldTime.TURNS_PER_DAY))
                return false;

            // check chance.
            if (!m_Rules.RollChance(GANGSTAS_RAID_CHANCE_PER_TURN))
                return false;

            // if no gangsta.
#if false
            disabled to take advantage of the new rival gang feature.
            if (HasActorOfModelID(map, GameActors.IDs.GANGSTA_MAN))
                return false;
#endif

            // clear.
            return true;
        }

        void FireEvent_GangstasRaid(Map map)
        {
            // remember time.
            m_Session.SetLastRaidTime(RaidType.GANGSTA, map.District, map.LocalTime.TurnCounter);

            // roll a random gang.
            GameGangs.IDs gangId = GameGangs.GANGSTAS[m_Rules.Roll(0, GameGangs.GANGSTAS.Length)];

            // do it.
            // spawn raid leader then squadies.
            Actor raidLeader = SpawnNewGangstaLeader(map, gangId);
            if (raidLeader != null)
            {
                for (int i = 0; i < GANGSTAS_RAID_SIZE - 1; i++)
                {
                    // spawn squadie.
                    Actor squadie = SpawnNewGangsta(map, gangId, raidLeader.Location.Position);
                    // add to leader squad.
                    if (squadie != null)
                        raidLeader.AddFollower(squadie);
                }
            }
            if (raidLeader == null)
                return;

            // notify AI.
            NotifyOrderablesAI(map, RaidType.GANGSTA, raidLeader.Location.Position);

            // announce.
            if (map == m_Player.Location.Map && !m_Player.IsSleeping && !m_Player.Model.Abilities.IsUndead)
            {
                // music.
                m_MusicManager.Stop();
                m_MusicManager.Play(GameMusics.GANGSTA, MusicPriority.PRIORITY_EVENT);

                // message.
                ClearMessages();
                AddMessage(new Message("You hear obnoxious loud music!", m_Session.WorldTime.TurnCounter, Color.LightGreen));
                AddMessage(MakePlayerCentricMessage("Cars seem to come from", raidLeader.Location.Position));
                if (!m_Player.IsBotPlayer)
                {
                    AddMessagePressEnter();
                    ClearMessages();
                }
            }

            // scoring event.
            if (map == m_Player.Location.Map)
            {
                m_Session.Scoring.AddEvent(m_Session.WorldTime.TurnCounter, "Gangstas raided the district.");
            }
        }
        #endregion

        #region BlackOps raid
        bool CheckForEvent_BlackOpsRaid(Map map)
        {
            // date.
            if (map.LocalTime.Day < BLACKOPS_RAID_DAY)
                return false;

            // last time : at least N day
            if (HasRaidHappenedSince(RaidType.BLACKOPS, map.District, map.LocalTime, BLACKOPS_RAID_DAY_GAP * WorldTime.TURNS_PER_DAY))
                return false;

            // check chance.
            if (!m_Rules.RollChance(BLACKOPS_RAID_CHANCE_PER_TURN))
                return false;

            // clear.
            return true;
        }

        void FireEvent_BlackOpsRaid(Map map)
        {
            // remember time.
            m_Session.SetLastRaidTime(RaidType.BLACKOPS, map.District, map.LocalTime.TurnCounter);

            // do it.
            // spawn raid leader then squadies.
            Actor raidLeader = SpawnNewBlackOpsLeader(map);
            if (raidLeader != null)
            {
                for (int i = 0; i < BLACKOPS_RAID_SIZE - 1; i++)
                {
                    // spawn squadie.
                    Actor squadie = SpawnNewBlackOpsTrooper(map, raidLeader.Location.Position);
                    // add to leader squad.
                    if (squadie != null)
                        raidLeader.AddFollower(squadie);
                }
            }
            if (raidLeader == null)
                return;

            // notify AI.
            NotifyOrderablesAI(map, RaidType.BLACKOPS, raidLeader.Location.Position);

            // announce.
            if (map == m_Player.Location.Map && !m_Player.IsSleeping && !m_Player.Model.Abilities.IsUndead)
            {
                // music.
                m_MusicManager.Stop();
                m_MusicManager.Play(GameMusics.ARMY, MusicPriority.PRIORITY_EVENT);

                // message.
                ClearMessages();
                AddMessage(new Message("You hear a chopper flying over the city!", m_Session.WorldTime.TurnCounter, Color.LightGreen));
                AddMessage(MakePlayerCentricMessage("The chopper has dropped something", raidLeader.Location.Position));
                if (!m_Player.IsBotPlayer)
                {
                    AddMessagePressEnter();
                    ClearMessages();
                }
            }

            // scoring event.
            if (map == m_Player.Location.Map)
            {
                m_Session.Scoring.AddEvent(m_Session.WorldTime.TurnCounter, "BlackOps raided the district.");
            }
        }
        #endregion

        #region Band of Survivors
        bool CheckForEvent_BandOfSurvivors(Map map)
        {
            // date.
            if (map.LocalTime.Day < SURVIVORS_BAND_DAY)
                return false;

            // last time : at least N day
            if (HasRaidHappenedSince(RaidType.SURVIVORS, map.District, map.LocalTime, SURVIVORS_BAND_DAY_GAP * WorldTime.TURNS_PER_DAY))
                return false;

            // check chance.
            if (!m_Rules.RollChance(SURVIVORS_BAND_CHANCE_PER_TURN))
                return false;

            // clear.
            return true;
        }

        void FireEvent_BandOfSurvivors(Map map)
        {
            // remember time.
            m_Session.SetLastRaidTime(RaidType.SURVIVORS, map.District, map.LocalTime.TurnCounter);

            // do it.
            // spawn dudes.
            Actor bandScout = SpawnNewSurvivor(map);
            if (bandScout != null)
            {
                for (int i = 0; i < SURVIVORS_BAND_SIZE - 1; i++)
                    SpawnNewSurvivor(map, bandScout.Location.Position);
            }
            if (bandScout == null)
                return;

            // notify AI.
            NotifyOrderablesAI(map, RaidType.SURVIVORS, bandScout.Location.Position);

            // announce.
            if (map == m_Player.Location.Map && !m_Player.IsSleeping && !m_Player.Model.Abilities.IsUndead)
            {
                // music.
                m_MusicManager.Stop();
                m_MusicManager.Play(GameMusics.SURVIVORS, MusicPriority.PRIORITY_EVENT);

                // message.
                ClearMessages();
                AddMessage(new Message("You hear shooting and honking in the distance.", m_Session.WorldTime.TurnCounter, Color.LightGreen));
                AddMessage(MakePlayerCentricMessage("A van has stopped", bandScout.Location.Position));
                if (!m_Player.IsBotPlayer)
                {
                    AddMessagePressEnter();
                    ClearMessages();
                }
            }

            // scoring event.
            if (map == m_Player.Location.Map)
            {
                m_Session.Scoring.AddEvent(m_Session.WorldTime.TurnCounter, "A Band of Survivors entered the district.");
            }
        }
        #endregion

        #endregion

        #endregion

        #region Spawning new actors
        int DistanceToPlayer(Map map, int x, int y)
        {
            if (m_Player == null || m_Player.Location.Map != map)
                return int.MaxValue;
            return m_Rules.GridDistance(m_Player.Location.Position, x, y);
        }

        int DistanceToPlayer(Map map, Point pos)
        {
            return DistanceToPlayer(map, pos.X, pos.Y);
        }

        bool IsAdjacentToEnemy(Map map, Point pos, Actor actor)
        {
            int xmin = pos.X - 1;
            int xmax = pos.X + 1;
            int ymin = pos.Y - 1;
            int ymax = pos.Y + 1;
            map.TrimToBounds(ref xmin, ref ymin);
            map.TrimToBounds(ref xmax, ref ymax);

            for (int x = xmin; x <= xmax; x++)
                for (int y = ymin; y <= ymax; y++)
                {
                    if (x == pos.X && y == pos.Y)
                        continue;
                    Actor other = map.GetActorAt(x, y);
                    if (other == null)
                        continue;
                    if (m_Rules.AreEnemies(actor, other))
                        return true;
                }

            return false;
        }

        bool SpawnActorOnMapBorder(Map map, Actor actorToSpawn, int minDistToPlayer, bool mustBeOutside)
        {
            // find a good spot.
            int maxTries = 4 * (map.Width + map.Height);

            int i = 0;
            Point pos = new Point();
            do
            {
                ++i;

                // roll a position on the border.
                int x = (m_Rules.RollChance(50) ? 0 : map.Width - 1);
                int y = (m_Rules.RollChance(50) ? 0 : map.Height - 1);
                if (m_Rules.RollChance(50))
                    x = m_Rules.RollX(map);
                else
                    y = m_Rules.RollY(map);

                // must be free, (outside), far enough to player and not adjacent to an enemy.
                pos.X = x;
                pos.Y = y;
                if (mustBeOutside && map.GetTileAt(pos.X, pos.Y).IsInside)
                    continue;
                if (!m_Rules.IsWalkableFor(actorToSpawn, map, pos.X, pos.Y))
                    continue;
                if (DistanceToPlayer(map, pos) < minDistToPlayer)
                    continue;
                if (IsAdjacentToEnemy(map, pos, actorToSpawn))
                    continue;

                // success!                
                map.PlaceActorAt(actorToSpawn, pos);
                // trigger stuff
                OnActorEnterTile(actorToSpawn);
                return true;
            }
            while (i <= maxTries);

            // failed.
            return false;
        }

        bool SpawnActorNear(Map map, Actor actorToSpawn, int minDistToPlayer, Point nearPoint, int maxDistToPoint)
        {
            // find a good spot.
            int maxTries = 4 * (map.Width + map.Height);

            int i = 0;
            Point pos = new Point();
            do
            {
                ++i;

                // roll a position.
                int x = nearPoint.X + m_Rules.Roll(1, maxDistToPoint + 1) - m_Rules.Roll(1, maxDistToPoint + 1);
                int y = nearPoint.Y + m_Rules.Roll(1, maxDistToPoint + 1) - m_Rules.Roll(1, maxDistToPoint + 1);

                // trim to map.
                pos.X = x;
                pos.Y = y;
                map.TrimToBounds(ref pos);

                // must free, outside, far enough to player and not adjacent to an enemy.
                if (map.GetTileAt(pos.X, pos.Y).IsInside)
                    continue;
                if (!m_Rules.IsWalkableFor(actorToSpawn, map, pos.X, pos.Y))
                    continue;
                if (DistanceToPlayer(map, pos) < minDistToPlayer)
                    continue;
                if (IsAdjacentToEnemy(map, pos, actorToSpawn))
                    continue;

                // success!                
                map.PlaceActorAt(actorToSpawn, pos);
                return true;
            }
            while (i <= maxTries);

            // failed.
            return false;
        }

        void SpawnNewUndead(Map map, int day)
        {
            ////////////////
            // Create actor.
            ////////////////
            Actor newUndead = m_TownGenerator.CreateNewUndead(map.LocalTime.TurnCounter);

            ///////////////////
            // Spawn hi level?
            ///////////////////
            if (s_Options.AllowUndeadsEvolution && Rules.HasEvolution(m_Session.GameMode))
            {
                // chances.
                int levelupChance = Math.Min(75, day * 2); // +2% per day, max 75%.
                bool doLevelUp = false;
                GameActors.IDs levelupID = (GameActors.IDs)newUndead.Model.ID;
                if (m_Rules.RollChance(levelupChance))
                {
                    doLevelUp = true;
                    levelupID = NextUndeadEvolution((GameActors.IDs)newUndead.Model.ID);
                    if (m_Rules.RollChance(levelupChance))
                        levelupID = NextUndeadEvolution(levelupID);
                }

                // restrict some models.
                if (levelupID == GameActors.IDs.UNDEAD_ZOMBIE_LORD && day < ZOMBIE_LORD_EVOLUTION_MIN_DAY)
                    doLevelUp = false;

                // levelup?
                if (doLevelUp)
                {
                    newUndead.Model = GameActors[levelupID];
                }
            }

            ///////////////////
            // Try to spawn it.
            ///////////////////
            SpawnActorOnMapBorder(map, newUndead, SPAWN_DISTANCE_TO_PLAYER, true);
        }

        void SpawnNewSewersUndead(Map map, int day)
        {
            ////////////////
            // Create actor.
            ////////////////
            Actor newUndead = m_TownGenerator.CreateNewSewersUndead(map.LocalTime.TurnCounter);

            ///////////////////
            // Try to spawn it.
            ///////////////////
            SpawnActorOnMapBorder(map, newUndead, SPAWN_DISTANCE_TO_PLAYER, false);
        }

        void SpawnNewSubwayUndead(Map map, int day)
        {
            ////////////////
            // Create actor.
            ////////////////
            Actor newUndead = m_TownGenerator.CreateNewSubwayUndead(map.LocalTime.TurnCounter);

            ///////////////////
            // Try to spawn it.
            ///////////////////
            SpawnActorOnMapBorder(map, newUndead, SPAWN_DISTANCE_TO_PLAYER, false);
        }


        void SpawnNewRefugee(Map map)
        {
            ////////////////
            // Create actor.
            ////////////////
            Actor newCivilian = m_TownGenerator.CreateNewRefugee(map.LocalTime.TurnCounter, REFUGEES_WAVE_ITEMS);

            ///////////////////
            // Try to spawn it.
            ///////////////////
            SpawnActorOnMapBorder(map, newCivilian, SPAWN_DISTANCE_TO_PLAYER, true);
        }

        Actor SpawnNewSurvivor(Map map)
        {
            ////////////////
            // Create actor.
            ////////////////
            Actor newSurvivor = m_TownGenerator.CreateNewSurvivor(map.LocalTime.TurnCounter);

            ///////////////////
            // Try to spawn it.
            ///////////////////
            if (SpawnActorOnMapBorder(map, newSurvivor, SPAWN_DISTANCE_TO_PLAYER, true))
                return newSurvivor;
            else
                return null;
        }

        Actor SpawnNewSurvivor(Map map, Point bandPos)
        {
            ////////////////
            // Create actor.
            ////////////////
            Actor newSurvivor = m_TownGenerator.CreateNewSurvivor(map.LocalTime.TurnCounter);

            ///////////////////
            // Try to spawn it.
            ///////////////////
            if (SpawnActorNear(map, newSurvivor, SPAWN_DISTANCE_TO_PLAYER, bandPos, 3))
                return newSurvivor;
            else
                return null;
        }

        Actor SpawnNewNatGuardLeader(Map map)
        {
            ////////////////
            // Create actor.
            ////////////////
            Actor newNatLeader = m_TownGenerator.CreateNewArmyNationalGuard(map.LocalTime.TurnCounter, "Sgt");

            // skills.
            m_TownGenerator.GiveStartingSkillToActor(newNatLeader, Skills.IDs.LEADERSHIP);

            // additional items : z-tracker.
            if (map.LocalTime.Day > NATGUARD_ZTRACKER_DAY)
            {
                newNatLeader.Inventory.AddAll(m_TownGenerator.MakeItemZTracker());
            }

            ///////////////////
            // Try to spawn it.
            ///////////////////
            bool spawned = SpawnActorOnMapBorder(map, newNatLeader, SPAWN_DISTANCE_TO_PLAYER, true);

            // done.
            return spawned ? newNatLeader : null;
        }

        Actor SpawnNewNatGuardTrooper(Map map, Point leaderPos)
        {
            ////////////////
            // Create actor.
            ////////////////
            Actor newNatGuard = m_TownGenerator.CreateNewArmyNationalGuard(map.LocalTime.TurnCounter, "Pvt");

            // additional items : combat knife or grenades.
            if (m_Rules.RollChance(50))
                newNatGuard.Inventory.AddAll(m_TownGenerator.MakeItemCombatKnife());
            else
                newNatGuard.Inventory.AddAll(m_TownGenerator.MakeItemGrenade());

            ///////////////////
            // Try to spawn it.
            ///////////////////
            bool spawned = SpawnActorNear(map, newNatGuard, SPAWN_DISTANCE_TO_PLAYER, leaderPos, 3);

            // done.
            return spawned ? newNatGuard : null;
        }

        Actor SpawnNewBikerLeader(Map map, GameGangs.IDs gangId)
        {
            ////////////////
            // Create actor.
            ////////////////
            Actor newBikerLeader = m_TownGenerator.CreateNewBikerMan(map.LocalTime.TurnCounter, gangId);

            // skills.
            m_TownGenerator.GiveStartingSkillToActor(newBikerLeader, Skills.IDs.LEADERSHIP);
            m_TownGenerator.GiveStartingSkillToActor(newBikerLeader, Skills.IDs.TOUGH);
            m_TownGenerator.GiveStartingSkillToActor(newBikerLeader, Skills.IDs.TOUGH);
            m_TownGenerator.GiveStartingSkillToActor(newBikerLeader, Skills.IDs.TOUGH);
            m_TownGenerator.GiveStartingSkillToActor(newBikerLeader, Skills.IDs.STRONG);
            m_TownGenerator.GiveStartingSkillToActor(newBikerLeader, Skills.IDs.STRONG);
            m_TownGenerator.GiveStartingSkillToActor(newBikerLeader, Skills.IDs.STRONG);

            ///////////////////
            // Try to spawn it.
            ///////////////////
            bool spawned = SpawnActorOnMapBorder(map, newBikerLeader, SPAWN_DISTANCE_TO_PLAYER, true);

            // done.
            return spawned ? newBikerLeader : null;
        }

        Actor SpawnNewBiker(Map map, GameGangs.IDs gangId, Point leaderPos)
        {
            ////////////////
            // Create actor.
            ////////////////
            Actor newBiker = m_TownGenerator.CreateNewBikerMan(map.LocalTime.TurnCounter, gangId);

            // skils.
            m_TownGenerator.GiveStartingSkillToActor(newBiker, Skills.IDs.TOUGH);
            m_TownGenerator.GiveStartingSkillToActor(newBiker, Skills.IDs.STRONG);

            ///////////////////
            // Try to spawn it.
            ///////////////////
            bool spawned = SpawnActorNear(map, newBiker, SPAWN_DISTANCE_TO_PLAYER, leaderPos, 3);

            // done.
            return spawned ? newBiker : null;
        }

        Actor SpawnNewGangstaLeader(Map map, GameGangs.IDs gangId)
        {
            ////////////////
            // Create actor.
            ////////////////
            Actor newGangstaLeader = m_TownGenerator.CreateNewGangstaMan(map.LocalTime.TurnCounter, gangId);

            // skills.
            m_TownGenerator.GiveStartingSkillToActor(newGangstaLeader, Skills.IDs.LEADERSHIP);
            m_TownGenerator.GiveStartingSkillToActor(newGangstaLeader, Skills.IDs.AGILE);
            m_TownGenerator.GiveStartingSkillToActor(newGangstaLeader, Skills.IDs.AGILE);
            m_TownGenerator.GiveStartingSkillToActor(newGangstaLeader, Skills.IDs.AGILE);
            m_TownGenerator.GiveStartingSkillToActor(newGangstaLeader, Skills.IDs.FIREARMS);

            ///////////////////
            // Try to spawn it.
            ///////////////////
            bool spawned = SpawnActorOnMapBorder(map, newGangstaLeader, SPAWN_DISTANCE_TO_PLAYER, true);

            // done.
            return spawned ? newGangstaLeader : null;
        }

        Actor SpawnNewGangsta(Map map, GameGangs.IDs gangId, Point leaderPos)
        {
            ////////////////
            // Create actor.
            ////////////////
            Actor newGangsta = m_TownGenerator.CreateNewGangstaMan(map.LocalTime.TurnCounter, gangId);

            // skils.
            m_TownGenerator.GiveStartingSkillToActor(newGangsta, Skills.IDs.AGILE);

            ///////////////////
            // Try to spawn it.
            ///////////////////
            bool spawned = SpawnActorNear(map, newGangsta, SPAWN_DISTANCE_TO_PLAYER, leaderPos, 3);

            // done.
            return spawned ? newGangsta : null;
        }

        Actor SpawnNewBlackOpsLeader(Map map)
        {
            ////////////////
            // Create actor.
            ////////////////
            Actor newBOLeader = m_TownGenerator.CreateNewBlackOps(map.LocalTime.TurnCounter, "Officer");

            // skills.
            m_TownGenerator.GiveStartingSkillToActor(newBOLeader, Skills.IDs.LEADERSHIP);
            m_TownGenerator.GiveStartingSkillToActor(newBOLeader, Skills.IDs.AGILE);
            m_TownGenerator.GiveStartingSkillToActor(newBOLeader, Skills.IDs.AGILE);
            m_TownGenerator.GiveStartingSkillToActor(newBOLeader, Skills.IDs.AGILE);
            m_TownGenerator.GiveStartingSkillToActor(newBOLeader, Skills.IDs.FIREARMS);
            m_TownGenerator.GiveStartingSkillToActor(newBOLeader, Skills.IDs.FIREARMS);
            m_TownGenerator.GiveStartingSkillToActor(newBOLeader, Skills.IDs.FIREARMS);
            m_TownGenerator.GiveStartingSkillToActor(newBOLeader, Skills.IDs.TOUGH);
            m_TownGenerator.GiveStartingSkillToActor(newBOLeader, Skills.IDs.TOUGH);
            m_TownGenerator.GiveStartingSkillToActor(newBOLeader, Skills.IDs.TOUGH);

            ///////////////////
            // Try to spawn it.
            ///////////////////
            bool spawned = SpawnActorOnMapBorder(map, newBOLeader, SPAWN_DISTANCE_TO_PLAYER, true);

            // done.
            return spawned ? newBOLeader : null;
        }

        Actor SpawnNewBlackOpsTrooper(Map map, Point leaderPos)
        {
            ////////////////
            // Create actor.
            ////////////////
            Actor newBO = m_TownGenerator.CreateNewBlackOps(map.LocalTime.TurnCounter, "Agent");

            // skills.
            m_TownGenerator.GiveStartingSkillToActor(newBO, Skills.IDs.AGILE);
            m_TownGenerator.GiveStartingSkillToActor(newBO, Skills.IDs.FIREARMS);
            m_TownGenerator.GiveStartingSkillToActor(newBO, Skills.IDs.TOUGH);

            ///////////////////
            // Try to spawn it.
            ///////////////////
            bool spawned = SpawnActorNear(map, newBO, SPAWN_DISTANCE_TO_PLAYER, leaderPos, 3);

            // done.
            return spawned ? newBO : null;
        }
        #endregion

        #region Updating Player FOV
        void UpdatePlayerFOV(Actor player)
        {
            if (player == null)
                return;
            m_PlayerFOV = LOS.ComputeFOVFor(m_Rules, player, m_Session.WorldTime, m_Session.World.Weather);
            player.Location.Map.SetViewAndMarkVisited(m_PlayerFOV);
        }
        #endregion

        // alpha10.1 Bot Mode - DEBUG build only
        #region Experimental Bot Mode
#if DEBUG
        bool m_isBotMode = false;
        BaseAI m_botControl = null;
        const int BOT_DELAY = DELAY_SHORT;
        readonly Object m_botLock = new Object(); // necessary because dev keys presses from RogueForm can happen at any time

        public void BotToggleControl()
        {
            lock (m_botLock)
            {
                if (m_isBotMode)
                    BotReleaseControl();
                else
                    BotTakeControl();
            }
        }

        void BotTakeControl()
        {
            // bot restrictions check
            if (m_Player == null || m_Player.IsDead)
            {
                AddMessage(MakeErrorMessage("Bot cannot take control of null/dead player"));
                return;
            }

            if (m_botControl != null)
                m_botControl.LeaveControl();

            try
            {
                Type aiClass = m_Player.Model.DefaultController;
                if (aiClass == null)
                    throw new Exception("actor model has null defaultcontroller");
                ActorController aiController = aiClass.GetConstructor(Type.EmptyTypes).Invoke(null) as ActorController;
                if (!(aiController is BaseAI))
                    throw new Exception("actor model defaultcontroller is not BaseAI");

                m_botControl = aiController as BaseAI;
                m_botControl.TakeControl(m_Player);
                m_Player.IsBotPlayer = true;
                m_isBotMode = true;
                AddMessage(MakeMessage(m_Player, "is now bot controlled by " + m_botControl.GetType() + ".", Color.LightGreen));
            }
            catch (Exception e)
            {
                ClearMessages();
                AddMessage(MakeErrorMessage("error while creating bot ai:"));
                AddMessage(MakeErrorMessage(e.Message));
                AddMessagePressEnter();
            }
        }

        void BotReleaseControl()
        {
            if (m_botControl == null)
                return;
            if (m_Player != null)
                m_Player.IsBotPlayer = false;
            m_botControl.LeaveControl();
            m_botControl = null;
            m_isBotMode = false;
            if (m_Player != null)
                AddMessage(MakeMessage(m_Player, "is now human controlled.", Color.LightGreen));
        }
#endif
        #endregion

        #region Handling Player/AI actor action

        void HandlePlayerActor(Actor player)
        {
            // Upkeep.
            UpdatePlayerFOV(player);    // make sure LOS is up to date.
            m_Player = player;      // remember player.
            ComputeViewRect(player.Location.Position);

            // Update survival scoring.
            m_Session.Scoring.TurnsSurvived = m_Session.WorldTime.TurnCounter;

            // Check if long wait.
            #region
            if (m_IsPlayerLongWait)
            {
                if (CheckPlayerWaitLong(player))
                {
                    // continue waiting.
                    DoWait(player);
                    return;
                }
                else
                {
                    // stop long wait.
                    m_IsPlayerLongWait = false;
                    m_IsPlayerLongWaitForcedStop = false;

                    // wait ended or interrupted.
                    if (m_Session.WorldTime.TurnCounter >= m_PlayerLongWaitEnd.TurnCounter)
                    {
                        AddMessage(new Message("Wait ended.", m_Session.WorldTime.TurnCounter, Color.Yellow));
                    }
                    else
                    {
                        AddMessage(new Message("Wait interrupted!", m_Session.WorldTime.TurnCounter, Color.Red));
                    }
                }
            }
            #endregion

            /////////////////////////////////////////////////
            // Loop until the player has made a valid choice
            /////////////////////////////////////////////////
            bool loop = true;
            do
            {
                ///////////////////
                // 1. Redraw
                // 2. Get input.
                // 3. Handle input
                ///////////////////

                // 1. Redraw
                m_UI.UI_SetCursor(null);

                // alpha10.1 bot mode?
#if DEBUG
                lock (m_botLock)
                {
                    if (m_isBotMode)
                    {
                        try { Thread.Sleep(BOT_DELAY); } catch { }  // AnimDelay() does not work here because it just pause the ui
                        RedrawPlayScreen();
                        if (m_botControl != null) // for some reason even with the lock this can become null here. wth?? is thread.sleep the culprit??
                        {
                            ActorAction botAction = m_botControl.GetAction(this);
                            if (botAction == null || !botAction.IsLegal())
                            {
                                AddMessage(MakeErrorMessage("Bot issued " + (botAction == null ? "NULL" : "illegal " + botAction.ToString()) + " action"));
                                botAction = new ActionWait(player, this);
                            }
                            botAction.Perform();
                            // copy-paste is bad
                            UpdatePlayerFOV(player);
                            ComputeViewRect(player.Location.Position);
                            m_Session.LastTurnPlayerActed = m_Session.WorldTime.TurnCounter;
                            RedrawPlayScreen();
                        }
                        return;
                    }
                }
#endif

                // hint available?
                // alpha10 no hint if undead
                if (m_Player != null && !m_Player.IsDead && !m_Player.Model.Abilities.IsUndead)
                {
                    // alpha10 fix properly handle hint overlay
                    int availableHint = -1;
                    if (s_Options.IsAdvisorEnabled && (availableHint = GetAdvisorFirstAvailableHint()) != -1)
                    {
                        Point overlayPos = MapToScreen(m_Player.Location.Position.X - 3, m_Player.Location.Position.Y - 1);
                        if (m_HintAvailableOverlay == null)
                        {
                            m_HintAvailableOverlay = new OverlayPopup(
                                null,
                                Color.White, Color.White, Color.Black,
                                overlayPos);
                            AddOverlay(m_HintAvailableOverlay);
                        }
                        else
                        {
                            m_HintAvailableOverlay.ScreenPosition = overlayPos;
                            if (!HasOverlay(m_HintAvailableOverlay))
                                AddOverlay(m_HintAvailableOverlay);
                        }

                        string hintTitle;
                        string[] hintBody;
                        GetAdvisorHintText((AdvisorHint)availableHint, out hintTitle, out hintBody);
                        m_HintAvailableOverlay.Lines = new string[] {
                            string.Format("HINT AVAILABLE PRESS <{0}>", s_KeyBindings.Get(PlayerCommand.ADVISOR).ToString()),
                            hintTitle };
                    }
                    else if (m_HintAvailableOverlay != null && HasOverlay(m_HintAvailableOverlay))
                    {
                        RemoveOverlay(m_HintAvailableOverlay);
                    }
                }
                RedrawPlayScreen();

                // 2. Get input.
                // Peek keyboard & mouse until we got an event.
                m_UI.UI_PeekKey();  // consume keys to avoid repeats.
                bool inputLoop = true;
                bool hasKey = false;
                KeyEventArgs inKey;
                Point prevMousePos = m_UI.UI_GetMousePosition();
                Point mousePos = new Point(-1, -1);
                MouseButtons? mouseButtons = null;
                do
                {
                    inKey = m_UI.UI_PeekKey();
                    if (inKey != null)
                    {
                        hasKey = true;
                        inputLoop = false;
                    }
                    else
                    {
                        mousePos = m_UI.UI_GetMousePosition();
                        mouseButtons = m_UI.UI_PeekMouseButtons();
                        if (mousePos != prevMousePos || mouseButtons != null)
                        {
                            inputLoop = false;
                        }
                    }
                }
                while (inputLoop);


                // 3. Handle input
                if (hasKey)
                {
                    //////////////
                    // Handle key
                    //////////////
                    #region
                    PlayerCommand command = InputTranslator.KeyToCommand(inKey);
                    if (command == PlayerCommand.QUIT_GAME)    // quit game.
                    {
                        if (HandleQuitGame())
                        {
                            // stop sim thread.
                            StopSimThread(true);  // alpha10 abort allowed when quitting
                            // quit asap.
                            RedrawPlayScreen();
                            m_IsGameRunning = false;
                            return;
                        }
                    }
                    else
                    {
                        switch (command)
                        {
                            #region options, menu etc...
                            case PlayerCommand.ABANDON_GAME:
                                if (HandleAbandonGame())
                                {
                                    StopSimThread(true); // alpha10 abort allowed when quitting
                                    loop = false;
                                    KillActor(null, m_Player, "suicide");
                                }
                                break;

                            case PlayerCommand.HELP_MODE:
                                HandleHelpMode();
                                break;

                            case PlayerCommand.HINTS_SCREEN_MODE:
                                HandleHintsScreen();
                                break;

                            case PlayerCommand.ADVISOR:
                                HandleAdvisor(player);
                                break;

                            case PlayerCommand.OPTIONS_MODE:
                                HandleOptions(true);
                                ApplyOptions(true);
                                break;

                            case PlayerCommand.KEYBINDING_MODE:
                                HandleRedefineKeys();
                                break;

                            case PlayerCommand.MESSAGE_LOG:
                                HandleMessageLog();
                                break;

                            // alpha10.1 moved sim thread responsability out to DoLoadGame
                            case PlayerCommand.LOAD_GAME:
                                // load.
                                HandleLoadGame();
                                // refresh player local variable!!
                                player = m_Player;
                                // stop looping.
                                loop = false;
                                // stop the update loop!
                                m_HasLoadedGame = true;
                                break;
                            // alpha10.1 moved sim thread responsability out to DoSaveGame
                            case PlayerCommand.SAVE_GAME:
                                HandleSaveGame();
                                break;

                            case PlayerCommand.SCREENSHOT:
                                HandleScreenshot();
                                break;

                            case PlayerCommand.CITY_INFO:
                                HandleCityInfo();
                                break;
                            #endregion

                            #region actual game actions.
                            case PlayerCommand.WAIT_OR_SELF:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = false;
                                DoWait(player);
                                break;

                            case PlayerCommand.WAIT_LONG:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = false;
                                StartPlayerWaitLong(player);
                                break;

                            case PlayerCommand.MOVE_N:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !DoPlayerBump(player, Direction.N);
                                break;
                            case PlayerCommand.MOVE_NE:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !DoPlayerBump(player, Direction.NE);
                                break;
                            case PlayerCommand.MOVE_E:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !DoPlayerBump(player, Direction.E);
                                break;
                            case PlayerCommand.MOVE_SE:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !DoPlayerBump(player, Direction.SE);
                                break;
                            case PlayerCommand.MOVE_S:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !DoPlayerBump(player, Direction.S);
                                break;
                            case PlayerCommand.MOVE_SW:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !DoPlayerBump(player, Direction.SW);
                                break;
                            case PlayerCommand.MOVE_W:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !DoPlayerBump(player, Direction.W);
                                break;
                            case PlayerCommand.MOVE_NW:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !DoPlayerBump(player, Direction.NW);
                                break;
                            case PlayerCommand.USE_EXIT:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !DoUseExit(player, player.Location.Position);
                                break;

                            case PlayerCommand.ITEM_SLOT_0:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !DoPlayerItemSlot(player, 0, inKey);
                                break;
                            case PlayerCommand.ITEM_SLOT_1:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !DoPlayerItemSlot(player, 1, inKey);
                                break;
                            case PlayerCommand.ITEM_SLOT_2:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !DoPlayerItemSlot(player, 2, inKey);
                                break;
                            case PlayerCommand.ITEM_SLOT_3:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !DoPlayerItemSlot(player, 3, inKey);
                                break;
                            case PlayerCommand.ITEM_SLOT_4:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !DoPlayerItemSlot(player, 4, inKey);
                                break;
                            case PlayerCommand.ITEM_SLOT_5:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !DoPlayerItemSlot(player, 5, inKey);
                                break;
                            case PlayerCommand.ITEM_SLOT_6:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !DoPlayerItemSlot(player, 6, inKey);
                                break;
                            case PlayerCommand.ITEM_SLOT_7:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !DoPlayerItemSlot(player, 7, inKey);
                                break;
                            case PlayerCommand.ITEM_SLOT_8:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !DoPlayerItemSlot(player, 8, inKey);
                                break;
                            case PlayerCommand.ITEM_SLOT_9:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !DoPlayerItemSlot(player, 9, inKey);
                                break;

                            case PlayerCommand.RUN_TOGGLE:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                HandlePlayerRunToggle(player);
                                break;

                            case PlayerCommand.CLOSE_DOOR:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !HandlePlayerCloseDoor(player);
                                break;
                            case PlayerCommand.BARRICADE_MODE:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !HandlePlayerBarricade(player);
                                break;
                            case PlayerCommand.BREAK_MODE:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !HandlePlayerBreak(player);
                                break;
                            case PlayerCommand.BUILD_LARGE_FORTIFICATION:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !HandlePlayerBuildFortification(player, true);
                                break;
                            case PlayerCommand.BUILD_SMALL_FORTIFICATION:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !HandlePlayerBuildFortification(player, false);
                                break;
                            case PlayerCommand.ORDER_MODE:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !HandlePlayerOrderMode(player);
                                break;
                            case PlayerCommand.PULL_MODE: // alpha10
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !HandlePlayerPull(player);
                                break;
                            case PlayerCommand.PUSH_MODE:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !HandlePlayerPush(player);
                                break;
                            case PlayerCommand.FIRE_MODE:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !HandlePlayerFireMode(player);
                                break;

                            case PlayerCommand.SHOUT:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !HandlePlayerShout(player, null);
                                break;

                            case PlayerCommand.SLEEP:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !HandlePlayerSleep(player);
                                break;

                            case PlayerCommand.SWITCH_PLACE:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !HandlePlayerSwitchPlace(player);
                                break;

                            case PlayerCommand.USE_SPRAY:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !HandlePlayerUseSpray(player);
                                break;

                            case PlayerCommand.LEAD_MODE:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !HandlePlayerTakeLead(player);
                                break;

                            case PlayerCommand.GIVE_ITEM:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !HandlePlayerGiveItem(player, mousePos);
                                break;

                            case PlayerCommand.NEGOCIATE_TRADE:  // alpha10
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !HandlePlayerNegociateTrade(player); // alpha10
                                break;

                            case PlayerCommand.MARK_ENEMIES_MODE:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                HandlePlayerMarkEnemies(player);
                                break;

                            case PlayerCommand.EAT_CORPSE:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !HandlePlayerEatCorpse(player, mousePos);
                                break;

                            case PlayerCommand.REVIVE_CORPSE:
                                if (TryPlayerInsanity())
                                {
                                    loop = false;
                                    break;
                                }
                                loop = !HandlePlayerReviveCorpse(player, mousePos);
                                break;
                            #endregion

                            case PlayerCommand.NONE:
                                break;

                            default:
                                throw new ArgumentException("command unhandled");
                        }
                    }
                    #endregion
                } // has key
                else
                {
                    ////////////////
                    // Handle mouse
                    ////////////////
                    #region
                    // Look?
                    bool isLooking = HandleMouseLook(mousePos);
                    if (isLooking)
                        continue;

                    // Inventory?
                    bool hasDoneInventoryAction;
                    bool isInventory = HandleMouseInventory(mousePos, mouseButtons, out hasDoneInventoryAction);
                    if (isInventory)
                    {
                        if (hasDoneInventoryAction)
                        {
                            loop = false;
                        }
                        else
                            continue;
                    }

                    // Corpses?
                    bool hasDoneCorpsesAction;
                    bool isCorpses = HandleMouseOverCorpses(mousePos, mouseButtons, out hasDoneCorpsesAction);
                    if (isCorpses)
                    {
                        if (hasDoneCorpsesAction)
                        {
                            loop = false;
                        }
                        else
                            continue;
                    }

                    // Neither look nor inventory nor corpses, cleanup.
                    ClearOverlays();
                    #endregion
                }
            }
            while (loop);

            // Upkeep.
            UpdatePlayerFOV(player);    // make sure LOS is up to date.
            ComputeViewRect(player.Location.Position);
            m_Session.LastTurnPlayerActed = m_Session.WorldTime.TurnCounter;
        }

        bool TryPlayerInsanity()
        {
            if (!m_Rules.IsActorInsane(m_Player))
                return false;
            if (!m_Rules.RollChance(Rules.SANITY_INSANE_ACTION_CHANCE))
                return false;

            ActorAction insaneAction = GenerateInsaneAction(m_Player);
            if (insaneAction == null)
                return false;
            if (!insaneAction.IsLegal())
                return false;

            ClearMessages();
            AddMessage(new Message("(your insanity takes over)", m_Player.Location.Map.LocalTime.TurnCounter, Color.Orange));
            if (!m_Player.IsBotPlayer)
                AddMessagePressEnter();

            insaneAction.Perform();

            return true;
        }

        bool HandleQuitGame()
        {
            AddMessage(MakeYesNoMessage("REALLY QUIT GAME"));
            RedrawPlayScreen();

            bool answer = WaitYesOrNo();

            if (!answer)
                AddMessage(new Message("Good. Keep roguing!", m_Session.WorldTime.TurnCounter, Color.Yellow));
            else
                AddMessage(new Message("Bye!", m_Session.WorldTime.TurnCounter, Color.Yellow));

            return answer;
        }

        bool HandleAbandonGame()
        {
            AddMessage(MakeYesNoMessage("REALLY KILL YOURSELF"));
            RedrawPlayScreen();

            bool answer = WaitYesOrNo();

            if (!answer)
                AddMessage(new Message("Good. No reason to make the undeads life easier by removing yours!", m_Session.WorldTime.TurnCounter, Color.Yellow));
            else
                AddMessage(new Message("You can't bear the horror anymore...", m_Session.WorldTime.TurnCounter, Color.Yellow));

            return answer;
        }

        void HandleScreenshot()
        {
            // prepare.
            AddMessage(new Message("Taking screenshot...", m_Session.WorldTime.TurnCounter, Color.Yellow));
            RedrawPlayScreen();

            // shot it!
            string shotname = DoTakeScreenshot();
            if (shotname == null)
            {
                AddMessage(new Message("Could not save screenshot.", m_Session.WorldTime.TurnCounter, Color.Red));
            }
            else
            {
                AddMessage(new Message(String.Format("screenshot {0} saved.", shotname), m_Session.WorldTime.TurnCounter, Color.Yellow));
            }

            // refresh.
            RedrawPlayScreen();
        }

        string DoTakeScreenshot()
        {
            string shotname = GetUserNewScreenshotName();
            if (m_UI.UI_SaveScreenshot(ScreenshotFilePath(shotname)) != null)
                return shotname;
            else
                return null;
        }

        void HandleHelpMode()
        {
            if (m_Manual == null)
            {
                m_UI.UI_Clear(Color.Black);
                int gy = 0;
                m_UI.UI_DrawStringBold(Color.Red, "Game manual not available ingame.", 0, gy);
                gy += BOLD_LINE_SPACING;
                DrawFootnote(Color.White, "press ENTER");
                m_UI.UI_Repaint();
                WaitEnter();
                return;
            }

            bool loop = true;
            List<string> lines = m_Manual.FormatedLines;
            do
            {
                // draw header.
                m_UI.UI_Clear(Color.Black);
                int gy = 0;
                DrawHeader();
                gy += BOLD_LINE_SPACING;
                m_UI.UI_DrawStringBold(Color.Yellow, "Game Manual", 0, gy);
                gy += BOLD_LINE_SPACING;
                m_UI.UI_DrawStringBold(Color.White, "---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+", 0, gy);
                gy += BOLD_LINE_SPACING;

                // draw manual.
                int iLine = m_ManualLine;
                do
                {
                    // ignore commands
                    bool ignore = (lines[iLine] == "<SECTION>");

                    if (!ignore)
                    {
                        m_UI.UI_DrawStringBold(Color.LightGray, lines[iLine], 0, gy);
                        gy += BOLD_LINE_SPACING;
                    }
                    ++iLine;
                }
                while (iLine < lines.Count && gy < CANVAS_HEIGHT - 2 * BOLD_LINE_SPACING);

                // draw foot.
                m_UI.UI_DrawStringBold(Color.White, "---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+", 0, gy);
                gy += BOLD_LINE_SPACING;
                DrawFootnote(Color.White, "cursor and PgUp/PgDn to move, numbers to jump to section, ESC to leave");

                m_UI.UI_Repaint();

                // get command.
                KeyEventArgs key = m_UI.UI_WaitKey();
                int choice = KeyToChoiceNumber(key.KeyCode);

                if (choice >= 0)
                {
                    if (choice == 0)
                    {
                        m_ManualLine = 0;
                    }
                    else
                    {
                        // jump to Nth section.
                        int prevLine = m_ManualLine;
                        int sectionCount = 0;
                        m_ManualLine = 0;
                        while (sectionCount < choice && m_ManualLine < lines.Count)
                        {
                            if (lines[m_ManualLine] == "<SECTION>")
                            {
                                ++sectionCount;
                            }
                            ++m_ManualLine;
                        }

                        // if section not found, don't move.
                        if (m_ManualLine >= lines.Count)
                        {
                            m_ManualLine = prevLine;
                        }
                    }
                }
                else
                {
                    switch (key.KeyCode)
                    {
                        case Keys.Escape:
                            loop = false;
                            break;

                        case Keys.Up:
                            --m_ManualLine;
                            break;
                        case Keys.Down:
                            ++m_ManualLine;
                            break;
                        case Keys.PageUp:
                            m_ManualLine -= TEXTFILE_LINES_PER_PAGE;
                            break;
                        case Keys.PageDown:
                            m_ManualLine += TEXTFILE_LINES_PER_PAGE;
                            break;
                    }
                }

                if (m_ManualLine < 0) m_ManualLine = 0;
                if (m_ManualLine + TEXTFILE_LINES_PER_PAGE >= lines.Count) m_ManualLine = Math.Max(0, lines.Count - TEXTFILE_LINES_PER_PAGE);
            }
            while (loop);
        }

        #region Hints screen
        void HandleHintsScreen()
        {
            // draw header.
            m_UI.UI_Clear(Color.Black);
            int gy = 0;
            DrawHeader();
            gy += BOLD_LINE_SPACING;
            m_UI.UI_DrawStringBold(Color.Yellow, "Advisor Hints", 0, gy);
            gy += BOLD_LINE_SPACING;

            // prepare : get all the hints text into one huuuuuge list of line :D
            m_UI.UI_DrawStringBold(Color.White, "preparing...", 0, gy);
            gy += BOLD_LINE_SPACING;
            m_UI.UI_Repaint();
            List<string> lines = new List<string>();
            for (int i = (int)AdvisorHint._FIRST; i < (int)AdvisorHint._COUNT; i++)
            {
                string title;
                string[] body;
                GetAdvisorHintText((AdvisorHint)i, out title, out body);
                if (s_Hints.IsAdvisorHintGiven((AdvisorHint)i)) title += " (hint already given)"; // alpha10

                lines.Add(String.Format("HINT {0} : {1}", i, title));
                lines.AddRange(body);
                lines.Add("~~~~");
                lines.Add("");
            }

            // display & handle loop.
            int currentLine = 0;
            bool loop = true;
            do
            {
                // header.
                m_UI.UI_Clear(Color.Black);
                gy = 0;
                DrawHeader();
                gy += BOLD_LINE_SPACING;
                m_UI.UI_DrawStringBold(Color.Yellow, "Advisor Hints", 0, gy);
                gy += BOLD_LINE_SPACING;

                // display currently viewed lines.
                m_UI.UI_DrawStringBold(Color.White, "---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+", 0, gy);
                gy += BOLD_LINE_SPACING;
                int iLine = currentLine;
                do
                {
                    m_UI.UI_DrawStringBold(Color.LightGray, lines[iLine], 0, gy);
                    gy += BOLD_LINE_SPACING;
                    ++iLine;
                }
                while (iLine < lines.Count && gy < CANVAS_HEIGHT - 2 * BOLD_LINE_SPACING);

                // draw foot.
                m_UI.UI_DrawStringBold(Color.White, "---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+", 0, gy);
                gy += BOLD_LINE_SPACING;
                DrawFootnote(Color.White, "cursor and PgUp/PgDn to move, R to reset hints, ESC to leave");

                m_UI.UI_Repaint();

                // get command.
                KeyEventArgs key = m_UI.UI_WaitKey();
                switch (key.KeyCode)
                {
                    case Keys.Escape:
                        loop = false;
                        break;

                    case Keys.Up:
                        --currentLine;
                        break;
                    case Keys.Down:
                        ++currentLine;
                        break;
                    case Keys.PageUp:
                        currentLine -= TEXTFILE_LINES_PER_PAGE;
                        break;
                    case Keys.PageDown:
                        currentLine += TEXTFILE_LINES_PER_PAGE;
                        break;

                    case Keys.R:
                        // do it.
                        s_Hints.ResetAllHints();

                        // notify.
                        m_UI.UI_Clear(Color.Black);
                        gy = 0;
                        DrawHeader();
                        gy += BOLD_LINE_SPACING;
                        m_UI.UI_DrawStringBold(Color.Yellow, "Advisor Hints", 0, gy);
                        gy += BOLD_LINE_SPACING;
                        m_UI.UI_DrawStringBold(Color.White, "Hints reset done.", 0, gy);
                        m_UI.UI_Repaint();
                        m_UI.UI_Wait(DELAY_LONG);
                        break;
                }

                if (currentLine < 0) currentLine = 0;
                if (currentLine + TEXTFILE_LINES_PER_PAGE >= lines.Count) currentLine = Math.Max(0, lines.Count - TEXTFILE_LINES_PER_PAGE);
            }
            while (loop);

        }
        #endregion

        void HandleMessageLog()
        {
            // draw header.
            m_UI.UI_Clear(Color.Black);
            int gy = 0;
            DrawHeader();
            gy += BOLD_LINE_SPACING;
            m_UI.UI_DrawStringBold(Color.Yellow, "Message Log", 0, gy);
            gy += BOLD_LINE_SPACING;
            m_UI.UI_DrawStringBold(Color.White, "---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+", 0, gy);
            gy += BOLD_LINE_SPACING;

            // log.
            foreach (Message msg in m_MessageManager.History)
            {
                m_UI.UI_DrawString(msg.Color, msg.Text, 0, gy);
                gy += LINE_SPACING;
            }

            // foot.
            DrawFootnote(Color.White, "press ESC to leave");

            // wait.
            m_UI.UI_Repaint();
            WaitEscape();
        }

        void HandleCityInfo()
        {
            int gx, gy;

            gx = gy = 0;
            m_UI.UI_Clear(Color.Black);
            m_UI.UI_DrawStringBold(Color.White, "CITY INFORMATION", gy, gy);
            gy += 2 * BOLD_LINE_SPACING;

            /////////////////////
            // Undead : no info!
            // Living : normal.
            /////////////////////
            if (m_Player.Model.Abilities.IsUndead)
            {
                #region Undead : no info
                m_UI.UI_DrawStringBold(Color.Red, "You can't remember where you are...", gx, gy);
                gy += BOLD_LINE_SPACING;
                m_UI.UI_DrawStringBold(Color.Red, "Must be that rotting brain of yours...", gx, gy);
                gy += 2 * BOLD_LINE_SPACING;
                #endregion
            }
            else
            {
                #region Living : show info
                ////////////
                // City map
                ////////////
                #region
                m_UI.UI_DrawStringBold(Color.White, "> DISTRICTS LAYOUT", gx, gy);
                gy += BOLD_LINE_SPACING;

                // coordinates.
                gy += BOLD_LINE_SPACING;
                for (int y = 0; y < m_Session.World.Size; y++)
                {
                    Color color = (y == m_Player.Location.Map.District.WorldPosition.Y ? Color.LightGreen : Color.White);
                    m_UI.UI_DrawStringBold(color, y.ToString(), 20, gy + y * 3 * BOLD_LINE_SPACING + BOLD_LINE_SPACING);
                    m_UI.UI_DrawStringBold(color, ".", 20, gy + y * 3 * BOLD_LINE_SPACING);
                    m_UI.UI_DrawStringBold(color, ".", 20, gy + y * 3 * BOLD_LINE_SPACING + 2 * BOLD_LINE_SPACING);
                }
                gy -= BOLD_LINE_SPACING;
                for (int x = 0; x < m_Session.World.Size; x++)
                {
                    Color color = (x == m_Player.Location.Map.District.WorldPosition.X ? Color.LightGreen : Color.White);
                    m_UI.UI_DrawStringBold(color, String.Format("..{0}..", (char)('A' + x)), 32 + x * 48, gy);
                }
                // districts.
                gy += BOLD_LINE_SPACING;
                int mx = 32;
                int my = gy;
                for (int y = 0; y < m_Session.World.Size; y++)
                    for (int x = 0; x < m_Session.World.Size; x++)
                    {
                        District d = m_Session.World[x, y];
                        char dStatus = d == m_Session.CurrentMap.District ? '*' : m_Session.Scoring.HasVisited(d.EntryMap) ? '-' : '?';
                        Color dColor;
                        string dChar;
                        switch (d.Kind)
                        {
                            case DistrictKind.BUSINESS: dColor = Color.Red; dChar = "Bus"; break;
                            case DistrictKind.GENERAL: dColor = Color.Gray; dChar = "Gen"; break;
                            case DistrictKind.GREEN: dColor = Color.Green; dChar = "Gre"; break;
                            case DistrictKind.RESIDENTIAL: dColor = Color.Orange; dChar = "Res"; break;
                            case DistrictKind.SHOPPING: dColor = Color.White; dChar = "Sho"; break;
                            default:
                                throw new ArgumentOutOfRangeException("unhandled district kind");
                        }

                        string lchar = "";
                        for (int i = 0; i < 5; i++)
                            lchar += dStatus;
                        Color lColor = (d == m_Player.Location.Map.District ? Color.LightGreen : dColor);

                        m_UI.UI_DrawStringBold(lColor, lchar, mx + x * 48, my + (y * 3) * BOLD_LINE_SPACING);
                        m_UI.UI_DrawStringBold(lColor, dStatus.ToString(), mx + x * 48, my + (y * 3 + 1) * BOLD_LINE_SPACING);
                        m_UI.UI_DrawStringBold(dColor, dChar, mx + x * 48 + 8, my + (y * 3 + 1) * BOLD_LINE_SPACING);
                        m_UI.UI_DrawStringBold(lColor, dStatus.ToString(), mx + x * 48 + 4 * 8, my + (y * 3 + 1) * BOLD_LINE_SPACING);
                        m_UI.UI_DrawStringBold(lColor, lchar, mx + x * 48, my + (y * 3 + 2) * BOLD_LINE_SPACING);
                    }
                // subway line.
                const string subwayChar = "=";
                int subwayY = m_Session.World.Size / 2;
                for (int x = 1; x < m_Session.World.Size; x++)
                {
                    m_UI.UI_DrawStringBold(Color.White, subwayChar, mx + x * 48 - 8, my + (subwayY * 3) * BOLD_LINE_SPACING + BOLD_LINE_SPACING);
                }

                gy += (m_Session.World.Size * 3 + 1) * BOLD_LINE_SPACING;
                m_UI.UI_DrawStringBold(Color.White, "Legend", gx, gy);
                gy += BOLD_LINE_SPACING;
                m_UI.UI_DrawString(Color.White, "  *   - current     ?   - unvisited", gx, gy);
                gy += LINE_SPACING;
                m_UI.UI_DrawString(Color.White, "  Bus - Business    Gen - General    Gre - Green", gx, gy);
                gy += LINE_SPACING;
                m_UI.UI_DrawString(Color.White, "  Res - Residential Sho - Shopping", gx, gy);
                gy += LINE_SPACING;
                m_UI.UI_DrawString(Color.White, "  =   - Subway Line", gx, gy);
                gy += LINE_SPACING;
                #endregion

                /////////////////////
                // Notable locations
                /////////////////////
                #region
                gy += BOLD_LINE_SPACING;
                m_UI.UI_DrawStringBold(Color.White, "> NOTABLE LOCATIONS", gx, gy);
                gy += BOLD_LINE_SPACING;
                int buildingsY = gy;
                for (int y = 0; y < m_Session.World.Size; y++)
                    for (int x = 0; x < m_Session.World.Size; x++)
                    {
                        District d = m_Session.World[x, y];
                        Map map = d.EntryMap;

                        // Subway station?
                        Zone subwayZone;
                        if ((subwayZone = map.GetZoneByPartialName(NAME_SUBWAY_STATION)) != null)
                        {
                            m_UI.UI_DrawStringBold(Color.Blue, String.Format("at {0} : {1}.", World.CoordToString(x, y), subwayZone.Name), gx, gy);
                            gy += BOLD_LINE_SPACING;
                            if (gy >= CANVAS_HEIGHT - 2 * BOLD_LINE_SPACING)
                            {
                                gy = buildingsY;
                                gx += 25 * BOLD_LINE_SPACING;
                            }

                        }

                        // Sewers maintenance?
                        // alpha10 removed
                        //Zone sewersZone;
                        //if ((sewersZone = map.GetZoneByPartialName(NAME_SEWERS_MAINTENANCE)) != null)
                        //{
                        //    m_UI.UI_DrawStringBold(Color.Green, String.Format("at {0} : {1}.", World.CoordToString(x, y), sewersZone.Name), gx, gy);
                        //    gy += BOLD_LINE_SPACING;
                        //    if (gy >= CANVAS_HEIGHT - 2 * BOLD_LINE_SPACING)
                        //    {
                        //        gy = buildingsY;
                        //        gx += 25 * BOLD_LINE_SPACING;
                        //    }
                        //}

                        // Police station?
                        if (map == m_Session.UniqueMaps.PoliceStation_OfficesLevel.TheMap.District.EntryMap)
                        {
                            m_UI.UI_DrawStringBold(Color.CadetBlue, String.Format("at {0} : Police Station.", World.CoordToString(x, y)), gx, gy);
                            gy += BOLD_LINE_SPACING;
                            if (gy >= CANVAS_HEIGHT - 2 * BOLD_LINE_SPACING)
                            {
                                gy = buildingsY;
                                gx += 25 * BOLD_LINE_SPACING;
                            }
                        }

                        // Hospital?
                        if (map == m_Session.UniqueMaps.Hospital_Admissions.TheMap.District.EntryMap)
                        {
                            m_UI.UI_DrawStringBold(Color.White, String.Format("at {0} : Hospital.", World.CoordToString(x, y)), gx, gy);
                            gy += BOLD_LINE_SPACING;
                            if (gy >= CANVAS_HEIGHT - 2 * BOLD_LINE_SPACING)
                            {
                                gy = buildingsY;
                                gx += 25 * BOLD_LINE_SPACING;
                            }
                        }

                        // Secrets
                        // - CHAR Underground Facility?
                        if (m_Session.PlayerKnows_CHARUndergroundFacilityLocation && map == m_Session.UniqueMaps.CHARUndergroundFacility.TheMap.District.EntryMap)
                        {
                            m_UI.UI_DrawStringBold(Color.Red, String.Format("at {0} : {1}.", World.CoordToString(x, y), m_Session.UniqueMaps.CHARUndergroundFacility.TheMap.Name), gx, gy);
                            gy += BOLD_LINE_SPACING;
                            if (gy >= CANVAS_HEIGHT - 2 * BOLD_LINE_SPACING)
                            {
                                gy = buildingsY;
                                gx += 25 * BOLD_LINE_SPACING;
                            }
                        }
                        // - The Sewers Thing?
                        if (m_Session.PlayerKnows_TheSewersThingLocation &&
                            map == m_Session.UniqueActors.TheSewersThing.TheActor.Location.Map.District.EntryMap &&
                            !m_Session.UniqueActors.TheSewersThing.TheActor.IsDead)
                        {
                            m_UI.UI_DrawStringBold(Color.Red, String.Format("at {0} : The Sewers Thing lives down there.", World.CoordToString(x, y)), gx, gy);
                            gy += BOLD_LINE_SPACING;
                            if (gy >= CANVAS_HEIGHT - 2 * BOLD_LINE_SPACING)
                            {
                                gy = buildingsY;
                                gx += 25 * BOLD_LINE_SPACING;
                            }
                        }
                    }
                #endregion
                #endregion
            }

            DrawFootnote(Color.White, "press ESC to leave");
            m_UI.UI_Repaint();
            WaitEscape();
        }

        #region Ingame actions modes
        bool HandleMouseLook(Point mousePos)
        {
            // Ignore if out of view rect.
            Point mouseMap = MouseToMap(mousePos);
            if (!IsInViewRect(mouseMap))
                return false;

            // Nothing to do if out of map, but still handle the mouse.
            if (!m_Session.CurrentMap.IsInBounds(mouseMap))
                return true;

            // Do look.
            ClearOverlays();
            if (IsVisibleToPlayer(m_Session.CurrentMap, mouseMap))
            {
                Point tileScreenPos = MapToScreen(mouseMap);
                string[] description = DescribeStuffAt(m_Session.CurrentMap, mouseMap);
                if (description != null)
                {
                    Point popupPos = new Point(tileScreenPos.X + TILE_SIZE, tileScreenPos.Y);
                    AddOverlay(new OverlayPopup(description, Color.White, Color.White, POPUP_FILLCOLOR, popupPos));
                    if (s_Options.ShowTargets)
                    {
                        Actor actorThere = m_Session.CurrentMap.GetActorAt(mouseMap);
                        if (actorThere != null)
                            DrawActorRelations(actorThere);
                    }
                }
            }

            // Handled mouse.
            return true;
        }

        bool HandleMouseInventory(Point mousePos, MouseButtons? mouseButtons, out bool hasDoneAction)
        {
            // Ignore if not on an inventory slot.
            Inventory inv;
            Point itemPos;
            int iSlot;
            Item it = MouseToInventoryItem(mousePos, out inv, out itemPos, out iSlot);
            if (inv == null)
            {
                hasDoneAction = false;
                return false;
            }

            // Do inventory stuff.
            bool isPlayerInventory = (inv == m_Player.Inventory);
            hasDoneAction = false;
            ClearOverlays();
            AddOverlay(new OverlayRect(Color.Cyan, new Rectangle(itemPos.X, itemPos.Y, 32, 32)));
            AddOverlay(new OverlayRect(Color.Cyan, new Rectangle(itemPos.X + 1, itemPos.Y + 1, 30, 30)));
            if (it != null)
            {
                string[] lines = DescribeItemLong(it, isPlayerInventory, iSlot);
                int longestLine = 1 + FindLongestLine(lines);
                int ovX = itemPos.X - 7 * longestLine;
                int ovY = itemPos.Y + 32;

                AddOverlay(new OverlayPopup(lines, Color.White, Color.White, POPUP_FILLCOLOR, new Point(ovX, ovY)));

                // item action?
                if (mouseButtons.HasValue)
                {
                    if (mouseButtons == MouseButtons.Left)
                        hasDoneAction = OnLMBItem(inv, it);
                    else if (mouseButtons == MouseButtons.Right)
                        hasDoneAction = OnRMBItem(inv, it);
                }
            }

            // Handled mouse.
            return true;
        }

        Item MouseToInventoryItem(Point screen, out Inventory inv, out Point itemPos, out int iSlot)
        {
            inv = null;
            itemPos = Point.Empty;
            iSlot = -1; // alpha10

            if (m_Player == null)
                return null;

            Inventory playerInv = m_Player.Inventory;
            Point playerSlot = MouseToInventorySlot(INVENTORYPANEL_X, INVENTORYPANEL_Y, screen.X, screen.Y);
            int playerItemIndex = playerSlot.X + playerSlot.Y * INVENTORY_SLOTS_PER_LINE;
            if (playerItemIndex >= 0 && playerItemIndex < playerInv.MaxCapacity)
            {
                inv = playerInv;
                itemPos = InventorySlotToScreen(INVENTORYPANEL_X, INVENTORYPANEL_Y, playerSlot.X, playerSlot.Y);
                iSlot = playerItemIndex; // alpha10
                return playerInv[playerItemIndex];
            }

            Inventory groundInv = m_Player.Location.Map.GetItemsAt(m_Player.Location.Position);
            Point groundSlot = MouseToInventorySlot(INVENTORYPANEL_X, GROUNDINVENTORYPANEL_Y, screen.X, screen.Y);
            itemPos = InventorySlotToScreen(INVENTORYPANEL_X, GROUNDINVENTORYPANEL_Y, groundSlot.X, groundSlot.Y);
            if (groundInv == null)
                return null;
            int groundItemIndex = groundSlot.X + groundSlot.Y * INVENTORY_SLOTS_PER_LINE;
            if (groundItemIndex >= 0 && groundItemIndex < groundInv.MaxCapacity)
            {
                inv = groundInv;
                iSlot = groundItemIndex; // alpha10
                return groundInv[groundItemIndex];
            }

            return null;
        }

        bool OnLMBItem(Inventory inv, Item it)
        {
            if (inv == m_Player.Inventory)
            {
                // LMB in player inv = use/equip toggle
                if (it.IsEquipped)
                {
                    string reason;
                    if (m_Rules.CanActorUnequipItem(m_Player, it, out reason))
                    {
                        DoUnequipItem(m_Player, it);
                        return false;
                    }
                    else
                    {
                        AddMessage(MakeErrorMessage(String.Format("Cannot unequip {0} : {1}.", it.TheName, reason)));
                        return false;
                    }
                }
                else if (it.Model.IsEquipable)
                {
                    string reason;
                    if (m_Rules.CanActorEquipItem(m_Player, it, out reason))
                    {
                        DoEquipItem(m_Player, it);
                        return false;
                    }
                    else
                    {
                        AddMessage(MakeErrorMessage(String.Format("Cannot equip {0} : {1}.", it.TheName, reason)));
                        return false;
                    }
                }
                else
                {
                    // try to use item.
                    string reason;
                    if (m_Rules.CanActorUseItem(m_Player, it, out reason))
                    {
                        DoUseItem(m_Player, it);
                        return true;
                    }
                    else
                    {
                        AddMessage(MakeErrorMessage(String.Format("Cannot use {0} : {1}.", it.TheName, reason)));
                    }
                }
            }
            else // ground inventory
            {
                // LMB in ground inv = take
                string reason;
                if (m_Rules.CanActorGetItem(m_Player, it, out reason))
                {
                    DoTakeItem(m_Player, m_Player.Location.Position, it);
                    return true;
                }
                else
                {
                    AddMessage(MakeErrorMessage(String.Format("Cannot take {0} : {1}.", it.TheName, reason)));
                    return false;
                }
            }

            return false;
        }

        bool OnRMBItem(Inventory inv, Item it)
        {
            if (inv == m_Player.Inventory)
            {
                string reason;
                if (m_Rules.CanActorDropItem(m_Player, it, out reason))
                {
                    DoDropItem(m_Player, it);
                    return true;
                }
                else
                {
                    AddMessage(MakeErrorMessage(String.Format("Cannot drop {0} : {1}.", it.TheName, reason)));
                    return false;
                }
            }

            return false;
        }

        #region Corpses
        bool HandleMouseOverCorpses(Point mousePos, MouseButtons? mouseButtons, out bool hasDoneAction)
        {
            // Ignore if not on a corpse slot.
            Point corpsePos;
            Corpse corpse = MouseToCorpse(mousePos, out corpsePos);
            if (corpse == null)
            {
                hasDoneAction = false;
                return false;
            }

            // Do corpse stuff.
            hasDoneAction = false;
            ClearOverlays();
            AddOverlay(new OverlayRect(Color.Cyan, new Rectangle(corpsePos.X, corpsePos.Y, 32, 32)));
            AddOverlay(new OverlayRect(Color.Cyan, new Rectangle(corpsePos.X + 1, corpsePos.Y + 1, 30, 30)));
            if (corpse != null)
            {
                string[] lines = DescribeCorpseLong(corpse, true);
                int longestLine = 1 + FindLongestLine(lines);
                int ovX = corpsePos.X - 7 * longestLine;
                int ovY = corpsePos.Y + 32;

                AddOverlay(new OverlayPopup(lines, Color.White, Color.White, POPUP_FILLCOLOR, new Point(ovX, ovY)));

                // mouse action?
                if (mouseButtons.HasValue)
                {
                    if (mouseButtons == MouseButtons.Left)
                        hasDoneAction = OnLMBCorpse(corpse);
                    else if (mouseButtons == MouseButtons.Right)
                        hasDoneAction = OnRMBCorpse(corpse);
                }
            }

            // Handled mouse.
            return true;
        }

        Corpse MouseToCorpse(Point screen, out Point corpsePos)
        {
            corpsePos = Point.Empty;

            if (m_Player == null)
                return null;

            List<Corpse> corpsesList = m_Player.Location.Map.GetCorpsesAt(m_Player.Location.Position);
            if (corpsesList == null)
                return null;
            Point corpseSlot = MouseToInventorySlot(INVENTORYPANEL_X, CORPSESPANEL_Y, screen.X, screen.Y);
            corpsePos = InventorySlotToScreen(INVENTORYPANEL_X, CORPSESPANEL_Y, corpseSlot.X, corpseSlot.Y);
            int corpseIndex = corpseSlot.X + corpseSlot.Y * INVENTORY_SLOTS_PER_LINE;
            if (corpseIndex >= 0 && corpseIndex < corpsesList.Count)
                return corpsesList[corpseIndex];

            return null;
        }

        bool OnLMBCorpse(Corpse c)
        {
            if (c.IsDragged)
            {
                string reason;
                if (m_Rules.CanActorStopDragCorpse(m_Player, c, out reason))
                {
                    DoStopDragCorpse(m_Player, c);
                    return false;
                }
                else
                {
                    AddMessage(MakeErrorMessage(String.Format("Cannot stop dragging {0} corpse : {1}.", c.DeadGuy.Name, reason)));
                    return false;
                }
            }
            else
            {
                string reason;
                if (m_Rules.CanActorStartDragCorpse(m_Player, c, out reason))
                {
                    DoStartDragCorpse(m_Player, c);
                    return false;
                }
                else
                {
                    AddMessage(MakeErrorMessage(String.Format("Cannot start dragging {0} corpse : {1}.", c.DeadGuy.Name, reason)));
                    return false;
                }
            }
        }

        bool OnRMBCorpse(Corpse c)
        {
            string reason;
            if (m_Player.Model.Abilities.IsUndead)
            {
                if (m_Rules.CanActorEatCorpse(m_Player, c, out reason))
                {
                    DoEatCorpse(m_Player, c);
                    return true;
                }
                else
                {
                    AddMessage(MakeErrorMessage(String.Format("Cannot eat {0} corpse : {1}.", c.DeadGuy.Name, reason)));
                    return false;
                }
            }
            else
            {
                if (m_Rules.CanActorButcherCorpse(m_Player, c, out reason))
                {
                    DoButcherCorpse(m_Player, c);
                    return true;
                }
                else
                {
                    AddMessage(MakeErrorMessage(String.Format("Cannot butcher {0} corpse : {1}.", c.DeadGuy.Name, reason)));
                    return false;
                }
            }
        }

        bool HandlePlayerEatCorpse(Actor player, Point mousePos)
        {
            // Ignore if not on a corpse slot.
            Point corpsePos;
            Corpse corpse = MouseToCorpse(mousePos, out corpsePos);
            if (corpse == null)
                return false;

            // Check legality.
            string reason;
            if (!m_Rules.CanActorEatCorpse(player, corpse, out reason))
            {
                AddMessage(MakeErrorMessage(String.Format("Cannot eat {0} corpse : {1}.", corpse.DeadGuy.Name, reason)));
                return false;
            }

            // Do it.
            DoEatCorpse(player, corpse);
            return true;
        }

        bool HandlePlayerReviveCorpse(Actor player, Point mousePos)
        {
            // Ignore if not on a corpse slot.
            Point corpsePos;
            Corpse corpse = MouseToCorpse(mousePos, out corpsePos);
            if (corpse == null)
                return false;

            // Check legality.
            string reason;
            if (!m_Rules.CanActorReviveCorpse(player, corpse, out reason))
            {
                AddMessage(MakeErrorMessage(String.Format("Cannot revive {0} : {1}.", corpse.DeadGuy.Name, reason)));
                return false;
            }

            // Do it.
            DoReviveCorpse(player, corpse);
            return true;
        }

        public void DoStartDragCorpse(Actor a, Corpse c)
        {
            c.DraggedBy = a;
            a.DraggedCorpse = c;
            if (IsVisibleToPlayer(a))
                AddMessage(MakeMessage(a, String.Format("{0} dragging {1} corpse.", Conjugate(a, VERB_START), c.DeadGuy.Name)));
        }

        public void DoStopDragCorpse(Actor a, Corpse c)
        {
            c.DraggedBy = null;
            a.DraggedCorpse = null;
            if (IsVisibleToPlayer(a))
                AddMessage(MakeMessage(a, String.Format("{0} dragging {1} corpse.", Conjugate(a, VERB_STOP), c.DeadGuy.Name)));
        }

        public void DoStopDraggingCorpses(Actor a)
        {
            if (a.DraggedCorpse != null)
            {
                DoStopDragCorpse(a, a.DraggedCorpse);
            }
        }

        public void DoButcherCorpse(Actor a, Corpse c)
        {
            bool isVisible = IsVisibleToPlayer(a);

            // spend ap.
            SpendActorActionPoints(a, Rules.BASE_ACTION_COST);

            // cause insanity.
            SeeingCauseInsanity(a, a.Location, Rules.SANITY_HIT_BUTCHERING_CORPSE, String.Format("{0} butchering {1}", a.Name, c.DeadGuy.Name));

            // damage.
            int dmg = m_Rules.ActorDamageVsCorpses(a);

            if (isVisible)
                AddMessage(MakeMessage(a, String.Format("{0} {1} corpse for {2} damage.", Conjugate(a, VERB_BUTCHER), c.DeadGuy.Name, dmg)));

            InflictDamageToCorpse(c, dmg);

            // destroy?
            if (c.HitPoints <= 0)
            {
                DestroyCorpse(c, a.Location.Map);
                if (isVisible)
                    AddMessage(new Message(String.Format("{0} corpse is no more.", c.DeadGuy.Name), a.Location.Map.LocalTime.TurnCounter, Color.Purple));
            }
        }

        public void DoEatCorpse(Actor a, Corpse c)
        {
            bool isVisible = IsVisibleToPlayer(a);

            // spend ap.
            SpendActorActionPoints(a, Rules.BASE_ACTION_COST);

            // damage.
            int dmg = m_Rules.ActorDamageVsCorpses(a);

            // msg.
            if (isVisible)
            {
                AddMessage(MakeMessage(a, String.Format("{0} {1} corpse.", Conjugate(a, VERB_FEAST_ON), c.DeadGuy.Name, dmg)));
                // alpha10 replace with sfx
                m_MusicManager.Stop();
                m_MusicManager.Play(GameSounds.UNDEAD_EAT, MusicPriority.PRIORITY_EVENT);
            }

            // dmh corpse.
            InflictDamageToCorpse(c, dmg);

            // destroy?
            if (c.HitPoints <= 0)
            {
                DestroyCorpse(c, a.Location.Map);
                if (isVisible)
                    AddMessage(new Message(String.Format("{0} corpse is no more.", c.DeadGuy.Name), a.Location.Map.LocalTime.TurnCounter, Color.Purple));
            }

            // heal if undead / food.
            if (a.Model.Abilities.IsUndead)
            {
                RegenActorHitPoints(a, Rules.ActorBiteHpRegen(a, dmg));
                a.FoodPoints = Math.Min(a.FoodPoints + m_Rules.ActorBiteNutritionValue(a, dmg), m_Rules.ActorMaxRot(a));
            }
            else
            {
                // recover food points.
                a.FoodPoints = Math.Min(a.FoodPoints + m_Rules.ActorBiteNutritionValue(a, dmg), m_Rules.ActorMaxFood(a));
                // infection!
                InfectActor(a, m_Rules.CorpseEeatingInfectionTransmission(c.DeadGuy.Infection));
            }

            // cause insanity.
            SeeingCauseInsanity(a, a.Location, a.Model.Abilities.IsUndead ? Rules.SANITY_HIT_UNDEAD_EATING_CORPSE : Rules.SANITY_HIT_LIVING_EATING_CORPSE,
                String.Format("{0} eating {1}", a.Name, c.DeadGuy.Name));
        }

        public void DoReviveCorpse(Actor actor, Corpse corpse)
        {
            bool visible = IsVisibleToPlayer(actor);

            // spend ap.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // make sure there is a walkable spot for revival.
            Map map = actor.Location.Map;
            List<Point> revivePoints = actor.Location.Map.FilterAdjacentInMap(actor.Location.Position,
                (pt) =>
                {
                    if (map.GetActorAt(pt) != null) return false;
                    if (map.GetMapObjectAt(pt) != null) return false;
                    return true;
                });
            if (revivePoints == null)
            {
                if (visible)
                    AddMessage(MakeMessage(actor, String.Format("{0} not enough room for reviving {1}.", Conjugate(actor, VERB_HAVE), corpse.DeadGuy.Name)));
                return;
            }
            Point revivePt = revivePoints[m_Rules.Roll(0, revivePoints.Count)];

            // spend medikit.
            Item medikit = actor.Inventory.GetSmallestStackByModel(GameItems.MEDIKIT);  // alpha10
                                                                                        //actor.Inventory.GetFirstMatching((it) => it.Model == GameItems.MEDIKIT);
            actor.Inventory.Consume(medikit);

            // try.
            int chance = m_Rules.CorpseReviveChance(actor, corpse);
            if (m_Rules.RollChance(chance))
            {
                // do it.
                corpse.DeadGuy.IsDead = false;
                corpse.DeadGuy.HitPoints = m_Rules.CorpseReviveHPs(actor, corpse);
                corpse.DeadGuy.Doll.RemoveDecoration(GameImages.BLOODIED);
                corpse.DeadGuy.Activity = Activity.IDLE;
                corpse.DeadGuy.TargetActor = null;
                map.RemoveCorpse(corpse);
                map.PlaceActorAt(corpse.DeadGuy, revivePt);
                // msg.
                if (visible)
                    AddMessage(MakeMessage(actor, Conjugate(actor, VERB_REVIVE), corpse.DeadGuy));
                // thank you... or not?
                if (!m_Rules.AreEnemies(actor, corpse.DeadGuy))
                    DoSay(corpse.DeadGuy, actor, "Thank you, you saved my life!", Sayflags.NONE);
            }
            else
            {
                // msg.
                if (visible)
                    AddMessage(MakeMessage(actor, String.Format("{0} to revive", Conjugate(actor, VERB_FAIL)), corpse.DeadGuy));
            }
        }

        void InflictDamageToCorpse(Corpse c, float dmg)
        {
            c.HitPoints -= dmg;
        }

        void DestroyCorpse(Corpse c, Map m)
        {
            if (c.DraggedBy != null)
            {
                c.DraggedBy.DraggedCorpse = null;
                c.DraggedBy = null;
            }
            m.RemoveCorpse(c);
        }
        #endregion

        bool DoPlayerItemSlot(Actor player, int slot, KeyEventArgs key)
        {
            // get key modifier and redirect to proper action.
            // Ctrl  -> equip/unequip/use item from player inv
            // Shift -> take item from ground inv
            // Alt -> drop item from player inv.
            if ((key.Modifiers & Keys.Control) != 0)
                return DoPlayerItemSlotUse(player, slot);
            else if (key.Shift)
                return DoPlayerItemSlotTake(player, slot);
            else if (key.Alt)
                return DoPlayerItemSlotDrop(player, slot);

            // nope.
            return false;
        }

        bool DoPlayerItemSlotUse(Actor player, int slot)
        {
            Inventory inv = player.Inventory;
            Item it = inv[slot];

            // if no item, nothing to do.
            if (it == null)
            {
                AddMessage(MakeErrorMessage(String.Format("No item at inventory slot {0}.", (slot + 1))));
                return false;
            }

            // ty to unequip/equip/use. 
            // shameful copy of OnLMBItem.
            // shame on me.
            if (it.IsEquipped)
            {
                string reason;
                if (m_Rules.CanActorUnequipItem(player, it, out reason))
                {
                    DoUnequipItem(player, it);
                    return false;
                }
                else
                {
                    AddMessage(MakeErrorMessage(String.Format("Cannot unequip {0} : {1}.", it.TheName, reason)));
                    return false;
                }
            }
            else if (it.Model.IsEquipable)
            {
                string reason;
                if (m_Rules.CanActorEquipItem(player, it, out reason))
                {
                    DoEquipItem(player, it);
                    return false;
                }
                else
                {
                    AddMessage(MakeErrorMessage(String.Format("Cannot equip {0} : {1}.", it.TheName, reason)));
                    return false;
                }
            }
            else
            {
                // try to use item.
                string reason;
                if (m_Rules.CanActorUseItem(player, it, out reason))
                {
                    DoUseItem(player, it);
                    return true;
                }
                else
                {
                    AddMessage(MakeErrorMessage(String.Format("Cannot use {0} : {1}.", it.TheName, reason)));
                }
            }

            // nothing done.
            return false;
        }

        bool DoPlayerItemSlotTake(Actor player, int slot)
        {
            Inventory inv = player.Location.Map.GetItemsAt(player.Location.Position);

            // if no items on ground, nothing to do.
            if (inv == null || inv.IsEmpty)
            {
                AddMessage(MakeErrorMessage("No items on ground."));
                return false;
            }

            // if no item, nothing to do.
            Item it = inv[slot];
            if (it == null)
            {
                AddMessage(MakeErrorMessage(String.Format("No item at ground slot {0}.", (slot + 1))));
                return false;
            }

            // try to take.
            string reason;
            if (m_Rules.CanActorGetItem(player, it, out reason))
            {
                DoTakeItem(player, player.Location.Position, it);
                return true;
            }
            else
            {
                AddMessage(MakeErrorMessage(String.Format("Cannot take {0} : {1}.", it.TheName, reason)));
                return false;
            }
        }

        bool DoPlayerItemSlotDrop(Actor player, int slot)
        {
            Inventory inv = player.Inventory;
            Item it = inv[slot];

            // if no item, nothing to do.
            if (it == null)
            {
                AddMessage(MakeErrorMessage(String.Format("No item at inventory slot {0}.", (slot + 1))));
                return false;
            }

            // try to drop.
            string reason;
            if (m_Rules.CanActorDropItem(player, it, out reason))
            {
                DoDropItem(player, it);
                return true;
            }
            else
            {
                AddMessage(MakeErrorMessage(String.Format("Cannot drop {0} : {1}.", it.TheName, reason)));
                return false;
            }
        }

        bool HandlePlayerShout(Actor player, string text)
        {
            string reason;
            if (!m_Rules.CanActorShout(player, out reason))
            {
                AddMessage(MakeErrorMessage(String.Format("Can't shout : {0}.", reason)));
                return false;
            }

            DoShout(player, text);
            return true;
        }

        bool HandlePlayerGiveItem(Actor player, Point screen)
        {
            // get player inventory item under mouse.
            Inventory inv;
            Point itemPos;
            int iSlot;
            Item gift = MouseToInventoryItem(screen, out inv, out itemPos, out iSlot);
            if (inv == null || inv != player.Inventory || gift == null)
                return false;

            // handle give item.
            bool loop = true;
            bool actionDone = false;
            ClearOverlays();
            AddOverlay(new OverlayPopup(GIVE_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
            do
            {
                ///////////////////
                // 1. Redraw
                // 2. Get input.
                // 3. Handle input
                ///////////////////

                // 1. Redraw
                AddMessage(new Message(String.Format("Giving {0} to...", gift.TheName), m_Session.WorldTime.TurnCounter, Color.Yellow));
                RedrawPlayScreen();

                // 2. Get input.
                Direction dir = WaitDirectionOrCancel();

                // 3. Handle input
                if (dir == null)
                {
                    loop = false;
                }
                else if (dir != Direction.NEUTRAL)
                {
                    Point pos = player.Location.Position + dir;
                    if (player.Location.Map.IsInBounds(pos))
                    {
                        Actor other = player.Location.Map.GetActorAt(pos);
                        if (other != null)
                        {
                            string reason;
                            if (m_Rules.CanActorGiveItemTo(player, other, gift, out reason))
                            {
                                // do it.
                                actionDone = true;
                                loop = false;
                                DoGiveItemTo(player, other, gift);
                            }
                            else
                            {
                                AddMessage(MakeErrorMessage(String.Format("Can't give {0} to {1} : {2}.", gift.TheName, other.TheName, reason)));
                            }
                        }
                        else
                            AddMessage(MakeErrorMessage("Noone there."));
                    }
                }
            }
            while (loop);

            // cleanup.
            ClearOverlays();

            // return if we did an action.
            return actionDone;
        }

        // alpha10 new trade window dialog
        bool HandlePlayerTradeNegociation(Actor player, Actor npc)
        {
            BaseAI npcAI = npc.Controller as BaseAI;
            bool isOnPlayerInventory = true;
            int iPlayerSelectedItem = -1;
            int iNpcSelectedItem = -1;
            int state = 0; // 0 selecting 1st item; 1 selecting 2nd item; 2 making the offer

            // pre-compute all possible trade deals ratings
            TradeRating[,] ratingPairs = new TradeRating[player.Inventory.CountItems, npc.Inventory.CountItems];
            for (int i = 0; i < player.Inventory.CountItems; i++)
            {
                Item offered = player.Inventory[i];
                for (int j = 0; j < npc.Inventory.CountItems; j++)
                    ratingPairs[i, j] = npcAI.RateTradeOffer(this, player, offered, npc.Inventory[j]);
            }

            // roll charisma to later accept or refuse "maybe" deal.
            int charismaChance = m_Rules.ActorCharismaticTradeChance(player);
            bool charismaSuccess = m_Session.Player_TurnCharismaRoll < charismaChance;

            // remember if player is trusted leader to notify him.
            bool isTrustedLeader = (npc.Leader == player) && m_Rules.IsActorTrustingLeader(npc);

            // loop
            bool loop = true;
            bool actionDone = false;
            List<String> lines = new List<string>();
            List<Color> colors = new List<Color>();

            Color tradeToColor(TradeRating r)
            {
                if (r == TradeRating.ACCEPT) return TRADE_COLOR_ACCEPT;
                if (r == TradeRating.REFUSE) return TRADE_COLOR_REFUSE;
                if (charismaSuccess) return TRADE_COLOR_MAYBE_SUCCESS;
                return TRADE_COLOR_MAYBE_FAILED;
            };

            do
            {
                ///////////////////
                // 1. Redraw
                // 2. Handle state
                ///////////////////

                // 1. Redraw

                lines.Clear();
                colors.Clear();

                if (state == 2)
                {
                    lines.Add("Mode: Making the offer");
                }
                else
                {
                    if (isOnPlayerInventory)
                    {
                        if (state == 0)
                            lines.Add("Mode: Proposing an item");
                        else
                            lines.Add("Mode: Selecting your item to exhange");
                    }
                    else
                    {
                        if (state == 0)
                            lines.Add("Mode: Asking for an item");
                        else
                            lines.Add("Mode: Selecting an item to exhange");
                    }
                }
                colors.Add(Color.Yellow);

                lines.Add(" ");
                colors.Add(Color.Black);

                // header help 1: trusted leader
                if (isTrustedLeader)
                {
                    lines.Add(" "); colors.Add(Color.White);
                    lines.Add(string.Format("You are {0} trusted leader, will accept all trades.", HisOrHer(npc)));
                    colors.Add(Color.LightGreen);
                }

                // header help 2: charisma roll
                if (charismaSuccess)
                {
                    lines.Add(string.Format("Charisma roll success {0}/{1}%", m_Session.Player_TurnCharismaRoll, charismaChance));
                    colors.Add(Color.LightGreen);
                }
                else
                {
                    lines.Add(string.Format("Charisma roll failed {0}/{1}%", m_Session.Player_TurnCharismaRoll, charismaChance));
                    colors.Add(Color.Red);
                }

                void ListTradeItems(Actor a, bool isActive)
                {
                    lines.Add(string.Format("{0} items", a.TheName));
                    colors.Add(Color.White);
                    for (int i = 0; i < a.Inventory.CountItems; i++)
                    {
                        Item it = a.Inventory[i];
                        if (isActive)
                        {
                            lines.Add(string.Format("{0}. {1}", (i == 9 ? 0 : (i + 1)), DescribeItemShort(it)));
                            if (state == 0)  // proposing item
                                colors.Add(Color.Yellow);
                            else  // trading for current item
                            {
                                TradeRating r = (a == player && isActive ? ratingPairs[i, iNpcSelectedItem] : ratingPairs[iPlayerSelectedItem, i]);
                                colors.Add(tradeToColor(r));
                            }
                        }
                        else
                        {
                            lines.Add("-. " + DescribeItemShort(it));
                            colors.Add(i == (a == player ? iPlayerSelectedItem : iNpcSelectedItem) ? TRADE_COLOR_SELECTED_ITEM : Color.Gray);
                        }
                    }
                };

                // list items, player and npc
                lines.Add(" "); colors.Add(Color.Black);
                ListTradeItems(player, isOnPlayerInventory && state != 2);
                lines.Add(" "); colors.Add(Color.Black);
                ListTradeItems(npc, !isOnPlayerInventory && state != 2);

                // footnote help: trade ratings color legend
                if (state != 0 && !isTrustedLeader)
                {
                    lines.Add(" "); colors.Add(Color.White);
                    lines.Add("Trade color legend : "); colors.Add(Color.White);
                    lines.Add("  asked/offered"); colors.Add(TRADE_COLOR_SELECTED_ITEM);
                    lines.Add("  will accept"); colors.Add(TRADE_COLOR_ACCEPT);
                    lines.Add("  will accept due to your charisma"); colors.Add(TRADE_COLOR_MAYBE_SUCCESS);
                    lines.Add("  will refuse due to failed charisma"); colors.Add(TRADE_COLOR_MAYBE_FAILED);
                    lines.Add("  will refuse"); colors.Add(TRADE_COLOR_REFUSE);
                }

                // draw
                ClearOverlays();
                AddOverlay(new OverlayPopup(TRADING_DIALOG_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
                OverlayPopupTitleColors ov = new OverlayPopupTitleColors(
                    string.Format("Trading with {0}", npc.TheName), Color.White,
                    lines.ToArray(), colors.ToArray(),
                    Color.White, Color.Black, new Point(32, 32));
                AddOverlay(ov);
                RedrawPlayScreen();

                // 2. Handle state
                if (state == 2)  // make offer
                {
                    ClearMessages();
                    Item offered = player.Inventory[iPlayerSelectedItem];
                    Item asked = npc.Inventory[iNpcSelectedItem];
                    AddMessage(MakeMessage(player, string.Format("{0} {1} for {2}.", Conjugate(player, VERB_OFFER), offered.TheName, asked.TheName)));

                    // get rating and apply charisma
                    TradeRating r = ratingPairs[iPlayerSelectedItem, iNpcSelectedItem];
                    if (r == TradeRating.MAYBE)
                        r = (charismaSuccess ? TradeRating.ACCEPT : TradeRating.REFUSE);

                    // npc accept or refuse
                    if (r == TradeRating.ACCEPT)
                    {
                        // accept: make deal and done.
                        AddMessage(MakeMessage(npc, Conjugate(npc, VERB_ACCEPT_THE_DEAL) + "."));
                        SwapActorItems(player, offered, npc, asked);
                        loop = false;
                        actionDone = true;
                        // sanity recover after player trade chat
                        // to be consistent with fast trade, should also recover san a failed trade but this will be
                        // abused by the player as a refused trade doesnt end the turn.
                        if (player.Model.Abilities.HasSanity)
                        {
                            RegenActorSanity(player, Rules.SANITY_RECOVER_CHAT_OR_TRADE);
                            AddMessage(MakeMessage(player, string.Format("{0} better after chatting with", Conjugate(player, VERB_FEEL)), npc));
                        }
                        if (npc.Model.Abilities.HasSanity)
                        {
                            RegenActorSanity(npc, Rules.SANITY_RECOVER_CHAT_OR_TRADE);
                            AddMessage(MakeMessage(npc, string.Format("{0} better after chatting with", Conjugate(npc, VERB_FEEL)), player));
                        }
                    }
                    else if (r == TradeRating.REFUSE)
                    {
                        // refuse: can make another offer.
                        AddMessage(MakeMessage(npc, Conjugate(npc, VERB_REFUSE_THE_DEAL) + "."));
                        isOnPlayerInventory = !isOnPlayerInventory;
                        iPlayerSelectedItem = iNpcSelectedItem = -1;
                        state = 0;
                    }

                    // done
                    AddMessagePressEnter();
                }
                else
                {
                    // Select 1st or 2nd item
                    KeyEventArgs inKey = m_UI.UI_WaitKey();

                    if (inKey.KeyCode == Keys.Escape)  // back/abort
                    {
                        if (state == 0)
                            loop = false;
                        else
                        {
                            state = 0;
                            if (isOnPlayerInventory)
                                iPlayerSelectedItem = -1;
                            else
                                iNpcSelectedItem = -1;
                            isOnPlayerInventory = !isOnPlayerInventory;
                        }
                    }
                    else if (inKey.KeyCode == Keys.Tab)  // switch inventory
                    {
                        if (state == 0)
                        {
                            isOnPlayerInventory = !isOnPlayerInventory;
                            iPlayerSelectedItem = iNpcSelectedItem = -1;
                        }
                    }
                    else
                    {
                        int slot = KeyToChoiceNumber(inKey.KeyCode);
                        if (slot != -1) // select an item
                        {
                            if (slot == 0) slot = 9;
                            else slot--;

                            if (isOnPlayerInventory)
                            {
                                if (slot < player.Inventory.CountItems)
                                {
                                    iPlayerSelectedItem = slot;
                                    if (state == 0)  // offering item 1st
                                    {
                                        state = 1;
                                        isOnPlayerInventory = false;
                                    }
                                    else
                                    {
                                        // offering item 2nd
                                        state = 2;
                                    }
                                }
                            }
                            else
                            {
                                if (slot < npc.Inventory.CountItems)
                                {
                                    iNpcSelectedItem = slot;
                                    if (state == 0)  // asking item 1st
                                    {
                                        state = 1;
                                        isOnPlayerInventory = true;
                                    }
                                    else
                                    {
                                        // asking item 2nd
                                        state = 2;
                                    }
                                }
                            }
                        }
                    }
                }

            }
            while (loop);

            // if trade done, spend player ap.
            if (actionDone)
                SpendActorActionPoints(player, Rules.BASE_ACTION_COST);

            // alpha10.1
            // cleanup
            ClearOverlays();

            return actionDone;
        }

        bool HandlePlayerNegociateTrade(Actor player)
        {
            // handle select adjacent npc
            bool loop = true;
            bool actionDone = false;
            ClearOverlays();
            AddOverlay(new OverlayPopup(NEGOCIATE_TRADE_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
            do
            {
                ///////////////////
                // 1. Redraw
                // 2. Get input.
                // 3. Handle input
                ///////////////////

                // 1. Redraw
                RedrawPlayScreen();

                // 2. Get input.
                Direction dir = WaitDirectionOrCancel();

                // 3. Handle input
                if (dir == null)
                {
                    loop = false;
                }
                else if (dir != Direction.NEUTRAL)
                {
                    Point pos = player.Location.Position + dir;
                    if (player.Location.Map.IsInBounds(pos))
                    {
                        Actor other = player.Location.Map.GetActorAt(pos);
                        if (other != null)
                        {
                            string reason;
                            if (m_Rules.CanActorInitiateTradeWith(player, other, out reason))
                            {
                                actionDone = HandlePlayerTradeNegociation(player, other);
                                loop = false;
                            }
                            else
                            {
                                AddMessage(MakeErrorMessage(String.Format("Can't trade with {0} : {1}.", other.TheName, reason)));
                            }
                        }
                        else
                            AddMessage(MakeErrorMessage("Noone there."));
                    }
                }
            }
            while (loop);

            // cleanup.
            ClearOverlays();

            // return if we did an action.
            return actionDone;
        }

        void HandlePlayerRunToggle(Actor player)
        {
            string reason;
            if (!m_Rules.CanActorRun(player, out reason))
            {
                AddMessage(MakeErrorMessage(String.Format("Cannot run now : {0}.", reason)));
                return;
            }

            // ok.
            player.IsRunning = !player.IsRunning;
            AddMessage(MakeMessage(player, String.Format("{0} running.", Conjugate(player, player.IsRunning ? VERB_START : VERB_STOP))));
        }

        bool HandlePlayerCloseDoor(Actor player)
        {
            bool loop = true;
            bool actionDone = false;

            ClearOverlays();
            AddOverlay(new OverlayPopup(CLOSE_DOOR_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));

            do
            {
                ///////////////////
                // 1. Redraw
                // 2. Get input.
                // 3. Handle input
                ///////////////////

                // 1. Redraw
                RedrawPlayScreen();

                // 2. Get input.
                Direction dir = WaitDirectionOrCancel();

                // 3. Handle input
                if (dir == null)
                {
                    loop = false;
                }
                else if (dir != Direction.NEUTRAL)
                {
                    Point pos = player.Location.Position + dir;
                    if (player.Location.Map.IsInBounds(pos))
                    {
                        MapObject mapObj = player.Location.Map.GetMapObjectAt(pos);
                        if (mapObj != null && mapObj is DoorWindow)
                        {
                            DoorWindow door = mapObj as DoorWindow;
                            string reason;
                            if (m_Rules.IsClosableFor(player, door, out reason))
                            {
                                DoCloseDoor(player, door);
                                RedrawPlayScreen();
                                loop = false;
                                actionDone = true;
                            }
                            else
                            {
                                AddMessage(MakeErrorMessage(String.Format("Can't close {0} : {1}.", door.TheName, reason)));
                            }
                        }
                        else
                            AddMessage(MakeErrorMessage("Nothing to close there."));
                    }
                }
            }
            while (loop);

            // cleanup.
            ClearOverlays();

            // return if we did an action.
            return actionDone;
        }

        bool HandlePlayerBarricade(Actor player)
        {
            bool loop = true;
            bool actionDone = false;

            ClearOverlays();
            AddOverlay(new OverlayPopup(BARRICADE_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));

            do
            {
                ///////////////////
                // 1. Redraw
                // 2. Get input.
                // 3. Handle input
                ///////////////////

                // 1. Redraw
                RedrawPlayScreen();

                // 2. Get input.
                Direction dir = WaitDirectionOrCancel();

                // 3. Handle input
                if (dir == null)
                {
                    loop = false;
                }
                else if (dir != Direction.NEUTRAL)
                {
                    Point pos = player.Location.Position + dir;
                    if (player.Location.Map.IsInBounds(pos))
                    {
                        MapObject mapObj = player.Location.Map.GetMapObjectAt(pos);
                        if (mapObj != null)
                        {
                            // barricading a door.
                            if (mapObj is DoorWindow)
                            {
                                DoorWindow door = mapObj as DoorWindow;
                                string reason;
                                if (m_Rules.CanActorBarricadeDoor(player, door, out reason))
                                {
                                    DoBarricadeDoor(player, door);
                                    RedrawPlayScreen();
                                    loop = false;
                                    actionDone = true;
                                }
                                else
                                {
                                    AddMessage(MakeErrorMessage(String.Format("Cannot barricade {0} : {1}.", door.TheName, reason)));
                                }
                            }
                            // repairing a fortification.
                            else if (mapObj is Fortification)
                            {
                                Fortification fort = mapObj as Fortification;
                                string reason;
                                if (m_Rules.CanActorRepairFortification(player, fort, out reason))
                                {
                                    DoRepairFortification(player, fort);
                                    RedrawPlayScreen();
                                    loop = false;
                                    actionDone = true;
                                }
                                else
                                {
                                    AddMessage(MakeErrorMessage(String.Format("Cannot repair {0} : {1}.", fort.TheName, reason)));
                                }
                            }
                            else
                                AddMessage(MakeErrorMessage(String.Format("{0} cannot be repaired or barricaded.", mapObj.TheName)));
                        }
                        else
                            AddMessage(MakeErrorMessage("Nothing to barricade there."));
                    }
                }
            }
            while (loop);

            // cleanup.
            ClearOverlays();

            // return if we did an action.
            return actionDone;
        }

        bool HandlePlayerBreak(Actor player)
        {
            bool loop = true;
            bool actionDone = false;

            ClearOverlays();
            AddOverlay(new OverlayPopup(BREAK_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));

            do
            {
                ///////////////////
                // 1. Redraw
                // 2. Get input.
                // 3. Handle input
                ///////////////////

                // 1. Redraw
                RedrawPlayScreen();

                // 2. Get input.
                Direction dir = WaitDirectionOrCancel();

                // 3. Handle input
                if (dir == null)
                {
                    loop = false;
                }
                else
                {
                    // handle neutral direction = through exit.
                    if (dir == Direction.NEUTRAL)
                    {
                        Exit exitThere = player.Location.Map.GetExitAt(player.Location.Position);
                        if (exitThere == null)
                            AddMessage(MakeErrorMessage("No exit there."));
                        else
                        {
                            // attack/break?
                            string reason;
                            Map mapTo = exitThere.ToMap;
                            Actor actorTo = mapTo.GetActorAt(exitThere.ToPosition);
                            if (actorTo != null)
                            {
                                // only if enemy.
                                if (m_Rules.AreEnemies(player, actorTo))
                                {
                                    // check melee rule.
                                    if (m_Rules.CanActorMeleeAttack(player, actorTo, out reason))
                                    {
                                        DoMeleeAttack(player, actorTo);
                                        loop = false;
                                        actionDone = true;
                                    }
                                    else
                                        AddMessage(MakeErrorMessage(String.Format("Cannot attack {0} : {1}.", actorTo.Name, reason)));
                                }
                                else
                                    AddMessage(MakeErrorMessage(String.Format("{0} is not your enemy.", actorTo.Name)));
                            }
                            else
                            {
                                // break?
                                MapObject objTo = mapTo.GetMapObjectAt(exitThere.ToPosition);
                                if (objTo != null)
                                {
                                    // check break rule.
                                    if (m_Rules.IsBreakableFor(player, objTo, out reason))
                                    {
                                        DoBreak(player, objTo);
                                        loop = false;
                                        actionDone = true;
                                    }
                                    else
                                        AddMessage(MakeErrorMessage(String.Format("Cannot break {0} : {1}.", objTo.TheName, reason)));
                                }
                                else
                                    AddMessage(MakeErrorMessage("Nothing to break or attack on the other side."));
                            }

                        }
                    }
                    else
                    {
                        // adjacent direction.
                        Point pos = player.Location.Position + dir;
                        if (player.Location.Map.IsInBounds(pos))
                        {
                            MapObject mapObj = player.Location.Map.GetMapObjectAt(pos);
                            if (mapObj != null)
                            {
                                string reason;
                                if (m_Rules.IsBreakableFor(player, mapObj, out reason))
                                {
                                    DoBreak(player, mapObj);
                                    RedrawPlayScreen();
                                    loop = false;
                                    actionDone = true;
                                }
                                else
                                {
                                    AddMessage(MakeErrorMessage(String.Format("Cannot break {0} : {1}.", mapObj.TheName, reason)));
                                }
                            }
                            else
                                AddMessage(MakeErrorMessage("Nothing to break there."));
                        }
                    }
                }
            }
            while (loop);

            // cleanup.
            ClearOverlays();

            // return if we did an action.
            return actionDone;
        }

        bool HandlePlayerBuildFortification(Actor player, bool isLarge)
        {
            /////////////////////////////////////
            // Check skill & has enough material.
            /////////////////////////////////////
            if (player.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.CARPENTRY) == 0)
            {
                AddMessage(MakeErrorMessage("need carpentry skill."));
                return false;
            }
            int need = m_Rules.ActorBarricadingMaterialNeedForFortification(player, isLarge);
            if (m_Rules.CountBarricadingMaterial(player) < need)
            {
                AddMessage(MakeErrorMessage(string.Format("not enough barricading material, need {0}.", need)));
                return false;
            }

            bool loop = true;
            bool actionDone = false;

            ClearOverlays();
            AddOverlay(new OverlayPopup(isLarge ? BUILD_LARGE_FORT_MODE_TEXT : BUILD_SMALL_FORT_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));

            do
            {
                ///////////////////
                // 1. Redraw
                // 2. Get input.
                // 3. Handle input
                ///////////////////

                // 1. Redraw
                RedrawPlayScreen();

                // 2. Get input.
                Direction dir = WaitDirectionOrCancel();

                // 3. Handle input
                if (dir == null)
                {
                    loop = false;
                }
                else if (dir != Direction.NEUTRAL)
                {
                    Point pos = player.Location.Position + dir;
                    if (player.Location.Map.IsInBounds(pos))
                    {
                        string reason;
                        if (m_Rules.CanActorBuildFortification(player, pos, isLarge, out reason))
                        {
                            DoBuildFortification(player, pos, isLarge);
                            RedrawPlayScreen();
                            loop = false;
                            actionDone = true;
                        }
                        else
                        {
                            AddMessage(MakeErrorMessage(String.Format("Cannot build here : {0}.", reason)));
                        }
                    }
                }
            }
            while (loop);

            // cleanup.
            ClearOverlays();

            // return if we did an action.
            return actionDone;
        }

        bool HandlePlayerFireMode(Actor player)
        {
            bool loop = true;
            bool actionDone = false;

            // If grenade equipped, redirected to HandlePlayerThrowGrenade.
            ItemGrenade grenade = player.GetEquippedWeapon() as ItemGrenade;
            ItemGrenadePrimed primedGrenade = player.GetEquippedWeapon() as ItemGrenadePrimed;
            if (grenade != null || primedGrenade != null)
                return HandlePlayerThrowGrenade(player);

            // Check if weapon to fire.
            ItemRangedWeapon rangedWeapon = player.GetEquippedWeapon() as ItemRangedWeapon;
            if (rangedWeapon == null)
            {
                AddMessage(MakeErrorMessage("No weapon ready to fire."));
                RedrawPlayScreen();
                return false;
            }
            if (rangedWeapon.Ammo <= 0)
            {
                AddMessage(MakeErrorMessage("No ammo left."));
                RedrawPlayScreen();
                return false;
            }

            // Get targeting data.
            HashSet<Point> fov = LOS.ComputeFOVFor(m_Rules, player, m_Session.WorldTime, m_Session.World.Weather);
            List<Actor> potentialTargets = m_Rules.GetEnemiesInFov(player, fov);

            if (potentialTargets == null || potentialTargets.Count == 0)
            {
                AddMessage(MakeErrorMessage("No targets to fire at."));
                RedrawPlayScreen();
                return false;
            }

            // Loop.
            Attack rangedAttack = m_Rules.ActorRangedAttack(player, player.CurrentRangedAttack, 0, null);
            int iCurrentTarget = 0;
            List<Point> LoF = new List<Point>(rangedAttack.Range);
            FireMode mode = m_Session.Player_CurrentFireMode;  // alpha10
            do
            {
                Actor currentTarget = potentialTargets[iCurrentTarget];
                LoF.Clear();
                string reason;
                bool canFireAtTarget = m_Rules.CanActorFireAt(player, currentTarget, LoF, out reason);
                int dToTarget = m_Rules.GridDistance(player.Location.Position, currentTarget.Location.Position);

                string modeDesc;
                if (mode == FireMode.RAPID)
                    modeDesc = string.Format("RAPID fire average hit chances {0}% {1}%", m_Rules.ComputeChancesRangedHit(player, currentTarget, 1), m_Rules.ComputeChancesRangedHit(player, currentTarget, 2));
                else
                    modeDesc = string.Format("Normal fire average hit chance {0}%", m_Rules.ComputeChancesRangedHit(player, currentTarget, 0));

                ///////////////////
                // 1. Redraw
                // 2. Get input.
                // 3. Handle input
                ///////////////////

                // 1. Redraw
                List<string> overlayPopupText = new List<string>();
                overlayPopupText.AddRange(FIRE_MODE_TEXT);
                overlayPopupText.Add(modeDesc);
                ClearOverlays();
                AddOverlay(new OverlayPopup(overlayPopupText.ToArray(), MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
                Point targetScreen = MapToScreen(currentTarget.Location.Position);
                AddOverlay(new OverlayImage(targetScreen, GameImages.ICON_TARGET));
                string lineImage = canFireAtTarget ? (dToTarget <= rangedAttack.EfficientRange ? GameImages.ICON_LINE_CLEAR : GameImages.ICON_LINE_BAD) : GameImages.ICON_LINE_BLOCKED;
                foreach (Point pt in LoF)
                {
                    Point screenPt = MapToScreen(pt);
                    AddOverlay(new OverlayImage(screenPt, lineImage));
                }
                RedrawPlayScreen();

                // 2. Get input.
                KeyEventArgs key = m_UI.UI_WaitKey();
                PlayerCommand command = InputTranslator.KeyToCommand(key);

                // 3. Handle input
                if (key.KeyCode == Keys.Escape) //command == PlayerCommand.EXIT_OR_CANCEL)
                {
                    loop = false;
                }
                else if (key.KeyCode == Keys.T)  // next target
                {
                    iCurrentTarget = (iCurrentTarget + 1) % potentialTargets.Count;
                }
                else if (key.KeyCode == Keys.M)    // next mode
                {
                    // switch.
                    mode = (FireMode)(((int)mode + 1) % (int)FireMode._COUNT);
                    // tell.
                    AddMessage(new Message(String.Format("Switched to {0} fire mode.", mode.ToString()), m_Session.WorldTime.TurnCounter, Color.Yellow));
                    // alpha10
                    // save preference to session
                    m_Session.Player_CurrentFireMode = mode;
                }
                else if (key.KeyCode == Keys.F) // do fire
                {
                    if (canFireAtTarget)
                    {
                        DoRangedAttack(player, currentTarget, LoF, mode);
                        RedrawPlayScreen();
                        loop = false;
                        actionDone = true;
                    }
                    else
                    {
                        AddMessage(MakeErrorMessage(String.Format("Can't fire at {0} : {1}.", currentTarget.TheName, reason)));
                    }
                }
            }
            while (loop);

            // cleanup.
            ClearOverlays();

            // return if we did an action.
            return actionDone;
        }

        void HandlePlayerMarkEnemies(Actor player)
        {
            // Pre-conditions.
            // FIXME put all that into a rule Rule.CanMakeEnemyOf()
            if (player.Model.Abilities.IsUndead)
            {
                AddMessage(MakeErrorMessage("Undeads can't have personal enemies."));
                return;
            }

            // List all visible actors.
            #region
            Map map = player.Location.Map;
            List<Actor> visibleActors = new List<Actor>();
            foreach (Point p in m_PlayerFOV)
            {
                Actor a = map.GetActorAt(p);
                if (a == null || a.IsPlayer)
                    continue;
                visibleActors.Add(a);
            }
            if (visibleActors.Count == 0)
            {
                AddMessage(MakeErrorMessage("No visible actors to mark."));
                RedrawPlayScreen();
                return;
            }
            #endregion

            // Loop.
            bool loop = true;
            int iCurrentActor = 0;
            do
            {
                Actor currentActor = visibleActors[iCurrentActor];

                ///////////////////
                // 1. Redraw
                // 2. Get input.
                // 3. Handle input
                ///////////////////

                // 1. Redraw
                ClearOverlays();
                AddOverlay(new OverlayPopup(MARK_ENEMIES_MODE, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
                Point targetScreen = MapToScreen(currentActor.Location.Position);
                AddOverlay(new OverlayImage(targetScreen, GameImages.ICON_TARGET));
                RedrawPlayScreen();

                // 2. Get input.
                KeyEventArgs key = m_UI.UI_WaitKey();
                PlayerCommand command = InputTranslator.KeyToCommand(key);

                // 3. Handle input
                if (key.KeyCode == Keys.Escape)// command == PlayerCommand.EXIT_OR_CANCEL)
                {
                    loop = false;
                }
                else if (key.KeyCode == Keys.T)  // next actor
                {
                    iCurrentActor = (iCurrentActor + 1) % visibleActors.Count;
                }

                else if (key.KeyCode == Keys.E) // toggle.
                {
                    // never make enemies of leader/follower/enemy faction.
                    // FIXME put all that into a rule Rule.CanMakeEnemyOf()
                    bool allowed = true;
                    if (currentActor.Leader == player)
                    {
                        AddMessage(MakeErrorMessage("Can't make a follower your enemy."));
                        allowed = false;
                    }
                    else if (player.Leader == currentActor)
                    {
                        AddMessage(MakeErrorMessage("Can't make your leader your enemy."));
                        allowed = false;
                    }
                    else if (m_Rules.AreEnemies(m_Player, currentActor))
                    {
                        AddMessage(MakeErrorMessage("Already enemies."));
                        allowed = false;
                    }

                    // do it?
                    if (allowed)
                    {
                        AddMessage(new Message(String.Format("{0} is now a personal enemy.", currentActor.TheName), m_Session.WorldTime.TurnCounter, Color.Orange));
                        DoMakeAggression(player, currentActor);
                    }
                }
            }
            while (loop);

            // cleanup.
            ClearOverlays();
        }

        bool HandlePlayerThrowGrenade(Actor player)
        {
            bool loop = true;
            bool actionDone = false;

            // Get grenade equipped.
            ItemGrenade unprimedGrenade = player.GetEquippedWeapon() as ItemGrenade;
            ItemGrenadePrimed primedGrenade = player.GetEquippedWeapon() as ItemGrenadePrimed;
            if (unprimedGrenade == null && primedGrenade == null)
            {
                AddMessage(MakeErrorMessage("No grenade to throw."));
                RedrawPlayScreen();
                return false;
            }
            ItemGrenadeModel grenadeModel;
            if (unprimedGrenade != null)
                grenadeModel = unprimedGrenade.Model as ItemGrenadeModel;
            else
                grenadeModel = (primedGrenade.Model as ItemGrenadePrimedModel).GrenadeModel;

            // Get data.
            Map map = player.Location.Map;
            Point targetThrow = player.Location.Position;
            int maxThrowDist = m_Rules.ActorMaxThrowRange(player, grenadeModel.MaxThrowDistance);

            // Loop.
            List<Point> LoT = new List<Point>();
            do
            {
                // get LoT.
                LoT.Clear();
                string reason;
                bool canThrowAtTarget = m_Rules.CanActorThrowTo(player, targetThrow, LoT, out reason);

                ///////////////////
                // 1. Redraw
                // 2. Get input.
                // 3. Handle input
                ///////////////////

                // 1. Redraw
                ClearOverlays();
                AddOverlay(new OverlayPopup(THROW_GRENADE_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
                string lineImage = canThrowAtTarget ? GameImages.ICON_LINE_CLEAR : GameImages.ICON_LINE_BLOCKED;
                foreach (Point pt in LoT)
                {
                    Point screenPt = MapToScreen(pt);
                    AddOverlay(new OverlayImage(screenPt, lineImage));
                }
                RedrawPlayScreen();

                // 2. Get input.
                KeyEventArgs key = m_UI.UI_WaitKey();
                PlayerCommand command = InputTranslator.KeyToCommand(key);

                // 3. Handle input
                if (key.KeyCode == Keys.Escape)// command == PlayerCommand.EXIT_OR_CANCEL)
                {
                    loop = false;
                }
                else if (key.KeyCode == Keys.F) // do throw.
                {
                    if (canThrowAtTarget)
                    {
                        bool doIt = true;

                        // if within the blast radius, ask for confirmation...
                        if (m_Rules.GridDistance(player.Location.Position, targetThrow) <= grenadeModel.BlastAttack.Radius)
                        {
                            ClearMessages();
                            AddMessage(new Message("You are in the blast radius!", m_Session.WorldTime.TurnCounter, Color.Yellow));
                            AddMessage(MakeYesNoMessage("Really throw there"));
                            RedrawPlayScreen();
                            doIt = WaitYesOrNo();
                            ClearMessages();
                            RedrawPlayScreen();
                        }

                        if (doIt)
                        {
                            // fire in the hole!
                            if (unprimedGrenade != null)
                                DoThrowGrenadeUnprimed(player, targetThrow);
                            else
                                DoThrowGrenadePrimed(player, targetThrow);
                            RedrawPlayScreen();
                            loop = false;
                            actionDone = true;
                        }
                    }
                    else
                    {
                        AddMessage(MakeErrorMessage(String.Format("Can't throw there : {0}.", reason)));
                    }
                }
                else
                {
                    // direction?
                    Direction dir = CommandToDirection(command);
                    if (dir != null)
                    {
                        Point pos = targetThrow + dir;
                        if (map.IsInBounds(pos) && m_Rules.GridDistance(player.Location.Position, pos) <= maxThrowDist)
                            targetThrow = pos;
                    }
                }
            }
            while (loop);

            // cleanup.
            ClearOverlays();

            // return if we did an action.
            return actionDone;
        }

        bool HandlePlayerSleep(Actor player)
        {
            // Check rule.
            string reason;
            if (!m_Rules.CanActorSleep(player, out reason))
            {
                AddMessage(MakeErrorMessage(String.Format("Cannot sleep now : {0}.", reason)));
                return false;
            }

            // Ask for confirmation.
            AddMessage(MakeYesNoMessage("Really sleep there"));
            RedrawPlayScreen();
            bool confirm = WaitYesOrNo();
            if (!confirm)
            {
                AddMessage(new Message("Good, keep those eyes wide open.", m_Session.WorldTime.TurnCounter, Color.Yellow));
                return false;
            }

            // alpha10.1 check autosave before player starts to sleep
            CheckAutoSaveTime();

            // Start sleeping.
            AddMessage(new Message("Goodnight, happy nightmares!", m_Session.WorldTime.TurnCounter, Color.Yellow));
            DoStartSleeping(player);
            RedrawPlayScreen();
            // check music.
            m_MusicManager.Stop();
            m_MusicManager.PlayLooping(GameMusics.SLEEP, MusicPriority.PRIORITY_EVENT);
            return true;
        }

        bool HandlePlayerSwitchPlace(Actor player)
        {
            bool loop = true;
            bool actionDone = false;

            ClearOverlays();
            AddOverlay(new OverlayPopup(SWITCH_PLACE_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));

            do
            {
                ///////////////////
                // 1. Redraw
                // 2. Get input.
                // 3. Handle input
                ///////////////////

                // 1. Redraw
                RedrawPlayScreen();

                // 2. Get input.
                Direction dir = WaitDirectionOrCancel();

                // 3. Handle input
                if (dir == null)
                {
                    loop = false;
                }
                else if (dir != Direction.NEUTRAL)
                {
                    Point pos = player.Location.Position + dir;
                    if (player.Location.Map.IsInBounds(pos))
                    {
                        Actor other = player.Location.Map.GetActorAt(pos);
                        if (other != null)
                        {
                            string reason;
                            if (m_Rules.CanActorSwitchPlaceWith(player, other, out reason))
                            {
                                // switch place.
                                actionDone = true;
                                loop = false;
                                DoSwitchPlace(player, other);
                            }
                            else
                            {
                                AddMessage(MakeErrorMessage(String.Format("Can't switch place : {0}", reason)));
                            }
                        }
                        else
                            AddMessage(MakeErrorMessage("Noone there."));
                    }
                }
            }
            while (loop);

            // cleanup.
            ClearOverlays();

            // return if we did an action.
            return actionDone;
        }

        bool HandlePlayerTakeLead(Actor player)
        {
            bool loop = true;
            bool actionDone = false;

            ClearOverlays();
            AddOverlay(new OverlayPopup(TAKE_LEAD_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));

            do
            {
                ///////////////////
                // 1. Redraw
                // 2. Get input.
                // 3. Handle input
                ///////////////////

                // 1. Redraw
                RedrawPlayScreen();

                // 2. Get input.
                Direction dir = WaitDirectionOrCancel();

                // 3. Handle input
                if (dir == null)
                {
                    loop = false;
                }
                else if (dir != Direction.NEUTRAL)
                {
                    Point pos = player.Location.Position + dir;
                    if (player.Location.Map.IsInBounds(pos))
                    {
                        Actor other = player.Location.Map.GetActorAt(pos);
                        if (other != null)
                        {
                            string reason;
                            if (m_Rules.CanActorTakeLead(player, other, out reason))
                            {
                                // take lead.
                                actionDone = true;
                                loop = false;

                                // alpha10.1 steal lead vs take lead
                                if (other.HasLeader)
                                    DoStealLead(player, other);
                                else
                                    DoTakeLead(player, other);

                                // scoring.
                                m_Session.Scoring.AddEvent(m_Session.WorldTime.TurnCounter, String.Format("Recruited {0}.", other.TheName));

                                // help message.
                                AddMessage(new Message("(you can now set directives and orders for your new follower).", m_Session.WorldTime.TurnCounter, Color.White));
                                AddMessage(new Message(String.Format("(to give order : press <{0}>).", s_KeyBindings.Get(PlayerCommand.ORDER_MODE).ToString()), m_Session.WorldTime.TurnCounter, Color.White));

                            }
                            else if (other.Leader == player)
                            {
                                if (m_Rules.CanActorCancelLead(player, other, out reason))
                                {
                                    // ask for confirmation.
                                    AddMessage(MakeYesNoMessage(String.Format("Really ask {0} to leave", other.TheName)));
                                    RedrawPlayScreen();
                                    bool confirm = WaitYesOrNo();
                                    if (confirm)
                                    {
                                        // cancel lead.
                                        actionDone = true;
                                        loop = false;
                                        DoCancelLead(player, other);

                                        // scoring.
                                        m_Session.Scoring.AddEvent(m_Session.WorldTime.TurnCounter, String.Format("Fired {0}.", other.TheName));
                                    }
                                    else
                                        AddMessage(new Message("Good, together you are strong.", m_Session.WorldTime.TurnCounter, Color.Yellow));
                                }
                                else
                                {
                                    AddMessage(MakeErrorMessage(String.Format("{0} can't leave : {1}.", other.TheName, reason)));
                                }
                            }
                            else
                            {
                                AddMessage(MakeErrorMessage(String.Format("Can't lead {0} : {1}.", other.TheName, reason)));
                            }
                        }
                        else
                            AddMessage(MakeErrorMessage("Noone there."));
                    }
                }
            }
            while (loop);

            // cleanup.
            ClearOverlays();

            // return if we did an action.
            return actionDone;
        }

        bool HandlePlayerPush(Actor player)
        {
            // fail immediatly for stupid cases.
            if (!m_Rules.HasActorPushAbility(player))
            {
                AddMessage(MakeErrorMessage("Cannot push objects."));
                return false;
            }
            if (m_Rules.IsActorTired(player))
            {
                AddMessage(MakeErrorMessage("Too tired to push."));
                return false;
            }


            bool loop = true;
            bool actionDone = false;

            ClearOverlays();
            AddOverlay(new OverlayPopup(PUSH_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));

            do
            {
                ///////////////////
                // 1. Redraw
                // 2. Get input.
                // 3. Handle input
                ///////////////////

                // 1. Redraw
                RedrawPlayScreen();

                // 2. Get input.
                Direction dir = WaitDirectionOrCancel();

                // 3. Handle input
                if (dir == null)
                {
                    loop = false;
                }
                else if (dir != Direction.NEUTRAL)
                {
                    Point pos = player.Location.Position + dir;
                    if (player.Location.Map.IsInBounds(pos))
                    {
                        // shove actor vs push object.
                        Actor other = player.Location.Map.GetActorAt(pos);
                        MapObject mapObj = player.Location.Map.GetMapObjectAt(pos);
                        string reason;
                        if (other != null)
                        {
                            // shove.
                            if (m_Rules.CanActorShove(player, other, out reason))
                            {
                                if (HandlePlayerShoveActor(player, other))
                                {
                                    loop = false;
                                    actionDone = true;
                                }
                            }
                            else
                                AddMessage(MakeErrorMessage(String.Format("Cannot shove {0} : {1}.", other.TheName, reason)));

                        }
                        else if (mapObj != null)
                        {
                            // push.
                            if (m_Rules.CanActorPush(player, mapObj, out reason))
                            {
                                if (HandlePlayerPushObject(player, mapObj))
                                {
                                    loop = false;
                                    actionDone = true;
                                }
                            }
                            else
                            {
                                AddMessage(MakeErrorMessage(String.Format("Cannot move {0} : {1}.", mapObj.TheName, reason)));
                            }
                        }
                        else
                        {
                            // nothing to push/shove.
                            AddMessage(MakeErrorMessage("Nothing to push there."));
                        }
                    }
                }
            }
            while (loop);

            // cleanup.
            ClearOverlays();

            // return if we did an action.
            return actionDone;
        }

        bool HandlePlayerPushObject(Actor player, MapObject mapObj)
        {
            bool loop = true;
            bool actionDone = false;

            ClearOverlays();
            AddOverlay(new OverlayPopup(new string[] { String.Format(PUSH_OBJECT_MODE_TEXT, mapObj.TheName) }, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
            AddOverlay(new OverlayRect(Color.Yellow, new Rectangle(MapToScreen(mapObj.Location.Position), new Size(TILE_SIZE, TILE_SIZE))));

            do
            {
                ///////////////////
                // 1. Redraw
                // 2. Get input.
                // 3. Handle input
                ///////////////////

                // 1. Redraw
                RedrawPlayScreen();

                // 2. Get input.
                Direction dir = WaitDirectionOrCancel();

                // 3. Handle input
                if (dir == null)
                {
                    loop = false;
                }
                else if (dir != Direction.NEUTRAL)
                {
                    Point movePos = mapObj.Location.Position + dir;
                    if (player.Location.Map.IsInBounds(movePos))
                    {
                        string reason;
                        if (m_Rules.CanPushObjectTo(mapObj, movePos, out reason))
                        {
                            DoPush(player, mapObj, movePos);
                            loop = false;
                            actionDone = true;
                        }
                        else
                        {
                            AddMessage(MakeErrorMessage(String.Format("Cannot move {0} there : {1}.", mapObj.TheName, reason)));
                        }
                    }
                }
            }
            while (loop);

            // cleanup.
            ClearOverlays();

            // return if we did an action.
            return actionDone;
        }

        bool HandlePlayerShoveActor(Actor player, Actor other)
        {
            bool loop = true;
            bool actionDone = false;

            ClearOverlays();
            AddOverlay(new OverlayPopup(new string[] { String.Format(SHOVE_ACTOR_MODE_TEXT, other.TheName) }, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
            AddOverlay(new OverlayRect(Color.Yellow, new Rectangle(MapToScreen(other.Location.Position), new Size(TILE_SIZE, TILE_SIZE))));

            do
            {
                ///////////////////
                // 1. Redraw
                // 2. Get input.
                // 3. Handle input
                ///////////////////

                // 1. Redraw
                RedrawPlayScreen();

                // 2. Get input.
                Direction dir = WaitDirectionOrCancel();

                // 3. Handle input
                if (dir == null)
                {
                    loop = false;
                }
                else if (dir != Direction.NEUTRAL)
                {
                    Point movePos = other.Location.Position + dir;
                    if (player.Location.Map.IsInBounds(movePos))
                    {
                        string reason;
                        if (m_Rules.CanShoveActorTo(other, movePos, out reason))
                        {
                            DoShove(player, other, movePos);
                            loop = false;
                            actionDone = true;
                        }
                        else
                        {
                            AddMessage(MakeErrorMessage(String.Format("Cannot shove {0} there : {1}.", other.TheName, reason)));
                        }
                    }
                }
            }
            while (loop);

            // cleanup.
            ClearOverlays();

            // return if we did an action.
            return actionDone;
        }

        // alpha10
        bool HandlePlayerPull(Actor player)
        {
            // fail immediatly for stupid cases.
            if (!m_Rules.HasActorPushAbility(player))
            {
                AddMessage(MakeErrorMessage("Cannot pull objects."));
                return false;
            }
            if (m_Rules.IsActorTired(player))
            {
                AddMessage(MakeErrorMessage("Too tired to pull."));
                return false;
            }
            MapObject otherMobj = player.Location.Map.GetMapObjectAt(player.Location.Position);
            if (otherMobj != null)
            {
                AddMessage(MakeErrorMessage(string.Format("Cannot pull : {0} is blocking.", otherMobj.TheName)));
                return false;
            }


            bool loop = true;
            bool actionDone = false;

            ClearOverlays();
            AddOverlay(new OverlayPopup(PULL_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));

            do
            {
                ///////////////////
                // 1. Redraw
                // 2. Get input.
                // 3. Handle input
                ///////////////////

                // 1. Redraw
                RedrawPlayScreen();

                // 2. Get input.
                Direction dir = WaitDirectionOrCancel();

                // 3. Handle input
                if (dir == null)
                {
                    loop = false;
                }
                else if (dir != Direction.NEUTRAL)
                {
                    Point pos = player.Location.Position + dir;
                    if (player.Location.Map.IsInBounds(pos))
                    {
                        MapObject mapObj = player.Location.Map.GetMapObjectAt(pos);
                        Actor other = player.Location.Map.GetActorAt(pos);
                        string reason;
                        if (other != null)
                        {
                            // pull-shove.
                            if (m_Rules.CanActorShove(player, other, out reason))  // if can shove, can pull-shove.
                            {
                                if (HandlePlayerPullActor(player, other))
                                {
                                    loop = false;
                                    actionDone = true;
                                }
                            }
                            else
                                AddMessage(MakeErrorMessage(String.Format("Cannot pull {0} : {1}.", other.TheName, reason)));
                        }
                        else if (mapObj != null)
                        {
                            // pull.
                            if (m_Rules.CanActorPush(player, mapObj, out reason))  // if can push, can pull.
                            {
                                if (HandlePlayerPullObject(player, mapObj))
                                {
                                    loop = false;
                                    actionDone = true;
                                }
                            }
                            else
                            {
                                AddMessage(MakeErrorMessage(String.Format("Cannot move {0} : {1}.", mapObj.TheName, reason)));
                            }
                        }
                        else
                        {
                            // nothing to pull.
                            AddMessage(MakeErrorMessage("Nothing to pull there."));
                        }
                    }
                }
            }
            while (loop);

            // cleanup.
            ClearOverlays();

            // return if we did an action.
            return actionDone;
        }

        // alpha10
        bool HandlePlayerPullObject(Actor player, MapObject mapObj)
        {
            bool loop = true;
            bool actionDone = false;

            ClearOverlays();
            AddOverlay(new OverlayPopup(new string[] { String.Format(PULL_OBJECT_MODE_TEXT, mapObj.TheName) }, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
            AddOverlay(new OverlayRect(Color.Yellow, new Rectangle(MapToScreen(mapObj.Location.Position), new Size(TILE_SIZE, TILE_SIZE))));

            do
            {
                ///////////////////
                // 1. Redraw
                // 2. Get input.
                // 3. Handle input
                ///////////////////

                // 1. Redraw
                RedrawPlayScreen();

                // 2. Get input.
                Direction dir = WaitDirectionOrCancel();

                // 3. Handle input
                if (dir == null)
                {
                    loop = false;
                }
                else if (dir != Direction.NEUTRAL)
                {
                    Point moveToPos = player.Location.Position + dir;
                    if (player.Location.Map.IsInBounds(moveToPos))
                    {
                        string reason;
                        if (m_Rules.CanPullObject(player, mapObj, moveToPos, out reason))
                        {
                            DoPull(player, mapObj, moveToPos);
                            loop = false;
                            actionDone = true;
                        }
                        else
                        {
                            AddMessage(MakeErrorMessage(String.Format("Cannot pull there : {0}.", reason)));
                        }
                    }
                }
            }
            while (loop);

            // cleanup.
            ClearOverlays();

            // return if we did an action.
            return actionDone;
        }

        // alpha10
        bool HandlePlayerPullActor(Actor player, Actor other)
        {
            bool loop = true;
            bool actionDone = false;

            ClearOverlays();
            AddOverlay(new OverlayPopup(new string[] { String.Format(PULL_ACTOR_MODE_TEXT, other.TheName) }, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
            AddOverlay(new OverlayRect(Color.Yellow, new Rectangle(MapToScreen(other.Location.Position), new Size(TILE_SIZE, TILE_SIZE))));

            do
            {
                ///////////////////
                // 1. Redraw
                // 2. Get input.
                // 3. Handle input
                ///////////////////

                // 1. Redraw
                RedrawPlayScreen();

                // 2. Get input.
                Direction dir = WaitDirectionOrCancel();

                // 3. Handle input
                if (dir == null)
                {
                    loop = false;
                }
                else if (dir != Direction.NEUTRAL)
                {
                    Point moveToPos = player.Location.Position + dir;
                    if (player.Location.Map.IsInBounds(moveToPos))
                    {
                        string reason;
                        if (m_Rules.CanPullActor(player, other, moveToPos, out reason))
                        {
                            DoPullActor(player, other, moveToPos);
                            loop = false;
                            actionDone = true;
                        }
                        else
                        {
                            AddMessage(MakeErrorMessage(String.Format("Cannot pull there : {0}.", reason)));
                        }
                    }
                }
            }
            while (loop);

            // cleanup.
            ClearOverlays();

            // return if we did an action.
            return actionDone;
        }

        bool HandlePlayerUseSpray(Actor player)
        {
            // get equipped item.
            Item it = player.GetEquippedItem(DollPart.LEFT_HAND);
            if (it == null)
            {
                AddMessage(MakeErrorMessage("No spray equipped."));
                RedrawPlayScreen();
                return false;
            }

            //////////////////////////////////////////////
            // Handle concrete action depending on spray.
            // 1. Spray paint.
            // 2. Spray scent.
            //////////////////////////////////////////////

            // 1. Spray paint.
            ItemSprayPaint sprayPaint = it as ItemSprayPaint;
            if (sprayPaint != null)
                return HandlePlayerTag(player);

            // 2. Spray scent.
            ItemSprayScent sprayScent = it as ItemSprayScent;
            if (sprayScent != null)
            {
                // alpha10 new way to use stench killer
                return HandlePlayerSprayOdorSuppressor(player);
            }

            // no spray equipped.
            AddMessage(MakeErrorMessage("No spray equipped."));
            RedrawPlayScreen();
            return false;
        }

        bool HandlePlayerTag(Actor player)
        {
            bool loop = true;
            bool actionDone = false;

            // Check if has spray paint.
            ItemSprayPaint sprayPaint = player.GetEquippedItem(DollPart.LEFT_HAND) as ItemSprayPaint;
            if (sprayPaint == null)
            {
                AddMessage(MakeErrorMessage("No spray paint equipped."));
                RedrawPlayScreen();
                return false;
            }
            if (sprayPaint.PaintQuantity <= 0)
            {
                AddMessage(MakeErrorMessage("No paint left."));
                RedrawPlayScreen();
                return false;
            }


            ClearOverlays();
            AddOverlay(new OverlayPopup(TAG_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
            do
            {
                ///////////////////
                // 1. Redraw
                // 2. Get input.
                // 3. Handle input
                ///////////////////

                // 1. Redraw
                RedrawPlayScreen();

                // 2. Get input.
                Direction dir = WaitDirectionOrCancel();

                // 3. Handle input
                if (dir == null)
                {
                    loop = false;
                }
                else if (dir != Direction.NEUTRAL)
                {
                    Point pos = player.Location.Position + dir;
                    if (player.Location.Map.IsInBounds(pos))
                    {
                        string reason;
                        if (CanTag(player.Location.Map, pos, out reason))
                        {
                            DoTag(player, sprayPaint, pos);
                            loop = false;
                            actionDone = true;
                        }
                        else
                        {
                            AddMessage(MakeErrorMessage(String.Format("Can't tag there : {0}.", reason)));
                            RedrawPlayScreen();
                        }

                    }
                }
            }
            while (loop);

            // cleanup.
            ClearOverlays();

            // return if we did an action.
            return actionDone;
        }

        bool CanTag(Map map, Point pos, out string reason)
        {
            ///////////////////////
            // Can't tag if:
            // 1. Out of bounds.
            // 2. An actor there.
            // 3. An object there.
            ///////////////////////

            // 1. Out of bounds.
            if (!map.IsInBounds(pos))
            {
                reason = "out of map";
                return false;
            }

            // 2. An actor there.
            Actor other = map.GetActorAt(pos);
            if (other != null)
            {
                reason = "someone there";
                return false;
            }

            // 3. An object there.
            MapObject mapObj = map.GetMapObjectAt(pos);
            if (mapObj != null)
            {
                reason = "something there";
                return false;
            }

            reason = "";
            return true;
        }

        // alpha10 new way to use stench killer
        bool HandlePlayerSprayOdorSuppressor(Actor player)
        {
            bool loop = true;
            bool actionDone = false;

            // Check if has odor suppressor.
            ItemSprayScent spray = player.GetEquippedItem(DollPart.LEFT_HAND) as ItemSprayScent;
            if (spray == null)
            {
                AddMessage(MakeErrorMessage("No spray equipped."));
                RedrawPlayScreen();
                return false;
            }
            if (spray.SprayQuantity <= 0)
            {
                AddMessage(MakeErrorMessage("No spray left."));
                RedrawPlayScreen();
                return false;
            }

            ClearOverlays();
            AddOverlay(new OverlayPopup(SPRAY_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
            do
            {
                ///////////////////
                // 1. Redraw
                // 2. Get input.
                // 3. Handle input
                ///////////////////

                // 1. Redraw
                RedrawPlayScreen();

                // 2. Get input.
                Direction dir = WaitDirectionOrCancel();

                // 3. Handle input
                if (dir == null)
                {
                    loop = false;
                }
                else
                {
                    Actor sprayOn = null;

                    if (dir == Direction.NEUTRAL)
                    {
                        sprayOn = player;
                    }
                    else
                    {
                        Point pos = player.Location.Position + dir;
                        if (player.Location.Map.IsInBounds(pos))
                            sprayOn = player.Location.Map.GetActorAt(pos);
                    }

                    if (sprayOn == null)
                    {
                        AddMessage(MakeErrorMessage("No one to spray on here."));
                        RedrawPlayScreen();
                    }
                    else
                    {
                        string reason;
                        if (m_Rules.CanActorSprayOdorSuppressor(player, spray, sprayOn, out reason))
                        {
                            DoSprayOdorSuppressor(player, spray, sprayOn);
                            loop = false;
                            actionDone = true;
                        }
                        else
                        {
                            AddMessage(MakeErrorMessage(String.Format("Can't spray here : {0}.", reason)));
                            RedrawPlayScreen();
                        }

                    }
                }
            }
            while (loop);

            // cleanup.
            ClearOverlays();

            // return if we did an action.
            return actionDone;
        }

        void StartPlayerWaitLong(Actor player)
        {
            // alpha10.1 check autosave before player starts long wait
            CheckAutoSaveTime();

            // start waiting.
            m_IsPlayerLongWait = true;
            m_IsPlayerLongWaitForcedStop = false;
            m_PlayerLongWaitEnd = new WorldTime(m_Session.WorldTime.TurnCounter + WorldTime.TURNS_PER_HOUR);

            // message.
            AddMessage(MakeMessage(player, String.Format("{0} waiting.", Conjugate(player, VERB_START))));
            RedrawPlayScreen();
        }

        bool CheckPlayerWaitLong(Actor player)
        {
            ///////////////////////
            // Stop waiting if:
            // 1. Force stop wait flag set.
            // 2. Time reached.
            // 3. Hungry or sleepy.
            // 4. Enemy.
            // 5. Sanity check
            ///////////////////////

            // 1. Force stop wait flag set.
            if (m_IsPlayerLongWaitForcedStop)
                return false;

            // 2. Time reached.
            if (m_Session.WorldTime.TurnCounter >= m_PlayerLongWaitEnd.TurnCounter)
                return false;

            // 3. Hungry or sleepy.
            if (m_Rules.IsActorHungry(player) || m_Rules.IsActorStarving(player) || m_Rules.IsActorSleepy(player) || m_Rules.IsActorExhausted(player))
                return false;

            // 4. Enemy.
            foreach (Point p in m_PlayerFOV)
            {
                Actor other = player.Location.Map.GetActorAt(p);
                if (other != null && m_Rules.AreEnemies(player, other))
                    return false;
            }

            // 5. Sanity check
            if (TryPlayerInsanity())
                return false;

            // all clear, waiting not interrupted.
            return true;
        }

        #region Ordering followers
        bool HandlePlayerOrderMode(Actor player)
        {
            // check if we have followers to order.
            if (player.CountFollowers == 0)
            {
                AddMessage(MakeErrorMessage("No followers to give orders to."));
                return false;
            }

            // get followers data.
            Actor[] followers = new Actor[player.CountFollowers];
            HashSet<Point>[] fovs = new HashSet<Point>[player.CountFollowers];
            bool[] hasLinkWith = new bool[player.CountFollowers];
            int iFo = 0;
            foreach (Actor fo in player.Followers)
            {
                followers[iFo] = fo;
                fovs[iFo] = LOS.ComputeFOVFor(m_Rules, fo, m_Session.WorldTime, m_Session.World.Weather);
                bool inView = fovs[iFo].Contains(player.Location.Position) && m_PlayerFOV.Contains(fo.Location.Position);
                bool linkedByPhone = AreLinkedByPhone(player, fo);
                hasLinkWith[iFo] = inView || linkedByPhone;
                ++iFo;
            }

            // if one follower and he's linked, skip selection and directly go to its menu.
            if (player.CountFollowers == 1 && hasLinkWith[0])
            {
                bool done = HandlePlayerOrderFollower(player, followers[0]);
                // cleanup.
                ClearOverlays();
                ClearMessages();
                // done.
                return done;
            }

            // loop.
            bool loop = true;
            bool actionDone = false;
            const int maxFoOnPage = MAX_MESSAGES - 2;
            int iFirstFollower = 0;
            do
            {

                ///////////////////
                // 1. Redraw
                // 2. Get input.
                // 3. Handle input
                ///////////////////

                // 1. Redraw
                ClearOverlays();
                AddOverlay(new OverlayPopup(ORDER_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
                ClearMessages();
                AddMessage(new Message("Choose a follower.", m_Session.WorldTime.TurnCounter, Color.Yellow));
                int foShown;
                for (foShown = 0; foShown < maxFoOnPage && (iFirstFollower + foShown < followers.Length); foShown++)
                {
                    iFo = foShown + iFirstFollower;
                    Actor f = followers[iFo];
                    string desc = DescribePlayerFollowerStatus(f);

                    if (hasLinkWith[iFo])
                        AddMessage(new Message(String.Format("{0}. {1}/{2} {3}... {4}.", (1 + foShown), iFo + 1, followers.Length, f.Name, desc), m_Session.WorldTime.TurnCounter, Color.LightGreen));
                    else
                        AddMessage(new Message(String.Format("{0}. {1}/{2} ({3}) {4}.", (1 + foShown), iFo + 1, followers.Length, f.Name, desc), m_Session.WorldTime.TurnCounter, Color.DarkGray));
                }
                if (foShown < followers.Length)
                {
                    AddMessage(new Message("9. next", m_Session.WorldTime.TurnCounter, Color.LightGreen));
                }
                RedrawPlayScreen();

                // 2. Get input.
                KeyEventArgs key = m_UI.UI_WaitKey();
                int choice = KeyToChoiceNumber(key.KeyCode);

                // 3. Handle input
                if (key.KeyCode == Keys.Escape)
                {
                    loop = false;
                }
                else if (choice == 9)
                {
                    iFirstFollower += maxFoOnPage;
                    if (iFirstFollower >= followers.Length)
                        iFirstFollower = 0;
                }
                else if (choice >= 1 && choice <= foShown)
                {
                    // Follower must be linked.
                    int f = iFirstFollower + choice - 1;
                    if (hasLinkWith[f])
                    {
                        /////////////////////////////////////////////
                        // Follower selected, select directive/order
                        /////////////////////////////////////////////
                        Actor selectedFollower = followers[f];
                        if (HandlePlayerOrderFollower(player, selectedFollower))
                        {
                            loop = false;
                            actionDone = true;
                        }
                    }
                }
            }
            while (loop);

            // cleanup.
            ClearOverlays();
            ClearMessages();

            // return if we did an action.
            return actionDone;
        }

        bool HandlePlayerDirectiveFollower(Actor player, Actor follower)
        {
            bool loop = true;
            bool actionDone = false;

            // loop.
            do
            {
                ///////////////////
                // 1. Redraw
                // 2. Get input.
                // 3. Handle input
                ///////////////////
                ActorDirective directives = (follower.Controller as AIController).Directives;

                // 1. Redraw
                ClearOverlays();
                AddOverlay(new OverlayPopup(ORDER_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
                ClearMessages();
                AddMessage(new Message(String.Format("{0} directives...", follower.Name), m_Session.WorldTime.TurnCounter, Color.Yellow));
                AddMessage(new Message(String.Format("1. {0} items.", directives.CanTakeItems ? "Take" : "Don't take"), m_Session.WorldTime.TurnCounter, Color.LightGreen));
                AddMessage(new Message(String.Format("2. {0} weapons.", directives.CanFireWeapons ? "Fire" : "Don't fire"), m_Session.WorldTime.TurnCounter, Color.LightGreen));
                AddMessage(new Message(String.Format("3. {0} grenades.", directives.CanThrowGrenades ? "Throw" : "Don't throw"), m_Session.WorldTime.TurnCounter, Color.LightGreen));
                AddMessage(new Message(String.Format("4. {0}.", directives.CanSleep ? "Sleep" : "Don't sleep"), m_Session.WorldTime.TurnCounter, Color.LightGreen));
                AddMessage(new Message(String.Format("5. {0}.", directives.CanTrade ? "Trade" : "Don't trade"), m_Session.WorldTime.TurnCounter, Color.LightGreen));
                AddMessage(new Message(String.Format("6. {0}.", ActorDirective.CourageString(directives.Courage)), m_Session.WorldTime.TurnCounter, Color.LightGreen));
                RedrawPlayScreen();

                // 2. Get input.
                KeyEventArgs key = m_UI.UI_WaitKey();
                int choice = KeyToChoiceNumber(key.KeyCode);

                // 3. Handle input
                if (key.KeyCode == Keys.Escape)
                {
                    loop = false;
                }
                else if (choice >= 1 && choice <= 6)
                {
                    switch (choice)
                    {
                        case 1: // items
                            directives.CanTakeItems = !directives.CanTakeItems;
                            break;
                        case 2: // weapons
                            directives.CanFireWeapons = !directives.CanFireWeapons;
                            break;
                        case 3: // grenades.
                            directives.CanThrowGrenades = !directives.CanThrowGrenades;
                            break;
                        case 4: // sleep
                            directives.CanSleep = !directives.CanSleep;
                            break;
                        case 5: // trade
                            directives.CanTrade = !directives.CanTrade;
                            break;
                        case 6:  // courage: coward -> cautious -> courageous.
                            switch (directives.Courage)
                            {
                                case ActorCourage.COWARD:
                                    directives.Courage = ActorCourage.CAUTIOUS;
                                    break;
                                case ActorCourage.CAUTIOUS:
                                    directives.Courage = ActorCourage.COURAGEOUS;
                                    break;
                                case ActorCourage.COURAGEOUS:
                                    directives.Courage = ActorCourage.COWARD;
                                    break;
                            }
                            break;
                    }
                }
            }
            while (loop);

            return actionDone;
        }

        bool HandlePlayerOrderFollower(Actor player, Actor follower)
        {
            // check trust.
            if (!m_Rules.IsActorTrustingLeader(follower))
            {
                // say/phone
                if (IsVisibleToPlayer(follower))
                    DoSay(follower, player, "Sorry, I don't trust you enough yet.", Sayflags.IS_FREE_ACTION | Sayflags.IS_IMPORTANT);
                else if (AreLinkedByPhone(follower, player))
                {
                    ClearMessages();
                    AddMessage(MakeMessage(follower, "Sorry, I don't trust you enough yet."));
                    AddMessagePressEnter();
                }
                // refuse!
                return false;
            }

            // current order.
            string desc = DescribePlayerFollowerStatus(follower);

            // compute follower fov.
            HashSet<Point> followerFOV = LOS.ComputeFOVFor(m_Rules, follower, m_Session.WorldTime, m_Session.World.Weather);

            // loop.
            bool loop = true;
            bool actionDone = false;
            do
            {
                ///////////////////
                // 1. Redraw
                // 2. Get input.
                // 3. Handle input
                ///////////////////

                // 1. Redraw
                string startStopFollow = (follower.Controller as OrderableAI).DontFollowLeader ? "Start" : "Stop";
                ClearOverlays();
                AddOverlay(new OverlayPopup(ORDER_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
                ClearMessages();
                AddMessage(new Message(String.Format("Order {0} to...", follower.Name), m_Session.WorldTime.TurnCounter, Color.Yellow));
                AddMessage(new Message(String.Format("0. Cancel current order {0}.", desc), m_Session.WorldTime.TurnCounter, Color.Green));
                AddMessage(new Message("1. Set directives...", m_Session.WorldTime.TurnCounter, Color.Cyan));
                AddMessage(new Message("2. Barricade (one)...    6. Drop all items.      A. Give me...", m_Session.WorldTime.TurnCounter, Color.LightGreen));
                AddMessage(new Message("3. Barricade (max)...    7. Build small fort.    B. Sleep now.", m_Session.WorldTime.TurnCounter, Color.LightGreen));
                AddMessage(new Message(String.Format("4. Guard...              8. Build large fort.    C. {0} following me.   ", startStopFollow), m_Session.WorldTime.TurnCounter, Color.LightGreen));
                AddMessage(new Message("5. Patrol...             9. Report events.       D. Where are you?", m_Session.WorldTime.TurnCounter, Color.LightGreen));
                RedrawPlayScreen();

                // 2. Get input.
                KeyEventArgs key = m_UI.UI_WaitKey();
                int choice = KeyToChoiceNumber(key.KeyCode);

                // 3. Handle input
                if (key.KeyCode == Keys.Escape)
                {
                    loop = false;
                }
                // first set of choices 0-9
                else if (choice >= 0 && choice <= 9)
                {

                    #region
                    switch (choice)
                    {
                        case 0: // cancel current order
                            DoCancelOrder(player, follower);
                            loop = false;
                            actionDone = true;
                            break;

                        case 1: // set directives.
                            HandlePlayerDirectiveFollower(player, follower);
                            break;

                        case 2: // barricade (one)
                            if (HandlePlayerOrderFollowerToBarricade(player, follower, followerFOV, false))
                            {
                                loop = false;
                                actionDone = true;
                            }
                            break;

                        case 3: // barricade (max)
                            if (HandlePlayerOrderFollowerToBarricade(player, follower, followerFOV, true))
                            {
                                loop = false;
                                actionDone = true;
                            }
                            break;

                        case 4: // guard
                            if (HandlePlayerOrderFollowerToGuard(player, follower, followerFOV))
                            {
                                loop = false;
                                actionDone = true;
                            }
                            break;

                        case 5: // patrol
                            if (HandlePlayerOrderFollowerToPatrol(player, follower, followerFOV))
                            {
                                loop = false;
                                actionDone = true;
                            }
                            break;

                        case 6: // drop all items
                            if (HandlePlayerOrderFollowerToDropAllItems(player, follower))
                            {
                                loop = false;
                                actionDone = true;
                            }
                            break;

                        case 7: // build small fort.
                            if (HandlePlayerOrderFollowerToBuildFortification(player, follower, followerFOV, false))
                            {
                                loop = false;
                                actionDone = true;
                            }
                            break;

                        case 8: // build large fort.
                            if (HandlePlayerOrderFollowerToBuildFortification(player, follower, followerFOV, true))
                            {
                                loop = false;
                                actionDone = true;
                            }
                            break;

                        case 9: // report
                            if (HandlePlayerOrderFollowerToReport(player, follower))
                            {
                                loop = false;
                                actionDone = true;
                            }
                            break;

                    }
                    #endregion
                }
                // second set of choices A-xxx
                #region
                else
                {
                    switch (key.KeyCode)
                    {
                        case Keys.A:    // give items...
                            if (HandlePlayerOrderFollowerToGiveItems(player, follower))
                            {
                                loop = false;
                                actionDone = true;
                            }
                            break;

                        case Keys.B: // sleep now
                            if (HandlePlayerOrderFollowerToSleep(player, follower))
                            {
                                loop = false;
                                actionDone = true;
                            }
                            break;

                        case Keys.C: // toggle follow
                            if (HandlePlayerOrderFollowerToToggleFollow(player, follower))
                            {
                                loop = false;
                                actionDone = true;
                            }
                            break;

                        case Keys.D: // where are ou?
                            if (HandlePlayerOrderFollowerToReportPosition(player, follower))
                            {
                                loop = false;
                                actionDone = true;
                            }
                            break;
                    }
                }
                #endregion
            }
            while (loop);

            // return if we did an action.
            return actionDone;
        }

        bool HandlePlayerOrderFollowerToBuildFortification(Actor player, Actor follower, HashSet<Point> followerFOV, bool isLarge)
        {
            bool loop = true;
            bool actionDone = false;
            Map map = player.Location.Map;
            Point? highlightedTile = null;
            Color highlightColor = Color.White;

            do
            {
                ///////////////////
                // 1. Redraw
                // 2. Get input.
                // 3. Handle input
                ///////////////////

                // 1. Redraw
                ClearOverlays();
                AddOverlay(new OverlayPopup(ORDER_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
                if (highlightedTile != null)
                    AddOverlay(new OverlayRect(highlightColor, new Rectangle(MapToScreen(highlightedTile.Value.X, highlightedTile.Value.Y), new Size(TILE_SIZE, TILE_SIZE))));
                ClearMessages();
                AddMessage(new Message(String.Format("Ordering {0} to build {1} fortification...", follower.Name, isLarge ? "large" : "small"), m_Session.WorldTime.TurnCounter, Color.Yellow));
                AddMessage(new Message("<LMB> on a map object.", m_Session.WorldTime.TurnCounter, Color.LightGreen));
                RedrawPlayScreen();

                // 2. Get input.
                KeyEventArgs key;
                Point mousePos;
                MouseButtons? mouseButtons;
                WaitKeyOrMouse(out key, out mousePos, out mouseButtons);

                if (key != null)
                {
                    if (key.KeyCode == Keys.Escape)
                        loop = false;
                }
                else
                {
                    // Get map position in view rect.
                    Point mapPos = MouseToMap(mousePos);
                    if (map.IsInBounds(mapPos) && IsInViewRect(mapPos))
                    {
                        // must be in player & follower FoV.
                        if (IsVisibleToPlayer(map, mapPos) && followerFOV.Contains(mapPos))
                        {
                            // Check if can build here.
                            string reason;
                            if (m_Rules.CanActorBuildFortification(follower, mapPos, isLarge, out reason))
                            {
                                // highlight.
                                highlightedTile = mapPos;
                                highlightColor = Color.LightGreen;
                                // if mouse down, give order.
                                if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left)
                                {
                                    DoGiveOrderTo(player, follower, new ActorOrder(isLarge ? ActorTasks.BUILD_LARGE_FORTIFICATION : ActorTasks.BUILD_SMALL_FORTIFICATION, new Location(player.Location.Map, mapPos)));
                                    loop = false;
                                    actionDone = true;
                                }
                            }
                            else
                            {
                                // de-hightlight.
                                highlightedTile = mapPos;
                                highlightColor = Color.Red;

                                // if mouse down, illegal.
                                if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left)
                                {
                                    AddMessage(MakeErrorMessage(String.Format("Can't build {0} fortification : {1}.", isLarge ? "large" : "small", reason)));
                                    AddMessagePressEnter();
                                }
                            }
                        } // visible
                        else
                        {
                            // de-hightlight.
                            highlightedTile = mapPos;
                            highlightColor = Color.Red;
                        }
                    }
                }

            }
            while (loop);

            // return if we did an action.
            return actionDone;
        }

        bool HandlePlayerOrderFollowerToBarricade(Actor player, Actor follower, HashSet<Point> followerFOV, bool toTheMax)
        {
            bool loop = true;
            bool actionDone = false;
            Map map = player.Location.Map;
            Point? highlightedTile = null;
            Color highlightColor = Color.White;

            do
            {
                ///////////////////
                // 1. Redraw
                // 2. Get input.
                // 3. Handle input
                ///////////////////

                // 1. Redraw
                ClearOverlays();
                AddOverlay(new OverlayPopup(ORDER_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
                if (highlightedTile != null)
                    AddOverlay(new OverlayRect(highlightColor, new Rectangle(MapToScreen(highlightedTile.Value.X, highlightedTile.Value.Y), new Size(TILE_SIZE, TILE_SIZE))));
                ClearMessages();
                AddMessage(new Message(String.Format("Ordering {0} to barricade...", follower.Name), m_Session.WorldTime.TurnCounter, Color.Yellow));
                AddMessage(new Message("<LMB> on a map object.", m_Session.WorldTime.TurnCounter, Color.LightGreen));
                RedrawPlayScreen();

                // 2. Get input.
                KeyEventArgs key;
                Point mousePos;
                MouseButtons? mouseButtons;
                WaitKeyOrMouse(out key, out mousePos, out mouseButtons);

                if (key != null)
                {
                    if (key.KeyCode == Keys.Escape)
                        loop = false;
                }
                else
                {
                    // Get map position in view rect.
                    Point mapPos = MouseToMap(mousePos);
                    if (map.IsInBounds(mapPos) && IsInViewRect(mapPos))
                    {
                        // must be in player & follower FoV.
                        if (IsVisibleToPlayer(map, mapPos) && followerFOV.Contains(mapPos))
                        {

                            // Check if something to barricade here.
                            DoorWindow door = map.GetMapObjectAt(mapPos) as DoorWindow;
                            if (door != null)
                            {
                                // Check if can barricade here.
                                string reason;
                                if (m_Rules.CanActorBarricadeDoor(follower, door, out reason))
                                {
                                    // highlight.
                                    highlightedTile = mapPos;
                                    highlightColor = Color.LightGreen;
                                    // if mouse down, give order.
                                    if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left)
                                    {
                                        DoGiveOrderTo(player, follower, new ActorOrder(toTheMax ? ActorTasks.BARRICADE_MAX : ActorTasks.BARRICADE_ONE, door.Location));
                                        loop = false;
                                        actionDone = true;
                                    }
                                }
                                else
                                {
                                    // de-hightlight.
                                    highlightedTile = mapPos;
                                    highlightColor = Color.Red;
                                    // if mouse down, illegal.
                                    if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left)
                                    {
                                        AddMessage(MakeErrorMessage(String.Format("Can't barricade {0} : {1}.", door.TheName, reason)));
                                        AddMessagePressEnter();
                                    }
                                }
                            }
                            else
                            {
                                // de-hightlight.
                                highlightedTile = mapPos;
                                highlightColor = Color.Red;
                            }
                        } // visible
                        else
                        {
                            // de-hightlight.
                            highlightedTile = mapPos;
                            highlightColor = Color.Red;
                        }
                    }
                }

            }
            while (loop);

            // return if we did an action.
            return actionDone;
        }

        bool HandlePlayerOrderFollowerToGuard(Actor player, Actor follower, HashSet<Point> followerFOV)
        {
            bool loop = true;
            bool actionDone = false;
            Map map = player.Location.Map;
            Point? highlightedTile = null;
            Color highlightColor = Color.White;

            do
            {
                ///////////////////
                // 1. Redraw
                // 2. Get input.
                // 3. Handle input
                ///////////////////

                // 1. Redraw
                ClearOverlays();
                AddOverlay(new OverlayPopup(ORDER_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
                if (highlightedTile != null)
                    AddOverlay(new OverlayRect(highlightColor, new Rectangle(MapToScreen(highlightedTile.Value.X, highlightedTile.Value.Y), new Size(TILE_SIZE, TILE_SIZE))));
                ClearMessages();
                AddMessage(new Message(String.Format("Ordering {0} to guard...", follower.Name), m_Session.WorldTime.TurnCounter, Color.Yellow));
                AddMessage(new Message("<LMB> on a map position.", m_Session.WorldTime.TurnCounter, Color.LightGreen));
                RedrawPlayScreen();

                // 2. Get input.
                KeyEventArgs key;
                Point mousePos;
                MouseButtons? mouseButtons;
                WaitKeyOrMouse(out key, out mousePos, out mouseButtons);

                if (key != null)
                {
                    if (key.KeyCode == Keys.Escape)
                        loop = false;
                }
                else
                {
                    // Get map position in view rect.
                    Point mapPos = MouseToMap(mousePos);
                    if (map.IsInBounds(mapPos) && IsInViewRect(mapPos))
                    {
                        // must be in player & follower FoV.
                        if (IsVisibleToPlayer(map, mapPos) && followerFOV.Contains(mapPos))
                        {
                            // Check if walkable here or same spot.
                            string reason;
                            if (mapPos == follower.Location.Position || m_Rules.IsWalkableFor(follower, map, mapPos.X, mapPos.Y, out reason))
                            {
                                // highlight.
                                highlightedTile = mapPos;
                                highlightColor = Color.LightGreen;
                                // if mouse down, give order.
                                if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left)
                                {
                                    DoGiveOrderTo(player, follower, new ActorOrder(ActorTasks.GUARD, new Location(map, mapPos)));
                                    loop = false;
                                    actionDone = true;
                                }
                            }
                            else
                            {
                                // de-hightlight.
                                highlightedTile = mapPos;
                                highlightColor = Color.Red;
                                // if mouse down, illegal.
                                if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left)
                                {
                                    AddMessage(MakeErrorMessage(String.Format("Can't guard here : {0}", reason)));
                                    AddMessagePressEnter();
                                }
                            }

                        } // visible
                        else
                        {
                            // de-hightlight.
                            highlightedTile = mapPos;
                            highlightColor = Color.Red;
                        }
                    }
                }

            }
            while (loop);

            // return if we did an action.
            return actionDone;
        }

        bool HandlePlayerOrderFollowerToPatrol(Actor player, Actor follower, HashSet<Point> followerFOV)
        {
            bool loop = true;
            bool actionDone = false;
            Map map = player.Location.Map;
            Point? highlightedTile = null;
            Color highlightColor = Color.White;

            do
            {
                ///////////////////
                // 1. Redraw
                // 2. Get input.
                // 3. Handle input
                ///////////////////

                // 1. Redraw
                ClearOverlays();
                AddOverlay(new OverlayPopup(ORDER_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
                if (highlightedTile != null)
                {
                    AddOverlay(new OverlayRect(highlightColor, new Rectangle(MapToScreen(highlightedTile.Value.X, highlightedTile.Value.Y), new Size(TILE_SIZE, TILE_SIZE))));
                    List<Zone> zonesHere = map.GetZonesAt(highlightedTile.Value.X, highlightedTile.Value.Y);
                    if (zonesHere != null && zonesHere.Count > 0)
                    {
                        string[] zonesNames = new string[zonesHere.Count + 1];
                        zonesNames[0] = "Zone(s) here :";
                        for (int i = 0; i < zonesHere.Count; i++)
                        {
                            zonesNames[i + 1] = String.Format("- {0}", zonesHere[i].Name);
                        }
                        AddOverlay(new OverlayPopup(zonesNames, Color.White, Color.White, POPUP_FILLCOLOR, MapToScreen(highlightedTile.Value.X + 1, highlightedTile.Value.Y + 1)));
                    }
                }
                ClearMessages();
                AddMessage(new Message(String.Format("Ordering {0} to patrol...", follower.Name), m_Session.WorldTime.TurnCounter, Color.Yellow));
                AddMessage(new Message("<LMB> on a map position.", m_Session.WorldTime.TurnCounter, Color.LightGreen));
                RedrawPlayScreen();

                // 2. Get input.
                KeyEventArgs key;
                Point mousePos;
                MouseButtons? mouseButtons;
                WaitKeyOrMouse(out key, out mousePos, out mouseButtons);

                if (key != null)
                {
                    if (key.KeyCode == Keys.Escape)
                        loop = false;
                }
                else
                {
                    // Get map position in view rect.
                    Point mapPos = MouseToMap(mousePos);
                    if (map.IsInBounds(mapPos) && IsInViewRect(mapPos))
                    {
                        // must be in player & follower FoV.
                        if (IsVisibleToPlayer(map, mapPos) && followerFOV.Contains(mapPos))
                        {
                            bool validPatrol = true;
                            string reason = "";

                            // Must have a zone.
                            if (map.GetZonesAt(mapPos.X, mapPos.Y) == null)
                            {
                                validPatrol = false;
                                reason = "no zone here";
                            }
                            // Check if walkable here or same spot.
                            else if (!(mapPos == follower.Location.Position || m_Rules.IsWalkableFor(follower, map, mapPos.X, mapPos.Y, out reason)))
                            {
                                validPatrol = false;
                            }

                            if (validPatrol)
                            {
                                // highlight.
                                highlightedTile = mapPos;
                                highlightColor = Color.LightGreen;
                                // if mouse down, give order.
                                if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left)
                                {
                                    DoGiveOrderTo(player, follower, new ActorOrder(ActorTasks.PATROL, new Location(map, mapPos)));
                                    loop = false;
                                    actionDone = true;
                                }
                            }
                            else
                            {
                                // de-hightlight.
                                highlightedTile = mapPos;
                                highlightColor = Color.Red;
                                // if mouse down, illegal.
                                if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left)
                                {
                                    AddMessage(MakeErrorMessage(String.Format("Can't patrol here : {0}", reason)));
                                    AddMessagePressEnter();
                                }
                            }
                        } // visible
                        else
                        {
                            // de-hightlight.
                            highlightedTile = mapPos;
                            highlightColor = Color.Red;
                        }
                    }
                }
            }
            while (loop);

            // return if we did an action.
            return actionDone;
        }

        bool HandlePlayerOrderFollowerToDropAllItems(Actor player, Actor follower)
        {
            // if no items, nothing to drop.
            if (follower.Inventory.IsEmpty)
                return false;

            // do give order.
            DoGiveOrderTo(player, follower, new ActorOrder(ActorTasks.DROP_ALL_ITEMS, follower.Location));

            // emote.
            DoSay(follower, player, "Well ok...", Sayflags.IS_FREE_ACTION);

            // update trust. 1 give item penalty per items to drop.
            ModifyActorTrustInLeader(follower, follower.Inventory.CountItems * Rules.TRUST_GIVE_ITEM_ORDER_PENALTY, true);

            // done.
            return true;
        }

        bool HandlePlayerOrderFollowerToReport(Actor player, Actor follower)
        {
            // just give the order.
            DoGiveOrderTo(player, follower, new ActorOrder(ActorTasks.REPORT_EVENTS, follower.Location));

            // done.
            return true;
        }

        bool HandlePlayerOrderFollowerToSleep(Actor player, Actor follower)
        {
            // just give the order.
            DoGiveOrderTo(player, follower, new ActorOrder(ActorTasks.SLEEP_NOW, follower.Location));

            // done.
            return true;
        }

        bool HandlePlayerOrderFollowerToToggleFollow(Actor player, Actor follower)
        {
            // just give the order.
            DoGiveOrderTo(player, follower, new ActorOrder(ActorTasks.FOLLOW_TOGGLE, follower.Location));

            // done.
            return true;
        }

        bool HandlePlayerOrderFollowerToReportPosition(Actor player, Actor follower)
        {
            // just give the order.
            DoGiveOrderTo(player, follower, new ActorOrder(ActorTasks.WHERE_ARE_YOU, follower.Location));

            // done.
            return true;
        }

        bool HandlePlayerOrderFollowerToGiveItems(Actor player, Actor follower)
        {
            // sanity checks.
            if (follower.Inventory == null || follower.Inventory.IsEmpty)
            {
                ClearMessages();
                AddMessage(MakeErrorMessage(String.Format("{0} has no items to give.", follower.TheName)));
                AddMessagePressEnter();
                return false;
            }
            // must be adjacent.
            if (player.Location.Map != follower.Location.Map || !m_Rules.IsAdjacent(player.Location.Position, follower.Location.Position))
            {
                ClearMessages();
                AddMessage(MakeErrorMessage(String.Format("{0} is not next to you.", follower.TheName)));
                AddMessagePressEnter();
                return false;
            }

            // loop.
            bool loop = true;
            bool actionDone = false;

            int iFirstItem = 0;
            const int maxItOnPage = MAX_MESSAGES - 2;
            Inventory foInventory = follower.Inventory;
            do
            {
                ///////////////////
                // 1. Redraw
                // 2. Get input.
                // 3. Handle input
                ///////////////////

                // 1. Redraw
                ClearOverlays();
                AddOverlay(new OverlayPopup(ORDER_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
                ClearMessages();
                AddMessage(new Message(String.Format("Ordering {0} to give...", follower.Name), m_Session.WorldTime.TurnCounter, Color.Yellow));

                int itShown;
                for (itShown = 0; itShown < maxItOnPage && (iFirstItem + itShown < foInventory.CountItems); itShown++)
                {
                    int iIt = iFirstItem + itShown;
                    AddMessage(new Message(String.Format("{0}. {1}/{2} {3}.", (1 + itShown), iIt + 1, foInventory.CountItems, DescribeItemShort(foInventory[iIt])), m_Session.WorldTime.TurnCounter, Color.LightGreen));
                }
                if (itShown < foInventory.CountItems)
                {
                    AddMessage(new Message("9. next", m_Session.WorldTime.TurnCounter, Color.LightGreen));
                }
                RedrawPlayScreen();

                // 2. Get input.
                KeyEventArgs key = m_UI.UI_WaitKey();
                int choice = KeyToChoiceNumber(key.KeyCode);

                // 3. Handle input
                if (key.KeyCode == Keys.Escape)
                {
                    loop = false;
                }
                else if (choice == 9)
                {
                    iFirstItem += maxItOnPage;
                    if (iFirstItem >= foInventory.CountItems)
                        iFirstItem = 0;
                }
                else if (choice >= 1 && choice <= itShown)
                {
                    // get item.
                    int i = iFirstItem + choice - 1;
                    Item it = foInventory[i];

                    // try to do it.
                    string reason;
                    if (m_Rules.CanActorGiveItemTo(follower, player, it, out reason))
                    {
                        DoGiveItemTo(follower, m_Player, it);
                        loop = false;
                        actionDone = true;
                    }
                    else
                    {
                        ClearMessages();
                        AddMessage(MakeErrorMessage(String.Format("{0} cannot give {1} : {2}.", follower.TheName, DescribeItemShort(it), reason)));
                        AddMessagePressEnter();
                    }
                }
            }
            while (loop);

            // done.
            return actionDone;
        }
        #endregion
        #endregion

        void HandleAiActor(Actor aiActor)
        {
            // Get and perform action from AI controler.
            ActorAction desiredAction = aiActor.Controller.GetAction(this);

            // Insane effect?
            if (m_Rules.IsActorInsane(aiActor) && m_Rules.RollChance(Rules.SANITY_INSANE_ACTION_CHANCE))
            {
                ActorAction insaneAction = GenerateInsaneAction(aiActor);
                if (insaneAction != null && insaneAction.IsLegal())
                    desiredAction = insaneAction;
            }

            // Do action.
            if (desiredAction != null)
            {
                if (desiredAction.IsLegal())
                    desiredAction.Perform();
                else
                {
                    // AI attempted illegal action.                    
                    SpendActorActionPoints(aiActor, Rules.BASE_ACTION_COST);

                    // alpha10.1 
                    // in debug build, throw exception.
                    // in release build just complain and do a wait action instead.
#if DEBUG
                    throw new InvalidOperationException(String.Format("AI attempted illegal action {0}; actorAI: {1}; fail reason : {2}.",
                        desiredAction.GetType().ToString(), aiActor.Controller.GetType().ToString(), desiredAction.FailReason));
#else
                    DoWait(aiActor);
                    DoEmote(aiActor, "My AI attempted an illegal action, I'll  wait instead of crashing your game :)", true);
#endif
                }
            }
            else
                throw new InvalidOperationException("AI returned null action.");
        }
        #endregion

        #region Advisor hints.
        void HandleAdvisor(Actor player)
        {
            ///////////////////////////////
            // If all hints given, say so.
            ///////////////////////////////
            if (s_Hints.HasAdvisorGivenAllHints())
            {
                ShowAdvisorMessage(
                    "YOU KNOW THE BASICS!",
                    new string[] {
                        "The Advisor has given you all the hints.",
                        "You can disable the advisor in the options.",
                        "Read the manual or discover the rest of the game by yourself.",
                        "Good luck and have fun!",
                        String.Format("To REDEFINE THE KEYS : <{0}>.", s_KeyBindings.Get(PlayerCommand.KEYBINDING_MODE).ToString()),
                        String.Format("To CHANGE OPTIONS    : <{0}>.", s_KeyBindings.Get(PlayerCommand.OPTIONS_MODE).ToString()),
                        String.Format("To READ THE MANUAL   : <{0}>.", s_KeyBindings.Get(PlayerCommand.HELP_MODE).ToString())
                    });
                return;
            }

            /////////////////////////////////
            // Show the first appliable hint.
            /////////////////////////////////

            for (int i = (int)AdvisorHint._FIRST; i < (int)AdvisorHint._COUNT; i++)
            {
                if (s_Hints.IsAdvisorHintGiven((AdvisorHint)i))
                    continue;
                if (IsAdvisorHintAppliable((AdvisorHint)i))
                {
                    AdvisorGiveHint((AdvisorHint)i);
                    return;
                }
            }

            // no hint.
            ShowAdvisorMessage("No hint available.",
                new string[] {
                    "The Advisor has now new hint for you in this situation.",
                    "You will see a popup when he has something to say.",
                    String.Format("To REDEFINE THE KEYS : <{0}>.", s_KeyBindings.Get(PlayerCommand.KEYBINDING_MODE).ToString()),
                    String.Format("To CHANGE OPTIONS    : <{0}>.", s_KeyBindings.Get(PlayerCommand.OPTIONS_MODE).ToString()),
                    String.Format("To READ THE MANUAL   : <{0}>.", s_KeyBindings.Get(PlayerCommand.HELP_MODE).ToString())
                });
        }

        // alpha10 obsolete
        //bool HasAdvisorAnyHintToGive()
        //{
        //    for (int i = (int)AdvisorHint._FIRST; i < (int)AdvisorHint._COUNT; i++)
        //    {
        //        if (s_Hints.IsAdvisorHintGiven((AdvisorHint)i))
        //            continue;
        //        if (IsAdvisorHintAppliable((AdvisorHint)i))
        //            return true;
        //    }

        //    return false;
        //}

        // alpha10
        /// <summary>
        /// 
        /// </summary>
        /// <returns>-1 if none</returns>
        int GetAdvisorFirstAvailableHint()
        {
            for (int i = (int)AdvisorHint._FIRST; i < (int)AdvisorHint._COUNT; i++)
            {
                if (s_Hints.IsAdvisorHintGiven((AdvisorHint)i))
                    continue;
                if (IsAdvisorHintAppliable((AdvisorHint)i))
                    return i;
            }

            return -1;
        }

        void AdvisorGiveHint(AdvisorHint hint)
        {
            /////////////////
            // Mark as given
            /////////////////
            s_Hints.SetAdvisorHintAsGiven(hint);

            ////////////////
            // Save status.
            ////////////////
            SaveHints();

            /////////
            // Show
            /////////
            ShowAdvisorHint(hint);
        }

        bool IsAdvisorHintAppliable(AdvisorHint hint)
        {
            Map map = m_Player.Location.Map;
            Point pos = m_Player.Location.Position;

            switch (hint)
            {
                case AdvisorHint.ACTOR_MELEE:   // adjacent to an enemy.
                    return IsAdjacentToEnemy(map, pos, m_Player);

                case AdvisorHint.BARRICADE:  // barricading.
                    return map.HasAnyAdjacentInMap(pos, (pt) =>
                        {
                            DoorWindow door = map.GetMapObjectAt(pt) as DoorWindow;
                            if (door == null)
                                return false;
                            return m_Rules.CanActorBarricadeDoor(m_Player, door);
                        });

                case AdvisorHint.BUILD_FORTIFICATION: // building fortifications.
                    return map.HasAnyAdjacentInMap(pos, (pt) =>
                    {
                        return m_Rules.CanActorBuildFortification(m_Player, pt, false);
                    });

                case AdvisorHint.CELLPHONES:
                    return m_Player.Inventory.GetFirstByModel(GameItems.CELL_PHONE) != null;

                case AdvisorHint.CITY_INFORMATION:  // city information, wait a bit...
                    return map.LocalTime.Hour >= 12;

                case AdvisorHint.CORPSE:
                    return !m_Player.Model.Abilities.IsUndead && map.GetCorpsesAt(pos) != null;

                case AdvisorHint.CORPSE_EAT:
                    return m_Player.Model.Abilities.IsUndead && map.GetCorpsesAt(pos) != null;

                case AdvisorHint.DOORWINDOW_OPEN:   // can open an adj door/window.
                    return map.HasAnyAdjacentInMap(pos, (pt) =>
                        {
                            DoorWindow door = map.GetMapObjectAt(pt) as DoorWindow;
                            if (door == null)
                                return false;
                            return m_Rules.IsOpenableFor(m_Player, door);
                        });

                case AdvisorHint.DOORWINDOW_CLOSE:   // can close an open door/window.
                    return map.HasAnyAdjacentInMap(pos, (pt) =>
                    {
                        DoorWindow door = map.GetMapObjectAt(pt) as DoorWindow;
                        if (door == null)
                            return false;
                        return m_Rules.IsClosableFor(m_Player, door);
                    });

                case AdvisorHint.EXIT_STAIRS_LADDERS:  // using stairs, laders.
                    return map.GetExitAt(pos) != null;

                case AdvisorHint.EXIT_LEAVING_DISTRICT: // leaving the district.
                    {
                        foreach (Direction d in Direction.COMPASS)
                        {
                            Point pt = pos + d;
                            if (map.IsInBounds(pt))
                                continue;
                            if (map.GetExitAt(pt) != null)
                                return true;
                        }
                        return false;
                    }

                case AdvisorHint.FLASHLIGHT:
                    return m_Player.Inventory.HasItemOfType(typeof(ItemLight));

                case AdvisorHint.GAME_SAVE_LOAD:    // saving/loading. wait a bit...
                    return map.LocalTime.Hour >= 7;

                case AdvisorHint.GRENADE:
                    {
                        Inventory inv = m_Player.Inventory;
                        if (inv == null || inv.IsEmpty)
                            return false;
                        return inv.HasItemOfType(typeof(ItemGrenade));
                    }

                case AdvisorHint.ITEM_GRAB_CONTAINER: // can take an item from an adjacent container.
                    return map.HasAnyAdjacentInMap(pos, (pt) => m_Rules.CanActorGetItemFromContainer(m_Player, pt));

                case AdvisorHint.ITEM_GRAB_FLOOR:   // can take an item from the flor.
                    {
                        Inventory invThere = map.GetItemsAt(pos);
                        if (invThere == null)
                            return false;
                        foreach (Item it in invThere.Items)
                            if (m_Rules.CanActorGetItem(m_Player, it))
                                return true;
                        return false;
                    }

                case AdvisorHint.ITEM_EQUIP:  // equip an item.
                    {
                        Inventory inv = m_Player.Inventory;
                        if (inv == null || inv.IsEmpty)
                            return false;
                        foreach (Item it in inv.Items)
                            if (!it.IsEquipped && m_Rules.CanActorEquipItem(m_Player, it))
                                return true;
                        return false;
                    }

                case AdvisorHint.ITEM_UNEQUIP:  // unequip an item.
                    {
                        Inventory inv = m_Player.Inventory;
                        if (inv == null || inv.IsEmpty)
                            return false;
                        foreach (Item it in inv.Items)
                            if (m_Rules.CanActorUnequipItem(m_Player, it))
                                return true;
                        return false;
                    }

                case AdvisorHint.ITEM_DROP: // dropping an item.
                    {
                        Inventory inv = m_Player.Inventory;
                        if (inv == null || inv.IsEmpty)
                            return false;
                        foreach (Item it in inv.Items)
                            if (m_Rules.CanActorDropItem(m_Player, it))
                                return true;
                        return false;
                    }

                case AdvisorHint.ITEM_TYPE_BARRICADING: // barricading material.
                    {
                        Inventory inv = m_Player.Inventory;
                        if (inv == null || inv.IsEmpty)
                            return false;
                        return inv.HasItemOfType(typeof(ItemBarricadeMaterial));
                    }

                case AdvisorHint.ITEM_USE: // using an item.
                    {
                        Inventory inv = m_Player.Inventory;
                        if (inv == null || inv.IsEmpty)
                            return false;
                        foreach (Item it in inv.Items)
                            if (m_Rules.CanActorUseItem(m_Player, it))
                                return true;
                        return false;
                    }

                case AdvisorHint.KEYS_OPTIONS:  // redefining keys & options.
                    return true;

                case AdvisorHint.LEADING_CAN_RECRUIT:   // can recruit follower.
                    return map.HasAnyAdjacentInMap(pos, (pt) =>
                        {
                            Actor other = map.GetActorAt(pt);
                            if (other == null)
                                return false;
                            return m_Rules.CanActorTakeLead(m_Player, other);
                        });

                case AdvisorHint.LEADING_GIVE_ORDERS:   // give orders to followers.
                    return m_Player.CountFollowers > 0;

                case AdvisorHint.LEADING_NEED_SKILL:    // could recruit...
                    return map.HasAnyAdjacentInMap(pos, (pt) =>
                        {
                            Actor other = map.GetActorAt(pt);
                            if (other == null)
                                return false;
                            return !m_Rules.AreEnemies(m_Player, other);
                        });

                case AdvisorHint.LEADING_SWITCH_PLACE:  // switch place.
                    return map.HasAnyAdjacentInMap(pos, (pt) =>
                    {
                        Actor other = map.GetActorAt(pt);
                        if (other == null)
                            return false;
                        return m_Rules.CanActorSwitchPlaceWith(m_Player, other);
                    });

                case AdvisorHint.MOUSE_LOOK:    // always!
                    return map.LocalTime.TurnCounter >= 2;  // don't spam at turn 0.

                case AdvisorHint.MOVE_BASIC:    // always!
                    return true;

                case AdvisorHint.MOVE_JUMP:  // can jump.
                    return !m_Rules.IsActorTired(m_Player) &&
                        map.HasAnyAdjacentInMap(pos, (pt) =>
                        {
                            MapObject obj = map.GetMapObjectAt(pt);
                            if (obj == null)
                                return false;
                            return obj.IsJumpable;
                        });

                case AdvisorHint.MOVE_RUN:   // running.
                    return map.LocalTime.TurnCounter >= 5 && m_Rules.CanActorRun(m_Player);  // don't spam at turn 0.                 

                case AdvisorHint.MOVE_RESTING: // resting.
                    return m_Rules.IsActorTired(m_Player);

                case AdvisorHint.NIGHT: // night effects, wait a bit.
                    return map.LocalTime.TurnCounter >= 1 * WorldTime.TURNS_PER_HOUR;

                case AdvisorHint.NPC_TRADE: // trading.
                    return map.HasAnyAdjacentInMap(pos, (pt) =>
                        {
                            Actor other = map.GetActorAt(pt);
                            if (other == null)
                                return false;
                            return m_Rules.CanActorInitiateTradeWith(m_Player, other);
                        });

                case AdvisorHint.NPC_GIVING_ITEM: // giving items.
                    {
                        Inventory inv = m_Player.Inventory;
                        if (inv == null || inv.IsEmpty)
                            return false;
                        return map.HasAnyAdjacentInMap(pos, (pt) =>
                            {
                                Actor other = map.GetActorAt(pt);
                                if (other == null)
                                    return false;
                                return !m_Rules.AreEnemies(m_Player, other);
                            });
                    }

                case AdvisorHint.NPC_SHOUTING:  // shouting.
                    return map.HasAnyAdjacentInMap(pos, (pt) =>
                        {
                            Actor other = map.GetActorAt(pt);
                            if (other == null)
                                return false;
                            return other.IsSleeping && !m_Rules.AreEnemies(m_Player, other);
                        });

                case AdvisorHint.OBJECT_BREAK: // breaking around.
                    return map.HasAnyAdjacentInMap(pos, (pt) =>
                        {
                            MapObject obj = map.GetMapObjectAt(pt);
                            if (obj == null)
                                return false;
                            return m_Rules.IsBreakableFor(m_Player, obj);
                        });

                case AdvisorHint.OBJECT_PUSH:   // pushable around.
                    return map.HasAnyAdjacentInMap(pos, (pt) =>
                    {
                        MapObject obj = map.GetMapObjectAt(pt);
                        if (obj == null)
                            return false;
                        return m_Rules.CanActorPush(m_Player, obj);
                    });

                case AdvisorHint.RAIN:  // rainy weather, wait a bit.
                    return m_Rules.IsWeatherRain(m_Session.World.Weather) && map.LocalTime.TurnCounter >= 2 * WorldTime.TURNS_PER_HOUR;

                case AdvisorHint.SPRAYS_PAINT:    // using spraypaint.
                    return m_Player.Inventory.HasItemOfType(typeof(ItemSprayPaint));

                case AdvisorHint.SPRAYS_SCENT:    // using scent sprays.
                    return m_Player.Inventory.HasItemOfType(typeof(ItemSprayScent));

                case AdvisorHint.STATE_HUNGRY:
                    return m_Rules.IsActorHungry(m_Player);

                case AdvisorHint.STATE_SLEEPY:
                    return m_Rules.IsActorSleepy(m_Player);

                case AdvisorHint.WEAPON_FIRE: // can fire a weapon.
                    {
                        ItemRangedWeapon rw = m_Player.GetEquippedWeapon() as ItemRangedWeapon;
                        if (rw == null)
                            return false;
                        return rw.Ammo >= 0;
                    }

                case AdvisorHint.WEAPON_RELOAD: // reloading a weapon.
                    {
                        ItemRangedWeapon rw = m_Player.GetEquippedWeapon() as ItemRangedWeapon;
                        if (rw == null)
                            return false;
                        Inventory inv = m_Player.Inventory;
                        if (inv == null || inv.IsEmpty)
                            return false;
                        foreach (Item it in inv.Items)
                            if (it is ItemAmmo && m_Rules.CanActorUseItem(m_Player, it))
                                return true;
                        return false;
                    }

                // alpha10 new hints

                case AdvisorHint.SANITY:  // sanity
                    return m_Player.Sanity < 0.80f * m_Rules.ActorMaxSanity(m_Player);

                case AdvisorHint.INFECTION:
                    return m_Player.Infection > 0;

                case AdvisorHint.TRAPS:
                    return m_Player.Inventory.HasItemOfType(typeof(ItemTrap));

                default:
                    throw new ArgumentOutOfRangeException("unhandled hint");
            }
        }

        void GetAdvisorHintText(AdvisorHint hint, out string title, out string[] body)
        {
            switch (hint)
            {
                case AdvisorHint.ACTOR_MELEE:
                    title = "ATTACK AN ENEMY IN MELEE";
                    body = new string[] {
                            "You are next to an enemy.",
                            "To ATTACK him, try to MOVE on him."};
                    break;

                case AdvisorHint.BARRICADE:
                    title = "BARRICADING A DOOR/WINDOW";
                    body = new string[] {
                            "You can barricade an adjacent door or window.",
                            "Barricading uses material such as planks.",
                            String.Format("To BARRICADE : <{0}>.", s_KeyBindings.Get(PlayerCommand.BARRICADE_MODE).ToString())
                        };
                    break;

                case AdvisorHint.BUILD_FORTIFICATION:
                    title = "BUILDING FORTIFICATIONS";
                    body = new string[] {
                            "You can now build fortifications thanks to the carpentry skill.",
                            "You need enough barricading materials.",
                            String.Format("To BUILD SMALL FORTIFICATIONS : <{0}>.", s_KeyBindings.Get(PlayerCommand.BUILD_SMALL_FORTIFICATION).ToString()),
                            String.Format("To BUILD LARGE FORTIFICATIONS : <{0}>.", s_KeyBindings.Get(PlayerCommand.BUILD_LARGE_FORTIFICATION).ToString())
                        };
                    break;

                case AdvisorHint.CELLPHONES:
                    title = "CELLPHONES";
                    body = new string[] {
                            "You have found a cellphone.",
                            "Cellphones are useful to keep contact with your follower(s).",
                            "You and your follower(s) must have a cellphone equipped.",
                            "You can recharge cellphones at power generators."
                        };
                    break;

                case AdvisorHint.CITY_INFORMATION:
                    title = "CITY INFORMATION";
                    body = new string[] {
                            "You know the layout of your town.",
                            "You aso know the most notable locations.",
                            String.Format("To VIEW THE CITY INFORMATION : <{0}>.", s_KeyBindings.Get(PlayerCommand.CITY_INFO).ToString())
                        };
                    break;

                // alpha10 merged corpses hints
                case AdvisorHint.CORPSE:
                    title = "CORPSES";
                    body = new string[] {
                            "You are standing on a CORPSE.",
                            "Corpses will slowly rot away but may resurrect as zombies.",
                            "You can BUTCHER a corpse as a way to prevent that.",
                            "You can also DRAG corpses to move them.",
                            "You can try to REVIVE corpses if you have the medic skill and a medikit.",
                            "If you are desperate and starving you can resort to cannibalism by EATING corpses.",
                            "To act, hover the mouse on it in the corpse list and...",
                            "TO BUTCHER the CORPSE : <RMB>",
                            "TO DRAG the CORPSE : <LMB>",
                            string.Format("TO REVIVE the CORPSE : <{0}>", s_KeyBindings.Get(PlayerCommand.REVIVE_CORPSE).ToString()),
                            string.Format("TO EAT the CORPSE : <{0}>", s_KeyBindings.Get(PlayerCommand.EAT_CORPSE).ToString())
                    };
                    break;

                case AdvisorHint.CORPSE_EAT:
                    title = "EATING CORPSES";
                    body = new string[] {
                            "You can eat a corpse to regain health.",
                            String.Format("TO EAT A CORPSE : <RMB> on it in the corpse list.")
                    };
                    break;

                case AdvisorHint.DOORWINDOW_OPEN:
                    title = "OPENING A DOOR/WINDOW";
                    body = new string[] {
                            "You are next to a closed door or window.",
                            "To OPEN it, try to MOVE on it."
                        };
                    break;

                case AdvisorHint.DOORWINDOW_CLOSE:
                    title = "CLOSING A DOOR/WINDOW";
                    body = new string[] {
                            "You are next to an open door or window.",
                            String.Format("To CLOSE : <{0}>.", s_KeyBindings.Get(PlayerCommand.CLOSE_DOOR).ToString())
                        };
                    break;

                case AdvisorHint.EXIT_STAIRS_LADDERS:
                    title = "USING STAIRS & LADDERS";
                    body = new string[] {
                            "You are standing on stairs or a ladder.",
                            "You can use this exit to go on another map.",
                            String.Format("To USE THE EXIT : <{0}>.", s_KeyBindings.Get(PlayerCommand.USE_EXIT).ToString())
                        };
                    break;

                case AdvisorHint.FLASHLIGHT:
                    title = "LIGHTING";
                    body = new string[] {
                            "You have found a lighting item, such as a flashlight.",
                            "Equip the item to increase your view distance (FoV).",
                            "Standing next to someone with a light on has the same effect.",
                            "You can recharge flashlights at power generators."
                        };
                    break;

                case AdvisorHint.GAME_SAVE_LOAD:
                    title = "SAVING AND LOADING GAME";
                    body = new string[] {
                            "Now could be a good time to save your game.",
                            "You can have only one save game active.",
                            String.Format("To SAVE THE GAME : <{0}>.", s_KeyBindings.Get(PlayerCommand.SAVE_GAME).ToString()),
                            String.Format("To LOAD THE GAME : <{0}>.", s_KeyBindings.Get(PlayerCommand.LOAD_GAME).ToString()),
                            "You can also load the game from the main menu.",
                            "Saving or loading can take a bit of time, please be patient.",
                            "Or consider turning some game options to lower settings."
                        };
                    break;

                case AdvisorHint.EXIT_LEAVING_DISTRICT:
                    title = "LEAVING THE DISTRICT";
                    body = new string[] {
                            "You are next to a district EXIT.",
                            "You can leave this district by MOVING into the exit."
                        };
                    break;

                case AdvisorHint.GRENADE:
                    title = "GRENADES";
                    body = new string[] {
                            "You have found a grenade.",
                            "To THROW a GRENADE, EQUIP it and FIRE it.",
                            String.Format("To FIRE : <{0}>.", s_KeyBindings.Get(PlayerCommand.FIRE_MODE).ToString())
                        };
                    break;

                case AdvisorHint.ITEM_GRAB_CONTAINER:
                    title = "TAKING AN ITEM FROM A CONTAINER";
                    body = new string[] {
                            "You are next to a container, such as a wardrobe or a shelf.",
                            "You can TAKE the item there by MOVING into the object."
                        };
                    break;

                case AdvisorHint.ITEM_GRAB_FLOOR:
                    title = "TAKING AN ITEM FROM THE FLOOR";
                    body = new string[] {
                            "You are standing on a stack of items.",
                            "The items are listed on the right panel in the ground inventory.",
                            "To TAKE an item, move your mouse on the item on the ground inventory and <LMB>.",
                            "Shortcut : <Ctrl-item slot number>."
                        };
                    break;

                case AdvisorHint.ITEM_DROP:
                    title = "DROPPING AN ITEM";
                    body = new string[] {
                            "You can drop items from your inventory.",
                            "To DROP an item, <RMB> on it.",
                            "The item must be unequiped first."
                        };
                    break;

                case AdvisorHint.ITEM_EQUIP:
                    title = "EQUIPING AN ITEM";
                    body = new string[] {
                            "You have an equipable item in your inventory.",
                            "Typical equipable items are weapons, lights and phones.",
                            "To EQUIP the item, <LMB> on it in your inventory.",
                            "Shortcut : <Ctrl-item slot number>"
                        };
                    break;

                case AdvisorHint.ITEM_TYPE_BARRICADING:
                    title = "ITEM - BARRICADING MATERIAL";
                    body = new string[] {
                            "You have some barricading materials, such as planks.",
                            "Barricading material is used when you barricade doors/windows or build fortifications.",
                            "To build fortifications you need the CARPENTRY skill."
                        };
                    break;

                case AdvisorHint.ITEM_UNEQUIP:
                    title = "UNEQUIPING AN ITEM";
                    body = new string[] {
                            "You have equiped an item.",
                            "The item is displayed with a green background.",
                            "To UNEQUIP the item, <LMB> on it in your inventory.",
                            "Shortcut: <Ctrl-item slot number>"
                        };
                    break;

                case AdvisorHint.ITEM_USE:
                    title = "USING AN ITEM";
                    body = new string[] {
                            "You can use one of your item.",
                            "Typical usable items are food, medecine and ammunition.",
                            "To USE the item, <LMB> on it in your inventory.",
                            "Shortcut: <Ctrl-item slot number>"
                        };
                    break;

                case AdvisorHint.KEYS_OPTIONS:
                    title = "KEYS & OPTIONS";
                    body = new string[] {
                            String.Format("You can view and redefine the KEYS by pressing <{0}>.", s_KeyBindings.Get(PlayerCommand.KEYBINDING_MODE).ToString()),
                            String.Format("You can change OPTIONS by pressing <{0}>.", s_KeyBindings.Get(PlayerCommand.OPTIONS_MODE).ToString()),
                            "Some option changes will only take effect when starting a new game.",
                            "Keys and Options are saved."
                        };
                    break;

                case AdvisorHint.LEADING_CAN_RECRUIT:
                    title = "LEADING - RECRUITING";
                    body = new string[] {
                            "You can recruit a follower next to you!",
                            String.Format("To RECRUIT : <{0}>.", s_KeyBindings.Get(PlayerCommand.LEAD_MODE).ToString())
                        };
                    break;

                case AdvisorHint.LEADING_GIVE_ORDERS:
                    title = "LEADING - GIVING ORDERS";
                    body = new string[] {
                            "You can give orders and directives to your follower.",
                            "You can also fire your followers.",
                            String.Format("To GIVE ORDERS : <{0}>.", s_KeyBindings.Get(PlayerCommand.ORDER_MODE).ToString()),
                            String.Format("To FIRE YOUR FOLLOWER : <{0}>.", s_KeyBindings.Get(PlayerCommand.LEAD_MODE).ToString())
                        };
                    break;

                case AdvisorHint.LEADING_NEED_SKILL:
                    title = "LEADING - LEADERSHIP SKILL";
                    body = new string[] {
                            "You can try to recruit a follower if you have the LEADERSHIP skill.",
                            "The higher the skill, the more followers you can recruit."
                        };
                    break;

                case AdvisorHint.LEADING_SWITCH_PLACE:
                    title = "LEADING - SWITCHING PLACE";
                    body = new string[] {
                            "You can switch place with followers next to you.",
                            String.Format("To SWITCH PLACE : <{0}>.", s_KeyBindings.Get(PlayerCommand.SWITCH_PLACE).ToString())
                        };
                    break;

                case AdvisorHint.MOUSE_LOOK:
                    title = "LOOKING WITH THE MOUSE";
                    body = new string[] {
                            "You can LOOK at actors and objects on the map.",
                            "Move the MOUSE over something interesting.",
                            "You will get a detailed description of the actor or object.",
                            "This is useful to learn the game or assessing the tactical situation."
                        };
                    break;

                case AdvisorHint.MOVE_BASIC:
                    title = "MOVEMENT - DIRECTIONS";
                    body = new string[] {
                            "MOVE your character around with the movements keys.",
                            "The default keys are your NUMPAD numbers.",
                            "",
                            "7 8 9",
                            "4 - 6",
                            "1 2 3",
                            "",
                            "5 makes you WAIT one turn.",
                            "The move keys are the most important ones.",
                            "When asked for a DIRECTION, press a MOVE key.",
                            "Be sure to remember that!",
                            "...and remember to keep NumLock on!"
                        };
                    break;

                case AdvisorHint.MOVE_JUMP:
                    title = "MOVEMENT - JUMPING";
                    body = new string[] {
                            "You can JUMP on or over an obstacle next to you.",
                            "Typical jumpable objects are cars, fences and furniture.",
                            "The object is described with 'Can be jumped on'.",
                            "Some enemies can't jump and won't be able to follow you.",
                            "Jumping is tiring and spends stamina.",
                            "To jump, just MOVE on the obstacle."
                        };
                    break;

                case AdvisorHint.MOVE_RUN:
                    title = "MOVEMENT - RUNNING";
                    body = new string[] {
                            "You can RUN to move faster.",
                            "Running is tiring and spend stamina.",
                            String.Format("To TOGGLE RUNNING : <{0}>.", s_KeyBindings.Get(PlayerCommand.RUN_TOGGLE).ToString())
                        };
                    break;

                case AdvisorHint.MOVE_RESTING:
                    title = "MOVEMENT - RESTING";
                    body = new string[] {
                            "You are TIRED because you lost too much STAMINA.",
                            "Being tired is bad for you!",
                            "You move slowly.",
                            "You can't do tiring activities such as running, fighting and jumping.",
                            "You always recover a bit of stamina each turn.",
                            "But you can REST to recover stamina faster.",
                            String.Format("To REST/WAIT : <{0}>.", s_KeyBindings.Get(PlayerCommand.WAIT_OR_SELF).ToString())
                        };
                    break;

                case AdvisorHint.NIGHT:
                    title = "NIGHT TIME";
                    body = new string[] {
                            "It is night. Night time is penalizing for livings.",
                            "They tire faster (stamina and sleep) and don't see very far.",
                            "Undeads are not penalized by night at all."
                        };
                    break;

                case AdvisorHint.NPC_GIVING_ITEM:
                    title = "GIVING ITEMS";
                    body = new string[] {
                            "You can GIVE ITEMS to other actors.",
                            String.Format("To GIVE AN ITEM : move the mouse over your item and press <{0}>.", s_KeyBindings.Get(PlayerCommand.GIVE_ITEM).ToString())
                        };
                    break;

                case AdvisorHint.NPC_SHOUTING:
                    title = "SHOUTING";
                    body = new string[] {
                            "Someone is sleeping near you.",
                            "You can SHOUT to try to wake him or her up.",
                            "Other actors can also shout to wake their friends up when they see danger.",
                            String.Format("To SHOUT : <{0}>.", s_KeyBindings.Get(PlayerCommand.SHOUT).ToString())
                        };
                    break;

                case AdvisorHint.NPC_TRADE:
                    title = "TRADING";
                    body = new string[] {
                            "You can TRADE with an actor next to you.",
                            "Actor that can trade with you have a $ icon on the map.",
                            "Trading means exhanging items.",
                            "To ask for a TRADE offer, just try to MOVE into the actor and accept or refuse the offer.",
                            "You can also initiate a more detailled trade negociation.",
                            String.Format("To NEGOCIATE A TRADE : press <{0}> and select an npc with the directions.", s_KeyBindings.Get(PlayerCommand.NEGOCIATE_TRADE).ToString())
                        };
                    break;

                case AdvisorHint.OBJECT_BREAK:
                    title = "BREAKING OBJECTS";
                    body = new string[] {
                            "You can try to BREAK an object around you.",
                            "Typical breakable objects are furnitures, doors and windows.",
                            String.Format("To BREAK : <{0}>.", s_KeyBindings.Get(PlayerCommand.BREAK_MODE).ToString())
                        };
                    break;

                case AdvisorHint.OBJECT_PUSH:  // alpha10 also pulling and mention shoving actors
                    title = "PUSHING/PULLING OBJECTS";
                    body = new string[] {
                            "You can PUSH/PULL an OBJECT around you.",
                            "Only MOVABLE objects can be pushed/pulled.",
                            "Movable objects will be described as 'Can be moved'",
                            "You can also PUSH/PULL ACTORS around you.",
                            String.Format("To PUSH : <{0}>.", s_KeyBindings.Get(PlayerCommand.PUSH_MODE).ToString()),
                            String.Format("To PULL : <{0}>.", s_KeyBindings.Get(PlayerCommand.PULL_MODE).ToString())
                        };
                    break;

                case AdvisorHint.RAIN:
                    title = "RAIN";
                    body = new string[] {
                            "It is raining. Rain has various effects.",
                            "Livings vision is reduced.",
                            "Firearms have more chance to jam.",
                            "Scents evaporate faster."
                        };
                    break;

                case AdvisorHint.SPRAYS_PAINT:
                    title = "SPRAYS - SPRAYPAINT";
                    body = new string[] {
                            "You have found a can of spraypaint.",
                            "You can tag a symbol on walls and floors.",
                            "This is useful to mark some places and locations.",
                            String.Format("To SPRAY : equip the spray and press <{0}>.", s_KeyBindings.Get(PlayerCommand.USE_SPRAY).ToString())
                        };
                    break;

                case AdvisorHint.SPRAYS_SCENT:
                    title = "SPRAYS - SCENT SPRAY";
                    body = new string[] {
                            "You have found a scent spray.",
                            "You can spray some perfurme on yourself or another adjacent actor.",
                            "This is useful to confuse the undeads because they hunt using their smell.",
                            String.Format("To SPRAY : equip the spray and press <{0}>.", s_KeyBindings.Get(PlayerCommand.USE_SPRAY).ToString())
                        };
                    break;

                case AdvisorHint.STATE_HUNGRY:
                    title = "STATE - HUNGRY";
                    body = new string[] {
                            "You are HUNGRY.",
                            "If you become starved you can die!",
                            "You should EAT soon.",
                            "To eat, just USE a food item, such as groceries.",
                            "Read the manual for more explanations on hunger."
                        };
                    break;

                case AdvisorHint.STATE_SLEEPY:
                    title = "STATE - SLEEPY";
                    body = new string[] {
                            "You are SLEEPY.",
                            "This is bad for you!",
                            "You have a number of penalties.",
                            "You should find a place to SLEEP.",
                            "Couches are good places to sleep.",
                            String.Format("To SLEEP : <{0}>.", s_KeyBindings.Get(PlayerCommand.SLEEP).ToString()),
                            "Read the manual for more explanations on sleep."
                        };
                    break;

                case AdvisorHint.WEAPON_FIRE:
                    title = "FIRING A WEAPON";
                    body = new string[] {
                            "You can fire your equiped ranged weapon.",
                            "You need to have valid targets.",
                            "To fire on a target you need ammunitions and a clear line of fine.",
                            "The target must be within the weapon range.",
                            "The closer the target is, the easier it is to hit and it does slightly more damage.",
                            String.Format("To FIRE : <{0}>.", s_KeyBindings.Get(PlayerCommand.FIRE_MODE).ToString()),
                            "When firing you can switch to rapid fire mode : you will shoot twice but at reduced accuracy.",
                            "Remember you need to have visible enemies to fire at.",
                            "Read the manual for more explanation about firing and ranged weapons."
                        };
                    break;

                case AdvisorHint.WEAPON_RELOAD:
                    title = "RELOADING A WEAPON";
                    body = new string[] {
                            "You can reload your equiped ranged weapon.",
                            "To RELOAD, just USE a compatible ammo item.",
                        };
                    break;

                // alpha10 new hints

                case AdvisorHint.SANITY:  // sanity
                    title = "SANITY";
                    body = new string[] {
                        "You should care about your SANITY.",
                        "If it gets too low, you can go insane.",
                        "Living in this horrible world and seing horrible things will lower your sanity.",
                        "You can recover sanity by :",
                        "- Talking to people.",
                        "- Having followers you trust.",
                        "- Killing undeads.",
                        "- Using entertainment items.",
                        "- Taking pills."
                    };
                    break;

                case AdvisorHint.INFECTION:
                    title = "INFECTION";
                    body = new string[] {
                        "You are INFECTED!",
                        "Most undeads bites are infectious.",
                        "A low infection value will make you sick.",
                        "A full infection value is death.",
                        "Infection only worsen when you are biten.",
                        "Cure the infection with appropriate meds."
                    };
                    break;

                case AdvisorHint.TRAPS:
                    title = "TRAPS";
                    body = new string[] {
                        "You are carrying TRAPS.",
                        "Traps are a good way to protect places.",
                        "Drop activated traps on tiles.",
                        "Some traps are activated by dropping them.",
                        "Other traps need to be activated before being dropped.",
                        "You are always safe from your own traps.",
                        "Traps layed by your followers are also safe."
                    };
                    break;

                default:
                    throw new ArgumentOutOfRangeException("unhandled hint");
            }
        }

        void ShowAdvisorHint(AdvisorHint hint)
        {
            string title;
            string[] body;

            GetAdvisorHintText(hint, out title, out body);
            ShowAdvisorMessage(title, body);
        }

        void ShowAdvisorMessage(string title, string[] lines)
        {
            // clear.
            ClearMessages();
            ClearOverlays();

            // tell.
            string[] text = new string[lines.Length + 2];
            text[0] = "HINT : " + title;
            Array.Copy(lines, 0, text, 1, lines.Length);
            text[lines.Length + 1] = String.Format("(hint {0}/{1})", s_Hints.CountAdvisorHintsGiven(), (int)AdvisorHint._COUNT);
            AddOverlay(new OverlayPopup(text, Color.White, Color.White, Color.Black, new Point(0, 0)));

            // wait.
            ClearMessages();
            AddMessage(new Message("You can disable the advisor in the options screen.", m_Session.WorldTime.TurnCounter, Color.White));
            AddMessage(new Message(String.Format("To show the options screen : <{0}>.", s_KeyBindings.Get(PlayerCommand.OPTIONS_MODE).ToString()), m_Session.WorldTime.TurnCounter, Color.White));
            AddMessagePressEnter();

            // clear.
            ClearMessages();
            ClearOverlays();
            RedrawPlayScreen();
        }
        #endregion

        #region Input helpers
        void WaitKeyOrMouse(out KeyEventArgs key, out Point mousePos, out MouseButtons? mouseButtons)
        {
            // Peek keyboard & mouse until we got an event.
            m_UI.UI_PeekKey();  // consume keys to avoid repeats.
            KeyEventArgs inKey;
            Point prevMousePos = m_UI.UI_GetMousePosition();
            mousePos = new Point(-1, -1);
            mouseButtons = null;
            for (; ; )
            {
                inKey = m_UI.UI_PeekKey();
                if (inKey != null)
                {
                    key = inKey;
                    return;
                }
                else
                {
                    mousePos = m_UI.UI_GetMousePosition();
                    mouseButtons = m_UI.UI_PeekMouseButtons();
                    if (mousePos != prevMousePos || mouseButtons != null)
                    {
                        key = null;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Loop input until Exit/Cancel or a direction command is issued.
        /// </summary>
        /// <returns>null if Exit/Cancel</returns>
        Direction WaitDirectionOrCancel()
        {
            for (; ; )
            {
                KeyEventArgs inKey = m_UI.UI_WaitKey();
                PlayerCommand command = InputTranslator.KeyToCommand(inKey);
                if (inKey.KeyCode == Keys.Escape)// command == PlayerCommand.EXIT_OR_CANCEL)
                    return null;
                Direction dir = CommandToDirection(command);
                if (dir != null)
                    return dir;
            }
        }

        void WaitEnter()
        {
            for (; ; )
            {
                KeyEventArgs inKey = m_UI.UI_WaitKey();
                if (inKey.KeyCode == Keys.Enter)
                    return;
            }
        }

        void WaitEscape()
        {
            for (; ; )
            {
                KeyEventArgs inKey = m_UI.UI_WaitKey();
                if (inKey.KeyCode == Keys.Escape)
                    return;
            }
        }

        /// <summary>
        /// -1 if no choice.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        int KeyToChoiceNumber(Keys key)
        {
            switch (key)
            {
                case Keys.NumPad0:
                case Keys.D0:
                    return 0;

                case Keys.NumPad1:
                case Keys.D1:
                    return 1;

                case Keys.NumPad2:
                case Keys.D2:
                    return 2;

                case Keys.NumPad3:
                case Keys.D3:
                    return 3;

                case Keys.NumPad4:
                case Keys.D4:
                    return 4;

                case Keys.NumPad5:
                case Keys.D5:
                    return 5;

                case Keys.NumPad6:
                case Keys.D6:
                    return 6;

                case Keys.NumPad7:
                case Keys.D7:
                    return 7;

                case Keys.NumPad8:
                case Keys.D8:
                    return 8;

                case Keys.NumPad9:
                case Keys.D9:
                    return 9;

                default:
                    return -1;
            }
        }

        bool WaitYesOrNo()
        {
            for (; ; )
            {
                KeyEventArgs inKey = m_UI.UI_WaitKey();
                if (inKey.KeyCode == Keys.Y)
                    return true;
                else if (inKey.KeyCode == Keys.N || inKey.KeyCode == Keys.Escape)
                    return false;
            }
        }
        #endregion

        #region Describing a game element
        string[] DescribeStuffAt(Map map, Point mapPos)
        {
            // Actor?
            Actor actor = map.GetActorAt(mapPos);
            if (actor != null)
            {
                return DescribeActor(actor);
            }

            // Object/Items?
            MapObject obj = map.GetMapObjectAt(mapPos);
            if (obj != null)
            {
                return DescribeMapObject(obj, map, mapPos);
            }

            // Items?
            Inventory inv = map.GetItemsAt(mapPos);
            if (inv != null && !inv.IsEmpty)
            {
                return DescribeInventory(inv);
            }

            // Corpses?
            List<Corpse> corpses = map.GetCorpsesAt(mapPos);
            if (corpses != null)
            {
                return DescribeCorpses(corpses);
            }

            // Nothing to describe!
            return null;
        }

        string[] DescribeActor(Actor actor)
        {
            List<string> lines = new List<string>(10);

            // 1. Name-Faction(Gang), Model, SpawnTime, Order & Leader(trust if player), (Murder counter if player law enforcer);
            //    Enemy & Self-Defence.
            if (actor.Faction != null)
            {
                if (actor.IsInAGang)
                    lines.Add(String.Format("{0}, {1}-{2}.", Capitalize(actor.Name), actor.Faction.MemberName, GameGangs.NAMES[actor.GangID]));
                else
                    lines.Add(String.Format("{0}, {1}.", Capitalize(actor.Name), actor.Faction.MemberName));
            }
            else
                lines.Add(String.Format("{0}.", Capitalize(actor.Name)));
            lines.Add(String.Format("{0}.", Capitalize(actor.Model.Name)));

            lines.Add(String.Format("{0} since {1}.", actor.Model.Abilities.IsUndead ? "Undead" : "Staying alive", new WorldTime(actor.SpawnTime).ToString()));
            AIController ai = actor.Controller as AIController;
            if (ai != null && ai.Order != null)
            {
                lines.Add(String.Format("Order : {0}.", ai.Order.ToString()));
            }
            if (actor.HasLeader)
            {
                if (actor.Leader.IsPlayer)
                {
                    if (actor.TrustInLeader >= Rules.TRUST_BOND_THRESHOLD)
                        lines.Add(String.Format("Trust : BOND."));
                    else if (actor.TrustInLeader >= Rules.TRUST_MAX)
                        lines.Add("Trust : MAX.");
                    else
                        lines.Add(String.Format("Trust : {0}/T:{1}-B:{2}.", actor.TrustInLeader, Rules.TRUST_TRUSTING_THRESHOLD, Rules.TRUST_BOND_THRESHOLD));
                    OrderableAI orderAI = ai as OrderableAI;
                    if (orderAI != null)
                    {
                        if (orderAI.DontFollowLeader)
                            lines.Add("Ordered to not follow you.");
                    }
                    // gauges.
                    lines.Add(String.Format("Foo : {0} {1}h", actor.FoodPoints, FoodToHoursUntilHungry(actor.FoodPoints)));
                    lines.Add(String.Format("Slp : {0} {1}h", actor.SleepPoints, m_Rules.SleepToHoursUntilSleepy(actor.SleepPoints, actor.Location.Map.LocalTime.IsNight)));
                    lines.Add(String.Format("San : {0} {1}h", actor.Sanity, m_Rules.SanityToHoursUntilUnstable(actor)));
                    lines.Add(String.Format("Inf : {0} {1}%", actor.Infection, m_Rules.ActorInfectionPercent(actor)));
                }
                else
                    lines.Add(String.Format("Leader : {0}.", Capitalize(actor.Leader.Name)));
            }

            // show murder counter if trusting follower or player is a law enforcer.
            if (actor.MurdersCounter > 0 && m_Player.Model.Abilities.IsLawEnforcer)
            {
                lines.Add("WANTED FOR MURDER!");
                lines.Add(String.Format("{0} murder{1}!", actor.MurdersCounter, actor.MurdersCounter > 1 ? "s" : ""));
            }
            else if (actor.HasLeader && actor.Leader.IsPlayer && m_Rules.IsActorTrustingLeader(actor))
            {
                if (actor.MurdersCounter > 0)
                    lines.Add(String.Format("* Confess {0} murder{1}! *", actor.MurdersCounter, actor.MurdersCounter > 1 ? "s" : ""));
                else
                    lines.Add("Has committed no murders.");
            }
            if (actor.IsAggressorOf(m_Player))
                lines.Add("Aggressed you.");
            if (m_Player.IsSelfDefenceFrom(actor))
                lines.Add(String.Format("You can kill {0} in self-defence.", HimOrHer(actor)));
            if (m_Player.IsAggressorOf(actor))
                lines.Add(String.Format("You aggressed {0}.", HimOrHer(actor)));
            if (actor.IsSelfDefenceFrom(m_Player))
                lines.Add("Killing you would be self-defence.");
            if (!m_Player.Faction.IsEnemyOf(actor.Faction) && m_Rules.AreGroupEnemies(m_Player, actor)) // alpha10
                lines.Add("You are enemies through groups.");

            lines.Add("");

            // 2. Activity & Hunger/Sleep/Sanity
            string activityLine = DescribeActorActivity(actor);
            if (activityLine != null)
                lines.Add(activityLine);
            else
                lines.Add(" ");  // blank activity line
            if (actor.Model.Abilities.HasToSleep)
            {
                if (m_Rules.IsActorExhausted(actor))
                    lines.Add("Exhausted!");
                else if (m_Rules.IsActorSleepy(actor))
                    lines.Add("Sleepy.");
            }
            if (actor.Model.Abilities.HasToEat)
            {
                if (m_Rules.IsActorStarving(actor))
                    lines.Add("Starving!");
                else if (m_Rules.IsActorHungry(actor))
                    lines.Add("Hungry.");
            }
            else if (actor.Model.Abilities.IsRotting)
            {
                if (m_Rules.IsRottingActorStarving(actor))
                    lines.Add("Starving!");
                else if (m_Rules.IsRottingActorHungry(actor))
                    lines.Add("Hungry.");
            }
            if (actor.Model.Abilities.HasSanity)
            {
                if (m_Rules.IsActorInsane(actor))
                    lines.Add("Insane!");
                else if (m_Rules.IsActorDisturbed(actor))
                    lines.Add("Disturbed.");
            }

            // 3. Speed
            lines.Add(String.Format("Spd : {0:F2}", (float)m_Rules.ActorSpeed(actor) / (float)Rules.BASE_SPEED));

            // 4. HP & STA.
            StringBuilder sb = new StringBuilder();
            int maxHP = m_Rules.ActorMaxHPs(actor);
            if (actor.HitPoints != maxHP)
                sb.Append(String.Format("HP  : {0:D2}/{1:D2}", actor.HitPoints, maxHP));
            else
                sb.Append(String.Format("HP  : {0:D2} MAX", actor.HitPoints));
            if (actor.Model.Abilities.CanTire)
            {
                int maxSTA = m_Rules.ActorMaxSTA(actor);
                if (actor.StaminaPoints != maxSTA)
                    sb.Append(String.Format("   STA : {0}/{1}", actor.StaminaPoints, maxSTA));
                else
                    sb.Append(string.Format("   STA : {0} MAX", actor.StaminaPoints));
            }
            lines.Add(sb.ToString());

            // 5. Attack, Dmg, Defence.
            Attack attack = m_Rules.ActorMeleeAttack(actor, actor.CurrentMeleeAttack, null);
            lines.Add(String.Format("Atk : {0:D2} Dmg : {1:D2}", attack.HitValue, attack.DamageValue));
            Defence defence = m_Rules.ActorDefence(actor, actor.CurrentDefence);
            lines.Add(String.Format("Def : {0:D2}", defence.Value));
            lines.Add(String.Format("Arm : {0}/{1}", defence.Protection_Hit, defence.Protection_Shot));
            lines.Add(" ");

            // 6. Flavor
            lines.Add(actor.Model.FlavorDescription);
            lines.Add(" ");

            // 7. Skills
            if (actor.Sheet.SkillTable != null && actor.Sheet.SkillTable.CountSkills > 0)
            {
                foreach (Skill sk in actor.Sheet.SkillTable.Skills)
                    lines.Add(String.Format("{0}-{1}", sk.Level, Skills.Name(sk.ID)));
                lines.Add(" ");
            }

            // alpha10
            // 8. Unusual abilities
            // unusual abilities for undeads
            if (actor.Model.Abilities.IsUndead)
            {
                // fov
                lines.Add(string.Format("- FOV : {0}.", actor.Model.StartingSheet.BaseViewRange));

                // smell rating
                int smell = (int)(100 * m_Rules.ActorSmell(actor));  // appliyes z-tracker skill
                lines.Add(
                    smell == 0 ? "- Has no sense of smell." :
                    smell < 50 ? "- Has poor sense of smell." :
                    smell < 100 ? "- Has good sense of smell." :
                    "- Has excellent sense of smell.");

                // grab?
                if (actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.Z_GRAB) > 0)
                    lines.Add("- Z-Grab : this undead can grab its victims.");

                if (actor.Model.Abilities.IsUndeadMaster) lines.Add("- Other undeads follow this undead tracks.");
                else if (smell > 0) lines.Add("- This undead will follow zombie masters tracks.");
                if (actor.Model.Abilities.IsIntelligent) lines.Add("- This undead is intelligent.");
                if (actor.Model.Abilities.CanDisarm) lines.Add("- This undead can disarm.");
                if (actor.Model.Abilities.CanJump)
                {
                    if (actor.Model.Abilities.CanJumpStumble) lines.Add("- This undead can jump but may stumble.");
                    else lines.Add("- This undead can jump.");
                }
                if (m_Rules.HasActorPushAbility(actor)) lines.Add("- This undead can push.");
                if (actor.Model.Abilities.ZombieAI_Explore) lines.Add("- This undead will explore.");

                // things some of them cannot do
                if (!actor.Model.Abilities.IsRotting) lines.Add("- This undead will not rot.");
                if (!actor.Model.Abilities.CanBashDoors) lines.Add("- This undead cannot bash doors.");
                if (!actor.Model.Abilities.CanBreakObjects) lines.Add("- This undead cannot break objects.");
                if (!actor.Model.Abilities.CanZombifyKilled) lines.Add("- This undead cannot infect livings.");
                if (!actor.Model.Abilities.AI_CanUseAIExits) lines.Add("- This undead live in this map.");
            }
            // misc unusual abilities
            if (actor.Model.Abilities.IsLawEnforcer) lines.Add("- Is a law enforcer.");
            if (actor.Model.Abilities.IsSmall) lines.Add("- Is small and can sneak through things.");

            // 9. Inventory.
            if (actor.Inventory != null && !actor.Inventory.IsEmpty)
            {
                lines.Add(String.Format("Items {0}/{1} : ", actor.Inventory.CountItems, m_Rules.ActorMaxInv(actor)));
                lines.AddRange(DescribeInventory(actor.Inventory));
            }

            // done.
            return lines.ToArray();
        }

        string DescribeActorActivity(Actor actor)
        {
            if (actor.IsPlayer)
                return null;

            switch (actor.Activity)
            {
                case Activity.IDLE:
                    return null;

                case Activity.CHASING:
                    if (actor.TargetActor == null)
                        return "Chasing!";
                    else
                        return String.Format("Chasing {0}!", actor.TargetActor.Name);

                case Activity.FIGHTING:
                    if (actor.TargetActor == null)
                        return "Fighting!";
                    else
                        return String.Format("Fighting {0}!", actor.TargetActor.Name);

                case Activity.TRACKING:
                    return "Tracking!";

                case Activity.FLEEING:
                    return "Fleeing!";

                case Activity.FLEEING_FROM_EXPLOSIVE:
                    return "Fleeing from explosives!";

                case Activity.FOLLOWING:
                    if (actor.TargetActor == null)
                        return "Following.";
                    else
                    {
                        // alpha10
                        if (actor.Leader == actor.TargetActor)
                            return string.Format("Following {0} leader.", HisOrHer(actor));
                        return string.Format("Following {0}.", actor.TargetActor.Name);
                    }

                case Activity.FOLLOWING_ORDER:
                    return "Following orders.";

                case Activity.SLEEPING:
                    return "Sleeping.";

                default:
                    throw new ArgumentException("unhandled activity " + actor.Activity);
            }
        }

        string DescribePlayerFollowerStatus(Actor follower)
        {
            string desc;

            BaseAI foAI = follower.Controller as BaseAI;
            if (foAI.Order == null)
                desc = "(no orders)";
            else
                desc = foAI.Order.ToString();
            desc += String.Format("(trust:{0})", follower.TrustInLeader);

            return desc;
        }

        string[] DescribeMapObject(MapObject obj, Map map, Point mapPos)
        {
            List<string> lines = new List<string>(4);

            // 1. Name
            lines.Add(String.Format("{0}.", obj.AName));

            // 2. Special flags.
            if (obj.IsJumpable)
                lines.Add("Can be jumped on.");
            if (obj.IsCouch)
                lines.Add("Is a couch.");
            if (obj.GivesWood)
                lines.Add("Can be dismantled for wood.");
            if (obj.IsMovable)
                lines.Add("Can be moved.");
            if (obj.StandOnFovBonus)
                lines.Add("Increases view range.");

            // 3. Common Status: Break, Fire.
            //    Concrete MapObjects status.
            StringBuilder sb = new StringBuilder();
            if (obj.BreakState == MapObject.Break.BROKEN)
                sb.Append("Broken! ");
            if (obj.FireState == MapObject.Fire.ONFIRE)
                sb.Append("On fire! ");
            else if (obj.FireState == MapObject.Fire.ASHES)
                sb.Append("Burnt to ashes! ");
            lines.Add(sb.ToString());
            if (obj is PowerGenerator)
            {
                PowerGenerator powGen = obj as PowerGenerator;
                if (powGen.IsOn)
                    lines.Add("Currently ON.");
                else
                    lines.Add("Currently OFF.");
                float powerRatio = m_Rules.ComputeMapPowerRatio(obj.Location.Map);
                lines.Add(String.Format("The power gauge reads {0}%.", (int)(100 * powerRatio)));
            }
            else if (obj is Board)
            {
                lines.Add("The text reads : ");
                lines.AddRange((obj as Board).Text);
            }

            // 4. HitPoints & Barricade
            if (obj.MaxHitPoints > 0)
            {
                if (obj.HitPoints < obj.MaxHitPoints)
                    lines.Add(String.Format("HP        : {0}/{1}", obj.HitPoints, obj.MaxHitPoints));
                else
                    lines.Add(String.Format("HP        : {0} MAX", obj.HitPoints));

                DoorWindow door = obj as DoorWindow;
                if (door != null)
                {
                    if (door.BarricadePoints < Rules.BARRICADING_MAX)
                        lines.Add(String.Format("Barricades: {0}/{1}", door.BarricadePoints, Rules.BARRICADING_MAX));
                    else
                        lines.Add(String.Format("Barricades: {0} MAX", door.BarricadePoints));
                }
            }

            // 5. Weight?
            if (obj.Weight > 0)
            {
                lines.Add(String.Format("Weight    : {0}", obj.Weight));
            }

            // 6. Items there
            Inventory inv = map.GetItemsAt(mapPos);
            if (inv != null && !inv.IsEmpty)
            {
                lines.AddRange(DescribeInventory(inv));
            }

            return lines.ToArray();
        }

        string[] DescribeInventory(Inventory inv)
        {
            List<string> lines = new List<string>(inv.CountItems);

            foreach (Item it in inv.Items)
            {
                if (it.IsEquipped)
                    lines.Add(String.Format("- {0} (equipped)", DescribeItemShort(it)));
                else
                    lines.Add(String.Format("- {0}", DescribeItemShort(it)));
            }

            return lines.ToArray();
        }

        string[] DescribeCorpses(List<Corpse> corpses)
        {
            List<string> lines = new List<string>(corpses.Count + 2);

            if (corpses.Count > 1)
                lines.Add("There are corpses there...");
            else
                lines.Add("There is a corpse here.");
            lines.Add(" ");

            foreach (Corpse c in corpses)
            {
                lines.Add(String.Format("- Corpse of {0}.", c.DeadGuy.Name));
            }
            return lines.ToArray();
        }

        string[] DescribeCorpseLong(Corpse c, bool isInPlayerTile)
        {
            List<string> lines = new List<string>(10);

            // 1. Corpse of XXX
            lines.Add(String.Format("Corpse of {0}.", c.DeadGuy.Name));
            lines.Add(" ");

            // 2. Necrology infos.
            int necrology = m_Player.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.NECROLOGY);

            string deadSince = "???";
            if (necrology > 0)
                deadSince = WorldTime.MakeTimeDurationMessage(m_Session.WorldTime.TurnCounter - c.Turn);
            lines.Add(String.Format("Death     : {0}.", deadSince));

            string infectionEst = "???";
            if (necrology >= Rules.SKILL_NECROLOGY_LEVEL_FOR_INFECTION)
            {
                int infectionP = m_Rules.ActorInfectionPercent(c.DeadGuy);
                if (infectionP == 0) infectionEst = "0/7 - none";
                else if (infectionP < 5) infectionEst = "1/7 - traces";
                else if (infectionP < 15) infectionEst = "2/7 - minor";
                else if (infectionP < 30) infectionEst = "3/7 - low";
                else if (infectionP < 55) infectionEst = "4/7 - average";
                else if (infectionP < 70) infectionEst = "5/7 - important";
                else if (infectionP < 99) infectionEst = "6/7 - great";
                else infectionEst = "7/7 - total";
            }
            lines.Add(String.Format("Infection : {0}.", infectionEst));

            string riseEst = "???";
            if (necrology >= Rules.SKILL_NECROLOGY_LEVEL_FOR_RISE)
            {
                int riseP = 2 * m_Rules.CorpseZombifyChance(c, c.DeadGuy.Location.Map.LocalTime, false);
                if (riseP < 5) riseEst = "0/6 - extremely unlikely";
                else if (riseP < 20) riseEst = "1/6 - unlikely";
                else if (riseP < 40) riseEst = "2/6 - possible";
                else if (riseP < 60) riseEst = "3/6 - likely";
                else if (riseP < 80) riseEst = "4/6 - very likely";
                else if (riseP < 99) riseEst = "5/6 - most likely";
                else riseEst = "6/6 - certain";
            }
            lines.Add(String.Format("Rise      : {0}.", riseEst));
            lines.Add(" ");

            // 3. Decay
            int rotLevel = Rules.CorpseRotLevel(c);
            switch (rotLevel)
            {
                case 5: lines.Add("The corpse is about to crumble to dust."); break;
                case 4: lines.Add("The corpse is almost entirely rotten."); break;
                case 3: lines.Add("The corpse is badly damaged."); break;
                case 2: lines.Add("The corpse is damaged."); break;
                case 1: lines.Add("The corpse is bruised and smells."); break;
                case 0: lines.Add("The corpse looks fresh."); break;
                default: throw new Exception("unhandled rot level");
            }

            // 3. Medic info.
            string reviveEst = "???";
            int medic = m_Player.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.MEDIC);
            if (medic >= Rules.SKILL_MEDIC_LEVEL_FOR_REVIVE_EST)
            {
                int reviveP = m_Rules.CorpseReviveChance(m_Player, c);
                if (reviveP == 0) reviveEst = "impossible";
                else if (reviveP < 5) reviveEst = "0/6 - extremely unlikely";
                else if (reviveP < 20) reviveEst = "1/6 - unlikely";
                else if (reviveP < 40) reviveEst = "2/6 - possible";
                else if (reviveP < 60) reviveEst = "3/6 - likely";
                else if (reviveP < 80) reviveEst = "4/6 - very likely";
                else if (reviveP < 99) reviveEst = "5/6 - most likely";
                else reviveEst = "6/6 - certain";
            }
            lines.Add(String.Format("Revive    : {0}.", reviveEst));

            // 5. Special keys.
            if (isInPlayerTile)
            {
                lines.Add(" ");
                lines.Add("----");
                lines.Add("LBM to start/stop dragging.");
                lines.Add(String.Format("RBM to {0}.", m_Player.Model.Abilities.IsUndead ? "eat" : "butcher"));
                if (!m_Player.Model.Abilities.IsUndead)
                {
                    lines.Add(String.Format("to eat: <{0}>", s_KeyBindings.Get(PlayerCommand.EAT_CORPSE).ToString()));
                    lines.Add(String.Format("to revive : <{0}>", s_KeyBindings.Get(PlayerCommand.REVIVE_CORPSE).ToString()));
                }
            }

            return lines.ToArray();
        }
        #region Items
        string DescribeItemShort(Item it)
        {
            string name = it.Quantity > 1 ? it.Model.PluralName : it.AName;

            if (it is ItemFood)
            {
                ItemFood food = it as ItemFood;
                if (m_Rules.IsFoodSpoiled(food, m_Session.WorldTime.TurnCounter))
                    name += " (spoiled)";
                else if (m_Rules.IsFoodExpired(food, m_Session.WorldTime.TurnCounter))
                    name += " (expired)";
            }
            else if (it is ItemRangedWeapon)
            {
                ItemRangedWeapon rw = it as ItemRangedWeapon;
                name += String.Format(" ({0}/{1})", rw.Ammo, (rw.Model as ItemRangedWeaponModel).MaxAmmo);
            }
            else if (it is ItemTrap)
            {
                ItemTrap trap = it as ItemTrap;
                if (trap.IsActivated) name += "(activated)";
                if (trap.IsTriggered) name += "(triggered)";
                if (trap.Owner == m_Player) name += "(yours)";  // alpha10
            }

            if (it.Quantity > 1)
                return String.Format("{0} {1}", it.Quantity, name);
            else
                return name;
        }

        string[] DescribeItemLong(Item it, bool isPlayerInventory, int iSlot)
        {
            List<string> lines = new List<string>();
            bool isDefaultUse = true; // alpha10

            // 1. Name & stacking.
            if (it.Model.IsStackable)
            {
                lines.Add(String.Format("{0} {1}/{2}", DescribeItemShort(it), it.Quantity, it.Model.StackingLimit));
            }
            else
                lines.Add(DescribeItemShort(it));

            // 2. Special flags.
            // unbreakable?
            if (it.Model.IsUnbreakable)
            {
                lines.Add("Unbreakable.");
            }

            // 3. Item specific stuff...
            string inInvAdditionalDesc = null;
            if (it is ItemWeapon)
            {
                lines.AddRange(DescribeItemWeapon(it as ItemWeapon));
                if (it is ItemRangedWeapon)
                {
                    isDefaultUse = false;
                    inInvAdditionalDesc = String.Format("to fire : <{0}>", s_KeyBindings.Get(PlayerCommand.FIRE_MODE).ToString());
                }
            }
            else if (it is ItemFood)
            {
                lines.AddRange(DescribeItemFood(it as ItemFood));
            }
            else if (it is ItemMedicine)
            {
                lines.AddRange(DescribeItemMedicine(it as ItemMedicine));
            }
            else if (it is ItemBarricadeMaterial)
            {
                lines.AddRange(DescribeItemBarricadeMaterial(it as ItemBarricadeMaterial));
                isDefaultUse = false;
                inInvAdditionalDesc = String.Format("to build : <{0}>/<{1}>/<{2}>",
                    s_KeyBindings.Get(PlayerCommand.BARRICADE_MODE).ToString(), s_KeyBindings.Get(PlayerCommand.BUILD_SMALL_FORTIFICATION).ToString(),
                    s_KeyBindings.Get(PlayerCommand.BUILD_LARGE_FORTIFICATION).ToString());
            }
            else if (it is ItemBodyArmor)
            {
                lines.AddRange(DescribeItemBodyArmor(it as ItemBodyArmor));
            }
            else if (it is ItemSprayPaint)
            {
                lines.AddRange(DescribeItemSprayPaint(it as ItemSprayPaint));
                isDefaultUse = false;
                inInvAdditionalDesc = String.Format("to spray : <{0}>", s_KeyBindings.Get(PlayerCommand.USE_SPRAY).ToString());
            }
            else if (it is ItemSprayScent)
            {
                lines.AddRange(DescribeItemSprayScent(it as ItemSprayScent));
                isDefaultUse = false;
                inInvAdditionalDesc = String.Format("to spray : <{0}>", s_KeyBindings.Get(PlayerCommand.USE_SPRAY).ToString());
            }
            else if (it is ItemLight)
            {
                lines.AddRange(DescribeItemLight(it as ItemLight));
            }
            else if (it is ItemTracker)
            {
                lines.AddRange(DescribeItemTracker(it as ItemTracker));
            }
            else if (it is ItemAmmo)
            {
                lines.AddRange(DescribeItemAmmo(it as ItemAmmo));
                isDefaultUse = false;
                inInvAdditionalDesc = string.Format("to reload : <LMB> or <Ctrl-{0}>", iSlot + 1);
            }
            else if (it is ItemExplosive)
            {
                lines.AddRange(DescribeItemExplosive(it as ItemExplosive));
                inInvAdditionalDesc = String.Format("to throw : <{0}>", s_KeyBindings.Get(PlayerCommand.FIRE_MODE).ToString());
            }
            else if (it is ItemTrap)
            {
                lines.AddRange(DescribeItemTrap(it as ItemTrap));
                // alpha10
                if ((it as ItemTrap).TrapModel.ActivatesWhenDropped)
                    inInvAdditionalDesc = "to activate trap : drop it";
                else
                    inInvAdditionalDesc = "to activate trap : use it";
            }
            else if (it is ItemEntertainment)
            {
                lines.AddRange(DescribeItemEntertainment(it as ItemEntertainment));
            }

            // 3. Flavor description
            lines.Add(" ");
            lines.Add(it.Model.FlavorDescription);

            // 4. Special keys.
            // alpha10 added more special keys very few players know about!
            if (isPlayerInventory)
            {
                lines.Add(" ");
                lines.Add("----");
                if (it.Model.IsEquipable)
                    lines.Add(string.Format("to {0} : <LMB> or <Ctrl-{1}>", it.IsEquipped ? "unequip" : "equip", iSlot + 1));
                else if (isDefaultUse)
                    lines.Add(string.Format("to use : <LMB> or <Ctrl-{0}>", iSlot + 1));
                if (!it.IsEquipped)
                    lines.Add("to drop : <RMB>");
                lines.Add(String.Format("to give : <{0}>", s_KeyBindings.Get(PlayerCommand.GIVE_ITEM).ToString()));
                if (inInvAdditionalDesc != null)
                    lines.Add(inInvAdditionalDesc);
            }
            else
            {
                lines.Add(" ");
                lines.Add("----");
                lines.Add(string.Format("to take : <LMB> or <Shift-{0}>", iSlot + 1));
            }

            // done.
            return lines.ToArray();
        }

        string[] DescribeItemExplosive(ItemExplosive ex)
        {
            List<string> lines = new List<string>();

            ItemExplosiveModel m = ex.Model as ItemExplosiveModel;
            ItemPrimedExplosive primed = ex as ItemPrimedExplosive;

            lines.Add("> explosive");

            // 1. Explosive attack.
            if (m.BlastAttack.CanDamageObjects)
                lines.Add("Can damage objects.");
            if (m.BlastAttack.CanDestroyWalls)
                lines.Add("Can destroy walls.");

            if (primed != null)
                lines.Add(String.Format("Fuse          : {0} turn(s) left!", primed.FuseTimeLeft));
            else
                lines.Add(String.Format("Fuse          : {0} turn(s)", m.FuseDelay));
            lines.Add(String.Format("Blast radius  : {0}", m.BlastAttack.Radius));

            // 2. Damage for each distance.
            StringBuilder sb = new StringBuilder();
            for (int blastRadius = 0; blastRadius <= m.BlastAttack.Radius; blastRadius++)
            {
                sb.Append(String.Format("{0};", m_Rules.BlastDamage(blastRadius, m.BlastAttack)));
            }
            lines.Add(String.Format("Blast damages : {0}", sb.ToString()));

            // 3. Specialized explosives.
            // grenade?
            ItemGrenade grenade = ex as ItemGrenade;
            if (grenade != null)
            {
                lines.Add("> grenade");

                ItemGrenadeModel greModel = grenade.Model as ItemGrenadeModel;
                int rng = m_Rules.ActorMaxThrowRange(m_Player, greModel.MaxThrowDistance);
                if (rng != greModel.MaxThrowDistance)
                    lines.Add(String.Format("Throwing rng  : {0} ({1})", rng, greModel.MaxThrowDistance));
                else
                    lines.Add(String.Format("Throwing rng  : {0}", rng));
            }

            // 4. Primed?
            if (primed != null)
            {
                lines.Add("PRIMED AND READY TO EXPLODE!");
            }

            return lines.ToArray();
        }

        string[] DescribeItemWeapon(ItemWeapon w)
        {
            List<string> lines = new List<string>();

            ItemWeaponModel m = w.Model as ItemWeaponModel;

            lines.Add("> weapon");

            // 1. Attack
            lines.Add(String.Format("Atk : +{0}", m.Attack.HitValue));
            lines.Add(String.Format("Dmg : +{0}", m.Attack.DamageValue));
            // alpha10
            if (m.Attack.StaminaPenalty != 0)
                lines.Add(String.Format("Sta : -{0}", m.Attack.StaminaPenalty));
            if (m.Attack.DisarmChance != 0)
                lines.Add(String.Format("Disarm : +{0}%", m.Attack.DisarmChance));

            // 2. Melee vs Ranged items
            ItemMeleeWeapon mw = w as ItemMeleeWeapon;
            if (mw != null)
            {
                if (mw.IsFragile)
                    lines.Add("Breaks easily.");
                // alpha10 tool
                if (mw.IsTool)
                {
                    lines.Add("Is a tool.");
                    int toolBashDmg = mw.ToolBashDamageBonus;
                    if (toolBashDmg != 0)
                        lines.Add(string.Format("Tool Dmg   : +{0} = +{1}", toolBashDmg, toolBashDmg + m.Attack.DamageValue));
                    float toolBuild = mw.ToolBuildBonus;
                    if (toolBuild != 0)
                        lines.Add(string.Format("Tool Build : +{0}%", (int)(100 * toolBuild)));
                }
            }
            else
            {
                ItemRangedWeapon rw = w as ItemRangedWeapon;
                if (rw != null)
                {
                    ItemRangedWeaponModel rm = w.Model as ItemRangedWeaponModel;
                    if (rm.IsFireArm)
                        lines.Add("> firearm");
                    else if (rm.IsBow)
                        lines.Add("> bow");
                    else
                        lines.Add("> ranged weapon");

                    // alpha10
                    lines.Add(string.Format("Rapid Fire Atk: {0} {1}", rm.RapidFireHit1Value, rm.RapidFireHit2Value));

                    lines.Add(string.Format("Rng  : {0}-{1}", rm.Attack.Range, rm.Attack.EfficientRange));
                    if (rw.Ammo < rm.MaxAmmo)
                        lines.Add(string.Format("Amo  : {0}/{1}", rw.Ammo, rm.MaxAmmo));
                    else
                        lines.Add(string.Format("Amo  : {0} MAX", rw.Ammo));
                    lines.Add(string.Format("Type : {0}", DescribeAmmoType(rm.AmmoType)));
                }
            }

            // done.
            return lines.ToArray();
        }

        string DescribeAmmoType(AmmoType at)
        {
            switch (at)
            {
                case AmmoType.BOLT: return "bolts";
                case AmmoType.HEAVY_PISTOL: return "heavy pistol bullets";
                case AmmoType.HEAVY_RIFLE: return "heavy rifle bullets";
                case AmmoType.LIGHT_PISTOL: return "light pistol bullets";
                case AmmoType.LIGHT_RIFLE: return "light rifle bullets";
                case AmmoType.SHOTGUN: return "shotgun cartridge";
                default:
                    throw new ArgumentOutOfRangeException("unhandled ammo type");
            }
        }

        string[] DescribeItemAmmo(ItemAmmo am)
        {
            List<string> lines = new List<string>();

            lines.Add("> ammo");

            // 1. Ammo type
            lines.Add(string.Format("Type : {0}", DescribeAmmoType(am.AmmoType)));

            return lines.ToArray();
        }

        string[] DescribeItemFood(ItemFood f)
        {
            List<string> lines = new List<string>();

            ItemFoodModel m = f.Model as ItemFoodModel;

            lines.Add("> food");

            // 1. Fresh/Expired, Best-Before
            if (f.IsPerishable)
            {
                if (m_Rules.IsFoodStillFresh(f, m_Session.WorldTime.TurnCounter))
                    lines.Add("Fresh.");
                else if (m_Rules.IsFoodExpired(f, m_Session.WorldTime.TurnCounter))
                    lines.Add("*Expired*");
                else if (m_Rules.IsFoodSpoiled(f, m_Session.WorldTime.TurnCounter))
                    lines.Add("**SPOILED**");
                lines.Add(String.Format("Best-Before : {0}", f.BestBefore.ToString()));
            }
            else
                lines.Add("Always fresh.");


            // 2. Nutrition
            int nutrition = m_Rules.FoodItemNutrition(f, m_Session.WorldTime.TurnCounter);
            int nutritionForPlayer = (m_Player == null ? nutrition : m_Rules.ActorItemNutritionValue(m_Player, nutrition));
            if (nutritionForPlayer == m.Nutrition)
                lines.Add(String.Format("Nutrition   : +{0}", nutrition));
            else
                lines.Add(String.Format("Nutrition   : +{0} (+{1})", nutritionForPlayer, nutrition));

            return lines.ToArray();
        }

        string[] DescribeItemMedicine(ItemMedicine med)
        {
            List<string> lines = new List<string>();

            ItemMedicineModel m = med.Model as ItemMedicineModel;

            lines.Add("> medicine");

            // alpha10 dont add lines for zero values

            int healingForPlayer = (m_Player == null ? m.Healing : m_Rules.ActorMedicineEffect(m_Player, m.Healing));
            if (m.Healing != 0)
            {
                if (healingForPlayer == m.Healing)
                    lines.Add(String.Format("Healing : +{0}", m.Healing));
                else
                    lines.Add(String.Format("Healing : +{0} (+{1})", healingForPlayer, m.Healing));
            }

            int staminaForPlayer = (m_Player == null ? m.StaminaBoost : m_Rules.ActorMedicineEffect(m_Player, m.StaminaBoost));
            if (m.StaminaBoost != 0)
            {
                if (staminaForPlayer == m.StaminaBoost)
                    lines.Add(String.Format("Stamina : +{0}", m.StaminaBoost));
                else
                    lines.Add(String.Format("Stamina : +{0} (+{1})", staminaForPlayer, m.StaminaBoost));
            }

            int sleepForPlayer = (m_Player == null ? m.SleepBoost : m_Rules.ActorMedicineEffect(m_Player, m.SleepBoost));
            if (m.SleepBoost != 0)
            {
                if (sleepForPlayer == m.SleepBoost)
                    lines.Add(String.Format("Sleep   : +{0}", m.SleepBoost));
                else
                    lines.Add(String.Format("Sleep   : +{0} (+{1})", sleepForPlayer, m.SleepBoost));
            }

            int sanForPlayer = (m_Player == null ? m.SanityCure : m_Rules.ActorMedicineEffect(m_Player, m.SanityCure));
            if (m.SanityCure != 0)
            {
                if (sanForPlayer == m.SanityCure)
                    lines.Add(String.Format("Sanity  : +{0}", m.SanityCure));
                else
                    lines.Add(String.Format("Sanity  : +{0} (+{1})", sanForPlayer, m.SanityCure));
            }

            if (Rules.HasInfection(m_Session.GameMode))
            {
                int cureForPlayer = (m_Player == null ? m.InfectionCure : m_Rules.ActorMedicineEffect(m_Player, m.InfectionCure));
                if (m.InfectionCure != 0)
                {
                    if (cureForPlayer == m.InfectionCure)
                        lines.Add(String.Format("Cure    : +{0}", m.InfectionCure));
                    else
                        lines.Add(String.Format("Cure    : +{0} (+{1})", cureForPlayer, m.InfectionCure));
                }
            }

            return lines.ToArray();
        }

        string[] DescribeItemBarricadeMaterial(ItemBarricadeMaterial bm)
        {
            List<string> lines = new List<string>();

            ItemBarricadeMaterialModel m = bm.Model as ItemBarricadeMaterialModel;

            lines.Add("> barricade material");

            // 1. Barricading value.
            int barForPlayer = (m_Player == null ? m.BarricadingValue : m_Rules.ActorBarricadingPoints(m_Player, m.BarricadingValue));
            if (barForPlayer == m.BarricadingValue)
                lines.Add(String.Format("Barricading : +{0}", m.BarricadingValue));
            else
                lines.Add(String.Format("Barricading : +{0} (+{1})", barForPlayer, m.BarricadingValue));

            return lines.ToArray();
        }

        string[] DescribeItemBodyArmor(ItemBodyArmor b)
        {
            List<string> lines = new List<string>();

            lines.Add("> body armor");

            // 1. Protection value.
            lines.Add(string.Format("Protection vs Hits  : +{0}", b.Protection_Hit));
            lines.Add(string.Format("Protection vs Shots : +{0}", b.Protection_Shot));
            lines.Add(string.Format("Encumbrance         : -{0} DEF", b.Encumbrance));
            lines.Add(string.Format("Weight              : -{0:F2} SPD", 0.01f * b.Weight));

            // 2. Unsuspicious effects.
            List<string> unsuspicious = new List<string>();
            List<string> suspicious = new List<string>();
            if (b.IsFriendlyForCops()) unsuspicious.Add("Cops");
            if (b.IsHostileForCops()) suspicious.Add("Cops");
            foreach (GameGangs.IDs gang in GameGangs.BIKERS)
            {
                if (b.IsHostileForBiker(gang)) suspicious.Add(GameGangs.NAMES[(int)gang]);
                if (b.IsFriendlyForBiker(gang)) unsuspicious.Add(GameGangs.NAMES[(int)gang]);
            }
            // alpha10 fixed rule & desc mismatch
            //foreach (GameGangs.IDs gang in GameGangs.GANGSTAS)
            //{
            //    if (b.IsHostileForBiker(gang)) suspicious.Add(GameGangs.NAMES[(int)gang]);
            //    if (b.IsFriendlyForBiker(gang)) unsuspicious.Add(GameGangs.NAMES[(int)gang]);
            //}
            if (unsuspicious.Count > 0)
            {
                lines.Add("Unsuspicious to:");
                foreach (string s in unsuspicious)
                    lines.Add("- " + s);
            }
            if (suspicious.Count > 0)
            {
                lines.Add("Suspicious to:");
                foreach (string s in suspicious)
                    lines.Add("- " + s);
            }

            return lines.ToArray();
        }

        string[] DescribeItemSprayPaint(ItemSprayPaint sp)
        {
            List<string> lines = new List<string>();

            ItemSprayPaintModel m = sp.Model as ItemSprayPaintModel;

            lines.Add("> spray paint");

            // 1. Paint
            if (sp.PaintQuantity < m.MaxPaintQuantity)
                lines.Add(String.Format("Paint : {0}/{1}", sp.PaintQuantity, m.MaxPaintQuantity));
            else
                lines.Add(String.Format("Paint : {0} MAX", sp.PaintQuantity));

            return lines.ToArray();
        }

        string[] DescribeItemSprayScent(ItemSprayScent sp)
        {
            List<string> lines = new List<string>();

            ItemSprayScentModel m = sp.Model as ItemSprayScentModel;

            lines.Add("> spray scent");

            // 1. Spray.
            if (sp.SprayQuantity < m.MaxSprayQuantity)
                lines.Add(String.Format("Spray    : {0}/{1}", sp.SprayQuantity, m.MaxSprayQuantity));
            else
                lines.Add(String.Format("Spray    : {0} MAX", sp.SprayQuantity));

            // alpha10
            // 2. Odor & Strength
            lines.Add(string.Format("Odor     : {0}", Capitalize(sp.Odor.ToString().ToLower())));
            lines.Add(string.Format("Strength : {0}h", sp.Strength / WorldTime.TURNS_PER_HOUR));

            return lines.ToArray();
        }


        string[] DescribeItemLight(ItemLight lt)
        {
            List<string> lines = new List<string>();

            ItemLightModel m = lt.Model as ItemLightModel;

            lines.Add("> light");

            // 1. Batteries
            lines.Add(DescribeBatteries(lt.Batteries, m.MaxBatteries));

            // 2. FoV
            lines.Add(String.Format("FOV       : +{0}", lt.FovBonus));

            return lines.ToArray();
        }

        string[] DescribeItemTracker(ItemTracker tr)
        {
            List<string> lines = new List<string>();

            ItemTrackerModel m = tr.Model as ItemTrackerModel;

            lines.Add("> tracker");

            // 1. Batteries
            lines.Add(DescribeBatteries(tr.Batteries, m.MaxBatteries));
            // alpha10 range if applicable
            // TODO -- should be an tracker item property, hardcoding is baaaad -_-
            if (tr.CanTrackUndeads)
                lines.Add(string.Format("Range: {0}", Rules.ZTRACKINGRADIUS));
            else
                lines.Add("Range: whole map");

            // alpha10
            // 2. Clock
            if (tr.HasClock)
            {
                lines.Add(" ");
                if (tr.Batteries == 0)
                    lines.Add("Out of batteries, can't give the time.");
                else if (!tr.IsEquipped)
                    lines.Add("Equip the item to read the time.");
                else
                    lines.Add(string.Format("The clock reads: {0}h, {1}", m_Session.WorldTime.Hour, DescribeDayPhase(m_Session.WorldTime.Phase)));
            }

            return lines.ToArray();
        }

        string[] DescribeItemTrap(ItemTrap tr)
        {
            List<string> lines = new List<string>();

            ItemTrapModel m = tr.Model as ItemTrapModel;

            lines.Add("> trap");

            // 1. Status
            if (tr.IsActivated)
            {
                lines.Add("** Activated! **");
                // alpha10
                if (m_Rules.IsSafeFromTrap(tr, m_Player))
                {
                    lines.Add("You will safely avoid this trap.");
                    if (tr.Owner != null)
                        lines.Add(string.Format("Trap setup by {0}.", tr.Owner.Name));
                }
            }
            else if (tr.IsTriggered)
            {
                // alpha10
                lines.Add("** Triggered! **");
                if (m_Rules.IsSafeFromTrap(tr, m_Player))
                {
                    lines.Add("You will safely avoid this trap.");
                    if (tr.Owner != null)
                        lines.Add(string.Format("Trap setup by {0}.", tr.Owner.Name));
                }
            }
            // alpha10
            lines.Add(string.Format("Trigger chance for you : {0}%.", m_Rules.GetTrapTriggerChance(tr, m_Player)));

            // 2. Flags
            if (m.IsOneTimeUse) lines.Add("Desactives when triggered.");
            if (m.IsNoisy) lines.Add(String.Format("Makes {0} noise.", m.NoiseName));
            if (m.UseToActivate) lines.Add("Use to activate.");
            // if (m.IsFlammable) lines.Add("Can be put on fire.");

            // 3. Stats
            lines.Add(String.Format("Damage  : {0} x{1} = {2}", m.Damage, tr.Quantity, tr.Quantity * m.Damage));  // alpha10
            lines.Add(String.Format("Trigger : {0}% x{1} = {2}%", m.TriggerChance, tr.Quantity, tr.Quantity * m.TriggerChance));  // alpha10
            lines.Add(String.Format("Break   : {0}%", m.BreakChance));
            if (m.BlockChance > 0) lines.Add(String.Format("Block   : {0}%", m.BlockChance));
            if (m.BreakChanceWhenEscape > 0) lines.Add(String.Format("{0}% to break on escape", m.BreakChanceWhenEscape));

            return lines.ToArray();
        }

        string[] DescribeItemEntertainment(ItemEntertainment ent)
        {
            List<String> lines = new List<string>();

            ItemEntertainmentModel m = ent.EntertainmentModel;

            lines.Add("> entertainment");

            // player bored?
            if (m_Player != null && ent.IsBoringFor(m_Player)) // alpha10 boring items item centric
                lines.Add("* BORED OF IT! *");

            // San & Bore chance.
            lines.Add(String.Format("Sanity : +{0}", m.Value));
            lines.Add(String.Format("Boring : {0}%", m.BoreChance));

            return lines.ToArray();
        }

        string DescribeBatteries(int batteries, int maxBatteries)
        {
            int hours = BatteriesToHours(batteries);
            if (batteries < maxBatteries)
                return String.Format("Batteries : {0}/{1} ({2}h)", batteries, maxBatteries, hours);
            else
                return String.Format("Batteries : {0} MAX ({1}h)", batteries, hours);
        }
        #endregion

        string DescribeSkillShort(Skills.IDs id)
        {
            switch (id)
            {
                case Skills.IDs.AGILE:
                    return String.Format("+{0} melee ATK, +{1} DEF", Rules.SKILL_AGILE_ATK_BONUS, Rules.SKILL_AGILE_DEF_BONUS);
                case Skills.IDs.AWAKE:
                    return String.Format("+{0}% max SLP, +{1}% SLP regen ", (int)(100 * Rules.SKILL_AWAKE_SLEEP_BONUS), (int)(100 * Rules.SKILL_AWAKE_SLEEP_REGEN_BONUS));
                case Skills.IDs.BOWS:
                    return String.Format("bows +{0} ATK, +{1} DMG", Rules.SKILL_BOWS_ATK_BONUS, Rules.SKILL_BOWS_DMG_BONUS);
                case Skills.IDs.CARPENTRY:
                    return String.Format("build, -{0} mat. at lvl 3, +{1}% barricading", Rules.SKILL_CARPENTRY_LEVEL3_BUILD_BONUS, (int)(100 * Rules.SKILL_CARPENTRY_BARRICADING_BONUS));
                case Skills.IDs.CHARISMATIC:
                    return String.Format("+{0} trust per turn, +{1}% trade rolls, steal followers", Rules.SKILL_CHARISMATIC_TRUST_BONUS, Rules.SKILL_CHARISMATIC_TRADE_BONUS);  // alpha10.1 steal followers
                case Skills.IDs.FIREARMS:
                    return String.Format("firearms +{0} ATK, +{1} DMG", Rules.SKILL_FIREARMS_ATK_BONUS, Rules.SKILL_FIREARMS_DMG_BONUS);
                case Skills.IDs.HARDY:
                    return String.Format("sleeping anywhere heals, +{0}% chance to heal when sleeping", Rules.SKILL_HARDY_HEAL_CHANCE_BONUS);
                case Skills.IDs.HAULER:
                    return String.Format("+{0} inventory slots", Rules.SKILL_HAULER_INV_BONUS);
                case Skills.IDs.HIGH_STAMINA:
                    return String.Format("+{0} STA", Rules.SKILL_HIGH_STAMINA_STA_BONUS);
                case Skills.IDs.LEADERSHIP:
                    return String.Format("+{0} max Followers", Rules.SKILL_LEADERSHIP_FOLLOWER_BONUS);
                case Skills.IDs.LIGHT_EATER:
                    return String.Format("+{0}% max FOO, +{1}% items food points", (int)(100 * Rules.SKILL_LIGHT_EATER_MAXFOOD_BONUS), (int)(100 * Rules.SKILL_LIGHT_EATER_FOOD_BONUS));
                case Skills.IDs.LIGHT_FEET:
                    return String.Format("+{0}% to avoid and escape traps", Rules.SKILL_LIGHT_FEET_TRAP_BONUS);
                case Skills.IDs.LIGHT_SLEEPER:
                    return String.Format("+{0}% noise wake up chance", Rules.SKILL_LIGHT_SLEEPER_WAKEUP_CHANCE_BONUS);
                case Skills.IDs.MARTIAL_ARTS:
                    return String.Format("unarmed only +{0} ATK, +{1} DMG, +{2}% disarm", Rules.SKILL_MARTIAL_ARTS_ATK_BONUS, Rules.SKILL_MARTIAL_ARTS_DMG_BONUS, Rules.SKILL_MARTIAL_ARTS_DISARM_BONUS);
                case Skills.IDs.MEDIC:
                    return String.Format("+{0}% medicine items effects, +{1}% revive ", (int)(100 * Rules.SKILL_MEDIC_BONUS), Rules.SKILL_MEDIC_REVIVE_BONUS);
                case Skills.IDs.NECROLOGY:
                    return String.Format("+{0}/+{1} DMG vs undeads/corpses, data on corpses", Rules.SKILL_NECROLOGY_UNDEAD_BONUS, Rules.SKILL_NECROLOGY_CORPSE_BONUS);
                case Skills.IDs.STRONG:
                    return String.Format("+{0} melee DMG, +{1}% resist disarming, +{2} throw range", Rules.SKILL_STRONG_DMG_BONUS, Rules.SKILL_STRONG_RESIST_DISARM_BONUS, Rules.SKILL_STRONG_THROW_BONUS);
                case Skills.IDs.STRONG_PSYCHE:
                    return String.Format("+{0}% SAN threshold", (int)(100 * Rules.SKILL_STRONG_PSYCHE_LEVEL_BONUS));
                case Skills.IDs.TOUGH:
                    return String.Format("+{0} HP", Rules.SKILL_TOUGH_HP_BONUS);
                case Skills.IDs.UNSUSPICIOUS:
                    return String.Format("+{0}% unnoticed by law enforcers and gangs", Rules.SKILL_UNSUSPICIOUS_BONUS);

                case Skills.IDs.Z_AGILE:
                    return String.Format("+{0} melee ATK, +{1} DEF, can jump", Rules.SKILL_ZAGILE_ATK_BONUS, Rules.SKILL_ZAGILE_DEF_BONUS);
                case Skills.IDs.Z_EATER:
                    return String.Format("+{0}% eating HP regen", (int)(100 * Rules.SKILL_ZEATER_REGEN_BONUS));
                case Skills.IDs.Z_GRAB:
                    return String.Format("can grab enemies, +{0}% per level", Rules.SKILL_ZGRAB_CHANCE);
                case Skills.IDs.Z_INFECTOR:
                    return String.Format("+{0}% infection damage", (int)(100 * Rules.SKILL_ZINFECTOR_BONUS));
                case Skills.IDs.Z_LIGHT_EATER:
                    return String.Format("+{0}% max ROT, +{1}% from eating", (int)(100 * Rules.SKILL_ZLIGHT_EATER_MAXFOOD_BONUS), (int)(100 * Rules.SKILL_ZLIGHT_EATER_FOOD_BONUS));
                case Skills.IDs.Z_LIGHT_FEET:
                    return String.Format("+{0}% to avoid traps", Rules.SKILL_ZLIGHT_FEET_TRAP_BONUS);
                case Skills.IDs.Z_STRONG:
                    return String.Format("+{0} melee DMG, can push", Rules.SKILL_ZSTRONG_DMG_BONUS);
                case Skills.IDs.Z_TOUGH:
                    return String.Format("+{0} HP", Rules.SKILL_ZTOUGH_HP_BONUS);
                case Skills.IDs.Z_TRACKER:
                    return String.Format("+{0}% smell", (int)(100 * Rules.SKILL_ZTRACKER_SMELL_BONUS));

                default:
                    throw new ArgumentOutOfRangeException("unhandled skill id");
            }
        }

        string DescribeDayPhase(DayPhase phase)
        {
            switch (phase)
            {
                case DayPhase.AFTERNOON: return "Afternoon";
                case DayPhase.DEEP_NIGHT: return "Deep Night";
                case DayPhase.EVENING: return "Evening";
                case DayPhase.MIDDAY: return "Midday";
                case DayPhase.MIDNIGHT: return "Midnight";
                case DayPhase.MORNING: return "Morning";
                case DayPhase.SUNRISE: return "Sunrise";
                case DayPhase.SUNSET: return "Sunset";

                default: throw new ArgumentOutOfRangeException("unhandled dayphase");
            }
        }

        string DescribeWeather(Weather weather)
        {
            switch (weather)
            {
                case Weather.CLOUDY: return "Cloudy";
                case Weather.HEAVY_RAIN: return "Heavy rain";
                case Weather.RAIN: return "Rain";
                case Weather.CLEAR: return "Clear";

                default:
                    throw new ArgumentOutOfRangeException("unhandled weather");
            }
        }

        Color WeatherColor(Weather weather)
        {
            switch (weather)
            {
                case Weather.CLOUDY: return Color.Gray;
                case Weather.HEAVY_RAIN: return Color.Blue;
                case Weather.RAIN: return Color.LightBlue;
                case Weather.CLEAR: return Color.Yellow;

                default:
                    throw new ArgumentOutOfRangeException("unhandled weather");
            }
        }

        int BatteriesToHours(int batteries)
        {
            return batteries / WorldTime.TURNS_PER_HOUR;
        }

        int FoodToHoursUntilHungry(int food)
        {
            int left = food - Rules.FOOD_HUNGRY_LEVEL;
            if (left <= 0)
                return 0;
            return left / WorldTime.TURNS_PER_HOUR;
        }

        int FoodToHoursUntilRotHungry(int food)
        {
            int left = food - Rules.ROT_HUNGRY_LEVEL;
            if (left <= 0)
                return 0;
            return left / WorldTime.TURNS_PER_HOUR;
        }

        public bool IsAlmostHungry(Actor actor)
        {
            if (!actor.Model.Abilities.HasToEat)
                return false;
            return FoodToHoursUntilHungry(actor.FoodPoints) <= 3;
        }

        public bool IsAlmostRotHungry(Actor actor)
        {
            if (!actor.Model.Abilities.IsRotting)
                return false;
            return FoodToHoursUntilRotHungry(actor.FoodPoints) <= 3;
        }
        #endregion

        #region Translating commands
        public static Direction CommandToDirection(PlayerCommand cmd)
        {
            switch (cmd)
            {
                case PlayerCommand.MOVE_N:
                    return Direction.N;
                case PlayerCommand.MOVE_NE:
                    return Direction.NE;
                case PlayerCommand.MOVE_E:
                    return Direction.E;
                case PlayerCommand.MOVE_SE:
                    return Direction.SE;
                case PlayerCommand.MOVE_S:
                    return Direction.S;
                case PlayerCommand.MOVE_SW:
                    return Direction.SW;
                case PlayerCommand.MOVE_W:
                    return Direction.W;
                case PlayerCommand.MOVE_NW:
                    return Direction.NW;
                case PlayerCommand.WAIT_OR_SELF:
                    return Direction.NEUTRAL;

                default:
                    return null;
            }
        }
        #endregion

        #region Actions primitives: DoXXX

        #region Movement
        public void DoMoveActor(Actor actor, Location newLocation)
        {
            Location oldLocation = actor.Location;

            // Try to leave tile.
            if (!TryActorLeaveTile(actor))
            {
                // waste ap.
                SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
                return;
            }

            // Do the move.
            if (oldLocation.Map == newLocation.Map)
                newLocation.Map.PlaceActorAt(actor, newLocation.Position);
            else
                throw new NotImplementedException("DoMoveActor : illegal to change map.");

            // If dragging corpse, move it along.
            Corpse draggedCorpse = actor.DraggedCorpse;
            if (draggedCorpse != null)
            {
                oldLocation.Map.MoveCorpseTo(draggedCorpse, newLocation.Position);
                if (IsVisibleToPlayer(newLocation) || IsVisibleToPlayer(oldLocation))
                    AddMessage(MakeMessage(actor, String.Format("{0} {1} corpse.", Conjugate(actor, VERB_DRAG), draggedCorpse.DeadGuy.TheName)));
            }

            // Spend AP & STA, check for running, jumping and dragging corpse.
            #region
            int moveCost = Rules.BASE_ACTION_COST;

            // running?
            if (actor.IsRunning)
            {
                // x2 faster.
                moveCost /= 2;
                // cost STA.
                SpendActorStaminaPoints(actor, Rules.STAMINA_COST_RUNNING);
            }

            bool isJump = false;
            MapObject mapObj = newLocation.Map.GetMapObjectAt(newLocation.Position.X, newLocation.Position.Y);
            if (mapObj != null && !mapObj.IsWalkable && mapObj.IsJumpable)
                isJump = true;

            // jumping?
            if (isJump)
            {
                // cost STA.
                SpendActorStaminaPoints(actor, Rules.STAMINA_COST_JUMP);

                // show.
                if (IsVisibleToPlayer(actor))
                    AddMessage(MakeMessage(actor, Conjugate(actor, VERB_JUMP_ON), mapObj));

                // if CanJumpStumble ability, has a chance to stumble.
                if (actor.Model.Abilities.CanJumpStumble && m_Rules.RollChance(Rules.JUMP_STUMBLE_CHANCE))
                {
                    // stumble!
                    moveCost += Rules.JUMP_STUMBLE_ACTION_COST;

                    // show.
                    if (IsVisibleToPlayer(actor))
                        AddMessage(MakeMessage(actor, String.Format("{0}!", Conjugate(actor, VERB_STUMBLE))));
                }
            }

            // dragging?
            if (draggedCorpse != null)
            {
                // cost STA.
                SpendActorStaminaPoints(actor, Rules.STAMINA_COST_MOVE_DRAGGED_CORPSE);
            }

            // spend move AP.
            SpendActorActionPoints(actor, moveCost);
            #endregion

            // If actor can move again, make sure he drops his scent here.
            // If we don't do this, since scents are dropped only in new turns,
            // there will be "holes" in the scent paths, and this is not fair
            // for zombies who will loose track of running livings easily.
            if (actor.ActionPoints > 0) // alpha10 fix; was Rules.BASE_ACTION_COST
                DropActorScents(actor);

            // Screams of terror?
            #region
            if (!actor.IsPlayer &&
                (actor.Activity == Activity.FLEEING || actor.Activity == Activity.FLEEING_FROM_EXPLOSIVE) &&
                !actor.Model.Abilities.IsUndead &&
                actor.Model.Abilities.CanTalk)
            {
                // loud noise.
                OnLoudNoise(newLocation.Map, newLocation.Position, "A loud SCREAM");

                // player hears?
                if (m_Rules.RollChance(PLAYER_HEAR_SCREAMS_CHANCE) && !IsVisibleToPlayer(actor))
                {
                    AddMessageIfAudibleForPlayer(actor.Location, MakePlayerCentricMessage("You hear screams of terror", actor.Location.Position));
                }
            }
            #endregion

            // Trigger stuff.
            OnActorEnterTile(actor);
        }

        public void DoMoveActor(Actor actor, Direction direction)
        {
            DoMoveActor(actor, actor.Location + direction);
        }

        public void OnActorEnterTile(Actor actor)
        {
            Map map = actor.Location.Map;
            Point pos = actor.Location.Position;

            // Check traps.
            // Don't check if there is a covering mobj there.
            if (!m_Rules.IsTrapCoveringMapObjectThere(map, pos))
            {
                Inventory itemsThere = map.GetItemsAt(pos);
                if (itemsThere != null)
                {
                    List<Item> removeThem = null;
                    foreach (Item it in itemsThere.Items)
                    {
                        ItemTrap trap = it as ItemTrap;
                        if (trap == null || !trap.IsActivated)
                            continue;
                        if (TryTriggerTrap(trap, actor))
                        {
                            if (removeThem == null) removeThem = new List<Item>(itemsThere.CountItems);
                            removeThem.Add(it);
                        }
                    }
                    if (removeThem != null)
                    {
                        foreach (Item it in removeThem)
                            map.RemoveItemAt(it, pos);
                    }
                    // Kill actor?
                    if (actor.HitPoints <= 0)
                        KillActor(null, actor, "trap");
                }
            }
        }

        bool TryActorLeaveTile(Actor actor)
        {
            Map map = actor.Location.Map;
            Point pos = actor.Location.Position;
            bool canLeave = true;

            // Check traps.
            if (!m_Rules.IsTrapCoveringMapObjectThere(map, pos))
            {
                Inventory itemsThere = map.GetItemsAt(pos);
                if (itemsThere != null)
                {
                    List<Item> removeThem = null;
                    bool hasTriggeredTraps = false;
                    foreach (Item it in itemsThere.Items)
                    {
                        ItemTrap trap = it as ItemTrap;
                        if (trap == null || !trap.IsTriggered)
                            continue;
                        hasTriggeredTraps = true;
                        bool isDestroyed = false;
                        if (!TryEscapeTrap(trap, actor, out isDestroyed))
                        {
                            canLeave = false;
                            continue;
                        }
                        if (isDestroyed)
                        {
                            if (removeThem == null) removeThem = new List<Item>(itemsThere.CountItems);
                            removeThem.Add(it);
                        }
                    }
                    if (removeThem != null)
                    {
                        foreach (Item it in removeThem)
                            map.RemoveItemAt(it, pos);
                    }
                    // if can leave, force un-trigger all traps.
                    if (canLeave && hasTriggeredTraps)
                        UntriggerAllTrapsHere(actor.Location);
                }
            }

            // Check adjacent Z-Grabs
            bool visible = IsVisibleToPlayer(actor);
            map.ForEachAdjacentInMap(pos, (adj) =>
            {
                Actor grabber = map.GetActorAt(adj);
                if (grabber == null)
                    return;
                if (!grabber.Model.Abilities.IsUndead)
                    return;
                if (!m_Rules.AreEnemies(grabber, actor))
                    return;
                int chance = m_Rules.ZGrabChance(grabber, actor);
                if (chance == 0)
                    return;
                if (m_Rules.RollChance(m_Rules.ZGrabChance(grabber, actor)))
                {
                    // grabbed!
                    if (visible)
                        AddMessage(MakeMessage(grabber, Conjugate(grabber, VERB_GRAB), actor));
                    // stuck there!
                    canLeave = false;
                }
            });

            return canLeave;
        }
        #endregion

        #region Traps
        /// <summary>
        /// @return true if item must be removed (destroyed).
        /// </summary>
        /// <param name="trap"></param>
        /// <returns></returns>
        bool TryTriggerTrap(ItemTrap trap, Actor victim)
        {
            // check trigger chance.
            if (m_Rules.CheckTrapTriggers(trap, victim))
                DoTriggerTrap(trap, victim.Location.Map, victim.Location.Position, victim, null);
            else
            {
                if (IsVisibleToPlayer(victim))
                    AddMessage(MakeMessage(victim, String.Format("safely {0} {1}.", Conjugate(victim, VERB_AVOID), trap.TheName)));
            }
            // destroy?
            return trap.Quantity == 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trap"></param>
        /// <param name="victim"></param>
        /// <param name="isDestroyed"></param>
        /// <returns>true succesful escape</returns>
        bool TryEscapeTrap(ItemTrap trap, Actor victim, out bool isDestroyed)
        {
            isDestroyed = false;
            ItemTrapModel model = trap.TrapModel;

            // no brainer.
            if (model.BlockChance <= 0)
                return true;

            bool visible = IsVisibleToPlayer(victim);
            bool canEscape = false;

            // check escape chance.
            if (m_Rules.CheckTrapEscape(trap, victim))
            {
                // un-triggered and escape.
                trap.IsTriggered = false;
                canEscape = true;

                // tell
                if (visible)
                    AddMessage(MakeMessage(victim, String.Format("{0} {1}.", Conjugate(victim, VERB_ESCAPE), trap.TheName)));

                // then check break on escape chance.
                if (m_Rules.CheckTrapEscapeBreaks(trap, victim))
                {
                    if (visible)
                        AddMessage(MakeMessage(victim, String.Format("{0} {1}.", Conjugate(victim, VERB_BREAK), trap.TheName)));
                    --trap.Quantity;
                    isDestroyed = trap.Quantity <= 0;
                }
            }
            else
            {
                // tell
                if (visible)
                    AddMessage(MakeMessage(victim, String.Format("is trapped by {0}!", trap.TheName)));
            }

            // escape?
            return canEscape;
        }

        void UntriggerAllTrapsHere(Location loc)
        {
            Inventory itemsThere = loc.Map.GetItemsAt(loc.Position);
            if (itemsThere == null) return;
            foreach (Item it in itemsThere.Items)
            {
                ItemTrap trap = it as ItemTrap;
                if (trap == null || !trap.IsTriggered)
                    continue;
                trap.IsTriggered = false;
            }
        }

        /// <summary>
        /// Checks that there is a map object that triggers the traps there.
        /// If so, a map object triggers ALL activated the traps here.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="pos"></param>
        void CheckMapObjectTriggersTraps(Map map, Point pos)
        {
            if (!m_Rules.IsTrapTriggeringMapObjectThere(map, pos))
                return;

            MapObject mobj = map.GetMapObjectAt(pos);

            Inventory itemsThere = map.GetItemsAt(pos);
            if (itemsThere == null) return;

            List<Item> removeThem = null;
            foreach (Item it in itemsThere.Items)
            {
                ItemTrap trap = it as ItemTrap;
                if (trap == null || !trap.IsActivated)
                    continue;
                DoTriggerTrap(trap, map, pos, null, mobj);
                if (trap.Quantity <= 0)
                {
                    if (removeThem == null) removeThem = new List<Item>(itemsThere.CountItems);
                    removeThem.Add(it);
                }
            }
            if (removeThem != null)
            {
                foreach (Item it in removeThem)
                    map.RemoveItemAt(it, pos);
            }
        }

        /// <summary>
        /// Trigger a trap by an actor or a map object.
        /// </summary>
        /// <param name="trap"></param>
        /// <param name="map"></param>
        /// <param name="pos"></param>
        /// <param name="victim"></param>
        /// <param name="mobj"></param>
        void DoTriggerTrap(ItemTrap trap, Map map, Point pos, Actor victim, MapObject mobj)
        {
            ItemTrapModel model = trap.TrapModel;
            bool visible = IsVisibleToPlayer(map, pos);

            // flag.
            trap.IsTriggered = true;

            // effect: damage on victim? (actor)
            int damage = model.Damage * trap.Quantity;
            if (damage > 0 && victim != null)
            {
                InflictDamage(victim, damage);
                if (visible)
                {
                    AddMessage(MakeMessage(victim, String.Format("is hurt by {0} for {1} damage!", trap.AName, damage)));
                    AddOverlay(new OverlayImage(MapToScreen(victim.Location.Position), GameImages.ICON_MELEE_DAMAGE));
                    AddOverlay(new OverlayText(MapToScreen(victim.Location.Position).Add(DAMAGE_DX, DAMAGE_DY), Color.White, damage.ToString(), Color.Black));
                    RedrawPlayScreen();
                    AnimDelay(victim.IsPlayer ? DELAY_NORMAL : DELAY_SHORT);
                    ClearOverlays();
                    RedrawPlayScreen();
                }
            }
            // effect: noise? (actor, mobj)
            if (model.IsNoisy)
            {
                if (visible)
                {
                    if (victim != null)
                        AddMessage(MakeMessage(victim, String.Format("stepping on {0} makes a bunch of noise!", trap.AName)));
                    else if (mobj != null)
                        AddMessage(new Message(String.Format("{0} makes a lot of noise!", Capitalize(trap.TheName)), map.LocalTime.TurnCounter));
                }
                OnLoudNoise(map, pos, model.NoiseName);
            }

            // if one time trigger = desactivate.
            if (model.IsOneTimeUse)
                trap.Desactivate();  //alpha10 //trap.IsActivated = false;

            // then check break chance (actor, mobj)
            if (m_Rules.CheckTrapStepOnBreaks(trap, mobj))
            {
                if (visible)
                {
                    if (victim != null)
                        AddMessage(MakeMessage(victim, String.Format("{0} {1}.", Conjugate(victim, VERB_CRUSH), trap.TheName)));
                    else if (mobj != null)
                        AddMessage(new Message(String.Format("{0} breaks the {1}.", Capitalize(mobj.TheName), trap.TheName), map.LocalTime.TurnCounter));
                }
                --trap.Quantity;
            }
        }
        #endregion

        #region Leaving maps & Using exits.
        public bool DoLeaveMap(Actor actor, Point exitPoint, bool askForConfirmation)
        {
            bool isPlayer = actor.IsPlayer;

            Map fromMap = actor.Location.Map;
            Point fromPos = actor.Location.Position;

            // get exit.
            Exit exit = fromMap.GetExitAt(exitPoint);
            if (exit == null)
            {
                if (isPlayer)
                {
                    AddMessage(MakeErrorMessage("There is nowhere to go there."));
                }
                return true;
            }

            // if player, ask for a confirmation.
            if (isPlayer && askForConfirmation)
            {
                ClearMessages();
                AddMessage(MakeYesNoMessage(String.Format("REALLY LEAVE {0}", fromMap.Name)));
                RedrawPlayScreen();
                bool confirm = WaitYesOrNo();
                if (!confirm)
                {
                    AddMessage(new Message("Let's stay here a bit longer...", m_Session.WorldTime.TurnCounter, Color.Yellow));
                    RedrawPlayScreen();
                    return false;
                }
            }

            // alpha10.1 check autosave before player leaving map
            if (isPlayer)
                CheckAutoSaveTime();

            // Try to leave tile.
            if (!TryActorLeaveTile(actor))
            {
                // waste ap.
                SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
                return false;
            }

            // spend AP **IF AI**
            if (!actor.IsPlayer)
                SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // if player is leaving and changing district, prepare district.
            // alpha10.1 disallow bots from leaving districts
            bool playerChangedDistrict = false;  // alpha10
            if (isPlayer && !actor.IsBotPlayer && exit.ToMap.District != fromMap.District)
            {
                playerChangedDistrict = true;  // alpha10
                BeforePlayerEnterDistrict(exit.ToMap.District);
            }

            /////////////////////////////////////
            // 1. If spot not available, cancel.
            // 2. Remove from previous map (+ corpse)
            // 3. Enter map (+corpse).
            // 4. Handle followers.
            /////////////////////////////////////

            // 1. If spot not available, cancel.
            Actor other = exit.ToMap.GetActorAt(exit.ToPosition);
            if (other != null)
            {
                if (isPlayer)
                {
                    AddMessage(MakeErrorMessage(String.Format("{0} is blocking your way.", other.Name)));
                }
                return true;
            }
            MapObject blockingObj = exit.ToMap.GetMapObjectAt(exit.ToPosition);
            if (blockingObj != null)
            {
                bool canJump = blockingObj.IsJumpable && m_Rules.HasActorJumpAbility(actor);
                bool ignoreIt = blockingObj.IsCouch;
                if (!canJump && !ignoreIt)
                {
                    if (isPlayer)
                    {
                        AddMessage(MakeErrorMessage(String.Format("{0} is blocking your way.", blockingObj.AName)));
                    }
                    return true;
                }
            }

            // 2. Remove from previous map (+corpse)
            if (IsVisibleToPlayer(actor))
            {
                AddMessage(MakeMessage(actor, String.Format("{0} {1}.", Conjugate(actor, VERB_LEAVE), fromMap.Name)));
            }
            fromMap.RemoveActor(actor);
            if (actor.DraggedCorpse != null)
                fromMap.RemoveCorpse(actor.DraggedCorpse);
            if (isPlayer && exit.ToMap.District != fromMap.District)
            {
                OnPlayerLeaveDistrict();
            }

            // 3. Enter map (+corpse)
            exit.ToMap.PlaceActorAt(actor, exit.ToPosition);
            exit.ToMap.MoveActorToFirstPosition(actor);
            if (actor.DraggedCorpse != null)
            {
                exit.ToMap.AddCorpseAt(actor.DraggedCorpse, exit.ToPosition);
            }
            if (IsVisibleToPlayer(actor) || isPlayer)
            {
                AddMessage(MakeMessage(actor, String.Format("{0} {1}.", Conjugate(actor, VERB_ENTER), exit.ToMap.Name)));
            }
            if (isPlayer)
            {
                // scoring event.
                if (fromMap.District != exit.ToMap.District)
                {
                    m_Session.Scoring.AddEvent(m_Session.WorldTime.TurnCounter, String.Format("Entered district {0}.", exit.ToMap.District.Name));
                }

                // change map.
                SetCurrentMap(exit.ToMap);
            }
            // Trigger stuff.
            OnActorEnterTile(actor);
            // 4. Handle followers.
            if (actor.CountFollowers > 0)
            {
                DoFollowersEnterMap(actor, fromMap, fromPos, exit.ToMap, exit.ToPosition);
            }

            // alpha10
            // handle player changing district
            if (playerChangedDistrict)
                AfterPlayerEnterDistrict();

            // done.
            return true;
        }

        void DoFollowersEnterMap(Actor leader, Map fromMap, Point fromPos, Map toMap, Point toPos)
        {
            bool leavePeopleBehind = toMap.District != fromMap.District;
            bool isPlayer = m_Player == leader;
            List<Actor> leftBehind = null;

            foreach (Actor fo in leader.Followers)
            {
                // can follow only if was adj to leader and find free adj spot on the new map.
                bool canFollow = false;
                List<Point> adjList = null;

                if (m_Rules.IsAdjacent(fromPos, fo.Location.Position))
                {
                    adjList = toMap.FilterAdjacentInMap(toPos, (pt) => m_Rules.IsWalkableFor(fo, toMap, pt.X, pt.Y));
                    if (adjList == null || adjList.Count == 0)
                        canFollow = false;
                    else
                        canFollow = true;
                }

                if (!canFollow)
                {
                    // cannot follow.
                    if (leftBehind == null) leftBehind = new List<Actor>(3);
                    leftBehind.Add(fo);
                }
                else
                {
                    // can follow, do it now.
                    // Try to leave tile.
                    if (TryActorLeaveTile(fo))
                    {
                        Point spot = adjList[m_Rules.Roll(0, adjList.Count)];
                        fromMap.RemoveActor(fo);
                        toMap.PlaceActorAt(fo, spot);
                        toMap.MoveActorToFirstPosition(fo);
                        // Trigger stuff.
                        OnActorEnterTile(fo);
                    }
                }
            }

            // make followers left behind leave if must.
            if (leftBehind != null)
            {
                foreach (Actor leaveMe in leftBehind)
                {
                    if (leavePeopleBehind)
                    {
                        leader.RemoveFollower(leaveMe);
                        if (isPlayer)
                        {
                            // scoring.
                            m_Session.Scoring.AddEvent(m_Session.WorldTime.TurnCounter, String.Format("{0} was left behind.", leaveMe.TheName));

                            // message.
                            ClearMessages();
                            AddMessage(new Message(String.Format("{0} could not follow you out of the district and left you!", leaveMe.TheName), m_Session.WorldTime.TurnCounter, Color.Red));
                            AddMessagePressEnter();
                            ClearMessages();
                        }
                    }
                    else
                    {
                        if (leaveMe.Location.Map == fromMap)
                        {
                            if (isPlayer)
                            {
                                // message.
                                ClearMessages();
                                AddMessage(new Message(String.Format("{0} could not follow and is still in {1}.", leaveMe.TheName, fromMap.Name), m_Session.WorldTime.TurnCounter, Color.Yellow));
                                AddMessagePressEnter();
                                ClearMessages();
                            }
                        }
                    }
                }
            }
        }

        public bool DoUseExit(Actor actor, Point exitPoint)
        {
            // leave map.
            return DoLeaveMap(actor, exitPoint, false);
        }
        #endregion

        #region Leading
        public void DoSwitchPlace(Actor actor, Actor other)
        {
            // spend a bunch of ap.
            SpendActorActionPoints(actor, 2 * Rules.BASE_ACTION_COST);

            // swap positions.
            Map map = other.Location.Map;
            Point actorPos = actor.Location.Position;
            map.RemoveActor(other);
            map.PlaceActorAt(actor, other.Location.Position);
            map.PlaceActorAt(other, actorPos);

            // message.
            if (IsVisibleToPlayer(actor) || IsVisibleToPlayer(other))
            {
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_SWITCH_PLACE_WITH), other));
            }
        }

        public void DoTakeLead(Actor actor, Actor other)
        {
            // spend AP.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // take lead.
            actor.AddFollower(other);

            // reset trust in leader.
            int prevTrust = other.GetTrustIn(actor);
            other.TrustInLeader = prevTrust;

            // message.
            if (IsVisibleToPlayer(actor) || IsVisibleToPlayer(other))
            {
                if (actor == m_Player)
                    ClearMessages();
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_PERSUADE), other, " to join."));
                if (prevTrust != 0)
                    DoSay(other, actor, "Ah yes I remember you.", Sayflags.IS_FREE_ACTION);
            }
        }

        // alpha10.1 
        public void DoStealLead(Actor actor, Actor other)
        {
            Actor prevLeader = other.Leader;

            // spend AP.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // remove from previous leader
            prevLeader.RemoveFollower(other);

            // take lead.
            actor.AddFollower(other);

            // reset trust in leader.
            int prevTrust = other.GetTrustIn(actor);
            other.TrustInLeader = prevTrust;

            // message.
            if (IsVisibleToPlayer(actor) || IsVisibleToPlayer(other))
            {
                if (actor == m_Player)
                    ClearMessages();
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_PERSUADE), other, String.Format(" to leave {0} and join.", prevLeader.Name)));
                if (prevTrust != 0)
                    DoSay(other, actor, "Ah yes I remember you.", Sayflags.IS_FREE_ACTION);
            }
        }

        public void DoCancelLead(Actor actor, Actor follower)
        {
            // spend AP.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // remove lead.
            actor.RemoveFollower(follower);

            // reset trust in leader.
            follower.SetTrustIn(actor, follower.TrustInLeader);
            follower.TrustInLeader = Rules.TRUST_NEUTRAL;

            // message.
            if (IsVisibleToPlayer(actor) || IsVisibleToPlayer(follower))
            {
                if (actor == m_Player)
                    ClearMessages();
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_PERSUADE), follower, " to leave."));
            }
        }
        #endregion

        #region Waiting
        public void DoWait(Actor actor)
        {
            // spend AP.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // message.
            if (IsVisibleToPlayer(actor))
            {
                if (actor.StaminaPoints < m_Rules.ActorMaxSTA(actor))
                    AddMessage(MakeMessage(actor, String.Format("{0} {1} breath.", Conjugate(actor, VERB_CATCH), HisOrHer(actor))));
                else
                    AddMessage(MakeMessage(actor, String.Format("{0}.", Conjugate(actor, VERB_WAIT))));
            }

            // regen STA.
            RegenActorStaminaPoints(actor, Rules.STAMINA_REGEN_WAIT);
        }
        #endregion

        #region Bumping
        public bool DoPlayerBump(Actor player, Direction direction)
        {
            ActionBump bump = new ActionBump(player, this, direction);

            if (bump == null)
                return false;

            // special case: tearing down barricades as living.
            // alpha10.1 moved up because civs models can now bash doors as a bump action; added break check and simplified test
            if ((bump.ConcreteAction is ActionBreak || bump.ConcreteAction is ActionBashDoor) && !player.Model.Abilities.IsUndead)
            {
                string doWhat = bump.ConcreteAction is ActionBreak ? ("break " + (bump.ConcreteAction as ActionBreak).MapObject.TheName) : "tear down the barricade";

                if (m_Rules.IsActorTired(player))
                {
                    AddMessage(MakeErrorMessage("Too tired to " + doWhat + "."));
                    RedrawPlayScreen();
                    return false;
                }
                else
                {
                    // ask for confirmation.
                    AddMessage(MakeYesNoMessage("Really " + doWhat));
                    RedrawPlayScreen();
                    bool confirm = WaitYesOrNo();

                    if (confirm)
                    {
                        //DoBreak(player, door);
                        bump.ConcreteAction.Perform();
                        return true;
                    }
                    else
                    {
                        AddMessage(new Message("Good, keep everything secure.", m_Session.WorldTime.TurnCounter, Color.Yellow));
                        return false;
                    }
                }
                //DoorWindow door = player.Location.Map.GetMapObjectAt(player.Location.Position + direction) as DoorWindow;
                //if (door != null && door.IsBarricaded && !player.Model.Abilities.IsUndead)
                //{
                //    if (!m_Rules.IsActorTired(player))
                //    {
                //        // ask for confirmation.
                //        AddMessage(MakeYesNoMessage("Really tear down the barricade"));
                //        RedrawPlayScreen();
                //        bool confirm = WaitYesOrNo();

                //        if (confirm)
                //        {
                //            DoBreak(player, door);
                //            return true;
                //        }
                //        else
                //        {
                //            AddMessage(new Message("Good, keep everything secure.", m_Session.WorldTime.TurnCounter, Color.Yellow));
                //            return false;
                //        }
                //    }
                //    else
                //    {
                //        AddMessage(MakeErrorMessage("Too tired to tear down the barricade."));
                //        RedrawPlayScreen();
                //        return false;
                //    }
                //}
            }

            if (bump.IsLegal())
            {
                bump.Perform();
                return true;
            }

            AddMessage(MakeErrorMessage(String.Format("Cannot do that : {0}.", bump.FailReason)));
            return false;
        }
        #endregion

        #region Making enemies & Attacking
        public void DoMakeAggression(Actor aggressor, Actor target)
        {
            // no need if in enemy factions.
            if (aggressor.Faction.IsEnemyOf(target.Faction))
                return;

            bool alreadyEnemies = aggressor.IsAggressorOf(target) || target.IsAggressorOf(aggressor);

            // if target is AI and has not aggressor as enemy, emote.
            if (!target.IsPlayer && !target.IsSleeping && !aggressor.IsAggressorOf(target) && !target.IsAggressorOf(aggressor))
                DoSay(target, aggressor, "BASTARD! TRAITOR!", Sayflags.IS_FREE_ACTION | Sayflags.IS_DANGER);

            // aggressor and selfdefence
            aggressor.MarkAsAgressorOf(target);
            target.MarkAsSelfDefenceFrom(aggressor);


            // then handle special cases.
            // make enemy of all faction actors on maps:
            // 1. Making an enemy of cops.
            // 2. Making an enemy of soldiers.
            #region
            if (!target.IsSleeping)
            {
                Faction tFaction = target.Faction;
                // 1. Making an enemy of cops.
                if (tFaction == GameFactions.ThePolice)
                {
                    // only non-law enforcers or murderers make enemies of cops by attacking cops.
                    if (!aggressor.Model.Abilities.IsLawEnforcer || m_Rules.IsMurder(aggressor, target))
                        OnMakeEnemyOfCop(aggressor, target, alreadyEnemies);
                }
                // 2. Making an enemy of soldiers.
                else if (tFaction == GameFactions.TheArmy)
                {
                    OnMakeEnemyOfSoldier(aggressor, target, alreadyEnemies);
                }
            }
            #endregion
        }

        // FIXME factorize common code with OnMakeEnemyOfSoldier
        void OnMakeEnemyOfCop(Actor aggressor, Actor cop, bool wasAlreadyEnemy)
        {
            // say.
            if (!wasAlreadyEnemy)
                DoSay(cop, aggressor, String.Format("TO DISTRICT PATROLS : {0} MUST DIE!", aggressor.TheName), Sayflags.IS_FREE_ACTION | Sayflags.IS_DANGER);

            // make enemy of all cops in the district.
            MakeEnemyOfTargetFactionInDistrict(aggressor, cop,
                (a) =>
                {
                    if (a.IsPlayer && a != cop && !a.IsSleeping && !m_Rules.AreEnemies(a, aggressor))
                    {
                        int turn = m_Session.WorldTime.TurnCounter;
                        ClearMessages();
                        AddMessage(new Message("You get a message from your police radio.", turn, Color.White));
                        AddMessage(new Message(String.Format("{0} is armed and dangerous. Shoot on sight!", aggressor.TheName), turn, Color.White));
                        AddMessage(new Message(String.Format("Current location : {0}@{1},{2}", aggressor.Location.Map.Name, aggressor.Location.Position.X, aggressor.Location.Position.Y), turn, Color.White));
                        if (!a.IsBotPlayer)
                            AddMessagePressEnter();
                    }
                });
        }

        // FIXME factorize common code with OnMakeEnemyOfCop
        void OnMakeEnemyOfSoldier(Actor aggressor, Actor soldier, bool wasAlreadyEnemy)
        {
            // say.
            if (!wasAlreadyEnemy)
                DoSay(soldier, aggressor, String.Format("TO DISTRICT SQUADS : {0} MUST DIE!", aggressor.TheName), Sayflags.IS_FREE_ACTION | Sayflags.IS_DANGER);

            // make enemy of all cops in the district.
            MakeEnemyOfTargetFactionInDistrict(aggressor, soldier,
                (a) =>
                {
                    if (a.IsPlayer && a != soldier && !a.IsSleeping && !m_Rules.AreEnemies(a, aggressor))
                    {
                        int turn = m_Session.WorldTime.TurnCounter;
                        ClearMessages();
                        AddMessage(new Message("You get a message from your army radio.", turn, Color.White));
                        AddMessage(new Message(String.Format("{0} is armed and dangerous. Shoot on sight!", aggressor.Name), turn, Color.White));
                        AddMessage(new Message(String.Format("Current location : {0}@{1},{2}", aggressor.Location.Map.Name, aggressor.Location.Position.X, aggressor.Location.Position.Y), turn, Color.White));
                        if (!a.IsBotPlayer)
                            AddMessagePressEnter();
                    }
                });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aggressor"></param>
        /// <param name="target"></param>
        /// <param name="fn">action to call on faction actor BEFORE making agressor an enemy.</param>
        void MakeEnemyOfTargetFactionInDistrict(Actor aggressor, Actor target, Action<Actor> fn)
        {
            Faction tFaction = target.Faction;
            foreach (Map m in target.Location.Map.District.Maps)
            {
                foreach (Actor a in m.Actors)
                {
                    if (a == aggressor || a == target)
                        continue;
                    if (a.Faction != tFaction)
                        continue;
                    if (a.Leader == aggressor)
                        continue;

                    // perform additional action on actor.
                    if (fn != null)
                        fn(a);

                    // aggression & self defence.
                    aggressor.MarkAsAgressorOf(a);
                    a.MarkAsSelfDefenceFrom(aggressor);
                }
            }
        }

#if false
        obsolete Actor.HasActorAsPersonalEnemy checks for leaders & followers.
        void MakeEnemyOfGroup(Actor a, IEnumerable<Actor> group)
        {
            if (group == null || a == null)
                return;

            foreach (Actor member in group)
            {
                a.MarkAsPersonalEnemy(member);
                member.MarkAsPersonalEnemy(a);
            }
        }

        void MakeEnemiesGroupsSub(IEnumerable<Actor> groupA, IEnumerable<Actor> groupB)
        {
            if (groupA == null || groupB == null)
                return;

            // O(n^2) beauty
            foreach (Actor a in groupA)
                foreach (Actor b in groupB)
                {
                    a.MarkAsPersonalEnemy(b);
                    b.MarkAsPersonalEnemy(a);
                }
        }
#endif

        public void DoMeleeAttack(Actor attacker, Actor defender)
        {
            // set activiy & target.
            attacker.Activity = Activity.FIGHTING;
            attacker.TargetActor = defender;

            // if not already enemies, attacker is aggressor.
            if (!m_Rules.AreEnemies(attacker, defender))
                DoMakeAggression(attacker, defender);

            // get attack & defence.
            Attack attack = m_Rules.ActorMeleeAttack(attacker, attacker.CurrentMeleeAttack, defender);
            Defence defence = m_Rules.ActorDefence(defender, defender.CurrentDefence);

            // spend APs & STA.
            SpendActorActionPoints(attacker, Rules.BASE_ACTION_COST);
            SpendActorStaminaPoints(attacker, Rules.STAMINA_COST_MELEE_ATTACK + attack.StaminaPenalty);

            // resolve attack.
            int hitRoll = m_Rules.RollSkill(attack.HitValue);
            int defRoll = m_Rules.RollSkill(defence.Value);

            // loud noise.
            OnLoudNoise(attacker.Location.Map, attacker.Location.Position, "Nearby fighting");

            // if defender is long waiting player, force stop.
            if (m_IsPlayerLongWait && defender.IsPlayer)
            {
                m_IsPlayerLongWaitForcedStop = true;
            }

            // show/hear.
            bool isDefVisible = IsVisibleToPlayer(defender);
            bool isAttVisible = IsVisibleToPlayer(attacker);
            bool isPlayer = attacker.IsPlayer || defender.IsPlayer;
            bool isBot = attacker.IsBotPlayer || defender.IsBotPlayer;  // alpha10.1 handle bot

            if (!isDefVisible && !isAttVisible && !isPlayer &&
                m_Rules.RollChance(PLAYER_HEAR_FIGHT_CHANCE))
            {
                AddMessageIfAudibleForPlayer(attacker.Location, MakePlayerCentricMessage("You hear fighting", attacker.Location.Position));
            }

            if (isAttVisible)
            {
                AddOverlay(new OverlayRect(Color.Yellow, new Rectangle(MapToScreen(attacker.Location.Position), new Size(TILE_SIZE, TILE_SIZE))));
                AddOverlay(new OverlayRect(Color.Red, new Rectangle(MapToScreen(defender.Location.Position), new Size(TILE_SIZE, TILE_SIZE))));
                AddOverlay(new OverlayImage(MapToScreen(attacker.Location.Position), GameImages.ICON_MELEE_ATTACK));
            }

            // Hit vs Missed
            if (hitRoll > defRoll)
            {
                // alpha10
                // roll for attacker disarming defender
                if (attacker.Model.Abilities.CanDisarm && m_Rules.RollChance(attack.DisarmChance))
                {
                    Item disarmIt = Disarm(defender);
                    if (disarmIt != null)
                    {
                        // show
                        if (isDefVisible)
                        {
                            if (isPlayer)
                                ClearMessages();
                            AddMessage(MakeMessage(attacker, Conjugate(attacker, VERB_DISARM), defender));
                            AddMessage(new Message(string.Format("{0} is sent flying!", disarmIt.TheName), attacker.Location.Map.LocalTime.TurnCounter));
                            if (isPlayer && !isBot)
                            {
                                AddMessagePressEnter();
                            }
                            else
                            {
                                RedrawPlayScreen();
                                AnimDelay(DELAY_SHORT);
                            }
                        }
                    }
                }

                // roll damage - double potential if def is sleeping.
                int dmgRoll = m_Rules.RollDamage(defender.IsSleeping ? attack.DamageValue * 2 : attack.DamageValue) - defence.Protection_Hit;
                // damage?
                if (dmgRoll > 0)
                {
                    // inflict dmg.
                    InflictDamage(defender, dmgRoll);

                    // regen HP/Rot and infection?
                    if (attacker.Model.Abilities.CanZombifyKilled && !defender.Model.Abilities.IsUndead)
                    {
                        RegenActorHitPoints(attacker, Rules.ActorBiteHpRegen(attacker, dmgRoll));
                        attacker.FoodPoints = Math.Min(attacker.FoodPoints + m_Rules.ActorBiteNutritionValue(attacker, dmgRoll), m_Rules.ActorMaxRot(attacker));
                        if (isAttVisible)
                        {
                            AddMessage(MakeMessage(attacker, Conjugate(attacker, VERB_FEAST_ON), defender, " flesh !"));
                        }
                        InfectActor(defender, Rules.InfectionForDamage(attacker, dmgRoll));
                    }

                    // Killed?
                    if (defender.HitPoints <= 0) // def killed!
                    {
                        // show.
                        if (isAttVisible || isDefVisible)
                        {
                            AddMessage(MakeMessage(attacker, Conjugate(attacker, defender.Model.Abilities.IsUndead ? VERB_DESTROY : m_Rules.IsMurder(attacker, defender) ? VERB_MURDER : VERB_KILL), defender, " !"));
                            AddOverlay(new OverlayImage(MapToScreen(defender.Location.Position), GameImages.ICON_KILLED));
                            RedrawPlayScreen();
                            AnimDelay(DELAY_LONG);
                        }

                        // kill.
                        KillActor(attacker, defender, "hit");

                        // cause insanity?
                        if (attacker.Model.Abilities.IsUndead && !defender.Model.Abilities.IsUndead)
                            SeeingCauseInsanity(attacker, attacker.Location, Rules.SANITY_HIT_EATEN_ALIVE, String.Format("{0} eaten alive", defender.Name));

                        // turn victim into zombie; always turn player into zombie NOW if killed by zombifier or if was infected.
                        if (Rules.HasImmediateZombification(m_Session.GameMode) || defender == m_Player)
                        {
                            if (attacker.Model.Abilities.CanZombifyKilled && !defender.Model.Abilities.IsUndead && m_Rules.RollChance(s_Options.ZombificationChance))
                            {
                                if (defender.IsPlayer)
                                {
                                    // remove player corpse.
                                    defender.Location.Map.TryRemoveCorpseOf(defender);
                                }
                                // add new zombie.
                                Zombify(attacker, defender, false);

                                // show
                                if (isDefVisible)
                                {
                                    AddMessage(MakeMessage(attacker, Conjugate(attacker, "turn"), defender, " into a Zombie!"));
                                    RedrawPlayScreen();
                                    AnimDelay(DELAY_LONG);
                                }
                            }
                            else if (defender == m_Player && !defender.Model.Abilities.IsUndead && defender.Infection > 0)
                            {
                                // remove player corpse.
                                defender.Location.Map.TryRemoveCorpseOf(defender);
                                // zombify player!
                                Zombify(null, defender, false);

                                // show
                                AddMessage(MakeMessage(defender, Conjugate(defender, "turn") + " into a Zombie!"));
                                RedrawPlayScreen();
                                AnimDelay(DELAY_LONG);
                            }
                        }
                    }
                    else
                    {
                        // show
                        if (isAttVisible || isDefVisible)
                        {
                            AddMessage(MakeMessage(attacker, Conjugate(attacker, attack.Verb), defender, String.Format(" for {0} damage.", dmgRoll)));
                            AddOverlay(new OverlayImage(MapToScreen(defender.Location.Position), GameImages.ICON_MELEE_DAMAGE));
                            AddOverlay(new OverlayText(MapToScreen(defender.Location.Position).Add(DAMAGE_DX, DAMAGE_DY), Color.White, dmgRoll.ToString(), Color.Black));
                            RedrawPlayScreen();
                            AnimDelay(isPlayer ? DELAY_NORMAL : DELAY_SHORT);
                        }
                    }
                }
                else
                {
                    if (isAttVisible || isDefVisible)
                    {
                        AddMessage(MakeMessage(attacker, Conjugate(attacker, attack.Verb), defender, " for no effect."));
                        AddOverlay(new OverlayImage(MapToScreen(defender.Location.Position), GameImages.ICON_MELEE_MISS));
                        RedrawPlayScreen();
                        AnimDelay(isPlayer ? DELAY_NORMAL : DELAY_SHORT);
                    }
                }

            }   // end of hit
            else // miss
            {
                // show
                if (isAttVisible || isDefVisible)
                {
                    AddMessage(MakeMessage(attacker, Conjugate(attacker, VERB_MISS), defender));
                    AddOverlay(new OverlayImage(MapToScreen(defender.Location.Position), GameImages.ICON_MELEE_MISS));
                    RedrawPlayScreen();
                    AnimDelay(isPlayer ? DELAY_NORMAL : DELAY_SHORT);
                }
            }

            // weapon break?
            ItemMeleeWeapon meleeWeapon = attacker.GetEquippedWeapon() as ItemMeleeWeapon;
            if (meleeWeapon != null && !(meleeWeapon.Model as ItemMeleeWeaponModel).IsUnbreakable)
            {
                if (m_Rules.RollChance(meleeWeapon.IsFragile ? Rules.MELEE_WEAPON_FRAGILE_BREAK_CHANCE : Rules.MELEE_WEAPON_BREAK_CHANCE))
                {
                    // do it.
                    // stackable weapons : only break ONE.
                    OnUnequipItem(attacker, meleeWeapon);
                    if (meleeWeapon.Quantity > 1)
                        --meleeWeapon.Quantity;
                    else
                        attacker.Inventory.RemoveAllQuantity(meleeWeapon);

                    // message.
                    if (isAttVisible)
                    {
                        AddMessage(MakeMessage(attacker, String.Format(": {0} breaks and is now useless!", meleeWeapon.TheName)));
                        RedrawPlayScreen();
                        AnimDelay(isPlayer ? DELAY_NORMAL : DELAY_SHORT);
                    }
                }
            }

            // alpha10 bug fix; clear overlays only if action is visible
            if (isAttVisible || isDefVisible)
                ClearOverlays();
        }

        public void DoRangedAttack(Actor attacker, Actor defender, List<Point> LoF, FireMode mode)
        {
            // if not enemies, aggression.
            if (!m_Rules.AreEnemies(attacker, defender))
                DoMakeAggression(attacker, defender);

            // resolve, depending on mode.
            switch (mode)
            {
                case FireMode.DEFAULT:
                    // spend AP.
                    SpendActorActionPoints(attacker, Rules.BASE_ACTION_COST);

                    // do attack.
                    DoSingleRangedAttack(attacker, defender, LoF, 0);
                    break;

                case FireMode.RAPID:
                    // spend AP.
                    SpendActorActionPoints(attacker, Rules.BASE_ACTION_COST);

                    // 1st attack
                    DoSingleRangedAttack(attacker, defender, LoF, 1);

                    // 2nd attack.
                    // special cases:
                    // - target was killed by 1st attack.
                    // - no more ammo.
                    ItemRangedWeapon w = attacker.GetEquippedWeapon() as ItemRangedWeapon;
                    if (defender.IsDead)
                    {
                        // spend 2nd shot ammo.
                        --w.Ammo;

                        // shoot at nothing.
                        Attack attack = attacker.CurrentRangedAttack;
                        AddMessage(MakeMessage(attacker, String.Format("{0} at nothing.", Conjugate(attacker, attack.Verb))));
                    }
                    else if (w.Ammo <= 0)
                    {
                        // fail silently.
                        return;
                    }
                    else
                    {
                        // perform attack normally.
                        DoSingleRangedAttack(attacker, defender, LoF, 2);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("unhandled mode");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="defender"></param>
        /// <param name="LoF"></param>
        /// <param name="shotCounter">0 for normal shot, 1 for 1st rapid fire shot, 2 for 2nd rapid fire shot</param>
        void DoSingleRangedAttack(Actor attacker, Actor defender, List<Point> LoF, int shotCounter)
        {
            // set activiy & target.
            attacker.Activity = Activity.FIGHTING;
            attacker.TargetActor = defender;

            // get attack & defence.
            int targetDistance = m_Rules.GridDistance(attacker.Location.Position, defender.Location.Position);
            Attack attack = m_Rules.ActorRangedAttack(attacker, attacker.CurrentRangedAttack, targetDistance, defender);
            Defence defence = m_Rules.ActorDefence(defender, defender.CurrentDefence);

            // spend STA.
            SpendActorStaminaPoints(attacker, attack.StaminaPenalty);

            // Firearms weapon jam?
            if (attack.Kind == AttackKind.FIREARM)
            {
                int jamChances = m_Rules.IsWeatherRain(m_Session.World.Weather) ? Rules.FIREARM_JAM_CHANCE_RAIN : Rules.FIREARM_JAM_CHANCE_NO_RAIN;
                if (m_Rules.RollChance(jamChances))
                {
                    if (IsVisibleToPlayer(attacker))
                    {
                        AddMessage(MakeMessage(attacker, " : weapon jam!"));
                        return;
                    }
                }
            }

            // spend ammo.
            ItemRangedWeapon weapon = attacker.GetEquippedWeapon() as ItemRangedWeapon;
            if (weapon == null)
                throw new InvalidOperationException("DoSingleRangedAttack but no equipped ranged weapon");
            --weapon.Ammo;

            // check we are firing through something and it intercepts the attack.
            if (DoCheckFireThrough(attacker, LoF))
            {
                return;
            }

            // if defender is long waiting player, force stop.
            if (m_IsPlayerLongWait && defender.IsPlayer)
            {
                m_IsPlayerLongWaitForcedStop = true;
            }

            // resolve attack.
            int hitValue = (shotCounter == 0 ? attack.HitValue : shotCounter == 1 ? attack.Hit2Value : attack.Hit3Value);
            int hitRoll = m_Rules.RollSkill(hitValue);
            int defRoll = m_Rules.RollSkill(defence.Value);

            // show/hear.
            bool isDefVisible = IsVisibleToPlayer(defender.Location);
            bool isAttVisible = IsVisibleToPlayer(attacker.Location);
            bool isPlayer = attacker.IsPlayer || defender.IsPlayer;

            if (!isDefVisible && !isAttVisible && !isPlayer &&
                m_Rules.RollChance(PLAYER_HEAR_FIGHT_CHANCE))
            {
                AddMessageIfAudibleForPlayer(attacker.Location, MakePlayerCentricMessage("You hear firing", attacker.Location.Position));
            }

            if (isAttVisible)
            {
                AddOverlay(new OverlayRect(Color.Yellow, new Rectangle(MapToScreen(attacker.Location.Position), new Size(TILE_SIZE, TILE_SIZE))));
                AddOverlay(new OverlayRect(Color.Red, new Rectangle(MapToScreen(defender.Location.Position), new Size(TILE_SIZE, TILE_SIZE))));
                AddOverlay(new OverlayImage(MapToScreen(attacker.Location.Position), GameImages.ICON_RANGED_ATTACK));
            }

            // Hit vs Missed
            #region
            if (hitRoll > defRoll)
            {
                // roll damage - double potential if def is sleeping.
                int dmgRoll = m_Rules.RollDamage(defender.IsSleeping ? attack.DamageValue * 2 : attack.DamageValue) - defence.Protection_Shot;
                if (dmgRoll > 0)
                {
                    // inflict dmg.
                    InflictDamage(defender, dmgRoll);

                    // Killed?
                    if (defender.HitPoints <= 0) // def killed!
                    {
                        // show.
                        if (isDefVisible)
                        {
                            AddMessage(MakeMessage(attacker, Conjugate(attacker, defender.Model.Abilities.IsUndead ? VERB_DESTROY : m_Rules.IsMurder(attacker, defender) ? VERB_MURDER : VERB_KILL), defender, " !"));
                            AddOverlay(new OverlayImage(MapToScreen(defender.Location.Position), GameImages.ICON_KILLED));
                            RedrawPlayScreen();
                            AnimDelay(DELAY_LONG);
                        }

                        // kill.
                        KillActor(attacker, defender, "shot");
                    }
                    else
                    {
                        // show
                        if (isDefVisible)
                        {
                            AddMessage(MakeMessage(attacker, Conjugate(attacker, attack.Verb), defender, String.Format(" for {0} damage.", dmgRoll)));
                            AddOverlay(new OverlayImage(MapToScreen(defender.Location.Position), GameImages.ICON_RANGED_DAMAGE));
                            AddOverlay(new OverlayText(MapToScreen(defender.Location.Position).Add(DAMAGE_DX, DAMAGE_DY), Color.White, dmgRoll.ToString(), Color.Black));
                            RedrawPlayScreen();
                            AnimDelay(isPlayer ? DELAY_NORMAL : DELAY_SHORT);
                        }
                    }
                }
                else
                {
                    if (isDefVisible)
                    {
                        AddMessage(MakeMessage(attacker, Conjugate(attacker, attack.Verb), defender, " for no effect."));
                        AddOverlay(new OverlayImage(MapToScreen(defender.Location.Position), GameImages.ICON_RANGED_MISS));
                        RedrawPlayScreen();
                        AnimDelay(isPlayer ? DELAY_NORMAL : DELAY_SHORT);
                    }
                }

            }   // end of hit
            else // miss
            {
                // show
                if (isDefVisible)
                {
                    AddMessage(MakeMessage(attacker, Conjugate(attacker, VERB_MISS), defender));
                    AddOverlay(new OverlayImage(MapToScreen(defender.Location.Position), GameImages.ICON_RANGED_MISS));
                    RedrawPlayScreen();
                    AnimDelay(isPlayer ? DELAY_NORMAL : DELAY_SHORT);
                }
            }
            #endregion

            // alpha10 bug fix; clear overlays only if action is visible
            if (isAttVisible || isDefVisible)
                ClearOverlays();
        }

        bool DoCheckFireThrough(Actor attacker, List<Point> LoF)
        {
            // check if we are firing through an object that blocks the LoF and breaks.
            foreach (Point pt in LoF)
            {
                MapObject mapObj = attacker.Location.Map.GetMapObjectAt(pt);
                if (mapObj == null)
                    continue;
                if (mapObj.BreaksWhenFiredThrough &&
                    mapObj.BreakState != MapObject.Break.BROKEN &&      // not if already broken.
                    !mapObj.IsWalkable)                                 // not if not blocking.
                {
                    // message.
                    bool isAttVisible = IsVisibleToPlayer(attacker);
                    bool isObjVisible = IsVisibleToPlayer(mapObj);
                    if (isAttVisible || isObjVisible)
                    {
                        if (isAttVisible)
                        {
                            AddOverlay(new OverlayRect(Color.Yellow, new Rectangle(MapToScreen(attacker.Location.Position), new Size(TILE_SIZE, TILE_SIZE))));
                            AddOverlay(new OverlayImage(MapToScreen(attacker.Location.Position), GameImages.ICON_RANGED_ATTACK));
                        }
                        if (isObjVisible)
                            AddOverlay(new OverlayRect(Color.Red, new Rectangle(MapToScreen(pt), new Size(TILE_SIZE, TILE_SIZE))));

                        AnimDelay(attacker.IsPlayer ? DELAY_NORMAL : DELAY_SHORT);
                    }

                    // destroy that object.
                    DoDestroyObject(mapObj);

                    // fire intercepted.
                    return true;
                }
            }

            // Line Of Fire completly clear, process normally.
            return false;
        }

        public void DoThrowGrenadeUnprimed(Actor actor, Point targetPos)
        {
            // get grenade.
            ItemGrenade grenade = actor.GetEquippedWeapon() as ItemGrenade;
            if (grenade == null)
                throw new InvalidOperationException("throwing grenade but no grenade equiped ");

            // spend AP.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // consume grenade.
            actor.Inventory.Consume(grenade);

            // drop primed grenade at target position.
            Map map = actor.Location.Map;
            ItemGrenadePrimed primedGrenade = new ItemGrenadePrimed(m_GameItems[grenade.PrimedModelID]);
            map.DropItemAt(primedGrenade, targetPos);

            // message about throwing.
            bool isVisible = IsVisibleToPlayer(actor) || IsVisibleToPlayer(actor.Location.Map, targetPos);
            if (isVisible)
            {
                AddOverlay(new OverlayRect(Color.Yellow, new Rectangle(MapToScreen(actor.Location.Position), new Size(TILE_SIZE, TILE_SIZE))));
                AddOverlay(new OverlayRect(Color.Red, new Rectangle(MapToScreen(targetPos), new Size(TILE_SIZE, TILE_SIZE))));
                AddMessage(MakeMessage(actor, String.Format("{0} a {1}!", Conjugate(actor, VERB_THROW), grenade.Model.SingleName)));
                RedrawPlayScreen();
                AnimDelay(DELAY_LONG);
                ClearOverlays();
                RedrawPlayScreen();
            }
        }

        public void DoThrowGrenadePrimed(Actor actor, Point targetPos)
        {
            // get grenade.
            ItemGrenadePrimed primedGrenade = actor.GetEquippedWeapon() as ItemGrenadePrimed;
            if (primedGrenade == null)
                throw new InvalidOperationException("throwing primed grenade but no primed grenade equiped ");

            // spend AP.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // remove grenade from inventory.
            actor.Inventory.RemoveAllQuantity(primedGrenade);

            // drop primed grenade at target position.
            actor.Location.Map.DropItemAt(primedGrenade, targetPos);

            // message about throwing.
            bool isVisible = IsVisibleToPlayer(actor) || IsVisibleToPlayer(actor.Location.Map, targetPos);
            if (isVisible)
            {
                AddOverlay(new OverlayRect(Color.Yellow, new Rectangle(MapToScreen(actor.Location.Position), new Size(TILE_SIZE, TILE_SIZE))));
                AddOverlay(new OverlayRect(Color.Red, new Rectangle(MapToScreen(targetPos), new Size(TILE_SIZE, TILE_SIZE))));
                AddMessage(MakeMessage(actor, String.Format("{0} back a {1}!", Conjugate(actor, VERB_THROW), primedGrenade.Model.SingleName)));
                RedrawPlayScreen();
                AnimDelay(DELAY_LONG);
                ClearOverlays();
                RedrawPlayScreen();
            }
        }

        void ShowBlastImage(Point screenPos, BlastAttack attack, int damage)
        {
            float alpha = 0.1f + (float)damage / (float)attack.Damage[0];
            if (alpha > 1) alpha = 1;
            AddOverlay(new OverlayTransparentImage(alpha, screenPos, GameImages.ICON_BLAST));
            AddOverlay(new OverlayText(screenPos, Color.Red, damage.ToString(), Color.Black));
        }

        void DoBlast(Location location, BlastAttack blastAttack)
        {
            // noise.
            OnLoudNoise(location.Map, location.Position, "A loud EXPLOSION");

            // blast icon vs audio.
            bool isVisible = IsVisibleToPlayer(location);
            if (isVisible)
            {
                ShowBlastImage(MapToScreen(location.Position), blastAttack, blastAttack.Damage[0]);
                RedrawPlayScreen();
                AnimDelay(DELAY_LONG);
                RedrawPlayScreen();
            }
            else if (m_Rules.RollChance(PLAYER_HEAR_EXPLOSION_CHANCE))
            {
                AddMessageIfAudibleForPlayer(location, MakePlayerCentricMessage("You hear an explosion", location.Position));
            }

            // ground zero explosion.
            ApplyExplosionDamage(location, 0, blastAttack);

            // explosion wave.
            for (int waveDistance = 1; waveDistance <= blastAttack.Radius; waveDistance++)
            {
                // do it.
                bool anyVisible = ApplyExplosionWave(location, waveDistance, blastAttack);

                // show.
                if (anyVisible)
                {
                    isVisible = true; // alpha10
                    RedrawPlayScreen();
                    AnimDelay(DELAY_NORMAL);
                }
            }

            // alpha10 bug fix; clear overlays only if action is visible
            if (isVisible)
                ClearOverlays();
        }

        bool ApplyExplosionWave(Location center, int waveDistance, BlastAttack blast)
        {
            bool anyVisible = false;
            Map map = center.Map;

            Point pt = new Point();
            int xmin = center.Position.X - waveDistance;
            int xmax = center.Position.X + waveDistance;
            int ymin = center.Position.Y - waveDistance;
            int ymax = center.Position.Y + waveDistance;

            // north.
            if (ymin >= 0)
            {
                pt.Y = ymin;
                for (int x = xmin; x <= xmax; x++)
                {
                    pt.X = x;
                    anyVisible |= ApplyExplosionWaveSub(center, pt, waveDistance, blast);
                }
            }

            // south.
            if (ymax < map.Height)
            {
                pt.Y = ymax;
                for (int x = xmin; x <= xmax; x++)
                {
                    pt.X = x;
                    anyVisible |= ApplyExplosionWaveSub(center, pt, waveDistance, blast);
                }
            }

            // west.
            // do dont west corners twice!
            // hence the ymin + 1 and < ymax checks.
            if (xmin >= 0)
            {
                pt.X = xmin;
                for (int y = ymin + 1; y < ymax; y++)
                {
                    pt.Y = y;
                    anyVisible |= ApplyExplosionWaveSub(center, pt, waveDistance, blast);
                }
            }

            // east.
            // don't do east corners twice!
            // hence the ymin + 1 and < ymax checks.
            if (xmax < map.Width)
            {
                pt.X = xmax;
                for (int y = ymin + 1; y < ymax; y++)
                {
                    pt.Y = y;
                    anyVisible |= ApplyExplosionWaveSub(center, pt, waveDistance, blast);
                }
            }

            // return if any explosion was visible.
            return anyVisible;
        }

        bool ApplyExplosionWaveSub(Location blastCenter, Point pt, int waveDistance, BlastAttack blast)
        {
            if (blastCenter.Map.IsInBounds(pt) &&
                LOS.CanTraceFireLine(blastCenter, pt, waveDistance, null))
            {
                // do damage.
                int damage = ApplyExplosionDamage(new Location(blastCenter.Map, pt), waveDistance, blast);

                // show if visible.
                if (IsVisibleToPlayer(blastCenter.Map, pt))
                {
                    ShowBlastImage(MapToScreen(pt), blast, damage);
                    return true;
                }
                else
                    return false;
            }

            return false;
        }

        int ApplyExplosionDamage(Location location, int distanceFromBlast, BlastAttack blast)
        {
            Map map = location.Map;

            int modifiedDamage = m_Rules.BlastDamage(distanceFromBlast, blast);

            // if no damage, don't bother.
            if (modifiedDamage <= 0)
                return 0;

            // damage actor / carried explosives chain reaction.
            #region
            Actor victim = map.GetActorAt(location.Position);
            if (victim != null)
            {
                // carried explosives chain reaction.
                #region
                Inventory carriedItems = victim.Inventory;
                ExplosionChainReaction(carriedItems, location);
                #endregion

                // damage.
                #region
                int dmgToVictim = modifiedDamage - (victim.CurrentDefence.Protection_Hit + victim.CurrentDefence.Protection_Shot) / 2;
                if (dmgToVictim > 0)
                {
                    // inflict.
                    InflictDamage(victim, dmgToVictim);

                    // message.
                    if (IsVisibleToPlayer(victim))
                    {
                        AddMessage(new Message(String.Format("{0} is hit for {1} damage!", victim.Name, dmgToVictim), map.LocalTime.TurnCounter, Color.Crimson));
                    }

                    // die? do not kill someone who is already dead, this could happen because of multiple explosions in a single turn.
                    if (victim.HitPoints <= 0 && !victim.IsDead)
                    {
                        // kill him.
                        KillActor(null, victim, String.Format("explosion {0} damage", dmgToVictim));

                        // message?
                        if (IsVisibleToPlayer(victim))
                        {
                            AddMessage(new Message(String.Format("{0} dies in the explosion!", victim.Name), map.LocalTime.TurnCounter, Color.Crimson));
                        }
                    }
                }
                else
                    AddMessage(new Message(String.Format("{0} is hit for no damage.", victim.Name), map.LocalTime.TurnCounter, Color.White));
                #endregion
            }
            #endregion

            // destroy items / ground explosives chain reaction.
            #region
            Inventory groundInv = map.GetItemsAt(location.Position);
            if (groundInv != null)
            {
                // ground explosives chain reaction.
                #region
                ExplosionChainReaction(groundInv, location);
                #endregion

                // pick items to destroy - don't destroy explosives ready to go, we need them for the chain reaction.
                #region
                // the more damage, the more chance.
                // never destroy uniques or unbreakables.
                int destroyChance = modifiedDamage;
                List<Item> destroyItems = new List<Item>(groundInv.CountItems);
                foreach (Item it in groundInv.Items)
                {
                    if (it.IsUnique || it.Model.IsUnbreakable)
                        continue;
                    if (it is ItemPrimedExplosive)
                    {
                        if ((it as ItemPrimedExplosive).FuseTimeLeft <= 0)
                            continue;
                    }
                    if (!m_Rules.RollChance(destroyChance))
                        continue;
                    destroyItems.Add(it);
                }

                // do it.
                foreach (Item it in destroyItems)
                    map.RemoveItemAt(it, location.Position);
                destroyItems = null;
                #endregion
            }
            #endregion

            // damage objects?
            #region
            if (blast.CanDamageObjects)
            {
                MapObject obj = map.GetMapObjectAt(location.Position);
                if (obj != null)
                {
                    DoorWindow door = obj as DoorWindow;
                    // damage only breakables or barricaded door/windows.
                    if (obj.IsBreakable || (door != null && door.IsBarricaded))
                    {
                        int damageToObject = modifiedDamage;

                        // barricaded doors absorb part of the damage.
                        if (door != null && door.IsBarricaded)
                        {
                            int barricadeDamage = Math.Min(door.BarricadePoints, damageToObject);
                            door.BarricadePoints -= barricadeDamage;
                            damageToObject -= barricadeDamage;
                        }

                        // then directly damage the object.
                        if (damageToObject >= 0)
                        {
                            obj.HitPoints -= damageToObject;
                            if (obj.HitPoints <= 0)
                                DoDestroyObject(obj);
                        }
                    }
                }
            }
            #endregion

            // damage corpses?
            #region
            List<Corpse> corpses = map.GetCorpsesAt(location.Position);
            if (corpses != null)
            {
                foreach (Corpse c in corpses)
                    InflictDamageToCorpse(c, modifiedDamage);
            }
            #endregion

            // destroy walls?
            if (blast.CanDestroyWalls)
            {
                throw new NotImplementedException("blast.destroyWalls");
            }

            // return damage done.
            return modifiedDamage;
        }

        void ExplosionChainReaction(Inventory inv, Location location)
        {
            if (inv == null || inv.IsEmpty)
                return;

            // set each explosive item ready to explode.
            List<ItemExplosive> removedExplosives = null;
            List<ItemPrimedExplosive> addedExplosives = null;
            foreach (Item it in inv.Items)
            {
                // explosive?
                ItemExplosive explosive = it as ItemExplosive;
                if (explosive == null)
                    continue;

                // if a primed explosive, just force fuse to zero.
                ItemPrimedExplosive primedExplosive = explosive as ItemPrimedExplosive;
                if (primedExplosive != null)
                {
                    primedExplosive.FuseTimeLeft = 0;
                    continue;
                }

                // unprimed explosive, prime it, force fuse to zero and drop it at location.
                if (removedExplosives == null)
                    removedExplosives = new List<ItemExplosive>();
                if (addedExplosives == null)
                    addedExplosives = new List<ItemPrimedExplosive>();

                removedExplosives.Add(explosive);
                // add as many primed explosives at explosive quantity (stackables explosives).
                for (int nbPrimedToDrop = 0; nbPrimedToDrop < it.Quantity; nbPrimedToDrop++)
                {
                    primedExplosive = new ItemPrimedExplosive(m_GameItems[explosive.PrimedModelID]);
                    primedExplosive.FuseTimeLeft = 0;
                    addedExplosives.Add(primedExplosive);
                }
            }

            // remove explosives from inventory.
            if (removedExplosives != null)
            {
                foreach (Item removeIt in removedExplosives)
                    inv.RemoveAllQuantity(removeIt);
            }

            // drop primed explosives.
            if (addedExplosives != null)
            {
                foreach (Item addIt in addedExplosives)
                    location.Map.DropItemAt(addIt, location.Position);
            }
        }
        #endregion

        #region Chatting, Trading, Saying and Shouting.
        public void DoChat(Actor speaker, Actor target)
        {
            // spend APs.
            SpendActorActionPoints(speaker, Rules.BASE_ACTION_COST);

            // message
            bool isSpeakerVisible = IsVisibleToPlayer(speaker);
            bool isTargetVisible = IsVisibleToPlayer(target);
            if (isSpeakerVisible || isTargetVisible)
                AddMessage(MakeMessage(speaker, Conjugate(speaker, VERB_CHAT_WITH), target));

            // trade?
            if (m_Rules.CanActorInitiateTradeWith(speaker, target))
            {
                DoTrade(speaker, target);
            }

            // alpha10 recover san after "normal" chat or fast trade
            if (speaker.Model.Abilities.HasSanity)
            {
                RegenActorSanity(speaker, Rules.SANITY_RECOVER_CHAT_OR_TRADE);
                if (IsVisibleToPlayer(speaker))
                    AddMessage(MakeMessage(speaker, string.Format("{0} better after chatting with", Conjugate(speaker, VERB_FEEL)), target));
            }

            if (target.Model.Abilities.HasSanity)
            {
                RegenActorSanity(target, Rules.SANITY_RECOVER_CHAT_OR_TRADE);
                if (IsVisibleToPlayer(target))
                    AddMessage(MakeMessage(target, string.Format("{0} better after chatting with", Conjugate(speaker, VERB_FEEL)), speaker));
            }
        }

        // alpha10 "fast" trade uses new trade mechanic of rating items and trades.
        // npcs will mostly only make mutually beneficial deals.
        // speaker and target are also somehow reversed from how they were in rs9(!?)
        // for the player should try to mimick most of trade results obtained by player negociating trade but not mandatory.
        public void DoTrade(Actor speaker, Actor target)
        {
            // clean up activities
            speaker.Activity = Activity.IDLE;
            target.Activity = Activity.IDLE;

            bool isVisible = IsVisibleToPlayer(speaker) || IsVisibleToPlayer(target);
            if (isVisible) AddMessage(MakeMessage(speaker, string.Format("wants to make a quick trade with {0}.", target.Name)));

            // the basic idea is to pick an item the speaker wants from target, 
            // and offer an item the speaker is willing to get rid of.
            BaseAI speakerAI = speaker.Controller as BaseAI;
            BaseAI targetAI = target.Controller as BaseAI;
            Item offered, asked;
            offered = asked = null;

            // target not willing to trade if is ordered not to
            if ((!targetAI.Directives.CanTrade) && (speaker != target.Leader))
            {
                if (isVisible) AddMessage(MakeMessage(target, "is not willing to trade."));
                return;
            }

            // if speaker is the player, make the npc the speaker so the npc is the one offering an item.
            // alpha10.1 but not for bot
            if (speaker.IsPlayer)
            {
                // swap speaker and target so npc is always speaker in fast trade
                Actor swap = target;
                target = speaker;
                speaker = swap;
                targetAI = null;  // now player
                speakerAI = speaker.Controller as BaseAI;
            }

            // local lambdas just because -_-

            // get an item the speaker would like from target inventory.
            Item pickAskedItem(out ItemRating rating)
            {
                // pick an item in target inventory the speaker wants, or any item if target has only junk.
                List<Item> wants = target.Inventory.Filter((it) =>
                {
                    ItemRating r = speakerAI.RateItem(this, it, false);
                    // wants anything but junk. 
                    // don't limit to things speaker needs because the target ai is more likely to value the same item
                    // as being needed for himself! also makes for more varied deals.
                    return r != ItemRating.JUNK;
                });
                if (wants.Count == 0)
                {
                    // no non-junk items, extend to all items...
                    wants.AddRange(target.Inventory.Items);
                }

                // pick one from the wanted list.
                Item wantIt = wants[m_Rules.Roll(0, wants.Count)];
                rating = speakerAI.RateItem(this, wantIt, false);
                return wantIt;
            };

            // can return null 
            // get an item the speaker is willing to exhange for the target item it wants.
            Item pickOfferedItem(Item askedItem, ItemRating askedItemRating)
            {
                List<Item> offerables;

                // if target is npc: 
                //   - offer any item that could pass a trade deal with this npc (read their ai mind)
                // if target is player: 
                //   - cannot use rate trade offer on the npc itself...
                //   - so offer only items we rate less than the one we want (player should negociate deal instead)
                //   - accepting equal item ratings lead to bad deals for the npc, offering a need for a need (eg: a rifle for bullets!)
                // in all offers, never offer the same item model as the one asked eg: a pistol for a pistol!
                if (target.IsPlayer)
                {
                    offerables = speaker.Inventory.Filter((it) =>
                    {
                        return it.Model != askedItem.Model && speakerAI.RateItem(this, it, true) < askedItemRating;
                    });
                }
                else
                {
                    offerables = speaker.Inventory.Filter((it) =>
                    {
                        if (it.Model == askedItem.Model)
                            return false;
                        // read target ai mind...
                        TradeRating tr = targetAI.RateTradeOffer(this, speaker, it, askedItem);
                        // accept "Maybe" items to be a bit more realistic in not always making perfect deals
                        // ("hey! the ai always accept ai trades! they are cheating!")
                        // and let charisma influence the final result.
                        return tr != TradeRating.REFUSE;
                    });
                }

                if (offerables.Count == 0)
                {
                    // all our items are more valuable than the one we want or only silly deals. no deal.
                    return null;
                }

                Item offerIt = offerables[m_Rules.Roll(0, offerables.Count)];
                return offerIt;
            }

            ItemRating askedRating;
            asked = pickAskedItem(out askedRating);
            offered = pickOfferedItem(asked, askedRating);

            // if no item pairs found, failed trade.
            // either the target has no interesting items for speaker,
            // or the speaker has items too valuable for a trade.
            if ((asked == null) || (offered == null))
            {
                if (asked == null)
                {
                    // speaker finds nothing interesting in target inventory
                    if (isVisible)
                        AddMessage(MakeMessage(speaker, "is not interested in any item of your items."));
                }
                else
                {
                    // speaker has no item to give away (should not happen if target is player)
                    if (isVisible)
                        AddMessage(MakeMessage(speaker, string.Format("would prefer to keep {0} items.", HisOrHer(speaker))));
                }
                if (target.IsPlayer)
                    // help confused players...
                    AddMessage(new Message("(maybe try negociating a deal instead)", m_Session.WorldTime.TurnCounter, Color.Yellow));
                return;
            }

            // propose.
            // if player, ask.
            // if target is ai, check for it.
            // alpha10.1 handle bot player

            bool acceptTrade;
            if (isVisible) AddMessage(MakeMessage(speaker, string.Format("{0} {1} for {2}.", Conjugate(speaker, VERB_OFFER), offered.AName, asked.AName)));
            if (target.IsPlayer && !target.IsBotPlayer)  // speaker always ai unless bot
            {
                // ask player.
                AddOverlay(new OverlayPopup(TRADE_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, Point.Empty));
                RedrawPlayScreen();
                acceptTrade = WaitYesOrNo();
                ClearOverlays();
                RedrawPlayScreen();
            }
            else
            {
                // ask target ai/bot
                BaseAI ai;
#if DEBUG
                ai = target.IsPlayer && target.IsBotPlayer ? m_botControl : targetAI;
#else
                ai = targetAI;
#endif

                TradeRating r = ai.RateTradeOffer(this, speaker, offered, asked);
                if (r == TradeRating.ACCEPT)
                    acceptTrade = true;
                else if (r == TradeRating.REFUSE)
                    acceptTrade = false;
                else
                {
                    // use charisma on "maybe" trades, similar to what we do for the player in the negociating command we the ai won't
                    // exploit the game by asking several times so its ok not to store the charisma roll -_-
                    // note that a duo of charismatic npcs could in theory trade back and forth ha!
                    if (m_Rules.RollChance(m_Rules.ActorCharismaticTradeChance(speaker)))
                    {
                        if (isVisible) DoEmote(target, "Okay you convinced me.");
                        acceptTrade = true;
                    }
                    else
                        acceptTrade = false;
                }
            }

            // so, deal or not?
            if (acceptTrade)
            {
                if (isVisible) AddMessage(MakeMessage(target, string.Format("{0}.", Conjugate(target, VERB_ACCEPT_THE_DEAL))));
                if (target.IsPlayer || speaker.IsPlayer)
                    RedrawPlayScreen();

                // do it
                SwapActorItems(speaker, offered, target, asked);
            }
            else
            {
                if (isVisible) AddMessage(MakeMessage(target, string.Format("{0}.", Conjugate(target, VERB_REFUSE_THE_DEAL))));
                if (target.IsPlayer || speaker.IsPlayer)
                    RedrawPlayScreen();
            }
        }

        /// <summary>
        /// Swap items after a succesful trade. Used in "fast" trades and player negociating trade.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="itA"></param>
        /// <param name="b"></param>
        /// <param name="itB"></param>
        void SwapActorItems(Actor a, Item itA, Actor b, Item itB)
        {
            if (itA.IsEquipped)
                DoUnequipItem(a, itA);
            if (itB.IsEquipped)
                DoUnequipItem(b, itB);

            a.Inventory.RemoveAllQuantity(itA);
            b.Inventory.RemoveAllQuantity(itB);

            a.Inventory.AddAll(itB);
            b.Inventory.AddAll(itA);
        }

        [Flags]
        public enum Sayflags
        {
            NONE = 0,
            /// <summary>
            /// If told to the player and visible will highlight pause the game.
            /// </summary>
            IS_IMPORTANT = (1 << 0),

            /// <summary>
            /// Does not cost action points (emote).
            /// </summary>
            IS_FREE_ACTION = (1 << 1),

            // alpha10
            /// <summary>
            /// A warning or menace, should be highlighted.
            /// </summary>
            IS_DANGER = (1 << 2)
        }

        public void DoSay(Actor speaker, Actor target, string text, Sayflags flags)
        {
            Color sayColor = ((flags & Sayflags.IS_DANGER) != 0) ? SAYOREMOTE_DANGER_COLOR : SAYOREMOTE_NORMAL_COLOR;

            // spend APS?
            if ((flags & Sayflags.IS_FREE_ACTION) == 0)
                SpendActorActionPoints(speaker, Rules.BASE_ACTION_COST);

            // message.
            if (IsVisibleToPlayer(speaker) || (IsVisibleToPlayer(target) && !(m_Player.IsSleeping && target == m_Player)))
            {
                bool isPlayer = target.IsPlayer;
                bool isBot = target.IsBotPlayer; // alpha10.1 handle bot
                bool isImportant = (flags & Sayflags.IS_IMPORTANT) != 0;
                if (isPlayer && isImportant)
                    ClearMessages();
                AddMessage(MakeMessage(speaker, String.Format("to {0} : ", target.TheName), sayColor));
                AddMessage(MakeMessage(speaker, String.Format("\"{0}\"", text), sayColor));
                if (isPlayer && isImportant && !isBot)
                {
                    AddOverlay(new OverlayRect(Color.Yellow, new Rectangle(MapToScreen(speaker.Location.Position), new Size(TILE_SIZE, TILE_SIZE))));
                    AddMessagePressEnter();
                    ClearOverlays();
                    RemoveLastMessage();
                    RedrawPlayScreen();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="speaker"></param>
        /// <param name="text">can be null</param>
        public void DoShout(Actor speaker, string text)
        {
            // spend APs.
            SpendActorActionPoints(speaker, Rules.BASE_ACTION_COST);

            // loud noise.
            OnLoudNoise(speaker.Location.Map, speaker.Location.Position, "A SHOUT");

            // message.
            if (IsVisibleToPlayer(speaker) || AreLinkedByPhone(speaker, m_Player))
            {
                // if player follower, alert!
                if (speaker.Leader == m_Player && !m_Player.IsBotPlayer)  // alpha10.1 handle bot
                {
                    ClearMessages();
                    AddOverlay(new OverlayRect(Color.Yellow, new Rectangle(MapToScreen(speaker.Location.Position), new Size(TILE_SIZE, TILE_SIZE))));
                    AddMessage(MakeMessage(speaker, String.Format("{0}!!", Conjugate(speaker, VERB_RAISE_ALARM))));
                    if (text != null)
                        DoEmote(speaker, text, true);
                    AddMessagePressEnter();
                    ClearOverlays();
                    RemoveLastMessage();
                }
                else
                {
                    if (text == null)
                        AddMessage(MakeMessage(speaker, String.Format("{0}!", Conjugate(speaker, VERB_SHOUT))));
                    else
                        DoEmote(speaker, String.Format("{0} \"{1}\"", Conjugate(speaker, VERB_SHOUT), text), true);
                }
            }
        }

        public void DoEmote(Actor actor, string text, bool isDanger = false)
        {
            if (IsVisibleToPlayer(actor))
                AddMessage(new Message(String.Format("{0} : {1}", actor.Name, text), actor.Location.Map.LocalTime.TurnCounter, isDanger ? SAYOREMOTE_DANGER_COLOR : SAYOREMOTE_NORMAL_COLOR));
        }
        #endregion

        #region Items
        public void DoTakeFromContainer(Actor actor, Point position)
        {
            Map map = actor.Location.Map;

            // get topmost item.
            Item it = map.GetItemsAt(position).TopItem;

            // take it.
            DoTakeItem(actor, position, it);
        }

        public void DoTakeItem(Actor actor, Point position, Item it)
        {
            Map map = actor.Location.Map;

            // spend APs.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // special case for traps
            if (it is ItemTrap)
            {
                ItemTrap trap = it as ItemTrap;
                // taking a trap desactivates it.
                trap.Desactivate(); // alpha10 // trap.IsActivated = false;
            }

            // add to inventory.
            int quantityAdded;
            int quantityBefore = it.Quantity;
            actor.Inventory.AddAsMuchAsPossible(it, out quantityAdded);
            // if added all, remove from map.
            if (quantityAdded == quantityBefore)
            {
                Inventory itemsThere = map.GetItemsAt(position);
                if (itemsThere != null && itemsThere.Contains(it))
                    map.RemoveItemAt(it, position);
            }

            // message
            if (IsVisibleToPlayer(actor) || IsVisibleToPlayer(new Location(map, position)))
            {
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_TAKE), it));
            }

            // automatically equip item if flags set & possible, and not already equipped something.
            if (!it.Model.DontAutoEquip && m_Rules.CanActorEquipItem(actor, it) && actor.GetEquippedItem(it.Model.EquipmentPart) == null)
                DoEquipItem(actor, it);
        }

        public void DoGiveItemTo(Actor actor, Actor target, Item gift)
        {
            // spend APs.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // if leader give to follower, improve trust.
            if (target.Leader == actor)
            {
                // interesting item?
                BaseAI ai = target.Controller as BaseAI;
                bool isInterestingItem = (ai != null && ai.IsInterestingItemToOwn(this, gift, BaseAI.ItemSource.ANOTHER_ACTOR));

                // emote.
                if (isInterestingItem)
                    DoSay(target, actor, "Thank you, I really needed that!", Sayflags.IS_FREE_ACTION);
                else
                    DoSay(target, actor, "Thanks I guess...", Sayflags.IS_FREE_ACTION);

                // update trust.
                ModifyActorTrustInLeader(target, isInterestingItem ? Rules.TRUST_GOOD_GIFT_INCREASE : Rules.TRUST_MISC_GIFT_INCREASE, true);
            }
            // if follower give to leader, decrease trust.
            else if (actor.Leader == target)
            {
                // emote.
                DoSay(target, actor, "Well, here it is...", Sayflags.IS_FREE_ACTION);

                // update trust.
                ModifyActorTrustInLeader(actor, Rules.TRUST_GIVE_ITEM_ORDER_PENALTY, true);
            }

            // transfer item : drop then take (solves problem of partial quantities transfer).
            DropItem(actor, gift);
            DoTakeItem(target, actor.Location.Position, gift);

            // message.
            if (IsVisibleToPlayer(actor) || IsVisibleToPlayer(target))
            {
                AddMessage(MakeMessage(actor, String.Format("{0} {1} to", Conjugate(actor, VERB_GIVE), gift.TheName), target));
            }

        }

        /// <summary>
        /// AP free
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="it"></param>
        public void DoEquipItem(Actor actor, Item it)
        {
            // unequip previous item first.
            Item previousItem = actor.GetEquippedItem(it.Model.EquipmentPart);
            if (previousItem != null)
            {
                DoUnequipItem(actor, previousItem);
            }

            // equip part.
            it.EquippedPart = it.Model.EquipmentPart;

            // update revelant datas.
            OnEquipItem(actor, it);

            // message
            if (IsVisibleToPlayer(actor))
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_EQUIP), it));
        }

        /// <summary>
        /// AP free
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="it"></param>
        public void DoUnequipItem(Actor actor, Item it, bool canMessage = true)
        {
            // unequip part.
            it.EquippedPart = DollPart.NONE;

            // update revelant datas.
            OnUnequipItem(actor, it);

            // message.
            if (canMessage && IsVisibleToPlayer(actor))
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_UNEQUIP), it));
        }

        void OnEquipItem(Actor actor, Item it)
        {
            #region Weapons
            if (it.Model is ItemWeaponModel)
            {
                if (it.Model is ItemMeleeWeaponModel)
                {
                    ItemMeleeWeaponModel meleeModel = it.Model as ItemMeleeWeaponModel;
                    actor.CurrentMeleeAttack = Attack.MeleeAttack(
                        meleeModel.Attack.Verb,
                        (meleeModel.Attack.HitValue + actor.Sheet.UnarmedAttack.HitValue),
                        (meleeModel.Attack.DamageValue + actor.Sheet.UnarmedAttack.DamageValue),
                        meleeModel.Attack.StaminaPenalty,
                        meleeModel.Attack.DisarmChance);
                }
                else if (it.Model is ItemRangedWeaponModel)
                {
                    ItemRangedWeaponModel rangedModel = it.Model as ItemRangedWeaponModel;
                    actor.CurrentRangedAttack = Attack.RangedAttack(
                        rangedModel.Attack.Kind, rangedModel.Attack.Verb,
                        rangedModel.Attack.HitValue, rangedModel.Attack.Hit2Value, rangedModel.Attack.Hit3Value,
                        rangedModel.Attack.DamageValue,
                        rangedModel.Attack.Range);
                }
            }
            #endregion
            #region Armors
            else if (it.Model is ItemBodyArmorModel)
            {
                ItemBodyArmorModel armorModel = it.Model as ItemBodyArmorModel;
                actor.CurrentDefence += armorModel.ToDefence();
            }
            #endregion
            #region Batteries
            else if (it.Model is ItemTrackerModel)
            {
                ItemTracker trIt = it as ItemTracker;
                --trIt.Batteries;
            }
            else if (it.Model is ItemLightModel)
            {
                ItemLight ltIt = it as ItemLight;
                --ltIt.Batteries;
            }
            #endregion
        }

        void OnUnequipItem(Actor actor, Item it)
        {
            if (it.Model is ItemWeaponModel)
            {
                if (it.Model is ItemMeleeWeaponModel)
                {
                    actor.CurrentMeleeAttack = actor.Sheet.UnarmedAttack;
                }
                else if (it.Model is ItemRangedWeaponModel)
                {
                    actor.CurrentRangedAttack = Attack.BLANK;
                }
            }
            else if (it.Model is ItemBodyArmorModel)
            {
                ItemBodyArmorModel armorModel = it.Model as ItemBodyArmorModel;
                actor.CurrentDefence -= armorModel.ToDefence();
            }
        }

        public void DoDropItem(Actor actor, Item it)
        {
            // spend APs.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // which item to drop (original or a clone)
            Item dropIt = it;
            // discard?
            bool discardMe = false;

            // special case for traps and discared items.
            if (it is ItemTrap)
            {
                ItemTrap trap = it as ItemTrap;

                // drop one at a time.
                ItemTrap clone = trap.Clone();
                //alpha10 clone.IsActivated = trap.IsActivated;
                if (trap.IsActivated) // alpha10
                    clone.Activate(actor);
                dropIt = clone;

                // trap activates when dropped?
                if (clone.TrapModel.ActivatesWhenDropped)
                    clone.Activate(actor); // alpha10 //clone.IsActivated = true;

                // make sure source stack is desactivated (activate only activate the stack top item).
                trap.Desactivate();  // alpha10  //trap.IsActivated = false;
            }
            else
            {
                // drop or discard.
                if (it is ItemTracker)
                {
                    discardMe = (it as ItemTracker).Batteries <= 0;
                }
                else if (it is ItemLight)
                {
                    discardMe = (it as ItemLight).Batteries <= 0;
                }
                else if (it is ItemSprayPaint)
                {
                    discardMe = (it as ItemSprayPaint).PaintQuantity <= 0;
                }
                else if (it is ItemSprayScent)
                {
                    discardMe = (it as ItemSprayScent).SprayQuantity <= 0;
                }
            }

            if (discardMe)
            {
                DiscardItem(actor, it);
                // message
                if (IsVisibleToPlayer(actor))
                    AddMessage(MakeMessage(actor, Conjugate(actor, VERB_DISCARD), it));
            }
            else
            {
                if (dropIt == it)
                    DropItem(actor, it);
                else
                    DropCloneItem(actor, it, dropIt);
                // message
                if (IsVisibleToPlayer(actor))
                    AddMessage(MakeMessage(actor, Conjugate(actor, VERB_DROP), dropIt));
            }
        }

        void DiscardItem(Actor actor, Item it)
        {
            // remove from inventory.
            actor.Inventory.RemoveAllQuantity(it);

            // make sure it is unequipped.
            it.EquippedPart = DollPart.NONE;
        }

        void DropItem(Actor actor, Item it)
        {
            // remove from inventory.
            actor.Inventory.RemoveAllQuantity(it);

            // add to ground.
            actor.Location.Map.DropItemAt(it, actor.Location.Position);

            // make sure it is unequipped.
            it.EquippedPart = DollPart.NONE;
        }

        void DropCloneItem(Actor actor, Item it, Item clone)
        {
            // remove one quantity from inventory.
            if (--it.Quantity <= 0)
                actor.Inventory.RemoveAllQuantity(it);

            // add to ground.
            actor.Location.Map.DropItemAt(clone, actor.Location.Position);

            // make sure it is unequipped.
            clone.EquippedPart = DollPart.NONE;
        }

        public void DoUseItem(Actor actor, Item it)
        {
            // alpha10 defrag ai inventories
            bool defragInventory = !actor.IsPlayer && it.Model.IsStackable;

            // concrete use.
            if (it is ItemFood)
                DoUseFoodItem(actor, it as ItemFood);
            else if (it is ItemMedicine)
                DoUseMedicineItem(actor, it as ItemMedicine);
            else if (it is ItemAmmo)
                DoUseAmmoItem(actor, it as ItemAmmo);
            //else if (it is ItemSprayScent)  // alpha10 new way to use spray scent
            //    DoUseSprayScentItem(actor, it as ItemSprayScent);
            else if (it is ItemTrap)
                DoUseTrapItem(actor, it as ItemTrap);
            else if (it is ItemEntertainment)
                DoUseEntertainmentItem(actor, it as ItemEntertainment);

            // alpha10 defrag ai inventories
            if (defragInventory)
                actor.Inventory.Defrag();
        }

        public void DoEatFoodFromGround(Actor actor, Item it)
        {
            ItemFood food = it as ItemFood;

            // spend APs.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // recover food points.
            int baseNutrition = m_Rules.FoodItemNutrition(food, actor.Location.Map.LocalTime.TurnCounter);
            actor.FoodPoints = Math.Min(actor.FoodPoints + m_Rules.ActorItemNutritionValue(actor, baseNutrition), m_Rules.ActorMaxFood(actor));

            // consume it.
            Inventory inv = actor.Location.Map.GetItemsAt(actor.Location.Position);
            inv.Consume(food);

            // message.
            bool isVisible = IsVisibleToPlayer(actor);
            if (isVisible)
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_EAT), food));

            // vomit?
            if (m_Rules.IsFoodSpoiled(food, actor.Location.Map.LocalTime.TurnCounter))
            {
                if (m_Rules.RollChance(Rules.FOOD_EXPIRED_VOMIT_CHANCE))
                {
                    DoVomit(actor);

                    // message.
                    if (isVisible)
                    {
                        AddMessage(MakeMessage(actor, String.Format("{0} from eating spoiled food!", Conjugate(actor, VERB_VOMIT))));
                    }
                }
            }
        }

        void DoUseFoodItem(Actor actor, ItemFood food)
        {
            //////////////////////////////////////
            // If player, prevent wasteful usage.
            //////////////////////////////////////
            if (actor == m_Player && actor.FoodPoints >= m_Rules.ActorMaxFood(actor) - 1)
            {
                AddMessage(MakeErrorMessage("Don't waste food!"));
                return;
            }

            // spend APs.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // recover food points.
            int baseNutrition = m_Rules.FoodItemNutrition(food, actor.Location.Map.LocalTime.TurnCounter);
            actor.FoodPoints = Math.Min(actor.FoodPoints + m_Rules.ActorItemNutritionValue(actor, baseNutrition), m_Rules.ActorMaxFood(actor));

            // consume it.
            actor.Inventory.Consume(food);

            // canned food drops empty cans.
            if (food.Model == GameItems.CANNED_FOOD)
            {
                ItemTrap emptyCan = new ItemTrap(GameItems.EMPTY_CAN);// alpha10 { IsActivated = true };
                emptyCan.Activate(actor);  // alpha10
                actor.Location.Map.DropItemAt(emptyCan, actor.Location.Position);
            }

            // message.
            bool isVisible = IsVisibleToPlayer(actor);
            if (isVisible)
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_EAT), food));

            // vomit?
            if (m_Rules.IsFoodSpoiled(food, actor.Location.Map.LocalTime.TurnCounter))
            {
                if (m_Rules.RollChance(Rules.FOOD_EXPIRED_VOMIT_CHANCE))
                {
                    DoVomit(actor);

                    // message.
                    if (isVisible)
                    {
                        AddMessage(MakeMessage(actor, String.Format("{0} from eating spoiled food!", Conjugate(actor, VERB_VOMIT))));
                    }
                }
            }
        }

        void DoVomit(Actor actor)
        {
            // beuargh.
            actor.StaminaPoints -= Rules.FOOD_VOMIT_STA_COST;
            actor.SleepPoints = Math.Max(0, actor.SleepPoints - WorldTime.TURNS_PER_HOUR);
            actor.FoodPoints = Math.Max(0, actor.FoodPoints - WorldTime.TURNS_PER_HOUR);

            // drop vomit ^^.
            Location loc = actor.Location;
            Map map = loc.Map;
            map.GetTileAt(loc.Position.X, loc.Position.Y).AddDecoration(GameImages.DECO_VOMIT);
        }

        void DoUseMedicineItem(Actor actor, ItemMedicine med)
        {
            //////////////////////////////////////
            // If player, prevent wasteful usage.
            //////////////////////////////////////
            if (actor == m_Player)
            {
                int HPneed = m_Rules.ActorMaxHPs(actor) - actor.HitPoints;
                int STAneed = m_Rules.ActorMaxSTA(actor) - actor.StaminaPoints;
                int SLPneed = m_Rules.ActorMaxSleep(actor) - 2 - actor.SleepPoints;
                int CureNeed = actor.Infection;
                int SanNeed = m_Rules.ActorMaxSanity(actor) - actor.Sanity;

                bool HPwaste = HPneed <= 0 || med.Healing <= 0;
                bool STAwaste = STAneed <= 0 || med.StaminaBoost <= 0;
                bool SLPwaste = SLPneed <= 0 || med.SleepBoost <= 0;
                bool CureWaste = CureNeed <= 0 || med.InfectionCure <= 0;
                bool SanWaste = SanNeed <= 0 || med.SanityCure <= 0;

                if (HPwaste && STAwaste && SLPwaste && CureWaste && SanWaste)
                {
                    AddMessage(MakeErrorMessage("Don't waste medicine!"));
                    return;
                }
            }

            // spend APs.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // recover HPs, STA, SLP, INF, SAN.
            actor.HitPoints = Math.Min(actor.HitPoints + m_Rules.ActorMedicineEffect(actor, med.Healing), m_Rules.ActorMaxHPs(actor));
            actor.StaminaPoints = Math.Min(actor.StaminaPoints + m_Rules.ActorMedicineEffect(actor, med.StaminaBoost), m_Rules.ActorMaxSTA(actor));
            actor.SleepPoints = Math.Min(actor.SleepPoints + m_Rules.ActorMedicineEffect(actor, med.SleepBoost), m_Rules.ActorMaxSleep(actor));
            actor.Infection = Math.Max(0, actor.Infection - m_Rules.ActorMedicineEffect(actor, med.InfectionCure));
            actor.Sanity = Math.Min(actor.Sanity + m_Rules.ActorMedicineEffect(actor, med.SanityCure), m_Rules.ActorMaxSanity(actor));

            // consume it.
            actor.Inventory.Consume(med);

            // message.
            if (IsVisibleToPlayer(actor))
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_HEAL_WITH), med));
        }

        void DoUseAmmoItem(Actor actor, ItemAmmo ammoItem)
        {
            // spend APs.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // get weapon.
            ItemRangedWeapon ranged = actor.GetEquippedWeapon() as ItemRangedWeapon;
            ItemRangedWeaponModel model = ranged.Model as ItemRangedWeaponModel;

            // compute ammo spent.
            int ammoSpent = Math.Min(model.MaxAmmo - ranged.Ammo, ammoItem.Quantity);

            // reload.
            ranged.Ammo += ammoSpent;

            // spend ammo clip.
            ammoItem.Quantity -= ammoSpent;

            // if no ammo left, remove item.
            if (ammoItem.Quantity <= 0)
                actor.Inventory.RemoveAllQuantity(ammoItem);

            // message.
            if (IsVisibleToPlayer(actor))
            {
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_RELOAD), ranged));
            }
        }

        // alpha10 obsolete
#if false
        void DoUseSprayScentItem(Actor actor, ItemSprayScent spray)
        {
            // spend APs.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // consume spray.
            --spray.SprayQuantity;

            // add odor.
            Map map = actor.Location.Map;
            ItemSprayScentModel model = spray.Model as ItemSprayScentModel;
            map.ModifyScentAt(model.Odor, model.Strength, actor.Location.Position);

            // message.
            if (IsVisibleToPlayer(actor))
            {
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_SPRAY), spray));
            }
        }
#endif

        void DoUseTrapItem(Actor actor, ItemTrap trap)
        {
            // spend APs.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // toggle activation.
            // alpha10 //trap.IsActivated = !trap.IsActivated;
            if (trap.IsActivated)
                trap.Desactivate();
            else
                trap.Activate(actor);

            // message.
            if (IsVisibleToPlayer(actor))
                AddMessage(MakeMessage(actor, Conjugate(actor, (trap.IsActivated ? VERB_ACTIVATE : VERB_DESACTIVATE)), trap));
        }

        void DoUseEntertainmentItem(Actor actor, ItemEntertainment ent)
        {
            bool visible = IsVisibleToPlayer(actor);

            // spend APs.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // recover san.
            RegenActorSanity(actor, ent.EntertainmentModel.Value);

            // message.
            if (visible)
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_ENJOY), ent));

            // check boring chance.
            // 100% means discard it.
            int boreChance = ent.EntertainmentModel.BoreChance;
            bool bored = false;
            bool discarded = false;
            if (boreChance == 100)
            {
                actor.Inventory.Consume(ent);
                discarded = true;
            }
            else if (boreChance > 0)
            {
                if (m_Rules.RollChance(boreChance))
                    bored = true;
            }
            if (bored)
                ent.AddBoringFor(actor); // alpha10 boring items item centric

            // message.
            if (visible)
            {
                if (bored)
                    AddMessage(MakeMessage(actor, String.Format("{0} now bored of {1}.", Conjugate(actor, VERB_BE), ent.TheName)));
                if (discarded)
                    AddMessage(MakeMessage(actor, Conjugate(actor, VERB_DISCARD), ent));
            }
        }

        public void DoRechargeItemBattery(Actor actor, Item it)
        {
            // spend APs.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // recharge.
            if (it is ItemLight)
            {
                ItemLight light = it as ItemLight;
                light.Batteries += WorldTime.TURNS_PER_HOUR;
            }
            else if (it is ItemTracker)
            {
                ItemTracker track = it as ItemTracker;
                track.Batteries += WorldTime.TURNS_PER_HOUR;
            }

            // message.
            if (IsVisibleToPlayer(actor))
            {
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_RECHARGE), it, " batteries."));
            }

        }
        #endregion

        #region Doors
        public void DoOpenDoor(Actor actor, DoorWindow door)
        {
            // Do it.
            door.SetState(DoorWindow.STATE_OPEN);

            // Message.
            if (IsVisibleToPlayer(actor) || IsVisibleToPlayer(door))
            {
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_OPEN), door));
                RedrawPlayScreen();
            }

            // Spend APs.
            int openCost = Rules.BASE_ACTION_COST;
            SpendActorActionPoints(actor, openCost);
        }

        public void DoCloseDoor(Actor actor, DoorWindow door)
        {
            // Do it.
            door.SetState(DoorWindow.STATE_CLOSED);

            // Message.
            if (IsVisibleToPlayer(actor) || IsVisibleToPlayer(door))
            {
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_CLOSE), door));
                RedrawPlayScreen();
            }

            // Spend APs.
            int closeCost = Rules.BASE_ACTION_COST;
            SpendActorActionPoints(actor, closeCost);
        }

        public void DoBarricadeDoor(Actor actor, DoorWindow door)
        {
            // get barricading item.
            ItemBarricadeMaterial it = actor.Inventory.GetSmallestStackByType(typeof(ItemBarricadeMaterial)) as ItemBarricadeMaterial; // alpha10
            ItemBarricadeMaterialModel m = it.Model as ItemBarricadeMaterialModel;

            // do it.
            actor.Inventory.Consume(it);
            door.BarricadePoints = Math.Min(door.BarricadePoints + m_Rules.ActorBarricadingPoints(actor, m.BarricadingValue), Rules.BARRICADING_MAX);

            // message.
            bool isVisible = IsVisibleToPlayer(actor) || IsVisibleToPlayer(door);
            if (isVisible)
            {
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_BARRICADE), door));
            }

            // spend AP.
            int barricadingCost = Rules.BASE_ACTION_COST;
            SpendActorActionPoints(actor, barricadingCost);
        }
        #endregion

        #region Building & Repairing
        public void DoBuildFortification(Actor actor, Point buildPos, bool isLarge)
        {
            // spend AP.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // consume material.
            int need = m_Rules.ActorBarricadingMaterialNeedForFortification(actor, isLarge);
            for (int i = 0; i < need; i++)
            {
                Item it = actor.Inventory.GetSmallestStackByType(typeof(ItemBarricadeMaterial)); // alpha10
                                                                                                 //actor.Inventory.GetFirstByType(typeof(ItemBarricadeMaterial));
                actor.Inventory.Consume(it);
            }

            // add object.
            Fortification fortObj = isLarge ? m_TownGenerator.MakeObjLargeFortification(GameImages.OBJ_LARGE_WOODEN_FORTIFICATION) : m_TownGenerator.MakeObjSmallFortification(GameImages.OBJ_SMALL_WOODEN_FORTIFICATION);
            actor.Location.Map.PlaceMapObjectAt(fortObj, buildPos);

            // message.
            if (IsVisibleToPlayer(actor) || IsVisibleToPlayer(new Location(actor.Location.Map, buildPos)))
            {
                AddMessage(MakeMessage(actor, String.Format("{0} a {1} fortification.", Conjugate(actor, VERB_BUILD), isLarge ? "large" : "small")));
            }

            // check traps.
            CheckMapObjectTriggersTraps(actor.Location.Map, buildPos);
        }

        public void DoRepairFortification(Actor actor, Fortification fort)
        {
            // spend AP.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // spend material.
            ItemBarricadeMaterial material = actor.Inventory.GetSmallestStackByType(typeof(ItemBarricadeMaterial)) as ItemBarricadeMaterial; // alpha10
                                                                                                                                             //actor.Inventory.GetFirstByType(typeof(ItemBarricadeMaterial)) as ItemBarricadeMaterial;
            if (material == null)
                throw new InvalidOperationException("no material");
            actor.Inventory.Consume(material);

            // repair HP.
            fort.HitPoints = Math.Min(fort.MaxHitPoints,
                fort.HitPoints + m_Rules.ActorBarricadingPoints(actor, (material.Model as ItemBarricadeMaterialModel).BarricadingValue));

            // message.
            if (IsVisibleToPlayer(actor) || IsVisibleToPlayer(fort))
            {
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_REPAIR), fort));
            }
        }
        #endregion

        #region Switching map object
        public void DoSwitchPowerGenerator(Actor actor, PowerGenerator powGen)
        {
            // spend AP.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // switch it.
            powGen.TogglePower();

            // message.
            if (IsVisibleToPlayer(actor) || IsVisibleToPlayer(powGen))
            {
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_SWITCH), powGen, powGen.IsOn ? " on." : " off."));
            }

            // check for special effects.
            OnMapPowerGeneratorSwitch(actor.Location, powGen);

            // done.
        }
        #endregion

        #region Breaking stuff
        void DoDestroyObject(MapObject mapObj)
        {
            DoorWindow door = mapObj as DoorWindow;
            bool isWindow = (door != null && door.IsWindow);

            // force HP to zero.
            mapObj.HitPoints = 0;

            // drop plank and improvised weapons?
            if (mapObj.GivesWood)
            {
                // drop planks.
                int nbPlanks = 1 + mapObj.MaxHitPoints / DoorWindow.BASE_HITPOINTS;
                while (nbPlanks > 0)
                {
                    Item planks = new ItemBarricadeMaterial(m_GameItems.WOODENPLANK)
                    {
                        Quantity = Math.Min(m_GameItems.WOODENPLANK.StackingLimit, nbPlanks)
                    };
                    if (planks.Quantity < 1) planks.Quantity = 1;
                    mapObj.Location.Map.DropItemAt(planks, mapObj.Location.Position);
                    nbPlanks -= planks.Quantity;
                }

                // drop improvised weapons?
                if (m_Rules.RollChance(Rules.IMPROVED_WEAPONS_FROM_BROKEN_WOOD_CHANCE))
                {
                    // improvised club, improvised spear.
                    ItemMeleeWeapon impWpn;
                    if (m_Rules.RollChance(50))
                        impWpn = new ItemMeleeWeapon(m_GameItems.IMPROVISED_CLUB);
                    else
                        impWpn = new ItemMeleeWeapon(m_GameItems.IMPROVISED_SPEAR);

                    // drop it.
                    mapObj.Location.Map.DropItemAt(impWpn, mapObj.Location.Position);
                }
            }

            // remove object - but not windows.
            if (isWindow)
            {
                door.SetState(DoorWindow.STATE_BROKEN);
            }
            else
                mapObj.Location.Map.RemoveMapObjectAt(mapObj.Location.Position.X, mapObj.Location.Position.Y);

            // loud noise.
            OnLoudNoise(mapObj.Location.Map, mapObj.Location.Position, "A loud *CRASH*");
        }

        public void DoBreak(Actor actor, MapObject mapObj)
        {
            Attack bashAttack = m_Rules.ActorMeleeAttack(actor, actor.CurrentMeleeAttack, null, mapObj);


            #region Attacking a barricaded door.
            DoorWindow door = mapObj as DoorWindow;
            if (door != null && door.IsBarricaded)
            {
                // Spend APs & STA.
                int bashCost = Rules.BASE_ACTION_COST;
                SpendActorActionPoints(actor, bashCost);
                SpendActorStaminaPoints(actor, Rules.STAMINA_COST_MELEE_ATTACK);

                // Bash.
                door.BarricadePoints -= bashAttack.DamageValue;

                // loud noise.
                OnLoudNoise(door.Location.Map, door.Location.Position, "A loud *BASH*");

                // message.
                if (IsVisibleToPlayer(actor) || IsVisibleToPlayer(door))
                {
                    if (IsVisibleToPlayer(door))
                    {
                        // alpha10 tell & show damage
                        Point screenPos = MapToScreen(mapObj.Location.Position);
                        AddOverlay(new OverlayImage(screenPos, GameImages.ICON_MELEE_DAMAGE));
                        AddOverlay(new OverlayText(screenPos.Add(DAMAGE_DX, DAMAGE_DY), Color.White, bashAttack.DamageValue.ToString(), Color.Black)); // alpha10
                        AddMessage(MakeMessage(actor, string.Format("{0} the barricade for {1} damage.", Conjugate(actor, VERB_BASH), bashAttack.DamageValue))); // alpha10
                        RedrawPlayScreen();
                        AnimDelay(actor.IsPlayer ? DELAY_NORMAL : DELAY_SHORT);
                        ClearOverlays();
                    }
                    else
                    {
                        AddMessage(MakeMessage(actor, string.Format("{0} the barricade.", Conjugate(actor, VERB_BASH)))); // alpha10
                    }
                }
                else
                {
                    if (m_Rules.RollChance(PLAYER_HEAR_BASH_CHANCE))
                        AddMessageIfAudibleForPlayer(door.Location, MakePlayerCentricMessage("You hear someone bashing barricades", door.Location.Position));

                }


                // done.
                return;
            }
            #endregion

            #region Attacking a un-barricaded door or a normal object
            else
            {
                // Always hit.
                mapObj.HitPoints -= bashAttack.DamageValue;

                // Spend APs & STA.
                int bashCost = Rules.BASE_ACTION_COST;
                SpendActorActionPoints(actor, bashCost);
                SpendActorStaminaPoints(actor, Rules.STAMINA_COST_MELEE_ATTACK);

                // Broken?
                bool isBroken = false;
                if (mapObj.HitPoints <= 0)
                {
                    // breaks.
                    DoDestroyObject(mapObj);
                    isBroken = true;
                }

                // loud noise.
                OnLoudNoise(mapObj.Location.Map, mapObj.Location.Position, "A loud *CRASH*");

                // Message.
                bool isActorVisible = IsVisibleToPlayer(actor);
                bool isDoorVisible = IsVisibleToPlayer(mapObj);
                bool isPlayer = actor.IsPlayer;

                if (isActorVisible || isDoorVisible)
                {
                    if (isActorVisible)
                        AddOverlay(new OverlayRect(Color.Yellow, new Rectangle(MapToScreen(actor.Location.Position), new Size(TILE_SIZE, TILE_SIZE))));
                    if (isDoorVisible)
                        AddOverlay(new OverlayRect(Color.Red, new Rectangle(MapToScreen(mapObj.Location.Position), new Size(TILE_SIZE, TILE_SIZE))));

                    if (isBroken)
                    {
                        AddMessage(MakeMessage(actor, Conjugate(actor, VERB_BREAK), mapObj));
                        if (isActorVisible)
                            AddOverlay(new OverlayImage(MapToScreen(actor.Location.Position), GameImages.ICON_MELEE_ATTACK));
                        if (isDoorVisible)
                            AddOverlay(new OverlayImage(MapToScreen(mapObj.Location.Position), GameImages.ICON_KILLED));
                        RedrawPlayScreen();
                        AnimDelay(DELAY_LONG);
                    }
                    else
                    {
                        if (isDoorVisible)
                        {
                            AddMessage(MakeMessage(actor, string.Format("{0} {1} for {2} damage.", Conjugate(actor, VERB_BASH), mapObj.TheName, bashAttack.DamageValue))); // alpha10
                            AddOverlay(new OverlayImage(MapToScreen(mapObj.Location.Position), GameImages.ICON_MELEE_DAMAGE));
                            AddOverlay(new OverlayText(MapToScreen(mapObj.Location.Position).Add(DAMAGE_DX, DAMAGE_DY), Color.White, bashAttack.DamageValue.ToString(), Color.Black)); // alpha10
                        }
                        else if (isActorVisible)
                        {
                            AddMessage(MakeMessage(actor, string.Format("{0} {1}.", Conjugate(actor, VERB_BASH), mapObj.TheName))); // alpha10
                        }

                        if (isActorVisible)
                            AddOverlay(new OverlayImage(MapToScreen(actor.Location.Position), GameImages.ICON_MELEE_ATTACK));

                        RedrawPlayScreen();
                        AnimDelay(isPlayer ? DELAY_NORMAL : DELAY_SHORT);
                    }

                    // alpha10 bug fix; clear overlays only if action is visible
                    ClearOverlays(); // was in the wrong place!
                }  // any is visible
                else
                {
                    if (isBroken)
                    {
                        if (m_Rules.RollChance(PLAYER_HEAR_BREAK_CHANCE))
                            AddMessageIfAudibleForPlayer(mapObj.Location, MakePlayerCentricMessage("You hear someone breaking furniture", mapObj.Location.Position));
                    }
                    else
                    {
                        if (m_Rules.RollChance(PLAYER_HEAR_BASH_CHANCE))
                            AddMessageIfAudibleForPlayer(mapObj.Location, MakePlayerCentricMessage("You hear someone bashing furniture", mapObj.Location.Position));
                    }
                }
            }
            #endregion
        }
        #endregion

        #region Pushing/Pulling & Shoving
        // alpha10
        void DoPushPullFollowersHelp(Actor actor, MapObject mapObj, bool isPulling, ref int staCost)
        {
            bool isVisibleMobj = IsVisibleToPlayer(mapObj);

            Location objLoc = new Location(actor.Location.Map, mapObj.Location.Position);
            List<Actor> helpers = null;
            foreach (Actor fo in actor.Followers)
            {
                // follower can help if: not sleeping, idle and adj to map object.
                if (!fo.IsSleeping && (fo.Activity == Activity.IDLE || fo.Activity == Activity.FOLLOWING) && m_Rules.IsAdjacent(fo.Location, mapObj.Location))
                {
                    if (helpers == null) helpers = new List<Actor>(actor.CountFollowers);
                    helpers.Add(fo);
                }
            }
            if (helpers != null)
            {
                // share the sta cost.
                staCost = mapObj.Weight / (1 + helpers.Count);
                foreach (Actor h in helpers)
                {
                    // spend fo AP & STA.
                    SpendActorActionPoints(h, Rules.BASE_ACTION_COST);
                    SpendActorStaminaPoints(h, staCost);
                    // message.
                    if (isVisibleMobj || IsVisibleToPlayer(h))
                        AddMessage(MakeMessage(h, String.Format("{0} {1} {2} {3}.", Conjugate(h, VERB_HELP), actor.Name, (isPulling ? "pulling" : "pushing"), mapObj.TheName)));
                }
            }
        }

        public void DoPush(Actor actor, MapObject mapObj, Point toPos)
        {
            bool isVisible = IsVisibleToPlayer(actor) || IsVisibleToPlayer(mapObj);
            int staCost = mapObj.Weight;

            // followers help?
            if (actor.CountFollowers > 0)
                DoPushPullFollowersHelp(actor, mapObj, false, ref staCost); // alpha10

            // spend AP & STA.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
            SpendActorStaminaPoints(actor, staCost);

            // do it : move object, then move actor if he is pushing it away and can enter the tile.
            Map map = mapObj.Location.Map;
            Point prevObjPos = mapObj.Location.Position;
            map.RemoveMapObjectAt(mapObj.Location.Position.X, mapObj.Location.Position.Y);
            map.PlaceMapObjectAt(mapObj, toPos);
            if (!m_Rules.IsAdjacent(toPos, actor.Location.Position) && m_Rules.IsWalkableFor(actor, map, prevObjPos.X, prevObjPos.Y))
            {
                // pushing away, need to follow.
                if (TryActorLeaveTile(actor))  // alpha10
                {
                    map.RemoveActor(actor);
                    map.PlaceActorAt(actor, prevObjPos);
                    OnActorEnterTile(actor);  // alpha10
                }
            }

            // noise/message.
            if (isVisible)
            {
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_PUSH), mapObj));
                RedrawPlayScreen();
            }
            else
            {
                // loud noise.
                OnLoudNoise(map, toPos, "Something being pushed");

                // player hears?
                if (m_Rules.RollChance(PLAYER_HEAR_PUSHPULL_CHANCE))
                {
                    AddMessageIfAudibleForPlayer(mapObj.Location, MakePlayerCentricMessage("You hear something being pushed", toPos));
                }
            }

            // check traps.
            CheckMapObjectTriggersTraps(map, toPos);
        }

        public void DoShove(Actor actor, Actor target, Point toPos)
        {
            // Target try to leave tile.
            if (!TryActorLeaveTile(target))
            {
                // waste ap.
                SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
                return;
            }

            // spend AP & STA.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
            SpendActorStaminaPoints(actor, Rules.DEFAULT_ACTOR_WEIGHT);

            // force target to stop dragging corpses.
            DoStopDraggingCorpses(target);

            // do it : move target, then move actor if he is pushing it away and can enter the tile.
            Map map = target.Location.Map;
            Point prevTargetPos = target.Location.Position;
            map.PlaceActorAt(target, toPos);
            if (!m_Rules.IsAdjacent(toPos, actor.Location.Position) && m_Rules.IsWalkableFor(actor, map, prevTargetPos.X, prevTargetPos.Y))
            {
                // shoving away, need to follow.
                // Try to leave tile.
                if (TryActorLeaveTile(actor))  // alpha10
                {
                    map.RemoveActor(actor);
                    map.PlaceActorAt(actor, prevTargetPos);
                    // Trigger stuff.
                    OnActorEnterTile(actor);
                }
            }

            // message.
            bool isVisible = IsVisibleToPlayer(actor) || IsVisibleToPlayer(target) || IsVisibleToPlayer(map, toPos);
            if (isVisible)
            {
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_SHOVE), target));
                RedrawPlayScreen();
            }

            // if target is sleeping, wakes him up!
            if (target.IsSleeping)
                DoWakeUp(target);

            // Trigger stuff.
            OnActorEnterTile(target);
        }

        // alpha10
        public void DoPull(Actor actor, MapObject mapObj, Point moveActorToPos)
        {
            bool isVisible = IsVisibleToPlayer(actor) || IsVisibleToPlayer(mapObj);
            int staCost = mapObj.Weight;

            // try leaving tile
            if (!TryActorLeaveTile(actor))
            {
                // waste ap.
                SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
                return;
            }

            // followers help?
            if (actor.CountFollowers > 0)
                DoPushPullFollowersHelp(actor, mapObj, true, ref staCost);

            // spend AP & STA.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
            SpendActorStaminaPoints(actor, staCost);

            // do it : move actor then move object
            Map map = mapObj.Location.Map;
            // actor...
            Point pullObjectTo = actor.Location.Position;
            map.RemoveActor(actor);
            map.PlaceActorAt(actor, moveActorToPos);  // assumed to be walkable, checked by rules
            // ...object
            map.RemoveMapObjectAt(mapObj.Location.Position.X, mapObj.Location.Position.Y);
            map.PlaceMapObjectAt(mapObj, pullObjectTo);

            // noise/message.
            if (isVisible)
            {
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_PULL), mapObj));
                RedrawPlayScreen();
            }
            else
            {
                // loud noise.
                OnLoudNoise(map, mapObj.Location.Position, "Something being pushed");

                // player hears?
                if (m_Rules.RollChance(PLAYER_HEAR_PUSHPULL_CHANCE))
                {
                    AddMessageIfAudibleForPlayer(mapObj.Location, MakePlayerCentricMessage("You hear something being pushed", mapObj.Location.Position));
                }
            }

            // check triggers
            OnActorEnterTile(actor);
            CheckMapObjectTriggersTraps(map, mapObj.Location.Position);
        }

        // alpha10
        public void DoPullActor(Actor actor, Actor target, Point moveActorToPos)
        {
            bool isVisible = IsVisibleToPlayer(actor) || IsVisibleToPlayer(target);

            // try leaving tile, both actors and target
            if (!TryActorLeaveTile(actor))
            {
                // waste ap.
                SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
                return;
            }
            if (!TryActorLeaveTile(target))
            {
                // waste ap.
                SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
                return;
            }

            // spend AP & STA.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
            SpendActorStaminaPoints(actor, Rules.DEFAULT_ACTOR_WEIGHT);

            // force target to stop dragging corpses.
            DoStopDraggingCorpses(target);

            // do it : move actor then move target
            Map map = target.Location.Map;
            // move actor...
            Point pullTargetTo = actor.Location.Position;
            map.RemoveActor(actor);
            map.PlaceActorAt(actor, moveActorToPos);
            // ...move target
            map.RemoveActor(target);
            map.PlaceActorAt(target, pullTargetTo);

            // if target is sleeping, wakes him up!
            if (target.IsSleeping)
                DoWakeUp(target);

            // message
            if (isVisible)
            {
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_PULL), target));
                RedrawPlayScreen();
            }

            // Trigger stuff.
            OnActorEnterTile(actor);
            OnActorEnterTile(target);
        }
        #endregion

        #region Sleeping & Waking up
        public void DoStartSleeping(Actor actor)
        {
            // spend AP.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // force actor to stop dragging corpses.
            DoStopDraggingCorpses(actor);

            // set activity & state.
            actor.Activity = Activity.SLEEPING;
            actor.IsSleeping = true;
        }

        public void DoWakeUp(Actor actor)
        {
            // set activity & state.
            actor.Activity = Activity.IDLE;
            actor.IsSleeping = false;

            // message.
            if (IsVisibleToPlayer(actor))
            {
                AddMessage(MakeMessage(actor, String.Format("{0}.", Conjugate(actor, VERB_WAKE_UP))));
            }

            // stop sleep music if player.
            if (actor.IsPlayer && m_MusicManager.Music == GameMusics.SLEEP)
                m_MusicManager.Stop();
        }
        #endregion

        #region Tagging
        void DoTag(Actor actor, ItemSprayPaint spray, Point pos)
        {
            // spend AP.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // spend paint.
            --spray.PaintQuantity;

            // add tag decoration.
            Map map = actor.Location.Map;
            map.GetTileAt(pos.X, pos.Y).AddDecoration((spray.Model as ItemSprayPaintModel).TagImageID);

            // message.
            if (IsVisibleToPlayer(actor))
            {
                AddMessage(MakeMessage(actor, String.Format("{0} a tag.", Conjugate(actor, VERB_SPRAY))));
            }
        }
        #endregion

        // alpha10 new way to use spray scent
        #region Spray scent
        public void DoSprayOdorSuppressor(Actor actor, ItemSprayScent suppressor, Actor sprayOn)
        {
            // spend AP.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // spend spray.
            --suppressor.SprayQuantity;

            // add odor suppressor on spray target
            sprayOn.OdorSuppressorCounter += suppressor.Strength;

            // message.
            if (IsVisibleToPlayer(actor))
            {
                AddMessage(MakeMessage(actor, string.Format("{0} {1}.", Conjugate(actor, VERB_SPRAY),
                    (sprayOn == actor ? HimselfOrHerself(actor) : sprayOn.Name))));
            }
        }
        #endregion

        #region Ordering
        void DoGiveOrderTo(Actor master, Actor slave, ActorOrder order)
        {
            // master spend AP.
            SpendActorActionPoints(master, Rules.BASE_ACTION_COST);

            // refuse if :
            // - master is not slave leader.
            // - slave is not trusting leader.
            if (master != slave.Leader)
            {
                DoSay(slave, master, "Who are you to give me orders?", Sayflags.IS_FREE_ACTION);
                return;
            }
            if (!m_Rules.IsActorTrustingLeader(slave))
            {
                DoSay(slave, master, "Sorry, I don't trust you enough yet.", Sayflags.IS_FREE_ACTION | Sayflags.IS_IMPORTANT);
                return;
            }

            // get AI.
            AIController ai = slave.Controller as AIController;
            if (ai == null)
                return;

            // give order.
            ai.SetOrder(order);

            // message.
            if (IsVisibleToPlayer(master) || IsVisibleToPlayer(slave))
            {
                AddMessage(MakeMessage(master, Conjugate(master, VERB_ORDER), slave, String.Format(" to {0}.", order.ToString())));
            }
        }

        void DoCancelOrder(Actor master, Actor slave)
        {
            // master spend AP.
            SpendActorActionPoints(master, Rules.BASE_ACTION_COST);

            // get AI.
            AIController ai = slave.Controller as AIController;
            if (ai == null)
                return;

            // cancel order.
            ai.SetOrder(null);

            // message.
            if (IsVisibleToPlayer(master) || IsVisibleToPlayer(slave))
            {
                AddMessage(MakeMessage(master, Conjugate(master, VERB_ORDER), slave, " to forget its orders."));
            }
        }
        #endregion

        #endregion

        #region Loud noises
        void OnLoudNoise(Map map, Point noisePosition, string noiseName)
        {
            ////////////////////////////////////////////
            // Check if nearby sleeping actors wake up.
            // Check long wait interruption.
            ////////////////////////////////////////////
            int xmin = noisePosition.X - Rules.LOUD_NOISE_RADIUS;
            int xmax = noisePosition.X + Rules.LOUD_NOISE_RADIUS;
            int ymin = noisePosition.Y - Rules.LOUD_NOISE_RADIUS;
            int ymax = noisePosition.Y + Rules.LOUD_NOISE_RADIUS;
            map.TrimToBounds(ref xmin, ref ymin);
            map.TrimToBounds(ref xmax, ref ymax);

            ///////////////////////////
            // Waking up nearby actors.
            ///////////////////////////
            for (int x = xmin; x <= xmax; x++)
            {
                for (int y = ymin; y <= ymax; y++)
                {
                    // sleeping actor?
                    Actor actor = map.GetActorAt(x, y);
                    if (actor == null || !actor.IsSleeping)
                        continue;

                    // ignore if too far.
                    int noiseDistance = m_Rules.GridDistance(noisePosition, x, y);
                    if (noiseDistance > Rules.LOUD_NOISE_RADIUS)
                        continue;

                    // roll chance of waking up.
                    int wakeupChance = m_Rules.ActorLoudNoiseWakeupChance(actor, noiseDistance);
                    if (!m_Rules.RollChance(wakeupChance))
                        continue;

                    // wake up!
                    DoWakeUp(actor);
                    if (IsVisibleToPlayer(actor))
                    {
                        AddMessage(new Message(String.Format("{0} wakes {1} up!", noiseName, actor.TheName), map.LocalTime.TurnCounter, actor == m_Player ? Color.Red : Color.White));
                        RedrawPlayScreen();
                    }
                }
            }

            ///////////////////////////
            // Interrupting long wait.
            ///////////////////////////
            if (m_IsPlayerLongWait && map == m_Player.Location.Map && IsVisibleToPlayer(map, noisePosition))
            {
                // interrupt!
                m_IsPlayerLongWaitForcedStop = true;
            }
        }
        #endregion

        #region Damaging, Killing & Disarming actors
        void InflictDamage(Actor actor, int dmg)
        {
            // HP.
            actor.HitPoints -= dmg;

            // Stamina.
            if (actor.Model.Abilities.CanTire)
            {
                actor.StaminaPoints -= dmg;
            }

            // Body armor breaks?
            Item torsoItem = actor.GetEquippedItem(DollPart.TORSO);
            if (torsoItem != null && torsoItem is ItemBodyArmor)
            {
                if (m_Rules.RollChance(Rules.BODY_ARMOR_BREAK_CHANCE))
                {
                    // do it.
                    OnUnequipItem(actor, torsoItem);
                    actor.Inventory.RemoveAllQuantity(torsoItem);

                    // message.
                    if (IsVisibleToPlayer(actor))
                    {
                        AddMessage(MakeMessage(actor, String.Format(": {0} breaks and is now useless!", torsoItem.TheName)));
                        RedrawPlayScreen();
                        AnimDelay(actor.IsPlayer ? DELAY_NORMAL : DELAY_SHORT);
                    }
                }
            }

            // If sleeping, wake up dude!
            if (actor.IsSleeping)
                DoWakeUp(actor);
        }

        // alpha10 drop corpse optional
        public void KillActor(Actor killer, Actor deadGuy, string reason, bool canDropCorpse = true)
        {
            // Sanity check.
#if false
            for some reason, this can happen with starved actors. no f*****g idea why since this is the only place where we set the dead flag.
            if (deadGuy.IsDead)
                throw new InvalidOperationException(String.Format("killing deadGuy that is already dead : killer={0} deadGuy={1} reason={2}", (
                    killer == null ? "N/A" : killer.TheName), deadGuy.TheName, reason));
#endif

            // Set dead flag.
            deadGuy.IsDead = true;

            // force to stop dragging corpses.
            DoStopDraggingCorpses(deadGuy);

            // untrigger all traps here.
            UntriggerAllTrapsHere(deadGuy.Location);

            // living killing undead = restore sanity.
            if (killer != null && !killer.Model.Abilities.IsUndead && killer.Model.Abilities.HasSanity && deadGuy.Model.Abilities.IsUndead)
                RegenActorSanity(killer, Rules.SANITY_RECOVER_KILL_UNDEAD);

            // death of bonded leader/follower hits sanity.
            if (deadGuy.HasLeader)
            {
                if (m_Rules.HasActorBondWith(deadGuy.Leader, deadGuy))
                {
                    SpendActorSanity(deadGuy.Leader, Rules.SANITY_HIT_BOND_DEATH);
                    if (IsVisibleToPlayer(deadGuy.Leader))
                    {
                        if (deadGuy.Leader.IsPlayer && !deadGuy.Leader.IsBotPlayer) ClearMessages();
                        AddMessage(MakeMessage(deadGuy.Leader, String.Format("{0} deeply disturbed by {1} sudden death!",
                            Conjugate(deadGuy.Leader, VERB_BE), deadGuy.Name)));
                        if (deadGuy.Leader.IsPlayer && !deadGuy.Leader.IsBotPlayer) AddMessagePressEnter();
                    }
                }
            }
            else if (deadGuy.CountFollowers > 0)
            {
                foreach (Actor fo in deadGuy.Followers)
                {
                    if (m_Rules.HasActorBondWith(fo, deadGuy))
                    {
                        SpendActorSanity(fo, Rules.SANITY_HIT_BOND_DEATH);
                        if (IsVisibleToPlayer(fo))
                        {
                            if (fo.IsPlayer && !fo.IsBotPlayer) ClearMessages();
                            AddMessage(MakeMessage(fo, String.Format("{0} deeply disturbed by {1} sudden death!",
                                Conjugate(fo, VERB_BE), deadGuy.Name)));
                            if (fo.IsPlayer && !fo.IsBotPlayer) AddMessagePressEnter();
                        }
                    }
                }
            }

            // Unique actor?
            if (deadGuy.IsUnique)
            {
                if (killer != null)
                    m_Session.Scoring.AddEvent(deadGuy.Location.Map.LocalTime.TurnCounter,
                        String.Format("* {0} was killed by {1} {2}! *", deadGuy.TheName, killer.Model.Name, killer.TheName));
                else
                    m_Session.Scoring.AddEvent(deadGuy.Location.Map.LocalTime.TurnCounter,
                        String.Format("* {0} died by {1}! *", deadGuy.TheName, reason));
            }

            // Player dead?
            // BEFORE removing followers & dropping items.
            if (deadGuy == m_Player)
                PlayerDied(killer, reason);

            // Remove followers.
            deadGuy.RemoveAllFollowers();

            // Remove from leader.
            #region
            if (deadGuy.Leader != null)
            {
                // player's follower killed : scoring and message.
                if (deadGuy.Leader.IsPlayer)
                {
                    string deathEvent;
                    if (killer != null)
                        deathEvent = String.Format("Follower {0} was killed by {1} {2}!", deadGuy.TheName, killer.Model.Name, killer.TheName);
                    else
                        deathEvent = String.Format("Follower {0} died by {1}!", deadGuy.TheName, reason);
                    m_Session.Scoring.AddEvent(deadGuy.Location.Map.LocalTime.TurnCounter, deathEvent);
                }

                deadGuy.Leader.RemoveFollower(deadGuy);
            }
            #endregion

            // Remove aggressor & self defence relations.
            bool wasMurder = (killer != null && m_Rules.IsMurder(killer, deadGuy));
            deadGuy.RemoveAllAgressorSelfDefenceRelations();

            // Remove from map.
            deadGuy.Location.Map.RemoveActor(deadGuy);

            // Drop some inventory items.
            #region
            if (deadGuy.Inventory != null && !deadGuy.Inventory.IsEmpty)
            {
                int deadItemsCount = deadGuy.Inventory.CountItems;
                Item[] dropThem = new Item[deadItemsCount];
                for (int i = 0; i < dropThem.Length; i++)
                    dropThem[i] = deadGuy.Inventory[i];
                for (int i = 0; i < dropThem.Length; i++)
                {
                    Item it = dropThem[i];
                    int chance = (it is ItemAmmo || it is ItemFood) ? Rules.VICTIM_DROP_AMMOFOOD_ITEM_CHANCE : Rules.VICTIM_DROP_GENERIC_ITEM_CHANCE;
                    if (it.Model.IsUnbreakable || it.IsUnique || m_Rules.RollChance(chance))
                        DropItem(deadGuy, it);
                }
            }
            #endregion

            // Blood splat/Remains
            if (!deadGuy.Model.Abilities.IsUndead)
                SplatterBlood(deadGuy.Location.Map, deadGuy.Location.Position);
#if false
            disabled: avoid unecessary large saved games
            if (deadGuy.Model.Abilities.IsUndead)
                UndeadRemains(deadGuy.Location.Map, deadGuy.Location.Position);
            else
                SplatterBlood(deadGuy.Location.Map, deadGuy.Location.Position);
#endif

            // Corpse?
            if (Rules.HasCorpses(m_Session.GameMode))
            {
                if (!deadGuy.Model.Abilities.IsUndead && canDropCorpse)
                {
                    DropCorpse(deadGuy);
                }
            }

            // One more kill
            if (killer != null)
                ++killer.KillsCount;

            // Player scoring
            if (killer == m_Player)
                PlayerKill(deadGuy);

            // Undead level up?
            #region
            if (killer != null && Rules.HasEvolution(m_Session.GameMode))
            {
                if (killer.Model.Abilities.IsUndead)
                {
                    #region
                    // check for evolution.
                    ActorModel levelUpModel = CheckUndeadEvolution(killer);
                    if (levelUpModel != null)
                    {
                        // Remember skills if any.
                        SkillTable savedSkills = null;
                        if (killer.Sheet.SkillTable != null && killer.Sheet.SkillTable.Skills != null)
                            savedSkills = new SkillTable(killer.Sheet.SkillTable.Skills);

                        // Do the transformation.
                        killer.Model = levelUpModel;

                        // If player, make sure it is setup properly.
                        if (killer.IsPlayer)
                            PrepareActorForPlayerControl(killer);

                        // If had skills, give them back.
                        if (savedSkills != null)
                        {
                            foreach (Skill s in savedSkills.Skills)
                                for (int i = 0; i < s.Level; i++)
                                {
                                    killer.Sheet.SkillTable.AddOrIncreaseSkill(s.ID);
                                    OnSkillUpgrade(killer, (Skills.IDs)s.ID);
                                }
                            m_TownGenerator.RecomputeActorStartingStats(killer);
                        }

                        // Message.
                        if (IsVisibleToPlayer(killer))
                        {
                            AddOverlay(new OverlayRect(Color.Yellow, new Rectangle(MapToScreen(killer.Location.Position), new Size(TILE_SIZE, TILE_SIZE))));
                            AddMessage(MakeMessage(killer, String.Format("{0} a {1} horror!", Conjugate(killer, VERB_TRANSFORM_INTO), levelUpModel.Name)));
                            RedrawPlayScreen();
                            AnimDelay(DELAY_LONG);
                            ClearOverlays();
                        }
                    }
                    #endregion
                }
            }
            #endregion

            // Trust : leader killing a follower target or adjacent enemy.
            if (killer != null && killer.CountFollowers > 0)
            {
                foreach (Actor fo in killer.Followers)
                {
                    bool gainTrust = false;
                    if (fo.TargetActor == deadGuy || (m_Rules.AreEnemies(fo, deadGuy) && m_Rules.IsAdjacent(fo.Location, deadGuy.Location)))
                        gainTrust = true;

                    if (gainTrust)
                    {
                        DoSay(fo, killer, "That was close! Thanks for the help!!", Sayflags.IS_FREE_ACTION);
                        ModifyActorTrustInLeader(fo, Rules.TRUST_LEADER_KILL_ENEMY, true);
                    }
                }
            }

            // Murder?
            #region
            if (wasMurder)
            {
                // one more murder.
                ++killer.MurdersCounter;

                // if player, log.
                if (killer.IsPlayer)
                    m_Session.Scoring.AddEvent(m_Session.WorldTime.TurnCounter, String.Format("Murdered {0} a {1}!", deadGuy.TheName, deadGuy.Model.Name));
                // message.
                if (IsVisibleToPlayer(killer))
                    AddMessage(MakeMessage(killer, String.Format("murdered {0}!!", deadGuy.Name)));

                // check for npcs law enforcers witnessing the murder.
                Map map = killer.Location.Map;
                Point killerPos = killer.Location.Position;
                foreach (Actor a in map.Actors)
                {
                    // check ability and state/relationship
                    if (!a.Model.Abilities.IsLawEnforcer || a.IsDead || a.IsSleeping || a.IsPlayer ||
                        a == killer || a == deadGuy || a.Leader == killer || killer.Leader == a)
                        continue;

                    // do as less computations as possible : we don't need all the actor f*****g fov, just the line to the murderer.

                    // fov range check. 
                    if (m_Rules.GridDistance(a.Location.Position, killerPos) > m_Rules.ActorFOV(a, map.LocalTime, m_Session.World.Weather))
                        continue;

                    // LOS check.
                    if (!LOS.CanTraceViewLine(a.Location, killerPos))
                        continue;

                    // we see the murderer!
                    // make enemy and emote.
                    DoSay(a, killer, String.Format("MURDER! {0} HAS KILLED {1}!", killer.TheName, deadGuy.TheName), Sayflags.IS_FREE_ACTION | Sayflags.IS_IMPORTANT);
                    DoMakeAggression(a, killer);
                }
            }
            #endregion

            // Emote: a law enforcer killing a murderer feels warm and fuzzy inside.
            #region
            if (killer != null && deadGuy.MurdersCounter > 0 && killer.Model.Abilities.IsLawEnforcer && !killer.Faction.IsEnemyOf(deadGuy.Faction))
            {
                if (killer.IsPlayer)
                    AddMessage(new Message("You feel like you did your duty with killing a murderer.", m_Session.WorldTime.TurnCounter, Color.White));
                else
                    DoSay(killer, deadGuy, "Good riddance, murderer!", Sayflags.IS_FREE_ACTION | Sayflags.IS_DANGER);
            }
            #endregion

            //////////////////////////////////////////////
            // Player or Player Followers Killing Uniques
            //////////////////////////////////////////////
            #region
            // The Sewers Thing
            if (deadGuy == m_Session.UniqueActors.TheSewersThing.TheActor)
            {
                if (killer == m_Player || killer.Leader == m_Player)
                {
                    // scoring.
                    m_Session.Scoring.SetCompletedAchievement(Achievement.IDs.KILLED_THE_SEWERS_THING);

                    // achievement!
                    ShowNewAchievement(Achievement.IDs.KILLED_THE_SEWERS_THING);
                }
            }
            #endregion
        }

        // alpha10
        /// <summary>
        /// 
        /// </summary>
        /// <param name="actor"></param>
        /// <returns>the disarmed item or null if actor had no equipped item</returns>
        Item Disarm(Actor actor)
        {
            Item disarmIt = null;

            // pick equipped item to disarm : prefer weapon, then any right handed item(?), then left handed.
            disarmIt = actor.GetEquippedWeapon();
            if (disarmIt == null)
            {
                disarmIt = actor.GetEquippedItem(DollPart.RIGHT_HAND);
                if (disarmIt == null)
                {
                    disarmIt = actor.GetEquippedItem(DollPart.LEFT_HAND);
                }
            }

            if (disarmIt == null)
                return null;

            // unequip, remove from inv and drop item in a random adjacent tile
            // if none possible, will drop on same tile (which then has no almost no gameplay effect 
            // because the actor can take it back asap at no ap cost... unless he dies)
            DoUnequipItem(actor, disarmIt, false);
            actor.Inventory.RemoveAllQuantity(disarmIt);
            List<Point> dropTiles = new List<Point>(8);
            actor.Location.Map.ForEachAdjacentInMap(actor.Location.Position,
                (pt) =>
                {
                    // checking if can drop there is eq to checking if can throw it there
                    if (!actor.Location.Map.IsBlockingThrow(pt.X, pt.Y))
                        dropTiles.Add(pt);
                });
            Point dropOnTile;
            if (dropTiles.Count > 0)
                dropOnTile = dropTiles[m_Rules.Roll(0, dropTiles.Count)];
            else
                dropOnTile = actor.Location.Position;
            actor.Location.Map.DropItemAt(disarmIt, dropOnTile);

            // done
            return disarmIt;
        }
        #endregion

        #region Undeads leveling up
        ActorModel CheckUndeadEvolution(Actor undead)
        {
            // check option & game mode.
            if (!s_Options.AllowUndeadsEvolution || !Rules.HasEvolution(m_Session.GameMode))
                return null;

            // evolve?
            bool evolve = false;
            switch (undead.Model.ID)
            {
                // zombie master 4 kills  & Day > X -> zombie lord
                case (int)GameActors.IDs.UNDEAD_ZOMBIE_MASTER:
                    {
                        if (undead.KillsCount < 4)
                            return null;
                        if (undead.Location.Map.LocalTime.Day < ZOMBIE_LORD_EVOLUTION_MIN_DAY && !undead.IsPlayer)
                            return null;
                        evolve = true;
                        break;
                    }

                // zombie lord 8 kills -> zombie prince.
                case (int)GameActors.IDs.UNDEAD_ZOMBIE_LORD:
                    {
                        if (undead.KillsCount < 8)
                            return null;
                        evolve = true;
                        break;
                    }

                // skeleton 2 kills -> red eyed skeleton
                case (int)GameActors.IDs.UNDEAD_SKELETON:
                    {
                        if (undead.KillsCount < 2)
                            return null;
                        evolve = true;
                        break;
                    }
                // red eye skeleton 4 kills -> red skeleton
                case (int)GameActors.IDs.UNDEAD_RED_EYED_SKELETON:
                    {
                        if (undead.KillsCount < 4)
                            return null;
                        evolve = true;
                        break;
                    }

                // zombie -> dark eyed zombie
                case (int)GameActors.IDs.UNDEAD_ZOMBIE:
                    evolve = true;
                    break;

                // dark eyed zombie -> dark zombie
                case (int)GameActors.IDs.UNDEAD_DARK_EYED_ZOMBIE:
                    evolve = true;
                    break;

                // zombified 2 kills -> neophyte
                case (int)GameActors.IDs.UNDEAD_MALE_ZOMBIFIED:
                case (int)GameActors.IDs.UNDEAD_FEMALE_ZOMBIFIED:
                    {
                        if (undead.KillsCount < 2)
                            return null;
                        evolve = true;
                        break;
                    }

                // neophyte 4 kills & Day > X -> disciple
                case (int)GameActors.IDs.UNDEAD_MALE_NEOPHYTE:
                case (int)GameActors.IDs.UNDEAD_FEMALE_NEOPHYTE:
                    {
                        if (undead.KillsCount < 4)
                            return null;
                        if (undead.Location.Map.LocalTime.Day < DISCIPLE_EVOLUTION_MIN_DAY && !undead.IsPlayer)
                            return null; ;
                        evolve = true;
                        break;
                    }

                default:
                    evolve = false;
                    break;
            }

            // evolve vs no evolution.
            if (evolve)
            {
                GameActors.IDs evolutionID = NextUndeadEvolution((GameActors.IDs)undead.Model.ID);
                if (evolutionID == (GameActors.IDs)undead.Model.ID)
                    return null;
                else
                    return GameActors[evolutionID];
            }
            else
                return null;
        }

        public GameActors.IDs NextUndeadEvolution(GameActors.IDs fromModelID)
        {
            switch (fromModelID)
            {
                case GameActors.IDs.UNDEAD_SKELETON: return GameActors.IDs.UNDEAD_RED_EYED_SKELETON;
                case GameActors.IDs.UNDEAD_RED_EYED_SKELETON: return GameActors.IDs.UNDEAD_RED_SKELETON;

                case GameActors.IDs.UNDEAD_ZOMBIE: return GameActors.IDs.UNDEAD_DARK_EYED_ZOMBIE;
                case GameActors.IDs.UNDEAD_DARK_EYED_ZOMBIE: return GameActors.IDs.UNDEAD_DARK_ZOMBIE;

                case GameActors.IDs.UNDEAD_FEMALE_ZOMBIFIED: return GameActors.IDs.UNDEAD_FEMALE_NEOPHYTE;
                case GameActors.IDs.UNDEAD_MALE_ZOMBIFIED: return GameActors.IDs.UNDEAD_MALE_NEOPHYTE;
                case GameActors.IDs.UNDEAD_FEMALE_NEOPHYTE: return GameActors.IDs.UNDEAD_FEMALE_DISCIPLE;
                case GameActors.IDs.UNDEAD_MALE_NEOPHYTE: return GameActors.IDs.UNDEAD_MALE_DISCIPLE;

                case GameActors.IDs.UNDEAD_ZOMBIE_MASTER: return GameActors.IDs.UNDEAD_ZOMBIE_LORD;
                case GameActors.IDs.UNDEAD_ZOMBIE_LORD: return GameActors.IDs.UNDEAD_ZOMBIE_PRINCE;

                default: return fromModelID;
            }
        }
        #endregion

        #region Blood & Remains
        public void SplatterBlood(Map map, Point position)
        {
            // splatter floor there.
            Tile tile = map.GetTileAt(position.X, position.Y);
            if (map.IsWalkable(position.X, position.Y) && !tile.HasDecoration(GameImages.DECO_BLOODIED_FLOOR))
            {
                tile.AddDecoration(GameImages.DECO_BLOODIED_FLOOR);
                map.AddTimer(new TaskRemoveDecoration(WorldTime.TURNS_PER_DAY, position.X, position.Y, GameImages.DECO_BLOODIED_FLOOR));
            }

            // splatter adjacent walls.
            foreach (Direction d in Direction.COMPASS)
            {
                if (!m_Rules.RollChance(BLOOD_WALL_SPLAT_CHANCE))
                    continue;
                Point next = position + d;
                if (!map.IsInBounds(next))
                    continue;
                Tile tileNext = map.GetTileAt(next.X, next.Y);
                if (tileNext.Model.IsWalkable)
                    continue;
                if (tileNext.HasDecoration(GameImages.DECO_BLOODIED_WALL))
                    continue;
                tileNext.AddDecoration(GameImages.DECO_BLOODIED_WALL);
                map.AddTimer(new TaskRemoveDecoration(WorldTime.TURNS_PER_DAY, next.X, next.Y, GameImages.DECO_BLOODIED_WALL));
            }
        }

        public void UndeadRemains(Map map, Point position)
        {
            // add deco there.
            Tile tile = map.GetTileAt(position.X, position.Y);
            if (map.IsWalkable(position.X, position.Y) && !tile.HasDecoration(GameImages.DECO_ZOMBIE_REMAINS))
                tile.AddDecoration(GameImages.DECO_ZOMBIE_REMAINS);
        }
        #endregion

        #region Corpses
        public void DropCorpse(Actor deadGuy)
        {
            // add blood to deadguy.
            deadGuy.Doll.AddDecoration(DollPart.TORSO, GameImages.BLOODIED);

            // make and add corpse.
            int corpseHp = m_Rules.ActorMaxHPs(deadGuy);
            float rotation = m_Rules.Roll(30, 60);
            if (m_Rules.RollChance(50)) rotation = -rotation;
            float scale = 1.0f;
            Corpse corpse = new Corpse(deadGuy, corpseHp, corpseHp, deadGuy.Location.Map.LocalTime.TurnCounter, rotation, scale);
            deadGuy.Location.Map.AddCorpseAt(corpse, deadGuy.Location.Position);
        }
        #endregion

        #region Player death & Post mortem
        void PlayerDied(Actor killer, string reason)
        {
            // stop sim thread.
            StopSimThread(true);   // alpha10 abort allowed when dying

            // mouse.
            m_UI.UI_SetCursor(null);

            // music.
            m_MusicManager.Stop();
            m_MusicManager.Play(GameMusics.PLAYER_DEATH, MusicPriority.PRIORITY_EVENT);

            ///////////
            // Scoring
            ///////////
            #region
            m_Session.Scoring.TurnsSurvived = m_Session.WorldTime.TurnCounter;
            m_Session.Scoring.SetKiller(killer);
            if (m_Player.CountFollowers > 0)
            {
                foreach (Actor fo in m_Player.Followers)
                    m_Session.Scoring.AddFollowerWhenDied(fo);
            }

            List<Zone> zone = m_Player.Location.Map.GetZonesAt(m_Player.Location.Position.X, m_Player.Location.Position.Y);
            if (zone == null)
            {
                m_Session.Scoring.DeathPlace = m_Player.Location.Map.Name;
            }
            else
            {
                string zoneName = zone[0].Name;
                m_Session.Scoring.DeathPlace = String.Format("{0} at {1}", m_Player.Location.Map.Name, zoneName);
            }
            if (killer != null)
                m_Session.Scoring.DeathReason = String.Format("{0} by {1} {2}",
                    m_Rules.IsMurder(killer, m_Player) ? "Murdered" : "Killed", killer.Model.Name, killer.TheName);
            else
                m_Session.Scoring.DeathReason = String.Format("Death by {0}", reason);
            m_Session.Scoring.AddEvent(m_Session.WorldTime.TurnCounter, "Died.");
            #endregion

            /////////////////////////////////////////
            // Tip, Message, screenshot & permadeath.
            /////////////////////////////////////////
            #region
            int iTip = m_Rules.Roll(0, GameTips.TIPS.Length);
            AddOverlay(new OverlayPopup(new string[] { "TIP OF THE DEAD", "Did you know that...", GameTips.TIPS[iTip] }, Color.White, Color.White, POPUP_FILLCOLOR, new Point(0, 0)));

            ClearMessages();
            AddMessage(new Message("**** YOU DIED! ****", m_Session.WorldTime.TurnCounter, Color.Red));
            if (killer != null)
                AddMessage(new Message(String.Format("Killer : {0}.", killer.TheName), m_Session.WorldTime.TurnCounter, Color.Red));
            AddMessage(new Message(String.Format("Reason : {0}.", reason), m_Session.WorldTime.TurnCounter, Color.Red));
            if (m_Player.Model.Abilities.IsUndead)
                AddMessage(new Message("You die one last time... Game over!", m_Session.WorldTime.TurnCounter, Color.Red));
            else
                AddMessage(new Message("You join the realm of the undeads... Game over!", m_Session.WorldTime.TurnCounter, Color.Red));

            // if permadeath on delete save file.
            if (s_Options.IsPermadeathOn)
                DeleteSavedGame(GetUserSave());

            // screenshot.
            if (s_Options.IsDeathScreenshotOn)
            {
                RedrawPlayScreen();
                string shotname = DoTakeScreenshot();
                if (shotname == null)
                    AddMessage(MakeErrorMessage("could not save death screenshot."));
                else
                    AddMessage(new Message(String.Format("Death screenshot saved : {0}.", shotname), m_Session.WorldTime.TurnCounter, Color.Red));
            }

            AddMessagePressEnter();
            #endregion

            // post mortem.
            HandlePostMortem();

            // music.
            m_MusicManager.Stop();

            // alpha10.1 bot release control
#if DEBUG
            BotReleaseControl();
#endif
        }

        string TimeSpanToString(TimeSpan rt)
        {
            // alpha10 shortened
            string timeDays = rt.Days == 0 ? "" : String.Format("{0} d ", rt.Days);
            string timeHours = rt.Hours == 0 ? "" : String.Format("{0:D2} h ", rt.Hours);
            string timeMinutes = rt.Minutes == 0 ? "" : String.Format("{0:D2} m ", rt.Minutes);
            string timeSeconds = rt.Seconds == 0 ? "" : String.Format("{0:D2} s", rt.Seconds);
            return String.Format("{0}{1}{2}{3}", timeDays, timeHours, timeMinutes, timeSeconds);
        }

        void HandlePostMortem()
        {
            ////////////////
            // Prepare data.
            ////////////////
            WorldTime deathTime = new WorldTime();
            deathTime.TurnCounter = m_Session.Scoring.TurnsSurvived;
            bool isMale = m_Player.Model.DollBody.IsMale;
            string heOrShe = isMale ? "He" : "She";
            string hisOrHer = HisOrHer(m_Player);
            string himOrHer = isMale ? "him" : "her";
            string name = m_Player.TheName.Replace("(YOU) ", "");
            TimeSpan rt = m_Session.Scoring.RealLifePlayingTime;
            string realTimeString = TimeSpanToString(rt);
            m_Session.Scoring.Side = m_Player.Model.Abilities.IsUndead ? DifficultySide.FOR_UNDEAD : DifficultySide.FOR_SURVIVOR;
            m_Session.Scoring.DifficultyRating = Scoring.ComputeDifficultyRating(s_Options, m_Session.Scoring.Side, m_Session.Scoring.ReincarnationNumber);

            ////////////////////////////////////
            // Format scoring into a text file.
            ///////////////////////////////////
            TextFile graveyard = new TextFile();

            graveyard.Append(String.Format("ROGUE SURVIVOR {0}", SetupConfig.GAME_VERSION));
            graveyard.Append("POST MORTEM");

            #region Summary
            graveyard.Append(String.Format("{0} was {1} and {2}.", name, AorAn(m_Player.Model.Name), AorAn(m_Player.Faction.MemberName)));
            graveyard.Append(String.Format("{0} survived to see {1}.", heOrShe, deathTime.ToString()));
            graveyard.Append(String.Format("{0}'s spirit guided {1} for {2}.", name, himOrHer, realTimeString));
            if (m_Session.Scoring.ReincarnationNumber > 0)
                graveyard.Append(String.Format("{0} was reincarnation {1}.", heOrShe, m_Session.Scoring.ReincarnationNumber));
            graveyard.Append(" ");
            #endregion

            graveyard.Append("> SCORING");
            #region
            graveyard.Append(String.Format("{0} scored a total of {1} points.", heOrShe, m_Session.Scoring.TotalPoints));
            graveyard.Append(String.Format("- difficulty rating of {0}%.", (int)(100 * m_Session.Scoring.DifficultyRating)));
            graveyard.Append(String.Format("- {0} base points for survival.", m_Session.Scoring.SurvivalPoints));
            graveyard.Append(String.Format("- {0} base points for kills.", m_Session.Scoring.KillPoints));
            graveyard.Append(String.Format("- {0} base points for achievements.", m_Session.Scoring.AchievementPoints));
            graveyard.Append(" ");
            #endregion

            graveyard.Append("> ACHIEVEMENTS");
            #region
            foreach (Achievement ach in m_Session.Scoring.Achievements)
            {
                if (ach.IsDone)
                    graveyard.Append(String.Format("- {0} for {1} points!", ach.Name, ach.ScoreValue));
                else
                    graveyard.Append(String.Format("- Fail : {0}.", ach.TeaseName));
            }
            if (m_Session.Scoring.CompletedAchievementsCount == 0)
            {
                graveyard.Append("Didn't achieve anything notable. And then died.");
                graveyard.Append(String.Format("(unlock all the {0} achievements to win this game version)", Scoring.MAX_ACHIEVEMENTS));
            }
            else
            {
                graveyard.Append(String.Format("Total : {0}/{1}.", m_Session.Scoring.CompletedAchievementsCount, Scoring.MAX_ACHIEVEMENTS));
                if (m_Session.Scoring.CompletedAchievementsCount >= Scoring.MAX_ACHIEVEMENTS)
                {
                    graveyard.Append("*** You achieved everything! You can consider having won this version of the game! CONGRATULATIONS! ***");
                }
                else
                    graveyard.Append("(unlock all the achievements to win this game version)");
                graveyard.Append("(later versions of the game will feature real winning conditions and multiple endings...)");
            }
            graveyard.Append(" ");
            #endregion

            graveyard.Append("> DEATH");
            #region
            graveyard.Append(String.Format("{0} in {1}.", m_Session.Scoring.DeathReason, m_Session.Scoring.DeathPlace));
            graveyard.Append(" ");
            #endregion

            graveyard.Append("> KILLS");
            #region
            if (m_Session.Scoring.HasNoKills)
            {
                graveyard.Append(String.Format("{0} was a pacifist. Or too scared to fight.", heOrShe));
            }
            else
            {
                // models kill list.
                foreach (Scoring.KillData killData in m_Session.Scoring.Kills)
                {
                    string modelName = killData.Amount > 1 ? Models.Actors[killData.ActorModelID].PluralName : Models.Actors[killData.ActorModelID].Name;
                    graveyard.Append(String.Format("{0,4} {1}.", killData.Amount, modelName));
                }
            }
            // murders? only livings.
            if (!m_Player.Model.Abilities.IsUndead)
            {
                if (m_Player.MurdersCounter > 0)
                {
                    graveyard.Append(String.Format("{0} committed {1} murder{2}!", heOrShe, m_Player.MurdersCounter, m_Player.MurdersCounter > 1 ? "s" : ""));
                }
            }

            graveyard.Append(" ");
            #endregion

            graveyard.Append("> FUN FACTS!");
            #region
            graveyard.Append(String.Format("While {0} has died, others are still having fun!", name));
            string[] funFacts = CompileDistrictFunFacts(m_Player.Location.Map.District);
            for (int i = 0; i < funFacts.Length; i++)
                graveyard.Append(funFacts[i]);
            graveyard.Append("");
            #endregion

            graveyard.Append("> SKILLS");
            #region
            if (m_Player.Sheet.SkillTable.Skills == null)
            {
                graveyard.Append(String.Format("{0} was a jack of all trades. Or an incompetent.", heOrShe));
            }
            else
            {
                foreach (Skill sk in m_Player.Sheet.SkillTable.Skills)
                {
                    graveyard.Append(String.Format("{0}-{1}.", sk.Level, Skills.Name(sk.ID)));
                }
            }
            graveyard.Append(" ");
            #endregion

            graveyard.Append("> INVENTORY");
            #region
            if (m_Player.Inventory.IsEmpty)
            {
                graveyard.Append(String.Format("{0} was humble. Or dirt poor.", heOrShe));
            }
            else
            {
                foreach (Item it in m_Player.Inventory.Items)
                {
                    string desc = DescribeItemShort(it);
                    if (it.IsEquipped)
                        graveyard.Append(String.Format("- {0} (equipped).", desc));
                    else
                        graveyard.Append(String.Format("- {0}.", desc));
                }
            }
            graveyard.Append(" ");
            #endregion

            graveyard.Append("> FOLLOWERS");
            #region
            if (m_Session.Scoring.FollowersWhendDied == null || m_Session.Scoring.FollowersWhendDied.Count == 0)
            {
                graveyard.Append(String.Format("{0} was doing fine alone. Or everyone else was dead.", heOrShe));
            }
            else
            {
                // names.
                StringBuilder sb = new StringBuilder(String.Format("{0} was leading", heOrShe));
                bool firstFo = true;
                int i = 0;
                int count = m_Session.Scoring.FollowersWhendDied.Count;
                foreach (Actor fo in m_Session.Scoring.FollowersWhendDied)
                {
                    if (firstFo)
                        sb.Append(" ");
                    else
                    {
                        if (i == count)
                            sb.Append(".");
                        else if (i == count - 1)
                            sb.Append(" and ");
                        else
                            sb.Append(", ");
                    }
                    sb.Append(fo.TheName);
                    ++i;
                    firstFo = false;
                }
                sb.Append(".");
                graveyard.Append(sb.ToString());

                // skills.
                foreach (Actor fo in m_Session.Scoring.FollowersWhendDied)
                {
                    graveyard.Append(String.Format("{0} skills : ", fo.Name));
                    if (fo.Sheet.SkillTable != null && fo.Sheet.SkillTable.Skills != null)
                    {
                        foreach (Skill sk in fo.Sheet.SkillTable.Skills)
                        {
                            graveyard.Append(String.Format("{0}-{1}.", sk.Level, Skills.Name(sk.ID)));
                        }
                    }
                }
            }
            graveyard.Append(" ");
            #endregion

            graveyard.Append("> EVENTS");
            #region
            if (m_Session.Scoring.HasNoEvents)
            {
                graveyard.Append(String.Format("{0} had a quiet life. Or dull and boring.", heOrShe));
            }
            else
            {
                foreach (Scoring.GameEventData ev in m_Session.Scoring.Events)
                {
                    WorldTime evTime = new WorldTime();
                    evTime.TurnCounter = ev.Turn;
                    graveyard.Append(String.Format("- {0,13} : {1}", evTime.ToString(), ev.Text));
                }
            }
            graveyard.Append(" ");
            #endregion

            graveyard.Append("> CUSTOM OPTIONS");
            #region
            graveyard.Append(String.Format("- difficulty rating of {0}%.", (int)(100 * m_Session.Scoring.DifficultyRating)));
            if (s_Options.IsPermadeathOn)
                graveyard.Append(String.Format("- {0} : yes.", GameOptions.Name(GameOptions.IDs.GAME_PERMADEATH)));
            if (!s_Options.AllowUndeadsEvolution && Rules.HasEvolution(m_Session.GameMode)) // alpha10 only if manually disabled
                graveyard.Append(String.Format("- {0} : {1}.", GameOptions.Name(GameOptions.IDs.GAME_ALLOW_UNDEADS_EVOLUTION), s_Options.AllowUndeadsEvolution ? "yes" : "no"));
            if (s_Options.CitySize != GameOptions.DEFAULT_CITY_SIZE)
                graveyard.Append(String.Format("- {0} : {1}.", GameOptions.Name(GameOptions.IDs.GAME_CITY_SIZE), s_Options.CitySize));
            if (s_Options.DayZeroUndeadsPercent != GameOptions.DEFAULT_DAY_ZERO_UNDEADS_PERCENT)
                graveyard.Append(String.Format("- {0} : {1}%.", GameOptions.Name(GameOptions.IDs.GAME_DAY_ZERO_UNDEADS_PERCENT), s_Options.DayZeroUndeadsPercent));
            if (s_Options.DistrictSize != GameOptions.DEFAULT_DISTRICT_SIZE)
                graveyard.Append(String.Format("- {0} : {1}.", GameOptions.Name(GameOptions.IDs.GAME_DISTRICT_SIZE), s_Options.DistrictSize));
            if (s_Options.MaxCivilians != GameOptions.DEFAULT_MAX_CIVILIANS)
                graveyard.Append(String.Format("- {0} : {1}.", GameOptions.Name(GameOptions.IDs.GAME_MAX_CIVILIANS), s_Options.MaxCivilians));
            if (s_Options.MaxUndeads != GameOptions.DEFAULT_MAX_UNDEADS)
                graveyard.Append(String.Format("- {0} : {1}.", GameOptions.Name(GameOptions.IDs.GAME_MAX_UNDEADS), s_Options.MaxUndeads));
            if (!s_Options.NPCCanStarveToDeath)
                graveyard.Append(String.Format("- {0} : {1}.", GameOptions.Name(GameOptions.IDs.GAME_NPC_CAN_STARVE_TO_DEATH), s_Options.NPCCanStarveToDeath ? "yes" : "no"));
            if (s_Options.StarvedZombificationChance != GameOptions.DEFAULT_STARVED_ZOMBIFICATION_CHANCE)
                graveyard.Append(String.Format("- {0} : {1}%.", GameOptions.Name(GameOptions.IDs.GAME_STARVED_ZOMBIFICATION_CHANCE), s_Options.StarvedZombificationChance));
            if (!s_Options.RevealStartingDistrict)
                graveyard.Append(String.Format("- {0} : {1}.", GameOptions.Name(GameOptions.IDs.GAME_REVEAL_STARTING_DISTRICT), s_Options.RevealStartingDistrict ? "yes" : "no"));
            if (s_Options.SimulateDistricts != GameOptions.DEFAULT_SIM_DISTRICTS)
                graveyard.Append(String.Format("- {0} : {1}.", GameOptions.Name(GameOptions.IDs.GAME_SIMULATE_DISTRICTS), GameOptions.Name(s_Options.SimulateDistricts)));
            if (s_Options.SimulateWhenSleeping)
                graveyard.Append(String.Format("- {0} : {1}.", GameOptions.Name(GameOptions.IDs.GAME_SIMULATE_SLEEP), s_Options.SimulateWhenSleeping ? "yes" : "no"));
            if (s_Options.ZombieInvasionDailyIncrease != GameOptions.DEFAULT_ZOMBIE_INVASION_DAILY_INCREASE)
                graveyard.Append(String.Format("- {0} : {1}%.", GameOptions.Name(GameOptions.IDs.GAME_ZOMBIE_INVASION_DAILY_INCREASE), s_Options.ZombieInvasionDailyIncrease));
            if (s_Options.ZombificationChance != GameOptions.DEFAULT_ZOMBIFICATION_CHANCE)
                graveyard.Append(String.Format("- {0} : {1}%.", GameOptions.Name(GameOptions.IDs.GAME_ZOMBIFICATION_CHANCE), s_Options.ZombificationChance));
            if (s_Options.MaxReincarnations != GameOptions.DEFAULT_MAX_REINCARNATIONS)
                graveyard.Append(String.Format("- {0} : {1}.", GameOptions.Name(GameOptions.IDs.GAME_MAX_REINCARNATIONS), s_Options.MaxReincarnations));
            graveyard.Append(" ");
            #endregion

            graveyard.Append("> R.I.P");
            #region
            graveyard.Append(String.Format("May {0} soul rest in peace.", HisOrHer(m_Player)));
            graveyard.Append(String.Format("For {0} body is now a meal for evil.", HisOrHer(m_Player)));
            graveyard.Append("The End.");
            #endregion

            /////////////////////
            // Save to graveyard
            /////////////////////
            int gx, gy;
            gx = gy = 0;
            m_UI.UI_Clear(Color.Black);
            m_UI.UI_DrawStringBold(Color.Yellow, "Saving post mortem to graveyard...", 0, 0);
            gy += BOLD_LINE_SPACING;
            m_UI.UI_Repaint();
            string graveName = GetUserNewGraveyardName();
            string graveFile = GraveFilePath(graveName);
            if (!graveyard.Save(graveFile))
            {
                m_UI.UI_DrawStringBold(Color.Red, "Could not save to graveyard.", 0, gy);
                gy += BOLD_LINE_SPACING;
            }
            else
            {
                m_UI.UI_DrawStringBold(Color.Yellow, "Grave saved to :", 0, gy);
                gy += BOLD_LINE_SPACING;
                m_UI.UI_DrawString(Color.White, graveFile, 0, gy);
                gy += BOLD_LINE_SPACING;
            }
            DrawFootnote(Color.White, "press ENTER");
            m_UI.UI_Repaint();
            WaitEnter();

            ///////////////////////////////
            // Display grave as text file.
            ///////////////////////////////
            graveyard.FormatLines(TEXTFILE_CHARS_PER_LINE);
            int iLine = 0;
            bool loop = false;
            do
            {
                // header.
                m_UI.UI_Clear(Color.Black);
                gx = gy = 0;
                DrawHeader();
                gy += BOLD_LINE_SPACING;

                // text.
                int linesThisPage = 0;
                m_UI.UI_DrawStringBold(Color.White, "---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+", 0, gy);
                gy += BOLD_LINE_SPACING;
                while (linesThisPage < TEXTFILE_LINES_PER_PAGE && iLine < graveyard.FormatedLines.Count)
                {
                    string line = graveyard.FormatedLines[iLine];
                    m_UI.UI_DrawStringBold(Color.White, line, gx, gy);
                    gy += BOLD_LINE_SPACING;
                    ++iLine;
                    ++linesThisPage;
                }

                // foot.
                m_UI.UI_DrawStringBold(Color.White, "---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+", 0, CANVAS_HEIGHT - 2 * BOLD_LINE_SPACING);
                if (iLine < graveyard.FormatedLines.Count)
                    DrawFootnote(Color.White, "press ENTER for more");
                else
                    DrawFootnote(Color.White, "press ENTER to leave");

                // wait.
                m_UI.UI_Repaint();
                WaitEnter();

                // loop?
                loop = (iLine < graveyard.FormatedLines.Count);
            }
            while (loop);

            /////////////
            // Hi Score?
            /////////////
            StringBuilder skillsSb = new StringBuilder();
            if (m_Player.Sheet.SkillTable.Skills != null)
            {
                foreach (Skill sk in m_Player.Sheet.SkillTable.Skills)
                {
                    skillsSb.AppendFormat("{0}-{1} ", sk.Level, Skills.Name(sk.ID));
                }
            }
            HiScore newHiScore = HiScore.FromScoring(name, m_Session.Scoring, skillsSb.ToString());
            if (m_HiScoreTable.Register(newHiScore))
            {
                SaveHiScoreTable();
                HandleHiScores(true);
            }
        }

        #endregion

        #region New day/night, Scoring, Advancement
        void OnNewNight()
        {
            UpdatePlayerFOV(m_Player);

            //----- Upgrade Player (undead only once every 2 nights)
            if (m_Player.Model.Abilities.IsUndead && m_Player.Location.Map.LocalTime.Day % 2 == 1)
            {
                // Mode.
                ClearOverlays();
                AddOverlay(new OverlayPopup(UPGRADE_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, Point.Empty));

                // music.
                m_MusicManager.Stop();
                m_MusicManager.Play(GameMusics.INTERLUDE, MusicPriority.PRIORITY_EVENT);

                // Message.
                ClearMessages();
                AddMessage(new Message("You will hunt another day!", m_Session.WorldTime.TurnCounter, Color.Green));
                UpdatePlayerFOV(m_Player);
                if (!m_Player.IsBotPlayer)
                    AddMessagePressEnter();

                // Upgrade time!
                // alpha10.1 handle bot skill upgrade, bot followers will upgrade as npcs
                if (m_Player.IsBotPlayer)
                {
                    HandleNPCSkillUpgrade(m_Player);
                }
                else
                {
                    HandlePlayerDecideUpgrade(m_Player);
                    HandlePlayerFollowersUpgrade();
                }

                // Resume play.
                ClearMessages();
                AddMessage(new Message("Welcome to the night.", m_Session.WorldTime.TurnCounter, Color.White));
                ClearOverlays();
                RedrawPlayScreen();

                // music
                m_MusicManager.Stop();
            }
        }

        void OnNewDay()
        {
            /////////////////////////
            // Normal day processing
            /////////////////////////

            //----- Upgrade Player (living only)
            if (!m_Player.Model.Abilities.IsUndead)
            {
                // Mode.
                ClearOverlays();
                AddOverlay(new OverlayPopup(UPGRADE_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, Point.Empty));

                // music.
                m_MusicManager.Stop();
                m_MusicManager.Play(GameMusics.INTERLUDE, MusicPriority.PRIORITY_EVENT);

                // Message.
                ClearMessages();
                AddMessage(new Message("You survived another night!", m_Session.WorldTime.TurnCounter, Color.Green));
                UpdatePlayerFOV(m_Player);
                if (!m_Player.IsBotPlayer)
                    AddMessagePressEnter();

                // Upgrade time!
                // alpha10.1 handle bot skill upgrade, bot followers will upgrade as npcs
                if (m_Player.IsBotPlayer)
                {
                    HandleNPCSkillUpgrade(m_Player);
                }
                else
                {
                    HandlePlayerDecideUpgrade(m_Player);
                    HandlePlayerFollowersUpgrade();
                }

                // Resume play.
                ClearMessages();
                AddMessage(new Message("Welcome to tomorrow.", m_Session.WorldTime.TurnCounter, Color.White));
                ClearOverlays();
                RedrawPlayScreen();

                // music
                m_MusicManager.Stop();
            }

            // alpha10 obsolete
            //// Check weather change.
            //CheckWeatherChange();

            //////////////////////////////
            // New day achievements.
            // 1. Reached day X (living only)
            //////////////////////////////
            // 1. Reached day X (living only)
            if (!m_Player.Model.Abilities.IsUndead)
            {
                if (m_Session.WorldTime.Day == 7)
                {
                    // scoring.
                    m_Session.Scoring.SetCompletedAchievement(Achievement.IDs.REACHED_DAY_07);

                    // achievement!
                    ShowNewAchievement(Achievement.IDs.REACHED_DAY_07);
                }
                else if (m_Session.WorldTime.Day == 14)
                {
                    // scoring.
                    m_Session.Scoring.SetCompletedAchievement(Achievement.IDs.REACHED_DAY_14);

                    // achievement!
                    ShowNewAchievement(Achievement.IDs.REACHED_DAY_14);
                }
                else if (m_Session.WorldTime.Day == 21)
                {
                    // scoring.
                    m_Session.Scoring.SetCompletedAchievement(Achievement.IDs.REACHED_DAY_21);

                    // achievement!
                    ShowNewAchievement(Achievement.IDs.REACHED_DAY_21);
                }
                else if (m_Session.WorldTime.Day == 28)
                {
                    // scoring.
                    m_Session.Scoring.SetCompletedAchievement(Achievement.IDs.REACHED_DAY_28);

                    // achievement!
                    ShowNewAchievement(Achievement.IDs.REACHED_DAY_28);
                }
            }
        }

        void HandlePlayerDecideUpgrade(Actor upgradeActor)
        {
            // roll N skills to updgrade.
            List<Skills.IDs> upgradeChoices = RollSkillsToUpgrade(upgradeActor, 3 * 100);

            // "you" vs follower name.
            string youName = upgradeActor == m_Player ? "You" : upgradeActor.Name;

            // loop.
            bool loop = true;
            do
            {
                OverlayPopupTitle popup = null;

                ///////////////////
                // 1. Redraw
                // 2. Read input
                // 3. Handle input
                ///////////////////

                // 1. Redraw
                ClearMessages();
                AddMessage(new Message(youName + " can improve or learn one of these skills. Choose wisely.", m_Session.WorldTime.TurnCounter, Color.Green));

                if (upgradeChoices.Count == 0)
                {
                    AddMessage(MakeErrorMessage(youName + " can't learn anything new!"));
                }
                else
                {
                    List<string> popupLines = new List<string>();
                    popupLines.Add(" ");

                    for (int iChoice = 0; iChoice < upgradeChoices.Count; iChoice++)
                    {
                        Skills.IDs sk = upgradeChoices[iChoice];
                        int level = upgradeActor.Sheet.SkillTable.GetSkillLevel((int)sk);
                        string text = string.Format("{0}. {1} {2}/{3}", iChoice + 1, Skills.Name(sk), level + 1, Skills.MaxSkillLevel(sk));
                        AddMessage(new Message(text, m_Session.WorldTime.TurnCounter, Color.LightGreen));

                        popupLines.Add(text);
                        popupLines.Add("    " + DescribeSkillShort(sk));
                        popupLines.Add(" ");
                    }

                    popupLines.Add("ESC. don't upgrade");

                    if (upgradeActor != m_Player)
                    {
                        popupLines.Add(" ");
                        popupLines.Add(upgradeActor.Name + " current skills");
                        foreach (Skill sk in upgradeActor.Sheet.SkillTable.Skills)
                        {
                            popupLines.Add(string.Format("{0} {1}", Skills.Name(sk.ID), sk.Level));
                        }
                    }

                    popup = new OverlayPopupTitle(upgradeActor == m_Player ? "Select skill to upgrade" : "Select skill to upgrade for " + upgradeActor.Name, Color.White, popupLines.ToArray(), Color.White, Color.White, Color.Black, new Point(64, 64));
                    AddOverlay(popup);
                }
                AddMessage(new Message("ESC if you don't want to upgrade.", m_Session.WorldTime.TurnCounter, Color.White));
                RedrawPlayScreen();

                // 2. Read input
                KeyEventArgs inKey = m_UI.UI_WaitKey();

                // 3. Handle input
                PlayerCommand command = InputTranslator.KeyToCommand(inKey);
                if (inKey.KeyCode == Keys.Escape)// command == PlayerCommand.EXIT_OR_CANCEL)
                {
                    loop = false;
                    if (popup != null) RemoveOverlay(popup);
                    RedrawPlayScreen();
                }
                else
                {
                    // get choice.
                    int choice = KeyToChoiceNumber(inKey.KeyCode);

                    if (choice >= 1 && choice <= upgradeChoices.Count)
                    {
                        // upgrade skill.
                        Skills.IDs skID = upgradeChoices[choice - 1];
                        Skill sk = SkillUpgrade(upgradeActor, skID);

                        // message & scoring.
                        if (sk.Level == 1)
                        {
                            AddMessage(new Message(String.Format("{0} learned skill {1}.", upgradeActor.Name, Skills.Name(sk.ID)), m_Session.WorldTime.TurnCounter, Color.LightGreen));
                            m_Session.Scoring.AddEvent(m_Session.WorldTime.TurnCounter, String.Format("{0} learned skill {1}.", upgradeActor.Name, Skills.Name(sk.ID)));
                        }
                        else
                        {
                            AddMessage(new Message(String.Format("{0} improved skill {1} to level {2}.", upgradeActor.Name, Skills.Name(sk.ID), sk.Level), m_Session.WorldTime.TurnCounter, Color.LightGreen));
                            m_Session.Scoring.AddEvent(m_Session.WorldTime.TurnCounter, String.Format("{0} improved skill {1} to level {2}.", upgradeActor.Name, Skills.Name(sk.ID), sk.Level));
                        }
                        AddMessagePressEnter();
                        if (popup != null) RemoveOverlay(popup);
                        RedrawPlayScreen();
                        loop = false;
                    }
                }

            } while (loop);
        }

        void HandlePlayerFollowersUpgrade()
        {
            // if no followers, nothing to do.
            if (m_Player.CountFollowers == 0)
                return;

            // Message.
            ClearMessages();
            AddMessage(new Message("Your followers learned new skills at your side!", m_Session.WorldTime.TurnCounter, Color.Green));
            AddMessagePressEnter();

            // Do it.
            foreach (Actor follower in m_Player.Followers)
            {
                // player pick for the follower.
                HandlePlayerDecideUpgrade(follower);
            }

        }

        void HandleLivingNPCsUpgrade(Map map)
        {
            foreach (Actor a in map.Actors)
            {
                // ignore player, we do it separatly.
                if (a == m_Player)
                    continue;
                // ignore player followers (upgraded already)
                if (a.Leader == m_Player)
                    continue;
                // not undeads!
                if (a.Model.Abilities.IsUndead)
                    continue;

                // do it!
                HandleNPCSkillUpgrade(a);  // alpha10.1
            }
        }

        // alpha10.1 factorized to handle bot skill upgrade
        void HandleNPCSkillUpgrade(Actor a)
        {
            List<Skills.IDs> upgradeFrom = RollSkillsToUpgrade(a, 3 * 100);
            Skills.IDs? chosenSkill = NPCPickSkillToUpgrade(a, upgradeFrom);
            if (chosenSkill == null)
                return;
            // upgrade it!
            SkillUpgrade(a, chosenSkill.Value);
        }

        void HandleUndeadNPCsUpgrade(Map map)
        {
            foreach (Actor a in map.Actors)
            {
                // ignore player, we do it separatly.
                if (a == m_Player)
                    continue;
                // ignore player followers (upgraded already)
                if (a.Leader == m_Player)
                    continue;
                // undeads only, and some branches only.
                if (!a.Model.Abilities.IsUndead)
                    continue;
                if (!s_Options.SkeletonsUpgrade && GameActors.IsSkeletonBranch(a.Model))
                    continue;
                if (!s_Options.RatsUpgrade && GameActors.IsRatBranch(a.Model))
                    continue;
                if (!s_Options.ShamblersUpgrade && GameActors.IsShamblerBranch(a.Model))
                    continue;

                // do it!
                List<Skills.IDs> upgradeFrom = RollSkillsToUpgrade(a, 3 * 100);
                Skills.IDs? chosenSkill = NPCPickSkillToUpgrade(a, upgradeFrom);
                if (chosenSkill == null)
                    continue;
                // upgrade it!
                SkillUpgrade(a, chosenSkill.Value);
            }
        }

        List<Skills.IDs> RollSkillsToUpgrade(Actor actor, int maxTries)
        {
            int count = (actor.Model.Abilities.IsUndead ? Rules.UNDEAD_UPGRADE_SKILLS_TO_CHOOSE_FROM : Rules.UPGRADE_SKILLS_TO_CHOOSE_FROM);
            List<Skills.IDs> list = new List<Skills.IDs>(count);

            for (int i = 0; i < count; i++)
            {
                Skills.IDs? newSk;
                int attempt = 0;
                do
                {
                    ++attempt;
                    newSk = RollRandomSkillToUpgrade(actor, maxTries);
                    if (newSk == null)
                        return list;
                } while (list.Contains(newSk.Value) && attempt < maxTries);

                list.Add(newSk.Value);
            }

            return list;
        }

        Skills.IDs? NPCPickSkillToUpgrade(Actor npc, List<Skills.IDs> chooseFrom)
        {
            if (chooseFrom == null || chooseFrom.Count == 0)
                return null;

            // Compute skill utilities and get best utility.
            int N = chooseFrom.Count;
            int[] utilities = new int[N];
            int bestUtility = -1;
            for (int i = 0; i < N; i++)
            {
                utilities[i] = NPCSkillUtility(npc, chooseFrom[i]);
                if (utilities[i] > bestUtility)
                    bestUtility = utilities[i];
            }

            // Randomly choose on of the best.
            List<Skills.IDs> bestSkills = new List<Skills.IDs>(N);
            for (int i = 0; i < N; i++)
                if (utilities[i] == bestUtility)
                    bestSkills.Add(chooseFrom[i]);
            return bestSkills[m_Rules.Roll(0, bestSkills.Count)];
        }

        int NPCSkillUtility(Actor actor, Skills.IDs skID)
        {
            const int USELESS_UTIL = 0;
            const int LOW_UTIL = 1;
            const int AVG_UTIL = 2;
            const int HI_UTIL = 3;

            if (actor.Model.Abilities.IsUndead)
            {
                // undeads.
                switch (skID)
                {
                    // useful one.
                    case Skills.IDs.Z_GRAB:
                    case Skills.IDs.Z_INFECTOR:
                    case Skills.IDs.Z_LIGHT_EATER:
                        return HI_UTIL;

                    // ok ones.
                    case Skills.IDs.Z_AGILE:
                    case Skills.IDs.Z_STRONG:
                    case Skills.IDs.Z_TOUGH:
                    case Skills.IDs.Z_TRACKER:
                        return AVG_UTIL;

                    // meh ones.
                    case Skills.IDs.Z_EATER:
                    case Skills.IDs.Z_LIGHT_FEET:
                        return LOW_UTIL;

                    default:
                        return USELESS_UTIL;
                }
            }
            else
            {
                switch (skID)
                {
                    case Skills.IDs.AGILE:
                        return AVG_UTIL;

                    case Skills.IDs.AWAKE:
                        // useful only if has to sleep.                    
                        return actor.Model.Abilities.HasToSleep ? HI_UTIL : USELESS_UTIL;

                    case Skills.IDs.BOWS:
                        {
                            // useful only if has bow weapon.
                            if (actor.Inventory != null)
                            {
                                foreach (Item it in actor.Inventory.Items)
                                    if (it is ItemRangedWeapon)
                                    {
                                        if ((it.Model as ItemRangedWeaponModel).IsBow)
                                            return HI_UTIL;
                                    }
                            }
                            return USELESS_UTIL;
                        }

                    case Skills.IDs.CARPENTRY:
                        return LOW_UTIL;

                    case Skills.IDs.CHARISMATIC:
                        // useful only if leader.
                        return actor.CountFollowers > 0 ? LOW_UTIL : USELESS_UTIL;

                    case Skills.IDs.FIREARMS:
                        {
                            // useful only if has firearm weapon.
                            if (actor.Inventory != null)
                            {
                                foreach (Item it in actor.Inventory.Items)
                                    if (it is ItemRangedWeapon)
                                    {
                                        if ((it.Model as ItemRangedWeaponModel).IsFireArm)
                                            return HI_UTIL;
                                    }
                            }
                            return USELESS_UTIL;
                        }

                    case Skills.IDs.HARDY:
                        // useful only if has to sleep.                    
                        return actor.Model.Abilities.HasToSleep ? HI_UTIL : USELESS_UTIL;

                    case Skills.IDs.HAULER:
                        return HI_UTIL;

                    case Skills.IDs.HIGH_STAMINA:
                        return HI_UTIL;  // alpha10; was previously rated as avg

                    case Skills.IDs.LEADERSHIP:
                        // useful only if not follower.
                        return actor.HasLeader ? USELESS_UTIL : LOW_UTIL;

                    case Skills.IDs.LIGHT_EATER:
                        // useful only if has to eat.                    
                        return actor.Model.Abilities.HasToEat ? HI_UTIL : USELESS_UTIL;

                    case Skills.IDs.LIGHT_FEET:
                        return AVG_UTIL;

                    case Skills.IDs.LIGHT_SLEEPER:
                        // useful only if has to sleep.                    
                        return actor.Model.Abilities.HasToSleep ? AVG_UTIL : USELESS_UTIL;

                    case Skills.IDs.MARTIAL_ARTS:
                        {
                            // useless if any weapon in inventory.
                            if (actor.Inventory != null)
                            {
                                foreach (Item it in actor.Inventory.Items)
                                {
                                    if (it is ItemWeapon)
                                        return LOW_UTIL;
                                }
                            }
                            return AVG_UTIL;
                        }

                    case Skills.IDs.MEDIC:
                        return LOW_UTIL;

                    case Skills.IDs.NECROLOGY:
                        return LOW_UTIL; // alpha10 ; was previously rated as useless

                    case Skills.IDs.STRONG:
                        return AVG_UTIL;

                    case Skills.IDs.STRONG_PSYCHE:
                        // useful only if has sanity.
                        return actor.Model.Abilities.HasSanity ? HI_UTIL : USELESS_UTIL;

                    case Skills.IDs.TOUGH:
                        return HI_UTIL;

                    case Skills.IDs.UNSUSPICIOUS:
                        // useful only if murderer and not law enforcer.
                        return actor.MurdersCounter > 0 && !actor.Model.Abilities.IsLawEnforcer ? LOW_UTIL : USELESS_UTIL;

                    default:
                        return USELESS_UTIL;
                }
            }
        }

        Skills.IDs? RollRandomSkillToUpgrade(Actor actor, int maxTries)
        {
            int attempt = 0;
            int skID;
            bool isUndead = actor.Model.Abilities.IsUndead;

            do
            {
                ++attempt;
                skID = isUndead ? (int)Skills.RollUndead(Rules.DiceRoller) : (int)Skills.RollLiving(Rules.DiceRoller);
            }
            while (actor.Sheet.SkillTable.GetSkillLevel(skID) >= Skills.MaxSkillLevel(skID) && attempt < maxTries);

            if (attempt >= maxTries)
                return null;
            else
                return (Skills.IDs)skID;
        }

        void DoLooseRandomSkill(Actor actor)
        {
            int[] skills = actor.Sheet.SkillTable.SkillsList;
            if (skills == null) return;

            // pick a skill.
            int iSkill = m_Rules.Roll(0, skills.Length);
            Skills.IDs lostSkill = (Skills.IDs)skills[iSkill];

            // regress.
            actor.Sheet.SkillTable.DecOrRemoveSkill((int)lostSkill);

            // message.
            if (IsVisibleToPlayer(actor))
                AddMessage(MakeMessage(actor, String.Format("regressed in {0}!", Skills.Name(lostSkill))));
        }

        public Skill SkillUpgrade(Actor actor, Skills.IDs id)
        {
            actor.Sheet.SkillTable.AddOrIncreaseSkill((int)id);
            Skill sk = actor.Sheet.SkillTable.GetSkill((int)id);
            OnSkillUpgrade(actor, id);

            return sk;
        }

        public void OnSkillUpgrade(Actor actor, Skills.IDs id)
        {
            switch (id)
            {
                case Skills.IDs.HAULER:
                    if (actor.Inventory != null)
                        actor.Inventory.MaxCapacity = m_Rules.ActorMaxInv(actor);
                    break;

                default:
                    // no special upkeep to do.
                    break;
            }
        }

        void ChangeWeather()
        {
            bool canSeeWeather = m_Rules.CanActorSeeSky(m_Player); // alpha10

            // roll & annouce new weather.
            string desc;
            Weather newWeather;
            switch (m_Session.World.Weather)
            {
                case Weather.CLEAR:
                    newWeather = Weather.CLOUDY;
                    desc = "Clouds are covering the sun.";
                    break;

                case Weather.CLOUDY:
                    if (m_Rules.RollChance(50))
                    {
                        newWeather = Weather.CLEAR;
                        desc = "The sky is clear again.";
                    }
                    else
                    {
                        newWeather = Weather.RAIN;
                        desc = "Rain is starting to fall.";
                    }
                    break;

                case Weather.RAIN:
                    if (m_Rules.RollChance(50))
                    {
                        newWeather = Weather.CLOUDY;
                        desc = "The rain has stopped.";
                    }
                    else
                    {
                        newWeather = Weather.HEAVY_RAIN;
                        desc = "The weather is getting worse!";
                    }
                    break;

                case Weather.HEAVY_RAIN:
                    newWeather = Weather.RAIN;
                    desc = "The rain is less heavy.";
                    break;

                default:
                    throw new ArgumentOutOfRangeException("unhandled weather");
            }

            // change.
            m_Session.World.Weather = newWeather;

            // message.
            if (canSeeWeather)
                AddMessage(new Message(desc, m_Session.WorldTime.TurnCounter, Color.White));

            // scoring.
            m_Session.Scoring.AddEvent(m_Session.WorldTime.TurnCounter, String.Format("The weather changed to {0}.", DescribeWeather(m_Session.World.Weather)));
        }

        /// <summary>
        /// Add kill to scoring record.
        /// </summary>
        /// <param name="victim"></param>
        void PlayerKill(Actor victim)
        {
            // scoring.
            m_Session.Scoring.AddKill(m_Player, victim, m_Session.WorldTime.TurnCounter);
        }
        #endregion

        #region Infection & Zombification
        void InfectActor(Actor actor, int addInfection)
        {
            actor.Infection = Math.Min(m_Rules.ActorInfectionHPs(actor), actor.Infection + addInfection);
        }

        /// <summary>
        /// Zombify an actor during the game or zombify the player at game start.
        /// </summary>
        /// <param name="zombifier"></param>
        /// <param name="deadVictim"></param>
        /// <param name="isStartingGame"></param>
        /// <returns></returns>
        Actor Zombify(Actor zombifier, Actor deadVictim, bool isStartingGame)
        {
            Actor newZombie = m_TownGenerator.MakeZombified(zombifier, deadVictim, isStartingGame ? 0 : deadVictim.Location.Map.LocalTime.TurnCounter);

            // add to map.
            if (!isStartingGame)
                deadVictim.Location.Map.PlaceActorAt(newZombie, deadVictim.Location.Position);

            // reset AP - dont act this turn.
            newZombie.ActionPoints = 0;

            // if zombifying player, remember it!
            if (deadVictim == m_Player || deadVictim.IsPlayer)
                m_Session.Scoring.SetZombifiedPlayer(newZombie);

            // keep half of the skills from living form at random.
            SkillTable livingSkills = deadVictim.Sheet.SkillTable;
            if (livingSkills != null && livingSkills.CountSkills > 0)
            {
                if (newZombie.Sheet.SkillTable == null)
                    newZombie.Sheet.SkillTable = new SkillTable();
                int nbLivingSkills = livingSkills.CountSkills;
                int nbSkillsToKeep = livingSkills.CountTotalSkillLevels / 2;
                for (int i = 0; i < nbSkillsToKeep; i++)
                {
                    Skills.IDs keepSkill = (Skills.IDs)livingSkills.SkillsList[m_Rules.Roll(0, nbLivingSkills)];
                    Skills.IDs? zombiefiedSkill = ZombifySkill(keepSkill);
                    if (zombiefiedSkill.HasValue)
                        SkillUpgrade(newZombie, zombiefiedSkill.Value);
                }
                m_TownGenerator.RecomputeActorStartingStats(newZombie);
            }

            // cause insanity.
            if (!isStartingGame)
                SeeingCauseInsanity(newZombie, newZombie.Location, Rules.SANITY_HIT_ZOMBIFY, String.Format("{0} turning into a zombie", deadVictim.Name));

            // done.
            return newZombie;
        }

        public Skills.IDs? ZombifySkill(Skills.IDs skill)
        {
            switch (skill)
            {
                case Skills.IDs.AGILE: return Skills.IDs.Z_AGILE;
                case Skills.IDs.LIGHT_EATER: return Skills.IDs.Z_LIGHT_EATER;
                case Skills.IDs.LIGHT_FEET: return Skills.IDs.Z_LIGHT_FEET;
                case Skills.IDs.MEDIC: return Skills.IDs.Z_INFECTOR;
                case Skills.IDs.STRONG: return Skills.IDs.Z_STRONG;
                case Skills.IDs.TOUGH: return Skills.IDs.Z_TOUGH;
                default: return null;
            }
        }
        #endregion

        #region Applying/Unapplying effects: ApplyXXX/UnapplyXXX

        /// <summary>
        /// Put the object on fire : firestate = onfire, jump -1.
        /// </summary>
        /// <param name="mapObj"></param>
        public void ApplyOnFire(MapObject mapObj)
        {
            // put object on fire.
            mapObj.FireState = MapObject.Fire.ONFIRE;
            // can't jump on it.
            --mapObj.JumpLevel;
        }

        /// <summary>
        /// Unapply fire effects. FIXME: need to distinguish Unapply (burnable again) vs PutOutFire (ashes)?
        /// </summary>
        /// <param name="mapObj"></param>
        public void UnapplyOnFire(MapObject mapObj)
        {
            // restore jumpability.
            ++mapObj.JumpLevel;
            // extinguish fire, burnable again.
            mapObj.FireState = MapObject.Fire.BURNABLE;
        }
        #endregion

        #region View & Drawing

        #region View
        public void ComputeViewRect(Point mapCenter)
        {
            int left = mapCenter.X - HALF_VIEW_WIDTH;
            int right = mapCenter.X + HALF_VIEW_WIDTH;

            int top = mapCenter.Y - HALF_VIEW_HEIGHT;
            int bottom = mapCenter.Y + HALF_VIEW_HEIGHT;

            m_MapViewRect = new Rectangle(left, top, 1 + right - left, 1 + bottom - top);
        }

        public bool IsInViewRect(Point mapPosition)
        {
            return m_MapViewRect.Contains(mapPosition);
        }
        #endregion

        public void RedrawPlayScreen()
        {
            // alpha10 dont display some infos
            bool canSeeSky = m_Rules.CanActorSeeSky(m_Player);
            bool canKnowTime = m_Rules.CanActorKnowTime(m_Player);

            // get mutex.
            Monitor.Enter(m_UI);

            m_UI.UI_Clear(Color.Black);
            {
                // map & minimap
                Color mapTint = Color.White; // disabled changing brightness bad for the eyes TintForDayPhase(m_Session.WorldTime.Phase);
                m_UI.UI_DrawLine(Color.DarkGray, RIGHTPANEL_X, 0, RIGHTPANEL_X, MESSAGES_Y);
                DrawMap(m_Session.CurrentMap, mapTint);

                m_UI.UI_DrawLine(Color.DarkGray, RIGHTPANEL_X, MINIMAP_Y - 4, CANVAS_WIDTH, MINIMAP_Y - 4);
                DrawMiniMap(m_Session.CurrentMap);

                // messages
                m_UI.UI_DrawLine(Color.DarkGray, MESSAGES_X, MESSAGES_Y - 1, CANVAS_WIDTH, MESSAGES_Y - 1);
                DrawMessages();

                // location info.
                #region
                //    x0            x1 
                // y0 <map name>
                // y1 <zone name>
                // y2 <day>        <dayphase>
                // y3 <hour>       <weather>/<lighting>
                // y4 <turn>       <scoring>@<difficulty> <mode>
                // y5 <life>/<lives>
                // y6 <murders>
                const int X0 = LOCATIONPANEL_TEXT_X;
                const int X1 = LOCATIONPANEL_TEXT_X + 128;
                const int Y0 = LOCATIONPANEL_TEXT_Y;
                const int Y1 = Y0 + LINE_SPACING;
                const int Y2 = Y1 + LINE_SPACING;
                const int Y3 = Y2 + LINE_SPACING;
                const int Y4 = Y3 + LINE_SPACING;
                const int Y5 = Y4 + LINE_SPACING;
                const int Y6 = Y5 + LINE_SPACING;

                m_UI.UI_DrawLine(Color.DarkGray, LOCATIONPANEL_X, LOCATIONPANEL_Y, LOCATIONPANEL_X, CANVAS_HEIGHT);
                m_UI.UI_DrawString(Color.White, m_Session.CurrentMap.Name, X0, Y0);
                m_UI.UI_DrawString(Color.White, LocationText(m_Session.CurrentMap, m_Player), X0, Y1);
                m_UI.UI_DrawString(Color.White, String.Format("Day  {0}", m_Session.WorldTime.Day), X0, Y2);
                if (canKnowTime)
                    m_UI.UI_DrawString(Color.White, String.Format("Hour {0}", m_Session.WorldTime.Hour), X0, Y3);
                else
                    m_UI.UI_DrawString(Color.White, "Hour ??", X0, Y3);

                // alpha10 desc day fov effect, not if cant know time
                string dayPhaseString;
                if (canKnowTime)
                {
                    dayPhaseString = DescribeDayPhase(m_Session.WorldTime.Phase);
                    int timeFovPenalty = m_Rules.NightFovPenalty(m_Player, m_Session.WorldTime);
                    if (timeFovPenalty != 0)
                        dayPhaseString += "  fov -" + timeFovPenalty;
                }
                else
                {
                    dayPhaseString = "???";
                }

                m_UI.UI_DrawString(m_Session.WorldTime.IsNight ? NIGHT_COLOR : DAY_COLOR, dayPhaseString, X1, Y2);

                Color weatherOrLightingColor;
                string weatherOrLightingString;
                switch (m_Session.CurrentMap.Lighting)
                {
                    case Lighting.OUTSIDE:
                        weatherOrLightingColor = WeatherColor(m_Session.World.Weather);
                        // alpha10 only show weather if can see it
                        if (m_Rules.CanActorSeeSky(m_Player))
                        {
                            weatherOrLightingString = DescribeWeather(m_Session.World.Weather);
                            // alpha10 desc weather fov effect
                            int fovPenalty = m_Rules.WeatherFovPenalty(m_Player, m_Session.World.Weather);
                            if (fovPenalty != 0)
                                weatherOrLightingString += "  fov -" + fovPenalty;
                        }
                        else
                            weatherOrLightingString = "???";
                        break;
                    case Lighting.DARKNESS:
                        weatherOrLightingColor = Color.Blue;
                        weatherOrLightingString = "Darkness";
                        // alpha10 desc darkness fov effect
                        int darknessFov = m_Rules.DarknessFov(m_Player);
                        if (darknessFov != m_Player.Sheet.BaseViewRange)
                            weatherOrLightingString += "  fov " + darknessFov;
                        break;
                    case Lighting.LIT:
                        weatherOrLightingColor = Color.Yellow;
                        weatherOrLightingString = "Lit";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("unhandled lighting");
                }
                m_UI.UI_DrawString(weatherOrLightingColor, weatherOrLightingString, X1, Y3);
                m_UI.UI_DrawString(Color.White, String.Format("Turn {0}", m_Session.WorldTime.TurnCounter), X0, Y4);
                m_UI.UI_DrawString(Color.White, String.Format("Score   {0}@{1}% {2}", m_Session.Scoring.TotalPoints, (int)(100 * Scoring.ComputeDifficultyRating(s_Options, m_Session.Scoring.Side, m_Session.Scoring.ReincarnationNumber)), Session.DescShortGameMode(m_Session.GameMode)), X1, Y4);
                m_UI.UI_DrawString(Color.White, String.Format("Avatar  {0}/{1}", (1 + m_Session.Scoring.ReincarnationNumber), (1 + s_Options.MaxReincarnations)), X1, Y5);
                if (m_Player.MurdersCounter > 0)
                    m_UI.UI_DrawString(Color.White, String.Format("Murders {0}", m_Player.MurdersCounter), X1, Y6);
                #endregion

                // character status.
                if (m_Player != null)
                    DrawActorStatus(m_Player, RIGHTPANEL_TEXT_X, RIGHTPANEL_TEXT_Y);

                // inventories.
                if (m_Player != null)
                {
                    if (m_Player.Inventory != null && m_Player.Model.Abilities.HasInventory)
                        DrawInventory(m_Player.Inventory, "Inventory", true, INVENTORY_SLOTS_PER_LINE, m_Player.Inventory.MaxCapacity, INVENTORYPANEL_X, INVENTORYPANEL_Y);
                    DrawInventory(m_Player.Location.Map.GetItemsAt(m_Player.Location.Position), "Items on ground", true, INVENTORY_SLOTS_PER_LINE, Map.GROUND_INVENTORY_SLOTS, INVENTORYPANEL_X, GROUNDINVENTORYPANEL_Y);
                    DrawCorpsesList(m_Player.Location.Map.GetCorpsesAt(m_Player.Location.Position), "Corpses on ground", INVENTORY_SLOTS_PER_LINE, INVENTORYPANEL_X, CORPSESPANEL_Y);
                }

                // character skills.
                if (m_Player != null && m_Player.Sheet.SkillTable != null && m_Player.Sheet.SkillTable.CountSkills > 0)
                    DrawActorSkillTable(m_Player, RIGHTPANEL_TEXT_X, SKILLTABLE_Y);

                // overlays
                Monitor.Enter(m_Overlays);
                foreach (Overlay o in m_Overlays)
                    o.Draw(m_UI);
                Monitor.Exit(m_Overlays);

                // DEV STATS
#if DEBUG
                if (s_Options.DEV_ShowActorsStats)
                {
                    int countLiving, countUndead;
                    countLiving = CountLivings(m_Session.CurrentMap);
                    countUndead = CountUndeads(m_Session.CurrentMap);
                    m_UI.UI_DrawString(Color.White, String.Format("Living {0} vs {1} Undead", countLiving, countUndead), RIGHTPANEL_TEXT_X, SKILLTABLE_Y - 32);
                }
#endif
            }

            m_UI.UI_Repaint();

            // release mutex.
            Monitor.Exit(m_UI);
        }

        string LocationText(Map map, Actor actor)
        {
            if (map == null || actor == null)
                return "";

            StringBuilder sb = new StringBuilder(String.Format("({0},{1}) ", actor.Location.Position.X, actor.Location.Position.Y));

            List<Zone> zones = map.GetZonesAt(actor.Location.Position.X, actor.Location.Position.Y);
            if (zones == null || zones.Count == 0)
                return sb.ToString();

            foreach (Zone z in zones)
                sb.Append(String.Format("{0} ", z.Name));

            return sb.ToString();
        }

        /// <summary>
        /// OBSOLETE
        /// </summary>
        /// <param name="phase"></param>
        /// <returns></returns>
        Color TintForDayPhase(DayPhase phase)
        {
            switch (phase)
            {
                case DayPhase.MORNING:
                case DayPhase.MIDDAY:
                case DayPhase.AFTERNOON:
                    return TINT_DAY;

                case DayPhase.SUNRISE:
                    return TINT_SUNRISE;

                case DayPhase.SUNSET:
                    return TINT_SUNSET;

                case DayPhase.MIDNIGHT:
                    return TINT_MIDNIGHT;

                case DayPhase.DEEP_NIGHT:
                    return TINT_NIGHT;

                case DayPhase.EVENING:
                    return TINT_EVENING;
                default:
                    throw new ArgumentOutOfRangeException("unhandled dayphase");
            }
        }

        #region Drawing elements
        public void DrawMap(Map map, Color tint)
        {
            // trim to outer map bounds.
            int left = Math.Max(-1, m_MapViewRect.Left);
            int right = Math.Min(map.Width + 1, m_MapViewRect.Right);
            int top = Math.Max(-1, m_MapViewRect.Top);
            int bottom = Math.Min(map.Height + 1, m_MapViewRect.Bottom);

            // get weather image.
            string weatherImage;
            switch (m_Session.World.Weather)
            {
                case Weather.RAIN:
                    weatherImage = (m_Session.WorldTime.TurnCounter % 2 == 0 ? GameImages.WEATHER_RAIN1 : GameImages.WEATHER_RAIN2);
                    break;
                case Weather.HEAVY_RAIN:
                    weatherImage = (m_Session.WorldTime.TurnCounter % 2 == 0 ? GameImages.WEATHER_HEAVY_RAIN1 : GameImages.WEATHER_HEAVY_RAIN2);
                    break;

                default:
                    weatherImage = null;
                    break;
            }

            ///////////////////////////////////////////
            // Layered draw:
            // 1. Tiles.
            // 2. Corpses.
            // 3. (Target statut), Map objects.
            // 4. Scents.
            // 5. Items, Actors (if visible).
            // 6. Water cover.
            // 7. Weather (if visible and not inside).
            ///////////////////////////////////////////
            Point position = new Point();
            bool isUndead = m_Player.Model.Abilities.IsUndead;
            bool hasSmell = m_Player.Model.StartingSheet.BaseSmellRating > 0;
            int playerSmellTheshold = m_Rules.ActorSmellThreshold(m_Player);
            for (int x = left; x < right; x++)
            {
                position.X = x;
                for (int y = top; y < bottom; y++)
                {
                    position.Y = y;
                    Point toScreen = MapToScreen(x, y);
                    bool isVisible = IsVisibleToPlayer(map, position);
                    bool drawWater = false;
                    Tile tile = map.IsInBounds(x, y) ? map.GetTileAt(x, y) : null;

                    // 1. Tile
                    if (map.IsInBounds(x, y))
                        DrawTile(tile, toScreen, tint);
                    else if (map.IsMapBoundary(x, y))
                    {
                        if (map.GetExitAt(position) != null)
                            DrawExit(toScreen);
                    }
#if DEBUG
                    DrawTileDev(map, tile, x, y, toScreen);
#endif

                    // 2. Corpses
                    if (isVisible)
                    {
                        List<Corpse> corpses = map.GetCorpsesAt(x, y);
                        if (corpses != null)
                        {
                            foreach (Corpse c in corpses)
                                DrawCorpse(c, toScreen.X, toScreen.Y, tint);
                        }
                    }

                    // 3. (TargetStatus), Map objects
                    if (s_Options.ShowPlayerTargets && !m_Player.IsSleeping && m_Player.Location.Position == position)
                        DrawPlayerActorTargets(m_Player);
                    MapObject mapObj = map.GetMapObjectAt(x, y);
                    if (mapObj != null)
                    {
                        DrawMapObject(mapObj, toScreen, tint);
                        drawWater = true;
                    }

                    // 4. Scents
                    #region
                    if (!m_Player.IsSleeping && map.IsInBounds(x, y) && m_Rules.GridDistance(m_Player.Location.Position, position) <= 1)
                    {
                        // scents alpha is low to be able to see objects behind them (eg: scent on a door)
                        // squaring alpha helps increase discrimination for player.

                        if (isUndead)
                        {
                            // Undead can see living & zm scents.
                            if (hasSmell)
                            {
                                // living scent?
                                int livingScent = map.GetScentByOdorAt(Odor.LIVING, position);
                                if (livingScent >= playerSmellTheshold)
                                {
                                    float alpha = 0.90f * (float)livingScent / (float)OdorScent.MAX_STRENGTH;
                                    alpha *= alpha;
                                    m_UI.UI_DrawTransparentImage(alpha, GameImages.ICON_SCENT_LIVING, toScreen.X, toScreen.Y);
                                }

                                // zombie master scent?
                                int masterScent = map.GetScentByOdorAt(Odor.UNDEAD_MASTER, position);
                                if (masterScent >= playerSmellTheshold)
                                {
                                    float alpha = 0.90f * (float)masterScent / (float)OdorScent.MAX_STRENGTH;
                                    alpha *= alpha;
                                    m_UI.UI_DrawTransparentImage(alpha, GameImages.ICON_SCENT_ZOMBIEMASTER, toScreen.X, toScreen.Y);
                                }
                            }
                        }
                        else
                        {
                            // Living can see some perfumes.
                            // alpha10 obsolete // perfume: living suppressor?
                            //int livingSupr = map.GetScentByOdorAt(Odor.PERFUME_LIVING_SUPRESSOR, position);
                            //if (livingSupr > 0)
                            //{
                            //    float alpha = 0.90f * (float)livingSupr / (float)OdorScent.MAX_STRENGTH;
                            //    //alpha *= alpha;
                            //    m_UI.UI_DrawTransparentImage(alpha, GameImages.ICON_SCENT_LIVING_SUPRESSOR, toScreen.X, toScreen.Y);
                            //}
                        }
                    }
                    #endregion

                    // 5. Items, Actors (if visible)
                    if (isVisible)
                    {
                        // 4.2. Items
                        Inventory inv = map.GetItemsAt(x, y);
                        if (inv != null)
                        {
                            DrawItemsStack(inv, toScreen.X, toScreen.Y, tint);
                            drawWater = true;
                        }

                        // 4.3. Actors
                        Actor actor = map.GetActorAt(x, y);
                        if (actor != null)
                        {
                            DrawActorSprite(actor, toScreen, tint);
                            drawWater = true;
                        }
                    }

                    // 6. Water cover.
                    if (tile != null && tile.HasDecorations)
                        drawWater = true;
                    if (drawWater && tile.Model.IsWater)
                        DrawTileWaterCover(tile, toScreen, tint);


                    // 7. Weather (if visible and not inside).
                    if (isVisible && weatherImage != null && tile != null && !tile.IsInside)
                        m_UI.UI_DrawImage(weatherImage, toScreen.X, toScreen.Y);
                }
            }

            // DEV: scents
#if false
            for (int x = left; x < right; x++)
                for (int y = top; y < bottom; y++)
                {
                    if (map.IsInBounds(x, y))
                    {
                        int scent = map.GetScentByOdorAt(Odor.LIVING, new Point(x, y));
                        if (scent > 0)
                        {
                            m_UI.UI_DrawString(Color.White, String.Format("{0}", scent), MapToScreen(x, y).X, MapToScreen(x, y).Y);
                        }
                    }
                }
#endif
        }

        string MovingWaterImage(TileModel model, int turnCount)
        {
            if (model == m_GameTiles.FLOOR_SEWER_WATER)
            {
                int i = turnCount % 3;
                switch (i)
                {
                    case 0: return GameImages.TILE_FLOOR_SEWER_WATER_ANIM1;
                    case 1: return GameImages.TILE_FLOOR_SEWER_WATER_ANIM2;
                    default: return GameImages.TILE_FLOOR_SEWER_WATER_ANIM3;
                }
            }

            return null;
        }

        public void DrawTile(Tile tile, Point screen, Color tint)
        {
            if (tile.IsInView)  // visible
            {
                // tile.
                m_UI.UI_DrawImage(tile.Model.ImageID, screen.X, screen.Y, tint);

                // animation layer.
                string movingWater = MovingWaterImage(tile.Model, m_Session.WorldTime.TurnCounter);
                if (movingWater != null)
                    m_UI.UI_DrawImage(movingWater, screen.X, screen.Y, tint);

                // decorations.
                if (tile.HasDecorations)
                    foreach (string deco in tile.Decorations)
                        m_UI.UI_DrawImage(deco, screen.X, screen.Y, tint);
            }
            else if (tile.IsVisited && !IsPlayerSleeping()) // memorized
            {
                // tile.
                m_UI.UI_DrawGrayLevelImage(tile.Model.ImageID, screen.X, screen.Y);

                // animation layer.
                string movingWater = MovingWaterImage(tile.Model, m_Session.WorldTime.TurnCounter);
                if (movingWater != null)
                    m_UI.UI_DrawGrayLevelImage(movingWater, screen.X, screen.Y);

                // deocrations.
                if (tile.HasDecorations)
                    foreach (string deco in tile.Decorations)
                        m_UI.UI_DrawGrayLevelImage(deco, screen.X, screen.Y);
            }
        }

        public void DrawTileWaterCover(Tile tile, Point screen, Color tint)
        {
            if (tile.IsInView)  // visible
            {
                // tile.
                m_UI.UI_DrawImage(tile.Model.WaterCoverImageID, screen.X, screen.Y, tint);
            }
            else if (tile.IsVisited && !IsPlayerSleeping()) // memorized
            {
                // tile.
                m_UI.UI_DrawGrayLevelImage(tile.Model.WaterCoverImageID, screen.X, screen.Y);
            }
        }

        public void DrawExit(Point screen)
        {
            m_UI.UI_DrawImage(GameImages.MAP_EXIT, screen.X, screen.Y);
        }

        public void DrawTileRectangle(Point mapPosition, Color color)
        {
            m_UI.UI_DrawRect(color, new Rectangle(MapToScreen(mapPosition), new Size(TILE_SIZE, TILE_SIZE)));
        }

        public void DrawMapObject(MapObject mapObj, Point screen, Color tint)
        {
            // pushables objects in water floating animation.
            if (mapObj.IsMovable && mapObj.Location.Map.GetTileAt(mapObj.Location.Position.X, mapObj.Location.Position.Y).Model.IsWater)
            {
                int yDrift = (mapObj.Location.Position.X + m_Session.WorldTime.TurnCounter) % 2 == 0 ? -2 : 0;
                screen.Y -= yDrift;
            }

            if (IsVisibleToPlayer(mapObj))
            {
                DrawMapObject(mapObj, screen, mapObj.ImageID, (imageID, gx, gy) => m_UI.UI_DrawImage(imageID, gx, gy, tint));

                if (mapObj.HitPoints < mapObj.MaxHitPoints && mapObj.HitPoints > 0)
                    DrawMapHealthBar(mapObj.HitPoints, mapObj.MaxHitPoints, screen.X, screen.Y);

                DoorWindow door = mapObj as DoorWindow;
                if (door != null && door.BarricadePoints > 0)
                {
                    DrawMapHealthBar(door.BarricadePoints, Rules.BARRICADING_MAX, screen.X, screen.Y, Color.Green);
                    m_UI.UI_DrawImage(GameImages.EFFECT_BARRICADED, screen.X, screen.Y, tint);
                }
            }
            else if (IsKnownToPlayer(mapObj) && !IsPlayerSleeping())
            {
                DrawMapObject(mapObj, screen, mapObj.HiddenImageID, (imageID, gx, gy) => m_UI.UI_DrawGrayLevelImage(imageID, gx, gy));
            }
        }

        void DrawMapObject(MapObject mapObj, Point screen, string imageID, Action<string, int, int> drawFn)
        {
            // draw image.
            drawFn(imageID, screen.X, screen.Y);

            // draw effects.
            if (mapObj.IsOnFire)
                drawFn(GameImages.EFFECT_ONFIRE, screen.X, screen.Y);
        }

        public void DrawActorSprite(Actor actor, Point screen, Color tint)
        {
            int gx = screen.X;
            int gy = screen.Y;

            // player follower?
            if (actor.Leader != null && actor.Leader == m_Player)
            {
                if (m_Rules.HasActorBondWith(actor, m_Player))
                    m_UI.UI_DrawImage(GameImages.PLAYER_FOLLOWER_BOND, gx, gy, tint);
                else if (m_Rules.IsActorTrustingLeader(actor))
                    m_UI.UI_DrawImage(GameImages.PLAYER_FOLLOWER_TRUST, gx, gy, tint);
                else
                    m_UI.UI_DrawImage(GameImages.PLAYER_FOLLOWER, gx, gy, tint);
            }

            gx += ACTOR_OFFSET;
            gy += ACTOR_OFFSET;

            // model
            if (actor.Model.ImageID != null)
                m_UI.UI_DrawImage(actor.Model.ImageID, gx, gy, tint);

            // skinning/clothing and body equipment.
            DrawActorDecoration(actor, gx, gy, DollPart.SKIN, tint);
            DrawActorDecoration(actor, gx, gy, DollPart.FEET, tint);
            DrawActorDecoration(actor, gx, gy, DollPart.LEGS, tint);
            DrawActorDecoration(actor, gx, gy, DollPart.TORSO, tint);
            DrawActorDecoration(actor, gx, gy, DollPart.TORSO, tint);
            if (actor.GetEquippedItem(DollPart.TORSO) != null)
                DrawActorEquipment(actor, gx - ACTOR_OFFSET, gy - ACTOR_OFFSET, DollPart.TORSO, tint);
            DrawActorDecoration(actor, gx, gy, DollPart.EYES, tint);
            DrawActorDecoration(actor, gx, gy, DollPart.HEAD, tint);

            // hands equipment
            DrawActorEquipment(actor, gx - ACTOR_OFFSET, gy - ACTOR_OFFSET, DollPart.LEFT_HAND, tint);
            DrawActorEquipment(actor, gx - ACTOR_OFFSET, gy - ACTOR_OFFSET, DollPart.RIGHT_HAND, tint);

            gx -= ACTOR_OFFSET;
            gy -= ACTOR_OFFSET;

            // personal enemy?
            if (m_Player != null)
            {
                bool imSelfDefence = m_Player.IsSelfDefenceFrom(actor);
                bool imTheAggressor = m_Player.IsAggressorOf(actor);
                bool groupEnemies = !m_Player.Faction.IsEnemyOf(actor.Faction) && m_Rules.AreGroupEnemies(m_Player, actor); // alpha10
                if (imSelfDefence)
                    m_UI.UI_DrawImage(GameImages.ICON_SELF_DEFENCE, gx, gy, tint);
                else if (imTheAggressor)
                    m_UI.UI_DrawImage(GameImages.ICON_AGGRESSOR, gx, gy, tint);
                else if (groupEnemies)
                    m_UI.UI_DrawImage(GameImages.ICON_INDIRECT_ENEMIES, gx, gy, tint);
            }

            // activity
            #region
            switch (actor.Activity)
            {
                case Activity.IDLE:
                    break;

                case Activity.CHASING:
                case Activity.FIGHTING:
                    if (actor.IsPlayer)
                        break;
                    if (actor.TargetActor == null)
                        break;

                    if (actor.TargetActor != null && actor.TargetActor == m_Player)
                        m_UI.UI_DrawImage(GameImages.ACTIVITY_CHASING_PLAYER, gx, gy, tint);
                    else
                        m_UI.UI_DrawImage(GameImages.ACTIVITY_CHASING, gx, gy, tint);
                    break;

                case Activity.TRACKING:
                    if (actor.IsPlayer)
                        break;

                    m_UI.UI_DrawImage(GameImages.ACTIVITY_TRACKING, gx, gy, tint);
                    break;

                case Activity.FLEEING:
                    if (actor.IsPlayer)
                        break;

                    m_UI.UI_DrawImage(GameImages.ACTIVITY_FLEEING, gx, gy, tint);
                    break;

                case Activity.FLEEING_FROM_EXPLOSIVE:
                    if (actor.IsPlayer)
                        break;

                    m_UI.UI_DrawImage(GameImages.ACTIVITY_FLEEING_FROM_EXPLOSIVE, gx, gy, tint);
                    break;

                case Activity.FOLLOWING:
                    if (actor.IsPlayer)
                        break;
                    if (actor.TargetActor == null)
                        break;

                    if (actor.TargetActor.IsPlayer)
                        m_UI.UI_DrawImage(GameImages.ACTIVITY_FOLLOWING_PLAYER, gx, gy);
                    else if (actor.TargetActor == actor.Leader) // alpha10
                        m_UI.UI_DrawImage(GameImages.ACTIVITY_FOLLOWING_LEADER, gx, gy);
                    else
                        m_UI.UI_DrawImage(GameImages.ACTIVITY_FOLLOWING, gx, gy);
                    break;

                case Activity.FOLLOWING_ORDER:
                    m_UI.UI_DrawImage(GameImages.ACTIVITY_FOLLOWING_ORDER, gx, gy);
                    break;

                case Activity.SLEEPING:
                    m_UI.UI_DrawImage(GameImages.ACTIVITY_SLEEPING, gx, gy);
                    break;

                default:
                    throw new InvalidOperationException("unhandled activity " + actor.Activity);
            }
            #endregion

            // health bar.
            int maxHP = m_Rules.ActorMaxHPs(actor);
            if (actor.HitPoints < maxHP)
            {
                DrawMapHealthBar(actor.HitPoints, maxHP, gx, gy);
            }

            // run/tired icon.
            #region
            if (actor.IsRunning)
                m_UI.UI_DrawImage(GameImages.ICON_RUNNING, gx, gy, tint);
            else if (actor.Model.Abilities.CanRun && !m_Rules.CanActorRun(actor))
                m_UI.UI_DrawImage(GameImages.ICON_CANT_RUN, gx, gy, tint);
            #endregion

            // sleepy, hungry & insane icons.
            #region
            if (actor.Model.Abilities.HasToSleep)
            {
                if (m_Rules.IsActorExhausted(actor))
                    m_UI.UI_DrawImage(GameImages.ICON_SLEEP_EXHAUSTED, gx, gy, tint);
                else if (m_Rules.IsActorSleepy(actor))
                    m_UI.UI_DrawImage(GameImages.ICON_SLEEP_SLEEPY, gx, gy, tint);
                else if (m_Rules.IsAlmostSleepy(actor))
                    m_UI.UI_DrawImage(GameImages.ICON_SLEEP_ALMOST_SLEEPY, gx, gy, tint);
            }

            if (actor.Model.Abilities.HasToEat)
            {
                if (m_Rules.IsActorStarving(actor))
                    m_UI.UI_DrawImage(GameImages.ICON_FOOD_STARVING, gx, gy, tint);
                else if (m_Rules.IsActorHungry(actor))
                    m_UI.UI_DrawImage(GameImages.ICON_FOOD_HUNGRY, gx, gy, tint);
                else if (IsAlmostHungry(actor))
                    m_UI.UI_DrawImage(GameImages.ICON_FOOD_ALMOST_HUNGRY, gx, gy, tint);
            }
            else if (actor.Model.Abilities.IsRotting)
            {
                if (m_Rules.IsRottingActorStarving(actor))
                    m_UI.UI_DrawImage(GameImages.ICON_ROT_STARVING, gx, gy, tint);
                else if (m_Rules.IsRottingActorHungry(actor))
                    m_UI.UI_DrawImage(GameImages.ICON_ROT_HUNGRY, gx, gy, tint);
                else if (IsAlmostRotHungry(actor))
                    m_UI.UI_DrawImage(GameImages.ICON_ROT_ALMOST_HUNGRY, gx, gy, tint);
            }

            if (actor.Model.Abilities.HasSanity)
            {
                if (m_Rules.IsActorInsane(actor))
                    m_UI.UI_DrawImage(GameImages.ICON_SANITY_INSANE, gx, gy, tint);
                else if (m_Rules.IsActorDisturbed(actor))
                    m_UI.UI_DrawImage(GameImages.ICON_SANITY_DISTURBED, gx, gy, tint);
            }
            #endregion

            // can trade with player icon.
            // alpha10.1 or has needed item (not for undead player duh)
            if (m_Player != null)
            {
                if (actor != m_Player && !m_Player.Model.Abilities.IsUndead && ActorHasVitalItemForPlayer(actor))
                    m_UI.UI_DrawImage(GameImages.ICON_HAS_VITAL_ITEM, gx, gy, tint);
                else if (m_Rules.CanActorInitiateTradeWith(m_Player, actor))
                    m_UI.UI_DrawImage(GameImages.ICON_CAN_TRADE, gx, gy, tint);
            }

            // alpha10 odor suppressed icon (will overlap with sleep healing but its fine)
            if (actor.OdorSuppressorCounter > 0)
                m_UI.UI_DrawImage(GameImages.ICON_ODOR_SUPPRESSED, gx, gy, tint);

            // sleep-healing icon.
            if (actor.IsSleeping && (m_Rules.IsOnCouch(actor) || m_Rules.ActorHealChanceBonus(actor) > 0))
                m_UI.UI_DrawImage(GameImages.ICON_HEALING, gx, gy, tint);

            // is a leader icon.
            if (actor.CountFollowers > 0)
                m_UI.UI_DrawImage(GameImages.ICON_LEADER, gx, gy, tint);

            // alpha10
            // z-grab skill warning icon
            if (actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.Z_GRAB) > 0)
                m_UI.UI_DrawImage(GameImages.ICON_ZGRAB, gx, gy, tint);

            // combat assitant helper.
            if (s_Options.IsCombatAssistantOn)
            {
                if (actor != m_Player && m_Player != null && m_Rules.AreEnemies(actor, m_Player))
                {
                    if (m_Rules.WillActorActAgainBefore(m_Player, actor))
                        m_UI.UI_DrawImage(GameImages.ICON_THREAT_SAFE, gx, gy, tint);
                    else if (m_Rules.WillOtherActTwiceBefore(m_Player, actor))
                        m_UI.UI_DrawImage(GameImages.ICON_THREAT_HIGH_DANGER, gx, gy, tint);
                    else
                        m_UI.UI_DrawImage(GameImages.ICON_THREAT_DANGER, gx, gy, tint);
                }
            }
        }

        // alpha10.1
        /// <summary>
        /// Checks if npc has a vital item for the player :
        /// - Food if player is hungry
        /// - Anti-sleep meds if player is sleepy
        /// - Healing meds if player injured
        /// - Curing meds if player infected
        /// </summary>
        /// <param name="actor"></param>
        /// <returns></returns>
        private bool ActorHasVitalItemForPlayer(Actor actor)
        {
            if (actor.Inventory == null)
                return false;
            if (actor.Inventory.IsEmpty)
                return false;

            // hungry -> food
            if (m_Rules.IsActorHungry(m_Player)
                && actor.Inventory.HasItemOfType(typeof(ItemFood)))
                return true;

            // sleepy -> anti-sleep meds
            if (m_Rules.IsActorSleepy(m_Player)
                && actor.Inventory.HasItemMatching((it) => it is ItemMedicine && (it as ItemMedicine).SleepBoost > 0))
                return true;

            // injured -> healing meds
            if (m_Player.HitPoints < m_Rules.ActorMaxHPs(m_Player)
                && actor.Inventory.HasItemMatching((it) => it is ItemMedicine && (it as ItemMedicine).Healing > 0))
                return true;

            // infected -> curing meds
            if (m_Player.Infection > 0
                && actor.Inventory.HasItemMatching((it) => it is ItemMedicine && (it as ItemMedicine).InfectionCure > 0))
                return true;

            // no vital items
            return false;
        }

        public void DrawActorDecoration(Actor actor, int gx, int gy, DollPart part, Color tint)
        {
            List<string> decos = actor.Doll.GetDecorations(part);
            if (decos == null)
                return;

            foreach (string imageID in decos)
                m_UI.UI_DrawImage(imageID, gx, gy, tint);
        }

        public void DrawActorDecoration(Actor actor, int gx, int gy, DollPart part, float rotation, float scale)
        {
            List<string> decos = actor.Doll.GetDecorations(part);
            if (decos == null)
                return;

            foreach (string imageID in decos)
                m_UI.UI_DrawImageTransform(imageID, gx, gy, rotation, scale);
        }

        public void DrawActorEquipment(Actor actor, int gx, int gy, DollPart part, Color tint)
        {
            Item it = actor.GetEquippedItem(part);
            if (it == null)
                return;

            m_UI.UI_DrawImage(it.ImageID, gx, gy, tint);
        }

        public void DrawCorpse(Corpse c, int gx, int gy, Color tint)
        {
            float rotation = c.Rotation;
            float scale = c.Scale;
            int offset = 0;// TILE_SIZE / 2;

            Actor actor = c.DeadGuy;

            gx += ACTOR_OFFSET + offset;
            gy += ACTOR_OFFSET + offset;

            // model.
            if (actor.Model.ImageID != null)
                m_UI.UI_DrawImageTransform(actor.Model.ImageID, gx, gy, rotation, scale);

            // skinning/clothing.
            DrawActorDecoration(actor, gx, gy, DollPart.SKIN, rotation, scale);
            DrawActorDecoration(actor, gx, gy, DollPart.FEET, rotation, scale);
            DrawActorDecoration(actor, gx, gy, DollPart.LEGS, rotation, scale);
            DrawActorDecoration(actor, gx, gy, DollPart.TORSO, rotation, scale);
            DrawActorDecoration(actor, gx, gy, DollPart.TORSO, rotation, scale);
            DrawActorDecoration(actor, gx, gy, DollPart.EYES, rotation, scale);
            DrawActorDecoration(actor, gx, gy, DollPart.HEAD, rotation, scale);

            gx -= ACTOR_OFFSET + offset;
            gy -= ACTOR_OFFSET + offset;

            // rotting.
            int rotLevel = Rules.CorpseRotLevel(c);
            string img = null;
            switch (rotLevel)
            {
                case 5:
                case 4:
                case 3:
                case 2:
                case 1: img = "rot" + rotLevel + "_"; break;
                case 0: break;
                default: throw new Exception("unhandled rot level");
            }
            if (img != null)
            {
                // anim frame.
                img += 1 + (m_Session.WorldTime.TurnCounter % 2);
                // a bit of offset for a nice flies movement effect.
                int rotdx = (m_Session.WorldTime.TurnCounter % 5) - 2;
                int rotdy = ((m_Session.WorldTime.TurnCounter / 3) % 5) - 2;
                m_UI.UI_DrawImage(img, gx + rotdx, gy + rotdy);
            }
        }

        public void DrawCorpsesList(List<Corpse> list, string title, int slots, int gx, int gy)
        {
            int x, y;
            int slot = 0;

            // Draw title.
            int n = (list == null ? 0 : list.Count);
            if (n > 0) title += " : " + n;
            gy -= BOLD_LINE_SPACING;
            m_UI.UI_DrawStringBold(Color.White, title, gx, gy);
            gy += BOLD_LINE_SPACING;

            // Draw slots.
            x = gx; y = gy; slot = 0;
            for (int i = 0; i < slots; i++)
            {
                m_UI.UI_DrawImage(GameImages.ITEM_SLOT, x, y);
                x += TILE_SIZE;
            }

            // Draw corpses.
            if (list == null)
                return;

            x = gx; y = gy; slot = 0;
            foreach (Corpse c in list)
            {
                if (c.IsDragged)
                    m_UI.UI_DrawImage(GameImages.CORPSE_DRAGGED, x, y);
                DrawCorpse(c, x, y, Color.White);
                if (++slot >= slots)
                    break;
                else
                    x += TILE_SIZE;
            }
        }

        // alpha10
        /// <summary>
        /// Highlight with overlays which visible actors are
        /// - are the target of this actor 
        /// - targeting this actor
        /// - in group with this actor
        /// </summary>
        /// <param name="actor"></param>
        public void DrawActorRelations(Actor actor)
        {
            Point offset = new Point(TILE_SIZE / 2, TILE_SIZE / 2);

            // target of this actor
            if (actor.TargetActor != null && !actor.TargetActor.IsDead && IsVisibleToPlayer(actor.TargetActor))
                AddOverlay(new OverlayImage(MapToScreen(actor.TargetActor.Location.Position), GameImages.ICON_IS_TARGET));

            // actors targeting this actor or in same group
            bool isTargettedHighlighted = false;
            foreach (Actor other in actor.Location.Map.Actors)
            {
                if (other == actor || other.IsDead || !IsVisibleToPlayer(other))
                    continue;

                // targetting this actor
                if (other.TargetActor == actor && (other.Activity == Activity.CHASING || other.Activity == Activity.FIGHTING))
                {
                    if (!isTargettedHighlighted)
                    {
                        AddOverlay(new OverlayImage(MapToScreen(actor.Location.Position), GameImages.ICON_IS_TARGETTED));
                        isTargettedHighlighted = true;
                    }
                    AddOverlay(new OverlayImage(MapToScreen(other.Location.Position), GameImages.ICON_IS_TARGETING));
                }

                // in group with actor
                if (other.IsInGroupWith(actor))
                    AddOverlay(new OverlayImage(MapToScreen(other.Location.Position), GameImages.ICON_IS_IN_GROUP));
            }
        }

        /// <summary>
        /// immediate mode
        /// </summary>
        /// <param name="player"></param>
        public void DrawPlayerActorTargets(Actor player)
        {
            Point offset = new Point(TILE_SIZE / 2, TILE_SIZE / 2);

            if (player.TargetActor != null && !player.TargetActor.IsDead && IsVisibleToPlayer(player.TargetActor))
            {
                Point gpos = MapToScreen(player.TargetActor.Location.Position);
                m_UI.UI_DrawImage(GameImages.ICON_IS_TARGET, gpos.X, gpos.Y);
            }
            foreach (Actor a in player.Location.Map.Actors)
            {
                if (a == player || a.IsDead || !IsVisibleToPlayer(a))
                    continue;
                if (a.TargetActor == player && (a.Activity == Activity.CHASING || a.Activity == Activity.FIGHTING))
                {
                    Point gpos = MapToScreen(player.Location.Position);
                    m_UI.UI_DrawImage(GameImages.ICON_IS_TARGETTED, gpos.X, gpos.Y);
                    break;
                }
            }
        }

        public void DrawItemsStack(Inventory inventory, int gx, int gy, Color tint)
        {
            if (inventory == null)
                return;

            foreach (Item it in inventory.Items)
                DrawItem(it, gx, gy, tint);
        }

        public void DrawMapIcon(Point position, string imageID)
        {
            m_UI.UI_DrawImage(imageID, position.X * RogueGame.TILE_SIZE, position.Y * RogueGame.TILE_SIZE);
        }

        public void DrawMapHealthBar(int hitPoints, int maxHitPoints, int gx, int gy)
        {
            DrawMapHealthBar(hitPoints, maxHitPoints, gx, gy, Color.Red);
        }

        public void DrawMapHealthBar(int hitPoints, int maxHitPoints, int gx, int gy, Color barColor)
        {
            int hpX = gx + 4;
            int hpY = gy + TILE_SIZE - 4;
            int barLength = (int)(20 * (float)hitPoints / (float)maxHitPoints);
            m_UI.UI_FillRect(Color.Black, new Rectangle(hpX, hpY, 20, 4));
            if (barLength > 0)
                m_UI.UI_FillRect(barColor, new Rectangle(hpX + 1, hpY + 1, barLength, 2));

        }

        public void DrawBar(int value, int previousValue, int maxValue, int refValue, int maxWidth, int height, int gx, int gy,
            Color fillColor, Color lossFillColor, Color gainFillColor, Color emptyColor)
        {
            m_UI.UI_FillRect(emptyColor, new Rectangle(gx, gy, maxWidth, height));

            int prevBarLength = (int)(maxWidth * (float)previousValue / (float)maxValue);
            int barLength = (int)(maxWidth * (float)value / (float)maxValue);

            if (value > previousValue)
            {
                // gain
                if (barLength > 0)
                    m_UI.UI_FillRect(gainFillColor, new Rectangle(gx, gy, barLength, height));
                if (prevBarLength > 0)
                    m_UI.UI_FillRect(fillColor, new Rectangle(gx, gy, prevBarLength, height));
            }
            else if (value < previousValue)
            {
                // loss
                if (prevBarLength > 0)
                    m_UI.UI_FillRect(lossFillColor, new Rectangle(gx, gy, prevBarLength, height));
                if (barLength > 0)
                    m_UI.UI_FillRect(fillColor, new Rectangle(gx, gy, barLength, height));
            }
            else
            {
                // no change.
                if (barLength > 0)
                    m_UI.UI_FillRect(fillColor, new Rectangle(gx, gy, barLength, height));
            }

            // reference line.
            int refLength = (int)(maxWidth * (float)refValue / (float)maxValue);
            m_UI.UI_DrawLine(Color.White, gx + refLength, gy, gx + refLength, gy + height);
        }

        public void DrawMiniMap(Map map)
        {
            // clear minimap.
            if (s_Options.IsMinimapOn)
            {
                m_UI.UI_ClearMinimap(Color.Black);
            }

            // set visited tiles color.
            #region
            if (s_Options.IsMinimapOn)
            {
                Point pt = new Point();
                for (int x = 0; x < map.Width; x++)
                {
                    pt.X = x;
                    for (int y = 0; y < map.Height; y++)
                    {
                        pt.Y = y;
                        Tile tile = map.GetTileAt(x, y);
                        if (tile.IsVisited)
                        {
                            // exits override tile color.
                            if (map.GetExitAt(pt) != null)
                                m_UI.UI_SetMinimapColor(x, y, Color.HotPink);
                            else
                                m_UI.UI_SetMinimapColor(x, y, tile.Model.MinimapColor);
                        }
                    }
                }
            }
            #endregion

            // show minimap.
            if (s_Options.IsMinimapOn)
            {
                m_UI.UI_DrawMinimap(MINIMAP_X, MINIMAP_Y);
            }

            // show view rect.
            m_UI.UI_DrawRect(Color.White, new Rectangle(MINIMAP_X + m_MapViewRect.Left * MINITILE_SIZE, MINIMAP_Y + m_MapViewRect.Top * MINITILE_SIZE, m_MapViewRect.Width * MINITILE_SIZE, m_MapViewRect.Height * MINITILE_SIZE));

            // show player tags.
            #region
            if (s_Options.ShowPlayerTagsOnMinimap)
            {
                for (int x = 0; x < map.Width; x++)
                    for (int y = 0; y < map.Height; y++)
                    {
                        Tile tile = map.GetTileAt(x, y);
                        if (tile.IsVisited)
                        {
                            string minitag = null;
                            if (tile.HasDecoration(GameImages.DECO_PLAYER_TAG1))
                                minitag = GameImages.MINI_PLAYER_TAG1;
                            else if (tile.HasDecoration(GameImages.DECO_PLAYER_TAG2))
                                minitag = GameImages.MINI_PLAYER_TAG2;
                            else if (tile.HasDecoration(GameImages.DECO_PLAYER_TAG3))
                                minitag = GameImages.MINI_PLAYER_TAG3;
                            else if (tile.HasDecoration(GameImages.DECO_PLAYER_TAG4))
                                minitag = GameImages.MINI_PLAYER_TAG4;
                            if (minitag != null)
                            {
                                Point pos = new Point(MINIMAP_X + x * MINITILE_SIZE, MINIMAP_Y + y * MINITILE_SIZE);
                                m_UI.UI_DrawImage(minitag, pos.X - MINI_TRACKER_OFFSET, pos.Y - MINI_TRACKER_OFFSET);
                            }
                        }
                    }
            }
            #endregion

            // show player & tracked actors.
            // add tracked targets images out of player fov on the map.
            #region
            if (m_Player != null)
            {
                // tracker items.
                if (!m_Player.IsSleeping)
                {
                    ItemTracker tracker = m_Player.GetEquippedItem(DollPart.LEFT_HAND) as ItemTracker;

                    // tracking...
                    if (tracker != null && tracker.Batteries > 0)
                    {
                        // ...followers?
                        #region
                        if (m_Player.CountFollowers > 0 && tracker.CanTrackFollowersOrLeader)
                        {
                            foreach (Actor fo in m_Player.Followers)
                            {
                                // only track in same map.
                                if (fo.Location.Map != m_Player.Location.Map)
                                    continue;

                                ItemTracker foTracker = fo.GetEquippedItem(DollPart.LEFT_HAND) as ItemTracker;
                                if (foTracker != null && foTracker.CanTrackFollowersOrLeader)
                                {
                                    // show follower position.
                                    Point foMiniPos = new Point(MINIMAP_X + fo.Location.Position.X * MINITILE_SIZE, MINIMAP_Y + fo.Location.Position.Y * MINITILE_SIZE);
                                    m_UI.UI_DrawImage(GameImages.MINI_FOLLOWER_POSITION, foMiniPos.X - MINI_TRACKER_OFFSET, foMiniPos.Y - MINI_TRACKER_OFFSET);

                                    // if out of FoV but in view,, draw on map.
                                    if (IsInViewRect(fo.Location.Position) && !IsVisibleToPlayer(fo))
                                    {
                                        Point screenPos = MapToScreen(fo.Location.Position);
                                        m_UI.UI_DrawImage(GameImages.TRACK_FOLLOWER_POSITION, screenPos.X, screenPos.Y);
                                    }
                                }
                            }
                        }
                        #endregion

                        // ...undeads?
                        #region
                        if (tracker.CanTrackUndeads)
                        {
                            foreach (Actor other in map.Actors)
                            {
                                if (other == m_Player)
                                    continue;
                                if (!other.Model.Abilities.IsUndead)
                                    continue;
                                // only track in same map.
                                if (other.Location.Map != m_Player.Location.Map)
                                    continue;
                                if (m_Rules.GridDistance(other.Location.Position, m_Player.Location.Position) > Rules.ZTRACKINGRADIUS)
                                    continue;

                                // close undead, show it.
                                Point undeadPos = new Point(MINIMAP_X + other.Location.Position.X * MINITILE_SIZE, MINIMAP_Y + other.Location.Position.Y * MINITILE_SIZE);
                                m_UI.UI_DrawImage(GameImages.MINI_UNDEAD_POSITION, undeadPos.X - MINI_TRACKER_OFFSET, undeadPos.Y - MINI_TRACKER_OFFSET);

                                // if out of FoV but in view,, draw on map.
                                if (IsInViewRect(other.Location.Position) && !IsVisibleToPlayer(other))
                                {
                                    Point screenPos = MapToScreen(other.Location.Position);
                                    m_UI.UI_DrawImage(GameImages.TRACK_UNDEAD_POSITION, screenPos.X, screenPos.Y);
                                }
                            }
                        }
                        #endregion

                        // ...BlackOps?
                        #region
                        if (tracker.CanTrackBlackOps)
                        {
                            foreach (Actor other in map.Actors)
                            {
                                if (other == m_Player)
                                    continue;
                                if (other.Faction != GameFactions.TheBlackOps)
                                    continue;
                                // only track in same map.
                                if (other.Location.Map != m_Player.Location.Map)
                                    continue;

                                // blackop, show it.
                                Point boPos = new Point(MINIMAP_X + other.Location.Position.X * MINITILE_SIZE, MINIMAP_Y + other.Location.Position.Y * MINITILE_SIZE);
                                m_UI.UI_DrawImage(GameImages.MINI_BLACKOPS_POSITION, boPos.X - MINI_TRACKER_OFFSET, boPos.Y - MINI_TRACKER_OFFSET);

                                // if out of FoV but in view,, draw on map.
                                if (IsInViewRect(other.Location.Position) && !IsVisibleToPlayer(other))
                                {
                                    Point screenPos = MapToScreen(other.Location.Position);
                                    m_UI.UI_DrawImage(GameImages.TRACK_BLACKOPS_POSITION, screenPos.X, screenPos.Y);
                                }
                            }
                        }
                        #endregion

                        // ...Police?
                        if (tracker.CanTrackPolice)
                        {
                            foreach (Actor other in map.Actors)
                            {
                                if (other == m_Player)
                                    continue;
                                if (other.Faction != GameFactions.ThePolice)
                                    continue;
                                // only track in same map.
                                if (other.Location.Map != m_Player.Location.Map)
                                    continue;

                                // policeman, show it.
                                Point boPos = new Point(MINIMAP_X + other.Location.Position.X * MINITILE_SIZE, MINIMAP_Y + other.Location.Position.Y * MINITILE_SIZE);
                                m_UI.UI_DrawImage(GameImages.MINI_POLICE_POSITION, boPos.X - MINI_TRACKER_OFFSET, boPos.Y - MINI_TRACKER_OFFSET);

                                // if out of FoV but in view,, draw on map.
                                if (IsInViewRect(other.Location.Position) && !IsVisibleToPlayer(other))
                                {
                                    Point screenPos = MapToScreen(other.Location.Position);
                                    m_UI.UI_DrawImage(GameImages.TRACK_POLICE_POSITION, screenPos.X, screenPos.Y);
                                }
                            }
                        }
                    }
                }

                // player.
                Point pos = new Point(MINIMAP_X + m_Player.Location.Position.X * MINITILE_SIZE, MINIMAP_Y + m_Player.Location.Position.Y * MINITILE_SIZE);
                m_UI.UI_DrawImage(GameImages.MINI_PLAYER_POSITION, pos.X - MINI_TRACKER_OFFSET, pos.Y - MINI_TRACKER_OFFSET);
            }
            #endregion
        }

        public void DrawActorStatus(Actor actor, int gx, int gy)
        {
            // 1. Name & occupation
            m_UI.UI_DrawStringBold(actor.IsInvincible ? Color.LightGreen : Color.White, String.Format("{0}, {1}", actor.Name, actor.Faction.MemberName), gx, gy);

            // 2. Bars: Health, Stamina, Food, Sleep, Infection.
            gy += BOLD_LINE_SPACING;
            int maxHP = m_Rules.ActorMaxHPs(actor);
            m_UI.UI_DrawStringBold(Color.White, String.Format("HP  {0}", actor.HitPoints), gx, gy);
            DrawBar(actor.HitPoints, actor.PreviousHitPoints, maxHP, 0, 100, BOLD_LINE_SPACING, gx + BOLD_LINE_SPACING * 5, gy, Color.Red, Color.DarkRed, Color.OrangeRed, Color.Gray);
            m_UI.UI_DrawStringBold(Color.White, String.Format("{0}", maxHP), gx + BOLD_LINE_SPACING * 6 + 100, gy);

            gy += BOLD_LINE_SPACING;
            if (actor.Model.Abilities.CanTire)
            {
                int maxSTA = m_Rules.ActorMaxSTA(actor);
                m_UI.UI_DrawStringBold(Color.White, String.Format("STA {0}", actor.StaminaPoints), gx, gy);
                DrawBar(actor.StaminaPoints, actor.PreviousStaminaPoints, maxSTA, Rules.STAMINA_MIN_FOR_ACTIVITY, 100, BOLD_LINE_SPACING, gx + BOLD_LINE_SPACING * 5, gy, Color.Green, Color.DarkGreen, Color.LightGreen, Color.Gray);
                m_UI.UI_DrawStringBold(Color.White, String.Format("{0}", maxSTA), gx + BOLD_LINE_SPACING * 6 + 100, gy);
                if (actor.IsRunning)
                    m_UI.UI_DrawStringBold(Color.LightGreen, "RUNNING!", gx + BOLD_LINE_SPACING * 9 + 100, gy);
                else if (m_Rules.CanActorRun(actor))
                    m_UI.UI_DrawStringBold(Color.Green, "can run", gx + BOLD_LINE_SPACING * 9 + 100, gy);
                else if (m_Rules.IsActorTired(actor))
                    m_UI.UI_DrawStringBold(Color.Gray, "TIRED", gx + BOLD_LINE_SPACING * 9 + 100, gy);
            }

            gy += BOLD_LINE_SPACING;
            if (actor.Model.Abilities.HasToEat)
            {
                int maxFood = m_Rules.ActorMaxFood(actor);
                m_UI.UI_DrawStringBold(Color.White, String.Format("FOO {0}", actor.FoodPoints), gx, gy);
                DrawBar(actor.FoodPoints, actor.PreviousFoodPoints, maxFood, Rules.FOOD_HUNGRY_LEVEL, 100, BOLD_LINE_SPACING, gx + BOLD_LINE_SPACING * 5, gy, Color.Chocolate, Color.Brown, Color.Beige, Color.Gray);
                m_UI.UI_DrawStringBold(Color.White, String.Format("{0}", maxFood), gx + BOLD_LINE_SPACING * 6 + 100, gy);
                if (m_Rules.IsActorHungry(actor))
                {
                    if (m_Rules.IsActorStarving(actor))
                        m_UI.UI_DrawStringBold(Color.Red, "STARVING!", gx + BOLD_LINE_SPACING * 9 + 100, gy);
                    else
                        m_UI.UI_DrawStringBold(Color.Yellow, "Hungry", gx + BOLD_LINE_SPACING * 9 + 100, gy);
                }
                else
                    m_UI.UI_DrawStringBold(Color.White, String.Format("{0}h", FoodToHoursUntilHungry(actor.FoodPoints)), gx + BOLD_LINE_SPACING * 9 + 100, gy);
            }
            else if (actor.Model.Abilities.IsRotting)
            {
                int maxFood = m_Rules.ActorMaxRot(actor);
                m_UI.UI_DrawStringBold(Color.White, String.Format("ROT {0}", actor.FoodPoints), gx, gy);
                DrawBar(actor.FoodPoints, actor.PreviousFoodPoints, maxFood, Rules.ROT_HUNGRY_LEVEL, 100, BOLD_LINE_SPACING, gx + BOLD_LINE_SPACING * 5, gy, Color.Chocolate, Color.Brown, Color.Beige, Color.Gray);
                m_UI.UI_DrawStringBold(Color.White, String.Format("{0}", maxFood), gx + BOLD_LINE_SPACING * 6 + 100, gy);
                if (m_Rules.IsRottingActorHungry(actor))
                {
                    if (m_Rules.IsRottingActorStarving(actor))
                        m_UI.UI_DrawStringBold(Color.Red, "STARVING!", gx + BOLD_LINE_SPACING * 9 + 100, gy);
                    else
                        m_UI.UI_DrawStringBold(Color.Yellow, "Hungry", gx + BOLD_LINE_SPACING * 9 + 100, gy);
                }
                else
                    m_UI.UI_DrawStringBold(Color.White, String.Format("{0}h", FoodToHoursUntilRotHungry(actor.FoodPoints)), gx + BOLD_LINE_SPACING * 9 + 100, gy);
            }

            gy += BOLD_LINE_SPACING;
            if (actor.Model.Abilities.HasToSleep)
            {
                int maxSleep = m_Rules.ActorMaxSleep(actor);
                m_UI.UI_DrawStringBold(Color.White, String.Format("SLP {0}", actor.SleepPoints), gx, gy);
                DrawBar(actor.SleepPoints, actor.PreviousSleepPoints, maxSleep, Rules.SLEEP_SLEEPY_LEVEL, 100, BOLD_LINE_SPACING, gx + BOLD_LINE_SPACING * 5, gy, Color.Blue, Color.DarkBlue, Color.LightBlue, Color.Gray);
                m_UI.UI_DrawStringBold(Color.White, String.Format("{0}", maxSleep), gx + BOLD_LINE_SPACING * 6 + 100, gy);
                if (m_Rules.IsActorSleepy(actor))
                {
                    if (m_Rules.IsActorExhausted(actor))
                        m_UI.UI_DrawStringBold(Color.Red, "EXHAUSTED!", gx + BOLD_LINE_SPACING * 9 + 100, gy);
                    else
                        m_UI.UI_DrawStringBold(Color.Yellow, "Sleepy", gx + BOLD_LINE_SPACING * 9 + 100, gy);
                }
                else
                    m_UI.UI_DrawStringBold(Color.White, String.Format("{0}h", m_Rules.SleepToHoursUntilSleepy(actor.SleepPoints, m_Session.WorldTime.IsNight)), gx + BOLD_LINE_SPACING * 9 + 100, gy);
            }

            gy += BOLD_LINE_SPACING;
            if (actor.Model.Abilities.HasSanity)
            {
                int maxSan = m_Rules.ActorMaxSanity(actor);
                m_UI.UI_DrawStringBold(Color.White, String.Format("SAN {0}", actor.Sanity), gx, gy);
                DrawBar(actor.Sanity, actor.PreviousSanity, maxSan, m_Rules.ActorDisturbedLevel(actor), 100, BOLD_LINE_SPACING, gx + BOLD_LINE_SPACING * 5, gy, Color.Orange, Color.DarkOrange, Color.OrangeRed, Color.Gray);
                m_UI.UI_DrawStringBold(Color.White, String.Format("{0}", maxSan), gx + BOLD_LINE_SPACING * 6 + 100, gy);
                if (m_Rules.IsActorDisturbed(actor))
                {
                    if (m_Rules.IsActorInsane(actor))
                        m_UI.UI_DrawStringBold(Color.Red, "INSANE!", gx + BOLD_LINE_SPACING * 9 + 100, gy);
                    else
                        m_UI.UI_DrawStringBold(Color.Yellow, "Disturbed", gx + BOLD_LINE_SPACING * 9 + 100, gy);
                }
                else
                    m_UI.UI_DrawStringBold(Color.White, String.Format("{0}h", m_Rules.SanityToHoursUntilUnstable(actor)), gx + BOLD_LINE_SPACING * 9 + 100, gy);
            }

            if (Rules.HasInfection(m_Session.GameMode) && !actor.Model.Abilities.IsUndead)
            {
                int maxInf = m_Rules.ActorInfectionHPs(actor);
                int refInf = (Rules.INFECTION_LEVEL_1_WEAK * maxInf) / 100;
                gy += BOLD_LINE_SPACING;
                m_UI.UI_DrawStringBold(Color.White, String.Format("INF {0}", actor.Infection), gx, gy);
                DrawBar(actor.Infection, actor.Infection, maxInf, refInf, 100, BOLD_LINE_SPACING, gx + BOLD_LINE_SPACING * 5, gy, Color.Purple, Color.Black, Color.Black, Color.Gray);
                m_UI.UI_DrawStringBold(Color.White, String.Format("{0}%", m_Rules.ActorInfectionPercent(actor)), gx + BOLD_LINE_SPACING * 6 + 100, gy);
            }

            // 3. Melee & Ranged Attacks.
            gy += BOLD_LINE_SPACING;
            Attack melee = m_Rules.ActorMeleeAttack(actor, actor.CurrentMeleeAttack, null);
            int dmgBonusVsUndead = m_Rules.ActorDamageBonusVsUndeads(actor);
            m_UI.UI_DrawStringBold(Color.White, String.Format("Melee  Atk {0:D2}  Dmg {1:D2}/{2:D2}", melee.HitValue, melee.DamageValue, melee.DamageValue + dmgBonusVsUndead), gx, gy);

            gy += BOLD_LINE_SPACING;
            Attack ranged = m_Rules.ActorRangedAttack(actor, actor.CurrentRangedAttack, actor.CurrentRangedAttack.EfficientRange, null);
            ItemRangedWeapon rangedWeapon = actor.GetEquippedWeapon() as ItemRangedWeapon;
            int ammo, maxAmmo;
            ammo = maxAmmo = 0;
            if (rangedWeapon != null)
            {
                ammo = rangedWeapon.Ammo;
                maxAmmo = (rangedWeapon.Model as ItemRangedWeaponModel).MaxAmmo;
                m_UI.UI_DrawStringBold(Color.White, String.Format("Ranged Atk {0:D2}  Dmg {1:D2}/{2:D2} Rng {3}-{4} Amo {5}/{6}",
                    ranged.HitValue, ranged.DamageValue, ranged.DamageValue + dmgBonusVsUndead, ranged.Range, ranged.EfficientRange, ammo, maxAmmo), gx, gy);
            }

            // 4. (living)Def, Pro, Spd, FoV & Nb of followers / (undead)Def, Spd, Fov, Sml, Kills
            gy += BOLD_LINE_SPACING;
            Defence defence = m_Rules.ActorDefence(actor, actor.CurrentDefence);

            if (actor.Model.Abilities.IsUndead)
            {
                m_UI.UI_DrawStringBold(Color.White, String.Format("Def {0:D2} Spd {1:F2} FoV {2} Sml {3:F2} Kills {4}",
                    defence.Value,
                    (float)m_Rules.ActorSpeed(actor) / (float)Rules.BASE_SPEED,
                    m_Rules.ActorFOV(actor, m_Session.WorldTime, m_Session.World.Weather),
                    m_Rules.ActorSmell(actor),
                    actor.KillsCount),
                    gx, gy);
            }
            else
            {
                m_UI.UI_DrawStringBold(Color.White, String.Format("Def {0:D2} Arm {1:D1}/{2:D1} Spd {3:F2} FoV {4}/{5} Fol {6}/{7}",
                    defence.Value, defence.Protection_Hit, defence.Protection_Shot,
                    (float)m_Rules.ActorSpeed(actor) / (float)Rules.BASE_SPEED,
                    m_Rules.ActorFOV(actor, m_Session.WorldTime, m_Session.World.Weather),
                    actor.Sheet.BaseViewRange,
                    actor.CountFollowers, m_Rules.ActorMaxFollowers(actor)),
                    gx, gy);
            }

            // 5. Odor suppressor // alpha10
            gy += BOLD_LINE_SPACING;
            if (actor.OdorSuppressorCounter > 0)
                m_UI.UI_DrawStringBold(Color.LightBlue, string.Format("Odor suppr : {0} -{1}", actor.OdorSuppressorCounter, m_Rules.OdorsDecay(actor.Location.Map, actor.Location.Position, m_Session.World.Weather)), gx, gy);
        }

        public void DrawInventory(Inventory inventory, string title, bool drawSlotsNumbers, int slotsPerLine, int maxSlots, int gx, int gy)
        {
            int x, y;
            int slot = 0;

            // Draw title.
            gy -= BOLD_LINE_SPACING;
            m_UI.UI_DrawStringBold(Color.White, title, gx, gy);
            gy += BOLD_LINE_SPACING;

            // Draw slots.
            x = gx; y = gy; slot = 0;
            for (int i = 0; i < maxSlots; i++)
            {
                m_UI.UI_DrawImage(GameImages.ITEM_SLOT, x, y);
                if (++slot >= slotsPerLine)
                {
                    slot = 0;
                    y += TILE_SIZE;
                    x = gx;
                }
                else
                    x += TILE_SIZE;
            }

            // Draw items.
            if (inventory == null)
                return;

            x = gx; y = gy; slot = 0;
            foreach (Item it in inventory.Items)
            {
                if (it.IsEquipped)
                    m_UI.UI_DrawImage(GameImages.ITEM_EQUIPPED, x, y);
                if (it is ItemRangedWeapon)
                {
                    ItemRangedWeapon w = it as ItemRangedWeapon;
                    if (w.Ammo <= 0)
                        m_UI.UI_DrawImage(GameImages.ICON_OUT_OF_AMMO, x, y);
                    DrawBar(w.Ammo, w.Ammo, (w.Model as ItemRangedWeaponModel).MaxAmmo, 0, 28, 3, x + 2, y + 27, Color.Blue, Color.Blue, Color.Blue, Color.DarkGray);
                }
                else if (it is ItemSprayPaint)
                {
                    ItemSprayPaint sp = it as ItemSprayPaint;
                    DrawBar(sp.PaintQuantity, sp.PaintQuantity, (sp.Model as ItemSprayPaintModel).MaxPaintQuantity, 0, 28, 3, x + 2, y + 27, Color.Gold, Color.Gold, Color.Gold, Color.DarkGray);
                }
                else if (it is ItemSprayScent)
                {
                    ItemSprayScent sp = it as ItemSprayScent;
                    DrawBar(sp.SprayQuantity, sp.SprayQuantity, (sp.Model as ItemSprayScentModel).MaxSprayQuantity, 0, 28, 3, x + 2, y + 27, Color.Cyan, Color.Cyan, Color.Cyan, Color.DarkGray);
                }
                else if (it is ItemLight)
                {
                    ItemLight lt = it as ItemLight;
                    if (lt.Batteries <= 0)
                        m_UI.UI_DrawImage(GameImages.ICON_OUT_OF_BATTERIES, x, y);
                    DrawBar(lt.Batteries, lt.Batteries, (lt.Model as ItemLightModel).MaxBatteries, 0, 28, 3, x + 2, y + 27, Color.Yellow, Color.Yellow, Color.Yellow, Color.DarkGray);
                }
                else if (it is ItemTracker)
                {
                    ItemTracker tr = it as ItemTracker;
                    if (tr.Batteries <= 0)
                        m_UI.UI_DrawImage(GameImages.ICON_OUT_OF_BATTERIES, x, y);
                    DrawBar(tr.Batteries, tr.Batteries, (tr.Model as ItemTrackerModel).MaxBatteries, 0, 28, 3, x + 2, y + 27, Color.Pink, Color.Pink, Color.Pink, Color.DarkGray);
                }
                else if (it is ItemFood)
                {
                    ItemFood food = it as ItemFood;
                    if (m_Rules.IsFoodExpired(food, m_Session.WorldTime.TurnCounter))
                        m_UI.UI_DrawImage(GameImages.ICON_EXPIRED_FOOD, x, y);
                    else if (m_Rules.IsFoodSpoiled(food, m_Session.WorldTime.TurnCounter))
                        m_UI.UI_DrawImage(GameImages.ICON_SPOILED_FOOD, x, y);
                }
                else if (it is ItemTrap)
                {
                    ItemTrap trap = it as ItemTrap;
                    DrawTrapItem(trap, x, y);  // alpha10
                }
                else if (it is ItemEntertainment)
                {
                    if (m_Player != null && ((it as ItemEntertainment).IsBoringFor(m_Player))) // alpha10 boring items item centric
                        m_UI.UI_DrawImage(GameImages.ICON_BORING_ITEM, x, y);
                }
                DrawItem(it, x, y);

                if (++slot >= slotsPerLine)
                {
                    slot = 0;
                    y += TILE_SIZE;
                    x = gx;
                }
                else
                    x += TILE_SIZE;
            }

            // Draw slots numbers.
            if (drawSlotsNumbers)
            {
                x = gx + 4; y = gy + TILE_SIZE;
                for (int i = 0; i < inventory.MaxCapacity; i++)
                {
                    m_UI.UI_DrawString(Color.White, (i + 1).ToString(), x, y);
                    x += TILE_SIZE;
                }

            }
        }

        public void DrawItem(Item it, int gx, int gy)
        {
            DrawItem(it, gx, gy, Color.White);
        }

        public void DrawItem(Item it, int gx, int gy, Color tint)
        {
            m_UI.UI_DrawImage(it.ImageID, gx, gy, tint);

            if (it.Model.IsStackable)
            {
                string q = string.Format("{0}", it.Quantity);
                int tx = gx + TILE_SIZE - 10;
                if (it.Quantity > 100)
                    tx -= 10;
                else if (it.Quantity > 10)
                    tx -= 4;
                m_UI.UI_DrawString(Color.DarkGray, q, tx + 1, gy + 1);
                m_UI.UI_DrawString(Color.White, q, tx, gy);
            }
            if (it is ItemTrap)
            {
                DrawTrapItem(it as ItemTrap, gx, gy);  // alpha10
            }
        }

        // alpha10 factorized code
        void DrawTrapItem(ItemTrap trap, int gx, int gy)
        {
            if (trap.IsTriggered)
            {
                // alpha10
                if (trap.Owner == m_Player)
                    m_UI.UI_DrawImage(GameImages.ICON_TRAP_TRIGGERED_SAFE_PLAYER, gx, gy);
                else if (m_Rules.IsSafeFromTrap(trap, m_Player))
                    m_UI.UI_DrawImage(GameImages.ICON_TRAP_TRIGGERED_SAFE_GROUP, gx, gy);
                else
                    m_UI.UI_DrawImage(GameImages.ICON_TRAP_TRIGGERED, gx, gy);
            }
            else if (trap.IsActivated)
            {
                // alpha10
                if (trap.Owner == m_Player)
                    m_UI.UI_DrawImage(GameImages.ICON_TRAP_ACTIVATED_SAFE_PLAYER, gx, gy);
                else if (m_Rules.IsSafeFromTrap(trap, m_Player))
                    m_UI.UI_DrawImage(GameImages.ICON_TRAP_ACTIVATED_SAFE_GROUP, gx, gy);
                else
                    m_UI.UI_DrawImage(GameImages.ICON_TRAP_ACTIVATED, gx, gy);
            }
        }

        public void DrawActorSkillTable(Actor actor, int gx, int gy)
        {
            gy -= BOLD_LINE_SPACING;
            m_UI.UI_DrawStringBold(Color.White, "Skills", gx, gy);
            gy += BOLD_LINE_SPACING;

            IEnumerable<Skill> skills = actor.Sheet.SkillTable.Skills;
            if (skills == null)
                return;

            int x, y;
            int count = 0;
            x = gx; y = gy;
            foreach (Skill sk in skills)
            {
                Color skColor = Color.White;

                // alpha10 highlight if active skills are active or not
                switch (sk.ID)
                {
                    case (int)Skills.IDs.MARTIAL_ARTS:
                        skColor = (actor.GetEquippedWeapon() == null ? Color.LightGreen : Color.Red);
                        break;
                    case (int)Skills.IDs.HARDY:
                        if (actor.IsSleeping) skColor = Color.LightGreen;
                        break;
                }

                m_UI.UI_DrawString(skColor, String.Format("{0}-", sk.Level), x, y);
                x += 16;
                m_UI.UI_DrawString(skColor, Skills.Name(sk.ID), x, y);
                x -= 16;

                if (++count >= SKILLTABLE_LINES)
                {
                    count = 0;
                    y = gy;
                    x += 120;
                }
                else
                    y += LINE_SPACING;
            }
        }
        #endregion

        #region Overlays
        void AddOverlay(Overlay o)
        {
            lock (m_Overlays)  // alpha10
            {
                m_Overlays.Add(o);
            }
        }

        void ClearOverlays()
        {
            lock (m_Overlays)  // alpha10
            {
                m_Overlays.Clear();
            }
        }

        void RemoveOverlay(Overlay o)
        {
            lock (m_Overlays)  // alpha10
            {
                m_Overlays.Remove(o);
            }
        }

        // alpha10
        bool HasOverlay(Overlay o)
        {
            bool hasIt = false;
            lock (m_Overlays)
            {
                if (m_Overlays.Contains(o))
                    hasIt = true;
            }
            return hasIt;
        }
        #endregion

        #region Coordinates conversion
        Point MapToScreen(Point mapPosition)
        {
            return MapToScreen(mapPosition.X, mapPosition.Y);
        }

        Point MapToScreen(int x, int y)
        {
            return new Point((x - m_MapViewRect.Left) * RogueGame.TILE_SIZE, (y - m_MapViewRect.Top) * RogueGame.TILE_SIZE);
        }

        Point ScreenToMap(Point screenPosition)
        {
            return ScreenToMap(screenPosition.X, screenPosition.Y);
        }

        Point ScreenToMap(int gx, int gy)
        {
            return new Point(m_MapViewRect.Left + gx / RogueGame.TILE_SIZE, m_MapViewRect.Top + gy / RogueGame.TILE_SIZE);
        }

        Point MouseToMap(Point mousePosition)
        {
            return MouseToMap(mousePosition.X, mousePosition.Y);
        }

        Point MouseToMap(int mouseX, int mouseY)
        {
            mouseX = (int)(mouseX / m_UI.UI_GetCanvasScaleX());
            mouseY = (int)(mouseY / m_UI.UI_GetCanvasScaleY());
            return ScreenToMap(mouseX, mouseY);
        }

        Point MouseToInventorySlot(int invX, int invY, int mouseX, int mouseY)
        {
            mouseX = (int)(mouseX / m_UI.UI_GetCanvasScaleX());
            mouseY = (int)(mouseY / m_UI.UI_GetCanvasScaleY());

            return new Point((mouseX - invX) / 32, (mouseY - invY) / 32);
        }

        Point InventorySlotToScreen(int invX, int invY, int slotX, int slotY)
        {
            return new Point(invX + slotX * 32, invY + slotY * 32);
        }
        #endregion

        #region Visibility/Smell & memory helpers
        bool IsVisibleToPlayer(Location location)
        {
            return IsVisibleToPlayer(location.Map, location.Position);
        }

        bool IsVisibleToPlayer(Map map, Point position)
        {
            return m_Player != null
                && map == m_Player.Location.Map && map.IsInBounds(position.X, position.Y)
                && map.GetTileAt(position.X, position.Y).IsInView;
        }

        bool IsVisibleToPlayer(Actor actor)
        {
            return actor == m_Player || IsVisibleToPlayer(actor.Location);
        }

        bool IsVisibleToPlayer(MapObject mapObj)
        {
            return IsVisibleToPlayer(mapObj.Location);
        }

        bool IsKnownToPlayer(Map map, Point position)
        {
            return map.IsInBounds(position.X, position.Y) && map.GetTileAt(position.X, position.Y).IsVisited;
        }

        bool IsKnownToPlayer(Location location)
        {
            return IsKnownToPlayer(location.Map, location.Position);
        }

        bool IsKnownToPlayer(MapObject mapObj)
        {
            return IsKnownToPlayer(mapObj.Location);
        }

        bool IsPlayerSleeping()
        {
            return m_Player != null && m_Player.IsSleeping;
        }
        #endregion

        #region Text helpers
        int FindLongestLine(string[] lines)
        {
            if (lines == null || lines.Length == 0)
                return 0;

            int max = Int32.MinValue;

            foreach (string s in lines)
            {
                if (s == null)  // sanity check.
                    continue;
                if (s.Length > max)
                    max = s.Length;
            }

            return max;
        }
        #endregion

        #endregion

        #region Game loading & saving
        void HandleSaveGame()
        {
            // alpha10.1
            // manually saving the game delays (reschedule) the next autosave
            ScheduleNextAutoSave();

            DoSaveGame(GetUserSave());
        }

        // alpha10.1
        void CheckAutoSaveTime()
        {
            // sanity checks
            if (!m_IsGameRunning || m_Player == null || m_Player.IsDead)
                return;

            // option off?
            if (s_Options.AutoSavePeriodInHours <= 0)
                return;

            // not time yet?
            if (m_Session.WorldTime.TurnCounter < m_Session.NextAutoSaveTime)
                return;

            // autosave now and reschedule
            ScheduleNextAutoSave();
            Overlay popup = new OverlayPopup(new string[] { "AUTOSAVING..." }, Color.Yellow, Color.White, Color.Black, MapToScreen(m_Player.Location.Position.X, m_Player.Location.Position.Y));
            AddOverlay(popup);
            DoSaveGame(GetUserSave(), true);
            RemoveOverlay(popup);
            RedrawPlayScreen();
        }

        // alpha10.1
        void ScheduleNextAutoSave()
        {
            m_Session.NextAutoSaveTime = m_Session.WorldTime.TurnCounter + WorldTime.TURNS_PER_HOUR * s_Options.AutoSavePeriodInHours;
        }

        void HandleLoadGame()
        {
            DoLoadGame(GetUserSave());
        }

        // alpha10.1 messages modified for autosave
        // alpha10.1 start & stop sim thread here instead of caller
        void DoSaveGame(string saveName, bool isAutoSave = false)
        {
            StopSimThread(false);  // alpha10.1

            string savingOrAutosaving = isAutoSave ? "AUTOSAVING" : "SAVING";

            ClearMessages();
            AddMessage(new Message(String.Format("{0} GAME, PLEASE WAIT...", savingOrAutosaving), m_Session.WorldTime.TurnCounter, Color.Yellow));
            RedrawPlayScreen();
            m_UI.UI_Repaint();

            // save session object.
            Session.Save(m_Session, saveName, Session.SaveFormat.FORMAT_BIN);

            AddMessage(new Message(String.Format("{0} DONE.", savingOrAutosaving), m_Session.WorldTime.TurnCounter, Color.Yellow));
            RedrawPlayScreen();
            m_UI.UI_Repaint();

            StartSimThread();  // alpha10.1
        }

        // alpha10.1 start & stop sim thread here instead of caller
        void DoLoadGame(string saveName)
        {
            StopSimThread(false); // alpha10.1

            ClearMessages();
            AddMessage(new Message("LOADING GAME, PLEASE WAIT...", m_Session.WorldTime.TurnCounter, Color.Yellow));
            RedrawPlayScreen();
            m_UI.UI_Repaint();

            if (!LoadGame(saveName))
            {
                AddMessage(new Message("LOADING FAILED, NO GAME SAVED OR VERSION NOT COMPATIBLE.", m_Session.WorldTime.TurnCounter, Color.Red));
            }

            StartSimThread();  // alpha10.1
        }

        void DeleteSavedGame(string saveName)
        {
            // do it.
            if (Session.Delete(saveName))
            {
                // tell.
                AddMessage(new Message("PERMADEATH : SAVE GAME DELETED!", m_Session.WorldTime.TurnCounter, Color.Red));
            }
        }

        bool LoadGame(string saveName)
        {
            // load session object.
            bool loaded = Session.Load(saveName, Session.SaveFormat.FORMAT_BIN);
            if (!loaded)
                return false;
            m_Session = Session.Get;
            m_Rules = new Rules(new DiceRoller(m_Session.Seed));

            RefreshPlayer();

            AddMessage(new Message("LOADING DONE.", m_Session.WorldTime.TurnCounter, Color.Yellow));
            AddMessage(new Message("Welcome back to Rogue Survivor!", m_Session.WorldTime.TurnCounter, Color.LightGreen));
            RedrawPlayScreen();
            m_UI.UI_Repaint();

            // Log ;/
            m_Session.Scoring.AddEvent(m_Session.WorldTime.TurnCounter, "<Loaded game>");

            return true;
        }
        #endregion

        #region Options loading, saving & applying
        void LoadOptions()
        {
            // load.
            s_Options = GameOptions.Load(GetUserOptionsFilePath());
        }

        void SaveOptions()
        {
            // save
            GameOptions.Save(s_Options, GetUserOptionsFilePath());
        }

        void ApplyOptions(bool ingame)
        {
            m_MusicManager.IsMusicEnabled = Options.PlayMusic;
            m_MusicManager.Volume = Options.MusicVolume;

            // update difficulty.
            if (m_Session != null && m_Session.Scoring != null)
            {
                m_Session.Scoring.Side = (m_Player == null || !m_Player.Model.Abilities.IsUndead) ? DifficultySide.FOR_SURVIVOR : DifficultySide.FOR_UNDEAD;
                m_Session.Scoring.DifficultyRating = Scoring.ComputeDifficultyRating(s_Options, m_Session.Scoring.Side, m_Session.Scoring.ReincarnationNumber);
            }

            if (!m_MusicManager.IsMusicEnabled)
                m_MusicManager.Stop();
        }
        #endregion

        #region Keybindings loading & saving
        void LoadKeybindings()
        {
            m_UI.UI_Clear(Color.Black);
            m_UI.UI_DrawStringBold(Color.White, "Loading keybindings...", 0, 0);
            m_UI.UI_Repaint();

            s_KeyBindings = Keybindings.Load(GetUserConfigPath() + "keys.dat");

            m_UI.UI_Clear(Color.Black);
            m_UI.UI_DrawStringBold(Color.White, "Loading keybindings... done!", 0, 0);
            m_UI.UI_Repaint();

        }

        void SaveKeybindings()
        {
            m_UI.UI_Clear(Color.Black);
            m_UI.UI_DrawStringBold(Color.White, "Saving keybindings...", 0, 0);
            m_UI.UI_Repaint();

            Keybindings.Save(s_KeyBindings, GetUserConfigPath() + "keys.dat");

            m_UI.UI_Clear(Color.Black);
            m_UI.UI_DrawStringBold(Color.White, "Saving keybindings... done!", 0, 0);
            m_UI.UI_Repaint();
        }
        #endregion

        #region Hints saving & loading
        void LoadHints()
        {
            m_UI.UI_Clear(Color.Black);
            m_UI.UI_DrawStringBold(Color.White, "Loading hints...", 0, 0);
            m_UI.UI_Repaint();

            s_Hints = GameHintsStatus.Load(GetUserConfigPath() + "hints.dat");

            m_UI.UI_Clear(Color.Black);
            m_UI.UI_DrawStringBold(Color.White, "Loading hints... done!", 0, 0);
            m_UI.UI_Repaint();
        }

        void SaveHints()
        {
            GameHintsStatus.Save(s_Hints, GetUserConfigPath() + "hints.dat");
        }
        #endregion

        #region Menu helpers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entries">choices text</param>
        /// <param name="values">(options values) can be null, array must be same length as choices</param>
        /// <param name="gx"></param>
        /// <param name="gy"></param>
        /// <param name="valuesOnNewLine">false: draw values on same line as entries; true: draw values on new lines</param>
        /// <param param name="rightPadding">x padding to add when displaying values</param>
        /// <returns></returns>
        void DrawMenuOrOptions(int currentChoice, Color entriesColor, string[] entries, Color valuesColor, string[] values, int gx, ref int gy, bool valuesOnNewLine = false, int rightPadding = 256)
        {
            int right = gx + rightPadding;

            if (values != null && entries.Length != values.Length)
                throw new ArgumentException("values length!= choices length");

            // display.
            Color entriesShadowColor = Color.FromArgb(entriesColor.A, entriesColor.R / 2, entriesColor.G / 2, entriesColor.B / 2);
            for (int i = 0; i < entries.Length; i++)
            {
                string choiceStr;
                if (i == currentChoice)
                    choiceStr = String.Format("---> {0}", entries[i]);
                else
                    choiceStr = String.Format("     {0}", entries[i]);
                m_UI.UI_DrawStringBold(entriesColor, choiceStr, gx, gy, entriesShadowColor);

                if (values != null)
                {
                    string valueStr;
                    if (i == currentChoice && !valuesOnNewLine)
                        valueStr = String.Format("{0} <---", values[i]);
                    else
                        valueStr = values[i];

                    if (valuesOnNewLine)
                    {
                        gy += BOLD_LINE_SPACING;
                        m_UI.UI_DrawStringBold(valuesColor, valueStr, gx + right, gy);
                    }
                    else
                    {
                        m_UI.UI_DrawStringBold(valuesColor, valueStr, right, gy);
                    }
                }

                gy += BOLD_LINE_SPACING;
            }
        }
        #endregion

        #region Drawing headers & footnotes
        void DrawHeader()
        {
            m_UI.UI_DrawStringBold(Color.Red, "ROGUE SURVIVOR - " + SetupConfig.GAME_VERSION, 0, 0, Color.DarkRed);
        }

        void DrawFootnote(Color color, string text)
        {
            Color shadowColor = Color.FromArgb(color.A, color.R / 2, color.G / 2, color.B / 2);
            m_UI.UI_DrawStringBold(color, String.Format("<{0}>", text), 0, CANVAS_HEIGHT - BOLD_LINE_SPACING, shadowColor);
        }
        #endregion

        #region Game user data paths
        public static string GetUserBasePath()
        {
            return SetupConfig.DirPath;
            /*
            string myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return myDocs + @"\Rogue Survivor\" + SetupConfig.GAME_VERSION + @"\";
             */
        }

        public static string GetUserSavesPath()
        {
            return GetUserBasePath() + @"Saves\";
        }

        public static string GetUserSave()
        {
            return GetUserSavesPath() + "save.dat";
        }

        public static string GetUserDocsPath()
        {
            return GetUserBasePath() + @"Docs\";
        }

        public static string GetUserGraveyardPath()
        {
            return GetUserBasePath() + @"Graveyard\";
        }

        /// <summary>
        /// "grave_[id]"
        /// </summary>
        /// <returns></returns>
        public string GetUserNewGraveyardName()
        {
            string name;
            int i = 0;
            bool isFreeID = false;
            do
            {
                name = String.Format("grave_{0:D3}", i);
                isFreeID = !File.Exists(GraveFilePath(name));
                ++i;
            }
            while (!isFreeID);

            return name;
        }

        public static string GraveFilePath(string graveName)
        {
            return GetUserGraveyardPath() + graveName + ".txt";
        }

        public static string GetUserConfigPath()
        {
            return GetUserBasePath() + @"Config\";
        }

        public static string GetUserOptionsFilePath()
        {
            return GetUserConfigPath() + @"options.dat";
        }

        public static string GetUserScreenshotsPath()
        {
            return GetUserBasePath() + @"Screenshots\";
        }

        /// <summary>
        /// "screenshot_[id]"
        /// </summary>
        /// <returns></returns>
        public string GetUserNewScreenshotName()
        {
            string name;
            int i = 0;
            bool isFreeID = false;
            do
            {
                name = String.Format("screenshot_{0:D3}", i);
                isFreeID = !File.Exists(ScreenshotFilePath(name));
                ++i;
            }
            while (!isFreeID);

            return name;
        }

        public string ScreenshotFilePath(string shotname)
        {
            return GetUserScreenshotsPath() + shotname + "." + m_UI.UI_ScreenshotExtension();
        }

        bool CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return true;
            }
            else
                return false;
        }

        bool CheckDirectory(string path, string description, ref int gy)
        {
            m_UI.UI_DrawString(Color.White, String.Format("{0} : {1}...", description, path), 0, gy);
            gy += BOLD_LINE_SPACING;
            m_UI.UI_Repaint();
            bool created = CreateDirectory(path);
            m_UI.UI_DrawString(Color.White, "ok.", 0, gy);
            gy += BOLD_LINE_SPACING;
            m_UI.UI_Repaint();

            return created;
        }

        bool CheckCopyOfManual()
        {
            string src_path = @"Resources\Manual\";
            string dst_path = GetUserDocsPath();
            string filename = "RS Manual.txt";

            // copy file.
            bool copied = false;
            Logger.WriteLine(Logger.Stage.INIT_MAIN, "checking for manual...");
            if (!File.Exists(dst_path + filename))
            {
                Logger.WriteLine(Logger.Stage.INIT_MAIN, "copying manual...");
                copied = true;
                File.Copy(src_path + filename, dst_path + filename);
                Logger.WriteLine(Logger.Stage.INIT_MAIN, "copying manual... done!");
            }
            Logger.WriteLine(Logger.Stage.INIT_MAIN, "checking for manual... done!");

            return copied;
        }

        string GetUserManualFilePath()
        {
            return GetUserDocsPath() + "RS Manual.txt";
        }

        string GetUserHiScorePath()
        {
            return GetUserSavesPath();
        }

        string GetUserHiScoreFilePath()
        {
            return GetUserHiScorePath() + "hiscores.dat";
        }

        string GetUserHiScoreTextFilePath()
        {
            return GetUserHiScorePath() + "hiscores.txt";
        }
        #endregion

        #region Generating world
        void GenerateWorld(bool isVerbose, int size)
        {
            // say so.
            if (isVerbose)
            {
                m_UI.UI_Clear(Color.Black);
                m_UI.UI_DrawStringBold(Color.White, "Generating game world...", 0, 0);
                m_UI.UI_Repaint();
            }

            //////////////////////
            // Create blank world
            //////////////////////
            if (isVerbose)
            {
                m_UI.UI_Clear(Color.Black);
                m_UI.UI_DrawStringBold(Color.White, "Creating empty world...", 0, 0);
                m_UI.UI_Repaint();
            }
            m_Session.World = new World(size);
            World world = m_Session.World;

            ////////////////////////
            // Roll initial weather
            ////////////////////////
            world.Weather = (Weather)m_Rules.Roll((int)Weather._FIRST, (int)Weather._COUNT);
            world.NextWeatherCheckTurn = m_Rules.Roll(WEATHER_MIN_DURATION, WEATHER_MAX_DURATION);  // alpha10

            //////////////////////////////////////////////
            // Roll locations of special buildings.
            // Only ONE special building max per district.
            //////////////////////////////////////////////
            List<Point> noSpecialDistricts = new List<Point>();
            for (int x = 0; x < world.Size; x++)
                for (int y = 0; y < world.Size; y++)
                    noSpecialDistricts.Add(new Point(x, y));

            Point policeStationDistrictPos = noSpecialDistricts[m_Rules.Roll(0, noSpecialDistricts.Count)];
            noSpecialDistricts.Remove(policeStationDistrictPos);

            Point hospitalDistrictPos = noSpecialDistricts[m_Rules.Roll(0, noSpecialDistricts.Count)];
            noSpecialDistricts.Remove(hospitalDistrictPos);

            /////////////////////////
            // Create districts maps
            /////////////////////////
            // Surface, Sewers and Subways.
            #region
            for (int x = 0; x < world.Size; x++)
            {
                for (int y = 0; y < world.Size; y++)
                {
                    if (isVerbose)
                    {
                        m_UI.UI_Clear(Color.Black);
                        m_UI.UI_DrawStringBold(Color.White, String.Format("Creating District@{0}...", World.CoordToString(x, y)), 0, 0);
                        m_UI.UI_Repaint();
                    }

                    // create the district.
                    District district = new District(new Point(x, y), GenerateDistrictKind(world, x, y));
                    world[x, y] = district;

                    // create the entry map.
                    district.EntryMap = GenerateDistrictEntryMap(world, district, policeStationDistrictPos, hospitalDistrictPos);
                    district.Name = district.EntryMap.Name;

                    // create other maps.
                    // - sewers
                    Map sewers = GenerateDistrictSewersMap(district);
                    district.SewersMap = sewers;
                    // - subway (only in the middle district line)
                    if (y == world.Size / 2)
                    {
                        Map subwayMap = GenerateDistrictSubwayMap(district);
                        district.SubwayMap = subwayMap;
                    }
                }
            }
            #endregion

            ///////////////
            // Unique Maps
            ///////////////
            if (isVerbose)
            {
                m_UI.UI_Clear(Color.Black);
                m_UI.UI_DrawStringBold(Color.White, "Generating unique maps...", 0, 0);
                m_UI.UI_Repaint();
            }
            m_Session.UniqueMaps.CHARUndergroundFacility = CreateUniqueMap_CHARUndegroundFacility(world);

            /////////////////
            // Unique Actors
            /////////////////
            if (isVerbose)
            {
                m_UI.UI_Clear(Color.Black);
                m_UI.UI_DrawStringBold(Color.White, "Generating unique actors...", 0, 0);
                m_UI.UI_Repaint();
            }
            // "Sewers Thing" - in one of the sewers
            m_Session.UniqueActors.TheSewersThing = SpawnUniqueSewersThing(world);
            // Unique survivors NPCs.
            m_Session.UniqueActors.BigBear = CreateUniqueBigBear(world);
            m_Session.UniqueActors.FamuFataru = CreateUniqueFamuFataru(world);
            m_Session.UniqueActors.Santaman = CreateUniqueSantaman(world);
            m_Session.UniqueActors.Roguedjack = CreateUniqueRoguedjack(world);
            m_Session.UniqueActors.Duckman = CreateUniqueDuckman(world);
            m_Session.UniqueActors.HansVonHanz = CreateUniqueHansVonHanz(world);

            // alpha10 Make all uniques npcs invincible until spotted
            foreach (UniqueActor uniqueActor in m_Session.UniqueActors.ToArray())
                uniqueActor.TheActor.IsInvincible = true;

            /////////////////
            // Unique Items
            /////////////////
            // "Subway Worker Badge" - somewhere on the subway tracks...
            m_Session.UniqueItems.TheSubwayWorkerBadge = SpawnUniqueSubwayWorkerBadge(world);

            //////////////////
            // Link districts
            //////////////////
            #region
            for (int x = 0; x < world.Size; x++)
            {
                for (int y = 0; y < world.Size; y++)
                {
                    if (isVerbose)
                    {
                        m_UI.UI_Clear(Color.Black);
                        m_UI.UI_DrawStringBold(Color.White, String.Format("Linking District@{0}...", World.CoordToString(x, y)), 0, 0);
                        m_UI.UI_Repaint();
                    }

                    #region Entry maps (surface)
                    // add exits (from and to).
                    Map map = world[x, y].EntryMap;

                    if (y > 0)
                    {
                        // north.
                        Map toMap = world[x, y - 1].EntryMap;
                        for (int fromX = 0; fromX < map.Width; fromX++)
                        {
                            int toX = fromX;
                            if (toX >= toMap.Width)
                                continue;
                            // link?
                            if (m_Rules.RollChance(DISTRICT_EXIT_CHANCE_PER_TILE))
                            {
                                Point ptMapFrom = new Point(fromX, -1);
                                Point ptMapTo = new Point(fromX, toMap.Height - 1);
                                Point ptFromMapFrom = new Point(fromX, toMap.Height);
                                Point ptFromMapTo = new Point(fromX, 0);
                                if (CheckIfExitIsGood(map, ptMapFrom, toMap, ptMapTo) &&
                                    CheckIfExitIsGood(toMap, ptFromMapFrom, map, ptFromMapTo))
                                {
                                    GenerateExit(map, ptMapFrom, toMap, ptMapTo);
                                    GenerateExit(toMap, ptFromMapFrom, map, ptFromMapTo);
                                }
                            }
                        }
                    }
                    if (x > 0)
                    {
                        // west.
                        Map toMap = world[x - 1, y].EntryMap;
                        for (int fromY = 0; fromY < map.Height; fromY++)
                        {
                            int toY = fromY;
                            if (toY >= toMap.Height)
                                continue;
                            // link?
                            if (m_Rules.RollChance(DISTRICT_EXIT_CHANCE_PER_TILE))
                            {
                                Point ptMapFrom = new Point(-1, fromY);
                                Point ptMapTo = new Point(toMap.Width - 1, fromY);
                                Point ptFromMapFrom = new Point(toMap.Width, fromY);
                                Point ptFromMapTo = new Point(0, fromY);
                                if (CheckIfExitIsGood(map, ptMapFrom, toMap, ptMapTo) &&
                                    CheckIfExitIsGood(toMap, ptFromMapFrom, map, ptFromMapTo))
                                {
                                    GenerateExit(map, ptMapFrom, toMap, ptMapTo);
                                    GenerateExit(toMap, ptFromMapFrom, map, ptFromMapTo);
                                }
                            }
                        }
                    }
                    #endregion

                    #region Sewers
                    map = world[x, y].SewersMap;
                    if (y > 0)
                    {
                        // north.
                        Map toMap = world[x, y - 1].SewersMap;
                        for (int fromX = 0; fromX < map.Width; fromX++)
                        {
                            int toX = fromX;
                            if (toX >= toMap.Width)
                                continue;
                            Point ptMapFrom = new Point(fromX, -1);
                            Point ptMapTo = new Point(fromX, toMap.Height - 1);
                            Point ptFromMapFrom = new Point(fromX, toMap.Height);
                            Point ptFromMapTo = new Point(fromX, 0);
                            GenerateExit(map, ptMapFrom, toMap, ptMapTo);
                            GenerateExit(toMap, ptFromMapFrom, map, ptFromMapTo);
                        }
                    }
                    if (x > 0)
                    {
                        // west.
                        Map toMap = world[x - 1, y].SewersMap;
                        for (int fromY = 0; fromY < map.Height; fromY++)
                        {
                            int toY = fromY;
                            if (toY >= toMap.Height)
                                continue;

                            Point ptMapFrom = new Point(-1, fromY);
                            Point ptMapTo = new Point(toMap.Width - 1, fromY);
                            Point ptFromMapFrom = new Point(toMap.Width, fromY);
                            Point ptFromMapTo = new Point(0, fromY);

                            GenerateExit(map, ptMapFrom, toMap, ptMapTo);
                            GenerateExit(toMap, ptFromMapFrom, map, ptFromMapTo);

                        }
                    }
                    #endregion

                    #region Subways
                    map = world[x, y].SubwayMap;
                    if (map != null)
                    {
                        if (x > 0)
                        {
                            // west.
                            Map toMap = world[x - 1, y].SubwayMap;
                            for (int fromY = 0; fromY < map.Height; fromY++)
                            {
                                int toY = fromY;
                                if (toY >= toMap.Height)
                                    continue;

                                Point ptMapFrom = new Point(-1, fromY);
                                Point ptMapTo = new Point(toMap.Width - 1, fromY);
                                Point ptFromMapFrom = new Point(toMap.Width, fromY);
                                Point ptFromMapTo = new Point(0, fromY);

                                if (!map.IsWalkable(map.Width - 1, fromY))
                                    continue;
                                if (!toMap.IsWalkable(0, fromY))
                                    continue;

                                GenerateExit(map, ptMapFrom, toMap, ptMapTo);
                                GenerateExit(toMap, ptFromMapFrom, map, ptFromMapTo);

                            }
                        }
                    }
                    #endregion
                }
            }
            #endregion

            //////////////////////////////////////////
            // Easter egg: "roguedjack was here" tag.
            //////////////////////////////////////////
            #region
            Map easterEggTagMap = world[0, 0].SewersMap;
            easterEggTagMap.RemoveMapObjectAt(1, 1);
            easterEggTagMap.GetTileAt(1, 1).RemoveAllDecorations();
            easterEggTagMap.GetTileAt(1, 1).AddDecoration(GameImages.DECO_ROGUEDJACK_TAG);
            #endregion

            //////////////////////////////
            // Spawn player on center map
            //////////////////////////////
            if (isVerbose)
            {
                m_UI.UI_Clear(Color.Black);
                m_UI.UI_DrawStringBold(Color.White, "Spawning player...", 0, 0);
                m_UI.UI_Repaint();
            }
            int gridCenter = world.Size / 2;

            Map startMap = world[gridCenter, gridCenter].EntryMap;
            GeneratePlayerOnMap(startMap, m_TownGenerator);
            SetCurrentMap(startMap);
            RefreshPlayer();
            UpdatePlayerFOV(m_Player);  // to make sure we get notified of actors acting before us in turn 0.
#if DEBUG
            AddDevCheatItems();
            AddDevCheatSkills();
            AddDevMiscStuff();
#endif

            ////////////////////////
            // Reveal starting map?
            ////////////////////////
            #region
            if (s_Options.RevealStartingDistrict)
            {
                List<Zone> startZones = startMap.GetZonesAt(m_Player.Location.Position.X, m_Player.Location.Position.Y);
                if (startZones != null)
                {
                    Zone startZone = startZones[0];
                    for (int x = 0; x < startMap.Width; x++)
                        for (int y = 0; y < startMap.Height; y++)
                        {
                            bool revealThisTile = false;

                            // reveal if:
                            // - starting zone (house).
                            // - outside.

                            // - starting zone (house).
                            List<Zone> zones = startMap.GetZonesAt(x, y);
                            if (zones != null && zones[0] == startZone)
                                revealThisTile = true;
                            else if (!startMap.GetTileAt(x, y).IsInside)
                                revealThisTile = true;

                            // reveal?
                            if (revealThisTile)
                                startMap.GetTileAt(x, y).IsVisited = true;
                        }
                }
            }
            #endregion

            /////////
            // Done.
            /////////
            if (isVerbose)
            {
                m_UI.UI_Clear(Color.Black);
                m_UI.UI_DrawStringBold(Color.White, "Generating game world... done!", 0, 0);
                m_UI.UI_Repaint();
            }
        }

        bool CheckIfExitIsGood(Map fromMap, Point from, Map toMap, Point to)
        {
            // Don't if tile not walkable or map object there.
            if (!toMap.GetTileAt(to.X, to.Y).Model.IsWalkable)
                return false;
            if (toMap.GetMapObjectAt(to.X, to.Y) != null)
                return false;

            // good spot.
            return true;
        }

        void GenerateExit(Map fromMap, Point from, Map toMap, Point to)
        {
            // add exit.
            fromMap.SetExitAt(from, new Exit(toMap, to));
        }

        #region Uniques
        UniqueActor SpawnUniqueSewersThing(World world)
        {
            ///////////////////////////////////////////////////////
            // 1. Pick a random sewers map.
            // 2. Create Sewers Thing.
            // 3. Spawn in sewers map.
            // 4. Add warning board in maintenance rooms (if any).
            ///////////////////////////////////////////////////////

            // 1. Pick a random sewers map.
            Map map = world[m_Rules.Roll(0, world.Size), m_Rules.Roll(0, world.Size)].SewersMap;

            // 2. Create Sewers Thing.
            ActorModel model = GameActors.SewersThing;
            Actor actor = model.CreateNamed(GameFactions.TheUndeads, "The Sewers Thing", false, 0);

            // 3. Spawn in sewers map.
            DiceRoller roller = new DiceRoller(map.Seed);
            bool spawned = m_TownGenerator.ActorPlace(roller, 10000, map, actor);
            if (!spawned)
                throw new InvalidOperationException("could not spawn unique The Sewers Thing");

            // 4. Add warning board in maintenance rooms (if any).
            Zone maintenanceZone = map.GetZoneByPartialName(NAME_SEWERS_MAINTENANCE);
            if (maintenanceZone != null)
            {
                m_TownGenerator.MapObjectPlaceInGoodPosition(map, maintenanceZone.Bounds,
                    (pt) => map.IsWalkable(pt.X, pt.Y) && map.GetActorAt(pt) == null && map.GetItemsAt(pt) == null,
                    roller,
                    (pt) => m_TownGenerator.MakeObjBoard(GameImages.OBJ_BOARD,
                        new string[] { "TO SEWER WORKERS :",
                                       "- It lives here.",
                                       "- Do not disturb.",
                                       "- Approach with caution.",
                                       "- Watch your back.",
                                       "- In case of emergency, take refuge here.",
                                       "- Do not let other people interact with it!"}));
            }

            // done.
            return new UniqueActor() { TheActor = actor, IsSpawned = true };
        }

        UniqueActor CreateUniqueBigBear(World world)
        {
            #region
            ActorModel model = GameActors.MaleCivilian;
            Actor actor = model.CreateNamed(GameFactions.TheCivilians, "Big Bear", false, 0);
            actor.IsUnique = true;
            // actor.Controller = new CivilianAI();// alpha10.1 defined by model like other actors

            actor.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_BIG_BEAR);

            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HAULER);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HAULER);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HAULER);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.STRONG);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.STRONG);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.STRONG);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.STRONG);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.STRONG);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.TOUGH);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.TOUGH);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.TOUGH);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.TOUGH);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.TOUGH);

            Item bat = new ItemMeleeWeapon(GameItems.UNIQUE_BIGBEAR_BAT) { IsUnique = true };
            actor.Inventory.AddAll(bat);
            actor.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
            #endregion

            // done.
            return new UniqueActor()
            {
                TheActor = actor,
                IsSpawned = false,
                IsWithRefugees = true,
                EventMessage = "You hear an angry man shouting 'FOOLS!'",
                EventThemeMusic = GameMusics.BIGBEAR_THEME_SONG
            };
        }

        UniqueActor CreateUniqueFamuFataru(World world)
        {

            #region
            ActorModel model = GameActors.FemaleCivilian;
            Actor actor = model.CreateNamed(GameFactions.TheCivilians, "Famu Fataru", false, 0);
            actor.IsUnique = true;
            // actor.Controller = new CivilianAI(); // alpha10.1 defined by model like other actors

            actor.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_FAMU_FATARU);

            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HAULER);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HAULER);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HAULER);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.AGILE);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.AGILE);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.AGILE);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.AGILE);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.AGILE);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HIGH_STAMINA);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HIGH_STAMINA);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HIGH_STAMINA);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HIGH_STAMINA);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HIGH_STAMINA);

            Item katana = new ItemMeleeWeapon(GameItems.UNIQUE_FAMU_FATARU_KATANA) { IsUnique = true };
            actor.Inventory.AddAll(katana);
            actor.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
            #endregion

            // done.
            return new UniqueActor()
            {
                TheActor = actor,
                IsSpawned = false,
                IsWithRefugees = true,
                EventMessage = "You hear a woman laughing.",
                EventThemeMusic = GameMusics.FAMU_FATARU_THEME_SONG
            };
        }

        UniqueActor CreateUniqueSantaman(World world)
        {
            #region
            ActorModel model = GameActors.MaleCivilian;
            Actor actor = model.CreateNamed(GameFactions.TheCivilians, "Santaman", false, 0);
            actor.IsUnique = true;
            // actor.Controller = new CivilianAI(); // alpha10.1 defined by model like other actors

            actor.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_SANTAMAN);

            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HAULER);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HAULER);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HAULER);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.AWAKE);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.AWAKE);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.AWAKE);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.AWAKE);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.AWAKE);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.FIREARMS);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.FIREARMS);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.FIREARMS);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.FIREARMS);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.FIREARMS);

            Item shotty = new ItemRangedWeapon(GameItems.UNIQUE_SANTAMAN_SHOTGUN) { IsUnique = true };
            actor.Inventory.AddAll(shotty);
            actor.Inventory.AddAll(m_TownGenerator.MakeItemShotgunAmmo());
            actor.Inventory.AddAll(m_TownGenerator.MakeItemShotgunAmmo());
            actor.Inventory.AddAll(m_TownGenerator.MakeItemShotgunAmmo());
            actor.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
            #endregion

            // done.
            return new UniqueActor()
            {
                TheActor = actor,
                IsSpawned = false,
                IsWithRefugees = true,
                EventMessage = "You hear christmas music and drunken vomitting.",
                EventThemeMusic = GameMusics.SANTAMAN_THEME_SONG
            };
        }

        UniqueActor CreateUniqueRoguedjack(World world)
        {
            #region
            ActorModel model = GameActors.MaleCivilian;
            Actor actor = model.CreateNamed(GameFactions.TheCivilians, "Roguedjack", false, 0);
            actor.IsUnique = true;
            // actor.Controller = new CivilianAI(); // alpha10.1 defined by model like other actors

            actor.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_ROGUEDJACK);

            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HAULER);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HAULER);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HAULER);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.LEADERSHIP);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.LEADERSHIP);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.LEADERSHIP);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.LEADERSHIP);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.LEADERSHIP);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.CHARISMATIC);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.CHARISMATIC);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.CHARISMATIC);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.CHARISMATIC);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.CHARISMATIC);

            Item basher = new ItemMeleeWeapon(GameItems.UNIQUE_ROGUEDJACK_KEYBOARD) { IsUnique = true };
            actor.Inventory.AddAll(basher);
            actor.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
            #endregion

            // done.
            return new UniqueActor()
            {
                TheActor = actor,
                IsSpawned = false,
                IsWithRefugees = true,
                EventMessage = "You hear a man shouting in French.",
                EventThemeMusic = GameMusics.ROGUEDJACK_THEME_SONG
            };
        }

        UniqueActor CreateUniqueDuckman(World world)
        {
            #region
            ActorModel model = GameActors.MaleCivilian;
            Actor actor = model.CreateNamed(GameFactions.TheCivilians, "Duckman", false, 0);
            actor.IsUnique = true;
            // actor.Controller = new CivilianAI(); // alpha10.1 defined by model like other actors

            actor.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_DUCKMAN);

            // awesome superhero!
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.CHARISMATIC);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.CHARISMATIC);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.CHARISMATIC);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.CHARISMATIC);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.CHARISMATIC);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.LEADERSHIP);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.STRONG);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.STRONG);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.STRONG);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.STRONG);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.STRONG);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HIGH_STAMINA);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HIGH_STAMINA);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HIGH_STAMINA);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HIGH_STAMINA);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HIGH_STAMINA);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.MARTIAL_ARTS);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.MARTIAL_ARTS);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.MARTIAL_ARTS);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.MARTIAL_ARTS);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.MARTIAL_ARTS);

            actor.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
            #endregion

            return new UniqueActor()
            {
                TheActor = actor,
                IsSpawned = false,
                IsWithRefugees = true,
                EventMessage = "You hear loud demented QUACKS.",
                EventThemeMusic = GameMusics.DUCKMAN_THEME_SONG
            };
        }

        UniqueActor CreateUniqueHansVonHanz(World world)
        {
            #region
            ActorModel model = GameActors.MaleCivilian;
            Actor actor = model.CreateNamed(GameFactions.TheCivilians, "Hans von Hanz", false, 0);
            actor.IsUnique = true;
            //actor.Controller = new CivilianAI(); // alpha10.1 defined by model like other actors

            actor.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_HANS_VON_HANZ);

            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HAULER);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HAULER);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HAULER);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.FIREARMS);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.FIREARMS);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.FIREARMS);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.FIREARMS);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.FIREARMS);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.LEADERSHIP);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.LEADERSHIP);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.LEADERSHIP);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.LEADERSHIP);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.LEADERSHIP);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.NECROLOGY);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.NECROLOGY);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.NECROLOGY);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.NECROLOGY);
            m_TownGenerator.GiveStartingSkillToActor(actor, Skills.IDs.NECROLOGY);

            Item pistol = new ItemRangedWeapon(GameItems.UNIQUE_HANS_VON_HANZ_PISTOL) { IsUnique = true };
            actor.Inventory.AddAll(pistol);
            actor.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
            #endregion

            // done.
            return new UniqueActor()
            {
                TheActor = actor,
                IsSpawned = false,
                IsWithRefugees = true,
                EventMessage = "You hear a man barking orders in German.",
                EventThemeMusic = GameMusics.HANS_VON_HANZ_THEME_SONG
            };
        }

        UniqueItem SpawnUniqueSubwayWorkerBadge(World world)
        {
            ///////////////////////////////////
            // 1. Pick a random Subway map.
            //    Fails if not found.
            // 2. Pick a position in the rails.
            // 3. Drop it.
            ///////////////////////////////////

            Item it = new Item(GameItems.UNIQUE_SUBWAY_BADGE) { IsUnique = true, IsForbiddenToAI = true };

            // 1. Pick a random Subway map.
            List<Map> allSubways = new List<Map>();
            for (int x = 0; x < world.Size; x++)
                for (int y = 0; y < world.Size; y++)
                    if (world[x, y].HasSubway)
                        allSubways.Add(world[x, y].SubwayMap);
            if (allSubways.Count == 0)
                return new UniqueItem() { TheItem = it, IsSpawned = false };
            Map subway = allSubways[m_Rules.Roll(0, allSubways.Count)];

            // 2. Pick a position in the rails.
            Rectangle railsRect = subway.GetZoneByPartialName(NAME_SUBWAY_RAILS).Bounds;
            Point dropPt = new Point(m_Rules.Roll(railsRect.Left, railsRect.Right), m_Rules.Roll(railsRect.Top, railsRect.Bottom));

            // 3. Drop it.
            subway.DropItemAt(it, dropPt);
            // blood! deceased worker.
            subway.GetTileAt(dropPt).AddDecoration(GameImages.DECO_BLOODIED_FLOOR);

            // done.
            return new UniqueItem() { TheItem = it, IsSpawned = true };
        }

        UniqueMap CreateUniqueMap_CHARUndegroundFacility(World world)
        {
            ////////////////////////////////////////////////
            // 1. Find all business districts with offices.
            // 2. Pick one business district at random.
            // 3. Generate underground map there.
            ////////////////////////////////////////////////

            // 1. Find all business districts with offices.
            List<District> goodDistricts = null;
            for (int x = 0; x < world.Size; x++)
                for (int y = 0; y < world.Size; y++)
                {
                    if (world[x, y].Kind == DistrictKind.BUSINESS)
                    {
                        bool hasOffice = false;
                        foreach (Zone z in world[x, y].EntryMap.Zones)
                        {
                            if (z.HasGameAttribute(ZoneAttributes.IS_CHAR_OFFICE))
                            {
                                hasOffice = true;
                                break;
                            }
                        }
                        if (hasOffice)
                        {
                            if (goodDistricts == null)
                                goodDistricts = new List<District>();
                            goodDistricts.Add(world[x, y]);
                        }
                    }
                }

            // 2. Pick one business district at random.
            if (goodDistricts == null)
            {
                throw new InvalidOperationException("world has no business districts with offices");
            }
            District chosenDistrict = goodDistricts[m_Rules.Roll(0, goodDistricts.Count)];

            // 3. Generate underground map there.
            List<Zone> offices = new List<Zone>();
            foreach (Zone z in chosenDistrict.EntryMap.Zones)
            {
                if (z.HasGameAttribute(ZoneAttributes.IS_CHAR_OFFICE))
                {
                    offices.Add(z);
                }
            }
            Zone chosenOffice = offices[m_Rules.Roll(0, offices.Count)];
            Point baseEntryPos;  // alpha10
            Map map = m_TownGenerator.GenerateUniqueMap_CHARUnderground(chosenDistrict.EntryMap, chosenOffice, out baseEntryPos);
            map.District = chosenDistrict;
            map.Name = String.Format("CHAR Underground Facility @{0}-{1}", baseEntryPos.X, baseEntryPos.Y); // alpha10
            chosenDistrict.AddUniqueMap(map);
            return new UniqueMap() { TheMap = map };

        }
        #endregion

        #region District Maps
        DistrictKind GenerateDistrictKind(World world, int gridX, int gridY)
        {
            // Decide district kind - some districts are harcoded:
            //   - (0,0) : always Business.
            if (gridX == 0 && gridY == 0)
                return DistrictKind.BUSINESS;
            else
                return (DistrictKind)m_Rules.Roll((int)DistrictKind._FIRST, (int)DistrictKind._COUNT);
        }

        Map GenerateDistrictEntryMap(World world, District district, Point policeStationDistrictPos, Point hospitalDistrictPos)
        {
            int gridX = district.WorldPosition.X;
            int gridY = district.WorldPosition.Y;

            ///////////////////////////
            // 1. Compute unique seed.
            // 2. Set params for kind.
            // 3. Generate map.
            ///////////////////////////

            // 1. Compute unique seed.
            int gridSeed = m_Session.Seed + gridY * world.Size + gridX;

            // 3. Set gen params.
            #region
            BaseTownGenerator.Parameters genParams = BaseTownGenerator.DEFAULT_PARAMS;
            genParams.MapWidth = genParams.MapHeight = s_Options.DistrictSize;
            genParams.District = district;
            int factor = 8;
            string kindName = "District";
            switch (district.Kind)
            {
                case DistrictKind.SHOPPING:
                    // more shops, less other types.
                    kindName = "Shopping District";
                    genParams.CHARBuildingChance /= factor;
                    genParams.ShopBuildingChance *= factor;
                    genParams.ParkBuildingChance /= factor;
                    break;
                case DistrictKind.GREEN:
                    // more parks, less other types.
                    kindName = "Green District";
                    genParams.CHARBuildingChance /= factor;
                    genParams.ParkBuildingChance *= factor;
                    genParams.ShopBuildingChance /= factor;
                    break;
                case DistrictKind.BUSINESS:
                    // more offices, less other types.
                    kindName = "Business District";
                    genParams.CHARBuildingChance *= factor;
                    genParams.ParkBuildingChance /= factor;
                    genParams.ShopBuildingChance /= factor;
                    break;
                case DistrictKind.RESIDENTIAL:
                    // more housings, less other types.
                    kindName = "Residential District";
                    genParams.CHARBuildingChance /= factor;
                    genParams.ParkBuildingChance /= factor;
                    genParams.ShopBuildingChance /= factor;
                    break;
                case DistrictKind.GENERAL:
                    // use default params.
                    kindName = "District";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("unhandled district kind");
            }

            // Special params.
            genParams.GeneratePoliceStation = (district.WorldPosition == policeStationDistrictPos);
            genParams.GenerateHospital = (district.WorldPosition == hospitalDistrictPos);
            #endregion

            // 4. Generate map.
            BaseTownGenerator.Parameters prevParams = m_TownGenerator.Params;
            m_TownGenerator.Params = genParams;
            Map map = m_TownGenerator.Generate(gridSeed);
            map.Name = String.Format("{0}@{1}", kindName, World.CoordToString(gridX, gridY));
            m_TownGenerator.Params = prevParams;

            // done.
            return map;
        }

        Map GenerateDistrictSewersMap(District district)
        {
            // Compute uniqueseed.
            int sewersSeed = (district.EntryMap.Seed << 1) ^ district.EntryMap.Seed;

            // Generate map.
            Map sewers = m_TownGenerator.GenerateSewersMap(sewersSeed, district);
            sewers.Name = String.Format("Sewers@{0}-{1}", district.WorldPosition.X, district.WorldPosition.Y);

            // done.
            return sewers;
        }

        Map GenerateDistrictSubwayMap(District district)
        {
            // Compute uniqueseed.
            int subwaySeed = (district.EntryMap.Seed << 2) ^ district.EntryMap.Seed;

            // Generate map.
            Map subway = m_TownGenerator.GenerateSubwayMap(subwaySeed, district);
            subway.Name = String.Format("Subway@{0}-{1}", district.WorldPosition.X, district.WorldPosition.Y);

            // done.
            return subway;
        }
        #endregion

        void GeneratePlayerOnMap(Map map, BaseTownGenerator townGen)
        {
            DiceRoller roller = new DiceRoller(map.Seed);

            /////////////////////////////////////////////////////
            // Create player actor : living/undead x male/female
            /////////////////////////////////////////////////////
            #region
            ActorModel playerModel;
            Actor player;
            if (m_CharGen.IsUndead)
            {
                // Handle specific undead type.
                // Zombified : need living, then zombify.
                switch (m_CharGen.UndeadModel)
                {
                    case GameActors.IDs.UNDEAD_SKELETON:
                        {
                            // Create the Skeleton.
                            player = m_GameActors.Skeleton.CreateNumberedName(m_GameFactions.TheUndeads, 0);
                            break;
                        }

                    case GameActors.IDs.UNDEAD_ZOMBIE:
                        {
                            // Create the Zombie.
                            player = m_GameActors.Zombie.CreateNumberedName(m_GameFactions.TheUndeads, 0);
                            break;
                        }

                    case GameActors.IDs.UNDEAD_MALE_ZOMBIFIED:
                    case GameActors.IDs.UNDEAD_FEMALE_ZOMBIFIED:
                        {
                            // First create as living.
                            playerModel = m_CharGen.IsMale ? m_GameActors.MaleCivilian : m_GameActors.FemaleCivilian;
                            player = playerModel.CreateAnonymous(m_GameFactions.TheCivilians, 0);
                            townGen.DressCivilian(roller, player);
                            townGen.GiveNameToActor(roller, player);
                            // Then zombify.
                            player = Zombify(null, player, true);
                            break;
                        }

                    case GameActors.IDs.UNDEAD_ZOMBIE_MASTER:
                        {
                            // Create the ZM.
                            player = m_GameActors.ZombieMaster.CreateNumberedName(m_GameFactions.TheUndeads, 0);
                            break;
                        }
                    default:
                        throw new ArgumentOutOfRangeException("unhandled undeadModel");
                }

                // Then make sure player related stuff are setup properly.
                PrepareActorForPlayerControl(player);
            }
            else
            {
                // Create living.
                playerModel = m_CharGen.IsMale ? m_GameActors.MaleCivilian : m_GameActors.FemaleCivilian;
                player = playerModel.CreateAnonymous(m_GameFactions.TheCivilians, 0);
                townGen.DressCivilian(roller, player);
                townGen.GiveNameToActor(roller, player);
                player.Sheet.SkillTable.AddOrIncreaseSkill((int)m_CharGen.StartingSkill);

                townGen.RecomputeActorStartingStats(player);
                OnSkillUpgrade(player, m_CharGen.StartingSkill);
                // slightly randomize Food and Sleep - 0..25%.
                int foodDeviation = (int)(0.25f * player.FoodPoints);
                player.FoodPoints = player.FoodPoints - m_Rules.Roll(0, foodDeviation);
                int sleepDeviation = (int)(0.25f * player.SleepPoints);
                player.SleepPoints = player.SleepPoints - m_Rules.Roll(0, sleepDeviation);
            }

            player.Controller = new PlayerController();
            #endregion

            /////////////
            // Spawn him.
            /////////////
            #region
            // living: try to spawn inside on a couch, then if failed spawn anywhere inside.
            // undead: spawn outside.
            // NEVER spawn in CHAR Office!!
            bool preferedSpawnOk = townGen.ActorPlace(roller, 10 * map.Width * map.Height, map, player,
                (pt) =>
                {
                    bool isInside = map.GetTileAt(pt.X, pt.Y).IsInside;
                    if ((m_CharGen.IsUndead && isInside) || (!m_CharGen.IsUndead && !isInside))
                        return false;

                    if (IsInCHAROffice(new Location(map, pt)))
                        return false;

                    MapObject mapObj = map.GetMapObjectAt(pt);
                    if (m_CharGen.IsUndead)
                        return mapObj == null;
                    else
                        return mapObj != null && mapObj.IsCouch;
                });

            if (!preferedSpawnOk)
            {
                // no couch, try inside but never in char office.
                bool spawnedInside = townGen.ActorPlace(roller, map.Width * map.Height, map, player,
                    (pt) => map.GetTileAt(pt.X, pt.Y).IsInside && !IsInCHAROffice(new Location(map, pt)));

                if (!spawnedInside)
                {
                    // could not spawn inside, do it outside...
                    while (!townGen.ActorPlace(roller, int.MaxValue, map, player, (pt) => !IsInCHAROffice(new Location(map, pt))))
                        ;
                }
            }
            #endregion
        }

        void RefreshPlayer()
        {
            // get player.
            foreach (Actor a in m_Session.CurrentMap.Actors)
            {
                if (a.IsPlayer)
                {
                    m_Player = a;
                    break;
                }
            }

            // compute view.
            if (m_Player != null)
                ComputeViewRect(m_Player.Location.Position);

        }

        void PrepareActorForPlayerControl(Actor newPlayerAvatar)
        {
            // inventory && skills.
            if (newPlayerAvatar.Inventory == null)
                newPlayerAvatar.Inventory = new Inventory(1);
            if (newPlayerAvatar.Sheet.SkillTable == null)
                newPlayerAvatar.Sheet.SkillTable = new SkillTable();

            // if follower, leave leader.
            if (newPlayerAvatar.Leader != null)
                newPlayerAvatar.Leader.RemoveFollower(newPlayerAvatar);
        }
        #endregion

        #region Changing map & district

        void SetCurrentMap(Map map)
        {
            // set session field.
            m_Session.CurrentMap = map;

            // alpha10 update background music
            UpdateBgMusic();
        }

        void OnPlayerLeaveDistrict()
        {
            // remember when we left the district.
            m_Session.CurrentMap.LocalTime.TurnCounter = m_Session.WorldTime.TurnCounter;
        }

        void BeforePlayerEnterDistrict(District district)
        {
            // get entry map.
            Map entryMap = district.EntryMap;

            // get when we left the district.
            int lastTime = entryMap.LocalTime.TurnCounter;

            // if option set, simulate to catch current turn.
            // otherwise just jump int time.
            #region
            if (s_Options.IsSimON)
            {
                int catchupTo = m_Session.WorldTime.TurnCounter;  // alpha10
                int turnsToCatchup = catchupTo - entryMap.LocalTime.TurnCounter; // alpha10

                StopSimThread(false);  // alpha10

                if (turnsToCatchup > 0)
                {
                    // music.
                    m_MusicManager.Stop();
                    m_MusicManager.PlayLooping(GameMusics.INTERLUDE, MusicPriority.PRIORITY_EVENT);

                    // force player view to darkness (so he gets no messages).
                    if (m_Player != null)
                    {
                        m_Player.Location.Map.ClearView();
                        entryMap.ClearView();
                    }

                    // alpha10 nope not here 
                    // stop simulation thread & get mutex.
                    //StopSimThread();
                    ////Monitor.Enter(m_SimMutex);  // alpha10 obsolete

                    // simulate loop.
                    #region
                    double timerStart = DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
                    double lastRedraw = 0;
                    bool aborted = false;
                    while (entryMap.LocalTime.TurnCounter < catchupTo) // alpha10 changed from <= to <
                    {
                        double timerNow = DateTime.UtcNow.TimeOfDay.TotalMilliseconds;

                        // time to redraw?
                        bool doRedraw = (entryMap.LocalTime.TurnCounter == m_Session.WorldTime.TurnCounter) ||      // show last turn
                            entryMap.LocalTime.TurnCounter == lastTime ||                                           // show 1st turn
                            timerNow >= lastRedraw + 1000;                                                          // show every seconds

                        // redraw?
                        #region
                        if (doRedraw)
                        {
                            // remember we redrawed.
                            lastRedraw = timerNow;

                            // show.
                            ClearMessages();
                            AddMessage(new Message(String.Format("Simulating district, please wait {0}/{1}...", entryMap.LocalTime.TurnCounter, m_Session.WorldTime.TurnCounter), m_Session.WorldTime.TurnCounter, Color.White));
                            AddMessage(new Message("(this is an option you can tune)", m_Session.WorldTime.TurnCounter, Color.White));

                            // estimate turns per seconds and time left.
                            int turnsDone = entryMap.LocalTime.TurnCounter - lastTime;
                            if (turnsDone > 1)
                            {
                                int turnsLeft = m_Session.WorldTime.TurnCounter - entryMap.LocalTime.TurnCounter;
                                double turnsPerSecs = 1000.0f * (float)turnsDone / (1 + timerNow - timerStart);
                                AddMessage(new Message(String.Format("Turns per second    : {0:F2}.", turnsPerSecs), m_Session.WorldTime.TurnCounter, Color.White));

                                int secsLeft = (int)(turnsLeft / turnsPerSecs);
                                int mins = secsLeft / 60;
                                int secs = secsLeft % 60;
                                string etaFormat = (mins > 0 ? String.Format("{0} min {1:D2} secs", mins, secs) : String.Format("{0} secs", secs));
                                AddMessage(new Message(String.Format("Estimated time left : {0}.", etaFormat), m_Session.WorldTime.TurnCounter, Color.White));
                            }
                            if (aborted)
                                AddMessage(new Message("Simulation aborted!", m_Session.WorldTime.TurnCounter, Color.Red));
                            else
                                AddMessage(new Message("<keep ESC pressed to abort the simulation>", m_Session.WorldTime.TurnCounter, Color.Yellow));
                            RedrawPlayScreen();
                        }
                        #endregion

                        // aborted?
                        if (aborted)
                            break;

                        // check for abort.
                        #region
                        KeyEventArgs key = m_UI.UI_PeekKey();
                        if (key != null && key.KeyCode == Keys.Escape)
                        {
                            // jump in time for each map.
                            foreach (Map map in district.Maps)
                                map.LocalTime.TurnCounter = m_Session.WorldTime.TurnCounter;
                            // abort!
                            aborted = true;
                        }
                        #endregion

                        // if not aborted, simulate the district.
                        if (!aborted)
                        {
                            // sim the district.
                            SimulateDistrict(district);
                        }
                    }
                    #endregion

                    // alpha10 obsolete and fix
                    //// release mutex and restart sim thread.
                    //Monitor.Exit(m_SimMutex); // alpha10 obsolete
                    //RestartSimThread();  // alpha10 no no no! restart AFTER chaging the player district duh!

                    // Sim ends - either aborted or normal end.
                    #region
                    // remove "ESC" message.
                    RemoveLastMessage();

                    // since sim arbitrary messes with actor APs, we're not quite sure were they are now.
                    // so force them back to zero to have a clean start.
                    foreach (Map map in district.Maps)
                        foreach (Actor a in map.Actors)
                            if (!a.IsSleeping)
                                a.ActionPoints = 0;

                    // stop music.
                    m_MusicManager.Stop();
                    #endregion
                }  // sim has catchup to do
            } // sim on
            else
            {
                // jump in time for each map.
                foreach (Map map in district.Maps)
                    map.LocalTime.TurnCounter = m_Session.WorldTime.TurnCounter;
            }
            #endregion
        }

        // alpha10
        void AfterPlayerEnterDistrict()
        {
            // restart sim thread if on
            if (s_Options.IsSimON && s_Options.SimThread)
                StartSimThread();
        }

        void OnPlayerChangeMap()
        {
            RefreshPlayer();
        }
        #endregion

        #region Simulation
        SimFlags ComputeSimFlagsForTurn(int turn)
        {
            bool loDetail = false;

            switch (s_Options.SimulateDistricts)
            {
                case GameOptions.SimRatio.FULL:
                    loDetail = false;
                    break;
                case GameOptions.SimRatio.THREE_QUARTER:    // 3/4, skip 1 out of 4.
                    loDetail = (turn % 4 == 3);
                    break;
                case GameOptions.SimRatio.TWO_THIRDS:    // 2/3, skip 1 out of 3.
                    loDetail = (turn % 3 == 2);
                    break;
                case GameOptions.SimRatio.HALF:    // 1/2, skip 1 out of 2.
                    loDetail = (turn % 2 == 1);
                    break;
                case GameOptions.SimRatio.ONE_THIRD:    // 1/3, play 1 out of 3.
                    loDetail = (turn % 3 != 0);
                    break;
                case GameOptions.SimRatio.ONE_QUARTER:    // 1/4, play 1 out of 4.
                    loDetail = (turn % 4 != 0);
                    break;
                case GameOptions.SimRatio.OFF:
                    loDetail = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("unhandled simRatio");
            }

            return loDetail ? SimFlags.LODETAIL_TURN : SimFlags.HIDETAIL_TURN;
        }

        void SimulateDistrict(District d)
        {
            AdvancePlay(d, ComputeSimFlagsForTurn(d.EntryMap.LocalTime.TurnCounter));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        /// <returns>true if simulated a district; false if didn't need to simulate.</returns>
        bool SimulateNearbyDistricts(District d)
        {
            bool hadToSim = false;
            int xmin = d.WorldPosition.X - 1;
            int xmax = d.WorldPosition.X + 1;
            int ymin = d.WorldPosition.Y - 1;
            int ymax = d.WorldPosition.Y + 1;
            m_Session.World.TrimToBounds(ref xmin, ref ymin);
            m_Session.World.TrimToBounds(ref xmax, ref ymax);

            for (int dx = xmin; dx <= xmax; dx++)
                for (int dy = ymin; dy <= ymax; dy++)
                {
                    // don't sim same district!
                    if (dx == d.WorldPosition.X && dy == d.WorldPosition.Y)
                        continue;

                    District otherDistrict = m_Session.World[dx, dy];

                    // don't sim if up to date!
                    int dTurns = d.EntryMap.LocalTime.TurnCounter - otherDistrict.EntryMap.LocalTime.TurnCounter;
                    if (dTurns > 0)
                    {
                        //Logger.WriteLine(Logger.Stage.RUN_MAIN, "sim has to catch " + dTurns + " turns");
                        // simulate district.
                        hadToSim = true;
                        SimulateDistrict(otherDistrict);
                        //Console.Out.WriteLine("  sim simulated district " + otherDistrict.Name+ " turn now "+otherDistrict.EntryMap.LocalTime.TurnCounter);
                    }
                    //else  // DEBUG
                    //    Console.Out.WriteLine("  sim district " + otherDistrict.Name + " is up to date " + otherDistrict.EntryMap.LocalTime.TurnCounter);
                }

            return hadToSim;
        }
        #endregion

        #region Simulation Thread
        // alpha10 obsolete, we do it "manually" in some places
        //void RestartSimThread()
        //{
        //    StopSimThread();
        //    StartSimThread();
        //}

        void StartSimThread()
        {
            if (s_Options.IsSimON && s_Options.SimThread)
            {
                Logger.WriteLine(Logger.Stage.RUN_MAIN, "starting sim...");

                if (m_SimThread == null)
                {
                    Logger.WriteLine(Logger.Stage.RUN_MAIN, "...allocating sim thread");
                    m_SimThread = new Thread(new ThreadStart(SimThreadProc));
                    m_SimThread.Name = "Simulation Thread";
                }
                else
                {
                    Logger.WriteLine(Logger.Stage.RUN_MAIN, "...sim thread already allocated");
                }

                Logger.WriteLine(Logger.Stage.RUN_MAIN, "...sim thread start.");
                lock (m_SimStateLock) { m_SimThreadDoRun = true; }; // alpha10
                m_SimThread.Start();
            }
        }

        // alpha10 StopSimThread is now blocking until the sim thread has actually stopped
        // allowed to abort when ending a game or dying because of weird bug in release build where the sim thread 
        // doesnt want to stop when dying as undead and we have to abort it(!)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="abort">true to stop the thread by aborting, false to stop it cleanly (recommended)</param>
        void StopSimThread(bool abort)
        {
            Logger.WriteLine(Logger.Stage.RUN_MAIN, "stopping & clearing sim thread...");

            if (m_SimThread != null)
            {
                // abort thread if asked to otherwise stop it cleanly
                if (abort)
                {
                    Logger.WriteLine(Logger.Stage.RUN_MAIN, "...aborting sim thread");
                    try
                    {
                        m_SimThread.Abort();
                    }
                    catch (Exception e)
                    {
                        Logger.WriteLine(Logger.Stage.RUN_MAIN, "...exception when aborting (ignored) " + e.Message);
                    }
                    m_SimThread = null;
                    m_SimThreadDoRun = false;
                }
                else
                {
                    // try to stop cleanly
                    Logger.WriteLine(Logger.Stage.RUN_MAIN, "...telling sim thread to stop");
                    lock (m_SimStateLock) { m_SimThreadDoRun = false; };
                    Logger.WriteLine(Logger.Stage.RUN_MAIN, "...sim thread told to stop");
                    for (; ; )
                    {
                        Logger.WriteLine(Logger.Stage.RUN_MAIN, "...waiting for sim thread to stop");
                        Thread.Sleep(10);
                        bool stopped = false;
                        lock (m_SimStateLock) { stopped = !m_SimThreadIsWorking; }
                        if (!stopped && !m_SimThread.IsAlive)
                        {
                            Logger.WriteLine(Logger.Stage.RUN_MAIN, "...sim thread is not alive and did not stop properly, consider it stopped");
                            stopped = true;
                        }
                        if (stopped)
                            break;
                    }
                    Logger.WriteLine(Logger.Stage.RUN_MAIN, "...sim thread has stopped");
                    m_SimThread = null;
                }
            }

            Logger.WriteLine(Logger.Stage.RUN_MAIN, "stopping & clearing sim thread done!");
        }

        void SimThreadProc()
        {
            Logger.WriteLine(Logger.Stage.RUN_MAIN, "sim thread: starting loop");

            District playerDistrict = m_Player.Location.Map.District;  // alpha10

            lock (m_SimStateLock) { m_SimThreadIsWorking = true; }  // alpha10

            for (; ; )  // alpha10
            //while (true)
            {
                //Console.Out.WriteLine("sim thread loop");
                // alpha10
                bool stop = false;
                lock (m_SimStateLock) { stop = !m_SimThreadDoRun; }
                if (stop)
                    break;

                Thread.Sleep(10);
                //Monitor.Enter(m_SimMutex); // alpha10 obsolete
                try
                {
                    if (m_Player != null)
                    {
                        SimulateNearbyDistricts(playerDistrict);
                    }
                }
                catch (Exception e)
                {
                    Logger.WriteLine(Logger.Stage.RUN_MAIN, "sim thread: exception while running sim thread!");
                    Logger.WriteLine(Logger.Stage.RUN_MAIN, "sim thread: " + e.Message);
                    // stop sim thread, better than crashing i guess...
                    break;
                }
                //finally
                //{
                //    Monitor.Exit(m_SimMutex); // alpha10 obsolete
                //}
            }

            Logger.WriteLine(Logger.Stage.RUN_MAIN, "sim thread: told to stop, stoping work");
            lock (m_SimStateLock) { m_SimThreadIsWorking = false; }
            Logger.WriteLine(Logger.Stage.RUN_MAIN, "sim thread: working stopped");
        }
        #endregion

        #region Achievement banner
        void ShowNewAchievement(Achievement.IDs id)
        {
            // one more achievement.
            ++m_Session.Scoring.CompletedAchievementsCount;

            // get data.
            Achievement ach = m_Session.Scoring.GetAchievement(id);
            string musicToPlay = ach.MusicID;
            string title = ach.Name;
            string[] text = ach.Text;

            // add event.
            m_Session.Scoring.AddEvent(m_Session.WorldTime.TurnCounter, String.Format("** Achievement : {0} for {1} points. **", title, ach.ScoreValue));

            // music.
            m_MusicManager.Stop();
            m_MusicManager.Play(musicToPlay, MusicPriority.PRIORITY_EVENT);

            // prepare banner.
            int longestLine = FindLongestLine(text);
            string starsLine = new string('*', Math.Max(longestLine, 50));
            List<string> lines = new List<string>(text.Length + 3 + 2);
            lines.Add(starsLine);
            lines.Add(String.Format("ACHIEVEMENT : {0}", title));
            lines.Add("CONGRATULATIONS!");
            for (int i = 0; i < text.Length; i++)
                lines.Add(text[i]);
            lines.Add(String.Format("Achievements : {0}/{1}.", m_Session.Scoring.CompletedAchievementsCount, Scoring.MAX_ACHIEVEMENTS));
            lines.Add(starsLine);

            // banner.
            Point pos = new Point(0, 0);
            AddOverlay(new OverlayPopup(lines.ToArray(), Color.Gold, Color.Gold, Color.DimGray, pos));
            ClearMessages();
            if (!m_Player.IsBotPlayer)
                AddMessagePressEnter();
            ClearOverlays();
        }
        #endregion

        #region Special player events
        void ShowSpecialDialogue(Actor speaker, string[] text)
        {
            // music.
            m_MusicManager.Stop();
            m_MusicManager.Play(GameMusics.INTERLUDE, MusicPriority.PRIORITY_EVENT);

            // overlays.
            AddOverlay(new OverlayPopup(text, Color.Gold, Color.Gold, Color.DimGray, new Point(0, 0)));
            AddOverlay(new OverlayRect(Color.Yellow, new Rectangle(MapToScreen(speaker.Location.Position), new Size(TILE_SIZE, TILE_SIZE))));

            // message & wait enter.
            ClearMessages();
            if (!m_Player.IsBotPlayer)
                AddMessagePressEnter();
            ClearOverlays();  // alpha10 fix
            m_MusicManager.Stop();
        }

        void CheckSpecialPlayerEventsAfterAction(Actor player)
        {
            //////////////////////////////////////////////////////////
            //
            // Special events - limited to some factions/actors
            // 1. Breaking into CHAR office for the 1st time: !undead !char
            // 2. Visiting CHAR Underground facility for the 1st time.
            // 3. Sighting The Sewers Thing : !thesewersthing
            // 4. Police Station script.
            // 5. Sighting Jason Myers : !jasonmyers
            // 6. Sighting Duckman // alpha10 disabled
            //
            // Generic 1st Time flags :
            // 1. Visiting a new map.
            // 2. Sighting an actor : actor model, unique NPCs.
            // 
            // Item interactions :
            // 1. Subway Worker Badge in Subway maps.
            //////////////////////////////////////////////////////////


            #region Special events
            // 1. Breaking into CHAR office for the 1st time: !undead !char
            #region
            if (!player.Model.Abilities.IsUndead && player.Faction != GameFactions.TheCHARCorporation)
            {
                if (!m_Session.Scoring.HasCompletedAchievement(Achievement.IDs.CHAR_BROKE_INTO_OFFICE))
                {
                    if (IsInCHAROffice(player.Location))
                    {
                        // completed.
                        m_Session.Scoring.SetCompletedAchievement(Achievement.IDs.CHAR_BROKE_INTO_OFFICE);

                        // achievement!
                        ShowNewAchievement(Achievement.IDs.CHAR_BROKE_INTO_OFFICE);
                    }
                }
            }
            #endregion

            // 2. Visiting CHAR Underground facility for the 1st time.
            #region

            if (!m_Session.Scoring.HasCompletedAchievement(Achievement.IDs.CHAR_FOUND_UNDERGROUND_FACILITY))
            {
                if (player.Location.Map == m_Session.UniqueMaps.CHARUndergroundFacility.TheMap)
                {
                    lock (m_Session) // thread safe
                    {
                        // completed.
                        m_Session.Scoring.SetCompletedAchievement(Achievement.IDs.CHAR_FOUND_UNDERGROUND_FACILITY);

                        // achievement!
                        ShowNewAchievement(Achievement.IDs.CHAR_FOUND_UNDERGROUND_FACILITY);

                        // make sure the player knows about it now and it is activated.
                        m_Session.PlayerKnows_CHARUndergroundFacilityLocation = true;
                        m_Session.CHARUndergroundFacility_Activated = true;
                        m_Session.UniqueMaps.CHARUndergroundFacility.TheMap.IsSecret = false;

                        // open the exit to and from surface for AIs.
                        Map surfaceMap = m_Session.UniqueMaps.CHARUndergroundFacility.TheMap.District.EntryMap;
                        Point? surfaceEntry = surfaceMap.FindFirstInMap(
                            (pt) =>
                            {
                                Exit e = surfaceMap.GetExitAt(pt);
                                if (e == null)
                                    return false;
                                return e.ToMap == m_Session.UniqueMaps.CHARUndergroundFacility.TheMap;
                            });
                        if (surfaceEntry == null)
                            throw new InvalidOperationException("could not find exit to CUF in surface map");
                        Exit toCUF = surfaceMap.GetExitAt(surfaceEntry.Value);
                        toCUF.IsAnAIExit = true;

                        Point? cufExit = m_Session.UniqueMaps.CHARUndergroundFacility.TheMap.FindFirstInMap(
                            (pt) =>
                            {
                                Exit e = m_Session.UniqueMaps.CHARUndergroundFacility.TheMap.GetExitAt(pt);
                                if (e == null)
                                    return false;
                                return e.ToMap == surfaceMap;
                            });
                        if (cufExit == null)
                            throw new InvalidOperationException("could not find exit to surface in CUF map");
                        Exit fromCUF = m_Session.UniqueMaps.CHARUndergroundFacility.TheMap.GetExitAt(cufExit.Value);
                        fromCUF.IsAnAIExit = true;
                    }
                }
            }
            #endregion

            // 3. Sighting The Sewers Thing : !thesewersthing
            #region
            if (player != m_Session.UniqueActors.TheSewersThing.TheActor)
            {
                if (!m_Session.PlayerKnows_TheSewersThingLocation &&
                    player.Location.Map == m_Session.UniqueActors.TheSewersThing.TheActor.Location.Map &&
                    !m_Session.UniqueActors.TheSewersThing.TheActor.IsDead)
                {
                    if (IsVisibleToPlayer(m_Session.UniqueActors.TheSewersThing.TheActor))
                    {
                        lock (m_Session) // thread safe
                        {
                            m_Session.PlayerKnows_TheSewersThingLocation = true;

                            // message + music, so the player notices it.
                            m_MusicManager.Stop();
                            m_MusicManager.Play(GameMusics.FIGHT, MusicPriority.PRIORITY_EVENT);
                            ClearMessages();
                            AddMessage(new Message("Hey! What's that THING!?", m_Session.WorldTime.TurnCounter, Color.Yellow));
                            if (!m_Player.IsBotPlayer)
                                AddMessagePressEnter();
                        }
                    }
                }
            }
            #endregion

            // 4. Police Station script.
            #region
            if (player.Location.Map == m_Session.UniqueMaps.PoliceStation_JailsLevel.TheMap &&
                !m_Session.UniqueActors.PoliceStationPrisoner.TheActor.IsDead)
            {
                Actor prisoner = m_Session.UniqueActors.PoliceStationPrisoner.TheActor;
                Map map = player.Location.Map;
                switch (m_Session.ScriptStage_PoliceStationPrisoner)
                {
                    case ScriptStage.STAGE_0:   // nothing happened yet, waiting to offer deal.
                        // alpha10.1 check if near prisoner, not generator because prisoner can now spawn in any of the cells
                        //           also slightly modified what he/she says.
                        /////////////////////////////////////////////
                        // Player is near the prisoner : offer deal.
                        /////////////////////////////////////////////
                        if (m_Rules.GridDistance(player.Location.Position, prisoner.Location.Position) <= 2 &&
                            //map.HasAnyAdjacentInMap(player.Location.Position, (pt) => map.GetMapObjectAt(pt) is PowerGenerator) &&
                            !prisoner.IsSleeping &&
                            IsVisibleToPlayer(prisoner))  // alpha10 fix: and visible!
                        {
                            lock (m_Session) // thread safe
                            {
                                // Offer deal.
                                string[] text = new string[]
                                {
                                    "\" Psssst! Hey! You over there! \"",
                                    String.Format("{0} is discretly calling you from {1} cell. You listen closely...", prisoner.Name, HisOrHer(prisoner)),
                                    "\" Listen! I shouldn't be here! Just drove a bit too fast!",
                                    "  Look, I know what's happening! I worked down there! At the CHAR facility!",
                                    "  They didn't want me to leave but I did! Like I'm stupid enough to stay down there uh?",
                                    "  Now listen! Let's make a deal...",
                                    "  Stupid cops won't listen to me. You look clever...",
                                    "  You just have to push the button at the end of the corridor to open my cell.",
                                    "  The cops are too busy to care about small fish like me!",
                                    "  Then I'll tell you where is the underground facility and just get the hell out of here.",
                                    "  I don't give a fuck about CHAR anymore, you can do what you want with that!",
                                    "  There are plenty of cool stuff to loot down there!",
                                    "  Do it PLEASE! I REALLY shoudn't be there! \"",
                                    String.Format("Looks like {0} wants you to turn the generator on to open the cells...", HeOrShe(prisoner))
                                };
                                ShowSpecialDialogue(prisoner, text);

                                // Scoring event.
                                m_Session.Scoring.AddEvent(m_Session.WorldTime.TurnCounter, String.Format("{0} offered a deal.", prisoner.Name));

                                // Next stage.
                                m_Session.ScriptStage_PoliceStationPrisoner = ScriptStage.STAGE_1;
                            }
                        }
                        break;

                    case ScriptStage.STAGE_1:  // offered deal, waiting for opened cell.
                        ///////////////////////////////////////////////
                        // Wait to get out of cell and next to player.
                        ///////////////////////////////////////////////
                        if (!map.HasZonePartiallyNamedAt(prisoner.Location.Position, NAME_POLICE_STATION_JAILS_CELL) &&
                            m_Rules.IsAdjacent(player.Location.Position, prisoner.Location.Position) &&
                            !prisoner.IsSleeping)
                        {
                            lock (m_Session) // thread safe
                            {
                                // Thank you and give info.
                                string[] text = new string[]
                                {
                                    "\" Thank you! Thank you so much!",
                                    "  As promised, I'll tell you the big secret!",
                                    String.Format("  The CHAR Underground Facility is in district {0}.", World.CoordToString(m_Session.UniqueMaps.CHARUndergroundFacility.TheMap.District.WorldPosition.X,  m_Session.UniqueMaps.CHARUndergroundFacility.TheMap.District.WorldPosition.Y)),
                                    "  Look for a CHAR Office, a room with an iron door.",
                                    "  Now I must hurry! Thanks a lot for saving me!",
                                    "  I don't want them to... UGGH...",
                                    "  What's happening? NO!",
                                    "  NO NOT ME! aAAAAAaaaa! NOT NOW! AAAGGGGGGGRRR \""
                                };
                                ShowSpecialDialogue(prisoner, text);

                                // Scoring event.
                                m_Session.Scoring.AddEvent(m_Session.WorldTime.TurnCounter, String.Format("Freed {0}.", prisoner.Name));

                                // reveal location.
                                m_Session.PlayerKnows_CHARUndergroundFacilityLocation = true;

                                // Scoring event.
                                m_Session.Scoring.AddEvent(m_Session.WorldTime.TurnCounter, "Learned the location of the CHAR Underground Facility.");

                                // transformation.
                                // - zombify.
                                KillActor(null, prisoner, "transformation", false);  // alpha10 don't drop corpse!
                                Actor monster = Zombify(null, prisoner, false);
                                // - turn into a ZP.
                                monster.Model = m_GameActors.ZombiePrince;
                                // - zero AP so player don't get hit asap.
                                monster.ActionPoints = 0;

                                // Scoring event.
                                m_Session.Scoring.AddEvent(m_Session.WorldTime.TurnCounter, String.Format("{0} turned into a {1}!", prisoner.Name, monster.Model.Name));

                                // fight music!
                                m_MusicManager.Play(GameMusics.FIGHT, MusicPriority.PRIORITY_EVENT);

                                // Next stage.
                                m_Session.ScriptStage_PoliceStationPrisoner = ScriptStage.STAGE_2;
                            }
                        }
                        break;

                    case ScriptStage.STAGE_2: // monsterized!
                        // nothing to do...
                        break;

                    default:
                        throw new ArgumentOutOfRangeException("unhandled script stage " + m_Session.ScriptStage_PoliceStationPrisoner);
                }
            }
            #endregion

            // 5. Sighting Jason Myer : !jasonmyers
            #region
            if (player != m_Session.UniqueActors.JasonMyers.TheActor)
            {
                if (!m_Session.UniqueActors.JasonMyers.TheActor.IsDead)
                {
                    if (IsVisibleToPlayer(m_Session.UniqueActors.JasonMyers.TheActor))
                    {
                        lock (m_Session) // thread safe
                        {
                            // music.
                            if (m_MusicManager.Music != GameMusics.INSANE)
                            {
                                m_MusicManager.Stop();
                                m_MusicManager.Play(GameMusics.INSANE, MusicPriority.PRIORITY_EVENT);
                            }

                            // message if 1st time.
                            if (!m_Session.Scoring.HasSighted(m_Session.UniqueActors.JasonMyers.TheActor.Model.ID))
                            {
                                ClearMessages();
                                AddMessage(new Message("Nice axe you have there!", m_Session.WorldTime.TurnCounter, Color.Yellow));
                                if (!m_Player.IsBotPlayer)
                                    AddMessagePressEnter();
                            }
                        }
                    }
                }
            }
            #endregion

            // 6. Sighting Duckman
            #region
            // alpha10 disabled
            //if (player != m_Session.UniqueActors.Duckman.TheActor)
            //{
            //    if (!m_Session.UniqueActors.Duckman.TheActor.IsDead && IsVisibleToPlayer(m_Session.UniqueActors.Duckman.TheActor))
            //    {
            //        // keep playing music.
            //        if (!m_MusicManager.IsPlaying(GameMusics.DUCKMAN_THEME_SONG))
            //        {
            //            m_MusicManager.Play(GameMusics.DUCKMAN_THEME_SONG);
            //        }
            //    }
            //    else if (m_MusicManager.IsPlaying(GameMusics.DUCKMAN_THEME_SONG))
            //    {
            //        m_MusicManager.Stop(GameMusics.DUCKMAN_THEME_SONG);
            //    }
            //}
            #endregion

            #endregion

            #region Item interactions
            // 1. Subway Worker Badge in Subway maps.
            //    conditions: In Subway, Must be Equipped, Next to closed gates.
            //    effects: Turn all generators on.
            if (m_Session.UniqueItems.TheSubwayWorkerBadge.TheItem.IsEquipped &&
                player.Location.Map == player.Location.Map.District.SubwayMap &&
                player.Inventory.Contains(m_Session.UniqueItems.TheSubwayWorkerBadge.TheItem))
            {
                // must be adjacent to closed gates.
                Map map = player.Location.Map;
                if (map.HasAnyAdjacentInMap(player.Location.Position, (pt) =>
                {
                    MapObject obj = map.GetMapObjectAt(pt);
                    if (obj == null)
                        return false;
                    return obj.ImageID == GameImages.OBJ_GATE_CLOSED;
                }))
                {
                    // turn all power on!
                    DoTurnAllGeneratorsOn(map);

                    // message.
                    AddMessage(new Message("The gate system scanned your badge and turned the power on!", m_Session.WorldTime.TurnCounter, Color.Green));
                }
            }
            #endregion

            #region Generic 1st time flags
            // 1. Visiting a new map.
            if (!m_Session.Scoring.HasVisited(player.Location.Map))
            {
                // visit.
                m_Session.Scoring.AddVisit(m_Session.WorldTime.TurnCounter, player.Location.Map);
                m_Session.Scoring.AddEvent(m_Session.WorldTime.TurnCounter, String.Format("Visited {0}.", player.Location.Map.Name));
            }

            // 2. Sighting an actor : actor model, unique NPCs.
            foreach (Point p in m_PlayerFOV)
            {
                Actor other = player.Location.Map.GetActorAt(p);
                if (other == null || other == player)
                    continue;
                m_Session.Scoring.AddSighting(other.Model.ID, m_Session.WorldTime.TurnCounter);
                // alpha10 unique npcs lose their invincibility when sighted and highlight them.
                if (other.IsUnique)
                {
                    if (other.IsInvincible)  // 1st sighting
                    {
                        PlayUniqueActorMusicAndMessage(m_Session.ActorToUniqueActor(other), false);
                        other.IsInvincible = false;
                    }
                }
            }
            #endregion
        }
        #endregion

        #region Reincarnation
        void HandleReincarnation()
        {
            // Reincarnate?
            // don't bother if option set to zero.
            if (s_Options.MaxReincarnations <= 0 || !AskForReincarnation())
            {
                m_MusicManager.Stop();
                return;
            }

            // play music.
            m_MusicManager.Stop();
            m_MusicManager.PlayLooping(GameMusics.LIMBO, MusicPriority.PRIORITY_EVENT);

            // Waiting screen...
            m_UI.UI_Clear(Color.Black);
            m_UI.UI_DrawStringBold(Color.Yellow, "Reincarnation - Purgatory", 0, 0);
            m_UI.UI_DrawStringBold(Color.White, "(preparing reincarnations, please wait...)", 0, 2 * BOLD_LINE_SPACING);
            m_UI.UI_Repaint();

            // Decide available reincarnation targets.
            int countDummy;
            Actor randomR = FindReincarnationAvatar(GameOptions.ReincMode.RANDOM_ACTOR, out countDummy);
            int countLivings;
            Actor livingR = FindReincarnationAvatar(GameOptions.ReincMode.RANDOM_LIVING, out countLivings);
            int countUndead;
            Actor undeadR = FindReincarnationAvatar(GameOptions.ReincMode.RANDOM_UNDEAD, out countUndead);
            int countFollower;
            Actor followerR = FindReincarnationAvatar(GameOptions.ReincMode.RANDOM_FOLLOWER, out countFollower);
            Actor killerR = FindReincarnationAvatar(GameOptions.ReincMode.KILLER, out countDummy);
            Actor zombifiedR = FindReincarnationAvatar(GameOptions.ReincMode.ZOMBIFIED, out countDummy);

            // Get fun facts.
            string[] funFacts = CompileDistrictFunFacts(m_Player.Location.Map.District);

            // Reincarnate.
            // Choose avatar from a set of reincarnation modes.
            bool choiceMade = false;
            string[] entries =
            {
                GameOptions.Name(GameOptions.ReincMode.RANDOM_ACTOR),
                GameOptions.Name(GameOptions.ReincMode.RANDOM_LIVING),
                GameOptions.Name(GameOptions.ReincMode.RANDOM_UNDEAD),
                GameOptions.Name(GameOptions.ReincMode.RANDOM_FOLLOWER),
                GameOptions.Name(GameOptions.ReincMode.KILLER),
                GameOptions.Name(GameOptions.ReincMode.ZOMBIFIED)
            };
            string[] values =
            {
                DescribeAvatar(randomR),
                String.Format("{0}   (out of {1} possibilities)", DescribeAvatar(livingR), countLivings),
                String.Format("{0}   (out of {1} possibilities)", DescribeAvatar(undeadR), countUndead),
                String.Format("{0}   (out of {1} possibilities)", DescribeAvatar(followerR), countFollower),
                DescribeAvatar(killerR),
                DescribeAvatar(zombifiedR)
            };
            int selected = 0;
            Actor avatar = null;
            do
            {
                // show screen.
                int gx, gy;
                gx = gy = 0;
                m_UI.UI_Clear(Color.Black);
                m_UI.UI_DrawStringBold(Color.Yellow, "Reincarnation - Choose Avatar", gx, gy);
                gy += 2 * BOLD_LINE_SPACING;

                DrawMenuOrOptions(selected, Color.White, entries, Color.LightGreen, values, gx, ref gy);
                gy += 2 * BOLD_LINE_SPACING;

                m_UI.UI_DrawStringBold(Color.Pink, ".-* District Fun Facts! *-.", gx, gy);
                gy += BOLD_LINE_SPACING;
                m_UI.UI_DrawStringBold(Color.Pink, String.Format("at current date : {0}.", new WorldTime(m_Session.WorldTime.TurnCounter).ToString()), gx, gy);
                gy += 2 * BOLD_LINE_SPACING;
                for (int i = 0; i < funFacts.Length; i++)
                {
                    m_UI.UI_DrawStringBold(Color.Pink, funFacts[i], gx, gy);
                    gy += BOLD_LINE_SPACING;
                }

                DrawFootnote(Color.White, "cursor to move, ENTER to select, ESC to cancel and end game");

                m_UI.UI_Repaint();

                // get menu action.
                KeyEventArgs key = m_UI.UI_WaitKey();
                switch (key.KeyCode)
                {
                    case Keys.Up:       // move up
                        if (selected > 0) --selected;
                        else selected = entries.Length - 1;
                        break;
                    case Keys.Down:     // move down
                        selected = (selected + 1) % entries.Length;
                        break;
                    case Keys.Escape:   // cancel & end game
                        choiceMade = true;
                        avatar = null;
                        break;

                    case Keys.Enter:    // validate
                        {
                            switch (selected)
                            {
                                case 0: // random actor
                                    avatar = randomR;
                                    break;
                                case 1: // random survivor
                                    avatar = livingR;
                                    break;
                                case 2: // random undead
                                    avatar = undeadR;
                                    break;
                                case 3: // random follower
                                    avatar = followerR;
                                    break;
                                case 4: // killer
                                    avatar = killerR;
                                    break;
                                case 5: // zombified
                                    avatar = zombifiedR;
                                    break;
                            }
                            choiceMade = avatar != null;
                            break;
                        }
                }
            }
            while (!choiceMade);

            // If canceled, stop.
            if (avatar == null)
            {
                m_MusicManager.Stop();
                return;
            }

            // Perform reincarnation.
            // 1. Make actor the player.
            // 2. Update all player-centric data.
            #region
            // 1. Make actor the player.
            avatar.Controller = new PlayerController();
            if (avatar.Activity != Activity.SLEEPING)
                avatar.Activity = Activity.IDLE;
            PrepareActorForPlayerControl(avatar);

            // 2. Update all player-centric data.
            m_Player = avatar;
            m_Session.CurrentMap = avatar.Location.Map;
            m_Session.Scoring.StartNewLife(m_Session.WorldTime.TurnCounter);
            m_Session.Scoring.AddEvent(m_Session.WorldTime.TurnCounter, String.Format("(reincarnation {0})", m_Session.Scoring.ReincarnationNumber));
            m_Session.Scoring.Side = m_Player.Model.Abilities.IsUndead ? DifficultySide.FOR_UNDEAD : DifficultySide.FOR_SURVIVOR;
            m_Session.Scoring.DifficultyRating = Scoring.ComputeDifficultyRating(s_Options, m_Session.Scoring.Side, m_Session.Scoring.ReincarnationNumber);
            /// forget all maps memory.
            for (int dx = 0; dx < m_Session.World.Size; dx++)
                for (int dy = 0; dy < m_Session.World.Size; dy++)
                {
                    District d = m_Session.World[dx, dy];
                    foreach (Map m in d.Maps)
                        m.SetAllAsUnvisited();
                }
            #endregion

            // Cleanup and refresh.
            m_MusicManager.Stop();
            UpdatePlayerFOV(m_Player);
            ComputeViewRect(m_Player.Location.Position);
            ClearMessages();
            AddMessage(new Message(String.Format("{0} feels disoriented for a second...", m_Player.Name), m_Session.WorldTime.TurnCounter, Color.Yellow));
            RedrawPlayScreen();

            // Play reinc sfx or special music for actor.
            string music = GameMusics.REINCARNATE;
            if (m_Player == m_Session.UniqueActors.JasonMyers.TheActor)
                music = GameMusics.INSANE;
            // apha10 replace with sfx
            m_MusicManager.Stop();
            m_MusicManager.Play(music, MusicPriority.PRIORITY_EVENT);

            // restart sim thread.
            StopSimThread(false);  // alpha10 stop-start
            StartSimThread();
        }

        string DescribeAvatar(Actor a)
        {
            if (a == null)
                return "(N/A)";
            bool isLeader = a.CountFollowers > 0;
            bool isFollower = a.HasLeader;
            return String.Format("{0}, a {1}{2}", a.Name, a.Model.Name, isLeader ? ", leader" : isFollower ? ", follower" : "");
        }

        bool AskForReincarnation()
        {
            // show screen.
            int gx, gy;
            gx = gy = 0;
            m_UI.UI_Clear(Color.Black);
            m_UI.UI_DrawStringBold(Color.Yellow, "Limbo", gx, gy);
            gy += 2 * BOLD_LINE_SPACING;
            m_UI.UI_DrawStringBold(Color.White, String.Format("Leave body {0}/{1}.", (1 + m_Session.Scoring.ReincarnationNumber), (1 + s_Options.MaxReincarnations)), gx, gy);
            gy += BOLD_LINE_SPACING;
            m_UI.UI_DrawStringBold(Color.White, "Remember lives.", gx, gy);
            gy += BOLD_LINE_SPACING;
            m_UI.UI_DrawStringBold(Color.White, "Remember purpose.", gx, gy);
            gy += BOLD_LINE_SPACING;
            m_UI.UI_DrawStringBold(Color.White, "Clear again.", gx, gy);
            gy += BOLD_LINE_SPACING;

            // ask question or no more lives left.
            if (m_Session.Scoring.ReincarnationNumber >= s_Options.MaxReincarnations)
            {
                // no more lives left.
                m_UI.UI_DrawStringBold(Color.LightGreen, "Humans interesting.", gx, gy);
                gy += BOLD_LINE_SPACING;
                m_UI.UI_DrawStringBold(Color.LightGreen, "Time to leave.", gx, gy);
                gy += BOLD_LINE_SPACING;
                gy += 2 * BOLD_LINE_SPACING;
                m_UI.UI_DrawStringBold(Color.Yellow, "No more reincarnations left.", gx, gy);
                DrawFootnote(Color.White, "press ENTER");
                m_UI.UI_Repaint();
                WaitEnter();
                return false;
            }
            else
            {
                // one more life available.
                m_UI.UI_DrawStringBold(Color.White, "Leave?", gx, gy);
                gy += BOLD_LINE_SPACING;
                m_UI.UI_DrawStringBold(Color.White, "Live?", gx, gy);

                gy += 2 * BOLD_LINE_SPACING;
                m_UI.UI_DrawStringBold(Color.Yellow, "Reincarnate? Y to confirm, N to cancel.", gx, gy);
                m_UI.UI_Repaint();

                // ask question.
                return WaitYesOrNo();
            }
        }

        bool IsSuitableReincarnation(Actor a, bool asLiving)
        {
            if (a == null)
                return false;
            if (a.IsDead || a.IsPlayer)
                return false;

            // same district only.
            if (a.Location.Map.District != m_Session.CurrentMap.District)
                return false;

            // forbid some special maps.
            if (a.Location.Map == m_Session.UniqueMaps.CHARUndergroundFacility.TheMap)
                return false;

            // forbid some special actors.
            if (a == m_Session.UniqueActors.PoliceStationPrisoner.TheActor)
                return false;

            // (option) not in sewers.
            if (a.Location.Map == a.Location.Map.District.SewersMap)
                return false;

            // living vs undead checks.
            if (asLiving)
            {
                if (a.Model.Abilities.IsUndead)
                    return false;
                // (option) civilians only.
                if (s_Options.IsLivingReincRestricted && a.Faction != GameFactions.TheCivilians)
                    return false;

                return true;
            }
            else
            {
                if (a.Model.Abilities.IsUndead)
                {
                    // (option) not rats.
                    if (!s_Options.CanReincarnateAsRat && a.Model == GameActors.RatZombie)
                        return false;

                    return true;
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reincMode"></param>
        /// <param name="matchingActors">how many actors where matching the reincarnation mode</param>
        /// <returns>null if not found</returns>
        Actor FindReincarnationAvatar(GameOptions.ReincMode reincMode, out int matchingActors)
        {
            switch (reincMode)
            {
                case GameOptions.ReincMode.RANDOM_FOLLOWER:
                    #region
                    {
                        if (m_Session.Scoring.FollowersWhendDied == null)
                        {
                            matchingActors = 0;
                            return null;
                        }

                        // list all suitable followers.
                        List<Actor> suitableFollowers = new List<Actor>(m_Session.Scoring.FollowersWhendDied.Count);
                        foreach (Actor fo in m_Session.Scoring.FollowersWhendDied)
                            if (IsSuitableReincarnation(fo, true))
                                suitableFollowers.Add(fo);

                        // make sure we have at least one suitable!
                        matchingActors = suitableFollowers.Count;
                        if (suitableFollowers.Count == 0)
                            return null;

                        // random one.
                        return suitableFollowers[m_Rules.Roll(0, suitableFollowers.Count)];
                    }
                #endregion

                case GameOptions.ReincMode.KILLER:
                    #region
                    {
                        Actor killer = m_Session.Scoring.Killer;
                        if (IsSuitableReincarnation(killer, true) || IsSuitableReincarnation(killer, false))
                        {
                            matchingActors = 1;
                            return killer;
                        }
                        else
                        {
                            matchingActors = 0;
                            return null;
                        }
                    }
                #endregion

                case GameOptions.ReincMode.RANDOM_ACTOR:
                case GameOptions.ReincMode.RANDOM_LIVING:
                case GameOptions.ReincMode.RANDOM_UNDEAD:
                    #region
                    {
                        // get a list of all suitable actors in the world.
                        bool asLiving = (reincMode == GameOptions.ReincMode.RANDOM_LIVING || (reincMode == GameOptions.ReincMode.RANDOM_ACTOR && m_Rules.RollChance(50)));
                        List<Actor> allSuitables = new List<Actor>();
                        for (int dx = 0; dx < m_Session.World.Size; dx++)
                            for (int dy = 0; dy < m_Session.World.Size; dy++)
                            {
                                District district = m_Session.World[dx, dy];
                                foreach (Map map in district.Maps)
                                    foreach (Actor a in map.Actors)
                                        if (IsSuitableReincarnation(a, asLiving))
                                            allSuitables.Add(a);
                            }

                        // pick one at random.
                        matchingActors = allSuitables.Count;
                        if (allSuitables.Count == 0)
                            return null;
                        else
                            return allSuitables[m_Rules.Roll(0, allSuitables.Count)];
                    }
                #endregion

                case GameOptions.ReincMode.ZOMBIFIED:
                    #region
                    {
                        Actor zombie = m_Session.Scoring.ZombifiedPlayer;
                        if (IsSuitableReincarnation(zombie, false))
                        {
                            matchingActors = 1;
                            return zombie;
                        }
                        else
                        {
                            matchingActors = 0;
                            return null;
                        }
                    }
                #endregion

                default:
                    throw new ArgumentOutOfRangeException("unhandled reincarnation mode " + reincMode.ToString());
            }
        }
        #endregion

        #region Insanity
        ActorAction GenerateInsaneAction(Actor actor)
        {
            // Let's the insanity flow...
            int roll = m_Rules.Roll(0, 5);
            switch (roll)
            {
                // shout
                case 0: return new ActionShout(actor, this, "AAAAAAAAAAA!!!");

                // random bump
                case 1: return new ActionBump(actor, this, m_Rules.RollDirection());

                // random bash.
                case 2:
                    Direction d = m_Rules.RollDirection();
                    MapObject mobj = actor.Location.Map.GetMapObjectAt(actor.Location.Position + d);
                    if (mobj == null) return null;
                    return new ActionBreak(actor, this, mobj);

                // random use/unequip-drop
                case 3:
                    Inventory inv = actor.Inventory;
                    if (inv == null || inv.CountItems == 0) return null;
                    Item it = inv[m_Rules.Roll(0, inv.CountItems)];
                    ActionUseItem useIt = new ActionUseItem(actor, this, it);
                    if (useIt.IsLegal())
                        return useIt;
                    if (it.IsEquipped)
                        return new ActionUnequipItem(actor, this, it);
                    return new ActionDropItem(actor, this, it);

                // random agression.
                case 4:
                    int fov = m_Rules.ActorFOV(actor, actor.Location.Map.LocalTime, m_Session.World.Weather);
                    foreach (Actor a in actor.Location.Map.Actors)
                    {
                        if (a == actor) continue;
                        if (m_Rules.AreEnemies(actor, a)) continue;
                        if (!LOS.CanTraceViewLine(actor.Location, a.Location.Position, fov)) continue;
                        if (m_Rules.RollChance(50))
                        {
                            // force leaving of leader.
                            if (actor.HasLeader)
                            {
                                actor.Leader.RemoveFollower(actor);
                                actor.TrustInLeader = Rules.TRUST_NEUTRAL;
                            }
                            // agress.
                            DoMakeAggression(actor, a);
                            return new ActionSay(actor, this, a, "YOU ARE ONE OF THEM!!", Sayflags.IS_IMPORTANT | Sayflags.IS_DANGER);
                        }
                    }
                    return null;

                default:
                    return null;
            }
        }

        void SeeingCauseInsanity(Actor whoDoesTheAction, Location loc, int sanCost, string what)
        {
            foreach (Actor a in loc.Map.Actors)
            {
                if (!a.Model.Abilities.HasSanity) continue;

                // can't see if sleeping or out of fov.
                if (a.IsSleeping) continue;
                int fov = m_Rules.ActorFOV(a, loc.Map.LocalTime, m_Session.World.Weather);
                if (!LOS.CanTraceViewLine(loc, a.Location.Position, fov)) continue;

                // san hit.
                SpendActorSanity(a, sanCost);

                // msg.
                if (whoDoesTheAction == a)
                {
                    if (a.IsPlayer)
                        AddMessage(new Message("That was a very disturbing thing to do...", loc.Map.LocalTime.TurnCounter, Color.Orange));
                    else if (IsVisibleToPlayer(a))
                        AddMessage(MakeMessage(a, String.Format("{0} done something very disturbing...", Conjugate(a, VERB_HAVE))));
                }
                else
                {
                    if (a.IsPlayer)
                        AddMessage(new Message(String.Format("Seeing {0} is very disturbing...", what), loc.Map.LocalTime.TurnCounter, Color.Orange));
                    else if (IsVisibleToPlayer(a))
                        AddMessage(MakeMessage(a, String.Format("{0} something very disturbing...", Conjugate(a, VERB_SEE))));
                }
            }
        }
        #endregion

        #region Special map events
        void OnMapPowerGeneratorSwitch(Location location, PowerGenerator powGen)
        {
            Map map = location.Map;
            //////////////////////////////////////////////////////
            // Maps:
            // 1. CHAR Underground Facility.
            // 2. Subway
            // 3. Police Station Jails.
            // 4. Hospital Power.
            /////////////////////////////////////////////////////

            // 1. CHAR Underground Facility
            // Darkness->Lit, TODO: Elevator off->on.
            #region
            if (map == m_Session.UniqueMaps.CHARUndergroundFacility.TheMap)
            {
                lock (m_Session) // thread safe
                {
                    // check all power generators are on.
                    bool allAreOn = m_Rules.ComputeMapPowerRatio(map) >= 1.0f;

                    // change map lighting.
                    if (allAreOn)
                    {
                        if (map.Lighting != Lighting.LIT)
                        {
                            map.Lighting = Lighting.LIT;

                            // message.
                            if (m_Player.Location.Map == map)
                            {
                                ClearMessages();
                                AddMessage(new Message("The Facility lights turn on!", map.LocalTime.TurnCounter, Color.Green));
                                RedrawPlayScreen();
                            }

                            // achievement?
                            if (!m_Session.Scoring.HasCompletedAchievement(Achievement.IDs.CHAR_POWER_UNDERGROUND_FACILITY))
                            {
                                // completed!
                                m_Session.Scoring.SetCompletedAchievement(Achievement.IDs.CHAR_POWER_UNDERGROUND_FACILITY);

                                // achievement!
                                ShowNewAchievement(Achievement.IDs.CHAR_POWER_UNDERGROUND_FACILITY);
                            }
                        }
                    }
                    else // part off
                    {
                        if (map.Lighting != Lighting.DARKNESS)
                        {
                            map.Lighting = Lighting.DARKNESS;

                            // message.
                            if (m_Player.Location.Map == map)
                            {
                                ClearMessages();
                                AddMessage(new Message("The Facility lights turn off!", map.LocalTime.TurnCounter, Color.Red));
                                RedrawPlayScreen();
                            }
                        }
                    }
                }
            }
            #endregion

            // 2. Subway
            // Darkness->Lit, Gates->open.
            #region
            if (map == map.District.SubwayMap)
            {
                lock (m_Session) // thread safe
                {
                    // check all power generators are on.
                    bool allAreOn = m_Rules.ComputeMapPowerRatio(map) >= 1.0f;

                    // change map lighting, open/close fences.
                    if (allAreOn)
                    {
                        if (map.Lighting != Lighting.LIT)
                        {
                            // lit.
                            map.Lighting = Lighting.LIT;

                            // message.
                            if (m_Player.Location.Map == map)
                            {
                                ClearMessages();
                                AddMessage(new Message("The station power turns on!", map.LocalTime.TurnCounter, Color.Green));
                                AddMessage(new Message("You hear the gates opening.", map.LocalTime.TurnCounter, Color.Green));
                                RedrawPlayScreen();
                            }

                            // open iron gates.
                            DoOpenSubwayGates(map);
                        }
                    }
                    else // part off
                    {
                        if (map.Lighting != Lighting.DARKNESS)
                        {
                            // message.
                            if (m_Player.Location.Map == map)
                            {
                                ClearMessages();
                                AddMessage(new Message("The station power turns off!", map.LocalTime.TurnCounter, Color.Red));
                                AddMessage(new Message("You hear the gates closing.", map.LocalTime.TurnCounter, Color.Red));
                                RedrawPlayScreen();
                            }

                            // darkness.
                            map.Lighting = Lighting.DARKNESS;

                            // close iron gates.
                            DoCloseSubwayGates(map);

                        }
                    }
                }
            }
            #endregion

            // 3. Police Station Jails.
            #region
            if (map == m_Session.UniqueMaps.PoliceStation_JailsLevel.TheMap)
            {
                lock (m_Session) // thread safe
                {
                    // check all power generators are on.
                    bool allAreOn = m_Rules.ComputeMapPowerRatio(map) >= 1.0f;

                    // open/close cells.
                    if (allAreOn)
                    {
                        // message.
                        if (m_Player.Location.Map == map)
                        {
                            ClearMessages();
                            AddMessage(new Message("The cells are opening.", map.LocalTime.TurnCounter, Color.Green));
                            RedrawPlayScreen();
                        }

                        // open cells.
                        DoOpenPoliceJailCells(map);
                    }
                    else
                    {
                        // message.
                        if (m_Player.Location.Map == map)
                        {
                            ClearMessages();
                            AddMessage(new Message("The cells are closing.", map.LocalTime.TurnCounter, Color.Green));
                            RedrawPlayScreen();
                        }

                        // open cells.
                        DoClosePoliceJailCells(map);
                    }
                }
            }
            #endregion

            // 4. Hospital Power.
            #region
            if (map == m_Session.UniqueMaps.Hospital_Power.TheMap)
            {
                lock (m_Session) // thread safe
                {
                    // check all power generators are on.
                    bool allAreOn = m_Rules.ComputeMapPowerRatio(map) >= 1.0f;

                    // open/close cells.
                    if (allAreOn)
                    {
                        // message.
                        if (m_Player.Location.Map == map)
                        {
                            ClearMessages();
                            AddMessage(new Message("The lights turn on and you hear something opening upstairs.", map.LocalTime.TurnCounter, Color.Green));
                            RedrawPlayScreen();
                        }

                        // turn power on.
                        DoHospitalPowerOn();
                    }
                    else
                    {
                        if (map.Lighting != Lighting.DARKNESS)
                        {
                            // message.
                            if (m_Player.Location.Map == map)
                            {
                                ClearMessages();
                                AddMessage(new Message("The lights turn off and you hear something closing upstairs.", map.LocalTime.TurnCounter, Color.Green));
                                RedrawPlayScreen();
                            }

                            // turn power off.
                            DoHospitalPowerOff();
                        }
                    }
                }
            }
            #endregion
        }

        // alpha10.1 common code for checking crushing closing gates.
        // they do not insta-kill the actor anymore but inflict (large) damage and can't close if the actor is still there.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gate"></param>
        /// <param name="crushingDamage">damage to inflict, bypass protection</param>
        /// <returns>true if the gate can close, false if it must stay open</returns>
        bool CheckForGateClosingCrush(MapObject gate, int crushingDamage)
        {
            Actor crushedActor = gate.Location.Map.GetActorAt(gate.Location.Position);
            if (crushedActor == null)
                return true;
            if (crushedActor.IsInvincible)
                return false;

            InflictDamage(crushedActor, crushingDamage);
            if (IsVisibleToPlayer(crushedActor))
            {
                AddMessage(MakeMessage(crushedActor, String.Format("is crushed for {0} damage!", crushingDamage)));
                AddOverlay(new OverlayImage(MapToScreen(crushedActor.Location.Position), GameImages.ICON_MELEE_DAMAGE));
                AddOverlay(new OverlayText(MapToScreen(crushedActor.Location.Position).Add(DAMAGE_DX, DAMAGE_DY), Color.White, crushingDamage.ToString(), Color.Black));
                RedrawPlayScreen();
                AnimDelay(crushedActor.IsPlayer ? DELAY_NORMAL : DELAY_SHORT);
                ClearOverlays();
                RedrawPlayScreen();
            }

            if (crushedActor.HitPoints <= 0)
            {
                KillActor(null, crushedActor, "crushed");
                return true;
            }
            else
                return false;
        }


        void DoOpenSubwayGates(Map map)
        {
            foreach (MapObject obj in map.MapObjects)
            {
                if (obj.ImageID == GameImages.OBJ_GATE_CLOSED)
                {
                    obj.IsWalkable = true;
                    obj.ImageID = GameImages.OBJ_GATE_OPEN;
                }
            }
        }

        void DoCloseSubwayGates(Map map)
        {
            foreach (MapObject obj in map.MapObjects)
            {
                if (obj.ImageID == GameImages.OBJ_GATE_OPEN)
                {
                    // alpha10.1
                    if (CheckForGateClosingCrush(obj, Rules.CRUSHING_GATES_DAMAGE))
                    {
                        obj.IsWalkable = false;
                        obj.ImageID = GameImages.OBJ_GATE_CLOSED;
                    }
                    /* obsolete
                    obj.IsWalkable = false;
                    obj.ImageID = GameImages.OBJ_GATE_CLOSED;
                    Actor crushedActor = map.GetActorAt(obj.Location.Position);
                    if (crushedActor != null && !crushedActor.IsInvincible) // alpha10
                    {
                        KillActor(null, crushedActor, "crushed");
                        if (m_Player.Location.Map == map)
                        {
                            AddMessage(new Message("Someone got crushed between the closing gates!", map.LocalTime.TurnCounter, Color.Red));
                            RedrawPlayScreen();
                        }
                    }
                    */
                }
            }
        }

        void DoOpenPoliceJailCells(Map map)
        {
            foreach (MapObject obj in map.MapObjects)
            {
                if (obj.ImageID == GameImages.OBJ_GATE_CLOSED)
                {
                    obj.IsWalkable = true;
                    obj.ImageID = GameImages.OBJ_GATE_OPEN;
                }
            }
        }

        void DoClosePoliceJailCells(Map map)
        {
            foreach (MapObject obj in map.MapObjects)
            {
                if (obj.ImageID == GameImages.OBJ_GATE_OPEN)
                {
                    // alpha10.1
                    if (CheckForGateClosingCrush(obj, Rules.CRUSHING_GATES_DAMAGE))
                    {
                        obj.IsWalkable = false;
                        obj.ImageID = GameImages.OBJ_GATE_CLOSED;
                    }
                    /* obsolete
                    obj.IsWalkable = false;
                    obj.ImageID = GameImages.OBJ_GATE_CLOSED;
                    Actor crushedActor = map.GetActorAt(obj.Location.Position);
                    if (crushedActor != null && !crushedActor.IsInvincible) // alpha10
                    {
                        KillActor(null, crushedActor, "crushed");
                        if (m_Player.Location.Map == map)
                        {
                            AddMessage(new Message("Someone got crushed between the closing cells!", map.LocalTime.TurnCounter, Color.Red));
                            RedrawPlayScreen();
                        }
                    }
                    */
                }
            }
        }

        void DoHospitalPowerOn()
        {
            // turn all hospital lights on.
            m_Session.UniqueMaps.Hospital_Admissions.TheMap.Lighting = Lighting.LIT;
            m_Session.UniqueMaps.Hospital_Offices.TheMap.Lighting = Lighting.LIT;
            m_Session.UniqueMaps.Hospital_Patients.TheMap.Lighting = Lighting.LIT;
            m_Session.UniqueMaps.Hospital_Power.TheMap.Lighting = Lighting.LIT;
            m_Session.UniqueMaps.Hospital_Storage.TheMap.Lighting = Lighting.LIT;

            // open storage gates.
            foreach (MapObject obj in m_Session.UniqueMaps.Hospital_Storage.TheMap.MapObjects)
            {
                if (obj.ImageID == GameImages.OBJ_GATE_CLOSED)
                {
                    obj.IsWalkable = true;
                    obj.ImageID = GameImages.OBJ_GATE_OPEN;
                }
            }
        }

        void DoHospitalPowerOff()
        {
            // turn all hospital lights off.
            m_Session.UniqueMaps.Hospital_Admissions.TheMap.Lighting = Lighting.DARKNESS;
            m_Session.UniqueMaps.Hospital_Offices.TheMap.Lighting = Lighting.DARKNESS;
            m_Session.UniqueMaps.Hospital_Patients.TheMap.Lighting = Lighting.DARKNESS;
            m_Session.UniqueMaps.Hospital_Power.TheMap.Lighting = Lighting.DARKNESS;
            m_Session.UniqueMaps.Hospital_Storage.TheMap.Lighting = Lighting.DARKNESS;

            // close storage gate.
            Map map = m_Session.UniqueMaps.Hospital_Storage.TheMap;
            foreach (MapObject obj in map.MapObjects)
            {
                if (obj.ImageID == GameImages.OBJ_GATE_OPEN)
                {
                    // alpha10.1
                    if (CheckForGateClosingCrush(obj, Rules.CRUSHING_GATES_DAMAGE))
                    {
                        obj.IsWalkable = false;
                        obj.ImageID = GameImages.OBJ_GATE_CLOSED;
                    }
                    /* obsolete
                    obj.IsWalkable = false;
                    obj.ImageID = GameImages.OBJ_GATE_CLOSED;
                    Actor crushedActor = map.GetActorAt(obj.Location.Position);
                    if (crushedActor != null)
                    {
                        KillActor(null, crushedActor, "crushed");
                        if (m_Player.Location.Map == map)
                        {
                            AddMessage(new Message("Someone got crushed between the closing gate!", map.LocalTime.TurnCounter, Color.Red));
                            RedrawPlayScreen();
                        }
                    }
                    */
                }
            }
        }

        void DoTurnAllGeneratorsOn(Map map)
        {
            foreach (MapObject obj in map.MapObjects)
            {
                PowerGenerator powGen = obj as PowerGenerator;
                if (powGen == null)
                    continue;
                if (!powGen.IsOn)
                {
                    powGen.TogglePower();
                    OnMapPowerGeneratorSwitch(powGen.Location, powGen);
                }
            }
        }
        #endregion

        #region Various predicates
        public static bool IsInCHAROffice(Location location)
        {
            List<Zone> zones = location.Map.GetZonesAt(location.Position.X, location.Position.Y);
            if (zones == null)
                return false;
            foreach (Zone z in zones)
            {
                if (z.HasGameAttribute(ZoneAttributes.IS_CHAR_OFFICE))
                    return true;
            }
            return false;
        }

        public bool IsInCHARProperty(Location location)
        {
            return location.Map == Session.UniqueMaps.CHARUndergroundFacility.TheMap ||
                IsInCHAROffice(location);
        }

        #endregion

        #region Phone
        bool AreLinkedByPhone(Actor speaker, Actor target)
        {
            // only leader-follower
            if (speaker.Leader != target && target.Leader != speaker)
                return false;

            // check if equipped phones.
            ItemTracker trSpeaker = speaker.GetEquippedItem(DollPart.LEFT_HAND) as ItemTracker;
            if (trSpeaker == null || !trSpeaker.CanTrackFollowersOrLeader)
                return false;
            ItemTracker trTarget = target.GetEquippedItem(DollPart.LEFT_HAND) as ItemTracker;
            if (trTarget == null || !trTarget.CanTrackFollowersOrLeader)
                return false;

            // yep!
            return true;
        }
        #endregion

        #region Fun Facts!
        [Flags]
        enum MapListFlags
        {
            NONE = 0,

            /// <summary>
            /// Exclude map with the IsSecret property.
            /// </summary>
            EXCLUDE_SECRET_MAPS = (1 << 0)
        }

        List<Actor> ListWorldActors(Predicate<Actor> pred, MapListFlags flags)
        {
            List<Actor> list = new List<Actor>();

            for (int dx = 0; dx < m_Session.World.Size; dx++)
                for (int dy = 0; dy < m_Session.World.Size; dy++)
                    list.AddRange(ListDistrictActors(m_Session.World[dx, dy], flags, pred));

            return list;
        }

        List<Actor> ListDistrictActors(District d, MapListFlags flags, Predicate<Actor> pred)
        {
            List<Actor> list = new List<Actor>();

            foreach (Map m in d.Maps)
            {
                if ((flags & MapListFlags.EXCLUDE_SECRET_MAPS) != 0 && m.IsSecret)
                    continue;
                foreach (Actor a in m.Actors)
                    if (pred == null || pred(a))
                        list.Add(a);
            }

            return list;
        }

        string FunFactActorResume(Actor a, string info)
        {
            if (a == null)
                return "(N/A)";
            return String.Format("{0} - {1}, a {2} - {3}",
                info, a.TheName, a.Model.Name, a.Location.Map.Name);
        }

        string[] CompileDistrictFunFacts(District d)
        {
            List<string> list = new List<string>();

            ///////////////////////////////////////////
            // 1. Oldest actors alive living & undead.
            // 2. Most kills living & undead.
            // 3. Most murders.
            ///////////////////////////////////////////

            // list actors.
            List<Actor> allLivings = ListDistrictActors(d, MapListFlags.EXCLUDE_SECRET_MAPS, (a) => !a.IsDead && !a.Model.Abilities.IsUndead);
            List<Actor> allUndeads = ListDistrictActors(d, MapListFlags.EXCLUDE_SECRET_MAPS, (a) => !a.IsDead && a.Model.Abilities.IsUndead);
            List<Actor> allActors = ListDistrictActors(d, MapListFlags.EXCLUDE_SECRET_MAPS, null);

            // add player (cause he's dead now)
            if (m_Player.Model.Abilities.IsUndead)
                allUndeads.Add(m_Player);
            else
                allLivings.Add(m_Player);
            allActors.Add(m_Player);

            // 1. Oldest actors alive living & undead.
            if (allLivings.Count > 0)
            {
                allLivings.Sort((a, b) => a.SpawnTime < b.SpawnTime ? -1 : a.SpawnTime == b.SpawnTime ? 0 : 1);
                list.Add("- Oldest Livings Surviving");
                list.Add(String.Format("    1st {0}.", FunFactActorResume(allLivings[0], new WorldTime(allLivings[0].SpawnTime).ToString())));
                if (allLivings.Count > 1)
                    list.Add(String.Format("    2nd {0}.", FunFactActorResume(allLivings[1], new WorldTime(allLivings[1].SpawnTime).ToString())));
            }
            else
                list.Add("    No living actors alive!");

            if (allUndeads.Count > 0)
            {
                allUndeads.Sort((a, b) => a.SpawnTime < b.SpawnTime ? -1 : a.SpawnTime == b.SpawnTime ? 0 : 1);
                list.Add("- Oldest Undeads Rotting Around");
                list.Add(String.Format("    1st {0}.", FunFactActorResume(allUndeads[0], new WorldTime(allUndeads[0].SpawnTime).ToString())));
                if (allUndeads.Count > 1)
                    list.Add(String.Format("    2nd {0}.", FunFactActorResume(allUndeads[1], new WorldTime(allUndeads[1].SpawnTime).ToString())));
            }
            else
                list.Add("    No undeads shambling around!");

            // 2. Most kills living & undead.
            if (allLivings.Count > 0)
            {
                allLivings.Sort((a, b) => a.KillsCount > b.KillsCount ? -1 : a.KillsCount == b.KillsCount ? 0 : 1);
                list.Add("- Deadliest Livings Kicking ass");
                if (allLivings[0].KillsCount > 0)
                {
                    list.Add(String.Format("    1st {0}.", FunFactActorResume(allLivings[0], allLivings[0].KillsCount.ToString())));
                    if (allLivings.Count > 1 && allLivings[1].KillsCount > 0)
                        list.Add(String.Format("    2nd {0}.", FunFactActorResume(allLivings[1], allLivings[1].KillsCount.ToString())));
                }
                else
                    list.Add("    Livings can't fight for their lives apparently.");
            }
            if (allUndeads.Count > 0)
            {
                allUndeads.Sort((a, b) => a.KillsCount > b.KillsCount ? -1 : a.KillsCount == b.KillsCount ? 0 : 1);
                list.Add("- Deadliest Undeads Chewing Brains");
                if (allUndeads[0].KillsCount > 0)
                {
                    list.Add(String.Format("    1st {0}.", FunFactActorResume(allUndeads[0], allUndeads[0].KillsCount.ToString())));
                    if (allUndeads.Count > 1 && allUndeads[1].KillsCount > 0)
                        list.Add(String.Format("    2nd {0}.", FunFactActorResume(allUndeads[1], allUndeads[1].KillsCount.ToString())));
                }
                else
                    list.Add("    Undeads don't care for brains apparently.");
            }

            // 3. Most murders.
            if (allLivings.Count > 0)
            {
                allLivings.Sort((a, b) => a.MurdersCounter > b.MurdersCounter ? -1 : a.MurdersCounter == b.MurdersCounter ? 0 : 1);
                list.Add("- Most Murderous Murderer Murdering");
                if (allLivings[0].MurdersCounter > 0)
                {
                    list.Add(String.Format("    1st {0}.", FunFactActorResume(allLivings[0], allLivings[0].MurdersCounter.ToString())));
                    if (allLivings.Count > 1 && allLivings[1].MurdersCounter > 0)
                        list.Add(String.Format("    2nd {0}.", FunFactActorResume(allLivings[1], allLivings[1].MurdersCounter.ToString())));
                }
                else
                    list.Add("    No murders committed!");
            }

            // done.
            return list.ToArray();
        }
        #endregion

        #region Dev commands
        public void DEV_ToggleShowActorsStats()
        {
            s_Options.DEV_ShowActorsStats = !s_Options.DEV_ShowActorsStats;
        }

        // alpha10
        public void DEV_TogglePlayerInvincibility()
        {
            if (m_Session == null || m_Player == null)
                return;

            m_Player.IsInvincible = !m_Player.IsInvincible;
            AddMessage(new Message("DEAR DEV, YOU ARE NOW " + (m_Player.IsInvincible ? "INVINCIBLE" : "NOT INVINCIBLE"), m_Session.WorldTime.TurnCounter, Color.LightGreen));
        }

        // alpha10.1
        public void DEV_MaxTrust()
        {
            if (m_Session == null || m_Player == null)
                return;
            if (m_Player.Followers == null)  // alpha10.1 fix
                return;

            foreach (Actor f in m_Player.Followers)
                f.TrustInLeader = Rules.TRUST_MAX;
            AddMessage(new Message("DEAR DEV, FOLLOWERS TRUST MAXED.", m_Session.WorldTime.TurnCounter, Color.LightGreen));
        }
        #endregion

        #region Data files
        void LoadData()
        {
            LoadDataSkills();
            LoadDataItems();
            LoadDataActors();
        }

        void LoadDataActors()
        {
            m_GameActors.LoadFromCSV(m_UI, @"Resources\Data\Actors.csv");
        }

        void LoadDataItems()
        {
            // load all data.
            m_GameItems.LoadMedicineFromCSV(m_UI, @"Resources\Data\Items_Medicine.csv");
            m_GameItems.LoadFoodFromCSV(m_UI, @"Resources\Data\Items_Food.csv");
            m_GameItems.LoadMeleeWeaponsFromCSV(m_UI, @"Resources\Data\Items_MeleeWeapons.csv");
            m_GameItems.LoadRangedWeaponsFromCSV(m_UI, @"Resources\Data\Items_RangedWeapons.csv");
            m_GameItems.LoadExplosivesFromCSV(m_UI, @"Resources\Data\Items_Explosives.csv");
            m_GameItems.LoadBarricadingMaterialFromCSV(m_UI, @"Resources\Data\Items_Barricading.csv");
            m_GameItems.LoadArmorsFromCSV(m_UI, @"Resources\Data\Items_Armors.csv");
            m_GameItems.LoadTrackersFromCSV(m_UI, @"Resources\Data\Items_Trackers.csv");
            m_GameItems.LoadSpraypaintsFromCSV(m_UI, @"Resources\Data\Items_Spraypaints.csv");
            m_GameItems.LoadLightsFromCSV(m_UI, @"Resources\Data\Items_Lights.csv");
            m_GameItems.LoadScentspraysFromCSV(m_UI, @"Resources\Data\Items_Scentsprays.csv");
            m_GameItems.LoadTrapsFromCSV(m_UI, @"Resources\Data\Items_Traps.csv");
            m_GameItems.LoadEntertainmentFromCSV(m_UI, @"Resources\Data\Items_Entertainment.csv");

            // create.
            m_GameItems.CreateModels();
        }

        void LoadDataSkills()
        {
            Skills.LoadSkillsFromCSV(m_UI, @"Resources\Data\Skills.csv");
        }
        #endregion

        // alpha10
        #region Background music
        void UpdateBgMusic()
        {
            if (!s_Options.PlayMusic)
                return;
            if (m_Player == null)
                return;

            // don't interrupt music that has higher priority than bg
            if (m_MusicManager.IsPlaying && m_MusicManager.Priority > MusicPriority.PRIORITY_BGM)
                return;

            // get current map music and play it if not already playing it
            string mapMusic = m_Session.CurrentMap.BgMusic;
            if (string.IsNullOrEmpty(mapMusic))
                return;
            if (m_MusicManager.Music == mapMusic && m_MusicManager.IsPlaying)
                return;

            m_MusicManager.Stop();
            m_MusicManager.Play(mapMusic, MusicPriority.PRIORITY_BGM);
        }
        #endregion

#if DEBUG
        #region Dev
        void AddDevCheatItems()
        {
            /*        
            Inventory inv = m_Player.Inventory;
   
            Item it = new ItemBarricadeMaterial(m_GameItems.WOODENPLANK) { Quantity = 4 };
            inv.AddAll(it);
            it = new ItemBarricadeMaterial(m_GameItems.WOODENPLANK) { Quantity = 3 };
            inv.AddAll(it);
            it = new ItemBarricadeMaterial(m_GameItems.WOODENPLANK) { Quantity = 2 };
            inv.AddAll(it);

            Map map = m_Player.Location.Map;
            Point pos = m_Player.Location.Position;
            it = new ItemBarricadeMaterial(m_GameItems.WOODENPLANK) { Quantity = 3 };
            map.DropItemAt(it, pos);
            it = new ItemBarricadeMaterial(m_GameItems.WOODENPLANK) { Quantity = 3 };
            map.DropItemAt(it, pos);
            Item it2 = new ItemBarricadeMaterial(m_GameItems.WOODENPLANK) { Quantity = 3 };
            map.DropItemAt(it2, pos);
            it.Quantity--;
            */

            /* traps
            Item it = new ItemTrap(m_GameItems.BEAR_TRAP);
            inv.AddAll(it);
            it = new ItemTrap(m_GameItems.BEAR_TRAP);
            inv.AddAll(it);
            it = new ItemTrap(m_GameItems.BEAR_TRAP);
            inv.AddAll(it);
            it = new ItemTrap(m_GameItems.BEAR_TRAP);
            inv.AddAll(it);
            */
        }

        void AddDevCheatSkills()
        {
            // all the living skills.
            /*
            for (int id = (int)Skills.IDs._FIRST_LIVING; id < (int)Skills.IDs._FIRST_UNDEAD; id++)
                for (int l = 0; l < 5; l++)
                    m_Player.Sheet.SkillTable.AddOrIncreaseSkill(id);
            */
        }

        void AddDevMiscStuff()
        {
            // insane right of the bat.
            /*            
            m_Player.Sanity = 0;
            */

            // join cops.
            //m_Player.Faction = GameFactions.ThePolice;
        }

        void DrawTileDev(Map m, Tile t, int x, int y, Point toScreen)
        {
        }
        #endregion
#endif
    }
}
