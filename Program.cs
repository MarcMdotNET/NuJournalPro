using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NuJournalPro.Data;
using NuJournalPro.Models.Database;
using NuJournalPro.Models.Settings;
using NuJournalPro.Services;
using NuJournalPro.Services.Helpers;
using NuJournalPro.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetSection("pgsqlSettings")["pgsqlConnection"];

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<NuJournalUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddDefaultUI()
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Load custom configurations.
builder.Services.Configure<OwnerSettings>(builder.Configuration.GetSection("OwnerSettings"));
builder.Services.Configure<DefaultUserSettings>(builder.Configuration.GetSection("DefaultUserSettings"));
builder.Services.Configure<DefaultGraphics>(builder.Configuration.GetSection("DefaultGraphics"));
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.Configure<ContactUsSettings>(builder.Configuration.GetSection("ContactUsSettings"));

// Register custom services.
builder.Services.AddSingleton<IImageService, ImageService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddTransient<SetupDataService>();

var app = builder.Build();

// Get the database update with the latest migrations and create the Owner user if it doesn't already exist.
var dataService = app.Services.CreateScope().ServiceProvider.GetRequiredService<SetupDataService>();
await dataService.ManageDataAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
