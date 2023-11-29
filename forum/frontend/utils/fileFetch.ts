interface FetchOptions {
  method: string;
  url: string;
  body: BodyInit;
}

interface FetchReturn {
  data: any;
  error: any;
  status: number | null;
}

export default async ({
  method,
  url,
  body,
}: FetchOptions): Promise<FetchReturn> => {
  const isBodyAllowed = ['POST', 'PUT'].includes(method);

  const options: RequestInit = {
    method,
    credentials: 'include',
    ...(isBodyAllowed && body ? { body: body } : {}),
  };

  try {
    const response = await fetch(url, options);
    return await processResponse(response);
  } catch (error) {
    console.error(`Error fetching ${url} with ${method}: `, error);
    return { data: null, error: error, status: null };
  }
};

const processResponse = async (response: Response) => {
  const responseText = await response.text();
  try {
    const responseJSON = JSON.parse(responseText);
    return { data: responseJSON, error: null, status: response.status };
  } catch (error) {
    if (!response.ok) {
      console.error(`Error in response: `, responseText);
      return { data: null, error: responseText, status: response.status };
    }
    return { data: responseText, error: null, status: response.status };
  }
};
