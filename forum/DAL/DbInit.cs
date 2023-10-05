using forum.Models;

namespace forum.DAL;

// Database initializer - Seeds the database with default content if there are no content in DB
public static class DbInit
{
    public static void Seed(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();
        var context = serviceScope.ServiceProvider.GetRequiredService<ForumDbContext>();

        //context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        if (!context.Categories.Any())
        {
            var categoriesList = new List<Category>
            {
                new()
                {
                    Name = "General"
                },
                new()
                {
                    Name = "News"
                },
                new()
                {
                    Name = "Sports"
                },
                new()
                {
                    Name = "Politics"
                }
            };
            context.AddRange(categoriesList);
            context.SaveChanges();
            Console.WriteLine("Categories added");
        }

        if (!context.Tags.Any())
        {
            var tagsList = new List<Tag>
            {
                new()
                {
                    Name = "Java"
                },
                new()
                {
                    Name = "C#"
                },
                new()
                {
                    Name = "Python"
                },
                new()
                {
                    Name = "JavaScript"
                },
                new()
                {
                    Name = "Web"
                },
                new()
                {
                    Name = "Beginner"
                }
            };
            context.AddRange(tagsList);
            context.SaveChanges();
            Console.WriteLine("Tags added");
        }

        if (!context.Users.Any())
        {
            var userList = new List<ApplicationUser>
            {
                new()
                {
                    UserName = "CoolGuy",
                    PasswordHash = "AQAAAAEAACcQAA",
                    CreationDate = DateTime.Now
                },
                new()
                {
                    UserName = "HackerMan",
                    PasswordHash = "AQAAAAEAACcQAA",
                    CreationDate = DateTime.Now
                },
                new()
                {
                    UserName = "TheBoss",
                    PasswordHash = "AQAAAAEAACcQAA",
                    CreationDate = DateTime.Now
                }
            };
            context.AddRange(userList);
            context.SaveChanges();
            Console.WriteLine("Users added");
        }

        // Fetches all users from the database, to be used when seeding posts
        var addedUsers = context.Users.ToList();
        var tags = context.Tags.ToArray();

        if (!context.Posts.Any() && addedUsers.Count > 0) // If there are no posts in the database and there are users
        {
            var postsList = new List<Post>
            {
                new()
                {
                    Title = "\ud83d\udcdc Why JavaScript should be considered a gift from GOD! \ud83d\udcdc",
                    Content =
                        "Ladies and gentlemen, gather 'round, for today, we embark on a divine journey through the ethereal realms of JavaScript! \ud83d\ude80\ud83c\udf0c\n\n##  **Unleash the Versatility**\nBehold, for JavaScript is the omnipotent chameleon of coding languages! It dances seamlessly not only in the sacred halls of browsers but also dons the crown of servers (praise be to Node.js) and blesses mobile apps with its touch (hail React Native)!\n\n##  **A Cosmic Force of Popularity**\nIt is not just a language; it's a celestial phenomenon! JavaScript's ubiquity transcends the boundaries of realms, making it one of the most widely-used languages, embraced by mortals and tech gods alike.\n\n##  **A Sacred Evolution**\nJavaScript is on an eternal quest for perfection. ES6, ES7, ES8... it evolves faster than the speed of light, adapting to the celestial needs of modern development.\n\n##  **The Art of Interactivity**\nWitness the magic as JavaScript breathes life into the lifeless! It grants websites the gift of interactivity and dynamism, ensnaring users in a spellbinding trance.\n\n##  **A Cosmic Job Market**\nBy embracing the holy scriptures of JavaScript, you open the gates to an abundance of job opportunities in the ever-expanding tech universe. Devs, rejoice! \ud83d\ude4c\ud83c\udf20\n\n##  **The Fellowship of Community**\nJavaScript's community is not just a community; it's a sacred brotherhood! On StackOverflow, GitHub, and countless other altars, the faithful gather to bestow wisdom upon the seeking souls.\n\nYes, it has its quirks (the enigmatic \"undefined\" and the mystical \"NaN\"), but what godly creation doesn't have its mysteries? \ud83e\udd37\u200d\u2642\ufe0f\ud83c\udf0c\n\nSo, let us kneel before JavaScript, the divine thread that weaves the very fabric of the web, a celestial gift that keeps on giving to us humble developers! \ud83d\ude4f\n\nDo you too believe in the divinity of JavaScript or have celestial tales to share? \ud83c\udf20\ud83d\udd2e #JavaScriptGift #DevotionToCode\n",
                    DateCreated = DateTime.Now,
                    DateLastEdited = DateTime.Now,
                    UserId = addedUsers[0].Id,
                    CategoryId = 1,
                    Tags = new List<Tag>
                    {
                        tags.First(t => t.Name == "JavaScript"),
                        new()
                        {
                            Name = "Science"
                        }
                    }
                },
                new()
                {
                    Title = "Second post",
                    Content = "This is the second post",
                    DateCreated = DateTime.Now,
                    DateLastEdited = DateTime.Now,
                    UserId = addedUsers[1].Id,
                    CategoryId = 2,
                    Tags = new List<Tag>
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
                    UserId = addedUsers[2].Id,
                    CategoryId = 3,
                    Tags = new List<Tag>
                    {
                        tags.First(t => t.Name == "JavaScript"),
                        tags.First(t => t.Name == "Web"),
                        tags.First(t => t.Name == "Beginner")
                    }
                }
            };
            context.AddRange(postsList);
            context.SaveChanges();


            Console.WriteLine("Temp posts added");
        }

        //        if (!context.Comments.Any() && !context.Posts.Any() && addedUsers.Count > 0)
        if (!context.Comments.Any())
        {
            // Create some top-level comments
            var comment1 = new Comment
            {
                Content = "This post is soo cool!",
                DateCreated = DateTime.Now,
                UserId = addedUsers[1].Id,
                PostId = 1,
                TotalLikes = 420
            };
            var comment2 = new Comment
            {
                Content = "I hate this post",
                DateCreated = DateTime.Now,
                UserId = addedUsers[2].Id,
                PostId = 1,
                TotalLikes = 69
            };

            // Add comments to the database
            context.Comments.AddRange(comment1, comment2);
            context.SaveChanges();

            // Add replies to comments
            var reply1 = new Comment
            {
                Content = "This is sooo right!!!",
                DateCreated = DateTime.Now,
                UserId = addedUsers[0].Id,
                PostId = 1,
                ParentCommentId = 1 // Set the parent comment ID
            };
            var reply2 = new Comment
            {
                Content = "You are stupid",
                DateCreated = DateTime.Now,
                UserId = addedUsers[1].Id,
                PostId = 1,
                ParentCommentId = 1 // Set the parent comment ID
            };
            var reply3 = new Comment
            {
                Content = "No, you are stupid!",
                DateCreated = DateTime.Now,
                UserId = addedUsers[1].Id,
                PostId = 1,
                ParentCommentId = 2 // Set the parent comment ID
            };

            context.Comments.AddRange(reply1, reply2, reply3);
            context.SaveChanges();

            // Adds reply to "reply1".
            // Gets stored in the database but does not seem to create a relation.
            // Also does not work when trying to add a reply in the website
            var reply1Reply1 = new Comment
            {
                Content = "Actually, this is very wrong!",
                DateCreated = DateTime.Now,
                UserId = addedUsers[2].Id,
                PostId = 1,
                ParentCommentId = 3 // Set the parent comment ID
            };
            context.Comments.AddRange(reply1Reply1);
            context.SaveChanges();

            Console.WriteLine("Comments added");
        }
    }
}