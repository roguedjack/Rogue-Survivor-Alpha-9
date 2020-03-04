using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX.AudioVideoPlayback;
using Microsoft.DirectX;

namespace djack.RogueSurvivor.Engine
{
    // alpha10 updated to IMusicManager
    class MDXSoundManager : IMusicManager
    {
        #region Fields
        bool m_IsMusicEnabled;
        int m_Volume;
        int m_Attenuation;
        Dictionary<string, Audio> m_Musics;
        // alpha10
        Audio m_CurrentAudio;
        #endregion

        #region Properties
        public bool IsMusicEnabled
        {
            get { return m_IsMusicEnabled; }
            set { m_IsMusicEnabled = value; }
        }
        public int Volume 
        {
            get { return m_Volume; }
            set 
            { 
                m_Volume = value;
                OnVolumeChange();
            }
        }
        // alpha10
        public string Music { get; private set; }
        public int Priority { get; private set; }

        public bool IsPlaying
        {
            get
            {
                return m_CurrentAudio != null && MdxIsPlaying(m_CurrentAudio);
            }
        }
    
        public bool HasEnded
        {
            get
            {
                return m_CurrentAudio != null && MdxHasEnded(m_CurrentAudio);
            }
        }
        #endregion

        #region Init
        public MDXSoundManager()
        {
            m_Musics = new Dictionary<string, Audio>();
            this.Volume = 100;
            this.Priority = MusicPriority.PRIORITY_NULL;
        }

        string FullName(string fileName)
        {
            return fileName + ".mp3";
        }
        #endregion

        #region Loading music
        public bool Load(string musicname, string filename)
        {
            filename = FullName(filename);
            Logger.WriteLine(Logger.Stage.INIT_SOUND, String.Format("loading music {0} file {1}", musicname, filename));
            try
            {
                Audio music = new Audio(filename);
                m_Musics.Add(musicname, music);
            }
            catch (Exception e)
            {
                Logger.WriteLine(Logger.Stage.INIT_SOUND, String.Format("failed to load music file {0} exception {1}.", filename, e.ToString()));
            }


            return true;
        }

        public void Unload(string musicname)
        {
            m_Musics.Remove(musicname);
        }
        #endregion

        #region Playing music

        private void OnVolumeChange()
        {
            m_Attenuation = ComputeDXAttenuationFromVolume();
            foreach (Audio a in m_Musics.Values)
                try
                {
                    a.Volume = -m_Attenuation; // yep mdx volume is negative and means attenuation instead of volume.
                }
                catch (DirectXException)
                {
                }
        }

        /**
         * MDX is retarded, "volume" audio property means attenuation instead and 0 is max volume and -10000 is zero db.
         * Go figure.
         */
        private int ComputeDXAttenuationFromVolume()
        {
            const int MIN_ATT = 10000;
            const int ATT_FACTOR = 2500; // should be min_att but it doesn't work. mdx is weird like that.
            if (m_Volume <= 0)
                return MIN_ATT;
            int att = ((100 - m_Volume) * ATT_FACTOR) / 100;
            return att;
        }

        /// <summary>
        /// Restart playing a music from the beginning if music is enabled.
        /// </summary>
        /// <param name="musicname"></param>
        public void Play(string musicname, int priority)
        {
            if (!m_IsMusicEnabled)
                return;

            Audio music;
            if (m_Musics.TryGetValue(musicname, out music))
            {
                Logger.WriteLine(Logger.Stage.RUN_SOUND, String.Format("playing music {0}.", musicname));
                MdxPlay(music);
                this.Music = musicname;
                this.Priority = priority;
            }
        }

#if false
        /// <summary>
        /// Start playing a music from the beginning if not already playing and if music is enabled.
        /// </summary>
        /// <param name="musicname"></param>
        public void PlayIfNotAlreadyPlaying(string musicname)
        {
            if (!m_IsMusicEnabled)
                return;

            Audio music;
            if (m_Musics.TryGetValue(musicname, out music))
            {
                if (!IsPlaying(music))
                    Play(music);
            }
        }
#endif

