using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using djack.RogueSurvivor;

namespace Setup
{
    public partial class ConfigForm : Form
    {
        public ConfigForm()
        {
            InitializeComponent();

            this.l_GameVersion.Text = "Game version : " + SetupConfig.GAME_VERSION;
        }

        private void b_SaveExit_Click(object sender, EventArgs e)
        {
            SetupConfig.Save();
            this.Close();
        }

        private void b_Exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SetupForm_Load(object sender, EventArgs e)
        {
            SetupConfig.Load();

            switch (SetupConfig.Video)
            {
                case SetupConfig.eVideo.VIDEO_GDI_PLUS: rb_Video_GDI.Checked = true; break;
                case SetupConfig.eVideo.VIDEO_MANAGED_DIRECTX: rb_Video_MDX.Checked = true; break;
            }
            switch (SetupConfig.Sound)
            {
                case SetupConfig.eSound.SOUND_MANAGED_DIRECTX: rb_Sound_MDX.Checked = true; break;
                case SetupConfig.eSound.SOUND_SFML: rb_Sound_SFML.Checked = true; break;
                case SetupConfig.eSound.SOUND_NOSOUND: rb_Sound_NoSound.Checked = true; break;
            }
        }

        private void rb_Video_MDX_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_Video_MDX.Checked && SetupConfig.Video != SetupConfig.eVideo.VIDEO_MANAGED_DIRECTX)
                SetupConfig.Video = SetupConfig.eVideo.VIDEO_MANAGED_DIRECTX;
        }

        private void rb_Video_GDI_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_Video_GDI.Checked && SetupConfig.Video != SetupConfig.eVideo.VIDEO_GDI_PLUS)
                SetupConfig.Video = SetupConfig.eVideo.VIDEO_GDI_PLUS;
        }

        private void rb_Sound_MDX_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_Sound_MDX.Checked && SetupConfig.Sound != SetupConfig.eSound.SOUND_MANAGED_DIRECTX)
                SetupConfig.Sound = SetupConfig.eSound.SOUND_MANAGED_DIRECTX;
        }

        private void rb_Audio_SFML_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_Sound_SFML.Checked && SetupConfig.Sound != SetupConfig.eSound.SOUND_SFML)
                SetupConfig.Sound = SetupConfig.eSound.SOUND_SFML;
        }

        private void rb_Sound_NoSound_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_Sound_NoSound.Checked && SetupConfig.Sound != SetupConfig.eSound.SOUND_NOSOUND)
                SetupConfig.Sound = SetupConfig.eSound.SOUND_NOSOUND;
        }
    }
}
