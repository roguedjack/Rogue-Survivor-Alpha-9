using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;

using djack.RogueSurvivor.Engine;

namespace djack.RogueSurvivor.UI
{
    public partial class GDIGameCanvas : UserControl, IGameCanvas
    {
        #region Types
        public interface IGfx
        {
            void Draw(Graphics g);
        }
        #endregion

        #region Fields
        RogueForm m_RogueForm;
        Bitmap m_RenderImage;
        Graphics m_RenderGraphics;
        bool m_NeedRedraw = true;
        Color m_ClearColor = Color.CornflowerBlue;
        List<IGfx> m_Gfxs = new List<IGfx>(100);
        Dictionary<Color, Brush> m_BrushesCache = new Dictionary<Color, Brush>(32);
        Dictionary<Color, Pen> m_PensCache = new Dictionary<Color, Pen>(32);
        Bitmap m_MinimapBitmap;
        Color[,] m_MinimapColors = new Color[RogueGame.MINITILE_SIZE * RogueGame.MAP_MAX_WIDTH, RogueGame.MINITILE_SIZE * RogueGame.MAP_MAX_HEIGHT];
        #endregion

        #region Properties
        public bool ShowFPS { get; set; }

        public bool NeedRedraw
        {
            get { return m_NeedRedraw; }
            set { m_NeedRedraw = value; }
        }

        public Point MouseLocation { get; set; }

        public float ScaleX
        {
            get
            {
                if (m_RogueForm == null)
                    return 1f;
                return (float)m_RogueForm.ClientRectangle.Width / (float)RogueGame.CANVAS_WIDTH;
            }
        }

        public float ScaleY
        {
            get
            {
                if (m_RogueForm == null)
                    return 1f;
                return (float)m_RogueForm.ClientRectangle.Height / (float)RogueGame.CANVAS_HEIGHT;
            }
        }
        #endregion

        #region Init
        public GDIGameCanvas()
        {
            InitializeComponent();

            m_RenderImage = new Bitmap(RogueGame.CANVAS_WIDTH, RogueGame.CANVAS_HEIGHT);
            m_RenderGraphics = Graphics.FromImage(m_RenderImage);
            m_MinimapBitmap = new Bitmap(RogueGame.MINITILE_SIZE * RogueGame.MAP_MAX_WIDTH, RogueGame.MINITILE_SIZE * RogueGame.MAP_MAX_HEIGHT);
        }
        #endregion


        #region UserControl
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            m_RogueForm.UI_PostKey(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            this.MouseLocation = e.Location;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            double T0 = DateTime.UtcNow.TimeOfDay.TotalMilliseconds;

            if (m_NeedRedraw)
            {
                DoDraw(m_RenderGraphics);
                m_NeedRedraw = false;
            }

            if (ScaleX == 1f && ScaleY == 1f)
                e.Graphics.DrawImageUnscaled(m_RenderImage, 0, 0);
            else
                e.Graphics.DrawImage(m_RenderImage, m_RogueForm.ClientRectangle);

            double T1 = DateTime.UtcNow.TimeOfDay.TotalMilliseconds;

            if (ShowFPS)
            {
                double DT = T1 - T0;
                if (DT == 0) DT = Double.Epsilon;
                e.Graphics.DrawString(String.Format("Frame time={0:F} FPS={1:F}", DT, 1000.0 / DT), Font, Brushes.Yellow, 0, 0);
            }
        }
        #endregion

        #region Drawing
        void DoDraw(Graphics g)
        {
            if (m_RogueForm == null)
                return;

            /////////
            // Clear
            /////////
            g.Clear(m_ClearColor);

            ///////////////////
            // Render each gfx
            ///////////////////
            foreach (IGfx gfx in m_Gfxs)
                gfx.Draw(g);
        }
        #endregion

        #region IGameCanvas implementation
        public void BindForm(RogueForm form)
        {
            m_RogueForm = form;
            FillGameForm();
        }

        public void FillGameForm()
        {
            Location = new Point(0, 0);
            if (m_RogueForm != null)
                Size = MinimumSize = MaximumSize = m_RogueForm.Size;
        }

