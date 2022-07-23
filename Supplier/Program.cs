using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;

namespace Supplier
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Started sending!");
            var connString = "Endpoint=sb://daprbindingeventhub01.servicebus.windows.net/;SharedAccessKeyName=sendlisten;SharedAccessKey=imXKNeIintq4tDaUxy333T3FqmB4xDsp2eN8NYcVwNY=;EntityPath=stockrefill";
            var products = new List<string> { "Shoes111", "Jacket222", "Jacket223", "Shoes112" };
            var rand = new Random();

            await using var producerClient = new EventHubProducerClient(connString);
            do
            {
                using var eventBatch = await producerClient.CreateBatchAsync();
                for (int i = 0; i < 5; i++)
                {
                    var item = new
                    {
                        sku = products[rand.Next(products.Count)],
                        Quantity = 1
                    };
                    var message = JsonSerializer.Serialize(item);
                    eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(message)));
                }

                await producerClient.SendAsync(eventBatch);
                Console.WriteLine($"Sent batch @{DateTime.Now.ToLongTimeString()}");
            } while (true);
        }
    }
}
