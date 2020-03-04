using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

using GDIFont = System.Drawing.Font;
using DXFont = Microsoft.DirectX.Direct3D.Font;

using djack.RogueSurvivor.Engine;

namespace djack.RogueSurvivor.UI
{
    public partial class DXGameCanvas : UserControl, IGameCanvas
    {
        #region Types
        interface IGfx
        {
            void Draw(Device dev);
        }
        #endregion

        #region Fields
        RogueForm m_RogueForm;
        bool m_NeedRedraw = true;
        Color m_ClearColor = Color.CornflowerBlue;
        List<IGfx> m_Gfxs = new List<IGfx>(100);

        #region DirectX
        bool m_DXInitialized;
        PresentParameters m_PresentParameters;
        Device m_Device;

        #region Automatic zoom support

        /// <summary>
        /// We render the scene to this texture.
        /// </summary>
        Texture m_RenderTexture;

        Surface m_RenderSurface;

        /// <summary>
        /// Helper to render to the texture.
        /// </summary>
        RenderToSurface m_RenderToSurface;
        #endregion

        /// <summary>
        /// GDI+ images to DirectX textures.
        /// </summary>
        Dictionary<Image, Texture> m_ImageToTextures = new Dictionary<Image, Texture>(32);

        /// <summary>
        /// GDI+ fonts to DirectX fonts.
        /// </summary>
        Dictionary<GDIFont, DXFont> m_FontsToFonts = new Dictionary<GDIFont, DXFont>(3);

        /// <summary>
        /// Sprite helper used to draw images.
        /// </summary>
        Sprite m_Sprite;

        /// <summary>
        /// Sprite helper used to draw text.
        /// </summary>
        Sprite m_TextSprite;

        /// <summary>
        /// Blank texture to draw colored shapes.
        /// </summary>
        Texture m_BlankTexture;

        /// <summary>
        /// Texture where to draw minimap.
        /// </summary>
        Texture m_MinimapTexture;

        /// <summary>
        /// Minimap
        /// </summary>
        Color[,] m_MinimapColors = new Color[RogueGame.MINITILE_SIZE * RogueGame.MAP_MAX_WIDTH, RogueGame.MINITILE_SIZE * RogueGame.MAP_MAX_HEIGHT];
        byte[] m_MinimapBytes = new byte[4 * RogueGame.MINITILE_SIZE * RogueGame.MAP_MAX_WIDTH * RogueGame.MINITILE_SIZE * RogueGame.MAP_MAX_HEIGHT];

        #endregion

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
        public DXGameCanvas()
        {
            Logger.WriteLine(Logger.Stage.INIT_GFX, "DXGameCanvas::InitializeComponent");
            InitializeComponent();

            // prevent flickering (gdi conflicting with directx)
            Logger.WriteLine(Logger.Stage.INIT_GFX, "DXGameCanvas::SetStyle");
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.Opaque | ControlStyles.EnableNotifyMessage, true);

            Logger.WriteLine(Logger.Stage.INIT_GFX, "DXGameCanvas() done.");
        }
        #endregion

        #region DirectX creation/maintenance
        public void InitDX()
        {
            m_PresentParameters = new PresentParameters()
            {
                Windowed = true,
                SwapEffect = SwapEffect.Discard
            };

            Logger.WriteLine(Logger.Stage.INIT_GFX, "creating device...");
            m_Device = new Device(0, DeviceType.Hardware, this, CreateFlags.SoftwareVertexProcessing, m_PresentParameters);

            Logger.WriteLine(Logger.Stage.INIT_GFX, "device info :");
            AdapterDetails devInfo = Manager.Adapters[0].Information;
            Caps devCaps = m_Device.DeviceCaps;
            Logger.WriteLine(Logger.Stage.INIT_GFX, String.Format("- device desc           : {0}", devInfo.Description));
            Logger.WriteLine(Logger.Stage.INIT_GFX, String.Format("- max texture size      : {0}x{1}", devCaps.MaxTextureWidth, devCaps.MaxTextureHeight));
            Logger.WriteLine(Logger.Stage.INIT_GFX, String.Format("- vertex shader version : {0}", devCaps.VertexShaderVersion.ToString()));
            Logger.WriteLine(Logger.Stage.INIT_GFX, String.Format("- pixel shader version  : {0}", devCaps.PixelShaderVersion.ToString()));

            Logger.WriteLine(Logger.Stage.INIT_GFX, "device reset..");
            m_Device_DeviceReset(m_Device, null);

            m_Device.DeviceLost += new EventHandler(m_Device_DeviceLost);
            m_Device.DeviceReset += new EventHandler(m_Device_DeviceReset);

            m_DXInitialized = true;
        }

