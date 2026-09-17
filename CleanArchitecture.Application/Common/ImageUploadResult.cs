namespace CleanArchitecture.Application.Common;

public record ImageUploadResult(
    Guid Id,
    string PublicId,
    string Url);
