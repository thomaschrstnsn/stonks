use std::pin::Pin;
use std::time::Duration;
use stocks::updates_server::{Updates, UpdatesServer};
use stocks::{StockReply, StockRequest};
use tokio::sync::mpsc;
use tokio_stream::{wrappers::ReceiverStream, Stream, StreamExt};
use tonic::{transport::Server, Request, Response, Status};

pub mod stocks {
    tonic::include_proto!("stocks");
}

type StockResult<T> = Result<Response<T>, Status>;
type ResponseStream = Pin<Box<dyn Stream<Item = Result<StockReply, Status>> + Send>>;

#[derive(Debug, Default)]
pub struct StockUpdatesService {}

#[tonic::async_trait]
impl Updates for StockUpdatesService {
    type GetStockUpdatesStream = ResponseStream;

    async fn get_stock_updates(
        &self,
        request: Request<StockRequest>,
    ) -> StockResult<Self::GetStockUpdatesStream> {
        println!("received request: {:?}", request);

        let repeat = std::iter::repeat(StockReply {
            symbol: "AAPL".to_owned(),
            bid: 2.0,
            ask: 3.0,
        });

        let mut stream = Box::pin(tokio_stream::iter(repeat).throttle(Duration::from_millis(1000)));

        let (tx, rx) = mpsc::channel(128);
        tokio::spawn(async move {
            while let Some(item) = stream.next().await {
                match tx.send(Result::<_, Status>::Ok(item)).await {
                    Ok(_) => {
                        // item was queued to client
                    }
                    Err(_item) => {
                        break;
                    }
                }
            }
            println!("client disconnected");
        });

        let output_stream = ReceiverStream::new(rx);

        Ok(Response::new(
            Box::pin(output_stream) as Self::GetStockUpdatesStream
        ))
    }
}

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    let server = StockUpdatesService {};

    Server::builder()
        .add_service(UpdatesServer::new(server))
        .serve("127.0.0.1:8042".parse()?)
        .await?;

    Ok(())
}