        void m_Device_DeviceLost(object sender, EventArgs e)
        {
            // Device Lost
            // http://msdn.microsoft.com/en-us/library/bb174714(VS.85).aspx
            // Result Codes
            // http://msdn.microsoft.com/en-us/library/ms858172.aspx

            // ignore if device disposed.
            if (m_Device == null || m_Device.Disposed)
                return;

            // while device lost, loop.
            int result;
            while(true)
            {
                Thread.Sleep(100);
                if (m_Device.CheckCooperativeLevel(out result))
                    return;
                if (result != (int)ResultCode.DeviceLost)
                    break;
            }

            // if ready for reset, do it.
            if (result == (int)ResultCode.DeviceNotReset)
                m_Device.Reset(m_PresentParameters);
        }

        void m_Device_DeviceReset(object sender, EventArgs e)
        {
            m_Device.RenderState.CullMode = Cull.None;
            m_Device.RenderState.AlphaBlendEnable = true;

            m_ImageToTextures.Clear();
            m_FontsToFonts.Clear();

            Logger.WriteLine(Logger.Stage.INIT_GFX, "creating sprite...");
            m_Sprite = new Sprite(m_Device);

            Logger.WriteLine(Logger.Stage.INIT_GFX, "creating text sprite...");
            m_TextSprite = new Sprite(m_Device);

            Logger.WriteLine(Logger.Stage.INIT_GFX, "creating blank texture...");
            m_BlankTexture = new Texture(m_Device, new Bitmap(@"Resources\Images\blank_texture.png"), 0, Pool.Managed);

            if (m_RenderTexture != null)
            {
                Logger.WriteLine(Logger.Stage.CLEAN_GFX, "disposing render texture...");
                m_RenderTexture.Dispose();
                m_RenderTexture = null;
            }

            Logger.WriteLine(Logger.Stage.INIT_GFX, "creating render texture...");
            m_RenderTexture = new Texture(m_Device, RogueGame.CANVAS_WIDTH, RogueGame.CANVAS_HEIGHT, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
            m_RenderSurface = m_RenderTexture.GetSurfaceLevel(0);

            Logger.WriteLine(Logger.Stage.INIT_GFX, "creating render surface...");
            m_RenderToSurface = new RenderToSurface(m_Device, RogueGame.CANVAS_WIDTH, RogueGame.CANVAS_HEIGHT, Format.A8R8G8B8, false, DepthFormat.Unknown);

            Logger.WriteLine(Logger.Stage.INIT_GFX, "creating minimap texture...");
            const int numLevels = 1;    // 0 = crash on some cards plus it means generate a bunch of mipmaps so bad anyway.
            m_MinimapTexture = new Texture(m_Device,
                RogueGame.MAP_MAX_WIDTH * RogueGame.MINITILE_SIZE, RogueGame.MAP_MAX_HEIGHT * RogueGame.MINITILE_SIZE,
                numLevels, Usage.SoftwareProcessing,
                Format.A8R8G8B8,
                Pool.Managed);

            Logger.WriteLine(Logger.Stage.INIT_GFX, "init done.");
        }
        #endregion

        #region UserControl
        protected override void OnCreateControl()
        {
            if (!DesignMode)
            {
                InitDX();
            }

            base.OnCreateControl();

        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            m_RogueForm.UI_PostKey(e);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return true;
#if false
            switch (keyData)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                    return true;

                default:
                    return base.IsInputKey(keyData);
            }
#endif
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            this.MouseLocation = e.Location;
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            m_RogueForm.UI_PostMouseButtons(e.Button);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            NeedRedraw = true;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!m_DXInitialized)
                return;

