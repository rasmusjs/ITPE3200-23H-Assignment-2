export default async (id: number) => {
  if (!await checkLoginAndReroute()) {
    return;
  }

  const response = await genericFetch({
    method: 'GET',
    url: `http://localhost:5112/api/Post/LikePost/${id}`,
  });

  await updateUserActivityState();
  return response;
};
