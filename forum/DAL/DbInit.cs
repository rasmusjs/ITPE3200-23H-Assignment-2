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

        await context.Database.EnsureDeletedAsync(); // Deletes database if it exists
        await context.Database.EnsureCreatedAsync(); // Creates database if it doesn't exist

        // Sets categories
        var categoriesList = new List<Category>
        {
            new()
            {
                Name = "Algorithms", Color = "#a83432",
                PicturePath = "../images/categories/algorithms-cover.png"
            },
            new()
            {
                Name = "News", Color = "#a85b32",
                PicturePath = "../images/categories/news-cover.png"
            },
            new()
            {
                Name = "Database", Color = "#a89e32",
                PicturePath = "../images/categories/database-cover.png"
            },
            new()
            {
                Name = "Science", Color = "#4ca832",
                PicturePath = "../images/categories/science-cover.png"
            },
            new()
            {
                Name = "Code Review", Color = "#32a85f",
                PicturePath = "../images/categories/code-review-cover.png"
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
                PicturePath = "../images/categories/game-development-cover.png"
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
                new() { Name = "HTML" },
                new() { Name = "CSS" },
                new() { Name = "Git" },
                new() { Name = "Version Control" },
                new() { Name = "C#" },
                new() { Name = "Data Science" },
                new() { Name = "Machine Learning" },
                new() { Name = "JavaScript" },
                new() { Name = "PowerShell" },
                new() { Name = "Python" },
                new() { Name = "Unity" },
                new() { Name = "Windows" },
                new() { Name = "Java" },
                new() { Name = "Humor" },
                new() { Name = "SQL" },
                new() { Name = "Cloudflare" },
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

            // Default password for fake users
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
                Title = "Simple ToDo App",
                Content =
                    "Hello everyone,\n\nI've recently built a simple ToDo app using vanilla JavaScript and HTML. I'm looking for feedback on my code structure, naming conventions, and any best practices I might be overlooking. Here's the main code snippet:\n\n```html\n<!DOCTYPE html>\n<html lang=\"en\">\n<head>\n    <meta charset=\"UTF-8\">\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\n    <title>Simple ToDo App</title>\n</head>\n<body>\n    <div id=\"app\">\n        <input type=\"text\" id=\"taskInput\" placeholder=\"Enter your task\">\n        <button onclick=\"addTask()\">Add Task</button>\n        <ul id=\"taskList\"></ul>\n    </div>\n\n    <script>\n        function addTask() {\n            const taskInput = document.getElementById('taskInput');\n            const taskList = document.getElementById('taskList');\n\n            if(taskInput.value.trim() === '') return;\n\n            const li = document.createElement('li');\n            li.textContent = taskInput.value;\n            taskList.appendChild(li);\n\n            taskInput.value = '';\n        }\n    </script>\n</body>\n</html>",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "Code Review").CategoryId,
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "JavaScript"),
                    tags.First(t => t.Name == "HTML")
                }
            },
            new()
            {
                Title = "Beginner's Question: HTML Layout Structure",
                Content =
                    "Hi everyone,\n\nI'm just starting out with web development and have a basic question about HTML structure. I've created a simple webpage layout, and I'd like to get feedback on whether I'm structuring it correctly.\n\n```html\n<!DOCTYPE html>\n<html lang=\"en\">\n<head>\n    <meta charset=\"UTF-8\">\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\n    <title>My First Webpage</title>\n</head>\n<body>\n    <header>\n        <h1>Welcome to My Webpage</h1>\n        <nav>\n            <ul>\n                <li><a href=\"#home\">Home</a></li>\n                <li><a href=\"#about\">About</a></li>\n                <li><a href=\"#contact\">Contact</a></li>\n            </ul>\n        </nav>\n    </header>\n\n    <main>\n        <section id=\"home\">\n            <h2>Home</h2>\n            <p>This is the homepage content.</p>\n        </section>\n\n        <section id=\"about\">\n            <h2>About</h2>\n            <p>Information about the website.</p>\n        </section>\n\n        <section id=\"contact\">\n            <h2>Contact</h2>\n            <p>Contact details and form will go here.</p>\n        </section>\n    </main>\n\n    <footer>\n        <p>\u00a9 2023 My Webpage. All rights reserved.</p>\n    </footer>\n</body>\n</html>",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "Code Review").CategoryId,
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "Beginner"),
                    tags.First(t => t.Name == "HTML")
                }
            },
            new()
            {
                Title = "Binary Search Implementation in Python",
                Content =
                    "I've been practicing my algorithm skills and decided to implement the binary search algorithm in Python. I would love to get feedback on my implementation for correctness and efficiency.\n\n```python\ndef binary_search(arr, x):\n    \"\"\"\n    Perform binary search to find the index of x in arr.\n    \n    :param arr: List of sorted elements\n    :param x: Element to search for\n    :return: Index of x in arr or -1 if not found\n    \"\"\"\n    l = 0\n    r = len(arr) - 1\n    \n    while l <= r:\n        mid = (l + r) // 2\n        \n        # Check if x is present at mid\n        if arr[mid] == x:\n            return mid\n        \n        # If x is greater, ignore the left half\n        elif arr[mid] < x:\n            l = mid + 1\n        \n        # If x is smaller, ignore the right half\n        else:\n            r = mid - 1\n            \n    return -1\n\n# Test\narr = [1, 3, 5, 7, 9, 11, 13]\nx = 7\nprint(binary_search(arr, x))  # Expected output: 3",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "Algorithms").CategoryId,
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "Python"),
                }
            },
            new()
            {
                Title = "C# ML.NET Model Training Issue",
                Content =
                    "Hi all,\n\nI've started using ML.NET in C# for a basic regression model. Encountered an error during model training. Here's the code snippet:\n\n```csharp\nusing Microsoft.ML;\nusing Microsoft.ML.Data;\n\nvar context = new MLContext();\nIDataView dataView = context.Data.LoadFromTextFile<MyData>(\"data.csv\", separatorChar: ',');\n\nvar pipeline = context.Transforms.Concatenate(\"Features\", \"Value1\", \"Value2\")\n                .Append(context.Regression.Trainers.Sdca());\n\nvar model = pipeline.Fit(dataView); // Error occurs here",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "Algorithms").CategoryId,
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "C#"),
                    tags.First(t => t.Name == "Machine Learning")
                }
            },
            new()
            {
                Title = "SQL Query Help",
                Content =
                    "I'm working with a SQL database and I'm having trouble constructing a query to retrieve data based on specific conditions. Can anyone help me out?\n\n```sql\nSELECT * FROM users WHERE last_login < '2022-01-01' AND status = 'active';",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "Database").CategoryId,
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "SQL"),
                }
            },
            new()
            {
                Title = "Database Hosting with Cloudflare",
                Content =
                    "I've been researching hosting options for my database and came across Cloudflare's database hosting services. I have a few questions:\n\n1. \ud83d\ude80 How does Cloudflare's performance compare to other providers like AWS and Google Cloud?\n2. \ud83d\udd12 What are the security measures Cloudflare provides for databases?\n3. \ud83d\udcb0 Are there any hidden costs or considerations when using Cloudflare for database hosting?\n4. \ud83d\udd04 How easy is it to migrate an existing database to Cloudflare?\n\nWould love to hear from anyone with experience using Cloudflare for databases. Any insights or recommendations would be greatly appreciated!\n\nThanks!",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "Database").CategoryId,
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "Cloudflare"),
                }
            },
            new()
            {
                Title = "Why JavaScript should be considered a gift from GOD!",
                Content =
                    "Ladies and gentlemen, gather 'round, for today, we embark on a divine journey through the ethereal realms of JavaScript! \ud83d\ude80\ud83c\udf0c\n\n##  **Unleash the Versatility**\nBehold, for JavaScript is the omnipotent chameleon of coding languages! It dances seamlessly not only in the sacred halls of browsers but also dons the crown of servers (praise be to Node.js) and blesses mobile apps with its touch (hail React Native)!\n\n##  **A Cosmic Force of Popularity**\nIt is not just a language; it's a celestial phenomenon! JavaScript's ubiquity transcends the boundaries of realms, making it one of the most widely-used languages, embraced by mortals and tech gods alike.\n\n##  **A Sacred Evolution**\nJavaScript is on an eternal quest for perfection. ES6, ES7, ES8... it evolves faster than the speed of light, adapting to the celestial needs of modern development.\n\n##  **The Art of Interactivity**\nWitness the magic as JavaScript breathes life into the lifeless! It grants websites the gift of interactivity and dynamism, ensnaring users in a spellbinding trance.\n\n##  **A Cosmic Job Market**\nBy embracing the holy scriptures of JavaScript, you open the gates to an abundance of job opportunities in the ever-expanding tech universe. Devs, rejoice! \ud83d\ude4c\ud83c\udf20\n\n##  **The Fellowship of Community**\nJavaScript's community is not just a community; it's a sacred brotherhood! On StackOverflow, GitHub, and countless other altars, the faithful gather to bestow wisdom upon the seeking souls.\n\nYes, it has its quirks (the enigmatic \"undefined\" and the mystical \"NaN\"), but what godly creation doesn't have its mysteries? \ud83e\udd37\u200d\u2642\ufe0f\ud83c\udf0c\n\nSo, let us kneel before JavaScript, the divine thread that weaves the very fabric of the web, a celestial gift that keeps on giving to us humble developers! \ud83d\ude4f\n\nDo you too believe in the divinity of JavaScript or have celestial tales to share? \ud83c\udf20\ud83d\udd2e #JavaScriptGift #DevotionToCode\n",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "Front End").CategoryId,
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "JavaScript"),
                    tags.First(t => t.Name == "Humor")
                }
            },
            new()
            {
                Title = "How to Center Elements in CSS: A Beginner's Guide",
                Content =
                    "## Introduction \ud83d\udc4b\nCentering elements in CSS can be confusing for beginners. This guide aims to clarify the basics.\n\n## Horizontal Centering \ud83d\udccf\nInline Elements: Use `text-align: center`.\nBlock Elements: Use `margin: auto`.\n\n## Vertical Centering \ud83d\udcd0\nSingle Line of Text: Use `line-height`.\nMultiple Lines: Use Flexbox or Grid.\n\n## Common Pitfalls \u26a0\ufe0f\nForgot `DOCTYPE`: Always declare it.\nParent Dimensions: Make sure parent has defined width and height.\n\n## Resources \ud83d\udcda\nCSS Tricks Guide: [CSS Tricks Guide](https://css-tricks.com/)\nMDN Web Docs: [MDN Web Docs](https://developer.mozilla.org/)\n\nLooking forward to your questions and contributions! \ud83e\udd17",
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
                Title = "Diving Deep into HTML and CSS",
                Content =
                    "## Introduction \nHey web developers! \ud83d\udc69\u200d\ud83d\udcbb\n\nHTML and CSS are the building blocks of the web. They've evolved over the years, offering more flexibility and power.\n\n### Key HTML5 Features \ud83d\udcdc \n- **Semantic Tags:** Elements like `<article>`, `<nav>`, and `<aside>` make our content more understandable.\n- **Media Elements:** `<audio>` and `<video>` tags have revolutionized media integration without plugins.\n- **Canvas:** Allows for dynamic, scriptable rendering of 2D shapes and bitmap images.\n\n### CSS Flexbox and Grid \ud83c\udf9a\ufe0f \n- **Flexbox:** Simplifies layout, alignment, and distribution of items within a container. Perfect for 1D layouts! \ud83d\udea4\n- **Grid:** Introduces 2D layout control, making complex designs simpler and more responsive. \ud83d\uddf7\n\n## Conclusion \nThe evolving standards and capabilities in HTML and CSS continue to push the boundaries of what's possible on the web. Exciting times ahead! \ud83d\ude80\n\n---\n\nWhat are your favorite HTML and CSS features?\n",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "Front End").CategoryId,
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "HTML"),
                    tags.First(t => t.Name == "CSS"),
                }
            },
            new()
            {
                Title = "Managing Environments in C# Projects with Git Branches",
                Content =
                    "## Introduction \ud83d\udc4b\nWhen developing C# projects, you often need to manage multiple environments like development, staging, and production. Using Git branches effectively can simplify this process.\n\n## Why Use Git Branches? \ud83e\udd14\nGit branches allow you to isolate features or environments, making it easier to manage your codebase and deploy to different environments.\n\n## Best Practices \ud83d\udee0\ufe0f\nCreate specific branches for each environment.\nUse a Gitflow or similar workflow.\nAlways merge 'development' into 'staging' and 'staging' into 'production'.\n\n## Version Control in C# \ud83d\udd17\nC# and .NET provide built-in tools like `appsettings.json` to manage environment-specific configurations, making it seamless to integrate with version control systems like Git.",
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
                Title = "Leveraging Data Science in Back-End Development: A Comprehensive Guide",
                Content =
                    "## Introduction\nData science isn't just for data analysts or machine learning engineers. Back-end developers can also benefit from understanding and implementing data science concepts. This post aims to explore how data science techniques can improve back-end systems.\n\n## Why Data Science Matters in Back-End Development \ud83c\udfaf\nData is the lifeblood of any modern application. As back-end developers, we are responsible for storing, manipulating, and serving that data. Understanding basic data science techniques helps us to optimize these processes, thereby improving application performance and user experience.\n\n## Data Preprocessing \u2699\ufe0f\nOne of the first steps in utilizing data effectively is preprocessing. This includes cleaning and transforming raw data into a format that's easier to work with. For example, you might need to normalize text data or handle missing values before saving it to a database.\n\n## Real-time Analytics \ud83d\udcc8\nWith the advent of big data, analytics have moved beyond batch processing. Real-time analytics provide insights as data flows into the system. This is particularly useful for applications that need to react to data changes instantly, like stock trading platforms.\n\n## Machine Learning Models \ud83e\udd16\nImplementing machine learning models on the back-end can add intelligence to your application. For example, a recommendation engine could improve user engagement, or a fraud detection system could save your company money. You don't need to be a data scientist to implement basic machine learning algorithms.\n\n## Caching Strategies and Data Stores \ud83d\uddc4\ufe0f\nData science also includes making smart decisions about how and where to store data. Different types of databases have their own pros and cons, and your choice can greatly affect performance. Caching frequently accessed data can also reduce database load.\n\n## Conclusion \ud83c\udf1f\nData science isn't a field reserved for specialists. By leveraging data science techniques in back-end development, we can build smarter, faster, and more reliable applications.",
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
                Title = "Immersive Gaming Experiences with Unity: A Detailed Look",
                Content =
                    "## Introduction\nUnity has become a leading platform for both indie developers and large studios aiming to create compelling gaming experiences. This post will delve into techniques to enrich gameplay and engage players through Unity's various features.\n\n## The Power of Unity in Gaming \ud83d\udd79\ufe0f\nUnity's real strength lies in its versatility and the ease with which developers can create both 2D and 3D games. Its asset store and extensive documentation allow even novices to jump in and start developing their gaming ideas.\n\n## Storytelling through Environment \ud83c\udf0d\nOne way to enhance player engagement is through the game environment. Unity's lighting, shading, and physics tools can add depth and realism, helping to tell a story without words. The use of spatial audio further immerses the player into the game world.\n\n## User Interface and Experience Design \ud83d\udda5\ufe0f\nA well-designed UI is essential for player enjoyment. Unity offers a variety of built-in components for UI development, from basic buttons and sliders to complex scroll views and panels. Custom shaders can add a unique look and feel to your game's interface.\n\n## Multiplayer Functionality \ud83c\udf10\nMultiplayer games are all the rage, and Unity's networking features make it easier to create such experiences. Whether it's a cooperative play or a competitive arena, Unity provides the tools necessary to ensure seamless real-time interactions.\n\n## Virtual Reality and Augmented Reality \ud83d\udd76\ufe0f\nUnity is fully equipped to develop VR and AR games, offering a whole new level of immersion. The platform supports popular VR headsets and allows for intuitive interactions, making it easier for developers to enter the burgeoning VR/AR market.\n\n## Performance Optimization \u2699\ufe0f\nGames need to run smoothly to provide a good user experience. Unity's Profiler tool helps identify bottlenecks in game performance, while its scripting options allow for fine-tuned control over game mechanics, helping ensure that your game runs flawlessly on a range of hardware.\n\n## Conclusion \ud83c\udf1f\nUnity offers an array of tools and features that can be leveraged to create rich, engaging gaming experiences. Whether you're a beginner or a seasoned pro, the platform provides everything you need to bring your gaming vision to life.",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "Game Development").CategoryId,
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "Unity"),
                }
            },
            new()
            {
                Title = "Integrating Machine Learning into My C# Game",
                Content =
                    "Hey everyone!\n\nI'm currently working on a game developed in C#. Recently, I've started to integrate some machine learning features to enhance gameplay dynamics. Specifically, I'm training a model to adapt game difficulty based on a player's skill level and in-game behavior. This ensures a more tailored and challenging experience for each individual player.\n\nHas anyone else experimented with combining C# game development and machine learning? Would love to hear your insights and share some challenges I've encountered along the way.\n",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "Game Development").CategoryId,
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "C#"),
                    tags.First(t => t.Name == "Machine Learning"),
                }
            },
            new()
            {
                Title = "Python indentations are annoying",
                Content =
                    "Hey all!\n\nRan into a quirky Python issue today. Always remember to check for indentation errors, they can be sneaky! \ud83d\udd75\ufe0f\u200d\u2642\ufe0f Any fun debugging stories to share?\n",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "Debugging").CategoryId,
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "Python"),
                }
            },
            new()
            {
                Title = "Debugging in PowerShell",
                Content =
                    "### Introduction\nHey PowerShell enthusiasts! \u270c\ufe0f\n\nI've been working on some scripts lately, and I've encountered multiple challenges. Figured I'd share and hopefully help someone out.\n\n### Common Issues \ud83d\udeab\n- **Syntax Errors:** Always double-check for missing brackets or misused symbols. It's easy to overlook!\n- **Execution Policy:** Remember, sometimes your script won't run due to the set execution policy. Use `Get-ExecutionPolicy` to check it.\n- **Path Errors:** Ensure paths are correct and avoid hardcoding them when possible. Use relative paths or variables.\n\n### Tips and Tricks \ud83d\udc8e\n- Use `Set-PSDebug -Trace 1` to step through your script line by line. It's a lifesaver! \ud83e\udd17\n- Always have a backup of your original script before making major changes. \ud83d\udd10\n\nWould love to hear about your own PowerShell debugging adventures and insights!\n",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "Debugging").CategoryId,
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "PowerShell"),
                }
            },
            new()
            {
                Title = "Exploring Alternative Version Control Systems",
                Content =
                    "**Hello devs!**\n\nWhile Git is popular, I've been exploring alternative version control systems like Mercurial and Bazaar. They offer some unique features! Has anyone else tried branching out? Thoughts? \ud83e\udd14\n",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "Development").CategoryId,
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "Version Control"),
                }
            },
            new()
            {
                Title = "Windows Development: Embracing the Modern Stack",
                Content =
                    "**Hey fellow developers!**\n\nWindows development has come a long way, especially with the introduction of tools like Windows Subsystem for Linux (WSL) and the move towards universal apps with the UWP platform.\n\n**WSL 2 - Bridging the Gap \ud83c\udf09**  \nWSL 2 has revolutionized the way we think about cross-platform development on Windows. Now, seamlessly integrating Linux-based tools and workflows is a breeze. It's especially beneficial for those coming from a UNIX background, wanting to maintain a familiar development environment. \ud83d\udc27\n\n**UWP - Universal Windows Platform \ud83c\udf10**  \nThe promise of write-once, run-anywhere is getting closer with UWP. Creating apps that work across PCs, tablets, Xbox, and even HoloLens is now more streamlined. Plus, with tools like Xamarin, you can extend this even further across platforms.\n\n**Staying Updated \ud83d\udd03**  \nWith constant updates to .NET, Visual Studio, and other essential tools, it's crucial to stay updated. Not just for the new features, but for performance improvements and security patches as well\n",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "Development").CategoryId,
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "Windows"),
                }
            },
            new()
            {
                Title = "Why did the programmer quit his job?",
                Content =
                    "**Hey code enthusiasts!**\n\nJust wanted to share a little programming humor to brighten your day.\n\n**Q: Why did the programmer quit his job?**  \n**A:** Because he didn't get arrays!\n\nOkay, I'll admit, that was a bit of a stretch. But if coding has taught us anything, it's to appreciate a good (or bad) pun every now and then. \ud83d\ude02\n\nGot any coding jokes or puns to share? Let's hear them!\n",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "General").CategoryId,
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "Humor"),
                }
            },
            new()
            {
                Title = "Java 16 Released!",
                Content =
                    "**Hey devs!**\n\nJava 16 has just dropped, bringing with it a slew of new features and improvements! Time to brew some fresh code! \u2615\ufe0f Anyone tried the new features yet? Thoughts?\n",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "News").CategoryId,
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "Java"),
                }
            },
            new()
            {
                Title = "Advancements in Machine Learning: GPT-4 and Beyond",
                Content =
                    "**Introduction**  \nHello ML enthusiasts! \ud83c\udf1f\n\nMachine learning has taken another significant leap with the release of GPT-4.\n\n**Features & Improvements** \ud83d\udcca  \n- **Increased Parameters:** GPT-4 boasts more parameters, leading to even more accurate predictions and nuances.\n- **Transfer Learning Enhancements:** Improved efficiency in transferring knowledge from one domain to another.\n- **Broader Language Support:** Supporting even more languages and dialects, making AI truly global. \ud83c\udf0d\n\n**Real-world Applications** \ud83c\udf10  \n- **Healthcare:** From predicting diseases to helping in drug discovery.\n- **Automotive:** Assisting in self-driving technology.\n- **Entertainment:** Creating music, art, and even writing scripts.\n\n**Closing Thoughts**  \nAs machine learning continues its rapid advancement, the potential applications in various sectors grow exponentially. Exciting times ahead! \ud83d\udca1\ud83c\udf89\n\nShare your thoughts and experiences on the latest in ML!\n",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "News").CategoryId,
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "Machine Learning"),
                }
            },
            new()
            {
                Title = "Machine Learning in Political Campaigns",
                Content =
                    "**Machine learning** is now playing a pivotal role in political campaigns, aiding in voter targeting and sentiment analysis. Could this be the future of political strategy? What do you think?\n",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "News").CategoryId,
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "Machine Learning"),
                }
            },
            new()
            {
                Title = "Windows OS: Privacy Concerns and Government Policies",
                Content =
                    "## Introduction \nIn recent times, the Windows OS has come under scrutiny for its privacy settings and data collection. Let's delve deeper.\n\n### Data Collection & Concerns \ud83d\udd0d \n- **Telemetry Data:** Windows collects data to enhance user experience, but how much is too much?\n- **Default Settings:** Some argue that many privacy-invading settings are on by default, raising concerns. \ud83d\udeab\n- **Data Sharing:** While Microsoft ensures user data security, concerns about sharing with third parties remain.\n\n### Governmental Intervention \ud83c\udfdb\ufe0f \n- **GDPR & Windows:** How does Windows fare against GDPR guidelines?\n- **Policies in Different Countries:** Different countries are imposing varied restrictions and regulations. \ud83c\udf0d\n\n### User Choices & Control \u2699\ufe0f \nEmpowering users by providing clear choices and granular controls is essential. Transparency is the key! \ud83d\udd11\n\n---\n\nDo you believe stricter regulations are needed? Or is it a user's responsibility to tweak settings as they see fit? Let's discuss!\n",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "News").CategoryId,
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "Windows"),
                }
            },
            new()
            {
                Title = "NVIDIA's Breakthrough in Machine Learning",
                Content =
                    "**NVIDIA** has recently unveiled their latest GPUs optimized for machine learning workloads. A powerhouse for both gaming and ML! \ud83d\ude80 Who's excited to test them out?\n",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "Technology").CategoryId,
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "Machine Learning"),
                }
            },
            new()
            {
                Title = "Exploring Unity: Beyond Game Development",
                Content =
                    "**Introduction**  \nGreetings Unity enthusiasts! \u2728\n\nUnity, renowned for game development, is now stretching its wings into other domains.\n\n**Extended Applications** \ud83d\udcc8  \n- **Virtual Reality (VR):** Crafting immersive experiences for users.\n- **Augmented Reality (AR):** From mobile apps to complex AR systems, Unity is at the forefront. \ud83d\udcf1\n- **Film Production:** Unity is becoming a favorite for filmmakers, aiding in real-time rendering and effects.\n\n**Benefits of Unity** \ud83c\udfd6\ufe0f  \n- **Versatile:** Adapts to various projects and needs.\n- **Asset Store:** A plethora of resources available for developers.\n- **Supportive Community:** An active community ready to help and share. \ud83c\udf1f\n\n**Conclusion**  \nUnity's flexibility and capabilities make it a top choice not only for game developers but also for professionals in various industries. From movies to real-world simulations, Unity is shaping the future!\n\nGot any cool Unity projects or experiences? Share away!\n",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "Technology").CategoryId,
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "Unity"),
                }
            },
            new()
            {
                Title = "Python in Bioinformatics and Genetic Research",
                Content =
                    "**Python in Bioinformatics**  \nPython is making strides in the realm of bioinformatics, aiding in genetic sequence analysis and data processing. The fusion of biology and Python is truly exciting! \ud83d\udd0d\n",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "Science").CategoryId,
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "Python"),
                }
            },
            new()
            {
                Title = "C# and Quantum Computing: The Leap Forward",
                Content =
                    "**C# in Quantum Computing**  \nC# is playing a pivotal role in the development of quantum computing applications.\n\n**Quantum and C#: The Synergy** \ud83c\udf0c  \n- **Q# Language:** Developed by Microsoft, it's designed to work seamlessly with C# for quantum programming.\n- **Complex Simulations:** C# provides the robustness needed for intricate quantum simulations. \u269b\ufe0f\n- **Interoperability:** C# interfaces efficiently with quantum hardware and other platforms.\n\n**Potential Outcomes** \ud83c\udf10  \n- **Medical Research:** Envision faster drug discovery and intricate molecule studies.\n- **Cryptography:** Quantum computing promises breakthroughs in secure communication. \ud83d\udd12\n- **Optimization Problems:** Solutions to previously unsolvable problems within reach.\n\n**Wrap Up**  \nC# continues to evolve, not just in traditional software domains, but also in pushing the boundaries of what's possible in science and quantum research. A fascinating journey ahead!\n\nShare your insights on C#, quantum, or both!\n",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "Science").CategoryId,
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "C#"),
                }
            },
            new()
            {
                Title = "Exploring the Depths of PowerShell and Windows",
                Content =
                    "# Welcome!  \nJoin us as we venture into the fascinating world of **PowerShell** and **Windows**. Unlock the secrets of automation and discover the immense power of scripting in the Windows environment.\n",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "General").CategoryId,
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
                    "# Welcome Noob!  \nEmbark on a journey into the exciting realm of **JavaScript** and **web development**. Whether you're a complete beginner or looking to reinforce your skills, this post is tailored just for you. Let's dive into the fundamentals of JavaScript and its crucial role in crafting dynamic web applications.\n",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "Front End").CategoryId,
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
                    "# Explore the Elegance of Python \ud83d\udc0d  \n**Python**, a language that embraces simplicity and readability. Pythonic code is akin to poetry for programmers.\n",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "General").CategoryId,
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "Python")
                }
            },
            new()
            {
                Title = "The Art of Debugging in Java",
                Content =
                    "Debugging is both an art and a science when coding in Java. Prove me wrong! Oh no, you cannot... because it is AWESOME!!!",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "Debugging").CategoryId,
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "Java")
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
                CategoryId = categoriesList.First(c => c.Name == "General").CategoryId,
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
                CategoryId = categoriesList.First(c => c.Name == "Development").CategoryId,
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
                CategoryId = categoriesList.First(c => c.Name == "Front End").CategoryId,
                Tags = new List<Tag>
                {
                    tags.First(t => t.Name == "CSS")
                }
            },
            new()
            {
                Title = "Game Development with Unity: Bringing Ideas to Life",
                Content =
                    "If you've ever dreamed of creating your own video game, Unity is the platform to make it happen. Explore the world of game development and start building your masterpiece!",
                DateCreated = DateTime.Now - TimeSpan.FromDays(random.Next(999)),
                DateLastEdited = DateTime.Now - TimeSpan.FromDays(random.Next(9)),
                TotalLikes = random.Next(9999),
                UserId = RandomUser(),
                CategoryId = categoriesList.First(c => c.Name == "Game Development").CategoryId,
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

            // Add comments to the database
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