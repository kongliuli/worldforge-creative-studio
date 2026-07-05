namespace WorldForge.Core.Services;

public record ImportResult(int DocumentsCreated, string Format);

public interface IImportService
{
    Task<ImportResult?> ImportZipAsync(Guid projectId, Stream zipStream, CancellationToken ct = default);
}
