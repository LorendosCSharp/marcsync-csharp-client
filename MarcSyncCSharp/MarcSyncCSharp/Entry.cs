using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MarcSync.Etr
{
    public class Entry
    {
        private readonly string accessToken;
        private readonly string collectionName;
        public Dictionary<string, object> Data { get; private set; }

        public Entry(string accessToken, string collectionName, Dictionary<string, object> data)
        {
            this.accessToken = accessToken;
            this.collectionName = collectionName;
            this.Data = data;
        }

        public async Task UpdateValue(string key, object value)
        {
            if (key == "_id")
                throw new Exception("Cannot update _id");

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", accessToken);

            var payload = new
            {
                filters = new { _id = Data["_id"] },
                data = new Dictionary<string, object> { { key, value } }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync($"https://api.marcsync.dev/v1/entries/{collectionName}", content);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new Exception("Invalid access token");

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                throw new Exception("Entry not found");

            if (response.StatusCode == (System.Net.HttpStatusCode)429)
                throw new Exception("Rate limit exceeded");

            Data[key] = value;
        }

        public async Task Delete()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", accessToken);

            var payload = new { filters = new { _id = Data["_id"] } };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Delete, $"https://api.marcsync.dev/v1/entries/{collectionName}")
            {
                Content = content
            };

            var response = await httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new Exception("Invalid access token");

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                throw new Exception("Entry not found");

            if (response.StatusCode == (System.Net.HttpStatusCode)429)
                throw new Exception("Rate limit exceeded");

            if (!response.IsSuccessStatusCode)
                throw new Exception("Unknown error while deleting entry");
        }
    }
}
