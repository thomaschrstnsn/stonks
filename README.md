# Streaming Stonks

![stonks](https://compote.slate.com/images/926e5009-c10a-48fe-b90e-fa0760f82fcd.png?crop=680%2C453%2Cx0%2Cy0&width=1280)

Streaming gRPC client/server application, showing (fictive) stock updates in dotnet 8.

Server is a simple [gRPC](https://grpc.io/) server that continously streams randomized stock updates to any client that requests updates.

Client is a [TUI](https://en.wikipedia.org/wiki/Text-based_user_interface) showing updates to the stock symbols it requests.
This is achieved using [Spectre.Console](https://spectreconsole.net/)

![screenshot](./screenshot.png)

## Running

- Server: `dotnet run --project StockServer`
- Client: `dotnet run --project StockClientTui`

