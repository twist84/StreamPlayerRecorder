using System;

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
            internal NAudio.Lame.ID3TagData CurrentSong;

            internal struct ElapsedStruct
            {
                internal int Time;
                internal string Text;
            };

            internal NAudio.Wave.PlaybackState PlaybackState;
            internal bool IsMuted;

            internal TimeSpan Time;
            internal ElapsedStruct Elapsed;

            internal int Bitrate, Volume, StreamDelay;
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
