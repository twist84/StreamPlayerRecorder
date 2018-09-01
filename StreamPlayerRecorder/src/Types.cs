using System;

using NAudio.Lame;
using NAudio.Wave;

namespace Types
{
    class Types
    {
        internal struct SongInfoStruct
        {
            internal struct RadioStationStruct
            {
                internal struct EndpointsStruct
                {
                    internal string Stream, Song;
                };

                internal string Name;
                internal EndpointsStruct Endpoints;
            };

            internal RadioStationStruct RadioStation;
            internal ID3TagData CurrentSong;

            internal struct ElapsedStruct
            {
                internal int Value;
                internal string Text;
            };

            internal struct VolumeStruct
            {
                internal bool Muted;
                internal int Value;
                internal string Text;
            };

            internal PlaybackState PlaybackState;

            internal TimeSpan Time;
            internal ElapsedStruct Elapsed;
            internal VolumeStruct Volume;

            internal int Bitrate, StreamDelay;
            internal bool IsContinuous;
        };

        internal struct FilterStruct
        {
            internal string StartsWith, Contains;
        }

        public struct Vector2D
        {
            public int X, Y;
        };
    }
}
