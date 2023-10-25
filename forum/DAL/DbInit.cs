using forum.Models;
using Microsoft.AspNetCore.Identity;

namespace forum.DAL;

// Database initializer - Seeds the database with default content if there are no content in DB
public static class DbInit
{
    public static async void Seed(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();
        var context = serviceScope.ServiceProvider.GetRequiredService<ForumDbContext>();

        //await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        // Sets categories
        var categoriesList = new List<Category>
        {
            new()
            {
                Name = "Entertainment", Color = "#a83432",
                PicturePath = "../images/categories/entertainment-cover.png"
            },
            new()
            {
                Name = "News", Color = "#a85b32",
                PicturePath = "../images/categories/news-cover.png"
            },
            new()
            {
                Name = "Politics", Color = "#a89e32",
                PicturePath = "../images/categories/politics-cover.png"
            },
            new()
            {
                Name = "Science", Color = "#4ca832",
                PicturePath = "../images/categories/science-cover.png"
            },
            new()
            {
                Name = "Sports", Color = "#32a85f",
                PicturePath = "../images/categories/sports-cover.png"
            },
            new()
            {
                Name = "Technology", Color = "#32a88c",
                PicturePath = "../images/categories/technology-cover.png"
            },
            new()
            {
                Name = "General", Color = "#329ea8",
                PicturePath = "../images/categories/general-cover.png"
            },
            new()
            {
                Name = "Debugging", Color = "#3269a8",
                PicturePath = "../images/categories/debugging-cover.png"
            },
            new()
            {
                Name = "Development", Color = "#3236a8",
                PicturePath = "../images/categories/development-cover.png"
            },
            new()
            {
                Name = "Front End", Color = "#6932a8",
                PicturePath = "../images/categories/frontend-cover.png"
            },
            new()
            {
                Name = "Game Development", Color = "#9a32a8",
                PicturePath = "../images/categories/gamedevelopment-cover.png"
            },
            new()
            {
                Name = "Back End", Color = "#a83281",
                PicturePath = "../images/categories/backend-cover.png"
            }
        };

        if (!context.Categories.Any())
        {
            context.AddRange(categoriesList);
            await context.SaveChangesAsync();
            Console.WriteLine("Categories added");
        }

        // Sets tags
        if (!context.Tags.Any())
        {
            var tagsList = new List<Tag>
            {
                new() { Name = "Beginner" },
                new() { Name = "CSS" },
                new() { Name = "C#" },
                new() { Name = "Gaming" },
                new() { Name = "Git" },
                new() { Name = "Data Science" },
                new() { Name = "Machine Learning" },
                new() { Name = "Internet Of Things" },
                new() { Name = "JavaScript" },
                new() { Name = "PowerShell" },
                new() { Name = "Python" },
                new() { Name = "Unity" },
                new() { Name = "Version Control" },
                new() { Name = "Windows" },
                new() { Name = "Java" }
            };

            // Add tags
            context.AddRange(tagsList);
            await context.SaveChangesAsync();
            Console.WriteLine("Tags added");
        }

        // Sets roles
        if (!context.Roles.Any())
        {
            var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var roleNames = new[] { "Admin", "User" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                    Console.WriteLine(roleName + (result.Succeeded ? " created" : " failed"));
                }
            }

            Console.WriteLine("Roles added");
        }

        // Sets users
        if (!context.Users.Any())
        {
            var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var userList = new List<ApplicationUser>
            {
                new()
                {
                    UserName = "coolguy",
                    Email = "coolguy@bracketbros.com",
                    CreationDate = DateTime.Now
                },
                new()
                {
                    UserName = "hackerman",
                    Email = "hackerman@bracketbros.com",
                    CreationDate = DateTime.Now
                },
                new()
                {
                    UserName = "theboss",
                    Email = "theboss@bracketbros.com",
                    CreationDate = DateTime.Now
                }
            };

            var password = "Password123!";

            // Add users to database via UserManager
            foreach (var applicationUser in userList)
            {
                // Add user to database
                var result = await userManager.CreateAsync(applicationUser, password);
                Console.WriteLine(applicationUser.UserName + (result.Succeeded ? " created" : " failed"));
                // Add role to the user
                var resultRole = await userManager.AddToRoleAsync(applicationUser, "User");
                Console.WriteLine(applicationUser.UserName + (resultRole.Succeeded ? " added" : " failed"));
            }

            // Add admin user
            var adminUser = new ApplicationUser
            {
                UserName = "torkratte",
                Email = "torkrattebol@bracketbros.com",
                CreationDate = DateTime.Now
            };
            await userManager.CreateAsync(adminUser, password);
            await userManager.AddToRoleAsync(adminUser, "Admin");


            Console.WriteLine("Users added");
        }

