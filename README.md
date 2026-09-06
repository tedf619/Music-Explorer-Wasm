# Music Explorer Wasm
### A C# Web App with Blazor Web Assembly 

Microsoft Blazor is a web framework that’s been around since 2018. What stands it apart from React, Angular and virtually all the other web frameworks is its native support for C#. You can develop most or even all a web project using solely C#. Blazor components are made up by .razor files, which combine HTML-like markup with C#. Most Blazor projects have the traditional client-server architecture, where much of the code runs on the server and the client handles the UI. But Blazor also lets you develop standalone projects, where all the code runs on the client. You need a server only to provide the client code for download. Once downloaded, the server is no longer needed, unless of course you need to get data stored on the server. The client C# code is compiled into Web Assembly (Wasm) and executed at near-native speed. Standalone projects represent sort of a middle ground between native apps and traditional web apps. 

To illustrate the power of Blazor Wasm, I’ll describe a simple but full-fledged standalone web application called MusicExplorerWasm. The project was built with .Net 10. The following figure shows the app in action. 

<img width="851" height="584" alt="image" src="https://github.com/user-attachments/assets/a22d5e7d-e366-46e0-b267-18090f847530" />

*Figure 1 - The MusicExplorerWasm interface.*

If it weren’t for the browser bar at the top, MusicExplorerWasm could almost pass for the Music Explorer desktop app I developed using Windows Forms. Both apps lookup recording artists and their albums. For each album they show the tracks included and the lyrics. A handful of free web services are used to get the actual data. To simplify the layout coding, I used MudBlazor components, which are free and open source. 

## How it works

There are three main user actions:

  1.	Searching for an artist. The MusicBrainz web service is called to get a list of albums.
  2.	Selecting an album. The MusicBrainz web service is called to get the album details, including the tracks. The CoverArtArchive service is called to get the cover art image.
  3.	Selecting a song. The LrcLib service is called to get the song lyrics.

The following sequence diagram shows the details.

<img width="852" height="629" alt="image" src="https://github.com/user-attachments/assets/5dfcdd1a-dde8-4059-9b0f-9d74e15ca676" />

*Figure 2 - The main operations of the app.*

Since the three web services have things in common, that functionality was migrated to a base class as shown in the next figure.

<img width="649" height="278" alt="image" src="https://github.com/user-attachments/assets/3ff8403f-e2cf-4e2b-ae51-671f2f15356b" />

*Figure 3 - The class hierarchy of web services.*

## The UI Layout

The app is hosted in a single form subdivided into four layers. Each layer uses MudStack components configured as flexboxes to achieve the desired layout. See the following figure.

<img width="974" height="687" alt="image" src="https://github.com/user-attachments/assets/347cfad0-8dbb-4795-99c2-ef02ed687e24" />

*Figure 4 - The four layers of panels used for the UI.*

*	Layer 1: Shown in purple, is the main page which acts as a flex container for the other layers.
*	Layer 2: Shown in blue, holds a top, middle and bottom panel. The top panel holds the Search Bar, the bottom panel holds the Status Bar and the middle panel holds the gist of the application.
*	Layer 3: Shown in red, subdivides the middle panel into three parts. The left panel holds the Album List, the middle panel holds the Album Details, the right panel holds the HTTP Log (not visible in Figure 1).
*	Layer 4: Shown in green, further subdivides the middle red panel into areas for the Album Details, the Track List and the Lyrics.

## Persisting Data

Applications sometimes save user data for subsequent runs. In the case of MusicExplorerWasm, we save the name of the last Artist entered. When starting up, we load the name so the user doesn’t have to reenter it. Reading and writing data is managed by class PersistentData, which uses the browser’s Local Storage with the following code:

```csharp
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
```
*Listing 1- The class that manages the app’s persistent user settings.*

We use the JavaScript runtime to interact with Local Storage because Web Assembly doesn’t yet have a way to interact with some browser resources. This will change over time but for the time being JavaScript is required. At any rate the use of Local Storage gives us a good excuse to show how to call low-level JS methods from C# in Blazor. 
You can use the browser Dev Tools to inspect the data stored in Local Storage, as shown in the next figure.

<img width="605" height="317" alt="image" src="https://github.com/user-attachments/assets/fcd9e21a-b7d5-45fa-b088-a64863b4342c" />

