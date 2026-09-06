using Microsoft.JSInterop;
using System.Text.Json;

namespace MusicExplorerWasm
{
  // reads/writes data to local storage in JSON format using JavaScript interop
public class PersistentData
{
  readonly IJSRuntime jsRuntime;

  public PersistentData(IJSRuntime js) => jsRuntime = js;

  public const string ArtistKey = "Artist";

  public async Task SetItemAsync<T>(string key, T value)
  {
    var json = JsonSerializer.Serialize(value);
    await jsRuntime.InvokeVoidAsync("localStorageInterop.setItem", key, json);
  }

  public async Task<T?> GetItemAsync<T>(string key)
  {
    var json = await jsRuntime.InvokeAsync<string>("localStorageInterop.getItem", key);
    return json is null ? default : JsonSerializer.Deserialize<T>(json);
  }

  public async Task RemoveItemAsync(string key)
  {
    await jsRuntime.InvokeVoidAsync("localStorageInterop.removeItem", key);
  }
}
}
