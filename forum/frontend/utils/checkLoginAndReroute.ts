export default async (): Promise<boolean> => {
  const userActivity = useUserActivity();
  const router = useRouter();

  if (!userActivity.value) {
    await router.push('/login');
    return false;
  }

  return true;
};
