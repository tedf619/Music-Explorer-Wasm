using System.Text.Json.Serialization;

/// <summary>
/// DTO (Data Transfer Object) classes for deserializing the MusicBrainz API responses related to 
/// releases, tracks, recordings, genres, labels, and works.
/// </summary>
namespace MusicExplorerWasm.MusicBrainzDto
{
    public sealed class ReleaseBrowseResponse
    {
        [JsonPropertyName("release-count")]
        public int ReleaseCount { get; set; }

        [JsonPropertyName("releases")]
        public List<Release> Releases { get; set; } = [];

        [JsonPropertyName("genres")]
        public List<Genre> Genres { get; set; } = [];
    }

    public sealed class Release
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("date")]
        public string? Date { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("media")]
        public List<Medium> Media { get; set; } = [];

        [JsonPropertyName("label-info")]
        public List<LabelInfo> LabelInfo { get; set; } = [];
    }

    public sealed class Medium
    {
        [JsonPropertyName("position")]
        public int Position { get; set; }

        [JsonPropertyName("format")]
        public string? Format { get; set; }

        [JsonPropertyName("track-count")]
        public int TrackCount { get; set; }

        [JsonPropertyName("tracks")]
        public List<Track> Tracks { get; set; } = [];
    }

    public sealed class Track
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("position")]
        public int Position { get; set; }

        [JsonPropertyName("number")]
        public string Number { get; set; } = "";

        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("length")]
        public int? LengthMs { get; set; }

        [JsonPropertyName("recording")]
        public Recording? Recording { get; set; }
    }

    public sealed class Recording
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("length")]
        public int? LengthMs { get; set; }

        [JsonPropertyName("relations")]
        public List<Relation> Relations { get; set; } = [];
    }

    public class Genre
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("disambiguation")]
        public string Disambiguation { get; set; } = "";
    }

    public class LabelInfo
    {
        [JsonPropertyName("catalog-number")]
        public string CatalogNumber { get; set; } = "";

        [JsonPropertyName("label")]
        public Label? Label { get; set; }
    }

    public class Label
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("disambiguation")]
        public string Disambiguation { get; set; } = "";

        [JsonPropertyName("genres")]
        public List<Genre> Genres { get; set; } = [];
    }

    public class Relation
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "";  // e.g. "performance"

        [JsonPropertyName("target-type")]
        public string TargetType { get; set; } = ""; // e.g. "work" for work relations

        [JsonPropertyName("direction")]
        public string Direction { get; set; } = "";

        [JsonPropertyName("work")]
        public Work? Work { get; set; } = null;
    }

    public class Work
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("type")]
        public string Type { get; set; } = ""; // e.g. "Song", "Symphony"

        [JsonPropertyName("language")]
        public string Language { get; set; } = "";

        [JsonPropertyName("disambiguation")]
        public string Disambiguation { get; set; } = "";

        [JsonPropertyName("iswcs")]
        public List<string> Iswcs { get; set; } = [];
    }
}