            double T0 = DateTime.UtcNow.TimeOfDay.TotalMilliseconds;

            if (NeedRedraw)
            {
                DoDraw(m_Device);
                m_NeedRedraw = false;
            }
            m_Device.Present();

            double T1 = DateTime.UtcNow.TimeOfDay.TotalMilliseconds;

            if (ShowFPS)
            {
                double DT = T1 - T0;
                if (DT == 0) DT = Double.Epsilon;
                e.Graphics.DrawString(String.Format("Frame time={0:F} FPS={1:F}", DT, 1000.0 / DT), Font, Brushes.Yellow, ClientRectangle.Right - 200, ClientRectangle.Bottom - 64);
            }

        }
        #endregion

        #region Drawing
        void DoDraw(Device dev)
        {
            if (m_RogueForm == null)
                return;

            ///////////////////////////////////
            // Render the scene to the texture
            ///////////////////////////////////
            m_RenderToSurface.BeginScene(m_RenderSurface);
            {
                /////////
                // Clear
                /////////
                dev.Clear(ClearFlags.Target, m_ClearColor, 1.0f, 0);

                ///////////////////
                // Render each gfx
                ///////////////////
                foreach (IGfx gfx in m_Gfxs)
                {
                    gfx.Draw(dev);
                }
            }
            m_RenderToSurface.EndScene(Filter.None);

            //////////////////////////////////////////////////////
            // Then draw scene texture scaled to fit whole canvas
            //////////////////////////////////////////////////////
            m_Device.BeginScene();
            {
                m_Sprite.Begin(SpriteFlags.None);
                {
                    m_Sprite.Draw2D(m_RenderTexture, new Rectangle(0, 0, RogueGame.CANVAS_WIDTH, RogueGame.CANVAS_HEIGHT),
                        new SizeF(m_RogueForm.ClientRectangle.Width, m_RogueForm.ClientRectangle.Height), PointF.Empty, Color.White);
                }
                m_Sprite.End();
            }
            m_Device.EndScene();
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
            m_Gfxs.Add(new GfxSprite(m_Sprite, new SizeF(img.Width, img.Height), img.Width, img.Height, GetTexture(img), Color.White, x, y));
            m_NeedRedraw = true;
        }

        public void AddImage(Image img, int x, int y, Color tint)
        {
            m_Gfxs.Add(new GfxSprite(m_Sprite, new SizeF(img.Width, img.Height), img.Width, img.Height, GetTexture(img), tint, x, y));
            m_NeedRedraw = true;
        }

        public void AddImageTransform(Image img, int x, int y, float rotation, float scale)
        {
            m_Gfxs.Add(new GfxSpriteTransform(rotation, scale, m_Sprite, new SizeF(img.Width, img.Height), img.Width, img.Height, GetTexture(img), Color.White, x, y));
        }

        public void AddTransparentImage(float alpha, Image img, int x, int y)
        {
            m_Gfxs.Add(new GfxSprite(m_Sprite, new SizeF(img.Width, img.Height), img.Width, img.Height, GetTexture(img), Color.FromArgb((int)(255 * alpha), Color.White), x, y));
            m_NeedRedraw = true;
        }

        public void AddPoint(Color color, int x, int y)
        {
            m_Gfxs.Add(new GfxRect(color, new Rectangle(x, y, 1, 1)));
            m_NeedRedraw = true;
        }

        public void AddLine(Color color, int xFrom, int yFrom, int xTo, int yTo)
        {
            m_Gfxs.Add(new GfxLine(color, xFrom, yFrom, xTo, yTo));
            m_NeedRedraw = true;
        }

        public void AddString(GDIFont font, Color color, string text, int gx, int gy)
        {
            m_Gfxs.Add(new GfxString(m_TextSprite, color, GetDXFont(font), text, gx, gy));
            m_NeedRedraw = true;
        }

        public void AddRect(Color color, Rectangle rect)
        {
            m_Gfxs.Add(new GfxRect(color, rect));
            m_NeedRedraw = true;
        }

        public void AddFilledRect(Color color, Rectangle rect)
        {
            m_Gfxs.Add(new GfxSprite(m_Sprite, new SizeF(rect.Width, rect.Height), 4, 4, m_BlankTexture, color, rect.Left, rect.Top));
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
            /////////////////////////////////
            // Write minimapColor to texture     
            /////////////////////////////////
#if false
            // BUGGY: assumes texture pitch = texture width, which DirectX does NOT garantee. 
            // convert colors to bytes and write all at once (much faster than convert+writing one by one)
            int iByte = 0;
            for (int y = 0; y < RogueGame.MINITILE_SIZE * RogueGame.MAP_MAX_HEIGHT; y++)
                for (int x = 0; x < RogueGame.MINITILE_SIZE * RogueGame.MAP_MAX_WIDTH; x++)
                {
                    m_MinimapBytes[iByte++] = m_MinimapColors[x, y].B;
                    m_MinimapBytes[iByte++] = m_MinimapColors[x, y].G;
                    m_MinimapBytes[iByte++] = m_MinimapColors[x, y].R;
                    m_MinimapBytes[iByte++] = m_MinimapColors[x, y].A;
                }
            GraphicsStream gs = m_MinimapTexture.LockRectangle(0, LockFlags.None);
            gs.Write(m_MinimapBytes, 0, m_MinimapBytes.Length);
            m_MinimapTexture.UnlockRectangle(0);
#endif

#if true
            // correct way to do it, uses pitch.
            int pitch;  // pitch = width of texture in memory, does not necessary equal to texture.Width.
            const int bytesPerPixel = 4; // A8R8G8B8
            GraphicsStream gs = m_MinimapTexture.LockRectangle(0, LockFlags.None, out pitch);
            for (int y = 0; y < RogueGame.MINITILE_SIZE * RogueGame.MAP_MAX_HEIGHT; y++)
                for (int x = 0; x < RogueGame.MINITILE_SIZE * RogueGame.MAP_MAX_WIDTH; x++)
                {
                    Color color = m_MinimapColors[x, y];
                    gs.Position = (y * pitch) + (x * bytesPerPixel);
                    gs.WriteByte(color.B);
                    gs.WriteByte(color.G);
                    gs.WriteByte(color.R);
                    gs.WriteByte(color.A);
                }
            m_MinimapTexture.UnlockRectangle(0);
#endif

            ///////////
            // Add gfx.
            ///////////
            m_Gfxs.Add(new GfxSprite(m_Sprite, new SizeF(RogueGame.MINITILE_SIZE * RogueGame.MAP_MAX_WIDTH, RogueGame.MINITILE_SIZE * RogueGame.MAP_MAX_HEIGHT),
                RogueGame.MINITILE_SIZE * RogueGame.MAP_MAX_WIDTH, RogueGame.MINITILE_SIZE * RogueGame.MAP_MAX_HEIGHT,
                m_MinimapTexture,
                Color.White,
                gx, gy));

            NeedRedraw = true;
        }

        public string SaveScreenShot(string filePath)
        {
            string file = filePath + "." + ScreenshotExtension();
            Logger.WriteLine(Logger.Stage.RUN_GFX, "taking screenshot...");
            try
            {
                SurfaceLoader.Save(filePath, ImageFileFormat.Png, m_RenderSurface);
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
            DisposeD3D();
        }
        #endregion

        #region DirectX resources
        Texture GetTexture(Image img)
        {
            Texture texture;
            if (m_ImageToTextures.TryGetValue(img, out texture))
                return texture;

            texture = Texture.FromBitmap(m_Device, new Bitmap(img), Usage.SoftwareProcessing, Pool.Managed);
            m_ImageToTextures.Add(img, texture);
            return texture;
        }

        DXFont GetDXFont(GDIFont font)
        {
            DXFont dxFont;
            if (m_FontsToFonts.TryGetValue(font, out dxFont))
                return dxFont;

            dxFont = new DXFont(m_Device, font);
            m_FontsToFonts.Add(font, dxFont);
            return dxFont;
        }
        #endregion

        #region Concrete gfxs
        class GfxSprite : IGfx
        {
            readonly Sprite m_Sprite;
            readonly Texture m_Texture;
            readonly Color m_Color;
            readonly int m_X;
            readonly int m_Y;
            readonly SizeF m_Size;
            readonly int m_TexWidth;
            readonly int m_TexHeight;

            public GfxSprite(Sprite sprite, SizeF size, int texWidth, int texHeight, Texture texture, Color color, int x, int y)
            {
                m_Sprite = sprite;
                m_Texture = texture;
                m_Color = color;
                m_X = x;
                m_Y = y;
                m_Size = size;
                m_TexWidth = texWidth;
                m_TexHeight = texHeight;
            }

            public void Draw(Device dev)
            {
                m_Sprite.Begin(SpriteFlags.AlphaBlend);
                {
                    m_Sprite.Draw2D(m_Texture, new Rectangle(0, 0, m_TexWidth, m_TexHeight), m_Size, new PointF(m_X, m_Y), m_Color);
                }
                m_Sprite.End();
            }
        }

        class GfxSpriteTransform : IGfx
        {
            readonly float m_Rotation;
            readonly float m_Scale;
            readonly Sprite m_Sprite;
            readonly Texture m_Texture;
            readonly Color m_Color;
            readonly int m_X;
            readonly int m_Y;
            readonly SizeF m_Size;
            readonly SizeF m_SizeScaled;
            readonly int m_TexWidth;
            readonly int m_TexHeight;
            readonly PointF m_RotationCenter;

            public GfxSpriteTransform(float rotation, float scale, Sprite sprite, SizeF size, int texWidth, int texHeight, Texture texture, Color color, int x, int y)
            {
                m_Rotation = (rotation * (float)Math.PI) / 180.0f;
                m_Scale = scale;
                m_Sprite = sprite;
                m_Texture = texture;
                m_Color = color;
                m_X = x;
                m_Y = y;
                m_Size = size;
                m_TexWidth = texWidth;
                m_TexHeight = texHeight;
                m_SizeScaled = new SizeF(size.Width * scale, size.Height * scale);
                m_RotationCenter = new PointF(texWidth / 2, texHeight / 2);
            }

            public void Draw(Device dev)
            {
                m_Sprite.Begin(SpriteFlags.AlphaBlend);
                {
                    m_Sprite.Draw2D(m_Texture, new Rectangle(0, 0, m_TexWidth, m_TexHeight), m_SizeScaled, m_RotationCenter, m_Rotation,
                        new PointF(m_X + m_SizeScaled.Width / 2, m_Y + m_SizeScaled.Height / 2), m_Color);
                }
                m_Sprite.End();
            }
        }

        class GfxLine : IGfx
        {
            readonly CustomVertex.TransformedColored[] m_Points = new CustomVertex.TransformedColored[2];

            public GfxLine(Color color, int xFrom, int yFrom, int xTo, int yTo)
            {
                const float Z = 0;
                const float W = 1;

                int argb = color.ToArgb();

                m_Points[0].Position = new Vector4(xFrom, yFrom, Z, W);
                m_Points[0].Color = argb;

                m_Points[1].Position = new Vector4(xTo, yTo, Z, W);
                m_Points[1].Color = argb;
            }

            public void Draw(Device dev)
            {
                dev.VertexFormat = CustomVertex.TransformedColored.Format;
                dev.SetTexture(0, null);
                dev.DrawUserPrimitives(PrimitiveType.LineList, 1, m_Points);
            }
        }

        class GfxString : IGfx
        {
            readonly Color m_Color;
            readonly DXFont m_Font;
            readonly string m_Text;
            readonly int m_X;
            readonly int m_Y;
            readonly Sprite m_TextSprite;

            public GfxString(Sprite textSprite, Color color, DXFont font, string text, int x, int y)
            {
                m_Color = color;
                m_Font = font;
                m_Text = text;
                m_X = x;
                m_Y = y;
                m_TextSprite = textSprite;
            }

            public void Draw(Device dev)
            {
                m_TextSprite.Begin(SpriteFlags.AlphaBlend);
                {
                    m_Font.DrawText(m_TextSprite, m_Text, m_X, m_Y, m_Color);
                }
                m_TextSprite.End();
            }
        }

        class GfxRect : IGfx
        {
            readonly CustomVertex.TransformedColored[] m_Points = new CustomVertex.TransformedColored[4];
            static readonly Int16[] s_Indices = new Int16[8] { 0, 1, 2, 3, 0, 2, 1, 3 };

            public GfxRect(Color color, Rectangle rect)
            {
                const float Z = 0;
                const float W = 1;

                int argb = color.ToArgb();

                m_Points[0].Position = new Vector4(rect.Left, rect.Top, Z, W);
                m_Points[0].Color = argb;

                m_Points[1].Position = new Vector4(rect.Right, rect.Top, Z, W);
                m_Points[1].Color = argb;

                m_Points[2].Position = new Vector4(rect.Left, rect.Bottom, Z, W);
                m_Points[2].Color = argb;

                m_Points[3].Position = new Vector4(rect.Right, rect.Bottom, Z, W);
                m_Points[3].Color = argb;
            }

            public void Draw(Device dev)
            {
                dev.VertexFormat = CustomVertex.TransformedColored.Format;
                dev.SetTexture(0, null);
                dev.DrawIndexedUserPrimitives(PrimitiveType.LineList, 0, 4, 4, s_Indices, true, m_Points);
            }
        }
        #endregion

        #region Disposing
        void DisposeD3D()
        {
            Logger.WriteLine(Logger.Stage.CLEAN_GFX, "disposing...");

            foreach (Texture t in m_ImageToTextures.Values)
                t.Dispose();
            m_ImageToTextures.Clear();

            foreach (DXFont f in m_FontsToFonts.Values)
                f.Dispose();
            m_FontsToFonts.Clear();

            if (m_BlankTexture != null)
            {
                Logger.WriteLine(Logger.Stage.CLEAN_GFX, "disposing blank texture...");
                m_BlankTexture.Dispose();
                m_BlankTexture = null;
            }

            if (m_MinimapTexture != null)
            {
                Logger.WriteLine(Logger.Stage.CLEAN_GFX, "disposing minimap texture...");
                m_MinimapTexture.Dispose();
                m_MinimapTexture = null;
            }

            if (m_Sprite != null)
            {
                Logger.WriteLine(Logger.Stage.CLEAN_GFX, "disposing sprite...");
                m_Sprite.Dispose();
                m_Sprite = null;
            }

            if (m_TextSprite != null)
            {
                Logger.WriteLine(Logger.Stage.CLEAN_GFX, "disposing text sprite...");
                m_TextSprite.Dispose();
                m_TextSprite = null;
            }

            if (m_RenderToSurface != null)
            {
                Logger.WriteLine(Logger.Stage.CLEAN_GFX, "disposing render surface...");
                m_RenderToSurface.Dispose();
                m_RenderToSurface = null;
            }

            if (m_RenderTexture != null)
            {
                Logger.WriteLine(Logger.Stage.CLEAN_GFX, "disposing render texture...");
                m_RenderTexture.Dispose();
                m_RenderTexture = null;
            }

            if (m_Device != null)
            {
                Logger.WriteLine(Logger.Stage.CLEAN_GFX, "disposing device...");
                m_Device.Dispose();
                m_Device = null;
            }

            Logger.WriteLine(Logger.Stage.CLEAN_GFX, "disposing done.");
        }
        #endregion
    }
}
