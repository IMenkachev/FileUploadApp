namespace FileUploadApp.Extensions
{
    using FileUploadApp.Contracts;
    using FileUploadApp.Services;
    using Microsoft.AspNetCore.Server.Kestrel.Core;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using System.Text.Json;

    public static class ServiceExtensions
    {
        public static IServiceCollection AddConfig(
             this IServiceCollection services, IConfiguration config)
        {
            services.Configure<FileUploadSettings>(
                config.GetSection("FileUploadSettings"));

            // Retrieve the settings
            var settings = services.BuildServiceProvider()
                .GetRequiredService<IOptions<FileUploadSettings>>().Value;

            // Kestrel's max request body size 50 MB
            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = settings.KestrelMaxRequestBodySize;
            });

            services.AddControllers()
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                        options.JsonSerializerOptions.WriteIndented = true;
                    });


            return services;
        }

        public static IServiceCollection AddMyDependencyGroup(
             this IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddSingleton<IFileStorageService, FileStorageService>();
            services.AddSingleton<IXmlToJsonConverter, XmlToJsonConverter>();
            services.AddSingleton<IRetryPolicy, RetryPolicy>();

            return services;
        }
    }
}
