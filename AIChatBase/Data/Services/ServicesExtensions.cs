using Microsoft.AspNetCore.Components.Authorization;
using AIChatBase.Authentication;
using AIChatBase.Data.Models.Customs;
using AIChatBase.Data.Services.Memory;
using AIChatBase.Data.Services.Navigations;
using AIChatBase.Data.Services.OssData;
using AIChatBase.Data.Services.OssId;
using Polly;
using Polly.Extensions.Http;
using AIChatBase.Data.Services.QYWX;
namespace AIChatBase.Data.Services
{
    public static class ServicesExtensions
    {
        public static void AddCustomServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient(nameof(OssDataService), options =>
            {
                options.BaseAddress = new Uri(configuration["OssDataServiceApi"]);
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                return httpClientHandler;
            }).AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(2, _ => TimeSpan.FromSeconds(3)));

            services.AddHttpClient(nameof(OssIdService), options =>
            {
                options.BaseAddress = new Uri(configuration["OssIdServiceApi"]);
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                return httpClientHandler;
            })
            .AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(2, _ => TimeSpan.FromSeconds(3)));

            services.AddHttpClient(nameof(QYWXService), options =>
            {
                options.BaseAddress = new Uri("https://qyapi.weixin.qq.com");
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                return httpClientHandler;
            }).AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(2, _ => TimeSpan.FromSeconds(3)));


            services.AddTransient<IOssDataService, OssDataService>();
            services.AddTransient<IOssIdService, OssIdService>();
            services.AddTransient<IQYWXService, QYWXService>();
            services.AddSingleton<IBxMemoryService, BxMemoryService>();
            services.AddTransient<INavigationManagerService, NavigationManagerService>();
            services.AddScoped<UserInitialState>();
            services.AddScoped<CustomAuthenticationService>();
            services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
        }
    }
}
