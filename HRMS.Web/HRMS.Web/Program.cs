using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using HRMS.Web.AttendanceScheduler;
using HRMS.Web.BusinessLayer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quartz.Impl;
using Quartz.Spi;
using Quartz;
using System.Globalization;
using WebMarkupMin.AspNetCore8;
using HRMS.Web.BusinessLayer.S3;
using DinkToPdf.Contracts;
using DinkToPdf;

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
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
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
builder.Services.AddSingleton<IS3Service, S3Service>();
builder.Services.AddSingleton<IBusinessLayer, BusinessLayer>();
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // This lambda determines whether user consent for non-essential cookies is needed for a given request.  
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Index"; // Default
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                // If not an API request, redirect to your error page
                if (!context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.Redirect("/Home/ErrorPage");
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                }
                return Task.CompletedTask;
            }
        };
    });


//builder.Services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

// Enable runtime compilation of Razor views
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();



//Scheduker
IServiceCollection service = builder.Services.AddHostedService<QuartzHostedService>();
builder.Services.AddSingleton<QuartzJobRunner>();
 builder.Services.AddSingleton<IJobFactory, JobFactory>();
builder.Services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
builder.Services.TryAddTransient<AttendanceReminderJob>();
builder.Services.AddSingleton(new JobSchedule(
    jobType: typeof(AttendanceReminderJob),
    cronExpression: "0 32 02 * * ?"));


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
