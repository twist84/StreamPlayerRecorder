using System.Collections.Generic;

using NAudio.Lame;
using NAudio.Wave;

using static Types.Types;

namespace Constants
{
    internal class Constants
    {
        internal static SongInfoStruct SongInfo;
        internal static readonly int TickRate = 1000 * 1;

        internal static System.Threading.Thread RecordStreamThread = null;

        internal static bool IsDefaultStaion;


        internal static List<FilterStruct> Filters = new List<FilterStruct>() {
            new FilterStruct { StartsWith = "roderick", Contains = "carter" },
            new FilterStruct { StartsWith = "metal", Contains = "radio" },
            new FilterStruct { StartsWith = "id", Contains = "psa" },
        };

        internal static MediaFoundationReader Mp3Reader = null;
        internal static LameMP3FileWriter Mp3Writer = null;


        internal static string[] Commands = {
                "play, pause, p",
                "start, stop, s",

                "volume up, vu",
                "volume down, vd",
                "mute, unmute, m",

                "open, o",
                "quit, q"
            };
    }
}
