using System.Net.Http.Headers;

namespace Services.Helpers
{
    public static class ApiHelper
    {
        public static HttpClient ApiClient { get; set; }

        public static void InitializeClient() 
        { 
            ApiClient = new HttpClient(); 
            ApiClient.DefaultRequestHeaders.Accept.Clear();
        }
    }
}
