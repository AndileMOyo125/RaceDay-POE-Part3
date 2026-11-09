using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Helpers;
namespace RaceDay.Api.Controllers;
[ApiController]
[Route("api/users/me/profile-image")]
[SessionAuthorize]
public class ProfileImageController(RaceDayContext db, IConfiguration config) : RaceDayControllerBase
{
    [HttpPost, RequestSizeLimit(5 * 1024 * 1024 + 64 * 1024)]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.UserId == CurrentUserId);
        if (user == null) return Unauthorized(new { message = "Please sign in again." });
        if (file == null || file.Length == 0) return BadRequest(new { message = "Choose a profile image." });
        var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowed.Contains(file.ContentType.ToLowerInvariant()) || file.Length > 5 * 1024 * 1024) return BadRequest(new { message = "Use a JPG, PNG or WEBP image up to 5 MB." });
        var connection = config["AzureBlobStorage:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connection)) return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Azure Blob Storage is not configured. Set AzureBlobStorage__ConnectionString on the API." });
        var blobs = new BlobServiceClient(connection);
        var container = blobs.GetBlobContainerClient(config["AzureBlobStorage:ProfileContainerName"] ?? "raceday-profile-images");
        await container.CreateIfNotExistsAsync(PublicAccessType.Blob);
        var blob = container.GetBlobClient($"user-{CurrentUserId}-{Guid.NewGuid():N}{Path.GetExtension(file.FileName).ToLowerInvariant()}");
        await using var stream = file.OpenReadStream();
        await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
        user.ProfileImageUrl = blob.Uri.ToString();
        await db.SaveChangesAsync();
        return Ok(new { profileImageUrl = user.ProfileImageUrl });
    }
}
