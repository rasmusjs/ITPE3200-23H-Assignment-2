export default async () => {
  return await genericFetch({
    method: 'GET',
    url: 'http://localhost:5112/api/Account/GetUserComments',
  });
};