        public void Clear(Color clearColor)
        {
            m_ClearColor = clearColor;
            m_Gfxs.Clear();

            m_NeedRedraw = true;
        }

        public void AddImage(Image img, int x, int y)
        {
            m_Gfxs.Add(new GfxImage(img, x, y));
            m_NeedRedraw = true;
        }

        /// <summary>
        /// GDI implemention ignore the color parameter.
        /// </summary>
        /// <param name="img"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color">not used in GDI implementation</param>
        public void AddImage(Image img, int x, int y, Color color)
        {
            AddImage(img, x, y);
        }

        public void AddTransparentImage(float alpha, Image img, int x, int y)
        {
            m_Gfxs.Add(new GfxTransparentImage(alpha, img, x, y));
            m_NeedRedraw = true;
        }

        public void AddPoint(Color color, int x, int y)
        {
            m_Gfxs.Add(new GfxRect(GetPen(color), new Rectangle(x, y, 1, 1)));
            m_NeedRedraw = true;
        }

        public void AddLine(Color color, int xFrom, int yFrom, int xTo, int yTo)
        {
            m_Gfxs.Add(new GfxLine(GetPen(color), xFrom, yFrom, xTo, yTo));
            m_NeedRedraw = true;
        }

        public void AddString(Font font, Color color, string text, int gx, int gy)
        {
            m_Gfxs.Add(new GfxString(color, font, text, gx, gy));
            m_NeedRedraw = true;
        }

        public void AddRect(Color color, Rectangle rect)
        {
            m_Gfxs.Add(new GfxRect(GetPen(color), rect));
            m_NeedRedraw = true;
        }

        private Pen GetPen(Color color)
        {
            Pen p;
            if (!m_PensCache.TryGetValue(color, out p))
            {
                p = new Pen(color);
                m_PensCache.Add(color, p);
            }
            return p;
        }

        public void AddFilledRect(Color color, Rectangle rect)
        {
            Brush brush;
            if (!m_BrushesCache.TryGetValue(color, out brush))
            {
                brush = new SolidBrush(color);
                m_BrushesCache.Add(color, brush);
            }

            m_Gfxs.Add(new GfxFilledRect(brush, rect));
            m_NeedRedraw = true;
        }

        public void ClearMinimap(Color color)
        {
            for (int x = 0; x < RogueGame.MAP_MAX_WIDTH; x++)
                for (int y = 0; y < RogueGame.MAP_MAX_HEIGHT; y++)
                    SetMinimapColor(x, y, color);
        }

        public void SetMinimapColor(int x, int y, Color color)
        {
            // Set color in minimap.
            int x0 = x * RogueGame.MINITILE_SIZE;
            int y0 = y * RogueGame.MINITILE_SIZE;
            for (int px = 0; px < RogueGame.MINITILE_SIZE; px++)
                for (int py = 0; py < RogueGame.MINITILE_SIZE; py++)
                    m_MinimapColors[x0 + px, y0 + py] = color;
        }

        public void DrawMinimap(int gx, int gy)
        {
            ///////////
            // Add gfx.
            ///////////
            /* TODO
            m_Gfxs.Add(new GfxSprite(m_Sprite, new SizeF(RogueGame.MINITILE_SIZE * RogueGame.MAP_MAX_WIDTH, RogueGame.MINITILE_SIZE * RogueGame.MAP_MAX_HEIGHT),
                RogueGame.MINITILE_SIZE * RogueGame.MAP_MAX_WIDTH, RogueGame.MINITILE_SIZE * RogueGame.MAP_MAX_HEIGHT,
                m_MinimapTexture,
                Color.White,
                gx, gy));
             */

            NeedRedraw = true;
        }

        public string SaveScreenShot(string filePath)
        {
            string file = filePath + "." + ScreenshotExtension();
            Logger.WriteLine(Logger.Stage.RUN_GFX, "taking screenshot...");
            try
            {
                //TODO SurfaceLoader.Save(filePath, ImageFileFormat.Png, m_RenderSurface);
            }
            catch (Exception e)
            {
                Logger.WriteLine(Logger.Stage.RUN_GFX, String.Format("exception when taking screenshot : {0}", e.ToString()));
                return null;
            }
            Logger.WriteLine(Logger.Stage.RUN_GFX, "taking screenshot... done!");
            return file;
        }

