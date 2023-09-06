using Microsoft.EntityFrameworkCore;
using forum.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<PostDbContext>(options =>
{
    options.UseSqlite(
        builder.Configuration["ConnectionStrings:PostDbContextConnection"]);
});

var app = builder.Build();

app.UseStaticFiles(); // for adding middleware

app.MapDefaultControllerRoute();

app.Run();