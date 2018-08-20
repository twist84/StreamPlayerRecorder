using System;
using System.IO;
using System.Net;
using System.Threading;
using NAudio.Wave;

namespace StreamPlayerRecorder
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!SetupSongInfo()) return;

            if (!StartThread(new Thread(() => UpdateSongInfo(SongInfo.RadioStation.Endpoints.CurrentSong)), TickRate)) return;

            //if (!StartThread(SongInfo.RecordingThread = new Thread(RecordStream), 0)) return;
            if (!StartThread(new Thread(() => PlayStream(50)), 0)) return;

            while (true)
            {
                SongInfo.Time = TimeSpan.FromSeconds(SongInfo.Elapsed);
                string elapsed = $"{SongInfo.Time.Minutes:00}:{SongInfo.Time.Seconds:00}";
                if (SongInfo.Time.Hours > 0)
                    elapsed = $"{SongInfo.Time.Hours: 00}:" + elapsed;

                Console.Clear();
                Console.WriteLine($"Elapsed: {elapsed}\t\tVolume: {SongInfo.Volume:00}%");
                Console.WriteLine($"Song: {SongInfo.CurrentSong}");
                Console.Title = $"{SongInfo.CurrentSong}, {elapsed}, {SongInfo.Volume:00}%";

                Thread.Sleep(TickRate);
                SongInfo.Elapsed++;
            }
        }

        internal static bool SetupSongInfo()
        {
            SongInfo.RadioStation.Name = "Metal Rock Radio by Sonixcast";
            SongInfo.RadioStation.Url = "http://cabhs30.sonixcast.com:9964";
            SongInfo.RadioStation.Endpoints.Stream = $"{SongInfo.RadioStation.Url}/stream";
            SongInfo.RadioStation.Endpoints.CurrentSong = $"{SongInfo.RadioStation.Url}/currentsong";

            SongInfo.PreviousSong = "theTwister - Suck Me Arse";
            SongInfo.Elapsed = 0;

            //SongInfo.Volume = 50;
            SongInfo.Bitrate = 192;

            SongInfo.IsIndividial = false;

            return true;
        }

        internal static void UpdateSongInfo(string Url)
        {
            using (WebClient client = new WebClient())
            {
                SongInfo.PreviousSong = client.DownloadString(Url);
                while (true)
                {
                    SongInfo.CurrentSong = client.DownloadString(Url);
                    if (SongInfo.CurrentSong != SongInfo.PreviousSong)
                    {
                        if (SongInfo.IsIndividial)
                            RetartThread(SongInfo.RecordingThread, TickRate);

                        SongInfo.PreviousSong = SongInfo.CurrentSong;
                        SongInfo.Elapsed = 0;
                    }

                    Thread.Sleep(TickRate);
                }
            }
        }

        internal static void RecordStream()
        {
            if (!SongInfo.IsIndividial && !Directory.Exists($".\\{SongInfo.RadioStation.Name}\\"))
                Directory.CreateDirectory($".\\{SongInfo.RadioStation.Name}\\");

            string Mp3FileName = $"{SongInfo.RadioStation.Name}";
            if (SongInfo.IsIndividial)
                Mp3FileName += $"\\{SongInfo.CurrentSong}";

            MediaFoundationEncoder.EncodeToMp3(new MediaFoundationReader(SongInfo.RadioStation.Url), $".\\{Mp3FileName}.mp3", SongInfo.Bitrate * 1000);
        }

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

        internal static bool StartThread(Thread thread, int TickRateInternal)
        {
            thread.Start();
            Thread.Sleep(TickRateInternal);

            return true;
        }

        internal static bool StopThread(Thread thread, int TickRateInternal)
        {
            thread.Interrupt();
            thread.Abort();
            Thread.Sleep(TickRateInternal);

            return true;
        }

        internal static bool RetartThread(Thread thread, int TickRateInternal)
        {
            StopThread(SongInfo.RecordingThread, TickRate);
            Thread.Sleep(TickRate);
            StartThread(SongInfo.RecordingThread = new Thread(RecordStream), 0);

            return true;
        }

        internal static readonly int TickRate = 1000 * 1;

        internal static SongInfoStruct SongInfo;
        internal struct SongInfoStruct
        {
            internal struct RadioStationStruct
            {
                internal struct EndpointsStruct
                {
                    internal string Stream;
                    internal string CurrentSong;
                };

                internal string Name;
                internal string Url;
                internal EndpointsStruct Endpoints;
            };

            internal RadioStationStruct RadioStation;
            internal float Volume;
            internal int Bitrate;

            internal string PreviousSong;
            internal string CurrentSong;

            internal TimeSpan Time;
            internal int Elapsed;

            internal Thread RecordingThread;

            internal bool IsIndividial;
        };
    }
}
