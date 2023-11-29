export default async (registerData: registerData) => {
  const response = await genericFetch({
    method: 'POST',
    url: 'http://localhost:5112/api/Account/register',
    body: registerData,
  });

  await updateUserActivityState();
  return response;
};
