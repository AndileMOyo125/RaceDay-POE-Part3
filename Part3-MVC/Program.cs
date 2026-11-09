var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o => { o.Cookie.Name = ".RaceDay.Mvc"; o.Cookie.HttpOnly = true; o.Cookie.IsEssential = true; o.IdleTimeout = TimeSpan.FromMinutes(60); });
builder.Services.AddHttpClient("RaceDayApi", c => { var url = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5080/"; c.BaseAddress = new Uri(url.EndsWith("/") ? url : url + "/"); c.Timeout = TimeSpan.FromSeconds(30); }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseCookies = false });
builder.Services.AddScoped<RaceDay.Mvc.Services.RaceDayApiClient>();
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.UseHttpsRedirection(); app.UseStaticFiles(); app.UseRouting(); app.UseSession();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();
