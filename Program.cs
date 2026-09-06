using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using MusicExplorerWasm;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddSingleton<PersistentData>();

builder.Services.AddHttpClient<MusicBrainz>();
builder.Services.AddHttpClient<CoverArtArchive>();
builder.Services.AddHttpClient<LrcLib>();

await builder.Build().RunAsync();
