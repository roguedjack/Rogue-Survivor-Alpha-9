using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace djack.RogueSurvivor
{
    public static class SetupConfig
    {
        public const string GAME_VERSION = "alpha 10.1";

        public enum eVideo
        {
            VIDEO_INVALID,
            VIDEO_MANAGED_DIRECTX,
            VIDEO_GDI_PLUS,
            _COUNT
        }

        public enum eSound
        {
            SOUND_INVALID,
            SOUND_MANAGED_DIRECTX,
            SOUND_SFML,
            SOUND_NOSOUND,
            _COUNT
        }

        public static eVideo Video { get; set; }
        public static eSound Sound { get; set; }

        public static string DirPath
        {
            get
            {
                return Environment.CurrentDirectory + @"\Config\";
            }
        }

        static string FilePath
        {
            get
            {
                return DirPath + @"\setup.dat";
            }
        }

        public static void Save()
        {
            using (StreamWriter sw = File.CreateText(FilePath))
            {
                sw.WriteLine(toString(SetupConfig.Video));
                sw.WriteLine(toString(SetupConfig.Sound));
            }
        }

        public static void Load()
        {
            if (File.Exists(FilePath))
            {
                using (StreamReader sr = File.OpenText(FilePath))
                {
                    SetupConfig.Video = toVideo(sr.ReadLine());
                    SetupConfig.Sound = toSound(sr.ReadLine());
                }
            }
            else
            {
                if (!Directory.Exists(DirPath))
                    Directory.CreateDirectory(DirPath);

                SetupConfig.Video = eVideo.VIDEO_MANAGED_DIRECTX;
                SetupConfig.Sound = eSound.SOUND_MANAGED_DIRECTX;

                Save();
            }
        }

        public static string toString(eVideo v)
        {
            return v.ToString();
        }

        public static string toString(eSound s)
        {
            return s.ToString();
        }

        public static eVideo toVideo(string s)
        {
            if (s == eVideo.VIDEO_MANAGED_DIRECTX.ToString())
                return eVideo.VIDEO_MANAGED_DIRECTX;
            if (s == eVideo.VIDEO_GDI_PLUS.ToString())
                return eVideo.VIDEO_GDI_PLUS;
            return eVideo.VIDEO_INVALID;
        }

        public static eSound toSound(string s)
        {
            if (s == eSound.SOUND_MANAGED_DIRECTX.ToString())
                return eSound.SOUND_MANAGED_DIRECTX;
            if (s == eSound.SOUND_SFML.ToString())
                return eSound.SOUND_SFML;
            if (s == eSound.SOUND_NOSOUND.ToString())
                return eSound.SOUND_NOSOUND;
            return eSound.SOUND_INVALID;
        }
    }
}
