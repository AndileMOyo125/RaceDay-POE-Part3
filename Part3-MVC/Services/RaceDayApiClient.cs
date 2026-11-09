using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
namespace RaceDay.Mvc.Services;
public class RaceDayApiClient(IHttpClientFactory factory)
{
    public async Task<HttpResponseMessage> Send(HttpMethod method, string path, string? cookie = null, object? body = null, IFormFile? file = null)
    {
        var client = factory.CreateClient("RaceDayApi");
        using var req = new HttpRequestMessage(method, path.TrimStart('/'));
        if (!string.IsNullOrWhiteSpace(cookie)) req.Headers.TryAddWithoutValidation("Cookie", cookie);
        if (file != null) { var form = new MultipartFormDataContent(); var stream = file.OpenReadStream(); var part = new StreamContent(stream); part.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType); form.Add(part, "file", Path.GetFileName(file.FileName)); req.Content = form; }
        else if (body != null) req.Content = JsonContent.Create(body);
        return await client.SendAsync(req);
    }
    public static async Task<string> Error(HttpResponseMessage response)
    {
        var raw = await response.Content.ReadAsStringAsync();
        try
        {
            using var document = JsonDocument.Parse(raw);
            var root = document.RootElement;
            if (root.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String)
            {
                return message.GetString() ?? "Request failed.";
            }

            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                var validationMessages = errors.EnumerateObject()
                    .SelectMany(error => error.Value.ValueKind == JsonValueKind.Array
                        ? error.Value.EnumerateArray().Where(value => value.ValueKind == JsonValueKind.String).Select(value => value.GetString())
                        : Enumerable.Empty<string>())
                    .Where(value => !string.IsNullOrWhiteSpace(value));
                var summary = string.Join(" ", validationMessages);
                if (!string.IsNullOrWhiteSpace(summary)) return summary;
            }

            if (root.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
            {
                return title.GetString() ?? "Please check your details.";
            }
        }
        catch (JsonException)
        {
        }

        return string.IsNullOrWhiteSpace(raw) ? "Request failed. Please try again." : raw;
    }
}
