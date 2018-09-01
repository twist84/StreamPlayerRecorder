using System;
using System.Collections.Generic;
using System.Threading;

using NAudio.Lame;
using NAudio.Wave;

using static Types.Types;

namespace Constants
{
    internal class Constants
    {
        internal static Vector2D CommandPos;
        internal static Vector2D InputPos;
        internal static Vector2D HelpPos;
        internal static Vector2D InvalidPos;

        internal static bool IsDefaultStaion;

        internal static readonly int TickRate = 1000 * 1;

        internal static SongInfoStruct SongInfo;

        internal static Thread RecordStreamThread = null;

        internal static MediaFoundationReader Mp3Reader = null;
        internal static LameMP3FileWriter Mp3Writer = null;

        internal static List<FilterStruct> Filters;

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
