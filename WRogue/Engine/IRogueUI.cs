using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace djack.RogueSurvivor.Engine
{
    /// <summary>
    /// Provides UI functionalities to a Rogue game.
    /// </summary>
    interface IRogueUI
    {
        #region Input
        KeyEventArgs UI_WaitKey();
        KeyEventArgs UI_PeekKey();
        void UI_PostKey(KeyEventArgs e);

        Point UI_GetMousePosition();
        MouseButtons? UI_PeekMouseButtons();
        void UI_PostMouseButtons(MouseButtons buttons);

        void UI_SetCursor(Cursor cursor);
        #endregion

        #region Delay
        void UI_Wait(int msecs);
        #endregion

        #region Canvas Painting
        void UI_Repaint();
        void UI_Clear(Color clearColor);
        void UI_DrawImage(string imageID, int gx, int gy);
        void UI_DrawImage(string imageID, int gx, int gy, Color tint);
        void UI_DrawImageTransform(string imageID, int gx, int gy, float rotation, float scale);
        void UI_DrawGrayLevelImage(string imageID, int gx, int gy);
        void UI_DrawTransparentImage(float alpha, string imageID, int gx, int gy);
        void UI_DrawPoint(Color color, int gx, int gy);
        void UI_DrawLine(Color color, int gxFrom, int gyFrom, int gxTo, int gyTo);
        void UI_DrawRect(Color color, Rectangle rect);
        void UI_FillRect(Color color, Rectangle rect);
        void UI_DrawString(Color color, string text, int gx, int gy, Color? shadowColor = null);
        void UI_DrawStringBold(Color color, string text, int gx, int gy, Color? shadowColor = null);
        void UI_DrawPopup(string[] lines, Color textColor, Color boxBorderColor, Color boxFillColor, int gx, int gy);
        // alpha10
        void UI_DrawPopupTitle(string title, Color titleColor, string[] lines, Color textColor, Color boxBorderColor, Color boxFillColor, int gx, int gy);
        void UI_DrawPopupTitleColors(string title, Color titleColor, string[] lines, Color[] colors, Color boxBorderColor, Color boxFillColor, int gx, int gy);

        #region Minimap painting
        void UI_ClearMinimap(Color color);
        void UI_SetMinimapColor(int x, int y, Color color);
        void UI_DrawMinimap(int gx, int gy);
        #endregion

        #endregion

        #region Canvas scaling - to convert mouse coordinates to canvas coordinates.
        float UI_GetCanvasScaleX();
        float UI_GetCanvasScaleY();
        #endregion

        #region Screenshots
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath">file path without the extension, eg: c:\screenshot100</param>
        /// <returns></returns>
        string UI_SaveScreenshot(string filePath);

        /// <summary>
        /// Extension without the point eg: "png".
        /// </summary>
        /// <returns></returns>
        string UI_ScreenshotExtension();
        #endregion

        #region Exiting
        void UI_DoQuit();
        #endregion
    }
}