*Figure 5 – Using the browser’s Dev Tools to inspect user settings in Local Storage.*

We load the Artist settings during initialization of the Home page:

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
  if (!firstRender) return;
  artist = await persistentData.GetItemAsync<string>(PersistentData.ArtistKey) ?? "";
  StateHasChanged();
}
```
*Listing 2- Loading settings at startup time.*

Saving updated settings is just as easy. We save the artist’s name when the user clicks the Search button:

```csharp
async void SearchButton_Clicked(string theArtist)
 {
   artist = theArtist;
   await persistentData.SetItemAsync(PersistentData.ArtistKey, artist);
   mainContent!.FindArtistAlbums(artist);
 }
```
*Listing 3- Saving user settings.*

## Showing HTTP Traffic

The MainContent component uses the HTTP services MusicBrainz, CoverArtArchive and LrcLib to get data. In Blazor, which is based on ASP.NET, we don’t instantiate services directly. Instead, we tell the application Builder to create the service for us, as shown in the next listing.

```csharp
var builder = WebAssemblyHostBuilder.CreateDefault(args);
//...
builder.Services.AddHttpClient<MusicBrainz>();
builder.Services.AddHttpClient<CoverArtArchive>();
builder.Services.AddHttpClient<LrcLib>();
```
*Listing 4- Call the app Builder in Program.cs to instantiate our HTTP web services.*

To show HTTP traffic, we expose some events from the web services. Since the three http services derive from the same base class, we define those events in the base class, as shown in the next figure:

<img width="402" height="101" alt="image" src="https://github.com/user-attachments/assets/0596735f-2b97-4a6f-8f30-da01e9011a69" />

*Figure 6 – The events exposed by the HTTP web services.*

Those events are declared static, so we only need to wire them to handlers once, even though we have three different incarnations of web services (MusicBrainz, CoverArtArchive and LrcLib). The Home page handles the ProgressChanged event by updating the progress bar on the status bar.

```csharp
protected override void OnInitialized()
{
    base.OnInitialized();

    WebService.ProgressChanged += WebService_ProgressChanged;
}

// ...

async void WebService_ProgressChanged(int value, int total)
{
    progressValue = (value * 100) / total;
    StateHasChanged();

    if (value >= total)
    {
        await Task.Delay(500);  // give enough time for user to see completion
        progressValue = 0;
        StateHasChanged();
    }
}
```
*Listing 5- The code in Home.razor that deals with the ProgressChanged event.*

The other two events – LogMessage and LogException – are handled by MainContent by updating the HTTP Log.

```csharp
protected override void OnInitialized()
{
    base.OnInitialized();

    WebService.LogMessage += WebService_LogMessage;
    WebService.LogException += WebService_LogException;
}

void WebService_LogException(Exception ex, string message)
{
    httpLog?.AddItem(message, Color.Error);
}

void WebService_LogMessage(string message)
{
    httpLog?.AddItem(message, Color.Default);
}
```
*Listing 6- The code in MainContent.razor that handles the LogMessage and LogException events.*

When exceptions occur, they are shown in red text in the HTTP Log. Ordinary messages are shown in black. The ProgressChanged event is fired during the relatively lengthy process of retrieving an artist’s list of albums. The event handler updates the progress bar shown on the right side of the status bar while getting data.

Something to be aware of is that Music Explorer occasionally shows HTTP 503 (Service Temporarily Unavailable) errors in the log. These are triggered by the MusicBrainz service in order to throttle incoming requests. Since these errors are expected, the MusicBrainz web service pauses after failed requests and then automatically retries a few times. The following figure shows a log with an error.

<img width="903" height="620" alt="image" src="https://github.com/user-attachments/assets/dba27333-78f6-4887-aea1-0519de3fc885" />

*Figure 7 – The HTTP Log showing a communication error in red.*

## Conclusion

As you can see from MusicExplorerWasm, Blazor is a powerful tool for developing web applications in C#. In standalone mode, all the code is downloaded from the server, compiled into Web Assembly and run on the client. By offloading execution to the client, even a skimpy server can handle a lot of clients. All the server needs to do is send the client code over the wire. On subsequent runs, the client checks to see if the server has updated code. If so, the new code is downloaded, otherwise the old code on the client is used again.

There are many aspects of Blazor I didn’t cover here, including the syntax of razor files, how to handle dialog boxes and others. For all the details, see the source code.
