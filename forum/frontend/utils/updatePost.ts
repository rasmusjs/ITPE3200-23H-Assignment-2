export default async (post: editPostBody) => {
  if (!(await checkLoginAndReroute())) {
    return;
  }

  const response = await genericFetch({
    url: 'http://localhost:5112/api/Post/UpdatePost',
    method: 'POST',
    body: post,
  });

  await updateUserActivityState();
  return response;
};
