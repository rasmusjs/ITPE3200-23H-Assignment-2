export default async (post: createPostBody) => {
  const response = await genericFetch({
    url: 'http://localhost:5112/api/Post/createPost',
    method: 'POST',
    body: post,
  });

  await updateAllPostsState();
  await updateUserActivityState();
  return response;
};
