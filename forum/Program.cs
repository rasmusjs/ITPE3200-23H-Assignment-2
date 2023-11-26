using forum.DAL;
using forum.Models;
using Jdenticon.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ForumDbContext>(options => options.UseSqlite("Data Source=ForumDatabase.db"));
builder.Services.AddControllersWithViews();
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Password settings
        /*options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequiredUniqueChars = 6;*/
        
        // Using regex instead of the above
        // Source for regex https://stackoverflow.com/questions/8699033/password-dataannotation-in-asp-net-mvc-3

        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(60);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // User settings
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ForumDbContext>()
    .AddDefaultUI() // Need to add this for the login page to work https://itecnote.com/tecnote/r-unable-to-resolve-service-for-type-iemailsender-while-attempting-to-activate-registermodel/
    .AddDefaultTokenProviders();

//Taken from lecture, see https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-7.0 for more info

builder.Services.AddRazorPages();
builder.Services.AddSession();

// Add repositories to the container.
builder.Services.AddScoped<IForumRepository<ApplicationUser>, ForumRepository<ApplicationUser>>();
builder.Services.AddScoped<IForumRepository<Post>, ForumRepository<Post>>();
builder.Services.AddScoped<IForumRepository<Category>, ForumRepository<Category>>();
builder.Services.AddScoped<IForumRepository<Tag>, ForumRepository<Tag>>();
builder.Services.AddScoped<IForumRepository<Comment>, ForumRepository<Comment>>();

var loggerConfiguration = new LoggerConfiguration()
    .MinimumLevel.Information() // levels: Trace< Information < Warning < Error < Fatal
    .WriteTo.File($"Logs/app_{DateTime.Now:yyyy.MM.dd-HHmm_ss)}.log");

loggerConfiguration.Filter.ByExcluding(e => e.Properties.TryGetValue("SourceContext", out var value) &&
                                            e.Level == LogEventLevel.Information &&
                                            e.MessageTemplate.Text.Contains("Executed DbCommand"));

// Needed to fix fail: Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware[1]
// Source https://stackoverflow.com/questions/59199593/net-core-3-0-possible-object-cycle-was-detected-which-is-not-supported
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    );


// Add Serilog to the container.
var logger = loggerConfiguration.CreateLogger();
builder.Logging.AddSerilog(logger);
builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.Name = "BracketBros.Session";
    //options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // 30 minutes
    /*options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";*/
    options.SlidingExpiration = true;
});
//From https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-7.0&tabs=visual-studio

// Configure CORS to allow any origin
/*builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "OpenCorsPolicy",
        policy =>
        {
            policy.AllowAnyOrigin() // Use with caution, allows requests from any source
                .AllowCredentials()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});*/

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOriginFromVue",
        policy =>
            policy.WithOrigins("http://localhost:3000")
                .AllowCredentials()
                .AllowAnyHeader()
                .AllowAnyMethod());
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    DbInit.Seed(app); // for adding seed data
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

// Use the Open CORS policy
//app.UseCors("OpenCorsPolicy");
app.UseCors("AllowOriginFromVue");

app.UseStaticFiles(); // for adding middleware
app.UseSession();

//Taken from lecture, see https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-7.0&viewFallbackFrom=aspnetcore-3.0 for more info
app.UseJdenticon(); // For using for generating identicons. //https://jdenticon.com/#quick-start-asp-net-core
app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultControllerRoute();
app.MapRazorPages();
app.Run();