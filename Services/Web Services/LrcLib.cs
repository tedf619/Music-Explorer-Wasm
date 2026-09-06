using System.Text.Json;
using System.Text.Json.Serialization;

namespace MusicExplorerWasm
{
    public class LrcLibTrackLyrics
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("trackName")]
        public string TrackName { get; set; } = "";

        [JsonPropertyName("artistName")]
        public string ArtistName { get; set; } = "";

        [JsonPropertyName("albumName")]
        public string AlbumName { get; set; } = "";

        [JsonPropertyName("duration")]
        public double Duration { get; set; }

        [JsonPropertyName("instrumental")]
        public bool Instrumental { get; set; }

        [JsonPropertyName("plainLyrics")]
        public string PlainLyrics { get; set; } = "";

        [JsonPropertyName("syncedLyrics")]
        public string SyncedLyrics { get; set; } = "";

        [JsonPropertyName("lyricsfile")]
        public string LyricsFile { get; set; } = "";
    }

    /// <summary>
    /// This class retrieves lyrics using the LrcLib web service, which returns data in JSON format. 
    /// The lyrics are returned in the "plainlyrics" field.
    /// </summary>
    internal class LrcLib: WebService
    {
        public LrcLib(HttpClient httpClient)
            : base(httpClient, "https://lrclib.net/api/")
        {
        }

        public async Task<string> GetLyricsForSong(string artist, string title)
        {
            var artistFormatted = FormattedText(artist);
            var titleFormatted = FormattedText(title);

            string query = $"search?artist_name={artistFormatted}&track_name={titleFormatted}";

            try
            {
                var response = await GetAsync(query);
                string json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<List<LrcLibTrackLyrics>>(json);
                if (data == null || data.Count == 0) return "No lyrics found";
                return data[0].PlainLyrics;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "No lyrics found";
            }
        }

        public string FormattedText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            var result = text.ToLowerInvariant().Trim();
            result = result.Replace(".", "");
            result = result.Replace(",", "");
            result = result.Replace("<", "");
            result = result.Replace(">", "");
            result = result.Replace(" ", "+");
            return result;
        }
    }
}
