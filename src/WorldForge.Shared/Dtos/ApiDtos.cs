namespace WorldForge.Shared.Dtos;

public record ProjectSummaryDto(Guid Id, string Name, DateTime LastOpenedAt, string TenantTemplateId);

public record CreateProjectRequest(string Name);

public record ContradictionDto(
    Guid Id,
    string Type,
    string Severity,
    string Summary,
    string Status,
    Guid? RelatedEntityId,
    string? Suggestion);

public record LicenseVerifyRequest(string LicenseKey);

public record LicenseVerifyResponse(bool Valid, string? Message);

public record TenantSummaryDto(
    string Id,
    string DisplayName,
    string SecurityLevel,
    string SkinId,
    bool ContradictionDetection);

public record TenantFeatureFlagsDto(
    bool ContradictionDetection,
    bool Collaboration,
    bool CloudSync);

public record TenantDetailDto(
    string Id,
    string DisplayName,
    string SecurityLevel,
    string SkinId,
    IReadOnlyList<string> EntityTypes,
    IReadOnlyList<string> PluginIds,
    TenantFeatureFlagsDto Features);

public record HealthResponse(string Status, string Version);

public record ManuscriptNodeDto(
    Guid Id,
    string Title,
    Guid? ParentId,
    int SortOrder,
    int WordCount,
    string Status);

public record EntityLinkDto(Guid TargetEntityId, int StartOffset, int EndOffset);

public record ManuscriptDetailDto(
    Guid Id,
    string Title,
    string ContentJson,
    string Status,
    int WordCount,
    IReadOnlyList<EntityLinkDto> EntityLinks);

public record CreateManuscriptRequest(string Title, Guid? ParentId, int? SortOrder);

public record UpdateManuscriptRequest(
    string? Title,
    string? ContentJson,
    string? Status,
    IReadOnlyList<EntityLinkDto>? EntityLinks);

public record ReorderManuscriptItemDto(Guid Id, Guid? ParentId, int SortOrder);

public record ReorderManuscriptsRequest(IReadOnlyList<ReorderManuscriptItemDto> Items);

public record EntitySummaryDto(Guid Id, string Discriminator, string DisplayName);

public record EntityDetailDto(
    Guid Id,
    string Discriminator,
    string Title,
    string ContentJson);

public record UpsertEntityRequest(string Discriminator, string Title, string? ContentJson);

public record UpdateContradictionStatusRequest(string Status);

public record ProjectSettingsDto(bool AutoDetectOnSave, string PreferredOllamaModel);

public record UpdateProjectSettingsRequest(bool? AutoDetectOnSave, string? PreferredOllamaModel);
