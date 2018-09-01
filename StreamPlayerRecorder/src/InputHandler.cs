using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

using NAudio.Wave;

using static Constants.Constants;
using static ExtensionMethods.DirectoryEx;
using static Helper.Functions;
using static Threads.Threads;
using static Types.Types;

namespace InputHandler
{
    internal class InputHandler
    {
        internal static void HandleInput()
        {
            string HelpText = null;
            foreach (string Command in Commands)
                HelpText += $"\n     {Command}";
            UpdateConsoleLine(HelpPos, $"Commands:{HelpText}");

            bool exit = false;
            UpdateConsoleLine(CommandPos, "Enter a command:");
            while (!exit)
            {
                if (CommandIsValid(Console.ReadLine()))
                    UpdateConsoleLine(InvalidPos, $"");
            }
        }

        internal static void UpdateConsoleLine(Vector2D Pos, string Line)
        {
            InputPos.X = Console.CursorLeft;
            FillLine(Pos.Y, " ");
            Console.WriteLine(Line);
            Console.SetCursorPosition(InputPos.X, InputPos.Y);
        }

        internal static bool CommandIsValid(string Line)
        {
            bool ret = false;
            switch (Line.ToLower())
            {
                case "play":
                case "pause":
                case "p":
                    SongInfo.PlaybackState = SongInfo.PlaybackState == PlaybackState.Playing ? PlaybackState.Paused : PlaybackState.Playing;
                    ret = true;
                    break;
                case "start":
                case "stop":
                case "s":
                    if (RecordStreamThread.ThreadState == ThreadState.Running)
                        RecordStreamThread.Abort();
                    else
                    {
                        RecordStreamThread = new Thread(RecordStream);
                        RecordStreamThread.Start();
                    }
                    ret = true;
                    break;
                case "volume up":
                case "vu":
                    if (SongInfo.Volume.Value < 100)
                        SongInfo.Volume.Value += 5;
                    ret = true;
                    break;
                case "volume down":
                case "vd":
                    if (SongInfo.Volume.Value > 0)
                        SongInfo.Volume.Value -= 5;
                    ret = true;
                    break;
                case "mute":
                case "unmute":
                case "m":
                    SongInfo.Volume.Muted = !SongInfo.Volume.Muted;
                    ret = true;
                    break;
                case "open":
                case "o":
                    System.Diagnostics.Process.Start("explorer.exe", Path.Combine(Environment.CurrentDirectory, Regex.Replace(SongInfo.RadioStation.Name, "\\/", "-")));
                    ret = true;
                    break;
                case "del":
                case "d":
                    Empty(Path.Combine(Environment.CurrentDirectory, Regex.Replace(SongInfo.RadioStation.Name, "\\/", "-")));
                    ret = true;
                    break;
                case "quit":
                case "q":
                    Environment.Exit(0);
                    break;
                default:
                    UpdateConsoleLine(InvalidPos, $"Invalid command: {Line}");
                    ret = false;
                    break;
            }
            UpdateConsoleLine(CommandPos, "Enter a command:");
            UpdateConsoleLine(InputPos, "");

            return ret;
        }
    }
}
