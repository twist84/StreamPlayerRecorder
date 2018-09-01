using System.IO;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading;

using NAudio.Lame;
using NAudio.Wave;

using static Constants.Constants;
using static ExtensionMethods.StringEx;
using static Helper.Functions;

namespace Threads
{
    class Threads
    {
        internal static void UpdateSongInfo()
        {
            if (IsDefaultStaion)
            {
                ID3TagData TempID3 = new ID3TagData();

                using (WebClient client = new WebClient())
                {
                    SongInfo.CurrentSong = GetSong(client, SongInfo.RadioStation.Endpoints.Song);
                    while (true)
                    {
                        TempID3 = GetSong(client, SongInfo.RadioStation.Endpoints.Song);
                        if (SongInfo.CurrentSong.Artist != TempID3.Artist && SongInfo.CurrentSong.Title != TempID3.Title)
                        {
                            if (!SongInfo.IsContinuous)
                            {
                                if (RecordStreamThread.ThreadState == ThreadState.Running)
                                    RecordStreamThread.Abort();

                                Thread.Sleep(1000 * SongInfo.StreamDelay);

                                SongInfo.CurrentSong = TempID3;
                                SongInfo.Elapsed.Value = 0;

                                if (RecordStreamThread.ThreadState == ThreadState.Aborted)
                                {
                                    if (Mp3Reader != null) Mp3Reader = null;
                                    if (Mp3Writer != null) Mp3Writer = null;

                                    RecordStreamThread = new Thread(RecordStream);
                                    RecordStreamThread.Start();
                                }
                            }
                        }
                        UpdateElapsed();
                    }
                }
            }
            else
            {
                while (true)
                    UpdateElapsed();
            }
        }

        internal static void RecordStream()
        {
            if (!SongInfo.IsContinuous && !Directory.Exists($".\\{SongInfo.RadioStation.Name}\\")) Directory.CreateDirectory($".\\{Regex.Replace(SongInfo.RadioStation.Name, "\\/", "-")}\\");

            string Mp3Name = $"{Regex.Replace(SongInfo.CurrentSong.Artist, "\\/", "-")} - {Regex.Replace(SongInfo.CurrentSong.Title, "\\/", "-")}";
            string Mp3Path = $"{TrimInvalidChars(SongInfo.RadioStation.Name)}{(SongInfo.IsContinuous ? $"{string.Empty}" : $"\\{TrimInvalidChars(Mp3Name)}")}";

            using (Mp3Reader = new MediaFoundationReader(SongInfo.RadioStation.Endpoints.Stream))
            using (Mp3Writer = new LameMP3FileWriter($".\\{Mp3Path}.mp3", Mp3Reader.WaveFormat, SongInfo.Bitrate, SongInfo.CurrentSong))
                Mp3Reader.CopyTo(Mp3Writer);
        }

        internal static void PlayStream()
        {
            using (var mf = new MediaFoundationReader(SongInfo.RadioStation.Endpoints.Stream))
            using (var wo = new WaveOutEvent())
            {
                wo.Init(mf);
                wo.Volume = 0.01f * SongInfo.Volume.Value;

                SongInfo.PlaybackState = PlaybackState.Playing;
                wo.Play();

                while (true)
                {
                    PlayCheck(wo);
                    UpdateVolume(wo);
                }
            }
        }
    }
}
