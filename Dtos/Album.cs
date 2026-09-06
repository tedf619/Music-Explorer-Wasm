using System.Collections;

namespace MusicExplorerWasm
{
    public class Album
    {
        public string? Id;    // MusicBrainz ReleaseID
        public string? GroupId;  // MusicBrainz ReleaseGroupID
        public string? Title;
        public DateTime Date;
        public string? CoverArtUri;
        public string? Label;  // e.g. Sony, Warner
        public string? Genre;
        public string? Artist;
        public List<Track> Tracks = new List<Track>();
    }

    public class Track
    {
        public string? Id;     // MusicBrainz ID
        public string? RecordingId;  // MusicBrainz RecordingID
        public int Number;
        public TimeSpan Duration = TimeSpan.Zero;
        public string Title = "";
        public string Artist = "";
        public string Lyrics = "";
    }
}
