using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace djack.RogueSurvivor.Engine
{
    interface ISoundManager : IDisposable
    {
        #region Properties
        bool IsMusicEnabled { get; set; }
        int Volume { get; set; }
        #endregion


        #region Loading music
        bool Load(string musicname, string filename);

        void Unload(string musicname);
        #endregion

        #region Playing music
        /// <summary>
        /// Restart playing a music from the beginning if music is enabled.
        /// </summary>
        /// <param name="musicname"></param>
        void Play(string musicname);

        /// <summary>
        /// Start playing a music from the beginning if not already playing and if music is enabled.
        /// </summary>
        /// <param name="musicname"></param>
        void PlayIfNotAlreadyPlaying(string musicname);

        /// <summary>
        /// Restart playing in a loop a music from the beginning if music is enabled.
        /// </summary>
        /// <param name="musicname"></param>
        void PlayLooping(string musicname);

        void ResumeLooping(string musicname);

        void Stop(string musicname);

        void StopAll();

        bool IsPlaying(string musicname);

        bool IsPaused(string musicname);

        bool HasEnded(string musicname);
        #endregion
    }
}
