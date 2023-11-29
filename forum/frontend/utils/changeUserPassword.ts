export default async (oldPassword: string, newPassword: string) => {
  return genericFetch({
    method: 'POST',
    url: 'http://localhost:5112/api/Account/changePassword',
    body: {
      oldPassword: oldPassword,
      newPassword: newPassword,
    },
  });
};
