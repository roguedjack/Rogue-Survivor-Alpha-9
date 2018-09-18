using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace djack.RogueSurvivor.Engine
{
    // alpha10 Added concept of music priority, can play only one music at a time, renamed to MusicManager and
    // some cleanup. Concrete classes updated.

    static class MusicPriority
    {
        /// <summary>
        /// Lowest priority when not playing any music.
        /// </summary>
        public const int PRIORITY_NULL = 0;  // must be 0!

        /// <summary>
        /// Medium priority for background musics.
        /// </summary>
        public const int PRIORITY_BGM = 1;

        /// <summary>
        /// High priority for events musics.
        /// </summary>
        public const int PRIORITY_EVENT = 2;
    }

    interface IMusicManager : IDisposable
    {
        #region Properties
        bool IsMusicEnabled { get; set; }
        int Volume { get; set; }
        // alpha10
        int Priority { get; }
        string Music { get; }
        bool IsPlaying { get; }
        bool HasEnded { get; }
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
        void Play(string musicname, int priority);

        /// <summary>
        /// Start playing a music from the beginning if not already playing and if music is enabled.
        /// </summary>
        /// <param name="musicname"></param>
        //void PlayIfNotAlreadyPlaying(string musicname, int priority);

        /// <summary>
        /// Restart playing in a loop a music from the beginning if music is enabled.
        /// </summary>
        /// <param name="musicname"></param>
        void PlayLooping(string musicname, int priority);

        //void ResumeLooping(string musicname, int priority);

        // alpha10
        void Stop();

        //void Stop(string musicname);

        //void StopAll();

        //bool IsPlaying(string musicname);

        //bool IsPaused(string musicname);

        //bool HasEnded(string musicname);
        #endregion
    }
}
