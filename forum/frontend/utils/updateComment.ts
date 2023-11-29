export default async (comment: updateCommentBody) => {
  if (!(await checkLoginAndReroute())) {
    return;
  }

  const response = await genericFetch({
    url: 'http://localhost:5112/api/Post/UpdateComment',
    method: 'POST',
    body: comment,
  });

  await updateAllPostsState();
  await updateUserActivityState();
  return response;
};
