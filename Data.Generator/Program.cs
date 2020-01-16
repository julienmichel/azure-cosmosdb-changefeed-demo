using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Data.Generator
{
    public class Program
    {
        const string endpoint = "{FILL_ME}";
        const string apikey = "{FILL_ME}";
        public static async Task Main(string[] args)
        {
            using CosmosClient client = new CosmosClient(endpoint, apikey);
            Database db = await client.CreateDatabaseIfNotExistsAsync("StoreDB", 1000);
            Container container = await db.CreateContainerIfNotExistsAsync("CartContainer", "/location");

            var itemsingest = new Bogus.Faker<Item>()
                .RuleFor(o => o.id, f => f.Random.Guid().ToString())
                .RuleFor(o => o.name, f => f.Commerce.ProductName())
                .RuleFor(o => o.price, f => Math.Round(f.Random.Decimal(5m, 150m), 2))
                .RuleFor(o => o.location, f => f.Address.StateAbbr());

            IEnumerable<Item> items = itemsingest.GenerateLazy(100);
            foreach (Item item in items)
            {
                Item resp = await container.CreateItemAsync(item, new PartitionKey(item.location));
                Console.WriteLine(resp.id + " - " + resp.location + " - " + resp.name);
            }

        }
    }
}
