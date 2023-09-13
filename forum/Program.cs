using forum.DAL;
using Microsoft.EntityFrameworkCore;
using forum.Models;
using Serilog;
using Serilog.Events;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ForumDbContext>(options =>
{
    options.UseSqlite(
        builder.Configuration["ConnectionStrings:PostDbContextConnection"]);
});
builder.Services.AddScoped<IForumRepository<Post>, ForumRepository<Post>>();
builder.Services.AddScoped<IForumRepository<Category>, ForumRepository<Category>>();
builder.Services.AddScoped<IForumRepository<Tag>, ForumRepository<Tag>>();
builder.Services.AddScoped<IForumRepository<Comment>, ForumRepository<Comment>>();


var loggerConfiguration = new LoggerConfiguration()
    .MinimumLevel.Information() // levels: Trace< Information < Warning < Erorr < Fatal
    .WriteTo.File($"Logs/app_{DateTime.Now:yyyyMMdd_HHmmss}.log");

loggerConfiguration.Filter.ByExcluding(e => e.Properties.TryGetValue("SourceContext", out var value) &&
                                            e.Level == LogEventLevel.Information &&
                                            e.MessageTemplate.Text.Contains("Executed DbCommand"));

var logger = loggerConfiguration.CreateLogger();
builder.Logging.AddSerilog(logger);
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

app.MapDefaultControllerRoute();

app.Run();