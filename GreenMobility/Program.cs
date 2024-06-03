using AspNetCoreHero.ToastNotification;
using GreenMobility.Models;
using GreenMobility.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

#region Localizer
builder.Services.AddSingleton<LanguageService>();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddMvc().AddViewLocalization().AddDataAnnotationsLocalization(options =>
    options.DataAnnotationLocalizerProvider = (type, factory) =>
    {
        var assemblyName = new AssemblyName(typeof(SharedResource).GetTypeInfo().Assembly.FullName);
        return factory.Create(nameof(SharedResource), assemblyName.Name);
    });

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportCultures = new List<CultureInfo>
    {
        new CultureInfo("en-US"),
        new CultureInfo("vi-VN"),
    };
    options.DefaultRequestCulture = new RequestCulture(culture: "vi-VN", uiCulture: "vi-VN");
    options.SupportedCultures = supportCultures;
    options.SupportedUICultures = supportCultures;
    options.RequestCultureProviders.Insert(0, new QueryStringRequestCultureProvider());
});
#endregion
builder.Services.AddControllersWithViews()
    .AddMvcOptions(option =>
    {
        option.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    });
builder.Services.AddRazorPages()
    .AddRazorRuntimeCompilation();
builder.Services.AddDbContext<GreenMobilityContext>(options => options.UseSqlServer(
builder.Configuration.GetConnectionString("dbGreenMobility")
));

builder.Services.AddSingleton<HtmlEncoder>(
           HtmlEncoder.Create(allowedRanges: new[] { UnicodeRanges.All }));

builder.Services.AddSession();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(p =>
    {
        p.LoginPath = "/login.html";
        p.AccessDeniedPath = "/not-found.html";
        p.Cookie.Name = "AuthenticationCookie";
    });

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie("AdminCookie", options =>
    {
        options.LoginPath = "/Admin/AdminAccounts/Login";
        options.AccessDeniedPath = "/Admin/Error/AccessDenied";
        options.Cookie.Name = "AdminAuthenticationCookie";
    });


builder.Services.AddNotyf(config =>
{
    config.DurationInSeconds = 3;
    config.IsDismissable = true;
    config.Position = NotyfPosition.BottomRight;
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);
app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();