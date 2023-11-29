export default async () => {
  return genericFetch({
    method: 'GET',
    url: 'http://localhost:5112/api/Post/posts',
  });
};
