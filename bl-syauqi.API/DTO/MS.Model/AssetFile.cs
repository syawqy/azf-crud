using System.Collections.Generic;

namespace bl_syauqi.API.DTO.MS.Model
{
    public class AssetFile
    {
        public List<Source> Sources { get; set; }
        public List<VideoTrack> VideoTracks { get; set; }
        public List<AudioTrack> AudioTracks { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public string Duration { get; set; }
    }
}
