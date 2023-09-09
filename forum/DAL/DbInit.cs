using Microsoft.EntityFrameworkCore;
using forum.Models;

namespace forum.DAL;

public static class DbInit
{
    public static void Seed(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();
        ForumDbContext context = serviceScope.ServiceProvider.GetRequiredService<ForumDbContext>();
        //context.Database.EnsureDeleted();
        //context.Database.EnsureCreated();

        if (!context.Categories.Any())
        {
            var categoriesList = new List<Category>()
            {
                new()
                {
                    Name = "General",
                },
                new()
                {
                    Name = "News",
                },
                new()
                {
                    Name = "Sports",
                },
                new()
                {
                    Name = "Politics",
                }
            };
            context.AddRange(categoriesList);
            context.SaveChanges();
            Console.WriteLine("Categories added");
        }

        if (!context.Tags.Any())
        {
            var tagsList = new List<Tag>()
            {
                new()
                {
                    Name = "Java",
                },
                new()
                {
                    Name = "C#",
                },
                new()
                {
                    Name = "Python",
                },
                new()
                {
                    Name = "JavaScript",
                }
            };
            context.AddRange(tagsList);
            context.SaveChanges();
            Console.WriteLine("Tags added");
        }

        if (!context.Users.Any())
        {
            var usersList = new List<User>()
            {
                new()
                {
                    Username = "user123",
                    Email = "user123@example.com",
                    Password = "password123",
                    CreationDate = DateTime.Now
                },
                new()
                {
                    Username = "johndoe",
                    Email = "johndoe@example.com",
                    Password = "doe@123",
                    CreationDate = DateTime.Now
                },
                new()
                {
                    Username = "alice",
                    Email = "alice@example.com",
                    Password = "alicePass",
                    CreationDate = DateTime.Now
                }
            };

            context.AddRange(usersList);
            context.SaveChanges();
            Console.WriteLine("Temp users added");
        }

        /*
        var ordersToUpdate = context.Orders.Include(o => o.OrderItems);
        foreach (var order in ordersToUpdate)
        {
            order.TotalPrice = order.OrderItems?.Sum(oi => oi.OrderItemPrice) ?? 0;
        }
        context.SaveChanges();*/
    }
}