public class StaticExporter(IWebHostEnvironment env)
{
    public async Task ExportAsync(string baseUrl)
    {
        var outputDir = Path.Combine(env.ContentRootPath, "..", "..", "publish", "static");
        var http = new HttpClient { BaseAddress = new Uri(baseUrl) };

        var pages = DiscoverPages();

        Console.WriteLine($"Exporting {pages.Count} pages to {outputDir}...");

        foreach (var (url, filename) in pages)
        {
            try
            {
                var response = await http.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var html = await response.Content.ReadAsStringAsync();

                var filePath = Path.Combine(outputDir, filename);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                await File.WriteAllTextAsync(filePath, html);

                Console.WriteLine($"  OK  {url} -> {filename}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  FAIL {url}: {ex.Message}");
            }
        }

        CopyStaticAssets(outputDir);
    }

    private List<(string url, string filename)> DiscoverPages()
    {
        var pagesDir = Path.Combine(env.ContentRootPath, "Pages");
        var cshtmlFiles = Directory.GetFiles(pagesDir, "*.cshtml", SearchOption.AllDirectories);

        var result = new List<(string, string)>();

        foreach (var file in cshtmlFiles)
        {
            var relativePath = Path.GetRelativePath(pagesDir, file);

            if (relativePath.StartsWith("Shared") || relativePath.StartsWith("_"))
                continue;

            relativePath = relativePath.Replace('\\', '/');

            var url = "/" + relativePath
                .Replace("/Index.cshtml", "")
                .Replace("Index.cshtml", "")
                .Replace(".cshtml", "");

            if (string.IsNullOrEmpty(url)) url = "/";

            var filename = relativePath
                .Replace(".cshtml", ".html")
                .Replace("/Index.html", "/index.html");

            if (url == "/") filename = "index.html";

            result.Add((url, filename));
        }

        return result;
    }

    private void CopyStaticAssets(string outputDir)
    {
        var wwwroot = Path.Combine(env.ContentRootPath, "wwwroot");
        if (!Directory.Exists(wwwroot)) return;

        foreach (var dirPath in Directory.GetDirectories(wwwroot, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(wwwroot, outputDir));
        }

        foreach (var filePath in Directory.GetFiles(wwwroot, "*", SearchOption.AllDirectories))
        {
            var dest = filePath.Replace(wwwroot, outputDir);
            File.Copy(filePath, dest, overwrite: true);
        }

        Console.WriteLine($"  OK  static assets copied to {outputDir}");
    }
}
