export default async (id: number) => {
  return genericFetch({
    method: 'GET',
    url: `http://localhost:5112/api/Post/${id}`,
  });
};
