namespace MarcSync.Etr
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class Entry
    {
        private readonly string accessToken;
        private readonly string collectionName;
        public Dictionary<string, object> Data { get; private set; }
        private readonly HttpClient httpClient;

        public Entry(string accessToken, string collectionName, Dictionary<string, object> data)
        {
            this.accessToken = accessToken;
            this.collectionName = collectionName;
            this.Data = data;
            this.httpClient = new HttpClient();
            this.httpClient.DefaultRequestHeaders.Add("Authorization", accessToken);
        }

        // Helper method for checking response status
        private async Task CheckResponseStatus(HttpResponseMessage response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new Exception("Invalid access token");
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                throw new Exception("Entry not found");
            if (response.StatusCode == (System.Net.HttpStatusCode)429)
                throw new Exception("Rate limit exceeded");
            if (!response.IsSuccessStatusCode)
                throw new Exception("Unknown error occurred");
        }

        // Update a single field in the entry
        public async Task UpdateValue(string key, object value)
        {
            if (key == "_id")
                throw new Exception("Cannot update _id");

            var payload = new
            {
                filters = new { _id = Data["_id"] },
                data = new Dictionary<string, object> { { key, value } }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync($"https://api.marcsync.dev/v1/entries/{collectionName}", content);
            await CheckResponseStatus(response);

            Data[key] = value;
        }

        // Update multiple fields in the entry
        public async Task UpdateValues(Dictionary<string, object> values)
        {
            if (values.ContainsKey("_id"))
                throw new Exception("Cannot update _id");

            var payload = new
            {
                filters = new { _id = Data["_id"] },
                data = values
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync($"https://api.marcsync.dev/v1/entries/{collectionName}", content);
            await CheckResponseStatus(response);

            foreach (var key in values.Keys)
            {
                Data[key] = values[key];
            }
        }

        // Delete this entry
        public async Task Delete()
        {
            var payload = new { filters = new { _id = Data["_id"] } };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"https://api.marcsync.dev/v1/entries/{collectionName}")
            {
                Content = content
            });

            await CheckResponseStatus(response);
        }
    }
}
