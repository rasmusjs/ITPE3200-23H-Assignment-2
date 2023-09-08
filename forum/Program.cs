using forum.Controllers;
using forum.DAL;
using Microsoft.EntityFrameworkCore;
using forum.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ForumDbContext>(options =>
{
    options.UseSqlite(
        builder.Configuration["ConnectionStrings:PostDbContextConnection"]);
});
builder.Services.AddScoped<IForumRepository<Post>, ForumRepository<Post>>();

var app = builder.Build();

app.UseStaticFiles(); // for adding middleware

app.MapDefaultControllerRoute();

app.Run();