namespace djack.RogueSurvivor
{
    /// <summary>
    /// *** constains custom code ***
    /// </summary>
    partial class RogueForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            // dispose canvas resources.
            if (disposing && m_GameCanvas != null)
            {
                m_GameCanvas.DisposeUnmanagedResources();
            }

            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// *** contains custom uggly code! ***
        /// </summary>
        private void InitializeComponent()
        {
            Logger.WriteLine(Logger.Stage.INIT_MAIN, "new ComponentResourceManager...");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RogueForm));
            Logger.WriteLine(Logger.Stage.INIT_MAIN, "creating GameCanvas...");
            switch (SetupConfig.Video)
            {
                case SetupConfig.eVideo.VIDEO_MANAGED_DIRECTX:
                    Logger.WriteLine(Logger.Stage.INIT_MAIN, "DXGameCanvas implementation...");
                    this.m_GameCanvas = new djack.RogueSurvivor.UI.DXGameCanvas();
                    break;
                default:
                    Logger.WriteLine(Logger.Stage.INIT_MAIN, "GDIPlusGameCanvas implementation...");
                    this.m_GameCanvas = new djack.RogueSurvivor.UI.GDIPlusGameCanvas();
                    break;
            }
            Logger.WriteLine(Logger.Stage.INIT_MAIN, "SuspendLayout...");
            this.SuspendLayout();
            // 
            // m_GameCanvas
            // 
            Logger.WriteLine(Logger.Stage.INIT_MAIN, "setup GameCanvas...");
            m_GameCanvas.NeedRedraw = true;
            // FIXME uggly hax :) use some proper design pattern instead...
            System.Windows.Forms.UserControl canvasAsUserControl = (m_GameCanvas as System.Windows.Forms.UserControl);
            canvasAsUserControl.Location = new System.Drawing.Point(279, 83);
            canvasAsUserControl.Name = "canvasCtrl";
            canvasAsUserControl.Size = new System.Drawing.Size(150, 150);
            canvasAsUserControl.TabIndex = 0;
            // 
            // RogueForm
            // 
            Logger.WriteLine(Logger.Stage.INIT_MAIN, "setup RogueForm");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(canvasAsUserControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RogueForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Rogue Survivor";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            Logger.WriteLine(Logger.Stage.INIT_MAIN, "ResumeLayout");
            this.ResumeLayout(false);

            Logger.WriteLine(Logger.Stage.INIT_MAIN, "InitializeComponent() done.");
        }

        #endregion

        private djack.RogueSurvivor.UI.IGameCanvas m_GameCanvas;




    }
}

