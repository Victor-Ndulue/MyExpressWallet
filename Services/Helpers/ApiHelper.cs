using System.Net.Http.Headers;

namespace Services.Helpers
{
    public static class ApiHelper
    {      
        public static async Task<HttpResponseMessage> PostRequestAsync(string apiUrl, HttpContent data, string authorization)
        {
            using (var apiClient = new HttpClient())
            {
                //apiClient.DefaultRequestHeaders.Clear();
                apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorization);
                apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                return await apiClient.PostAsync(apiUrl, data);
            }
        }
    }
}