        public string ScreenshotExtension()
        {
            return "png";
        }

        public void DisposeUnmanagedResources()
        {
            // nothing to do.
        }        
        #endregion

        #region Concrete gfxs
        class GfxImage : IGfx
        {
            readonly Image m_Img;
            readonly int m_X;
            readonly int m_Y;

            public GfxImage(Image img, int x, int y)
            {
                m_Img = img;
                m_X = x;
                m_Y = y;
            }

            public void Draw(Graphics g)
            {
                g.DrawImageUnscaled(m_Img, m_X, m_Y);
            }
        }

        class GfxTransparentImage : IGfx
        {
            readonly Image m_Img;
            readonly int m_X;
            readonly int m_Y;
            readonly float m_Alpha;

            readonly ImageAttributes m_ImgAttributes;

            public GfxTransparentImage(float alpha, Image img, int x, int y)
            {
                m_Img = img;
                m_X = x;
                m_Y = y;
                m_Alpha = alpha;

                float[][] m = new float[][] {
                    new float[] {1.0f, 0.0f, 0.0f, 0.0f,  0.0f},
                    new float[] {0.0f, 1.0f, 0.0f, 0.0f,  0.0f},
                    new float[] {0.0f, 0.0f, 1.0f, 0.0f,  0.0f},
                    new float[] {0.0f, 0.0f, 0.0f, alpha, 0.0f},
                    new float[] {0.0f, 0.0f, 0.0f, 0.0f,  1.0f}
                };

                m_ImgAttributes = new ImageAttributes();
                m_ImgAttributes.SetColorMatrix(new ColorMatrix(m), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            }

            public void Draw(Graphics g)
            {
                g.DrawImage(m_Img, new Rectangle(m_X, m_Y, m_Img.Width, m_Img.Height), 0, 0, m_Img.Width, m_Img.Height, GraphicsUnit.Pixel, m_ImgAttributes);
            }
        }

        class GfxLine : IGfx
        {
            readonly Pen m_Pen;
            readonly int m_xFrom, m_yFrom;
            readonly int m_xTo, m_yTo;

            public GfxLine(Pen pen, int xFrom, int yFrom, int xTo, int yTo)
            {
                m_Pen = pen;
                m_xFrom = xFrom;
                m_yFrom = yFrom;
                m_xTo = xTo;
                m_yTo = yTo;
            }

            public void Draw(Graphics g)
            {
                g.DrawLine(m_Pen, m_xFrom, m_yFrom, m_xTo, m_yTo);
            }
        }

        class GfxString : IGfx
        {
            readonly Color m_Color;
            readonly Font m_Font;
            readonly string m_Text;
            readonly int m_X;
            readonly int m_Y;
            readonly Brush m_Brush;

            public GfxString(Color color, Font font, string text, int x, int y)
            {
                m_Color = color;
                m_Font = font;
                m_Text = text;
                m_X = x;
                m_Y = y;
                m_Brush = new SolidBrush(color);
            }

            public void Draw(Graphics g)
            {
                g.DrawString(m_Text, m_Font, m_Brush, m_X, m_Y);
            }
        }

        class GfxFilledRect : IGfx
        {
            readonly Brush m_Brush;
            readonly Rectangle m_Rect;

            public GfxFilledRect(Brush brush, Rectangle rect)
            {
                m_Brush = brush;
                m_Rect = rect;
            }

            public void Draw(Graphics g)
            {
                g.FillRectangle(m_Brush, m_Rect);
            }
        }

        class GfxRect : IGfx
        {
            readonly Pen m_Pen;
            readonly Rectangle m_Rect;

            public GfxRect(Pen pen, Rectangle rect)
            {
                m_Pen = pen;
                m_Rect = rect;
            }

            public void Draw(Graphics g)
            {
                g.DrawRectangle(m_Pen, m_Rect);
            }
        }
        #endregion
    }
}
