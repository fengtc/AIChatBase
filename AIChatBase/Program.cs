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

#region �������
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

builder.Services.AddControllers(); // ��ӿ�����֧��
builder.Services.AddAuthenticationCore();
builder.Services.AddHttpClient();
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddLocalization();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddCustomServices(builder.Configuration);

//���AI����ע��
builder.Services.AddAIServices(builder.Configuration);

// ��Ӧѹ��
builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    //���ָ����MimeType��ʹ��ѹ������
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "image/svg+xml" });
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = (CompressionLevel)4; // 4 or 5 is OK 4/5ѹ��������
});

//�ӽ���ע��
var ProtectionKey = builder.Configuration.GetSection("ProtectionKey").Value;
var ProtectionIV = builder.Configuration.GetSection("ProtectionIV").Value;
builder.Services.AddTransient(ec => new EncryptionService(new OSSEncryption.KeyInfo(ProtectionKey, ProtectionIV)));
builder.WebHost.UseWebRoot("wwwroot");
builder.WebHost.UseStaticWebAssets();

builder.Services.AddSignalR(options =>
{
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(300); // ���ÿͻ��˳�ʱʱ�䣬5���� ����λΪ��
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

// ���� CSP ͷ
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Content-Security-Policy", "frame-ancestors *"); // ����������Դ�� iframe ��Ƕ
    await next();
});

// ע���Զ����м��
//app.UseMiddleware<LoggingMiddleware>();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();