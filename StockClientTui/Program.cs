// See https://aka.ms/new-console-template for more information

using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;

using var channel = GrpcChannel.ForAddress("http://localhost:8042");
var client = new StockServer.Updates.UpdatesClient(channel);

Console.WriteLine("Starting stream from server:");

await StreamFromServer(client);

Console.WriteLine("Stream ended, bye bye...");


static async Task StreamFromServer(StockServer.Updates.UpdatesClient client)
{
    using var call = client.GetStockUpdates(new Empty());

    await foreach (var update in call.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine($"{update.Symbol}: Bid: {update.Bid} Ask: {update.Ask}");
    }
}