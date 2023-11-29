BracketBros
======================

* * *

BracketBros is a programming discussion forum made for the ITPE3200 portfolio exam (project 1). The forum is made with
.NET Core 6.0, C# and with a MVC framework. This is a MVP solution where the focus is the back-end. The front-end is
made with HTML, CSS, JavaScript and jQuery for input validation.

Frontend repository:
[https://github.com/ole-kristian-rudjord/BracketBros-frontend
](https://github.com/ole-kristian-rudjord/BracketBros-frontend)

**⚡ Quick start**
---

When starting the server you will be sent to the post view, where there is an chronological feed with forum posts. You
can then browse posts, see comments by going to each posts or search for specific posts.

To interact with the site, you need to register a user. Register a username, email and password. The password must
consist of at least 8 characters, one alphanumeric character, one lowercase and one uppercase letter.

`You can also log into the account "Hackerman" with the password "Password123!"`

When you have logged in you can create/edit posts, comment and like posts. You get a dashboard with overview of your
post, comments and likes. In the upper right corner you have your profile panel. Here you can change username, upload a
profile picture, change mail/password, enable 2FA and download all your user data. You can delete your user and data
under "personal data". Then all your posts and comments will be marked as posted by Anonymous.

### Admin functionality

` For admin priviliges you can login with user «torkratte» and password «Password123!».`

We have an admin role which has access to an admin dashboard. Here the user can edit, delete or add new categories and
tags. They can change both the color, name and picture. An admin can also delete all posts and comments, while a regular
user can only delete its own content.

**📝 Functionality**
---
This forum website allows users to browse posts and comment. Both in a feed, by searching or by using categories and
tags. The feed can be sorted after different criterias. The user can register an account to create posts, comment on
other posts and like posts. When the user is logged in it gets a dashboard view with an overview over the users posts,
comments and likes. We have admin users with extra functionality. They have their own admin dashboard where they can
change categories and tags. There is a user dashboard where the user can change their information and upload a custom
profile picture.


**🔨 Project architecture**
---

BracketBros is built using the Model-View-Controller (MVC) architecture as required for this task. This is the
architecture we would have chosen anyways, as it offers different advantages like seperation of the different
components, easy to maintain and scale, reusable code and good support in .NET Core to mention some.

### Architectural layers

| Layer       | Description                                                                                                                                                                                                                                                           |
|-------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Areas       | This layer contains the user account management functionality (auto generated from the 'Microsoft.AspNetCore.Identity.UI' package).                                                                                                                                   |
| Controllers | This layer contains the controllers that handle HTTP requests. When a user requests a page, the corresponding controller is responsible for returning the appropriate response. For example, the ForumController is responsible for handling requests to forum pages. |
| Models      | This layer contains the models that represent the data in the application. For example, the Forum model represents a forum and the Post model represents a post in a forum.                                                                                           |
| Views       | This layer contains the Razor Pages that are used to render the user interface. When a controller needs to render a page, it passes the appropriate model to the view. The view then uses the model to generate the HTML output for the page.                         |
| DAL         | Data Access Layer - This layer contains the code that interacts with the database. This code is responsible for performing CRUD operations on the data in the database.                                                                                               |
| ViewModel   | This layer contains the classes that represent the data that is passed to the views. The ViewModels are used to simplify the code in the views and make them more reusable                                                                                            |

### Static resources

| Component | Description                                                                                                                                                                                         |
|-----------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| wwwroot   | This houses the static files that are served to the client, such as CSS and JavaScript files. These files are served directly to the client without any processing by the ASP.NET Core application. |

### Supporting components

| Component  | Description                                                                                                                                                                     |
|------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Migrations | Contains the changes to our database schema. These are auto generated. They allow us to version our database schema in a way that corresponds with the changes in our codebase. |
| Logs       | Contains error and information logging for debugging and error handling.                                                                                                        |

### Diagrams

<p align="center">
    <img src="./forum/wwwroot/images/documentation/webapp_architecture.png" alt="Diagram of the application architecture" width="auto" height="auto">
  Diagram of the application architecture
</p>

<p align="center">
    <img src="./forum/wwwroot/images/documentation/ForumDatabase.png" alt="Entity Relationship diagram of the database" width="auto" height="auto">
  Entity Relationship diagram of the database
</p>

###   

**⌨️ Code**
---
We have tried to follow the C# coding convention for syntax and naming conventions to make sure the code is readable and
managable. However, we haven't focused much on this, so the code might sometimes break conventions.

The code is documented with comments throughout the project. The code should either be self explanatory or explained
with comments. We have also provided comments with links where we have found inspiration or code from the internet.

### Packages

These are the packages that we have imported to our project and a description of what they do.

| Package                                             | Description                                                                                                                                                                  |
|-----------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | Provides classes and interfaces that are used to implement user authentication and authorization in ASP.NET Core applications.                                               |
| `Microsoft.AspNetCore.Identity.UI`                  | Provides Razor Pages and components that are used to implement common user account management tasks, such as login, registration, and password reset.                        |
| `Microsoft.VisualStudio.Web.CodeGeneration.Design`  | Provides classes and interfaces that are used to generate code for ASP.NET Core applications. Used for scaffolding to generate code for adding authentication.               |
| `Microsoft.EntityFrameworkCore.Design`              | Provides classes and interfaces that are used to design and manage Entity Framework Core models.                                                                             |
| `Microsoft.EntityFrameworkCore.Proxies`             | Provides classes and interfaces that are used to implement lazy loading.                                                                                                     |
| `Microsoft.EntityFrameworkCore.Sqlite`              | Provides classes and interfaces that are used to interact with SQLite databases.                                                                                             |
| `Microsoft.EntityFrameworkCore.SqlServer`           | Provides classes and interfaces that are used to interact with SQL Server databases.                                                                                         |
| `Microsoft.EntityFrameworkCore.Tools`               | Provides tools that can be used to manage Entity Framework Core models and databases.                                                                                        |
| `Serilog.Extensions.Logging.File`                   | Provides classes and interfaces that are used to log to files.                                                                                                               |
| `Jdenticon.AspNetCore`                              | Provides auto generation of avatars for users who doesn't have a profile picture. This generates a unique picture based on username.                                         |
| `Markdig`                                           | This import provides markdown support for writing posts.                                                                                                                     |
| `HtmlSanitizer`                                     | This import is used to sanitize HTML when writing in Markdown. This is to provide just basic Markdown support and to not open up for potential injections or malicious code. |