        /// <summary>
        /// Restart playing in a loop a music from the beginning if music is enabled.
        /// </summary>
        /// <param name="musicname"></param>
        public void PlayLooping(string musicname, int priority)
        {
            if (!m_IsMusicEnabled)
                return;

            Audio music;
            if (m_Musics.TryGetValue(musicname, out music))
            {
                Logger.WriteLine(Logger.Stage.RUN_SOUND, String.Format("playing looping music {0}.", musicname));
                music.Ending += new EventHandler(OnLoopingMusicEnding);
                MdxPlay(music);
                this.Music = musicname;
                this.Priority = priority;
            }
        }


#if false
        public void ResumeLooping(string musicname)
        {
            if (!m_IsMusicEnabled)
                return;

            Audio music;
            if (m_Musics.TryGetValue(musicname, out music))
            {
                Logger.WriteLine(Logger.Stage.RUN_SOUND, String.Format("resuming looping music {0}.", musicname));
                Resume(music);
            }
        }
#endif

        void OnLoopingMusicEnding(object sender, EventArgs e)
        {
            Audio music = (Audio)sender;
            MdxPlay(music);
        }

        public void Stop()
        {
            if (m_CurrentAudio != null)
                MdxStop(m_CurrentAudio);
            this.Music = "";
            this.Priority = MusicPriority.PRIORITY_NULL;
        }

#if false
        public void Stop(string musicname)
        {
            Audio music;
            if (m_Musics.TryGetValue(musicname, out music))
            {
                Logger.WriteLine(Logger.Stage.RUN_SOUND, String.Format("stopping music {0}.", musicname));
                Stop(music);
            }
        }

        public void StopAll()
        {
            Logger.WriteLine(Logger.Stage.RUN_SOUND, "stopping all musics.");
            foreach (Audio a in m_Musics.Values)
            {
                Stop(a);
            }
        }

        public bool IsPlaying(string musicname)
        {
            Audio music;
            if (m_Musics.TryGetValue(musicname, out music))
            {
                return IsPlaying(music);
            }
            else
                return false;
        }

        public bool IsPaused(string musicname)
        {
            Audio music;
            if (m_Musics.TryGetValue(musicname, out music))
            {
                return IsPaused(music);
            }
            else
                return false;
        }

        public bool HasEnded(string musicname)
        {
            Audio music;
            if (m_Musics.TryGetValue(musicname, out music))
            {
                return HasEnded(music);
            }
            else
                return false;
        }
#endif

        void MdxStop(Audio audio)
        {
            audio.Ending -= OnLoopingMusicEnding;
            //audio.Pause();
            audio.Stop();  // alpha10.1
            m_CurrentAudio = null;
        }

        void MdxPlay(Audio audio)
        {
            audio.Stop();
            audio.SeekCurrentPosition(0, SeekPositionFlags.AbsolutePositioning);
            audio.Volume = -m_Attenuation;
            audio.Play();
            m_CurrentAudio = audio;
        }

#if false
        void Resume(Audio audio)
        {
            audio.Play();
        }
#endif

        bool MdxIsPlaying(Audio audio)
        {
            return audio.CurrentPosition > 0 && audio.CurrentPosition < audio.Duration && audio.State == StateFlags.Running;
        }

#if false
        bool IsPaused(Audio audio)
        {
            return (audio.State & StateFlags.Paused) != 0;
        }
#endif

        bool MdxHasEnded(Audio audio)
        {
            return audio.CurrentPosition >= audio.Duration;
        }
#endregion

#region IDisposable
        public void Dispose()
        {
            Stop(); // alpha10

            Logger.WriteLine(Logger.Stage.CLEAN_SOUND, "disposing MDXMusicManager...");
            foreach (string musicname in m_Musics.Keys)
            {
                Audio music = m_Musics[musicname];
                if(music==null)
                {
                    Logger.WriteLine(Logger.Stage.CLEAN_SOUND, String.Format("WARNING: null music for key {0}", musicname));
                    continue;
                }
                Logger.WriteLine(Logger.Stage.CLEAN_SOUND, String.Format("disposing music {0}.", musicname));
                music.Stop(); // alpha10
                music.Dispose();
            }

            m_Musics.Clear();
            Logger.WriteLine(Logger.Stage.CLEAN_SOUND, "disposing MDXMusicManager done.");
        }
#endregion
    }
}
