export {};

declare global {
  interface UserActivity {
    username: string;
    profilePicture: string | null;
    role: "User" | "Admin";
    createationdate: string;
    posts: number[];
    likedPosts: number[];
    savedPosts: number[];
    comments: number[];
    likedComments: number[];
    savedComments: number[];
  }

  interface user {
    username: string;
    profilePicture: string | null;
    role: "User" | "Admin";
    creationdate: string;
    posts: int[] | null;
    likedPosts: int[] | null;
    savedPosts: int[] | null;
    comments: int[] | null;
    likedComments: int[] | null;
    savedComments: int[] | null;
  }

  interface category {
    categoryId: number;
    name: string;
    color: string;
    url: string;
  }

  interface tag {
    tagId: number;
    name: string;
  }

  interface post {
    id: number;
    user: user;
    dateCreated: string;
    dateLastEdited: string;
    category: category;
    tags: tag[];
    title: string;
    content: string;
    totalLikes: number;
    totalComments: number;
  }

  interface comment {
    commentId: number;
    parentCommentId: number;
    content: string;
    totalLikes: number;
    dateCreated: string;
    dateLastEdited: string | null;
    postId: number;
    user: user;
    isLiked: boolean;
    commentReplies: comment[];
  }

  interface loginData {
    Identifier: string;
    Password: string;
    RememberMe: boolean;
  }

  interface registerData {
    email: string;
    username: string;
    password: string;
  }

  interface createPostBody {
    Title: string;
    CategoryId: number;
    TagsId: number[];
    Content: string;
  }
  interface editPostBody extends createPostBody {
    id: number;
  }

  interface createCommentBody {
    ParentCommentId: number | null;
    PostId: number;
    Content: string;
  }

  interface updateCommentBody extends createCommentBody {
    commentId: number;
  }
}
