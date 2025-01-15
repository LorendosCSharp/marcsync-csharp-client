using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MarcSync.Client;

public class Test
{
    public static void Main()
    {
        Console.WriteLine("Hello World!");
        Test2 test2 = new Test2();
        test2.Test().Wait();
    }
}

public class Test2
{
    public async Task Test()
    {
        MarcSyncClient marcSyncClient = new MarcSyncClient("eyJhbGciOiJIUzI1NiIsInR5cCI6Im1hcmNzeW5jQWNjZXNzIn0.eyJkYXRhYmFzZUlkIjoiMmU0ZTQxZmMtZjI4YS00ZWU2LTkyZTctZmZjZGVjMDMwZjQzIiwidXNlcklkIjoiNjNlNjc3MDUyMmJkMiIsInRva2VuSWQiOiI2Nzg4MjE3MGU2NTAzYjEyZGQ3YjA2MmIiLCJuYmYiOjE3MzY5NzQ3MDQsImV4cCI6ODgxMzY4ODgzMDQsImlhdCI6MTczNjk3NDcwNCwiaXNzIjoibWFyY3N5bmMifQ.oBlPgNnF-ZfQkxBqYHYF6ckr2Il2i7OLgBca2QaMfNQ");

        var collection = marcSyncClient.GetCollection("test");
        var entries = await collection.GetEntries();
        
        Console.WriteLine(JsonSerializer.Serialize(entries.First()));
    }
}
