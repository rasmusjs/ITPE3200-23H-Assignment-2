using Microsoft.EntityFrameworkCore;
using forum.Models;

namespace forum.DAL;

public static class DbInit
{
    public static void Seed(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();
        ForumDbContext context = serviceScope.ServiceProvider.GetRequiredService<ForumDbContext>();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

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

        if (!context.Posts.Any())
        {
            var postsList = new List<Post>()
            {
                new()
                {
                    Title = "First post",
                    Content = "This is the first post",
                    DateCreated = DateTime.Now,
                    DateLastEdited = DateTime.Now,
                    UserId = 1,
                    CategoryId = 1,
                    Tags = new List<Tag>()
                    {
                        new()
                        {
                            Name = "Java"
                        },
                        new()
                        {
                            Name = "C#"
                        }
                    }
                },
                new()
                {
                    Title = "Second post",
                    Content = "This is the second post",
                    DateCreated = DateTime.Now,
                    DateLastEdited = DateTime.Now,
                    UserId = 2,
                    CategoryId = 2,
                    Tags = new List<Tag>()
                    {
                        new()
                        {
                            Name = "PowerShell"
                        },
                        new()
                        {
                            Name = "Windows"
                        }
                    }
                },
                new()
                {
                    Title = "Third post",
                    Content = "This is the third post",
                    DateCreated = DateTime.Now,
                    DateLastEdited = DateTime.Now,
                    UserId = 3,
                    CategoryId = 3,
                    Tags = new List<Tag>()
                    {
                        new()
                        {
                            Name = "Java"
                        },
                        new()
                        {
                            Name = "Linux"
                        }
                    }
                }
            };
            /*var mapTags = new Dictionary<string, Tag>();
            foreach (var tag in postsList.SelectMany(post => post.Tags))
            {
                if (!mapTags.ContainsKey(tag.Name))
                {
                    mapTags.Add(tag.Name, tag);
                }
                else
                {
                    tag.TagId = mapTags[tag.Name].TagId;
                }
            }*/

            context.AddRange(postsList);
            //context.AddRange(mapTags);
            context.SaveChanges();


            Console.WriteLine("Temp posts added");
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