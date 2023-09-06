var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseStaticFiles(); // for adding middleware

app.MapDefaultControllerRoute();

app.Run();