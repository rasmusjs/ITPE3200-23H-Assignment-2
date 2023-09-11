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
            context.AddRange(postsList);
            context.SaveChanges();


            Console.WriteLine("Temp posts added");
        }

        /*if (!context.Comments.Any())
        {
            var commentsList = new List<Comment>()
            {
                new()
                {
                    CommentId = 1,
                    Content = "This is a great post!",
                    Likes = 10,
                    DateCreated = DateTime.Now.AddHours(-1),
                    PostId = 1,
                    UserId = 2,
                },

                new()
                {
                    CommentId = 2,
                    Content = "I agree with you!",
                    Likes = 5,
                    DateCreated = DateTime.Now.AddMinutes(-30),
                    PostId = 1,
                    UserId = 3,
                },

                new()

                {
                    CommentId = 3,
                    Content = "Thanks for sharing!",
                    Likes = 3,
                    DateCreated = DateTime.Now.AddMinutes(-15),
                    PostId = 1,
                    UserId = 4,
                },

                new()

                {
                    CommentId = 4,
                    Content = "I have a question...",
                    DateCreated = DateTime.Now.AddMinutes(-10),
                    PostId = 1,
                    UserId = 5,
                    ParentCommentId = 1,
                },

                new()

                {
                    CommentId = 5,
                    Content = "Sure, what's your question?",
                    DateCreated = DateTime.Now.AddMinutes(-5),
                    PostId = 1,
                    UserId = 2,
                    ParentCommentId = 1,
                }
            };
            context.AddRange(commentsList);
            context.SaveChanges();
        }*/
    }
}