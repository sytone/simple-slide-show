using Serilog;
using SimpleSlideShow.Models;
using SimpleSlideShow.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Configure Kestrel to use Urls from appsettings
var urls = builder.Configuration["Urls"];
if (!string.IsNullOrEmpty(urls))
{
    builder.WebHost.UseUrls(urls);
}

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
	loggerConfiguration
		.ReadFrom.Configuration(context.Configuration)
		.ReadFrom.Services(services)
		.Enrich.FromLogContext();
});

builder.Services.AddControllers();
builder.Services.Configure<SlideShowSettings>(builder.Configuration.GetSection("SlideShowSettings"));
builder.Services.AddSingleton<ImageService>();
builder.Services.AddSingleton<SlideShowService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.MapFallbackToFile("index.html");

var imageService = app.Services.GetRequiredService<ImageService>();
imageService.StartMonitoring();

app.Run();
