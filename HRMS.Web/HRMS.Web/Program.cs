using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using HRMS.Web.BusinessLayer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using WebMarkupMin.AspNetCore8;

var builder = WebApplication.CreateBuilder(args);

//Logging
builder.Host.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});
// Add services to the container.
builder.Services.AddControllersWithViews();

#region LanguageService
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddMvcCore()
    .AddViewLocalization();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new List<CultureInfo>
    {
        new CultureInfo("en-US"),
        new CultureInfo("ar-EG")
    };
    options.DefaultRequestCulture = new RequestCulture(culture: "ar", uiCulture: "ar");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders.Insert(0, new QueryStringRequestCultureProvider());
});

builder.Services.AddWebMarkupMin(
       options =>
       {
           options.AllowMinificationInDevelopmentEnvironment = true;
           options.AllowCompressionInDevelopmentEnvironment = true;
       })
       .AddHtmlMinification(
           options =>
           {
               options.MinificationSettings.RemoveRedundantAttributes = true;
               options.MinificationSettings.RemoveHttpProtocolFromAttributes = true;
               options.MinificationSettings.RemoveHttpsProtocolFromAttributes = true;
           })
       .AddHttpCompression();
#endregion LanguageService

//builder.Services.AddNotyf(config => { config.DurationInSeconds = 10; config.IsDismissable = true; config.Position = NotyfPosition.BottomRight; });

builder.Services.AddDistributedMemoryCache();

builder.Services.AddAuthentication();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(999999);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddMvcCore()
    .SetCompatibilityVersion(CompatibilityVersion.Latest)
        .AddDataAnnotations()
        .AddCors();

builder.Services.AddSingleton<IBusinessLayer, BusinessLayer>();
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // This lambda determines whether user consent for non-essential cookies is needed for a given request.  
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
//builder.Services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

// Enable runtime compilation of Razor views
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();


var app = builder.Build();

var loggerFactory = app.Services.GetService<ILoggerFactory>();
var path = Directory.GetCurrentDirectory();
//loggerFactory.AddFile($"{path}\\Logs\\Log.txt");

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();

//Required using WebMarkupMin.AspNetCore8;
//app.UseWebMarkupMin();

app.UseStaticFiles();
app.UseSession();
//app.UseMvc();
app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "Area",
pattern: "{area:exists}/{controller=DashBoard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
