namespace Setup
{
    partial class ConfigForm
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
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.b_SaveExit = new System.Windows.Forms.Button();
            this.gb_Video = new System.Windows.Forms.GroupBox();
            this.rb_Video_GDI = new System.Windows.Forms.RadioButton();
            this.rb_Video_MDX = new System.Windows.Forms.RadioButton();
            this.b_Exit = new System.Windows.Forms.Button();
            this.gb_Sound = new System.Windows.Forms.GroupBox();
            this.rb_Sound_NoSound = new System.Windows.Forms.RadioButton();
            this.rb_Sound_SFML = new System.Windows.Forms.RadioButton();
            this.rb_Sound_MDX = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.l_GameVersion = new System.Windows.Forms.Label();
            this.gb_Video.SuspendLayout();
            this.gb_Sound.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // b_SaveExit
            // 
            this.b_SaveExit.Location = new System.Drawing.Point(5, 111);
            this.b_SaveExit.Name = "b_SaveExit";
            this.b_SaveExit.Size = new System.Drawing.Size(122, 23);
            this.b_SaveExit.TabIndex = 2;
            this.b_SaveExit.Text = "Save and Exit";
            this.b_SaveExit.UseVisualStyleBackColor = true;
            this.b_SaveExit.Click += new System.EventHandler(this.b_SaveExit_Click);
            // 
            // gb_Video
            // 
            this.gb_Video.AutoSize = true;
            this.gb_Video.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gb_Video.Controls.Add(this.rb_Video_GDI);
            this.gb_Video.Controls.Add(this.rb_Video_MDX);
            this.gb_Video.Location = new System.Drawing.Point(6, 3);
            this.gb_Video.Name = "gb_Video";
            this.gb_Video.Size = new System.Drawing.Size(121, 80);
            this.gb_Video.TabIndex = 0;
            this.gb_Video.TabStop = false;
            this.gb_Video.Text = "Video";
            // 
            // rb_Video_GDI
            // 
            this.rb_Video_GDI.AutoSize = true;
            this.rb_Video_GDI.Location = new System.Drawing.Point(7, 44);
            this.rb_Video_GDI.Name = "rb_Video_GDI";
            this.rb_Video_GDI.Size = new System.Drawing.Size(50, 17);
            this.rb_Video_GDI.TabIndex = 1;
            this.rb_Video_GDI.TabStop = true;
            this.rb_Video_GDI.Text = "GDI+";
            this.rb_Video_GDI.UseVisualStyleBackColor = true;
            this.rb_Video_GDI.CheckedChanged += new System.EventHandler(this.rb_Video_GDI_CheckedChanged);
            // 
            // rb_Video_MDX
            // 
            this.rb_Video_MDX.AutoSize = true;
            this.rb_Video_MDX.Checked = true;
            this.rb_Video_MDX.Location = new System.Drawing.Point(7, 20);
            this.rb_Video_MDX.Name = "rb_Video_MDX";
            this.rb_Video_MDX.Size = new System.Drawing.Size(108, 17);
            this.rb_Video_MDX.TabIndex = 0;
            this.rb_Video_MDX.TabStop = true;
            this.rb_Video_MDX.Text = "Managed DirectX";
            this.rb_Video_MDX.UseVisualStyleBackColor = true;
            this.rb_Video_MDX.CheckedChanged += new System.EventHandler(this.rb_Video_MDX_CheckedChanged);
            // 
            // b_Exit
            // 
            this.b_Exit.Location = new System.Drawing.Point(131, 111);
            this.b_Exit.Name = "b_Exit";
            this.b_Exit.Size = new System.Drawing.Size(121, 23);
            this.b_Exit.TabIndex = 3;
            this.b_Exit.Text = "Exit";
            this.b_Exit.UseVisualStyleBackColor = true;
            this.b_Exit.Click += new System.EventHandler(this.b_Exit_Click);
            // 
            // gb_Sound
            // 
            this.gb_Sound.AutoSize = true;
            this.gb_Sound.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gb_Sound.Controls.Add(this.rb_Sound_NoSound);
            this.gb_Sound.Controls.Add(this.rb_Sound_SFML);
            this.gb_Sound.Controls.Add(this.rb_Sound_MDX);
            this.gb_Sound.Location = new System.Drawing.Point(134, 3);
            this.gb_Sound.Name = "gb_Sound";
            this.gb_Sound.Size = new System.Drawing.Size(121, 102);
            this.gb_Sound.TabIndex = 1;
            this.gb_Sound.TabStop = false;
            this.gb_Sound.Text = "Sound";
            // 
            // rb_Sound_NoSound
            // 
            this.rb_Sound_NoSound.AutoSize = true;
            this.rb_Sound_NoSound.Location = new System.Drawing.Point(6, 66);
            this.rb_Sound_NoSound.Name = "rb_Sound_NoSound";
            this.rb_Sound_NoSound.Size = new System.Drawing.Size(71, 17);
            this.rb_Sound_NoSound.TabIndex = 2;
            this.rb_Sound_NoSound.TabStop = true;
            this.rb_Sound_NoSound.Text = "No sound";
            this.rb_Sound_NoSound.UseVisualStyleBackColor = true;
            this.rb_Sound_NoSound.CheckedChanged += new System.EventHandler(this.rb_Sound_NoSound_CheckedChanged);
            // 
            // rb_Sound_SFML
            // 
            this.rb_Sound_SFML.AutoSize = true;
            this.rb_Sound_SFML.Location = new System.Drawing.Point(7, 43);
            this.rb_Sound_SFML.Name = "rb_Sound_SFML";
            this.rb_Sound_SFML.Size = new System.Drawing.Size(71, 17);
            this.rb_Sound_SFML.TabIndex = 1;
            this.rb_Sound_SFML.TabStop = true;
            this.rb_Sound_SFML.Text = "SFML 1.6";
            this.rb_Sound_SFML.UseVisualStyleBackColor = true;
            this.rb_Sound_SFML.CheckedChanged += new System.EventHandler(this.rb_Audio_SFML_CheckedChanged);
            // 
            // rb_Sound_MDX
            // 
            this.rb_Sound_MDX.AutoSize = true;
            this.rb_Sound_MDX.Checked = true;
            this.rb_Sound_MDX.Location = new System.Drawing.Point(7, 20);
            this.rb_Sound_MDX.Name = "rb_Sound_MDX";
            this.rb_Sound_MDX.Size = new System.Drawing.Size(108, 17);
            this.rb_Sound_MDX.TabIndex = 0;
            this.rb_Sound_MDX.TabStop = true;
            this.rb_Sound_MDX.Text = "Managed DirectX";
            this.rb_Sound_MDX.UseVisualStyleBackColor = true;
            this.rb_Sound_MDX.CheckedChanged += new System.EventHandler(this.rb_Sound_MDX_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Controls.Add(this.gb_Sound);
            this.panel1.Controls.Add(this.b_Exit);
            this.panel1.Controls.Add(this.gb_Video);
            this.panel1.Controls.Add(this.b_SaveExit);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 42);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(264, 137);
            this.panel1.TabIndex = 4;
            // 
            // l_GameVersion
            // 
            this.l_GameVersion.AutoSize = true;
            this.l_GameVersion.Dock = System.Windows.Forms.DockStyle.Top;
            this.l_GameVersion.Location = new System.Drawing.Point(0, 0);
            this.l_GameVersion.Name = "l_GameVersion";
            this.l_GameVersion.Size = new System.Drawing.Size(106, 13);
            this.l_GameVersion.TabIndex = 5;
            this.l_GameVersion.Text = "<game version here>";
            // 
            // ConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(264, 179);
            this.Controls.Add(this.l_GameVersion);
            this.Controls.Add(this.panel1);
            this.Name = "ConfigForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Rogue Survivor Config";
            this.Load += new System.EventHandler(this.SetupForm_Load);
            this.gb_Video.ResumeLayout(false);
            this.gb_Video.PerformLayout();
            this.gb_Sound.ResumeLayout(false);
            this.gb_Sound.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button b_SaveExit;
        private System.Windows.Forms.GroupBox gb_Video;
        private System.Windows.Forms.RadioButton rb_Video_GDI;
        private System.Windows.Forms.RadioButton rb_Video_MDX;
        private System.Windows.Forms.Button b_Exit;
        private System.Windows.Forms.GroupBox gb_Sound;
        private System.Windows.Forms.RadioButton rb_Sound_MDX;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label l_GameVersion;
        private System.Windows.Forms.RadioButton rb_Sound_SFML;
        private System.Windows.Forms.RadioButton rb_Sound_NoSound;


    }
}

