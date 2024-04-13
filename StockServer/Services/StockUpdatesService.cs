using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace StockServer.Services;

public class StockUpdatesService : Updates.UpdatesBase
{
     private readonly ILogger<StockUpdatesService> _logger;
     
     public StockUpdatesService(ILogger<StockUpdatesService> logger)
     {
         _logger = logger;
     }

     public override async Task GetStockUpdates(Empty request, IServerStreamWriter<StockReply> responseStream, ServerCallContext context)
     {
         while (true)
         {
             await responseStream.WriteAsync(new StockReply { Symbol = "AAPL", Ask = 123, Bid = 124 });
             await Task.Delay(TimeSpan.FromSeconds(1));
         }
     }
}
