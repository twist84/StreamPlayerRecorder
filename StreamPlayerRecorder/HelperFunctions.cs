using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

using NAudio.Lame;
using NAudio.Wave;

using static Constants.Constants;

namespace Helper
{
    internal static class Functions
    {
        internal static void UpdateElapsed()
        {
            SongInfo.Time = TimeSpan.FromSeconds(SongInfo.Elapsed.Time);
            SongInfo.Elapsed.Text = $"{SongInfo.Time.Minutes:00}:{SongInfo.Time.Seconds:00}";
            if (SongInfo.Time.Hours > 0)
                SongInfo.Elapsed.Text = $"{SongInfo.Time.Hours: 00}:" + SongInfo.Elapsed.Text;

            Thread.Sleep(TickRate);
            SongInfo.Elapsed.Time++;
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

    }
}
