using System.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class Collection
{
    private readonly string accessToken;
    private string collectionName;
    private readonly HttpClient httpClient;

    public Collection(string accessToken, string collectionName)
    {
        this.accessToken = accessToken;
        this.collectionName = collectionName;
        this.httpClient = new HttpClient();
        this.httpClient.DefaultRequestHeaders.Add("Authorization", accessToken);
    }

    // Helper method for checking the response status codes
    private async Task CheckResponseStatus(HttpResponseMessage response)
    {
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new Exception("Invalid access token");
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            throw new Exception("Collection not found");
        if (response.StatusCode == (System.Net.HttpStatusCode)429)
            throw new Exception("Rate limit exceeded");
        if (!response.IsSuccessStatusCode)
            throw new Exception("Unknown error occurred");
    }

    // Method to create an entry and return an Entry object
    public async Task<Entry> CreateEntry(Dictionary<string, object> data)
    {
        if (data.ContainsKey("_id"))
            throw new Exception("Cannot set _id while creating entry");

        var payload = new { data };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync($"https://api.marcsync.dev/v0/entries/{collectionName}", content);
        await CheckResponseStatus(response);

        var responseBody = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);
        data["_id"] = responseData["objectId"];

        return new Entry(accessToken, collectionName, data);
    }

    // Method to get entries
    public async Task<List<Entry>> GetEntries(Dictionary<string, object> filters = null)
    {
        filters ??= new Dictionary<string, object>();

        var payload = new { filters };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await httpClient.PatchAsync($"https://api.marcsync.dev/v1/entries/{collectionName}?methodOverwrite=GET", content);
        await CheckResponseStatus(response);

        var responseBody = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);
        return JsonSerializer.Deserialize<List<Dictionary<string, object>>>(responseData["entries"].ToString())
            .Select(entry => new Entry(this.accessToken, collectionName, entry)).ToList();
    }

    public async Task<int> DeleteEntry(string entryId)
    {
        var filters = new Dictionary<string, object>();
        filters.Add("_id", entryId);

        var payload = new { filters };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await httpClient.PatchAsync($"https://api.marcsync.dev/v1/entries/{collectionName}?methodOverwrite=DELETE", content);
        await CheckResponseStatus(response);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);
            return int.Parse(responseData["deletedEntries"].ToString());
    }
    
    public async Task<int> DeleteEntries(Dictionary<string, object> filters = null)
    {
        filters ??= new Dictionary<string, object>();

        var payload = new { filters };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await httpClient.PatchAsync($"https://api.marcsync.dev/v1/entries/{collectionName}?methodOverwrite=DELETE", content);
        await CheckResponseStatus(response);

        var responseBody = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);
        return int.Parse(responseData["deletedEntries"].ToString());
    }
    
    public string GetName()
    {
        return collectionName;
    }
}
