using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
using System.Net;
using System.Net.Http.Json;

namespace RaceDay.Mvc.Controllers;

public class HomeController(RaceDayApiClient api) : Controller
{
    public async Task<IActionResult> Index(string? eventType, string? search)
    {
        var cookie = HttpContext.Session.GetString("ApiCookie");
        if (cookie == null) return View();

        using var response = await api.Send(HttpMethod.Get, "api/events", cookie);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Clear();
                TempData["Error"] = "Your session has expired. Please sign in again.";
                return RedirectToAction("Login", "Account");
            }

            TempData["Error"] = await RaceDayApiClient.Error(response);
            return View("Dashboard", new List<EventDto>());
        }

        var events = await response.Content.ReadFromJsonAsync<List<EventDto>>() ?? new();
        var role = HttpContext.Session.GetString("Role");
        ViewBag.Role = role;

        if (role == "Organiser")
        {
            using var userResponse = await api.Send(HttpMethod.Get, "api/users/me", cookie);
            if (!userResponse.IsSuccessStatusCode)
            {
                TempData["Error"] = await RaceDayApiClient.Error(userResponse);
                return View("Dashboard", new List<EventDto>());
            }

            var user = await userResponse.Content.ReadFromJsonAsync<UserDto>();
            events = events.Where(e => e.OrganiserId == user?.UserId).ToList();
        }
        else
        {
            events = events.Where(e => e.EventDate >= DateTime.Now).ToList();
        }

        ViewBag.TotalEvents = events.Count;
        var dashboardEvents = events;
        var counts = new Dictionary<int, int>();
        var dashboardErrors = new List<string>();
        if (role == "Organiser")
        {
            foreach (var ev in dashboardEvents)
            {
                using var enrolmentsResponse = await api.Send(
                    HttpMethod.Get, $"api/events/{ev.EventId}/enrolments", cookie);
                if (enrolmentsResponse.IsSuccessStatusCode)
                {
                    var enrolments = await enrolmentsResponse.Content
                        .ReadFromJsonAsync<List<EnrolmentDto>>() ?? new();
                    counts[ev.EventId] = enrolments.Count;
                }
                else
                {
                    dashboardErrors.Add($"{ev.EventName}: {await RaceDayApiClient.Error(enrolmentsResponse)}");
                }
            }
        }

        if (dashboardErrors.Count > 0)
        {
            TempData["Error"] = "Some enrolment totals could not be loaded. " + string.Join(" ", dashboardErrors);
        }

        ViewBag.EnrolmentCounts = counts;
        ViewBag.UpcomingEventsCount = dashboardEvents.Count(e => e.EventDate >= DateTime.Now);
        ViewBag.TotalEnrolments = counts.Values.Sum();

        if (!string.IsNullOrWhiteSpace(eventType))
        {
            events = events.Where(e => e.EventType.Equals(eventType, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            events = events.Where(e =>
                e.EventName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                e.Location.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (role == "Organiser")
        {
            events = events.OrderByDescending(e => e.CreatedDate).Take(3).ToList();
        }

        return View("Dashboard", events);
    }

    public IActionResult Error() => View();
}
