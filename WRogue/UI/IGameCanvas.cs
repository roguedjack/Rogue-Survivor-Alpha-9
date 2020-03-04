using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using GDIFont = System.Drawing.Font;

namespace djack.RogueSurvivor.UI
{
    public interface IGameCanvas
    {
        #region Properties
        bool ShowFPS { get; set; }
        bool NeedRedraw { get; set; }
        Point MouseLocation { get; set; }
        float ScaleX { get; }
        float ScaleY { get; }
        #endregion

        #region GameCanvas Interface
        void BindForm(RogueForm form);
        void FillGameForm();

        void Clear(Color clearColor);

        void AddImage(Image img, int x, int y);
        void AddImage(Image img, int x, int y, Color tint);
        void AddImageTransform(Image img, int x, int y, float rotation, float scale);
        void AddTransparentImage(float alpha, Image img, int x, int y);

        void AddPoint(Color color, int x, int y);
        void AddLine(Color color, int xFrom, int yFrom, int xTo, int yTo);
        void AddRect(Color color, Rectangle rect);
        void AddFilledRect(Color color, Rectangle rect);
        void AddString(GDIFont font, Color color, string text, int gx, int gy);

        void ClearMinimap(Color color);
        void SetMinimapColor(int x, int y, Color color);
        void DrawMinimap(int gx, int gy);

        /// <summary>
        /// Saves to a png.
        /// </summary>
        /// <param name="filePath">with the .png extension</param>
        /// <returns></returns>
        string SaveScreenShot(string filePath);
        string ScreenshotExtension();

        void DisposeUnmanagedResources();
        #endregion
    }
}
