using Blazored.LocalStorage;
using Microsoft.AspNetCore.ResponseCompression;
using AIChatBase.Components;
using AIChatBase.Data.Services;
using OSSEncryption;
using System.IO.Compression;
using AIChatBase.CustomComponents.AI;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(options =>
{
    options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
});

#region 健康检查
builder.Services.AddHealthChecks();
#endregion

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDevExpressBlazor(options => {
    options.BootstrapVersion = DevExpress.Blazor.BootstrapVersion.v5;
    options.SizeMode = DevExpress.Blazor.SizeMode.Medium;
});
builder.Services.AddMvc();

builder.Services.AddControllers(); // 添加控制器支持
builder.Services.AddAuthenticationCore();
builder.Services.AddHttpClient();
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddLocalization();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddCustomServices(builder.Configuration);

//添加AI服务注入
builder.Services.AddAIServices(builder.Configuration);

// 响应压缩
builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    //针对指定的MimeType来使用压缩策略
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "image/svg+xml" });
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = (CompressionLevel)4; // 4 or 5 is OK 4/5压缩损耗最低
});

//加解密注册
var ProtectionKey = builder.Configuration.GetSection("ProtectionKey").Value;
var ProtectionIV = builder.Configuration.GetSection("ProtectionIV").Value;
builder.Services.AddTransient(ec => new EncryptionService(new OSSEncryption.KeyInfo(ProtectionKey, ProtectionIV)));
builder.WebHost.UseWebRoot("wwwroot");
builder.WebHost.UseStaticWebAssets();

builder.Services.AddSignalR(options =>
{
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(300); // 设置客户端超时时间，5分钟 ，单位为秒
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();

app.UseAuthorization();

app.UseRequestLocalization("zh-Hans");

app.UseAntiforgery();

// 配置 CSP 头
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Content-Security-Policy", "frame-ancestors *"); // 允许所有来源的 iframe 内嵌
    await next();
});

// 注册自定义中间件
//app.UseMiddleware<LoggingMiddleware>();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();