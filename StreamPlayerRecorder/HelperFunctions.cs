using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

using NAudio.Lame;
using NAudio.Wave;

using static Constants.Constants;
using static Types.Types;

namespace Helper
{
    internal static class Functions
    {
        internal static void UpdateElapsed()
        {
            SongInfo.Time = TimeSpan.FromSeconds(SongInfo.Elapsed.Value);
            SongInfo.Elapsed.Text = $"{SongInfo.Time.Minutes:00}:{SongInfo.Time.Seconds:00}";
            if (SongInfo.Time.Hours > 0)
                SongInfo.Elapsed.Text = $"{SongInfo.Time.Hours: 00}:" + SongInfo.Elapsed.Text;

            Thread.Sleep(TickRate);
            SongInfo.Elapsed.Value++;
        }

        internal static ID3TagData GetSong(WebClient client, string Url)
        {
            client.Encoding = System.Text.Encoding.ASCII;
            string[] split = Regex.Split(client.DownloadString(Url), " - ");

            if (!FilterByList(split[0]))
                return new ID3TagData { Artist = split[0], Title = split[1] };

            return SongInfo.CurrentSong;
        }

        internal static bool FilterByList(string StringToCheck, bool ret = false)
        {
            foreach (var Filter in Filters)
                if (StringToCheck.ToLower().StartsWith(Filter.StartsWith) && StringToCheck.ToLower().Contains(Filter.Contains))
                    ret = true;

            return ret;
        }

        internal static void PlayCheck(WaveOutEvent wo)
        {
            switch (wo.PlaybackState)
            {
                case PlaybackState.Playing:
                    if (SongInfo.PlaybackState == PlaybackState.Paused)
                        wo.Pause();
                    break;
                case PlaybackState.Paused:
                    if (SongInfo.PlaybackState == PlaybackState.Playing)
                        wo.Play();
                    break;
            }
        }

        internal static string CleanFileName(string fileName) => Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));

        internal static void Empty(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);

            foreach (FileInfo file in directory.GetFiles())
                file.Delete();
            foreach (DirectoryInfo subDirectory in directory.GetDirectories())
                subDirectory.Delete(true);
        }

        internal static void UpdateTitleText(string Line)
        {
            Console.Title = Line;
        }

        internal static void FillLine(int LineNum, string FillChar)
        {
            string temp = FillChar;
            Console.SetCursorPosition(0, Console.WindowTop + LineNum);
            for (int i = 0; i < Console.BufferWidth; i++)
                FillChar += temp;
            Console.Write(FillChar);
            Console.SetCursorPosition(0, Console.WindowTop + LineNum);
        }

        internal static void UpdateVolume(WaveOutEvent wo)
        {
            wo.Volume = SongInfo.Volume.Muted ? 0.01f * 0 : 0.01f * SongInfo.Volume.Value;
            SongInfo.Volume.Text = SongInfo.Volume.Muted ? "Muted" : $"{SongInfo.Volume.Value:00}%";

            Thread.Sleep(TickRate);
        }

        internal static Vector2D ToVector2D(int x, int y) => new Vector2D { X = x, Y = y };
        internal static FilterStruct ToFilter(string starts_with, string contains) => new FilterStruct { StartsWith = starts_with, Contains = contains };
    }
}
