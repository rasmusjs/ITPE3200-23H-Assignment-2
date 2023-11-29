// State for managing posts
export const useAllPosts = () => useState<post[]>('allPosts', () => []);

// State for managing UserActivity
export const useUserActivity = () =>
  useState<UserActivity | null>('userActivity', () => null);
