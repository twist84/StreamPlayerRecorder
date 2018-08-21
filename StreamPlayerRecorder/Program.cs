using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

using NAudio.Lame;
using NAudio.Wave;

namespace StreamPlayerRecorder
{
    class Program
    {
        internal static readonly int TickRate = 1000 * 1;

        internal static Thread RecordStreamThread = null;

        static void Main(string[] args)
        {
            if (!SetupSongInfo()) return;

            new Thread(() => UpdateSongInfo()).Start();
            Thread.Sleep(TickRate);
            new Thread(() => PlayStream(20)).Start();
            RecordStreamThread = new Thread(RecordStream);
            RecordStreamThread.Start();

            while (true)
            {
                SongInfo.Time = TimeSpan.FromSeconds(SongInfo.Elapsed);
                string elapsed = $"{SongInfo.Time.Minutes:00}:{SongInfo.Time.Seconds:00}";
                if (SongInfo.Time.Hours > 0)
                    elapsed = $"{SongInfo.Time.Hours: 00}:" + elapsed;

                Console.Clear();
                Console.WriteLine($"Elapsed: {elapsed}\t\tVolume: {SongInfo.Volume:00}%");
                Console.WriteLine($"Artist: {SongInfo.CurrentSong.Artist}, Title: {SongInfo.CurrentSong.Title}");
                Console.Title = $"{SongInfo.CurrentSong.Artist}, {SongInfo.CurrentSong.Title}, {elapsed}, {SongInfo.Volume:00}%";

                Thread.Sleep(TickRate);
                SongInfo.Elapsed++;
            }
        }

        internal static bool MetalRockRadio;

        internal static bool SetupSongInfo()
        {
            MetalRockRadio = false; // Change this to false if you want to set another station
            if (!MetalRockRadio)
            {
                SongInfo.IsIndividial = false; // Needs to be false
                SongInfo.RadioStation.Name = "Brian FM Ashburton"; // Change this to the station name
                SongInfo.RadioStation.Endpoints.Stream = "http://5.135.154.72:10106"; // Change this to the station stream
                SongInfo.CurrentSong = new ID3TagData { Artist = "Unknown", Title = "Unknown" }; // Can't get the song info because this app is only meant for Metal Rock Radio by Sonixcast
            }
            else
            {
                SongInfo.RadioStation.Name = "Metal Rock Radio by Sonixcast";
                SongInfo.RadioStation.Url = "http://cabhs30.sonixcast.com:9964";
                SongInfo.RadioStation.Endpoints.Stream = $"{SongInfo.RadioStation.Url}/stream";
                SongInfo.RadioStation.Endpoints.Song = $"{SongInfo.RadioStation.Url}/currentsong";

                SongInfo.Elapsed = 0;
                SongInfo.Bitrate = 192;
                //SongInfo.Volume = 50;

                SongInfo.StreamDelay = 18;
                SongInfo.IsIndividial = true;
            }

            return true;
        }

        internal static void UpdateSongInfo()
        {
            if (!MetalRockRadio)
                return;

            ID3TagData TempID3 = new ID3TagData();

            using (WebClient client = new WebClient())
            {
                SongInfo.CurrentSong = GetSong(client, SongInfo.RadioStation.Endpoints.Song);
                while (true)
                {
                    TempID3 = GetSong(client, SongInfo.RadioStation.Endpoints.Song);
                    if (SongInfo.CurrentSong.Artist != TempID3.Artist && SongInfo.CurrentSong.Title != TempID3.Title)
                    {
                        if (SongInfo.IsIndividial)
                        {
                            if (RecordStreamThread.ThreadState == ThreadState.Running)
                                RecordStreamThread.Abort();

                                Thread.Sleep(1000 * SongInfo.StreamDelay);

                            SongInfo.CurrentSong = TempID3;
                            SongInfo.Elapsed = 0;

                            if (RecordStreamThread.ThreadState == ThreadState.Aborted)
                            {
                                if (Mp3Reader != null) Mp3Reader = null;
                                if (Mp3Writer != null) Mp3Writer = null;

                                RecordStreamThread = new Thread(RecordStream);
                                RecordStreamThread.Start();
                            }
                        }
                    }

                    Thread.Sleep(TickRate);
                }
            }
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
                if (StringToCheck.StartsWith(Filter.StartsWith) && StringToCheck.Contains(Filter.Contains))
                    ret = true;

            return ret;
        }

        internal static List<FilterStruct> Filters = new List<FilterStruct>() {
            new FilterStruct { StartsWith = "Roderick", Contains = "Carter" },
            new FilterStruct { StartsWith = "Metal", Contains = "Radio" },
            new FilterStruct { StartsWith = "METAL", Contains = "RADIO" },
            new FilterStruct { StartsWith = "ID", Contains = "PSA" },
        };

        internal struct FilterStruct
        {
            internal string StartsWith;
            internal string Contains;

        }

        internal static void RecordStream()
        {
            if (SongInfo.IsIndividial && !Directory.Exists($".\\{SongInfo.RadioStation.Name}\\")) Directory.CreateDirectory($".\\{Regex.Replace(SongInfo.RadioStation.Name, "\\/", "-")}\\");

            string Mp3FilePath = $"{CleanFileName(SongInfo.RadioStation.Name)}";
            if (SongInfo.IsIndividial) Mp3FilePath += Path.DirectorySeparatorChar + CleanFileName($"{Regex.Replace(SongInfo.CurrentSong.Artist, "\\/", "-")} - {Regex.Replace(SongInfo.CurrentSong.Title, "\\/", "-")}");

            using (Mp3Reader = new MediaFoundationReader(SongInfo.RadioStation.Endpoints.Stream))
            using (Mp3Writer = new LameMP3FileWriter($".\\{Mp3FilePath}.mp3", Mp3Reader.WaveFormat, SongInfo.Bitrate, SongInfo.CurrentSong))
                Mp3Reader.CopyTo(Mp3Writer);
        }

        internal static MediaFoundationReader Mp3Reader = null;
        internal static LameMP3FileWriter Mp3Writer = null;

        internal static string CleanFileName(string fileName) => Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));

        internal static void PlayStream(int InitialVolume)
        {
            SongInfo.Volume = InitialVolume;
            
            using (var mf = new MediaFoundationReader(SongInfo.RadioStation.Endpoints.Stream))
            using (var wo = new WaveOutEvent())
            {
                wo.Init(mf);
                wo.Volume = 0.01f * InitialVolume;
                wo.Play();

                while (wo.PlaybackState == PlaybackState.Playing)
                {
                    //wo.Volume = 0.01f * SongInfo.Volume;

                    Thread.Sleep(TickRate);
                }
            }
        }

        internal static SongInfoStruct SongInfo;
        internal struct SongInfoStruct
        {
            internal struct RadioStationStruct
            {
                internal struct EndpointsStruct
                {
                    internal string Stream;
                    internal string Song;
                };

                internal string Name;
                internal string Url;
                internal EndpointsStruct Endpoints;
            };

            internal RadioStationStruct RadioStation;
            internal ID3TagData CurrentSong;

            internal TimeSpan Time;
            internal int Elapsed;

            internal int Bitrate;
            internal float Volume;

            internal int StreamDelay;
            internal bool IsIndividial;
        };
    }
}
