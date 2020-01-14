using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Data.Processor
{
    class Program
    {
        const string endpoint = "https://jmisql.documents.azure.com:443/";
        const string apikey = "xQKHTC9oNwGkxZZVhnhpKQI0RptgzezbK8ZiKAcxG1W9Ezyx35UjLUr9SNp1oxujKs9xfcBKPFiXUfBzrDsogQ==";
        public static async Task Main(string[] args)
        {
            using CosmosClient client = new CosmosClient(endpoint, apikey);
            Database db = client.GetDatabase("StoreDB");
            Container container = db.GetContainer("CartContainer");
            Container leasecontainer = await db.CreateContainerIfNotExistsAsync("Lease", "/id");

            ChangeFeedProcessorBuilder builder = container.GetChangeFeedProcessorBuilder(
                "sampleProcessor",
                async (IReadOnlyCollection<Item> changes, CancellationToken cancellationToken) =>
                {
                    await Console.Out.WriteLineAsync("***Changes occured***");
                    foreach(Item change in changes)
                    {
                        await Console.Out.WriteLineAsync(change.id + " - " + change.location + " - " + change.name);
                    }
                });

            ChangeFeedProcessor processor = builder
                .WithInstanceName("firstinstance")
                .WithLeaseContainer(leasecontainer)
                .Build();

            await processor.StartAsync();

            Console.ReadKey();

            await processor.StopAsync();
        }
    }
}
