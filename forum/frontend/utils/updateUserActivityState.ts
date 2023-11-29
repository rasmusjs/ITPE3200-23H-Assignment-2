export default async () => {
  const { data } = await genericFetch({
    method: 'GET',
    url: 'http://localhost:5112/api/Account/UserActivity',
  });

  const userActivity = useUserActivity();

  if (data.username) {
    userActivity.value = data;
  } else {
    userActivity.value = null;
  }
};
