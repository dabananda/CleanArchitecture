using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Infrastructure.Configurations;
using CleanArchitecture.Infrastructure.Persistence;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ImageUploadResult = CleanArchitecture.Application.Common.ImageUploadResult;

namespace CleanArchitecture.Infrastructure.Services;

public class CloudinaryImageService : IImageService
{
    private readonly Cloudinary _cloudinary;
    private readonly ApplicationDbContext _context;

    public CloudinaryImageService(CloudinarySettings cloudinarySettings, ApplicationDbContext context)
    {
        var account = new Account(cloudinarySettings.CloudName, cloudinarySettings.ApiKey, cloudinarySettings.ApiSecret);
        _cloudinary = new Cloudinary(account);
        _context = context;
    }

    public async Task<Result<ImageUploadResult>> UploadAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return Result<ImageUploadResult>.Failure(ErrorType.Validation, "No file provided or file is empty.");
        }

        await using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = "cleanarchitecture/images",
            UseFilename = false,
            UniqueFilename = true
        };

        var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

        if (result.Error != null)
        {
            return Result<ImageUploadResult>.Failure(ErrorType.Failure, result.Error.Message);
        }

        var image = new Image(
            result.PublicId,
            result.Url?.ToString() ?? string.Empty,
            result.SecureUrl?.ToString() ?? string.Empty);

        await _context.Images.AddAsync(image, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<ImageUploadResult>.Success(new ImageUploadResult(
            image.Id,
            result.PublicId,
            result.SecureUrl?.ToString() ?? string.Empty));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var image = await _context.Images.FindAsync([id], cancellationToken);

        if (image is null)
        {
            return Result.Failure(ErrorType.NotFound, $"Image with id '{id}' was not found.");
        }

        var deleteParams = new DeletionParams(image.PublicId);
        var result = await _cloudinary.DestroyAsync(deleteParams);

        if (result.Error != null)
        {
            return Result.Failure(ErrorType.Failure, result.Error.Message);
        }

        image.MarkDeleted();
        _context.Images.Update(image);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success("Image deleted successfully.");
    }

    public async Task<Result> UpdateEntityIdAsync(Guid imageId, Guid entityId, CancellationToken cancellationToken = default)
    {
        var image = await _context.Images.FindAsync([imageId], cancellationToken);

        if (image is null)
        {
            return Result.Failure(ErrorType.NotFound, $"Image with id '{imageId}' was not found.");
        }

        image.SetEntityId(entityId);

        _context.Images.Update(image);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
