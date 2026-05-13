var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddSingleton<StaticExporter>();

if (args.Contains("--export"))
{
    builder.WebHost.UseUrls("http://127.0.0.1:5002");
    var exportApp = builder.Build();

    exportApp.MapStaticAssets();
    exportApp.MapRazorPages().WithStaticAssets();

    var exporter = exportApp.Services.GetRequiredService<StaticExporter>();
    _ = exportApp.StartAsync();
    await exporter.ExportAsync("http://127.0.0.1:5002");
    await exportApp.StopAsync();
    Console.WriteLine("Export complete.");
    return;
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
