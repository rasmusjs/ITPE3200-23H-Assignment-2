export default async () => {
  const allPosts = useAllPosts();

  const getAllPosts_response = await getAllPosts();
  if (getAllPosts_response.data) {
    allPosts.value = getAllPosts_response.data;
  }
};
