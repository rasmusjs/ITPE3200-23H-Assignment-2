BracketBros
======================

* * *

BracketBros is a programming discussion forum made for the ITPE3200 portfolio exam (project 1). The forum is made with .NET Core 6.0, C# and with a MVC framework. This is a MVP solution where the focus is the back-end. The front-end is made with ~~basic Bootstrap~~.

**⚡ Quick start**
---

When starting the server you will be sent to the post view, where there is an chronological feed with forum posts. You can then browse posts, see comments by going to each posts or search for specific posts.

To interact with the site, you need to register a user. Register a username, email and password. The password must consist of at least 8 characters, one alphanumeric character, one lowercase and one uppercase letter. 

When you have logged in you can create/edit posts, comment and like posts. You get a dashboard with overview of your post, comments and likes. In the upper right corner you have your profile panel. Here you can change username, upload a profile picture, change mail/password, enable 2FA and download all your user data. You can delete your user and data under "personal data". Then all your posts and comments will be marked as posted by Anonymous.

**🔨 Project architecture**
---

BracketBros is built using the Model-View-Controller (MVC) architecture as required for this task. This is the architecture we would have chosen anyways, as it offers different advantages like seperation of the different components, easy to maintain and scale, reusable code and good support in .NET Core to mention some.

The following sections break down the architecture into its different layers and component and providing detail for how they contribute to the application as a whole.

*The tables needs to be rewritten to provide more concrete example of use.*

### Architectural layers

| Layer | Description |
|---|---|
| Areas | This layer contains the user account management functionality (auto generated from the 'Microsoft.AspNetCore.Identity.UI' package). |
| Controllers | This layer contains the controllers that handle HTTP requests. When a user requests a page, the corresponding controller is responsible for returning the appropriate response. For example, the ForumController is responsible for handling requests to forum pages. |
| Models | This layer contains the models that represent the data in the application. For example, the Forum model represents a forum and the Post model represents a post in a forum.|
| Views | This layer contains the Razor Pages that are used to render the user interface. When a controller needs to render a page, it passes the appropriate model to the view. The view then uses the model to generate the HTML output for the page. |
| DAL | Data Access Layer - This layer contains the code that interacts with the database. This code is responsible for performing CRUD operations on the data in the database.|
| ViewModel | This layer contains the classes that represent the data that is passed to the views. The ViewModels are used to simplify the code in the views and make them more reusable |

### Static resources

| Component | Description |
|---|---|
| wwwroot | This houses the static files that are served to the client, such as CSS and JavaScript files. These files are served directly to the client without any processing by the ASP.NET Core application. |

### Supporting components

| Component | Description |
|---|---|
| Migrations | Contains the changes to our database schema. These are auto generated. They allow us to version our database schema in a way that corresponds with the changes in our codebase. |
| Logs | Contains error logging. What do we log??? How??? |

### Diagrams

_**UMLs GOES HERE**_

### Views

Something cool about the views. discuss why you chose a particular structure or design pattern for your views.ViewModels. Partial views? How data is passed to the views. Layouts. 

### Database
Something about how the database works

### Design
Spør Ole

**📝 Functionality**
---
This forum website allows users to browse posts and comment. Both in a feed, by searching or by using categories. The user can register an account to create posts, comment on other posts and like posts. When the user is logged in it gets a dashboard view with an overview over the users posts, comments and likes. The following sections break down the functionality into different aspects.

### Navigation
Different feed views. What the user can do with posts (edit, delete, like, etc). Menu and navigation bars. Search functionality. Pagination and scrolling, filtering and sorting, responsiveness (different screens?), dynamic features, call to action (sent to register account), browsing by categories. Markdown support

### Content
ALL about posts baby 

### Error handling
Error handling, input validation authentication/authorization, etc.

Identity, try/catches in ForumRepository, error logging with LogError method, regex for input validation + html sanitizer

### User management

We use `Microsoft.AspNetCore.Identity` for authentication and authorization. This is an ASP.NET Core API that supports user interface for login functionality. It manages users, passwords, profile data and more. Identity creates a Razor Class Library with the 'Identity' area endpoint, e.g. '/Identity/Account/Login'. The user is not authorized to do anything other than to browse the forum without an user account. The user must register with a username, email and password. The password must consist of at least 8 characters, one alphanumeric character, one lowercase and one uppercase letter. 

When a user is authenticated it can create/edit posts, comment and like posts. The user has access to a dashboard with overview of their post, comments and likes. In the upper right corner they have your profile panel. Here they can change username, upload a profile picture, change mail/password, enable 2FA and download all their user data. They can delete their user and data under "personal data". Then all the users posts and comments will be marked as posted by 'Anonymous'.

We use `Jdenticon.AspNetCore` for generating random profile pictures if the user has not uploaded one. The image is generated by converting the username into a hash, which is used as a unique id for generation. The user can later upload a custom profile picture in their profile settings.

**⌨️ Code**
---
We have tried to follow the C# coding convention for syntax and naming conventions to make sure the code is readable and managable. However, we haven't focused much on this, so the code might sometimes break conventions.

*If there's something unique or innovative about the way you've coded the project, this is the place to mention it.*

The project is structured in a typical MVC fashion. We have different directories for each layer, components and resources. Controllers are located in /Controllers, user account management under /Area, the different views in /Views, ViewModels under /ViewModels, static files like CSS, HTML and Javascript under /wwwroot, data access layer under /DAL. 

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
| `Markdig` | This import provides markdown support for writing posts. |
| `HtmlSanitizer` | This import is used to sanitize HTML when writing in Markdown. This is to provide just basic Markdown support and to not open up for potential injections or malicious code. |