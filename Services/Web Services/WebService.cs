namespace MusicExplorerWasm
{
  internal class WebService
  {
    protected HttpClient http;

    public WebService(HttpClient httpClient, string baseAddress)
    {
      http = httpClient;
      http.BaseAddress = new Uri(baseAddress);
    }

    protected async Task<HttpResponseMessage> GetAsync(string relativeUri)
    {
      for (int i = 0; i < 3; i++)
      {
        using HttpRequestMessage request = new(HttpMethod.Get, relativeUri);
        var response = await DoGetAsync(request);
        if (response != null) return response;

        // wait for a short period before retrying
        await Task.Delay(1000);
      }
      return new();
    }

    async Task<HttpResponseMessage?> DoGetAsync(HttpRequestMessage request)
    {
      NotifyRequest(request);

      try
      {
        var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        await NotifyResponse(response);
        return response;
      }
      catch (Exception ex)
      {
        NotifyException(ex);
        return null;
      }
    }

    void NotifyRequest(HttpRequestMessage request)
    {
      string message = $"HTTP Request {request.Method} {http.BaseAddress}{request.RequestUri}";
      FireLogMessage(message);
    }

    async Task NotifyResponse(HttpResponseMessage response)
    {
      var length = response.Content.Headers.ContentLength;
      string byteCount = length?.ToString("n0") ?? string.Empty;

      if (byteCount == String.Empty)  // some responses don't have a Content-Length header
      {
        string html = await response.Content.ReadAsStringAsync();
        byteCount = html.Length.ToString("n0");
      }

      string message = $"HTTP Response {response.StatusCode} ({byteCount} bytes)";
      FireLogMessage(message);
    }

    void NotifyException(Exception ex)
    {
      string message = $"HTTP Exception {ex.Message}";
      FireLogException(ex, message);
    }

    #region Events (static!)
    public delegate void LogMessageHandler(string message);
    public static event LogMessageHandler? LogMessage;
    async public void FireLogMessage(string message)
    {
      LogMessage?.Invoke(message);
    }

    public delegate void LogExceptionHandler(Exception ex, string message);
    public static event LogExceptionHandler? LogException;
    public void FireLogException(Exception ex, string message)
    {
      LogException?.Invoke(ex, message);
    }

    public delegate void ProgressHandler(int value, int total);
    public static event ProgressHandler? ProgressChanged;
    public void FireProgressChanged(int value, int total)
    {
      ProgressChanged?.Invoke(value, total);
    }
    #endregion
  }
}