        // Fetches all users from the database, to be used when seeding posts
        var addedUsers = context.Users.ToList();
        var tags = context.Tags.ToArray();

        // Random number generator
        Random random = new();

        string RandomUser()
        {
            return addedUsers[random.Next(1, addedUsers.Count())].Id;
        }

        // Sets posts
        var postsList = new List<Post>
        {
            new()
            {
                Title = "\ud83d\udcdc Why JavaScript should be considered a gift from GOD! \ud83d\udcdc",
                Content = "Ladies and gentlemen, gather 'round, for today, we embark on a divine journey through the ethereal realms of JavaScript! \ud83d\ude80\ud83c\udf0c\n\n##  **Unleash the Versatility**\nBehold, for JavaScript is the omnipotent chameleon of coding languages! It dances seamlessly not only in the sacred halls of browsers but also dons the crown of servers (praise be to Node.js) and blesses mobile apps with its touch (hail React Native)!\n\n##  **A Cosmic Force of Popularity**\nIt is not just a language; it's a celestial phenomenon! JavaScript's ubiquity transcends the boundaries of realms, making it one of the most widely-used languages, embraced by mortals and tech gods alike.\n\n##  **A Sacred Evolution**\nJavaScript is on an eternal quest for perfection. ES6, ES7, ES8... it evolves faster than the speed of light, adapting to the celestial needs of modern development.\n\n##  **The Art of Interactivity**\nWitness the magic as JavaScript breathes life into the lifeless! It grants websites the gift of interactivity and dynamism, ensnaring users in a spellbinding trance.\n\n##  **A Cosmic Job Market**\nBy embracing the holy scriptures of JavaScript, you open the gates to an abundance of job opportunities in the ever-expanding tech universe. Devs, rejoice! \ud83d\ude4c\ud83c\udf20\n\n##  **The Fellowship of Community**\nJavaScript's community is not just a community; it's a sacred brotherhood! On StackOverflow, GitHub, and countless other altars, the faithful gather to bestow wisdom upon the seeking souls.\n\nYes, it has its quirks (the enigmatic \"undefined\" and the mystical \"NaN\"), but what godly creation doesn't have its mysteries? \ud83e\udd37\u200d\u2642\ufe0f\ud83c\udf0c\n\nSo, let us kneel before JavaScript, the divine thread that weaves the very fabric of the web, a celestial gift that keeps on giving to us humble developers! \ud83d\ude4f\n\nDo you too believe in the divinity of JavaScript or have celestial tales to share? \ud83c\udf20\ud83d\udd2e #JavaScriptGift #DevotionToCode\n",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "Entertainment").CategoryId,
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "JavaScript")
                }
            },
            new()
            {
                Title = "How to Center Elements in CSS: A Beginner's Guide \ud83c\udfaf",
                Content = "### Introduction \ud83d\udc4b\nCentering elements in CSS can be confusing for beginners. This guide aims to clarify the basics.\n\n### Horizontal Centering \ud83d\udccf\nInline Elements: Use `text-align: center`.\nBlock Elements: Use `margin: auto`.\n\n### Vertical Centering \ud83d\udcd0\nSingle Line of Text: Use `line-height`.\nMultiple Lines: Use Flexbox or Grid.\n\n### Common Pitfalls \u26a0\ufe0f\nForgot `DOCTYPE`: Always declare it.\nParent Dimensions: Make sure parent has defined width and height.\n\n### Resources \ud83d\udcda\nCSS Tricks Guide: [CSS Tricks Guide](https://css-tricks.com/)\nMDN Web Docs: [MDN Web Docs](https://developer.mozilla.org/)\n\nLooking forward to your questions and contributions! \ud83e\udd17",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "Front End").CategoryId,
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "Beginner"),
                    tags.First(t => t.Name == "CSS")
                }
            },
            new()
            {
                Title = "Managing Environments in C# Projects with Git Branches \ud83c\udf33\n",
                Content = "## Introduction \ud83d\udc4b\nWhen developing C# projects, you often need to manage multiple environments like development, staging, and production. Using Git branches effectively can simplify this process.\n\n## Why Use Git Branches? \ud83e\udd14\nGit branches allow you to isolate features or environments, making it easier to manage your codebase and deploy to different environments.\n\n## Best Practices \ud83d\udee0\ufe0f\nCreate specific branches for each environment.\nUse a Gitflow or similar workflow.\nAlways merge 'development' into 'staging' and 'staging' into 'production'.\n\n## Version Control in C# \ud83d\udd17\nC# and .NET provide built-in tools like `appsettings.json` to manage environment-specific configurations, making it seamless to integrate with version control systems like Git.",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "Back End").CategoryId,
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "C#"),
                    tags.First(t => t.Name == "Version Control")
                }
            },
            new()
            {
                Title = "Leveraging Data Science in Back-End Development: A Comprehensive Guide \ud83d\udcca",
                Content = "## Introduction\nData science isn't just for data analysts or machine learning engineers. Back-end developers can also benefit from understanding and implementing data science concepts. This post aims to explore how data science techniques can improve back-end systems.\n\n## Why Data Science Matters in Back-End Development \ud83c\udfaf\nData is the lifeblood of any modern application. As back-end developers, we are responsible for storing, manipulating, and serving that data. Understanding basic data science techniques helps us to optimize these processes, thereby improving application performance and user experience.\n\n## Data Preprocessing \u2699\ufe0f\nOne of the first steps in utilizing data effectively is preprocessing. This includes cleaning and transforming raw data into a format that's easier to work with. For example, you might need to normalize text data or handle missing values before saving it to a database.\n\n## Real-time Analytics \ud83d\udcc8\nWith the advent of big data, analytics have moved beyond batch processing. Real-time analytics provide insights as data flows into the system. This is particularly useful for applications that need to react to data changes instantly, like stock trading platforms.\n\n## Machine Learning Models \ud83e\udd16\nImplementing machine learning models on the back-end can add intelligence to your application. For example, a recommendation engine could improve user engagement, or a fraud detection system could save your company money. You don't need to be a data scientist to implement basic machine learning algorithms.\n\n## Caching Strategies and Data Stores \ud83d\uddc4\ufe0f\nData science also includes making smart decisions about how and where to store data. Different types of databases have their own pros and cons, and your choice can greatly affect performance. Caching frequently accessed data can also reduce database load.\n\n## Conclusion \ud83c\udf1f\nData science isn't a field reserved for specialists. By leveraging data science techniques in back-end development, we can build smarter, faster, and more reliable applications.",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "Back End").CategoryId,
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "Data Science"),
                }
            },
            new()
            {
                Title = "Immersive Gaming Experiences with Unity: A Detailed Look \ud83c\udfae",
                Content = "## Introduction\nUnity has become a leading platform for both indie developers and large studios aiming to create compelling gaming experiences. This post will delve into techniques to enrich gameplay and engage players through Unity's various features.\n\n## The Power of Unity in Gaming \ud83d\udd79\ufe0f\nUnity's real strength lies in its versatility and the ease with which developers can create both 2D and 3D games. Its asset store and extensive documentation allow even novices to jump in and start developing their gaming ideas.\n\n## Storytelling through Environment \ud83c\udf0d\nOne way to enhance player engagement is through the game environment. Unity's lighting, shading, and physics tools can add depth and realism, helping to tell a story without words. The use of spatial audio further immerses the player into the game world.\n\n## User Interface and Experience Design \ud83d\udda5\ufe0f\nA well-designed UI is essential for player enjoyment. Unity offers a variety of built-in components for UI development, from basic buttons and sliders to complex scroll views and panels. Custom shaders can add a unique look and feel to your game's interface.\n\n## Multiplayer Functionality \ud83c\udf10\nMultiplayer games are all the rage, and Unity's networking features make it easier to create such experiences. Whether it's a cooperative play or a competitive arena, Unity provides the tools necessary to ensure seamless real-time interactions.\n\n## Virtual Reality and Augmented Reality \ud83d\udd76\ufe0f\nUnity is fully equipped to develop VR and AR games, offering a whole new level of immersion. The platform supports popular VR headsets and allows for intuitive interactions, making it easier for developers to enter the burgeoning VR/AR market.\n\n## Performance Optimization \u2699\ufe0f\nGames need to run smoothly to provide a good user experience. Unity's Profiler tool helps identify bottlenecks in game performance, while its scripting options allow for fine-tuned control over game mechanics, helping ensure that your game runs flawlessly on a range of hardware.\n\n## Conclusion \ud83c\udf1f\nUnity offers an array of tools and features that can be leveraged to create rich, engaging gaming experiences. Whether you're a beginner or a seasoned pro, the platform provides everything you need to bring your gaming vision to life.",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "Entertainment").CategoryId,
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "Unity"),
                    tags.First(t => t.Name == "Gaming"),
                }
            },
            new()
            {
                Title = "Exploring the Depths of PowerShell and Windows",
                Content =
                    "Welcome!, we'll venture into the fascinating world of PowerShell and Windows. Join me as we unlock the secrets of automation and discover the power of scripting in the Windows environment.",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = random.Next(1, categoriesList.Count),
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "PowerShell"),
                    tags.First(t => t.Name == "Windows")
                }
            },
            new()
            {
                Title = "Mastering Web Development",
                Content =
                    "Welcome noob! we'll embark on a journey into the exciting realm of JavaScript and web development. Whether you're a complete beginner or looking to reinforce your skills, this post is tailored just for you. Let's explore the fundamentals of JavaScript and how it plays a crucial role in creating dynamic web applications.",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = random.Next(1, categoriesList.Count),
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "JavaScript"),
                    tags.First(t => t.Name == "Beginner")
                }
            },
            new()
            {
                Title = "The Beauty of Python's Simplicity",
                Content =
                    "Explore the elegance of Python, a language that embraces simplicity and readability. Pythonic code is like poetry for programmers.",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = random.Next(1, categoriesList.Count),
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "Python")
                }
            },
            new()
            {
                Title = "The Art of Debugging: Finding the Needle in the Haystack",
                Content =
                    "Debugging is both an art and a science. Learn the techniques and tools that seasoned developers use to hunt down and fix bugs in their code.",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = random.Next(1, categoriesList.Count),
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "Python")
                }
            },
            new()
            {
                Title = "Diving into Data Science: A Beginner's Guide",
                Content =
                    "Interested in data science? Discover the essential concepts and tools you need to start your journey into the exciting world of data analysis and machine learning.",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = random.Next(1, categoriesList.Count),
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "Data Science"),
                    tags.First(t => t.Name == "Beginner"),
                    tags.First(t => t.Name == "Machine Learning")
                }
            },
            new()
            {
                Title = "The Power of Git: Version Control Made Easy",
                Content =
                    "Git is a developer's best friend. Learn how this version control system simplifies collaboration and tracking changes in your codebase.",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = random.Next(1, categoriesList.Count),
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "Git"),
                    tags.First(t => t.Name == "Version Control")
                }
            },

            new()
            {
                Title = "Building Responsive Web Design with CSS Grid",
                Content =
                    "Dive into the world of CSS Grid and unlock the potential for creating stunning, responsive web layouts with ease.",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = random.Next(1, categoriesList.Count),
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "CSS")
                }
            },
            new()
            {
                Title = "Exploring the Internet of Things (IoT)",
                Content =
                    "Get ready for a journey into the Internet of Things, where everyday objects are connected to the digital world. Discover the possibilities and challenges of IoT.",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = random.Next(1, categoriesList.Count),
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "Internet Of Things")
                }
            },
            new()
            {
                Title = "Game Development with Unity: Bringing Ideas to Life",
                Content =
                    "If you've ever dreamed of creating your own video game, Unity is the platform to make it happen. Explore the world of game development and start building your masterpiece.",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = random.Next(1, categoriesList.Count),
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "Unity")
                }
            }
        };

        if (!context.Posts.Any() && addedUsers.Count > 0) // If there are no posts in the database and there are users
        {
            context.AddRange(postsList);
            await context.SaveChangesAsync();
            Console.WriteLine("Temp posts added");
        }

        // Sets different comments or random posts
        if (context.Posts.Any() && addedUsers.Count > 0 &&
            !context.Comments.Any()) // If there are no posts in the database and there are users
        {
            var comments = new List<Comment>();

            //Taken from https://www.cardenhall.com/wp-content/uploads/2016/10/POSITIVE-COMMENTS-List.pdf
            var commentContent = new List<string>
            {
                "A powerful argument! I commend you for your quick thinking.",
                "A splendid job! I commend you for your thorough work.",
                "A well-developed theme! I knew you could do it!",
                "An A-1 paper! I like how you've tackled this assignment.",
                "Appreciated I like the way you're working.", "Astounding I like the way you've handled this.",
                "Awesome I like the way you settle down to work.", "Beautiful I like your style.",
                "Bravo I love your care.", "Brilliant I noticed that you got right down to work.",
                "Dazzling Impressive", "Dedicated effort In fine style", "Delightful Incredible",
                "Desirable It looks like you've put a lot of work into this.", "Exactly right! Keep it up.",
                "Excellent Keep up the good work.", "Exceptional Magnificent", "Exciting Majestic thoughts",
                "Exemplary Marvelous", "Exhilarating Meritorious", "Extraordinary Much better",
                "Fabulous My goodness, how impressive!", "Fantastic Nice going", "Favorable Noble", "Fine Noteworthy",
                "Fine job Now you've figured it out.", "First-rate An orderly paper", "Go to the head of the class.",
                "Outstanding", "Good for you Phenomenal", "Good reasoning Praiseworthy",
                "Good thinking Prestigious work", "Good work/Good job Proper", "Grand Purrrfect", "Great Remarkable",
                "Great going Resounding results", "Honorable Respectable", "I appreciate your cooperation.",
                "Right on target", "I appreciate your help.", "Sensational",
                "Sharp thinking This shows fine improvement.", "Spectacular This shows you've really been thinking.",
                "Splendid Thorough", "Stupendous Thoughtful!", "Successful effort Tiptop", "Super Top-notch",
                "Super job/Super work Top-notch work!", "Superb Very creative",
                "Superior work/Superior job Very fine work", "Supreme Very interesting", "Terrific Very stylish",
                "Terrific job Well thought out", "Thank you What a stylish paper!",
                "Thank you for getting right to work.", "What careful work!", "Thank you for such a fine effort.",
                "What neat work!", "That looks like it's going to be a good report.",
                "Where have you been hiding all this talent?", "That was fun.", "Wonderful", "That's a good point.",
                "Worthy", "That's a very good observation.", "You are really in touch with the feeling here.",
                "That's an interesting point of view.", "You are showing that you were thinking.",
                "That's an interesting way of looking at it.", "You make it look so easy.",
                "That's certainly one way of looking at it.", "You must have been practicing.", "That's clever.",
                "You really outdid yourself today.", "That's coming along nicely.", "You really scored here.",
                "That's great! You're becoming an expert at this.", "That's quite an improvement.",
                "You're on the ball today.", "That's really nice.", "You're on the right track now.", "That's right.",
                "Good for you.", "You're really moving.", "That's the right answer.", "You're right on target.",
                "That's very perceptive.", "You've come a long way with this one.",
                "The results were worth all your hard work.", "You've got it now.", "This gets a four-star rating.",
                "You've put in a full day today.", "This is a moving scene.", "You've really been paying attention.",
                "This is a winner! You've shown a lot of patience with this.",
                "This is fun, isn't it? Your hard work has paid off.", "This is prize-winning work.",
                "Your remark shows a lot of sensitivity.", "This is quite an accomplishment.",
                "Your style has spark.", "This is something special.", "Your work has such personality.",
                "This kind of work pleases me very much.", "This paper has pizzazz!", "This really has flair."
            };

            foreach (var content in commentContent)
                comments.Add(new Comment
                {
                    Content = content,
                    DateCreated = DateTime.Now,
                    UserId = RandomUser(),
                    PostId = random.Next(1, postsList.Count),
                    TotalLikes = random.Next(999)
                });

            // Add comments to the database
            context.Comments.AddRange(comments);

            await context.SaveChangesAsync();
            Console.WriteLine("Temp comments added");
        }
    }
}