using EsCheckWeb.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();

builder.Services.AddScoped<ElasticServerStore>();
builder.Services.AddScoped<ElasticService>();
builder.Services.AddHttpClient<ElasticRawService>();

var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=EsCheck}/{action=Index}/{id?}");

app.Run();