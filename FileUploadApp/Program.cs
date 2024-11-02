using FileUploadApp.Extensions;
using FileUploadApp.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddConfig(builder.Configuration)
    .AddMyDependencyGroup();

var settings = builder.Configuration.GetSection("FileUploadSettings").Get<FileUploadSettings>();

var app = builder.Build();

// Register error handlin middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

// Use custom middleware only on specified paths
app.UseWhen(context => context.Request.Path.StartsWithSegments(settings.UploadEndpointPath, StringComparison.OrdinalIgnoreCase),
    appBuilder => appBuilder.UseMiddleware<FileValidationMiddleware>());

// Configure the HTTP request pipeline.
app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=FileUpload}/{id?}");

app.MapControllers();

app.Run();
