// See https://aka.ms/new-console-template for more information

using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using StockServer;

using var channel = GrpcChannel.ForAddress("http://localhost:8042");
var client = new StockServer.Updates.UpdatesClient(channel);

Console.WriteLine("Starting stream from server:");

var symbols = new[] { "IBM", "Meta", "Alpha", "Apple", "Tesla" };

await StreamFromServer(client, symbols);

Console.WriteLine("Stream ended, bye bye...");


static async Task StreamFromServer(Updates.UpdatesClient client, string[] symbols)
{
    using var call = client.GetStockUpdates(new StockRequest{ Symbols = { symbols }});

    await foreach (var update in call.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine($"{update.Symbol}: Bid: {update.Bid} Ask: {update.Ask}");
    }
}