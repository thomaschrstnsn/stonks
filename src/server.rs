use rand::prelude::*;
use std::pin::Pin;
use std::time::Duration;
use stocks::updates_server::{Updates, UpdatesServer};
use stocks::{StockReply, StockRequest};
use tokio::sync::mpsc;
use tokio::time::sleep;
use tokio_stream::{wrappers::ReceiverStream, Stream};
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

        let mut stocks: Vec<_> = request
            .into_inner()
            .symbols
            .iter()
            .map(|s| initial_state_for_symbol(s))
            .collect();

        let (tx, rx) = mpsc::channel(128);
        tokio::spawn(async move {
            let stream = stocks.clone().into_iter();
            for item in stream {
                match tx.send(Result::<_, Status>::Ok(item)).await {
                    Ok(_) => {
                        // item was queued to client
                    }
                    Err(_item) => {
                        break;
                    }
                }
            }

            loop {
                sleep(Duration::from_secs_f64(rand::random::<f64>())).await;

                stocks.shuffle(&mut thread_rng());

                let stock_to_update: &mut StockReply = stocks.get_mut(0).unwrap();

                update(stock_to_update);

                match tx
                    .send(Result::<_, Status>::Ok(stock_to_update.clone()))
                    .await
                {
                    Ok(_) => {
                        // item was queued to client
                    }
                    Err(_item) => {
                        break;
                    }
                }
            }
        });

        let output_stream = ReceiverStream::new(rx);

        Ok(Response::new(
            Box::pin(output_stream) as Self::GetStockUpdatesStream
        ))
    }
}

fn initial_state_for_symbol(symbol: &str) -> StockReply {
    let mut rng = rand::thread_rng();

    let ask = rng.gen::<f64>() * 1000.0f64;
    let delta = (rng.gen::<f64>() - 0.5f64) * 0.01 * ask;

    StockReply {
        symbol: symbol.to_owned(),
        ask,
        bid: ask + delta,
    }
}

fn update(stock: &mut StockReply) {
    let mut rng = rand::thread_rng();

    let delta = (rng.gen::<f64>() - 0.5f64) * 0.01 * stock.bid;

    stock.bid += delta;
    stock.ask += delta;
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
