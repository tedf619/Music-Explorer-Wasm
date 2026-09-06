using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MusicExplorerWasm
{
  public record AlbumInfo(string Title, string FirstReleaseDate);

  internal class MusicBrainz : WebService
  {
    public MusicBrainz(HttpClient httpClient)
        : base(httpClient, "https://musicbrainz.org/ws/2/")
    {
      httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("MusicExplorer/1.0.2 (mister.magoo@gmail.com)");
      httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<string?> FindArtistId(string artistName)
    {
      string url =
          $"artist/?query={Uri.EscapeDataString($"artist:\"{artistName}\"")}" +
          "&limit=1&fmt=json";

      using HttpResponseMessage response = await GetAsync(url);
      response.EnsureSuccessStatusCode();

      string json = await response.Content.ReadAsStringAsync();
      await Task.Delay(1100);
      using JsonDocument doc = JsonDocument.Parse(json);
      JsonElement artists = doc.RootElement.GetProperty("artists");

      string? bestId = null;

      foreach (JsonElement artist in artists.EnumerateArray()) // should only be one artist since we limited to 1
        bestId = artist.GetProperty("id").GetString();

      return bestId;
    }

    // get all the albums for a given artist (e.g. The Beatles)
    public async Task<List<Album>?> FindAlbums(string artistId, string artistName)
    {
      var albums = new List<Album>();

      int offset = 0;
      int totalCount = 0;

      FireProgressChanged(5, 100);  // to report initial progress

      try
      {
        do
        {
          var albumsFound = new Dictionary<string, AlbumInfo>(); // key is release-group-id
          int PageSize = 100; // MusicBrainz allows up to 100 per page

          string query =
              $"release-group?artist={artistId}&" +
              $"primary-type=Album&" +
              $"country=US&" +
              $"limit={PageSize}&offset={offset}&fmt=json";

          using HttpResponseMessage response = await GetAsync(query);
          response.EnsureSuccessStatusCode();

          string json = await response.Content.ReadAsStringAsync();
          await Task.Delay(1100);
          using JsonDocument doc = JsonDocument.Parse(json);
          JsonElement root = doc.RootElement;

          totalCount = root.GetProperty("release-group-count").GetInt32();

          foreach (JsonElement rg in root.GetProperty("release-groups").EnumerateArray())
          {
            string rgId = rg.GetProperty("id").GetString() ?? "";
            if (rgId.Length == 0 || albumsFound.ContainsKey(rgId)) continue;

            string title = rg.TryGetProperty("title", out var t) ? t.GetString() ?? "(unknown)" : "(unknown)";
            string? firstReleaseDate = rg.TryGetProperty("first-release-date", out var frd) ? frd.GetString() : null;
            if (string.IsNullOrEmpty(firstReleaseDate)) continue;

            albumsFound[rgId] = new AlbumInfo(title, firstReleaseDate);

            var album = new Album
            {
              GroupId = rgId,
              Title = title,
              Date = GetDate(firstReleaseDate ?? ""),
              Artist = artistName,
              Label = ""
            };
            if (albums.Find(a => a.GroupId == album.GroupId) != null) continue;

            albums.Add(album);
          }

          offset += PageSize;
          FireProgressChanged(offset, totalCount);

        } while (offset < totalCount);

        return albums;
      }
      catch (WebException wex)
      {
        FireLogException(wex, $"A communication error occurred ({wex.Message}). The MusicBrainz server might be down.");
        return [];
      }
      catch (Exception ex)
      {
        FireLogException(ex, $"An error occurred ({ex.Message}). The error may have been caused by bad data from the MusicBrainz server.");
        return [];
      }
    }

    public async Task GetDetailedAlbumInfo(Album album)
    {
      MusicBrainzDto.Release? release = await FindReleaseWithGreatestTrackCount(album);
      if (release == null) return;
      album.Id = release.Id;

      await PopulateDetails(album);
    }

    public async Task<MusicBrainzDto.Release?> FindReleaseWithGreatestTrackCount(Album album)
    {
      const int limit = 100;
      var offset = 0;
      MusicBrainzDto.Release? largestRelease = null;
      var largestCount = -1;

      while (true)
      {
        var query =
            $"release?release-group={album.GroupId}" +
            $"&inc=media+genres+labels" +
            $"&format=CD" +
            $"&status=official" +
            $"&country=US" +
            $"&fmt=json&limit={limit}&offset={offset}";

        using HttpResponseMessage response = await GetAsync(query);
        string json = await response.Content.ReadAsStringAsync();
        var page = JsonSerializer.Deserialize<MusicBrainzDto.ReleaseBrowseResponse>(json);

        if (page == null) break;

        await Task.Delay(1100); // respect MusicBrainz rate limit
        var releases = page?.Releases ?? [];
        if (releases.Count == 0) break;

        foreach (var release in releases)
        {
          var trackCount = release.Media.Sum(m => m.TrackCount);

          if (trackCount > largestCount)
          {
            largestRelease = release;
            largestCount = trackCount;
          }
        }

        offset += releases.Count;

        if (offset >= page!.ReleaseCount) break;
      }

      var label = largestRelease?.LabelInfo?.FirstOrDefault()?.Label;
      album.Label = label?.Name ?? "";
      album.Genre = GetGenre(label);

      return largestRelease;
    }

    string GetGenre(MusicBrainzDto.Label? label)
    {
      if (label == null) return "(Unknown)";
      var genres = label?.Genres?.OrderByDescending(g => g.Count).ToList();
      if (genres == null || genres.Count == 0) return "(Unknown)";

      if (genres.Count < 2)
        return genres.First().Name ?? "(Unknown)";

      if (genres[0].Count == genres[1].Count)
        return $"{genres[0].Name}, {genres[1].Name}";

      return genres.First().Name ?? "(Unknown)";
    }

    public async Task PopulateDetails(Album album)
    {
      var query = $"release/{album.Id}?inc=recordings&fmt=json";

      using HttpResponseMessage response = await GetAsync(query);
      string json = await response.Content.ReadAsStringAsync();
      var release = JsonSerializer.Deserialize<MusicBrainzDto.Release>(json);
      if (release == null) return;

      var trackNumbersFounds = new Dictionary<string, MusicBrainzDto.Track>();

      foreach (var medium in release.Media.OrderBy(m => m.Position))
      {
        foreach (MusicBrainzDto.Track mbTrack in medium.Tracks.OrderBy(t => t.Position))
        {
          if (mbTrack == null) continue;
          if (trackNumbersFounds.ContainsKey(mbTrack.Number)) continue;

          trackNumbersFounds.Add(mbTrack.Number, mbTrack);
          var track = new Track();
          track.Id = mbTrack.Id;
          track.Artist = album.Artist!;
          track.Number = mbTrack.Position;
          track.Title = mbTrack.Title;
          track.Duration = mbTrack.LengthMs == null ? TimeSpan.Zero : TimeSpan.FromMilliseconds((long)mbTrack.LengthMs);
          track.RecordingId = mbTrack.Recording?.Id ?? mbTrack.Recording?.Id;

          album.Tracks.Add(track);
        }
      }
    }

    // return a DateTime from a string of the form yyyy-MM-dd, yyyy-MM or yyyy
    DateTime GetDate(string s)
    {
      if (string.IsNullOrEmpty(s)) return DateTime.MinValue;
      string[] words = s.Split("-".ToCharArray());
      if (words.Length == 3) return DateTime.ParseExact(s, "yyyy-MM-dd", null);
      if (words.Length == 2) return DateTime.ParseExact(s, "yyyy-MM", null);
      if (words.Length == 1) return DateTime.ParseExact(s, "yyyy", null);
      return DateTime.MinValue;
    }
  }
}
