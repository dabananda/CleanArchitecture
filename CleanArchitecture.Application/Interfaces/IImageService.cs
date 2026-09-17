using CleanArchitecture.Application.Common;
using Microsoft.AspNetCore.Http;

namespace CleanArchitecture.Application.Interfaces;

public interface IImageService
{
    Task<Result<ImageUploadResult>> UploadAsync(IFormFile file, CancellationToken cancellationToken = default);
    Task<Result> UpdateEntityIdAsync(Guid imageId, Guid entityId, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
