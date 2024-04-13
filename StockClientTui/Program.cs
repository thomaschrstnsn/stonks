using Grpc.Core;
using Grpc.Net.Client;
using StockServer;
using Spectre.Console;
using Spectre.Console.Rendering;

using var channel = GrpcChannel.ForAddress("http://localhost:8042");
var client = new Updates.UpdatesClient(channel);

var symbols = new[] { "INTC", "AMD", "TSLA", "RIVN", "QTTB",  };

using var call = client.GetStockUpdates(new StockRequest { Symbols = { symbols } });
var streamOfStocks = call.ResponseStream;

var table = new Table().Centered();
table.Title($"Stock updates");
table.AddColumn("Symbol");
table.AddColumn(new TableColumn("Bid").RightAligned());
table.AddColumn(new TableColumn("Ask").RightAligned());
table.Border(TableBorder.Ascii);

await AnsiConsole.Live(table)
    .StartAsync(async ctx =>
    {
        var state = new State();
        await foreach (var update in streamOfStocks.ReadAllAsync())
        {
            state.UpdateAndRender(update, table);
            ctx.Refresh();
        }
    });

public class State
{
    private readonly Dictionary<string, StockReply> _symbols;
    private int _numberOfUpdates;

    public State()
    {
        _symbols = new Dictionary<string, StockReply>();
        _numberOfUpdates = 0;
    }

    public void UpdateAndRender(StockReply stock, Table table)
    {
        _symbols[stock.Symbol] = stock;

        _numberOfUpdates++;

        table.Rows.Clear();

        foreach (var row in _symbols.Values
                     .OrderBy(s => s.Symbol)
                     .Select(stock =>
                         new TableRow(new IRenderable[]
                         {
                             new Markup($"[bold yellow underline]{stock.Symbol}[/]"),
                             new Markup($"[green]{stock.Bid:0.000}[/]"),
                             new Markup($"[red]{stock.Ask:0.000}[/]"),
                         })))
        {
            table.Rows.Add(row);
        }
        
        table.Caption($"Updates: {_numberOfUpdates}");
    }
}