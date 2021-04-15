﻿namespace bl_syauqi.API.DTO.MS.Model
{
    public class AudioTrack
    {
        public long Id { get; set; }
        public string Codec { get; set; }
        public string Language { get; set; }
        public long Channels { get; set; }
        public long SamplingRate { get; set; }
        public long Bitrate { get; set; }
    }
}
