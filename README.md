BracketBros
======================

* * *

BracketBros is a programming discussion forum made for the ITPE3200 portfolio exam (project 1). The forum is made with .NET Core 6.0, C# and with a MVC framework. This is a MVP solution where the focus is the back-end. The front-end is made with ~~basic Bootstrap~~.

**⚡ Quick start**
---

When starting the server you will be sent to the post view, where there is an chronological feed with forum posts. You can then browse posts, see comments by going to each posts or search for specific posts.

To interact with the site, you need to register a user. Register a username, email and password. The password must consist of at least 8 characters, one alphanumeric character, one lowercase and one uppercase letter. 

When you have logged in you can create/edit posts, comment and like posts. In the upper right corner you have your profile panel. Here you can change username, upload a profile picture, change mail/password, enable 2FA and download all your user data.

**🔨 Project architecture**
---

A bit about how the program is structured

**This table shows the general structure for our applications:**

*The table needs to be rewritten to provide more concrete example of use.*

| Layer | Description |
|---|---|
| Areas | This layer contains the user account management functionality (auto generated from the 'Microsoft.AspNetCore.Identity.UI' package. |
| Controllers | This layer contains the controllers that handle HTTP requests. When a user requests a page, the corresponding controller is responsible for returning the appropriate response. For example, the ForumController is responsible for handling requests to forum pages. |
| Models | This layer contains the models that represent the data in the application. For example, the Forum model represents a forum and the Post model represents a post in a forum.|
| Views | This layer contains the Razor Pages that are used to render the user interface. When a controller needs to render a page, it passes the appropriate model to the view. The view then uses the model to generate the HTML output for the page. |
| wwwroot | This layer contains the static files that are served to the client, such as CSS and JavaScript files. These files are served directly to the client without any processing by the ASP.NET Core application. |
| DAL | Data Access Layer - This layer contains the code that interacts with the database. This code is responsible for performing CRUD operations on the data in the database.|
| ViewModel | This layer contains the classes that represent the data that is passed to the views. The ViewModels are used to simplify the code in the views and make them more reusable |


_**UMLs GOES HERE**_

### Views

Something cool about the views

###  

**📝 Functionality**
---
Something about the functionality of the site. 

### Navigation and content
????

### error handling
Error handling, input validation authentication/authorization, etc.

### User management

Microsoft.AspNetCore.Identity and Jdenticon.AspNetCore

**⌨️ Code**
---
No idea what goes here. We'll figure it out.

The code is documented with comments throughout the project. The code should either be self explanatory or explained with comments. We have also provided comments with links where we have found inspiration or code from the internet.

### Packages

These are the packages that we have imported to our project and a description of what they do.

*Need to rewrite more specific what we are using the packages for*

| Package | Description |
|---|---|
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | Provides classes and interfaces that are used to implement user authentication and authorization in ASP.NET Core applications. |
| `Microsoft.AspNetCore.Identity.UI` | Provides Razor Pages and components that are used to implement common user account management tasks, such as login, registration, and password reset. |
| `Microsoft.VisualStudio.Web.CodeGeneration.Design` | Provides classes and interfaces that are used to generate code for ASP.NET Core applications. Used for scaffolding to generate code for adding authentication. |
| `Microsoft.EntityFrameworkCore.Design` | Provides classes and interfaces that are used to design and manage Entity Framework Core models. |
| `Microsoft.EntityFrameworkCore.Proxies` | Provides classes and interfaces that are used to implement lazy loading. |
| `Microsoft.EntityFrameworkCore.Sqlite` | Provides classes and interfaces that are used to interact with SQLite databases. |
| `Microsoft.EntityFrameworkCore.SqlServer` | Provides classes and interfaces that are used to interact with SQL Server databases. |
| `Microsoft.EntityFrameworkCore.Tools` | Provides tools that can be used to manage Entity Framework Core models and databases. |
| `Serilog.Extensions.Logging.File` | Provides classes and interfaces that are used to log to files. |
| `Jdenticon.AspNetCore` | Provides auto generation of avatars for users who doesn't have a profile picture. This generates a unique picture based on username. |
| `Markdig` | This import provides markdown support for posts. |