using Microsoft.AspNetCore.Mvc;using RaceDay.Mvc.Models;using RaceDay.Mvc.Services;using System.Net.Http.Json;
namespace RaceDay.Mvc.Controllers;
public class ParticipantController(RaceDayApiClient api):Controller
{
 string? Cookie=>HttpContext.Session.GetString("ApiCookie");
 public async Task<IActionResult> MyEvents(){if(Cookie==null)return RedirectToAction("Login","Account");using var r=await api.Send(HttpMethod.Get,"api/users/me/enrolments",Cookie);if(!r.IsSuccessStatusCode){TempData["Error"]=await RaceDayApiClient.Error(r);return View(new List<EnrolmentDto>());}return View(await r.Content.ReadFromJsonAsync<List<EnrolmentDto>>()??new());}
 public async Task<IActionResult> MyResults(){if(Cookie==null)return RedirectToAction("Login","Account");using var r=await api.Send(HttpMethod.Get,"api/users/me/results",Cookie);if(!r.IsSuccessStatusCode){TempData["Error"]=await RaceDayApiClient.Error(r);return View(new List<ResultDto>());}return View(await r.Content.ReadFromJsonAsync<List<ResultDto>>()??new());}
}
