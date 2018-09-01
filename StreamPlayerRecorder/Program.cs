using System;
using System.Collections.Generic;
using System.Threading;

using NAudio.Lame;

using static Constants.Constants;
using static ExtensionMethods.ConsoleEx;
using static Helper.Functions;
using static InputHandler.InputHandler;
using static Threads.Threads;
using static Types.Types;

namespace StreamPlayerRecorder
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!Remove.MinimizeButton()) return;
            if (!Remove.MaximizeButton()) return;

            IsDefaultStaion = true;

            SongInfo.RadioStation.Name = IsDefaultStaion ? "Metal Rock Radio by Sonixcast" : "Brian FM Ashburton";
            SongInfo.RadioStation.Endpoints.Stream = IsDefaultStaion ? $"http://cabhs30.sonixcast.com:9964/stream" : "http://5.135.154.72:10106";
            SongInfo.RadioStation.Endpoints.Song = IsDefaultStaion ? $"http://cabhs30.sonixcast.com:9964/currentsong" : "";
            SongInfo.IsContinuous = IsDefaultStaion ? false : true;
            SongInfo.CurrentSong = new ID3TagData { Artist = "Unknown", Title = "Unknown" };
            SongInfo.Elapsed.Value = 0;
            SongInfo.Bitrate = 192;
            SongInfo.Volume.Value = 20;
            SongInfo.Volume.Muted = false;
            SongInfo.StreamDelay = IsDefaultStaion ? 14 : 0;

            HelpPos = ToVector2D(0, Console.WindowHeight - 6 - Commands.Length);
            InvalidPos = ToVector2D(0, Console.WindowHeight - 5);
            CommandPos = ToVector2D(0, Console.WindowHeight - 3);
            InputPos = ToVector2D(1, Console.WindowHeight - 2);

            Filters = new List<FilterStruct>() {
                ToFilter("roderick", "carter"),
                ToFilter("metal", "radio"),
                ToFilter("id", "psa")
            };

            new Thread(() => { while (true) { Console.BufferWidth = Console.WindowWidth = 120; Console.BufferHeight = Console.WindowHeight = 30; Thread.Sleep(100); } }).Start();
            new Thread(HandleInput).Start();
            new Thread(UpdateSongInfo).Start();
            new Thread(PlayStream).Start();
            Thread.Sleep(TickRate);
            RecordStreamThread = new Thread(RecordStream);
            RecordStreamThread.Start();

            while (true)
            {
                //Console.CursorVisible = false;
                UpdateConsoleLine(ToVector2D(0, 0), $"Artist: {SongInfo.CurrentSong.Artist}");
                UpdateConsoleLine(ToVector2D(0, 1), $"Title: {SongInfo.CurrentSong.Title}");
                UpdateConsoleLine(ToVector2D(0, 2), $"Elapsed: {SongInfo.Elapsed.Text}");
                UpdateConsoleLine(ToVector2D(0, 3), $"Volume: {SongInfo.Volume.Text}");
                UpdateConsoleLine(ToVector2D(0, 4), $"Recording: {RecordStreamThread.ThreadState == ThreadState.Running}");
                UpdateTitleText($"{SongInfo.CurrentSong.Artist}, {SongInfo.CurrentSong.Title}, {SongInfo.Elapsed.Text}, {SongInfo.Volume.Text}");

                Thread.Sleep(TickRate);
            }
        }
    }
}