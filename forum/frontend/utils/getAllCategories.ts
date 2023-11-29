export default async () => {
  const { data: categoriesData } = await genericFetch({
    method: 'GET',
    url: 'http://localhost:5112/api/Post/GetCategories',
  });

  categoriesData.forEach((category: { url: string }) => {
    category.url = category.url.replace('../', 'http://localhost:5112/');
  });

  return categoriesData;
};
