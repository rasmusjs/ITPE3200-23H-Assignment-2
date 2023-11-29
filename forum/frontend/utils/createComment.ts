export default async (comment: createCommentBody) => {
  const response = await genericFetch({
    url: 'http://localhost:5112/api/Post/CreateComment',
    method: 'POST',
    body: comment,
  });

  await updateAllPostsState();
  await updateUserActivityState();
  return response;
};
