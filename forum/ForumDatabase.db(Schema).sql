BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS "ApplicationUserComment" (
	"LikedCommentsCommentId"	INTEGER NOT NULL,
	"UserLikesId"	TEXT NOT NULL,
	CONSTRAINT "PK_ApplicationUserComment" PRIMARY KEY("LikedCommentsCommentId","UserLikesId"),
	CONSTRAINT "FK_ApplicationUserComment_AspNetUsers_UserLikesId" FOREIGN KEY("UserLikesId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE,
	CONSTRAINT "FK_ApplicationUserComment_Comments_LikedCommentsCommentId" FOREIGN KEY("LikedCommentsCommentId") REFERENCES "Comments"("CommentId") ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS "ApplicationUserPost" (
	"LikedPostsPostId"	INTEGER NOT NULL,
	"UserLikesId"	TEXT NOT NULL,
	CONSTRAINT "PK_ApplicationUserPost" PRIMARY KEY("LikedPostsPostId","UserLikesId"),
	CONSTRAINT "FK_ApplicationUserPost_AspNetUsers_UserLikesId" FOREIGN KEY("UserLikesId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE,
	CONSTRAINT "FK_ApplicationUserPost_Posts_LikedPostsPostId" FOREIGN KEY("LikedPostsPostId") REFERENCES "Posts"("PostId") ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS "AspNetRoleClaims" (
	"Id"	INTEGER NOT NULL,
	"RoleId"	TEXT NOT NULL,
	"ClaimType"	TEXT,
	"ClaimValue"	TEXT,
	CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY("RoleId") REFERENCES "AspNetRoles"("Id") ON DELETE CASCADE,
	CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY("Id" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS "AspNetRoles" (
	"Id"	TEXT NOT NULL,
	"Name"	TEXT,
	"NormalizedName"	TEXT,
	"ConcurrencyStamp"	TEXT,
	CONSTRAINT "PK_AspNetRoles" PRIMARY KEY("Id")
);
CREATE TABLE IF NOT EXISTS "AspNetUserClaims" (
	"Id"	INTEGER NOT NULL,
	"UserId"	TEXT NOT NULL,
	"ClaimType"	TEXT,
	"ClaimValue"	TEXT,
	CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE,
	CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY("Id" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS "AspNetUserLogins" (
	"LoginProvider"	TEXT NOT NULL,
	"ProviderKey"	TEXT NOT NULL,
	"ProviderDisplayName"	TEXT,
	"UserId"	TEXT NOT NULL,
	CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY("LoginProvider","ProviderKey"),
	CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS "AspNetUserRoles" (
	"UserId"	TEXT NOT NULL,
	"RoleId"	TEXT NOT NULL,
	CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY("UserId","RoleId"),
	CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY("RoleId") REFERENCES "AspNetRoles"("Id") ON DELETE CASCADE,
	CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS "AspNetUserTokens" (
	"UserId"	TEXT NOT NULL,
	"LoginProvider"	TEXT NOT NULL,
	"Name"	TEXT NOT NULL,
	"Value"	TEXT,
	CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY("UserId","LoginProvider","Name"),
	CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS "AspNetUsers" (
	"Id"	TEXT NOT NULL,
	"CreationDate"	TEXT NOT NULL,
	"ProfilePicture"	BLOB,
	"UserName"	TEXT,
	"NormalizedUserName"	TEXT,
	"Email"	TEXT,
	"NormalizedEmail"	TEXT,
	"EmailConfirmed"	INTEGER NOT NULL,
	"PasswordHash"	TEXT,
	"SecurityStamp"	TEXT,
	"ConcurrencyStamp"	TEXT,
	"PhoneNumber"	TEXT,
	"PhoneNumberConfirmed"	INTEGER NOT NULL,
	"TwoFactorEnabled"	INTEGER NOT NULL,
	"LockoutEnd"	TEXT,
	"LockoutEnabled"	INTEGER NOT NULL,
	"AccessFailedCount"	INTEGER NOT NULL,
	CONSTRAINT "PK_AspNetUsers" PRIMARY KEY("Id")
);
CREATE TABLE IF NOT EXISTS "Categories" (
	"CategoryId"	INTEGER NOT NULL,
	"Name"	TEXT NOT NULL,
	"Color"	TEXT NOT NULL,
	"PicturePath"	TEXT,
	CONSTRAINT "PK_Categories" PRIMARY KEY("CategoryId" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS "Comments" (
	"CommentId"	INTEGER NOT NULL,
	"Content"	TEXT NOT NULL,
	"TotalLikes"	INTEGER NOT NULL,
	"DateCreated"	TEXT NOT NULL,
	"DateLastEdited"	TEXT,
	"PostId"	INTEGER NOT NULL,
	"UserId"	TEXT,
	"ParentCommentId"	INTEGER,
	CONSTRAINT "FK_Comments_AspNetUsers_UserId" FOREIGN KEY("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE SET NULL,
	CONSTRAINT "FK_Comments_Comments_ParentCommentId" FOREIGN KEY("ParentCommentId") REFERENCES "Comments"("CommentId"),
	CONSTRAINT "FK_Comments_Posts_PostId" FOREIGN KEY("PostId") REFERENCES "Posts"("PostId") ON DELETE CASCADE,
	CONSTRAINT "PK_Comments" PRIMARY KEY("CommentId" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS "PostTag" (
	"PostsPostId"	INTEGER NOT NULL,
	"TagsTagId"	INTEGER NOT NULL,
	CONSTRAINT "PK_PostTag" PRIMARY KEY("PostsPostId","TagsTagId"),
	CONSTRAINT "FK_PostTag_Posts_PostsPostId" FOREIGN KEY("PostsPostId") REFERENCES "Posts"("PostId") ON DELETE CASCADE,
	CONSTRAINT "FK_PostTag_Tags_TagsTagId" FOREIGN KEY("TagsTagId") REFERENCES "Tags"("TagId") ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS "Posts" (
	"PostId"	INTEGER NOT NULL,
	"Title"	TEXT NOT NULL,
	"Content"	TEXT NOT NULL,
	"TotalLikes"	INTEGER NOT NULL,
	"DateCreated"	TEXT NOT NULL,
	"DateLastEdited"	TEXT,
	"UserId"	TEXT,
	"CategoryId"	INTEGER NOT NULL,
	CONSTRAINT "FK_Posts_AspNetUsers_UserId" FOREIGN KEY("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE SET NULL,
	CONSTRAINT "FK_Posts_Categories_CategoryId" FOREIGN KEY("CategoryId") REFERENCES "Categories"("CategoryId") ON DELETE CASCADE,
	CONSTRAINT "PK_Posts" PRIMARY KEY("PostId" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS "Tags" (
	"TagId"	INTEGER NOT NULL,
	"Name"	TEXT NOT NULL,
	CONSTRAINT "PK_Tags" PRIMARY KEY("TagId" AUTOINCREMENT)
);
CREATE INDEX IF NOT EXISTS "EmailIndex" ON "AspNetUsers" (
	"NormalizedEmail"
);
CREATE INDEX IF NOT EXISTS "IX_ApplicationUserComment_UserLikesId" ON "ApplicationUserComment" (
	"UserLikesId"
);
CREATE INDEX IF NOT EXISTS "IX_ApplicationUserPost_UserLikesId" ON "ApplicationUserPost" (
	"UserLikesId"
);
CREATE INDEX IF NOT EXISTS "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" (
	"RoleId"
);
CREATE INDEX IF NOT EXISTS "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" (
	"UserId"
);
CREATE INDEX IF NOT EXISTS "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" (
	"UserId"
);
CREATE INDEX IF NOT EXISTS "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" (
	"RoleId"
);
CREATE INDEX IF NOT EXISTS "IX_Comments_ParentCommentId" ON "Comments" (
	"ParentCommentId"
);
CREATE INDEX IF NOT EXISTS "IX_Comments_PostId" ON "Comments" (
	"PostId"
);
CREATE INDEX IF NOT EXISTS "IX_Comments_UserId" ON "Comments" (
	"UserId"
);
CREATE INDEX IF NOT EXISTS "IX_PostTag_TagsTagId" ON "PostTag" (
	"TagsTagId"
);
CREATE INDEX IF NOT EXISTS "IX_Posts_CategoryId" ON "Posts" (
	"CategoryId"
);
CREATE INDEX IF NOT EXISTS "IX_Posts_UserId" ON "Posts" (
	"UserId"
);
CREATE UNIQUE INDEX IF NOT EXISTS "RoleNameIndex" ON "AspNetRoles" (
	"NormalizedName"
);
CREATE UNIQUE INDEX IF NOT EXISTS "UserNameIndex" ON "AspNetUsers" (
	"NormalizedUserName"
);
COMMIT;
