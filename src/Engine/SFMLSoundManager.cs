using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SFML.Audio;

using SFMLMusic = SFML.Audio.Music;

namespace djack.RogueSurvivor.Engine
{
    class SFMLSoundManager : ISoundManager
    {
        #region Fields
        bool m_IsMusicEnabled;
        int m_Volume;
        Dictionary<string, SFMLMusic> m_Musics;
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
        #endregion

        #region Init
        public SFMLSoundManager()
        {
            m_Musics = new Dictionary<string, SFMLMusic>();
            m_Volume = 100;
        }

        string FullName(string fileName)
        {
            return fileName + ".ogg";
        }
        #endregion

        #region Loading music
        public bool Load(string musicname, string filename)
        {
            filename = FullName(filename);
            Logger.WriteLine(Logger.Stage.INIT_SOUND, String.Format("loading music {0} file {1}", musicname, filename));
            try
            {
                SFMLMusic music = new SFMLMusic(filename);
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
            foreach (SFMLMusic a in m_Musics.Values)
                a.Volume = m_Volume;
        }

        /// <summary>
        /// Restart playing a music from the beginning if music is enabled.
        /// </summary>
        /// <param name="musicname"></param>
        public void Play(string musicname)
        {
            if (!m_IsMusicEnabled)
                return;

            SFMLMusic music;
            if (m_Musics.TryGetValue(musicname, out music))
            {
                Logger.WriteLine(Logger.Stage.RUN_SOUND, String.Format("playing music {0}.", musicname));
                Play(music);
            }
        }

        /// <summary>
        /// Start playing a music from the beginning if not already playing and if music is enabled.
        /// </summary>
        /// <param name="musicname"></param>
        public void PlayIfNotAlreadyPlaying(string musicname)
        {
            if (!m_IsMusicEnabled)
                return;

            SFMLMusic music;
            if (m_Musics.TryGetValue(musicname, out music))
            {
                if (!IsPlaying(music))
                    Play(music);
            }
        }

        /// <summary>
        /// Restart playing in a loop a music from the beginning if music is enabled.
        /// </summary>
        /// <param name="musicname"></param>
        public void PlayLooping(string musicname)
        {
            if (!m_IsMusicEnabled)
                return;

            SFMLMusic music;
            if (m_Musics.TryGetValue(musicname, out music))
            {
                Logger.WriteLine(Logger.Stage.RUN_SOUND, String.Format("playing looping music {0}.", musicname));
                music.Loop = true;
                Play(music);
            }
        }

        public void ResumeLooping(string musicname)
        {
            if (!m_IsMusicEnabled)
                return;

            SFMLMusic music;
            if (m_Musics.TryGetValue(musicname, out music))
            {
                Logger.WriteLine(Logger.Stage.RUN_SOUND, String.Format("resuming looping music {0}.", musicname));
                Resume(music);
            }
        }

        public void Stop(string musicname)
        {
            SFMLMusic music;
            if (m_Musics.TryGetValue(musicname, out music))
            {
                Logger.WriteLine(Logger.Stage.RUN_SOUND, String.Format("stopping music {0}.", musicname));
                Stop(music);
            }
        }

        public void StopAll()
        {
            Logger.WriteLine(Logger.Stage.RUN_SOUND, "stopping all musics.");
            foreach (SFMLMusic a in m_Musics.Values)
            {
                Stop(a);
            }
        }

        public bool IsPlaying(string musicname)
        {
            SFMLMusic music;
            if (m_Musics.TryGetValue(musicname, out music))
            {
                return IsPlaying(music);
            }
            else
                return false;
        }

        public bool IsPaused(string musicname)
        {
            SFMLMusic music;
            if (m_Musics.TryGetValue(musicname, out music))
            {
                return IsPaused(music);
            }
            else
                return false;
        }

        public bool HasEnded(string musicname)
        {
            SFMLMusic music;
            if (m_Musics.TryGetValue(musicname, out music))
            {
                return HasEnded(music);
            }
            else
                return false;
        }

        void Stop(SFMLMusic audio)
        {
            audio.Stop();
        }

        void Play(SFMLMusic audio)
        {
            audio.Stop();
            audio.Volume = m_Volume;
            audio.Play();
        }

        void Resume(SFMLMusic audio)
        {
            audio.Play();
        }

        bool IsPlaying(SFMLMusic audio)
        {
            return audio.Status == SoundStatus.Playing;
        }

        bool IsPaused(SFMLMusic audio)
        {
            return audio.Status == SoundStatus.Paused;
        }

        bool HasEnded(SFMLMusic audio)
        {
            return audio.Status == SoundStatus.Stopped || audio.PlayingOffset >= audio.Duration;
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            Logger.WriteLine(Logger.Stage.CLEAN_SOUND, "disposing SFMLMusicManager...");
            foreach (string musicname in m_Musics.Keys)
            {
                SFMLMusic music = m_Musics[musicname];
                if(music==null)
                {
                    Logger.WriteLine(Logger.Stage.CLEAN_SOUND, String.Format("WARNING: null music for key {0}", musicname));
                    continue;
                }
                Logger.WriteLine(Logger.Stage.CLEAN_SOUND, String.Format("disposing music {0}.", musicname));
                music.Dispose();
            }

            m_Musics.Clear();
            Logger.WriteLine(Logger.Stage.CLEAN_SOUND, "disposing SFMLMusicManager done.");
        }
        #endregion
    }
}
