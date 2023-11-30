BracketBros
======================

* * *

BracketBros is a programming discussion forum made for the ITPE3200 portfolio exam (project 2). It is inspired by classic BB Forums. The forum is made with .NET Core 6.0 and C# in the backend. The front-end is made with Nuxt.js, Vue.js, Typescript, Vuetify and Sass. 

To see our commits you can visit our frontend repository:
[https://github.com/ole-kristian-rudjord/BracketBros-frontend
](https://github.com/ole-kristian-rudjord/BracketBros-frontend)

### Prerequisites

To run this project you need to have npm or a equivalent JavaScript packet manager. You can download it from [Node.js](https://nodejs.org/en). We used Node version v20.10.0 and npm version
10.2.3  After this is installed you need to run:

```bash
# Move to correct directory 
cd forum/frontend

# install dependencies  
npm install

# build the solution
npm run build
```

After this is done you can now start run the solution as you would normaly.

### **⚡ Quick start**

When starting the server you will should be sent to the index home page. If not navigate to [http://localhost:3000/](http://localhost:3000/). Here you can click on the the different categories or tags to search posts containing them. The feed is an chronological feed with forum posts. You can then browse posts, see comments by going to each posts or search for specific posts. To interact with the site, you need to register a user. Register a username, email and password. The password must consist of at least 8 characters, one alphanumeric character, one lowercase and one uppercase letter (Own work, 2023).

`You can also log into the account "Hackerman" with the password "Password123!"`

When you have logged in you can create/edit posts, comment and like posts. You get a dashboard with overview of your post, comments and likes. In the upper right corner you have your profile panel. Here you can  upload a profile picture and change your password (Own work, 2023).

### Admin functionality

` For admin priviliges you can login with user «torkratte» and password «Password123!».`  For normal user access you can make a new user by registering.

We have an admin role which has access to an admin dashboard. Admins can delete all posts and comments, while a regular user can only delete and edits its own content. (Own work, 2023)

### Sources

This report is based on our previous report and we will be self-referencing this:
Own work. (2023). Bracket Bros: Exam 1 project report [Unpuplished exam paper]. OsloMet