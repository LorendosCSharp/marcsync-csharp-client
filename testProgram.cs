using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarcSync.Client;

class Program
{
    static async Task Main(string[] args)
    {
        string accessToken = "tocken";
        MarcSyncClient marcSync = new MarcSyncClient(accessToken);

        string collectionName = "test_collection2";
        var collection = marcSync.GetCollection(collectionName);

        // Add test data
        var testEntries = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "name", "Alice" }, { "email", "alice@example.com" }, { "age", 30 } },
            new Dictionary<string, object> { { "name", "Bob" }, { "email", "bob@example.com" }, { "age", 25 } },
            new Dictionary<string, object> { { "name", "Charlie" }, { "email", "charlie@example.com" }, { "age", 35 } }
        };

        foreach (var entryData in testEntries)
        {
            try
            {
                await collection.CreateEntry(entryData);
                Console.WriteLine($"Entry added: {entryData["name"]}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error adding entry: {e.Message}");
            }
        }

        // Fetch and display entries
        try
        {
            var entries = await collection.GetEntries();
            Console.WriteLine("Entries in the collection:");
            foreach (var entry in entries)
            {
                Console.WriteLine(string.Join(", ", entry));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error fetching entries: {e.Message}");
        }
    }
}
