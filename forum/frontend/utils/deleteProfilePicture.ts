export default async () => {
  if (!(await checkLoginAndReroute())) {
    return;
  }

  const response = await genericFetch({
    url: `http://localhost:5112/api/Account/removeProfilePicture`,
    method: 'GET',
  });

  await updateUserActivityState();
  return response;
};
