using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Helpers;
namespace RaceDay.Api.Controllers;
[ApiController]
[Route("api/events/{eventId:int}/banner")]
[SessionAuthorize(Roles = RoleNames.Organiser)]
public class EventBannersController(RaceDayContext db, IConfiguration config) : RaceDayControllerBase
{
    [HttpPost]
    [RequestSizeLimit(5 * 1024 * 1024 + 64 * 1024)]
    public async Task<IActionResult> Upload(int eventId, IFormFile file)
    {
        var ev = await db.Events.FirstOrDefaultAsync(x => x.EventId == eventId);
        if (ev == null) return NotFoundMessage("Event not found.");
        if (ev.OrganiserId != CurrentUserId) return Forbidden("You can only upload banners for events that you created.");
        if (file == null || file.Length == 0) return BadRequest(new { message = "Choose an image to upload." });
        var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowed.Contains(file.ContentType.ToLowerInvariant())) return BadRequest(new { message = "Only JPG, PNG and WEBP images are allowed." });
        if (file.Length > 5 * 1024 * 1024) return BadRequest(new { message = "The banner must be 5 MB or smaller." });
        var connection = config["AzureBlobStorage:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connection)) return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Azure Blob Storage is not configured. Set AzureBlobStorage__ConnectionString on the API." });
        var blobs = new BlobServiceClient(connection);
        var containerName = config["AzureBlobStorage:ContainerName"] ?? "raceday-event-banners";
        var container = blobs.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.Blob);
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var blob = container.GetBlobClient($"event-{eventId}-{Guid.NewGuid():N}{extension}");
        await using var stream = file.OpenReadStream();
        await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
        ev.BannerImageUrl = blob.Uri.ToString();
        await db.SaveChangesAsync();
        return Ok(new { bannerImageUrl = ev.BannerImageUrl });
    }
}
