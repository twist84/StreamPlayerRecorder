using System;
using System.Threading;

using NAudio.Lame;

using static Constants.Constants;
using static ExtensionMethods.ConsoleEx;
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
            if (!Remove.MinimizeButton()) return;

            IsDefaultStaion = true;

            SongInfo.RadioStation.Name = IsDefaultStaion ? "Metal Rock Radio by Sonixcast" : "Brian FM Ashburton";
            SongInfo.RadioStation.Endpoints.Stream = IsDefaultStaion ? $"http://cabhs30.sonixcast.com:9964/stream" : "http://5.135.154.72:10106";
            SongInfo.RadioStation.Endpoints.Song = IsDefaultStaion ? $"http://cabhs30.sonixcast.com:9964/currentsong" : "";
            SongInfo.IsContinuous = IsDefaultStaion ? false : true;
            SongInfo.CurrentSong = new ID3TagData { Artist = "Unknown", Title = "Unknown" };
            SongInfo.Elapsed.Time = 0;
            SongInfo.Bitrate = 192;
            SongInfo.Volume = 20;
            SongInfo.StreamDelay = IsDefaultStaion ? 14 : 0;

            new Thread(() => { while (true) { Console.BufferWidth = Console.WindowWidth = 120; Console.BufferHeight = Console.WindowHeight = 30; Thread.Sleep(100); } }).Start();
            new Thread(HandleInput).Start();
            new Thread(UpdateSongInfo).Start();
            new Thread(() => PlayStream(SongInfo.Volume)).Start();
            Thread.Sleep(TickRate);
            RecordStreamThread = new Thread(RecordStream);
            RecordStreamThread.Start();

            while (true)
            {
                //Console.CursorVisible = false;
                UpdateConsoleLine(new Vector2D { Y = 0 }, $"Artist: {SongInfo.CurrentSong.Artist}, Title: {SongInfo.CurrentSong.Title}");
                UpdateConsoleLine(new Vector2D { Y = 1 }, $"Elapsed: {SongInfo.Elapsed.Text}\t\tVolume: {SongInfo.Volume:00}%");
                UpdateTitleText($"{SongInfo.CurrentSong.Artist}, {SongInfo.CurrentSong.Title}, {SongInfo.Elapsed.Text}, {SongInfo.Volume:00}%");

                Thread.Sleep(TickRate);
            }
        }
    }
}