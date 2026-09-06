namespace MusicExplorerWasm
{
    internal class CoverArtArchive: WebService
    {
        public CoverArtArchive(HttpClient httpClient)
            : base(httpClient, "https://coverartarchive.org/")
        {
        }

        /// <summary>
        /// Get the URI of the image -- not the actual bytes
        /// </summary>
        public async Task<string> GetAlbumCoverArtUriAsync(string releaseGroupId)
        {
            string url = $"{http.BaseAddress}/release-group/{releaseGroupId}/front-250";  // 250x250 px front cover image
            return url;
        }
    }
}
