export default async (loginData: loginData) => {
  return await genericFetch({
    method: 'POST',
    url: 'http://localhost:5112/api/Account/login',
    body: loginData,
  });
};
