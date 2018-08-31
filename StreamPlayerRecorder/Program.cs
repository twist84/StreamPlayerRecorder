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
    using InputHandler;
    class Program
    {
        internal static readonly int TickRate = 1000 * 1;

        internal static Thread RecordStreamThread = null;

        static void Main(string[] args)
        {
            if (!SetupSongInfo()) return;
            new Thread(InputHandler.HandleInput).Start();

            new Thread(() => UpdateSongInfo()).Start();
            Thread.Sleep(TickRate);
            new Thread(() => PlayStream(SongInfo.Volume)).Start();
            RecordStreamThread = new Thread(RecordStream);
            RecordStreamThread.Start();

            while (true)
            {
                //Console.CursorVisible = false;
                InputHandler.UpdateConsoleLine(new InputHandler.Vector2D { Y = 0 }, $"Artist: {SongInfo.CurrentSong.Artist}, Title: {SongInfo.CurrentSong.Title}");
                InputHandler.UpdateConsoleLine(new InputHandler.Vector2D { Y = 1 }, $"Elapsed: {SongInfo.Elapsed.Text}\t\tVolume: {SongInfo.Volume:00}%");
                InputHandler.UpdateTitleText($"{SongInfo.CurrentSong.Artist}, {SongInfo.CurrentSong.Title}, {SongInfo.Elapsed.Text}, {SongInfo.Volume:00}%");

                Thread.Sleep(TickRate);
            }
        }

        internal static bool IsDefaultStaion;

        internal static bool SetupSongInfo()
        {
            IsDefaultStaion = true; // Change this to false if you want to set another station
            if (IsDefaultStaion)
            {
                SongInfo.IsContinuous = false;
                SongInfo.StreamDelay = 14;

                SongInfo.RadioStation.Name = "Metal Rock Radio by Sonixcast";
                SongInfo.RadioStation.Endpoints.Stream = $"http://cabhs30.sonixcast.com:9964/stream";
                SongInfo.RadioStation.Endpoints.Song = $"http://cabhs30.sonixcast.com:9964/currentsong";
            }
            else
            {
                SongInfo.IsContinuous = true; // Needs to be true
                SongInfo.RadioStation.Name = "Brian FM Ashburton"; // Change this to the station name
                SongInfo.RadioStation.Endpoints.Stream = "http://5.135.154.72:10106"; // Change this to the station stream
                SongInfo.CurrentSong = new ID3TagData { Artist = "Unknown", Title = "Unknown" }; // Can't get the song info because this app is only meant for Metal Rock Radio by Sonixcast
            }


            SongInfo.Elapsed.Time = 0;
            SongInfo.Bitrate = 192;
            SongInfo.Volume = 50;

            return true;
        }

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
                                SongInfo.Elapsed.Time = 0;

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

        internal static List<FilterStruct> Filters = new List<FilterStruct>() {
            new FilterStruct { StartsWith = "roderick", Contains = "carter" },
            new FilterStruct { StartsWith = "metal", Contains = "radio" },
            new FilterStruct { StartsWith = "id", Contains = "psa" },
        };

        internal struct FilterStruct
        {
            internal string StartsWith;
            internal string Contains;

        }

        internal static void RecordStream()
        {
            if (!SongInfo.IsContinuous && !Directory.Exists($".\\{SongInfo.RadioStation.Name}\\")) Directory.CreateDirectory($".\\{Regex.Replace(SongInfo.RadioStation.Name, "\\/", "-")}\\");

            string Mp3FilePath = $"{CleanFileName(SongInfo.RadioStation.Name)}";
            if (!SongInfo.IsContinuous) Mp3FilePath += Path.DirectorySeparatorChar + CleanFileName($"{Regex.Replace(SongInfo.CurrentSong.Artist, "\\/", "-")} - {Regex.Replace(SongInfo.CurrentSong.Title, "\\/", "-")}");

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

                SongInfo.PlaybackState = PlaybackState.Playing;
                wo.Play();

                while (true)
                {
                    PlayCheck(wo);
                    if (SongInfo.IsMuted)
                        wo.Volume = 0.01f * 0;
                    else
                        wo.Volume = 0.01f * SongInfo.Volume;

                    Thread.Sleep(TickRate);
                }
            }
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

        public static SongInfoStruct SongInfo;
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
                internal EndpointsStruct Endpoints;
            };

            internal RadioStationStruct RadioStation;
            internal ID3TagData CurrentSong;

            internal struct ElapsedStruct
            {
                internal int Time;
                internal string Text;
            };

            internal PlaybackState PlaybackState;
            internal bool IsMuted;

            internal TimeSpan Time;
            internal ElapsedStruct Elapsed;

            internal int Bitrate;
            internal int Volume;

            internal int StreamDelay;
            internal bool IsContinuous;
        };
    }
}

