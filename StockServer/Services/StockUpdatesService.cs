using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Options;

namespace StockServer.Services;

public class StockUpdatesService : Updates.UpdatesBase
{
     private readonly ILogger<StockUpdatesService> _logger;
     private readonly Random _random;

     public StockUpdatesService(
         ILogger<StockUpdatesService> logger)
     {
         _logger = logger;
         _random = new Random();
     }

     public override async Task GetStockUpdates(StockRequest request, IServerStreamWriter<StockReply> responseStream, ServerCallContext context)
     {
         _logger.LogInformation("Handling stock update request for symbols: {Symbols}", request.Symbols);
         
         if (request.Symbols.Count == 0)
         {
             return;
         }
         
         var stocks = request.Symbols
             .Select(InitialStateForSymbol)
             .ToArray();

         foreach (var initial in stocks)
         {
             await responseStream.WriteAsync(initial);
         }
         
         while (true)
         {
             await Task.Delay(TimeSpan.FromSeconds(1d * _random.NextDouble()));

             _random.Shuffle(stocks);

             var stockToBeUpdated = stocks.First();
             Update(stockToBeUpdated);
             await responseStream.WriteAsync(stockToBeUpdated);
         }
     }

     private StockReply InitialStateForSymbol(string symbol)
     {
         var ask = _random.NextDouble() * (_random.Next() % 1000);
         var delta = (_random.NextDouble() - 0.5d) * 0.01d * ask;
         
         return new StockReply { Symbol = symbol, Ask = ask, Bid = ask + delta };
     }

     private void Update(StockReply stockReply)
     {
         var delta = (_random.NextDouble() - 0.5d) * 0.01d * stockReply.Bid;

         stockReply.Bid += delta;
         stockReply.Ask += delta;
     }
}
