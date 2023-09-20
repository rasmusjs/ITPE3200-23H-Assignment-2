using forum.DAL;
using Microsoft.EntityFrameworkCore;
using forum.Models;
using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.Identity;


var builder = WebApplication.CreateBuilder(args);
//ForumDbContextFactory forumDbContextFactory = new();


// TODO: FIX REMOVE THIS LINE
builder.Services.AddDbContext<ForumDbContext>(options => options.UseSqlite("Data Source=ForumDatabase.db"));

// TODO: ADD THIS LINE
/*var connectionString = builder.Configuration.GetConnectionString("ForumDbContextConnection") ??
                       throw new InvalidOperationException("Connection string 'ForumDbContextConnection' not found.");
                       */

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ForumDbContext>(options =>
{
    options.UseSqlite(
        builder.Configuration["ConnectionStrings:ForumDbContextConnection"]);
});

/*builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ForumDbContext>();*/

/*builder.Services.AddDefaultIdentity<IdentityUser>()
    .AddEntityFrameworkStores<ForumDbContext>();*/

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequiredUniqueChars = 6;

        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(60);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // User settings
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ForumDbContext>().AddDefaultUI() // Need to add this for the login page to work https://itecnote.com/tecnote/r-unable-to-resolve-service-for-type-iemailsender-while-attempting-to-activate-registermodel/
    .AddDefaultTokenProviders();

//Taken from lecture, see https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-7.0 for more info

builder.Services.AddRazorPages();
builder.Services.AddSession();

builder.Services.AddScoped<IForumRepository<User>, ForumRepository<User>>();
builder.Services.AddScoped<IForumRepository<Post>, ForumRepository<Post>>();
builder.Services.AddScoped<IForumRepository<Category>, ForumRepository<Category>>();
builder.Services.AddScoped<IForumRepository<Tag>, ForumRepository<Tag>>();
builder.Services.AddScoped<IForumRepository<Comment>, ForumRepository<Comment>>();


/*
var loggerConfiguration = new LoggerConfiguration()
    .MinimumLevel.Information() // levels: Trace< Information < Warning < Error < Fatal
    .WriteTo.File($"Logs/app_{DateTime.Now:yyyy.MM.dd-HHmm_ss)}.log");

loggerConfiguration.Filter.ByExcluding(e => e.Properties.TryGetValue("SourceContext", out var value) &&
                                            e.Level == LogEventLevel.Information &&
                                            e.MessageTemplate.Text.Contains("Executed DbCommand"));

var logger = loggerConfiguration.CreateLogger();
builder.Logging.AddSerilog(logger);
*/


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

app.UseStaticFiles(); // for adding middleware
app.UseSession();

builder.Services.AddDistributedMemoryCache();

/*
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".AdventureWorks.Session";
    options.IdleTimeout = TimeSpan.FromSeconds(1800); // 30 minutes
    options.Cookie.IsEssential = true;
});*/

//Taken from lecture, see https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-7.0&viewFallbackFrom=aspnetcore-3.0 for more info

app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultControllerRoute();
app.MapRazorPages();
app.Run();