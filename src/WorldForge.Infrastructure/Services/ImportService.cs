using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using WorldForge.Core.Services;

namespace WorldForge.Infrastructure.Services;

public partial class ImportService(
    IProjectService projectService,
    IManuscriptService manuscripts) : IImportService
{
    private const int MaxZipBytes = 50 * 1024 * 1024;

    public async Task<ImportResult?> ImportZipAsync(
        Guid projectId, Stream zipStream, CancellationToken ct = default)
    {
        if (await projectService.GetAsync(projectId, ct) is null)
            return null;

        using var zip = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: true);
        if (zip.Entries.Sum(e => e.Length) > MaxZipBytes)
            return null;

        var scrivx = zip.Entries.FirstOrDefault(e =>
            e.Name.EndsWith(".scrivx", StringComparison.OrdinalIgnoreCase));
        if (scrivx is not null)
        {
            var count = await ImportScrivenerAsync(projectId, zip, scrivx, ct);
            return new ImportResult(count, "scrivener");
        }

        var mdCount = await ImportMarkdownZipAsync(projectId, zip, ct);
        return new ImportResult(mdCount, "markdown-zip");
    }

    private async Task<int> ImportMarkdownZipAsync(
        Guid projectId, ZipArchive zip, CancellationToken ct)
    {
        var created = 0;
        foreach (var entry in zip.Entries.Where(e =>
                     e.Name.EndsWith(".md", StringComparison.OrdinalIgnoreCase) &&
                     !e.FullName.Contains("__MACOSX", StringComparison.Ordinal)))
        {
            await using var stream = entry.Open();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var md = await reader.ReadToEndAsync(ct);
            var title = Path.GetFileNameWithoutExtension(entry.Name);
            var doc = await manuscripts.CreateAsync(projectId, title, null, null, ct);
            if (doc is null) continue;

            await manuscripts.SaveAsync(projectId, doc.Id, new SaveManuscriptRequest(
                null, MarkdownToTipTap(md), null, null), ct);
            created++;
        }

        return created;
    }

    private async Task<int> ImportScrivenerAsync(
        Guid projectId, ZipArchive zip, ZipArchiveEntry scrivxEntry, CancellationToken ct)
    {
        await using var stream = scrivxEntry.Open();
        var doc = XDocument.Load(stream);
        var root = doc.Descendants("BinderItem").FirstOrDefault();
        if (root is null) return 0;

        return await ImportBinderItemAsync(projectId, root, null, zip, ct);
    }

    private async Task<int> ImportBinderItemAsync(
        Guid projectId,
        XElement item,
        Guid? parentId,
        ZipArchive zip,
        CancellationToken ct)
    {
        var created = 0;
        var title = item.Element("Title")?.Value?.Trim();
        if (string.IsNullOrWhiteSpace(title))
            return 0;

        var type = item.Attribute("Type")?.Value;
        Guid? docId = null;
        if (type is "Text" or "Document")
        {
            var manuscript = await manuscripts.CreateAsync(projectId, title, parentId, null, ct);
            if (manuscript is not null)
            {
                docId = manuscript.Id;
                created++;
                var uuid = item.Attribute("UUID")?.Value;
                var text = TryReadScrivenerContent(zip, uuid);
                if (text is not null)
                {
                    await manuscripts.SaveAsync(projectId, manuscript.Id, new SaveManuscriptRequest(
                        null, MarkdownToTipTap(text), null, null), ct);
                }
            }
        }

        var nextParent = docId ?? parentId;
        foreach (var child in item.Elements("Children").Elements("BinderItem"))
            created += await ImportBinderItemAsync(projectId, child, nextParent, zip, ct);

        return created;
    }

    private static string? TryReadScrivenerContent(ZipArchive zip, string? uuid)
    {
        if (string.IsNullOrEmpty(uuid)) return null;
        var entry = zip.Entries.FirstOrDefault(e =>
            e.FullName.Contains(uuid, StringComparison.OrdinalIgnoreCase) &&
            (e.Name.EndsWith(".rtf", StringComparison.OrdinalIgnoreCase) ||
             e.Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) ||
             e.Name.EndsWith(".md", StringComparison.OrdinalIgnoreCase)));
        if (entry is null) return null;

        using var reader = new StreamReader(entry.Open(), Encoding.UTF8);
        var raw = reader.ReadToEnd();
        return entry.Name.EndsWith(".rtf", StringComparison.OrdinalIgnoreCase)
            ? RtfStrip().Replace(raw, " ").Trim()
            : raw;
    }

    public static string MarkdownToTipTap(string markdown)
    {
        var paragraphs = markdown
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(p => new
            {
                type = "paragraph",
                content = new[] { new { type = "text", text = p.Replace("\n", " ") } },
            })
            .ToList();

        if (paragraphs.Count == 0)
            paragraphs.Add(new { type = "paragraph", content = new[] { new { type = "text", text = "" } } });

        return JsonSerializer.Serialize(new { type = "doc", content = paragraphs });
    }

    [GeneratedRegex(@"\\[a-z]+\d* ?|\\'[0-9a-f]{2}|[{}]")]
    private static partial Regex RtfStrip();
}
