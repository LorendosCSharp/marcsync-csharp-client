namespace MarcSync.Coll
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using MarcSync.Etr;

    public class Collection
    {
        private readonly string accessToken;
        private string collectionName;

        public Collection(string accessToken, string collectionName)
        {
            this.accessToken = accessToken;
            this.collectionName = collectionName;
        }

        public async Task CreateEntry(Dictionary<string, object> data)
        {
            if (data.ContainsKey("_id"))
                throw new Exception("Cannot set _id while creating entry");

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", accessToken);

            var payload = new { data };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"https://api.marcsync.dev/v0/entries/{collectionName}", content);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new Exception("Invalid access token");

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                throw new Exception("Collection not found");

            if (response.StatusCode == (System.Net.HttpStatusCode)429)
                throw new Exception("Rate limit exceeded");

            if (!response.IsSuccessStatusCode)
                throw new Exception("Unknown error while creating entry");

            var responseBody = await response.Content.ReadAsStringAsync();
            var responseData = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);

            data["_id"] = responseData["objectId"];
        }

        public async Task<List<Dictionary<string, object>>> GetEntries(Dictionary<string, object> filters = null)
        {
            filters ??= new Dictionary<string, object>();

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", accessToken);

            var payload = new { filters };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await httpClient.GetAsync($"https://api.marcsync.dev/v0/entries/{collectionName}");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new Exception("Invalid access token");

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                throw new Exception("Collection not found");

            if (response.StatusCode == (System.Net.HttpStatusCode)429)
                throw new Exception("Rate limit exceeded");

            var responseBody = await response.Content.ReadAsStringAsync();
            var responseData = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);

            var entries = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(responseData["entries"].ToString());
            return entries;
        }

        public string GetName()
        {
            return collectionName;
        }
    }
}