namespace InputHandler
{
    using StreamPlayerRecorder;
    internal class InputHandler
    {
        public struct Vector2D
        {
            public int X, Y;
        };
        public static Vector2D CommandPos = new Vector2D() { X = 0, Y = Console.WindowHeight - 3 };
        public static Vector2D InputPos = new Vector2D() { X = 1, Y = Console.WindowHeight - 2 };
        public static Vector2D HelpPos = new Vector2D() { X = 0, Y = 5 };

        public static void HandleInput()
        {
            string[] Commands = {
                "play, p",
                "pause, stop, s",
                "mute, m",
                "unmute, um",
                "volume up, volu, vol++, v++, v+",
                "volume down, vold, vol--, v--, v-",
                "exit, quit, q"
            };
            string HelpText = null;
            foreach (string Command in Commands)
                HelpText += $"\n     {Command}";
            UpdateConsoleLine(HelpPos, $"Commands:{HelpText}");

            bool exit = false;
            UpdateConsoleLine(CommandPos, "Enter a command:");
            bool IsValidInput = false;
            while (!exit)
            {
                var line = Console.ReadLine(); //blocks until key event
                switch (line.ToLower())
                {
                    case "play":
                    case "p":
                        if (Program.SongInfo.PlaybackState != PlaybackState.Playing)
                            Program.SongInfo.PlaybackState = PlaybackState.Playing;
                        IsValidInput = true;
                        break;
                    case "pause":
                    case "stop":
                    case "s":
                        if (Program.SongInfo.PlaybackState != PlaybackState.Paused)
                            Program.SongInfo.PlaybackState = PlaybackState.Paused;
                        IsValidInput = true;
                        break;
                    case "mute":
                    case "m":
                        Program.SongInfo.IsMuted = true;
                        IsValidInput = true;
                        break;
                    case "unmute":
                    case "um":
                        Program.SongInfo.IsMuted = false;
                        IsValidInput = true;
                        break;
                    case "volume up":
                    case "volu":
                    case "vol++":
                    case "v++":
                    case "v+":
                        if (Program.SongInfo.Volume < 100)
                            Program.SongInfo.Volume += 5;
                        IsValidInput = true;
                        break;
                    case "volume down":
                    case "vold":
                    case "vol--":
                    case "v--":
                    case "v-":
                        if (Program.SongInfo.Volume > 0)
                            Program.SongInfo.Volume -= 5;
                        IsValidInput = true;
                        break;
                    case "exit":
                    case "quit":
                    case "q":
                        Environment.Exit(0);
                        break;
                    default:
                        UpdateConsoleLine(new Vector2D { Y = Console.WindowHeight - 5 }, $"Invalid command: {line}");
                        IsValidInput = false;
                        break;
                }
                UpdateConsoleLine(CommandPos, "Enter a command:");
                UpdateConsoleLine(InputPos, "");

                if (IsValidInput)
                    UpdateConsoleLine(new Vector2D { Y = Console.WindowHeight - 5 }, $"");
            }
        }

        public static void UpdateTitleText(string Line)
        {
            Console.Title = Line;
        }

        public static void UpdateConsoleLine(Vector2D Pos, string Line)
        {
            InputPos.X = Console.CursorLeft;
            FillLine(Pos.Y, " ");
            Console.WriteLine(Line);
            Console.SetCursorPosition(InputPos.X, InputPos.Y);
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
    }
}